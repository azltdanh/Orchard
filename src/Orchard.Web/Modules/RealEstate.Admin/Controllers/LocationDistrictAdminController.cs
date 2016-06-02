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
    public class LocationDistrictAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly ILocationDistrictService _districtService;
        private readonly IUserGroupService _groupService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public LocationDistrictAdminController(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            ILocationDistrictService districtService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _addressService = addressService;
            _groupService = groupService;
            _districtService = districtService;
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

        public ActionResult Index(LocationDistrictIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to list districts")))
                return new HttpUnauthorizedResult();

            //ImportData();

            IContentQuery<LocationDistrictPart, LocationDistrictPartRecord> districts = Services.ContentManager
                .Query<LocationDistrictPart, LocationDistrictPartRecord>();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int provinceId = _groupService.GetUserDefaultProvinceId(user);

            // default options
            if (options == null)
            {
                options = new LocationDistrictIndexOptions {ProvinceId = provinceId};
            }

            // default setting from Group setting
            if (!options.ProvinceId.HasValue) options.ProvinceId = provinceId;

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                options.IsRestrictedLocations = true;
                options.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);

                int[] enableProvinceIds = options.Provinces.Select(a => a.Id).ToArray();
                districts = districts.Where(u => enableProvinceIds.Contains(u.Province.Id));
            }
            else
            {
                options.Provinces = _addressService.GetProvinces();
            }

            switch (options.Filter)
            {
                case LocationDistrictsFilter.All:
                    //districts = districts.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                districts = districts.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            if (options.ProvinceId > 0) districts = districts.Where(u => u.Province.Id == options.ProvinceId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(districts.Count());

            switch (options.Order)
            {
                case LocationDistrictsOrder.SeqOrder:
                    districts = districts.OrderBy(u => u.Province).OrderBy(u => u.SeqOrder);
                    break;
                case LocationDistrictsOrder.Name:
                    districts = districts.OrderBy(u => u.Province).OrderBy(u => u.Name);
                    break;
            }

            List<LocationDistrictPart> results = districts
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationDistrictsIndexViewModel
            {
                LocationDistricts = results
                    .Select(x => new LocationDistrictEntry {LocationDistrict = x})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);
            routeData.Values.Add("Options.ProvinceId", options.ProvinceId);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationDistrictsIndexViewModel
            {
                LocationDistricts = new List<LocationDistrictEntry>(),
                Options = new LocationDistrictIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationDistrictEntry> checkedEntries = viewModel.LocationDistricts.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationDistrictsBulkAction.None:
                    break;
                case LocationDistrictsBulkAction.Enable:
                    foreach (LocationDistrictEntry entry in checkedEntries)
                    {
                        Enable(entry.LocationDistrict.Id);
                    }
                    break;
                case LocationDistrictsBulkAction.Disable:
                    foreach (LocationDistrictEntry entry in checkedEntries)
                    {
                        Disable(entry.LocationDistrict.Id);
                    }
                    break;
                case LocationDistrictsBulkAction.Delete:
                    foreach (LocationDistrictEntry entry in checkedEntries)
                    {
                        Delete(entry.LocationDistrict.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var createModel = new LocationDistrictCreateViewModel
            {
                IsEnabled = true,
                ReturnUrl = returnUrl
            };

            // default setting from Group setting
            if (createModel.ProvinceId <= 0) createModel.ProvinceId = _groupService.GetUserDefaultProvinceId(user);

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                createModel.IsRestrictedLocations = true;
                createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
            }
            else
            {
                createModel.Provinces = _addressService.GetProvinces();
            }

            var district = Services.ContentManager.New<LocationDistrictPart>("LocationDistrict");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationDistrict.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(district);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(LocationDistrictCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_districtService.VerifyDistrictUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueDistrictName", T("District with that name already exists."));
                }
            }

            var district = Services.ContentManager.New<LocationDistrictPart>("LocationDistrict");
            if (ModelState.IsValid)
            {
                district.Name = createModel.Name;
                district.ShortName = createModel.ShortName;
                district.SeqOrder = createModel.SeqOrder;
                district.IsEnabled = createModel.IsEnabled;
                district.Province = _addressService.GetProvince(createModel.ProvinceId);

                Services.ContentManager.Create(district);
            }

            dynamic model = Services.ContentManager.UpdateEditor(district, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
                {
                    // User chỉ có quyền trong User's Location from User setting
                    createModel.IsRestrictedLocations = true;
                    createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                }
                else
                {
                    createModel.Provinces = _addressService.GetProvinces();
                }

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationDistrict.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Districts_Changed");

            Services.Notifier.Information(T("<a href='{0}'>{1} - {2}</a> created.",
                Url.Action("Edit", new {district.Id}), district.Name, district.Province.Name));

            return RedirectToAction("Create", new {createModel.ReturnUrl});
            //return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var district = Services.ContentManager.Get<LocationDistrictPart>(id);
            var editModel = new LocationDistrictEditViewModel
            {
                LocationDistrict = district,
                ProvinceId = district.Province.Id,
                ReturnUrl = returnUrl
            };

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                editModel.IsRestrictedLocations = true;
                editModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
            }
            else
            {
                editModel.Provinces = _addressService.GetProvinces();
            }

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationDistrict.Edit",
                Model: editModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(district);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var district = Services.ContentManager.Get<LocationDistrictPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(district, this);

            var editModel = new LocationDistrictEditViewModel {LocationDistrict = district};
            if (TryUpdateModel(editModel))
            {
                if (!_districtService.VerifyDistrictUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueDistrictName", T("District with that name already exists."));
                }
                else
                {
                    district.Province = _addressService.GetProvince(editModel.ProvinceId);
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
                }
                else
                {
                    editModel.Provinces = _addressService.GetProvinces();
                }

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationDistrict.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Districts_Changed");

            Services.Notifier.Information(T("<a href='{0}'>{1} - {2}</a> updated.",
                Url.Action("Edit", new {district.Id}), district.Name, district.Province.Name));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var district = Services.ContentManager.Get<LocationDistrictPart>(id);

            if (district != null)
            {
                district.IsEnabled = true;

                _signals.Trigger("Districts_Changed");

                Services.Notifier.Information(T("District {0} updated", district.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var district = Services.ContentManager.Get<LocationDistrictPart>(id);

            if (district != null)
            {
                district.IsEnabled = false;

                _signals.Trigger("Districts_Changed");

                Services.Notifier.Information(T("District {0} updated", district.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationDistricts,
                    T("Not authorized to manage districts")))
                return new HttpUnauthorizedResult();

            var district = Services.ContentManager.Get<LocationDistrictPart>(id);

            if (district != null)
            {
                Services.ContentManager.Remove(district.ContentItem);

                _signals.Trigger("Districts_Changed");

                Services.Notifier.Information(T("District {0} deleted", district.Name));
            }

            return RedirectToAction("Index");
        }
    }
}