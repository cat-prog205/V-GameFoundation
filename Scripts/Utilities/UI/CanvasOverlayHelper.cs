namespace V_GameFoundation.Scripts.Utilities.UI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public enum CanvasOverlayType
    {
        MainUI       = 0,
        Popup        = 100,
        Notification = 500,
        Transition   = 999,
        Debug        = 1000
    }

    public class CanvasOverlayHelper
    {
        private static Dictionary<CanvasOverlayType, Canvas> canvasMap = new();

        public static Canvas GetCanvas(CanvasOverlayType type)
        {
            if (canvasMap.ContainsKey(type))
            {
                if (canvasMap[type] != null) return canvasMap[type];

                canvasMap.Remove(type);
            }

            var go     = new GameObject($"[CanvasOverlay_{type}]");
            var canvas = go.AddComponent<Canvas>();
            go.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvas.sortingOrder = (int)type;

            go.layer = LayerMask.NameToLayer("UI");

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode            = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution    = new Vector2(1920, 1080);
            scaler.screenMatchMode        = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight     = 0.5f;
            scaler.referencePixelsPerUnit = 100;

            Object.DontDestroyOnLoad(go);

            canvasMap.Add(type, canvas);

            return canvas;
        }
    }
}