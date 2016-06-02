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
    public class PropertyFlagAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyFlagService _propertyFlagService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyFlagAdminController(
            IOrchardServices services,
            IPropertyFlagService propertyFlagService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyFlagService = propertyFlagService;
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

        public ActionResult Index(PropertyFlagIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to list propertyFlags")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyFlagIndexOptions();

            IContentQuery<PropertyFlagPart, PropertyFlagPartRecord> propertyFlags = Services.ContentManager
                .Query<PropertyFlagPart, PropertyFlagPartRecord>();

            switch (options.Filter)
            {
                case PropertyFlagsFilter.All:
                    //propertyFlags = propertyFlags.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyFlags =
                    propertyFlags.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyFlags.Count());

            switch (options.Order)
            {
                case PropertyFlagsOrder.SeqOrder:
                    propertyFlags = propertyFlags.OrderBy(u => u.SeqOrder);
                    break;
                case PropertyFlagsOrder.Name:
                    propertyFlags = propertyFlags.OrderBy(u => u.Name);
                    break;
            }

            List<PropertyFlagPart> results = propertyFlags
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyFlagsIndexViewModel
            {
                PropertyFlags = results
                    .Select(x => new PropertyFlagEntry {PropertyFlag = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyFlagsIndexViewModel
            {
                PropertyFlags = new List<PropertyFlagEntry>(),
                Options = new PropertyFlagIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyFlagEntry> checkedEntries = viewModel.PropertyFlags.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyFlagsBulkAction.None:
                    break;
                case PropertyFlagsBulkAction.Enable:
                    foreach (PropertyFlagEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyFlag.Id);
                    }
                    break;
                case PropertyFlagsBulkAction.Disable:
                    foreach (PropertyFlagEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyFlag.Id);
                    }
                    break;
                case PropertyFlagsBulkAction.Delete:
                    foreach (PropertyFlagEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyFlag.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var propertyFlag = Services.ContentManager.New<PropertyFlagPart>("PropertyFlag");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyFlag.Create",
                Model: new PropertyFlagCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyFlag);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyFlagCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyFlagService.VerifyPropertyFlagUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyFlagName", T("PropertyFlag with that name already exists."));
                }
            }

            var propertyFlag = Services.ContentManager.New<PropertyFlagPart>("PropertyFlag");
            if (ModelState.IsValid)
            {
                propertyFlag.Name = createModel.Name;
                propertyFlag.ShortName = createModel.ShortName;
                propertyFlag.Value = createModel.Value;
                propertyFlag.CssClass = createModel.CssClass;
                propertyFlag.SeqOrder = createModel.SeqOrder;
                propertyFlag.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(propertyFlag);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyFlag, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyFlag.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Flags_Changed");

            Services.Notifier.Information(T("PropertyFlag {0} created", propertyFlag.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var propertyFlag = Services.ContentManager.Get<PropertyFlagPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyFlag.Edit",
                Model: new PropertyFlagEditViewModel {PropertyFlag = propertyFlag}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyFlag);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var propertyFlag = Services.ContentManager.Get<PropertyFlagPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyFlag, this);

            var editModel = new PropertyFlagEditViewModel {PropertyFlag = propertyFlag};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyFlagService.VerifyPropertyFlagUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyFlagName", T("PropertyFlag with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyFlag.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Flags_Changed");

            Services.Notifier.Information(T("PropertyFlag {0} updated", propertyFlag.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var propertyFlag = Services.ContentManager.Get<PropertyFlagPart>(id);

            if (propertyFlag != null)
            {
                propertyFlag.IsEnabled = true;

                _signals.Trigger("Flags_Changed");

                Services.Notifier.Information(T("PropertyFlag {0} updated", propertyFlag.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var propertyFlag = Services.ContentManager.Get<PropertyFlagPart>(id);

            if (propertyFlag != null)
            {
                propertyFlag.IsEnabled = false;

                _signals.Trigger("Flags_Changed");

                Services.Notifier.Information(T("PropertyFlag {0} updated", propertyFlag.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyFlags,
                    T("Not authorized to manage propertyFlags")))
                return new HttpUnauthorizedResult();

            var propertyFlag = Services.ContentManager.Get<PropertyFlagPart>(id);

            if (propertyFlag != null)
            {
                Services.ContentManager.Remove(propertyFlag.ContentItem);

                _signals.Trigger("Flags_Changed");

                Services.Notifier.Information(T("PropertyFlag {0} deleted", propertyFlag.Name));
            }

            return RedirectToAction("Index");
        }
    }
}