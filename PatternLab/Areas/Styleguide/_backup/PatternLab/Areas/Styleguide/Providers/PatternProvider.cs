using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PatternLab.Models;

namespace PatternLab.Providers
{

    public interface IPatternProvider
    {
        List<Pattern> Patterns();
    }

    public class PatternProvider : IPatternProvider
    {
        public List<Pattern> Patterns()
        {
            var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(Pattern.PatternsPath));

            var views =
                root.GetFiles("*.cshtml", SearchOption.AllDirectories)
                    .Where(v => v.Directory != null && v.Directory.FullName != root.FullName);

            var patterns = views.Select(view => new Pattern
                {
                    View = view.FullName
                }).ToList();

            return patterns;
        }
    }
}