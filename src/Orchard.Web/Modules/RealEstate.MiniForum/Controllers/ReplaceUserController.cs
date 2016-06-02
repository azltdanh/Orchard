using System.Web.Mvc;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.Security;
using Orchard.Settings;
using RealEstate.Services;
using RealEstateForum.Service.Services;

namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class ReplaceUserController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly IUserGroupService _userGroupService;
        private readonly IPostAdminService _postService;

        public ReplaceUserController(IOrchardServices services, ISiteService siteService, IUserGroupService userGroupService, IPostAdminService postService)
        {
            _siteService = siteService;
            _userGroupService = userGroupService;
            _postService = postService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Services = services;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public ActionResult Index()//
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền Admin")))
                return new HttpUnauthorizedResult();

            
            return View();
        }
        [HttpPost]
        public ActionResult Index(FormCollection frm)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Không có quyền Admin")))
                return new HttpUnauthorizedResult();

            int userTo = 0;
            int userFrom = 0;

            if (!int.TryParse(frm["UserFrom"], out userFrom))
                AddModelError("UserFromNotIsNumber ", T("UserId phải là 1 số"));

            if (!int.TryParse(frm["UserTo"], out userTo))
                AddModelError("UserToNotIsNumber ", T("UserId phải là 1 số"));

            var tUserFrom = _userGroupService.GetUser(userFrom);
            var tUserTo = _userGroupService.GetUser(userTo);

            if (tUserFrom == null || tUserTo == null)
                AddModelError("UserNotExist", T("User này không tồn tại, vui lòng kiểm tra lại."));

            if(ModelState.IsValid)
            {
                _postService.ReplaceUserForumPost(userFrom, userTo);
            }

            return View();
        }

        #region UpdateModel
        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
    }
}