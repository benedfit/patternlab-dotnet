using System.Web.Mvc;
using PatternLab.Core;

namespace PatternLab.Source
{
    public class Application : PatternLabApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
        }
    }
}