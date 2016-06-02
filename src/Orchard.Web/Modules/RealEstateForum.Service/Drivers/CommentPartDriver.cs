using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Drivers
{
    public class CommentPartDriver : ContentPartDriver<CommentForumPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CommentForumPart";

        public Localizer T { get; set; }

        public CommentPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommentForumPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CommentForumPart",
                () => shapeHelper.Parts_CommentForumPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CommentForumPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CommentForumPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CommentForumPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}