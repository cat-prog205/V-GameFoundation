namespace VGameFoundation.Scripts.Services.LocalData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    using UnityEngine.Scripting;
    using VGameFoundation.DI;
    using VGameFoundation.Scripts.Utilities.Extension;
    using VGameFoundation.Scripts.Utilities.LogService;

    /// <summary>
    /// Turns <see cref="ILocalData"/> instances into json and back, and delegates the actual
    /// persistence to an <see cref="IUserDataStorage"/>. Everything version related lives here so
    /// swapping the storage backend never affects how saves are interpreted.
    /// </summary>
    public class HandleUserDataServices : IHandleUserDataServices
    {
        public const string UserDataPrefix = "LD-";

        public static readonly JsonSerializerSettings JsonSetting = new()
        {
            TypeNameHandling      = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

            // Required by PopulateObject: the default (Auto) reuses the collection instance
            // created in Init() and appends to it, which duplicates elements.
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };

        public static string KeyOf(Type type)
        {
            var attribute = type.GetCustomAttribute<LocalDataKeyAttribute>();
            return UserDataPrefix + (attribute?.Key ?? type.FullName);
        }

        private readonly IUserDataStorage               storage;
        private readonly IDependencyContainer           container;
        private readonly Dictionary<string, ILocalData> cache = new();

        [Preserve]
        public HandleUserDataServices(IUserDataStorage storage, IDependencyContainer container)
        {
            this.storage   = storage;
            this.container = container;
        }

        public bool HasCorruptedData { get; private set; }

        public UniTask Save<T>(T data, bool force = false) where T : class, ILocalData
        {
            var key = KeyOf(data.GetType());

            this.cache[key] = data;

            if (!force) return UniTask.CompletedTask;

            return this.Write((key, JsonConvert.SerializeObject(data, JsonSetting)));
        }

        public async UniTask<T> Load<T>() where T : class, ILocalData
        {
            return (T)(await this.Load(typeof(T)))[0];
        }

        public async UniTask<ILocalData[]> Load(params Type[] types)
        {
            var keys       = types.Select(KeyOf).ToArray();
            var candidates = await this.storage.LoadAsync(keys);

            return types.Select((type, i) => this.cache.GetOrAdd(keys[i], () => this.Restore(type, keys[i], candidates[i]))).ToArray();
        }

        public UniTask SaveAll()
        {
            return this.Write(this.cache.Select(entry => (entry.Key, JsonConvert.SerializeObject(entry.Value, JsonSetting))).ToArray());
        }

        private async UniTask Write(params (string key, string json)[] entries)
        {
            var batch = new Dictionary<string, string>();
            foreach (var (key, json) in entries) batch[key] = json;

            this.StampMeta(batch);

            await this.storage.SaveAsync(batch.Select(entry => (entry.Key, entry.Value)).ToArray());

            LogService.LogWithColor($"Saved user data ({batch.Count} entries)", Color.green);
        }

        /// <summary>
        /// Records which build wrote the save and bumps the generation counter a remote backend
        /// will need to reconcile two saves. Skipped until the data has been loaded, so an early
        /// save cannot overwrite a stored generation with a fresh zero.
        /// </summary>
        private void StampMeta(IDictionary<string, string> batch)
        {
            var key = KeyOf(typeof(UserDataMeta));

            if (!this.cache.TryGetValue(key, out var cached) || cached is not UserDataMeta meta) return;

            meta.Generation++;
            meta.SavedAtUtcTicks = DateTime.UtcNow.Ticks;
            meta.AppVersion      = Application.version;

            batch[key] = JsonConvert.SerializeObject(meta, JsonSetting);
        }

        /// <summary>
        /// Resolves the container bound instance, applies defaults through <see cref="ILocalData.Init"/>,
        /// then overlays the stored json on top. Fields absent from the json keep their default,
        /// which is what makes adding fields in a new build safe for existing players.
        /// Older candidates are tried in turn, and plain defaults are the last resort, so a
        /// corrupted save can never stall the loading flow.
        /// </summary>
        private ILocalData Restore(Type type, string key, string[] candidates)
        {
            var data = (ILocalData)this.container.Resolve(type);

            foreach (var json in candidates)
            {
                data.Init();

                // Saves written before this class had a version do not carry the field,
                // so treat them as v0 to let Migrate run.
                if (data is IMigratableLocalData legacy) legacy.SavedVersion = 0;

                if (!TryPopulate(data, json, key)) continue;

                Migrate(data, key);

                LogService.LogWithColor($"Loaded {key}", Color.green);
                return data;
            }

            if (candidates.Length > 0)
            {
                // Everything stored for this key was unusable. Flagged so that a future cloud
                // sync can refuse to upload a save that is really just freshly reset defaults.
                this.HasCorruptedData = true;
                LogService.LogError($"{key} is unrecoverable, resetting to default");
            }

            data.Init();
            if (data is IMigratableLocalData fresh) fresh.SavedVersion = fresh.LatestVersion;

            LogService.LogWithColor($"Initialized {key} with defaults", Color.green);
            return data;
        }

        private static void Migrate(ILocalData data, string key)
        {
            if (data is not IMigratableLocalData migratable) return;
            if (migratable.SavedVersion >= migratable.LatestVersion) return;

            var fromVersion = migratable.SavedVersion;
            migratable.Migrate(fromVersion);
            migratable.SavedVersion = migratable.LatestVersion;

            LogService.LogWithColor($"Migrated {key}: v{fromVersion} -> v{migratable.LatestVersion}", Color.cyan);
        }

        private static bool TryPopulate(ILocalData target, string json, string key)
        {
            try
            {
                JsonConvert.PopulateObject(json, target, JsonSetting);
                return true;
            }
            catch (Exception e)
            {
                LogService.LogError($"Failed to parse {key}: {e.Message}");
                return false;
            }
        }
    }
}
