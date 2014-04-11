using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PatternLab.Core.Models;

namespace PatternLab.Core.Providers
{
    public interface IPatternProvider
    {
        List<Pattern> Patterns();
    }

    public class PatternProvider : IPatternProvider
    {
        public static string PatternExtension = ".mustache";
        public static string PatternsFolder = "~/Views/Patterns";

        private List<Pattern> _patterns;

        public List<Pattern> Patterns()
        {
            if (_patterns != null) return _patterns;

            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(PatternsFolder));

            var patterns =
                root.GetFiles(string.Concat("*", PatternExtension), SearchOption.AllDirectories)
                    .Where(p => p.Directory != null && p.Directory.FullName != root.FullName && !p.Name.StartsWith("_"));

            _patterns = patterns.Select(p => new Pattern(p.FullName)).ToList();

            return _patterns;
        }
    }
}