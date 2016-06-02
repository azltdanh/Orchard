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
    public class CustomerFeedbackAdminController : Controller, IUpdateModel
    {
        private readonly ICustomerFeedbackService _customerFeedbackService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public CustomerFeedbackAdminController(
            IOrchardServices services,
            ICustomerFeedbackService customerFeedbackService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _customerFeedbackService = customerFeedbackService;
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

        public ActionResult Index(CustomerFeedbackIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to list customerFeedback")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CustomerFeedbackIndexOptions();

            IContentQuery<CustomerFeedbackPart, CustomerFeedbackPartRecord> customerFeedback = Services.ContentManager
                .Query<CustomerFeedbackPart, CustomerFeedbackPartRecord>();

            switch (options.Filter)
            {
                case CustomerFeedbackFilter.All:
                    //customerFeedback = customerFeedback.Where(u => u.RegistrationFeedback == UserFeedback.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                customerFeedback =
                    customerFeedback.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(customerFeedback.Count());

            switch (options.Order)
            {
                case CustomerFeedbackOrder.SeqOrder:
                    customerFeedback = customerFeedback.OrderBy(u => u.SeqOrder);
                    break;
                case CustomerFeedbackOrder.Name:
                    customerFeedback = customerFeedback.OrderBy(u => u.Name);
                    break;
            }

            List<CustomerFeedbackPart> results = customerFeedback
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CustomerFeedbackIndexViewModel
            {
                Feedbacks = results
                    .Select(x => new CustomerFeedbackEntry { Feedback = x.Record })
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
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var viewModel = new CustomerFeedbackIndexViewModel
            {
                Feedbacks = new List<CustomerFeedbackEntry>(),
                Options = new CustomerFeedbackIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CustomerFeedbackEntry> checkedEntries = viewModel.Feedbacks.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CustomerFeedbackBulkAction.None:
                    break;
                case CustomerFeedbackBulkAction.Enable:
                    foreach (CustomerFeedbackEntry entry in checkedEntries)
                    {
                        Enable(entry.Feedback.Id);
                    }
                    break;
                case CustomerFeedbackBulkAction.Disable:
                    foreach (CustomerFeedbackEntry entry in checkedEntries)
                    {
                        Disable(entry.Feedback.Id);
                    }
                    break;
                case CustomerFeedbackBulkAction.Delete:
                    foreach (CustomerFeedbackEntry entry in checkedEntries)
                    {
                        Delete(entry.Feedback.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var customerFeedback = Services.ContentManager.New<CustomerFeedbackPart>("CustomerFeedback");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerFeedback.Create",
                Model: new CustomerFeedbackCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customerFeedback);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CustomerFeedbackCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_customerFeedbackService.VerifyCustomerFeedbackUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueCustomerFeedbackName", T("CustomerFeedback with that name already exists."));
                }
            }

            var customerFeedback = Services.ContentManager.New<CustomerFeedbackPart>("CustomerFeedback");
            if (ModelState.IsValid)
            {
                customerFeedback.Name = createModel.Name;
                customerFeedback.ShortName = createModel.ShortName;
                customerFeedback.CssClass = createModel.CssClass;
                customerFeedback.SeqOrder = createModel.SeqOrder;
                customerFeedback.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(customerFeedback);
            }

            dynamic model = Services.ContentManager.UpdateEditor(customerFeedback, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerFeedback.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("CustomerFeedback_Changed");

            Services.Notifier.Information(T("CustomerFeedback {0} created", customerFeedback.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var customerFeedback = Services.ContentManager.Get<CustomerFeedbackPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerFeedback.Edit",
                Model: new CustomerFeedbackEditViewModel {CustomerFeedback = customerFeedback}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customerFeedback);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var customerFeedback = Services.ContentManager.Get<CustomerFeedbackPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(customerFeedback, this);

            var editModel = new CustomerFeedbackEditViewModel {CustomerFeedback = customerFeedback};
            if (TryUpdateModel(editModel))
            {
                if (!_customerFeedbackService.VerifyCustomerFeedbackUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueCustomerFeedbackName", T("CustomerFeedback with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerFeedback.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("CustomerFeedback_Changed");

            Services.Notifier.Information(T("CustomerFeedback {0} updated", customerFeedback.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var customerFeedback = Services.ContentManager.Get<CustomerFeedbackPart>(id);

            if (customerFeedback != null)
            {
                customerFeedback.IsEnabled = true;

                _signals.Trigger("CustomerFeedback_Changed");

                Services.Notifier.Information(T("CustomerFeedback {0} updated", customerFeedback.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var customerFeedback = Services.ContentManager.Get<CustomerFeedbackPart>(id);

            if (customerFeedback != null)
            {
                customerFeedback.IsEnabled = false;

                _signals.Trigger("CustomerFeedback_Changed");

                Services.Notifier.Information(T("CustomerFeedback {0} updated", customerFeedback.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCustomerFeedbacks,
                    T("Not authorized to manage customerFeedback")))
                return new HttpUnauthorizedResult();

            var customerFeedback = Services.ContentManager.Get<CustomerFeedbackPart>(id);

            if (customerFeedback != null)
            {
                Services.ContentManager.Remove(customerFeedback.ContentItem);

                _signals.Trigger("CustomerFeedback_Changed");

                Services.Notifier.Information(T("CustomerFeedback {0} deleted", customerFeedback.Name));
            }

            return RedirectToAction("Index");
        }
    }
}