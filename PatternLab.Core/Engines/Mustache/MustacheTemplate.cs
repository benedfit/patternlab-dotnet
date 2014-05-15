using System;
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
        /// Initialises a new Template with a collection of pattern parameters
        /// </summary>
        /// <param name="parameters">The pattern parameters</param>
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

            // Replace any listItem variables with the values in the pattern parameters
            template = _parameters.Where(p => p.Key.StartsWith("listItems.", StringComparison.InvariantCultureIgnoreCase)).Aggregate(template,
                (current, parameter) =>
                    current.Replace(parameter.Key, parameter.Value.ToString()));

            // Replace any Mustache variables with the values in the pattern parameters
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