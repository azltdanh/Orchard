using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Contrib.OnlineUsers.Models;

namespace Contrib.OnlineUsers.Drivers
{
    public class MembershipDriver:ContentPartDriver<MembershipPart>
    {
        protected override DriverResult Display(MembershipPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_Membership", () => shapeHelper.Parts_Membership(
                ContentPart: part,
                LastActive: part.LastActive
                ));
        }

        //GET
        protected override DriverResult Editor(MembershipPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Membership_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/Membership",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(MembershipPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);            
            return Editor(part, shapeHelper);
        }
    }
}