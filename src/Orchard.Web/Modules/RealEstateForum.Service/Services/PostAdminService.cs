using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;

using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.UI.Notify;

using RealEstateForum.Service.ViewModels;
using RealEstateForum.Service.Models;
using Vandelay.Industries.Models;
using RealEstate.Helpers;
using RealEstate.Services;
using Orchard.Users.Models;
using System.Text.RegularExpressions;
using Orchard.Data;

namespace RealEstateForum.Service.Services
{
    public interface IPostAdminService : IDependency
    {
        #region //Post

        ForumPostPart GetForumPostPartRecord(int id, string hostname);

        IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostQueryByHostName(string hostname);
        IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostQueryThreadId(int threadId, string hostName);
        IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostForumByTopic(int topicId, string hostname);
        IContentQuery<ForumPostPart, ForumPostPartRecord> GetPosts(string hostname);
        void ClearPostPinedExpired(string hostname);

        
        void ClearListPostFromTopicCache(ForumPostPart p, string hostname);
        int CountPostByTopicToCache(int topicId, string hostname);

        #endregion

        #region //Blog

        IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostFromBlogUserIdCache(int userId);
        void ShareToMyBlog(ForumPostPart post, UserPart user, string hostname);

        #endregion

        #region//Check Exist

        bool CheckIsExistPostTitle(string title, string hostname);
        bool CheckIsExistPostTitle(int postId, string title, string hostname);
        bool CheckIsOwnerOrManagerPost(int userId, int postId, string hostname);

        string UploadFileFolderDefault(HttpPostedFileBase file, string hostCurrent, int uploadId);

        #endregion

        #region//Save Meta

        void M_UpdateMetaDescriptionKeywords(string hostname);
        void M_UpdateMetaDescriptionKeywords(ForumPostPart p, bool overwrite);

        #endregion

        #region//Build

        PostIndexOptions BuildPostIndexOption(PostIndexOptions options, string hostname);
        IContentQuery<ForumPostPart, ForumPostPartRecord> SearchForumPost(PostIndexOptions options, string hostname);

        #endregion

        #region //Delete/Undelete
        void DeletePostForum(int postId);
        void UnDeletePostForum(int postId);
        void ApprovePostForum(int postId);
        string DeleteOrUndeletePostForum(int postId);
        bool PerDeletePostById(int postId);
        #endregion

        #region  // Helper

        string StripTagsReplace(string source);
        string StripTagsRegex(string source);
        string GetImagesSrc(string source);
        string MySubString(string inputText, int length);

        #endregion

        string GetDefaultImageUrl(ForumPostPart p, string threadShortName);
        void ReplaceUserForumPost(int userFrom, int userTo);
    }
    public class PostAdminService : IPostAdminService
    {
        #region //Init
        private readonly IRepository<CommentForumPartRecord> _commentRepository;
        private readonly IRepository<ForumPostPartRecord> _forumPostRepository;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private int cacheTimeSpan = 60 * 24; // Cache for 24 hours
        private readonly IThreadAdminService _threadService;
        private readonly IUserGroupService _userGroupService;
        private readonly IHostNameService _hostNameService;
        private readonly IFileCacheService _fileCacheService;

