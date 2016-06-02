using Orchard;
using Orchard.ContentManagement;
using RealEstate.Services;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.Services;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RealEstate.Helpers;
using Orchard.Data;

namespace RealEstate.API.Controllers
{
    public class ForumPostController : CrossSiteController, IUpdateModel
    {

         private readonly IUserGroupService _groupService;
         private readonly IThreadAdminService _threadService;
         private readonly IPropertySettingService _settingService;

         private readonly IRepository<CommentForumPartRecord> _commentRepository;
         private readonly IRepository<ForumPostPartRecord> _forumPostRepository;

         public ForumPostController(IOrchardServices services
             , IUserGroupService groupService
             , IThreadAdminService threadService
             , IPropertySettingService settingService
             ,IRepository<CommentForumPartRecord> commentRepository
            ,IRepository<ForumPostPartRecord> forumPostRepository)
        {
            Services = services;
            _commentRepository = commentRepository;
            _forumPostRepository = forumPostRepository;
            _groupService = groupService;
            _threadService = threadService;
            _settingService = settingService;
        }
        public IOrchardServices Services { get; set; }

        #region CREATE

        //Xử lý từ Ajax crossite
        [ValidateInput(false), HttpPost]
        public JsonResult Create(int ThreadId, string Title, string Content, string DomainName, int UserId, string CssImage)
        {
            try
            {
                var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");
                var user = _groupService.GetUser(UserId).Record;
                //var cssImage = Request.Files["CssImage"];

                postForum.Thread = _threadService.GetTopicPartRecordById(ThreadId, DomainName).Record;
                postForum.Title = Title.Trim();

                Content = HttpUtility.HtmlDecode(Content.Trim());
                postForum.Content = Content.Normalize().HtmlLinkAddRedirectAndNofollow();

                postForum.UserPost = user;
                postForum.DateCreated = DateTime.Now;
                postForum.DateUpdated = DateTime.Now;
                postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(611410).Record;//Publish
                postForum.StatusPost = _threadService.GetStatusForumPartRecordByCssClass("st-pending").Record;
                postForum.Views = 0;
                postForum.CssImage = CssImage;

                //Share blog
                postForum.IsShareBlog = false;
                postForum.Blog = null;
                postForum.BlogDateCreated = DateTime.Now;
                postForum.HostName = DomainName;


                Services.ContentManager.Create(postForum);
                postForum.BlogPostId = null;

                return Json(new { status = true, id = postForum.Id });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        //Xử lý từ WebRequest
        [ValidateInput(false), HttpPost]
        public JsonResult CreateREST(int ThreadId, string Title, string Content, string DomainName, int UserId, string CssImage, string apiKey)
        {
            string test = "";
            try
            {
                if (!string.IsNullOrEmpty(apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(apiKey))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");
                var user = _groupService.GetUser(UserId).Record;
                //var cssImage = Request.Files["CssImage"];
                test += "=>1";

                postForum.Thread = _threadService.GetTopicPartRecordById(ThreadId).Record;
                postForum.Title = Title.Trim();
                test += "=>2";

                Content = HttpUtility.HtmlDecode(Content.Trim());
                postForum.Content = Content.Normalize().HtmlLinkAddRedirectAndNofollow();
                test += "=>3";

                postForum.UserPost = user;
                postForum.DateCreated = DateTime.Now;
                postForum.DateUpdated = DateTime.Now;
                postForum.PublishStatus = _threadService.GetPublishStatusPartRecordById(611410).Record;//Publish
                postForum.StatusPost = _threadService.GetStatusForumPartRecordByCssClass("st-pending").Record;
                postForum.Views = 0;
                postForum.CssImage = CssImage;
                test += "=>4";

                //Share blog
                postForum.IsShareBlog = false;
                postForum.Blog = null;
                postForum.BlogDateCreated = DateTime.Now;
                postForum.HostName = DomainName;
                test += "=>5";


                Services.ContentManager.Create(postForum);
                postForum.BlogPostId = null;
                test += "=>6";

                return Json(new { status = true, id = postForum.Id });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion

        #region UPDATE

        [ValidateInput(false), HttpPost]
        public JsonResult Update(int id, int ThreadId, string Title, string Content, string DomainName, string CssImage)
        {
            try
            {
                var postForum = Services.ContentManager.Get<ForumPostPart>(id);
                postForum.Title = Title;

                Content = HttpUtility.HtmlDecode(Content.Trim());
                postForum.Content = Content.Normalize().HtmlLinkAddRedirectAndNofollow();
                postForum.CssImage = CssImage;

                postForum.Thread = _threadService.GetTopicPartRecordById(ThreadId, DomainName).Record;

                return Json(new { status = true, domainname = DomainName, threadId = ThreadId, Content = Content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [ValidateInput(false), HttpPost]
        public JsonResult UpdateREST(int id, int ThreadId, string Title, string Content, string DomainName, string CssImage, string apiKey)
        {
            try
            {
                if (!string.IsNullOrEmpty(apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(apiKey))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                var postForum = Services.ContentManager.Get<ForumPostPart>(id);
                postForum.Title = Title;

                Content = HttpUtility.HtmlDecode(Content.Trim());
                postForum.Content = Content.Normalize().HtmlLinkAddRedirectAndNofollow();
                postForum.CssImage = CssImage;

                postForum.Thread = _threadService.GetTopicPartRecordById(ThreadId).Record;

                return Json(new { status = true, domainname = DomainName, threadId = ThreadId, Content = Content });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion
        
        public JsonResult DeleteREST(int id, int UserId, string apiKey)
        {
            try
            {
                if (!string.IsNullOrEmpty(apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(apiKey))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                var postForum = Services.ContentManager.Get<ForumPostPart>(id);

                var user = _groupService.GetUser(UserId).Record;

                if (postForum.UserPost != user)
                    return Json(new { status = false, message = "Không thể xóa!"});


                Services.ContentManager.Remove(postForum.ContentItem);
                _forumPostRepository.Delete(postForum.Record);

                var lstComment = _commentRepository.Fetch(r => r.ForumPost.Id == id);
                var commentForumPartRecords = lstComment as CommentForumPartRecord[] ?? lstComment.ToArray();

                if (commentForumPartRecords.Any())
                {
                    foreach (var item in commentForumPartRecords)
                    {
                        _commentRepository.Delete(item);// Xóa tất cả comment thuộc bài viết này.
                    }
                }

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #region Implement IUpdateModel

        public void AddModelError(string key, Orchard.Localization.LocalizedString errorMessage)
        {
            throw new NotImplementedException();
        }

        public new bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}