using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.Helpers;

namespace RealEstate.Admin.Controllers
{
    public class AsyncUploadController : Controller, IUpdateModel
    {
        //private IFileStore _fileStore = new DiskFileStore();

        private readonly IUserGroupService _groupService;

        public AsyncUploadController(
            IOrchardServices services,
            IUserGroupService groupService,
            IShapeFactory shapeFactory)
        {
            Services = services;

            _groupService = groupService;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
        
    }
}