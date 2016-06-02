using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PaymentMethodPartDriver : ContentPartDriver<PaymentMethodPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PaymentMethodPart";

        public Localizer T { get; set; }

        public PaymentMethodPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PaymentMethodPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaymentMethodPart",
                () => shapeHelper.Parts_PaymentMethodPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PaymentMethodPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaymentMethodPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PaymentMethodPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PaymentMethodPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PaymentMethodPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}