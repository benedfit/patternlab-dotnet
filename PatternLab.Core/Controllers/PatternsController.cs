using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    public class PatternsController : Controller
    {
        public static IPatternProvider Provider { get; set; }

        public PatternsController()
        {
            Provider = new PatternProvider();
        }

        public ActionResult Index()
        {
            return
                View(
                    Provider.Patterns()
                        .Where(
                            p =>
                                !p.Name.StartsWith(
                                    PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture))));
        }

        public ActionResult ViewAll(string id)
        {
            ViewData = Provider.Data();

            if (string.IsNullOrEmpty(id))
            {
                return View(Provider.Patterns().Where(
                    p =>
                        !p.Name.StartsWith(
                            PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture))));
            }

            return
                View(
                    Provider.Patterns()
                        .Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                        .ToList());
        }

        public ActionResult ViewSingle(string id)
        {
            ViewData = Provider.Data();

            var pattern = Provider.Patterns()
                .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (pattern != null)
            {
                foreach (var item in pattern.Data)
                {
                    if (ViewData.ContainsKey(item.Key))
                    {
                        ViewData[item.Key] = item.Value;
                    }
                    else
                    {
                        ViewData.Add(item.Key, item.Value);
                    }
                }
            }

            return View(pattern);
        }
    }
}