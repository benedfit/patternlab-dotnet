using System;
using System.IO;
using System.Linq;
using Nustache.Core;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Engines.Mustache
{
    /// <summary>
    /// Pattern Lab override to Nustache.Core.FileSystemTemplateLocator
    /// </summary>
    public class MustacheTemplateLocator : FileSystemTemplateLocator
    {
        /// <summary>
        /// Initialises a new TemplateLocator
        /// </summary>
        public MustacheTemplateLocator() : base(string.Empty, string.Empty) { }

        /// <summary>
        /// Gets a Mustache template from it's name
        /// </summary>
        /// <param name="name">The name of the template</param>
        /// <returns>The Mustache template</returns>
        public new MustacheTemplate GetTemplate(string name)
        {
            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            
            // Strip any pattern parameters from the name then find a pattern who's Partial (e.g. atoms-colors) matches the value
            var pattern = provider.Patterns()
                .FirstOrDefault(
                    p => p.Partial.Equals(name.StripPatternParameters(), StringComparison.InvariantCultureIgnoreCase));

            if (pattern == null) return null;

            // If found, read the contents of the template from disk 
            var text = File.ReadAllText(pattern.FilePath);
            var reader = new StringReader(text);

            // Pass the pattern parameters to the template
            var template = new MustacheTemplate(name.ToPatternParameters());
            
            // Load the contents of the file into the template
            template.Load(reader);

            return template;
        }
    }
}