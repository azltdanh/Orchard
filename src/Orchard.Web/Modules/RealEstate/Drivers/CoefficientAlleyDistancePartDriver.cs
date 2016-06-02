using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class CoefficientAlleyDistancePartDriver : ContentPartDriver<CoefficientAlleyDistancePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CoefficientAlleyDistancePart";

        public Localizer T { get; set; }

        public CoefficientAlleyDistancePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CoefficientAlleyDistancePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientAlleyDistancePart",
                () => shapeHelper.Parts_CoefficientAlleyDistancePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CoefficientAlleyDistancePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientAlleyDistancePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CoefficientAlleyDistancePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CoefficientAlleyDistancePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CoefficientAlleyDistancePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}