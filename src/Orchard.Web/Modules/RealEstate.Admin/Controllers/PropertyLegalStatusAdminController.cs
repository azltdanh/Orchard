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
    public class PropertyLegalStatusAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyLegalStatusService _propertyLegalStatusService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyLegalStatusAdminController(
            IOrchardServices services,
            IPropertyLegalStatusService propertyLegalStatusService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyLegalStatusService = propertyLegalStatusService;
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

        public ActionResult Index(PropertyLegalStatusIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to list propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyLegalStatusIndexOptions();

            IContentQuery<PropertyLegalStatusPart, PropertyLegalStatusPartRecord> propertyLegalStatus =
                Services.ContentManager
                    .Query<PropertyLegalStatusPart, PropertyLegalStatusPartRecord>();

            switch (options.Filter)
            {
                case PropertyLegalStatusFilter.All:
                    //propertyLegalStatus = propertyLegalStatus.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyLegalStatus =
                    propertyLegalStatus.Where(
                        u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyLegalStatus.Count());

            switch (options.Order)
            {
                case PropertyLegalStatusOrder.SeqOrder:
                    propertyLegalStatus = propertyLegalStatus.OrderBy(u => u.SeqOrder);
                    break;
                case PropertyLegalStatusOrder.Name:
                    propertyLegalStatus = propertyLegalStatus.OrderBy(u => u.Name);
                    break;
            }

            List<PropertyLegalStatusPart> results = propertyLegalStatus
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyLegalStatusIndexViewModel
            {
                PropertyLegalStatus = results
                    .Select(x => new PropertyLegalStatusEntry {PropertyLegalStatus = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyLegalStatusIndexViewModel
            {
                PropertyLegalStatus = new List<PropertyLegalStatusEntry>(),
                Options = new PropertyLegalStatusIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyLegalStatusEntry> checkedEntries = viewModel.PropertyLegalStatus.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyLegalStatusBulkAction.None:
                    break;
                case PropertyLegalStatusBulkAction.Enable:
                    foreach (PropertyLegalStatusEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyLegalStatus.Id);
                    }
                    break;
                case PropertyLegalStatusBulkAction.Disable:
                    foreach (PropertyLegalStatusEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyLegalStatus.Id);
                    }
                    break;
                case PropertyLegalStatusBulkAction.Delete:
                    foreach (PropertyLegalStatusEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyLegalStatus.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var propertyLegalStatus = Services.ContentManager.New<PropertyLegalStatusPart>("PropertyLegalStatus");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLegalStatus.Create",
                Model: new PropertyLegalStatusCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyLegalStatus);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyLegalStatusCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyLegalStatusService.VerifyPropertyLegalStatusUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyLegalStatusName",
                        T("PropertyLegalStatus with that name already exists."));
                }
            }

            var propertyLegalStatus = Services.ContentManager.New<PropertyLegalStatusPart>("PropertyLegalStatus");
            if (ModelState.IsValid)
            {
                propertyLegalStatus.Name = createModel.Name;
                propertyLegalStatus.ShortName = createModel.ShortName;
                propertyLegalStatus.CssClass = createModel.CssClass;
                propertyLegalStatus.SeqOrder = createModel.SeqOrder;
                propertyLegalStatus.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(propertyLegalStatus);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyLegalStatus, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLegalStatus.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("LegalStatus_Changed");

            Services.Notifier.Information(T("PropertyLegalStatus {0} created", propertyLegalStatus.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var propertyLegalStatus = Services.ContentManager.Get<PropertyLegalStatusPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLegalStatus.Edit",
                Model: new PropertyLegalStatusEditViewModel {PropertyLegalStatus = propertyLegalStatus}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyLegalStatus);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var propertyLegalStatus = Services.ContentManager.Get<PropertyLegalStatusPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyLegalStatus, this);

            var editModel = new PropertyLegalStatusEditViewModel {PropertyLegalStatus = propertyLegalStatus};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyLegalStatusService.VerifyPropertyLegalStatusUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyLegalStatusName",
                        T("PropertyLegalStatus with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyLegalStatus.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("LegalStatus_Changed");

            Services.Notifier.Information(T("PropertyLegalStatus {0} updated", propertyLegalStatus.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var propertyLegalStatus = Services.ContentManager.Get<PropertyLegalStatusPart>(id);

            if (propertyLegalStatus != null)
            {
                propertyLegalStatus.IsEnabled = true;

                _signals.Trigger("LegalStatus_Changed");

                Services.Notifier.Information(T("PropertyLegalStatus {0} updated", propertyLegalStatus.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var propertyLegalStatus = Services.ContentManager.Get<PropertyLegalStatusPart>(id);

            if (propertyLegalStatus != null)
            {
                propertyLegalStatus.IsEnabled = false;

                _signals.Trigger("LegalStatus_Changed");

                Services.Notifier.Information(T("PropertyLegalStatus {0} updated", propertyLegalStatus.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyLegalStatus,
                    T("Not authorized to manage propertyLegalStatus")))
                return new HttpUnauthorizedResult();

            var propertyLegalStatus = Services.ContentManager.Get<PropertyLegalStatusPart>(id);

            if (propertyLegalStatus != null)
            {
                Services.ContentManager.Remove(propertyLegalStatus.ContentItem);

                _signals.Trigger("LegalStatus_Changed");

                Services.Notifier.Information(T("PropertyLegalStatus {0} deleted", propertyLegalStatus.Name));
            }

            return RedirectToAction("Index");
        }
    }
}