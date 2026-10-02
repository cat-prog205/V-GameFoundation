namespace BlueprintFlow.BlueprintReader.Converter.TypeConversion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Converter cải tiến cho mảng 1 chiều với nhiều format hỗ trợ
    /// Hỗ trợ: "1,2,3", "1;2;3", "1|2|3", "1 2 3"
    /// </summary>
    public class EnhancedArrayConverter : DefaultTypeConverter
    {
        private readonly char[] delimiters;
        private readonly bool autoDetectDelimiter;

        public EnhancedArrayConverter(char delimiter = ',')
        {
            this.delimiters = new[] { delimiter };
            this.autoDetectDelimiter = false;
        }

        public EnhancedArrayConverter(char[] delimiters)
        {
            this.delimiters = delimiters ?? new[] { ',' };
            this.autoDetectDelimiter = false;
        }

      

        public override object ConvertFromString(string text, Type typeInfo)
        {
            if (string.IsNullOrEmpty(text)) return null;

            try
            {
                if (!typeInfo.IsArray || typeInfo.GetArrayRank() != 1)
                {
                    throw new ArgumentException($"Type {typeInfo.Name} is not a 1D array");
                }

                var elementType = typeInfo.GetElementType();
                if (elementType == null) return null;

                // Auto-detect delimiter nếu được bật
                var delimiter = autoDetectDelimiter ? DetectDelimiter(text) : delimiters[0];
                
                // Split text theo delimiter
                var stringData = text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                if (stringData.Length == 0) return null;

                // Tạo mảng
                var array = Array.CreateInstance(elementType, stringData.Length);
                var converter = CsvHelper.TypeConverterCache.GetConverter(elementType);

                // Convert từng element
                for (int i = 0; i < stringData.Length; i++)
                {
                    try
                    {
                        var trimmedValue = stringData[i].Trim();
                        var value = converter.ConvertFromString(trimmedValue, elementType);
                        array.SetValue(value, i);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to convert array element [{i}] = '{stringData[i]}' to {elementType.Name}: {ex.Message}");
                        array.SetValue(GetDefaultValue(elementType), i);
                    }
                }

                return array;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert array from text: '{text}'. Error: {ex.Message}");
                return null;
            }
        }

        public override string ConvertToString(object value, Type typeInfo)
        {
            if (value == null) return string.Empty;

            try
            {
                var array = (Array)value;
                if (array.Rank != 1) return string.Empty;

                var elementType = typeInfo.GetElementType();
                var converter = CsvHelper.TypeConverterCache.GetConverter(elementType);
                var delimiter = delimiters[0];

                var elements = new string[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    var elementValue = array.GetValue(i);
                    elements[i] = converter.ConvertToString(elementValue, elementType);
                }

                return string.Join(delimiter, elements);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert array to string: {ex.Message}");
                return string.Empty;
            }
        }

        private char DetectDelimiter(string text)
        {
            var delimiterCounts = new Dictionary<char, int>();
            
            foreach (var delimiter in delimiters)
            {
                delimiterCounts[delimiter] = text.Count(c => c == delimiter);
            }

            // Trả về delimiter xuất hiện nhiều nhất
            return delimiterCounts.OrderByDescending(x => x.Value).First().Key;
        }

        private static object GetDefaultValue(Type type)
        {
            if (type == typeof(int) || type == typeof(float) || type == typeof(double) || 
                type == typeof(short) || type == typeof(long) || type == typeof(byte))
                return 0;
            
            if (type == typeof(bool)) return false;
            if (type == typeof(string)) return string.Empty;
            
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
