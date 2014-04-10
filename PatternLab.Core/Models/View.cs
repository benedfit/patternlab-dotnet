using System;
using System.Linq;
using System.Web;
using PatternLab.Core.Providers;
using PatternLab.Core.Helpers;

namespace PatternLab.Core.Models
{
    public class View
    {
        private string _name;
        private string _path;
        private string _state;
        private string _subType;
        private string _type;
        private string _url;

        public View(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                _url = filePath.Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], "~/").Replace(@"\", "/");
                
                _path = Url.Replace(string.Concat(ViewsProvider.ViewsFolder, "/"), string.Empty).Replace(ViewsProvider.ViewExtension, string.Empty);
                
                var pathFragments = _path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (pathFragments.Count > 0)
                {
                    _name = pathFragments[pathFragments.Count - 1];
                    
                    var nameFragments = _name.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (nameFragments.Count > 0)
                    {
                        _name = nameFragments.Count > 0 ? nameFragments[0]: string.Empty;
                        _state = nameFragments.Count > 1 ? nameFragments[1] : string.Empty;
                    }

                    pathFragments.RemoveAt(pathFragments.Count - 1);

                    _type = pathFragments.Count > 0 ? pathFragments[0] : string.Empty;
                    _subType = pathFragments.Count > 1 ? pathFragments[1] : string.Empty;
                }
            }
        }

        public string Name
        {
            get { return _name; }
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
            get { return string.Format("{0}{1}", Type, !string.IsNullOrEmpty(SubType) ? string.Concat("-", SubType) : string.Empty); }
        }
        
        public string Url { 
            get { return _url; }
        }
    }
}