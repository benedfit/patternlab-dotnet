using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using PatternLab.Areas.Styleguide.Models;

namespace PatternLab.Areas.Styleguide.Providers
{
    public interface IPatternProvider
    {
        List<Pattern> Patterns();
    }

    public class PatternProvider : IPatternProvider
    {
        private List<Pattern> _patterns;

        public List<Pattern> Patterns()
        {
            if (_patterns == null)
            {
                var root = new DirectoryInfo(HttpContext.Current.Server.MapPath(Pattern.PatternsPath));

                var views =
                    root.GetFiles("*.cshtml", SearchOption.AllDirectories)
                        .Where(v => v.Directory != null && v.Directory.FullName != root.FullName);

                _patterns = views.Select(view => new Pattern
                    {
                        View = view.FullName
                    }).ToList();
            }

            return _patterns;
        }
    }
}