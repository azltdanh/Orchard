using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class LocationApartmentBlockPartDriver : ContentPartDriver<LocationApartmentBlockPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationApartmentBlockPart";

        public Localizer T { get; set; }

        public LocationApartmentBlockPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        //protected override DriverResult Display(LocationApartmentBlockPart part, string displayType, dynamic shapeHelper)
        //{
        //    return ContentShape("Parts_LocationApartmentBlockPart",
        //        () => shapeHelper.Parts_LocationApartmentPart(ContentItem: part.ContentItem));
        //}

        protected override DriverResult Editor(LocationApartmentBlockPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationApartmentBlockPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationApartmentBlockPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during LocationApartmentBlockPart update!"));
            }
            return Editor(part, shapeHelper);
        }
    }
}
