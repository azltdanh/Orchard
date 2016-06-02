using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.NewLetter.Models;


namespace RealEstate.NewLetter.Drivers
{
    public class MessageInboxPartDriver : ContentPartDriver<MessageInboxPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/MessageInboxPart";

        public Localizer T { get; set; }

        public MessageInboxPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(MessageInboxPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_MessageInboxPart",
                () => shapeHelper.Parts_MessageInboxPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(MessageInboxPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_MessageInboxPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(MessageInboxPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}