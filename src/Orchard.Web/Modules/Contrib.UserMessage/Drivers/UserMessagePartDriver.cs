using System.Collections.Generic;
using Contrib.UserMessage.Models;
using Contrib.UserMessage.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;

namespace Contrib.UserMessage.Drivers {
    public class UserMessagePartDriver : ContentPartDriver<UserMessagePart> {
        protected override string Prefix { get { return "UserMessage"; } }
        public Localizer T { get; set; }

        protected override DriverResult Display(UserMessagePart userMessagePart, string displayType, dynamic shapeHelper) {
            return ContentShape(shapeHelper.Parts_UserMessagePart(UserMessage: userMessagePart));
        }

        protected override DriverResult Editor(UserMessagePart userMessagePart, dynamic shapeHelper) {
            var results = new List<DriverResult> {
                ContentShape("Parts_UserMessage_UserMessagePart",
                                () => shapeHelper.EditorTemplate(TemplateName: "Parts/UserMessagePart", Model: userMessagePart, Prefix: Prefix))
            };

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(UserMessagePart userMessagePart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(userMessagePart, Prefix, null, null);
            return Editor(userMessagePart, shapeHelper);
        }
    }
}