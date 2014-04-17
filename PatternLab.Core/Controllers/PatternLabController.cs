using System;
using System.Linq;
using System.Web.Mvc;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    public class PatternLabController : Controller
    {
        public static IPatternProvider Provider { get; set; }

        public PatternLabController()
        {
            Provider = new PatternProvider();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewAll(string id)
        {
            if (string.IsNullOrEmpty(id)) return View("viewall", "_Layout");

            Provider.Data().Add("patternPartial", string.Format("viewall-{0}", id.StripOrdinals()));

            /*var patterns =
                Provider.Patterns()
                    .Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();*/

            return View("viewall", "_Layout");
        }

        public ActionResult ViewSingle(string id)
        {
            var pattern = Provider.Patterns()
                .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (pattern == null) return null;

            Provider.Data().Add("viewSingle", true);
            Provider.Data().Add("patternPartial", pattern.Partial);
            Provider.Data().Add("lineage", "[]");
            Provider.Data().Add("lineageR", "[]");
            Provider.Data().Add("patternState", pattern.State);
            foreach (var data in pattern.Data)
            {
                if (Provider.Data().ContainsKey(data.Key))
                {
                    Provider.Data()[data.Key] = data.Value;
                }
                else
                {
                    Provider.Data().Add(data.Key, data.Value);
                }
            }

            return View(pattern.Url, "_Layout");
        }
    }
}