using System;
using System.Linq;
using System.Web.Mvc;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    public class PatternsController : Controller
    {
        public PatternsController()
        {
            Provider = new PatternProvider();
        }

        public static IPatternProvider Provider { get; set; }

        public ActionResult Index()
        {
            return View(Provider.Patterns());
        }

        public ActionResult ViewAll(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(Provider.Patterns());
            }

            return
                View(
                    Provider.Patterns()
                        .Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                        .ToList());
        }

        public ActionResult ViewSingle(string id)
        {
            return
                View(
                    Provider.Patterns()
                        .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}