using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class LocationDistrictPartDriver : ContentPartDriver<LocationDistrictPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/LocationDistrictPart";

        public Localizer T { get; set; }

        public LocationDistrictPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(LocationDistrictPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationDistrictPart",
                () => shapeHelper.Parts_LocationDistrictPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(LocationDistrictPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_LocationDistrictPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(LocationDistrictPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during LocationDistrictPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}