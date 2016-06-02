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
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class LocationProvinceAdminController : Controller, IUpdateModel
    {
        private readonly ILocationProvinceService _provinceService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public LocationProvinceAdminController(
            IOrchardServices services,
            ILocationProvinceService provinceService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _provinceService = provinceService;
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

        public ActionResult Index(LocationProvinceIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to list provinces")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new LocationProvinceIndexOptions();

            IContentQuery<LocationProvincePart, LocationProvincePartRecord> provinces = Services.ContentManager
                .Query<LocationProvincePart, LocationProvincePartRecord>();

            switch (options.Filter)
            {
                case LocationProvincesFilter.All:
                    //provinces = provinces.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                provinces = provinces.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(provinces.Count());

            switch (options.Order)
            {
                case LocationProvincesOrder.SeqOrder:
                    provinces = provinces.OrderBy(u => u.SeqOrder);
                    break;
                case LocationProvincesOrder.Name:
                    provinces = provinces.OrderBy(u => u.Name);
                    break;
            }

            List<LocationProvincePart> results = provinces
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationProvincesIndexViewModel
            {
                LocationProvinces = results
                    .Select(x => new LocationProvinceEntry {LocationProvince = x})
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
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationProvincesIndexViewModel
            {
                LocationProvinces = new List<LocationProvinceEntry>(),
                Options = new LocationProvinceIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationProvinceEntry> checkedEntries = viewModel.LocationProvinces.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationProvincesBulkAction.None:
                    break;
                case LocationProvincesBulkAction.Enable:
                    foreach (LocationProvinceEntry entry in checkedEntries)
                    {
                        Enable(entry.LocationProvince.Id);
                    }
                    break;
                case LocationProvincesBulkAction.Disable:
                    foreach (LocationProvinceEntry entry in checkedEntries)
                    {
                        Disable(entry.LocationProvince.Id);
                    }
                    break;
                case LocationProvincesBulkAction.Delete:
                    foreach (LocationProvinceEntry entry in checkedEntries)
                    {
                        Delete(entry.LocationProvince.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var province = Services.ContentManager.New<LocationProvincePart>("LocationProvince");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationProvince.Create",
                Model: new LocationProvinceCreateViewModel
                {
                    IsEnabled = true,
                    ReturnUrl = returnUrl
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(province);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(LocationProvinceCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_provinceService.VerifyProvinceUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueProvinceName", T("Province with that name already exists."));
                }
            }

            var province = Services.ContentManager.New<LocationProvincePart>("LocationProvince");
            if (ModelState.IsValid)
            {
                province.Name = createModel.Name;
                province.ShortName = createModel.ShortName;
                province.SeqOrder = createModel.SeqOrder;
                province.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(province);
            }

            dynamic model = Services.ContentManager.UpdateEditor(province, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationProvince.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Provinces_Changed");

            Services.Notifier.Information(T("Province <a href='{0}'>{1}</a> created.",
                Url.Action("Edit", new {province.Id}), province.Name));

            return RedirectToAction("Create", new {createModel.ReturnUrl});
            //return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var province = Services.ContentManager.Get<LocationProvincePart>(id);
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationProvince.Edit",
                Model: new LocationProvinceEditViewModel
                {
                    LocationProvince = province,
                    ReturnUrl = returnUrl
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(province);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var province = Services.ContentManager.Get<LocationProvincePart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(province, this);

            var editModel = new LocationProvinceEditViewModel {LocationProvince = province};
            if (TryUpdateModel(editModel))
            {
                if (!_provinceService.VerifyProvinceUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueProvinceName", T("Province with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationProvince.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Provinces_Changed");

            Services.Notifier.Information(T("Province <a href='{0}'>{1}</a> updated.",
                Url.Action("Edit", new {province.Id}), province.Name));

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
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var province = Services.ContentManager.Get<LocationProvincePart>(id);

            if (province != null)
            {
                province.IsEnabled = true;

                _signals.Trigger("Provinces_Changed");

                Services.Notifier.Information(T("Province {0} updated", province.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var province = Services.ContentManager.Get<LocationProvincePart>(id);

            if (province != null)
            {
                province.IsEnabled = false;

                _signals.Trigger("Provinces_Changed");

                Services.Notifier.Information(T("Province {0} updated", province.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationProvinces,
                    T("Not authorized to manage provinces")))
                return new HttpUnauthorizedResult();

            var province = Services.ContentManager.Get<LocationProvincePart>(id);

            if (province != null)
            {
                Services.ContentManager.Remove(province.ContentItem);

                _signals.Trigger("Provinces_Changed");

                Services.Notifier.Information(T("Province {0} deleted", province.Name));
            }

            return RedirectToAction("Index");
        }
    }
}