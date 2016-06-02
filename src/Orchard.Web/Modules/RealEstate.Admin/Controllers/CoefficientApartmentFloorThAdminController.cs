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
    public class CoefficientApartmentFloorThAdminController : Controller, IUpdateModel
    {
        private readonly ICoefficientApartmentFloorThService _coefficientApartmentFloorThService;
        private readonly ISiteService _siteService;

        public CoefficientApartmentFloorThAdminController(
            IOrchardServices services,
            ICoefficientApartmentFloorThService coefficientApartmentFloorThService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _coefficientApartmentFloorThService = coefficientApartmentFloorThService;
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

        public ActionResult Index(CoefficientApartmentFloorThIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to list CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new CoefficientApartmentFloorThIndexOptions();

            IContentQuery<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>
                coefficientApartmentFloorTh = Services.ContentManager
                    .Query<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>();

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(coefficientApartmentFloorTh.Count());

            coefficientApartmentFloorTh =
                coefficientApartmentFloorTh.OrderByDescending(u => u.MaxFloors).OrderBy(u => u.ApartmentFloorTh);

            List<CoefficientApartmentFloorThPart> results = coefficientApartmentFloorTh
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CoefficientApartmentFloorThIndexViewModel
            {
                CoefficientApartmentFloorThs = results
                    .Select(x => new CoefficientApartmentFloorThEntry {CoefficientApartmentFloorThPart = x})
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
                    T("Not authorized to manage CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            var viewModel = new CoefficientApartmentFloorThIndexViewModel
            {
                CoefficientApartmentFloorThs = new List<CoefficientApartmentFloorThEntry>(),
                Options = new CoefficientApartmentFloorThIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CoefficientApartmentFloorThEntry> checkedEntries =
                viewModel.CoefficientApartmentFloorThs.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CoefficientApartmentFloorThBulkAction.None:
                    break;
                case CoefficientApartmentFloorThBulkAction.Delete:
                    foreach (CoefficientApartmentFloorThEntry entry in checkedEntries)
                    {
                        Delete(entry.CoefficientApartmentFloorThPart.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorThPart =
                Services.ContentManager.New<CoefficientApartmentFloorThPart>("CoefficientApartmentFloorTh");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloorTh.Create",
                Model: new CoefficientApartmentFloorThCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientApartmentFloorThPart);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CoefficientApartmentFloorThCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            if (
                !_coefficientApartmentFloorThService.VerifyCoefficientApartmentFloorThUnicity(createModel.MaxFloors,
                    createModel.ApartmentFloorTh))
            {
                AddModelError("ApartmentFloorTh",
                    T("CoefficientApartmentFloorTh with that MaxFloors & ApartmentFloorTh already exists."));
            }

            var coefficientApartmentFloorThPart =
                Services.ContentManager.New<CoefficientApartmentFloorThPart>("CoefficientApartmentFloorTh");
            if (ModelState.IsValid)
            {
                coefficientApartmentFloorThPart.MaxFloors = createModel.MaxFloors;
                coefficientApartmentFloorThPart.ApartmentFloorTh = createModel.ApartmentFloorTh;
                coefficientApartmentFloorThPart.CoefficientApartmentFloorTh = createModel.CoefficientApartmentFloorTh;

                Services.ContentManager.Create(coefficientApartmentFloorThPart);
            }

            dynamic model = Services.ContentManager.UpdateEditor(coefficientApartmentFloorThPart, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloorTh.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("MaxFloors {0} CoefficientApartmentFloorTh {1} created",
                coefficientApartmentFloorThPart.MaxFloors, coefficientApartmentFloorThPart.ApartmentFloorTh));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorThPart = Services.ContentManager.Get<CoefficientApartmentFloorThPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloorTh.Edit",
                Model:
                    new CoefficientApartmentFloorThEditViewModel
                    {
                        CoefficientApartmentFloorThPart = coefficientApartmentFloorThPart
                    }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(coefficientApartmentFloorThPart);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorThPart = Services.ContentManager.Get<CoefficientApartmentFloorThPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(coefficientApartmentFloorThPart, this);

            var editModel = new CoefficientApartmentFloorThEditViewModel
            {
                CoefficientApartmentFloorThPart = coefficientApartmentFloorThPart
            };
            if (TryUpdateModel(editModel))
            {
                if (
                    !_coefficientApartmentFloorThService.VerifyCoefficientApartmentFloorThUnicity(id,
                        editModel.MaxFloors, editModel.ApartmentFloorTh))
                {
                    AddModelError("ApartmentFloorTh",
                        T("CoefficientApartmentFloorTh with that MaxFloors & ApartmentFloorTh already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CoefficientApartmentFloorTh.Edit",
                    Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("MaxFloors {0} CoefficientApartmentFloorTh {1} updated",
                coefficientApartmentFloorThPart.MaxFloors, coefficientApartmentFloorThPart.ApartmentFloorTh));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageConfigs,
                    T("Not authorized to manage CoefficientApartmentFloorTh")))
                return new HttpUnauthorizedResult();

            var coefficientApartmentFloorThPart = Services.ContentManager.Get<CoefficientApartmentFloorThPart>(id);

            if (coefficientApartmentFloorThPart != null)
            {
                Services.ContentManager.Remove(coefficientApartmentFloorThPart.ContentItem);
                Services.Notifier.Information(T("MaxFloors {0} CoefficientApartmentFloorTh {1} deleted",
                    coefficientApartmentFloorThPart.MaxFloors, coefficientApartmentFloorThPart.ApartmentFloorTh));
            }

            return RedirectToAction("Index");
        }
    }
}