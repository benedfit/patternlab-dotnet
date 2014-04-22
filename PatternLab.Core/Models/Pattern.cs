using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Nustache.Core;
using PatternLab.Core.Helpers;
using PatternLab.Core.Mustache;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Models
{
    public class Pattern
    {
        private readonly string _css;
        private readonly ViewDataDictionary _data;
        private readonly string _filePath;
        private readonly string _html;
        private readonly List<string> _lineages;
        private readonly string _name;
        private readonly string _state;
        private readonly string _subType;
        private readonly string _type;

        public Pattern(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            _filePath = filePath;

            var path =
                Url.Replace(string.Concat(PatternProvider.PatternsFolder, "/"), string.Empty)
                    .Replace(PatternProvider.PatternsExtension, string.Empty)
                    .Replace(PatternProvider.DataExtension, string.Empty);

            var pathFragments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathFragments.Count <= 0) return;

            _name = pathFragments[pathFragments.Count - 1];

            var nameFragments =
                _name.Split(new[] {PatternProvider.IdenfifierState}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            if (nameFragments.Count > 0)
            {
                _name = nameFragments.Count > 0 ? nameFragments[0] : string.Empty;
                _state = nameFragments.Count > 1 ? nameFragments[1] : string.Empty;
            }

            pathFragments.RemoveAt(pathFragments.Count - 1);

            _type = pathFragments.Count > 0 ? pathFragments[0] : string.Empty;
            _subType = pathFragments.Count > 1 ? pathFragments[1] : string.Empty;

            _css = string.Empty;
            if (Controllers.PatternLabController.Provider.Setting("cssEnabled")
                .Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO: Issue #8 - Implement CSS Rule Saver as per the PHP version
                _css = string.Empty;
            }

            _html = File.ReadAllText(_filePath);
            _lineages = new List<string>();

            foreach (Match match in Regex.Matches(_html, "{{>(.*?)}}"))
            {
                var partial = match.Groups[1].Value.Trim();
                
                var partialFragments = partial.Split(new[] { PatternProvider.IdentifierParameter }, StringSplitOptions.RemoveEmptyEntries);
                if (partialFragments.Length > 1)
                {
                    partial = partialFragments[0];
                }

                if (!_lineages.Contains(partial))
                {
                    _lineages.Add(partial);
                }
            }

            _data = new ViewDataDictionary();

            var folder = new DirectoryInfo(System.IO.Path.GetDirectoryName(_filePath) ?? string.Empty);

            if (filePath.Contains(PatternProvider.IdentifierPsuedo))
            {
                var psuedoNameFragments =
                    _name.Split(new[] {PatternProvider.IdentifierPsuedo}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                var pseudoName = psuedoNameFragments.Count > 0 ? psuedoNameFragments[0] : _name;

                _filePath =
                    Regex.Replace(
                        filePath.Replace(PatternProvider.DataExtension, PatternProvider.PatternsExtension),
                        @"~([A-Za-z0-9\-\@]+)", string.Empty);

                var dataFiles = folder.GetFiles(string.Concat("*", PatternProvider.DataExtension), SearchOption.AllDirectories)
                        .Where(d => d.Name.StartsWith(pseudoName));

                _data = PatternProvider.AppendData(_data, dataFiles);
            }
            else
            {
                var dataFile =
                    folder.GetFiles(string.Concat("*", PatternProvider.DataExtension), SearchOption.AllDirectories)
                        .FirstOrDefault(d => d.Name.Equals(string.Concat(_name, PatternProvider.DataExtension)));

                if (dataFile == null) return;

                _data = PatternProvider.AppendData(_data, dataFile);
            }
        }

        public string Css
        {
            get { return _css; }
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
            get { return Name.StartsWith(PatternProvider.IdentifierHidden.ToString(CultureInfo.InvariantCulture)); }
        }

        public string Html
        {
            get { return _html; }
        }

        public List<string> Lineages
        {
            get { return _lineages; }
        }

        public string Name
        {
            get { return _name.Replace(PatternProvider.IdentifierPsuedo, '-'); }
        }

        public string Partial
        {
            get { return string.Format("{0}-{1}", Type.StripOrdinals(), Name.StripOrdinals()); }
        }

        public string Path
        {
            get { return string.Format("{0}/{0}.html", PathDash); }
        }

        public string PathDash
        {
            get { return string.Format("{0}-{1}", TypeDash, Name); }
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

        public string Url
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