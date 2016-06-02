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
    public class CoefficientLengthAdminController : Controller, IUpdateModel
    {
        private readonly ICoefficientLengthService _coefficientLengthService;
        private readonly ISiteService _siteService;

        public CoefficientLengthAdminController(
            IOrchardServices services,
            ICoefficientLengthService coefficientLengthService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _coefficientLengthService = coefficientLengthService;
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

        public ActionResult Index(CoefficientLengthIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to list coefficientLengths")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CoefficientLengthIndexOptions();

            IContentQuery<CoefficientLengthPart, CoefficientLengthPartRecord> coefficientLengths =
                Services.ContentManager
                    .Query<CoefficientLengthPart, CoefficientLengthPartRecord>();

            switch (options.Filter)
            {
                case CoefficientLengthsFilter.All:
                    //coefficientLengths = coefficientLengths.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }
            /*
            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                coefficientLengths = coefficientLengths.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }
            */
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(coefficientLengths.Count());

            coefficientLengths = coefficientLengths.OrderBy(u => u.WidthRange);

            /*
            switch (options.Order)
            {
                case CoefficientLengthsOrder.SeqOrder:
                    coefficientLengths = coefficientLengths.OrderBy(u => u.SeqOrder);
                    break;
                case CoefficientLengthsOrder.Name:
                    coefficientLengths = coefficientLengths.OrderBy(u => u.Name);
                    break;
            }
             * */

            List<CoefficientLengthPart> results = coefficientLengths
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CoefficientLengthsIndexViewModel
            {
                CoefficientLengths = results
                    .Select(x => new CoefficientLengthEntry {CoefficientLength = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to manage coefficientLengths")))
                return new HttpUnauthorizedResult();

            var viewModel = new CoefficientLengthsIndexViewModel
            {
                CoefficientLengths = new List<CoefficientLengthEntry>(),
                Options = new CoefficientLengthIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CoefficientLengthEntry> checkedEntries = viewModel.CoefficientLengths.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CoefficientLengthsBulkAction.None:
                    break;
                case CoefficientLengthsBulkAction.Delete:
                    foreach (CoefficientLengthEntry entry in checkedEntries)
                    {
                        Delete(entry.CoefficientLength.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to manage coefficientLengths")))
                return new HttpUnauthorizedResult();

            var coefficientLength = Services.ContentManager.New<CoefficientLengthPart>("CoefficientLength");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientLength.Create",
                Model: new CoefficientLengthCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientLength);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CoefficientLengthCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to manage coefficientLengths")))
                return new HttpUnauthorizedResult();

            //if (!string.IsNullOrEmpty(createModel.Name))
            //{
            if (!_coefficientLengthService.VerifyCoefficientLengthUnicity(createModel.WidthRange))
            {
                AddModelError("NotUniqueCoefficientLengthName", T("CoefficientLength with that value already exists."));
            }
            //}

            var coefficientLength = Services.ContentManager.New<CoefficientLengthPart>("CoefficientLength");
            if (ModelState.IsValid)
            {
                coefficientLength.WidthRange = createModel.WidthRange;
                coefficientLength.MinLength = createModel.MinLength;
                coefficientLength.MaxLength = createModel.MaxLength;

                Services.ContentManager.Create(coefficientLength);
            }

            dynamic model = Services.ContentManager.UpdateEditor(coefficientLength, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientLength.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientLength {0} created", coefficientLength.WidthRange));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to manage coefficientLengths")))
                return new HttpUnauthorizedResult();

            var coefficientLength = Services.ContentManager.Get<CoefficientLengthPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientLength.Edit",
                Model: new CoefficientLengthEditViewModel {CoefficientLength = coefficientLength}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientLength);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to manage coefficientLengths")))
                return new HttpUnauthorizedResult();

            var coefficientLength = Services.ContentManager.Get<CoefficientLengthPart>(id);
            //string previousName = coefficientLength.Name;

            dynamic model = Services.ContentManager.UpdateEditor(coefficientLength, this);

            var editModel = new CoefficientLengthEditViewModel {CoefficientLength = coefficientLength};
            if (TryUpdateModel(editModel))
            {
                if (!_coefficientLengthService.VerifyCoefficientLengthUnicity(id, editModel.WidthRange))
                {
                    AddModelError("NotUniqueCoefficientLengthName",
                        T("CoefficientLength with that value already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientLength.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientLength {0} updated", coefficientLength.WidthRange));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageCoefficientLengths,
                    T("Not authorized to manage coefficientLengths")))
                return new HttpUnauthorizedResult();

            var coefficientLength = Services.ContentManager.Get<CoefficientLengthPart>(id);

            if (coefficientLength != null)
            {
                Services.ContentManager.Remove(coefficientLength.ContentItem);
                Services.Notifier.Information(T("CoefficientLength {0} deleted", coefficientLength.WidthRange));
            }

            return RedirectToAction("Index");
        }
    }
}