using System.Collections.Generic;
using System.Globalization;
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
        public static char HiddenPatternIdentifier = '_';
        public static string PatternDataExtension = ".json";
        public static string PatternsFolder = "~/Views/Patterns";
        public static char PatternStateIdenfifier = '@';
        public static char PsuedoPatternIdentifier = '~';
        public static string PatternViewExtension = ".mustache";

        private List<Pattern> _patterns;

        public List<Pattern> Patterns()
        {
            if (_patterns != null) return _patterns;

            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(PatternsFolder));

            var views =
                root.GetFiles(string.Concat("*", PatternViewExtension), SearchOption.AllDirectories)
                    .Where(
                        v =>
                            v.Directory != null && v.Directory.FullName != root.FullName &&
                            !v.Name.StartsWith(HiddenPatternIdentifier.ToString(CultureInfo.InvariantCulture)));

            _patterns = views.Select(v => new Pattern(v.FullName)).ToList();

            var pseudoViews = root.GetFiles(string.Concat("*", PatternDataExtension), SearchOption.AllDirectories)
                .Where(
                    v =>
                        v.Directory != null && v.Directory.FullName != root.FullName &&
                        v.Name.Contains(PsuedoPatternIdentifier) && !v.Name.StartsWith(HiddenPatternIdentifier.ToString(CultureInfo.InvariantCulture)));

            _patterns.AddRange(pseudoViews.Select(v => new Pattern(v.FullName)));
            _patterns = _patterns.OrderBy(p => p.PathDash).ToList();

            return _patterns;
        }
    }
}