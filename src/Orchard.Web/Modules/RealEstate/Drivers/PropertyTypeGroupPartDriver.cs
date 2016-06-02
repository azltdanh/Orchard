using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyTypeGroupPartDriver : ContentPartDriver<PropertyTypeGroupPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyTypeGroupPart";

        public Localizer T { get; set; }

        public PropertyTypeGroupPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyTypeGroupPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTypeGroupPart",
                () => shapeHelper.Parts_PropertyTypeGroupPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyTypeGroupPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTypeGroupPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyTypeGroupPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyTypeGroupPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyTypeGroupPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}