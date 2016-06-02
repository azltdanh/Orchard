using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyTypeConstructionPartDriver : ContentPartDriver<PropertyTypeConstructionPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyTypeConstructionPart";

        public Localizer T { get; set; }

        public PropertyTypeConstructionPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyTypeConstructionPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTypeConstructionPart",
                () => shapeHelper.Parts_PropertyTypeConstructionPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyTypeConstructionPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTypeConstructionPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyTypeConstructionPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyTypeConstructionPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyTypeConstructionPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}