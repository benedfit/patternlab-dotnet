using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public class MustacheScanner : Scanner
    {
        public new IEnumerable<Part> Scan(string template)
        {
            //TODO: #16 Implement listItems variable from PHP version

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

            return base.Scan(template);
        }
    }
}