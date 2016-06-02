using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PaymentUnitPartDriver : ContentPartDriver<PaymentUnitPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PaymentUnitPart";

        public Localizer T { get; set; }

        public PaymentUnitPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PaymentUnitPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaymentUnitPart",
                () => shapeHelper.Parts_PaymentUnitPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PaymentUnitPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaymentUnitPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PaymentUnitPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PaymentUnitPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PaymentUnitPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}