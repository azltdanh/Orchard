using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyAdvantagePartDriver : ContentPartDriver<PropertyAdvantagePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyAdvantagePart";

        public Localizer T { get; set; }

        public PropertyAdvantagePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyAdvantagePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyAdvantagePart",
                () => shapeHelper.Parts_PropertyAdvantagePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyAdvantagePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyAdvantagePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyAdvantagePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                //_notifier.Information(T("PropertyAdvantagePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyAdvantagePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}