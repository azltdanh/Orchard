using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class UserActionPartDriver : ContentPartDriver<UserActionPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/UserActionPart";

        public Localizer T { get; set; }

        public UserActionPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(UserActionPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserActionPart",
                () => shapeHelper.Parts_UserActionPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(UserActionPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserActionPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(UserActionPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("UserActionPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during UserActionPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}