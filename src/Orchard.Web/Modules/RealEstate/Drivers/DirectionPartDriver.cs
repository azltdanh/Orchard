using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class DirectionPartDriver : ContentPartDriver<DirectionPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/DirectionPart";

        public Localizer T { get; set; }

        public DirectionPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(DirectionPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_DirectionPart",
                () => shapeHelper.Parts_DirectionPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(DirectionPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_DirectionPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(DirectionPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("DirectionPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during DirectionPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}