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
    public class StreetRelationAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly ISiteService _siteService;
        private readonly IStreetRelationService _streetRelationService;

        public StreetRelationAdminController(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            IStreetRelationService streetRelationService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _addressService = addressService;
            _groupService = groupService;
            _streetRelationService = streetRelationService;
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

        public ActionResult Index(StreetRelationIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to list streetRelations")))
                return new HttpUnauthorizedResult();

            //ImportData();

            IContentQuery<StreetRelationPart, StreetRelationPartRecord> streetRelations = Services.ContentManager
                .Query<StreetRelationPart, StreetRelationPartRecord>();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int provinceId = _groupService.GetUserDefaultProvinceId(user);
            int districtId = _groupService.GetUserDefaultDistrictId(user);

            // default options
            if (options == null)
            {
                options = new StreetRelationIndexOptions
                {
                    ProvinceId = provinceId,
                    DistrictId = districtId,
                    WardId = 0,
                    StreetId = 0,
                    RelatedProvinceId = provinceId,
                    RelatedDistrictId = districtId,
                    RelatedWardId = 0,
                    RelatedStreetId = 0
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
                streetRelations = streetRelations.Where(u => enableProvinceIds.Contains(u.Province.Id));

                int[] enableDistrictIds = options.Districts.Select(a => a.Id).ToArray();
                streetRelations = streetRelations.Where(u => enableDistrictIds.Contains(u.District.Id));


                options.RelatedProvinces = _groupService.GetUserEnableEditLocationProvinces(user);
                options.RelatedDistricts = _groupService.GetUserEnableEditLocationDistricts(user,
                    options.RelatedProvinceId);

                int[] enableRelatedProvinceIds = options.RelatedProvinces.Select(a => a.Id).ToArray();
                streetRelations = streetRelations.Where(u => enableRelatedProvinceIds.Contains(u.RelatedProvince.Id));

                int[] enableRelatedDistrictIds = options.RelatedDistricts.Select(a => a.Id).ToArray();
                streetRelations = streetRelations.Where(u => enableRelatedDistrictIds.Contains(u.RelatedDistrict.Id));
            }
            else
            {
                options.Provinces = _addressService.GetProvinces();
                options.Districts = _addressService.GetDistricts(options.ProvinceId);

                options.RelatedProvinces = _addressService.GetProvinces();
                options.RelatedDistricts = _addressService.GetDistricts(options.RelatedProvinceId);
            }

            options.Wards = _addressService.GetWards(options.DistrictId);
            options.Streets = _addressService.GetAllStreets(options.DistrictId);

            options.RelatedWards = _addressService.GetWards(options.RelatedDistrictId);
            options.RelatedStreets = _addressService.GetAllStreets(options.RelatedDistrictId);

            switch (options.Filter)
            {
                case StreetRelationsFilter.All:
                    //streetRelations = streetRelations.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (options.ProvinceId > 0)
                streetRelations = streetRelations.Where(u => u.Province.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                streetRelations = streetRelations.Where(u => u.District.Id == options.DistrictId);
            if (options.WardId > 0) streetRelations = streetRelations.Where(u => u.Ward.Id == options.WardId);
            if (options.StreetId > 0) streetRelations = streetRelations.Where(u => u.Street.Id == options.StreetId);

            if (options.RelatedProvinceId > 0)
                streetRelations = streetRelations.Where(u => u.RelatedProvince.Id == options.RelatedProvinceId);
            if (options.RelatedDistrictId > 0)
                streetRelations = streetRelations.Where(u => u.RelatedDistrict.Id == options.RelatedDistrictId);
            if (options.RelatedWardId > 0)
                streetRelations = streetRelations.Where(u => u.RelatedWard.Id == options.RelatedWardId);
            if (options.RelatedStreetId > 0)
                streetRelations = streetRelations.Where(u => u.RelatedStreet.Id == options.RelatedStreetId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(streetRelations.Count());

            switch (options.Order)
            {
                case StreetRelationsOrder.Province:
                    streetRelations = streetRelations.OrderBy(u => u.Province);
                    break;
                case StreetRelationsOrder.District:
                    streetRelations = streetRelations.OrderBy(u => u.District);
                    break;
                case StreetRelationsOrder.Ward:
                    streetRelations = streetRelations.OrderBy(u => u.Ward);
                    break;
                case StreetRelationsOrder.Street:
                    streetRelations = streetRelations.OrderBy(u => u.Street);
                    break;
                case StreetRelationsOrder.RelatedProvince:
                    streetRelations = streetRelations.OrderBy(u => u.RelatedProvince);
                    break;
                case StreetRelationsOrder.RelatedDistrict:
                    streetRelations = streetRelations.OrderBy(u => u.RelatedDistrict);
                    break;
                case StreetRelationsOrder.RelatedWard:
                    streetRelations = streetRelations.OrderBy(u => u.RelatedWard);
                    break;
                case StreetRelationsOrder.RelatedStreet:
                    streetRelations = streetRelations.OrderBy(u => u.RelatedStreet);
                    break;
            }

            List<StreetRelationPart> results = streetRelations
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new StreetRelationsIndexViewModel
            {
                StreetRelations = results
                    .Select(x => new StreetRelationEntry {StreetRelation = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.ProvinceId", options.ProvinceId);
            routeData.Values.Add("Options.DistrictId", options.DistrictId);
            routeData.Values.Add("Options.WardId", options.WardId);
            routeData.Values.Add("Options.StreetId", options.StreetId);
            routeData.Values.Add("Options.RelatedProvinceId", options.RelatedProvinceId);
            routeData.Values.Add("Options.RelatedDistrictId", options.RelatedDistrictId);
            routeData.Values.Add("Options.RelatedWardId", options.RelatedWardId);
            routeData.Values.Add("Options.RelatedStreetId", options.RelatedStreetId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to manage streetRelations")))
                return new HttpUnauthorizedResult();

            var viewModel = new StreetRelationsIndexViewModel
            {
                StreetRelations = new List<StreetRelationEntry>(),
                Options = new StreetRelationIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<StreetRelationEntry> checkedEntries = viewModel.StreetRelations.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case StreetRelationsBulkAction.None:
                    break;
                case StreetRelationsBulkAction.Delete:
                    foreach (StreetRelationEntry entry in checkedEntries)
                    {
                        Delete(entry.StreetRelation.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to manage streetRelations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var createModel = new StreetRelationCreateViewModel
            {
                ReturnUrl = returnUrl,
                CoefficientAlleys =
                    Services.ContentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                        .List()
                        .OrderBy(a => a.StreetUnitPrice)
                        .Select(a => a.Record)
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

            createModel.Wards = _addressService.GetWards(createModel.DistrictId);
            createModel.Streets = _addressService.GetStreets(createModel.DistrictId);

            createModel.RelatedWards = _addressService.GetWards(createModel.RelatedDistrictId);
            createModel.RelatedStreets = _addressService.GetStreets(createModel.RelatedDistrictId);

            var streetRelation = Services.ContentManager.New<StreetRelationPart>("StreetRelation");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/StreetRelation.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(streetRelation);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(StreetRelationCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to manage streetRelations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (
                !_streetRelationService.VerifyStreetUnicity(createModel.StreetId, createModel.WardId,
                    createModel.DistrictId))
            {
                AddModelError("NotUniqueStreetName", T("Quan hệ đã có trong dữ liệu."));
            }
            if (
                !_streetRelationService.IsValidRelation(createModel.StreetId, createModel.WardId, createModel.DistrictId,
                    createModel.RelatedStreetId, createModel.RelatedWardId, createModel.RelatedDistrictId))
            {
                AddModelError("NotUniqueStreetName", T("Quan hệ không hợp lệ."));
            }
            var streetRelation = Services.ContentManager.New<StreetRelationPart>("StreetRelation");
            if (ModelState.IsValid)
            {
                streetRelation.Province = _addressService.GetProvince(createModel.ProvinceId);
                streetRelation.District = _addressService.GetDistrict(createModel.DistrictId);
                streetRelation.Ward = _addressService.GetWard(createModel.WardId);
                streetRelation.Street = _addressService.GetStreet(createModel.StreetId);
                streetRelation.StreetWidth = createModel.StreetWidth;

                streetRelation.RelatedValue = createModel.RelatedValue ?? 100;
                streetRelation.RelatedAlleyValue = createModel.RelatedAlleyValue ?? createModel.RelatedValue;

                streetRelation.RelatedProvince = _addressService.GetProvince(createModel.RelatedProvinceId);
                streetRelation.RelatedDistrict = _addressService.GetDistrict(createModel.RelatedDistrictId);
                streetRelation.RelatedWard = _addressService.GetWard(createModel.RelatedWardId);
                streetRelation.RelatedStreet = _addressService.GetStreet(createModel.RelatedStreetId);
                streetRelation.RelatedStreetWidth = createModel.RelatedStreetWidth;

                streetRelation.CoefficientAlley1Max = createModel.CoefficientAlley1Max;
                streetRelation.CoefficientAlley1Min = createModel.CoefficientAlley1Min;
                streetRelation.CoefficientAlleyEqual = createModel.CoefficientAlleyEqual;
                streetRelation.CoefficientAlleyMax = createModel.CoefficientAlleyMax;
                streetRelation.CoefficientAlleyMin = createModel.CoefficientAlleyMin;

                Services.ContentManager.Create(streetRelation);
            }

            dynamic model = Services.ContentManager.UpdateEditor(streetRelation, this);

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

                createModel.Wards = _addressService.GetWards(createModel.DistrictId);
                createModel.Streets = _addressService.GetStreets(createModel.DistrictId);

                createModel.RelatedWards = _addressService.GetWards(createModel.RelatedDistrictId);
                createModel.RelatedStreets = _addressService.GetStreets(createModel.RelatedDistrictId);

                createModel.CoefficientAlleys =
                    Services.ContentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                        .List()
                        .OrderBy(a => a.StreetUnitPrice)
                        .Select(a => a.Record);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/StreetRelation.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("Street Relation <a href='{0}'>{1}, {2}, {3}</a> created.",
                Url.Action("Edit", new {streetRelation.Id}), streetRelation.Street.Name, streetRelation.Ward.Name,
                streetRelation.District.Name));
            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.RedirectLocal(createModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to manage streetRelations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var streetRelation = Services.ContentManager.Get<StreetRelationPart>(id);
            var editModel = new StreetRelationEditViewModel
            {
                ReturnUrl = returnUrl,
                StreetRelation = streetRelation,
                ProvinceId = streetRelation.Province.Id,
                DistrictId = streetRelation.District.Id,
                Wards = _addressService.GetWards(streetRelation.District.Id),
                WardId = streetRelation.Ward.Id,
                Streets = _addressService.GetAllStreets(streetRelation.District.Id),
                StreetId = streetRelation.Street.Id,
                RelatedProvinceId = streetRelation.RelatedProvince.Id,
                RelatedDistrictId = streetRelation.RelatedDistrict.Id,
                RelatedWards = _addressService.GetWards(streetRelation.RelatedDistrict.Id),
                RelatedWardId = streetRelation.RelatedWard.Id,
                RelatedStreets = _addressService.GetAllStreets(streetRelation.RelatedDistrict.Id),
                RelatedStreetId = streetRelation.RelatedStreet.Id,
                CoefficientAlleys =
                    Services.ContentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                        .List()
                        .OrderBy(a => a.StreetUnitPrice)
                        .Select(a => a.Record)
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
                TemplateName: "Parts/StreetRelation.Edit",
                Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(streetRelation);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to manage streetRelations")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var streetRelation = Services.ContentManager.Get<StreetRelationPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(streetRelation, this);

            var editModel = new StreetRelationEditViewModel {StreetRelation = streetRelation};
            if (TryUpdateModel(editModel))
            {
                if (
                    !_streetRelationService.VerifyStreetUnicity(id, editModel.StreetId, editModel.WardId,
                        editModel.DistrictId))
                {
                    AddModelError("NotUniqueStreetName", T("Quan hệ đã có trong dữ liệu."));
                }
                else if (
                    !_streetRelationService.IsValidRelation(editModel.StreetId, editModel.WardId,
                        editModel.DistrictId, editModel.RelatedStreetId, editModel.RelatedWardId,
                        editModel.RelatedDistrictId))
                {
                    AddModelError("NotUniqueStreetName", T("Quan hệ không hợp lệ."));
                }
                else
                {
                    streetRelation.Province = _addressService.GetProvince(editModel.ProvinceId);
                    streetRelation.District = _addressService.GetDistrict(editModel.DistrictId);
                    streetRelation.Ward = _addressService.GetWard(editModel.WardId);
                    streetRelation.Street = _addressService.GetStreet(editModel.StreetId);

                    streetRelation.RelatedValue = editModel.RelatedValue;
                    streetRelation.RelatedAlleyValue = editModel.RelatedAlleyValue ?? editModel.RelatedValue;

                    streetRelation.RelatedProvince = _addressService.GetProvince(editModel.RelatedProvinceId);
                    streetRelation.RelatedDistrict = _addressService.GetDistrict(editModel.RelatedDistrictId);
                    streetRelation.RelatedWard = _addressService.GetWard(editModel.RelatedWardId);
                    streetRelation.RelatedStreet = _addressService.GetStreet(editModel.RelatedStreetId);
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

                editModel.Wards = _addressService.GetWards(editModel.DistrictId);
                editModel.Streets = _addressService.GetStreets(editModel.DistrictId);

                editModel.RelatedWards = _addressService.GetWards(editModel.RelatedDistrictId);
                editModel.RelatedStreets = _addressService.GetStreets(editModel.RelatedDistrictId);

                editModel.CoefficientAlleys =
                    Services.ContentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                        .List()
                        .OrderBy(a => a.StreetUnitPrice)
                        .Select(a => a.Record);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/StreetRelation.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("Street Relation <a href='{0}'>{1}, {2}, {3}</a> updated.",
                Url.Action("Edit", new {streetRelation.Id}), streetRelation.Street.Name, streetRelation.Ward.Name,
                streetRelation.District.Name));
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
                !Services.Authorizer.Authorize(Permissions.ManageStreetRelations,
                    T("Not authorized to manage streetRelations")))
                return new HttpUnauthorizedResult();

            var streetRelation = Services.ContentManager.Get<StreetRelationPart>(id);

            if (streetRelation != null)
            {
                Services.ContentManager.Remove(streetRelation.ContentItem);
                Services.Notifier.Information(T("Street {0} deleted", streetRelation.Id));
            }

            return RedirectToAction("Index");
        }
    }
}