using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nustache.Core;

namespace PatternLab.Core.Engines.Mustache
{
    /// <summary>
    /// Pattern Lab overrides to Nustache.Core.Template
    /// </summary>
    public class MustacheTemplate : Template
    {
        private readonly Dictionary<string, object> _parameters; 
        
        /// <summary>
        /// Initialises a new Template with a collection of Pattern Parameters
        /// </summary>
        /// <param name="parameters">The Pattern Parameters</param>
        public MustacheTemplate(Dictionary<string, object> parameters)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Loads a Template from a TextReader
        /// </summary>
        /// <param name="reader">The TextReader containing the Template</param>
        public new void Load(TextReader reader)
        {
            // Get the contents of the template from the TextReader
            var template = reader.ReadToEnd();

            // Replace any Mustache variables with the values in the Pattern Parameters
            template = _parameters.Aggregate(template,
                (current, parameter) =>
                    Regex.Replace(current, @"{{\s?" + parameter.Key + @"\s?}}", parameter.Value.ToString()));

            // Scan and parse the template with the Pattern Lab specific instance of Nustache classes
            var scanner = new MustacheScanner();
            var parser = new Parser();

            parser.Parse(this, scanner.Scan(template));
        }
    }
}