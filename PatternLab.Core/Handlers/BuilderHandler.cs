using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using Abot.Crawler;
using Abot.Poco;
using CsQuery.ExtensionMethods;
using CsQuery.Implementation;
using Nustache.Core;
using PatternLab.Core.Providers;

namespace PatternLab.Core.Handlers
{
    public class BuilderHandler : IHttpHandler
    {
        private readonly RouteData _routeData;

        public BuilderHandler(RouteData routeData)
        {
            _routeData = routeData;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            // TODO: #19 Create static output generator
            context.Response.Clear();

            var routeDataValues = _routeData.Values;
            var isSnapshot = true;

            var path = routeDataValues["path"] != null
                ? routeDataValues["path"].ToString()
                : string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                path = PatternProvider.FolderNameBuilder;
                isSnapshot = false;
            }
            path = Path.Combine(HttpRuntime.AppDomainAppPath, path);

            var start = DateTime.Now;
            var memory = 0;

            var message = new StringBuilder();
            message.Append("configuring pattern lab...<br/>");

            var crawlConfig = new CrawlConfiguration {MaxConcurrentThreads = 20, DownloadableContentTypes = "text/html"};
            
            var crawler = new PoliteWebCrawler(crawlConfig, null, null, null, null, null, null, null, null);
            crawler.PageCrawlCompletedAsync += crawler_PageCrawlCompletedAsync;
            
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

            
            
            /*var provider = Controllers.PatternLabController.Provider ?? new PatternProvider();
            var data = provider.Data();
            var patterns = provider.Patterns().Where(p => !p.Hidden).ToList();

            foreach (var pattern in patterns)
            {
                var patternFilePath = Path.Combine(path, "patterns", pattern.HtmlUrl);
                var patternFolderName = Path.GetDirectoryName(patternFilePath);

                if (patternFolderName != null)
                {
                    Directory.CreateDirectory(patternFolderName);
                }

                Render.StringToFile(pattern.Html, data, patternFilePath);
            }*/

            var elapsed = DateTime.Now - start;

            message.Append("your site has been generated...<br/>");
            if (isSnapshot)
            {
                message.AppendFormat("output directory: {0}<br />", path);
            }
            message.AppendFormat("site generation took {0} seconds and used {1}MB of memory...<br/>", elapsed.TotalSeconds, memory);

            context.Response.Write(message.ToString());
        }

        void crawler_PageCrawlCompletedAsync(object sender, PageCrawlCompletedArgs e)
        {
            var crawledPage = e.CrawledPage;
            var fileName = crawledPage.Uri.AbsolutePath;
            if (fileName.EndsWith("/"))
            {
                fileName = string.Concat(fileName, "index.html");
            }
            if (fileName.StartsWith("/"))
            {
                fileName = fileName.TrimStart('/');
            }
            var filePath = Path.Combine(PatternProvider.FolderNameBuilder, fileName);
            var folderName = Path.GetDirectoryName(filePath);

            if (folderName != null)
            {
                Directory.CreateDirectory(folderName);
            }

            Render.StringToFile(crawledPage.Content.Text, null, Path.Combine(HttpRuntime.AppDomainAppPath, filePath));
        }
    }

    public class BuilderRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new BuilderHandler(requestContext.RouteData);
        }
    }
}
