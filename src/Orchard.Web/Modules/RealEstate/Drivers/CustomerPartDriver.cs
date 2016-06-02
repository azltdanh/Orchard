using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class CustomerPartDriver : ContentPartDriver<CustomerPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/CustomerPart";

        public Localizer T { get; set; }

        public CustomerPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(CustomerPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerPart",
                () => shapeHelper.Parts_CustomerPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(CustomerPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_CustomerPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(CustomerPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                //_notifier.Information(T("CustomerPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during CustomerPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}