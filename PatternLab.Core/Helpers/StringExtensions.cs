using System.Text.RegularExpressions;
using System.Threading;

namespace PatternLab.Core.Helpers
{
    public static class StringExtensions
    {
        public static string StripOrdinals(this string value)
        {
            return Regex.Replace(value, @"[\d][\d]+[\-]", string.Empty);
        }

        public static string ToDisplayCase(this string value)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(value.ToLower()).Replace('-', ' ').Trim();
        }
    }
}