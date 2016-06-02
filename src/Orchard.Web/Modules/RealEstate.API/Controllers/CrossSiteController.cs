using RealEstate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RealEstate.API.Controllers
{
    [AllowCrossSiteJsonAttribute]
    public class CrossSiteController : Controller
    {
        //public CrossSiteController() { }

        //private static IPropertySettingService __settingService;
        //public CrossSiteController(IPropertySettingService settingService)
        //{
        //    __settingService = settingService;
        //}

        #region Cross site

        public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
        {

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                try
                {
                    var ctx = filterContext.RequestContext.HttpContext;
                    var origin = ctx.Request.Headers["Origin"].ToString();
                    var allowOrigin = "http://clbbds.vn";

                    //string lstCrossSiteDomain = __settingService.GetSetting("ListCrossSiteDomain");// "http://localhost:65290,http://clbbds.vn,http://nhadatnt.com,http://localhost:65291";
                    //if(!string.IsNullOrEmpty(lstCrossSiteDomain))
                    //{
                    //    lstCrossSiteDomain = "http://localhost:65290,http://clbbds.vn,http://nhadatnt.com,http://localhost:65291"
                    //}

                    if (ListAllowOrigin().Contains(origin))
                    {
                        allowOrigin = origin;
                    }


                    filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", allowOrigin);

                    filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                    filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept,origin, x-requested-with");//,Content-Range,Content-Disposition,Content-Description
                    filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Max-Age", "1728000");
                    filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true");
                    //filterContext.RequestContext.HttpContext.Response.End();
                    base.OnActionExecuting(filterContext);
                }
                catch(Exception)
                {

                }
                
            }

        }

        public static List<string> ListAllowOrigin()
        {
            return new List<string>()
            {
                "http://localhost:3212",
                "http://localhost:65290",
                "http://localhost:65291",
                "http://localhost:63786",
                "http://demo.dinhgianhadat.vn",
                "http://beta.dinhgianhadat.vn",
                "http://dulieunhadat.vn",
                "http://clbbds.vn",
                "http://nhadatnt.com",
                "http://royalmarina.vn"
            };
        }

        #endregion
    }
}