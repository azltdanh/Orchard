
using System.Collections.Generic;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Services;
using RealEstate.Models;
using System.Web;
using System.IO;
using ImageResizer;
using RealEstate.Helpers;

namespace RealEstate.Services
{
    public interface IVideoManageService : IDependency
    {
        IEnumerable<VideoTypePart> ListVideoTypes();
        VideoTypePart GetVideoTypePart(int id);
        string UploadFileVideoImage(HttpPostedFileBase file, string domain, int videoId);
        string UploadFileVideoImage(HttpPostedFileBase file, string domain, int videoId, string oldCssImage);
    }

    public class VideoManageService : IVideoManageService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly ISignals _signals;

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours

        public VideoManageService(
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IOrchardServices services
            )
        {
            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;

            Services = services;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public IEnumerable<VideoTypePart> ListVideoTypes()
        {
            return _contentManager.Query<VideoTypePart, VideoTypePartRecord>().List();
        }

        public VideoTypePart GetVideoTypePart(int id)
        {
            return _contentManager.Get<VideoTypePart>(id);
        }

        #region //Upload Image
        public string UploadFileVideoImage(HttpPostedFileBase fileBase, string domain, int videoId)
        {
            string fileLocation = "/Media/Video/" + domain + "/" + videoId + Path.GetExtension(fileBase.FileName);
            fileBase.SaveAsFileLocation(fileLocation);

            return fileLocation;
        }
        public string UploadFileVideoImage(HttpPostedFileBase fileBase, string domain, int videoId, string oldCssImage)
        {
            if (!string.IsNullOrEmpty(oldCssImage))
            {
                var fileInfo = new FileInfo("/Media/Video/" + domain + "/" + oldCssImage);
                if (fileInfo.Exists) fileInfo.Delete();
            }

            string fileLocation = "/Media/Video/" + domain + "/" + videoId + Path.GetExtension(fileBase.FileName);
            fileBase.SaveAsFileLocation(fileLocation);
            
            return fileLocation;
        }
        #endregion

    }
}
