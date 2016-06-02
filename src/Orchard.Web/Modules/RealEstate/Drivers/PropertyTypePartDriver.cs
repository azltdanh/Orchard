using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyTypePartDriver : ContentPartDriver<PropertyTypePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyTypePart";

        public Localizer T { get; set; }

        public PropertyTypePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyTypePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTypePart",
                () => shapeHelper.Parts_PropertyTypePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyTypePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTypePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyTypePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyTypePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyTypePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}