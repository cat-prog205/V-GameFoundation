namespace VGameFoundation.Editor.LocalData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using UnityEditor;
    using UnityEditorHomeMade;
    using UnityEngine;
    using VGameFoundation.DI;
    using VGameFoundation.Scripts.Services.LocalData;
    using VGameFoundation.Scripts.Utilities.Extension;

    [ToolEntry("Local Data", category: "VGameFoundation", keywords: "save json user data")]
    public class LocalDataEditor : EdWindowBase
    {
        private static readonly JsonSerializerSettings JsonSetting = HandleUserDataServices.JsonSetting;

        [SerializeField] private EdSearchField search = new();

        private List<ILocalData> localData = new();
        private IUserDataStorage storage;
        private bool             defaultExpanded = true;

        private IUserDataStorage Storage => this.storage ??= UserDataStorages.CreateDefault();

        private static Type[] DataTypes => ReflectionUtils.GetAllDerivedTypes<ILocalData>().ToArray();

        [MenuItem("VGameFoundation/Local Data Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalDataEditor>("Local Data");
            window.minSize = new Vector2(520, 420);
        }

        protected override void OnToolbarGUI()
        {
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(48)))
                this.LoadLocalData();

            if (GUILayout.Button("Sync Runtime", EditorStyles.toolbarButton, GUILayout.Width(92)))
                this.SyncWithRuntime();

            using (new EditorGUI.DisabledScope(this.localData.Count == 0))
            {
                if (GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.Width(80)))
                    this.SetAllExpanded(true);

                if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.Width(88)))
                    this.SetAllExpanded(false);
            }

            GUILayout.FlexibleSpace();
            this.search.OnToolbarGUI(200f);
        }

        protected override void OnBodyGUI()
        {
            var visible = this.localData.Where(this.MatchesSearch).ToList();
            if (this.localData.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Nothing loaded yet. Press Load to inspect the save folder, or Create Defaults to seed it.",
                    MessageType.Info);
                return;
            }

            if (visible.Count == 0)
            {
                EditorGUILayout.HelpBox($"No entries match \"{this.search.Query}\".", MessageType.Info);
                return;
            }

            foreach (var data in visible) this.DrawEntry(data);
        }

        protected override void OnFooterGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (EdGUI.ConfirmButton("Save", GUILayout.Height(26), GUILayout.Width(72)))
                    this.SaveLocalData();

                if (GUILayout.Button("Create Defaults", GUILayout.Height(26)))
                    this.CreateDefaultLocalData();

                if (EdGUI.WarningButton("Validate", GUILayout.Height(26), GUILayout.Width(80)))
                    this.ValidateLocalData();

                if (GUILayout.Button("Open Folder", GUILayout.Height(26), GUILayout.Width(90)))
                    this.OpenSaveFolder();

                GUILayout.FlexibleSpace();

                if (EdGUI.DangerButtonWithConfirm(
                        "Clear",
                        "This deletes every save file, including backups. Continue?",
                        "Clear local data",
                        GUILayout.Height(26),
                        GUILayout.Width(64)))
                    this.ClearLocalData();
            }

            EditorGUILayout.LabelField(
                $"{this.VisibleCount}/{this.localData.Count} visible",
                EditorStyles.miniLabel);
        }

        private int VisibleCount =>
            string.IsNullOrWhiteSpace(this.search.Query)
                ? this.localData.Count
                : this.localData.Count(this.MatchesSearch);

        private void DrawEntry(ILocalData data)
        {
            var key   = HandleUserDataServices.KeyOf(data.GetType());
            var title = data.GetType().Name;

            using var section = new EdSection(title, PersistKey(key), rightText: key, defaultExpanded: this.defaultExpanded);
            if (!section.Expanded) return;

            LocalDataInspector.Draw(data);
        }

        private void SetAllExpanded(bool expanded)
        {
            this.defaultExpanded = expanded;
            foreach (var data in this.localData)
            {
                var key = HandleUserDataServices.KeyOf(data.GetType());
                EdPersist.SetBool("Section." + PersistKey(key), expanded);
            }
        }

        private static string PersistKey(string key) => "localdata." + key;

        private bool MatchesSearch(ILocalData data)
        {
            var type = data.GetType();
            var key  = HandleUserDataServices.KeyOf(type);
            return this.search.MatchesAny(type.Name, key);
        }

        private void ReplaceData(List<ILocalData> data, bool expand)
        {
            this.localData       = data ?? new List<ILocalData>();
            this.defaultExpanded = expand;
        }

        private void OpenSaveFolder()
        {
            var path = FileUserDataStorage.DefaultRootPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            EditorUtility.RevealInFinder(path);
            this.SetStatus(path);
        }

        private void SyncWithRuntime()
        {
            if (!Application.isPlaying)
            {
                this.SetStatus("Sync Runtime only works in Play Mode.", MessageType.Error);
                Debug.LogError("Sync Runtime only works in Play Mode.");
                this.ReplaceData(null, true);
                return;
            }

            this.ReplaceData(DataTypes.Select(type => this.GetCurrentContainer().Resolve(type)).OfType<ILocalData>().ToList(), true);
            this.SetStatus($"Synced {this.localData.Count} runtime instances.");
        }

        private async void LoadLocalData()
        {
            var types      = DataTypes;
            var candidates = await this.Storage.LoadAsync(types.Select(HandleUserDataServices.KeyOf).ToArray());
            var loaded = types
                .Select((type, i) => Deserialize(type, candidates[i]))
                .Where(data => data != null)
                .ToList();

            this.ReplaceData(loaded, true);
            this.SetStatus($"Loaded {this.localData.Count} entries from disk.");
        }

        private async void SaveLocalData()
        {
            if (this.localData.Count == 0)
            {
                this.SetStatus("Nothing to save.", MessageType.Warning);
                return;
            }

            await this.Storage.SaveAsync(this.localData
                .Select(data => (HandleUserDataServices.KeyOf(data.GetType()), JsonConvert.SerializeObject(data, JsonSetting)))
                .ToArray());

            this.SetStatus($"Saved {this.localData.Count} entries.");
        }

        private async void CreateDefaultLocalData()
        {
            var created = DataTypes.Select(CreateDefault).Where(data => data != null).ToList();

            await this.Storage.SaveAsync(created
                .Select(data => (HandleUserDataServices.KeyOf(data.GetType()), JsonConvert.SerializeObject(data, JsonSetting)))
                .ToArray());

            this.ReplaceData(created, true);
            this.SetStatus($"Wrote {this.localData.Count} default entries.");
        }

        private async void ClearLocalData()
        {
            await this.Storage.DeleteAsync(DataTypes.Select(HandleUserDataServices.KeyOf).ToArray());
            this.ReplaceData(null, true);
            this.SetStatus("Cleared all local data.", MessageType.Warning);
        }

        private void ValidateLocalData()
        {
            var duplicates = DataTypes
                .GroupBy(HandleUserDataServices.KeyOf)
                .Where(group => group.Count() > 1)
                .ToArray();

            foreach (var group in duplicates)
            {
                Debug.LogError($"Duplicated local data key '{group.Key}': {string.Join(", ", group.Select(type => type.FullName))}");
            }

            var missingKey = DataTypes.Where(type => type.GetCustomAttributes(typeof(LocalDataKeyAttribute), false).Length == 0).ToArray();
            foreach (var type in missingKey)
            {
                Debug.LogWarning($"{type.FullName} has no [LocalDataKey], renaming or moving it will lose player data");
            }

            if (duplicates.Length == 0 && missingKey.Length == 0)
                this.SetStatus("Keys are valid.");
            else
                this.SetStatus($"{duplicates.Length} duplicate key(s), {missingKey.Length} missing [LocalDataKey].", MessageType.Warning);
        }

        private static ILocalData Deserialize(Type type, string[] candidates)
        {
            foreach (var json in candidates)
            {
                try
                {
                    if (JsonConvert.DeserializeObject(json, type, JsonSetting) is ILocalData data) return data;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot read {type.Name}: {e.Message}");
                }
            }

            return null;
        }

        private static ILocalData CreateDefault(Type type)
        {
            if (Activator.CreateInstance(type) is not ILocalData instance) return null;

            instance.Init();
            if (instance is IMigratableLocalData migratable) migratable.SavedVersion = migratable.LatestVersion;

            return instance;
        }
    }

    internal static class LocalDataInspector
    {
        private const int MaxDepth = 6;

        public static void Draw(object target)
        {
            if (target == null)
            {
                EditorGUILayout.HelpBox("null", MessageType.None);
                return;
            }

            DrawObject(target, 0);
        }

        private static void DrawObject(object target, int depth)
        {
            if (depth > MaxDepth)
            {
                EditorGUILayout.LabelField("...");
                return;
            }

            foreach (var field in EnumerateFields(target.GetType()))
            {
                EditorGUI.BeginChangeCheck();
                var value   = field.GetValue(target);
                var next    = DrawValue(ObjectNames.NicifyVariableName(field.Name), value, field.FieldType, depth);
                if (!EditorGUI.EndChangeCheck()) continue;

                try
                {
                    field.SetValue(target, next);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Cannot write {field.DeclaringType?.Name}.{field.Name}: {e.Message}");
                }
            }
        }

        private static IEnumerable<FieldInfo> EnumerateFields(Type type)
        {
            return type.GetRecursiveFields()
                .Where(field => !field.IsStatic
                    && !field.IsDefined(typeof(NonSerializedAttribute), inherit: true)
                    && !field.Name.Contains("k__BackingField", StringComparison.Ordinal));
        }

        private static object DrawValue(string label, object value, Type type, int depth)
        {
            if (type == typeof(bool)) return EditorGUILayout.Toggle(label, value is true);
            if (type == typeof(int)) return EditorGUILayout.IntField(label, value is int i ? i : 0);
            if (type == typeof(long)) return EditorGUILayout.LongField(label, value is long l ? l : 0);
            if (type == typeof(float)) return EditorGUILayout.FloatField(label, value is float f ? f : 0f);
            if (type == typeof(double)) return EditorGUILayout.DoubleField(label, value is double d ? d : 0d);
            if (type == typeof(string)) return EditorGUILayout.TextField(label, value as string ?? string.Empty);
            if (type.IsEnum)
            {
                var enumValue = value == null ? Enum.GetValues(type).GetValue(0) : value;
                return EditorGUILayout.EnumPopup(label, (Enum)enumValue);
            }

            if (type == typeof(Vector2)) return EditorGUILayout.Vector2Field(label, value is Vector2 v2 ? v2 : default);
            if (type == typeof(Vector3)) return EditorGUILayout.Vector3Field(label, value is Vector3 v3 ? v3 : default);
            if (type == typeof(Vector4)) return EditorGUILayout.Vector4Field(label, value is Vector4 v4 ? v4 : default);
            if (type == typeof(Color)) return EditorGUILayout.ColorField(label, value is Color c ? c : default);
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                return EditorGUILayout.ObjectField(label, value as UnityEngine.Object, type, true);

            if (typeof(IDictionary).IsAssignableFrom(type))
                return DrawDictionary(label, value as IDictionary, type, depth);

            if (typeof(IList).IsAssignableFrom(type) && type != typeof(string))
                return DrawList(label, value as IList, type, depth);

            if (type.IsClass || type.IsValueType)
            {
                var open = EditorGUILayout.Foldout(true, label, true);
                if (!open) return value;

                if (value == null)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(label);
                        if (GUILayout.Button("Create", GUILayout.Width(70)) && TryCreate(type, out var created))
                            return created;
                    }

                    return null;
                }

                using (new EditorGUI.IndentLevelScope())
                    DrawObject(value, depth + 1);

                return value;
            }

            EditorGUILayout.LabelField(label, value?.ToString() ?? "null");
            return value;
        }

        private static object DrawList(string label, IList list, Type type, int depth)
        {
            var open = EditorGUILayout.Foldout(true, $"{label} ({list?.Count ?? 0})", true);
            if (!open) return list;

            using var indent = new EditorGUI.IndentLevelScope();
            if (list == null)
            {
                if (GUILayout.Button("Create list", GUILayout.Width(90)) && TryCreate(type, out var created))
                    return created;
                return null;
            }

            var elementType = GetElementType(type);
            for (var i = 0; i < list.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    list[i] = DrawValue($"[{i}]", list[i], elementType, depth + 1);
                    if (!list.IsFixedSize && EdGUI.XButton("Remove"))
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }

            if (!list.IsFixedSize && elementType != null && GUILayout.Button("Add", GUILayout.Width(60)))
            {
                list.Add(elementType.IsValueType ? Activator.CreateInstance(elementType) : null);
            }

            return list;
        }

        private static object DrawDictionary(string label, IDictionary dictionary, Type type, int depth)
        {
            var open = EditorGUILayout.Foldout(true, $"{label} ({dictionary?.Count ?? 0})", true);
            if (!open) return dictionary;

            using var indent = new EditorGUI.IndentLevelScope();
            if (dictionary == null)
            {
                if (GUILayout.Button("Create dictionary", GUILayout.Width(130)) && TryCreate(type, out var created))
                    return created;
                return null;
            }

            var args       = type.GetGenericArguments();
            var valueType  = args.Length > 1 ? args[1] : typeof(object);
            var keys       = dictionary.Keys.Cast<object>().ToList();
            foreach (var key in keys)
                dictionary[key] = DrawValue(key?.ToString() ?? "null", dictionary[key], valueType, depth + 1);

            return dictionary;
        }

        private static Type GetElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType();
            if (type.IsGenericType) return type.GetGenericArguments()[0];
            return typeof(object);
        }

        private static bool TryCreate(Type type, out object created)
        {
            try
            {
                created = Activator.CreateInstance(type);
                return created != null;
            }
            catch
            {
                created = null;
                return false;
            }
        }
    }
}
