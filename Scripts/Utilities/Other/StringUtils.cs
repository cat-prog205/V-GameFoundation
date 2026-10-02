using UnityEngine;

namespace VGameFoundation.Script.Utilities
{
    using System.Text.RegularExpressions;

    public static class StringUtils
    {
        public static string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
        
        public static string FormatEnumString(string input)
        {
            input = input.Replace('_', ' ');
            return Regex.Replace(input, "(?<!^)([A-Z])", " $1");
        }
    }
}