using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Mvc;
using Orchard.Users.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.UI.Notify;
using System;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Caching;

namespace RealEstate.Users.ActiveDirectory.Services
{
    public class UserEventHandler : IUserEventHandler
    {
        private readonly ISignals _signals;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IAuthenticationService _authenticationService;
        //private readonly IActiveDirectoryService _activeDirectoryService;

        public UserEventHandler(
            ISignals signals,
            IHttpContextAccessor httpContext,
            IAuthenticationService authenticationService,
            //IActiveDirectoryService activeDirectoryService,
            IOrchardServices services)
        {
            _signals = signals;
            _httpContext = httpContext;
            _authenticationService = authenticationService;
            //_activeDirectoryService = activeDirectoryService;

            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void LoggedOut(IUser user)
        {
            //_httpContext.Current().Response.Redirect("http://www.google.com/");
            string siteUrl = Services.WorkContext.HttpContext.Request.Url.ToString();
            if (siteUrl.Contains("/thanh-vien") || siteUrl.Contains("/Account"))
                _httpContext.Current().Response.Redirect("/");
        }

        public void Creating(UserContext context) { }
        public void Created(UserContext context) {
            _signals.Trigger("Users_Changed");
        }
        public void LoggedIn(IUser user) {
            #region RealEstate.Forum
            //if (!_pageForumService.CheckHaveIdHomePage(user.Id))
            //{
            //    _pageForumService.CreateIdHomePage(user.Id);
            //    //_orchardServices.WorkContext.HttpContext.Response.Redirect("/trang-cong-dong");
            //}
            #endregion

            if (user.As<UserPart>().EmailStatus == UserStatus.Pending)
            {
                //var siteUrl = Services.WorkContext.CurrentSite.As<SiteSettings2Part>().BaseUrl;
                //if (String.IsNullOrWhiteSpace(siteUrl))
                //{
                //    //siteUrl = HttpContext.Request.ToRootUrlString();
                //}

                var wasLoggedInUser = _authenticationService.GetAuthenticatedUser();
                _authenticationService.SignOut();
                Services.WorkContext.HttpContext.Response.Redirect("/thanh-vien/chua-xac-thuc-email?email=" + user.Email); // RedirectToAction("ChallengeEmailSent");
            }

            if (user.As<UserPart>().RegistrationStatus == UserStatus.Pending)
            {
                var wasLoggedInUser = _authenticationService.GetAuthenticatedUser();
                _authenticationService.SignOut();
                Services.WorkContext.HttpContext.Response.Redirect("/thanh-vien/dang-cho-duyet"); // return RedirectToAction("RegistrationPending");
            }


            var returnUrl = _httpContext.Current().Request["ReturnUrl"];
            if (String.IsNullOrEmpty(returnUrl))
                _httpContext.Current().Response.Redirect("/");

        }
        public void AccessDenied(IUser user) { }
        public void ChangedPassword(IUser user) { }
        public void SentChallengeEmail(IUser user) {
            _httpContext.Current().Response.Cookies["userEmail"].Value = user.Email;
            var returnUrl = _httpContext.Current().Request["ReturnUrl"];
        }

        public void ConfirmedEmail(IUser user) {
            // Enable User on AD
            //var userPrincipal = _activeDirectoryService.GetDomainUserPrincipal(user.UserName);
            //userPrincipal.Enabled = true;
            //userPrincipal.Save();

            Services.Notifier.Information(T("Email của bạn đã được xác thực thành công."));
            _signals.Trigger("Users_Changed");

            // Log user in 
            //_authenticationService.SignIn(user, true);
            //_httpContext.Current().Response.Redirect("/control-panel");
        }

        public void Approved(IUser user) {
            // Enable User on AD
            //var userPrincipal = _activeDirectoryService.GetDomainUserPrincipal(user.UserName);
            //userPrincipal.Enabled = true;
            //userPrincipal.Save();

            _signals.Trigger("Users_Changed");

            // Log user in 
            //_authenticationService.SignIn(user, true);
            //_httpContext.Current().Response.Redirect("/control-panel");
        }

        public void Deleted(IUser user)
        {
            _signals.Trigger("Users_Changed");
        }

        public void LoggingIn(string userNameOrEmail, string password)
        {
            throw new NotImplementedException();
        }

        public void LogInFailed(string userNameOrEmail, string password)
        {
            throw new NotImplementedException();
        }
    }
}

