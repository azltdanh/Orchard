using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.Users.Models;
using Orchard.Security;

using RealEstateForum.Service.Models;
using RealEstateForum.Service.Services;
using RealEstateForum.Service.ViewModels;
using Orchard.UI.Navigation;
using System.Web.Routing;
using RealEstateForum.Service;
using RealEstate.Helpers;
using RealEstate.Services;
using Orchard.Mvc;


namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class UnitInvestController : Controller, IUpdateModel
    {
        //private bool _debugIndex = true;

        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly IHostNameService _hostNameService;
        private readonly IPostAdminService _postService;
        private readonly IContentManager _contentManager;
        private readonly IUserGroupService _groupService;

        public UnitInvestController(ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IContentManager contentManager,
            IUserGroupService groupService,
            IPostAdminService postService,
            IHostNameService hostNameService)
        {
            _signals = signals;
            _siteService = siteService;
            _hostNameService = hostNameService;
            _contentManager = contentManager;
            _groupService = groupService;
            _postService = postService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }
        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region Index

        public ActionResult Index(PagerParameters pagerParameters, UnitInvestOptions options)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền nhà tài trợ")))
                return new HttpUnauthorizedResult();

            var group = _groupService.GetCurrentDomainGroup();

            var list = _contentManager.Query<UnitInvestPart, UnitInvestPartRecord>().Where(a => a.GroupId == group.Id);

            //Search
            if (!string.IsNullOrEmpty(options.Search))
                list =
                    list.Where(
                        r =>
                            r.Name.Contains(options.Search) || r.Website.Contains(options.Search) ||
                            r.Content.Contains(options.Search));

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results =
                list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #region build Model

            var model = new UnitInvestIndexViewModel
            {
                UnitInvests = results.Select(r => new UnitInvestEntry
                {
                    UnitInvestPartPart = r
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền quản lý đơn vị tài trợ")))
                return new HttpUnauthorizedResult();

            var viewModel = new UnitInvestIndexViewModel
            {
                UnitInvests = new List<UnitInvestEntry>(),
                Options = new UnitInvestOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<UnitInvestEntry> checkedEntries = viewModel.UnitInvests.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                #region Action
                case UnitInvestBulkAction.Enable:
                    break;
                case UnitInvestBulkAction.Disable:
                    break;
                case UnitInvestBulkAction.Delete:
                    break;

                #endregion
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        #endregion

        #region Create

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền QL đơn vị tài trợ")))
                return new HttpUnauthorizedResult();

            var unitInvest = Services.ContentManager.New<UnitInvestPart>("UnitInvest");

            dynamic model = Services.ContentManager.BuildEditor(unitInvest);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UnitInvest.Create", Model: new UnitInvestCreateViewModel {}, Prefix: null);

            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(UnitInvestCreateViewModel createModel)
        {
            if (
                 !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền quản lý đơn vị tài trợ")))
                return new HttpUnauthorizedResult();

            var unitInvest = Services.ContentManager.New<UnitInvestPart>("UnitInvest");

            HttpPostedFileBase avatar = Request.Files["Avatar"];

            if (avatar == null || avatar.ContentLength == 0)
                AddModelError("NullFile", T("Vui lòng chọn chọn file avatar."));

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            if (ModelState.IsValid)
            {
                unitInvest.Name = createModel.Name;
                unitInvest.Website = createModel.Website;
                unitInvest.Content = createModel.Content;
                unitInvest.IsEnabled = createModel.IsEnabled;
                unitInvest.SeqOrder = createModel.SeqOrder;
                unitInvest.GroupId = _groupService.GetCurrentDomainGroup().Id;

                Services.ContentManager.Create(unitInvest);

                 // Upload Icon
                if (avatar != null && avatar.ContentLength > 0)
                {
                    unitInvest.Avatar = _postService.UploadFileFolderDefault(avatar, hostname, unitInvest.Id);
                }
            }
            dynamic model = Services.ContentManager.UpdateEditor(unitInvest, this);

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UnitInvest.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            Services.Notifier.Information(T("Đơn vị tài trợ <a href=\"{0}\">{1}</a> đã được tạo thành công!", Url.Action("Edit", new { id = unitInvest.Id }), createModel.Name));


            return RedirectToAction("Index");
        }

        #endregion
        
        #region Edit

        public ActionResult Edit(int id)
        {
            if (
                 !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền quản lý đơn vị tài trợ")))
                return new HttpUnauthorizedResult();

            var unitInvest = Services.ContentManager.Get<UnitInvestPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UnitInvest.Edit",
                Model: new UnitInvestEditViewModel { UnitInvest  = unitInvest}, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(unitInvest);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);

        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                  !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền quản lý đơn vị tài trợ")))
                return new HttpUnauthorizedResult();

            var unitInvest = Services.ContentManager.Get<UnitInvestPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(unitInvest, this);

            var editModel = new UnitInvestEditViewModel { UnitInvest = unitInvest };

            if (TryUpdateModel(editModel))
            {
                var user = Services.WorkContext.CurrentUser.As<UserPart>();
                string hostname = _groupService.GetHostNameByUser(user);

                HttpPostedFileBase image = Request.Files["AvatarUnitInvest"];

                if (image != null && image.ContentLength > 0 && unitInvest != null)
                {
                    unitInvest.Avatar = _postService.UploadFileFolderDefault(image, hostname, unitInvest.Id);
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/UnitInvest.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Đơn vị tài trợ <a href=\"{0}\">{1}</a> đã được cập nhật!", Url.Action("Edit", new { id = id }), editModel.Name));
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageUnitInvest,
                    T("Không có quyền quản lý đơn vị tài trợ")))
                return new HttpUnauthorizedResult();

            var unitInvest = Services.ContentManager.Get<UnitInvestPart>(id);

            if (unitInvest != null)
            {
                Services.ContentManager.Remove(unitInvest.ContentItem);

                Services.Notifier.Information(T("Đơn vị tài trợ {0} đã xóa", unitInvest.Name));
            }

            return RedirectToAction("Index");
        }


        #endregion

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