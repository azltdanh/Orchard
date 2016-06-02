using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyLocationPartDriver : ContentPartDriver<PropertyLocationPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyLocationPart";

        public Localizer T { get; set; }

        public PropertyLocationPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyLocationPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyLocationPart",
                () => shapeHelper.Parts_PropertyLocationPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyLocationPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyLocationPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyLocationPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyLocationPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyLocationPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}