using System.Web.Mvc;

namespace PatternLab.Core.Helpers
{
    /// <summary>
    /// Pattern Lab extension methods for ActionResults
    /// </summary>
    public static class ActionResultExtensions
    {
        /// <summary>
        /// Captures the contents of an ActionResult as a string
        /// </summary>
        /// <param name="result">The ActionResult</param>
        /// <param name="controllerContext">The current ControllerContext</param>
        /// <returns>Returns the contents of the ActionResult as a string</returns>
        public static string Capture(this ActionResult result, ControllerContext controllerContext)
        {
            // Use the current response
            using (var content = new ResponseCapture(controllerContext.RequestContext.HttpContext.Response))
            {
                // Execute the ActionResult against the current ControllerContext and return the contents
                result.ExecuteResult(controllerContext);

                return content.ToString();
            }
        }
    }
}