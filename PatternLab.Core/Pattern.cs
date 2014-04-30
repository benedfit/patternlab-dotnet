using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core
{
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

        public Pattern(string filePath) : this(filePath, string.Empty) { }

        public Pattern(string filePath, string pseudoName)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            _filePath = filePath;
            _pseudoName = pseudoName;

            var path =
                ViewUrl.Replace(string.Format("~/{0}/", PatternProvider.FolderNamePattern), string.Empty)
                    .Replace(PatternProvider.FileExtensionMustache, string.Empty)
                    .Replace(PatternProvider.FileExtensionData, string.Empty);

            var pathFragments = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathFragments.Count <= 0) return;

            _name = pathFragments[pathFragments.Count - 1];

            var nameFragments =
                _name.Split(new[] {PatternProvider.NameIdentifierState}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            if (nameFragments.Count > 0)
            {
                _name = nameFragments.Count > 0 ? nameFragments[0] : string.Empty;
                _state = nameFragments.Count > 1 ? nameFragments[1] : string.Empty;
            }

            pathFragments.RemoveAt(pathFragments.Count - 1);

            _type = pathFragments.Count > 0 ? pathFragments[0] : string.Empty;
            _subType = pathFragments.Count > 1 ? pathFragments[1] : string.Empty;
            _html = File.ReadAllText(_filePath);
            _lineages = new List<string>();

            foreach (Match match in Regex.Matches(_html, "{{>(.*?)}}"))
            {
                var partial = match.Groups[1].Value.Trim();

                var partialFragments = partial.Split(new[] {PatternProvider.NameIdentifierParameters},
                    StringSplitOptions.RemoveEmptyEntries);
                if (partialFragments.Length > 1)
                {
                    partial = partialFragments[0];
                }

                if (!_lineages.Contains(partial))
                {
                    _lineages.Add(partial);
                }
            }

            _pseudoPatterns = new List<string>();
            _data = new ViewDataDictionary();

            var folder = new DirectoryInfo(Path.GetDirectoryName(_filePath) ?? string.Empty);
            var dataFiles = folder.GetFiles(string.Concat("*", PatternProvider.FileExtensionData), SearchOption.AllDirectories)
                        .Where(d => d.Name.StartsWith(_name)).ToList();

            if (!string.IsNullOrEmpty(_pseudoName))
            {
                dataFiles =
                    dataFiles.Where(
                        d =>
                            !d.Name.Contains(PatternProvider.NameIdentifierPsuedo) ||
                            d.Name.EndsWith(string.Concat(_pseudoName, PatternProvider.FileExtensionData))).ToList();
            }
            else
            {
                foreach (
                    var pseudoNameFragments in
                        dataFiles.Where(d => d.Name.Contains(PatternProvider.NameIdentifierPsuedo))
                            .Select(dataFile => dataFile.Name.Replace(PatternProvider.FileExtensionData, string.Empty)
                                .Split(new[] {PatternProvider.NameIdentifierPsuedo},
                                    StringSplitOptions.RemoveEmptyEntries)
                                .ToList()).Where(pseudoNameFragments => pseudoNameFragments.Count > 0))
                {
                    pseudoName = pseudoNameFragments.Count > 1 ? pseudoNameFragments[1] : string.Empty;
                    if (!_pseudoPatterns.Contains(pseudoName))
                    {
                        _pseudoPatterns.Add(pseudoName);
                    }
                }

                dataFiles = dataFiles.Where(d => !d.Name.Contains(PatternProvider.NameIdentifierPsuedo)).ToList();
            }

            _data = PatternProvider.AppendData(_data, dataFiles);
        }

        public ViewDataDictionary Data
        {
            get { return _data; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public bool Hidden
        {
            get { return Name.StartsWith(PatternProvider.NameIdentifierHidden.ToString(CultureInfo.InvariantCulture)); }
        }

        public string Html
        {
            get { return _html; }
        }

        public string HtmlUrl
        {
            get { return string.Format("{0}/{0}{1}", PathDash, PatternProvider.FileExtensionHtml); }
        }

        public List<string> Lineages
        {
            get { return _lineages; }
        }

        public string Name
        {
            get { return !string.IsNullOrEmpty(_pseudoName) ? string.Format("{0}-{1}", _name, _pseudoName) : _name; }
        }

        public string Partial
        {
            get { return string.Format("{0}-{1}", Type.StripOrdinals(), Name.StripOrdinals()); }
        }

        public string PathDash
        {
            get { return string.Format("{0}-{1}", TypeDash, Name); }
        }

        public string PathSlash
        {
            get { return string.Format("{0}{1}/{2}", Type, !string.IsNullOrEmpty(SubType) ? string.Concat("/", SubType) : string.Empty, Name); }
        }

        public List<string> PseudoPatterns
        {
            get { return _pseudoPatterns; }
        } 

        public string State
        {
            get { return _state; }
        }

        public string SubType
        {
            get { return _subType; }
        }

        public string Type
        {
            get { return _type; }
        }

        public string TypeDash
        {
            get
            {
                return string.Format("{0}{1}", Type,
                    !string.IsNullOrEmpty(SubType) ? string.Concat("-", SubType) : string.Empty);
            }
        }

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