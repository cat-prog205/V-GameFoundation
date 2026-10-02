namespace VGameFoundation.Editor.LocalData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using VGameFoundation.DI;
    using VGameFoundation.Scripts.Services.LocalData;
    using VGameFoundation.Scripts.Utilities.Extension;

    public class LocalDataEditor : EditorWindow
    {
        private static readonly JsonSerializerSettings JsonSetting = HandleUserDataServices.JsonSetting;

        private static readonly Color LoadColor     = new(0.32f, 0.72f, 0.62f);
        private static readonly Color SaveColor     = new(0.38f, 0.70f, 0.42f);
        private static readonly Color SyncColor     = new(0.40f, 0.62f, 0.92f);
        private static readonly Color DefaultColor  = new(0.45f, 0.58f, 0.88f);
        private static readonly Color ValidateColor = new(0.92f, 0.72f, 0.32f);
        private static readonly Color FolderColor   = new(0.62f, 0.66f, 0.74f);
        private static readonly Color ClearColor    = new(0.86f, 0.38f, 0.38f);
        private static readonly Color HeaderBg      = new(0.16f, 0.17f, 0.20f);

        private List<ILocalData> localData = new();
        private readonly Dictionary<ILocalData, PropertyTree> trees = new();
        private readonly Dictionary<string, bool> expandedByKey = new();

        private IUserDataStorage storage;
        private Vector2          scroll;
        private string           search          = string.Empty;
        private bool             defaultExpanded = true;
        private string           status          = "Load data to inspect saved files.";

        private IUserDataStorage Storage => this.storage ??= UserDataStorages.CreateDefault();

        private static Type[] DataTypes => ReflectionUtils.GetAllDerivedTypes<ILocalData>().ToArray();

        [MenuItem("VGameFoundation/Local Data Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalDataEditor>("Local Data");
            window.minSize = new Vector2(520, 420);
        }

        private void OnGUI()
        {
            this.DrawHeader();
            this.DrawPrimaryActions();
            this.DrawSecondaryActions();
            this.DrawSearchBar();
            this.DrawList();
            this.DrawFooter();

            if (GUI.changed) this.Repaint();
        }

        private void OnDestroy()
        {
            this.DisposeTrees();
        }

        private void DrawHeader()
        {
            var rect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(rect, HeaderBg);

            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);
                GUILayout.Label("Local Data", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(this.localData.Count == 0 ? "No entries loaded" : $"{this.localData.Count} entries", EditorStyles.miniLabel);
                GUILayout.Space(12);
            }

            GUILayout.Space(8);
            EditorGUILayout.EndVertical();
        }

        private void DrawPrimaryActions()
        {
            GUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                this.ActionButton("Load", "Read saved files from disk", LoadColor, this.LoadLocalData);
                this.ActionButton("Save", "Write the list below back to disk", SaveColor, this.SaveLocalData);
                this.ActionButton("Sync Runtime", "Copy live instances from Play Mode", SyncColor, this.SyncWithRuntime);
                GUILayout.Space(10);
            }
        }

        private void DrawSecondaryActions()
        {
            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                this.ActionButton("Create Defaults", "Init every type and write it to disk", DefaultColor, this.CreateDefaultLocalData);
                this.ActionButton("Validate", "Check duplicated keys and missing [LocalDataKey]", ValidateColor, this.ValidateLocalData);
                this.ActionButton("Open Folder", "Reveal the save directory in Explorer", FolderColor, this.OpenSaveFolder);
                this.ActionButton("Clear", "Delete every save file, including backups", ClearColor, this.ClearLocalData);
                GUILayout.Space(10);
            }
        }

        private void DrawSearchBar()
        {
            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                GUILayout.Label("Search", GUILayout.Width(48));
                this.search = EditorGUILayout.TextField(this.search, EditorStyles.toolbarSearchField);

                using (new EditorGUI.DisabledScope(this.localData.Count == 0))
                {
                    if (GUILayout.Button("Expand All", EditorStyles.toolbarButton, GUILayout.Width(88)))
                    {
                        this.defaultExpanded = true;
                        this.expandedByKey.Clear();
                    }

                    if (GUILayout.Button("Collapse All", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    {
                        this.defaultExpanded = false;
                        this.expandedByKey.Clear();
                    }
                }

                GUILayout.Space(10);
            }
        }

        private void DrawList()
        {
            GUILayout.Space(8);
            this.scroll = EditorGUILayout.BeginScrollView(this.scroll);

            var visible = this.localData.Where(this.MatchesSearch).ToList();
            if (this.localData.Count == 0)
            {
                    DrawEmptyState("Nothing loaded yet.\nPress Load to inspect the save folder, or Create Defaults to seed it.");
            }
            else if (visible.Count == 0)
            {
                    DrawEmptyState($"No entries match \"{this.search}\".");
            }
            else
            {
                foreach (var data in visible) this.DrawEntry(data);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEntry(ILocalData data)
        {
            var key   = HandleUserDataServices.KeyOf(data.GetType());
            var title = data.GetType().Name;
            var open  = this.IsExpanded(key);

            GUILayout.Space(2);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);

                SirenixEditorGUI.BeginBox();

                using (new EditorGUILayout.HorizontalScope())
                {
                    var nextOpen = SirenixEditorGUI.Foldout(open, title);
                    if (nextOpen != open) this.expandedByKey[key] = nextOpen;
                    open = nextOpen;

                    GUILayout.FlexibleSpace();
                    GUILayout.Label(key, EditorStyles.miniLabel);
                }

                if (open)
                {
                    GUILayout.Space(4);
                    this.TreeOf(data).Draw(false);
                    GUILayout.Space(4);
                }

                SirenixEditorGUI.EndBox();
                GUILayout.Space(10);
            }
        }

        private void DrawFooter()
        {
            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12);
                var visible = string.IsNullOrWhiteSpace(this.search)
                    ? this.localData.Count
                    : this.localData.Count(this.MatchesSearch);

                GUILayout.Label($"{visible}/{this.localData.Count} visible", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
                GUILayout.Label(this.status, EditorStyles.miniLabel);
                GUILayout.Space(12);
            }

            GUILayout.Space(6);
        }

        private static void DrawEmptyState(string message)
        {
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(message, EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        private void ActionButton(string label, string tooltip, Color color, Action action)
        {
            var previous = GUI.backgroundColor;
            GUI.backgroundColor = color;
            if (GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Height(28))) action();
            GUI.backgroundColor = previous;
        }

        private bool MatchesSearch(ILocalData data)
        {
            if (string.IsNullOrWhiteSpace(this.search)) return true;

            var type = data.GetType();
            var key  = HandleUserDataServices.KeyOf(type);
            return type.Name.IndexOf(this.search, StringComparison.OrdinalIgnoreCase) >= 0
                || key.IndexOf(this.search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsExpanded(string key)
        {
            return this.expandedByKey.TryGetValue(key, out var expanded) ? expanded : this.defaultExpanded;
        }

        private PropertyTree TreeOf(ILocalData data)
        {
            if (this.trees.TryGetValue(data, out var tree)) return tree;

            tree = PropertyTree.Create(data);
            this.trees[data] = tree;
            return tree;
        }

        private void ReplaceData(List<ILocalData> data, bool expand)
        {
            this.DisposeTrees();
            this.localData       = data ?? new List<ILocalData>();
            this.defaultExpanded = expand;
            this.expandedByKey.Clear();
        }

        private void DisposeTrees()
        {
            foreach (var tree in this.trees.Values) tree.Dispose();
            this.trees.Clear();
        }

        private void OpenSaveFolder()
        {
            var path = FileUserDataStorage.DefaultRootPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            EditorUtility.RevealInFinder(path);
            this.status = path;
        }

        private void SyncWithRuntime()
        {
            if (!Application.isPlaying)
            {
                this.status = "Sync Runtime only works in Play Mode.";
                Debug.LogError(this.status);
                this.ReplaceData(null, true);
                return;
            }

            this.ReplaceData(DataTypes.Select(type => this.GetCurrentContainer().Resolve(type)).OfType<ILocalData>().ToList(), true);
            this.status = $"Synced {this.localData.Count} runtime instances.";
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
            this.status = $"Loaded {this.localData.Count} entries from disk.";
            this.Repaint();
        }

        private async void SaveLocalData()
        {
            if (this.localData.Count == 0)
            {
                this.status = "Nothing to save.";
                return;
            }

            await this.Storage.SaveAsync(this.localData
                .Select(data => (HandleUserDataServices.KeyOf(data.GetType()), JsonConvert.SerializeObject(data, JsonSetting)))
                .ToArray());

            this.status = $"Saved {this.localData.Count} entries.";
            this.Repaint();
        }

        private async void CreateDefaultLocalData()
        {
            var created = DataTypes.Select(CreateDefault).Where(data => data != null).ToList();

            await this.Storage.SaveAsync(created
                .Select(data => (HandleUserDataServices.KeyOf(data.GetType()), JsonConvert.SerializeObject(data, JsonSetting)))
                .ToArray());

            this.ReplaceData(created, true);
            this.status = $"Wrote {this.localData.Count} default entries.";
            this.Repaint();
        }

        private async void ClearLocalData()
        {
            if (!EditorUtility.DisplayDialog("Clear local data", "This deletes every save file, including backups. Continue?", "Delete", "Cancel"))
            {
                return;
            }

            await this.Storage.DeleteAsync(DataTypes.Select(HandleUserDataServices.KeyOf).ToArray());
            this.ReplaceData(null, true);
            this.status = "Cleared all local data.";
            this.Repaint();
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

            this.status = duplicates.Length == 0 && missingKey.Length == 0
                ? "Keys are valid."
                : $"{duplicates.Length} duplicate key(s), {missingKey.Length} missing [LocalDataKey].";
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
}
