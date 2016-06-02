using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateForum.Service.Drivers
{
    public class ForumPostStatusPartDriver: ContentPartDriver<ForumPostStatusPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ForumPostStatusPart";

        public Localizer T { get; set; }

        public ForumPostStatusPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ForumPostStatusPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumPostStatusPart",
                () => shapeHelper.Parts_ForumPostStatusPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ForumPostStatusPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumPostStatusPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ForumPostStatusPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}