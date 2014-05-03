using System.Web.Mvc;
using PatternLab.Core;

namespace PatternLab.Source
{
    public class Application : Global
    {
        public void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
        }
    }
}