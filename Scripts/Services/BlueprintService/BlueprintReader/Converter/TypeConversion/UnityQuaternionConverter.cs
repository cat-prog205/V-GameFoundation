namespace BlueprintFlow.BlueprintReader.Converter.TypeConversion
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Converter cho Unity Quaternion từ CSV
    /// Hỗ trợ format: 
    /// - XYZW (Euler angles): "0.0|90.0|0.0|0.0" (x, y, z, w)
    /// - Euler angles: "0.0|90.0|0.0" (x, y, z rotation in degrees)
    /// - Axis-Angle: "1.0|0.0|0.0|90.0" (axis x, y, z, angle in degrees)
    /// </summary>
    public class UnityQuaternionConverter : DefaultTypeConverter
    {
        private readonly char delimiter;
        private readonly QuaternionFormat format;

        public UnityQuaternionConverter(char delimiter = '|', QuaternionFormat format = QuaternionFormat.XYZW)
        {
            this.delimiter = delimiter;
            this.format = format;
        }

        public override object ConvertFromString(string text, Type typeInfo)
        {
            if (string.IsNullOrEmpty(text)) return Quaternion.identity;

            try
            {
                var stringData = text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                
                if (stringData.Length < 3)
                {
                    Debug.LogWarning($"Invalid Quaternion format: {text}. Expected at least 3 values.");
                    return Quaternion.identity;
                }

                switch (format)
                {
                    case QuaternionFormat.XYZW:
                        return ParseXYZW(stringData);
                    
                    case QuaternionFormat.EulerAngles:
                        return ParseEulerAngles(stringData);
                    
                    case QuaternionFormat.AxisAngle:
                        return ParseAxisAngle(stringData);
                    
                    default:
                        return ParseXYZW(stringData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert Quaternion from text: '{text}'. Error: {ex.Message}");
                return Quaternion.identity;
            }
        }

        public override string ConvertToString(object value, Type typeInfo)
        {
            if (value == null) return "0.0|0.0|0.0|1.0";

            try
            {
                var quaternion = (Quaternion)value;
                
                switch (format)
                {
                    case QuaternionFormat.XYZW:
                        return $"{quaternion.x:F6}{delimiter}{quaternion.y:F6}{delimiter}{quaternion.z:F6}{delimiter}{quaternion.w:F6}";
                    
                    case QuaternionFormat.EulerAngles:
                        var euler = quaternion.eulerAngles;
                        return $"{euler.x:F3}{delimiter}{euler.y:F3}{delimiter}{euler.z:F3}";
                    
                    case QuaternionFormat.AxisAngle:
                        var angle = 0f;
                        var axis = Vector3.zero;
                        quaternion.ToAngleAxis(out angle, out axis);
                        return $"{axis.x:F6}{delimiter}{axis.y:F6}{delimiter}{axis.z:F6}{delimiter}{angle:F3}";
                    
                    default:
                        return $"{quaternion.x:F6}{delimiter}{quaternion.y:F6}{delimiter}{quaternion.z:F6}{delimiter}{quaternion.w:F6}";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert Quaternion to string: {ex.Message}");
                return "0.0|0.0|0.0|1.0";
            }
        }

        private Quaternion ParseXYZW(string[] stringData)
        {
            if (stringData.Length < 4)
            {
                Debug.LogWarning("XYZW format requires 4 values (x, y, z, w). Using identity quaternion.");
                return Quaternion.identity;
            }

            var x = float.Parse(stringData[0].Trim());
            var y = float.Parse(stringData[1].Trim());
            var z = float.Parse(stringData[2].Trim());
            var w = float.Parse(stringData[3].Trim());

            return new Quaternion(x, y, z, w);
        }

        private Quaternion ParseEulerAngles(string[] stringData)
        {
            var x = float.Parse(stringData[0].Trim());
            var y = float.Parse(stringData[1].Trim());
            var z = float.Parse(stringData[2].Trim());

            return Quaternion.Euler(x, y, z);
        }

        private Quaternion ParseAxisAngle(string[] stringData)
        {
            if (stringData.Length < 4)
            {
                Debug.LogWarning("Axis-Angle format requires 4 values (axis x, y, z, angle). Using identity quaternion.");
                return Quaternion.identity;
            }

            var axisX = float.Parse(stringData[0].Trim());
            var axisY = float.Parse(stringData[1].Trim());
            var axisZ = float.Parse(stringData[2].Trim());
            var angle = float.Parse(stringData[3].Trim());

            var axis = new Vector3(axisX, axisY, axisZ);
            return Quaternion.AngleAxis(angle, axis);
        }
    }

    /// <summary>
    /// Định dạng cho Quaternion conversion
    /// </summary>
    public enum QuaternionFormat
    {
        /// <summary>
        /// Format XYZW: x, y, z, w components
        /// </summary>
        XYZW,
        
        /// <summary>
        /// Format Euler Angles: x, y, z rotation in degrees
        /// </summary>
        EulerAngles,
        
        /// <summary>
        /// Format Axis-Angle: axis x, y, z, angle in degrees
        /// </summary>
        AxisAngle
    }
}
