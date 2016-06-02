using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Drivers
{
    public class FrontEndSettingPartDriver : ContentPartDriver<FrontEndSettingPart>
    {
        private const string TemplateName = "Parts/FrontEndSettingPart";
        private readonly INotifier _notifier;

        public FrontEndSettingPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(FrontEndSettingPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_FrontEndSettingPart",
                () => shapeHelper.Parts_FrontEndSettingPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(FrontEndSettingPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_FrontEndSettingPart",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(FrontEndSettingPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("FrontEndSettingPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during FrontEndSettingPart update!"));
            }
            return Editor(part, shapeHelper);
        }
    }
}