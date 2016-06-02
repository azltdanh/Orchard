using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace RealEstate.Estimation
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T("RealEstate Settings"),
                menu => menu.Add(T("Estimation Log"), "40", item => item.Action("Index", "Estimation", new { area = "RealEstate.Estimation" })
                    .Permission(StandardPermissions.SiteOwner)));
        }
    }
}
