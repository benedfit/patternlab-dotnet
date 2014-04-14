using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PatternLab.Core.Helpers;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Models
{
    public class Pattern
    {
        private readonly string _filePath;
        private readonly List<Pattern> _lineage;
        private readonly List<Pattern> _lineageR;
        private readonly string _name;
        private readonly string _path;
        private readonly string _state;
        private readonly string _subType;
        private readonly string _type;
        private readonly string _url;

        public Pattern(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            _filePath = filePath;
            _url =
                _filePath.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], "~/")
                    .Replace(@"\", "/");

            _path =
                Url.Replace(string.Concat(PatternProvider.PatternsFolder, "/"), string.Empty)
                    .Replace(PatternProvider.PatternViewExtension, string.Empty)
                    .Replace(PatternProvider.PatternDataExtension, string.Empty);

            var pathFragments = _path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (pathFragments.Count <= 0) return;

            _name = pathFragments[pathFragments.Count - 1];

            var nameFragments =
                _name.Split(new[] {PatternProvider.PatternStateIdenfifier}, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            if (nameFragments.Count > 0)
            {
                _name = nameFragments.Count > 0 ? nameFragments[0] : string.Empty;
                _state = nameFragments.Count > 1 ? nameFragments[1] : string.Empty;
            }

            pathFragments.RemoveAt(pathFragments.Count - 1);

            _type = pathFragments.Count > 0 ? pathFragments[0] : string.Empty;
            _subType = pathFragments.Count > 1 ? pathFragments[1] : string.Empty;

            _lineage = new List<Pattern>();
            _lineageR = new List<Pattern>();
        }

        public string Css
        {
            // TODO: Issue #8 - Implement CSS Rule Saver as per the PHP version
            get { return string.Empty; }
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public List<Pattern> Lineage
        {
            // TODO: Issues #9 - Implement lineages as per PHP version
            get { return _lineage; }
        }

        public List<Pattern> LineageR
        {
            // TODO: Issues #9 - Implement lineages as per PHP version
            get { return _lineageR; }
        }

        public string Name
        {
            get { return _name.Replace(PatternProvider.PsuedoPatternIdentifier, '-'); }
        }

        public string Partial
        {
            get { return string.Format("{0}-{1}", Type.StripOrdinals(), Name.StripOrdinals()); }
        }

        public string Path
        {
            get { return _path; }
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
            get { return _url; }
        }
    }
}