using System;
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
    public class PropertySettingAdminController : Controller, IUpdateModel
    {
        private readonly IPropertySettingService _propertySettingService;
        private readonly ISiteService _siteService;

        public PropertySettingAdminController(
            IOrchardServices services,
            IPropertySettingService propertySettingService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _propertySettingService = propertySettingService;
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

        public ActionResult Index(PropertySettingIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to list propertySettings")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new PropertySettingIndexOptions();

            IContentQuery<PropertySettingPart, PropertySettingPartRecord> propertySettings = Services.ContentManager
                .Query<PropertySettingPart, PropertySettingPartRecord>();

            switch (options.Filter)
            {
                case PropertySettingsFilter.All:
                    //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                propertySettings =
                    propertySettings.Where(u => u.Name.Contains(options.Search) || u.Value.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(propertySettings.Count());

            List<PropertySettingPart> results = propertySettings
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertySettingsIndexViewModel
            {
                PropertySettings = results
                    .Select(x => new PropertySettingEntry {PropertySetting = x.Record})
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
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage propertySettings")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertySettingsIndexViewModel
            {
                PropertySettings = new List<PropertySettingEntry>(),
                Options = new PropertySettingIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<PropertySettingEntry> checkedEntries = viewModel.PropertySettings.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case PropertySettingsBulkAction.None:
                    break;
                case PropertySettingsBulkAction.Enable:
                    //foreach (PropertySettingEntry entry in checkedEntries)
                    //{
                    //    //Enable(entry.PropertySetting.Id);
                    //}
                    break;
                case PropertySettingsBulkAction.Disable:
                    //foreach (PropertySettingEntry entry in checkedEntries)
                    //{
                    //    //Disable(entry.PropertySetting.Id);
                    //}
                    break;
                case PropertySettingsBulkAction.Delete:
                    foreach (PropertySettingEntry entry in checkedEntries)
                    {
                        Delete(entry.PropertySetting.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage propertySettings")))
                return new HttpUnauthorizedResult();

            var propertySetting = Services.ContentManager.New<PropertySettingPart>("PropertySetting");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertySetting.Create",
                Model: new PropertySettingCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertySetting);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertySettingCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage propertySettings")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertySettingService.VerifyPropertySettingUnicity(createModel.Name))
                {
                    AddModelError("NotUniquePropertySettingName", T("PropertySetting with that name already exists."));
                }
            }

            var propertySetting = Services.ContentManager.New<PropertySettingPart>("PropertySetting");
            if (ModelState.IsValid)
            {
                propertySetting.Name = createModel.Name;
                propertySetting.Value = createModel.Value;

                Services.ContentManager.Create(propertySetting);
            }

            dynamic model = Services.ContentManager.UpdateEditor(propertySetting, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertySetting.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("PropertySetting {0} created", propertySetting.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage propertySettings")))
                return new HttpUnauthorizedResult();

            var propertySetting = Services.ContentManager.Get<PropertySettingPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertySetting.Edit",
                Model: new PropertySettingEditViewModel {PropertySetting = propertySetting}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(propertySetting);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage propertySettings")))
                return new HttpUnauthorizedResult();

            var propertySetting = Services.ContentManager.Get<PropertySettingPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(propertySetting, this);

            var editModel = new PropertySettingEditViewModel {PropertySetting = propertySetting};
            if (TryUpdateModel(editModel))
            {
                if (!_propertySettingService.VerifyPropertySettingUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniquePropertySettingName", T("PropertySetting with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertySetting.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("PropertySetting {0} updated", propertySetting.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage propertySettings")))
                return new HttpUnauthorizedResult();

            var propertySetting = Services.ContentManager.Get<PropertySettingPart>(id);

            if (propertySetting != null)
            {
                Services.ContentManager.Remove(propertySetting.ContentItem);
                Services.Notifier.Information(T("PropertySetting {0} deleted", propertySetting.Name));
            }

            return RedirectToAction("Index");
        }
    }
}