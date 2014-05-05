using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Helpers
{
    /// <summary>
    /// Pattern Lab extension methods for strings
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Strips leading digits and hyphen from strings
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>The string without leading digits and hyphen</returns>
        public static string StripOrdinals(this string value)
        {
            return Regex.Replace(value, @"[\d][\d]+[\-]", string.Empty);
        }

        /// <summary>
        /// Strips Pattern Parameters from a string
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>A string without Pattern Parameters</returns>
        public static string StripPatternParameters(this string value)
        {
            // String everything after first colon
            var fragments = value.Split(new[] { PatternProvider.IdentifierParameters }, StringSplitOptions.RemoveEmptyEntries);
            if (fragments.Length > 1)
            {
                value = fragments[0];
            }

            return value.Trim();
        }

        /// <summary>
        /// Formats a string to title case
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>The string in title case</returns>
        public static string ToDisplayCase(this string value)
        {
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;
            
            // Covert to title case and replace hyphens with spaces
            return textInfo.ToTitleCase(value.ToLower()).Replace(PatternProvider.IdentifierSpace, ' ').Trim();
        }

        /// <summary>
        /// Gets a collection of Pattern Parametrrs from a string
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>The collection of Pattern Parameters</returns>
        public static Dictionary<string, object> ToPatternParameters(this string value)
        {
            var parameters = new Dictionary<string, object>();
            
            // Check string contains Pattern Parameters
            value = value.Replace(value.StripPatternParameters(), string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return parameters;

            var regex = new Regex(@"^:([^(]+)(\((.*)\))?$");
            var match = regex.Match(value);

            // Get styleModifier from the first value after the colon
            var styleModifier = match.Groups[1].Value.Trim();
            if (!string.IsNullOrEmpty(styleModifier))
            {
                parameters.Add("styleModifier", styleModifier);
            }

            value = match.Groups[3].Value.Trim();
            if (string.IsNullOrEmpty(value)) return parameters;

            // Parse the Pattern Parameters from comma delimited, colon seperated key value pairs
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
                    // Handle any duplicates by updating existing values
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