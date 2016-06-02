using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Localization;
using Orchard.Logging;
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
    public class NewsVideoController : Controller, IUpdateModel
    {

        private readonly IGoogleApiService _googleApiService;
        private readonly ISignals _signals;
        private readonly IPropertyService _propertyService;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;

        public NewsVideoController(
            ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IGoogleApiService googleApiService,
            IPropertyService propertyService,
            IContentManager contentManager
            )
        {
            _propertyService = propertyService;
            _signals = signals;
            _siteService = siteService;
            _googleApiService = googleApiService;
            _contentManager = contentManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public ActionResult Index(PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageNewsVideo,
                    T("Không có quyền Quản lý video tin tức")))
                return new HttpUnauthorizedResult();

            var list =
                _contentManager.Query<NewsVideoPart, NewsVideoPartRecord>();


            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results =
                list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #region build Model

            var model = new NewsVideoIndexViewModel
            {
                NewsVideos = results.Select(r => new NewsVideoEntry
                {
                    NewsVideoPart = r
                }).ToList(),
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

        public ActionResult Create()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageNewsVideo,
                    T("Không có quyền Quản lý video tin tức")))
                return new HttpUnauthorizedResult();

            var newsVideo = Services.ContentManager.New<NewsVideoPart>("NewsVideo");

            dynamic model = Services.ContentManager.BuildEditor(newsVideo);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/NewsVideo.Create",
                Model: new NewsVideoCreateViewModel(), Prefix: null);

            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(NewsVideoCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageNewsVideo,
                    T("Không có quyền Quản lý video tin tức")))
                return new HttpUnauthorizedResult();


            var newsVideo = Services.ContentManager.New<NewsVideoPart>("NewsVideo");
            var videofile = Request.Files["YoutubeUpload"];

            if (videofile.ContentLength == 0)
                AddModelError("YoutubeUploadNull", T("Vui lòng chọn file video cần upload"));

            if (ModelState.IsValid)
            {
                newsVideo.Title = createModel.Title;
                newsVideo.Description = createModel.Description;
                newsVideo.Enable = createModel.Enable;
                newsVideo.SeqOrder = createModel.SeqOrder;

                Services.ContentManager.Create(newsVideo);

                //Upload Video
                //if (videofile.ContentLength > 0)
                var youtubeId = _googleApiService.VideoIdUpload(videofile,createModel.Title,createModel.Title,createModel.Description);
                newsVideo.YoutubeId = youtubeId;
            }
            dynamic model = Services.ContentManager.UpdateEditor(newsVideo, this);

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/NewsVideo.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            Services.Notifier.Information(T("Video <a href=\"{0}\">{1}</a> đã được tạo thành công!", Url.Action("Edit", new { id = newsVideo.Id }), createModel.Title));


            return RedirectToAction("Index");
        }


        public ActionResult Edit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageNewsVideo,
                    T("Không có quyền Quản lý video tin tức")))
                return new HttpUnauthorizedResult();

            var newsVideo = Services.ContentManager.Get<NewsVideoPart>(id);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/NewsVideo.Edit",
                Model: new NewsVideoEditViewModel { NewsVideo = newsVideo }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(newsVideo);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);

        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageNewsVideo,
                    T("Không có quyền Quản lý video tin tức")))
                return new HttpUnauthorizedResult();

            var newsVideo = Services.ContentManager.Get<NewsVideoPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(newsVideo, this);

            var editModel = new NewsVideoEditViewModel { NewsVideo = newsVideo };

            if (TryUpdateModel(editModel))
            {
                 var videofile = Request.Files["YoutubeUpload"];

                if (videofile.ContentLength > 0)
                    newsVideo.YoutubeId = _googleApiService.VideoIdUpload(videofile, editModel.Title, editModel.Title, editModel.Description);
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/NewsVideo.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Video <a href=\"{0}\">{1}</a> đã được cập nhật!",Url.Action("Edit",new { id = id}),editModel.Title));
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageNewsVideo,
                    T("Không có quyền Quản lý video tin tức")))
                return new HttpUnauthorizedResult();

            var newsVideo = Services.ContentManager.Get<NewsVideoPart>(id);

            if (newsVideo != null)
            {
                Services.ContentManager.Remove(newsVideo.ContentItem);

                Services.Notifier.Information(T("Video {0} Đã được xóa!", newsVideo.Title));
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
