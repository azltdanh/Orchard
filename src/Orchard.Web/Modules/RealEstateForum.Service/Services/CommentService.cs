using System;
using System.Collections.Generic;
using System.Linq;

using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;

using RealEstate.Helpers;
using RealEstateForum.Service.ViewModels;
using RealEstateForum.Service.Models;
using RealEstate.Services;
using Orchard.Users.Models;
using Orchard.Security;
using Orchard.Data;

namespace RealEstateForum.Service.Services
{
    public interface ICommentService : IDependency
    {
        IContentQuery<CommentForumPart, CommentForumPartRecord> LoadComment(int postId);
        void ClearLoadComentCache(int postId, int userId);
        CommentIndexViewModel BuildLoadComment(IEnumerable<CommentForumPart> results, int postId, UserPart user, string hostname);
        CommentIndexViewModel BuildLoadComment(int userId, int postId, int pageIndex);// Load comment cho trang cá nhân

        //Ajax
        ResultByPostComment PostCommentForum(int postId, string content);
        ResultByPostComment PostSubCommentForum(int postId, string content, int parentCommentId);
        bool DeleteComment(int commentId, int postId, UserPartRecord user, string hostname);
        bool PerDeleteComment(int commentId);
    }

    public class CommentService : ICommentService
    {
        private readonly IRepository<CommentForumPartRecord> _commentRepository;

        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly IUserPersonalService _userRealEstateService;
        private readonly IUserForumService _userForumSerivce;
        private readonly IPostAdminService _postService;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        //private int cacheTimeSpan = 60 * 24; // Cache for 24 hours

        public CommentService(
            IRepository<CommentForumPartRecord> commentRepository,
            IOrchardServices services,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IUserPersonalService userRealEstateService, 
            IUserForumService userForumSerivce,
            IPostAdminService postService
            )
        {
            _commentRepository = commentRepository;
            Services = services;
            _contentManager = contentManager;
            _userRealEstateService = userRealEstateService;
            _userForumSerivce = userForumSerivce;
            _postService = postService;

            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IContentQuery<CommentForumPart, CommentForumPartRecord> LoadComment(int postId)
        {
            //return _cacheManager.Get("ForumLoadComentCache_" + PostId, ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("ForumLoadComentCache_" + PostId + "_Changed"));

               return _contentManager.Query<CommentForumPart, CommentForumPartRecord>().Where(r => r.ForumPost.Id == postId);

            //});
        }
        public void ClearLoadComentCache(int postId, int userId)
        {
            //_signals.Trigger("ForumLoadComentCache_" + PostId + "_Changed");

            //for (int i = 1; i < 6; i++)
            //{
            //    _signals.Trigger("BuildLoadCommentCache_" + UserId + "_" + i + "_Changed");
            //}
        }
        public CommentIndexViewModel BuildLoadComment(IEnumerable<CommentForumPart> results, int postId, UserPart user, string hostname)
        {
            var model = new CommentIndexViewModel();

            bool isOwnerPost = user != null && _postService.CheckIsOwnerOrManagerPost(user.Id, postId, hostname);
            model.IsMangagementOrAdmin = (isOwnerPost || Services.Authorizer.Authorize(MiniForumPermissions.ManagementCommentPostForum) || Services.Authorizer.Authorize(StandardPermissions.SiteOwner));

            model.ListComment = results.Select(r => new CommentPostViewModel()
            {
                CommentId = r.Id,
                Content = r.Content,
                TimeAgo = r.DateCreated.Value.ToLocalTime().TimeAgo(),
                ParentCommentId = r.ParentCommentId,
                IsOwner = user != null &&  user.Id == r.UserComment.Id,
                ListSubComment = LoadComment(postId).Where(c => c.ParentCommentId == r.Id).List().Select(c => new SubCommentEntry()
                {
                    SubCommentId = c.Id,
                    TimeAgo = c.DateCreated.Value.ToLocalTime().TimeAgo(),
                    SubCommentContent = c.Content,
                    IsOwner = user != null && user.Id == c.UserComment.Id,
                    UserInfo = new UserInfo
                    {
                        Id = c.UserComment.Id,
                        UserName = c.UserComment.UserName,
                        Avatar = _userRealEstateService.GetUserAvatar(c.UserComment.Id),
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(c.UserComment.Id)
                    }
                }).ToList(),
                SeqOrder = r.SortOrder,
                UserInfo = new UserInfo
                {
                    Id = r.UserComment.Id,
                    UserName = r.UserComment.UserName,
                    Avatar = _userRealEstateService.GetUserAvatar(r.UserComment.Id),
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserComment.Id)
                }
            }).ToList();

            return model;
        }

        public CommentIndexViewModel BuildLoadComment(int userId, int postId, int pageIndex)
        {
                var query = _contentManager.Query<CommentForumPart, CommentForumPartRecord>()
                    .Where(r => r.ForumPost.Id == postId && r.ParentCommentId == 0).OrderByDescending(c=>c.DateCreated).List();
                //int takeItem = query.Count() > 5 ? 5 : query.Count();

                var model = new CommentIndexViewModel
                {
                    TotalCount = query.Count(),
                    ListComment = query.Select(r => new CommentPostViewModel()
                    {
                        CommentId = r.Id,
                        Content = r.Content,
                        SeqOrder = r.SortOrder,
                        TimeAgo = r.DateCreated.Value.ToLocalTime().TimeAgo(),
                        UserInfo = new UserInfo
                        {
                            Id = r.UserComment.Id,
                            UserName = r.UserComment.UserName,
                            Avatar = _userRealEstateService.GetUserAvatar(r.UserComment.Id),
                            DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserComment.Id)
                        },
                        ListSubComment =
                            _contentManager.Query<CommentForumPart, CommentForumPartRecord>()
                                .Where(x => x.ForumPost.Id == postId && x.ParentCommentId == r.Id)
                                .OrderByDescending(c => c.DateCreated)
                                .List()
                                .Select(x => new SubCommentEntry
                                {
                                    SubCommentId = x.Id,
                                    SubCommentContent = x.Content,
                                    TimeAgo = x.DateCreated.Value.ToLocalTime().TimeAgo(),
                                    UserInfo = new UserInfo
                                    {
                                        Id = x.UserComment.Id,
                                        UserName = x.UserComment.UserName,
                                        Avatar = _userRealEstateService.GetUserAvatar(x.UserComment.Id),
                                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(x.UserComment.Id)
                                    }
                                }).ToList()
                    }).ToList()
                };

