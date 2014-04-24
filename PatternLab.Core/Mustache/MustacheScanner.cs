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

            //TODO: #16 Implement listItems variable from PHP version
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

            regex = new Regex(@"\{\{(.*)?(listItems.)(.*)\}\}");
            template = regex.Replace(template, delegate(Match m)
            {
                var number = m.Groups[3].Value.Trim();
                var count = numbers.IndexOf(number) + 1;

                var result = new StringBuilder();

                result.Append(m.Value.Replace(string.Concat("listItems.", number),
                    count.ToString(CultureInfo.InvariantCulture)));

                return result.ToString();
            });

            return base.Scan(template);
        }
    }
}