using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;

using RealEstate.Services;
using RealEstate.Helpers;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.ViewModels;
using Orchard.Users.Models;
using Orchard.Services;
using Orchard.Caching;

namespace RealEstateForum.Service.Services
{
    public interface IPostForumFrontEndService : IDependency
    {
        ThreadForumFrontEndViewModel BuildListPostByThread(int threadId, string hostname);
        PostForumFrontEnDetailViewModel BuildPostForumDetail(ForumPostPartRecord p);
        WidgetIsMarketViewModel BuildListPostNewsMain(string hostname);
        List<PostItem> BuildListPostNewestWdiget(string hostname);
        List<ForumPostEntryFrontEnd> BuildPostEntry(List<ForumPostPart> results);
        List<PostItemOfUserWidgetViewModel> BuildListPostOfUser(List<ForumPostPart> p);
        PostFilterOptions BuildPostFilterOption(PostFilterOptions options, string hostname);
        IContentQuery<ForumPostPart, ForumPostPartRecord> ForumFilterFrontEnd(PostFilterOptions options, string hostname);

        List<PersonalPageEntry> BuildListPostByBlogId(List<ForumPostPart> results, int pageIndex, int userId);
        List<PostByTopicEntry> BuildPostByTopicEntry(IEnumerable<ForumPostPart> list, ThreadInfo thread);
        List<PostByTopicEntry> BuildPostByTopicEntry(IEnumerable<ForumPostPart> list);
        IContentQuery<ForumPostPart, ForumPostPartRecord> LoadPostOfUser(int userId);

        List<PostItem> GetListPinnedOrNewest(int topicId, string hostname);
        PostItem BuildPostDetailById(int? id, string hostName);
    }

    public class PostForumFrontEndService : IPostForumFrontEndService
    {
        private readonly ICommentService _commentService;
        private readonly IContentManager _contentManager;
        private readonly IThreadAdminService _threadService;
        private readonly IPostAdminService _postService;
        private readonly IUserGroupService _userGroupService;
        private readonly IUserPersonalService _userRealEstateService;
        private readonly IUserForumService _userForumSerivce;
        private readonly IPropertySettingService _propertySetting;
        private readonly IHostNameService _hostNameService;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private int cacheTimeSpan = 60 * 24; // Cache for 24 hours

        public PostForumFrontEndService(
            ICommentService commentService,
            IOrchardServices services,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IPostAdminService postService,
            IContentManager contentManager,
            IUserPersonalService userRealEstateService,
            IHostNameService hostNameService,
            IThreadAdminService threadService,
            IUserGroupService userGroupService,
            IUserForumService userForumSerivce,
            IPropertySettingService propertySetting)
        {
            _commentService = commentService;
            Services = services;
            _contentManager = contentManager;
            _threadService = threadService;
            _postService = postService;
            _userGroupService = userGroupService;
            _userRealEstateService = userRealEstateService;
            _hostNameService = hostNameService;
            _userForumSerivce = userForumSerivce;
            _propertySetting = propertySetting;

            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ThreadForumFrontEndViewModel BuildListPostByThread(int threadId, string hostname)
        {
            var listTopic = _threadService.GetListTopicFromCache(threadId, hostname);//Danh sách chuyên đề
            var threadInfo = _threadService.GetListThreadFromCache(hostname).Where(r => r.Id == threadId)
                .Select(r => new ThreadInfo
                {
                    Id = r.Id,
                    Name = r.Name,
                    ShortName = r.ShortName,
                    DefaultImage = r.DefaultImage
                }).FirstOrDefault();// Thông tin chuyên mục

            return new ThreadForumFrontEndViewModel
            {
                ThreadInfo = threadInfo,
                ListTopics = listTopic.Select(r => new ThreadForumFrontEndEntry
                {
                    PostNewest = _postService.GetListPostForumByTopic(r.Id, hostname).OrderByDescending(c => c.DateUpdated).Slice(1)//.OrderByDescending(t => t.DateUpdated)
                    .Select(t => threadInfo != null ? new PostItemNewest
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Content = _postService.MySubString(t.Content.StripHtml(), 500),
                        HostName = t.HostName,
                        DefaultImage = _postService.GetDefaultImageUrl(t, threadInfo.ShortName)//_threadService.GetThreadInfoByTopic(hostname, t.Thread).ShortName)
                    } : null).FirstOrDefault(),
                    TopicInfo = new ThreadInfo() { Id = r.Id, Name = r.Name, ShortName = r.ShortName },
                    ListPostItem = GetListPinnedOrNewest(r.Id, hostname).ToList(),
                    PostCount = _postService.CountPostByTopicToCache(r.Id, hostname)
                }).ToList()
            };
        }

