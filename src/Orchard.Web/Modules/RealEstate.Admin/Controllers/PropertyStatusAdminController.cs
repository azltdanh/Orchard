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
    public class PropertyStatusAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyStatusService _propertyStatusService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyStatusAdminController(
            IOrchardServices services,
            IPropertyStatusService propertyStatusService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyStatusService = propertyStatusService;
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

        public ActionResult Index(PropertyStatusIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to list propertyStatus")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyStatusIndexOptions();

            IContentQuery<PropertyStatusPart, PropertyStatusPartRecord> propertyStatus = Services.ContentManager
                .Query<PropertyStatusPart, PropertyStatusPartRecord>();

            switch (options.Filter)
            {
                case PropertyStatusFilter.All:
                    //propertyStatus = propertyStatus.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyStatus =
                    propertyStatus.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyStatus.Count());

            switch (options.Order)
            {
                case PropertyStatusOrder.SeqOrder:
                    propertyStatus = propertyStatus.OrderBy(u => u.SeqOrder);
                    break;
                case PropertyStatusOrder.Name:
                    propertyStatus = propertyStatus.OrderBy(u => u.Name);
                    break;
            }

            List<PropertyStatusPart> results = propertyStatus
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyStatusIndexViewModel
            {
                PropertyStatus = results
                    .Select(x => new PropertyStatusEntry {PropertyStatus = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyStatusIndexViewModel
            {
                PropertyStatus = new List<PropertyStatusEntry>(),
                Options = new PropertyStatusIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyStatusEntry> checkedEntries = viewModel.PropertyStatus.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyStatusBulkAction.None:
                    break;
                case PropertyStatusBulkAction.Enable:
                    foreach (PropertyStatusEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyStatus.Id);
                    }
                    break;
                case PropertyStatusBulkAction.Disable:
                    foreach (PropertyStatusEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyStatus.Id);
                    }
                    break;
                case PropertyStatusBulkAction.Delete:
                    foreach (PropertyStatusEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyStatus.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var propertyStatus = Services.ContentManager.New<PropertyStatusPart>("PropertyStatus");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyStatus.Create",
                Model: new PropertyStatusCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyStatus);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyStatusCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyStatusService.VerifyPropertyStatusUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyStatusName", T("PropertyStatus with that name already exists."));
                }
            }

            var propertyStatus = Services.ContentManager.New<PropertyStatusPart>("PropertyStatus");
            if (ModelState.IsValid)
            {
                propertyStatus.Name = createModel.Name;
                propertyStatus.ShortName = createModel.ShortName;
                propertyStatus.CssClass = createModel.CssClass;
                propertyStatus.SeqOrder = createModel.SeqOrder;
                propertyStatus.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(propertyStatus);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyStatus, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyStatus.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Status_Changed");

            Services.Notifier.Information(T("PropertyStatus {0} created", propertyStatus.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var propertyStatus = Services.ContentManager.Get<PropertyStatusPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyStatus.Edit",
                Model: new PropertyStatusEditViewModel {PropertyStatus = propertyStatus}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyStatus);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var propertyStatus = Services.ContentManager.Get<PropertyStatusPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyStatus, this);

            var editModel = new PropertyStatusEditViewModel {PropertyStatus = propertyStatus};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyStatusService.VerifyPropertyStatusUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyStatusName", T("PropertyStatus with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyStatus.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Status_Changed");

            Services.Notifier.Information(T("PropertyStatus {0} updated", propertyStatus.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var propertyStatus = Services.ContentManager.Get<PropertyStatusPart>(id);

            if (propertyStatus != null)
            {
                propertyStatus.IsEnabled = true;

                _signals.Trigger("Status_Changed");

                Services.Notifier.Information(T("PropertyStatus {0} updated", propertyStatus.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var propertyStatus = Services.ContentManager.Get<PropertyStatusPart>(id);

            if (propertyStatus != null)
            {
                propertyStatus.IsEnabled = false;

                _signals.Trigger("Status_Changed");

                Services.Notifier.Information(T("PropertyStatus {0} updated", propertyStatus.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyStatus,
                    T("Not authorized to manage propertyStatus")))
                return new HttpUnauthorizedResult();

            var propertyStatus = Services.ContentManager.Get<PropertyStatusPart>(id);

            if (propertyStatus != null)
            {
                Services.ContentManager.Remove(propertyStatus.ContentItem);

                _signals.Trigger("Status_Changed");

                Services.Notifier.Information(T("PropertyStatus {0} deleted", propertyStatus.Name));
            }

            return RedirectToAction("Index");
        }
    }
}