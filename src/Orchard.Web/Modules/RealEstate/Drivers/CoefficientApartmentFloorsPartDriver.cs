using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CoefficientApartmentFloorsPartDriver : ContentPartDriver<CoefficientApartmentFloorsPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CoefficientApartmentFloorsPart";

        public Localizer T { get; set; }

        public CoefficientApartmentFloorsPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CoefficientApartmentFloorsPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientApartmentFloorsPart",
                () => shapeHelper.Parts_CoefficientApartmentFloorsPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CoefficientApartmentFloorsPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientApartmentFloorsPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CoefficientApartmentFloorsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CoefficientApartmentFloorsPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CoefficientApartmentFloorsPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}