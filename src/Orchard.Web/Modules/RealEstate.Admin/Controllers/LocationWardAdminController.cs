using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class LocationWardAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly ILocationWardService _wardService;

        public LocationWardAdminController(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            ILocationWardService wardService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _addressService = addressService;
            _groupService = groupService;
            _wardService = wardService;
            _signals = signals;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Index(LocationWardIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to list wards")))
                return new HttpUnauthorizedResult();

            //ImportData();

            IContentQuery<LocationWardPart, LocationWardPartRecord> wards = Services.ContentManager
                .Query<LocationWardPart, LocationWardPartRecord>();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int provinceId = _groupService.GetUserDefaultProvinceId(user);
            int districtId = _groupService.GetUserDefaultDistrictId(user);

            // default options
            if (options == null)
            {
                options = new LocationWardIndexOptions {ProvinceId = provinceId, DistrictId = districtId};
            }

            // default setting from Group setting
            if (!options.ProvinceId.HasValue) options.ProvinceId = provinceId;
            if (!options.DistrictId.HasValue) options.DistrictId = districtId;

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                options.IsRestrictedLocations = true;
                options.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                options.Districts = _groupService.GetUserEnableEditLocationDistricts(user, options.ProvinceId);

                int[] enableProvinceIds = options.Provinces.Select(a => a.Id).ToArray();
                wards = wards.Where(u => enableProvinceIds.Contains(u.Province.Id));

                int[] enableDistrictIds = options.Districts.Select(a => a.Id).ToArray();
                wards = wards.Where(u => enableDistrictIds.Contains(u.District.Id));
            }
            else
            {
                options.Provinces = _addressService.GetProvinces();
                options.Districts = _addressService.GetDistricts(options.ProvinceId);
            }

            switch (options.Filter)
            {
                case LocationWardsFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                wards = wards.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            if (options.ProvinceId > 0) wards = wards.Where(u => u.Province.Id == options.ProvinceId);
            if (options.DistrictId > 0) wards = wards.Where(u => u.District.Id == options.DistrictId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(wards.Count());

            switch (options.Order)
            {
                case LocationWardsOrder.SeqOrder:
                    wards = wards.OrderBy(u => u.District).OrderBy(u => u.SeqOrder);
                    break;
                case LocationWardsOrder.Name:
                    wards = wards.OrderBy(u => u.District).OrderBy(u => u.Name);
                    break;
            }

            List<LocationWardPart> results = wards
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationWardsIndexViewModel
            {
                LocationWards = results
                    .Select(x => new LocationWardEntry {LocationWard = x})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.ProvinceId", options.ProvinceId);
            routeData.Values.Add("Options.DistrictId", options.DistrictId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationWardsIndexViewModel
            {
                LocationWards = new List<LocationWardEntry>(),
                Options = new LocationWardIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationWardEntry> checkedEntries = viewModel.LocationWards.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationWardsBulkAction.None:
                    break;
                case LocationWardsBulkAction.Enable:
                    foreach (LocationWardEntry entry in checkedEntries)
                    {
                        Enable(entry.LocationWard.Id);
                    }
                    break;
                case LocationWardsBulkAction.Disable:
                    foreach (LocationWardEntry entry in checkedEntries)
                    {
                        Disable(entry.LocationWard.Id);
                    }
                    break;
                case LocationWardsBulkAction.Delete:
                    foreach (LocationWardEntry entry in checkedEntries)
                    {
                        Delete(entry.LocationWard.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var createModel = new LocationWardCreateViewModel
            {
                IsEnabled = true,
                ReturnUrl = returnUrl
            };

            // default setting from Group setting
            if (createModel.ProvinceId <= 0) createModel.ProvinceId = _groupService.GetUserDefaultProvinceId(user);
            if (createModel.DistrictId <= 0) createModel.DistrictId = _groupService.GetUserDefaultDistrictId(user);

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                createModel.IsRestrictedLocations = true;
                createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                createModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, createModel.ProvinceId);
            }
            else
            {
                createModel.Provinces = _addressService.GetProvinces();
                createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
            }

            var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationWard.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(ward);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(LocationWardCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_wardService.VerifyWardUnicity(createModel.Name, createModel.DistrictId))
                {
                    AddModelError("NotUniqueWardName", T("Ward with that name already exists."));
                }
            }

            var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");
            if (ModelState.IsValid)
            {
                LocationDistrictPartRecord district = _addressService.GetDistrict(createModel.DistrictId);

                ward.Name = createModel.Name;
                ward.ShortName = createModel.ShortName;
                ward.SeqOrder = createModel.SeqOrder;
                ward.IsEnabled = createModel.IsEnabled;
                ward.District = district;
                ward.Province = district.Province;

                Services.ContentManager.Create(ward);
            }

            dynamic model = Services.ContentManager.UpdateEditor(ward, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
                {
                    // User chỉ có quyền trong User's Location from User setting
                    createModel.IsRestrictedLocations = true;
                    createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                    createModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user,
                        createModel.ProvinceId);
                }
                else
                {
                    createModel.Provinces = _addressService.GetProvinces();
                    createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
                }

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationWard.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Wards_Changed");

            Services.Notifier.Information(T("<a href='{0}'>{1} - {2}</a> created.", Url.Action("Edit", new {ward.Id}),
                ward.Name, ward.District.Name));

            // Create New
            return RedirectToAction("Create", new {createModel.ReturnUrl});

            //return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var ward = Services.ContentManager.Get<LocationWardPart>(id);
            var editModel = new LocationWardEditViewModel
            {
                LocationWard = ward,
                ProvinceId = ward.District.Province.Id,
                DistrictId = ward.District.Id,
                ReturnUrl = returnUrl
            };

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                editModel.IsRestrictedLocations = true;
                editModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                editModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, editModel.ProvinceId);
            }
            else
            {
                editModel.Provinces = _addressService.GetProvinces();
                editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
            }

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationWard.Edit",
                Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(ward);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var ward = Services.ContentManager.Get<LocationWardPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(ward, this);

            var editModel = new LocationWardEditViewModel {LocationWard = ward};
            if (TryUpdateModel(editModel))
            {
                if (!_wardService.VerifyWardUnicity(id, editModel.Name, editModel.DistrictId))
                {
                    AddModelError("NotUniqueWardName", T("Ward with that name already exists."));
                }
                else
                {
                    LocationDistrictPartRecord district = _addressService.GetDistrict(editModel.DistrictId);
                    ward.District = district;
                    ward.Province = district.Province;
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
                {
                    // User chỉ có quyền trong User's Location from User setting
                    editModel.IsRestrictedLocations = true;
                    editModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                    editModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, editModel.ProvinceId);
                }
                else
                {
                    editModel.Provinces = _addressService.GetProvinces();
                    editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
                }

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationWard.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Wards_Changed");

            Services.Notifier.Information(T("<a href='{0}'>{1} - {2}</a> updated.", Url.Action("Edit", new {ward.Id}),
                ward.Name, ward.District.Name));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var ward = Services.ContentManager.Get<LocationWardPart>(id);

            if (ward != null)
            {
                ward.IsEnabled = true;

                _signals.Trigger("Wards_Changed");

                Services.Notifier.Information(T("Ward {0} updated", ward.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var ward = Services.ContentManager.Get<LocationWardPart>(id);

            if (ward != null)
            {
                ward.IsEnabled = false;

                _signals.Trigger("Wards_Changed");

                Services.Notifier.Information(T("Ward {0} updated", ward.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationWards, T("Not authorized to manage wards")))
                return new HttpUnauthorizedResult();

            var ward = Services.ContentManager.Get<LocationWardPart>(id);

            if (ward != null)
            {
                Services.ContentManager.Remove(ward.ContentItem);

                _signals.Trigger("Wards_Changed");

                Services.Notifier.Information(T("Ward {0} deleted", ward.Name));
            }

            return RedirectToAction("Index");
        }
    }
}