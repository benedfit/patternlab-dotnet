using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Routing;
using Abot.Crawler;
using Abot.Poco;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Handlers
{
    public class BuilderHandler : IHttpHandler
    {
        private readonly HttpContextBase _httpContext;
        private readonly string _path;

        public BuilderHandler(RouteData routeData, HttpContextBase httpContext)
        {
            var routeDataValue = routeData.Values;

            _httpContext = httpContext;
            _path = routeDataValue["path"] != null
                ? routeDataValue["path"].ToString()
                : string.Empty;
            if (string.IsNullOrEmpty(_path))
            {
                _path = PatternProvider.FolderNameBuilder;
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // TODO: #19 Create static output generator
            context.Response.Clear();

            var start = DateTime.Now;
            var memory = 0;

            var message = new StringBuilder();
            message.Append("configuring pattern lab...<br/>");

            var crawlConfig = new CrawlConfiguration
            {
                MaxConcurrentThreads = 20,
                DownloadableContentTypes = "text/html, text/plain"
            };

            var crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);
            crawler.PageCrawlCompletedAsync += PageCrawlCompletedAsync;

            var crawlerPath = string.Format("{0}://{1}{2}{3}",
                context.Request.Url.Scheme,
                context.Request.Url.Host,
                context.Request.Url.Port == 80
                    ? string.Empty
                    : ":" + context.Request.Url.Port,
                context.Request.ApplicationPath);
            if (!crawlerPath.EndsWith("/"))
            {
                crawlerPath += "/";
            }

            var result = crawler.Crawl(new Uri(crawlerPath));
            var elapsed = DateTime.Now - start;

            message.Append("your site has been generated...<br/>");
            message.AppendFormat("site generation took {0} seconds and used {1}MB of memory...<br/>",
                elapsed.TotalSeconds, memory);

            context.Response.Write(message.ToString());
        }

        private void PageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            var crawledPage = e.CrawledPage;
            var virtualPath = crawledPage.Uri.AbsolutePath;
            if (virtualPath.EndsWith("/"))
            {
                virtualPath = string.Concat(virtualPath, "index.html");
            }
            var filePath = _httpContext.Server.MapPath(virtualPath);
            filePath = filePath.Replace(HttpRuntime.AppDomainAppPath,
                string.Format("{0}{1}\\", HttpRuntime.AppDomainAppPath, _path));
            var folderName = Path.GetDirectoryName(filePath);

            if (folderName != null)
            {
                Directory.CreateDirectory(folderName);
            }

            File.WriteAllText(filePath, crawledPage.Content.Text);
        }
    }

    public class BuilderRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new BuilderHandler(requestContext.RouteData, requestContext.HttpContext);
        }
    }
}
