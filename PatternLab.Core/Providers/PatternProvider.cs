using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IniParser;
using IniParser.Model;
using PatternLab.Core.Engines;
using PatternLab.Core.Helpers;

namespace PatternLab.Core.Providers
{
    /// <summary>
    /// The Pattern Lab file and data provider
    /// </summary>
    public class PatternProvider
    {
        /// <summary>
        /// The public directory
        /// </summary>
        public string DirectoryPathPublic
        {
            get
            {
                var directory = Setting("publicDir");
                if (!string.IsNullOrEmpty(directory))
                {
                    directory = "public";
                }

                return string.Format("{0}{1}{2}", HttpRuntime.AppDomainAppPath, directory,
                    Path.DirectorySeparatorChar);
            }
        }

        /// <summary>
        /// The source directory
        /// </summary>
        public string DirectoryPathSource
        {
            get
            {
                var directory = Setting("sourceDir");
                if (!string.IsNullOrEmpty(directory))
                {
                    return string.Format("{0}{1}{2}", HttpRuntime.AppDomainAppPath, directory,
                        Path.DirectorySeparatorChar);
                }

                return HttpRuntime.AppDomainAppPath;
            }
        }

        /// <summary>
        /// The file extension of data files
        /// </summary>
        public static string FileExtensionData = ".json";

        /// <summary>
        /// The file extension of escaped HTML files
        /// </summary>
        public static string FileExtensionEscapedHtml = ".escaped.html";

        /// <summary>
        /// The file extension of HTML files
        /// </summary>
        public static string FileExtensionHtml = ".html";

        /// <summary>
        /// The file name of the master view
        /// </summary>
        public static string FileNameMaster = "_Layout";

        /// <summary>
        /// The file name of the 'Viewer' page
        /// </summary>
        public static string FileNameViewer = "index.html";

        /// <summary>
        /// The path to the config file
        /// </summary>
        public static string FilePathConfig = "config/config.ini";

        /// <summary>
        /// The name of the folder containing data files
        /// </summary>
        public static string FolderNameData = "_data";

        /// <summary>
        /// The name of the folder containing pattern files
        /// </summary>
        public static string FolderNamePattern = "_patterns";

        /// <summary>
        /// Denotes a delimited list
        /// </summary>
        public static char IdentifierDelimiter = ',';

        /// <summary>
        /// Denotes a hidden object
        /// </summary>
        public static char IdentifierHidden = '_';

        /// <summary>
        /// Denotes object contains styleModifier
        /// </summary>
        public static char IdentifierModifier = ':';

        /// <summary>
        /// Denotes object contains more than one styleModifier value
        /// </summary>
        public static char IdentifierModifierSeparator = '|';

        /// <summary>
        /// Denotes object contains pattern parameters
        /// </summary>
        public static char IdentifierParameters = '(';

        /// <summary>
        /// Denotes a pattern parameter that is a string
        /// </summary>
        public static char IdentifierParameterString = '"';

        /// <summary>
        /// Denotes a psuedo pattern
        /// </summary>
        public static char IdentifierPsuedo = '~';

        /// <summary>
        /// Denotes a space character in display name parsing
        /// </summary>
        public static char IdentifierSpace = '-';

        /// <summary>
        /// Denotes a pattern has a state
        /// </summary>
        public static char IdentifierState = '@';

        /// <summary>
        /// Define the list of currently supported listItem variables
        /// </summary>
        public static List<string> ListItemVariables = new List<string>
        {
            "one",
            "two",
            "three",
            "four",
            "five",
            "six",
            "seven",
            "eight",
            "nine",
            "ten",
            "eleven",
            "twelve"
        };

        /// <summary>
        /// The pattern engines supported by Pattern Lab
        /// </summary>
        public static List<IPatternEngine> SupportedPatternEngines = new List<IPatternEngine>
        {
            // Mustache (.mustache)
            new MustachePatternEngine(),

            // Razor (.cshtml)
            new RazorPatternEngine()
        };

        /// <summary>
        /// The name of the 'View all' page view
        /// </summary>
        public static string ViewNameViewAllPage = "viewall";

        /// <summary>
        /// The name of the 'Viewer' page view
        /// </summary>
        public static string ViewNameViewerPage = "index";

        private string _cacheBuster;
        private IniData _config;
        private IDictionary<string, object> _data;
        private List<string> _ignoredDirectories;
        private List<string> _ignoredExtensions;
        private IPatternEngine _patternEngine;
        private List<Pattern> _patterns;

