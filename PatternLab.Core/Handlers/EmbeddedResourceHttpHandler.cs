using System.IO;
using System.Web;
using System.Web.Routing;
using PatternLab.Core.Models;

namespace PatternLab.Core.Handlers
{
    public class EmbeddedResourceHttpHandler : IHttpHandler
    {
        private readonly RouteData _routeData;

        public EmbeddedResourceHttpHandler(RouteData routeData)
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
            var filePath = routeDataValues["path"] != null ? routeDataValues["path"].ToString() : string.Empty;
            var fileName = Path.GetFileName(filePath);
            var virtualPath = string.Format("~/{0}/{1}", routeDataValues["root"], filePath);

            var resource = new EmbeddedResource(virtualPath);
            using (var stream = resource.Open())
            {
                if (stream.Length <= 0) return;

                context.Response.Clear();
                context.Response.ContentType = MimeMapping.GetMimeMapping(fileName);

                stream.CopyTo(context.Response.OutputStream);
            }
        }
    }
}