using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class PlanningMapAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IRepository<PlanningMapRecord> _planningMapRepository;
        private readonly ISiteService _siteService;

        public PlanningMapAdminController(
            ISiteService siteService,
            IAddressService addressService,
            IRepository<PlanningMapRecord> planningMapRepository,
            IShapeFactory shapeFactory,
            IOrchardServices services)
        {
            _siteService = siteService;
            _addressService = addressService;
            _planningMapRepository = planningMapRepository;

            T = NullLocalizer.Instance;
            Services = services;
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

        public ActionResult Index(PlanningMapIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManagePlanningMaps, T("Not authorized to list PlanningMaps")))
                return new HttpUnauthorizedResult();

            // default options
            if (options == null)
            {
                options = new PlanningMapIndexOptions {ProvinceId = 0, DistrictId = 0, WardId = 0};
            }

            options.Provinces = _addressService.GetProvinces();
            if (options.ProvinceId == null || options.ProvinceId == 0)
                options.ProvinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            options.Wards = _addressService.GetWards(options.DistrictId ?? 0);

            IQueryable<PlanningMapRecord> planningMaps = _planningMapRepository.Table;

            if (options.ProvinceId > 0)
                planningMaps = planningMaps.Where(u => u.LocationProvincePartRecord.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                planningMaps = planningMaps.Where(u => u.LocationDistrictPartRecord.Id == options.DistrictId);
            if (options.WardId > 0)
                planningMaps = planningMaps.Where(u => u.LocationWardPartRecord.Id == options.WardId);

            // Order
            planningMaps = planningMaps.OrderBy(a => a.LocationProvincePartRecord.SeqOrder)
                .ThenBy(a => a.LocationDistrictPartRecord.SeqOrder)
                .ThenBy(a => a.LocationWardPartRecord.SeqOrder);

            // Pager
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(planningMaps.Count());

            List<PlanningMapRecord> results = planningMaps.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            var model = new PlanningMapsIndexViewModel
            {
                PlanningMaps = results.Select(a => new PlanningMapEntry {PlanningMap = a}).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            var viewModel = new PlanningMapsIndexViewModel
            {
                PlanningMaps = new List<PlanningMapEntry>(),
                Options = new PlanningMapIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PlanningMapEntry> checkedEntries = viewModel.PlanningMaps.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PlanningMapBulkAction.None:
                    break;

                case PlanningMapBulkAction.Delete:
                    foreach (PlanningMapEntry entry in checkedEntries)
                    {
                        Delete(entry.PlanningMap.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/PlanningMap.Create",
                Model: new PlanningMapCreateViewModel
                {
                    Provinces = _addressService.GetProvinces(),
                    Districts = _addressService.GetDistricts(-1),
                    Wards = _addressService.GetWards(-1),
                    IsEnabled = true,
                    ReturnUrl = returnUrl
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(ward);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PlanningMapCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                // VerifyUnicity 
                if (_planningMapRepository.Fetch(a => a.LocationProvincePartRecord.Id == createModel.ProvinceId &&
                                                      a.LocationDistrictPartRecord.Id == createModel.DistrictId &&
                                                      a.LocationWardPartRecord.Id == createModel.WardId).Any())
                {
                    AddModelError("WardId",
                        T("PlanningMap {0} - {1} already exists.", _addressService.GetWard(createModel.WardId).Name,
                            _addressService.GetDistrict(createModel.DistrictId).ShortName));
                }
                else
                {
                    LocationWardPartRecord ward = _addressService.GetWard(createModel.WardId);

                    var record = new PlanningMapRecord
                    {
                        LocationProvincePartRecord = ward.Province,
                        LocationDistrictPartRecord = ward.District,
                        LocationWardPartRecord = ward,
                        ImagesPath = createModel.ImagesPath,
                        Width = createModel.Width ?? 0,
                        Height = createModel.Height ?? 0,
                        MinZoom = createModel.MinZoom ?? 0,
                        MaxZoom = createModel.MaxZoom ?? 0,
                        Ratio = createModel.Ratio ?? 0,
                        IsEnabled = createModel.IsEnabled
                    };

                    _planningMapRepository.Create(record);

                    Services.Notifier.Information(T("PlanningMap <a href='{0}'>{1} - {2}</a> created.",
                        Url.Action("Edit", new {record.Id}), ward.Name, ward.District.Name));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                createModel.Provinces = _addressService.GetProvinces();
                createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
                createModel.Wards = _addressService.GetWards(createModel.DistrictId);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PlanningMap.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";

                var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");
                dynamic model = Services.ContentManager.UpdateEditor(ward, this);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.RedirectLocal(createModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");
            PlanningMapRecord record = _planningMapRepository.Get(id);
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/PlanningMap.Edit",
                Model: new PlanningMapEditViewModel
                {
                    PlanningMap = record,
                    Provinces = _addressService.GetProvinces(),
                    ProvinceId = record.LocationProvincePartRecord.Id,
                    Districts = _addressService.GetDistricts(record.LocationProvincePartRecord.Id),
                    DistrictId = record.LocationDistrictPartRecord.Id,
                    Wards = _addressService.GetWards(record.LocationDistrictPartRecord.Id),
                    WardId = record.LocationWardPartRecord.Id,
                    ReturnUrl = returnUrl
                }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(ward);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, FormCollection frmCollection)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            PlanningMapRecord record = _planningMapRepository.Get(id);

            var editModel = new PlanningMapEditViewModel {PlanningMap = record};

            if (TryUpdateModel(editModel))
            {
                // VerifyUnicity 
                if (_planningMapRepository.Fetch(a => a.LocationProvincePartRecord.Id == editModel.ProvinceId &&
                                                      a.LocationDistrictPartRecord.Id == editModel.DistrictId &&
                                                      a.LocationWardPartRecord.Id == editModel.WardId)
                    .Any(a => a.Id != record.Id))
                {
                    AddModelError("WardId",
                        T("PlanningMap {0} - {1} already exists.", record.LocationWardPartRecord.Name,
                            record.LocationDistrictPartRecord.ShortName));
                }
                else
                {
                    LocationWardPartRecord ward = _addressService.GetWard(editModel.WardId);

                    record.LocationProvincePartRecord = ward.Province;
                    record.LocationDistrictPartRecord = ward.District;
                    record.LocationWardPartRecord = ward;
                    record.ImagesPath = editModel.ImagesPath;
                    record.Width = editModel.Width;
                    record.Height = editModel.Height;
                    record.MinZoom = editModel.MinZoom;
                    record.MaxZoom = editModel.MaxZoom;
                    record.Ratio = editModel.Ratio;
                    record.IsEnabled = editModel.IsEnabled;

                    _planningMapRepository.Update(record);

                    Services.Notifier.Information(T("PlanningMap <a href='{0}'>{1} - {2}</a> updated.",
                        Url.Action("Edit", new {record.Id}), ward.Name, ward.District.Name));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.Provinces = _addressService.GetProvinces();
                editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
                editModel.Wards = _addressService.GetWards(editModel.DistrictId);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PlanningMap.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";

                var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");
                dynamic model = Services.ContentManager.UpdateEditor(ward, this);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

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
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            PlanningMapRecord planningMap = _planningMapRepository.Get(id);

            if (planningMap != null)
            {
                planningMap.IsEnabled = true;
                _planningMapRepository.Update(planningMap);

                Services.Notifier.Information(T("PlanningMap {0} - {1} updated", planningMap.LocationWardPartRecord.Name,
                    planningMap.LocationDistrictPartRecord.ShortName));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            PlanningMapRecord planningMap = _planningMapRepository.Get(id);

            if (planningMap != null)
            {
                planningMap.IsEnabled = false;
                _planningMapRepository.Update(planningMap);

                Services.Notifier.Information(T("PlanningMap {0} - {1} updated", planningMap.LocationWardPartRecord.Name,
                    planningMap.LocationDistrictPartRecord.ShortName));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePlanningMaps,
                    T("Not authorized to manage PlanningMaps")))
                return new HttpUnauthorizedResult();

            PlanningMapRecord planningMap = _planningMapRepository.Get(id);

            if (planningMap != null)
            {
                _planningMapRepository.Delete(planningMap);

                Services.Notifier.Information(T("PlanningMap {0} - {1} deleted", planningMap.LocationWardPartRecord.Name,
                    planningMap.LocationDistrictPartRecord.ShortName));
            }

            return RedirectToAction("Index");
        }
    }
}