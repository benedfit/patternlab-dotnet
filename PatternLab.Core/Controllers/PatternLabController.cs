using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Controllers
{
    /// <summary>
    /// The Pattern Lab MVC controller
    /// </summary>
    public class PatternLabController : Controller
    {
        private const string PathFormat = "../../{0}/{1}";

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

            // Source currently forced to /
            var source = Provider.FolderPathSource;

            // Destination currently forced to /public
            var destination = Provider.FolderPathPublic;

            // Return the results of the generator
            return Content(builder.Generate(source, destination, enableCss, patternsOnly, noCache));
        }

        /// <summary>
        /// Renders the 'Viewer' page
        /// </summary>
        /// <returns>The 'Viewer' page</returns>
        public ActionResult Index(bool? enableCss, bool? noCache)
        {
            // Get data from provider and set additional variables
            var data = Provider.Data();
            data.cssEnabled = (enableCss.HasValue && enableCss.Value).ToString().ToLowerInvariant();
            data.cacheBuster = Provider.CacheBuster(noCache);

            // Render 'Viewer' page
            return View(PatternProvider.ViewNameViewerPage, data);
        }

        /// <summary>
        /// Renders the 'Snapshots' page
        /// </summary>
        /// <returns>The 'Snapshots' page</returns>
        public ActionResult Snapshots()
        {
            var data = Provider.Data();

            return View(PatternProvider.ViewNameSnapshots, PatternProvider.ViewNameViewSingle, data);
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
            var data = Provider.Data();
            data.cssEnabled = (enableCss.HasValue && enableCss.Value).ToString().ToLowerInvariant();
            data.cacheBuster = Provider.CacheBuster(noCache);
            data.patternPartial = string.Empty;

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
                

                // Find only the patterns that match the dash delimited type path filter
                var filteredPatterns =
                    patterns.Where(p => p.TypeDash.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (!filteredPatterns.Any())
                {
                    // If not patterns match the dash delimited type path, return those whose type matches the filter
                    filteredPatterns =
                        patterns.Where(p => p.Type.Equals(id, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    // If a type filter is specified, add it to the data collection
                    data.patternPartial = string.Format("{0}-{1}-{2}",
                        PatternProvider.ViewNameViewAllPage.ToLowerInvariant(),
                        id.StripOrdinals(), PatternProvider.KeywordPartialAll);
                }
                else
                {
                    // If a type path filter is specified, add it to the data collection
                    data.patternPartial = string.Format("{0}-{1}",
                        PatternProvider.ViewNameViewAllPage.ToLowerInvariant(),
                        id.StripOrdinals());
                }

                patterns = filteredPatterns;
            }

            var partials = new List<dynamic>();

            foreach (var pattern in patterns)
            {
                // Load the pattern's template
                var html = Provider.PatternEngine().Parse(pattern, data);
                var childLineages = new List<dynamic>();
                var parentLineages = new List<dynamic>();

                // TODO: #8 Implement CSS Rule Saver as per the PHP version. Currently unsupported
                var css = string.Empty;

                // Gather a list of child patterns that the current pattern's template references
                foreach (var childPattern in pattern.Lineages.Select(partial => Provider.Patterns().FirstOrDefault(
                    p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                    .Where(childPattern => childPattern != null))
                {
                    childLineages.Add(new
                    {
                        lineagePattern = childPattern.Partial,
                        lineagePath =
                            string.Format(PathFormat,
                                PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                                childPattern.HtmlUrl),
                        lineageState = PatternProvider.GetState(childPattern)
                    });
                }

                // Gather a list of parent patterns whose templates references the current pattern
                var patternPartial = pattern.Partial;
                var parentPatterns = Provider.Patterns().Where(p => p.Lineages.Contains(patternPartial));
                foreach (var parentPattern in parentPatterns)
                {
                    parentLineages.Add(new
                    {
                        lineagePattern = parentPattern.Partial,
                        lineagePath =
                            string.Format(PathFormat,
                                PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                                parentPattern.HtmlUrl),
                        lineageState = PatternProvider.GetState(parentPattern)
                    });
                }

                // Generate a JSON object to holder the patterns data
                partials.Add(new
                {
                    patternPartial = pattern.Partial,
                    patternLink = pattern.HtmlUrl,
                    patternName = pattern.Name.StripOrdinals().ToDisplayCase(),
                    patternDescExists = !string.IsNullOrEmpty(pattern.Description),
                    patternDesc = pattern.Description,
                    patternModifiersExist = pattern.Modifiers.Any(),
                    patternModifiers = pattern.Modifiers,
                    patternPartialCode = html,
                    patternPartialCodeE = Server.HtmlEncode(html),
                    patternEngineName = Provider.PatternEngine().Name(),
                    patternLineageExists = childLineages.Any(),
                    patternLineages = childLineages,
                    patternLineageRExists = parentLineages.Any(),
                    patternLineagesR = parentLineages,
                    patternLineageEExists = childLineages.Any() || parentLineages.Any(),
                    patternCSSExists = !string.IsNullOrEmpty(css),
                    patternCSS = css
                });
            }

            // Add all the pattern data to the main data collection
            data.partials = partials;

            // Render 'View all' page
            return View(PatternProvider.ViewNameViewAllPage, PatternProvider.ViewNameViewSingle, data);
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
            // Find pattern from dash delimited path
            var pattern = Provider.Patterns()
                .FirstOrDefault(p => p.PathDash.Equals(id, StringComparison.InvariantCultureIgnoreCase));

            // If pattern is not found return a 404
            if (pattern == null) return HttpNotFound();

            // Get data from provider and merge with pattern data
            var data = PatternProvider.MergeData(Provider.Data(), pattern.Data);
            data.cssEnabled = (enableCss.HasValue && enableCss.Value).ToString().ToLowerInvariant();
            data.cacheBuster = Provider.CacheBuster(noCache);

            var childLineages = new List<dynamic>();
            var parentLineages = new List<dynamic>();

            // Gather a list of child patterns that the current pattern's template references
            foreach (var childPattern in pattern.Lineages.Select(partial => Provider.Patterns().FirstOrDefault(
                p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                .Where(childPattern => childPattern != null))
            {
                childLineages.Add(new
                {
                    lineagePattern = childPattern.Partial,
                    lineagePath =
                        string.Format(PathFormat,
                            PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                            childPattern.HtmlUrl),
                    lineageState = PatternProvider.GetState(childPattern)
                });
            }

            // Gather a list of parent patterns whose templates references the current pattern
            var parentPatterns = Provider.Patterns().Where(p => p.Lineages.Contains(pattern.Partial));
            foreach (var parentPattern in parentPatterns)
            {
                parentLineages.Add(new
                {
                    lineagePattern = parentPattern.Partial,
                    lineagePath =
                        string.Format(PathFormat,
                            PatternProvider.FolderNamePattern.TrimStart(PatternProvider.IdentifierHidden),
                            parentPattern.HtmlUrl),
                    lineageState = PatternProvider.GetState(parentPattern)
                });
            }

            var serializer = new JavaScriptSerializer();

            var patternData = new
            {
                patternBreadcrumb = pattern.Breadcrumb,
                patternDesc = pattern.Description,
                patternEngine = string.Empty,
                patternModifiers = serializer.Serialize(pattern.Modifiers),
                patternName = pattern.Name.StripOrdinals(),
                patternPartial = pattern.Partial,
                lineage = serializer.Serialize(childLineages),
                lineageR = serializer.Serialize(parentLineages),
                patternState = PatternProvider.GetState(pattern)
            };

            // Add pattern specific data to the data collection
            data.patternFooterData = patternData;

            var html = pattern.Html;

            if (!string.IsNullOrEmpty(masterName))
            {
                // If a master has been specified, render 'pattern.html' using master view
                html = Provider.PatternEngine().Parse(pattern, data);

                // Add parsed template to model
                data.viewSingle = html;

                return View(masterName, data);
            }

            if (parse.HasValue && parse.Value)
            {
                // Parse template for 'pattern.escaped.html'
                html = Provider.PatternEngine().Parse(pattern, data);
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