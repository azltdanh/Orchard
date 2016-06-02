using System.Text;
using System.Web.Mvc;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using RealEstate.Models;
using RealEstate.Services;

namespace RealEstate.Filters
{
	public class HostNameFilter : FilterProvider, IResultFilter {
		private readonly IResourceManager _resourceManager;
        private readonly IHostNameService _hostnameServices;

        public HostNameFilter(IResourceManager resourceManager, IOrchardServices services, IHostNameService hostnameServices)
        {
			_resourceManager = resourceManager;
            Services = services;
            _hostnameServices = hostnameServices;
		}
        public IOrchardServices Services { get; set; }

		#region IResultFilter Members

		public void OnResultExecuting(ResultExecutingContext filterContext) {
			var viewResult = filterContext.Result as ViewResult;
			if (viewResult == null)
				return;

			//Determine if we're on an admin page
			//TODO: testing for Contains("/admin") is probably a poor choice of test, but it works and follows navigation convention for Orchard so far.
			bool isAdmin = filterContext.HttpContext.Request.Url.OriginalString.ToLower().Contains("/admin");

			//Get our part data/record if available for rendering scripts
            //HostNamePart part = _hostnameServices.GetHostNamePart(_hostnameServices.GetHostNameSite());
            string siteUrlHost = Services.WorkContext.HttpContext.Request.Url != null ? Services.WorkContext.HttpContext.Request.Url.Host : "dinhgianhadat.vn";
            var partList = Services.ContentManager.Query<HostNamePart, HostNamePartRecord>().Where(a => a.Name == siteUrlHost);

            if (partList.Count() > 0)
            {

                var part = partList.Slice(1).First();

                if (part == null || string.IsNullOrWhiteSpace(part.GoogleAnalyticsKey) || (!part.TrackOnAdmin && isAdmin))
                    part = _hostnameServices.GetHostNamePartByClass("dinhgianhadat.vn");

                if (part == null || string.IsNullOrWhiteSpace(part.GoogleAnalyticsKey) || (!part.TrackOnAdmin && isAdmin))
                    return; // Not a valid configuration, ignore filter

                if (part.UseAsyncTracking)
                {
                    StringBuilder script = new StringBuilder(800);
                    script.AppendLine("<script type=\"text/javascript\">");
                    script.AppendLine("var _gaq=_gaq||[];");
                    script.AppendLine("_gaq.push([\"_setAccount\",\"" + part.GoogleAnalyticsKey + "\"]);");
                    if (!string.IsNullOrEmpty(part.DomainName))
                    {
                        script.AppendLine("_gaq.push([\"_setDomainName\",\"" + part.DomainName + "\"]);");
                        script.AppendLine("_gaq.push([\"_setAllowHash\",false]);");
                    }
                    script.AppendLine("_gaq.push([\"_trackPageview\"]);");
                    script.AppendLine("(function() {");
                    script.AppendLine("\tvar ga=document.createElement(\"script\");ga.type=\"text/javascript\";ga.async=true;");
                    script.AppendLine("\tga.src=((\"https:\" == document.location.protocol)?\"https://ssl\":\"http://www\")+\".google-analytics.com/ga.js\";");
                    script.AppendLine("\tvar s=document.getElementsByTagName(\"script\")[0];s.parentNode.insertBefore(ga, s);");
                    script.AppendLine("})();");
                    script.AppendLine("</script>");
                    // Register Google's new, recommended asynchronous analytics script to the header
                    _resourceManager.RegisterHeadScript(script.ToString());
                }
                else
                {
                    StringBuilder script = new StringBuilder(700);
                    script.AppendLine("<script type=\"text/javascript\">");
                    script.AppendLine("var gaJsHost=((\"https:\"==document.location.protocol)?\"https://ssl.\":\"http://www.\");");
                    script.AppendLine("document.write(unescape(\"%3Cscript src='\"+gaJsHost+\"google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E\"));");
                    script.AppendLine("</script>");
                    script.AppendLine("<script type=\"text/javascript\">");
                    script.AppendLine("try {");
                    script.AppendLine("\tvar pageTracker=_gat._getTracker(\"" + part.GoogleAnalyticsKey + "\");");
                    if (!string.IsNullOrWhiteSpace(part.DomainName))
                        script.AppendLine("\tpageTracker._setDomainName(\"" + part.DomainName + "\");");
                    script.AppendLine("\tpageTracker._trackPageview();");
                    script.AppendLine("}");
                    script.AppendLine("catch(err){}");
                    script.AppendLine("</script>");
                    // Register Google's old synchronous analytics script to the footer
                    _resourceManager.RegisterFootScript(script.ToString());
                }
            }
        }

		public void OnResultExecuted(ResultExecutedContext filterContext) {
		}

		#endregion
	}
}