using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using RealEstate.Services;
using RealEstateForum.Service;

namespace RealEstate.MiniForum
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IHostNameService _hostNameService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public string MenuName { get { return "admin"; } }

        public AdminMenu(
            IHostNameService hostNameService,
            IUserGroupService groupService,
            IPropertyService propertyService,
            IOrchardServices services
            )
        {
            _hostNameService = hostNameService;
            _groupService = groupService;
            _propertyService = propertyService;
            Services = services;
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("RealEstate.MiniForum")
                .Add(T("Forum-Management"), "0.61",
                    menu => menu
                            .Add(T("QL Chuyên mục"), "10", item => item.Action("Index", "ThreadAdminForum", new { area = "RealEstate.MiniForum" }).Permission(MiniForumPermissions.ManagementMiniForum))
                            .Add(T("QL Chuyên mục"), "10", item => item.Action("Index", "ThreadAdminForum", new { area = "RealEstate.MiniForum" }).Permission(MiniForumPermissions.ManagementMiniForum))
                            .Add(T("QL Bài viết"), "10", item => item.Action("Index", "PostAdminForum", new { area = "RealEstate.MiniForum" }).Permission(MiniForumPermissions.ManagementMiniUpdatePostForum))
                            .Add(T("Bài viết chờ duyệt"), "10", item => item.Action("Index", "PostAdminForum", new { area = "RealEstate.MiniForum", PostStatusId = 1026356 }).Permission(MiniForumPermissions.ManagementMiniUpdatePostForum))
                            .Add(T("Đăng bài viết"), "10", item => item.Action("Create", "PostAdminForum", new { area = "RealEstate.MiniForum" }).Permission(MiniForumPermissions.ManagementMiniUpdatePostForum))
                        )
            .Add(T("RealEstate Settings"), "0.40",
                    menu => menu
                    .Add(T("Thay đổi Users"), "34.0", item => item.Action("Index", "ReplaceUser", new { area = "RealEstate.MiniForum" }).Permission(StandardPermissions.SiteOwner))
                    .Add(T("Danh sách users online"), "35.0", item => item.Action("GetListOnlineUsers", "User", new { area = "RealEstate.MiniForum" }).Permission(Permissions.ManageListUserOnline))
                    .Add(T("Hỗ trợ online"), "36.0", item => item.Action("index", "SupportOnlineConfig", new { area = "RealEstate.MiniForum" }).Permission(StandardPermissions.SiteOwner))
                    .Add(T("Bảng giá QC Banner"), "37.0", item => item.Action("index", "AdsPriceConfig", new { area = "RealEstate.MiniForum" }).Permission(StandardPermissions.SiteOwner))
                );
        }
    }
}