using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Drivers
{
    public class CommentLikePartDriver : ContentPartDriver<CommentLikePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CommentLikePart";

        public Localizer T { get; set; }

        public CommentLikePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CommentLikePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CommentLikePart",
                () => shapeHelper.Parts_CommentLikePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CommentLikePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CommentLikePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CommentLikePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}