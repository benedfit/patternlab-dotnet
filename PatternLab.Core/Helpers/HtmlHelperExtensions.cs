using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PatternLab.Core.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString CacheBuster(this HtmlHelper helper)
        {
            return new MvcHtmlString(Controllers.PatternsController.Provider.CacheBuster());
        }

        public static MvcHtmlString IpAddress(this HtmlHelper helper)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddresses = host.AddressList;
            return new MvcHtmlString(ipAddresses[ipAddresses.Length - 1].ToString());
        }

        public static bool IshControlsHide(this HtmlHelper helper, string name)
        {
            var setting = Controllers.PatternsController.Provider.Setting("ishControlsHide");
            if (string.IsNullOrEmpty(setting))
            {
                return false;
            }

            var hide = setting.Split(',');
            return hide.Contains(name, StringComparer.InvariantCultureIgnoreCase);
        }

        public static MvcHtmlString Config(this HtmlHelper helper, string settingName)
        {
            return new MvcHtmlString(Controllers.PatternsController.Provider.Setting(settingName));
        }
    }
}