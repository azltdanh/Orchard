using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using RealEstate.Helpers;
using System.Web;

namespace RealEstateForum.Service.Services
{
    public interface IFileCacheService : IDependency
    {
        string ThreadCacheFromFile(int threadId);
        void ThreadCacheToFile(int threadId, string content);
        void ClearCacheFileByThreadId(int threadId);

        string PostCacheFromFile(int postId);
        void PostCacheToFile(int postId, string content);
        void ClearCacheFileByPostId(int postId);
    }
    public class FileCacheService : IFileCacheService
    {
        //private readonly IContentManager _contentManager;
        //private readonly IThreadAdminService _threadService;
        //private readonly IHostNameService _hostNameService;

        public FileCacheService(
            IOrchardServices services)
            //IContentManager contentManager,
            //IThreadAdminService threadService,
            //IHostNameService hostNameService)
        {
            Services = services;
            //_contentManager = contentManager;
            //_threadService = threadService;
            //_hostNameService = hostNameService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        private static readonly string RootCachePath = HttpContext.Current.Server.MapPath("~/Media/Cache");


        public string ThreadCacheFromFile(int threadId)
        {
            string cachePath = RootCachePath + "/Forum/Thread";
            string filePath = cachePath + "/" + threadId + ".txt";

            return FileHelper.FileExits(filePath) ? FileHelper.ReadFileInUtf8(filePath) : null;
        }
        public void ThreadCacheToFile(int threadId, string content)
        {
            string cachePath = RootCachePath + "/Forum/Thread";
            string filePath = cachePath + "/" + threadId + ".txt";
            FileHelper.WriteFileInUtf8(filePath, content);
        }
        public void ClearCacheFileByThreadId(int threadId)
        {
            string cachePath = RootCachePath + "/Forum/Thread";
            string filePath = cachePath + "/" + threadId + ".txt";
            FileHelper.DeleteFile(filePath);
        }

        public string PostCacheFromFile(int postId)
        {
            string cachePath = RootCachePath + "/Forum/PostDetail";
            string filePath = cachePath + "/" + postId + ".txt";

            return FileHelper.FileExits(filePath) ? FileHelper.ReadFileInUtf8(filePath) : null;
        }
        public void PostCacheToFile(int postId, string content)
        {
            string cachePath = RootCachePath + "/Forum/PostDetail";
            string filePath = cachePath + "/" + postId + ".txt";
            FileHelper.WriteFileInUtf8(filePath, content);
        }
        public void ClearCacheFileByPostId(int postId)
        {
            string cachePath = RootCachePath + "/Forum/PostDetail";
            string filePath = cachePath + "/" + postId + ".txt";
            FileHelper.DeleteFile(filePath);
        }   
    }
}