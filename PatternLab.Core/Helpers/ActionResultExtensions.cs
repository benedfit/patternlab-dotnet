using System.Web.Mvc;

namespace PatternLab.Core.Helpers
{
    public static class ActionResultExtensions
    {
        public static string Capture(this ActionResult result, ControllerContext controllerContext)
        {
            using (var content = new ResponseCapture(controllerContext.RequestContext.HttpContext.Response))
            {
                result.ExecuteResult(controllerContext);

                return content.ToString();
            }
        }
    }
}