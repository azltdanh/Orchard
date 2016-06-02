

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class VideoManagePartDriver : ContentPartDriver<VideoManagePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/VideoManagePart";

        public Localizer T { get; set; }

        public VideoManagePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(VideoManagePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_VideoManagePart",
                () => shapeHelper.Parts_AdsTypePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(VideoManagePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_VideoManagePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(VideoManagePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
