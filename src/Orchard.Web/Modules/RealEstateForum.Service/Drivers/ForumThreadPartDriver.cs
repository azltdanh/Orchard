using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Drivers
{
    public class ForumThreadPartDriver : ContentPartDriver<ForumThreadPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ForumThreadPart";

        public Localizer T { get; set; }

        public ForumThreadPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ForumThreadPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumThreadPart",
                () => shapeHelper.Parts_ForumThreadPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ForumThreadPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumThreadPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ForumThreadPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}