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
    public class PropertyTypeGroupAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyTypeGroupService _propertyTypeGroupService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyTypeGroupAdminController(
            IOrchardServices services,
            IPropertyTypeGroupService propertyTypeGroupService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyTypeGroupService = propertyTypeGroupService;
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

        public ActionResult Index(PropertyTypeGroupIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to list propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyTypeGroupIndexOptions();

            IContentQuery<PropertyTypeGroupPart, PropertyTypeGroupPartRecord> propertyTypeGroups =
                Services.ContentManager
                    .Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>();

            switch (options.Filter)
            {
                case PropertyTypeGroupsFilter.All:
                    //propertyTypeGroups = propertyTypeGroups.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyTypeGroups =
                    propertyTypeGroups.Where(
                        u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyTypeGroups.Count());

            switch (options.Order)
            {
                case PropertyTypeGroupsOrder.SeqOrder:
                    propertyTypeGroups = propertyTypeGroups.OrderBy(u => u.SeqOrder);
                    break;
                case PropertyTypeGroupsOrder.Name:
                    propertyTypeGroups = propertyTypeGroups.OrderBy(u => u.Name);
                    break;
            }

            List<PropertyTypeGroupPart> results = propertyTypeGroups
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyTypeGroupsIndexViewModel
            {
                PropertyTypeGroups = results
                    .Select(x => new PropertyTypeGroupEntry {PropertyTypeGroup = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyTypeGroupsIndexViewModel
            {
                PropertyTypeGroups = new List<PropertyTypeGroupEntry>(),
                Options = new PropertyTypeGroupIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyTypeGroupEntry> checkedEntries = viewModel.PropertyTypeGroups.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyTypeGroupsBulkAction.None:
                    break;
                case PropertyTypeGroupsBulkAction.Enable:
                    foreach (PropertyTypeGroupEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyTypeGroup.Id);
                    }
                    break;
                case PropertyTypeGroupsBulkAction.Disable:
                    foreach (PropertyTypeGroupEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyTypeGroup.Id);
                    }
                    break;
                case PropertyTypeGroupsBulkAction.Delete:
                    foreach (PropertyTypeGroupEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyTypeGroup.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var propertyTypeGroup = Services.ContentManager.New<PropertyTypeGroupPart>("PropertyTypeGroup");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeGroup.Create",
                Model: new PropertyTypeGroupCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyTypeGroup);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyTypeGroupCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyTypeGroupService.VerifyPropertyTypeGroupUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyTypeGroupName",
                        T("PropertyTypeGroup with that name already exists."));
                }
            }

            var propertyTypeGroup = Services.ContentManager.New<PropertyTypeGroupPart>("PropertyTypeGroup");
            if (ModelState.IsValid)
            {
                propertyTypeGroup.Name = createModel.Name;
                propertyTypeGroup.ShortName = createModel.ShortName;
                propertyTypeGroup.CssClass = createModel.CssClass;
                propertyTypeGroup.SeqOrder = createModel.SeqOrder;
                propertyTypeGroup.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(propertyTypeGroup);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyTypeGroup, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeGroup.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("TypeGroups_Changed");

            Services.Notifier.Information(T("PropertyTypeGroup {0} created", propertyTypeGroup.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var propertyTypeGroup = Services.ContentManager.Get<PropertyTypeGroupPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeGroup.Edit",
                Model: new PropertyTypeGroupEditViewModel {PropertyTypeGroup = propertyTypeGroup}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyTypeGroup);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var propertyTypeGroup = Services.ContentManager.Get<PropertyTypeGroupPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyTypeGroup, this);

            var editModel = new PropertyTypeGroupEditViewModel {PropertyTypeGroup = propertyTypeGroup};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyTypeGroupService.VerifyPropertyTypeGroupUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyTypeGroupName",
                        T("PropertyTypeGroup with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeGroup.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("TypeGroups_Changed");

            Services.Notifier.Information(T("PropertyTypeGroup {0} updated", propertyTypeGroup.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var propertyTypeGroup = Services.ContentManager.Get<PropertyTypeGroupPart>(id);

            if (propertyTypeGroup != null)
            {
                propertyTypeGroup.IsEnabled = true;

                _signals.Trigger("TypeGroups_Changed");

                Services.Notifier.Information(T("PropertyTypeGroup {0} updated", propertyTypeGroup.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var propertyTypeGroup = Services.ContentManager.Get<PropertyTypeGroupPart>(id);

            if (propertyTypeGroup != null)
            {
                propertyTypeGroup.IsEnabled = false;

                _signals.Trigger("TypeGroups_Changed");

                Services.Notifier.Information(T("PropertyTypeGroup {0} updated", propertyTypeGroup.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypeGroups,
                    T("Not authorized to manage propertyTypeGroups")))
                return new HttpUnauthorizedResult();

            var propertyTypeGroup = Services.ContentManager.Get<PropertyTypeGroupPart>(id);

            if (propertyTypeGroup != null)
            {
                Services.ContentManager.Remove(propertyTypeGroup.ContentItem);

                _signals.Trigger("TypeGroups_Changed");

                Services.Notifier.Information(T("PropertyTypeGroup {0} deleted", propertyTypeGroup.Name));
            }

            return RedirectToAction("Index");
        }
    }
}