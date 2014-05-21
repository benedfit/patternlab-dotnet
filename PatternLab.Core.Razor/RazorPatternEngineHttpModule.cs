using System;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using PatternLab.Core.Razor;

// Module auto registers itself without the need for web.config
[assembly: PreApplicationStartMethod(typeof(RazorPatternEngineHttpModule), "LoadModule")]

namespace PatternLab.Core.Razor
{
    public class RazorPatternEngineHttpModule : IHttpModule
    {
        /// <summary>
        /// Disposes of the Pattern Lab HTTP module
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Initialises the Pattern Lab HTTP module
        /// </summary>
        /// <param name="context">The current context</param>
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += PreRequestHandlerExecute;
        }

        /// <summary>
        /// Fires when the HTTP module dynamically loads
        /// </summary>
        public static void LoadModule()
        {
            // Register the module
            DynamicModuleUtility.RegisterModule(typeof(RazorPatternEngineHttpModule));
        }

        /// <summary>
        /// The pre-request handler for the HTTP handler
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event argument</param>
        public void PreRequestHandlerExecute(object sender, EventArgs e)
        {
            // Register mustache pattern engine
            var context = ((HttpApplication)sender).Context;
            context.Application["patternEngine"] = new RazorPatternEngine();
        }
    }
}