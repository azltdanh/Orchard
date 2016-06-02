using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using RealEstate.Models;
namespace RealEstate.Drivers
{
    public class AdsPaymentConfigPartDriver : ContentPartDriver<AdsPaymentConfigPart>
    {
        private const string TemplateName = "Parts/AdsPaymentConfigPart";

        public Localizer T { get; set; }

        public AdsPaymentConfigPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(AdsPaymentConfigPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsPaymentConfigPart",
                () => shapeHelper.Parts_AdsPaymentConfigPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(AdsPaymentConfigPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsPaymentConfigPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AdsPaymentConfigPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}
