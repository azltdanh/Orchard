using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Drivers
{
    public class SupportOnlineConfigPartDriver : ContentPartDriver<SupportOnlineConfigPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/SupportOnlinePart";

        public Localizer T { get; set; }

        public SupportOnlineConfigPartDriver(IOrchardServices services, INotifier notifier)
        {
            _notifier = notifier;
            Services = services;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(SupportOnlineConfigPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_SupportOnlinePart",
                () => shapeHelper.Parts_SupportOnlinePart(
                    part: part));
        }

        //GET
        protected override DriverResult Editor(SupportOnlineConfigPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_SupportOnlinePart_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(SupportOnlineConfigPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}