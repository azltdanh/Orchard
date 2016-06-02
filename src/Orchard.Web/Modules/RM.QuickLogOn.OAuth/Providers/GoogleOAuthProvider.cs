using System;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using RM.QuickLogOn.OAuth.Models;
using RM.QuickLogOn.Providers;
using System.Web.Mvc;

namespace RM.QuickLogOn.OAuth.Providers
{
    [OrchardFeature("RM.QuickLogOn.OAuth")]
    public class GoogleOAuthProvider : IQuickLogOnProvider
    {
        //public const string Url = "https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state={3}&openid.realm={4}&hd={5}";
        public const string Url = "https://accounts.google.com/o/oauth2/auth?response_type=code&" +
                                  "redirect_uri={2}&" +
                                  "scope={1}&" +
                                  "client_id={0}&" +
                                   "state={3}";
                                   //"access_type=offline";
                                  //"login_hint={4}&" +
                                  //"openid.realm={4}&" +
                                  //"hd={5}";

        public const string Scope = "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";

        public string Name
        {
            get { return "Đăng nhập bằng gmail"; }//Google
        }

        public string Description
        {
            get { return "Đăng nhập bằng tài khoản gmail"; }//LogOn with Your Google account
        }

        public string GetLogOnUrl(WorkContext context, string urlReturn)//added by kenji nguyen 17/03/2013 - Urlreturn
        {
            var urlHelper = new UrlHelper(context.HttpContext.Request.RequestContext);
            var part = context.CurrentSite.As<GoogleSettingsPart>();
            var clientId = part.ClientId;
            var returnUrl = context.HttpContext.Request.Url;

            var redirectUrl = new Uri(returnUrl, urlHelper.Action("Auth", "GoogleOAuth", new { Area = "RM.QuickLogOn.OAuth" })).ToString();
            return string.Format(Url, clientId, urlHelper.Encode(Scope), urlHelper.Encode(redirectUrl), urlReturn);
        }
    }
}
