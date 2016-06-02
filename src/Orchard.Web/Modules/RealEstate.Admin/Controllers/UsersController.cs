using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Contrib.OnlineUsers.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Roles.ViewModels;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Users.Services;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using System.Web;
using System.IO;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class UsersController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserGroupService _groupService;
        private readonly IMembershipService _membershipService;
        private readonly IPropertyService _propertyService;
        private readonly IRoleService _roleService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly IUserActionService _userActionService;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly IHostNameService _hostNameService;

        private readonly IRepository<UserLocationRecord> _userLocationRepository;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IUserService _userService;

        public UsersController(
            IAuthenticationService authenticationService,
            IUserService userService,
            IAddressService addressService,
            IPropertyService propertyService,
            IUserGroupService groupService,
            IMembershipService membershipService,
            IEnumerable<IUserEventHandler> userEventHandlers,
            IRepository<UserLocationRecord> userLocationRepository,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IUserActionService userActionService,
            IRoleService roleService,
            ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IHostNameService hostNameService,
            IOrchardServices services)
        {
            _authenticationService = authenticationService;
            _userService = userService;
            _addressService = addressService;
            _propertyService = propertyService;
            _groupService = groupService;
            _membershipService = membershipService;
            _userEventHandlers = userEventHandlers;

            _userLocationRepository = userLocationRepository;
            _userRolesRepository = userRolesRepository;
            _userActionService = userActionService;
            _roleService = roleService;
            _hostNameService = hostNameService;

            _siteService = siteService;
            _signals = signals;
            Services = services;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #region Index

        public ActionResult Index(UserInGroupIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list users")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new UserInGroupIndexOptions();

            options.Groups = _groupService.GetGroupsWithDefault(null, null);
            options.Roles = _roleService.GetRoles().OrderBy(a => a.Name);

            IContentQuery<UserPart, UserPartRecord> users = Services.ContentManager
                .Query<UserPart, UserPartRecord>();

            // Filter by Group
            if (options.GroupId == 0)
            {
                List<int> userInGroupIds = _groupService.GetUserInGroupIds();
                users = users.Where(u => !userInGroupIds.Contains(u.Id));
            }
            if (options.GroupId != null && options.GroupId > 0)
            {
                List<int> userInGroupIds = _groupService.GetUserInGroupIds((int)options.GroupId);
                users = users.Where(u => userInGroupIds.Contains(u.Id));
            }

            // Filter by Role
            if (options.RoleId != null && options.RoleId > 0)
            {
                RoleRecord role = _roleService.GetRole((int)options.RoleId);
                if (role != null)
                {
                    List<int> userInRoleIds =
                        _userRolesRepository.Fetch(a => a.Role == role).Select(a => a.UserId).ToList();
                    users = users.Where(u => userInRoleIds.Contains(u.Id));
                }
            }

            switch (options.Filter)
            {
                case UsersFilter.Approved:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
                case UsersFilter.Pending:
                    users = users.Where(u => u.RegistrationStatus == UserStatus.Pending);
                    break;
                case UsersFilter.EmailPending:
                    users = users.Where(u => u.EmailStatus == UserStatus.Pending);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                users = users.Where(u => u.UserName.Contains(options.Search) || u.Email.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(users.Count());

            switch (options.Order)
            {
                case UsersOrder.Name:
                    users = users.OrderBy(u => u.UserName);
                    break;
                case UsersOrder.Email:
                    users = users.OrderBy(u => u.Email);
                    break;
            }

            List<UserPart> results = users
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new UsersInGroupIndexViewModel
            {
                Users = results
                    .Select(x => new UserInGroupEntry
                    {
                        User = x.Record,
                        Group = _groupService.GetBelongGroup(x.Id)
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var viewModel = new UsersInGroupIndexViewModel
            {
                Users = new List<UserInGroupEntry>(),
                Options = new UserInGroupIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<UserInGroupEntry> checkedEntries = viewModel.Users.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case UsersBulkAction.None:
                    break;
                case UsersBulkAction.Approve:
                    foreach (UserInGroupEntry entry in checkedEntries)
                    {
                        Approve(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.Disable:
                    foreach (UserInGroupEntry entry in checkedEntries)
                    {
                        Moderate(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.ChallengeEmail:
                    foreach (UserInGroupEntry entry in checkedEntries)
                    {
                        SendChallengeEmail(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.SkipChallengeEmail:
                    foreach (UserInGroupEntry entry in checkedEntries)
                    {
                        SkipChallengeEmail(entry.User.Id);
                    }
                    break;
                case UsersBulkAction.Delete:
                    foreach (UserInGroupEntry entry in checkedEntries)
                    {
                        Delete(entry.User.Id);
                    }
                    break;
            }

            return this.RedirectLocal(viewModel.ReturnUrl);
            //return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        #endregion

        #region BulkAction

        public ActionResult LoginAs(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("Không thể vào tài khoản Super User"));
                }
                else if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("Tài khoản đang đăng nhập"));
                }
                else if (user.UserName == "dunggd" && !Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                {
                    Services.Notifier.Error(T("Không thể vào tài khoản này"));
                }
                else
                {
                    _authenticationService.SignIn(user, false);
                }
            }

            return RedirectToAction("Index", "PropertyAdmin");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserPart>(id);

            if (user != null)
            {
                if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("Không thể xóa tài khoản Super User"));
                }
                else if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("Không thể xóa tài khoản đang đăng nhập"));
                }
                else if (user.UserName == "congty" || user.UserName == "dunggd")
                {
                    Services.Notifier.Error(T("Không thể xóa tài khoản này"));
                }
                else
                {
                    // RealEstate_UserGroupPartRecord --> Alert user is a groupAdmin
                    IContentQuery<UserGroupPart, UserGroupPartRecord> groups =
                        Services.ContentManager.Query<UserGroupPart, UserGroupPartRecord>()
                            .Where(a => a.GroupAdminUser.Id == user.Id);
                    if (groups.Count() > 0)
                    {
                        UserGroupPart group = groups.List().First();
                        Services.Notifier.Error(
                            T("Không thể xóa, tài khoản đang là admin của Group <a href='{0}'>{1}</a>.",
                                Url.Action("Activities", "UserGroupAdmin", new { group.Id }), group.Name));
                    }
                    else
                    {
                        // Delete User
                        _groupService.DeleteUser(user);

                        _signals.Trigger("Users_Changed");

                        Services.Notifier.Information(T("User {0} đã xóa", user.UserName));
                        //foreach (var userEventHandler in _userEventHandlers)
                        //{
                        //    userEventHandler.Deleted(user);
                        //}
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult SendChallengeEmail(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                //var siteUrl = Services.WorkContext.CurrentSite.As<SiteSettings2Part>().BaseUrl;
                //if (String.IsNullOrWhiteSpace(siteUrl))
                //{
                //    siteUrl = HttpContext.Request.RawUrl;//.ToRootUrlString();
                //}

                _userService.SendChallengeEmail(user.As<UserPart>(),
                    nonce =>
                        Url.MakeAbsolute(Url.Action("ChallengeEmail", "Account", new { Area = "Orchard.Users", nonce })));
                Services.Notifier.Information(T("E-mail xác nhận đã được gửi cho {0}", user.UserName));
            }


            return RedirectToAction("Index");
        }

        public ActionResult SkipChallengeEmail(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                user.As<UserPart>().EmailStatus = UserStatus.Approved;
                Services.Notifier.Information(T("User {0} đã được duyệt email", user.UserName));
                foreach (IUserEventHandler userEventHandler in _userEventHandlers)
                {
                    userEventHandler.Approved(user);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Approve(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                user.As<UserPart>().RegistrationStatus = UserStatus.Approved;
                Services.Notifier.Information(T("User {0} đã được duyệt", user.UserName));
                foreach (IUserEventHandler userEventHandler in _userEventHandlers)
                {
                    userEventHandler.Approved(user);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Moderate(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<IUser>(id);

            if (user != null)
            {
                if (String.Equals(Services.WorkContext.CurrentUser.UserName, user.UserName, StringComparison.Ordinal))
                {
                    Services.Notifier.Error(T("Không thể vô hiệu tài khoản đang đăng nhập"));
                }
                else
                {
                    user.As<UserPart>().RegistrationStatus = UserStatus.Pending;
                    Services.Notifier.Information(T("User {0} đã vô hiệu", user.UserName));
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.Get<UserGroupPart>(id);

            if (userGroup != null)
            {
                userGroup.IsEnabled = true;
                Services.Notifier.Information(T("Group {0} đã cập nhật thành công", userGroup.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.Get<UserGroupPart>(id);

            if (userGroup != null)
            {
                userGroup.IsEnabled = false;
                Services.Notifier.Information(T("Group {0} đã cập nhật thành công", userGroup.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveFromGroup(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.Get<UserGroupPart>(id);

            if (userGroup != null)
            {
                Services.ContentManager.Remove(userGroup.ContentItem);
                Services.Notifier.Information(T("Group {0} đã xóa", userGroup.Name));
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Create

        public ActionResult Create(string returnUrl)
        {
            //if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
            //return new HttpUnauthorizedResult();

            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord ownGroup = _groupService.GetOwnGroup(currentUser.Id);

            // User có quyền ManageUsers hoặc user là admin của 1 group
            if (ownGroup == null &&
                !Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var createModel = new UserInGroupCreateViewModel
            {
                ReturnUrl = returnUrl,
                GroupId = 0,
                DefaultAdsTypeId = 0,
                DefaultTypeGroupId = 0,
            };

            IContentQuery<UserGroupPart, UserGroupPartRecord> availableGroups =
                Services.ContentManager.Query<UserGroupPart, UserGroupPartRecord>();
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
            {
                // Get available Groups
                // user là admin của 1 group, chỉ có quyền tạo thêm user cho group của mình
                availableGroups = availableGroups.Where(a => a.Id == ownGroup.Id);
                createModel.GroupId = ownGroup != null ? ownGroup.Id : 0;

                // Get available Roles
                IEnumerable<RoleRecord> availableRoles = _groupService.GetGroupRoles(ownGroup);
                if (availableRoles != null)
                {
                    createModel.Roles =
                        availableRoles.Select(a => new UserRoleEntry { RoleId = a.Id, Name = a.Name, Granted = false })
                            .ToList();
                }
            }
            createModel.Groups =
                availableGroups.List()
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                    .ToList();

            createModel.AdsTypes =
                _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing");
            createModel.TypeGroups = _propertyService.GetTypeGroups();

            var user = Services.ContentManager.New<IUser>("User");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserInGroup.Create", Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(user);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(UserInGroupCreateViewModel createModel)
        {
            //if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
            //    return new HttpUnauthorizedResult();

            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord ownGroup = _groupService.GetOwnGroup(currentUser.Id);

            // User có quyền ManageUsers hoặc user là admin của 1 group
            if (ownGroup == null &&
                !Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            int ownGroupId = ownGroup != null ? ownGroup.Id : 0;

            #region Validate

            if (!string.IsNullOrEmpty(createModel.UserName))
            {
                if (!_userService.VerifyUserUnicity(createModel.UserName, createModel.Email))
                {
                    AddModelError("NotUniqueUserName", T("Tên truy cập và/hoặc Email đã được sử dụng."));
                }
            }

            if (!String.IsNullOrEmpty(createModel.Email))
            {
                if (!Regex.IsMatch(createModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase))
                {
                    // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                    ModelState.AddModelError("Email", T("Vui lòng nhập Email hợp lệ."));
                }
            }

            if (createModel.Password != createModel.ConfirmPassword &&
                !String.IsNullOrEmpty(createModel.ConfirmPassword))
            {
                AddModelError("ConfirmPassword", T("Mật khẩu chưa được trùng khớp."));
            }

            #endregion

            #region create

            var user = Services.ContentManager.New<IUser>("User");
            if (ModelState.IsValid)
            {
                user = _membershipService.CreateUser(new CreateUserParams(
                    createModel.UserName,
                    createModel.Password,
                    createModel.Email,
                    null, null, true));

                _signals.Trigger("Users_Changed");

                Services.Notifier.Information(T("User <a href='{0}'>{1}</a> đã tạo thành công.",
                    Url.Action("Activities", new { user.Id }), user.UserName));

                // Save Group
                if (createModel.GroupId.HasValue)
                {
                    var group = Services.ContentManager.Get<UserGroupPart>((int)createModel.GroupId);
                    UserInGroupRecord userInGroup = _groupService.AddUserToGroup(user.As<UserPart>(), group.Record);
                    userInGroup.DefaultAdsType = _propertyService.GetAdsType(createModel.DefaultAdsTypeId);
                    userInGroup.DefaultTypeGroup = _propertyService.GetTypeGroup(createModel.DefaultTypeGroupId);
                }

                // Save Roles

                var viewModel = new UserActivitiesIndexViewModel { Roles = new List<UserRoleEntry>() };
                UpdateModel(viewModel);

                IEnumerable<UserRolesPartRecord> currentUserRoleRecords =
                    _userRolesRepository.Fetch(x => x.UserId == user.Id).ToList();
                IEnumerable<RoleRecord> currentRoleRecords = currentUserRoleRecords.Select(x => x.Role).ToList();
                IEnumerable<RoleRecord> targetRoleRecords =
                    viewModel.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId)).ToList();

                //if (targetRoleRecords.Any())
                {
                    foreach (RoleRecord addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x)))
                    {
                        Services.Notifier.Warning(T("Thêm quyền {0} cho user {1}", addingRole.Name, user.UserName));
                        _userRolesRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = addingRole });
                    }
                    //if (currentUserRoleRecords.Any())
                    {
                        foreach (
                            UserRolesPartRecord removingRole in
                                currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role)))
                        {
                            Services.Notifier.Warning(T("Xóa quyền {0} của user {1}", removingRole.Role.Name,
                                user.UserName));
                            _userRolesRepository.Delete(removingRole);
                        }
                    }
                }
            }

            dynamic model = Services.ContentManager.UpdateEditor(user, this);

            #endregion

            #region error

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                IContentQuery<UserGroupPart, UserGroupPartRecord> availableGroups =
                    Services.ContentManager.Query<UserGroupPart, UserGroupPartRecord>();

                if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
                {
                    // Get available Groups
                    // user là admin của 1 group, chỉ có quyền tạo thêm user cho group của mình
                    availableGroups = availableGroups.Where(a => a.Id == ownGroupId);
                    createModel.GroupId = ownGroupId;

                    // Get available Roles
                    IEnumerable<RoleRecord> availableRoles = _groupService.GetGroupRoles(ownGroup);
                    if (availableRoles != null)
                    {
                        createModel.Roles =
                            availableRoles.Select(a => new UserRoleEntry { RoleId = a.Id, Name = a.Name, Granted = false })
                                .ToList();
                    }
                }
                createModel.Groups =
                    availableGroups.List()
                        .Select(
                            a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                        .ToList();

                createModel.AdsTypes =
                    _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing");
                createModel.TypeGroups = _propertyService.GetTypeGroups();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserInGroup.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
            {
                return RedirectToAction("Activities", "UserGroupAdmin", new { id = ownGroupId });
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.RedirectLocal(createModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Activities

        public ActionResult Activities(int id, UserGroupIndexOptions options, PagerParameters pagerParameters)
        {
            var user = Services.ContentManager.Get<UserPart>(id);
            var userUpdate = user.As<UserUpdateProfilePart>();

            #region default options

            if (options == null)
                options = new UserGroupIndexOptions
                {
                    UserActionId = 0,
                    ProvinceId = 0,
                    DistrictId = 0,
                    WardId = 0,
                    GroupId = 0,
                };

            options.UserActions = _userActionService.GetUserActions();
            options.Provinces = _addressService.GetSelectListProvinces();
            options.Districts = _addressService.GetSelectListDistricts(options.ProvinceId);
            options.Wards = _addressService.GetWards(options.DistrictId);

            if (string.IsNullOrEmpty(options.DateFrom))
                options.DateFrom = DateExtension.GetStartOfCurrentMonth().ToString("dd/MM/yyyy");
            if (string.IsNullOrEmpty(options.DateTo))
                options.DateTo = DateExtension.GetEndOfCurrentMonth().ToString("dd/MM/yyyy");

            #endregion

            #region Activities

            IEnumerable<UserActivityPartRecord> userActivities = _groupService.GetUserActivities(user.Record,
                options.DateFrom, options.DateTo).ToList();

            // Filter by UserActionId
            if (options.UserActionId > 0)
            {
                userActivities = userActivities.Where(a => a.UserActionPartRecord.Id == options.UserActionId);
            }

            options.TotalPoints = userActivities.Sum(a => a.UserActionPartRecord.Point);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(userActivities.Count());

            userActivities = userActivities.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            #endregion

            #region Roles

            List<UserRoleEntry> roles = null;
            List<int> userRoleIds = _groupService.GetUserRoles(user).Select(a => a.Id).ToList();
            IEnumerable<RoleRecord> availableRoles = _groupService.GetUserAvailableRoles(user);
            if (availableRoles != null)
            {
                roles =
                    availableRoles.Select(
                        a => new UserRoleEntry { RoleId = a.Id, Name = a.Name, Granted = userRoleIds.Contains(a.Id) })
                        .ToList();
            }

            #endregion

            #region Locations

            IEnumerable<UserLocationRecord> userLocations =
                _userLocationRepository.Fetch(a => a.UserPartRecord == user.Record && a.EnableIsAgencies != true); // Không hiện nv môi giới trong bảng này

            if (options.ProvinceId > 0)
                userLocations =
                    userLocations.Where(
                        a =>
                            a.LocationProvincePartRecord != null &&
                            a.LocationProvincePartRecord.Id == options.ProvinceId);

            if (options.DistrictId > 0)
                userLocations =
                    userLocations.Where(
                        a =>
                            a.LocationDistrictPartRecord != null &&
                            a.LocationDistrictPartRecord.Id == options.DistrictId);

            if (options.WardId > 0)
                userLocations =
                    userLocations.Where(
                        a => a.LocationWardPartRecord != null && a.LocationWardPartRecord.Id == options.WardId);

            //if (options.GroupId > 0)
            //    userLocations =
            //        userLocations.Where(
            //            a => a.UserGroupRecord != null && a.UserGroupRecord.Id == options.GroupId);

            //var hostName = _hostNameService.GetHostNameSite();
            //var hostDefault = _hostNameService.GetHostNamePart(hostName);

            //if (hostDefault != null && hostDefault.CssClass == "host-name-main")
            //    userLocations = userLocations.Where(a => a.UserGroupRecord == null || a.UserGroupRecord.Id == hostDefault.Id).OrderBy(a => a.UserPartRecord);
            //else
            //    userLocations = userLocations.Where(a => a.UserGroupRecord.Id == hostDefault.Id).OrderBy(a => a.UserPartRecord);

            #endregion

            #region Permissions

            UserGroupPartRecord jointGroup = _groupService.GetJointGroup(user.Id);
            UserGroupPartRecord ownGroup = _groupService.GetOwnGroup(user.Id);

            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            bool currentUserIsGroupAdmin = (jointGroup != null) && (jointGroup.GroupAdminUser.Id == currentUser.Id);

            bool enableViewUserPoints = Services.Authorizer.Authorize(Permissions.ViewJointGroupUserPoints) ||
                                        currentUserIsGroupAdmin;

            bool enableEditUser = Services.Authorizer.Authorize(Permissions.ManageUsers) || currentUserIsGroupAdmin;

            bool enableEditLocations = Services.Authorizer.Authorize(Permissions.ManageUsers);

            #endregion

            #region Group && Setting

            int groupId = jointGroup != null ? jointGroup.Id : 0;

            IEnumerable<UserGroupPartRecord> availableGroups = _groupService.GetAvailableGroups(user);

            if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
            {
                // Get available Groups
                // user là admin của 1 group, chỉ có quyền tạo thêm user cho group của mình
                UserGroupPartRecord currentUserOwnGroup = _groupService.GetOwnGroup(currentUser.Id);
                availableGroups = availableGroups.Where(a => a.Id == currentUserOwnGroup.Id);
            }

            int defaultAdsTypeId = 0;
            int defaultTypeGroupId = 0;
            UserInGroupRecord userInGroup = _groupService.GetUserInGroup(user);
            if (userInGroup != null)
            {
                defaultAdsTypeId = userInGroup.DefaultAdsType != null ? userInGroup.DefaultAdsType.Id : 0;
                defaultTypeGroupId = userInGroup.DefaultTypeGroup != null ? userInGroup.DefaultTypeGroup.Id : 0;
            }

            #endregion

            #region Build Model

            var model = new UserActivitiesIndexViewModel
            {
                // Activities
                UserActivities = userActivities,
                EnableViewUserPoints = enableViewUserPoints,

                // Account
                EnableEditProfile = enableEditUser,
                User = user,
                UserName = user.UserName,
                Email = user.Email,

                // Profile
                UserUpdateProfile = userUpdate,

                // Setting
                GroupId = groupId,
                Groups = availableGroups,
                DefaultAdsTypeId = defaultAdsTypeId,
                AdsTypes =
                    _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing"),
                DefaultTypeGroupId = defaultTypeGroupId,
                TypeGroups = _propertyService.GetTypeGroups(),

                // Roles
                EnableEditRoles = enableEditUser,
                Roles = roles,

                // Locations
                EnableEditLocations = enableEditLocations,
                UserLocations = userLocations,
                JointGroup = jointGroup,
                OwnGroup = ownGroup,
                Options = options,
                Pager = pagerShape
            };

            #endregion

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.UpdateUserAccount")]
        public ActionResult UpdateUserAccount(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            var user = Services.ContentManager.Get<UserPart>(id, VersionOptions.DraftRequired);
            string previousName = user.UserName;

            //dynamic model = Services.ContentManager.UpdateEditor(user, this);

            var editModel = new UserActivitiesIndexViewModel { User = user };
            if (TryUpdateModel(editModel))
            {
                if (!_userService.VerifyUserUnicity(id, editModel.UserName, editModel.Email))
                {
                    AddModelError("NotUniqueUserName", T("Tên truy cập và/hoặc Email đã được sử dụng."));
                    Services.Notifier.Error(T("Tên truy cập và/hoặc Email đã được sử dụng."));
                }
                else if (!Regex.IsMatch(editModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase))
                {
                    if (!String.IsNullOrEmpty(editModel.Email))
                    {
                        // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                        ModelState.AddModelError("Email", T("Vui lòng nhập Email hợp lệ."));
                        Services.Notifier.Error(T("Vui lòng nhập Email hợp lệ."));
                    }
                }
                else
                {
                    // also update the Super user if this is the renamed account
                    if (String.Equals(Services.WorkContext.CurrentSite.SuperUser, previousName, StringComparison.Ordinal))
                    {
                        _siteService.GetSiteSettings().As<SiteSettingsPart>().SuperUser = editModel.UserName;
                    }

                    user.UserName = editModel.UserName;
                    user.Email = editModel.Email;
                    user.NormalizedUserName = editModel.UserName.ToLowerInvariant();

                    // Save Group
                    if (editModel.GroupId.HasValue)
                    {
                        var group = Services.ContentManager.Get<UserGroupPart>((int)editModel.GroupId);
                        UserInGroupRecord userInGroup = _groupService.AddUserToGroup(user.As<UserPart>(), group.Record);
                        userInGroup.DefaultAdsType = _propertyService.GetAdsType(editModel.DefaultAdsTypeId);
                        userInGroup.DefaultTypeGroup = _propertyService.GetTypeGroup(editModel.DefaultTypeGroupId);
                    }

                    // Reset Password
                    if (!string.IsNullOrEmpty(editModel.Password))
                    {
                        _membershipService.SetPassword(user, editModel.Password);
                    }

                    Services.ContentManager.Publish(user.ContentItem);

                    _signals.Trigger("Users_Changed");

                    Services.Notifier.Information(T("User {0} đã cập nhật thành công", user.UserName));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("User {0} cập nhật thất bại", user.UserName));
            }

            string returnUrl = Url.Action("Activities", new { id }); // +"#profile";
            return Redirect(returnUrl);
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.UpdateUserRoles")]
        public ActionResult UpdateUserRoles(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            var user = Services.ContentManager.Get<UserPart>(id, VersionOptions.DraftRequired);

            //dynamic model = Services.ContentManager.UpdateEditor(user, this);

            var viewModel = new UserActivitiesIndexViewModel { Roles = new List<UserRoleEntry>() };
            UpdateModel(viewModel);

            IEnumerable<UserRolesPartRecord> currentUserRoleRecords =
                _userRolesRepository.Fetch(x => x.UserId == user.Id).ToList();
            IEnumerable<RoleRecord> currentRoleRecords = currentUserRoleRecords.Select(x => x.Role).ToList();

            IEnumerable<RoleRecord> targetRoleRecords =
                viewModel.Roles.Where(x => x.Granted).Select(x => _roleService.GetRole(x.RoleId)).ToList();

            foreach (RoleRecord addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x)))
            {
                Services.Notifier.Warning(T("Thêm quyền {0} cho user {1}", addingRole.Name, user.UserName));
                _userRolesRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = addingRole });
            }
            foreach (
                UserRolesPartRecord removingRole in
                    currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role)))
            {
                Services.Notifier.Warning(T("Xóa quyền {0} của user {1}", removingRole.Role.Name, user.UserName));
                _userRolesRepository.Delete(removingRole);
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("User {0} cập nhật thất bại", user.UserName));
            }

            string returnUrl = Url.Action("Activities", new { id }); // +"#roles";
            return Redirect(returnUrl);
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.AddUserLocation")]
        public ActionResult AddUserLocation(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
                return new HttpUnauthorizedResult();
            //string hostname = _hostNameService.GetHostNameSite();
            if (options.ProvinceId > 0)
            {
                var user = Services.ContentManager.Get<UserPart>(id);

                var record = new UserLocationRecord
                {
                    UserPartRecord = user.Record,
                    LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId),
                    LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId),
                    LocationWardPartRecord = _addressService.GetWard(options.WardId),
                    RetrictedAccessGroupProperties = options.RetrictedAccessGroupProperties,
                    EnableAccessProperties = options.EnableAccessProperties,
                    EnableIsAgencies = options.EnableIsAgencies,
                    EnableEditLocations = options.EnableEditLocations,
                };

                _userLocationRepository.Create(record);

                //clear cache
                _groupService.ClearUserLocationCache();
                _groupService.ClearUserLocationCache(options.ProvinceId);
            }

            string returnUrl = Url.Action("Activities", new { id }); // +"#locations";
            return Redirect(returnUrl);
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.EditUserLocation")]
        public ActionResult EditUserLocation(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
                return new HttpUnauthorizedResult();

            if (options.ProvinceId > 0)
            {
                UserLocationRecord record = _userLocationRepository.Get(options.UserLocationEditId ?? 0);
                if (record != null)
                {
                    record.LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId);
                    record.LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId);
                    record.LocationWardPartRecord = _addressService.GetWard(options.WardId);
                    record.RetrictedAccessGroupProperties = options.RetrictedAccessGroupProperties;
                    record.EnableAccessProperties = options.EnableAccessProperties;
                    record.EnableIsAgencies = options.EnableIsAgencies;
                    record.EnableEditLocations = options.EnableEditLocations;
                }

                _userLocationRepository.Update(record);

                //clear cache
                _groupService.ClearUserLocationCache();
                _groupService.ClearUserLocationCache(options.ProvinceId);
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("User location update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.UpdateUserProfile")]
        public ActionResult UpdateUserProfile(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input, HttpPostedFileBase fileBase)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers))
                return new HttpUnauthorizedResult();

            var user = Services.ContentManager.Get<UserUpdateProfilePart>(id, VersionOptions.DraftRequired);

            var editModel = new UserActivitiesIndexViewModel { UserUpdateProfile = user };
            if (TryUpdateModel(editModel))
            {
                user.DisplayName = editModel.UserUpdateProfile.DisplayName;
                user.Phone = editModel.UserUpdateProfile.Phone;
                user.AreaAgencies = editModel.UserUpdateProfile.AreaAgencies;
                user.EndDateAgencing = editModel.UserUpdateProfile.EndDateAgencing;

                // Avatar
                if (fileBase == null || fileBase.ContentLength <= 0) fileBase = Request.Files[0];
                if (fileBase != null && fileBase.ContentLength > 0)
                {
                    var fileLocation = fileBase.SaveAsUserAvatar(user.Id);
                    user.Avatar = fileLocation;
                }

                Services.ContentManager.Publish(user.ContentItem);
                _signals.Trigger("Users_Changed");

                Services.Notifier.Information(T("User {0} đã cập nhật thành công", user.Id));
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("User {0} cập nhật thất bại", user.Id));
            }

            string returnUrl = Url.Action("Activities", new { id }); // +"#profile";
            return Redirect(returnUrl);
        }

        #endregion

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}