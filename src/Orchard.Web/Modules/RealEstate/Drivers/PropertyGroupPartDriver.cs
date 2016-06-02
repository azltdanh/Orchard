using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{

    public class PropertyGroupPartDriver : ContentPartDriver<PropertyGroupPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyGroupPart";

        public Localizer T { get; set; }

        public PropertyGroupPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyGroupPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyGroupPart",
                () => shapeHelper.Parts_ContactPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyGroupPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyGroupPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyGroupPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyGroupPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyGroupPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}