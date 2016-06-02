using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Caching;
using Orchard.Services;
using Orchard.UI.Notify;

using RealEstateForum.Service.ViewModels;
using RealEstateForum.Service.Models;
using System.Web;
using Orchard.Data;
using RealEstate.Services;

namespace RealEstateForum.Service.Services
{
    public interface IThreadAdminService :  IDependency
    {
    #region //Interface THREAD

        IContentQuery<ForumThreadPart, ForumThreadPartRecord> GetListThreadByHostName(string hostname);
        IContentQuery<ForumThreadPart, ForumThreadPartRecord> GetListTopicQueryByThreadId(string hostName, int threadId);

        IEnumerable<ForumThreadPart> GetListThread();
        IEnumerable<ForumThreadPart> GetListThreadFromCache(string hostname);
        ThreadInfo GetThreadInfoById(string hostName, int id);
        ThreadInfo GetThreadInfoByTopic(string hostName, ForumThreadPartRecord postThread);
        ThreadInfo GetThreadInfoByShortName(string hostName, string shortName);
        ForumPostStatusPart GetStatusForumPartRecordByCssClass(string cssClass);


        bool CloseOrOpenThread(int threadId);
        bool DeleteThreadById(int threadId, string hostname);
        void ClearCacheUpdateThread(string hostname);

        bool CheckIsExistThreadName(string hostName, string threadName);
        bool CheckIsExistThreadName(string hostName, int threadId, string threadName);
        bool CheckIsExistThreadShortName(string hostName, string threadShortName);
        bool CheckIsExistThreadShortName(string hostName, int threadId, string threadShortName);

        int CountTopicByThreadIdToCache(int threadId, string hostname);
    #endregion
    #region //Interface TOPIC
        IEnumerable<ForumThreadPart> GetListTopicFromCache(string hostname);
        IEnumerable<ForumThreadPart> GetListTopicFromCache(int id, string hostname);
        ForumThreadPart GetTopicPartRecordById(int id, string hostname);
        ForumThreadPart GetTopicPartRecordById(int id);
        ForumThreadPart GetTopicPartRecordByShortName(string shortName, string hostname);
        void ClearCacheUpdateTopic(ForumThreadPart thread, string hostname);
        bool DeleteTopicById(int topicId, string hostname);

        bool CheckIsExistTopicName(string hostName, int threadId, string topicName);
        bool CheckIsExistTopicName(string hostName, int threadId, int topicId, string topicName);
        bool CheckIsExistTopicShortName(string hostName, int threadId, string topicShortName);
        bool CheckIsExistTopicShortName(string hostName, int threadId, int topicId, string topicShortName);
    #endregion
        string UploadFileThreadIcon(HttpPostedFileBase file, int threadId);

        #region //Status
        IEnumerable<PublishStatusPart> GetListPublishStatusFromCache();
        IEnumerable<ForumPostStatusPart> GetListPostStatusFromCache();
        PublishStatusPart GetPublishStatusPartRecordById(int id);
        PublishStatusPart GetPublishStatusPartRecordByName(string name);
        ForumPostStatusPart GetStatusForumPartRecordById(int id);
        #endregion
    }
    public class ThreadAdminService : IThreadAdminService
    {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IHostNameService _hostNameService;
        private readonly IRepository<ForumThreadPartRecord> _threadRepository;
        private readonly ISignals _signals;
        private int cacheTimeSpan = 60 * 24; // Cache for 24 hours

