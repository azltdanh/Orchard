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
    public class PropertyInteriorAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyInteriorService _propertyInteriorService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyInteriorAdminController(
            IOrchardServices services,
            IPropertyInteriorService propertyInteriorService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyInteriorService = propertyInteriorService;
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

        public ActionResult Index(PropertyInteriorIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to list propertyInteriors")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyInteriorIndexOptions();

            IContentQuery<PropertyInteriorPart, PropertyInteriorPartRecord> propertyInteriors = Services.ContentManager
                .Query<PropertyInteriorPart, PropertyInteriorPartRecord>();

            switch (options.Filter)
            {
                case PropertyInteriorsFilter.All:
                    //propertyInteriors = propertyInteriors.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyInteriors =
                    propertyInteriors.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyInteriors.Count());

            switch (options.Order)
            {
                case PropertyInteriorsOrder.SeqOrder:
                    propertyInteriors = propertyInteriors.OrderBy(u => u.SeqOrder);
                    break;
                case PropertyInteriorsOrder.Name:
                    propertyInteriors = propertyInteriors.OrderBy(u => u.Name);
                    break;
            }

            List<PropertyInteriorPart> results = propertyInteriors
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyInteriorsIndexViewModel
            {
                PropertyInteriors = results
                    .Select(x => new PropertyInteriorEntry {PropertyInterior = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyInteriorsIndexViewModel
            {
                PropertyInteriors = new List<PropertyInteriorEntry>(),
                Options = new PropertyInteriorIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyInteriorEntry> checkedEntries = viewModel.PropertyInteriors.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyInteriorsBulkAction.None:
                    break;
                case PropertyInteriorsBulkAction.Enable:
                    foreach (PropertyInteriorEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyInterior.Id);
                    }
                    break;
                case PropertyInteriorsBulkAction.Disable:
                    foreach (PropertyInteriorEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyInterior.Id);
                    }
                    break;
                case PropertyInteriorsBulkAction.Delete:
                    foreach (PropertyInteriorEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyInterior.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.New<PropertyInteriorPart>("PropertyInterior");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyInterior.Create",
                Model: new PropertyInteriorCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyInterior);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyInteriorCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyInteriorService.VerifyPropertyInteriorUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyInteriorName", T("PropertyInterior with that name already exists."));
                }
            }

            var propertyInterior = Services.ContentManager.New<PropertyInteriorPart>("PropertyInterior");
            if (ModelState.IsValid)
            {
                propertyInterior.Name = createModel.Name;
                propertyInterior.ShortName = createModel.ShortName;
                propertyInterior.CssClass = createModel.CssClass;
                propertyInterior.SeqOrder = createModel.SeqOrder;
                propertyInterior.IsEnabled = createModel.IsEnabled;
                propertyInterior.UnitPrice = createModel.UnitPrice;

                Services.ContentManager.Create(propertyInterior);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyInterior, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyInterior.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Interiors_Changed");

            Services.Notifier.Information(T("PropertyInterior {0} created", propertyInterior.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<PropertyInteriorPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyInterior.Edit",
                Model: new PropertyInteriorEditViewModel {PropertyInterior = propertyInterior}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyInterior);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<PropertyInteriorPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyInterior, this);

            var editModel = new PropertyInteriorEditViewModel {PropertyInterior = propertyInterior};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyInteriorService.VerifyPropertyInteriorUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyInteriorName", T("PropertyInterior with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyInterior.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Interiors_Changed");

            Services.Notifier.Information(T("PropertyInterior {0} updated", propertyInterior.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<PropertyInteriorPart>(id);

            if (propertyInterior != null)
            {
                propertyInterior.IsEnabled = true;

                _signals.Trigger("Interiors_Changed");

                Services.Notifier.Information(T("PropertyInterior {0} updated", propertyInterior.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<PropertyInteriorPart>(id);

            if (propertyInterior != null)
            {
                propertyInterior.IsEnabled = false;

                _signals.Trigger("Interiors_Changed");

                Services.Notifier.Information(T("PropertyInterior {0} updated", propertyInterior.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyInteriors,
                    T("Not authorized to manage propertyInteriors")))
                return new HttpUnauthorizedResult();

            var propertyInterior = Services.ContentManager.Get<PropertyInteriorPart>(id);

            if (propertyInterior != null)
            {
                Services.ContentManager.Remove(propertyInterior.ContentItem);

                _signals.Trigger("Interiors_Changed");

                Services.Notifier.Information(T("PropertyInterior {0} deleted", propertyInterior.Name));
            }

            return RedirectToAction("Index");
        }
    }
}