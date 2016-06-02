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
    public class AdsTypeAdminController : Controller, IUpdateModel
    {
        private readonly IAdsTypeService _adsTypeService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public AdsTypeAdminController(
            IOrchardServices services,
            IAdsTypeService adsTypeService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _adsTypeService = adsTypeService;
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

        public ActionResult Index(AdsTypeIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to list adsTypes")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new AdsTypeIndexOptions();

            IContentQuery<AdsTypePart, AdsTypePartRecord> adsTypes = Services.ContentManager
                .Query<AdsTypePart, AdsTypePartRecord>();

            switch (options.Filter)
            {
                case AdsTypesFilter.All:
                    //adsTypes = adsTypes.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                adsTypes = adsTypes.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(adsTypes.Count());

            switch (options.Order)
            {
                case AdsTypesOrder.SeqOrder:
                    adsTypes = adsTypes.OrderBy(u => u.SeqOrder);
                    break;
                case AdsTypesOrder.Name:
                    adsTypes = adsTypes.OrderBy(u => u.Name);
                    break;
            }

            List<AdsTypePart> results = adsTypes
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new AdsTypesIndexViewModel
            {
                AdsTypes = results
                    .Select(x => new AdsTypeEntry {AdsType = x.Record})
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
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdsTypesIndexViewModel
            {
                AdsTypes = new List<AdsTypeEntry>(),
                Options = new AdsTypeIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<AdsTypeEntry> checkedEntries = viewModel.AdsTypes.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case AdsTypesBulkAction.None:
                    break;
                case AdsTypesBulkAction.Enable:
                    foreach (AdsTypeEntry entry in checkedEntries)
                    {
                        Enable(entry.AdsType.Id);
                    }
                    break;
                case AdsTypesBulkAction.Disable:
                    foreach (AdsTypeEntry entry in checkedEntries)
                    {
                        Disable(entry.AdsType.Id);
                    }
                    break;
                case AdsTypesBulkAction.Delete:
                    foreach (AdsTypeEntry entry in checkedEntries)
                    {
                        Delete(entry.AdsType.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var adsType = Services.ContentManager.New<AdsTypePart>("AdsType");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AdsType.Create",
                Model: new AdsTypeCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(adsType);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(AdsTypeCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (!_adsTypeService.VerifyAdsTypeUnicity(createModel.Name))
                {
                    AddModelError("NotUniqueAdsTypeName", T("AdsType with that name already exists."));
                }
            }

            var adsType = Services.ContentManager.New<AdsTypePart>("AdsType");
            if (ModelState.IsValid)
            {
                adsType.Name = createModel.Name;
                adsType.ShortName = createModel.ShortName;
                adsType.CssClass = createModel.CssClass;
                adsType.SeqOrder = createModel.SeqOrder;
                adsType.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(adsType);
            }

            dynamic model = Services.ContentManager.UpdateEditor(adsType, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AdsType.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("AdsTypes_Changed");

            Services.Notifier.Information(T("AdsType {0} created", adsType.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var adsType = Services.ContentManager.Get<AdsTypePart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AdsType.Edit",
                Model: new AdsTypeEditViewModel {AdsType = adsType}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(adsType);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var adsType = Services.ContentManager.Get<AdsTypePart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(adsType, this);

            var editModel = new AdsTypeEditViewModel {AdsType = adsType};
            if (TryUpdateModel(editModel))
            {
                if (!_adsTypeService.VerifyAdsTypeUnicity(id, editModel.Name))
                {
                    AddModelError("NotUniqueAdsTypeName", T("AdsType with that name already exists."));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AdsType.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("AdsTypes_Changed");

            Services.Notifier.Information(T("AdsType {0} updated", adsType.Name));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var adsType = Services.ContentManager.Get<AdsTypePart>(id);

            if (adsType != null)
            {
                adsType.IsEnabled = true;

                _signals.Trigger("AdsTypes_Changed");

                Services.Notifier.Information(T("AdsType {0} updated", adsType.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var adsType = Services.ContentManager.Get<AdsTypePart>(id);

            if (adsType != null)
            {
                adsType.IsEnabled = false;

                _signals.Trigger("AdsTypes_Changed");

                Services.Notifier.Information(T("AdsType {0} updated", adsType.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
                return new HttpUnauthorizedResult();

            var adsType = Services.ContentManager.Get<AdsTypePart>(id);

            if (adsType != null)
            {
                Services.ContentManager.Remove(adsType.ContentItem);

                _signals.Trigger("AdsTypes_Changed");

                Services.Notifier.Information(T("AdsType {0} deleted", adsType.Name));
            }

            return RedirectToAction("Index");
        }
    }
}