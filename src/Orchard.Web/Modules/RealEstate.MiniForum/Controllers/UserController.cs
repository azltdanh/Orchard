using System.Web.Mvc;

using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.Settings;
using RealEstate.Services;
using RealEstateForum.Service.Services;
using Contrib.OnlineUsers.ViewModels;

namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class UserController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly IUserForumService _userForumService;
        public UserController(IOrchardServices services, ISiteService siteService, IUserForumService userForumService, IShapeFactory shapeFactory)
        {
            Services = services;
            _siteService = siteService;
            _userForumService = userForumService;


            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }
        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        #region get list online users
        public ActionResult GetListOnlineUsers(PagerParameters pagerParameters)
        {
            //if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
            //    return new HttpUnauthorizedResult();
            if (!Services.Authorizer.Authorize(Permissions.ManageListUserOnline, T("Not authorized to list ManageListUserOnline")))
                return new HttpUnauthorizedResult();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var listOnline = _userForumService.GetListUserOnline();

            var pagerShape = Shape.Pager(pager).TotalItemCount(listOnline.Count());

            var results = listOnline
                .Slice(pager.GetStartIndex(), pager.PageSize);

            var viewModel = new OnlineUsersViewModel
            {
                OnlineUsers = _userForumService.BuildListUserOnlineEntry(results),
                TotalCount = listOnline.Count(),
                Pager = pagerShape,
            };

            return View(viewModel);
        }
        #endregion
    }
}