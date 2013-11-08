using System.Linq;
using System.Web.Mvc;
using PatternLab.Areas.Styleguide.Providers;

namespace PatternLab.Areas.Styleguide.Controllers
{
    public class HtmlController : Controller
    {
        private readonly IPatternProvider _provider;

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

        public ActionResult Pattern(string id)
        {
            return View(_provider.Patterns().FirstOrDefault(p => p.Id == id));
        }

        public ActionResult Patterns(string level, string collection)
        {      
            return View("All", _provider.Patterns().Where(p => p.Level == level && p.Collection == collection));
        }
    }
}