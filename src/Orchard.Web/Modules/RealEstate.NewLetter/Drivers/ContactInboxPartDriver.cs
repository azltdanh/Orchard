using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.NewLetter.Models;

namespace RealEstate.NewLetter.Drivers
{
    public class ContactInboxPartDriver : ContentPartDriver<ContactInboxPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ContactInboxPart";

        public Localizer T { get; set; }

        public ContactInboxPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ContactInboxPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContactInboxPart",
                () => shapeHelper.Parts_ContactInboxPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ContactInboxPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContactInboxPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ContactInboxPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}