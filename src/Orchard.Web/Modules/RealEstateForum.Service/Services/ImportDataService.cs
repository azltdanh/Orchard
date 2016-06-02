using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;

using RealEstateForum.Service.Models;
using System.IO;

namespace RealEstateForum.Service.Services
{
    public interface IImportDataService : IDependency
    {
        void ImportDataByTopicId(int oldTopicId, int newTopicId, string hostname);
        void ImportDataCssImageForThread(int oldThreadId, int newThreadId);
    }

    public class ImportDataService : IImportDataService
    {
        private readonly IContentManager _contentManager;
        private readonly IThreadAdminService _threadService;
        private readonly IPostAdminService _postService;

        public ImportDataService(
            IOrchardServices services,
            IContentManager contentManager,
            IThreadAdminService threadService,
            IPostAdminService postService)
        {
             Services = services;
            _contentManager = contentManager;
            _postService = postService;
            _threadService = threadService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public void ImportDataByTopicId(int oldTopicId, int newTopicId, string hostname)
        {
            /*var listPostByOldTopicId = _contentManager.Query<PageForumPart, PageForumPartRecord>()
                .Where(r => r.Type == "NewPageContent" && r.ParentPageId == oldTopicId).List();
            if (listPostByOldTopicId != null)
            {
                DateTime? dateBlog = null;
                // 153368: bình thường / 153370: Rác: / 610187: Đang soạn => 611413: Bình thường / 611414: Rác / 611415 : Đang soạn
                foreach (var entry in listPostByOldTopicId)
                {
                    int StatusId = 611413;
                    if (entry.StatusForum != null)
                    {
                        switch (entry.StatusForum.Id)
                        {
                            case 153368:
                                StatusId = 611413;
                                break;
                            case 153370:
                                StatusId = 611414;
                                break;
                            case 610187:
                                StatusId = 611415;
                                break;
                            default:
                                StatusId = 611413;
                                break;
                        }
                    }

                    var p = Services.ContentManager.New<ForumPostPart>("ForumPost");
                    p.Title = entry.Name;
                    p.Description = entry.Title;
                    p.Content = entry.Content;
                    p.UserPost = entry.UserForum;
                    p.Blog = null;
                    p.BlogDateCreated = dateBlog;
                    p.Thread = _threadService.GetTopicPartRecordById(newTopicId, hostname);

                    p.PublishStatus = _postService.GetPublishStatusPartRecordById(611410);
                    p.StatusPost = _postService.GetStatusForumPartRecordById(StatusId);

                    p.DateCreated = entry.DateCreate.HasValue ? entry.DateCreate.Value : DateTime.Now;
                    p.DateUpdated = entry.DateUpdate;
                    p.Views = entry.ViewCount;
                    p.SeqOrder = entry.SeqOrder;
                    p.IsPinned = entry.Pinned;
                    p.TimeExpiredPinned = entry.TimeEndPinned;
                    p.IsHeighLight = entry.Hightlights;
                    p.IsMarket = entry.PublishMarket;
                    p.IsProject = entry.PublishProject;
                    p.IsShareBlog = false;

                    if (!string.IsNullOrEmpty(entry.CssImage))
                    {
                        p.CssImage = MoveFileToFolder(entry.CssImage, "/UserFiles/" + newTopicId + "/");///Media/Default/TopicImage
                    }
                    else
                    {
                        p.CssImage = null;
                    }


                    Services.ContentManager.Create(p);

                    Services.Notifier.Information(T("Bài viết {0} vừa được import", p.Title));
                }
                Services.Notifier.Information(T("Đã import {0} bài viết từ chuyên đề cũ: {1} => chuyên đề mới: {2}.", listPostByOldTopicId.Count(), oldTopicId, newTopicId));
            }
            else
            {
                Services.Notifier.Information(T("Không có bài viết nào được tìm thấy"));
            }*/
        }
        public void ImportDataCssImageForThread(int oldThreadId, int newThreadId)
        {
            //var oldCssImage = _contentManager.Query<PageForumPart, PageForumPartRecord>().Where(r => r.Id == oldThreadId).List().First().CssImage;
            //if (!string.IsNullOrEmpty(oldCssImage))
            //{
            //    var threadPart = Services.ContentManager.Get<ForumThreadPart>(newThreadId);
            //    string pathDefault = "/UserFiles/IconCategory/" + newThreadId + "/";
            //    threadPart.DefaultImage = pathDefault + MoveFileToFolder(oldCssImage, pathDefault);
            //    Services.Notifier.Information(T("Đã import CssImage {0} từ chuyên mục cũ: {1} => chuyên mục mới: {2}.", oldCssImage, oldThreadId, newThreadId));
            //}
        }
        public string MoveFileToFolder(string oldPathFile, string newPathFolder)
        {
            string fileName = oldPathFile.Split('/').Last();

            // 
            int index = oldPathFile.LastIndexOf('/');
            string oldPathFolder = oldPathFile.Substring(0, index);
            string _oldPathFolder = Services.WorkContext.HttpContext.Server.MapPath("~" + oldPathFolder);
            DirectoryInfo Folders = new DirectoryInfo(_oldPathFolder);

            // 
            string _oldFilePath = Services.WorkContext.HttpContext.Server.MapPath("~" + oldPathFile);
            FileInfo fileInfo = new FileInfo(_oldFilePath);

            string _uploadsFolder = Services.WorkContext.HttpContext.Server.MapPath("~" + newPathFolder);
            DirectoryInfo Folder = new DirectoryInfo(_uploadsFolder);
            if (!Folder.Exists) Folder.Create();

            //HttpPostedFileBase file = Path.getf

            
            string _newPathFile = Folder + "/" + fileName;

            
            if (fileInfo.Exists)
            {
                try
                {
                    File.Copy(_oldFilePath, _newPathFile, true);//
                    File.Copy(Path.Combine(_oldFilePath), Path.Combine(_newPathFile), true);
                    if (Folders.Exists)
                    {
                        Services.Notifier.Information(T("Đã xóa {0}", Folders.FullName));
                        Folders.Delete(true);//Xóa thư mục cũ trong Iconcategory đi
                    }
                    else
                    {
                        Services.Notifier.Information(T("Không xóa được thư mục {0}", Folders.FullName));
                    }
                }
                catch (Exception e)
                {
                    Services.Notifier.Information(T("{0}",e.Message));
                }
                return fileName;
            }

            return null;
        }
    }
}