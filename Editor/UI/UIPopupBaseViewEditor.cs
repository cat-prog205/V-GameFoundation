#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using VGameFoundation.Scripts.UI;
using System.Linq;

[CustomEditor(typeof(UIPopupBaseView), true)]
public class UIPopupBaseViewEditor : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        this.DrawCustomButtons();
    }

    private void DrawCustomButtons()
    {
        GUILayout.Space(20);
        GUILayout.Label("Quick Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("✚ Add Open Animation Module", GUILayout.Height(30))) this.ShowModuleSelector("openAnimation");

        GUILayout.Space(5);

        if (GUILayout.Button("✚ Add Close Animation Module", GUILayout.Height(30))) this.ShowModuleSelector("closeAnimation");
    }

    private void ShowModuleSelector(string propertyName)
    {
        var types = TypeCache.GetTypesDerivedFrom<UIAnimationModule>()
            .Where(t => !t.IsAbstract).ToList();

        var menu = new GenericMenu();

        foreach (var type in types)
        {
            var niceName = ObjectNames.NicifyVariableName(type.Name);
            menu.AddItem(new GUIContent(niceName), false, () =>
            {
                var targetView = this.target as Component;

                if (targetView == null) return;

                var serializedProp = this.serializedObject.FindProperty(propertyName);

                if (serializedProp == null) return;

                var oldComponent = serializedProp.objectReferenceValue as Component;

                var newModule = Undo.AddComponent(targetView.gameObject, type);

                serializedProp.objectReferenceValue = newModule;
                serializedProp.serializedObject.ApplyModifiedProperties();

                if (oldComponent != null && oldComponent != newModule) Undo.DestroyObjectImmediate(oldComponent);
            });
        }

        menu.ShowAsContext();
    }
}
#endif