using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CustomerStatusPartDriver : ContentPartDriver<CustomerStatusPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CustomerStatusPart";

        public Localizer T { get; set; }

        public CustomerStatusPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CustomerStatusPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerStatusPart",
                () => shapeHelper.Parts_CustomerStatusPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CustomerStatusPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerStatusPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CustomerStatusPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CustomerStatusPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CustomerStatusPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}