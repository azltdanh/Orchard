

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class VideoTypePartDriver : ContentPartDriver<VideoTypePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/VideoTypePart";

        public Localizer T { get; set; }

        public VideoTypePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(VideoTypePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_VideoTypePart",
                () => shapeHelper.Parts_AdsTypePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(VideoTypePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_VideoTypePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(VideoTypePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
