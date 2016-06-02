using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.UI.Admin;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class VideoManageController : Controller, IUpdateModel
    {
        private readonly ISignals _signals;
        private readonly IPropertyService _propertyService;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly IUserGroupService _groupService;
        private readonly IVideoManageService _videoManageService;
        private readonly IHostNameService _hostNameService;
        private readonly IGoogleApiService _googleApiService;

        public VideoManageController(
            ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IPropertyService propertyService,
            IContentManager contentManager,
            IUserGroupService groupService,
            IVideoManageService videoManageService,
            IHostNameService hostNameService,
            IGoogleApiService googleApiService
            )
        {
            _propertyService = propertyService;
            _signals = signals;
            _siteService = siteService;
            _contentManager = contentManager;
            _groupService = groupService;
            _videoManageService = videoManageService;
            _hostNameService = hostNameService;
            _googleApiService = googleApiService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region Index

        public ActionResult Index(PagerParameters pagerParameters, VideoOptions options)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            options.VideoTypes = _videoManageService.ListVideoTypes();

            int groupCurrent = 0;
            var group = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            if (group != null)
                groupCurrent = group.Id;

            var list =
                _contentManager.Query<VideoManagePart, VideoManagePartRecord>().Where(a => a.DomainGroupId == groupCurrent);

            //Search
            if (!string.IsNullOrEmpty(options.Search))
                list =
                    list.Where(
                        r =>
                            r.Title.Contains(options.Search) || r.Keyword.Contains(options.Search) ||
                            r.Description.Contains(options.Search));

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results =
                list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #region build Model

            var model = new VideoManageIndexViewModel
            {
                VideoManages = results.Select(r => new VideoManageEntry
                {
                    VideoManagePart = r
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
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            var viewModel = new VideoManageIndexViewModel
            {
                VideoManages = new List<VideoManageEntry>(),
                Options = new VideoOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<VideoManageEntry> checkedEntries = viewModel.VideoManages.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                #region Action

                case VideoManageBulkAction.Publish:
                    if (Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo))
                    {
                        //foreach (VideoManageEntry entry in checkedEntries)
                        //{
                        //    UpdateStatus(entry.Customer.Id, "st-approved");
                        //}
                    }
                    break;
                case VideoManageBulkAction.UnPublish:
                    break;
                case VideoManageBulkAction.Enable:
                    break;
                case VideoManageBulkAction.Disable:
                    break;
                case VideoManageBulkAction.Delete:
                    break;

                #endregion
            }

            return this.RedirectLocal(viewModel.ReturnUrl);
        }

        #endregion

        #region Create

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            var videoManage = Services.ContentManager.New<VideoManagePart>("VideoManage");

            dynamic model = Services.ContentManager.BuildEditor(videoManage);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/VideoManage.Create",
                Model: new VideoManageCreateViewModel
                {
                    VideoTypes = _videoManageService.ListVideoTypes(),
                    Publish = true,
                    Enable = true
                }, Prefix: null);

            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(VideoManageCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            var videoManage = Services.ContentManager.New<VideoManagePart>("VideoManage");

            HttpPostedFileBase file = Request.Files["YoutubeFile"];

            HttpPostedFileBase image = Request.Files["ImageFile"];

            if ((file == null || file.ContentLength == 0) && string.IsNullOrEmpty(createModel.ExistYoutubeId))
                AddModelError("NullFile", T("Vui lòng chọn chọn file video cần upload hoặc nhập YoutubeId đã có."));

            if (image == null || image.ContentLength == 0)
                AddModelError("NullFile", T("Vui lòng chọn chọn file image làm đại diện."));

            string hostCurrent = _hostNameService.GetHostNameSite();

            string youtubeId = createModel.ExistYoutubeId;
            if (string.IsNullOrEmpty(createModel.ExistYoutubeId))
                youtubeId = _googleApiService.VideoIdUpload(file, createModel.Title, createModel.Keyword, createModel.Description);

            if(string.IsNullOrEmpty(youtubeId))
                AddModelError("NullFile", T("Không upload được file lên youtube."));

            if (ModelState.IsValid)
            {
                videoManage.Title = createModel.Title;
                videoManage.Keyword = createModel.Keyword;
                videoManage.Description = createModel.Description;
                videoManage.Enable = createModel.Enable;
                videoManage.Publish = createModel.Publish;
                videoManage.SeqOrder = createModel.SeqOrder;
                videoManage.YoutubeId = youtubeId;
                videoManage.DomainGroupId = _groupService.GetCurrentDomainGroup().Id;
                videoManage.VideoType = _videoManageService.GetVideoTypePart(createModel.VideoTypeId).Record;

                Services.ContentManager.Create(videoManage);

                if (image != null && image.ContentLength > 0 && videoManage != null)
                {
                    videoManage.Image = _videoManageService.UploadFileVideoImage(image, hostCurrent, videoManage.Id);
                }
            }
            dynamic model = Services.ContentManager.UpdateEditor(videoManage, this);

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                createModel.VideoTypes = _videoManageService.ListVideoTypes();
                createModel.Publish = true;
                createModel.Enable = true;

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/VideoManage.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            Services.Notifier.Information(T("Video <a href=\"{0}\">{1}</a> đã được tạo thành công!", Url.Action("Edit", new { id = videoManage.Id }), createModel.Title));


            return RedirectToAction("Index");
        }

        #endregion

        #region Edit

        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            var videoManage = Services.ContentManager.Get<VideoManagePart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/VideoManage.Edit",
                Model: new VideoManageEditViewModel
                {
                    VideoManage = videoManage,
                    VideoTypes = _videoManageService.ListVideoTypes(),
                    Publish = true,
                    Enable = true
                }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(videoManage);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);

        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            var videoManage = Services.ContentManager.Get<VideoManagePart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(videoManage, this);

            var editModel = new VideoManageEditViewModel { VideoManage = videoManage };

            if (TryUpdateModel(editModel))
            {
                string hostCurrent = _hostNameService.GetHostNameSite();

                HttpPostedFileBase file = Request.Files["YoutubeFile"];
                HttpPostedFileBase image = Request.Files["ImageFile"];
                if (file != null && file.ContentLength > 0 || !string.IsNullOrEmpty(editModel.ExistYoutubeId))
                {
                    string videoId = editModel.ExistYoutubeId;
                    if(string.IsNullOrEmpty(editModel.ExistYoutubeId))
                        videoId = _googleApiService.VideoIdUpload(file, editModel.Title, editModel.Keyword,
                        editModel.Description);

                    if (string.IsNullOrEmpty(videoId))
                        AddModelError("NullFile", T("Không tạo được link vì không upload được lên Youtube."));

                    videoManage.YoutubeId = videoId;
                }
                videoManage.VideoType = _videoManageService.GetVideoTypePart(editModel.VideoTypeId).Record;

                if (image != null && image.ContentLength > 0 && videoManage != null)
                {
                    videoManage.Image = _videoManageService.UploadFileVideoImage(image, hostCurrent, videoManage.Id, videoManage.Image);
                }

            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.VideoTypes = _videoManageService.ListVideoTypes();
                editModel.Publish = true;
                editModel.Enable = true;

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/VideoManage.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Video <a href=\"{0}\">{1}</a> đã được cập nhật!", Url.Action("Edit", new { id = id }), editModel.Title));
            return RedirectToAction("Index");
        }

        #endregion


        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageYoutubeVideo,
                    T("Không có quyền Quản lý video")))
                return new HttpUnauthorizedResult();

            var videoManage = Services.ContentManager.Get<VideoManagePart>(id);

            if (videoManage != null)
            {
                Services.ContentManager.Remove(videoManage.ContentItem);

                Services.Notifier.Information(T("VideoManagePart {0} deleted", videoManage.Title));
            }

            return RedirectToAction("Index");
        }

        #region UpdateMOdel

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        #endregion
    }
}
