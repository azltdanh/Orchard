using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationApartmentRelationPartDriver : ContentPartDriver<LocationApartmentRelationPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationApartmentRelationPart";

        public Localizer T { get; set; }

        public LocationApartmentRelationPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(LocationApartmentRelationPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationApartmentRelationPart",
                () => shapeHelper.Parts_LocationApartmentRelationPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(LocationApartmentRelationPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationApartmentRelationPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationApartmentRelationPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                //_notifier.Information(T("LocationApartmentRelationPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during LocationApartmentRelationPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}