using Orchard;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Users.Services;
using Orchard.ContentManagement;
using Orchard.Users.Events;
using Orchard.Localization;


using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

using RealEstate.Services;
using RealEstateForum.Service.ViewModels;
using Piedone.Facebook.Suite.Services;
using Piedone.Facebook.Suite.Models;
using System.Text.RegularExpressions;

namespace RealEstate.API.Controllers
{
    public class APIAccountController : CrossSiteController, IUpdateModel
    {
        private readonly IMembershipService _membershipService;
        private readonly IContentManager _contentManager;
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserEventHandler _userEventHandler;
        private readonly IUserGroupService _groupService;
        private readonly IFacebookConnectService _facebookConnectService;
        private readonly IPropertySettingService _settingService;
        private string _mesage = "";

        public APIAccountController(IOrchardServices services,
            IMembershipService membershipService,
            IUserService userService, 
            IAuthenticationService authenticationService,
            IFacebookConnectService facebookConnectService,
            IContentManager contentManager,
            IUserEventHandler userEventHandler,
            IUserGroupService groupService,
            IPropertySettingService settingService)
        {
            Services = services;
            _contentManager = contentManager;
            _groupService = groupService;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _facebookConnectService = facebookConnectService;
            _userService = userService;
            _settingService = settingService;
            _userEventHandler = userEventHandler;
        }
        public IOrchardServices Services { get; set; }

        #region Login

