using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Orchard.ContentManagement;
using Orchard.Themes;
using Orchard.Localization;
using Orchard;
using Orchard.Logging;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Orchard.DisplayManagement;

using RealEstate.Helpers;
using RealEstateForum.Service.Services;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.ViewModels;
using RealEstate.MiniForum.FrontEnd.ViewModels;
using Orchard.UI.Navigation;
using Orchard.Settings;
using System.Web.Routing;
using RealEstateForum.Service;
using Orchard.Security;
using RealEstate.Services;
using RealEstate.NewLetter.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.Models;
using RealEstate.ViewModels;


namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    [Themed]
    public class PostForumFrontEndController : Controller, IUpdateModel
    {
        private readonly IThreadAdminService _threadService;
        private readonly IPostAdminService _postService;
        private readonly ISiteService _siteService;
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IFacebookApiService _facebookApiSevice;
        private readonly ICommentService _commentService;
        private readonly IPropertySettingService _propertySetting;
        private readonly IHostNameService _hostNameService;
        private readonly IForumFriendService _friendService;
        private readonly IPropertyService _propertyService;
        private readonly IFileCacheService _fileCacheService;
        private readonly IUserPersonalService _userRealEstateService;
        private readonly IFastFilterService _fastfilterService;
        private readonly IUserGroupService _groupService;

        private const string DefaultPath = "/Media/ForumPost/Images";//Media/Forum/Post


        public PostForumFrontEndController(
            IOrchardServices services,
            ISiteService siteService, 
            IThreadAdminService threadService, 
            IShapeFactory shapeFactory, 
            IPostAdminService postService, 
            IPostForumFrontEndService postForumService,
            IFacebookApiService facebookApiSevice,
            IHostNameService hostNameService,
            ICommentService commentService,
            IForumFriendService friendService,
            IPropertySettingService propertySetting,
            IFileCacheService fileCacheService,
            IUserPersonalService userRealEstateService,
            IFastFilterService fastfilterService,
            IPropertyService propertyService,
            IUserGroupService groupService)
        {
            _threadService = threadService;
            _postService = postService;
            _postForumService = postForumService;
            _siteService = siteService;
            _friendService = friendService;
            _facebookApiSevice = facebookApiSevice;
            _commentService = commentService;
            _hostNameService = hostNameService;
            _propertySetting = propertySetting;
            _propertyService = propertyService;
            _userRealEstateService = userRealEstateService;
            _fileCacheService = fileCacheService;
            _fastfilterService = fastfilterService;
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

        public ActionResult Index()
        {
            return View();
        }

        #region Đăng bài viết

        [Authorize]
        public ActionResult Create(int topicId = 0, int threadId = 0)
        {

            return Redirect("/");

            //var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");

            //var editor = Shape.EditorTemplate(
            //    TemplateName: "Parts/PostFrontEnd.Create",
            //    Model: BuildPostForumCreateViewModel(topicId, threadId),
            //    Prefix: null);
            //editor.Metadata.Position = "2";
            //dynamic model = Services.ContentManager.BuildEditor(postForum);
            //model.Content.Add(editor);

            //// Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            //return View((object)model);
        }

        [HttpPost,ValidateInput(false)]
        [Authorize]
        public ActionResult Create(PostForumCreateViewModel createModel)
        {
            return Redirect("/");


            #region Validate
            string hostname = _hostNameService.GetHostNameSite();
            if (!string.IsNullOrEmpty(createModel.Title) && _postService.CheckIsExistPostTitle(createModel.Title, hostname))
                AddModelError("IsExistTitle", T("Tên bài viết đã tồn tại. Vui lòng chọn tên khác."));

            #endregion

            var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var cssImage = Request.Files["CssImage"];

            if (ModelState.IsValid)
            {
                postForum.Thread = _threadService.GetTopicPartRecordById(createModel.TopicId, hostname).Record;
                postForum.Title = createModel.Title.Trim();

                createModel.Content = HttpUtility.HtmlDecode(createModel.Content.Trim());
                postForum.Content = !Services.Authorizer.Authorize(Permissions.RelAddAttribute) ? createModel.Content.Normalize().HtmlLinkAddRedirectAndNofollow() : createModel.Content.Normalize();

                postForum.UserPost = user.Record;
                postForum.DateCreated = DateTime.Now;
                postForum.DateUpdated = DateTime.Now;
                postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(createModel.PublishStatusId).Record;
                postForum.StatusPost = _threadService.GetStatusForumPartRecordById(createModel.StatusPostId).Record;
                postForum.Views = 0;

                //Share blog
                postForum.IsShareBlog = createModel.IsShareBlog;
                postForum.Blog = createModel.IsShareBlog ? user.Record : null;
                postForum.BlogDateCreated = createModel.IsShareBlog ? DateTime.Now : (DateTime?)null;
                postForum.HostName = _hostNameService.GetHostNameSite();


                Services.ContentManager.Create(postForum);
                postForum.BlogPostId = createModel.IsShareBlog ? (int?)postForum.Id : null;

                #region Upload CssImage
                if (cssImage != null && cssImage.ContentLength > 0)
                {
                    //postForum.CssImage = _postService.UploadFilePostCssImage(cssImage, createModel.TopicId, postForum.Id);
                    string fileLocation = DefaultPath + "/" + Guid.NewGuid() + Path.GetExtension(cssImage.FileName);
                    cssImage.SaveAsFileLocation(fileLocation);
                }
                #endregion

                #region Clear Cache

                _postService.ClearListPostFromTopicCache(postForum, hostname);//Clear Cache

                #endregion

                #region Save Meta

                _postService.M_UpdateMetaDescriptionKeywords(postForum, false);

                #endregion

                #region Topic Post FaceBook

                Session["ForumPostFacebook"] = createModel.AcceptPostToFacebok ? postForum.Id : 0;

                #endregion

                Services.Notifier.Information(T("Đã thêm bài viết <a href=\"{0}\">{1}</a> thành công.", Url.Action("Edit", "PostForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", Id = postForum.Id, title = postForum.Title.ToSlug() }), postForum.Title));

            }

            dynamic model = Services.ContentManager.UpdateEditor(postForum, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/PostFrontEnd.Create",
                    Model: BuildPostForumCreateViewModel(createModel.TopicId, createModel.ThreadId), Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            string displayForNameForUrl = postForum.Title.ToSlug().Count() > 100 ? postForum.Title.ToSlug().Substring(0, 100) : postForum.Title.ToSlug();
            return RedirectToAction("PostDetail", "PostForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", Id = postForum.Id, ThreadShortName = _threadService.GetThreadInfoById(hostname,postForum.Thread.ParentThreadId.Value).ShortName, TopicShortName = postForum.Thread.ShortName, Title = displayForNameForUrl });
        }
        #endregion

        #region Cập nhật bài viết

        [Authorize]
        public ActionResult Edit(int id, string returnUrl)// PostTitle.ToSlug()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _hostNameService.GetHostNameSite();
            if (_postService.CheckIsOwnerOrManagerPost(user.Id, id, hostname))
            {
                var postForum = Services.ContentManager.Get<ForumPostPart>(id);
                var editor = Shape.EditorTemplate(TemplateName: "Parts/PostFrontEnd.Edit",
                Model: BuildPostForumEditViewModel(postForum), Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(postForum);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            else
            {
                return Redirect("/");
            }
        }

        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(int id)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _hostNameService.GetHostNameSite();
            if (user != null && _postService.CheckIsOwnerOrManagerPost(user.Id, id, hostname))
            {
                var postForum = Services.ContentManager.Get<ForumPostPart>(id);
                string oldCssIamge = postForum.CssImage;

                dynamic model = Services.ContentManager.UpdateEditor(postForum, this);
                var editModel = new PostForumEditViewModel { ForumPost = postForum };

                var cssImage = Request.Files["CssImage"];

                if (TryUpdateModel(editModel))
                {
                    if (!string.IsNullOrEmpty(editModel.Title) && _postService.CheckIsExistPostTitle(postForum.Id, editModel.Title, hostname))
                        AddModelError("IsExistTitle", T("Tên bài viết đã tồn tại. Vui lòng chọn tên khác."));

                    if (ModelState.IsValid)
                    {

                        #region Update Model

                        postForum.Thread = _threadService.GetTopicPartRecordById(editModel.TopicId, hostname).Record;
                        postForum.Title = editModel.Title;

                        editModel.Content = HttpUtility.HtmlDecode(editModel.Content.Trim());
                        postForum.Content = !Services.Authorizer.Authorize(Permissions.RelAddAttribute) ? editModel.Content.Normalize().HtmlLinkAddRedirectAndNofollow() : editModel.Content.Normalize();

                        postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(editModel.PublishStatusId).Record;
                        postForum.StatusPost = _threadService.GetStatusForumPartRecordById(editModel.StatusPostId).Record;
                        postForum.DateUpdated = DateTime.Now;

                        #endregion

                        #region Share to My Blog

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
                                    postForum.BlogDateCreated = editModel.IsShareBlog ? DateTime.Now : (DateTime?) null;
                                    postForum.BlogPostId = postForum.Id;
                                }
                            }
                            else
                            {
                                _postService.ShareToMyBlog(postForum, user, hostname);
                            }
                        }

                        #endregion

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
                            string fileLocation = folderLocation + fileNameUpload;

                            cssImage.SaveAsFileLocation(fileLocation);
                        }

                        #endregion

                        #region Clear Cache
                        _postService.ClearListPostFromTopicCache(postForum, hostname);//Clear Cache
                        #endregion

                        #region Save Meta

                        _postService.M_UpdateMetaDescriptionKeywords(postForum, true);

                        #endregion

                        #region Topic Post FaceBook

                        Session["ForumPostFacebook"] = editModel.AcceptPostToFacebok ? postForum.Id : 0;

                        #endregion

                        Services.Notifier.Information(T("Đã cập nhật bài viết thành công!"));
                    }
                }

                if (!ModelState.IsValid)
                {
                    Services.TransactionManager.Cancel();

                    var editor = Shape.EditorTemplate(TemplateName: "Parts/PostFrontEnd.Edit", Model: BuildPostForumEditViewModel(postForum), Prefix: null);
                    editor.Metadata.Position = "2";
                    model.Content.Add(editor);

                    // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                    return View((object)model);
                }
                if (!String.IsNullOrEmpty(editModel.ReturnUrl))
                {
                    return this.Redirect(editModel.ReturnUrl);
                }

                string displayForNameForUrl = postForum.Title.ToSlug().Count() > 100 ? postForum.Title.ToSlug().Substring(0, 100) : postForum.Title.ToSlug();
                return RedirectToAction("PostDetail", "PostForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", Id = postForum.Id, ThreadShortName = _threadService.GetThreadInfoById(hostname,postForum.Thread.ParentThreadId.Value).ShortName, TopicShortName = postForum.Thread.ShortName, Title = displayForNameForUrl });
            }
            return Redirect("/");
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

        public PostForumCreateViewModel BuildPostForumCreateViewModel(int topicId, int threadId)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return new PostForumCreateViewModel()
            {
                ListThread = _threadService.GetListThreadFromCache(hostname).Where(r => r.IsOpen).OrderBy(r => r.SeqOrder).Select(r => new ThreadInfo() { Id = r.Id, Name = r.Name }).ToList(),
                ListTopic = threadId > 0 ? _threadService.GetListTopicFromCache(threadId, hostname).Select(r => new ThreadInfo() { Id = r.Id, Name = r.Name }).ToList() : null,
                ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel() { Id = r.Id, Value = r.Name }).ToList(),
                ListPostStatus = _threadService.GetListPostStatusFromCache().Where(r => r.ShortName != "cho-duyet").Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                AcceptPostToFacebok = true
            };
        }
        public PostForumEditViewModel BuildPostForumEditViewModel(ForumPostPart forum)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return new PostForumEditViewModel()
            {
                ForumPost = forum,
                ListThread = _threadService.GetListThreadFromCache(hostname).Where(r => r.IsOpen).OrderBy(r => r.SeqOrder).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                ListTopic = _threadService.GetListTopicFromCache(forum.Thread.ParentThreadId.Value, hostname).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList(),
                ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                ListPostStatus = _threadService.GetListPostStatusFromCache().Where(r => r.ShortName != "cho-duyet").Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList(),
                TopicId = forum.Thread.Id,
                PublishStatusId = forum.PublishStatus.Id,
                HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                AcceptPostToFacebok = false,
                StatusPostId = forum.StatusPost.Id
            };
        }

        #region Xem chi tiết
        public ActionResult PostDetail(int id)
        {
            string hostname = _hostNameService.GetHostNameSite();
            var postForum = Services.ContentManager.Get<ForumPostPart>(id);
            if (postForum == null)
                return RedirectToAction("ForumNotFound", "ForumError", new {area = "RealEstate.MiniForum.FrontEnd"});

            #region Cache File

            var modelTemp = new PostForumFrontEnDetailViewModel
            {
                ContentFromFile = _fileCacheService.PostCacheFromFile(id)
            };

            if (modelTemp.ContentFromFile == null)
            {
                modelTemp = _postForumService.BuildPostForumDetail(postForum.Record);

                if (modelTemp == null)
                    return RedirectToAction("ForumNotFound", "ForumError", new { area = "RealEstate.MiniForum.FrontEnd" });

                modelTemp.ContentFromFile = this.RenderView("PostDetailContentFileCache", modelTemp);
                _fileCacheService.PostCacheToFile(id,modelTemp.ContentFromFile);
            }
            modelTemp.PostDetail = new PostItem {Title = postForum.Title, Id = id};

            #endregion

            #region LinkDetail

            var thread = _threadService.GetThreadInfoByTopic(hostname, postForum.Thread);
            if(thread == null)
                return RedirectToAction("ForumNotFound", "ForumError", new { area = "RealEstate.MiniForum.FrontEnd" });

            var linkDetail = Url.Action("PostDetail", "PostForumFrontEnd", new
            {
                area = "RealEstate.MiniForum.FrontEnd", 
                Id = id,
                ThreadShortName = thread.ShortName,
                TopicShortName = postForum.Thread.ShortName,
                Title = postForum.DisplayNameForUrl
            });

            #endregion

            #region Facebook

            if (Session["ForumPostFacebook"] != null)
            {
                try
                {
                    const string fDefaultImage =
                        "http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg";
                    const string domain = "http://dinhgianhadat.vn";
                    const string fCaption = "Diễn đàn bất động sản - Các thông tin thị trường BĐS mới nhất.";
                    string fMessage = postForum.Title;

                    _facebookApiSevice.PostToFaceBook(domain, linkDetail, postForum.Title,
                        postForum.Content.StripHtml(), fCaption, fMessage, fDefaultImage);

                    Session["ForumPostFacebook"] = null;
                }
                catch (Exception)
                {
                }
            }

            #endregion

            postForum.Views = postForum.Views + 1;

            var editor = Shape.DisplayTemplate(TemplateName: "Parts/View.PostDetail", Model: modelTemp, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildDisplay(postForum);
            model.Content.Add(editor);

            ViewBag.LinkDetail = linkDetail;
            ViewBag.So_Trang = 1;/////


            return View((object)model);
        }

        #endregion

        #region Search 

        public ActionResult ResultsFilter(PostFilterOptions options, PagerParameters pagerParameters)
        {
            IContentQuery<ForumPostPart, ForumPostPartRecord> _listPost;
            string hostname = _hostNameService.GetHostNameSite();
            options = _postForumService.BuildPostFilterOption(options, hostname);

            _listPost = _postForumService.ForumFilterFrontEnd(options, hostname);

            #region SLICE

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = _listPost.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = _listPost.OrderByDescending(r=> r.DateUpdated).Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #endregion

            #region BUILD MODEL
            //Services.Notifier.Information(T("Controller: {0}",options.ListTopic.Count));

            var model = new ForumSearchResults
            {
                ListPostItem = _postForumService.BuildPostEntry(results),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };


            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        #endregion

        #region Video
        public ActionResult VideoDetail(int Id)
        {
            var detail = Services.ContentManager.Get<VideoManagePart>(Id);
            if (detail == null)
                return RedirectToAction("ForumNotFound", "ForumError", new { area = "RealEstate.MiniForum.FrontEnd" });

            var domain = _groupService.GetCurrentDomainGroup().Id;
            var list = Services.ContentManager.Query<VideoManagePart, VideoManagePartRecord>().Where(p => p.DomainGroupId == domain && p.Id != detail.Id);

            var model = new VideoManageIndexViewModel
            {
                VideoManagePart = detail,
                VideoManages = list.Slice(8).Select(r => new VideoManageEntry
                {
                    VideoManagePart = r
                }).ToList(),
            };

            return View(model);
        }

        public ActionResult ListVideo()
        {
            var domain = _groupService.GetCurrentDomainGroup().Id;
            var list = Services.ContentManager.Query<VideoManagePart, VideoManagePartRecord>().Where(p => p.DomainGroupId == domain).List();

            var model = new VideoManageIndexViewModel
            {
                VideoManages = list.Select(r => new VideoManageEntry
                {
                    VideoManagePart = r
                }).ToList(),
            };

            return View(model);
        }
        #endregion

        #region Ajax

        [HttpPost]
        public JsonResult AjaxLoadTopicFrontEnd(int threadId)
        {
            string hostname = _hostNameService.GetHostNameSite();
            var listTopic = _threadService.GetListTopicFromCache(threadId, hostname).Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList();

            return Json(new { list = listTopic });
        }

        public ActionResult ChangeStatusPost(int postId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _hostNameService.GetHostNameSite();
            if (user != null && _postService.CheckIsOwnerOrManagerPost(user.Id, postId, hostname))
            {
                string IsAction = _postService.DeleteOrUndeletePostForum(postId);// True: restored, False: Deleted

                if (IsAction == "st-none")
                {
                    return Json(new { status = true, IsAction = true });//, message = "Bài viết đã được khôi phục." 
                }
                else if (IsAction == "st-bin")
                {
                    return Json(new { status = true, IsAction = false });//, message = "Bài viết đã được khôi phục." 
                }
                else return Json(new { status = false, message = "Xin lỗi, bạn không thể thực hiện chức năng này.", IsAction = IsAction });
            }
            {
                return Json(new { status = false, message = "Permision Error."});
            }
        }

        [Authorize, HttpPost, ValidateInput(false)]
        public ActionResult AjaxSubmitCommentForum(int postId, string content, int? parentCommentId)
        {
            content = !string.IsNullOrEmpty(content) ? content.StripHtml() : content;
            if (content.Length <= 5000)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    content = content.Trim();
                    ResultByPostComment isComment = (parentCommentId.HasValue && parentCommentId != 0) ? _commentService.PostSubCommentForum(postId, content, parentCommentId.Value) : _commentService.PostCommentForum(postId, content);
                    bool isAdminOrManagement = (Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum) || Services.Authorizer.Authorize(StandardPermissions.SiteOwner));

                    return isComment.IsSuccess ? Json(new { status = true, Id = isComment.Id, AvatarUser = isComment.UserInfo.Avatar, DisplayName = isComment.UserInfo.DisplayName, TimeAgo = isComment.TimeAgo, IsAdminOrManagement = isAdminOrManagement }) : Json(new { status = false, mesage = "Không thể comment vào lúc này." });
                }
                else
                {
                    return Json(new { status = false, message = "Vui lòng nhập nội dung!" });
                }
            }
            else
            {
                return Json(new { status = false, message = "Nội dung comment có thể quá dài, bạn vui lòng kiểm tra lại!" });
            }
        }
        [ValidateInput(false)]
        public ActionResult AjaxPostToFacebook(string content, int postId)
        {
            string hostname = _hostNameService.GetHostNameSite();
            if (content.Length <= 5000)
            {
                //LinkDetail- PostTitle - PostContent
                string fDefaultImage = "http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg";
                string domain = "http://dinhgianhadat.vn";
                string fCaption = "Diễn đàn bất động sản - Các thông tin thị trường BĐS mới nhất.";
                string fMessage = content.StripHtml();

                var p = Services.ContentManager.Get<ForumPostPart>(postId);

                string TitleSlug = p.Title.ToSlug();
                string DisplayUrl = TitleSlug.Count() > 100 ? _postService.MySubString(TitleSlug, 100) : TitleSlug;
                string threadShortName = _threadService.GetThreadInfoById(hostname,p.Thread.ParentThreadId.Value).ShortName;

                string LinkDetail = Url.Action("PostDetail", new { controller = "PostForumFrontEnd", area = "RealEstate.MiniForum.FrontEnd", Id = postId, title = DisplayUrl, ThreadShortName = threadShortName, TopicShortName = p.Thread.ShortName });

                _facebookApiSevice.PostToFaceBook(domain, LinkDetail, p.Title, p.Content.StripHtml(), fCaption, fMessage, fDefaultImage);
            }

            return Json(new { });
        }

        public ActionResult AjaxLoadComment(int postId, PagerParameters pagerParameters)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            int totalCount = 0;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            string hostname = _hostNameService.GetHostNameSite();
            var cList = _commentService.LoadComment(postId);
            totalCount = cList.Count();

            var results = cList.Where(r => r.ParentCommentId == 0 && r.Id != r.ParentCommentId)
                .OrderByDescending(c => c.DateCreated).OrderBy(r => r.SortOrder)
                .Slice(pager.GetStartIndex(), pager.PageSize);

            var pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);

            #region BuildModel

            var model = _commentService.BuildLoadComment(results, postId, user, hostname);
            model.TotalCount = totalCount;
            model.Pager = pagerShape;

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView(model);
        }

        public ActionResult AjaxDeleteComment(int commentId, int postId)
        {
            var user = Services.WorkContext.CurrentUser != null ? Services.WorkContext.CurrentUser.As<UserPart>() : null;
            string hostname = _hostNameService.GetHostNameSite();
            bool isDelete = _commentService.DeleteComment(commentId, postId, user.Record, hostname);

            return isDelete ? Json(new { status = true }) : Json(new { status = false, message = "Không thể xóa comment."});
        }

        public ActionResult AjaxLoadForumPostOfUser(int? userId, PagerParameters pagerParameters)
        {
            int userCurentId = Services.WorkContext.CurrentUser != null ? Services.WorkContext.CurrentUser.As<UserPart>().Id : 0;

            int totalCount = 0;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            string value = _propertySetting.GetSetting("SL_Bai_Viet_User_Widget");
            if (String.IsNullOrEmpty(value) || Convert.ToInt32(value) == 0)
            {
                value = "10";
            }

            pager.PageSize = Convert.ToInt32(value);

            if (userId == null)
                userId = 0;

            var pList = _postForumService.LoadPostOfUser(userId.Value);
            totalCount = pList.Count();

            var results = pList.OrderByDescending(p => p.DateUpdated)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            #region BUILD MODEL

            var pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);

            var model = new PostOfUserWidgetViewModel
            {
                ListPostTitle = _postForumService.BuildListPostOfUser(results),
                Pager = pagerShape,
                TotalCount = totalCount,
                IsOwner = userId.HasValue && userId.Value == userCurentId ? true : false
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView(model);
        }

        public ActionResult AjaxLoadForumMenuOfUser(int? userId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var count = new Dictionary<string, int>();
            var statusDeleted = _propertyService.GetStatus("st-trashed");

            if (user != null && user.Id == userId)//DS Ban, DS yeu cau kb, DS tin nhan da nhan
            {
                count["message"] = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(r => r.UserReceived == user.Record && !r.IsUserDelete && !r.IsRead).Count();
                count["friend"] = _friendService.ListFriendByUser(user.Record).Count();
                count["requestfriend"] = _friendService.ListFriendRequestByUser(user.Record).Count();

                count["PropertyCount"] = _propertyService.GetProperties().Where(r =>r.CreatedUser.Id == userId && r.Status != statusDeleted).Count();

                ViewBag.Count = count;

                return PartialView();
            }
            else//DS Ban
            {
                var userSelect = Services.ContentManager.Get<UserPart>(userId.Value);
                count["friend"] = userSelect != null ? _friendService.ListFriendByUser(userSelect.Record).Count() : 0;
                count["UserId"] = userId.Value;
                count["PropertyCount"] = userSelect != null ? _propertyService.GetProperties().Where(r => r.CreatedUser.Id == userId && r.Status != statusDeleted).Count() : 0;
                ViewBag.UserName = userSelect.UserName;
                ViewBag.Count = count;

                return PartialView("AjaxLoadForumMenuOfFriend");
            }
        }

        [HttpPost]
        public ActionResult AjaxDeletePost(int postId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _hostNameService.GetHostNameSite();
            if (user != null && _postService.CheckIsOwnerOrManagerPost(user.Id, postId, hostname))
            {
                try
                {
                    _postService.DeletePostForum(postId);// True: restored, False: Deleted
                    return Json(new { status = true });

                }
                catch
                {
                    return Json(new { status = false, message = "Access Error." });
                }
            }
            {
                return Json(new { status = false, message = "Permision Error." });
            }
        }

        public ActionResult AjaxPostsHighlight()
        {
            var hostname = _hostNameService.GetHostNameSite();

            var takeLength = int.Parse(_fastfilterService.GetFrontEndSetting("Posts_Highlight_PageSize") ?? "5");// _propertySetting.GetSetting("Posts_Highlight_PageSize");

            var listPost = _postService.GetListPostQueryByHostName(hostname)
                            .Where(r => r.IsHeighLight)
                            .OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(takeLength));

            List<PostByTopicEntry> ListPost =  listPost.Select(r => new PostByTopicEntry
            {
                Id = r.Id,
                Title = r.Title,
                ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                TopicShortName = r.Thread.ShortName,
                DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                Content = _postService.MySubString(r.Content.StripHtml(), 200),
                UserInfo = new UserInfo()
                {
                    Id = r.UserPost.Id,
                    UserName = r.UserPost.UserName,
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                    Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id)
                }
            }).ToList();


            return PartialView("AjaxPostsHighlight", ListPost);
        }

        public ActionResult AjaxPostsIsProject()
        {
            var hostname = _hostNameService.GetHostNameSite();

            var takeLength = int.Parse(_fastfilterService.GetFrontEndSetting("Posts_Project_PageSize") ?? "5");

            var listPost = _postService.GetListPostQueryByHostName(hostname)
                            .Where(r => r.IsProject)
                            .OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(takeLength));

            List<PostByTopicEntry> ListPost = listPost.Select(r => new PostByTopicEntry
            {
                Id = r.Id,
                Title = r.Title,
                ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                TopicShortName = r.Thread.ShortName,
                DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                DefaultImage =
                      _postService.GetDefaultImageUrl(r,
                          _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                Content = _postService.MySubString(r.Content.StripHtml(), 200),
                UserInfo = new UserInfo()
                {
                    Id = r.UserPost.Id,
                    UserName = r.UserPost.UserName,
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                    Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id)
                }
            }).ToList();


            return PartialView("AjaxPostsIsProject", ListPost);
        }

        public ActionResult AjaxPostIsLawHousing()
        {
            string hostname = _hostNameService.GetHostNameSite();
            int lawId = 611421;

            var valueWithImage = _propertySetting.GetSetting("Posts_LawHouse_PageSize");

            if (String.IsNullOrEmpty(valueWithImage) || Convert.ToInt32(valueWithImage) == 0)
            {
                valueWithImage = "5";
            }
            //lay tat ca cac chuyen de cua chuyen muc phap-luat-nha-dat
            var listThreadId = _threadService.GetListTopicFromCache(lawId, hostname).Select(a => a.Id).ToList();
            //danh sach cac bai viet trong cac chuyen de phap-luat-nha-dat
            var listLawMain = _postService.GetListPostQueryByHostName(hostname)
                .Where(r => listThreadId.Contains(r.Thread.Id))
                .OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(valueWithImage));

            //danh sach cac bai viet la tin chinh hien thi

            var model = new WidgetIsMarketViewModel
            {
                ListPostWithImage = listLawMain.Select(r => new PostByTopicEntry
                {
                    Id = r.Id,
                    Title = r.Title,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    TopicShortName = r.Thread.ShortName,
                    DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                    Content = _postService.MySubString(r.Content.StripHtml(), 200),
                    UserInfo = new UserInfo()
                    {
                        Id = r.UserPost.Id,
                        UserName = r.UserPost.UserName,
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                        Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id)
                    }
                }).ToList()
            };

            return PartialView("AjaxPostIsLawHousing", model);
        }

        public ActionResult AjaxPostIsMarket()
        {
            string hostname = _hostNameService.GetHostNameSite();
            var valueWithImage = _propertySetting.GetSetting("SL_Tin_Tuc_Thi_Truong_Hien_Thi"); //_forumSettingsService.GetValueSettingDisplay("SL_Tin_Tuc_Thi_Truong_Hien_Thi").Value;
            var valueNotImage = _propertySetting.GetSetting("SL_Bai_Viet_Tin_Tuc_Thi_Truong");//_forumSettingsService.GetValueSettingDisplay("SL_Bai_Viet_Tin_Tuc_Thi_Truong").Value;
            if (String.IsNullOrEmpty(valueWithImage) || Convert.ToInt32(valueWithImage) == 0)
            {
                valueWithImage = "5";
            }
            if (String.IsNullOrEmpty(valueNotImage) || Convert.ToInt32(valueNotImage) == 0)
            {
                valueNotImage = "5";
            }

            var resultWithImage = _postService.GetListPostQueryByHostName(hostname).Where(r => r.IsMarket).OrderByDescending(r => r.DateUpdated).Slice(4);
            var listId = resultWithImage.Select(r => r.Id).ToList();

            var resultNotImage = _postService.GetListPostQueryByHostName(hostname).Where(r => r.IsMarket && !listId.Contains(r.Id)).OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(valueNotImage));

            var model = new WidgetIsMarketViewModel
            {
                ListPostWithImage = resultWithImage.Select(r => new PostByTopicEntry()
                {
                    Id = r.Id,
                    Title = r.Title,
                    DefaultImage =
                        _postService.GetDefaultImageUrl(r,
                            _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    TopicShortName = r.Thread.ShortName,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    Content = _postService.MySubString(r.Content.StripHtml(), 200)
                }).ToList(),
                ListPostNotImage = resultNotImage.Select(r => new WidgetPostItem()
                {
                    Id = r.Id,
                    Title = r.Title,
                    DefaultImage =
                        _postService.GetDefaultImageUrl(r,
                            _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    TopicShortName = r.Thread.ShortName,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName
                }).ToList()
            };

            return PartialView("AjaxPostIsMarket", model);
        }

        public ActionResult AjaxAllPostInThread(int threadid, int shwval, int lstval)
        {
            string hostname = _hostNameService.GetHostNameSite();

            var resultWithImage = _postService.GetListPostQueryByHostName(hostname).Where(r => r.Thread.Id == threadid).OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(shwval));
            var listId = resultWithImage.Select(r => r.Id).ToList();

            var resultNotImage = _postService.GetListPostQueryByHostName(hostname).Where(r => r.Thread.Id == threadid && !listId.Contains(r.Id)).Slice(Convert.ToInt32(lstval));


            var model = new WidgetIsMarketViewModel
            {
                ListPostWithImage = resultWithImage.Select(r => new PostByTopicEntry
                {
                    Id = r.Id,
                    Title = r.Title,
                    DefaultImage =
                        _postService.GetDefaultImageUrl(r,
                            _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    TopicShortName = r.Thread.ShortName,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    Content = _postService.MySubString(r.Content.StripHtml(), 500)
                }).ToList(),
                ListPostNotImage = resultNotImage.Select(r => new WidgetPostItem
                {
                    Id = r.Id,
                    Title = r.Title,
                    DefaultImage =
                        _postService.GetDefaultImageUrl(r,
                            _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    TopicShortName = r.Thread.ShortName,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName
                }).ToList()
            };


            return PartialView("AjaxAllPostInThread", model);
        }

        public ActionResult AjaxPostIsNewsMain(int shwval, int lstval)
        {
            int threadId = 880570;//Diễn Đàn
            string hostname = _hostNameService.GetHostNameSite();
            var _notShowThreadId = _threadService.GetListTopicQueryByThreadId(hostname, threadId).List().Select(r => r.Id).ToList();

            var resultWithImage = _postService.GetListPostQueryByHostName(hostname).Where(a=> !_notShowThreadId.Contains(a.Thread.Id)).OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(shwval));
            var listId = resultWithImage.Select(r => r.Id).ToList();

            var resultNotImage = _postService.GetListPostQueryByHostName(hostname).Where(r => !listId.Contains(r.Id) && !_notShowThreadId.Contains(r.Thread.Id)).Slice(Convert.ToInt32(lstval));


            var model = new WidgetIsMarketViewModel
            {
                ListPostWithImage = resultWithImage.Select(r => new PostByTopicEntry
                {
                    Id = r.Id,
                    Title = r.Title,
                    DefaultImage =
                        _postService.GetDefaultImageUrl(r,
                            _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    TopicShortName = r.Thread.ShortName,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    ThreadId = _threadService.GetThreadInfoByTopic(hostname, r.Thread).Id,
                    Content = _postService.MySubString(r.Content.StripHtml(), 500),
                }).ToList(),
                ListPostNotImage = resultNotImage.Select(r => new WidgetPostItem
                {
                    Id = r.Id,
                    Title = r.Title,
                    DefaultImage =
                        _postService.GetDefaultImageUrl(r,
                            _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    TopicShortName = r.Thread.ShortName,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    ThreadId = _threadService.GetThreadInfoByTopic(hostname, r.Thread).Id,
                }).ToList()
            };


            return PartialView("AjaxPostIsNewsMain", model);
        }

        public ActionResult AjaxPostInThread(int threadid)
        {
            string hostname = _hostNameService.GetHostNameSite();
            //danh sach cac bai viet trong cac chuyen de văn bản quy pham trang bannhadatnhatrang.com
            var listLawMain = _postService.GetListPostQueryByHostName(hostname)
                .Where(r => r.Thread.Id == threadid)
                .OrderByDescending(r => r.DateUpdated).Slice(5);

            //danh sach cac bai viet la tin chinh hien thi
            var model = new WidgetIsMarketViewModel
            {
                ListPostWithImage = listLawMain.Select(r => new PostByTopicEntry
                {
                    Id = r.Id,
                    Title = r.Title,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    TopicShortName = r.Thread.ShortName,
                    DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                    Content = _postService.MySubString(r.Content.StripHtml(), 200),
                    UserInfo = new UserInfo()
                    {
                        Id = r.UserPost.Id,
                        UserName = r.UserPost.UserName,
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                        Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id)
                    }
                }).ToList()
            };

            return PartialView("AjaxPostInThread", model);
        }


        public ActionResult AjaxGetVideo()
        {
            var getDomain = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            var domain = getDomain != null ? getDomain.Id : 103993;

            var list = Services.ContentManager.Query<VideoManagePart, VideoManagePartRecord>().Where(p => p.DomainGroupId == domain).OrderByDescending(a => a.Id);

            var model = new VideoManageIndexViewModel
            {
                VideoManagePart = list.Slice(1).FirstOrDefault(),
            };

            return PartialView("AjaxGetVideo", model);
        }
        #endregion


    }
}