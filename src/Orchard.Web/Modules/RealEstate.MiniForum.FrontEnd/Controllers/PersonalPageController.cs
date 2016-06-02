using System;
using System.Linq;
using System.Web.Routing;
using System.Web.Mvc;

using Orchard;
using Orchard.Themes;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Logging;
using Orchard.Users.Models;
using Orchard.DisplayManagement;

using RealEstate.Helpers;
using RealEstateForum.Service;
using RealEstateForum.Service.Services;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.ViewModels;
using RealEstate.Services;
using RealEstate.NewLetter.ViewModels;
using RealEstate.NewLetter.Services;

using Orchard.UI.Notify;


namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    [Themed]
    public class PersonalPageController : Controller, IUpdateModel
    {
        private readonly IPostAdminService _postService;
        private readonly IPostForumFrontEndService _postForumService;
        private readonly ISiteService _siteService;
        private readonly IUserPersonalService _userRealEstateService;
        private readonly IUserForumService _userForumSerivce;
        private readonly ICommentService _commentService;
        private readonly IMessageInboxService _messageInbox;
        private readonly IForumFriendService _forumFriendService;
        private readonly IUserGroupService _groupUserService;
        public PersonalPageController(
            IOrchardServices services,
            ISiteService siteService,
            IPostAdminService postService,
            IPostForumFrontEndService postForumService,
            IUserPersonalService userRealEstateService,
            IUserForumService userForumSerivce,
            ICommentService commentService,
            IMessageInboxService messageInbox,
            IForumFriendService forumFriendService,
            IUserGroupService groupUserService,
            IShapeFactory shapeFactory)
        {
            _siteService = siteService;
            _postService = postService;
            _postForumService = postForumService;
            _userRealEstateService = userRealEstateService;
            _userForumSerivce = userForumSerivce;
            _commentService = commentService;
            _messageInbox = messageInbox;
            _groupUserService = groupUserService;
            _forumFriendService = forumFriendService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [Authorize]
        public ActionResult MyPage(int? p = 0)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var lstPost = _postService.GetListPostFromBlogUserIdCache(user.Id);

            int pageIndex = p.Value == 0 ? 1 : p.Value;
            int pageSize = _siteService.GetSiteSettings().PageSize;
            int totalCount = lstPost.Count();

            var results = lstPost.Slice((pageIndex - 1) * pageSize, pageSize).ToList();
            int totalPage = totalCount / pageSize;
            if (totalCount % pageSize > 0)
            {
                totalPage++;
            }

            var model = new PersonalPageViewModel
            {
                IsAdminOrManagement =
                    (Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum) ||
                     Services.Authorizer.Authorize(StandardPermissions.SiteOwner)),
                Pager = totalPage,
                TotalCount = totalCount,
                ListPostHomePage = _postForumService.BuildListPostByBlogId(results, pageIndex, user.Id),
                IsPageOwner = true,
                UserCurent = new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName
                }
            };


            if (Request.IsAjaxRequest())
            {
                return PartialView("ListPost.PersonalPage", model);
            }

            return View(model);
        }

        [ValidateInput(false)]
        public ActionResult AjaxPersonalPostPage(string content, int userId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            if (user == null) return Json(new {status = false, mesage = "Not Authorize"});
            try
            {
                var userSelect = Services.ContentManager.Get<UserPart>(userId);

                var p = Services.ContentManager.New<ForumPostPart>("ForumPost");

                p.Thread = null;
                p.Blog = userSelect.Record;
                p.BlogDateCreated = DateTime.Now;
                p.Content = content;
                p.IsShareBlog = false;
                p.PublishStatus = null;
                p.SeqOrder = 1;
                p.StatusPost = null;
                p.UserPost = user.Record;

                Services.ContentManager.Create(p);

                string displayName = _userRealEstateService.GetUserNameOrDisplayName(user.Id);
                string avatar = _userRealEstateService.GetUserAvatar(user.Id);

                return Json(new 
                { 
                    status = true, 
                    content = content, 
                    timeago = p.BlogDateCreated.Value.TimeAgo(), 
                    PostId = p.Id, 
                    DisplayName = displayName,
                    Avatar = avatar,
                    IsAdminOrManagement = (Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum) || Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                });
            }
            catch
            {
                return Json(new { status = false, message = "Không thể đăng bài."});
            }
        }
        public ActionResult FriendPage(int userId, string userName, int? p = 0)
        {
            var userSelect = Services.ContentManager.Get<UserPart>(userId);
            if (userSelect != null)
            {
                var lstPost = _postService.GetListPostFromBlogUserIdCache(userSelect.Id);

                int pageIndex = p.Value == 0 ? 1 : p.Value;
                int pageSize = _siteService.GetSiteSettings().PageSize;
                int totalCount = lstPost.Count();

                var results = lstPost.Slice((pageIndex - 1) * pageSize, pageSize).ToList();
                int totalPage = totalCount / pageSize;
                if (totalCount % pageSize > 0)
                {
                    totalPage++;
                }

                var model = new PersonalPageViewModel
                {
                    IsAdminOrManagement = (Services.WorkContext.CurrentUser != null
                                           && (Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum)
                                               || Services.Authorizer.Authorize(StandardPermissions.SiteOwner))),
                    Pager = totalPage,
                    TotalCount = totalCount,
                    ListPostHomePage =
                        results != null
                            ? _postForumService.BuildListPostByBlogId(results, pageIndex, userSelect.Id)
                            : null,
                    IsPageOwner = false,
                    UserSelect = new UserInfo()
                    {
                        Id = userSelect.Id,
                        UserName = userSelect.UserName,
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(userSelect.Id)
                    }
                };

                if (Services.WorkContext.CurrentUser != null)
                {
                    var user = Services.WorkContext.CurrentUser.As<UserPart>();
                    model.UserCurent = new UserInfo()
                    {
                        Id = user.Id,
                        UserName = user.UserName
                    };
                }
                else
                {
                    model.UserCurent = null;
                }

                if (Request.IsAjaxRequest())
                {
                    return PartialView("ListPost.PersonalPage", model);
                }

                return View(model);
            }
            else
            {
                return Redirect("/");
            }
        }

        #region Message

        [Authorize]
        public ActionResult MyMessage()
        {
            //Tin nhắn đã nhận(Bạn bè || Admin) => Đếm số tin nhắn
            //Tin đã gửi
            //Gửi tin nhắn
            // Thông tin userId
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var model = new MesageFrontEndViewModel
            {
                CountMessageInboxAdmin = _messageInbox.CountMessageInboxFromAdmin(),
                CountMessageInboxFrontEnd = _messageInbox.CountMessageInboxUser(),
                UserInfo = new UserInfo
                {
                    Id = user.Id,
                    UserName = user.UserName
                }
            };
            return View(model);
        }


        #endregion

        #region List Friend Page

        [Authorize]
        public ActionResult MyListFriend()
        {
            return View();
        }
        [Authorize]
        public ActionResult MyListRequestFriend()
        {
            return View();
        }

        public ActionResult YourListFriend(int? id)
        {
            var userSelect = Services.ContentManager.Get<UserPart>(id.Value);
            ViewBag.UserId = id;
            ViewBag.UserName = userSelect != null ? userSelect.UserName : "congty";
            return View();
        }
        [Authorize]
        public ActionResult AjaxGetListRequestFriend(int? id, PagerParameters pagerParameters)
        {
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();
            var userSelect = Services.ContentManager.Get<UserPart>(id.Value);

            int totalCount = 0;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var listfriend = _forumFriendService.ListRequestFriendRecordByUser(userSelect.Record);

            totalCount = listfriend.Count();

            #region BUILD MODEL

            var pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);

            var results = listfriend.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            var model = new UserFriendIndexViewModel
            {
                ListUser = _forumFriendService.BuildListFriend(results, userCurent),
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView(model);
        }

        [Authorize]
        public ActionResult AjaxGetListFriend(int? id, PagerParameters pagerParameters)
        {
            var userSelect = Services.ContentManager.Get<UserPart>(id.Value);

            int totalCount = 0;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var listfriend = _forumFriendService.ListFriendRecordByUser(userSelect.Record);

            totalCount = listfriend.Count();

            #region BUILD MODEL

            var pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();

            var results = listfriend.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            var model = new UserFriendIndexViewModel
            {
                ListUser = _forumFriendService.BuildListFriend(results, userCurent),
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView(model);
        }
        [Authorize]
        public ActionResult AjaxSearchFriend(string kw, PagerParameters pagerParameters)
        {
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();

            int totalCount = 0;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var listfriend = _groupUserService.GetUsers().Where(r=>r.Id != userCurent.Id);// ListFriendRecordByUser(userCurent.Record);

            if (!string.IsNullOrEmpty(kw))
                listfriend = _groupUserService.GetUsers().Where(r => r.UserName.Contains(kw) || r.Email.Contains(kw));

            totalCount = listfriend.Count();

            #region BUILD MODEL

            var pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);

            var results = listfriend.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            var model = new UserFriendIndexViewModel
            {
                ListUser = _forumFriendService.BuildListFriend(results, userCurent),
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView("AjaxGetListFriend", model);
        }

        [Authorize]
        public ActionResult AjaxCheckFriend(int UserSelect)
        {
            /*ViewBag.ViewId = 0;
            ViewBag.UserSelect = UserSelect;
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();

            if (userCurent == null) return PartialView(); ;

            var _UserSelect = Services.ContentManager.Get<UserPart>(UserSelect);
            if (_UserSelect == null) return PartialView();

            if(_forumFriendService.ListFriendByUser(userCurent.Record).Where(r => r.UserRequest == _UserSelect.Record || r.UserReceived == _UserSelect.Record).Count() > 0)
            {
                ViewBag.ViewId = 1;//Da ket ban
                return PartialView();
            }

            if (_forumFriendService.ListFriendWaitingByUser(userCurent.Record).Where(r => r.UserReceived == _UserSelect.Record).Count() > 0)
            {
                ViewBag.ViewId = 2;//Da gui yeu cau ket ban
                return PartialView();
            }

            if (_forumFriendService.ListFriendRequestByUser(userCurent.Record).Where(r => r.UserRequest == _UserSelect.Record).Count() > 0)
            {
                ViewBag.ViewId = 3;//Nhan yeu cau ket ban
                return PartialView();
            }

            ViewBag.ViewId = 4;//Ket ban*/

            ViewBag.ViewId = 0;
            ViewBag.UserSelect = UserSelect;
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();

            if (userCurent == null) return PartialView(); ;

            var userSelect = Services.ContentManager.Get<UserPart>(UserSelect);

            ViewBag.ViewId = _forumFriendService.CheckFriend(userCurent,userSelect);

            return PartialView();
        }
        [Authorize]
        public ActionResult AjaxAddFriend(int userId)
        {
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();
            var userSelect = Services.ContentManager.Get<UserPart>(userId);

            bool addFriend = _forumFriendService.AddFriend(userCurent.Record, userSelect.Record);

            if (!addFriend)
            {
                return Json(new { status = false, message = "Đã là bạn hoặc đã gửi yêu cầu đến tài khoản này!"});
            }

            return Json(new { status = true });
        }
        public ActionResult AjaxAcceptFriend(int userId)
        {
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();
            var userSelect = Services.ContentManager.Get<UserPart>(userId);

            bool acceptFriend = _forumFriendService.AcceptFriend(userCurent.Record, userSelect.Record);

            return !acceptFriend ? Json(new { status = false, message = "Chưa có yêu cầu kết bạn từ user này!" }) : Json(new { status = true });
        }
        #endregion

        #region Ajax
        public ActionResult AjaxPerDeletePost(int PostId)
        {
            var user = Services.WorkContext.CurrentUser;
            if (user != null)
            {
                bool isDelete = _postService.PerDeletePostById(PostId);

                if (isDelete)
                    return Json(new { status = true});
                return Json(new { status = false, message = "Không thể xóa được." });
            }
            else
            {
                return Json(new { status = false, message = "Not Authorize"});
            }
        }
        public ActionResult AjaxPerDeleteComment(int commentId)
        {
            var user = Services.WorkContext.CurrentUser;
            if (user != null)
            {
                bool isDelete = _commentService.PerDeleteComment(commentId);

                return isDelete ? Json(new { status = true }) : Json(new { status = false, message = "Không thể xóa được." });
            }
            return Json(new { status = false, message = "Not Authorize" });
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