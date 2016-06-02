using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class UserGroupAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IRepository<UserGroupContactRecord> _groupContactRepository;
        private readonly IRepository<UserGroupLocationRecord> _groupLocationRepository;
        private readonly IUserGroupService _groupService;
        private readonly IRepository<UserGroupSharedLocationRecord> _groupSharedLocationRepository;
        private readonly IRepository<UserLocationRecord> _userLocationRepository;

        private readonly IPropertyService _propertyService;
        private readonly IPropertySettingService _settingService;
        private readonly ISiteService _siteService;
        private readonly IHostNameService _hostNameService;

        public UserGroupAdminController(
            IRepository<UserGroupContactRecord> groupContactRepository,
            IRepository<UserGroupLocationRecord> groupLocationRepository,
            IRepository<UserGroupSharedLocationRecord> groupSharedLocationRepository,
            IRepository<UserLocationRecord> userLocationRepository,
            IAddressService addressService,
            IPropertyService propertyService,
            IUserGroupService groupService,
            IPropertySettingService settingService,
            IHostNameService hostNameService,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IOrchardServices services)
        {
            _groupContactRepository = groupContactRepository;
            _groupLocationRepository = groupLocationRepository;
            _groupSharedLocationRepository = groupSharedLocationRepository;
            _userLocationRepository = userLocationRepository;

            _addressService = addressService;
            _propertyService = propertyService;
            _groupService = groupService;
            _settingService = settingService;
            _hostNameService = hostNameService;
            _siteService = siteService;

            Services = services;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #region Index

        public ActionResult Index(UserGroupIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to list User Group")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new UserGroupIndexOptions();

            IContentQuery<UserGroupPart, UserGroupPartRecord> list = Services.ContentManager
                .Query<UserGroupPart, UserGroupPartRecord>();

            switch (options.Filter)
            {
                case UserGroupFilter.All:
                    //userGroups = userGroups.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                list = list.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(list.Count());

            switch (options.Order)
            {
                case UserGroupOrder.SeqOrder:
                    list = list.OrderBy(u => u.SeqOrder);
                    break;
                case UserGroupOrder.Name:
                    list = list.OrderBy(u => u.Name);
                    break;
            }

            List<UserGroupPart> results = list
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new UserGroupIndexViewModel
            {
                UserGroups = results
                    .Select(x => new UserGroupEntry { UserGroup = x.Record, HostName = x.As<HostNamePart>() })
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
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var viewModel = new UserGroupIndexViewModel
            {
                UserGroups = new List<UserGroupEntry>(),
                Options = new UserGroupIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<UserGroupEntry> checkedEntries = viewModel.UserGroups.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case UserGroupBulkAction.None:
                    break;
                case UserGroupBulkAction.Enable:
                    foreach (UserGroupEntry entry in checkedEntries)
                    {
                        Enable(entry.UserGroup.Id);
                    }
                    break;
                case UserGroupBulkAction.Disable:
                    foreach (UserGroupEntry entry in checkedEntries)
                    {
                        Disable(entry.UserGroup.Id);
                    }
                    break;
                case UserGroupBulkAction.Delete:
                    foreach (UserGroupEntry entry in checkedEntries)
                    {
                        Delete(entry.UserGroup.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        #endregion

        public ActionResult MyGroup()
        {
            var ownGroup = _groupService.GetOwnGroup(Services.WorkContext.CurrentUser.Id);
            if (ownGroup != null)
            {
                return RedirectToAction("Activities", new { ownGroup.Id });
            }
            var belongGroup = _groupService.GetBelongGroup(Services.WorkContext.CurrentUser.Id);
            if (belongGroup != null)
            {
                return RedirectToAction("Activities", new { belongGroup.Id });
            }
            return RedirectToAction("Index", "PropertyAdmin");
        }

        #region Create

        public ActionResult Create(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.New<UserGroupPart>("UserGroup");
            userGroup.IsEnabled = true;
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/UserGroup.Create",
                Model: new UserGroupCreateViewModel
                {
                    DefaultProvinceId = 0,
                    Provinces = _addressService.GetSelectListProvinces(),
                    DefaultDistrictId = 0,
                    Districts = _addressService.GetSelectListDistricts(0),
                    DefaultPropertyStatusId = 0,
                    PropertyStatus = _propertyService.GetStatusForInternal(),
                    DefaultAdsTypeId = 0,
                    AdsTypes = _propertyService.GetAdsTypes(),
                    DefaultTypeGroupId = 0,
                    TypeGroups = _propertyService.GetTypeGroups(),
                    ReturnUrl = returnUrl
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(userGroup);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(UserGroupCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_groupService.VerifyUserGroupUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueUserGroupName", T("UserGroup with that name already exists."));
                }
            }

            var group = Services.ContentManager.New<UserGroupPart>("UserGroup");
            if (ModelState.IsValid)
            {
                group.Name = createModel.Name;
                group.ShortName = createModel.ShortName;
                group.SeqOrder = createModel.SeqOrder;
                group.IsEnabled = createModel.IsEnabled;
                group.Point = createModel.Point;

                group.ContactPhone = createModel.ContactPhone;
                if (createModel.GroupAdminUserId > 0)
                    group.GroupAdminUser = _groupService.GetUser(createModel.GroupAdminUserId).Record;
                if (createModel.DefaultProvinceId > 0)
                    group.DefaultProvince = _addressService.GetProvince(createModel.DefaultProvinceId);
                if (createModel.DefaultDistrictId > 0)
                    group.DefaultDistrict = _addressService.GetDistrict(createModel.DefaultDistrictId);
                group.DefaultPropertyStatus = _propertyService.GetStatus(createModel.DefaultPropertyStatusId);
                group.DefaultAdsType = _propertyService.GetAdsType(createModel.DefaultAdsTypeId);
                group.DefaultTypeGroup = _propertyService.GetTypeGroup(createModel.DefaultTypeGroupId);

                group.AllowedAdminSingleIPs = createModel.AllowedAdminSingleIPs;
                group.AllowedAdminMaskedIPs = createModel.AllowedAdminMaskedIPs;
                group.DeniedAdminSingleIPs = createModel.DeniedAdminSingleIPs;
                group.DeniedAdminMaskedIPs = createModel.DeniedAdminMaskedIPs;

                group.NumberOfAdsGoodDeal = createModel.NumberOfAdsGoodDeal;
                //group.NumberOfAdsVIP = createModel.NumberOfAdsVIP;
                group.NumberOfAdsHighlight = createModel.NumberOfAdsHighlight;

                Services.ContentManager.Create(group);
            }

            dynamic model = Services.ContentManager.UpdateEditor(group, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                createModel.Provinces = _addressService.GetSelectListProvinces();
                createModel.Districts = _addressService.GetSelectListDistricts(createModel.DefaultProvinceId);
                createModel.PropertyStatus = _propertyService.GetStatusForInternal();
                createModel.AdsTypes = _propertyService.GetAdsTypes();
                createModel.TypeGroups = _propertyService.GetTypeGroups();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("UserGroup <a href='{0}'>{1}</a> created successfully.",
                Url.Action("Activities", new { group.Id }), group.Name));

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.RedirectLocal(createModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Edit

        public ActionResult Edit(int id, PagerParameters pagerParameters, string returnUrl, string dateFrom,
            string dateTo)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupProfiles, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var group = Services.ContentManager.Get<UserGroupPart>(id);
            UserGroupEditViewModel editModel = _groupService.BuildEditViewModel(group, dateFrom, dateTo);

            // DefaultPropertyStatusId
            editModel.PropertyStatus = _propertyService.GetStatusForInternal();
            editModel.DefaultPropertyStatusId = group.DefaultPropertyStatus != null ? group.DefaultPropertyStatus.Id : 0;

            // DefaultAdsTypeId
            editModel.AdsTypes = _propertyService.GetAdsTypes();
            editModel.DefaultAdsTypeId = group.DefaultAdsType != null ? group.DefaultAdsType.Id : 0;

            // DefaultTypeGroupId
            editModel.TypeGroups = _propertyService.GetTypeGroups();
            editModel.DefaultTypeGroupId = group.DefaultTypeGroup != null ? group.DefaultTypeGroup.Id : 0;

            editModel.ReturnUrl = returnUrl;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            int totalCount = editModel.GroupUsers.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
            editModel.Pager = pagerShape;

            editModel.GroupUsers =
                editModel.GroupUsers.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize).ToList();

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.Edit", Model: editModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(group);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, PagerParameters pagerParameters, FormCollection frmCollection)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupProfiles, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var group = Services.ContentManager.Get<UserGroupPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(group, this);

            var editModel = new UserGroupEditViewModel { Group = group };
            if (TryUpdateModel(editModel))
            {
                if (!_groupService.VerifyUserGroupUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueUserGroupName", T("UserGroup with that name already exists."));
                }

                if (editModel.GroupAdminUserId > 0)
                    group.GroupAdminUser = _groupService.GetUser(editModel.GroupAdminUserId).Record;
                if (editModel.DefaultProvinceId > 0)
                    group.DefaultProvince = _addressService.GetProvince(editModel.DefaultProvinceId);
                if (editModel.DefaultDistrictId > 0)
                    group.DefaultDistrict = _addressService.GetDistrict(editModel.DefaultDistrictId);
                group.DefaultPropertyStatus = _propertyService.GetStatus(editModel.DefaultPropertyStatusId);
                group.DefaultAdsType = _propertyService.GetAdsType(editModel.DefaultAdsTypeId);
                group.DefaultTypeGroup = _propertyService.GetTypeGroup(editModel.DefaultTypeGroupId);

                var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

                // Add Users to Group

                #region Add Users to Group

                if (!string.IsNullOrEmpty(frmCollection["submit.BulkUpdateGroupUsers"]))
                {
                    //var group = userGroup.Record;

                    // Get Selected Users
                    //var selectedUserIds = new List<int>();
                    //if (editModel.GroupUsers != null)
                    //{
                    //    IEnumerable<UserInGroupEntry> checkedEntries = editModel.GroupUsers.Where(a => a.IsChecked);
                    //    selectedUserIds = checkedEntries.Select(a => a.UserInGroupRecord.UserPartRecord.Id).ToList();
                    //}

                    // Add Single User
                    if (editModel.ToAddUserId.HasValue)
                    {
                        UserPart user = _groupService.GetUser(editModel.ToAddUserId);
                        if (user != null)
                            _groupService.AddUserToGroup(user, group.Record);
                    }

                    // Add Multi Users
                    if (editModel.ToAddUserIds != null)
                        _groupService.AddUsersToGroup(editModel.ToAddUserIds, group.Record);

                    return RedirectToAction("Edit", new { id, page = pager.Page });
                }

                #endregion

                // Show Users Points from Date to Date

                #region Show Users Points

                if (!string.IsNullOrEmpty(frmCollection["submit.ShowUserPoints"]))
                {
                    return RedirectToAction("Edit",
                        new
                        {
                            id,
                            page = pager.Page,
                            returnUrl = editModel.ReturnUrl,
                            dateFrom = editModel.DateFrom,
                            dateTo = editModel.DateTo
                        });
                }

                #endregion
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.Provinces = _addressService.GetSelectListProvinces();
                editModel.PropertyStatus = _propertyService.GetStatusForInternal();
                editModel.AdsTypes = _propertyService.GetAdsTypes();
                editModel.TypeGroups = _propertyService.GetTypeGroups();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("UserGroup <a href='{0}'>{1}</a> updated",
                Url.Action("Edit", new { group.Id }), group.Name));

            // Save & Continue
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinue"]))
            {
                return RedirectToAction("Edit", new { id, returnUrl = editModel.ReturnUrl });
            }
            // Back to ReturnUrl
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Locations

        public ActionResult Locations(int id, UserGroupIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupLocations))
                return new HttpUnauthorizedResult();

            var group = Services.ContentManager.Get<UserGroupPart>(id);

            // default options
            if (options == null)
                options = new UserGroupIndexOptions
                {
                    ProvinceId = 0,
                    DistrictId = 0,
                    WardId = 0,
                };
            options.Provinces = _addressService.GetSelectListProvinces();
            options.Districts = _addressService.GetSelectListDistricts(options.ProvinceId);
            options.Wards = _addressService.GetWards(options.DistrictId);

            // Locations
            IEnumerable<UserGroupLocationRecord> groupLocations =
                _groupLocationRepository.Fetch(a => a.UserGroupPartRecord == group.Record);
            if (options.ProvinceId > 0)
                groupLocations =
                    groupLocations.Where(
                        a =>
                            a.LocationProvincePartRecord != null &&
                            a.LocationProvincePartRecord.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                groupLocations =
                    groupLocations.Where(
                        a =>
                            a.LocationDistrictPartRecord != null &&
                            a.LocationDistrictPartRecord.Id == options.DistrictId);
            if (options.WardId > 0)
                groupLocations =
                    groupLocations.Where(
                        a => a.LocationWardPartRecord != null && a.LocationWardPartRecord.Id == options.WardId);

            groupLocations = groupLocations.OrderBy(a => a.LocationProvincePartRecord.SeqOrder);

            var userGroupLocationRecords = groupLocations as IList<UserGroupLocationRecord> ?? groupLocations.ToList();

            // Pager
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(userGroupLocationRecords.Count());

            // Slice
            groupLocations = userGroupLocationRecords.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize).ToList();

            var model = new GroupLocationsIndexViewModel
            {
                Group = group,
                GroupLocations = groupLocations,
                Options = options,
                Pager = pagerShape
            };

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View("GroupLocations", model);
        }

        [HttpPost]
        [FormValueRequired("submit.Add")]
        public ActionResult Locations(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection frmCollection)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupLocations))
                return new HttpUnauthorizedResult();

            var group = Services.ContentManager.Get<UserGroupPart>(id);

            var record = new UserGroupLocationRecord
            {
                UserGroupPartRecord = group.Record,
                LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId),
                LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId),
                LocationWardPartRecord = _addressService.GetWard(options.WardId)
            };
            _groupLocationRepository.Create(record);

            return RedirectToAction("Locations", new { id });
        }

        #endregion

        #region BulkAction

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.Get<UserGroupPart>(id);

            if (userGroup != null)
            {
                userGroup.IsEnabled = true;
                Services.Notifier.Information(T("UserGroup {0} updated", userGroup.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.Get<UserGroupPart>(id);

            if (userGroup != null)
            {
                userGroup.IsEnabled = false;
                Services.Notifier.Information(T("UserGroup {0} updated", userGroup.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup, T("Not authorized to manage User Group")))
                return new HttpUnauthorizedResult();

            var userGroup = Services.ContentManager.Get<UserGroupPart>(id);

            if (userGroup != null)
            {
                Services.ContentManager.Remove(userGroup.ContentItem);
                Services.Notifier.Information(T("UserGroup {0} deleted", userGroup.Name));
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Activities

        public ActionResult Activities(int id, UserGroupIndexOptions options, PagerParameters pagerParameters)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            DateTime startIndex = DateTime.Now;

            if (!Services.Authorizer.Authorize(Permissions.ViewJointGroupUserPoints) &&
                (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            #region default options

            // default options
            if (options == null)
                options = new UserGroupIndexOptions
                {
                    // Location
                    ProvinceId = 0,
                    DistrictId = 0,
                    WardId = 0,
                    // Contact
                    AdsTypeId = 0,
                    // Shared Location
                    SharedProvinceId = 0,
                    SharedDistrictId = 0,
                    SharedWardId = 0,
                    GroupId = 0,
                    UserId = 0,
                };

            options.Provinces = _addressService.GetSelectListProvinces();
            if (options.Provinces[0].Value != "0") options.Provinces.Insert(0, new SelectListItem { Text = "-- Chọn tất cả Tỉnh/TP --", Value = "0" });
            if (!options.ProvinceId.HasValue) options.ProvinceId = group.DefaultProvince != null ? group.DefaultProvince.Id : 0;

            options.Districts = _addressService.GetSelectListDistricts(options.ProvinceId);
            if (!options.DistrictId.HasValue) options.DistrictId = group.DefaultDistrict != null ? group.DefaultDistrict.Id : 0;

            options.Wards = _addressService.GetWards(options.DistrictId);


            options.AdsTypes =
                _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing");
            options.TypeGroups = _propertyService.GetTypeGroups();

            options.Groups =
                Services.ContentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .Where(a => a.Id != id)
                    .List()
                    .Select(a => a.Record)
                    .OrderBy(a => a.SeqOrder);

            if (string.IsNullOrEmpty(options.DateFrom))
                options.DateFrom = DateExtension.GetStartOfCurrentMonth().ToString("dd/MM/yyyy");
            if (string.IsNullOrEmpty(options.DateTo))
                options.DateTo = DateExtension.GetEndOfCurrentMonth().ToString("dd/MM/yyyy");

            #endregion

            #region Activities

            IEnumerable<UserInGroupEntry> groupUsers = _groupService.GetGroupUsersEntries(group.Id, options.DateFrom,
                options.DateTo).ToList();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(groupUsers.Count());

            groupUsers = groupUsers.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize).ToList();

            IEnumerable<RoleRecord> roles = _groupService.GetGroupRoles(group.Record);

            #endregion

            #region Profile

            #endregion

            #region Settings

            #endregion

            #region Contacts

            IEnumerable<UserGroupContactRecord> groupContacts =
                _groupContactRepository.Fetch(a => a.UserGroupPartRecord == group.Record);

            if (options.ProvinceId > 0)
                groupContacts =
                    groupContacts.Where(
                        a =>
                            a.LocationProvincePartRecord != null &&
                            a.LocationProvincePartRecord.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                groupContacts =
                    groupContacts.Where(
                        a =>
                            a.LocationDistrictPartRecord != null &&
                            a.LocationDistrictPartRecord.Id == options.DistrictId);
            if (options.AdsTypeId > 0)
                groupContacts =
                    groupContacts.Where(a => a.AdsTypePartRecord != null && a.AdsTypePartRecord.Id == options.AdsTypeId);
            if (options.TypeGroupId > 0)
                groupContacts =
                    groupContacts.Where(
                        a =>
                            a.PropertyTypeGroupPartRecord != null &&
                            a.PropertyTypeGroupPartRecord.Id == options.TypeGroupId);

            //_groupContacts = _groupContacts.OrderBy(a => a.LocationProvincePartRecord);

            #endregion

            #region Locations

            IEnumerable<UserGroupLocationRecord> groupLocations =
                _groupLocationRepository.Fetch(a => a.UserGroupPartRecord == group.Record);
            if (options.ProvinceId > 0)
                groupLocations =
                    groupLocations.Where(
                        a =>
                            a.LocationProvincePartRecord != null &&
                            a.LocationProvincePartRecord.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                groupLocations =
                    groupLocations.Where(
                        a =>
                            a.LocationDistrictPartRecord != null &&
                            a.LocationDistrictPartRecord.Id == options.DistrictId);
            if (options.WardId > 0)
                groupLocations =
                    groupLocations.Where(
                        a => a.LocationWardPartRecord != null && a.LocationWardPartRecord.Id == options.WardId);

            groupLocations = groupLocations.OrderBy(a => a.LocationProvincePartRecord.SeqOrder);

            #endregion

            #region GroupAddUserAgencies

            IEnumerable<UserLocationRecord> userLocations;
            if (group.Id == _hostNameService.GetHostNamePartByClass("host-name-main").Id)
                userLocations = _userLocationRepository.Fetch(a => (a.UserGroupRecord.Id == group.Id || a.UserGroupRecord == null) && a.EnableIsAgencies == true);
            else
                userLocations = _userLocationRepository.Fetch(a => a.UserGroupRecord.Id == group.Id && a.EnableIsAgencies == true);

            if (options.UserId > 0)
                userLocations =
                    userLocations.Where(
                        a =>
                            a.UserPartRecord != null &&
                            a.UserPartRecord.Id == options.UserId);

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

            var pagersecond = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShapeSecond = Shape.Pager(pagersecond).TotalItemCount(userLocations.Count());

            userLocations = userLocations.Skip((pagersecond.Page - 1) * pagersecond.PageSize).Take(pagersecond.PageSize).ToList();

            #endregion

            #region Shared Locations

            IEnumerable<UserGroupSharedLocationRecord> groupSharedLocations =
                _groupSharedLocationRepository.Fetch(
                    a => a.SeederUserGroupPartRecord == group.Record || a.LeecherUserGroupPartRecord == group.Record);
            if (options.SharedProvinceId > 0)
                groupSharedLocations =
                    groupSharedLocations.Where(
                        a =>
                            a.LocationProvincePartRecord != null &&
                            a.LocationProvincePartRecord.Id == options.SharedProvinceId);
            if (options.SharedDistrictId > 0)
                groupSharedLocations =
                    groupSharedLocations.Where(
                        a =>
                            a.LocationDistrictPartRecord != null &&
                            a.LocationDistrictPartRecord.Id == options.SharedDistrictId);
            if (options.SharedWardId > 0)
                groupSharedLocations =
                    groupSharedLocations.Where(
                        a => a.LocationWardPartRecord != null && a.LocationWardPartRecord.Id == options.SharedWardId);

            groupSharedLocations = groupSharedLocations.OrderBy(a => a.LocationProvincePartRecord.SeqOrder);

            #endregion

            #region Permissions

            bool currentUserIsGroupAdmin = group.GroupAdminUser.Id == currentUser.Id;

            bool enableEditProfile = Services.Authorizer.Authorize(Permissions.EditJointGroupProfiles);
            bool enableEditSettings = Services.Authorizer.Authorize(Permissions.EditJointGroupSettings) || currentUserIsGroupAdmin;
            bool enableEditContacts = Services.Authorizer.Authorize(Permissions.EditJointGroupContacts) || currentUserIsGroupAdmin;
            bool enableEditLocations = Services.Authorizer.Authorize(Permissions.EditJointGroupLocations);
            bool enableEditSharedLocations = Services.Authorizer.Authorize(Permissions.EditJointGroupSharedLocations) || currentUserIsGroupAdmin;
            bool enableGroupAddUserAgencies = Services.Authorizer.Authorize(Permissions.EditJointGroupUserAgencies) || currentUserIsGroupAdmin;

            #endregion

            #region Build Model

            var model = new GroupActivitiesIndexViewModel
            {
                // Activities
                GroupUsers = groupUsers,
                Roles = roles,

                // Profile
                EnableEditProfile = enableEditProfile,
                Group = group,
                GroupAdminUserId = group.GroupAdminUser.Id,
                //AvailableGroupMemberUsers = new List<UserPartRecord>(),
                //  _groupService.GetAvailableGroupMemberUsers(group.Record),
                PropertyStatus = _propertyService.GetStatusForInternal(),
                AdsTypes =
                    _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing"),
                TypeGroups = _propertyService.GetTypeGroups(),

                // Settings
                EnableEditSettings = enableEditSettings,
                CurrentUserIpAddress = Services.WorkContext.HttpContext.Request.UserHostAddress,
                CheckAllowedIPs = _settingService.CheckAllowedIPs(),

                // Contacts
                EnableEditContacts = enableEditContacts,
                GroupContacts = groupContacts,

                // Locations
                EnableEditLocations = enableEditLocations,
                GroupLocations = groupLocations,

                // Shared Locations
                EnableEditSharedLocations = enableEditSharedLocations,
                GroupSharedLocations = groupSharedLocations,

                // GroupAddUserAgencies
                EnableGroupAddUserAgencies = enableGroupAddUserAgencies,
                UserLocations = userLocations,

                Options = options,
                Pager = pagerShape,
                PagerSecond = pagerShapeSecond,
                TotalExecutionTime = (DateTime.Now - startIndex).TotalSeconds
            };

            #endregion

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);
            pagerShapeSecond.RouteData(routeData);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View(model);
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.UpdateGroupUsers")]
        public ActionResult UpdateGroupUsers(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection frmCollection)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            if (!Services.Authorizer.Authorize(Permissions.ViewJointGroupUserPoints) &&
                (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var editModel = new UserGroupEditViewModel { Group = group };
            if (TryUpdateModel(editModel))
            {
                #region Add Users to Group

                //var group = userGroup.Record;

                // Get Selected Users
                //var selectedUserIds = new List<int>();
                //if (editModel.GroupUsers != null)
                //{
                //    IEnumerable<UserInGroupEntry> checkedEntries = editModel.GroupUsers.Where(a => a.IsChecked);
                //    selectedUserIds = checkedEntries.Select(a => a.UserInGroupRecord.UserPartRecord.Id).ToList();
                //}

                // Add Single User
                if (editModel.ToAddUserId.HasValue)
                {
                    UserPart user = _groupService.GetUser(editModel.ToAddUserId);
                    if (user != null)
                        _groupService.AddUserToGroup(user, group.Record);
                }

                // Add Multi Users
                if (editModel.ToAddUserIds != null)
                    _groupService.AddUsersToGroup(editModel.ToAddUserIds, group.Record);

                #endregion

                // Show Users Points from Date to Date

                #region Show Users Points

                if (!string.IsNullOrEmpty(frmCollection["submit.ShowUserPoints"]))
                {
                }

                #endregion
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities",
                new
                {
                    id,
                    page = pager.Page,
                    returnUrl = editModel.ReturnUrl,
                    dateFrom = editModel.DateFrom,
                    dateTo = editModel.DateTo
                });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.UpdateGroupProfile")]
        public ActionResult UpdateGroupProfile(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection frmCollection)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupProfiles))
                return new HttpUnauthorizedResult();

            var group = Services.ContentManager.Get<UserGroupPart>(id);

            var editModel = new UserGroupEditViewModel { Group = group };
            if (TryUpdateModel(editModel))
            {
                if (!_groupService.VerifyUserGroupUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueUserGroupName", T("UserGroup with that name already exists."));
                }

                if (editModel.GroupAdminUserId > 0)
                    group.GroupAdminUser = _groupService.GetUser(editModel.GroupAdminUserId).Record;

                // DefaultProvince
                if (editModel.DefaultProvinceId > 0)
                    group.DefaultProvince = _addressService.GetProvince(editModel.DefaultProvinceId);
                else
                {
                    group.DefaultProvince = null;
                    group.DefaultDistrict = null;
                }

                // DefaultDistrict
                if (editModel.DefaultDistrictId > 0)
                    group.DefaultDistrict = _addressService.GetDistrict(editModel.DefaultDistrictId);
                else
                    group.DefaultDistrict = null;

                if (editModel.DefaultPropertyStatusId > 0)
                    group.DefaultPropertyStatus = _propertyService.GetStatus(editModel.DefaultPropertyStatusId);
                if (editModel.DefaultAdsTypeId > 0)
                    group.DefaultAdsType = _propertyService.GetAdsType(editModel.DefaultAdsTypeId);
                if (editModel.DefaultTypeGroupId > 0)
                    group.DefaultTypeGroup = _propertyService.GetTypeGroup(editModel.DefaultTypeGroupId);
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.UpdateGroupIPs")]
        public ActionResult UpdateGroupIPs(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection frmCollection)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupSettings) && (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            var editModel = new UserGroupEditViewModel { Group = group };
            if (TryUpdateModel(editModel))
            {
                // Auto updated
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.AddGroupContact")]
        public ActionResult AddGroupContact(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            bool currentUserIsGroupAdmin = group.GroupAdminUser.Id == currentUser.Id;
            bool enableGroupAddUserAgencies = Services.Authorizer.Authorize(Permissions.EditJointGroupContacts) ||
                                             currentUserIsGroupAdmin;

            if (!enableGroupAddUserAgencies)
                return new HttpUnauthorizedResult();

            if (!String.IsNullOrEmpty(options.ContactPhone))
            {
                var record = new UserGroupContactRecord
                {
                    UserGroupPartRecord = group.Record,
                    LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId),
                    LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId),
                    AdsTypePartRecord = _propertyService.GetAdsType(options.AdsTypeId),
                    PropertyTypeGroupPartRecord = _propertyService.GetTypeGroup(options.TypeGroupId),
                    ContactPhone = options.ContactPhone
                };

                _groupContactRepository.Create(record);
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.EditGroupContact")]
        public ActionResult EditGroupContact(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupContacts))
                return new HttpUnauthorizedResult();

            if (!String.IsNullOrEmpty(options.ContactPhone))
            {
                if (options.GroupContactEditId > 0)
                {
                    UserGroupContactRecord groupContact = _groupContactRepository.Get(options.GroupContactEditId ?? 0);
                    if (groupContact != null)
                    {
                        if (options.ProvinceId > 0)
                            groupContact.LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId);
                        if (options.DistrictId > 0)
                            groupContact.LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId);
                        if (options.AdsTypeId > 0)
                            groupContact.AdsTypePartRecord = _propertyService.GetAdsType(options.AdsTypeId);
                        if (options.TypeGroupId > 0)
                            groupContact.PropertyTypeGroupPartRecord = _propertyService.GetTypeGroup(options.TypeGroupId);
                        groupContact.ContactPhone = options.ContactPhone;
                    }

                    _groupContactRepository.Update(groupContact);
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.AddGroupLocation")]
        public ActionResult AddGroupLocation(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupLocations))
                return new HttpUnauthorizedResult();

            var group = Services.ContentManager.Get<UserGroupPart>(id);

            var record = new UserGroupLocationRecord
            {
                UserGroupPartRecord = group.Record,
                LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId),
                LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId),
                LocationWardPartRecord = _addressService.GetWard(options.WardId)
            };

            if (record.LocationProvincePartRecord != null)
                _groupLocationRepository.Create(record);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.AddGroupSharedLocation")]
        public ActionResult AddGroupSharedLocation(int id, UserGroupIndexOptions options,
            PagerParameters pagerParameters, FormCollection input)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            IUser currentUser = Services.WorkContext.CurrentUser;

            if (group.GroupAdminUser.Id != currentUser.Id)
                if (!Services.Authorizer.Authorize(Permissions.EditJointGroupSharedLocations))
                    return new HttpUnauthorizedResult();

            var record = new UserGroupSharedLocationRecord
            {
                SeederUserGroupPartRecord = group.Record,
                LeecherUserGroupPartRecord = _groupService.GetGroup(options.GroupId),
                LocationProvincePartRecord = _addressService.GetProvince(options.SharedProvinceId),
                LocationDistrictPartRecord = _addressService.GetDistrict(options.SharedDistrictId),
                LocationWardPartRecord = _addressService.GetWard(options.SharedWardId)
            };

            if (record.LeecherUserGroupPartRecord != null && record.LocationProvincePartRecord != null)
                _groupSharedLocationRepository.Create(record);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Group information update failed"));
            }

            return RedirectToAction("Activities", new { id });
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.AddUserLocation")]
        public ActionResult AddUserLocation(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            bool currentUserIsGroupAdmin = group.GroupAdminUser.Id == currentUser.Id;
            bool enableGroupAddUserAgencies = Services.Authorizer.Authorize(Permissions.EditJointGroupUserAgencies) ||
                                             currentUserIsGroupAdmin;

            if (!enableGroupAddUserAgencies)
                return new HttpUnauthorizedResult();

            int newUserGroupId = 0;
            if (options.ProvinceId > 0)
            {
                var record = new UserLocationRecord
                {
                    UserPartRecord = _groupService.GetUser(options.UserId).Record,
                    LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId),
                    LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId),
                    LocationWardPartRecord = _addressService.GetWard(options.WardId),
                    EnableIsAgencies = true,
                    AreaAgencies = options.AreaAgencies,
                    EndDateAgencing = options.EndDateAgencing,
                    IsLeasing = options.IsLeasing,
                    IsSelling = options.IsSelling,
                    UserGroupRecord = group.Record
                };

                _userLocationRepository.Create(record);

                newUserGroupId = record.Id;
                //clear cache
                _groupService.ClearUserLocationCache();
                _groupService.ClearUserLocationCache(options.ProvinceId);
            }

            //cập nhật thông tin môi giới.
            IEnumerable<UserLocationRecord> userLocations;
            if (group.Id == _hostNameService.GetHostNamePartByClass("host-name-main").Id)
                userLocations = _userLocationRepository.Fetch(a => a.Id != newUserGroupId && a.UserPartRecord.Id == options.UserId && (a.UserGroupRecord.Id == group.Id || a.UserGroupRecord == null));
            else
                userLocations = _userLocationRepository.Fetch(a => a.Id != newUserGroupId && a.UserPartRecord.Id == options.UserId && a.UserGroupRecord.Id == group.Id);

            if (userLocations.Count() > 0)
            {
                UserLocationRecord partLocation = userLocations.FirstOrDefault();
                if (partLocation.IsSelling != options.IsSelling || partLocation.EndDateAgencing != options.EndDateAgencing || partLocation.IsLeasing != options.IsLeasing ||
                   partLocation.AreaAgencies != options.AreaAgencies)
                {
                    foreach (var entry in userLocations)
                    {
                        entry.AreaAgencies = options.AreaAgencies;
                        entry.IsLeasing = options.IsLeasing;
                        entry.IsSelling = options.IsSelling;
                        entry.EnableIsAgencies = true;
                        entry.EndDateAgencing = options.EndDateAgencing;

                        _userLocationRepository.Update(entry);
                    }
                }
            }

            string returnUrl = Url.Action("Activities", new { id }); // +"#locations";
            return Redirect(returnUrl);
        }

        [HttpPost, ActionName("Activities")]
        [FormValueRequired("submit.EditUserLocation")]
        public ActionResult EditUserLocation(int id, UserGroupIndexOptions options, PagerParameters pagerParameters,
            FormCollection input)
        {
            var group = Services.ContentManager.Get<UserGroupPart>(id);
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            bool currentUserIsGroupAdmin = group.GroupAdminUser.Id == currentUser.Id;
            bool enableGroupAddUserAgencies = Services.Authorizer.Authorize(Permissions.EditJointGroupUserAgencies) ||
                                             currentUserIsGroupAdmin;

            if (!enableGroupAddUserAgencies)
                return new HttpUnauthorizedResult();

            UserLocationRecord record = _userLocationRepository.Get(options.UserLocationEditId ?? 0);

            if (options.ProvinceId > 0)
            {
                if (record != null)
                {
                    record.UserPartRecord = _groupService.GetUser(options.UserId).Record;
                    record.LocationProvincePartRecord = _addressService.GetProvince(options.ProvinceId);
                    record.LocationDistrictPartRecord = _addressService.GetDistrict(options.DistrictId);
                    record.LocationWardPartRecord = _addressService.GetWard(options.WardId);
                    record.AreaAgencies = options.AreaAgencies;
                    record.EnableIsAgencies = true;
                    record.EndDateAgencing = options.EndDateAgencing;
                    record.IsLeasing = options.IsLeasing;
                    record.IsSelling = options.IsSelling;
                    record.UserGroupRecord = group.Record;
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

            //cập nhật thông tin môi giới.
            IEnumerable<UserLocationRecord> userLocations;
            if (group.Id == _hostNameService.GetHostNamePartByClass("host-name-main").Id)
                userLocations = _userLocationRepository.Fetch(a => a.Id != record.Id && a.UserPartRecord.Id == options.UserId && (a.UserGroupRecord.Id == group.Id || a.UserGroupRecord == null));
            else
                userLocations = _userLocationRepository.Fetch(a => a.Id != record.Id && a.UserPartRecord.Id == options.UserId && a.UserGroupRecord.Id == group.Id);

            if (userLocations.Count() > 0)
            {
                UserLocationRecord partLocation = userLocations.FirstOrDefault();
                if (partLocation.IsSelling != options.IsSelling || partLocation.EndDateAgencing != options.EndDateAgencing || partLocation.IsLeasing != options.IsLeasing ||
                   partLocation.AreaAgencies != options.AreaAgencies)
                {
                    foreach (var entry in userLocations)
                    {
                        entry.AreaAgencies = options.AreaAgencies;
                        entry.IsLeasing = options.IsLeasing;
                        entry.IsSelling = options.IsSelling;
                        entry.EnableIsAgencies = true;
                        entry.EndDateAgencing = options.EndDateAgencing;

                        _userLocationRepository.Update(entry);
                    }
                }
            }

            return RedirectToAction("Activities", new { id });
        }

        #region Setting

        public ActionResult Setting(int id)
        {
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = Services.ContentManager.Get<UserGroupPart>(id);

            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupSettings) && (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            var editModel = new UserGroupEditViewModel { Group = group };

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.Setting", Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(group);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Setting")]
        public ActionResult SettingPost(int id, FormCollection frmCollection)
        {
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = Services.ContentManager.Get<UserGroupPart>(id);

            if (!Services.Authorizer.Authorize(Permissions.EditJointGroupSettings) && (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            dynamic model = Services.ContentManager.UpdateEditor(group, this);

            var editModel = new UserGroupEditViewModel { Group = group };

            if (TryUpdateModel(editModel))
            {
                // Update
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.Setting", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("UserGroup <a href='{0}'>{1}</a> updated",
                Url.Action("Setting", new { group.Id }), group.Name));

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return RedirectToAction("Setting", new { group.Id });
        }

        #endregion

        #region Hostname

        public ActionResult HostName(int id)
        {
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = Services.ContentManager.Get<UserGroupPart>(id);

            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup) && (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            var editModel = new UserGroupEditViewModel { Group = group };

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.HostName", Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(group);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("HostName")]
        public ActionResult HostNamePost(int id, FormCollection frmCollection)
        {
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = Services.ContentManager.Get<UserGroupPart>(id);

            if (!Services.Authorizer.Authorize(Permissions.ManageJointGroup) && (group.GroupAdminUser.Id != currentUser.Id))
                return new HttpUnauthorizedResult();

            dynamic model = Services.ContentManager.UpdateEditor(group, this);

            var editModel = new UserGroupEditViewModel { Group = group };

            if (TryUpdateModel(editModel))
            {
                // Update
                var hostName = group.As<HostNamePart>();
                if (String.IsNullOrEmpty(hostName.DomainName))
                    hostName.DomainName = hostName.Name;
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UserGroup.HostName", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("UserGroup <a href='{0}'>{1}</a> updated",
                Url.Action("HostName", new { group.Id }), group.Name));

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return RedirectToAction("HostName", new { group.Id });
        }

        #endregion

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