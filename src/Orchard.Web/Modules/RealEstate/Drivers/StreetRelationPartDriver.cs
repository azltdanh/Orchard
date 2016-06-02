using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class StreetRelationPartDriver : ContentPartDriver<StreetRelationPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/StreetRelationPart";

        public Localizer T { get; set; }

        public StreetRelationPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(StreetRelationPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_StreetRelationPart",
                () => shapeHelper.Parts_StreetRelationPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(StreetRelationPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_StreetRelationPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(StreetRelationPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                //_notifier.Information(T("StreetRelationPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during StreetRelationPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}