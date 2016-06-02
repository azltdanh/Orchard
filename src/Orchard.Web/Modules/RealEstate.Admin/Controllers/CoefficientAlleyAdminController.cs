using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
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
    public class CoefficientAlleyAdminController : Controller, IUpdateModel
    {
        private readonly ICoefficientAlleyService _coefficientAlleyService;
        private readonly ISiteService _siteService;

        public CoefficientAlleyAdminController(
            IOrchardServices services,
            ICoefficientAlleyService coefficientAlleyService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _coefficientAlleyService = coefficientAlleyService;
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

        public ActionResult Index(CoefficientAlleyIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to list coefficientAlleys")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CoefficientAlleyIndexOptions();

            IContentQuery<CoefficientAlleyPart, CoefficientAlleyPartRecord> coefficientAlleys = Services.ContentManager
                .Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>();

            switch (options.Filter)
            {
                case CoefficientAlleysFilter.All:
                    //coefficientAlleys = coefficientAlleys.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }
            /*
            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                coefficientAlleys = coefficientAlleys.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }
            */
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(coefficientAlleys.Count());

            coefficientAlleys = coefficientAlleys.OrderBy(u => u.StreetUnitPrice);

            /*
            switch (options.Order)
            {
                case CoefficientAlleysOrder.SeqOrder:
                    coefficientAlleys = coefficientAlleys.OrderBy(u => u.SeqOrder);
                    break;
                case CoefficientAlleysOrder.Name:
                    coefficientAlleys = coefficientAlleys.OrderBy(u => u.Name);
                    break;
            }
             * */

            List<CoefficientAlleyPart> results = coefficientAlleys
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CoefficientAlleysIndexViewModel
            {
                CoefficientAlleys = results
                    .Select(x => new CoefficientAlleyEntry {CoefficientAlley = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to manage coefficientAlleys")))
                return new HttpUnauthorizedResult();

            var viewModel = new CoefficientAlleysIndexViewModel
            {
                CoefficientAlleys = new List<CoefficientAlleyEntry>(),
                Options = new CoefficientAlleyIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CoefficientAlleyEntry> checkedEntries = viewModel.CoefficientAlleys.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CoefficientAlleysBulkAction.None:
                    break;
                case CoefficientAlleysBulkAction.Delete:
                    foreach (CoefficientAlleyEntry entry in checkedEntries)
                    {
                        Delete(entry.CoefficientAlley.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to manage coefficientAlleys")))
                return new HttpUnauthorizedResult();

            var coefficientAlley = Services.ContentManager.New<CoefficientAlleyPart>("CoefficientAlley");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlley.Create",
                Model: new CoefficientAlleyCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientAlley);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CoefficientAlleyCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to manage coefficientAlleys")))
                return new HttpUnauthorizedResult();

            //if (!string.IsNullOrEmpty(createModel.Name))
            //{
            if (!_coefficientAlleyService.VerifyCoefficientAlleyUnicity(createModel.StreetUnitPrice))
            {
                AddModelError("NotUniqueCoefficientAlleyName", T("CoefficientAlley with that value already exists."));
            }
            //}

            var coefficientAlley = Services.ContentManager.New<CoefficientAlleyPart>("CoefficientAlley");
            if (ModelState.IsValid)
            {
                coefficientAlley.StreetUnitPrice = createModel.StreetUnitPrice;
                coefficientAlley.CoefficientAlley1Max = createModel.CoefficientAlley1Max;
                coefficientAlley.CoefficientAlley1Min = createModel.CoefficientAlley1Min;
                coefficientAlley.CoefficientAlleyMax = createModel.CoefficientAlleyMax;
                coefficientAlley.CoefficientAlleyMin = createModel.CoefficientAlleyMin;
                coefficientAlley.CoefficientAlleyEqual = createModel.CoefficientAlleyEqual;

                Services.ContentManager.Create(coefficientAlley);
            }

            dynamic model = Services.ContentManager.UpdateEditor(coefficientAlley, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlley.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientAlley {0} created", coefficientAlley.StreetUnitPrice));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to manage coefficientAlleys")))
                return new HttpUnauthorizedResult();

            var coefficientAlley = Services.ContentManager.Get<CoefficientAlleyPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlley.Edit",
                Model: new CoefficientAlleyEditViewModel {CoefficientAlley = coefficientAlley}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientAlley);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to manage coefficientAlleys")))
                return new HttpUnauthorizedResult();

            var coefficientAlley = Services.ContentManager.Get<CoefficientAlleyPart>(id);
            //string previousName = coefficientAlley.Name;

            dynamic model = Services.ContentManager.UpdateEditor(coefficientAlley, this);

            var editModel = new CoefficientAlleyEditViewModel {CoefficientAlley = coefficientAlley};
            if (TryUpdateModel(editModel))
            {
                if (!_coefficientAlleyService.VerifyCoefficientAlleyUnicity(id, editModel.StreetUnitPrice))
                {
                    AddModelError("NotUniqueCoefficientAlleyName", T("CoefficientAlley with that value already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlley.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientAlley {0} updated", coefficientAlley.StreetUnitPrice));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleys,
                    T("Not authorized to manage coefficientAlleys")))
                return new HttpUnauthorizedResult();

            var coefficientAlley = Services.ContentManager.Get<CoefficientAlleyPart>(id);

            if (coefficientAlley != null)
            {
                Services.ContentManager.Remove(coefficientAlley.ContentItem);
                Services.Notifier.Information(T("CoefficientAlley {0} deleted", coefficientAlley.StreetUnitPrice));
            }

            return RedirectToAction("Index");
        }
    }
}