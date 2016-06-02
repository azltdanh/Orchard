using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Navigation.Services;
using Orchard.Core.Title.Models;
using Orchard.Localization;
using Orchard.UI.Navigation;
using RealEstate.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace RealEstate.API.Controllers
{
    public class NavServiceController : ApiController
    {
        private readonly IContentManager _contentManager;
        private readonly INavigationManager _navManager;
        private readonly INavigationProvider _navProvider;
        private readonly IMenuProvider _menuProvider;
        private readonly IMenuService _menuService;

        public NavServiceController(IContentManager contentManager, INavigationManager navManager, INavigationProvider navProvider, IMenuProvider menuProvider, IMenuService menuService)
        {
            _menuService = menuService;
            _menuProvider = menuProvider;
            _navProvider = navProvider;
            _navManager = navManager;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        //[AllowCrossSiteJsonAttribute]
        public List<NavItem> GetMainNav(int id)
        {
            var content = _contentManager.Query("Page").List();
            foreach (ContentItem item in content)
            {
                var test = item.As<TitlePart>();
                var test2 = test;
            }

            var request = HttpContext.Current.Request;
            var url = string.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);

            IContent menu = _menuService.GetMenu("Main Menu");
            IEnumerable<MenuItem> menuItems = _navManager.BuildMenu(menu).ToList();
            List<NavItem> serialMenu = new List<NavItem>();
            foreach (var item in menuItems)
            {
                //checks for absolute urls entered in the Orchard admin so that it doesn't include the domain prefix
                if (item.Href.Contains("http"))
                {
                    NavItem myItem = new NavItem() { ItemName = item.Text.ToString(), ItemUrl = item.Href };
                    myItem.Items = new List<NavItem>();
                    foreach (var subItem in item.Items)
                    {
                        if (item.Href.Contains("http"))
                        {
                            myItem.Items.Add(new NavItem() { ItemName = subItem.Text.ToString(), ItemUrl = subItem.Href });
                        }
                        else
                        {
                            myItem.Items.Add(new NavItem() { ItemName = subItem.Text.ToString(), ItemUrl = url + subItem.Href });
                        }
                    }
                    serialMenu.Add(myItem);
                }
                else
                {
                    NavItem myItem = new NavItem() { ItemName = item.Text.ToString(), ItemUrl = url + item.Href };
                    myItem.Items = new List<NavItem>();
                    foreach (var subItem in item.Items)
                    {
                        if (subItem.Href.Contains("http"))
                        {
                            myItem.Items.Add(new NavItem() { ItemName = subItem.Text.ToString(), ItemUrl = subItem.Href });
                        }
                        else
                        {
                            myItem.Items.Add(new NavItem() { ItemName = subItem.Text.ToString(), ItemUrl = url + subItem.Href });
                        }

                    }
                    serialMenu.Add(myItem);
                }
            }
            return serialMenu;
        }

        //public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
        //{
        //    public override void OnActionExecuting(ActionExecutingContext filterContext)
        //    {
        //        //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
        //        //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        //        //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
        //        //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Max-Age", "1728000");
        //        //filterContext.RequestContext.HttpContext.Response.End();
        //        //base.OnActionExecuting(filterContext);

        //        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");

        //        if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
        //        {
        //            //These headers are handling the "pre-flight" OPTIONS call sent by the browser
        //            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        //            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
        //            HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000");
        //            HttpContext.Current.Response.End();
        //        }
        //    }
        //}
    }
    
}