            return model;
        }
        #region Ajax Excute
        public ResultByPostComment PostCommentForum(int postId, string content)
        {
            var results = new ResultByPostComment();
            try
            {
                var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;
                var comment = Services.ContentManager.New<CommentForumPart>("CommentForum");
                comment.Content = content;
                comment.DateCreated = DateTime.Now;
                comment.SortOrder = 1;
                comment.ForumPost = _contentManager.Get<ForumPostPart>(postId).Record;
                comment.UserComment = user;
                Services.ContentManager.Create(comment);
                comment.ParentCommentId = 0;

                // Clear cache
                //ClearLoadComentCache(PostId,user.Id);

                results.IsSuccess = true;
                results.Id = comment.Id;
                results.TimeAgo = comment.DateCreated.Value.ToLocalTime().TimeAgo();
                results.UserInfo = new UserInfo()
                {
                    Id = comment.UserComment.Id,
                    Avatar = _userRealEstateService.GetUserAvatar(comment.UserComment.Id),
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(comment.UserComment.Id)
                };

                return results;
            }
            catch
            {
                results.IsSuccess = false;
                return results;
            }
        }
        public ResultByPostComment PostSubCommentForum(int postId, string content, int parentCommentId)
        {
            var results = new ResultByPostComment();
            try
            {
                var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;
                var comment = Services.ContentManager.New<CommentForumPart>("CommentForum");
                comment.Content = content;
                comment.DateCreated = DateTime.Now;
                comment.SortOrder = 1;
                comment.ForumPost = _contentManager.Get<ForumPostPart>(postId).Record;
                comment.UserComment = user;
                comment.ParentCommentId = parentCommentId;
                Services.ContentManager.Create(comment);

                // Clear cache
                //ClearLoadComentCache(PostId,user.Id);

                results.IsSuccess = true;
                results.Id = comment.Id;
                results.TimeAgo = comment.DateCreated.Value.ToLocalTime().TimeAgo();
                results.UserInfo = new UserInfo()
                {
                    Id = comment.UserComment.Id,
                    Avatar = _userRealEstateService.GetUserAvatar(comment.UserComment.Id),
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(comment.UserComment.Id)
                };

                return results;
            }
            catch
            {
                results.IsSuccess = false;
                return results;
            }
        }

        public bool DeleteComment(int commentId, int postId, UserPartRecord user, string hostname)
        {
            bool isOwner = user != null && _postService.CheckIsOwnerOrManagerPost(user.Id, postId, hostname);
            bool isAdminOrMangement = (isOwner || Services.Authorizer.Authorize(MiniForumPermissions.ManagementCommentPostForum) || Services.Authorizer.Authorize(StandardPermissions.SiteOwner)) ? true : false;
            var comment = _commentRepository.Fetch(r => r.Id == commentId);
            if (user != comment.FirstOrDefault().UserComment) { return false; }

            if (isAdminOrMangement || user.Id == comment.FirstOrDefault().UserComment.Id)
            {
                try
                {
                    foreach (var item in comment)
                    {
                        if (item.ParentCommentId == 0)
                        {
                            var replyComment = _commentRepository.Fetch(r => r.ParentCommentId == item.Id);
                            if (replyComment.Count() > 0)
                            {
                                foreach (var child in replyComment)
                                {
                                    _commentRepository.Delete(child);// Xóa các comment con nếu có
                                }
                            }
                        }
                        _commentRepository.Delete(item);//Xóa các reply comment
                    }

                    // Clear cache
                    //ClearLoadComentCache(comment.FirstOrDefault().ForumPost.Id,comment.FirstOrDefault().UserComment.Id);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool PerDeleteComment(int commentId)
        {
            bool isAdminOrMangement = (Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum) || Services.Authorizer.Authorize(MiniForumPermissions.ManagementCommentPostForum) || Services.Authorizer.Authorize(StandardPermissions.SiteOwner)) ? true : false;
            if (isAdminOrMangement)
            {
                try
                {
                    var comment = _commentRepository.Fetch(r => r.Id == commentId);
                    foreach (var item in comment)
                    {
                        if (item.ParentCommentId == 0)
                        {
                            var replyComment = _commentRepository.Fetch(r => r.ParentCommentId == item.Id);
                            if (replyComment.Count() > 0)
                            {
                                foreach (var child in replyComment)
                                {
                                    _commentRepository.Delete(child);// Xóa các comment con nếu có
                                }
                            }
                        }
                        _commentRepository.Delete(item);//Xóa các reply comment
                    }

                    // Clear cache
                    //ClearLoadComentCache(comment.FirstOrDefault().ForumPost.Id, comment.FirstOrDefault().UserComment.Id);

                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}