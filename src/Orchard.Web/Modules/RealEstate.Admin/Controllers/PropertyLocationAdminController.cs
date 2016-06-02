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
    public class PropertyLocationAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyLocationService _propertyLocationService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyLocationAdminController(
            IOrchardServices services,
            IPropertyLocationService propertyLocationService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyLocationService = propertyLocationService;
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

        public ActionResult Index(PropertyLocationIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to list propertyLocations")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyLocationIndexOptions();

            IContentQuery<PropertyLocationPart, PropertyLocationPartRecord> propertyLocations = Services.ContentManager
                .Query<PropertyLocationPart, PropertyLocationPartRecord>();

            switch (options.Filter)
            {
                case PropertyLocationsFilter.All:
                    //propertyLocations = propertyLocations.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyLocations =
                    propertyLocations.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyLocations.Count());

            switch (options.Order)
            {
                case PropertyLocationsOrder.SeqOrder:
                    propertyLocations = propertyLocations.OrderBy(u => u.SeqOrder);
                    break;
                case PropertyLocationsOrder.Name:
                    propertyLocations = propertyLocations.OrderBy(u => u.Name);
                    break;
            }

            List<PropertyLocationPart> results = propertyLocations
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyLocationsIndexViewModel
            {
                PropertyLocations = results
                    .Select(x => new PropertyLocationEntry {PropertyLocation = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyLocationsIndexViewModel
            {
                PropertyLocations = new List<PropertyLocationEntry>(),
                Options = new PropertyLocationIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyLocationEntry> checkedEntries = viewModel.PropertyLocations.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyLocationsBulkAction.None:
                    break;
                case PropertyLocationsBulkAction.Enable:
                    foreach (PropertyLocationEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyLocation.Id);
                    }
                    break;
                case PropertyLocationsBulkAction.Disable:
                    foreach (PropertyLocationEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyLocation.Id);
                    }
                    break;
                case PropertyLocationsBulkAction.Delete:
                    foreach (PropertyLocationEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyLocation.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var propertyLocation = Services.ContentManager.New<PropertyLocationPart>("PropertyLocation");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLocation.Create",
                Model: new PropertyLocationCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyLocation);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyLocationCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyLocationService.VerifyPropertyLocationUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyLocationName", T("PropertyLocation with that name already exists."));
                }
            }

            var propertyLocation = Services.ContentManager.New<PropertyLocationPart>("PropertyLocation");
            if (ModelState.IsValid)
            {
                propertyLocation.Name = createModel.Name;
                propertyLocation.ShortName = createModel.ShortName;
                propertyLocation.CssClass = createModel.CssClass;
                propertyLocation.SeqOrder = createModel.SeqOrder;
                propertyLocation.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(propertyLocation);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyLocation, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLocation.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Locations_Changed");

            Services.Notifier.Information(T("PropertyLocation {0} created", propertyLocation.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var propertyLocation = Services.ContentManager.Get<PropertyLocationPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLocation.Edit",
                Model: new PropertyLocationEditViewModel {PropertyLocation = propertyLocation}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyLocation);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var propertyLocation = Services.ContentManager.Get<PropertyLocationPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyLocation, this);

            var editModel = new PropertyLocationEditViewModel {PropertyLocation = propertyLocation};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyLocationService.VerifyPropertyLocationUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyLocationName", T("PropertyLocation with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLocation.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Locations_Changed");

            Services.Notifier.Information(T("PropertyLocation {0} updated", propertyLocation.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var propertyLocation = Services.ContentManager.Get<PropertyLocationPart>(id);

            if (propertyLocation != null)
            {
                propertyLocation.IsEnabled = true;

                _signals.Trigger("Locations_Changed");

                Services.Notifier.Information(T("PropertyLocation {0} updated", propertyLocation.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var propertyLocation = Services.ContentManager.Get<PropertyLocationPart>(id);

            if (propertyLocation != null)
            {
                propertyLocation.IsEnabled = false;

                _signals.Trigger("Locations_Changed");

                Services.Notifier.Information(T("PropertyLocation {0} updated", propertyLocation.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLocations,
                    T("Not authorized to manage propertyLocations")))
                return new HttpUnauthorizedResult();

            var propertyLocation = Services.ContentManager.Get<PropertyLocationPart>(id);

            if (propertyLocation != null)
            {
                Services.ContentManager.Remove(propertyLocation.ContentItem);

                _signals.Trigger("Locations_Changed");

                Services.Notifier.Information(T("PropertyLocation {0} deleted", propertyLocation.Name));
            }

            return RedirectToAction("Index");
        }
    }
}