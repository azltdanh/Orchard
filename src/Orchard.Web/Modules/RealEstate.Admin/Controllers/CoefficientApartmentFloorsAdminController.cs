using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
    public class CoefficientApartmentFloorsAdminController : Controller, IUpdateModel
    {
        private readonly ICoefficientApartmentFloorsService _coefficientApartmentFloorsService;
        private readonly ISiteService _siteService;

        public CoefficientApartmentFloorsAdminController(
            IOrchardServices services,
            ICoefficientApartmentFloorsService coefficientApartmentFloorsService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _coefficientApartmentFloorsService = coefficientApartmentFloorsService;
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

        public ActionResult Index(CoefficientApartmentFloorsIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to list CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CoefficientApartmentFloorsIndexOptions();

            IContentQuery<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord>
                coefficientApartmentFloors = Services.ContentManager
                    .Query<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord>();

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(coefficientApartmentFloors.Count());

            coefficientApartmentFloors = coefficientApartmentFloors.OrderBy(u => u.Floors);

            List<CoefficientApartmentFloorsPart> results = coefficientApartmentFloors
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CoefficientApartmentFloorsIndexViewModel
            {
                CoefficientApartmentFloors = results
                    .Select(x => new CoefficientApartmentFloorsEntry {CoefficientApartmentFloorsPart = x})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            var viewModel = new CoefficientApartmentFloorsIndexViewModel
            {
                CoefficientApartmentFloors = new List<CoefficientApartmentFloorsEntry>(),
                Options = new CoefficientApartmentFloorsIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CoefficientApartmentFloorsEntry> checkedEntries =
                viewModel.CoefficientApartmentFloors.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CoefficientApartmentFloorsBulkAction.None:
                    break;
                case CoefficientApartmentFloorsBulkAction.Delete:
                    foreach (CoefficientApartmentFloorsEntry entry in checkedEntries)
                    {
                        Delete(entry.CoefficientApartmentFloorsPart.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorsPart =
                Services.ContentManager.New<CoefficientApartmentFloorsPart>("CoefficientApartmentFloors");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloors.Create",
                Model: new CoefficientApartmentFloorsCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientApartmentFloorsPart);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CoefficientApartmentFloorsCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            if (!_coefficientApartmentFloorsService.VerifyCoefficientApartmentFloorsUnicity(createModel.Floors))
            {
                AddModelError("Floors", T("CoefficientApartmentFloors with that Floors already exists."));
            }

            var coefficientApartmentFloorsPart =
                Services.ContentManager.New<CoefficientApartmentFloorsPart>("CoefficientApartmentFloors");
            if (ModelState.IsValid)
            {
                coefficientApartmentFloorsPart.Floors = createModel.Floors;
                coefficientApartmentFloorsPart.CoefficientApartmentFloors = createModel.CoefficientApartmentFloors;

                Services.ContentManager.Create(coefficientApartmentFloorsPart);
            }

            dynamic model = Services.ContentManager.UpdateEditor(coefficientApartmentFloorsPart, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloors.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientApartmentFloors {0} created",
                coefficientApartmentFloorsPart.Floors));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorsPart = Services.ContentManager.Get<CoefficientApartmentFloorsPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloors.Edit",
                Model:
                    new CoefficientApartmentFloorsEditViewModel
                    {
                        CoefficientApartmentFloorsPart = coefficientApartmentFloorsPart
                    }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientApartmentFloorsPart);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorsPart = Services.ContentManager.Get<CoefficientApartmentFloorsPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(coefficientApartmentFloorsPart, this);

            var editModel = new CoefficientApartmentFloorsEditViewModel
            {
                CoefficientApartmentFloorsPart = coefficientApartmentFloorsPart
            };
            if (TryUpdateModel(editModel))
            {
                if (!_coefficientApartmentFloorsService.VerifyCoefficientApartmentFloorsUnicity(id, editModel.Floors))
                {
                    AddModelError("Floors", T("CoefficientApartmentFloors with that Floors already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloors.Edit",
                    Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("CoefficientApartmentFloors {0} updated",
                coefficientApartmentFloorsPart.Floors));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloors")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorsPart = Services.ContentManager.Get<CoefficientApartmentFloorsPart>(id);

            if (coefficientApartmentFloorsPart != null)
            {
                Services.ContentManager.Remove(coefficientApartmentFloorsPart.ContentItem);
                Services.Notifier.Information(T("CoefficientApartmentFloors {0} deleted",
                    coefficientApartmentFloorsPart.Floors));
            }

            return RedirectToAction("Index");
        }
    }
}