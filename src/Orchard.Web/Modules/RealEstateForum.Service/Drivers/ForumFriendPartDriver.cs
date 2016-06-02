using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Orchard.ContentManagement.Drivers;
using RealEstateForum.Service.Models;
using Orchard.UI.Notify;
using Orchard.Localization;
using Orchard.ContentManagement;

namespace RealEstateForum.Service.Drivers
{
    public class ForumFriendPartDriver : ContentPartDriver<ForumFriendPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ForumFriendPart";

        public Localizer T { get; set; }

        public ForumFriendPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ForumFriendPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumFriendPart",
                () => shapeHelper.Parts_ForumFriendPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ForumFriendPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumFriendPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ForumFriendPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}