using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class AddressPartDriver : ContentPartDriver<AddressPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/AddressPart";

        public Localizer T { get; set; }

        public AddressPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(AddressPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AddressPart",
                () => shapeHelper.Parts_AddressPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(AddressPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_AddressPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(AddressPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("AddressPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during AddressPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}