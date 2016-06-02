using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Orchard.UI.Admin;
using Orchard.Users.Models;
using System.IO;

namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class MoveFileController : Controller
    {

        public MoveFileController(IOrchardServices services)
        {
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Services = services;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        // GET: MoveFile
        public ActionResult Index()
        {
            string sourcePath = Services.WorkContext.HttpContext.Server.MapPath("/Media/ForumPost/UserMedia");
            string targetPath = Services.WorkContext.HttpContext.Server.MapPath("/Media/ForumPost/Images");

            MoveFile(sourcePath, targetPath, true);

            Services.Notifier.Information(T("Move Images Files Success"));

            return View();
        }

        public ActionResult MoveFileAttach()
        {
            string sourcePath = Services.WorkContext.HttpContext.Server.MapPath("/Media/ForumPost/FileAttachment");
            string targetPath = Services.WorkContext.HttpContext.Server.MapPath("/Media/ForumPost/FileAttachments");

            MoveFile(sourcePath, targetPath, true);

            Services.Notifier.Information(T("Move Attachments Files Success"));

            return View("Index");
        }

        public ActionResult MoveCssImagesFile()
        {
            string targetPath = Services.WorkContext.HttpContext.Server.MapPath("/Media/ForumPost/Images");


            List<int> listTopicIds = new List<int>() { };

            for (int i = 611428; i <= 611460; i++)
            {
                listTopicIds.Add(i);
            }

            listTopicIds.AddRange(new List<int>() { 611460, 613760, 618537, 732439, 732447, 737069, 746309, 768076, 789197, 789198, 789198, 789200, 789201, 800051, 836022, 837725, 843454, 843461, 847255, 850252, 850819, 879917, 879918, 880492, 880495, 880571, 880572, 886809, 891895 });

            for (int i = 0; i < listTopicIds.Count; i++)
            {
                var tempPath = "/UserFiles/" + listTopicIds[i];
                string sourcePath = Services.WorkContext.HttpContext.Server.MapPath(tempPath);

                MoveFile(sourcePath, targetPath, false);
            }

            Services.Notifier.Information(T("Move Attachments Files Success"));

            return View("Index");
        }

        public void MoveFile(string sourcePath, string targetPath, bool isDeleteSourcePath)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            if (user.UserName.Equals("thanhtuank9"))
            {
                var userFilesFolder = new DirectoryInfo("/UserFiles");

                var sourcesFolder = new DirectoryInfo(sourcePath);
                var targetFolder = new DirectoryInfo(targetPath);

                if (!targetFolder.Exists) targetFolder.Create();
                if (sourcesFolder.Exists)
                {
                    #region Get Directories

                    //Tìm ra các thư mục con
                    string[] directories = System.IO.Directory.GetDirectories(sourcePath);

                    if (directories.Count() > 0)
                    {
                        foreach (var direction in directories)
                        {
                            //#1. Folder
                            var folder = new DirectoryInfo(direction);

                            //#2. Khác với thư mục sẽ được chuyển file qua
                            if (targetPath != direction)
                            {
                                string[] files = System.IO.Directory.GetFiles(direction);
                                //Services.Notifier.Information(T("FolderName: {0} => Files: {1} ", direction, files.Count()));
                                foreach (var file in files)
                                {
                                    //Services.Notifier.Information(T("FileName: " + file));
                                    string fileName = System.IO.Path.GetFileName(file);
                                    string destFile = System.IO.Path.Combine(targetPath, fileName);

                                    //Nếu file trong thư mục mới đã có thì xóa bên cái cũ đi
                                    var fileTarget = new FileInfo(targetFolder + "/" + fileName);
                                    if (fileTarget.Exists)
                                    {
                                        //Xóa trong thư mục nguồn đi
                                        string fileSourcName = System.IO.Path.GetFileName(file);
                                        var fileSource = new FileInfo(direction + "/" + fileSourcName);

                                        fileSource.Delete();
                                    }
                                    else
                                    {
                                        //Move qua thư mục mới
                                        System.IO.File.Move(file, destFile);
                                    }

                                }
                            }

                            //#3. Delete Folder
                            folder.Delete();
                        }
                    }


                    #endregion

                    #region Get Files

                    string[] fileMains = System.IO.Directory.GetFiles(sourcePath);
                    if (fileMains.Count() > 0)
                    {
                        foreach (var file in fileMains)
                        {
                            string fileName = System.IO.Path.GetFileName(file);
                            string destFile = System.IO.Path.Combine(targetPath, fileName);

                            //Nếu file trong thư mục mới đã có thì xóa bên cái cũ đi
                            var fileTarget = new FileInfo(targetFolder + "/" + fileName);

                            if (fileTarget.Exists)
                            {
                                //Xóa trong thư mục nguồn đi
                                string fileSourcName = System.IO.Path.GetFileName(file);
                                var fileSource = new FileInfo(sourcesFolder + "/" + fileSourcName);

                                fileSource.Delete();
                            }
                            else
                            {
                                //Move qua thư mục mới
                                System.IO.File.Move(file, destFile);
                            }
                        }
                    }

                    #endregion

                    if (isDeleteSourcePath && sourcesFolder != userFilesFolder)
                        sourcesFolder.Delete();
                }
            }
        }
    }
}