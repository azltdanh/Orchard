using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PaymentExchangePartDriver : ContentPartDriver<PaymentExchangePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PaymentExchangePart";

        public Localizer T { get; set; }

        public PaymentExchangePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PaymentExchangePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaymentExchangePart",
                () => shapeHelper.Parts_PaymentExchangePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PaymentExchangePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PaymentExchangePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PaymentExchangePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PaymentExchangePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PaymentExchangePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}