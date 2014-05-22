using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Razor.Tokenizer.Symbols;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Razor
{
    /// <summary>
    /// The Razor (.cshtml) pattern engine
    /// </summary>
    public class RazorPatternEngine : IPatternEngine
    {
        private List<string> _replacedKeys;

        /// <summary>
        /// The file extension of pattern templates read by pattern engine
        /// </summary>
        /// <returns>.cshtml</returns>
        public string Extension()
        {
            return ".cshtml";
        }

        /// <summary>
        /// The Regex pattern for finding lineages in templates read by pattern engine
        /// </summary>
        /// <returns>@Include\(""(.*?)""\)</returns>
        public string LineagePattern()
        {
            return @"@Include\(""(.*?)""";
        }

        /// <summary>
        /// The name of the pattern engine
        /// </summary>
        /// <returns>Razor</returns>
        public string Name()
        {
            return "Razor";
        }

        /// <summary>
        /// Parses a string against a data collection using Razor
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <param name="data">The data collection</param>
        /// <returns>The parsed string</returns>
        public string Parse(Pattern pattern, Dictionary<string, object> data)
        {
            // Replace keys with hyphens in data collection
            ParseKeys(data);

            // Convert data collection to dynamic
            dynamic model = new RazorDynamicDictionary(data);

            // Replace keys with hyphens, and those that are reserved by C# in template
            var template = _replacedKeys.Aggregate(pattern.Html,
                (current, key) =>
                    current.Replace(string.Concat(".", key),
                        string.Concat(".", PatternProvider.IdentifierHidden,
                            key.Replace(PatternProvider.IdentifierSpace, PatternProvider.IdentifierHidden))));

            return RazorParser.Parse(template, model, pattern.Partial);
        }

        /// <summary>
        /// Replaces hyphens with underscores in the data collection
        /// </summary>
        /// <param name="dictionary">The data collection</param>
        private void ParseKeys(IDictionary<string, object> dictionary)
        {
            if (_replacedKeys == null)
            {
                _replacedKeys = new List<string>();
            }

            foreach (
                var key in
                    dictionary.Keys.Where(
                        key =>
                            key.Contains(PatternProvider.IdentifierSpace) ||
                            Enum.GetNames(typeof (CSharpKeyword))
                                .Contains(key, StringComparer.InvariantCultureIgnoreCase))
                        .Where(key => !_replacedKeys.Contains(key)))
            {
                // If key contains hyphen, or is a C# keyworkd, add it to the list of replacements
                _replacedKeys.Add(key);
            }

            foreach (var key in _replacedKeys)
            {
                if (!dictionary.ContainsKey(key)) continue;

                // If collection contains key remove the current key and add in a parsed one
                var value = dictionary[key];
                var newKey = string.Concat(PatternProvider.IdentifierHidden,
                    key.Replace(PatternProvider.IdentifierSpace, PatternProvider.IdentifierHidden));

                dictionary.Remove(key);
                dictionary.Add(newKey, value);
            }

            foreach (var value in dictionary.Values.OfType<IDictionary<string, object>>())
            {
                // If the value is a dictionary, parse it too
                ParseKeys(value);
            }
        }
    }
}