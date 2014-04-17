using System.IO;
using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public class MustacheTemplate : Template
    {
        public new void Load(TextReader reader)
        {
            var template = reader.ReadToEnd();
            var scanner = new MustacheScanner();
            var parser = new Parser();

            parser.Parse(this, scanner.Scan(template));
        }
    }
}