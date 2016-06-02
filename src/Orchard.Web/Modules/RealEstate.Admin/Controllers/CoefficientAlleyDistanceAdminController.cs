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
    public class CoefficientAlleyDistanceAdminController : Controller, IUpdateModel
    {
        private readonly ICoefficientAlleyDistanceService _coefficientAlleyDistanceService;
        private readonly ISiteService _siteService;

        public CoefficientAlleyDistanceAdminController(
            IOrchardServices services,
            ICoefficientAlleyDistanceService coefficientAlleyDistanceService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _coefficientAlleyDistanceService = coefficientAlleyDistanceService;
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

        public ActionResult Index(CoefficientAlleyDistanceIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to list coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CoefficientAlleyDistanceIndexOptions();

            IContentQuery<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord> coefficientAlleyDistances =
                Services.ContentManager
                    .Query<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord>();

            switch (options.Filter)
            {
                case CoefficientAlleyDistancesFilter.All:
                    //coefficientAlleyDistances = coefficientAlleyDistances.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }
            /*
            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                coefficientAlleyDistances = coefficientAlleyDistances.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }
            */
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(coefficientAlleyDistances.Count());

            coefficientAlleyDistances = coefficientAlleyDistances.OrderBy(u => u.LastAlleyWidth);

            /*
            switch (options.Order)
            {
                case CoefficientAlleyDistancesOrder.SeqOrder:
                    coefficientAlleyDistances = coefficientAlleyDistances.OrderBy(u => u.SeqOrder);
                    break;
                case CoefficientAlleyDistancesOrder.Name:
                    coefficientAlleyDistances = coefficientAlleyDistances.OrderBy(u => u.Name);
                    break;
            }
             * */

            List<CoefficientAlleyDistancePart> results = coefficientAlleyDistances
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CoefficientAlleyDistancesIndexViewModel
            {
                CoefficientAlleyDistances = results
                    .Select(x => new CoefficientAlleyDistanceEntry {CoefficientAlleyDistance = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to manage coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            var viewModel = new CoefficientAlleyDistancesIndexViewModel
            {
                CoefficientAlleyDistances = new List<CoefficientAlleyDistanceEntry>(),
                Options = new CoefficientAlleyDistanceIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CoefficientAlleyDistanceEntry> checkedEntries =
                viewModel.CoefficientAlleyDistances.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CoefficientAlleyDistancesBulkAction.None:
                    break;
                case CoefficientAlleyDistancesBulkAction.Delete:
                    foreach (CoefficientAlleyDistanceEntry entry in checkedEntries)
                    {
                        Delete(entry.CoefficientAlleyDistance.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to manage coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            var coefficientAlleyDistance =
                Services.ContentManager.New<CoefficientAlleyDistancePart>("CoefficientAlleyDistance");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlleyDistance.Create",
                Model: new CoefficientAlleyDistanceCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientAlleyDistance);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CoefficientAlleyDistanceCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to manage coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            //if (!string.IsNullOrEmpty(createModel.Name))
            //{
            if (!_coefficientAlleyDistanceService.VerifyCoefficientAlleyDistanceUnicity(createModel.LastAlleyWidth))
            {
                AddModelError("NotUniqueCoefficientAlleyDistanceName",
                    T("CoefficientAlleyDistance with that value already exists."));
            }
            //}

            var coefficientAlleyDistance =
                Services.ContentManager.New<CoefficientAlleyDistancePart>("CoefficientAlleyDistance");
            if (ModelState.IsValid)
            {
                coefficientAlleyDistance.LastAlleyWidth = createModel.LastAlleyWidth;
                coefficientAlleyDistance.MaxAlleyDistance = createModel.MaxAlleyDistance;
                coefficientAlleyDistance.CoefficientDistance = createModel.CoefficientDistance;

                Services.ContentManager.Create(coefficientAlleyDistance);
            }

            dynamic model = Services.ContentManager.UpdateEditor(coefficientAlleyDistance, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlleyDistance.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientAlleyDistance {0} created",
                coefficientAlleyDistance.LastAlleyWidth));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to manage coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            var coefficientAlleyDistance = Services.ContentManager.Get<CoefficientAlleyDistancePart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlleyDistance.Edit",
                Model: new CoefficientAlleyDistanceEditViewModel {CoefficientAlleyDistance = coefficientAlleyDistance},
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientAlleyDistance);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to manage coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            var coefficientAlleyDistance = Services.ContentManager.Get<CoefficientAlleyDistancePart>(id);
            //string previousName = coefficientAlleyDistance.Name;

            dynamic model = Services.ContentManager.UpdateEditor(coefficientAlleyDistance, this);

            var editModel = new CoefficientAlleyDistanceEditViewModel
            {
                CoefficientAlleyDistance = coefficientAlleyDistance
            };
            if (TryUpdateModel(editModel))
            {
                if (
                    !_coefficientAlleyDistanceService.VerifyCoefficientAlleyDistanceUnicity(id, editModel.LastAlleyWidth))
                {
                    AddModelError("NotUniqueCoefficientAlleyDistanceName",
                        T("CoefficientAlleyDistance with that value already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientAlleyDistance.Edit",
                    Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientAlleyDistance {0} updated",
                coefficientAlleyDistance.LastAlleyWidth));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientAlleyDistances,
                    T("Not authorized to manage coefficientAlleyDistances")))
                return new HttpUnauthorizedResult();

            var coefficientAlleyDistance = Services.ContentManager.Get<CoefficientAlleyDistancePart>(id);

            if (coefficientAlleyDistance != null)
            {
                Services.ContentManager.Remove(coefficientAlleyDistance.ContentItem);
                Services.Notifier.Information(T("CoefficientAlleyDistance {0} deleted",
                    coefficientAlleyDistance.LastAlleyWidth));
            }

            return RedirectToAction("Index");
        }
    }
}