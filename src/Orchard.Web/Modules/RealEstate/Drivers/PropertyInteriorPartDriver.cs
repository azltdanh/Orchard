using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyInteriorPartDriver : ContentPartDriver<PropertyInteriorPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyInteriorPart";

        public Localizer T { get; set; }

        public PropertyInteriorPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyInteriorPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyInteriorPart",
                () => shapeHelper.Parts_PropertyInteriorPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyInteriorPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyInteriorPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyInteriorPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyInteriorPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyInteriorPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}