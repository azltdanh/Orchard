
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;
namespace RealEstateForum.Service.Drivers
{
    public class AdsPriceConfigPartDriver : ContentPartDriver<AdsPriceConfigPart>
    {
        //private readonly INotifier _notifier;
        private const string TemplateName = "Parts/AdsPriceConfigPart";

        public Localizer T { get; set; }

        public AdsPriceConfigPartDriver(IOrchardServices services)
        {
            //_notifier = notifier;
            Services = services;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(AdsPriceConfigPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsPriceConfigPart",
                () => shapeHelper.Parts_AdsPriceConfigPart(
                    part: part));
        }

        //GET
        protected override DriverResult Editor(AdsPriceConfigPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_AdsPriceConfigPart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(AdsPriceConfigPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}