using System.Web.Mvc;

namespace PatternLab.Areas.Styleguide
{
    public class StyleguideAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Styleguide";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Styleguide_default",
                "Styleguide/{controller}/{action}/{id}",
                new { controller = "Styleguide", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
