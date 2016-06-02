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
    public class PaymentUnitAdminController : Controller, IUpdateModel
    {
        private readonly IPaymentUnitService _paymentUnitService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PaymentUnitAdminController(
            IOrchardServices services,
            IPaymentUnitService paymentUnitService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _paymentUnitService = paymentUnitService;
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

        public ActionResult Index(PaymentUnitIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManagePaymentUnits, T("Not authorized to list paymentUnits")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PaymentUnitIndexOptions();

            IContentQuery<PaymentUnitPart, PaymentUnitPartRecord> paymentUnits = Services.ContentManager
                .Query<PaymentUnitPart, PaymentUnitPartRecord>();

            switch (options.Filter)
            {
                case PaymentUnitsFilter.All:
                    //paymentUnits = paymentUnits.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                paymentUnits =
                    paymentUnits.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(paymentUnits.Count());

            switch (options.Order)
            {
                case PaymentUnitsOrder.SeqOrder:
                    paymentUnits = paymentUnits.OrderBy(u => u.SeqOrder);
                    break;
                case PaymentUnitsOrder.Name:
                    paymentUnits = paymentUnits.OrderBy(u => u.Name);
                    break;
            }

            List<PaymentUnitPart> results = paymentUnits
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PaymentUnitsIndexViewModel
            {
                PaymentUnits = results
                    .Select(x => new PaymentUnitEntry {PaymentUnit = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var viewModel = new PaymentUnitsIndexViewModel
            {
                PaymentUnits = new List<PaymentUnitEntry>(),
                Options = new PaymentUnitIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PaymentUnitEntry> checkedEntries = viewModel.PaymentUnits.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PaymentUnitsBulkAction.None:
                    break;
                case PaymentUnitsBulkAction.Enable:
                    foreach (PaymentUnitEntry entry in checkedEntries)
                    {
                        Enable(entry.PaymentUnit.Id);
                    }
                    break;
                case PaymentUnitsBulkAction.Disable:
                    foreach (PaymentUnitEntry entry in checkedEntries)
                    {
                        Disable(entry.PaymentUnit.Id);
                    }
                    break;
                case PaymentUnitsBulkAction.Delete:
                    foreach (PaymentUnitEntry entry in checkedEntries)
                    {
                        Delete(entry.PaymentUnit.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var paymentUnit = Services.ContentManager.New<PaymentUnitPart>("PaymentUnit");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentUnit.Create",
                Model: new PaymentUnitCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(paymentUnit);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PaymentUnitCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_paymentUnitService.VerifyPaymentUnitUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePaymentUnitName", T("PaymentUnit with that name already exists."));
                }
            }

            var paymentUnit = Services.ContentManager.New<PaymentUnitPart>("PaymentUnit");
            if (ModelState.IsValid)
            {
                paymentUnit.Name = createModel.Name;
                paymentUnit.ShortName = createModel.ShortName;
                paymentUnit.CssClass = createModel.CssClass;
                paymentUnit.SeqOrder = createModel.SeqOrder;
                paymentUnit.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(paymentUnit);
            }

            dynamic model = Services.ContentManager.UpdateEditor(paymentUnit, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentUnit.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("PaymentUnits_Changed");

            Services.Notifier.Information(T("PaymentUnit {0} created", paymentUnit.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var paymentUnit = Services.ContentManager.Get<PaymentUnitPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentUnit.Edit",
                Model: new PaymentUnitEditViewModel {PaymentUnit = paymentUnit}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(paymentUnit);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var paymentUnit = Services.ContentManager.Get<PaymentUnitPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(paymentUnit, this);

            var editModel = new PaymentUnitEditViewModel {PaymentUnit = paymentUnit};
            if (TryUpdateModel(editModel))
            {
                if (!_paymentUnitService.VerifyPaymentUnitUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePaymentUnitName", T("PaymentUnit with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentUnit.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("PaymentUnits_Changed");

            Services.Notifier.Information(T("PaymentUnit {0} updated", paymentUnit.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var paymentUnit = Services.ContentManager.Get<PaymentUnitPart>(id);

            if (paymentUnit != null)
            {
                paymentUnit.IsEnabled = true;

                _signals.Trigger("PaymentUnits_Changed");

                Services.Notifier.Information(T("PaymentUnit {0} updated", paymentUnit.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var paymentUnit = Services.ContentManager.Get<PaymentUnitPart>(id);

            if (paymentUnit != null)
            {
                paymentUnit.IsEnabled = false;

                _signals.Trigger("PaymentUnits_Changed");

                Services.Notifier.Information(T("PaymentUnit {0} updated", paymentUnit.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentUnits,
                    T("Not authorized to manage paymentUnits")))
                return new HttpUnauthorizedResult();

            var paymentUnit = Services.ContentManager.Get<PaymentUnitPart>(id);

            if (paymentUnit != null)
            {
                Services.ContentManager.Remove(paymentUnit.ContentItem);

                _signals.Trigger("PaymentUnits_Changed");

                Services.Notifier.Information(T("PaymentUnit {0} deleted", paymentUnit.Name));
            }

            return RedirectToAction("Index");
        }
    }
}