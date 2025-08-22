using System;
using System.Collections.Generic;
using System.Linq;

namespace Playmaykr.Utils.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if a string is Null or white space
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string val)
        {
            return string.IsNullOrWhiteSpace(val);
        }

        /// <summary>
        /// Checks if a string is Null or empty
        /// </summary>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Checks if a string contains null, empty, or white space.
        /// </summary>
        public static bool IsBlank(this string val)
        {
            return val.IsNullOrWhiteSpace() || val.IsNullOrEmpty();
        }

        /// <summary>
        /// Checks if a string is null and returns an empty string if it is.
        /// </summary>
        public static string OrEmpty(this string val)
        {
            return val ?? string.Empty;
        }

        /// <summary>
        /// Shortens a string to the specified maximum length. If the string's length
        /// is less than the maxLength, the original string is returned.
        /// </summary>
        public static string Shorten(this string val, int maxLength)
        {
            if (val.IsBlank())
            {
                return val;
            }
            return val.Length <= maxLength ? val : val[..maxLength];
        }

        /// <summary>
        /// Slices a string from the start index to the end index.
        /// </summary>
        /// <result>The sliced string.</result>
        public static string Slice(this string val, int startIndex, int endIndex)
        {
            if (val.IsBlank())
            {
                throw new ArgumentNullException(nameof(val), "Value cannot be null or empty.");
            }

            if (startIndex < 0 || startIndex > val.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            // If the end index is negative, it will be counted from the end of the string.
            endIndex = endIndex < 0 ? val.Length + endIndex : endIndex;

            if (endIndex < 0 || endIndex < startIndex || endIndex > val.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(endIndex));
            }

            return val.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Converts the input string to an alphanumeric string, optionally allowing periods.
        /// </summary>
        /// <param name="input">The input string to be converted.</param>
        /// <param name="allowPeriods">A boolean flag indicating whether periods should be allowed in the output string.</param>
        /// <returns>
        /// A new string containing only alphanumeric characters, underscores, and optionally periods.
        /// If the input string is null or empty, an empty string is returned.
        /// </returns>
        public static string ConvertToAlphanumeric(this string input, bool allowPeriods = false)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            List<char> filteredChars = new();
            int lastValidIndex = -1;

            // Iterate over the input string, filtering and determining valid start/end indices
            foreach (char character in input.Where(character => char
                             .IsLetterOrDigit(character) || character == '_' || (allowPeriods && character == '.'))
                         .Where(character => filteredChars.Count != 0 || (!char.IsDigit(character) && character != '.')))
            {

                filteredChars.Add(character);
                lastValidIndex = filteredChars.Count - 1; // Update lastValidIndex for valid characters
            }

            // Remove trailing periods
            while (lastValidIndex >= 0 && filteredChars[lastValidIndex] == '.')
            {
                lastValidIndex--;
            }
    
            // Return the filtered string
            return lastValidIndex >= 0 ? new string(filteredChars.ToArray(), 0, lastValidIndex + 1) : string.Empty;
        }

        #region Rich text formatting, for Unity UI elements that support rich text.

        public static string RichColor(this string text, string color)
        {
            return $"<color={color}>{text}</color>";
        }
        
        public static string RichSize(this string text, int size)
        {
            return $"<size={size}>{text}</size>";
        }
        
        public static string RichBold(this string text)
        {
            return $"<b>{text}</b>";
        }
        
        public static string RichItalic(this string text)
        {
            return $"<i>{text}</i>";
        }
        
        public static string RichUnderline(this string text)
        {
            return $"<u>{text}</u>";
        }
        
        public static string RichStrikethrough(this string text)
        {
            return $"<s>{text}</s>";
        }
        
        public static string RichFont(this string text, string font)
        {
            return $"<font={font}>{text}</font>";
        }
        
        public static string RichAlign(this string text, string align)
        {
            return $"<align={align}>{text}</align>";
        }
        
        public static string RichGradient(this string text, string color1, string color2)
        {
            return $"<gradient={color1},{color2}>{text}</gradient>";
        }
        
        public static string RichRotation(this string text, float angle)
        {
            return $"<rotate={angle}>{text}</rotate>";
        }
        
        public static string RichSpace(this string text, float space)
        {
            return $"<space={space}>{text}</space>";
        }

        #endregion
    }
}