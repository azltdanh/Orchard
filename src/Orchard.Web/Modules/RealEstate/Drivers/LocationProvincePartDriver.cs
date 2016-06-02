using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationProvincePartDriver : ContentPartDriver<LocationProvincePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationProvincePart";

        public Localizer T { get; set; }

        public LocationProvincePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(LocationProvincePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationProvincePart",
                () => shapeHelper.Parts_LocationProvincePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(LocationProvincePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationProvincePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationProvincePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during LocationProvincePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}