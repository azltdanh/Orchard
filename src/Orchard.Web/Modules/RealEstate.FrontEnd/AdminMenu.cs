using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace RealEstate.FrontEnd
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("RealEstate.Frontend")

                .Add(T("RealEstate Settings"), "0.40",
                    menu => menu
                        .Add(T("FrontEnd Setting"), "32.0", item => item.Action("index", "FrontEndSetting", new { area = "RealEstate.Frontend" }).Permission(Permissions.ManagePropertySettings))
                        .Add(T("Alias Create"), "34.0", item => item.Action("AliasCollectionCreate", "AliasesMeta", new { area = "RealEstate.FrontEnd" }).Permission(StandardPermissions.SiteOwner))
                            .Add(T("Alias Property"), "10.0", item => item.Action("AliasCollectionCreate", "AliasesMeta", new { area = "RealEstate.FrontEnd" }).Permission(StandardPermissions.SiteOwner).LocalNav())
                            .Add(T("Alias Apartment"), "20.0", item => item.Action("AliasCollectionApartmentCreate", "AliasesMeta", new { area = "RealEstate.FrontEnd" }).Permission(StandardPermissions.SiteOwner).LocalNav())
                );
        }
    }
}
