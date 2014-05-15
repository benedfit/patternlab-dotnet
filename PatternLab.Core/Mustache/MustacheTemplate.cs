using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nustache.Core;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Mustache
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
            foreach (var parameter in _parameters)
            {
                var value = parameter.Value.ToString();
                var key = parameter.Key;

                if (value.Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase))
                {
                    // If 'true' strip out {{# }} sections and set {{^ }} sections to empty
                    template = Regex.Replace(template, @"{{#\s?" + key + @"\s?}}(.*?)?{{/\s?" + key + @"\s?}}", @"$1", RegexOptions.Singleline);
                    template = Regex.Replace(template, @"{{\^\s?" + key + @"\s?}}(.*?)?{{/\s?" + key + @"\s?}}", string.Empty, RegexOptions.Singleline);
                }
                else if (value.Equals(bool.FalseString, StringComparison.InvariantCultureIgnoreCase))
                {
                    // If 'true' strip out {{^ }} sections and set {{# }} sections to empty
                    template = Regex.Replace(template, @"{{\^\s?" + key + @"\s?}}(.*?)?{{/\s?" + key + @"\s?}}", @"$1", RegexOptions.Singleline);
                    template = Regex.Replace(template, @"{{#\s?" + key + @"\s?}}(.*?)?{{/\s?" + key + @"\s?}}", string.Empty, RegexOptions.Singleline);
                }
                else
                {
                    // Replace variables with value
                    template = Regex.Replace(template, @"{{\s?" + key + @"\s?}}", value.Replace(PatternProvider.IdentifierParameterString.ToString(), string.Empty));
                }
            }

            // Scan and parse the template with the Pattern Lab specific instance of Nustache classes
            var scanner = new MustacheScanner();
            var parser = new Parser();

            parser.Parse(this, scanner.Scan(template));
        }
    }
}