using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Nustache.Core;
using PatternLab.Core.Helpers;
using PatternLab.Core.Mustache;
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

        public ActionResult Builder(string id, bool? enableCss, bool? patternsOnly, bool? noCache)
        {
            var builder = new Builder(Provider, ControllerContext);

            // TODO: #20 Snapshots
            var destination = PatternProvider.FolderNameBuilder;

            return Content(builder.Generate(destination, enableCss, patternsOnly, noCache));
        }

        public ActionResult Index()
        {
            var model = new ViewDataDictionary(Provider.Data());

            return View("index", model);
        }

        public ActionResult ViewAll(string id, bool? enableCss, bool? noCache)
        {
            var model = new ViewDataDictionary(Provider.Data())
            {
                {"cssEnabled", enableCss.HasValue && enableCss.Value},
                {"cacheBuster", noCache.HasValue && noCache.Value ? "0" : Provider.CacheBuster()}
            };

            var styleGuideExcludes = Provider.Setting("styleGuideExcludes")
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();

            var patterns =
                Provider.Patterns()
                    .Where(
                        p =>
                            !p.Hidden && !string.IsNullOrEmpty(p.SubType) && !styleGuideExcludes.Contains(p.Type) &&
                            !styleGuideExcludes.Contains(p.Type.StripOrdinals()))
                    .ToList();

            if (!string.IsNullOrEmpty(id))
            {
                model.Add("patternPartial", string.Format("viewall-{0}", id.StripOrdinals()));

                patterns =
                    patterns.Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var partials = new List<object>();

            foreach (var pattern in patterns)
            {
                var html = Render.StringToString(pattern.Html, model, new MustacheTemplateLocator().GetTemplate);
                var lineages = new List<object>();

                // TODO: #8 Implement CSS Rule Saver as per the PHP version
                var css = string.Empty;

                foreach (var partial in pattern.Lineages)
                {
                    var childPattern =
                        Provider.Patterns().FirstOrDefault(
                            p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase));

                    if (childPattern != null)
                    {
                        lineages.Add(new
                        {
                            lineagePath = string.Format("../../patterns/{0}", childPattern.HtmlUrl),
                            lineagePattern = partial
                        });
                    }
                }

                partials.Add(new
                {
                    patternPartial = pattern.Partial,
                    patternLink = pattern.HtmlUrl,
                    patternName = pattern.Name.StripOrdinals().ToDisplayCase(),
                    patternPartialCode = html,
                    patternPartialCodeE = Server.HtmlEncode(html),
                    patternLineageExists = lineages.Count > 0,
                    patternLineages = lineages,
                    patternCSSExists = !string.IsNullOrEmpty(css),
                    patternCSS = css
                });
            }

            model.Add("partials", partials);

            return View("viewall", PatternProvider.FileNameLayout, model);
        }

        public ActionResult ViewSingle(string id, string masterName, bool? parse, bool? enableCss, bool? noCache)
        {
            var model = new ViewDataDictionary(Provider.Data())
            {
                {"cssEnabled", enableCss.HasValue && enableCss.Value},
                {"cacheBuster", noCache.HasValue && noCache.Value ? "0" : Provider.CacheBuster()}
            };

            var pattern = Provider.Patterns()
                .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (pattern == null) return null;

            var childLineages = new List<object>();
            var parentLineages = new List<object>();

            foreach (var childPattern in pattern.Lineages.Select(partial => Provider.Patterns().FirstOrDefault(
                p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                .Where(childPattern => childPattern != null))
            {
                childLineages.Add(new
                {
                    lineagePath = string.Format("../../patterns/{0}", childPattern.HtmlUrl),
                    lineagePattern = childPattern.Partial
                });
            }

            var parentPatterns = Provider.Patterns().Where(p => p.Lineages.Contains(pattern.Partial));
            foreach (var parentPattern in parentPatterns)
            {
                parentLineages.Add(new
                {
                    lineagePath = string.Format("../../patterns/{0}", parentPattern.HtmlUrl),
                    lineagePattern = parentPattern.Partial
                });
            }

            var serializer = new JavaScriptSerializer();

            model.Add("viewSingle", true);
            model.Add("patternPartial", pattern.Partial);
            model.Add("lineage", serializer.Serialize(childLineages));
            model.Add("lineageR", serializer.Serialize(parentLineages));
            model.Add("patternState", PatternProvider.GetState(pattern));
            
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
                return View(pattern.ViewUrl, masterName, model);
            }

            var html = pattern.Html;

            if (parse.HasValue && parse.Value)
            {
                html = Render.StringToString(html, model, new MustacheTemplateLocator().GetTemplate);
            }

            return Content(Server.HtmlEncode(html));
        }
    }
}