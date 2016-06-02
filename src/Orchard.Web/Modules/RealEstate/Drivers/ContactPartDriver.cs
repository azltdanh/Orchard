using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class ContactPartDriver : ContentPartDriver<ContactPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ContactPart";

        public Localizer T { get; set; }

        public ContactPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ContactPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContactPart",
                () => shapeHelper.Parts_ContactPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ContactPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ContactPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ContactPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("ContactPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during ContactPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}