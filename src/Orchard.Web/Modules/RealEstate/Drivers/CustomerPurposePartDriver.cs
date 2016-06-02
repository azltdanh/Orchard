using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CustomerPurposePartDriver : ContentPartDriver<CustomerPurposePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CustomerPurposePart";

        public Localizer T { get; set; }

        public CustomerPurposePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CustomerPurposePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerPurposePart",
                () => shapeHelper.Parts_CustomerPurposePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CustomerPurposePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerPurposePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CustomerPurposePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("CustomerPurposePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CustomerPurposePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}