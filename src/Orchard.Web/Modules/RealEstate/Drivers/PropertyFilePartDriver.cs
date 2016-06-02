using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class PropertyFilePartDriver : ContentPartDriver<PropertyFilePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/PropertyFilePart";

        public Localizer T { get; set; }

        public PropertyFilePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(PropertyFilePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyFilePart",
                () => shapeHelper.Parts_PropertyFilePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(PropertyFilePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyFilePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(PropertyFilePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("PropertyFilePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during PropertyFilePart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}