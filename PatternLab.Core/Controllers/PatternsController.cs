using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    public class PatternsController : Controller
    {
        public static IViewsProvider Provider { get; set; }

        public PatternsController()
        {
            Provider = new ViewsProvider();
        }

        public ActionResult Index()
        {
            return View(Provider.Views());
        }

        public ActionResult ViewAll(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View(Provider.Views());
            }

            return View(Provider.Views().Where(v => v.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList());
        }

        public ActionResult ViewSingle(string id)
        {
            return View(Provider.Views().FirstOrDefault(v => v.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}
