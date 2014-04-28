using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public class MustacheScanner : Scanner
    {
        public new IEnumerable<Part> Scan(string template)
        {
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

            regex = new Regex(@"{{#\s?listItems.([a-zA-Z]*)\s?}}.*?{{/\s?listItems.([a-zA-Z]*)\s?}}", RegexOptions.Singleline);
            template = regex.Replace(template, m => m.Groups[1].Value.Trim()
                .Equals(m.Groups[2].Value.Trim(), StringComparison.InvariantCultureIgnoreCase)
                ? ReplaceListItems(m)
                : m.Value);

            regex = new Regex(@"{{\s?listItems.([a-zA-Z]*)(.*)?\s?}}");
            template = regex.Replace(template, ReplaceListItems);

            return base.Scan(template);
        }

        private static string ReplaceListItems(Match match)
        {
            var numbers = new List<string>
            {
                "one",
                "two",
                "three",
                "four",
                "five",
                "six",
                "seven",
                "eight",
                "nine",
                "ten",
                "eleven",
                "twelve"
            };

            var number = match.Groups[1].Value.Trim();
            var index = numbers.IndexOf(number);
            var result = new StringBuilder();
            var random = new Random();
            var randomNumbers = new List<int>();

            while (randomNumbers.Count <= index)
            {
                var randomNumber = random.Next(1, numbers.Count);
                if (!randomNumbers.Contains(randomNumber))
                {
                    result.Append(match.Value.Replace(string.Concat("listItems.", number),
                        randomNumber.ToString(CultureInfo.InvariantCulture)));

                    randomNumbers.Add(randomNumber);
                }
            }

            return result.ToString();
        }
    }
}