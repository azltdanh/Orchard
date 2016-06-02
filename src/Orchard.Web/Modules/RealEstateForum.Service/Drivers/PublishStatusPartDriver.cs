using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Drivers
{
    public class PublishStatusPartDriver : ContentPartDriver<PublishStatusPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PublishStatusPart";

        public Localizer T { get; set; }

        public PublishStatusPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PublishStatusPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PublishStatusPart",
                () => shapeHelper.Parts_PublishStatusPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PublishStatusPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PublishStatusPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PublishStatusPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}