using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyLegalStatusPartDriver : ContentPartDriver<PropertyLegalStatusPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyLegalStatusPart";

        public Localizer T { get; set; }

        public PropertyLegalStatusPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyLegalStatusPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyLegalStatusPart",
                () => shapeHelper.Parts_PropertyLegalStatusPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyLegalStatusPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyLegalStatusPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyLegalStatusPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyLegalStatusPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyLegalStatusPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}