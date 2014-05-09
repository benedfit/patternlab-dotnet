using System.Web.Mvc;
using PatternLab.Core;

namespace $rootnamespace$
{
    /// <summary>
    /// Starter kit Pattern Lab application class
    /// </summary>
    public class Application : Global
    {
        /// <summary>
        /// The event that fires at application start
        /// </summary>
        public void Application_Start()
        {
            // Register any configured Areas
            AreaRegistration.RegisterAllAreas();
        }
    }
}