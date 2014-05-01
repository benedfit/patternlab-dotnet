using Nustache.Core;

namespace PatternLab.Core.Mustache
{
    public static class MustacheRender
    {
        public static string StringToString(string template, object data, TemplateLocator templateLocator)
        {
            return Render.StringToString(template, data, templateLocator);
        }
    }
}