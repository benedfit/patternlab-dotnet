using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nustache.Core;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Mustache
{
    public class MustacheTemplateLocator : FileSystemTemplateLocator
    {
        public MustacheTemplateLocator() : base(string.Empty, string.Empty) { }

        public new MustacheTemplate GetTemplate(string name)
        {
            var parameters = new Dictionary<string, object>();

            var nameFragments = name.Split(new[] { PatternProvider.NameIdentifierParameters }, StringSplitOptions.RemoveEmptyEntries);
            if (nameFragments.Length > 1)
            {
                // TODO: #10 Implement pattern parameters from PHP version            
                var parameterString = name.Replace(nameFragments[0], string.Empty);

                name = nameFragments[0];
            }

            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            var pattern = provider.Patterns()
                .FirstOrDefault(p => p.Partial.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            if (pattern == null) return null;

            var text = File.ReadAllText(pattern.FilePath);
            var reader = new StringReader(text);
            var template = new MustacheTemplate(parameters);
                
            template.Load(reader);

            return template;
        }
    }
}