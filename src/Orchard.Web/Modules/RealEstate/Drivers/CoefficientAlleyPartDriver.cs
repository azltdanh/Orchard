using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CoefficientAlleyPartDriver : ContentPartDriver<CoefficientAlleyPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CoefficientAlleyPart";

        public Localizer T { get; set; }

        public CoefficientAlleyPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CoefficientAlleyPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientAlleyPart",
                () => shapeHelper.Parts_CoefficientAlleyPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CoefficientAlleyPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientAlleyPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CoefficientAlleyPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CoefficientAlleyPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CoefficientAlleyPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}