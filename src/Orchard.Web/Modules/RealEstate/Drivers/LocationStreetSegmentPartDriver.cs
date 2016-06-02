using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationStreetSegmentPartDriver : ContentPartDriver<LocationStreetSegmentPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationStreetSegmentPart";

        public Localizer T { get; set; }

        public LocationStreetSegmentPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(LocationStreetSegmentPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationStreetSegmentPart",
                () => shapeHelper.Parts_LocationStreetSegmentPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(LocationStreetSegmentPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationStreetSegmentPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationStreetSegmentPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("LocationStreetSegmentPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during LocationStreetSegmentPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}