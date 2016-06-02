using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.ContentManagement;
using Orchard.UI.Admin;

using RealEstate.MiniForum.ViewModels;
using RealEstateForum.Service.Services;
using RealEstate.Services;
using Orchard.Users.Models;


namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class ImportDataController : Controller
    {
        private readonly IImportDataService _importData;
        private readonly IHostNameService _hostNameService;
        private readonly IUserGroupService _groupService;

        public ImportDataController(
            IOrchardServices services,
            IHostNameService hostNameService,
            IUserGroupService groupService,
            IImportDataService importData
        )
        {
            _hostNameService = hostNameService;
            _importData = importData;
            _groupService = groupService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Services = services;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Import()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền Admin")))
                return new HttpUnauthorizedResult();

            return View(new ImportDataViewModel());
        }
        [HttpPost]
        public ActionResult Import(ImportDataViewModel viewModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền Admin")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            if (ModelState.IsValid)
            {
                _importData.ImportDataByTopicId(viewModel.OldTopicId, viewModel.NewTopicId, hostname);
                return View();
            }
            else
            {
                return View(viewModel);
            }
        }

        #region Imort CssImage

        public ActionResult ImportCssImageThread()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền Admin")))
                return new HttpUnauthorizedResult();

            return View("Import",new ImportDataViewModel());
        }
        [HttpPost]
        public ActionResult ImportCssImageThread(ImportDataViewModel viewModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền Admin")))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                _importData.ImportDataCssImageForThread(viewModel.OldTopicId, viewModel.NewTopicId);// Chuyên mục cũ => chuyên mục mới
                return View("Import");
            }
            else
            {
                return View("Import",viewModel);
            }
        }
        #endregion
    }
}