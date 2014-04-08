using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace PatternLab.Core.Helpers
{
    public static class StringExtensions
    {
        public static string ToTileCase(this string value)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(value.ToLower()).Trim();
        }
    }
}