        public List<PostItem> GetListPinnedOrNewest(int topicId, string hostname)
        {
            // var listItem = new List<PostItem>();
            return _cacheManager.Get("BuildListPostByThreadCache_" + topicId + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("BuildListPostByThreadCache_" + topicId + "_" + hostname + "_Changed"));

                var listPinnedOrNewest = _postService.GetListPostForumByTopic(topicId, hostname).OrderByDescending(r => r.IsPinned)
                                         .OrderByDescending(r => r.DateUpdated).Slice(4);

                var model = listPinnedOrNewest.Select(r => new PostItem
                    {
                        Id = r.Id,
                        ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                        TopicShortName = r.Thread.ShortName,
                        Title = r.Title,
                        Content = _postService.MySubString(r.Content.StripHtml(), 300),
                        DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                        IsPinned = r.IsPinned && r.TimeExpiredPinned != null && r.TimeExpiredPinned >= DateTime.Now,
                        GetHostName = hostname,
                        UserInfo = new UserInfo()
                                    {
                                        Id = r.UserPost.Id,
                                        UserName = r.UserPost.UserName,
                                        Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id),
                                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id)
                                    }
                    }).ToList();

                return model;

                #region Old
                /*
                #region Bài viết ghim

                var listPostPinned = _postService.GetListPostForumByTopic(TopicId, hostname).Where(r => r.IsPinned && (r.TimeExpiredPinned != null && r.TimeExpiredPinned > DateTime.Now))
                    .OrderByDescending(r=>r.DateUpdated).Slice(1,4);

                foreach (var item in listPostPinned)// Thêm vào danh sách bài ghim
                {
                    ListItem.Add(new PostItem()
                                    {
                                        Id = item.Id,
                                        ThreadShortName = _threadService.GetThreadInfoByTopic(hostname,item.Thread).ShortName,
                                        TopicShortName = item.Thread.ShortName,
                                        Title = item.Title,
                                        Content = _postService.MySubString(item.Content.StripHtml(), 300),
                                        DateUpdated = item.DateUpdated,
                                        IsPinned = item.IsPinned,
                                        GetHostName = hostname,
                                        UserInfo = new UserInfo()
                                                    {
                                                        Id = item.UserPost.Id,
                                                        UserName = item.UserPost.UserName,
                                                        Avatar = _userRealEstateService.GetUserAvatar(item.UserPost.Id),
                                                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(item.UserPost.Id)
                                                    }
                                    });
                }
                #endregion

                #region Bài viết mới nhất

                int ListItemCount = ListItem.Count();
                if (ListItemCount < 4)// Nếu chưa đủ 4 bài ghim trong chuyên đề
                {
                    var listPostNewest = _postService.GetListPostForumByTopic(TopicId, hostname)
                        .OrderByDescending(r => r.DateUpdated).Slice(1,4 - ListItemCount);// Danh sách bài viết mới nhất
                    
                    foreach (var item in listPostNewest)
                    {
                        ListItem.Add(new PostItem()
                        {
                            Id = item.Id,
                            ThreadShortName = _threadService.GetThreadInfoByTopic(hostname,item.Thread).ShortName,
                            TopicShortName = item.Thread.ShortName,
                            Title = item.Title,
                            Content = _postService.MySubString(item.Content.StripHtml(), 300),
                            DateUpdated = item.DateUpdated,
                            IsPinned = item.IsPinned,
                            GetHostName = hostname,
                            UserInfo = new UserInfo()
                            {
                                Id = item.UserPost.Id,
                                UserName = item.UserPost.UserName,
                                Avatar = _userRealEstateService.GetUserAvatar(item.UserPost.Id),
                                DisplayName = _userRealEstateService.GetUserNameOrDisplayName(item.UserPost.Id)
                            }
                        });
                    }
                }

                #endregion

                return ListItem;
                 */
                #endregion
            });
        }

        #region PostDetail

        public PostForumFrontEnDetailViewModel BuildPostForumDetail(ForumPostPartRecord p)
        {
            string hostName = _hostNameService.GetHostNameSite();
            var thread = _threadService.GetThreadInfoByTopic(hostName, p.Thread);

            if (thread == null)
                return null;

            if (Services.WorkContext.CurrentUser != null)
            {
                var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;

                return new PostForumFrontEnDetailViewModel()
                {
                    IsShowSignature = false,
                    ThreadShortName = thread.ShortName,
                    UserCurrent = new UserInfo()
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(p.UserPost.Id),
                        Avatar = _userRealEstateService.GetUserAvatar(p.UserPost.Id)
                    },
                    PostDetail = new PostItem()
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Content = p.Content,
                        Description = p.Description,
                        IsPinned = p.IsPinned,
                        PostStatus = p.StatusPost,
                        DateUpdated = p.DateUpdated.Value.ToLocalTime(),
                        UserInfo = new UserInfo()
                        {
                            Id = p.UserPost.Id,
                            UserName = p.UserPost.UserName,
                            Signature = "",
                            DisplayName = _userRealEstateService.GetUserNameOrDisplayName(p.UserPost.Id),
                            Avatar = _userRealEstateService.GetUserAvatar(p.UserPost.Id)
                        }
                    }
                };
            }
            else
            {
                return new PostForumFrontEnDetailViewModel()
                {
                    IsShowSignature = false,
                    ThreadShortName = thread.ShortName,
                    UserCurrent = null,
                    PostDetail = new PostItem()
                    {
                        Title = p.Title,
                        Id = p.Id,
                        Content = p.Content,
                        IsPinned = p.IsPinned,
                        Description = p.Description,
                        PostStatus = p.StatusPost,
                        DateUpdated = p.DateUpdated.Value.ToLocalTime(),
                        UserInfo = new UserInfo()
                        {
                            Id = p.UserPost.Id,
                            UserName = p.UserPost.UserName,
                            Signature = "",
                            DisplayName = _userRealEstateService.GetUserNameOrDisplayName(p.UserPost.Id),
                            Avatar = _userRealEstateService.GetUserAvatar(p.UserPost.Id)
                        }
                    }
                };
            }
        }

        #endregion

        // Widget tin tức chính
        public WidgetIsMarketViewModel BuildListPostNewsMain(string hostname)
        {
            var valueWithImage = _propertySetting.GetSetting("SL_Tin_Tuc_Chinh_Hien_Thi"); //_forumSettingsService.GetValueSettingDisplay("SL_Tin_Tuc_Thi_Truong_Hien_Thi").Value;
            var valueNotImage = _propertySetting.GetSetting("SL_Bai_Viet_Tin_Tuc_Chinh");//_forumSettingsService.GetValueSettingDisplay("SL_Bai_Viet_Tin_Tuc_Thi_Truong").Value;
            if (String.IsNullOrEmpty(valueWithImage) || Convert.ToInt32(valueWithImage) == 0)
            {
                valueWithImage = "5";
            }
            if (String.IsNullOrEmpty(valueNotImage) || Convert.ToInt32(valueNotImage) == 0)
            {
                valueNotImage = "12";
            }

            var resultWithImage = _postService.GetListPostQueryByHostName(hostname).OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(valueWithImage));
            var listId = resultWithImage.Select(r => r.Id).ToList();

            var resultNotImage = _postService.GetListPostQueryByHostName(hostname).Where(r => !listId.Contains(r.Id)).Slice(Convert.ToInt32(valueNotImage));

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

            return model;
        }

        // Widget bài viết mới nhất
        public List<PostItem> BuildListPostNewestWdiget(string hostname)
        {
            var value = _propertySetting.GetSetting("SL_Bai_Viet_Moi");// _forumSettingsService.GetValueSettingDisplay("SL_Bai_Viet_Moi").Value;
            if (String.IsNullOrEmpty(value) || Convert.ToInt32(value) == 0)
            {
                value = "6";
            }
            var listNewest = _postService.GetListPostQueryByHostName(hostname)
                                .OrderByDescending(r => r.DateUpdated).Slice(Convert.ToInt32(value));

            return listNewest.Select(r => new PostItem
            {
                Id = r.Id,
                ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                TopicShortName = r.Thread.ShortName,
                Title = r.Title,
                Content = _postService.MySubString(r.Content.StripHtml(), 300),
                DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                IsPinned = r.IsPinned,
                UserInfo = new UserInfo
                {
                    Id = r.UserPost.Id,
                    UserName = r.UserPost.UserName,
                    Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id),
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id)
                }
            }).ToList();
        }

        // Build list post for search
        public List<ForumPostEntryFrontEnd> BuildPostEntry(List<ForumPostPart> results)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return results.Select(r => new ForumPostEntryFrontEnd
                {
                    Id = r.Id,
                    Title = r.Title,
                    DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                    Description = r.Description,
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    TopicShortName = r.Thread.ShortName,
                    SubContent = _postService.MySubString(r.Content.StripHtml(), 200),
                    DefaultImage = _postService.GetDefaultImageUrl(r, _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    UserInfo = new UserInfo
                    {
                        Id = r.UserPost.Id,
                        UserName = r.UserPost.UserName,
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id)
                    }
                }).ToList();
        }

        // Build List Post ForUser
        public List<PostItemOfUserWidgetViewModel> BuildListPostOfUser(List<ForumPostPart> p)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return p.Select(r => new PostItemOfUserWidgetViewModel
            {
                Id = r.Id,
                Title = r.Title,
                TopicShortName = r.Thread != null ? r.Thread.ShortName : "not",
                ThreadShortName = r.Thread != null ? _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName : "not",
                PostStatus = r.StatusPost.CssClass
            }).ToList();
        }

        #region Search FrontEnd

        public PostFilterOptions BuildPostFilterOption(PostFilterOptions options, string hostname)
        {
            if (options == null)
            {
                options = new PostFilterOptions { ThreadId = 0, TopicId = 0 };
            }
            options.ListThread = _threadService.GetListThreadFromCache(hostname).Where(r => r.IsOpen).OrderBy(r => r.SeqOrder).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList();

            if (!options.ThreadId.HasValue)
                options.ThreadId = 0;
            options.ListTopic = _threadService.GetListTopicFromCache(options.ThreadId.Value, hostname).Where(r => r.IsOpen).OrderBy(r => r.SeqOrder).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name }).ToList();

            //Services.Notifier.Information(T("{0} - {1}",options.ThreadId, options.ListTopic.Count()));

            return options;
        }
        public IContentQuery<ForumPostPart, ForumPostPartRecord> ForumFilterFrontEnd(PostFilterOptions options, string hostname)
        {
            IContentQuery<ForumPostPart, ForumPostPartRecord> results;

            results = _postService.GetListPostQueryByHostName(hostname);// Lấy những bài viết có trạng thái bình thường

            //Find by Thread
            if (options.ThreadId != 0 && options.ThreadId.HasValue)
            {
                var listTopicId = _threadService.GetListTopicFromCache(options.ThreadId.Value, hostname).Select(r => r.Id).ToArray();
                results = results.Where(r => listTopicId.Contains(r.Thread.Id));
                //Services.Notifier.Information(T("THreadId: {0}",options.ThreadId));
            }

            // Find by TopiId
            if (options.TopicId > 0)
            {
                results = results.Where(r => r.Thread.Id == options.TopicId);
                //Services.Notifier.Information(T("TopicId: {0}", options.TopicId));
            }

            //Find by SearchText
            if (!string.IsNullOrEmpty(options.SearchText))
                results = results.Where(r => r.Content.Contains(options.SearchText));

            //Services.Notifier.Information(T("SearchText: {0}", options.SearchText));

            //Find by post title
            if (!string.IsNullOrEmpty(options.Title))
                results = results.Where(r => r.Title.Contains(options.Title.Trim()));

            //Services.Notifier.Information(T("Title: {0}", options.Title));

            //find from DateCreated
            if (!string.IsNullOrEmpty(options.DateCreateFrom) && !string.IsNullOrEmpty(options.DateCreateTo))
            {
                const string format = "dd/MM/yyyy";
                var provider = CultureInfo.InvariantCulture;
                const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;
                DateTime createdFrom, createdTo;

                DateTime.TryParseExact(options.DateCreateFrom, format, provider, style, out createdFrom);
                DateTime.TryParseExact(options.DateCreateTo, format, provider, style, out createdTo);

                if (createdFrom.Year > 1) results = results.Where(p => p.DateCreated >= createdFrom);
                if (createdTo.Year > 1) results = results.Where(p => p.DateCreated <= createdTo.AddHours(24));
            }

            //Find by UserNameOrEmail
            //options.UserNameOrEmail = !string.IsNullOrEmpty(options.UserNameOrEmail) ? options.UserNameOrEmail.Trim() : "";
            if (string.IsNullOrEmpty(options.UserNameOrEmail)) return results;

            int? userId = _userGroupService.GetUsers().Where(r => r.UserName == options.UserNameOrEmail || r.Email == options.UserNameOrEmail).Select(r => r.Id).FirstOrDefault();
            results = results.Where(r => r.UserPost.Id == userId);
            //Services.Notifier.Information(T("UserNameOrEmail: {0}", options.UserNameOrEmail));

            return results;
        }

        #endregion

        #region Trang cá nhân

        public List<PersonalPageEntry> BuildListPostByBlogId(List<ForumPostPart> results, int pageIndex, int userId)
        {
            string hostName = _hostNameService.GetHostNameSite();

            return results.OrderByDescending(r => r.BlogDateCreated).Select(r => new PersonalPageEntry
            {
                BlogId = r.Blog.Id,
                BlogDateCreated = r.BlogDateCreated,
                TimeAgo = r.BlogDateCreated.Value.ToLocalTime().TimeAgo(),
                IsShareBlog = r.IsShareBlog,

                PostItemInfo = BuildPostDetailById(r.BlogPostId, hostName),
                //Title = r.Title,
                //TopicShortName = r.IsShareBlog && r.Thread  != null ? r.Thread.ShortName : "not",
                //ThreadShortName = r.IsShareBlog && r.Thread != null ? _threadService.GetThreadInfoByTopic(hostName,r.Thread).ShortName : "not",
                //DefaultImage = r.IsShareBlog && r.Thread != null ? _postService.GetDefaultImageUrl(r, _threadService.GetThreadInfoByTopic(hostName,r.Thread).ShortName) : "/Themes/Bootstrap/Styles/images/forum-category-default-images/noi-quy/noi-quy-va-huong-dan-chung-01.jpg",
                //DisplayForNameForUrl = r.IsShareBlog ? r.Title.ToSlug() : "",

                Content = r.Content,
                Id = r.Id,
                UserInfo = new UserInfo
                {
                    Id = r.UserPost.Id,
                    UserName = r.UserPost.UserName,
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                    Avatar = _userRealEstateService.GetUserAvatar(r.UserPost.Id),
                    Signature = ""
                },
                PostComment = _commentService.BuildLoadComment(userId, r.Id, pageIndex)
            }).ToList();
        }
        public List<PostByTopicEntry> BuildPostByTopicEntry(IEnumerable<ForumPostPart> list, ThreadInfo thread)
        {
            return list.Select(r => new PostByTopicEntry
                    {
                        Id = r.Id,
                        Title = r.Title.Trim(),
                        Content = _postService.MySubString(r.Content.StripHtml(), 300),
                        DefaultImage = _postService.GetDefaultImageUrl(r, thread.ShortName),
                        Description = !string.IsNullOrEmpty(r.Description) ? r.Description.StripHtml() : r.Description,
                        DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                        UserInfo = new UserInfo
                        {
                            Id = r.UserPost.Id,
                            DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                            UserName = r.UserPost.UserName.Split('@')[0]
                        }
                    }).ToList();
        }
        public List<PostByTopicEntry> BuildPostByTopicEntry(IEnumerable<ForumPostPart> list)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return list.Select(r => new PostByTopicEntry
                {
                    Id = r.Id,
                    Title = r.Title.Trim(),
                    Content = _postService.MySubString(r.Content.StripHtml(), 300),
                    DefaultImage = _postService.GetDefaultImageUrl(r, _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName),
                    Description = !string.IsNullOrEmpty(r.Description) ? r.Description.StripHtml() : r.Description,
                    DateUpdated = r.DateUpdated.Value.ToLocalTime(),
                    ThreadShortName = _threadService.GetThreadInfoByTopic(hostname, r.Thread).ShortName,
                    TopicShortName = r.Thread.ShortName,
                    UserInfo = new UserInfo
                    {
                        Id = r.UserPost.Id,
                        DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.UserPost.Id),
                        UserName = r.UserPost.UserName.Split('@')[0]
                    }

                }).ToList();
        }
        #endregion

        public IContentQuery<ForumPostPart, ForumPostPartRecord> LoadPostOfUser(int userId)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return _postService.GetListPostQueryByHostName(hostname).Where(r => r.UserPost.Id == userId && r.Blog == null);
        }

        public PostItem BuildPostDetailById(int? id, string hostName)
        {
            if (id == null)
                return null;

            var record = Services.ContentManager.Get<ForumPostPart>(id.Value);

            if (record != null)
            {
                var model = new PostItem
                    {
                        Id = record.Id,
                        Title = (!string.IsNullOrEmpty(record.Title)) ? record.Title : "",
                        Content = (!string.IsNullOrEmpty(record.Content)) ? _postService.MySubString(record.Content, 300) : "not",//record.Content.Substring(0,300),
                        TopicShortName = record.Thread != null ? record.Thread.ShortName : "not",
                        ThreadShortName = record.Thread != null ? 
                                        (_threadService.GetThreadInfoByTopic(hostName, record.Thread) != null ? _threadService.GetThreadInfoByTopic(hostName, record.Thread).ShortName : "not" ) : "not",
                        DefaultImage = record.Thread != null
                        ?_postService.GetDefaultImageUrl(record, (_threadService.GetThreadInfoByTopic(hostName, record.Thread) != null ?
                        _threadService.GetThreadInfoByTopic(hostName, record.Thread).ShortName : "not"))
                        : "/Themes/Bootstrap/Styles/images/forum-category-default-images/noi-quy/noi-quy-va-huong-dan-chung-01.jpg"
                    };

                return model;
            }
            return null;
        }
    }
}