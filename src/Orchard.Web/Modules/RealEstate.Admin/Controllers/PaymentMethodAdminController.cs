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
    public class PaymentMethodAdminController : Controller, IUpdateModel
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PaymentMethodAdminController(
            IOrchardServices services,
            IPaymentMethodService paymentMethodService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _paymentMethodService = paymentMethodService;
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

        public ActionResult Index(PaymentMethodIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to list paymentMethods")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PaymentMethodIndexOptions();

            IContentQuery<PaymentMethodPart, PaymentMethodPartRecord> paymentMethods = Services.ContentManager
                .Query<PaymentMethodPart, PaymentMethodPartRecord>();

            switch (options.Filter)
            {
                case PaymentMethodsFilter.All:
                    //paymentMethods = paymentMethods.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                paymentMethods =
                    paymentMethods.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(paymentMethods.Count());

            switch (options.Order)
            {
                case PaymentMethodsOrder.SeqOrder:
                    paymentMethods = paymentMethods.OrderBy(u => u.SeqOrder);
                    break;
                case PaymentMethodsOrder.Name:
                    paymentMethods = paymentMethods.OrderBy(u => u.Name);
                    break;
            }

            List<PaymentMethodPart> results = paymentMethods
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PaymentMethodsIndexViewModel
            {
                PaymentMethods = results
                    .Select(x => new PaymentMethodEntry {PaymentMethod = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var viewModel = new PaymentMethodsIndexViewModel
            {
                PaymentMethods = new List<PaymentMethodEntry>(),
                Options = new PaymentMethodIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PaymentMethodEntry> checkedEntries = viewModel.PaymentMethods.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PaymentMethodsBulkAction.None:
                    break;
                case PaymentMethodsBulkAction.Enable:
                    foreach (PaymentMethodEntry entry in checkedEntries)
                    {
                        Enable(entry.PaymentMethod.Id);
                    }
                    break;
                case PaymentMethodsBulkAction.Disable:
                    foreach (PaymentMethodEntry entry in checkedEntries)
                    {
                        Disable(entry.PaymentMethod.Id);
                    }
                    break;
                case PaymentMethodsBulkAction.Delete:
                    foreach (PaymentMethodEntry entry in checkedEntries)
                    {
                        Delete(entry.PaymentMethod.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var paymentMethod = Services.ContentManager.New<PaymentMethodPart>("PaymentMethod");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentMethod.Create",
                Model: new PaymentMethodCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(paymentMethod);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PaymentMethodCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_paymentMethodService.VerifyPaymentMethodUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePaymentMethodName", T("PaymentMethod with that name already exists."));
                }
            }

            var paymentMethod = Services.ContentManager.New<PaymentMethodPart>("PaymentMethod");
            if (ModelState.IsValid)
            {
                paymentMethod.Name = createModel.Name;
                paymentMethod.ShortName = createModel.ShortName;
                paymentMethod.CssClass = createModel.CssClass;
                paymentMethod.SeqOrder = createModel.SeqOrder;
                paymentMethod.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(paymentMethod);
            }

            dynamic model = Services.ContentManager.UpdateEditor(paymentMethod, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentMethod.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("PaymentMethods_Changed");

            Services.Notifier.Information(T("PaymentMethod {0} created", paymentMethod.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var paymentMethod = Services.ContentManager.Get<PaymentMethodPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentMethod.Edit",
                Model: new PaymentMethodEditViewModel {PaymentMethod = paymentMethod}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(paymentMethod);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var paymentMethod = Services.ContentManager.Get<PaymentMethodPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(paymentMethod, this);

            var editModel = new PaymentMethodEditViewModel {PaymentMethod = paymentMethod};
            if (TryUpdateModel(editModel))
            {
                if (!_paymentMethodService.VerifyPaymentMethodUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePaymentMethodName", T("PaymentMethod with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentMethod.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("PaymentMethods_Changed");

            Services.Notifier.Information(T("PaymentMethod {0} updated", paymentMethod.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var paymentMethod = Services.ContentManager.Get<PaymentMethodPart>(id);

            if (paymentMethod != null)
            {
                paymentMethod.IsEnabled = true;

                _signals.Trigger("PaymentMethods_Changed");

                Services.Notifier.Information(T("PaymentMethod {0} updated", paymentMethod.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var paymentMethod = Services.ContentManager.Get<PaymentMethodPart>(id);

            if (paymentMethod != null)
            {
                paymentMethod.IsEnabled = false;

                _signals.Trigger("PaymentMethods_Changed");

                Services.Notifier.Information(T("PaymentMethod {0} updated", paymentMethod.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentMethods,
                    T("Not authorized to manage paymentMethods")))
                return new HttpUnauthorizedResult();

            var paymentMethod = Services.ContentManager.Get<PaymentMethodPart>(id);

            if (paymentMethod != null)
            {
                Services.ContentManager.Remove(paymentMethod.ContentItem);

                _signals.Trigger("PaymentMethods_Changed");

                Services.Notifier.Information(T("PaymentMethod {0} deleted", paymentMethod.Name));
            }

            return RedirectToAction("Index");
        }
    }
}