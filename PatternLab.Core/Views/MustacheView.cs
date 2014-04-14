using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace PatternLab.Core.Views
{
    public class MustacheView : IView
    {
        private readonly string _physicalPath;

        public MustacheView(string physicalPath)
        {
            _physicalPath = physicalPath;
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            writer.Write(ToString());
        }

        public override string ToString()
        {
            return Parse(File.ReadAllText(_physicalPath));
        }

        public string Parse(string contents)
        {
            contents = Regex.Replace(contents, @"{{>\s*([A-Za-z0-9\-\~\@]+)\s*}}", GetPattern);
            return contents;
        }

        public virtual string GetPattern(Match match)
        {
            if (!match.Success) return string.Empty;
            var key = match.Result("$1");
            var pattern =
                Controllers.PatternsController.Provider.Patterns()
                    .FirstOrDefault(p => p.Partial.Equals(key, StringComparison.InvariantCultureIgnoreCase));
            return pattern != null ? new MustacheView(pattern.FilePath).ToString() : string.Empty;
        }
    }
}