using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Nustache.Core;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Engines.Mustache
{
    /// <summary>
    /// Pattern Lab override to Nustache.Core.Scanner
    /// </summary>
    public class MustacheScanner : Scanner
    {
        /// <summary>
        /// Scans a string for Mustache tags
        /// </summary>
        /// <param name="template">The Mustache template as a string</param>
        /// <returns>A collection of found Mustache tags</returns>
        public new IEnumerable<Part> Scan(string template)
        {
            // Find tags of the format {% foo %} within templates that don't match Mustache syntax
            // and replace them with Mustache friendly tags
            var regex = new Regex(@"\{%\s?(.*)\s?%\}");
            template = regex.Replace(template, delegate(Match m)
            {
                var result = m.Value;

                switch (m.Groups[1].Value.Trim())
                {
                    case "pattern-lab-head":
                        result = "{{> header }}";
                        break;
                    case "pattern-lab-foot":
                        result = "{{> footer }}";
                        break;
                    case "patternPartial":
                        result = "{{ patternPartial }}";
                        break;
                    case "lineage":
                        result = "{{{ lineage }}}";
                        break;
                    case "lineageR":
                        result = "{{{ lineageR }}}";
                        break;
                    case "patternState":
                        result = "{{ patternState }}";
                        break;
                    case "cssEnabled":
                        result = "{{ cssEnabled }}";
                        break;
                }

                return result;
            });

            // Parse template for listItems sections and replace with contents from listItems.json
            regex = new Regex(@"{{#\s?listItems.([a-zA-Z]*)\s?}}.*?{{/\s?listItems.([a-zA-Z]*)\s?}}", RegexOptions.Singleline);
            template = regex.Replace(template, m => m.Groups[1].Value.Trim()
                .Equals(m.Groups[2].Value.Trim(), StringComparison.InvariantCultureIgnoreCase)
                ? ReplaceListItems(m)
                : m.Value);

            // Parse template for listItems variables and replace with contents from listItems.json
            regex = new Regex(@"{{\s?listItems.([a-zA-Z]*)(.*)?\s?}}");
            template = regex.Replace(template, ReplaceListItems);

            return base.Scan(template);
        }

        /// <summary>
        /// Replace instance of listItems.x with x instance of values from listItems.json
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        private static string ReplaceListItems(Match match)
        {
            // Determine how may listItem variables need to be generated based on the number name
            var number = match.Groups[1].Value.Trim();
            var index = PatternProvider.ListItemVariables.IndexOf(number);
            var result = new StringBuilder();
            var random = new Random();
            var randomNumbers = new List<int>();

            // For the desired number of listItems randomly replace with the values from listitems.json
            while (randomNumbers.Count <= index)
            {
                // Check that the random number hasn't already been used
                var randomNumber = random.Next(1, PatternProvider.ListItemVariables.Count);
                if (randomNumbers.Contains(randomNumber)) continue;

                // E.g. replace {{ listItems.two }} with {{ 1 }}{{ 2 }}
                result.Append(match.Value.Replace(string.Concat("listItems.", number),
                    randomNumber.ToString(CultureInfo.InvariantCulture)));

                randomNumbers.Add(randomNumber);
            }

            return result.ToString();
        }
    }
}