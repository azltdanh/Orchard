using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationWardPartDriver : ContentPartDriver<LocationWardPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationWardPart";

        public Localizer T { get; set; }

        public LocationWardPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(LocationWardPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationWardPart",
                () => shapeHelper.Parts_LocationWardPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(LocationWardPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationWardPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationWardPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during LocationWardPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}