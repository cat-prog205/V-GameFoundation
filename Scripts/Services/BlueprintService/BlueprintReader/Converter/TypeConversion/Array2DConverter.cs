namespace BlueprintFlow.BlueprintReader.Converter.TypeConversion
{
    using System;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Converter cho mảng 2 chiều (2D Array) từ CSV
    /// Hỗ trợ format: "1,2,3;4,5,6;7,8,9" hoặc "1|2|3,4|5|6,7|8|9"
    /// </summary>
    public class Array2DConverter : DefaultTypeConverter
    {
        private readonly char rowDelimiter;    // Delimiter giữa các hàng
        private readonly char columnDelimiter; // Delimiter giữa các cột

        public Array2DConverter(char rowDelimiter = ';', char columnDelimiter = ',')
        {
            this.rowDelimiter = rowDelimiter;
            this.columnDelimiter = columnDelimiter;
        }

        public override object ConvertFromString(string text, Type typeInfo)
        {
            if (string.IsNullOrEmpty(text)) return null;

            try
            {
                // Kiểm tra xem có phải mảng 2 chiều không
                if (!typeInfo.IsArray || typeInfo.GetArrayRank() != 2)
                {
                    throw new ArgumentException($"Type {typeInfo.Name} is not a 2D array");
                }

                var elementType = typeInfo.GetElementType();
                if (elementType == null) return null;

                // Split theo row delimiter trước
                var rows = text.Split(rowDelimiter, StringSplitOptions.RemoveEmptyEntries);
                if (rows.Length == 0) return null;

                // Split từng row theo column delimiter
                var columns = rows[0].Split(columnDelimiter, StringSplitOptions.RemoveEmptyEntries);
                var rowCount = rows.Length;
                var colCount = columns.Length;

                // Tạo mảng 2 chiều
                var array = Array.CreateInstance(elementType, rowCount, colCount);
                var converter = CsvHelper.TypeConverterCache.GetConverter(elementType);

                // Parse từng cell
                for (int i = 0; i < rowCount; i++)
                {
                    var rowData = rows[i].Split(columnDelimiter, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < Math.Min(colCount, rowData.Length); j++)
                    {
                        try
                        {
                            var value = converter.ConvertFromString(rowData[j].Trim(), elementType);
                            array.SetValue(value, i, j);
                        }
                        catch (Exception ex)
                        {
                            // Log lỗi và set giá trị mặc định
                            Debug.LogWarning($"Failed to convert cell [{i},{j}] = '{rowData[j]}' to {elementType.Name}: {ex.Message}");
                            array.SetValue(GetDefaultValue(elementType), i, j);
                        }
                    }
                }

                return array;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert 2D array from text: '{text}'. Error: {ex.Message}");
                return null;
            }
        }

        public override string ConvertToString(object value, Type typeInfo)
        {
            if (value == null) return string.Empty;

            try
            {
                var array = (Array)value;
                if (array.Rank != 2) return string.Empty;

                var elementType = typeInfo.GetElementType();
                var converter = CsvHelper.TypeConverterCache.GetConverter(elementType);
                var rows = new string[array.GetLength(0)];

                for (int i = 0; i < array.GetLength(0); i++)
                {
                    var columns = new string[array.GetLength(1)];
                    for (int j = 0; j < array.GetLength(1); j++)
                    {
                        var cellValue = array.GetValue(i, j);
                        columns[j] = converter.ConvertToString(cellValue, elementType);
                    }
                    rows[i] = string.Join(columnDelimiter, columns);
                }

                return string.Join(rowDelimiter, rows);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to convert 2D array to string: {ex.Message}");
                return string.Empty;
            }
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
