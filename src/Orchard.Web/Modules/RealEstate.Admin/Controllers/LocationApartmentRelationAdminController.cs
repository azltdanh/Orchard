using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
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
    public class LocationApartmentRelationAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly ILocationApartmentRelationService _apartmentRelationService;
        private readonly IUserGroupService _groupService;
        private readonly ISiteService _siteService;

        public LocationApartmentRelationAdminController(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            ILocationApartmentRelationService apartmentRelationService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _addressService = addressService;
            _groupService = groupService;
            _apartmentRelationService = apartmentRelationService;
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

        public ActionResult Index(LocationApartmentRelationIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to list Apartment Relations")))
                return new HttpUnauthorizedResult();

            //ImportData();

            IContentQuery<LocationApartmentRelationPart, LocationApartmentRelationPartRecord> apartmentRelations =
                Services.ContentManager
                    .Query<LocationApartmentRelationPart, LocationApartmentRelationPartRecord>();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int provinceId = _groupService.GetUserDefaultProvinceId(user);
            int districtId = _groupService.GetUserDefaultDistrictId(user);

            // default options
            if (options == null)
            {
                options = new LocationApartmentRelationIndexOptions
                {
                    ProvinceId = provinceId,
                    DistrictId = districtId,
                    ApartmentId = 0,
                    RelatedProvinceId = provinceId,
                    RelatedDistrictId = districtId,
                    RelatedApartmentId = 0
                };
            }

            // default setting from Group setting

            if (!options.ProvinceId.HasValue) options.ProvinceId = provinceId;
            if (!options.DistrictId.HasValue) options.DistrictId = districtId;

            if (!options.RelatedProvinceId.HasValue) options.RelatedProvinceId = provinceId;
            if (!options.RelatedDistrictId.HasValue) options.RelatedDistrictId = districtId;

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                options.IsRestrictedLocations = true;

                options.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                options.Districts = _groupService.GetUserEnableEditLocationDistricts(user, options.ProvinceId);

                int[] enableProvinceIds = options.Provinces.Select(a => a.Id).ToArray();
                apartmentRelations = apartmentRelations.Where(u => enableProvinceIds.Contains(u.Province.Id));

                int[] enableDistrictIds = options.Districts.Select(a => a.Id).ToArray();
                apartmentRelations = apartmentRelations.Where(u => enableDistrictIds.Contains(u.District.Id));


                options.RelatedProvinces = _groupService.GetUserEnableEditLocationProvinces(user);
                options.RelatedDistricts = _groupService.GetUserEnableEditLocationDistricts(user,
                    options.RelatedProvinceId);

                int[] enableRelatedProvinceIds = options.RelatedProvinces.Select(a => a.Id).ToArray();
                apartmentRelations =
                    apartmentRelations.Where(u => enableRelatedProvinceIds.Contains(u.RelatedProvince.Id));

                int[] enableRelatedDistrictIds = options.RelatedDistricts.Select(a => a.Id).ToArray();
                apartmentRelations =
                    apartmentRelations.Where(u => enableRelatedDistrictIds.Contains(u.RelatedDistrict.Id));
            }
            else
            {
                options.Provinces = _addressService.GetProvinces();
                options.Districts = _addressService.GetDistricts(options.ProvinceId);

                options.RelatedProvinces = _addressService.GetProvinces();
                options.RelatedDistricts = _addressService.GetDistricts(options.RelatedProvinceId);
            }

            options.Apartments = _addressService.GetApartments(options.DistrictId);
            options.RelatedApartments = _addressService.GetApartments(options.RelatedDistrictId);

            switch (options.Filter)
            {
                case LocationApartmentRelationsFilter.All:
                    break;
            }

            if (options.ProvinceId > 0)
                apartmentRelations = apartmentRelations.Where(u => u.Province.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                apartmentRelations = apartmentRelations.Where(u => u.District.Id == options.DistrictId);
            if (options.ApartmentId > 0)
                apartmentRelations = apartmentRelations.Where(u => u.LocationApartment.Id == options.ApartmentId);

            if (options.RelatedProvinceId > 0)
                apartmentRelations = apartmentRelations.Where(u => u.RelatedProvince.Id == options.RelatedProvinceId);
            if (options.RelatedDistrictId > 0)
                apartmentRelations = apartmentRelations.Where(u => u.RelatedDistrict.Id == options.RelatedDistrictId);
            if (options.RelatedApartmentId > 0)
                apartmentRelations =
                    apartmentRelations.Where(u => u.RelatedLocationApartment.Id == options.RelatedApartmentId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(apartmentRelations.Count());

            switch (options.Order)
            {
                case LocationApartmentRelationsOrder.Province:
                    apartmentRelations = apartmentRelations.OrderBy(u => u.Province);
                    break;
                case LocationApartmentRelationsOrder.District:
                    apartmentRelations = apartmentRelations.OrderBy(u => u.District);
                    break;
                case LocationApartmentRelationsOrder.LocationApartment:
                    apartmentRelations = apartmentRelations.OrderBy(u => u.LocationApartment);
                    break;
                case LocationApartmentRelationsOrder.RelatedProvince:
                    apartmentRelations = apartmentRelations.OrderBy(u => u.RelatedProvince);
                    break;
                case LocationApartmentRelationsOrder.RelatedDistrict:
                    apartmentRelations = apartmentRelations.OrderBy(u => u.RelatedDistrict);
                    break;
                case LocationApartmentRelationsOrder.RelatedLocationApartment:
                    apartmentRelations = apartmentRelations.OrderBy(u => u.RelatedLocationApartment);
                    break;
            }

            List<LocationApartmentRelationPart> results = apartmentRelations
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationApartmentRelationsIndexViewModel
            {
                LocationApartmentRelations = results
                    .Select(x => new LocationApartmentRelationEntry {LocationApartmentRelation = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.ProvinceId", options.ProvinceId);
            routeData.Values.Add("Options.DistrictId", options.DistrictId);
            routeData.Values.Add("Options.ApartmentId", options.ApartmentId);
            routeData.Values.Add("Options.RelatedProvinceId", options.RelatedProvinceId);
            routeData.Values.Add("Options.RelatedDistrictId", options.RelatedDistrictId);
            routeData.Values.Add("Options.RelatedApartmentId", options.RelatedApartmentId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to manage Apartment Relations")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationApartmentRelationsIndexViewModel
            {
                LocationApartmentRelations = new List<LocationApartmentRelationEntry>(),
                Options = new LocationApartmentRelationIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationApartmentRelationEntry> checkedEntries =
                viewModel.LocationApartmentRelations.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationApartmentRelationsBulkAction.None:
                    break;
                case LocationApartmentRelationsBulkAction.Delete:
                    foreach (LocationApartmentRelationEntry entry in checkedEntries)
                    {
                        Delete(entry.LocationApartmentRelation.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to manage Apartment Relations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var createModel = new LocationApartmentRelationCreateViewModel
            {
                ReturnUrl = returnUrl,
            };

            // default setting from Group setting

            if (createModel.ProvinceId <= 0) createModel.ProvinceId = _groupService.GetUserDefaultProvinceId(user);
            if (createModel.DistrictId <= 0) createModel.DistrictId = _groupService.GetUserDefaultDistrictId(user);

            if (createModel.RelatedProvinceId <= 0)
                createModel.RelatedProvinceId = _groupService.GetUserDefaultProvinceId(user);
            if (createModel.RelatedDistrictId <= 0)
                createModel.RelatedDistrictId = _groupService.GetUserDefaultDistrictId(user);

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                createModel.IsRestrictedLocations = true;

                createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                createModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, createModel.ProvinceId);

                createModel.RelatedProvinces = _groupService.GetUserEnableEditLocationProvinces(user);
                createModel.RelatedDistricts = _groupService.GetUserEnableEditLocationDistricts(user,
                    createModel.RelatedProvinceId);
            }
            else
            {
                createModel.Provinces = _addressService.GetProvinces();
                createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);

                createModel.RelatedProvinces = _addressService.GetProvinces();
                createModel.RelatedDistricts = _addressService.GetDistricts(createModel.RelatedProvinceId);
            }

            createModel.Apartments = _addressService.GetApartments(createModel.DistrictId);
            createModel.RelatedApartments = _addressService.GetApartments(createModel.RelatedDistrictId);

            var apartmentRelation =
                Services.ContentManager.New<LocationApartmentRelationPart>("LocationApartmentRelation");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationApartmentRelation.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartmentRelation);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(LocationApartmentRelationCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to manage Apartment Relations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (
                !_apartmentRelationService.VerifyLocationApartmentUnicity(createModel.ApartmentId,
                    createModel.DistrictId))
            {
                AddModelError("NotUniqueApartmentName", T("Quan hệ đã có trong dữ liệu."));
            }
            if (
                !_apartmentRelationService.IsValidRelation(createModel.ApartmentId, createModel.DistrictId,
                    createModel.RelatedApartmentId, createModel.RelatedDistrictId))
            {
                AddModelError("NotUniqueApartmentName", T("Quan hệ không hợp lệ."));
            }
            var apartmentRelation =
                Services.ContentManager.New<LocationApartmentRelationPart>("LocationApartmentRelation");
            if (ModelState.IsValid)
            {
                apartmentRelation.Province = _addressService.GetProvince(createModel.ProvinceId);
                apartmentRelation.District = _addressService.GetDistrict(createModel.DistrictId);
                apartmentRelation.LocationApartment = _addressService.GetApartment(createModel.ApartmentId);

                apartmentRelation.RelatedValue = createModel.RelatedValue ?? 100;

                apartmentRelation.RelatedProvince = _addressService.GetProvince(createModel.RelatedProvinceId);
                apartmentRelation.RelatedDistrict = _addressService.GetDistrict(createModel.RelatedDistrictId);
                apartmentRelation.RelatedLocationApartment = _addressService.GetApartment(createModel.RelatedApartmentId);

                Services.ContentManager.Create(apartmentRelation);
            }

            dynamic model = Services.ContentManager.UpdateEditor(apartmentRelation, this);

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

                    createModel.RelatedProvinces = _groupService.GetUserEnableEditLocationProvinces(user);
                    createModel.RelatedDistricts = _groupService.GetUserEnableEditLocationDistricts(user,
                        createModel.RelatedProvinceId);
                }
                else
                {
                    createModel.Provinces = _addressService.GetProvinces();
                    createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);

                    createModel.RelatedProvinces = _addressService.GetProvinces();
                    createModel.RelatedDistricts = _addressService.GetDistricts(createModel.RelatedProvinceId);
                }

                createModel.Apartments = _addressService.GetApartments(createModel.DistrictId);
                createModel.RelatedApartments = _addressService.GetApartments(createModel.RelatedDistrictId);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationApartmentRelation.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("Apartment Relation <a href='{0}'>{1}, {2}</a> created.",
                Url.Action("Edit", new {apartmentRelation.Id}), apartmentRelation.LocationApartment.Name,
                apartmentRelation.District.Name));
            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.RedirectLocal(createModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to manage Apartment Relations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var apartmentRelation = Services.ContentManager.Get<LocationApartmentRelationPart>(id);
            var editModel = new LocationApartmentRelationEditViewModel
            {
                ReturnUrl = returnUrl,
                LocationApartmentRelation = apartmentRelation,
                ProvinceId = apartmentRelation.Province.Id,
                DistrictId = apartmentRelation.District.Id,
                Apartments = _addressService.GetApartments(apartmentRelation.District.Id),
                ApartmentId = apartmentRelation.LocationApartment.Id,
                RelatedProvinceId = apartmentRelation.RelatedProvince.Id,
                RelatedDistrictId = apartmentRelation.RelatedDistrict.Id,
                RelatedApartments = _addressService.GetApartments(apartmentRelation.RelatedDistrict.Id),
                RelatedApartmentId = apartmentRelation.RelatedLocationApartment.Id,
            };

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                editModel.IsRestrictedLocations = true;

                editModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                editModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, editModel.ProvinceId);

                editModel.RelatedProvinces = _groupService.GetUserEnableEditLocationProvinces(user);
                editModel.RelatedDistricts = _groupService.GetUserEnableEditLocationDistricts(user,
                    editModel.RelatedProvinceId);
            }
            else
            {
                editModel.Provinces = _addressService.GetProvinces();
                editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);

                editModel.RelatedProvinces = _addressService.GetProvinces();
                editModel.RelatedDistricts = _addressService.GetDistricts(editModel.RelatedProvinceId);
            }

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationApartmentRelation.Edit",
                Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartmentRelation);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to manage Apartment Relations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var apartmentRelation = Services.ContentManager.Get<LocationApartmentRelationPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(apartmentRelation, this);

            var editModel = new LocationApartmentRelationEditViewModel {LocationApartmentRelation = apartmentRelation};
            if (TryUpdateModel(editModel))
            {
                if (
                    !_apartmentRelationService.VerifyLocationApartmentUnicity(id, editModel.ApartmentId,
                        editModel.DistrictId))
                {
                    AddModelError("NotUniqueApartmentName", T("Quan hệ đã có trong dữ liệu."));
                }
                else if (
                    !_apartmentRelationService.IsValidRelation(editModel.ApartmentId, editModel.DistrictId,
                        editModel.RelatedApartmentId, editModel.RelatedDistrictId))
                {
                    AddModelError("NotUniqueApartmentName", T("Quan hệ không hợp lệ."));
                }
                else
                {
                    apartmentRelation.Province = _addressService.GetProvince(editModel.ProvinceId);
                    apartmentRelation.District = _addressService.GetDistrict(editModel.DistrictId);
                    apartmentRelation.LocationApartment = _addressService.GetApartment(editModel.ApartmentId);

                    apartmentRelation.RelatedValue = editModel.RelatedValue;

                    apartmentRelation.RelatedProvince = _addressService.GetProvince(editModel.RelatedProvinceId);
                    apartmentRelation.RelatedDistrict = _addressService.GetDistrict(editModel.RelatedDistrictId);
                    apartmentRelation.RelatedLocationApartment =
                        _addressService.GetApartment(editModel.RelatedApartmentId);
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

                    editModel.RelatedProvinces = _groupService.GetUserEnableEditLocationProvinces(user);
                    editModel.RelatedDistricts = _groupService.GetUserEnableEditLocationDistricts(user,
                        editModel.RelatedProvinceId);
                }
                else
                {
                    editModel.Provinces = _addressService.GetProvinces();
                    editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);

                    editModel.RelatedProvinces = _addressService.GetProvinces();
                    editModel.RelatedDistricts = _addressService.GetDistricts(editModel.RelatedProvinceId);
                }

                editModel.Apartments = _addressService.GetApartments(editModel.DistrictId);
                editModel.RelatedApartments = _addressService.GetApartments(editModel.RelatedDistrictId);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationApartmentRelation.Edit",
                    Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("Apartment Relation <a href='{0}'>{1}, {2}</a> updated.",
                Url.Action("Edit", new {apartmentRelation.Id}), apartmentRelation.LocationApartment.Name,
                apartmentRelation.District.Name));
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartmentRelations,
                    T("Not authorized to manage Apartment Relations")))
                return new HttpUnauthorizedResult();

            var apartmentRelation = Services.ContentManager.Get<LocationApartmentRelationPart>(id);

            if (apartmentRelation != null)
            {
                Services.ContentManager.Remove(apartmentRelation.ContentItem);
                Services.Notifier.Information(T("Apartment {0} deleted", apartmentRelation.Id));
            }

            return RedirectToAction("Index");
        }
    }
}