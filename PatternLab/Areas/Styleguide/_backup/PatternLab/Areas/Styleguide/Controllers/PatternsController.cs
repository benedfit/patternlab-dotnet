using System.Web.Mvc;
using PatternLab.Providers;

namespace PatternLab.Controllers
{
    public class PatternsController : Controller
    {
        private readonly IPatternProvider _provider;

        public PatternsController()
        {
            _provider = new PatternProvider();
        }
   
        public ActionResult Index()
        {
            return View(_provider.Patterns());
        }
    }
}