        public ThreadAdminService(IOrchardServices services,
            IContentManager contentManager,
            IClock clock,
            ISignals signals,
            IHostNameService hostNameService,
            IRepository<ForumThreadPartRecord> threadRepository,
            ICacheManager cacheManager)
        {
            Services = services;
            _contentManager = contentManager;
            _clock = clock;
            _signals = signals;
            _hostNameService = hostNameService;
            _threadRepository = threadRepository;
            _cacheManager = cacheManager;


            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

    #region //Thread Service

        public IContentQuery<ForumThreadPart, ForumThreadPartRecord> GetListThreadByHostName(string hostname)
        {
            if (hostname == _hostNameService.GetHostNamePartByClass("host-name-main").Name)
                return _contentManager.Query<ForumThreadPart, ForumThreadPartRecord>().Where(r => r.HostName == hostname || r.HostName == null);
            else
                return _contentManager.Query<ForumThreadPart, ForumThreadPartRecord>().Where(r => r.HostName == hostname);
        }
        public IContentQuery<ForumThreadPart, ForumThreadPartRecord> GetListTopicQueryByThreadId(string hostName, int threadId)
        {
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId != null && r.ParentThreadId == threadId);
        }

        public IEnumerable<ForumThreadPart> GetListThread()
        {
            return _contentManager.Query<ForumThreadPart, ForumThreadPartRecord>().Where(r => r.ParentThreadId == null).List();
        }

        public IEnumerable<ForumThreadPart> GetListThreadFromCache(string hostname)
        {
            return _cacheManager.Get("ListThreadFromCache_" + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ListThread_Changed_" + hostname));

                var result = GetListThreadByHostName(hostname).Where(r => r.ParentThreadId == null).List().Select(a => a);

                return result;
            });
        }
        
