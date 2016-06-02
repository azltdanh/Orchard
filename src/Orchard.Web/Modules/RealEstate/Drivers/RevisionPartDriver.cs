using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class RevisionPartDriver : ContentPartDriver<RevisionPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/RevisionPart";

        public Localizer T { get; set; }

        public RevisionPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(RevisionPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_RevisionPart",
                () => shapeHelper.Parts_RevisionPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(RevisionPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_RevisionPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(RevisionPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("RevisionPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during RevisionPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}