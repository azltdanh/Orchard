using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class AdsPaymentHistoryPartDriver : ContentPartDriver<AdsPaymentHistoryPart>
    {
        private const string TemplateName = "Parts/AdsPaymentHistoryPart";

        public Localizer T { get; set; }

        public AdsPaymentHistoryPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(AdsPaymentHistoryPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsPaymentHistoryPart",
                () => shapeHelper.Parts_AdsPaymentHistoryPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(AdsPaymentHistoryPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsPaymentHistoryPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AdsPaymentHistoryPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
