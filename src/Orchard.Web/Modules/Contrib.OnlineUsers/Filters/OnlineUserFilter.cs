using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Mvc.Filters;
using Orchard.Security;
using Contrib.OnlineUsers.Models;
using Orchard;

namespace Contrib.OnlineUsers.Filters
{
    public class OnlineUserFilter : FilterProvider, IActionFilter
    {
        public OnlineUserFilter(IOrchardServices services)
        {
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = Services.WorkContext.CurrentUser;
            if (user != null)
                user.As<MembershipPart>().LastActive = DateTime.UtcNow;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext) { }
    }
}