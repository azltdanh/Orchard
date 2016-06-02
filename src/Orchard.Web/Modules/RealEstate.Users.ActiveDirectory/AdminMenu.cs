using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace Orchard.Packaging
{
	[OrchardFeature("RealEstate.Users.ActiveDirectory")]
	public class AdminMenu : INavigationProvider
	{
		public Localizer T { get; set; }

		public string MenuName
		{
			get { return "admin"; }
		}

		public void GetNavigation(NavigationBuilder builder)
		{
			builder
				.Add(T("Settings"), menu => menu
					.Add(T("Active Directory"), "1", item => 
						item.Action("Settings", "ActiveDirectory", new { area = "RealEstate.Users.ActiveDirectory" }).Permission(StandardPermissions.SiteOwner)));
		}
	}
}