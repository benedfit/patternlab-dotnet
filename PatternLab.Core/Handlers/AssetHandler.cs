using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Handlers
{
    public class AssetHandler : IHttpHandler
    {
        private readonly RouteData _routeData;

        public AssetHandler(RouteData routeData)
        {
            _routeData = routeData;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var routeDataValues = _routeData.Values;

            var folder = routeDataValues["root"].ToString();
            if (PatternProvider.FolderNameData.EndsWith(folder, StringComparison.InvariantCultureIgnoreCase))
            {
                folder = string.Concat(PatternProvider.NameIdentifierHidden, folder);
            }

            var filePath = routeDataValues["path"] != null ? routeDataValues["path"].ToString() : string.Empty;
            var fileName = Path.GetFileName(filePath);
            var virtualPath = string.Format("/{0}/{1}", folder, filePath);

            using (var stream = VirtualPathProvider.OpenFile(virtualPath))
            {
                if (stream.Length <= 0) return;
                
                context.Response.Clear();
                context.Response.ContentType = MimeMapping.GetMimeMapping(fileName);

                stream.CopyTo(context.Response.OutputStream);
            }
        }
    }

    public class AssetRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new AssetHandler(requestContext.RouteData);
        }
    }
}