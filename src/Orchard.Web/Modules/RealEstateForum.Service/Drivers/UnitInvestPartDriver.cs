using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Drivers
{
    public class UnitInvestPartDriver : ContentPartDriver<UnitInvestPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/UnitInvestPart";

        public Localizer T { get; set; }

        public UnitInvestPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(UnitInvestPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UnitInvestPart",
                () => shapeHelper.Parts_UnitInvestPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(UnitInvestPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_UnitInvestPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(UnitInvestPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}