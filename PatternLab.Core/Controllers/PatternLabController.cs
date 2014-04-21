using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.HtmlControls;
using Nustache.Core;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    public class PatternLabController : Controller
    {
        public static IPatternProvider Provider { get; set; }

        public PatternLabController()
        {
            if (Provider == null)
            {
                Provider = new PatternProvider();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewAll(string id)
        {
            var patterns = Provider.Patterns().Where(p => !p.Hidden && !string.IsNullOrEmpty(p.SubType)).ToList();

            if (!string.IsNullOrEmpty(id))
            {

                Provider.Data().Add("patternPartial", string.Format("viewall-{0}", id.StripOrdinals()));

                patterns =
                    patterns.Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var partials = new List<object>();

            foreach (var pattern in patterns)
            {
                var html = Render.FileToString(pattern.FilePath, Provider.Data());

                partials.Add(new
                {
                    patternPartial = pattern.Partial,
                    patternLink = pattern.Path,
                    patternName = pattern.Name.StripOrdinals().ToDisplayCase(),
                    patternPartialCode = html,
                    patternPartialCodeE = Server.HtmlEncode(html),
                    patternLineageExists = pattern.Lineage.Count > 0,
                    patternLineages = pattern.Lineage,
                    patternCSSExists = !string.IsNullOrEmpty(pattern.Css),
                    patternCSS = pattern.Css
                });
            }

            Provider.Data().Add("partials", partials);

            return View("viewall", "_Layout");
        }

        public ActionResult ViewSingle(string id, string masterName, bool? parse)
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

            if (!string.IsNullOrEmpty(masterName))
            {
                return View(pattern.Url, masterName);
            }

            var html = pattern.Html;

            if (parse.HasValue && parse.Value)
            {
                html = Render.StringToString(html, Provider.Data());
            }

            return Content(Server.HtmlEncode(html));
        }
    }
}