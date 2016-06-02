using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class PropertySettingPartDriver : ContentPartDriver<PropertySettingPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertySettingPart";

        public Localizer T { get; set; }

        public PropertySettingPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertySettingPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertySettingPart",
                () => shapeHelper.Parts_PropertySettingPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertySettingPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertySettingPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertySettingPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertySettingPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertySettingPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}