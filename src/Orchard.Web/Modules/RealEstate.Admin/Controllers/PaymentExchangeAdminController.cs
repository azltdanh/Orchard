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
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class PaymentExchangeAdminController : Controller, IUpdateModel
    {
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PaymentExchangeAdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
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

        public ActionResult Index(PaymentExchangeIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to list paymentExchanges")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PaymentExchangeIndexOptions();

            IContentQuery<PaymentExchangePart, PaymentExchangePartRecord> paymentExchanges = Services.ContentManager
                .Query<PaymentExchangePart, PaymentExchangePartRecord>();

            switch (options.Filter)
            {
                case PaymentExchangesFilter.All:
                    //paymentExchanges = paymentExchanges.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }
            /*
            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                paymentExchanges = paymentExchanges.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }
            */
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(paymentExchanges.Count());

            paymentExchanges = paymentExchanges.OrderByDescending(u => u.Date);

            /*
            switch (options.Order)
            {
                case PaymentExchangesOrder.SeqOrder:
                    paymentExchanges = paymentExchanges.OrderBy(u => u.SeqOrder);
                    break;
                case PaymentExchangesOrder.Name:
                    paymentExchanges = paymentExchanges.OrderBy(u => u.Name);
                    break;
            }
             * */

            List<PaymentExchangePart> results = paymentExchanges
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PaymentExchangesIndexViewModel
            {
                PaymentExchanges = results
                    .Select(x => new PaymentExchangeEntry {PaymentExchange = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to manage paymentExchanges")))
                return new HttpUnauthorizedResult();

            var viewModel = new PaymentExchangesIndexViewModel
            {
                PaymentExchanges = new List<PaymentExchangeEntry>(),
                Options = new PaymentExchangeIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PaymentExchangeEntry> checkedEntries = viewModel.PaymentExchanges.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PaymentExchangesBulkAction.None:
                    break;
                case PaymentExchangesBulkAction.Delete:
                    foreach (PaymentExchangeEntry entry in checkedEntries)
                    {
                        Delete(entry.PaymentExchange.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to manage paymentExchanges")))
                return new HttpUnauthorizedResult();

            var paymentExchange = Services.ContentManager.New<PaymentExchangePart>("PaymentExchange");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentExchange.Create",
                Model: new PaymentExchangeCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(paymentExchange);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PaymentExchangeCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to manage paymentExchanges")))
                return new HttpUnauthorizedResult();
            /*
            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_paymentExchangeService.VerifyPaymentExchangeUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePaymentExchangeName", T("PaymentExchange with that name already exists."));
                }
            }
            */
            var paymentExchange = Services.ContentManager.New<PaymentExchangePart>("PaymentExchange");
            if (ModelState.IsValid)
            {
                paymentExchange.USD = createModel.USD;
                paymentExchange.SJC = createModel.SJC;
                paymentExchange.Date = createModel.Date;

                Services.ContentManager.Create(paymentExchange);
            }

            dynamic model = Services.ContentManager.UpdateEditor(paymentExchange, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentExchange.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("PaymentExchanges_Changed");

            Services.Notifier.Information(T("PaymentExchange {0} created", paymentExchange.Date));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to manage paymentExchanges")))
                return new HttpUnauthorizedResult();

            var paymentExchange = Services.ContentManager.Get<PaymentExchangePart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentExchange.Edit",
                Model: new PaymentExchangeEditViewModel {PaymentExchange = paymentExchange}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(paymentExchange);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to manage paymentExchanges")))
                return new HttpUnauthorizedResult();

            var paymentExchange = Services.ContentManager.Get<PaymentExchangePart>(id);
            //string previousName = paymentExchange.Name;

            dynamic model = Services.ContentManager.UpdateEditor(paymentExchange, this);

            var editModel = new PaymentExchangeEditViewModel {PaymentExchange = paymentExchange};
            if (TryUpdateModel(editModel))
            {
                /*
                if (!_paymentExchangeService.VerifyPaymentExchangeUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePaymentExchangeName", T("PaymentExchange with that name already exists."));
                }
                 * */
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentExchange.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("PaymentExchanges_Changed");

            Services.Notifier.Information(T("PaymentExchange {0} updated", paymentExchange.Date));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePaymentExchanges,
                    T("Not authorized to manage paymentExchanges")))
                return new HttpUnauthorizedResult();

            var paymentExchange = Services.ContentManager.Get<PaymentExchangePart>(id);

            if (paymentExchange != null)
            {
                Services.ContentManager.Remove(paymentExchange.ContentItem);

                _signals.Trigger("PaymentExchanges_Changed");

                Services.Notifier.Information(T("PaymentExchange {0} deleted", paymentExchange.Date));
            }

            return RedirectToAction("Index");
        }
    }
}