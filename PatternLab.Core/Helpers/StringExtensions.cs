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
        /// Strips pattern parameters from a string
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>A string without pattern parameters</returns>
        public static string StripPatternParameters(this string value)
        {
            // Strip everything after first colon
            var fragments = value.Split(new[] { PatternProvider.IdentifierModifier }, StringSplitOptions.RemoveEmptyEntries);
            if (fragments.Length > 1)
            {
                value = fragments[0];
            }

            // Strip everything after first open bracket
            fragments = value.Split(new[] { PatternProvider.IdentifierParameters }, StringSplitOptions.RemoveEmptyEntries);
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
        /// Gets a collection of Pattern Parametrrs from a string - http://patternlab.io/docs/pattern-parameters.html
        /// </summary>
        /// <param name="value">The string</param>
        /// <returns>The collection of pattern parameters</returns>
        public static Dictionary<string, object> ToPatternParameters(this string value)
        {
            var parameters = new Dictionary<string, object>();
            
            // Check whether or not the string contains pattern parameters
            value = value.Replace(value.StripPatternParameters(), string.Empty).Trim();
            if (string.IsNullOrEmpty(value)) return parameters;

            var regex = new Regex(@"^(:([^(]+))?(\((.*)\))?$");
            var match = regex.Match(value);

            // Get styleModifier from the first value after the colon
            var styleModifier = match.Groups[2].Value.Replace(PatternProvider.IdentifierModifierSeparator, ' ').Trim();
            if (!string.IsNullOrEmpty(styleModifier))
            {
                parameters.Add(PatternProvider.KeywordModifier, styleModifier);
            }

            value = match.Groups[4].Value.Trim();
            if (string.IsNullOrEmpty(value)) return parameters;

            // Parse the pattern parameters from comma delimited, colon seperated key value pairs
            var keyValuePairs = value.Split(new[] {PatternProvider.IdentifierDelimiter},
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var keyValuePair in keyValuePairs)
            {
                var parameter = keyValuePair.Split(new[] {PatternProvider.IdentifierModifier},
                    StringSplitOptions.RemoveEmptyEntries);
                var parameterKey = parameter.Length > 0 ? parameter[0].Trim() : string.Empty;
                var parameterValue = parameter.Length > 1 ? parameter[1].Trim() : string.Empty;

                if (string.IsNullOrEmpty(parameterKey) || string.IsNullOrEmpty(parameterValue)) continue;

                if (parameterKey.Equals(PatternProvider.KeywordListItems, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Handle listItems replacement
                    var format = string.Concat(parameterKey, ".{0}");

                    int index;
                    if (int.TryParse(parameterValue, out index))
                    {
                        if (index > 0 && index <= PatternProvider.ListItemVariables.Count)
                        {
                            parameterValue = PatternProvider.ListItemVariables[index - 1];
                        }
                    }

                    if (PatternProvider.ListItemVariables.Contains(parameterValue))
                    {
                        foreach (var listItemVariable in PatternProvider.ListItemVariables)
                        {
                            parameters.Add(string.Format(format, listItemVariable),
                                string.Format(format, parameterValue));
                        }
                    }
                }
                else
                {
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
            }

            return parameters;
        }
    }
}