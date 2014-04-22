using System.Collections.Generic;
using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public class MustacheScanner : Scanner
    {
        public new IEnumerable<Part> Scan(string template)
        {
            template = template.Replace("{% pattern-lab-head %}", "{{> header }}");
            template = template.Replace("{% pattern-lab-foot %}", "{{> footer }}");
            template = template.Replace("{% patternPartial %}", "{{ patternPartial }}");
            template = template.Replace("{% lineage %}", "{{{ lineage }}}");
            template = template.Replace("{% lineageR %}", "{{{ lineageR }}}");
            template = template.Replace("{% patternState %}", "{{ patternState }}");
            template = template.Replace("{% cssEnabled %}", "{{ cssEnabled }}");

            return base.Scan(template);
        }
    }
}