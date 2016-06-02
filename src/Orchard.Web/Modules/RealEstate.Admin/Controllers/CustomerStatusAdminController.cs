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
    public class CustomerStatusAdminController : Controller, IUpdateModel
    {
        private readonly ICustomerStatusService _customerStatusService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public CustomerStatusAdminController(
            IOrchardServices services,
            ICustomerStatusService customerStatusService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _customerStatusService = customerStatusService;
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

        public ActionResult Index(CustomerStatusIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to list customerStatus")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CustomerStatusIndexOptions();

            IContentQuery<CustomerStatusPart, CustomerStatusPartRecord> customerStatus = Services.ContentManager
                .Query<CustomerStatusPart, CustomerStatusPartRecord>();

            switch (options.Filter)
            {
                case CustomerStatusFilter.All:
                    //customerStatus = customerStatus.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                customerStatus =
                    customerStatus.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(customerStatus.Count());

            switch (options.Order)
            {
                case CustomerStatusOrder.SeqOrder:
                    customerStatus = customerStatus.OrderBy(u => u.SeqOrder);
                    break;
                case CustomerStatusOrder.Name:
                    customerStatus = customerStatus.OrderBy(u => u.Name);
                    break;
            }

            List<CustomerStatusPart> results = customerStatus
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CustomerStatusIndexViewModel
            {
                CustomerStatus = results
                    .Select(x => new CustomerStatusEntry {CustomerStatus = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var viewModel = new CustomerStatusIndexViewModel
            {
                CustomerStatus = new List<CustomerStatusEntry>(),
                Options = new CustomerStatusIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CustomerStatusEntry> checkedEntries = viewModel.CustomerStatus.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CustomerStatusBulkAction.None:
                    break;
                case CustomerStatusBulkAction.Enable:
                    foreach (CustomerStatusEntry entry in checkedEntries)
                    {
                        Enable(entry.CustomerStatus.Id);
                    }
                    break;
                case CustomerStatusBulkAction.Disable:
                    foreach (CustomerStatusEntry entry in checkedEntries)
                    {
                        Disable(entry.CustomerStatus.Id);
                    }
                    break;
                case CustomerStatusBulkAction.Delete:
                    foreach (CustomerStatusEntry entry in checkedEntries)
                    {
                        Delete(entry.CustomerStatus.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var customerStatus = Services.ContentManager.New<CustomerStatusPart>("CustomerStatus");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerStatus.Create",
                Model: new CustomerStatusCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customerStatus);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CustomerStatusCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_customerStatusService.VerifyCustomerStatusUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueCustomerStatusName", T("CustomerStatus with that name already exists."));
                }
            }

            var customerStatus = Services.ContentManager.New<CustomerStatusPart>("CustomerStatus");
            if (ModelState.IsValid)
            {
                customerStatus.Name = createModel.Name;
                customerStatus.ShortName = createModel.ShortName;
                customerStatus.CssClass = createModel.CssClass;
                customerStatus.SeqOrder = createModel.SeqOrder;
                customerStatus.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(customerStatus);
            }

            dynamic model = Services.ContentManager.UpdateEditor(customerStatus, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerStatus.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("CustomerStatus_Changed");

            Services.Notifier.Information(T("CustomerStatus {0} created", customerStatus.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var customerStatus = Services.ContentManager.Get<CustomerStatusPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerStatus.Edit",
                Model: new CustomerStatusEditViewModel {CustomerStatus = customerStatus}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customerStatus);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var customerStatus = Services.ContentManager.Get<CustomerStatusPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(customerStatus, this);

            var editModel = new CustomerStatusEditViewModel {CustomerStatus = customerStatus};
            if (TryUpdateModel(editModel))
            {
                if (!_customerStatusService.VerifyCustomerStatusUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueCustomerStatusName", T("CustomerStatus with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerStatus.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("CustomerStatus_Changed");

            Services.Notifier.Information(T("CustomerStatus {0} updated", customerStatus.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var customerStatus = Services.ContentManager.Get<CustomerStatusPart>(id);

            if (customerStatus != null)
            {
                customerStatus.IsEnabled = true;

                _signals.Trigger("CustomerStatus_Changed");

                Services.Notifier.Information(T("CustomerStatus {0} updated", customerStatus.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var customerStatus = Services.ContentManager.Get<CustomerStatusPart>(id);

            if (customerStatus != null)
            {
                customerStatus.IsEnabled = false;

                _signals.Trigger("CustomerStatus_Changed");

                Services.Notifier.Information(T("CustomerStatus {0} updated", customerStatus.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerStatus,
                    T("Not authorized to manage customerStatus")))
                return new HttpUnauthorizedResult();

            var customerStatus = Services.ContentManager.Get<CustomerStatusPart>(id);

            if (customerStatus != null)
            {
                Services.ContentManager.Remove(customerStatus.ContentItem);

                _signals.Trigger("CustomerStatus_Changed");

                Services.Notifier.Information(T("CustomerStatus {0} deleted", customerStatus.Name));
            }

            return RedirectToAction("Index");
        }
    }
}