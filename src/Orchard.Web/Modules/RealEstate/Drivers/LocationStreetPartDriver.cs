using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationStreetPartDriver : ContentPartDriver<LocationStreetPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationStreetPart";

        public Localizer T { get; set; }

        public LocationStreetPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(LocationStreetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationStreetPart",
                () => shapeHelper.Parts_LocationStreetPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(LocationStreetPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationStreetPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationStreetPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during LocationStreetPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}