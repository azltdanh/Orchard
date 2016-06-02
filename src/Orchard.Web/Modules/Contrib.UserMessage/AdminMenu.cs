using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Contrib.UserMessage {
    public class AdminMenu : INavigationProvider {
        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Messages"), "2.5", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            menu.Add(T("Received Messages"), "1.0", item => item.Action("Index", "Admin", new {area = "Contrib.UserMessage"}));
            menu.Add(T("Sent Messages"), "2.0", item => item.Action("SentMessages", "Admin", new { area = "Contrib.UserMessage" }));
        }
    }
}