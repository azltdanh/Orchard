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
    public class PropertyTypeConstructionAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyTypeConstructionService _propertyTypeConstructionService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyTypeConstructionAdminController(
            IOrchardServices services,
            IPropertyTypeConstructionService propertyTypeConstructionService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyTypeConstructionService = propertyTypeConstructionService;
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

        public ActionResult Index(PropertyTypeConstructionIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to list PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyTypeConstructionIndexOptions();

            IContentQuery<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord> list =
                Services.ContentManager
                    .Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>();

            options.PropertyGroups = _propertyTypeConstructionService.GetPropertyGroups();
            options.PropertyTypes = _propertyTypeConstructionService.GetPropertyTypes();

            switch (options.Filter)
            {
                case PropertyTypeConstructionsFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                list = list.Where(r => r.Name.Contains(options.Search) || r.ShortName.Contains(options.Search));
            }

            if (options.PropertyGroupId > 0)
            {
                list = list.Where(r => r.PropertyGroup.Id == options.PropertyGroupId);
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(list.Count());

            switch (options.Order)
            {
                case PropertyTypeConstructionsOrder.SeqOrder:
                    list = list.OrderBy(r => r.PropertyGroup).OrderBy(r => r.PropertyType).OrderBy(r => r.SeqOrder);
                    break;
                case PropertyTypeConstructionsOrder.Name:
                    list = list.OrderBy(r => r.PropertyGroup).OrderBy(r => r.PropertyType).OrderBy(r => r.Name);
                    break;
            }

            List<PropertyTypeConstructionPart> results = list
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyTypeConstructionsIndexViewModel
            {
                PropertyTypeConstructions = results
                    .Select(x => new PropertyTypeConstructionEntry {PropertyTypeConstruction = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);
            routeData.Values.Add("Options.PropertyGroupId", options.PropertyGroupId);
            routeData.Values.Add("Options.PropertyTypeId", options.PropertyTypeId);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyTypeConstructionsIndexViewModel
            {
                PropertyTypeConstructions = new List<PropertyTypeConstructionEntry>(),
                Options = new PropertyTypeConstructionIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyTypeConstructionEntry> checkedEntries =
                viewModel.PropertyTypeConstructions.Where(r => r.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyTypeConstructionsBulkAction.None:
                    break;
                case PropertyTypeConstructionsBulkAction.Enable:
                    foreach (PropertyTypeConstructionEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyTypeConstruction.Id);
                    }
                    break;
                case PropertyTypeConstructionsBulkAction.Disable:
                    foreach (PropertyTypeConstructionEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyTypeConstruction.Id);
                    }
                    break;
                case PropertyTypeConstructionsBulkAction.Delete:
                    foreach (PropertyTypeConstructionEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyTypeConstruction.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var propertyTypeConstruction =
                Services.ContentManager.New<PropertyTypeConstructionPart>("PropertyTypeConstruction");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeConstruction.Create",
                Model: new PropertyTypeConstructionCreateViewModel
                {
                    PropertyGroups = _propertyTypeConstructionService.GetPropertyGroups(),
                    PropertyTypes = _propertyTypeConstructionService.GetPropertyTypes().Where(a => a.Group.Id == 0)
                }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyTypeConstruction);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyTypeConstructionCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (
                    !_propertyTypeConstructionService.VerifyPropertyTypeConstructionUnicity(createModel.Name,
                        createModel.PropertyTypeId))
                {
                    AddModelError("NotUniquePropertyTypeConstructionName",
                        T("PropertyTypeConstruction with that name already exists."));
                }
            }

            var record = Services.ContentManager.New<PropertyTypeConstructionPart>("PropertyTypeConstruction");

            if (ModelState.IsValid)
            {
                PropertyTypePartRecord selectedPropertyType =
                    _propertyTypeConstructionService.GetPropertyTypes()
                        .SingleOrDefault(a => a.Id == createModel.PropertyTypeId);
                record.PropertyGroup = selectedPropertyType.Group;
                record.PropertyType = selectedPropertyType;

                record.Name = createModel.Name;
                record.ShortName = createModel.ShortName;
                record.CssClass = createModel.CssClass;
                record.SeqOrder = createModel.SeqOrder;
                record.IsEnabled = true;
                record.UnitPrice = createModel.UnitPrice;
                record.DefaultImgUrl = createModel.DefaultImgUrl;

                Services.ContentManager.Create(record);

                // Set as Default
                if (createModel.IsDefaultInFloorsRange)
                {
                    _propertyTypeConstructionService.SetAsDefaultInFloorsRange(record);
                }
            }

            dynamic model = Services.ContentManager.UpdateEditor(record, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                createModel.PropertyGroups = _propertyTypeConstructionService.GetPropertyGroups();
                createModel.PropertyTypes =
                    _propertyTypeConstructionService.GetPropertyTypes()
                        .Where(a => a.Group.Id == createModel.PropertyGroupId);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeConstruction.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("TypeConstructions_Changed");

            Services.Notifier.Information(T("PropertyTypeConstruction {0} created", record.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyTypeConstructionPart>(id);
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/PropertyTypeConstruction.Edit",
                Model: new PropertyTypeConstructionEditViewModel
                {
                    PropertyTypeConstruction = record,
                    PropertyGroups = _propertyTypeConstructionService.GetPropertyGroups(),
                    PropertyGroupId = record.PropertyGroup.Id,
                    PropertyTypes =
                        _propertyTypeConstructionService.GetPropertyTypes().Where(a => a.Group == record.PropertyGroup),
                    PropertyTypeId = record.PropertyType.Id
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(record);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyTypeConstructionPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(record, this);

            var editModel = new PropertyTypeConstructionEditViewModel {PropertyTypeConstruction = record};
            if (TryUpdateModel(editModel))
            {
                if (
                    !_propertyTypeConstructionService.VerifyPropertyTypeConstructionUnicity(id, editModel.Name,
                        editModel.PropertyTypeId))
                {
                    AddModelError("NotUniquePropertyTypeConstructionName",
                        T("PropertyTypeConstruction with that name already exists."));
                }
                else
                {
                    PropertyTypePartRecord selectedPropertyType =
                        _propertyTypeConstructionService.GetPropertyTypes()
                            .SingleOrDefault(a => a.Id == editModel.PropertyTypeId);
                    record.PropertyGroup = selectedPropertyType.Group;
                    record.PropertyType = selectedPropertyType;

                    // Set as Default
                    if (editModel.IsDefaultInFloorsRange)
                    {
                        _propertyTypeConstructionService.SetAsDefaultInFloorsRange(record);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.PropertyGroups = _propertyTypeConstructionService.GetPropertyGroups();
                editModel.PropertyTypes =
                    _propertyTypeConstructionService.GetPropertyTypes()
                        .Where(a => a.Group.Id == editModel.PropertyGroupId);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyTypeConstruction.Edit",
                    Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("TypeConstructions_Changed");

            Services.Notifier.Information(T("PropertyTypeConstruction {0} updated", record.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyTypeConstructionPart>(id);

            if (record != null)
            {
                record.IsEnabled = true;

                _signals.Trigger("TypeConstructions_Changed");

                Services.Notifier.Information(T("PropertyTypeConstruction {0} updated", record.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyTypeConstructionPart>(id);

            if (record != null)
            {
                record.IsEnabled = false;

                _signals.Trigger("TypeConstructions_Changed");

                Services.Notifier.Information(T("PropertyTypeConstruction {0} updated", record.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage PropertyTypeConstructions")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyTypeConstructionPart>(id);

            if (record != null)
            {
                Services.ContentManager.Remove(record.ContentItem);

                _signals.Trigger("TypeConstructions_Changed");

                Services.Notifier.Information(T("PropertyTypeConstruction {0} deleted", record.Name));
            }

            return RedirectToAction("Index");
        }
    }
}