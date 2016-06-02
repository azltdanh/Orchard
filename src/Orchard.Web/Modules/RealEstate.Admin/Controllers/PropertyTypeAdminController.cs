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
    public class PropertyTypeAdminController : Controller, IUpdateModel
    {
        private readonly IPropertyTypeService _propertyTypeService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public PropertyTypeAdminController(
            IOrchardServices services,
            IPropertyTypeService propertyTypeService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _propertyTypeService = propertyTypeService;
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

        public ActionResult Index(PropertyTypeIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to list propertyTypes")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertyTypeIndexOptions();

            IContentQuery<PropertyTypePart, PropertyTypePartRecord> propertyTypes = Services.ContentManager
                .Query<PropertyTypePart, PropertyTypePartRecord>();

            options.Groups = _propertyTypeService.GetGroups();

            switch (options.Filter)
            {
                case PropertyTypesFilter.All:
                    //propertyTypes = propertyTypes.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertyTypes =
                    propertyTypes.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            if (options.GroupId > 0)
            {
                propertyTypes = propertyTypes.Where(u => u.Group.Id == options.GroupId);
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertyTypes.Count());

            switch (options.Order)
            {
                case PropertyTypesOrder.SeqOrder:
                    propertyTypes = propertyTypes.OrderBy(u => u.Group).OrderBy(u => u.SeqOrder);
                    break;
                case PropertyTypesOrder.Name:
                    propertyTypes = propertyTypes.OrderBy(u => u.Group).OrderBy(u => u.Name);
                    break;
            }

            List<PropertyTypePart> results = propertyTypes
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyTypesIndexViewModel
            {
                PropertyTypes = results
                    .Select(x => new PropertyTypeEntry {PropertyType = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);
            routeData.Values.Add("Options.GroupId", options.GroupId);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyTypesIndexViewModel
            {
                PropertyTypes = new List<PropertyTypeEntry>(),
                Options = new PropertyTypeIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertyTypeEntry> checkedEntries = viewModel.PropertyTypes.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertyTypesBulkAction.None:
                    break;
                case PropertyTypesBulkAction.Enable:
                    foreach (PropertyTypeEntry entry in checkedEntries)
                    {
                        Enable(entry.PropertyType.Id);
                    }
                    break;
                case PropertyTypesBulkAction.Disable:
                    foreach (PropertyTypeEntry entry in checkedEntries)
                    {
                        Disable(entry.PropertyType.Id);
                    }
                    break;
                case PropertyTypesBulkAction.Delete:
                    foreach (PropertyTypeEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertyType.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var propertyType = Services.ContentManager.New<PropertyTypePart>("PropertyType");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyType.Create",
                Model: new PropertyTypeCreateViewModel {Groups = _propertyTypeService.GetGroups()}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyType);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyTypeCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertyTypeService.VerifyPropertyTypeUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertyTypeName", T("PropertyType with that name already exists."));
                }
            }

            var propertyType = Services.ContentManager.New<PropertyTypePart>("PropertyType");
            if (ModelState.IsValid)
            {
                propertyType.Name = createModel.Name;
                propertyType.ShortName = createModel.ShortName;
                propertyType.CssClass = createModel.CssClass;
                propertyType.SeqOrder = createModel.SeqOrder;
                propertyType.IsEnabled = createModel.IsEnabled;
                propertyType.Group = _propertyTypeService.GetGroups().SingleOrDefault(a => a.Id == createModel.GroupId);
                propertyType.UnitPrice = createModel.UnitPrice;
                propertyType.DefaultImgUrl = createModel.DefaultImgUrl;

                Services.ContentManager.Create(propertyType);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertyType, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                createModel.Groups = _propertyTypeService.GetGroups();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyType.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Types_Changed");

            Services.Notifier.Information(T("PropertyType {0} created", propertyType.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var propertyType = Services.ContentManager.Get<PropertyTypePart>(id);
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/PropertyType.Edit",
                Model: new PropertyTypeEditViewModel
                {
                    PropertyType = propertyType,
                    Groups = _propertyTypeService.GetGroups(),
                    GroupId = propertyType.Group.Id
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertyType);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var propertyType = Services.ContentManager.Get<PropertyTypePart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertyType, this);

            var editModel = new PropertyTypeEditViewModel {PropertyType = propertyType};
            if (TryUpdateModel(editModel))
            {
                if (!_propertyTypeService.VerifyPropertyTypeUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertyTypeName", T("PropertyType with that name already exists."));
                }
                else
                {
                    propertyType.Group = _propertyTypeService.GetGroups()
                        .SingleOrDefault(a => a.Id == editModel.GroupId);
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.Groups = _propertyTypeService.GetGroups();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyType.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Types_Changed");

            Services.Notifier.Information(T("PropertyType {0} updated", propertyType.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var propertyType = Services.ContentManager.Get<PropertyTypePart>(id);

            if (propertyType != null)
            {
                propertyType.IsEnabled = true;

                _signals.Trigger("Types_Changed");

                Services.Notifier.Information(T("PropertyType {0} updated", propertyType.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var propertyType = Services.ContentManager.Get<PropertyTypePart>(id);

            if (propertyType != null)
            {
                propertyType.IsEnabled = false;

                _signals.Trigger("Types_Changed");

                Services.Notifier.Information(T("PropertyType {0} updated", propertyType.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertyTypes,
                    T("Not authorized to manage propertyTypes")))
                return new HttpUnauthorizedResult();

            var propertyType = Services.ContentManager.Get<PropertyTypePart>(id);

            if (propertyType != null)
            {
                Services.ContentManager.Remove(propertyType.ContentItem);

                _signals.Trigger("Types_Changed");

                Services.Notifier.Information(T("PropertyType {0} deleted", propertyType.Name));
            }

            return RedirectToAction("Index");
        }
    }
}