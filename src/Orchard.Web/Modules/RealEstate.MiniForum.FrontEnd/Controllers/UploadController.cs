using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Services;
using RealEstate.Models;
using RealEstate.ViewModels;

using System.Linq;
using ImageResizer;
using RealEstate.Helpers;

namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    public class UploadController : Controller
    {

        private readonly IContentManager _contentManager;


        public UploadController(
            IOrchardServices services, IContentManager contentManager)
        {
            Services = services;
            _contentManager = contentManager;
        }
        public IOrchardServices Services { get; set; }

        private const string DefaultPathImage = "/Media/ForumPost/Images";//Media/Forum/Post
        private const string DefaultPathFileAttachment = "/Media/ForumPost/FileAttachments";

        [HttpPost]
        public ActionResult UploadMedia(HttpPostedFileBase fileBase)
        {
            if (fileBase == null || fileBase.ContentLength <= 0) fileBase = Request.Files[0];
            if (fileBase != null && fileBase.ContentLength > 0)
            {
                string fileLocation = DefaultPathImage + "/" + Guid.NewGuid() + Path.GetExtension(fileBase.FileName);
                //string uploadsFolder = Server.MapPath(DefaultPath + "/UserMedia/" + folderName);
                fileBase.SaveAsFileLocation(fileLocation);

                var b = new Bitmap(Server.MapPath(fileLocation));

                return Json(new { success = true, path = fileLocation, _w = b.Width, _h = b.Height });
            }
            return Json(new { success = false, name = fileBase.FileName });
        }
        // File Attachment
        [HttpPost]
        public ActionResult FileAttachments(HttpPostedFileBase fileBase, string folderNames)
        {
            if (fileBase == null || fileBase.ContentLength <= 0) fileBase = Request.Files[0];
            if (fileBase != null && fileBase.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath(DefaultPathFileAttachment);
                var folder = new DirectoryInfo(uploadsFolder);
                if (!folder.Exists) folder.Create();

                string fileLocation = DefaultPathFileAttachment + Guid.NewGuid() + Path.GetExtension(fileBase.FileName);
                //string fileLocation = DefaultPath + "/FileAttachment/" + folderNames + "/" + Guid.NewGuid() + Path.GetExtension(fileBase.FileName);

                fileBase.SaveAs(Server.MapPath(fileLocation));

                return Json(new { success = true, path = fileLocation, name = fileBase.FileName });
            }

            return Json(new { success = false, name = fileBase.FileName });
        }
    }
}