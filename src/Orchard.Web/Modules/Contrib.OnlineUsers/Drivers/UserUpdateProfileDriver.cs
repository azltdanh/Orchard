using Contrib.OnlineUsers.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Contrib.OnlineUsers.Drivers
{
    public class UserUpdateProfileDriver : ContentPartDriver<UserUpdateProfilePart>
    {
        protected override DriverResult Display(UserUpdateProfilePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserUpdateProfile", () => shapeHelper.Parts_UserUpdateProfile(
                ContentPart: part,
                Avatar: part.Avatar,
                FirstName: part.FirstName,
                LastName: part.LastName,
                DisplayName: part.DisplayName,
                Gender: part.Gender,
                DateOfBirth: part.DateOfBirth,
                Address: part.Address,
                Phone: part.Phone,
                Email: part.Email,
                Job: part.Job,
                Level: part.Level,
                Website: part.Website,
                Note: part.Note,
                Signature: part.Signature,
                IsSignature: part.IsSignature,
                NickName: part.NickName,
                IsSelling: part.IsSelling,
                IsLeasing: part.IsLeasing,
                PublishPhone: part.PublishPhone,
                PublishAddress: part.PublishAddress,
                PublishJob: part.PublishJob,
                PublishLevel: part.PublishLevel
                ));
        }

        //GET
        protected override DriverResult Editor(UserUpdateProfilePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserUpdateProfile_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/UserUpdateProfile",
                    Model: part,
                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(UserUpdateProfilePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}