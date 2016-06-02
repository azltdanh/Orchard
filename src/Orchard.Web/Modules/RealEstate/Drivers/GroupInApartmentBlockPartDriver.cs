using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class GroupInApartmentBlockPartDriver : ContentPartDriver<GroupInApartmentBlockPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/GroupInApartmentBlockPart";

        public Localizer T { get; set; }

        public GroupInApartmentBlockPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        //protected override DriverResult Display(GroupInApartmentBlockPart part, string displayType, dynamic shapeHelper)
        //{
        //    return ContentShape("Parts_GroupInApartmentBlockPart",
        //        () => shapeHelper.Parts_LocationApartmentPart(ContentItem: part.ContentItem));
        //}

        protected override DriverResult Editor(GroupInApartmentBlockPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_GroupInApartmentBlockPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(GroupInApartmentBlockPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
            }
            else
            {
                _notifier.Error(T("Error during GroupInApartmentBlockPart update!"));
            }
            return Editor(part, shapeHelper);
        }
    }
}