        public ThreadInfo GetThreadInfoById(string hostName, int id)
        {
            return GetListThreadByHostName(hostName).Where(r => r.Id == id)
                .Slice(1).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name, ShortName = r.ShortName, DefaultImage = r.DefaultImage }).FirstOrDefault();
        }
       public ThreadInfo GetThreadInfoByTopic(string hostName, ForumThreadPartRecord postThread)
        {
            return _cacheManager.Get("GetThreadInfoByTopicCache_" + hostName + "_" + postThread.ParentThreadId , ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("GetThreadInfoByTopicCache_" + hostName + "_" + postThread.Id + "_Changed"));

                return GetListThreadByHostName(hostName).Where(r => r.Id == postThread.ParentThreadId)
                    .Slice(1).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name, ShortName = r.ShortName, DefaultImage = r.DefaultImage }).FirstOrDefault();
            });
        }
        public ThreadInfo GetThreadInfoByShortName(string hostName, string shortName)
        {
            return GetListThreadByHostName(hostName)
                .Where(r => r.ParentThreadId == null && r.ShortName == shortName)
                .Slice(1).Select(r => new ThreadInfo { Id = r.Id, Name = r.Name, ShortName = r.ShortName, DefaultImage = r.DefaultImage}).FirstOrDefault();
        }
        public bool CloseOrOpenThread(int threadId)
        {
            var thread = _contentManager.Get<ForumThreadPart>(threadId);
            if (thread != null)
            {
                thread.IsOpen = !thread.IsOpen;

                return true;
            }
            return false;
        }
        public bool DeleteThreadById(int threadId, string hostname)
        {
            var haveTopic = _contentManager.Query<ForumThreadPart, ForumThreadPartRecord>().Where(r => r.ParentThreadId == threadId);
            if (haveTopic.Count() > 0)// If have topic
            {
                return false;
            }
            else
            {
                var thread = _contentManager.Get<ForumThreadPart>(threadId);
                _contentManager.Remove(thread.ContentItem);
                ClearCacheUpdateThread(hostname);//ClearCache
                return true;
            }
        }
        public void ClearCacheUpdateThread(string hostname)//int ThreadId
        {
            _signals.Trigger("ListThread_Changed_" + hostname);//Clear cache Thread
        }

        public bool CheckIsExistThreadName(string hostName, string threadName)
        {
            string threadNameTemp = threadName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId == null && r.Name == threadNameTemp).Count() > 0;
        }
        public bool CheckIsExistThreadName(string hostName, int threadId,string threadName)
        {
            string threadNameTemp = threadName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.Id != threadId && r.Name == threadNameTemp).Count() > 0;
        }
        public bool CheckIsExistThreadShortName(string hostName, string threadShortName)
        {
            string shortName = threadShortName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId == null && r.ShortName == shortName).Count() > 0;
        }
        public bool CheckIsExistThreadShortName(string hostName, int threadId, string threadShortName)
        {
            string shortName = threadShortName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.Id != threadId && r.ShortName == shortName).Count() > 0;
        }

        public int CountTopicByThreadIdToCache(int threadId, string hostname)
        {
            //return _cacheManager.Get("CountTopicByThread_Cache_" + ThreadId + hostname, ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("CountTopicByThread_Cache_Changed_" + ThreadId + hostname));

                return GetListThreadByHostName(hostname).Where(r => r.ParentThreadId == threadId).Count();
            //});
        }
        
    #endregion

    #region //Topic Service
        public IEnumerable<ForumThreadPart> GetListTopicFromCache(string hostname)
        {
            return _cacheManager.Get("ListTopicFromCache_" + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ListTopic_Changed_" + hostname));

                return GetListThreadByHostName(hostname).Where(r => r.ParentThreadId != null).OrderBy(a => a.SeqOrder).List().Select(a => a); ;
            });
        }

        public IEnumerable<ForumThreadPart> GetListTopicFromCache(int id, string hostname)
        {
            return _cacheManager.Get("ListTopicFromCache_" + id + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ListTopic_Changed_"+ id + hostname));

                return GetListThreadByHostName(hostname).Where(r => r.ParentThreadId != null && r.ParentThreadId == id).OrderBy(a => a.SeqOrder).List().Select(a => a);
            });
        }

        public ForumThreadPart GetTopicPartRecordById(int id, string hostname)
        {
            return _cacheManager.Get("GetTopicPartByIdCache_" + hostname + "_" + id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("GetTopicPartByIdCache_" + hostname + "_" + id + "_Changed"));

                return GetListThreadByHostName(hostname).Where(r => r.Id == id).Slice(1).Select(r=>r).FirstOrDefault();

            });
        }

        public ForumThreadPart GetTopicPartRecordById(int id)
        {
            return _contentManager.Query<ForumThreadPart, ForumThreadPartRecord>().Where(r => r.Id == id).Slice(1).FirstOrDefault();
        }

        public ForumThreadPart GetTopicPartRecordByShortName(string shortName, string hostname)
        {
            return _cacheManager.Get("GetTopicPartByShortNameCache_" + hostname + "_" + shortName, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("GetTopicPartByShortNameCache__" + hostname + "_" + shortName + "_Changed"));

                return GetListThreadByHostName(hostname).Where(r => r.ShortName == shortName).Slice(1).Select(r => r).FirstOrDefault();

            });
        }
        public void ClearCacheUpdateTopic(ForumThreadPart thread, string hostname)
        {
            _signals.Trigger("ListTopic_Changed_" + thread.ParentThreadId + hostname);//Clear cache ListTopic
            //_signals.Trigger("CountTopicByThread_Cache_Changed_" + Thread.ParentThreadId + hostname);//Clear cache Count Topic
            _signals.Trigger("GetThreadInfoByTopicCache_" + hostname + "_" + thread.ParentThreadId + "_Changed");//Clear cache Thread
            _signals.Trigger("GetTopicPartByIdCache_" + hostname + "_" + thread.Id + "_Changed");

            ClearCacheUpdateThread(hostname);
        }

        public bool DeleteTopicById(int topicId, string hostname)
        {
            //int? ThreadId = GetListThreadByHostName(hostname).Where(r=>r.Id == TopicId).Slice(1).Select(r => r.ParentThreadId).FirstOrDefault();// _contentManager.Get<ForumThreadPart>(TopicId).ParentThreadId.Value;
            try
            {
                var result = _contentManager.Query<ForumPostPart, ForumPostPartRecord>()
                    .Where(r => r.Thread != null && r.StatusPost == GetStatusForumPartRecordByCssClass("st-none").Record);

                result = hostname == _hostNameService.GetHostNamePartByClass("host-name-main").Name ? result.Where(r => r.HostName == hostname || r.HostName == null) : result.Where(r => r.HostName == hostname);

                var havePost = result.Where(r => r.Thread.Id == topicId);

                if (havePost.Count() > 0)// If have Post
                {
                    return false;
                }
                var thread = _contentManager.Get<ForumThreadPart>(topicId);

                _contentManager.Remove(thread.ContentItem);
                _threadRepository.Delete(thread.Record);
                ClearCacheUpdateTopic(thread, hostname);

                return true;

            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("Error: " + ex.Message));
                return false;
            }
        }

        public bool CheckIsExistTopicName(string hostName, int threadId, string topicName)
        {
            string threadName = topicName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId == threadId && r.Name == threadName).Count() > 0;
        }
        public bool CheckIsExistTopicName(string hostName, int threadId, int topicId, string topicName)
        {
            string threadName = topicName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId == threadId && r.Id != topicId && r.Name == threadName).Count() > 0;
        }
        public bool CheckIsExistTopicShortName(string hostName, int threadId, string topicShortName)
        {
            string shortName = topicShortName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId == threadId && r.ShortName == shortName).Count() > 0;
        }
        public bool CheckIsExistTopicShortName(string hostName, int threadId, int topicId, string topicShortName)
        {
            string shortName = topicShortName.Trim();
            return GetListThreadByHostName(hostName).Where(r => r.ParentThreadId == threadId && r.Id != topicId && r.ShortName == shortName).Count() > 0;
        }

    #endregion

    #region //Upload Image
        public string UploadFileThreadIcon(HttpPostedFileBase file, int threadId)
        {
            string uploadsFolder = Services.WorkContext.HttpContext.Server.MapPath("~/Media/Default/IconCategory/" + threadId.ToString());
            var folder = new DirectoryInfo(uploadsFolder);

            if (!folder.Exists) folder.Create();
            else
            {
                var getfiles = Directory.GetFiles(folder.FullName);
                foreach (var filePath in getfiles)
                {
                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists) fileInfo.Delete();
                }
            }

            HttpPostedFileBase fileBase = file;

            string fileLocation = "~/Media/Default/IconCategory/" + threadId + "/" + Guid.NewGuid() + Path.GetExtension(file.FileName);
            fileBase.SaveAs(Services.WorkContext.HttpContext.Server.MapPath(fileLocation));

            return fileLocation.Replace("~", "");
        }
    #endregion

        
        #region //Status
        public IEnumerable<PublishStatusPart> GetListPublishStatusFromCache()
        {
            return _cacheManager.Get("ListPublishStatusFromCache", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ListPublishStatusFromCache_Changed"));

                return _contentManager.Query<PublishStatusPart, PublishStatusPartRecord>().List().Select(r => r);
            });
        }
        public IEnumerable<ForumPostStatusPart> GetListPostStatusFromCache()
        {
            return _cacheManager.Get("ListPostStatusFromCache", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ListPostStatusFromCache_Changed"));

                return _contentManager.Query<ForumPostStatusPart, ForumPostStatusPartRecord>().List().Select(r => r);
            });
        }

        public PublishStatusPart GetPublishStatusPartRecordById(int id)
        {
            return GetListPublishStatusFromCache().FirstOrDefault(r => r.Id == id);
        }

        public PublishStatusPart GetPublishStatusPartRecordByName(string name)
        {
            return GetListPublishStatusFromCache().FirstOrDefault(r => r.Name == name);
        }

        public ForumPostStatusPart GetStatusForumPartRecordById(int id)
        {
            return GetListPostStatusFromCache().FirstOrDefault(r => r.Id == id);
        }
        public ForumPostStatusPart GetStatusForumPartRecordByCssClass(string cssClass)
        {
            return GetListPostStatusFromCache().FirstOrDefault(r => r.CssClass == cssClass);
        }
        #endregion
    }
}