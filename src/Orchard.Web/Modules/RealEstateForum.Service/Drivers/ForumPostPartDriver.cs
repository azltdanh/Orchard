using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Drivers
{
    public class ForumPostPartDriver : ContentPartDriver<ForumPostPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ForumPostPart";

        public Localizer T { get; set; }

        public ForumPostPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ForumPostPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumPostPart",
                () => shapeHelper.Parts_ForumPostPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ForumPostPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumPostPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ForumPostPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}