        [HttpPost]
        public JsonResult Login(string userName, string password)
        {
            try
            {
                var valid = ValidateLogOn(userName, password);
                if (valid != null)
                {
                    return Json(new { status = true, userId = valid.Id, userName = valid.UserName });
                }

                return Json(new { status = true, userId = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        public JsonResult ValidateLoginWithFacebook(FormCollection frm)
        {
            try
            {
                string email = frm["email"];
                string userName = !string.IsNullOrEmpty(frm["username"]) ? frm["username"] : frm["email"];

                var facebookUser = _contentManager.New<FacebookUserPart>("User");
                facebookUser.FacebookUserId = long.Parse(frm["id"]);
                facebookUser.Name = frm["name"];
                facebookUser.FirstName = frm["first_name"];
                facebookUser.LastName = frm["last_name"];
                facebookUser.FaceBookEmail = email;
                facebookUser.Link = frm["link"];
                facebookUser.FacebookUserName = userName;
                facebookUser.Gender = frm["gender"];
                facebookUser.TimeZone = int.Parse(frm["timezone"]);
                facebookUser.Locale = ((string)frm["locale"]).Replace('_', '-'); // Making locale Orchard-compatible
                facebookUser.IsVerified = !string.IsNullOrEmpty(frm["verified"]) ? bool.Parse(frm["verified"]) : false; // Maybe it is possible that verified is set, but is false -> don't take automatically as true if it's set

                var random = new Random();
                string password = random.Next().ToString(CultureInfo.InvariantCulture);
                if (!ValidateRegistrationFromFacebook(userName, email))
                {
                    var user = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, true));

                    var _userPart = user.As<UserPart>();
                    _userPart.EmailStatus = UserStatus.Approved;
                    _facebookConnectService.UpdateFacebookUser(user, facebookUser);

                    return Json(new { status = true, userId = user.Id});
                }
                else
                {
                    var userTemp = Services.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == email).Slice(1).FirstOrDefault();
                    var _userPart = userTemp.As<UserPart>();
                    _facebookConnectService.UpdateFacebookUser(_userPart, facebookUser);

                    return Json(new { status = true, userId = userTemp.Id});
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message});
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <returns>True => Exist</returns>
        private bool ValidateRegistrationFromFacebook(string userName, string email)
        {
            return !_userService.VerifyUserUnicity(userName, email);
        }

        public JsonResult ValidateLoginWithGoogle(FormCollection frm)
        {
            try
            {
                string email = frm["email"];
                var lowerEmail = email == null ? "" : email.ToLowerInvariant();

                var user = Services.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerEmail).Slice(1).FirstOrDefault();
                if (user == null)
                {
                    user = _membershipService.CreateUser(new CreateUserParams(lowerEmail, Guid.NewGuid().ToString(), lowerEmail, null, null, true)) as UserPart;
                    user.EmailStatus = UserStatus.Approved;
                }

                return Json(new { status = true, userId = user.Id });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion

        [HttpPost]
        public JsonResult Register(string userName,string email, string password, string confirmPassword)
        {
            try
            {
                if (ValidateRegistration(userName, email, password, confirmPassword))
                {
                    //Create User
                    var userReg = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, true));

                    var abc = userReg.As<UserPart>();
                    abc.EmailStatus = UserStatus.Approved;

                    return Json(new { status = true, userId = userReg.Id });
                }

                return Json(new { status = false, message = _mesage });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RegisterRest(string apikey, string userName, string email, string password, string confirmPassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(apikey) && !_settingService.GetSetting("API_Key_DGND").Equals(apikey))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                if (ValidateRegistration(userName, email, password, confirmPassword))
                {
                    //Create User
                    var userReg = _membershipService.CreateUser(new CreateUserParams(userName, password, email, null, null, true));

                    var abc = userReg.As<UserPart>();
                    abc.EmailStatus = UserStatus.Approved;

                    return Json(new { status = true, userId = userReg.Id });
                }

                return Json(new { status = false, message = _mesage });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RequestLostPassword(string email, string domain, string url)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(email))
                {
                    return Json(new { status = false, message = "Vui lòng nhập email" });
                }

                domain = "http://" + domain;
                _userService.SendLostPasswordEmail(email, nonce => domain + "" + url + "?code=" + nonce);
                
                return Json(new { status = true});
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateLostPassword(string confirmPassword, string password, string code)
        {
            try
            {
                IUser user;
                if ((user = _userService.ValidateLostPassword(code)) == null)
                {
                    return Json(new { status = false, message = "Mã code không đúng, vui lòng kiểm tra lại!" });
                }

                if (String.IsNullOrWhiteSpace(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    return Json(new { status = false, message = "Vui lòng nhập mật khẩu" });
                }

                if (password == null || password.Length < MinPasswordLength)
                {
                    return Json(new { status = false, message = "Mật khẩu phải ít nhất " + MinPasswordLength + " ký tự" });
                }

                if (!String.Equals(password, confirmPassword, StringComparison.Ordinal))
                {
                    return Json(new { status = false, message = "Mật khẩu mới nhập lại không trùng khớp, vui lòng kiểm tra lại!" });
                }

                _membershipService.SetPassword(user, password);

                _userEventHandler.ChangedPassword(user);

                return Json(new { status = true, userId = user.Id });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        public JsonResult ChangePassword(string currentPassword, string newPassword,string confirmPassword, string userName)
        {
            try
            {
                if (String.IsNullOrEmpty(currentPassword))
                {
                    return Json(new { status = false, message = "Vui lòng nhập mật khẩu hiện tại!"});
                }
                if (newPassword == null || newPassword.Length < MinPasswordLength)
                {
                    return Json(new { status = false, message = "Mật khẩu phải ít nhất "+ MinPasswordLength +" ký tự." });
                }

                if (!String.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
                {
                    return Json(new { status = false, message = "Mật khẩu nhập lại không trùng khớp" });
                }

                var validated = _membershipService.ValidateUser(userName, currentPassword);

                if (validated != null)
                {
                    _membershipService.SetPassword(validated, newPassword);
                    _userEventHandler.ChangedPassword(validated);

                    return Json(new { status = true, message = "Thay đổi mật khẩu thành công! " });
                }

                return Json(new { status = false, message = "Thay đổi mật khẩu không thành công! Vui lòng kiểm tra lại!" });

            }catch(Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #region Private

        private IUser ValidateLogOn(string userNameOrEmail, string password)
        {
            bool validate = true;

            if (String.IsNullOrEmpty(userNameOrEmail))
            {
                //ModelState.AddModelError("userNameOrEmail", T("You must specify a username or e-mail."));
                //validate = false;
            }
            if (String.IsNullOrEmpty(password))
            {
                //ModelState.AddModelError("password", T("You must specify a password."));
                //validate = false;
            }

            if (!validate)
                return null;

            var user = _membershipService.ValidateUser(userNameOrEmail, password);
            if (user == null)
            {
                //ModelState.AddModelError("_FORM", T("The username or e-mail or password provided is incorrect."));
            }

            return user;
        }

        private int MinPasswordLength
        {
            get
            {
                return _membershipService.GetSettings().MinRequiredPasswordLength;
            }
        }

        private bool ValidateRegistration(string userName, string email, string password, string confirmPassword)
        {
            bool validate = true;

            if (string.IsNullOrEmpty(userName))
            {
                _mesage = "Bạn phải nhập tên đăng nhập";
                validate = false;
            }
            else
            {
                if (userName.Length >= 255)
                {
                    _mesage = "Tên đăng nhập quá dài";
                    validate = false;
                }
            }

            if (string.IsNullOrEmpty(email))
            {
                _mesage = "Bạn phải nhập địa chỉ email";
                validate = false;
            }
            else if (email.Length >= 255)
            {
                _mesage = "Email quá dài";
                validate = false;
            }
            else if (!Regex.IsMatch(email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
            {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                _mesage = "Email không đúng";
                validate = false;
            }

            if (!validate)
                return false;

            if (!_userService.VerifyUserUnicity(userName, email))
            {
                _mesage = "Tên đăng nhập đã tồn tại";
                validate = false;
            }
            if (password == null || password.Length < MinPasswordLength)
            {
                _mesage = "Mật khẩu phải ít nhất " + MinPasswordLength + " ký tự";
                validate = false;
            }
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                _mesage = "Mật khẩu nhập lại không trùng khớp";
                validate = false;
            }

            return validate;
        }

        

        #endregion

        public JsonResult UpdateProfile(FormCollection frm)
        {
            try
            {
                int userId = int.Parse(frm["UserId"]);
                var userprofile = Services.ContentManager.Get(userId);
                dynamic model = Services.ContentManager.UpdateEditor(userprofile, this);

                //Init Data
                var editModel = new UserUpdateProfileEditViewModel { UserUpdate = userprofile };

                if (TryUpdateModel(editModel))
                {
                    editModel.Avatar = frm["Avatar"].ToString();
                    editModel.DisplayName = frm["DisplayName"].ToString();
                    editModel.Address = frm["Address"].ToString();
                    editModel.Phone = frm["Phone"].ToString();
                    editModel.Email = frm["Email"].ToString();
                    editModel.Website = frm["Website"].ToString();
                    editModel.Note = frm["Note"].ToString();
                }

                return Json(new { status = true, message = "success" });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #region UpdateModel
        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
    }
}