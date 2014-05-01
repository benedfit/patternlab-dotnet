using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public class MustacheTemplate : Template
    {
        private readonly Dictionary<string, object> _parameters; 
        
        public MustacheTemplate(Dictionary<string, object> parameters = null)
        {
            _parameters = parameters;
        }

        public new void Load(TextReader reader)
        {
            var template = reader.ReadToEnd();
            if (_parameters != null && _parameters.Any())
            {
                template = _parameters.Aggregate(template,
                    (current, parameter) =>
                        Regex.Replace(current, @"{{\s?" + parameter.Key + @"\s?}}", parameter.Value.ToString()));
            }

            var scanner = new MustacheScanner();
            var parser = new Parser();

            parser.Parse(this, scanner.Scan(template));
        }
    }
}