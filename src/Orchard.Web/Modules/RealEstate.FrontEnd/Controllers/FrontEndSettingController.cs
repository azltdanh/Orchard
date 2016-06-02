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
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Services;

namespace RealEstate.FrontEnd.Controllers
{
    [ValidateInput(false), Admin]
    public class FrontEndSettingController : Controller, IUpdateModel
    {
        private readonly IPropertySettingService _propertySettingService;
        //private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public FrontEndSettingController(
            IOrchardServices services,
            IPropertySettingService propertySettingService,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            ISignals signals)
        {
            Services = services;
            _propertySettingService = propertySettingService;
            _siteService = siteService;
            _signals = signals;

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

        public ActionResult Index(FrontEndSettingIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to list frontendSettings")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new FrontEndSettingIndexOptions();

            IContentQuery<FrontEndSettingPart, FrontEndSettingPartRecord> frontendSettings = Services.ContentManager
                .Query<FrontEndSettingPart, FrontEndSettingPartRecord>();
            switch (options.Filter)
            {
                case FrontEndSettingsFilter.All:
                    //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                frontendSettings =
                    frontendSettings.Where(u => u.Name.Contains(options.Search) || u.Value.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(frontendSettings.Count());

            List<FrontEndSettingPart> results = frontendSettings
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();


            var model = new FrontEndSettingsIndexViewModel
            {
                FrontEndSettings = results
                    .Select(x => new FrontEndSettingEntry {FrontEndSetting = x.Record})
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
                    T("Not authorized to manage frontendSettings")))
                return new HttpUnauthorizedResult();

            var viewModel = new FrontEndSettingsIndexViewModel
            {
                FrontEndSettings = new List<FrontEndSettingEntry>(),
                Options = new FrontEndSettingIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<FrontEndSettingEntry> checkedEntries = viewModel.FrontEndSettings.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case FrontEndSettingsBulkAction.None:
                    break;
                case FrontEndSettingsBulkAction.Enable:
                    //foreach (FrontEndSettingEntry entry in checkedEntries)
                    //{
                    //    //Enable(entry.PropertySetting.Id);
                    //}
                    break;
                case FrontEndSettingsBulkAction.Disable:
                    //foreach (FrontEndSettingEntry entry in checkedEntries)
                    //{
                    //    //Disable(entry.PropertySetting.Id);
                    //}
                    break;
                case FrontEndSettingsBulkAction.Delete:
                    foreach (FrontEndSettingEntry entry in checkedEntries)
                    {
                        Delete(entry.FrontEndSetting.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage FrontEndSettings")))
                return new HttpUnauthorizedResult();

            var frontendSetting = Services.ContentManager.New<FrontEndSettingPart>("FrontEndSetting");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/FrontEndSetting.Create",
                Model: new FrontEndSettingCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(frontendSetting);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(FrontEndSettingCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage FrontEndSettings")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_propertySettingService.VerifyPropertySettingUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueFrontEndSettingName", T("FrontEndSetting with that name already exists."));
                }
            }
            var frontendSetting = Services.ContentManager.New<FrontEndSettingPart>("FrontEndSetting");
            if (ModelState.IsValid)
            {
                frontendSetting.Name = createModel.Name;
                frontendSetting.Value = createModel.Value;
                Services.ContentManager.Create(frontendSetting);
            }

            dynamic model = Services.ContentManager.UpdateEditor(frontendSetting, this);

            //Clear cache
            _signals.Trigger("CacheFrontEndSetting_" + frontendSetting.Name + "_Changed");

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/FrontEndSetting.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            //Services.Notifier.Information(T("FrontEndSetting {0} created", frontendSetting.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage FrontEndSettings")))
                return new HttpUnauthorizedResult();

            var frontendSetting = Services.ContentManager.Get<FrontEndSettingPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/FrontEndSetting.Edit",
                Model:
                    new FrontEndSettingEditViewModel
                    {
                        FrontEndSetting = frontendSetting,
                        Name = frontendSetting.Name,
                        Value = frontendSetting.Value
                    }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(frontendSetting);
            model.Content.Add(editor);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage frontendSettings")))
                return new HttpUnauthorizedResult();

            var frontendSetting = Services.ContentManager.Get<FrontEndSettingPart>(id);
            //string previousName = frontendSetting.Name;

            dynamic model = Services.ContentManager.UpdateEditor(frontendSetting, this);

            var editModel = new FrontEndSettingEditViewModel {FrontEndSetting = frontendSetting};
            if (TryUpdateModel(editModel))
            {
                if (!_propertySettingService.VerifyPropertySettingUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueFrontEndSettingName", T("FrontEndSetting with that name already exists."));
                }
                frontendSetting.Name = editModel.Name;
                frontendSetting.Value = editModel.Value;

                //Clear cache
                _signals.Trigger("CacheFrontEndSetting_" + frontendSetting.Name + "_Changed");
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/FrontEndSetting.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("FrontEndSetting {0} updated", frontendSetting.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage frontendSettings")))
                return new HttpUnauthorizedResult();

            var frontendSetting = Services.ContentManager.Get<FrontEndSettingPart>(id);

            if (frontendSetting != null)
            {
                Services.ContentManager.Remove(frontendSetting.ContentItem);
                Services.Notifier.Information(T("FrontEndSetting {0} deleted", frontendSetting.Name));

                //Clear cache
                _signals.Trigger("CacheFrontEndSetting_" + frontendSetting.Name + "_Changed");
            }

            return RedirectToAction("Index");
        }
    }
}