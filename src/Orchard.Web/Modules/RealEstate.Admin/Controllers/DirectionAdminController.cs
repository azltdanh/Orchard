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
    public class DirectionAdminController : Controller, IUpdateModel
    {
        private readonly IDirectionService _directionService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public DirectionAdminController(
            IOrchardServices services,
            IDirectionService directionService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _directionService = directionService;
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

        public ActionResult Index(DirectionIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to list directions")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new DirectionIndexOptions();

            IContentQuery<DirectionPart, DirectionPartRecord> directions = Services.ContentManager
                .Query<DirectionPart, DirectionPartRecord>();

            switch (options.Filter)
            {
                case DirectionsFilter.All:
                    //directions = directions.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                directions =
                    directions.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(directions.Count());

            switch (options.Order)
            {
                case DirectionsOrder.SeqOrder:
                    directions = directions.OrderBy(u => u.SeqOrder);
                    break;
                case DirectionsOrder.Name:
                    directions = directions.OrderBy(u => u.Name);
                    break;
            }

            List<DirectionPart> results = directions
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new DirectionsIndexViewModel
            {
                Directions = results
                    .Select(r => new DirectionEntry {Direction = r})
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
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var viewModel = new DirectionsIndexViewModel
            {
                Directions = new List<DirectionEntry>(),
                Options = new DirectionIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<DirectionEntry> checkedEntries = viewModel.Directions.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case DirectionsBulkAction.None:
                    break;
                case DirectionsBulkAction.Enable:
                    foreach (DirectionEntry entry in checkedEntries)
                    {
                        Enable(entry.Direction.Id);
                    }
                    break;
                case DirectionsBulkAction.Disable:
                    foreach (DirectionEntry entry in checkedEntries)
                    {
                        Disable(entry.Direction.Id);
                    }
                    break;
                case DirectionsBulkAction.Delete:
                    foreach (DirectionEntry entry in checkedEntries)
                    {
                        Delete(entry.Direction.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var direction = Services.ContentManager.New<DirectionPart>("Direction");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Direction.Create",
                Model: new DirectionCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(direction);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(DirectionCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_directionService.VerifyDirectionUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueDirectionName", T("Direction with that name already exists."));
                }
            }

            var direction = Services.ContentManager.New<DirectionPart>("Direction");
            if (ModelState.IsValid)
            {
                direction.Name = createModel.Name;
                direction.ShortName = createModel.ShortName;
                direction.CssClass = createModel.CssClass;
                direction.SeqOrder = createModel.SeqOrder;
                direction.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(direction);
            }

            dynamic model = Services.ContentManager.UpdateEditor(direction, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Direction.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Directions_Changed");

            Services.Notifier.Information(T("Direction {0} created", direction.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var direction = Services.ContentManager.Get<DirectionPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Direction.Edit",
                Model: new DirectionEditViewModel {Direction = direction}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(direction);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var direction = Services.ContentManager.Get<DirectionPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(direction, this);

            var editModel = new DirectionEditViewModel {Direction = direction};
            if (TryUpdateModel(editModel))
            {
                if (!_directionService.VerifyDirectionUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueDirectionName", T("Direction with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Direction.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Directions_Changed");

            Services.Notifier.Information(T("Direction {0} updated", direction.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var direction = Services.ContentManager.Get<DirectionPart>(id);

            if (direction != null)
            {
                direction.IsEnabled = true;

                _signals.Trigger("Directions_Changed");

                Services.Notifier.Information(T("Direction {0} updated", direction.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var direction = Services.ContentManager.Get<DirectionPart>(id);

            if (direction != null)
            {
                direction.IsEnabled = false;

                _signals.Trigger("Directions_Changed");

                Services.Notifier.Information(T("Direction {0} updated", direction.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageDirections, T("Not authorized to manage directions")))
                return new HttpUnauthorizedResult();

            var direction = Services.ContentManager.Get<DirectionPart>(id);

            if (direction != null)
            {
                Services.ContentManager.Remove(direction.ContentItem);

                _signals.Trigger("Directions_Changed");

                Services.Notifier.Information(T("Direction {0} deleted", direction.Name));
            }

            return RedirectToAction("Index");
        }
    }
}