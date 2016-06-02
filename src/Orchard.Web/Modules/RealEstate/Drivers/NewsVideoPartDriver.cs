using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class NewsVideoPartDriver : ContentPartDriver<NewsVideoPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/NewsVideoPart";

        public Localizer T { get; set; }

        public NewsVideoPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(NewsVideoPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_NewsVideoPart",
                () => shapeHelper.Parts_AdsTypePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(NewsVideoPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_NewsVideoPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(NewsVideoPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
