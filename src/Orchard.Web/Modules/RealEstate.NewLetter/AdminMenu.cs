    using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;

namespace RealEstate.NewLetter
{
    public class AdminMenu : INavigationProvider
    {
        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.AddImageSet("RealEstate.NewLetter")
                .Add(T("Hộp thư đến"), "0.25",
                    menu => menu.Action("Inbox", "MessageInboxAdmin", new { area = "RealEstate.NewLetter" }).Permission(Permissions.ContactInobxNewLetter)
                        .Add(T("Hộp thư đến"), "10", item => item.Action("Inbox", "MessageInboxAdmin", new { area = "RealEstate.NewLetter" }).Permission(Permissions.ContactInobxNewLetter).LocalNav())
                        .Add(T("Tin đã gửi"), "10", item => item.Action("SendInbox", "MessageInboxAdmin", new { area = "RealEstate.NewLetter" }).Permission(Permissions.ContactInobxNewLetter).LocalNav())
                        .Add(T("Soạn tin nhắn mới"), "10", item => item.Action("CreateMessage", "MessageInboxAdmin", new { area = "RealEstate.NewLetter" }).Permission(Permissions.ContactInobxNewLetter).LocalNav())
                        .Add(T("Hộp thư liên hệ"), "10", item => item.Action("Index", "ContactInboxAdmin", new { area = "RealEstate.NewLetter" }).Permission(Permissions.ContactInobxNewLetter).LocalNav())
                )
                .Add(T("Gửi BĐS cho khách"), "0.82",
                    menu => menu
                        .Add(T("NewLetterCustomer"), "10", item => item.Action("index", "ListCustomer", new { area = "RealEstate.NewLetter" }).Permission(Permissions.ManageNewsletter))
                );
        }
    }
}
