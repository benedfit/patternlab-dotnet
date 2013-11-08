using System.Web.Mvc;
using PatternLab.Areas.Styleguide.Providers;

namespace PatternLab.Areas.Styleguide.Controllers
{
    public class HtmlController : Controller
    {
        private IPatternProvider _provider;

        public HtmlController()
        {
            _provider = new PatternProvider();
        }

        public ActionResult All()
        {
            return View(_provider.Patterns());
        }

        public ActionResult Index()
        {
            return View(_provider.Patterns());
        }
    }
}