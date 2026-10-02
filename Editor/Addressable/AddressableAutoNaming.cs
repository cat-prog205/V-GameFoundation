using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using System.Collections.Generic;

[InitializeOnLoad]
public class AddressableAutoNaming
{
     static AddressableAutoNaming()
     {
          // Subscribe to the event when assets are added to addressables
          AddressableAssetSettings.OnModificationGlobal += OnSettingsModification;
     }

     private static void OnSettingsModification(AddressableAssetSettings settings,
          AddressableAssetSettings.ModificationEvent                     modificationEvent, object arg0)
     {
          if (modificationEvent != AddressableAssetSettings.ModificationEvent.EntryAdded)
               return;

          if (arg0 is IList<AddressableAssetEntry> entries)
          {
               foreach (var entry in entries)
               {
                    if (entry == null) continue;

                    string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
                    Object asset     = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                    if (asset != null)
                    {
                         entry.address = asset.name;
                    }
               }

               // Save changes
               settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entries, true);
          }
     }
}
