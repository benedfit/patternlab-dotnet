using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core
{
    /// <summary>
    /// Patterns read from the directory structure - http://patternlab.io/docs/pattern-organization.html
    /// </summary>
    public class Pattern
    {
        private readonly ViewDataDictionary _data;
        private readonly string _filePath;
        private readonly string _html;
        private readonly List<string> _lineages;
        private readonly string _name;
        private readonly string _pseudoName;
        private readonly List<string> _pseudoPatterns;
        private readonly string _state;
        private readonly string _subType;
        private readonly string _type;

        /// <summary>
        /// Initialise a new pattern
        /// </summary>
        /// <param name="filePath">The template file path</param>
        public Pattern(string filePath) : this(filePath, string.Empty) { }

        /// <summary>
        /// Initialise a new pseudo-pattern - http://patternlab.io/docs/pattern-pseudo-patterns.html
        /// </summary>
        /// <param name="filePath">The template file path</param>
        /// <param name="pseudoName">The pseudo pattern name</param>
        public Pattern(string filePath, string pseudoName)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            _filePath = filePath;
            _pseudoName = pseudoName;

            // Parse the file path to replace data file extensions with template extensions (Needed for pseudo patterns)
            var path =
                ViewUrl.Replace(string.Format("~/{0}/", PatternProvider.FolderNamePattern), string.Empty)
                    .Replace(PatternProvider.FileExtensionMustache, string.Empty)
                    .Replace(PatternProvider.FileExtensionData, string.Empty);

            // Split the file path into fragments
            var pathFragments =
                path.Split(new[] {Path.AltDirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathFragments.Count <= 0) return;

            // Set the name to the last fragment
            _name = pathFragments[pathFragments.Count - 1];

            // If the name ends with a state value (e.g. @inprogress), split and set the state
            var nameFragments =
                _name.Split(new[] {PatternProvider.IdentifierState}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            if (nameFragments.Count > 1)
            {
                _name = nameFragments[0];
                _state = nameFragments[1];
            }

            // Remove the name from the fragments
            pathFragments.RemoveAt(pathFragments.Count - 1);

            // Set type and sub-type from the remaining fragments
            _type = pathFragments.Count > 0 ? pathFragments[0] : string.Empty;
            _subType = pathFragments.Count > 1 ? pathFragments[1] : string.Empty;

            // Read contents of template
            _html = File.ReadAllText(_filePath);

            // Find references to other patterns with the contents of the template
            _lineages = new List<string>();

            foreach (
                var partial in
                    Regex.Matches(_html, "{{>(.*?)}}")
                        .Cast<Match>()
                        .Select(match => match.Groups[1].Value.StripPatternParameters())
                        .Where(partial => !_lineages.Contains(partial)))
            {
                _lineages.Add(partial);
            }

            _pseudoPatterns = new List<string>();
            _data = new ViewDataDictionary();

            var folder = new DirectoryInfo(Path.GetDirectoryName(_filePath) ?? string.Empty);

            // Find all the data files for the pattern
            var dataFiles = folder.GetFiles(string.Concat("*", PatternProvider.FileExtensionData), SearchOption.AllDirectories)
                        .Where(d => d.Name.StartsWith(_name)).ToList();

            if (!string.IsNullOrEmpty(_pseudoName))
            {
                // If handling a pseudo pattern read in shared data files and the pseudo specific data file
                dataFiles =
                    dataFiles.Where(
                        d =>
                            !d.Name.Contains(PatternProvider.IdentifierPsuedo) ||
                            d.Name.EndsWith(string.Concat(_pseudoName, PatternProvider.FileExtensionData))).ToList();
            }
            else
            {
                // If the pattern isn't a pseudo pattern, create a list of pseudo patterns from data files
                foreach (
                    var pseudoNameFragments in
                        dataFiles.Where(d => d.Name.Contains(PatternProvider.IdentifierPsuedo))
                            .Select(dataFile => dataFile.Name.Replace(PatternProvider.FileExtensionData, string.Empty)
                                .Split(new[] {PatternProvider.IdentifierPsuedo},
                                    StringSplitOptions.RemoveEmptyEntries)
                                .ToList()).Where(pseudoNameFragments => pseudoNameFragments.Count > 0))
                {
                    pseudoName = pseudoNameFragments.Count > 1 ? pseudoNameFragments[1] : string.Empty;
                    if (!_pseudoPatterns.Contains(pseudoName))
                    {
                        _pseudoPatterns.Add(pseudoName);
                    }
                }

                // Ignore pseudo data files
                dataFiles = dataFiles.Where(d => !d.Name.Contains(PatternProvider.IdentifierPsuedo)).ToList();
            }

            // Append contents of data files into provider data collection
            _data = PatternProvider.AppendData(_data, dataFiles);
        }

        /// <summary>
        /// The contents of the pattern's data files
        /// </summary>
        public ViewDataDictionary Data
        {
            get { return _data; }
        }

        /// <summary>
        /// The path to the pattern's template
        /// </summary>
        public string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// Whether the pattern is hidden from navigation
        /// </summary>
        public bool Hidden
        {
            get { return Name.StartsWith(PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// The contents of the pattern's template
        /// </summary>
        public string Html
        {
            get { return _html; }
        }

        /// <summary>
        /// The URL used to access the pattern is generated HTML
        /// </summary>
        public string HtmlUrl
        {
            get { return string.Format("{0}/{0}{1}", PathDash, PatternProvider.FileExtensionHtml); }
        }

        /// <summary>
        /// The referenced patterns within the pattern's template
        /// </summary>
        public List<string> Lineages
        {
            get { return _lineages; }
        }

        /// <summary>
        /// The name of the pattern
        /// </summary>
        public string Name
        {
            get { return !string.IsNullOrEmpty(_pseudoName) ? string.Format("{0}-{1}", _name, _pseudoName) : _name; }
        }

        /// <summary>
        /// The partial path to the pattern (e.g. atoms-colors)
        /// </summary>
        public string Partial
        {
            get { return string.Format("{0}-{1}", Type.StripOrdinals(), Name.StripOrdinals()); }
        }

        /// <summary>
        /// The dash delimited path to the pattern (eg. 00-atoms-01-global-00-colors)
        /// </summary>
        public string PathDash
        {
            get { return string.Format("{0}-{1}", TypeDash, Name); }
        }

        /// <summary>
        /// The slash delimited path to the pattern (e.g. 00-atoms/01-global/00-colors)
        /// </summary>
        public string PathSlash
        {
            get { return string.Format("{0}{1}/{2}", Type, !string.IsNullOrEmpty(SubType) ? string.Concat("/", SubType) : string.Empty, Name); }
        }

        /// <summary>
        /// The list of pseudo patterns a pattern has - http://patternlab.io/docs/pattern-pseudo-patterns.html
        /// </summary>
        public List<string> PseudoPatterns
        {
            get { return _pseudoPatterns; }
        } 

        /// <summary>
        /// The state of the pattern - http://patternlab.io/docs/pattern-states.html
        /// </summary>
        public string State
        {
            get { return _state; }
        }

        /// <summary>
        /// The sub-type of the pattern - http://patternlab.io/docs/pattern-organization.html
        /// </summary>
        public string SubType
        {
            get { return _subType; }
        }

        /// <summary>
        /// The overal type of the pattern - http://patternlab.io/docs/pattern-organization.html
        /// </summary>
        public string Type
        {
            get { return _type; }
        }

        /// <summary>
        /// The dash delimited type path to the pattern (eg. 00-atoms-01-global)
        /// </summary>
        public string TypeDash
        {
            get
            {
                return string.Format("{0}{1}", Type,
                    !string.IsNullOrEmpty(SubType) ? string.Concat("-", SubType) : string.Empty);
            }
        }

        /// <summary>
        /// The virtual path to the pattern's template
        /// </summary>
        public string ViewUrl
        {
            get
            {
                return
                    FilePath.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], "~/")
                        .Replace(@"\", "/");
            }
        }
    }
}