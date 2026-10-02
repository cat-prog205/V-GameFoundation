#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build; 

namespace VGameFoundation.Script.Services.MobiCommon.Vibration
{
    using VGameFoundation.Scripts.Utilities.LogService;
    
    public static class NiceVibrationsSetup
    {
        private const string DEFINE_SYMBOL = "LOFELT_NICEVIBRATIONS";

        private static NamedBuildTarget GetSelectedTarget()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            return NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
        }

        [MenuItem("VGameFoundation/Vibration/Enable Nice Vibrations Support")]
        public static void EnableNiceVibrationsSupport()
        {
            var target = GetSelectedTarget();
            var defines = PlayerSettings.GetScriptingDefineSymbols(target);
            
            if (!defines.Contains(DEFINE_SYMBOL))
            {
                if (!string.IsNullOrEmpty(defines))
                    defines += ";" + DEFINE_SYMBOL;
                else
                    defines = DEFINE_SYMBOL;
                
                PlayerSettings.SetScriptingDefineSymbols(target, defines);
                LogService.Log($"[NiceVibrationsSetup] Enabled {DEFINE_SYMBOL} for {target}");
            }
            else
            {
                LogService.Log($"[NiceVibrationsSetup] {DEFINE_SYMBOL} already enabled for {target}");
            }
        }

        [MenuItem("VGameFoundation/Vibration/Disable Nice Vibrations Support")]
        public static void DisableNiceVibrationsSupport()
        {
            var target = GetSelectedTarget();
            var defines = PlayerSettings.GetScriptingDefineSymbols(target);
            
            if (defines.Contains(DEFINE_SYMBOL))
            {
                defines = defines.Replace(DEFINE_SYMBOL, "").Replace(";;", ";").Trim(';');
                PlayerSettings.SetScriptingDefineSymbols(target, defines);
                LogService.Log($"[NiceVibrationsSetup] Disabled {DEFINE_SYMBOL} for {target}");
            }
            else
            {
                LogService.Log($"[NiceVibrationsSetup] {DEFINE_SYMBOL} already disabled for {target}");
            }
        }

        [MenuItem("VGameFoundation/Vibration/Check Nice Vibrations Status")]
        public static void CheckNiceVibrationsStatus()
        {
            var target = GetSelectedTarget();
            var defines = PlayerSettings.GetScriptingDefineSymbols(target);
            var isEnabled = defines.Contains(DEFINE_SYMBOL);
            
            LogService.Log($"[NiceVibrationsSetup] Status for {target}: {(isEnabled ? "ENABLED" : "DISABLED")}");
            LogService.Log($"[NiceVibrationsSetup] Current defines: {defines}");
        }
    }
}
#endif