        /// <summary>
        /// Determines whether cache busting is enable or disabled
        /// </summary>
        /// <returns>The cache buster value to be appended to asset URLs</returns>
        public string CacheBuster()
        {
            // Return cached value if set
            if (!string.IsNullOrEmpty(_cacheBuster)) return _cacheBuster;

            bool enabled;
            // Check the config file to see if it's enabled
            if (!Boolean.TryParse(Setting("cacheBusterOn"), out enabled))
            {
                enabled = false;
            }

            // Return the current time as ticks if enabled, or 0 if disabled
            _cacheBuster = enabled ? DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) : "0";
            return _cacheBuster;
        }

        /// <summary>
        /// Clears the pattern provider's cached objects
        /// </summary>
        public void Clear()
        {
            _cacheBuster = null;
            _config = null;
            _data = null;
            _ignoredDirectories = null;
            _ignoredExtensions = null;
            _patterns = null;
            _patternEngine = null;
        }

        /// <summary>
        /// Reads the configuration settings from disk
        /// </summary>
        /// <returns>The configuration settings for Pattern Lab</returns>
        public IniData Config()
        {
            // Return cached value if set
            if (_config != null) return _config;

            // Configure the INI parser to handler the comments in the Pattern Lab config file
            var parser = new FileIniDataParser();
            parser.Parser.Configuration.AllowKeysWithoutSection = true;
            parser.Parser.Configuration.SkipInvalidLines = true;

            var path = Path.Combine(HttpRuntime.AppDomainAppPath, FilePathConfig);
            if (!File.Exists(path))
            {
                // If  the config doesn't exist create a new version
                var virtualPath = string.Format("~/{0}", FilePathConfig);
                var defaultConfig = new EmbeddedResource(string.Format("{0}.default", virtualPath));
                var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                Builder.CreateFile(virtualPath, defaultConfig.ReadAllText().Replace("$version$", version), null,
                    new DirectoryInfo(HttpRuntime.AppDomainAppPath));
            }

            // Read the contents of the config file into a read-only stream
            using (
                var stream = new FileStream(path, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
            {
                _config = parser.ReadData(new StreamReader(stream));
            }

            return _config;
        }

        /// <summary>
        /// Generates a data collection for the files in the data folder
        /// </summary>
        /// <returns>The data collection for Pattern Lab</returns>
        public IDictionary<string, object> Data()
        {
            // Return cached value if set
            if (_data != null) return _data;

            var host = Dns.GetHostEntry(Dns.GetHostName());

            // Get local IP address
            var ipAddresses = host.AddressList;
            var ipAddress = ipAddresses[ipAddresses.Length - 1].ToString();

            // Identify hidden ish controls from config
            var ishSettings = Setting("ishControlsHide")
                .Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries);
            var hiddenIshControls = ishSettings.ToDictionary(s => s.Trim(), s => true);

            // Hide the 'Page follow' ish control if disabled in config
            if (Setting("pageFollowNav").Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                hiddenIshControls.Add("tools-follow", true);
            }
            else
            {
                // TODO: #24 Implement page follow from PHP version. Currently always hidden. Delete this else statement once implemented
                hiddenIshControls.Add("tools-follow", true);
            }

            // Hide the 'Auto-reload' ish control if disabled in config
            if (Setting("autoReloadNav").Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                hiddenIshControls.Add("tools-reload", true);
            }
            else
            {
                // TODO: #23 Implement page auto-reload from PHP version. Currently always hidden. Delete this else statement once implemented
                hiddenIshControls.Add("tools-reload", true);
            }

            var patternLinks = new Dictionary<string, string>();
            var patternPaths = new Dictionary<string, object>();
            var viewAllPaths = new Dictionary<string, object>();
            var patternTypes = new List<object>();

            // Use all patterns that aren't hidden
            var patterns =
                Patterns()
                    .Where(p => !p.Hidden)
                    .ToList();

            if (patterns.Any())
            {
                // Get a list of distinct types
                var types = patterns.Select(p => p.Type).Distinct().ToList();
                foreach (var type in types)
                {
                    var typeName = type.StripOrdinals();
                    var typeDisplayName = typeName.ToDisplayCase();

                    // Create JSON object to hold information about the current type
                    var typeDetails =
                        new
                        {
                            patternTypeLC = typeName,
                            patternTypeUC = typeDisplayName,
                            patternTypeItems = new List<object>(),
                            patternItems = new List<object>()
                        };

                    // Get patterns that match the current type (e.g. Atoms)
                    var typedPatterns =
                        patterns.Where(p => p.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    // Get the sub-types from the patterns that match the current type (e.g. Global, under Atoms)
                    var subTypes =
                        typedPatterns.Select(p => p.SubType).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                    var typedPatternPaths = new Dictionary<string, string>();
                    var subTypePaths = new Dictionary<string, string>();

                    if (subTypes.Any())
                    {
                        foreach (var subType in subTypes)
                        {
                            var subTypeName = subType.StripOrdinals();
                            var subTypeDisplayName = subTypeName.ToDisplayCase();
                            var subTypePath = string.Format("{0}-{1}", type, subType);

                            // Create JSON object to hold information about the current sub-type
                            var subTypeDetails = new
                            {
                                patternSubtypeLC = subTypeName,
                                patternSubtypeUC = subTypeDisplayName,
                                patternSubtypeItems = new List<object>()
                            };

                            // Find all patterns that match the current type, and sub-type
                            var subTypedPatterns =
                                patterns.Where(
                                    p =>
                                        p.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase) &&
                                        p.SubType.Equals(subType, StringComparison.InvariantCultureIgnoreCase)).ToList();

                            foreach (var pattern in subTypedPatterns)
                            {
                                // Create JSON object to hold information about the pattern and add to sub-type JSON
                                subTypeDetails.patternSubtypeItems.Add(
                                    new
                                    {
                                        patternPath = pattern.HtmlUrl,
                                        patternState = GetState(pattern),
                                        patternPartial = pattern.Partial,
                                        patternName = pattern.Name.StripOrdinals().ToDisplayCase()
                                    });
                            }

                            // Add a 'View all' JSON object for use in the navigation
                            subTypeDetails.patternSubtypeItems.Add(
                                new
                                {
                                    patternPath = string.Format("{0}/{1}", subTypePath, FileNameViewer),
                                    patternPartial =
                                        string.Format("{0}-{1}-{2}", ViewNameViewAllPage, typeName, subTypeName),
                                    patternName = "View All"
                                });

                            // Add sub-type JSON object to the type JSON object
                            typeDetails.patternTypeItems.Add(subTypeDetails);

                            if (!subTypePaths.ContainsKey(subTypeName))
                            {
                                // Handle duplicate sub-type names
                                subTypePaths.Add(subTypeName, subTypePath);
                            }
                        }
                    }

                    foreach (var pattern in typedPatterns)
                    {
                        var patternName = pattern.Name.StripOrdinals();

                        if (!patternLinks.ContainsKey(pattern.Partial))
                        {
                            // Build list of link variables - http://patternlab.io/docs/data-link-variable.html
                            patternLinks.Add(pattern.Partial,
                                string.Format("../../{0}/{1}", FolderNamePattern.TrimStart(IdentifierHidden),
                                    pattern.HtmlUrl));
                        }

                        if (!typedPatternPaths.ContainsKey(patternName))
                        {
                            // Build list of pattern paths for footer
                            typedPatternPaths.Add(patternName, pattern.PathDash);
                        }

                        if (!subTypes.Any())
                        {
                            // Create JSON object for data required by footer
                            typeDetails.patternItems.Add(
                                new
                                {
                                    patternPath = pattern.HtmlUrl,
                                    patternState = GetState(pattern),
                                    patternPartial = pattern.Partial,
                                    patternName = pattern.Name.StripOrdinals().ToDisplayCase()
                                });
                        }
                    }

                    patternPaths.Add(typeName, typedPatternPaths);
                    viewAllPaths.Add(typeName, subTypePaths);
                    patternTypes.Add(typeDetails);
                }
            }

            // Get the media queries used by the patterns
            var mediaQueries = GetMediaQueries(DirectoryPathSource, IgnoredDirectories());

            var serializer = new JavaScriptSerializer();

            // Pass config settings and collections of pattern data to a new data collection
            _data = new Dictionary<string, object>
            {
                {"patternEngine", PatternEngine().Name()},
                {"ishminimum", Setting("ishMinimum")},
                {"ishmaximum", Setting("ishMaximum")},
                {"qrcodegeneratoron", Setting("qrCodeGeneratorOn")},
                {"ipaddress", ipAddress},
                {"xiphostname", Setting("xipHostname")},
                {"autoreloadnav", Setting("autoReloadNav")},
                {"autoreloadport", Setting("autoReloadPort")},
                {"pagefollownav", Setting("pageFollowNav")},
                {"pagefollowport", Setting("pageFollowPort")},
                {"ishControlsHide", hiddenIshControls},
                {"link", patternLinks},
                {"patternpaths", serializer.Serialize(patternPaths)},
                {"viewallpaths", serializer.Serialize(viewAllPaths)},
                {"mqs", mediaQueries},
                {"patternTypes", patternTypes}
            };

            var root = new DirectoryInfo(Path.Combine(HttpRuntime.AppDomainAppPath, FolderNameData));

            // Find any data files in the data folder and add these to the data collection
            var dataFiles = root.GetFiles(string.Concat("*", FileExtensionData), SearchOption.AllDirectories);

            _data = AppendData(_data, dataFiles);

            // Return the combined data collection
            return _data;
        }

        /// <summary>
        /// The list of directories ignored by Pattern Lab
        /// </summary>
        /// <returns>A list of directory names</returns>
        public List<string> IgnoredDirectories()
        {
            if (_ignoredDirectories != null) return _ignoredDirectories;

            // Read directory names from config
            _ignoredDirectories =
                Setting("id").Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Add some that are required to be ignored by the .NET version of Pattern Lab
            _ignoredDirectories.AddRange(new[] {"public"});

            return _ignoredDirectories;
        }

        /// <summary>
        /// The list of file extensions ignored by Pattern Lab
        /// </summary>
        /// <returns>A list of file extensions</returns>
        public List<string> IgnoredExtensions()
        {
            if (_ignoredExtensions != null) return _ignoredExtensions;

            // Read file extensions from config
            _ignoredExtensions =
                Setting("ie").Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Add some that are required to be ignored by the .NET version of Pattern Lab (string.empty handles README files)
            _ignoredExtensions.AddRange(new[] {string.Empty});

            return _ignoredExtensions;
        }

        /// <summary>
        /// The currently enabled pattern engine for handling templates
        /// </summary>
        /// <returns>A pattern engine</returns>
        public IPatternEngine PatternEngine()
        {
            if (_patternEngine != null) return _patternEngine;

            _patternEngine = SupportedPatternEngines.FirstOrDefault(e => e.Name().Equals(Setting("patternEngine"), StringComparison.InvariantCultureIgnoreCase)) ?? SupportedPatternEngines[0];

            return _patternEngine;
        }

        /// <summary>
        /// The list of patterns available to Pattern Lab
        /// </summary>
        /// <returns>A list of patterns</returns>
        public List<Pattern> Patterns()
        {
            if (_patterns != null) return _patterns;

            var root = new DirectoryInfo(Path.Combine(HttpRuntime.AppDomainAppPath, FolderNamePattern));

            // Find all template files in /patterns 
            var views = root.GetFiles(string.Concat("*", PatternEngine().Extension()), SearchOption.AllDirectories)
                    .Where(v => v.Directory != null && v.Directory.FullName != root.FullName);

            // Create a new pattern in the list for each file
            _patterns = views.Select(v => new Pattern(v.FullName)).ToList();

            // Find any patterns that contain pseudo patterns
            var parentPatterns = _patterns.Where(p => p.PseudoPatterns.Any()).ToList();
            foreach (var pattern in parentPatterns)
            {
                // Create a new pattern in the list for each pseudo pattern 
                _patterns.AddRange(pattern.PseudoPatterns.Select(p => new Pattern(pattern.FilePath, p)));
            }

            // Order the patterns by their dash delimited path
            _patterns = _patterns.OrderBy(p => p.PathDash).ToList();

            return _patterns;
        }

        /// <summary>
        /// Reads a setting from the config collection
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <returns>The value of the setting</returns>
        public string Setting(string name)
        {
            var value = Config().Global[name];

            if (!string.IsNullOrEmpty(value))
            {
                // Replace any encoded quotation marks
                value = value.Replace("\"", string.Empty);
            }

            return value;
        }

        /// <summary>
        /// Combines two data collections
        /// </summary>
        /// <param name="original">The original data collection</param>
        /// <param name="additional">The additional data</param>
        /// <returns>The combined data collection</returns>
        public static IDictionary<string, object> AppendData(IDictionary<string, object> original, IDictionary<string, object> additional)
        {
            foreach (var item in additional)
            {
                if (original.ContainsKey(item.Key))
                {
                    // Replace existing items (e.g. pattern specific data overrides provider-level data)
                    original[item.Key] = item.Value;
                }
                else
                {
                    // Add new items
                    original.Add(item.Key, item.Value);
                }
            }

            return original;
        }

        /// <summary>
        /// Combines a data collection and the contents of a data file
        /// </summary>
        /// <param name="original">The original data collection</param>
        /// <param name="dataFile">The data file</param>
        /// <returns>The combined data collection</returns>
        public static IDictionary<string, object> AppendData(IDictionary<string, object> original, FileInfo dataFile)
        {
            // Create new list of data files and append to collection
            return dataFile != null ? AppendData(original, new[] {dataFile}) : original;
        }

        /// <summary>
        /// Combines a data collection and the contents of multiple data files
        /// </summary>
        /// <param name="original">The original data collection</param>
        /// <param name="dataFiles">The data files</param>
        /// <returns>The combined data collection</returns>
        public static IDictionary<string, object> AppendData(IDictionary<string, object> original, IEnumerable<FileInfo> dataFiles)
        {
            var serializer = new JavaScriptSerializer();

            foreach (var dataFile in dataFiles)
            {
                // Serialize the contents of each file and append to collection
                AppendData(original,
                    serializer.Deserialize<IDictionary<string, object>>(File.ReadAllText(dataFile.FullName)));
            }

            return original;
        }

        /// <summary>
        /// Gets the media queries used by all CSS files in the directory
        /// </summary>
        /// <path>The path of the directory</path>
        /// <ignoredDirectories>The directory names to ignore</ignoredDirectories>
        /// <returns>A list of PX or EM values for use in the navigation</returns>
        public static List<string> GetMediaQueries(string path, List<string> ignoredDirectories)
        {
            var mediaQueries = new List<string>();

            // Find all .css files in application
            foreach (
                var filePath in Directory.GetFiles(path, "*.css", SearchOption.AllDirectories).ToList())
            {
                var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
                if (!string.IsNullOrEmpty(directory))
                {
                    // Remove application root from string
                    directory = directory.Replace(HttpRuntime.AppDomainAppPath, string.Empty);
                }

                // Skip files in ignored directories
                if (!ignoredDirectories.Where(directory.StartsWith).Any())
                {
                    var css = File.ReadAllText(filePath);
                    var queries = mediaQueries;

                    // Parse the contents and find any media queries used
                    mediaQueries.AddRange(
                        Regex.Matches(css, @"(min|max)-width:([ ]+)?(([0-9]{1,5})(\.[0-9]{1,20}|)(px|em))")
                            .Cast<Match>()
                            .Select(match => match.Groups[3].Value)
                            .Where(mediaQuery => !queries.Contains(mediaQuery)));
                }
            }

            // Sort the media queries by numeric value
            mediaQueries =
                mediaQueries.OrderBy(
                    m =>
                        double.Parse(m.Substring(0, m.LastIndexOfAny("0123456789".ToCharArray()) + 1),
                            CultureInfo.InvariantCulture))
                    .ToList();

            return mediaQueries;
        }

        /// <summary>
        /// Get the state of a pattern - http://patternlab.io/docs/pattern-states.html 
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <param name="state">The currently found state</param>
        /// <returns>The current state of the pattern, and its referenced child pattern</returns>
        public static string GetState(Pattern pattern, string state = null)
        {
            var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();

            // Read states from config. Priority is determined by the order
            var states = provider.Setting("patternStates")
                .Split(new[] {IdentifierDelimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (state == null)
            {
                // If this method hasn't been called already use the current patterns state
                state = pattern.State;
            }

            if (!string.IsNullOrEmpty(pattern.State))
            {
                var currentIndex = states.IndexOf(state);
                var newIndex = states.IndexOf(pattern.State);

                if (((newIndex < currentIndex || currentIndex < 0) && newIndex < states.Count - 1))
                {
                    // If the priority of the found state is lower that the current state and isn't the last configured state change the state to the lower value
                    state = pattern.State;
                }
            }

            // Find the lowest priority state of the pattern's referenced child patterns
            foreach (var childPattern in pattern.Lineages.Select(partial => provider.Patterns().FirstOrDefault(
                p => p.Partial.Equals(partial, StringComparison.InvariantCultureIgnoreCase)))
                .Where(childPattern => childPattern != null))
            {
                if (state == null)
                {
                    // Set to empty string to denote that this is a nested call
                    state = string.Empty;
                }

                state = GetState(childPattern, state);
            }

            if (string.IsNullOrEmpty(state))
            {
                // Reset value to null if empty for use with code-viewer.js
                state = null;
            }

            return state;
        }
    }
}