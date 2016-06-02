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
    public class CustomerPurposeAdminController : Controller, IUpdateModel
    {
        private readonly ICustomerPurposeService _customerPurposeService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public CustomerPurposeAdminController(
            IOrchardServices services,
            ICustomerPurposeService customerPurposeService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _customerPurposeService = customerPurposeService;
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

        public ActionResult Index(CustomerPurposeIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to list customerPurpose")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CustomerPurposeIndexOptions();

            IContentQuery<CustomerPurposePart, CustomerPurposePartRecord> customerPurpose = Services.ContentManager
                .Query<CustomerPurposePart, CustomerPurposePartRecord>();

            switch (options.Filter)
            {
                case CustomerPurposeFilter.All:
                    //customerPurpose = customerPurpose.Where(u => u.RegistrationPurpose == UserPurpose.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                customerPurpose =
                    customerPurpose.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(customerPurpose.Count());

            switch (options.Order)
            {
                case CustomerPurposeOrder.SeqOrder:
                    customerPurpose = customerPurpose.OrderBy(u => u.SeqOrder);
                    break;
                case CustomerPurposeOrder.Name:
                    customerPurpose = customerPurpose.OrderBy(u => u.Name);
                    break;
            }

            List<CustomerPurposePart> results = customerPurpose
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CustomerPurposeIndexViewModel
            {
                Purposes = results
                    .Select(x => new CustomerPurposeEntry { Purpose = x.Record })
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
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var viewModel = new CustomerPurposeIndexViewModel
            {
                Purposes = new List<CustomerPurposeEntry>(),
                Options = new CustomerPurposeIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CustomerPurposeEntry> checkedEntries = viewModel.Purposes.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CustomerPurposeBulkAction.None:
                    break;
                case CustomerPurposeBulkAction.Enable:
                    foreach (CustomerPurposeEntry entry in checkedEntries)
                    {
                        Enable(entry.Purpose.Id);
                    }
                    break;
                case CustomerPurposeBulkAction.Disable:
                    foreach (CustomerPurposeEntry entry in checkedEntries)
                    {
                        Disable(entry.Purpose.Id);
                    }
                    break;
                case CustomerPurposeBulkAction.Delete:
                    foreach (CustomerPurposeEntry entry in checkedEntries)
                    {
                        Delete(entry.Purpose.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var customerPurpose = Services.ContentManager.New<CustomerPurposePart>("CustomerPurpose");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerPurpose.Create",
                Model: new CustomerPurposeCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customerPurpose);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CustomerPurposeCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_customerPurposeService.VerifyCustomerPurposeUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueCustomerPurposeName", T("CustomerPurpose with that name already exists."));
                }
            }

            var customerPurpose = Services.ContentManager.New<CustomerPurposePart>("CustomerPurpose");
            if (ModelState.IsValid)
            {
                customerPurpose.Name = createModel.Name;
                customerPurpose.ShortName = createModel.ShortName;
                customerPurpose.CssClass = createModel.CssClass;
                customerPurpose.SeqOrder = createModel.SeqOrder;
                customerPurpose.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(customerPurpose);
            }

            dynamic model = Services.ContentManager.UpdateEditor(customerPurpose, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerPurpose.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("CustomerPurpose_Changed");

            Services.Notifier.Information(T("CustomerPurpose {0} created", customerPurpose.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var customerPurpose = Services.ContentManager.Get<CustomerPurposePart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerPurpose.Edit",
                Model: new CustomerPurposeEditViewModel {CustomerPurpose = customerPurpose}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customerPurpose);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var customerPurpose = Services.ContentManager.Get<CustomerPurposePart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(customerPurpose, this);

            var editModel = new CustomerPurposeEditViewModel {CustomerPurpose = customerPurpose};
            if (TryUpdateModel(editModel))
            {
                if (!_customerPurposeService.VerifyCustomerPurposeUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueCustomerPurposeName", T("CustomerPurpose with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerPurpose.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("CustomerPurpose_Changed");

            Services.Notifier.Information(T("CustomerPurpose {0} updated", customerPurpose.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var customerPurpose = Services.ContentManager.Get<CustomerPurposePart>(id);

            if (customerPurpose != null)
            {
                customerPurpose.IsEnabled = true;

                _signals.Trigger("CustomerPurpose_Changed");

                Services.Notifier.Information(T("CustomerPurpose {0} updated", customerPurpose.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var customerPurpose = Services.ContentManager.Get<CustomerPurposePart>(id);

            if (customerPurpose != null)
            {
                customerPurpose.IsEnabled = false;

                _signals.Trigger("CustomerPurpose_Changed");

                Services.Notifier.Information(T("CustomerPurpose {0} updated", customerPurpose.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerPurposes,
                    T("Not authorized to manage customerPurpose")))
                return new HttpUnauthorizedResult();

            var customerPurpose = Services.ContentManager.Get<CustomerPurposePart>(id);

            if (customerPurpose != null)
            {
                Services.ContentManager.Remove(customerPurpose.ContentItem);

                _signals.Trigger("CustomerPurpose_Changed");

                Services.Notifier.Information(T("CustomerPurpose {0} deleted", customerPurpose.Name));
            }

            return RedirectToAction("Index");
        }
    }
}