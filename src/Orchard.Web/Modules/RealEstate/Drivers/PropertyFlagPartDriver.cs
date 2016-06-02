using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyFlagPartDriver : ContentPartDriver<PropertyFlagPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyFlagPart";

        public Localizer T { get; set; }

        public PropertyFlagPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyFlagPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyFlagPart",
                () => shapeHelper.Parts_PropertyFlagPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyFlagPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyFlagPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyFlagPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyFlagPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyFlagPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}