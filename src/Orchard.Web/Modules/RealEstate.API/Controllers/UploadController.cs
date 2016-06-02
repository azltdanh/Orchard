using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using Orchard;
using RealEstate.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Services;
using RealEstate.Helpers;
using System.IO;
using Orchard.Data;
using Orchard.Caching;

namespace RealEstate.API.Controllers
{
    public class UploadController : CrossSiteController
    {
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly IAddressService _addressService;
        private readonly ISignals _signals;
        private readonly IRepository<PropertyFilePartRecord> _userPropertyFileRepository;
        private readonly IPropertySettingService _settingService;

        public UploadController(IOrchardServices services, 
            IPropertyService propertyService,
            IAddressService addressService, 
            IUserGroupService groupService,
            IRepository<PropertyFilePartRecord> userPropertyFileRepository,
            ISignals signals,
            IPropertySettingService settingService)
        {
            Services = services;
            _propertyService = propertyService;
            _groupService = groupService;
            _addressService = addressService;
            _signals = signals;
            _userPropertyFileRepository = userPropertyFileRepository;
            _settingService = settingService;
        }
        public IOrchardServices Services { get; set; }

        private static readonly string cssImageNewPath = "/Media/ForumPost/Images";//FileName

        public JsonResult UploadImage(HttpPostedFileBase file, FormCollection frm)//, int contentItemId, int userId
        {
            try
            {
                if (file == null || file.ContentLength <= 0) file = Request.Files[0];

                if (file == null || file.ContentLength <= 0) return Json(new { success = false, message = "File is null" });

                int contentItemId = int.Parse(frm["contentItemId"].ToString());
                int userId = int.Parse(frm["userId"].ToString());

               var pFile = new PropertyFilePart();

               UserPart createdUser = _groupService.GetUser(userId);

               // contentItem is a Property
               var contentItemProperty = Services.ContentManager.Get<PropertyPart>(contentItemId);
               if (contentItemProperty != null)
               {
                   pFile = _propertyService.UploadPropertyImage(file, contentItemProperty, createdUser, true);
               }
               else
               {
                   // contentItem is a Apartment
                   var contentItemApartment = Services.ContentManager.Get<LocationApartmentPart>(contentItemId);
                   if (contentItemApartment != null)
                       pFile = _addressService.UploadApartmentImage(file, contentItemApartment, createdUser.Record, true);
                   // always set isPublished = true for Apartment
                   //pFile = _addressService.UploadApartmentImage(file, contentItemApartment, createdUser, isPublished);
                   else
                   {
                       var contentItemApartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(contentItemId);
                       if (contentItemApartmentBlockInfo != null)
                           pFile = _propertyService.UploadPropertyImageForBlockInfo(file, contentItemApartmentBlockInfo, createdUser, true);
                   }
               }

                return
                    Json(
                        new
                        {
                            success = file.FileName,
                            fileId = pFile.Id,
                            fileName = pFile.Name,
                            path = pFile.Path,
                            published = pFile.Published,
                            contentItemId
                        });

            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message});
            }

        }

        public JsonResult UploadUserAvatar(HttpPostedFileBase file, FormCollection frm)
        {
            try
            {
                if (file == null || file.ContentLength <= 0) file = Request.Files[0];

                if (file == null || file.ContentLength <= 0) return Json(new { success = false, message = "File is null" });
                int userId = int.Parse(frm["UserId"].ToString());

                string fileLocation = "";// "/Media/Default/Avatars/" + userId + "-" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);
                if (file != null && file.ContentLength > 0)
                {
                    fileLocation = file.SaveAsUserAvatar(userId);
                }

                return
                    Json(
                        new
                        {
                            success= true,
                            img = fileLocation
                        });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Forum

        public JsonResult UploadImageForumPost(HttpPostedFileBase fileBase)
        {
            try
            {
                if (fileBase == null || fileBase.ContentLength <= 0) fileBase = Request.Files[0];

                if (fileBase == null || fileBase.ContentLength <= 0) return Json(new { success = false, message = "File is null" });

                string fileNameUpload = Guid.NewGuid() + Path.GetExtension(fileBase.FileName);

                string fileLocation = cssImageNewPath  + "/" + fileNameUpload;
                fileBase.SaveAsFileLocation(fileLocation);

                #region old

                //string fileLocation = "/Media/Default/Avatars/" + userId + "-" + DateTime.Now.Ticks + Path.GetExtension(file.FileName);

                //string fileFolder = cssImageNewPath;
                //string fileLocation = fileFolder + "/" + fileNameUpload;

                //if (file != null && file.ContentLength > 0)
                //{
                //    string uploadsFolder = Services.WorkContext.HttpContext.Server.MapPath(fileFolder);

                //    var folder = new DirectoryInfo(uploadsFolder);
                //    if (!folder.Exists) folder.Create();

                //    if (!string.IsNullOrEmpty(fileLocation))
                //    {
                //        file.SaveAs(Server.MapPath(fileLocation));
                //    }
                //}

                #endregion

                return Json(new { status = true, path = fileLocation, imageName = fileNameUpload });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message});
            }
        }

        #endregion

        #region AjaxPermanentlyDeletePropertyImage

        [HttpPost]
        public JsonResult AjaxPermanentlyDeletePropertyImage(int id)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                var fileInfo = new FileInfo(Server.MapPath("~" + file.Path));
                if (fileInfo.Exists) fileInfo.Delete();

                Services.ContentManager.Remove(file.ContentItem);
                _userPropertyFileRepository.Delete(file.Record);

                return Json(new { id, success = true, message = "Xóa thành công" });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        [HttpPost]
        public JsonResult AjaxPermanentlyDeletePropertyImageREST(int id, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(apiKey))
            {
                return Json(new { status = false, message = "Không thể truy cập" });
            }

            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {

                var fileInfo = new FileInfo(Server.MapPath("~" + file.Path));
                if (fileInfo.Exists) fileInfo.Delete();

                Services.ContentManager.Remove(file.ContentItem);
                _userPropertyFileRepository.Delete(file.Record);

                return Json(new { id, status = true, message = "Xóa thành công" });
            }
            return Json(new { id, status = false, message = "Không tìm thấy dữ liệu" });
        }

        #endregion

        #region AjaxPublishPropertyImage

        [HttpPost]
        public ActionResult AjaxPublishPropertyImage(int id, bool isPublished)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                file.Published = isPublished;

                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return Json(new { id, success = true, isPublished = file.Published, message = "Cập nhật thành công" });
            }

            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        [HttpPost]
        public ActionResult AjaxPublishPropertyImageREST(int id, bool isPublished, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(apiKey))
            {
                return Json(new { status = false, message = "Không thể truy cập" });
            }

            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                file.Published = isPublished;

                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return Json(new { id, status = true, isPublished = file.Published, message = "Cập nhật thành công" });
            }

            return Json(new { id, status = false, message = "Không tìm thấy dữ liệu" });
        }

        #endregion

        #region AjaxSetAvatarPropertyImage

        [HttpPost]
        public ActionResult AjaxSetAvatarPropertyImage(int id, bool isAvatar)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                IEnumerable<PropertyFilePart> files = new List<PropertyFilePart>();

                if (file.Property != null)
                    files =
                        Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.PropertyPartRecord == file.Property)
                            .List();
                else if (file.Apartment != null)
                    files =
                        Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.LocationApartmentPartRecord == file.Apartment)
                            .List();

                foreach (PropertyFilePart item in files)
                {
                    item.IsAvatar = false;
                }
                file.IsAvatar = true;
                file.Published = true;

                //Clear cache UploadFile
                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return
                    Json(
                        new
                        {
                            id,
                            success = true,
                            isAvatar = file.IsAvatar,
                            isPublished = file.Published,
                            message = "Cập nhật thành công"
                        });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        [HttpPost]
        public ActionResult AjaxSetAvatarPropertyImageREST(int id, bool isAvatar, string apiKey)
        {
            if (!string.IsNullOrEmpty(apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(apiKey))
            {
                return Json(new { status = false, message = "Không thể truy cập" });
            }

            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                IEnumerable<PropertyFilePart> files = new List<PropertyFilePart>();

                if (file.Property != null)
                    files =
                        Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.PropertyPartRecord == file.Property)
                            .List();
                else if (file.Apartment != null)
                    files =
                        Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.LocationApartmentPartRecord == file.Apartment)
                            .List();

                foreach (PropertyFilePart item in files)
                {
                    item.IsAvatar = false;
                }
                file.IsAvatar = true;
                file.Published = true;

                //Clear cache UploadFile
                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return
                    Json(
                        new
                        {
                            id,
                            status = true,
                            isAvatar = file.IsAvatar,
                            isPublished = file.Published,
                            message = "Cập nhật thành công"
                        });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        #endregion

        #region Cross site

        //public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
        //{

        //    public override void OnActionExecuting(ActionExecutingContext filterContext)
        //    {
        //        try
        //        {
        //            var ctx = filterContext.RequestContext.HttpContext;
        //            var origin = ctx.Request.Headers["Origin"].ToString();
        //            var allowOrigin = "http://localhost:65290";

        //            if (ListAllowOrigin().Contains(origin))
        //            {
        //                allowOrigin = origin;
        //            }


        //            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", allowOrigin);

        //            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        //            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept,origin, x-requested-with");//,Content-Range,Content-Disposition,Content-Description
        //            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Max-Age", "1728000");

        //            //filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true");
        //            //filterContext.RequestContext.HttpContext.Response.End();

        //            base.OnActionExecuting(filterContext);
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //    }

        //}

        //public static List<string> ListAllowOrigin()
        //{
        //    return new List<string>()
        //    {
        //        "http://localhost:65290",
        //        "http://clbbds.com"
        //    };
        //}

        #endregion
    }
}