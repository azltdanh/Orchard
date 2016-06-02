using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CoefficientLengthPartDriver : ContentPartDriver<CoefficientLengthPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CoefficientLengthPart";

        public Localizer T { get; set; }

        public CoefficientLengthPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CoefficientLengthPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientLengthPart",
                () => shapeHelper.Parts_CoefficientLengthPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CoefficientLengthPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CoefficientLengthPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CoefficientLengthPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CoefficientLengthPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CoefficientLengthPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}