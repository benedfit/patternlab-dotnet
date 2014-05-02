using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Helpers
{
    public static class StringExtensions
    {
        public static string StripOrdinals(this string value)
        {
            return Regex.Replace(value, @"[\d][\d]+[\-]", string.Empty);
        }

        public static string StripPatternParameters(this string value)
        {
            var fragments = value.Split(new[] { PatternProvider.IdentifierParameters }, StringSplitOptions.RemoveEmptyEntries);
            if (fragments.Length > 1)
            {
                value = fragments[0];
            }

            return value.Trim();
        }

        public static string ToDisplayCase(this string value)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(value.ToLower()).Replace(PatternProvider.IdentifierSpace, ' ').Trim();
        }

        public static Dictionary<string, object> ToPatternParameters(this string value)
        {
            var parameters = new Dictionary<string, object>();
            
            value = value.Replace(value.StripPatternParameters(), string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return parameters;

            var regex = new Regex(@"^:([^(]+)(\((.*)\))?$");
            var match = regex.Match(value);

            var styleModifier = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(styleModifier))
            {
                parameters.Add("styleModifier", styleModifier);
            }

            value = match.Groups[3].Value.Trim();
            if (string.IsNullOrEmpty(value)) return parameters;

            var keyValuePairs = value.Split(new[] {PatternProvider.IdentifierDelimiter},
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var keyValuePair in keyValuePairs)
            {
                var parameter = keyValuePair.Split(new[] {PatternProvider.IdentifierParameters},
                    StringSplitOptions.RemoveEmptyEntries);
                var parameterKey = parameter.Length > 0 ? parameter[0].Trim() : string.Empty;
                var parameterValue = parameter.Length > 1 ? parameter[1].Trim() : string.Empty;

                if (string.IsNullOrEmpty(parameterKey) || string.IsNullOrEmpty(parameterValue)) continue;

                if (parameters.ContainsKey(parameterKey))
                {
                    parameters[parameterKey] = parameterValue;
                }
                else
                {
                    parameters.Add(parameterKey, parameterValue);
                }
            }

            return parameters;
        }
    }
}