using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Drivers
{
    public class AliasesMetaPartDriver : ContentPartDriver<AliasesMetaPart>
    {
        private const string TemplateName = "Parts/AliasesMetaPart";
        private readonly INotifier _notifier;

        public AliasesMetaPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(AliasesMetaPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AliasesMetaWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_AliasesMetaWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }

        protected override DriverResult Editor(AliasesMetaPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_AliasesMetaPart",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AliasesMetaPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}