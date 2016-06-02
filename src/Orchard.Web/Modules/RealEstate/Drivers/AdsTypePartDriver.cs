using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class AdsTypePartDriver : ContentPartDriver<AdsTypePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/AdsTypePart";

        public Localizer T { get; set; }

        public AdsTypePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(AdsTypePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsTypePart",
                () => shapeHelper.Parts_AdsTypePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(AdsTypePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsTypePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AdsTypePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("AdsTypePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during AdsTypePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}