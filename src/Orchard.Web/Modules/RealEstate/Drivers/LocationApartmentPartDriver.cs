using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationApartmentPartDriver : ContentPartDriver<LocationApartmentPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationApartmentPart";

        public Localizer T { get; set; }

        public LocationApartmentPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        //protected override DriverResult Display(LocationApartmentPart part, string displayType, dynamic shapeHelper)
        //{
        //    return ContentShape("Parts_LocationApartmentPart",
        //        () => shapeHelper.Parts_LocationApartmentPart(ContentItem: part.ContentItem));
        //}

        protected override DriverResult Editor(LocationApartmentPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationApartmentPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationApartmentPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during LocationApartmentPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}