using System;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using System.Web.Mvc;

using Orchard;
using Orchard.Themes;
using Orchard.Settings;
using Orchard.DisplayManagement;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Localization;
using Orchard.UI.Notify;

using RealEstate.Helpers;
using RealEstate.FrontEnd.Services;
using RealEstateForum.Service.ViewModels;
using Contrib.OnlineUsers.Models;
using RealEstate.Models;
using RealEstate.ViewModels;
using Orchard.UI.Navigation;
using RealEstate.Services;
using System.Web.Routing;
using Orchard.Data;
using System.Collections.Generic;
using RealEstateForum.Service.Models;

namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    [Themed]
    public class UserUpdateProfileController : Controller, IUpdateModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ISiteService _siteService;
        private readonly IOnlineUsersService _onlineservice;
        private readonly IUserGroupService _groupService;
        private readonly IAddressService _addressService;
        private readonly IUserPersonalService _userPersonalService;
        private readonly IHostNameService _hostNameService;
        private readonly IRepository<UserLocationRecord> _userLocationRepository;
        private readonly RequestContext _requestContext;
        private readonly IContentManager _contentManager;


        public UserUpdateProfileController(
            IAuthenticationService authenticationService,
            IOrchardServices services,
            ISiteService siteService,
            IOnlineUsersService onlineservice,
            IUserGroupService groupService,
            IAddressService addressService,
            IUserPersonalService userPersonalService,
            IHostNameService hostNameService,
            IRepository<UserLocationRecord> userLocationRepository,
            IShapeFactory shapeFactory,
            RequestContext requestContext,
            IContentManager contentManager)
        {
            _authenticationService = authenticationService;
            Services = services;
            _onlineservice = onlineservice;
            _siteService = siteService;
            _groupService = groupService;
            _userPersonalService = userPersonalService;
            _addressService = addressService;
            _hostNameService = hostNameService;
            _userLocationRepository = userLocationRepository;
            _requestContext = requestContext;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }
        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        #region Edit Update Profile
        public ActionResult EditProfile()
        {
            if (_authenticationService.GetAuthenticatedUser() != null)
            {
                var userId = Services.WorkContext.CurrentUser.Id;
                var userprofile = Services.ContentManager.Get(userId);
                //var usersignature = userprofile.As<SignatureUserPart>();
                var editor = Shape.EditorTemplate(TemplateName: "Parts/UserUpdateProfile.Edit", Model: new UserUpdateProfileEditViewModel(), Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(userprofile);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            return Redirect("/thanh-vien/tu-choi-truy-cap");
        }
        [ValidateInput(false)]
        [HttpPost, ActionName("EditProfile")]
        public ActionResult EditProfilePOST(HttpPostedFileBase fileBase)
        {
            var userId = Services.WorkContext.CurrentUser.Id;
            var userprofile = Services.ContentManager.Get(userId);

            dynamic model = Services.ContentManager.UpdateEditor(userprofile, this);

            var editModel = new UserUpdateProfileEditViewModel { UserUpdate = userprofile };

            if (TryUpdateModel(editModel))
            {
                // Avatar
                if (fileBase == null || fileBase.ContentLength <= 0) fileBase = Request.Files[0];
                if (fileBase != null && fileBase.ContentLength > 0)
                {
                    var fileLocation = fileBase.SaveAsUserAvatar(userId);
                    editModel.Avatar = fileLocation;
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/UserUpdateProfile.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                //Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            Services.Notifier.Information(T("Thông tin cập nhật thành công!"));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return RedirectToAction(editModel.ReturnUrl);
            }
            return RedirectToAction("EditProfile", "UserUpdateProfile", new { area = "RealEstate.MiniForum.FrontEnd" });
        }

        #endregion

        #region Index Update Profile
        [Authorize]
        public ActionResult ViewProfile(int id, string username)
        {
            var results = Services.ContentManager.Get(id);
            if (results == null) return Redirect("/thanh-vien/tu-choi-truy-cap");

            var userPart = results.As<UserPart>();
            var userUpdatePart = results.As<UserUpdateProfilePart>();
            ViewBag.userId = id;

            var detailModel = new UserUpdateProfileOptions
            {
                UserPart = userPart,
                UserUpdateProfilePart = userUpdatePart
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView(detailModel);
            }

            return View(detailModel);
        }
        #endregion

        public ActionResult LoadUserAgencies(int propertyId)
        {
            var p = Services.ContentManager.Get<PropertyPart>(propertyId);

            var model = new PropertyDisplayIndexOptions { ProvinceId = p.Province.Id };

            return PartialView("UserAgencies", model);
        }

        public ActionResult LoadUserAgencies()
        {
            var model = new PropertyDisplayEntry();

            return PartialView("AgencyRealEstateDetail", model);
        }

        public ActionResult ListUserAgencies(UserGroupIndexOptions options, PagerParameters pagerParameters)
        {
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            #region default options

            if (options == null)
                options = new UserGroupIndexOptions()
                {
                    ProvinceId = 0,
                };

            options.Provinces = _addressService.GetSelectListProvinces();
            if (!options.ProvinceId.HasValue) options.ProvinceId = currentDomainGroup.DefaultProvince != null ? currentDomainGroup.DefaultProvince.Id : 0;
            options.Districts = _addressService.GetSelectListDistricts(options.ProvinceId);

            #endregion

            IEnumerable<UserLocationRecord> userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && a.UserGroupRecord.Id == currentDomainGroup.Id).ToList();

            var agency = userLocation;

            if (options.ProvinceId != null && options.ProvinceId > 0)
                agency = agency.Where(a => a.LocationProvincePartRecord != null && a.LocationProvincePartRecord.Id == options.ProvinceId);

            if (options.DistrictId != null && options.DistrictId > 0)
                agency = agency.Where(a => a.LocationDistrictPartRecord != null && a.LocationDistrictPartRecord.Id == options.DistrictId);


            if (!string.IsNullOrEmpty(options.NameAgencies))
            {
                var nameAgencies = _userPersonalService.GetUserUpdates(options.NameAgencies).Select(a => a.Id).ToList();
                agency = agency.Where(a => nameAgencies.Contains(a.UserPartRecord.Id));
            }

            agency = agency.GroupBy(x => x.UserPartRecord.Id).Select(a => a.First());

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters) { PageSize = 18 };

            var pagerShape = Shape.Pager(pager).TotalItemCount(agency.Count());
            var results = agency.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            options.UserPartIds = results.Select(a => a.UserPartRecord.Id).ToArray();

            IEnumerable<UserUpdateProfileRecord> userUpdateProfiles;
            if (options.UserPartIds != null && options.UserPartIds.Any())
                userUpdateProfiles = _groupService.GetUserUpdateProfiles(options.UserPartIds);
            else
                userUpdateProfiles = _groupService.GetUserUpdateProfiles(0);

            var model = new UserActivitiesIndexViewModel
            {
                UserUpdateOptions = userUpdateProfiles.Select(a => new UserUpdateOptions
                {
                    UserUpdateProfilePart = a,
                    UserPart = Services.ContentManager.Get<UserPart>((int)a.Id).Record,
                    UserLocation = _groupService.GetAgencyUserLocations().Where(i => i.UserPartRecord.Id == a.Id).FirstOrDefault(),
                }),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        public ActionResult AjaxGetUserProfile(int userId)
        {
            var result = Services.ContentManager.Get(userId);

            var userPart = result.As<UserPart>();
            var userUpdatePart = result.As<UserUpdateProfilePart>();

            var viewProfileUserOrther = Url.Action("ViewProfile", "UserUpdateProfile", new { area = "RealEstate.MiniForum.FrontEnd", Id = userId, username = userPart.UserName.ToSlug() });
            var linkHomeUserOrther = Url.Action("FriendPage", "PersonalPage", new { area = "RealEstate.MiniForum.FrontEnd", UserId = userId, UserName = userPart.UserName.ToSlug() });

            return Json(new
            {
                Image = userUpdatePart.Avatar,
                Username = userPart.UserName.Split('@')[0],
                DisplayName = !string.IsNullOrEmpty(userUpdatePart.DisplayName) ? userUpdatePart.DisplayName : userPart.UserName.Split('@')[0],
                uId = userId,
                Gender = userUpdatePart.Gender,
                ViewProfileUserOrther = viewProfileUserOrther,
                LinkHomeUserOrther = linkHomeUserOrther,
                CheckIsUserOnline = _onlineservice.CheckIsUserOnline(userId)
            });
        }

        public ActionResult AjaxGetUserAgencies(string AdsTypeCssClass, int? ProvinceId, int[] DistrictIds)
        {
            PropertyDisplayIndexOptions options = new PropertyDisplayIndexOptions();

            if (ProvinceId != null)
            {
                if (DistrictIds != null)
                {
                    options.UserLocations = _groupService.GetAgencyUserLocationProvinces(ProvinceId, DistrictIds);

                }
                else
                {
                    options.UserLocations = _groupService.GetAgencyUserLocationProvinces(ProvinceId);
                }
            }
            else
            {
                options.UserLocations = _groupService.GetAgencyUserLocations();
            }

            // ReSharper restore RedundantBoolCompare
            if (AdsTypeCssClass == "ad-selling")
                options.UserPartIds = options.UserLocations.Where(a => a.IsSelling != false).Select(a => a.UserPartRecord.Id).ToArray();
            else if (AdsTypeCssClass == "ad-leasing")
                options.UserPartIds = options.UserLocations.Where(a => a.IsLeasing != false).Select(a => a.UserPartRecord.Id).ToArray();
            else
                options.UserPartIds = options.UserLocations.Select(a => a.UserPartRecord.Id).ToArray();

            IEnumerable<UserUpdateProfileRecord> userUpdateProfiles;
            if (options.UserPartIds != null && options.UserPartIds.Any())
                userUpdateProfiles = _groupService.GetUserUpdateProfiles(options.UserPartIds);
            else
                userUpdateProfiles = _groupService.GetUserUpdateProfiles(0);

            options.UserUpdateOptions = userUpdateProfiles
            .OrderBy(r => Guid.NewGuid()).Take(5)
            .Select(a => new UserUpdateOptions
            {
                UserLocation = _groupService.GetAgencyUserLocations().Where(i => i.UserPartRecord.Id == a.Id).FirstOrDefault(),
                UserUpdateProfilePart = a,
                UserPart = _contentManager.Get<UserPart>(a.Id).Record,
            });

            return PartialView("AjaxGetUserAgencies", options);
        }

        #region ajax
        public ActionResult AjaxUnitInvest()
        {
            int groupId = _groupService.GetCurrentDomainGroup().Id;
            var list = _contentManager.Query<UnitInvestPart, UnitInvestPartRecord>().Where(a => a.IsEnabled && a.GroupId == groupId).OrderBy(a => a.SeqOrder).List();

            List<UnitInvestEntry> model = list.Select(a => new UnitInvestEntry { UnitInvestPartPart = a }).ToList();

            return PartialView(model);
        }
        #endregion

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