namespace BlueprintFlow.BlueprintReader.Converter.TypeConversion
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Converter cho Unity Color từ CSV
    /// Hỗ trợ format: 
    /// - RGBA (0.0-1.0): "1.0|0.5|0.2|1.0"
    /// - RGBA (0-255): "255|128|51|255"
    /// - Hex RGB: "#FF8000", "FF8000"
    /// - Hex RGBA: "#FF8000FF", "FF8000FF"
    /// </summary>
    public class UnityColorConverter : DefaultTypeConverter
    {
        private readonly char delimiter;
        private readonly bool use255Range; // true = 0-255, false = 0.0-1.0

        public UnityColorConverter(char delimiter = '|', bool use255Range = false)
        {
            this.delimiter = delimiter;
            this.use255Range = use255Range;
        }

        public override object ConvertFromString(string text, Type typeInfo)
        {
            if (string.IsNullOrEmpty(text)) return Color.white;

            try
            {
                // Kiểm tra xem có phải hex color không
                if (IsHexColor(text))
                {
                    return ParseHexColor(text);
                }

                // Parse RGBA format
                return ParseRgbaColor(text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert color from text: '{text}'. Error: {ex.Message}");
                return Color.white;
            }
        }

        public override string ConvertToString(object value, Type typeInfo)
        {
            if (value == null) return "1.0|1.0|1.0|1.0";

            try
            {
                var color = (Color)value;
                
                if (use255Range)
                {
                    // Convert to 0-255 range
                    var r = Mathf.RoundToInt(color.r * 255);
                    var g = Mathf.RoundToInt(color.g * 255);
                    var b = Mathf.RoundToInt(color.b * 255);
                    var a = Mathf.RoundToInt(color.a * 255);
                    
                    return $"{r}{delimiter}{g}{delimiter}{b}{delimiter}{a}";
                }
                else
                {
                    // Use 0.0-1.0 range
                    return $"{color.r:F3}{delimiter}{color.g:F3}{delimiter}{color.b:F3}{delimiter}{color.a:F3}";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert color to string: {ex.Message}");
                return "1.0|1.0|1.0|1.0";
            }
        }

        private bool IsHexColor(string text)
        {
            var trimmed = text.Trim();
            
            // Kiểm tra format hex: #RRGGBB, #RRGGBBAA, RRGGBB, RRGGBBAA
            if (trimmed.StartsWith("#"))
            {
                trimmed = trimmed.Substring(1);
            }
            
            // Hex phải có 6 hoặc 8 ký tự và chỉ chứa 0-9, A-F, a-f
            return (trimmed.Length == 6 || trimmed.Length == 8) && 
                   System.Text.RegularExpressions.Regex.IsMatch(trimmed, "^[0-9A-Fa-f]+$");
        }

        private Color ParseHexColor(string hexText)
        {
            var hex = hexText.TrimStart('#').ToUpper();
            
            if (hex.Length == 6)
            {
                // RGB format
                var r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
                var g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
                var b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
                return new Color(r, g, b, 1.0f);
            }
            else if (hex.Length == 8)
            {
                // RGBA format
                var r = Convert.ToInt32(hex.Substring(0, 2), 16) / 255f;
                var g = Convert.ToInt32(hex.Substring(2, 2), 16) / 255f;
                var b = Convert.ToInt32(hex.Substring(4, 2), 16) / 255f;
                var a = Convert.ToInt32(hex.Substring(6, 2), 16) / 255f;
                return new Color(r, g, b, a);
            }
            else
            {
                Debug.LogWarning($"Invalid hex color format: {hexText}. Expected 6 or 8 characters.");
                return Color.white;
            }
        }

        private Color ParseRgbaColor(string text)
        {
            var stringData = text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            
            if (stringData.Length < 3)
            {
                Debug.LogWarning($"Invalid RGBA color format: {text}. Expected at least 3 values (RGB).");
                return Color.white;
            }

            // Parse RGB values
            var r = ParseColorComponent(stringData[0]);
            var g = ParseColorComponent(stringData[1]);
            var b = ParseColorComponent(stringData[2]);
            
            // Parse Alpha (optional, default = 1.0)
            var a = stringData.Length >= 4 ? ParseColorComponent(stringData[3]) : 1.0f;

            return new Color(r, g, b, a);
        }

        private float ParseColorComponent(string component)
        {
            var value = float.Parse(component.Trim());
            
            if (use255Range)
            {
                // Convert from 0-255 to 0.0-1.0
                return Mathf.Clamp01(value / 255f);
            }
            
            // Already in 0.0-1.0 range
            return Mathf.Clamp01(value);
        }
    }
}
