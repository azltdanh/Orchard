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

using RealEstateForum.Service.Models;
using RealEstateForum.Service.Services;
using RealEstateForum.Service.ViewModels;
using Orchard.UI.Navigation;
using System.Web.Routing;
using RealEstateForum.Service;
using RealEstate.Helpers;
using RealEstate.Services;
using System.IO;


namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class PostAdminForumController : Controller, IUpdateModel
    {
        private bool _debugIndex = true;

        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly IThreadAdminService _threadService;
        private readonly IPostAdminService _postService;
        private readonly IFacebookApiService _facebookApiSevice;
        private readonly IHostNameService _hostNameService;
        private readonly IUserGroupService _groupService;

        private const string DefaultPath = "/Media/ForumPost/Images";//Media/Forum/Post

        public PostAdminForumController(ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IPostAdminService postService,
            IThreadAdminService threadService,
            IHostNameService hostNameService,
            IUserGroupService groupService,
            IFacebookApiService facebookApiSevice)
        {
            _signals = signals;
            _siteService = siteService;
            _threadService = threadService;
            _postService = postService;
            _facebookApiSevice = facebookApiSevice;
            _hostNameService = hostNameService;
            _groupService = groupService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }
        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region Post Management
        public ActionResult Index(PostIndexOptions options, PagerParameters pagerParameters)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            #region Facebook

            if (Session["AdminForumPostFacebook"] != null)
                {
                    int flag = (int)Session["AdminForumPostFacebook"];
                    if (flag != 0)
                    {
                        try
                        {
                            var postForum = Services.ContentManager.Get<ForumPostPart>(flag);

                            string displayForNameForUrl = postForum.Title.ToSlug().Count() > 100 ? postForum.Title.ToSlug().Substring(0, 100) : postForum.Title.ToSlug();
                            string linkDetail = Url.Action("PostDetail", "PostForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", Id = postForum.Id, ThreadShortName = _threadService.GetThreadInfoById(hostname, postForum.Thread.ParentThreadId.Value).ShortName, TopicShortName = postForum.Thread.ShortName, Title = displayForNameForUrl });
                            const string fDefaultImage = "http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg";
                            const string domain = "http://dinhgianhadat.vn";
                            const string fCaption = "Diễn đàn bất động sản - Các thông tin thị trường BĐS mới nhất.";
                            string fMessage = postForum.Title;

                            string titleSlug = postForum.Title.ToSlug();
                            string displayUrl = titleSlug.Count() > 100 ? _postService.MySubString(titleSlug, 100) : titleSlug;
                            string threadShortName = _threadService.GetThreadInfoById(hostname, postForum.Thread.ParentThreadId.Value).ShortName;

                            _facebookApiSevice.PostToFaceBook(domain, linkDetail, postForum.Title, postForum.Content.StripHtml(), fCaption, fMessage, fDefaultImage);

                            Session["AdminForumPostFacebook"] = null;
                        }
                        catch (Exception e)
                        {
                            Services.Notifier.Warning(T("FB Error: {0}", e.Message));
                        }
                    }
                }
            #endregion

            var startIndex = DateTime.Now;

            #region Filter

            DateTime startFilter = DateTime.Now;

            options = _postService.BuildPostIndexOption(options, hostname);

            IContentQuery<ForumPostPart, ForumPostPartRecord> listPost = _postService.SearchForumPost(options, hostname);



            if (_debugIndex) Services.Notifier.Information(T("FILTER {0}", (DateTime.Now - startFilter).TotalSeconds));
            if (_debugIndex) Services.Notifier.Information(T("Count 1 {0}", listPost.Count()));

            #endregion

            if(listPost.Count() < 1)
                Services.Notifier.Information(T("Không có bài viết nào được tìm thấy."));

            #region Order

            switch (options.Order)
            {
                case PostOrder.DateUpdated:
                    listPost = listPost.OrderByDescending(c => c.DateUpdated);
                    break;
                case PostOrder.Id:
                    listPost = listPost.OrderByDescending(c => c.Id);
                    break;
                case PostOrder.Name:
                    listPost = listPost.OrderBy(r => r.Title);
                    break;
                case PostOrder.DateCreated :
                    listPost = listPost.OrderByDescending(r => r.DateCreated);
                    break;
                default:
                    listPost = listPost.OrderByDescending(r => r.DateUpdated);
                    break;
            }

            #endregion

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = listPost.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = listPost.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region BUILD MODEL

            DateTime startBuildModel = DateTime.Now;

            var model = new PostIndexAdminViewModel
            {
                ForumPostEntry = results.Select(r=> new ForumPostEntry
                {
                    ForumPostItem = r
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            if (_debugIndex) Services.Notifier.Information(T("BUILD MODEL {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }
        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            Services.Notifier.Information(T("1. Not Error"));
            var viewModel = new PostIndexAdminViewModel { ForumPostEntry = new List<ForumPostEntry>(), Options = new PostIndexOptions() };
            Services.Notifier.Information(T("1.1. Not Error"));
            UpdateModel(viewModel);
            Services.Notifier.Information(T("2. Not Error"));

            var checkedEntries = viewModel.ForumPostEntry.Where(c => c.IsChecked);
            Services.Notifier.Information(T("3. Not Error"));

            switch (viewModel.Options.BulkAction)
            {
                case PostBulkAction.None:
                    break;
                case PostBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        _postService.DeletePostForum(entry.ForumPostItem.Id);
                    }
                    break;
                case PostBulkAction.UnDelete:
                    foreach (var entry in checkedEntries)
                    {
                        _postService.UnDeletePostForum(entry.ForumPostItem.Id);
                    }
                    break;
                case PostBulkAction.Approve:
                    foreach (var entry in checkedEntries)
                    {
                        _postService.ApprovePostForum(entry.ForumPostItem.Id);
                    }
                    break;
                case PostBulkAction.UpdateMetaKeyWord:
                    {
                        _postService.M_UpdateMetaDescriptionKeywords(hostname);
                        break;
                    }
            }
            Services.Notifier.Information(T("4. Not Error"));

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");

            dynamic model = Services.ContentManager.BuildEditor(postForum);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Create",
                Model: new PostCreateAdminViewModel
                {
                    ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                    AcceptPostToFacebok = true,
                    ReturnUrl = returnUrl
                }
            , Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Create(PostCreateAdminViewModel createModel)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var _postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");
            //var user = Services.WorkContext.CurrentUser.As<UserPart>();
            HttpPostedFileBase CssImage = Request.Files["CssImage"];

            #region Validate

            if (!string.IsNullOrEmpty(createModel.Title) && _postService.CheckIsExistPostTitle(createModel.Title, hostname))
                AddModelError("IsExistTitle", T("Tên bài viết đã tồn tại. Vui lòng chọn tên khác."));

            if (createModel.IsProject && (CssImage == null || CssImage.ContentLength < 1))
                AddModelError("IsProjectNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin dự án."));

            if (createModel.IsMarket && (CssImage == null || CssImage.ContentLength < 1))
                AddModelError("IsMarketNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin thị trường."));

            if (createModel.IsPinned && string.IsNullOrEmpty(createModel.TimeExpiredPinned.ToString()))
                AddModelError("IsPinnedNotTimeExpired", T("Bạn phải thêm thời gian hết hạn của bài ghim."));

            if (createModel.IsPinned && createModel.TimeExpiredPinned < DateTime.Now)
                AddModelError("TimeExpiredNotValid", T("Ngày bạn chọn đã hết hạn đăng, hãy chọn 1 ngày khác trong tương lai."));

            #endregion

            if (ModelState.IsValid)
            {

                _postForum.Title = createModel.Title.Trim();
                _postForum.Description = HttpUtility.HtmlDecode(createModel.Description) ?? createModel.Description;
                _postForum.Content = HttpUtility.HtmlDecode(createModel.Content);
                _postForum.Thread = _threadService.GetTopicPartRecordById(createModel.TopicId, hostname).Record;
                _postForum.UserPost = user.Record;
                //_postForum.CssImage = createModel.CssImage;
                _postForum.IsPinned = createModel.IsPinned;
                _postForum.TimeExpiredPinned = createModel.IsPinned ? createModel.TimeExpiredPinned : null;
                _postForum.IsProject = createModel.IsProject;
                _postForum.IsMarket = createModel.IsMarket;
                _postForum.IsHeighLight = createModel.IsHeighLight;
                _postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(createModel.PublishStatusId).Record;
                _postForum.DateCreated = DateTime.Now;
                _postForum.DateUpdated = DateTime.Now;
                _postForum.Views = 0;
                _postForum.StatusPost = _threadService.GetStatusForumPartRecordById(createModel.StatusPostId).Record;

                //Share blog
                _postForum.IsShareBlog = createModel.IsShareBlog;
                _postForum.Blog = createModel.IsShareBlog ? user.Record : null;
                _postForum.BlogDateCreated = createModel.IsShareBlog ? (DateTime?)DateTime.Now : null;
                _postForum.HostName = hostname;

                Services.ContentManager.Create(_postForum);
                _postForum.BlogPostId = createModel.IsShareBlog ? (int?)_postForum.Id : null;

                #region Upload CssImage
                if (CssImage != null && CssImage.ContentLength > 0)
                {
                    //_postForum.CssImage = _postService.UploadFilePostCssImage(CssImage, createModel.TopicId, _postForum.Id);//Upload ảnh bài viết
                    string fileName = Guid.NewGuid() + Path.GetExtension(CssImage.FileName);
                    string fileLocation = DefaultPath + "/" + fileName;
                    CssImage.SaveAsFileLocation(fileLocation);
                    _postForum.CssImage = fileName;
                }
                #endregion

                _postService.ClearListPostFromTopicCache(_postForum, hostname);//Clear Cache

                #region Save Meta

                _postService.M_UpdateMetaDescriptionKeywords(_postForum, false);

                #endregion

                #region Topic Post FaceBook

                Session["AdminForumPostFacebook"] = createModel.AcceptPostToFacebok ?  _postForum.Id : 0;

                #endregion

                //Clear Post Pined Expired
                _postService.ClearPostPinedExpired(hostname);

                Services.Notifier.Information(T("Đã thêm bài viết <a href=\"{0}\">{1}</a> thành công.", Url.Action("Edit", "PostAdminForum", new { area = "RealEstate.MiniForum", Id = _postForum.Id }), _postForum.Title));
            }

            dynamic model = Services.ContentManager.UpdateEditor(_postForum, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Create",
                    Model: new PostCreateAdminViewModel
                    {
                        ThreadId = createModel.ThreadId,
                        TopicId = createModel.TopicId,
                        ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                        ListTopic = !string.IsNullOrEmpty(createModel.ThreadId.ToString()) ? _threadService.GetListTopicFromCache(createModel.ThreadId, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList() : null,
                        ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                        ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                        HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                        AcceptPostToFacebok = true,
                    }, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.Redirect(createModel.ReturnUrl);
            }
            return RedirectToAction("index");
        }

        public ActionResult Edit(int Id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var postForum = Services.ContentManager.Get<ForumPostPart>(Id);
            //var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Edit",
                Model: new PostEditAdminViewModel
                {
                    ForumPost = postForum,
                    ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListTopic = _threadService.GetListTopicFromCache(postForum.Thread.ParentThreadId.Value, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    TopicId = postForum.Thread.Id,
                    PublishStatusId = postForum.PublishStatus.Id,
                    StatusPostId = postForum.StatusPost.Id,
                    HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                    AcceptPostToFacebok = false,
                    ReturnUrl = returnUrl
                }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(postForum);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int Id)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var postForum = Services.ContentManager.Get<ForumPostPart>(Id);

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            string oldCssIamge = postForum.CssImage;
            //var user = Services.WorkContext.CurrentUser.As<UserPart>();
            dynamic model = Services.ContentManager.UpdateEditor(postForum, this);
            var editModel = new PostEditAdminViewModel { ForumPost = postForum };
            var cssImage = Request.Files["CssImage"];
            if (TryUpdateModel(editModel))
            {
                #region Validate

                if (!string.IsNullOrEmpty(editModel.Title) && _postService.CheckIsExistPostTitle(postForum.Id, editModel.Title, hostname))
                    AddModelError("IsExistTitle", T("Tên bài viết đã tồn tại. Vui lòng chọn tên khác."));

                if (editModel.IsProject)
                {
                    if (string.IsNullOrEmpty(oldCssIamge))
                    {
                        if (cssImage == null || cssImage.ContentLength < 1)
                            AddModelError("IsProjectNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin dự án."));
                    }
                }

                if (editModel.IsMarket)
                {
                    if (string.IsNullOrEmpty(oldCssIamge))
                    {
                        if (cssImage == null || cssImage.ContentLength < 1)
                            AddModelError("IsMarketNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin thị trường."));
                    }
                }

                if (editModel.IsPinned && string.IsNullOrEmpty(editModel.TimeExpiredPinned.ToString()))
                    AddModelError("IsPinnedNotTimeExpired", T("Bạn phải thêm thời gian hết hạn của bài ghim."));

                if (editModel.IsPinned && editModel.TimeExpiredPinned < DateTime.Now)
                    AddModelError("TimeExpiredNotValid", T("Ngày bạn chọn đã hết hạn đăng, hãy chọn 1 ngày khác trong tương lai."));

                #endregion

                if (ModelState.IsValid)
                {

                    postForum.Title = editModel.Title;
                    postForum.Description = HttpUtility.HtmlDecode(editModel.Description);
                    postForum.Content = HttpUtility.HtmlDecode(editModel.Content);
                    postForum.Thread = _threadService.GetTopicPartRecordById(editModel.TopicId, hostname).Record;
                    postForum.IsPinned = editModel.IsPinned;
                    postForum.TimeExpiredPinned = editModel.IsPinned ? editModel.TimeExpiredPinned : null;
                    postForum.IsProject = editModel.IsProject;
                    postForum.IsMarket = editModel.IsMarket;
                    postForum.IsHeighLight = editModel.IsHeighLight;
                    postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(editModel.PublishStatusId).Record;
                    postForum.StatusPost = _threadService.GetStatusForumPartRecordById(editModel.StatusPostId).Record;
                    postForum.DateUpdated = DateTime.Now;
                    //_postForum.HostName = _hostNameService.GetHostNameSite();
                    if (editModel.IsShareBlog)
                    {
                        if (postForum.UserPost == user.Record) // Nếu là chủ bài viết
                        {
                            if (postForum.IsShareBlog)
                            {
                                postForum.BlogDateCreated = DateTime.Now;
                            }
                            else
                            {
                                postForum.IsShareBlog = editModel.IsShareBlog;
                                postForum.Blog = editModel.IsShareBlog ? user.Record : null;
                                postForum.BlogDateCreated = editModel.IsShareBlog ? (DateTime?) DateTime.Now : null;
                                postForum.BlogPostId = postForum.Id;
                            }
                        }
                        else
                        {
                            _postService.ShareToMyBlog(postForum, user, hostname);
                        }
                    }


                    #region Upload CssImage

                    if (cssImage != null && cssImage.ContentLength > 0)
                    {
                        //postForum.CssImage = _postService.UploadFilePostCssImage(cssImage, editModel.TopicId, postForum.Id, oldCssIamge);//Upload ảnh bài viết
                        string folderLocation = DefaultPath;

                        string uploadsFolder = Services.WorkContext.HttpContext.Server.MapPath(folderLocation);//"~/UserFiles/" + topicId
                        var folder = new DirectoryInfo(uploadsFolder);

                        if (!folder.Exists) folder.Create();

                        if (!string.IsNullOrEmpty(oldCssIamge))
                        {
                            var fileInfo = new FileInfo(folder + "/" + oldCssIamge);
                            if (fileInfo.Exists) fileInfo.Delete();
                        }

                        string fileNameUpload = Guid.NewGuid() + Path.GetExtension(cssImage.FileName);
                        string fileLocation = folderLocation + "/" + fileNameUpload;

                        cssImage.SaveAsFileLocation(fileLocation);
                        postForum.CssImage = fileNameUpload;
                    }
                    #endregion

                    #region Clear Cache
                    _postService.ClearListPostFromTopicCache(postForum, hostname);//Clear Cache
                    #endregion

                    #region Save Meta

                    _postService.M_UpdateMetaDescriptionKeywords(postForum, editModel.UpdateMeta);

                    #endregion

                    #region Topic Post FaceBook

                    Session["AdminForumPostFacebook"] = editModel.AcceptPostToFacebok ? postForum.Id : 0;

                    #endregion

                    Services.Notifier.Information(T("Đã cập nhật bài viết thành công!"));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Edit", Model: new PostEditAdminViewModel
                {
                    ForumPost = postForum,
                    ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListTopic = _threadService.GetListTopicFromCache(postForum.Thread.ParentThreadId.Value, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    TopicId = postForum.Thread.Id,
                    PublishStatusId = postForum.PublishStatus.Id,
                    StatusPostId = postForum.StatusPost.Id,
                    HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                    AcceptPostToFacebok = true,
                }, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.Redirect(editModel.ReturnUrl);

            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Post Management by Hostname
        public ActionResult HPostIndex(PostIndexOptions options, PagerParameters pagerParameters)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            if (Session["AdminForumPostFacebook"] != null)
            {
                int flag = (int)Session["AdminForumPostFacebook"];
                if (flag != 0)
                {
                    try
                    {
                        var postForum = Services.ContentManager.Get<ForumPostPart>(flag);

                        string DisplayForNameForUrl = postForum.Title.ToSlug().Count() > 100 ? postForum.Title.ToSlug().Substring(0, 100) : postForum.Title.ToSlug();
                        string linkDetail = Url.Action("PostDetail", "PostForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", Id = postForum.Id, ThreadShortName = _threadService.GetThreadInfoById(hostname, postForum.Thread.ParentThreadId.Value).ShortName, TopicShortName = postForum.Thread.ShortName, Title = DisplayForNameForUrl });
                        string fDefaultImage = "http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg";
                        string domain = "http://dinhgianhadat.vn";
                        string fCaption = "Diễn đàn bất động sản - Các thông tin thị trường BĐS mới nhất.";
                        string fMessage = postForum.Title;

                        string TitleSlug = postForum.Title.ToSlug();
                        string DisplayUrl = TitleSlug.Count() > 100 ? _postService.MySubString(TitleSlug, 100) : TitleSlug;
                        string threadShortName = _threadService.GetThreadInfoById(hostname, postForum.Thread.ParentThreadId.Value).ShortName;

                        _facebookApiSevice.PostToFaceBook(domain, linkDetail, postForum.Title, postForum.Content.StripHtml(), fCaption, fMessage, fDefaultImage);

                        Session["AdminForumPostFacebook"] = null;
                    }
                    catch (Exception e)
                    {
                        Services.Notifier.Warning(T("FB Error: {0}", e.Message));
                    }
                }
            }
            DateTime startIndex = DateTime.Now;

            IContentQuery<ForumPostPart, ForumPostPartRecord> _listPost;

            #region Filter

            DateTime startFilter = DateTime.Now;

            options = _postService.BuildPostIndexOption(options, hostname);

            _listPost = _postService.SearchForumPost(options, hostname);

            if (_debugIndex) Services.Notifier.Information(T("FILTER {0}", (DateTime.Now - startFilter).TotalSeconds));

            #endregion

            if (_listPost.Count() < 1)
                Services.Notifier.Information(T("Không có bài viết nào được tìm thấy."));

            #region Order

            switch (options.Order)
            {
                case PostOrder.Id:
                    _listPost = _listPost.OrderByDescending(c => c.Id);
                    break;
                case PostOrder.Name:
                    _listPost = _listPost.OrderBy(r => r.Title);
                    break;
                case PostOrder.DateCreated:
                    _listPost = _listPost.OrderByDescending(r => r.DateUpdated);
                    break;
            }

            #endregion

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = _listPost.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = _listPost.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region BUILD MODEL

            DateTime startBuildModel = DateTime.Now;

            var model = new PostIndexAdminViewModel
            {
                ForumPostEntry = results.OrderByDescending(a =>a.DateUpdated).Select(r => new ForumPostEntry
                {
                    ForumPostItem = r
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            if (_debugIndex) Services.Notifier.Information(T("BUILD MODEL {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }
        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult HPostIndex(string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var viewModel = new PostIndexAdminViewModel { ForumPostEntry = new List<ForumPostEntry>(), Options = new PostIndexOptions() };
            UpdateModel(viewModel);

            var checkedEntries = viewModel.ForumPostEntry.Where(c => c.IsChecked);

            switch (viewModel.Options.BulkAction)
            {
                case PostBulkAction.None:
                    break;
                case PostBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        _postService.DeletePostForum(entry.ForumPostItem.Id);
                    }
                    break;
                case PostBulkAction.UnDelete:
                    foreach (var entry in checkedEntries)
                    {
                        _postService.UnDeletePostForum(entry.ForumPostItem.Id);
                    }
                    break;
                case PostBulkAction.UpdateMetaKeyWord:
                    {
                        _postService.M_UpdateMetaDescriptionKeywords(hostname);
                        break;
                    }
            }
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("HPostIndex", ControllerContext.RouteData.Values);
        }

        public ActionResult HPostCreate(string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");

            dynamic model = Services.ContentManager.BuildEditor(postForum);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Create",
                Model: new PostCreateAdminViewModel
                {
                    ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ReturnUrl = returnUrl
                }
            , Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult HPostCreate(PostCreateAdminViewModel createModel)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var _postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");
            //var user = Services.WorkContext.CurrentUser.As<UserPart>();
            HttpPostedFileBase CssImage = Request.Files["CssImage"];

            #region Validate

            if (!string.IsNullOrEmpty(createModel.Title) && _postService.CheckIsExistPostTitle(createModel.Title, hostname))
                AddModelError("IsExistTitle", T("Tên bài viết đã tồn tại. Vui lòng chọn tên khác."));

            if (createModel.IsProject && (CssImage == null || CssImage.ContentLength < 1))
                AddModelError("IsProjectNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin dự án."));

            if (createModel.IsMarket && (CssImage == null || CssImage.ContentLength < 1))
                AddModelError("IsMarketNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin thị trường."));

            if (createModel.IsPinned && string.IsNullOrEmpty(createModel.TimeExpiredPinned.ToString()))
                AddModelError("IsPinnedNotTimeExpired", T("Bạn phải thêm thời gian hết hạn của bài ghim."));

            if (createModel.IsPinned && createModel.TimeExpiredPinned < DateTime.Now)
                AddModelError("TimeExpiredNotValid", T("Ngày bạn chọn đã hết hạn đăng, hãy chọn 1 ngày khác trong tương lai."));

            #endregion

            if (ModelState.IsValid)
            {
                DateTime? BlogDate = null;

                _postForum.Title = createModel.Title.Trim();
                _postForum.Description = createModel.Description;
                _postForum.Content = createModel.Content;
                _postForum.Thread = _threadService.GetTopicPartRecordById(createModel.TopicId, hostname).Record;
                _postForum.UserPost = user.Record;
                //_postForum.CssImage = createModel.CssImage;
                _postForum.IsPinned = createModel.IsPinned;
                _postForum.TimeExpiredPinned = createModel.IsPinned ? createModel.TimeExpiredPinned : null;
                _postForum.IsProject = createModel.IsProject;
                _postForum.IsMarket = createModel.IsMarket;
                _postForum.IsHeighLight = createModel.IsHeighLight;
                _postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(createModel.PublishStatusId).Record;
                _postForum.DateCreated = DateTime.Now;
                _postForum.DateUpdated = DateTime.Now;
                _postForum.Views = 0;
                _postForum.StatusPost = _threadService.GetStatusForumPartRecordById(createModel.StatusPostId).Record;

                _postForum.IsShareBlog = createModel.IsShareBlog;
                _postForum.Blog = createModel.IsShareBlog ? user.Record : null;
                _postForum.BlogDateCreated = createModel.IsShareBlog ? DateTime.Now : BlogDate;
                _postForum.HostName = hostname;

                Services.ContentManager.Create(_postForum);

                #region Upload CssImage
                if (CssImage != null && CssImage.ContentLength > 0)
                {
                    //_postForum.CssImage = _postService.UploadFilePostCssImage(CssImage, createModel.TopicId, _postForum.Id);//Upload ảnh bài viết
                    string fileLocation = DefaultPath + "/" + Guid.NewGuid() + Path.GetExtension(CssImage.FileName);
                    CssImage.SaveAsFileLocation(fileLocation);
                }
                #endregion

                _postService.ClearListPostFromTopicCache(_postForum, hostname);//Clear Cache

                #region Save Meta

                _postService.M_UpdateMetaDescriptionKeywords(_postForum, false);

                #endregion

                #region Topic Post FaceBook

                Session["AdminForumPostFacebook"] = _postForum.Id;

                #endregion
                Services.Notifier.Information(T("Đã thêm bài viết <a href=\"{0}\">{1}</a> thành công.", Url.Action("Edit", "PostAdminForum", new { area = "RealEstate.MiniForum", Id = _postForum.Id }), _postForum.Title));
            }

            dynamic model = Services.ContentManager.UpdateEditor(_postForum, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Create",
                    Model: new PostCreateAdminViewModel
                    {
                        ThreadId = createModel.ThreadId,
                        TopicId = createModel.TopicId,
                        ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                        ListTopic = !string.IsNullOrEmpty(createModel.ThreadId.ToString()) ? _threadService.GetListTopicFromCache(createModel.ThreadId, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList() : null,
                        ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                        ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    }, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.Redirect(createModel.ReturnUrl);
            }
            return RedirectToAction("index");
        }

        public ActionResult HPostEdit(int Id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var postForum = Services.ContentManager.Get<ForumPostPart>(Id);
            //var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Edit",
                Model: new PostEditAdminViewModel
                {
                    ForumPost = postForum,
                    ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListTopic = _threadService.GetListTopicFromCache(postForum.Thread.ParentThreadId.Value, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    TopicId = postForum.Thread.Id,
                    PublishStatusId = postForum.PublishStatus.Id,
                    StatusPostId = postForum.StatusPost.Id,
                    ReturnUrl = returnUrl
                }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(postForum);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult HPostEdit(int Id)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var _postForum = Services.ContentManager.Get<ForumPostPart>(Id);
            string oldCssIamge = _postForum.CssImage;
            //var user = Services.WorkContext.CurrentUser.As<UserPart>();
            dynamic model = Services.ContentManager.UpdateEditor(_postForum, this);
            var editModel = new PostEditAdminViewModel { ForumPost = _postForum };
            HttpPostedFileBase CssImage = Request.Files["CssImage"];
            if (TryUpdateModel(editModel))
            {
                #region Validate

                if (!string.IsNullOrEmpty(editModel.Title) && _postService.CheckIsExistPostTitle(_postForum.Id, editModel.Title, hostname))
                    AddModelError("IsExistTitle", T("Tên bài viết đã tồn tại. Vui lòng chọn tên khác."));

                if (editModel.IsProject)
                {
                    if (string.IsNullOrEmpty(oldCssIamge))
                    {
                        if (CssImage == null || CssImage.ContentLength < 1)
                            AddModelError("IsProjectNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin dự án."));
                    }
                }

                if (editModel.IsMarket)
                {
                    if (string.IsNullOrEmpty(oldCssIamge))
                    {
                        if (CssImage == null || CssImage.ContentLength < 1)
                            AddModelError("IsMarketNotImage", T("Không thể chuyển bài viết chưa có hình ảnh đại diện làm bài viết tin thị trường."));
                    }
                }

                if (editModel.IsPinned && string.IsNullOrEmpty(editModel.TimeExpiredPinned.ToString()))
                    AddModelError("IsPinnedNotTimeExpired", T("Bạn phải thêm thời gian hết hạn của bài ghim."));

                if (editModel.IsPinned && editModel.TimeExpiredPinned < DateTime.Now)
                    AddModelError("TimeExpiredNotValid", T("Ngày bạn chọn đã hết hạn đăng, hãy chọn 1 ngày khác trong tương lai."));

                #endregion

                if (ModelState.IsValid)
                {
                    DateTime? BlogDate = null;

                    _postForum.Title = editModel.Title;
                    _postForum.Description = editModel.Description;
                    _postForum.Content = editModel.Content;
                    _postForum.Thread = _threadService.GetTopicPartRecordById(editModel.TopicId, hostname).Record;
                    _postForum.IsPinned = editModel.IsPinned;
                    _postForum.TimeExpiredPinned = editModel.IsPinned ? editModel.TimeExpiredPinned : null;
                    _postForum.IsProject = editModel.IsProject;
                    _postForum.IsMarket = editModel.IsMarket;
                    _postForum.IsHeighLight = editModel.IsHeighLight;
                    _postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(editModel.PublishStatusId).Record;
                    _postForum.StatusPost = _threadService.GetStatusForumPartRecordById(editModel.StatusPostId).Record;

                    if (_postForum.UserPost == user.Record)// Nếu là chủ bài viết
                    {
                        if (_postForum.IsShareBlog)
                        {
                            _postForum.BlogDateCreated = DateTime.Now;
                        }
                        else
                        {
                            _postForum.IsShareBlog = editModel.IsShareBlog;
                            _postForum.Blog = editModel.IsShareBlog ? user.Record : null;
                            _postForum.BlogDateCreated = editModel.IsShareBlog ? BlogDate : null;
                        }
                    }
                    else
                    {
                        _postService.ShareToMyBlog(_postForum, user, hostname);
                    }


                    #region Upload CssImage
                    if (CssImage != null && CssImage.ContentLength > 0)
                    {
                        //_postForum.CssImage = _postService.UploadFilePostCssImage(CssImage, editModel.TopicId, _postForum.Id, oldCssIamge);//Upload ảnh bài viết
                        string folderLocation = DefaultPath;

                        string uploadsFolder = Services.WorkContext.HttpContext.Server.MapPath(folderLocation);//"~/UserFiles/" + topicId
                        var folder = new DirectoryInfo(uploadsFolder);

                        if (!folder.Exists) folder.Create();

                        if (!string.IsNullOrEmpty(oldCssIamge))
                        {
                            var fileInfo = new FileInfo(folder + "/" + oldCssIamge);
                            if (fileInfo.Exists) fileInfo.Delete();
                        }

                        string fileNameUpload = Guid.NewGuid() + Path.GetExtension(CssImage.FileName);
                        string fileLocation = folderLocation + fileNameUpload;

                        CssImage.SaveAsFileLocation(fileLocation);
                    }
                    #endregion

                    #region Clear Cache
                    _postService.ClearListPostFromTopicCache(_postForum, hostname);//Clear Cache
                    #endregion

                    #region Save Meta

                    _postService.M_UpdateMetaDescriptionKeywords(_postForum, editModel.UpdateMeta);

                    #endregion

                    Services.Notifier.Information(T("Đã cập nhật bài viết thành công!"));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Post.Edit", Model: new PostEditAdminViewModel
                {
                    ForumPost = _postForum,
                    ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListTopic = _threadService.GetListTopicFromCache(_postForum.Thread.ParentThreadId.Value, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                    ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                    TopicId = _postForum.Thread.Id,
                    PublishStatusId = _postForum.PublishStatus.Id,
                    StatusPostId = _postForum.StatusPost.Id,
                }, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.Redirect(editModel.ReturnUrl);

            }

            return RedirectToAction("HPostIndex");
        }

        #endregion

        public ActionResult Delete(int Id, string ReturnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniUpdatePostForum, T("Không có quyền cập nhật Forum")))
                return new HttpUnauthorizedResult();
            _postService.DeletePostForum(Id);

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            _postService.ClearListPostFromTopicCache(Services.ContentManager.Get<ForumPostPart>(Id), hostname);//Clear Cache

            Services.Notifier.Information(T("Đã xóa thành công bài viết ID: {0}", Id));

            if (!string.IsNullOrEmpty(ReturnUrl))
                return Redirect(ReturnUrl);
            else
                return RedirectToAction("index");

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