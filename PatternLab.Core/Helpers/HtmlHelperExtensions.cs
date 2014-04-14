using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PatternLab.Core.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString CacheBuster(this HtmlHelper helper)
        {
            bool enabled;
            if (!Boolean.TryParse(ConfigurationManager.AppSettings["PatternLabCacheBusterOn"], out enabled))
            {
                enabled = false;
            }

            return new MvcHtmlString(enabled ? DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture) : "0");
        }

        public static MvcHtmlString IpAddress(this HtmlHelper helper)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddresses = host.AddressList;
            return new MvcHtmlString(ipAddresses[ipAddresses.Length - 1].ToString());
        }

        public static bool IshControlsHide(this HtmlHelper helper, string name)
        {
            var hide = ConfigurationManager.AppSettings["PatternLabIshControlsHide"].Split(',');
            return hide.Contains(name, StringComparer.InvariantCultureIgnoreCase);
        }

        public static MvcHtmlString Setting(this HtmlHelper helper, string settingName)
        {
            return new MvcHtmlString(ConfigurationManager.AppSettings[settingName]);
        }
    }
}