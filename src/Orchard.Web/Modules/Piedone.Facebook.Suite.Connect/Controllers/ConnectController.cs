﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Services;
using Piedone.Facebook.Suite.Models;
using Piedone.Facebook.Suite.Services;
using Orchard.Services;
using Piedone.HelpfulLibraries.Utilities;
using Orchard.Users.Models;

namespace Piedone.Facebook.Suite.Controllers
{
    [HandleError, Themed]
    [OrchardFeature("Piedone.Facebook.Suite.Connect")]
    public class ConnectController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ISiteService _siteService;
        private readonly IFacebookConnectService _facebookConnectService;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly INotifier _notifier;
        private readonly IClock _clock;

        public Localizer T { get; set; }

        public ConnectController(
            IOrchardServices orchardServices,
            ISiteService siteService,
            IFacebookConnectService facebookConnectService,
            IUserService userService,
            IContentManager contentManager,
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            INotifier notifier,
            IClock clock)
        {
            _orchardServices = orchardServices;
            _siteService = siteService;
            _facebookConnectService = facebookConnectService;
            _userService = userService;
            _contentManager = contentManager;
            _authenticationService = authenticationService;
            _membershipService = membershipService;
            _notifier = notifier;
            _clock = clock;

            T = NullLocalizer.Instance;
        }

        [HttpPost]
        public void SaveSession(long userId, string accessToken, int expiresIn)
        {
            // Taken one second for the whole request to take...
            _facebookConnectService.SetSession(userId, accessToken, _clock.UtcNow.AddSeconds(expiresIn - 1));
            // Could return some status too...
        }

        [HttpPost]
        public void DestroySession()
        {
            _facebookConnectService.DestroySession();
            // Could return some status too...
        }



        public ActionResult Connect(string returnUrl)
        {
            
            var settings = GetSettings();
            var facebookUser = _facebookConnectService.FetchMe();

            if (ValidateUser(facebookUser))
            {
                if (_facebookConnectService.AuthenticatedFacebookUserIsSaved())
                {
                    var user = _facebookConnectService.UpdateAuthenticatedFacebookUser(facebookUser);
                    _authenticationService.SignIn(user.As<IUser>(), false);
                }
                // With this existing users can attach their FB account to their local accounts
                else if (_authenticationService.IsAuthenticated())
                {
                    _facebookConnectService.UpdateFacebookUser(_authenticationService.GetAuthenticatedUser(), facebookUser);
                }
                else if (settings.SimpleRegistration)
                {
                    return new ShapeResult(this, _orchardServices.New.FacebookConnectRegistrationChooser(ReturnUrl: returnUrl));
                }
                else
                {
                    #region edit by thanhtuank9

                    string userName = facebookUser.FaceBookEmail;
                    string email = facebookUser.FaceBookEmail;
                    var random = new Random();
                    string password = random.Next().ToString(CultureInfo.InvariantCulture);
                    if (ValidateRegistrationFromFacebook(userName, email))
                    {
                        var user = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, true));

                        if (user == null) return Redirect("/dien-dan/dien-dan-bat-dong-san");

                        var abc = user.As<UserPart>();
                        abc.EmailStatus = UserStatus.Approved;
                        _facebookConnectService.UpdateFacebookUser(user, facebookUser);
                        _authenticationService.SignIn(user, false);
                        return this.RedirectLocal("/blog-ca-nhan");
                    }
                    else
                    {
                        var userTemp = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == email).Slice(1).FirstOrDefault();
                        var user = userTemp.As<UserPart>();
                        _facebookConnectService.UpdateFacebookUser(user, facebookUser);
                        _authenticationService.SignIn(user, false);

                        return Redirect("/blog-ca-nhan");//
                    }
                    #endregion

                }
            }
            //_notifier.Error(T("You are not logged in and connected to the app."));
            return this.RedirectLocal(returnUrl); // "this" is necessary, as this is from an extension (Orchard.Mvc.Extensions.ControllerExtensions)
        }
        #region edit by kenji nguyen
        // validate login from facebook
        private bool ValidateRegistrationFromFacebook(string userName, string email)
        {

            if (!_userService.VerifyUserUnicity(userName, email))
            {
                ModelState.AddModelError("userExists", T("Tên đăng nhập hoặc email đã được đăng ký."));
            }
            return ModelState.IsValid;
        }
        
        #endregion

        public ActionResult SimpleRegistration(string returnUrl = "")
        {
            return SimpleRegistrationForm();
        }

        [HttpPost]
        public ActionResult SimpleRegistration(string userName, string returnUrl = "")
        {
            var settings = GetSettings();

            if (!settings.SimpleRegistration)
            {
                // Only adventurers who experiemnt will ever see this.
                _notifier.Error(T("Simple registration is not allowed on this site"));
                return this.RedirectLocal(returnUrl);
            }

            // This a notably elegant solution for not checking the e-mail :-). We don't require e-mail with simple registrations.
            if (!_userService.VerifyUserUnicity(userName, "dféfdéfdkék342ü45ü43ü453578"))
            {
                // Notifier or validation summary.
                _notifier.Error(T("The username you tried to register is taken. Please choose another one."));
                //ModelState.AddModelError("userExists", T("The username you tried to register is taken. Please choose another one."));
                return SimpleRegistrationForm();
            }

            var facebookUser = _facebookConnectService.FetchMe();

            if (!ValidateUser(facebookUser)) return SimpleRegistrationForm();

            var random = new Random();
            var user = _membershipService.CreateUser(
                new CreateUserParams(
                    userName,
                    random.Next().ToString(CultureInfo.InvariantCulture),
                    "",
                    "",
                    "",
                    true));

            _facebookConnectService.UpdateFacebookUser(user, facebookUser);

            _authenticationService.SignIn(user, false);

            return this.RedirectLocal(returnUrl);
        }

        private ShapeResult SimpleRegistrationForm()
        {
            return new ShapeResult(this, _orchardServices.New.FacebookConnectSimpleRegistration());
        }

        private FacebookConnectSettingsPart GetSettings()
        {
            return _siteService.GetSiteSettings().As<FacebookConnectSettingsPart>();
        }

        private bool ValidateUser(IFacebookUser facebookUser)
        {
            IEnumerable<FacebookConnectValidationKey> errors;
            if (!_facebookConnectService.UserIsValid(facebookUser, GetSettings(), out errors))
            {
                foreach (var error in errors)
                {
                    switch (error)
                    {
                        case FacebookConnectValidationKey.NotAuthenticated:
                            _notifier.Error(T("You are not logged in and connected to the app."));
                            //ModelState.AddModelError("notVerified", T("You are not logged in and connected to the app."));
                            break;
                        case FacebookConnectValidationKey.NotVerified:
                            _notifier.Error(T("You're not a verified Facebook user. Only verified users are allowed to register, so please verify your account."));
                            //ModelState.AddModelError("notVerified", T("You're not a verified Facebook user. Only verified users are allowed to register, so please verify your account."));
                            break;
                    }
                }

                return false;
            }

            return true;
        }
    }
}
