using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using RealEstate.UserControlPanel.Models;
using Orchard.UI.Notify;
using Orchard.Localization;
using Orchard.ContentManagement;
using JetBrains.Annotations;

namespace RealEstate.UserControlPanel.Drivers
{
    public class UserProfileInfomationDriver : ContentPartDriver<UserProfileInfomationPart>
    {
        protected override DriverResult Display(UserProfileInfomationPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_EditProfileInfomation", () => shapeHelper.Parts_UserPersonalInformation(
                ContentPart: part,
                Address: part.Address,
                Phone: part.Phone,
                CompanyName: part.CompanyName,
                DateOfBirth: part.DateOfBirth,
                Website: part.Website));
        }

        //GET
        protected override DriverResult Editor(UserProfileInfomationPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_EditProfileInfomation_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/EditProfileInfomation",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(UserProfileInfomationPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}