using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.CompilerServices;
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

            regex = new Regex(@"{{#\s?listItems.([a-zA-Z]*)\s?}}.*?{{/\s?listItems.([a-zA-Z]*)\s?}}", RegexOptions.Singleline);
            template = regex.Replace(template, delegate(Match m)
            {
                if (m.Groups[1].Value.Trim()
                    .Equals(m.Groups[2].Value.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    var number = m.Groups[1].Value.Trim();
                    var count = numbers.IndexOf(number) + 1;

                    var result = new StringBuilder();

                    for (var i = 1; i <= count; i++)
                    {
                        result.Append(m.Value.Replace(string.Concat("listItems.", number),
                            i.ToString(CultureInfo.InvariantCulture)));
                    }

                    return result.ToString();
                }

                return m.Value;
            });

            regex = new Regex(@"{{\s?listItems.([a-zA-Z]*)(.*)?\s?}}");
            template = regex.Replace(template, delegate(Match m)
            {
                var number = m.Groups[1].Value.Trim();
                var count = numbers.IndexOf(number) + 1;

                var result = new StringBuilder();

                for (var i = 1; i <= count; i++)
                {
                    result.Append(m.Value.Replace(string.Concat("listItems.", number),
                        i.ToString(CultureInfo.InvariantCulture)));
                }

                return result.ToString();
            });

            return base.Scan(template);
        }
    }
}