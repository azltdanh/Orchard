using System;
using System.Collections.Generic;
using System.Linq;

using Orchard.Settings;
using Orchard.UI.Notify;
using System.Web.Mvc;
using Orchard.UI.Admin;
using Orchard.ContentManagement;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using RealEstateForum.Service.ViewModels;
using Orchard.UI.Navigation;
using RealEstateForum.Service.Models;
using System.Web.Routing;
using RealEstateForum.Service;
using Orchard.Security;

namespace RealEstate.MiniForum.Controllers
{
    [ValidateInput(false), Admin]
    public class SupportOnlineConfigController : Controller, IUpdateModel
    {
        #region Init
        private readonly ISiteService _siteService;

        public SupportOnlineConfigController(
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

        public ActionResult Index(SupportOnlineOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý forum (ManagementMiniForum)")))
                return new HttpUnauthorizedResult();


            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new SupportOnlineOptions();

            var supportonline = Services.ContentManager
                .Query<SupportOnlineConfigPart, SupportOnlineConfigPartRecord>();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                supportonline = supportonline.Where(u => u.NumberPhone.Contains(options.Search) || u.SkypeNick.Contains(options.Search) || u.YahooNick.Contains(options.Search));
            }
            var pagerShape = Shape.Pager(pager).TotalItemCount(supportonline.Count());

            var results = supportonline
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new SupportOnlineIndexConfigViewModel
            {
                Supportss = results
                    .Select(x => new SupportOnlineEntry { Supports = x.Record }).OrderByDescending(c => c.Supports.Id)
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        //[Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var viewModel = new SupportOnlineIndexConfigViewModel { Supportss = new List<SupportOnlineEntry>(), Options = new SupportOnlineOptions() };
            UpdateModel(viewModel);

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();


            var supportonline = Services.ContentManager.New<SupportOnlineConfigPart>("SupportOnlineConfig");
            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/SupportOnlineConfigPart.Create",
                Model: new SupportOnlineCreateViewModel(),
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(supportonline);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(SupportOnlineCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var supportonline = Services.ContentManager.New<SupportOnlineConfigPart>("SupportOnlineConfig");

            if (ModelState.IsValid)
            {
                supportonline.NumberPhone = createModel.NumberPhone;
                supportonline.SkypeNick = createModel.SkypeNick;
                supportonline.YahooNick = createModel.YahooNick;

                Services.ContentManager.Create(supportonline);
            }

            dynamic model = Services.ContentManager.BuildEditor(supportonline);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/SupportOnlineConfigPart.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Thông tin hỗ trợ online đã được tạo!"));

            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            //if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
            //    return new HttpUnauthorizedResult();
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var supportonline = Services.ContentManager.Get<SupportOnlineConfigPart>(id);
            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/SupportOnlineConfigPart.Edit",
                Model: new SupportOnlineEditViewModel
                {
                    SupportOnline = supportonline,
                    ReturnUrl = returnUrl
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(supportonline);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id)
        {
            //if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
            //    return new HttpUnauthorizedResult();
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var supportonline = Services.ContentManager.Get<SupportOnlineConfigPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(supportonline, this);

            var editModel = new SupportOnlineEditViewModel { SupportOnline = supportonline };

            TryUpdateModel(editModel);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/SupportOnlineConfig.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Thông tin hỗ trợ online đã được cập nhật!"));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return Redirect(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền admin")))
                return new HttpUnauthorizedResult();

            var supportonline = Services.ContentManager.Get<SupportOnlineConfigPart>(id);

            if (supportonline != null)
            {
                Services.ContentManager.Remove(supportonline.ContentItem);
                Services.Notifier.Information(T("Thông tin {0} đã xóa.", supportonline.NumberPhone));
            }

            return RedirectToAction("Index");
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