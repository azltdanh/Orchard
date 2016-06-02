using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CoefficientApartmentFloorThPartDriver : ContentPartDriver<CoefficientApartmentFloorThPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CoefficientApartmentFloorThPart";

        public Localizer T { get; set; }

        public CoefficientApartmentFloorThPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CoefficientApartmentFloorThPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientApartmentFloorThPart",
                () => shapeHelper.Parts_CoefficientApartmentFloorThPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CoefficientApartmentFloorThPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientApartmentFloorThPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CoefficientApartmentFloorThPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CoefficientApartmentFloorThPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CoefficientApartmentFloorThPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}