        public PostAdminService(IOrchardServices services,
            IRepository<CommentForumPartRecord> commentRepository,
            IRepository<ForumPostPartRecord> forumPostRepository,
            IContentManager contentManager,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IThreadAdminService threadService,
            IHostNameService hostNameService,
            IUserGroupService userGroupService,
            IFileCacheService fileCacheService)
        {
            _commentRepository = commentRepository;
            Services = services;
            _contentManager = contentManager;
            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _hostNameService = hostNameService;
            _threadService = threadService;
            _userGroupService = userGroupService;
            _fileCacheService = fileCacheService;
            _forumPostRepository = forumPostRepository;


            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        #endregion

        private static readonly Regex RegexUrl1 = new Regex(@"\[url\=([^\]]+)\]([^\]]+)\[/url\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex RegexUrl2 = new Regex(@"\[url\](.+?)\[/url\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #region Post

        public ForumPostPart GetForumPostPartRecord(int id, string hostname)
        {
            return GetListPostQueryByHostName(hostname).Where(r=>r.Id == id).Slice(1).Select(r=>r).FirstOrDefault();
        }

        public IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostQueryByHostName(string hostname)// Tất cả bài viết theo host name.
        {
            // Lấy những bài viết có trạng thái 
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            IContentQuery<ForumPostPart, ForumPostPartRecord> result = GetPosts(hostname);
            var onlyPublishStatus = _threadService.GetPublishStatusPartRecordById(611412);//.GetPublishStatusPartRecordByName("Chỉ mình tôi");

            if (currentUser == null)
                result = result.Where(r => r.PublishStatus != onlyPublishStatus.Record);
            else
            {
                result =
                    result.Where(
                        r =>
                            r.PublishStatus != onlyPublishStatus.Record ||
                            (r.PublishStatus == onlyPublishStatus.Record && r.UserPost == currentUser.Record));
            }

            return result;
        }
        
        public IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostQueryThreadId(int threadId,string hostName)
        {
            var listTopicIds = _threadService.GetListTopicQueryByThreadId(hostName,threadId).List().Select(r=>r.Id).ToList();

            return GetListPostQueryByHostName(hostName).Where(r => listTopicIds.Contains(r.Thread.Id));
        }

        public IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostForumByTopic(int topicId, string hostname)
        {
            return GetListPostQueryByHostName(hostname).Where(r => r.Thread.Id == topicId);
        }

        public IContentQuery<ForumPostPart, ForumPostPartRecord> GetPosts(string hostname)
        {
            var result = _contentManager.Query<ForumPostPart, ForumPostPartRecord>()
               .Where(r =>
                   r.Thread != null
                       //&& !r.IsShareBlog
                   && r.StatusPost != null
                   //&& r.StatusPost == _threadService.GetStatusForumPartRecordByCssClass("st-none").Record//Update 12/09/2015
                   );

            result = hostname == _hostNameService.GetHostNamePartByClass("host-name-main").Name
                    ? result.Where(r => r.HostName == hostname || r.HostName == null)
                    : result.Where(r => r.HostName == hostname);

            return result;
        }

        public void ClearPostPinedExpired(string hostname)
        {
            var listpost = GetListPostQueryByHostName(hostname).Where(r=>r.IsPinned && r.TimeExpiredPinned < DateTime.Now).List();

            foreach(var item in  listpost)
            {
                item.IsPinned = false;
                item.TimeExpiredPinned = null;
            }
        }

        public void ClearListPostFromTopicCache(ForumPostPart p, string hostname)
        {
            _signals.Trigger("BuildListPostByThreadCache_" + p.Thread.Id + "_" + hostname);
            if (p.Thread.ParentThreadId != null)
                _fileCacheService.ClearCacheFileByThreadId(p.Thread.ParentThreadId.Value);

            _fileCacheService.ClearCacheFileByPostId(p.Id);
            //if (p.Blog != null) ClearListPostFromBlogUserIdCache(p.Blog.Id);
        }

        public int CountPostByTopicToCache(int topicId, string hostname)
        {
            return GetListPostQueryByHostName(hostname).Where(r => r.Thread != null && r.Thread.Id == topicId).Count();
        }

        #endregion

        #region //Blog
        public IContentQuery<ForumPostPart, ForumPostPartRecord> GetListPostFromBlogUserIdCache(int userId)
        {
               return _contentManager.Query<ForumPostPart, ForumPostPartRecord>()
                   .Where(r => r.Blog != null && r.Blog.Id == userId).OrderByDescending(r=> r.BlogDateCreated);
        }
        
        public void ShareToMyBlog(ForumPostPart post, UserPart user, string hostname)
        {
            var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");
            postForum.Title = "Share my blog - " + user.UserName;
            postForum.Description = post.Description;
            postForum.Content = MySubString(post.Content, 300);
            postForum.Thread = null;
            postForum.StatusPost = null;
            postForum.PublishStatus = null;
            postForum.DateCreated = DateTime.Now;
            postForum.DateUpdated = DateTime.Now;
            postForum.UserPost = user.Record;
            postForum.Views = 0;

            postForum.Blog = user.Record;
            postForum.BlogPostId = post.Id;
            postForum.BlogDateCreated = DateTime.Now;
            postForum.IsShareBlog = true;

            Services.ContentManager.Create(postForum);

        }
        #endregion

        #region//Check Exist
        public bool CheckIsExistPostTitle(string title, string hostname)
        {
            string titleTemp = title.Trim();
            return GetListPostQueryByHostName(hostname).Where(r => r.Title == titleTemp).Count() > 0;
        }
        public bool CheckIsExistPostTitle(int postId, string title, string hostname)
        {
            string tempTitle = title.Trim();
            return GetListPostQueryByHostName(hostname).Where(r => r.Id != postId && r.Title == tempTitle).Count() > 0;
        }
        public int CheckIsExistPostTitle(UserPart user, string title, string hostname)
        {
            string titleTemp = title.Trim();
            var postItem = GetListPostQueryByHostName(hostname).Where(r => r.IsShareBlog && r.UserPost == user.Record && r.Title == titleTemp);
            return postItem.Count() > 0 ? postItem.Slice(1).First().Id : 0;
        }
        public bool CheckIsOwnerOrManagerPost(int userId, int postId, string hostname)
        {
            if (Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum))//Nếu là quản lý forum.
                return true;

            return GetListPostQueryByHostName(hostname).Where(r => r.Id == postId && r.UserPost.Id == userId).Count() > 0;// Nếu là chủ của bài viết.
        }
        #endregion

        #region Upload Image

        public string UploadFileFolderDefault(HttpPostedFileBase file, string hostCurrent, int uploadId)
        {
            string uploadsFolder = Services.WorkContext.HttpContext.Server.MapPath("~/Media/Default/" + hostCurrent);
            var folder = new DirectoryInfo(uploadsFolder);

            if (!folder.Exists) folder.Create();

            string fileNameUpload = uploadId + Path.GetExtension(file.FileName);
            string fileLocation = "~/Media/Default/" + hostCurrent + "/" + fileNameUpload;

            //fileBase.SaveAs(Services.WorkContext.HttpContext.Server.MapPath(fileLocation));

            if (file.ContentLength < 250000) // file nhỏ hơn 250k không cần resize
            {
                file.SaveAs(Services.WorkContext.HttpContext.Server.MapPath(fileLocation));
            }
            else
            {
                var i = new ImageResizer.ImageJob(file, fileLocation, new ImageResizer.ResizeSettings("maxwidth=1024;format=jpg;mode=max"))
                {
                    CreateParentDirectory = true
                };
                i.Build();
            }

            return fileLocation.Replace("~", "");
        }

        #endregion

        #region //Update Meta 
        //Save Meta
        public void M_UpdateMetaDescriptionKeywords(string hostname)
        {
            var pList = GetListPostQueryByHostName(hostname).List();
            foreach (var item in pList)
            {
                M_UpdateMetaDescriptionKeywords(item, false);
            }
        }
        public void M_UpdateMetaDescriptionKeywords(ForumPostPart p, bool overwrite)
        {
            var pMeta = p.As<MetaPart>();
            if (overwrite == true || String.IsNullOrEmpty(pMeta.Keywords))
            {
                pMeta.Keywords = p.Title;
            }
            if (overwrite == true || String.IsNullOrEmpty(pMeta.Description))
            {
                if (!String.IsNullOrEmpty(p.Description))
                {
                    var decode = HttpUtility.HtmlDecode(p.Description);
                    pMeta.Description = decode.StripHtml();
                }
                else
                {
                    var decode = HttpUtility.HtmlDecode(p.Content);
                    pMeta.Description = decode.StripHtml().Length > 100 ? decode.StripHtml().Substring(0, 100) : decode.StripHtml();
                }
            }
        }

        #endregion

        #region //Build

        public PostIndexOptions BuildPostIndexOption(PostIndexOptions options, string hostname)
        {
                if (options == null)
                {
                    options = new PostIndexOptions
                    {
                        ThreadIdIndex = 0,
                        PublishStatusId = 0,
                        PostStatusId = 0,
                        PostId = 0
                    };
                }

                options.ListPostStatus = _threadService.GetListPostStatusFromCache().Select(r => new HashViewModel() { Id = r.Id, Value = r.Name }).ToList();
                options.ListPublishStatus = _threadService.GetListPublishStatusFromCache().Select(r => new HashViewModel() { Id = r.Id, Value = r.Name }).ToList();
                options.ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo() { Id = r.Id, Name = r.Name }).ToList();

                if (options.ThreadIdIndex == null)
                    options.ThreadIdIndex = 0;

                options.ListTopic = _threadService.GetListTopicFromCache(options.ThreadIdIndex.Value, hostname).Select(r => new ThreadInfo() { Id = r.Id, Name = r.Name }).ToList();

            return options;
        }
        public IContentQuery<ForumPostPart, ForumPostPartRecord> SearchForumPost(PostIndexOptions options, string hostname)
        {
            IContentQuery<ForumPostPart, ForumPostPartRecord> results = GetListPostQueryByHostName(hostname);
            //_contentManager.Query<ForumPostPart, ForumPostPartRecord>().Where(r => r.Thread != null);// Lấy những bài viết theo chuyên đề & host name

            //Find by hostname
            //results = hostname == _hostNameService.GetHostNamePartByClass("host-name-main").Name ? results.Where(r => (r.HostName == null || r.HostName == hostname)) : results.Where(r => r.HostName == hostname);
            
            //if (!options.IsAdminPage)
            //_results = _results.Where(r => r.StatusPost.Id == GetStatusForumPartRecordByCssClass("st-none").Id);

            //Find by Thread
            if (options.ThreadIdIndex != 0 && options.ThreadIdIndex.HasValue)
            {
                var listTopicId = _threadService.GetListTopicFromCache(options.ThreadIdIndex.Value, hostname).Select(r => r.Id).ToArray();
                results = results.Where(r => listTopicId.Contains(r.Thread.Id));
            }

            //Find by TopicIds
            if (options.TopicIds != null && options.TopicIds.Any())
            {
                results = results.Where(r => options.TopicIds.Contains(r.Thread.Id));
            }

            //Find by Id
            if (options.PostId.HasValue)
                results = results.Where(r => r.Id == options.PostId.Value);

            //Find by SearchText
            if (!string.IsNullOrEmpty(options.SearchText))
                results = results.Where(r => r.Content.Contains(options.SearchText));

            //Find by post title
            if (!string.IsNullOrEmpty(options.Title))
                results = results.Where(r => r.Title.Contains(options.Title.Trim()));

            //find from DateCreated
            if (!string.IsNullOrEmpty(options.DateCreateFrom) && !string.IsNullOrEmpty(options.DateCreateTo))
            {
                const string format = "dd/MM/yyyy";
                System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
                var style = System.Globalization.DateTimeStyles.AdjustToUniversal;
                DateTime createdFrom, createdTo;

                DateTime.TryParseExact(options.DateCreateFrom, format, provider, style, out createdFrom);
                DateTime.TryParseExact(options.DateCreateTo, format, provider, style, out createdTo);

                if (createdFrom.Year > 1) results = results.Where(p => p.DateCreated >= createdFrom);
                if (createdTo.Year > 1) results = results.Where(p => p.DateCreated <= createdTo.AddHours(24));
            }

            //Find by UserNameOrEmail
            //options.UserNameOrEmail = !string.IsNullOrEmpty(options.UserNameOrEmail) ? options.UserNameOrEmail.Trim() : "";
            if (!string.IsNullOrEmpty(options.UserNameOrEmail))
            {
                int? userId = _userGroupService.GetUsers().Where(r => r.UserName == options.UserNameOrEmail || r.Email == options.UserNameOrEmail).Select(r => r.Id).FirstOrDefault();
                if (userId.HasValue)
                    results = results.Where(r => r.UserPost.Id == userId);
            }

            // Find by PublishStatusId
            if (options.PublishStatusId != 0)
                results = results.Where(r => r.PublishStatus.Id == options.PublishStatusId);

            //Find by StatusPostId
            if (options.PostStatusId != 0)
                results = results.Where(r => r.StatusPost.Id == options.PostStatusId);

            //Find by IsProject
            if (options.IsProject)
                results = results.Where(r => r.IsProject);

            //Find by IsMarket
            if (options.IsMarket)
                results = results.Where(r => r.IsMarket);

            //Find by IsPinned
            if (options.IsPinned)
                results = results.Where(r => r.IsPinned);

            //Find by IsHeighlight
            if (options.IsHeighLight)
                results = results.Where(r => r.IsHeighLight);

            return results;
        }

        #endregion

        #region //Delete
        public string DeleteOrUndeletePostForum(int postId)
        {
            var postForum = Services.ContentManager.Get<ForumPostPart>(postId);
            if (postForum != null && postForum.StatusPost.Id == _threadService.GetStatusForumPartRecordByCssClass("st-bin").Id)
            {
                postForum.StatusPost = _threadService.GetStatusForumPartRecordByCssClass("st-none").Record;
                return "st-none";
            }
            if (postForum == null ||
                postForum.StatusPost.Id != _threadService.GetStatusForumPartRecordByCssClass("st-none").Id)
                return "none";

            postForum.StatusPost = _threadService.GetStatusForumPartRecordByCssClass("st-bin").Record;
            return "st-bin";
        }

        //ApprovePostForum
        public void ApprovePostForum(int postId)
        {
            var postForum = Services.ContentManager.Get<ForumPostPart>(postId);
            if (postForum != null)
            {
                postForum.StatusPost = _threadService.GetStatusForumPartRecordByCssClass("st-none").Record;
            }
        }

        public void DeletePostForum(int postId)
        {
            var postForum = Services.ContentManager.Get<ForumPostPart>(postId);
            if (postForum == null) return;

            //postForum.StatusPost = GetStatusForumPartRecordByCssClass("st-bin");
            Services.ContentManager.Remove(postForum.ContentItem);
            _forumPostRepository.Delete(postForum.Record);

            var lstComment = _commentRepository.Fetch(r => r.ForumPost.Id == postId);
            var commentForumPartRecords = lstComment as CommentForumPartRecord[] ?? lstComment.ToArray();

            if (!commentForumPartRecords.Any()) return;

            foreach (var item in commentForumPartRecords)
            {
                _commentRepository.Delete(item);// Xóa tất cả comment thuộc bài viết này.
            }
        }
        public void UnDeletePostForum(int postId)
        {
            var postForum = Services.ContentManager.Get<ForumPostPart>(postId);
            if (postForum != null)
            {
                postForum.StatusPost = _threadService.GetStatusForumPartRecordByCssClass("st-none").Record;
            }
        }

        public bool PerDeletePostById(int postId)
        {
            var postForum = Services.ContentManager.Get<ForumPostPart>(postId);
            if (postForum == null) return false;

            if (postForum.IsShareBlog)
            {
                postForum.IsShareBlog = false;
            }
            else
            {
                Services.ContentManager.Remove(postForum.ContentItem);
                _forumPostRepository.Delete(postForum.Record);

                var lstComment = _commentRepository.Fetch(r => r.ForumPost.Id == postId);
                var commentForumPartRecords = lstComment as CommentForumPartRecord[] ?? lstComment.ToArray();
                if (!commentForumPartRecords.Any()) return true;
                foreach (var item in commentForumPartRecords)
                {
                    _commentRepository.Delete(item);// Xóa tất cả comment thuộc bài viết này.
                }
            }

            return true;
        }
        #endregion

        #region //Helper

        public string StripTagsReplace(string source)
        {
            return Regex.Replace(source, "<(a)([^>]+)>", "<$1 rel=\"nofollow\" $2>");
        }
        //public string HtmlLinkAddRedirectAndNofollow(string source)
        //{
        //    return Regex.Replace(source, "<a[^>]+href=\"?'?(?!#[\\w-]+)([^'\">]+)\"?'?[^>]*>(.*?)</a>", "<a href=\"/redirect?url=$1\" rel=\"nofollow\">$2</a>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        //}
        
        public string StripTagsRegex(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }
        public string GetImagesSrc(string source)
        {
            return Regex.Match(source, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase).Groups[1].Value;
        }
        public string MySubString(string inputText, int length)
        {
            if (inputText.Length <= length) return inputText;
            inputText = inputText.Substring(0, length);
            int index = inputText.LastIndexOf(' ');

            if (index > 0)
                inputText = inputText.Substring(0, index);

            inputText = inputText + "...";
            return inputText;
        }
        #endregion

        #region //GetImage Defult

        private const string DefaultForumPath = "/Themes/Bootstrap/Styles/images/forum-category-default-images/";
        private static readonly Random Random = new Random();
        private static readonly object SyncLock = new object();
        public static int RandomNumber(int max)
        {
            // synchronize
            lock (SyncLock)
            {
                return Random.Next(max);
            }
        }

        public List<string> GetThreadDefaultImageUrls(string shortName)
        {
            return _cacheManager.Get("ThreadDefaultImageUrls_" + shortName, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ThreadDefaultImageUrls_" + shortName + "_Changed"));

                string threadDefaultPath = DefaultForumPath + shortName + "/";
                var imgUrl = new List<string>();
                var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath("~" + threadDefaultPath));
                if (folder.Exists)
                {
                    var files = Directory.GetFiles(folder.FullName, "*.jpg");
                    imgUrl.AddRange(files.Select(filePath => new FileInfo(filePath)).Select(file => threadDefaultPath + file.Name));
                }
                return imgUrl;
            });
        }
        public string GetThreadRandomImageUrl(string threadShortName)
        {
            string imgUrl = "";
            var imageUrls = GetThreadDefaultImageUrls(threadShortName);
            if (imageUrls.Count > 0)
            {
                imgUrl = imageUrls[RandomNumber(imageUrls.Count)];
            }
            return imgUrl;
        }
        public string GetDefaultImageUrl(ForumPostPart p, string threadShortName)// ThreadShortName = Name.ToSug()
        {
            string defaultPath = "/UserFiles/" + p.Thread.Id + "/";
            string imgUrl = defaultPath + p.CssImage;// FileName

            // check if image is exists
            var imgFile = new FileInfo(HttpContext.Current.Server.MapPath("~" + imgUrl));
            if (!imgFile.Exists) imgUrl = "";

            if (!string.IsNullOrEmpty(imgUrl)) return imgUrl;
            // get Random image form PropertyType images
            imgUrl = GetThreadRandomImageUrl(threadShortName);

            // get Default image of PropertyType
            if (String.IsNullOrEmpty(imgUrl))
                imgUrl = DefaultForumPath + threadShortName + ".jpg";
            return imgUrl;
        }

        public void ReplaceUserForumPost(int userFrom, int userTo)
        {
            var listForumPost = _contentManager.Query<ForumPostPart, ForumPostPartRecord>().List();

            foreach(var item in listForumPost)
            {
                if (item.UserPost != null && item.UserPost.Id == userFrom)
                {
                    var userToTemp = _userGroupService.GetUser(userTo);
                    if (userToTemp != null)
                    {
                        item.UserPost = userToTemp.Record;
                        Services.Notifier.Information(T("1. Bài viết {0} : {1} => {2}", item.Id, userFrom, userTo));
                    }
                }

                if (item.Blog != null && item.Blog.Id == userFrom)
                {
                    var userToTemp = _userGroupService.GetUser(userTo);
                    if (userToTemp == null) continue;
                    item.Blog = userToTemp.Record;
                    Services.Notifier.Information(T("2. Bài viết {0} : {1} => {2}", item.Id, userFrom, userTo));
                }
            }
        }
        #endregion
    }
}