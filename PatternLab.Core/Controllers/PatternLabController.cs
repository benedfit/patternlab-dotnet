using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PatternLab.Core.Engines.Mustache;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    /// <summary>
    /// The Pattern Lab MVC controller
    /// </summary>
    public class PatternLabController : Controller
    {
        /// <summary>
        /// The pattern provider
        /// </summary>
        public static PatternProvider Provider { get; set; }

        /// <summary>
        /// Initialises a new Pattern Lab MVC controller
        /// </summary>
        public PatternLabController()
        {
            if (Provider == null)
            {
                // Set the pattern provider if not set
                Provider = new PatternProvider();
            }
        }

        /// <summary>
        /// Builder has been deprecated. Please use Generate instead
        /// </summary>
        /// <returns>~/builder has been deprecated, please visit ~/generate instead</returns>
        [Obsolete("Builder has been deprecated. Please use Generate instead")]
        public ActionResult Builder()
        {
            return Content("~/builder has been deprecated, please visit ~/generate instead");
        }

        /// <summary>
        /// Renders the results of the static output generator
        /// </summary>
        /// <param name="id">The destination directory. Currently unsupported, and forced to /public</param>
        /// <param name="enableCss">Generate CSS for each pattern. Currently unsupported</param>
        /// <param name="patternsOnly">Generate only the patterns. Does NOT clean the destination folder</param>
        /// <param name="noCache">Set the cacheBuster value to 0</param>
        /// <returns>The results of the generator</returns>
        public ActionResult Generate(string id, bool? enableCss, bool? patternsOnly, bool? noCache)
        {
            var builder = new Builder(Provider, ControllerContext);

            // Destination currently forced to /public
            var destination = Provider.DirectoryPathPublic;

            // Return the results of the generator
            return Content(builder.Generate(destination, enableCss, patternsOnly, noCache));
        }

        /// <summary>
        /// Renders the 'Viewer' page
        /// </summary>
        /// <returns>The 'Viewer' page</returns>
        public ActionResult Index()
        {
            // Get data from provider
            var model = new Dictionary<string, object>(Provider.Data());

            // Render 'Viewer' page
            return View(PatternProvider.ViewNameViewerPage, model);
        }

        /// <summary>
        /// Renders 'View all' pages
        /// </summary>
        /// <param name="id">The dash delimited pattern type value to filter with (e.g. organisms-global)</param>
        /// <param name="enableCss">Generate CSS for each pattern. Currently unsupported</param>
        /// <param name="noCache">Set the cacheBuster value to 0</param>
        /// <returns>A 'View all' page</returns>
        public ActionResult ViewAll(string id, bool? enableCss, bool? noCache)
        {
            // Get data from provider and set additional variables
            var model = new Dictionary<string, object>(Provider.Data())
            {
                {"cssEnabled", (enableCss.HasValue && enableCss.Value).ToString().ToLower()},
                {"cacheBuster", noCache.HasValue && noCache.Value ? "0" : Provider.CacheBuster()}
            };

            // Get the list of patterns to exclude from the page
            var styleGuideExcludes = Provider.Setting("styleGuideExcludes")
                .Split(new[] {PatternProvider.IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Filter out any hidden or exluded patterns
            var patterns =
                Provider.Patterns()
                    .Where(
                        p =>
                            !p.Hidden && !string.IsNullOrEmpty(p.SubType) && !styleGuideExcludes.Contains(p.Type) &&
                            !styleGuideExcludes.Contains(p.Type.StripOrdinals()))
                    .ToList();

            if (!string.IsNullOrEmpty(id))
            {
                // If a type filter is specified, add it to the data collection             
                model.Add("patternPartial", string.Format("viewall-{0}", id.StripOrdinals()));

                // Find only the patterns that match the type filter
                patterns =
                    patterns.Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            var partials = new List<object>();

            foreach (var pattern in patterns)
            {
                // Load the pattern's template
                var html = Provider.PatternEngine().Parse(pattern, model);
                var lineages = new List<object>();

                // TODO: #8 Implement CSS Rule Saver as per the PHP version. Currently unsupported
                var css = string.Empty;

                // Gather a list of child patterns that the current pattern's template references
                foreach (var partial in pattern.Lineages)
                {
                    var childPattern =
                        Provider.Patterns().FirstOrDefault(
                            p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase));

                    if (childPattern != null)
                    {
                        lineages.Add(new
                        {
                            lineagePath =
                                string.Format("../../{0}/{1}",
                                    PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                                    childPattern.HtmlUrl),
                            lineagePattern = partial,
                            lineageState = PatternProvider.GetState(childPattern)
                        });
                    }
                }

                // Generate a JSON object to holder the patterns data
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

            // Add all the pattern data to the main data collection
            model.Add("partials", partials);

            // Render 'View all' page
            return View(PatternProvider.ViewNameViewAllPage, PatternProvider.ViewNameMaster, model);
        }

        /// <summary>
        /// Renders a single pattern page
        /// </summary>
        /// <param name="id">The dash delimited path of the pattern (e.g. atoms-colors)</param>
        /// <param name="masterName">The optional master view to use when rendering</param>
        /// <param name="parse">Whether or not to parse the template and replace Mustache tags with data</param>
        /// <param name="enableCss">Generate CSS for each pattern. Currently unsupported</param>
        /// <param name="noCache">Set the cacheBuster value to 0</param>
        /// <param name="extension">The optional extension of the template</param>
        /// <returns>A pattern page</returns>
        public ActionResult ViewSingle(string id, string masterName, bool? parse, bool? enableCss, bool? noCache, string extension)
        {
            // Get data from provider and set additional variables
            var model = new Dictionary<string, object>(Provider.Data())
            {
                {"cssEnabled", (enableCss.HasValue && enableCss.Value).ToString().ToLower()},
                {"cacheBuster", noCache.HasValue && noCache.Value ? "0" : Provider.CacheBuster()}
            };

            // Find pattern from dash delimited path
            var pattern = Provider.Patterns()
                .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            if (pattern == null) return HttpNotFound();

            // For all values in the pattern data collection update the main data collection
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

            var childLineages = new List<object>();
            var parentLineages = new List<object>();

            // Gather a list of child patterns that the current pattern's template references
            foreach (var childPattern in pattern.Lineages.Select(partial => Provider.Patterns().FirstOrDefault(
                p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                .Where(childPattern => childPattern != null))
            {
                childLineages.Add(new
                {
                    lineagePath =
                        string.Format("../../{0}/{1}",
                            PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                            childPattern.HtmlUrl),
                    lineagePattern = childPattern.Partial,
                    lineageState = PatternProvider.GetState(childPattern),
                    lineageCode = Provider.PatternEngine().Parse(childPattern, model)
                });
            }

            // Gather a list of parent patterns whose templates references the current pattern
            var parentPatterns = Provider.Patterns().Where(p => p.Lineages.Contains(pattern.Partial));
            foreach (var parentPattern in parentPatterns)
            {
                parentLineages.Add(new
                {
                    lineagePath =
                        string.Format("../../{0}/{1}",
                            PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                            parentPattern.HtmlUrl),
                    lineagePattern = parentPattern.Partial,
                    lineageState = PatternProvider.GetState(parentPattern)
                });
            }

            var serializer = new JavaScriptSerializer();

            // Add pattern specific data to the data collection
            model.Add("viewSingle", true);
            model.Add("patternPartial", pattern.Partial);
            model.Add("lineage", serializer.Serialize(childLineages));
            model.Add("lineageR", serializer.Serialize(parentLineages));
            model.Add("patternState", PatternProvider.GetState(pattern));

            var html = pattern.Html;

            if (!string.IsNullOrEmpty(masterName))
            {
                // If a master has been specified, render 'pattern.html' using master view
                html = Provider.PatternEngine().Parse(pattern, model);

                // Add parsed template to model
                model.Add("body", html);

                return View(masterName, model);
            }

            if (parse.HasValue && parse.Value)
            {
                // Parse template for 'pattern.escaped.html'
                html = Provider.PatternEngine().Parse(pattern, model);
            }
            else
            {
                // Check extension matches pattern engine for un-parsed templates
                extension = RouteData.Values["extension"] != null ? RouteData.Values["extension"].ToString() : extension;
                if (!extension.StartsWith("."))
                {
                    extension = string.Concat(".", extension);
                }

                if (!Provider.PatternEngine().Extension().Equals(extension, StringComparison.InvariantCultureIgnoreCase)) return HttpNotFound();
            }

            // Render pattern template
            return Content(Server.HtmlEncode(html));
        }
    }
}