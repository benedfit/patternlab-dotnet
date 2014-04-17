using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public class MustacheScanner : Scanner
    {
        private static readonly Regex DelimitersRegex = new Regex(@"^=\s*(\S+)\s+(\S+)\s*=$");

        public new IEnumerable<Part> Scan(string template)
        {
            template = template.Replace("{% pattern-lab-head %}", "{{> header }}");
            template = template.Replace("{% pattern-lab-foot %}", "{{> footer }}");
            template = template.Replace("{% patternPartial %}", "{{ patternPartial }}");
            template = template.Replace("{% lineage %}", "{{ lineage }}");
            template = template.Replace("{% lineageR %}", "{{ lineageR }}");
            template = template.Replace("{% patternState %}", "{{ patternState }}");
            template = template.Replace("{% cssEnabled %}", "{{ cssEnabled }}");

            var regex = MakeRegex("{{", "}}");
            var i = 0;
            var lineEnded = false;

            while (true)
            {
                Match m;

                if ((m = regex.Match(template, i)).Success)
                {
                    var previousLiteral = template.Substring(i, m.Index - i);

                    var leadingWhiteSpace = m.Groups[1];
                    var leadingLineEnd = m.Groups[2];
                    var leadingWhiteSpaceOnly = m.Groups[3];
                    var marker = m.Groups[4].Value.Trim();
                    var trailingWhiteSpace = m.Groups[5];
                    var trailingLineEnd = m.Groups[6];

                    var isStandalone = (leadingLineEnd.Success || (lineEnded && m.Index == i)) &&
                                       trailingLineEnd.Success;

                    Part part = null;

                    switch (marker[0])
                    {
                        case '=':
                        {
                            var delimiters = DelimitersRegex.Match(marker);

                            if (delimiters.Success)
                            {
                                var start = delimiters.Groups[1].Value;
                                var end = delimiters.Groups[2].Value;
                                regex = MakeRegex(start, end);
                            }
                        }
                            break;
                        case '#':
                            part = new Block(marker.Substring(1).Trim());
                            break;
                        case '^':
                            part = new InvertedBlock(marker.Substring(1).Trim());
                            break;
                        case '<':
                            part = new TemplateDefinition(marker.Substring(1).Trim());
                            break;
                        case '/':
                            part = new EndSection(marker.Substring(1).Trim());
                            break;
                        case '>':
                            part = new TemplateInclude(marker.Substring(1).Trim(),
                                lineEnded || i == 0 ? leadingWhiteSpaceOnly.Value : null);
                            break;
                        default:
                            if (marker[0] != '!')
                            {
                                if (marker == "else")
                                {
                                    part = new Block(marker);
                                }
                                else
                                {
                                    part = new VariableReference(marker);
                                    isStandalone = false;
                                }
                            }
                            break;
                    }

                    if (!isStandalone)
                    {
                        previousLiteral += leadingWhiteSpace;
                    }
                    else
                    {
                        previousLiteral += leadingLineEnd;

                        if (part is TemplateInclude)
                        {
                            previousLiteral += leadingWhiteSpaceOnly;
                        }
                    }

                    if (previousLiteral != "")
                    {
                        yield return new LiteralText(previousLiteral);
                    }

                    if (part != null)
                    {
                        yield return part;
                    }

                    i = m.Index + m.Length;

                    if (!isStandalone)
                    {
                        i -= trailingWhiteSpace.Length;
                    }

                    lineEnded = trailingLineEnd.Success;
                }
                else
                {
                    break;
                }
            }

            if (i >= template.Length) yield break;

            var remainingLiteral = template.Substring(i);

            yield return new LiteralText(remainingLiteral);
        }

        private static Regex MakeRegex(string start, string end)
        {
            return new Regex(
                @"((^|\r?\n)?([\r\t\v ]*))" +
                Regex.Escape(start) +
                @"([\{]?[^" + Regex.Escape(end.Substring(0, 1)) + @"]+?\}?)" +
                Regex.Escape(end) +
                @"([\r\t\v ]*(\r?\n|$)?)"
                );
        }
    }
}