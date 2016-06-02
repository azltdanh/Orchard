using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class UserGroupPartDriver : ContentPartDriver<UserGroupPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/Part";

        public Localizer T { get; set; }

        public UserGroupPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(UserGroupPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserGroupPart",
                () => shapeHelper.Parts_UserGroupPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(UserGroupPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserGroupPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(UserGroupPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                //_notifier.Information(T("UserGroupPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during UserGroupPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}