using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CustomerFeedbackPartDriver : ContentPartDriver<CustomerFeedbackPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CustomerFeedbackPart";

        public Localizer T { get; set; }

        public CustomerFeedbackPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CustomerFeedbackPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerFeedbackPart",
                () => shapeHelper.Parts_CustomerFeedbackPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CustomerFeedbackPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerFeedbackPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CustomerFeedbackPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CustomerFeedbackPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CustomerFeedbackPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}