using System.Web;
using System.Web.Routing;

namespace PatternLab.Core.Handlers
{
    public class AssetRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new AssetHttpHandler(requestContext.RouteData);
        }
    }
}