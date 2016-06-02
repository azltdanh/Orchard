using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyStatusPartDriver : ContentPartDriver<PropertyStatusPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyStatusPart";

        public Localizer T { get; set; }

        public PropertyStatusPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyStatusPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyStatusPart",
                () => shapeHelper.Parts_PropertyStatusPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyStatusPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyStatusPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyStatusPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyStatusPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyStatusPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}