using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Web.Mvc;

using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;


using RealEstateForum.Service.Models;
using RealEstateForum.Service.ViewModels;

namespace RealEstate.MiniForum.Controllers
{
    public class AdsPriceConfigController : Controller, IUpdateModel
    {
        #region Init

        private readonly ISiteService _siteService;

        public AdsPriceConfigController(
            IOrchardServices services,
            ISiteService siteService,
            IShapeFactory shapeFactory
            )
        {
            Services = services;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        #endregion

        [Admin]
        public ActionResult Index(PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not Authorize")))
                return new HttpUnauthorizedResult();


            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var adsPrice = Services.ContentManager
                .Query<AdsPriceConfigPart, AdsPriceConfigPartRecord>(); 

            var pagerShape = Shape.Pager(pager).TotalItemCount(adsPrice.Count());

            var results = adsPrice
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new AdsPriceConfigViewModel
            {
                AdsPriceConfigs = results
                    .Select(x => new AdsPriceConfigEntry { AdsPriceConfig =  x})
                    .ToList(),
                Pager = pagerShape
            };

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost, Admin]
        //[Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not Authorize")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdsPriceConfigViewModel { AdsPriceConfigs = new List<AdsPriceConfigEntry>() };
            UpdateModel(viewModel);

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        [Admin]
        public ActionResult Create(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();


            var adsPriceConfig = Services.ContentManager.New<AdsPriceConfigPart>("AdsPriceConfig");
            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/AdsPriceConfigPart.Create",
                Model: new AdsPriceConfigCreateViewModel(),
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(adsPriceConfig);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, Admin, ActionName("Create")]
        public ActionResult CreatePost(AdsPriceConfigCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var adsPriceConfig = Services.ContentManager.New<AdsPriceConfigPart>("AdsPriceConfig");

            if (ModelState.IsValid)
            {
                adsPriceConfig.Price = createModel.Price;
                adsPriceConfig.CssClass = createModel.CssClass;

                Services.ContentManager.Create(adsPriceConfig);
            }

            dynamic model = Services.ContentManager.BuildEditor(adsPriceConfig);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/AdsPriceConfigPart.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Thông tin đã được tạo! ID: {0}", adsPriceConfig.Id));

            return RedirectToAction("Index");
        }

        [Admin]
        public ActionResult Edit(int id, string returnUrl)
        {
            //if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
            //    return new HttpUnauthorizedResult();
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var adsPriceConfig = Services.ContentManager.Get<AdsPriceConfigPart>(id);
            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/AdsPriceConfigPart.Edit",
                Model: new AdsPriceConfigEditViewModel
                {
                    AdsPriceConfig = adsPriceConfig,
                    ReturnUrl =  returnUrl
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(adsPriceConfig);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, Admin, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var adsPriceConfig = Services.ContentManager.Get<AdsPriceConfigPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(adsPriceConfig, this);

            var editModel = new AdsPriceConfigEditViewModel { AdsPriceConfig = adsPriceConfig };

            TryUpdateModel(editModel);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/AdsPriceConfig.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Thông tin đã được cập nhật! ID: {0}",adsPriceConfig.Id));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return Redirect(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost, Admin]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var adsPriceConfig = Services.ContentManager.Get<AdsPriceConfigPart>(id);

            if (adsPriceConfig == null) return RedirectToAction("Index");

            Services.ContentManager.Remove(adsPriceConfig.ContentItem);
            Services.Notifier.Information(T("Thông tin {0} đã xóa.", adsPriceConfig.Price));

            return RedirectToAction("Index");
        }


        public ActionResult AjaxLoadBannerPrice()
        {
            var adsPrice = Services.ContentManager
                .Query<AdsPriceConfigPart, AdsPriceConfigPartRecord>();

            var model = new AdsPriceConfigViewModel
            {
                AdsPriceConfigs = adsPrice.List()
                    .Select(x => new AdsPriceConfigEntry { AdsPriceConfig = x })
                    .ToList()
            };

            return PartialView(model);
        }

        #region UpdateModel
        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
    }
}