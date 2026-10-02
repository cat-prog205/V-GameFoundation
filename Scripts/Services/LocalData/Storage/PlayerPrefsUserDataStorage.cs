namespace VGameFoundation.Scripts.Services.LocalData
{
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Scripting;

    /// <summary>
    /// PlayerPrefs backed storage, kept so a project that already shipped on PlayerPrefs can read
    /// its existing saves. Prefer <see cref="FileUserDataStorage"/> for new projects: PlayerPrefs
    /// rewrites and flushes its whole store on every save, so the cost grows with total data size.
    /// </summary>
    public sealed class PlayerPrefsUserDataStorage : IUserDataStorage
    {
        private const string BackupSuffix = "~bak";

        [Preserve]
        public PlayerPrefsUserDataStorage()
        {
        }

        public UniTask<string[][]> LoadAsync(params string[] keys)
        {
            return UniTask.FromResult(keys.Select(ReadCandidates).ToArray());
        }

        public UniTask SaveAsync(params (string key, string payload)[] entries)
        {
            foreach (var (key, payload) in entries)
            {
                // The previous value is copied while still encoded, so no extra work is needed.
                var previous = PlayerPrefs.GetString(key);
                if (!string.IsNullOrEmpty(previous)) PlayerPrefs.SetString(key + BackupSuffix, previous);

                PlayerPrefs.SetString(key, payload);
            }

            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }

        public UniTask DeleteAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.DeleteKey(key + BackupSuffix);
            }

            PlayerPrefs.Save();
            return UniTask.CompletedTask;
        }

        private static string[] ReadCandidates(string key)
        {
            return new[] { PlayerPrefs.GetString(key), PlayerPrefs.GetString(key + BackupSuffix) }
                .Where(payload => !string.IsNullOrEmpty(payload))
                .ToArray();
        }
    }
}
