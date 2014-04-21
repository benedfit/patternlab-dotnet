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
            var model = new ViewDataDictionary(Provider.Data());

            return View(model);
        }

        public ActionResult ViewAll(string id)
        {
            var model = new ViewDataDictionary(Provider.Data());

            var patterns = Provider.Patterns().Where(p => !p.Hidden && !string.IsNullOrEmpty(p.SubType)).ToList();

            if (!string.IsNullOrEmpty(id))
            {

                model.Add("patternPartial", string.Format("viewall-{0}", id.StripOrdinals()));

                patterns =
                    patterns.Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var partials = new List<object>();

            foreach (var pattern in patterns)
            {
                var html = Render.FileToString(pattern.FilePath, ViewData);

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

            model.Add("partials", partials);

            return View("viewall", "_Layout", model);
        }

        public ActionResult ViewSingle(string id, string masterName, bool? parse)
        {
            var model = new ViewDataDictionary(Provider.Data());

            var pattern = Provider.Patterns()
                .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (pattern == null) return null;

            model.Add("viewSingle", true);
            model.Add("patternPartial", pattern.Partial);
            model.Add("lineage", "[]");
            model.Add("lineageR", "[]");
            model.Add("patternState", pattern.State);
            foreach (var data in pattern.Data)
            {
                if (model.ContainsKey(data.Key))
                {
                    model[data.Key] = data.Value;
                }
                else
                {
                    model.Add(data.Key, data.Value);
                }
            }

            if (!string.IsNullOrEmpty(masterName))
            {
                return View(pattern.Url, masterName, model);
            }

            var html = pattern.Html;

            if (parse.HasValue && parse.Value)
            {
                html = Render.StringToString(html, model);
            }

            return Content(Server.HtmlEncode(html));
        }
    }
}