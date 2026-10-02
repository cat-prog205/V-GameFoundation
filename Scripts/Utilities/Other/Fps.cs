namespace VGameFoundation.Script.Utilities
{
    using UnityEngine;

    public class Fps : MonoBehaviour
    {
        private Color textColor = Color.red;
        [Range(1, 10)] 
        private int sizePercentage = 2; 
        private Vector2 offset = new Vector2(5, 5);
        
        private float deltaTime = 0.0f;
        
        private void Update()
        {
            this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;

            var style = new GUIStyle();

            int fontSize = h * sizePercentage / 100;
            var rect     = new Rect(offset.x, offset.y, w, fontSize * 1.5f); 

            style.alignment        = TextAnchor.UpperLeft;
            style.fontSize         = fontSize;
            style.normal.textColor = this.textColor;

            var msec = this.deltaTime * 1000.0f;
            var fps  = 1.0f / this.deltaTime;
            var text = $"{msec:0.0} ms ({fps:0.} fps)";

            GUI.Label(rect, text, style);
        }
    }
}