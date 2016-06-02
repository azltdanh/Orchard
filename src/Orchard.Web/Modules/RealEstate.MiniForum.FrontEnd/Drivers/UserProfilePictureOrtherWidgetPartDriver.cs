using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;
namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class UserProfilePictureOrtherWidgetPartDriver : ContentPartDriver<UserProfilePictureOrtherWidgetPart>
    {
        protected override DriverResult Display(UserProfilePictureOrtherWidgetPart part, string displayType, dynamic shapeHelper)
        {
            //var model = new UserUpdateProfileOptions();
            return ContentShape("Parts_UserProfilePictureOrtherWidget", () =>
            {
                var shape = shapeHelper.Parts_UserProfilePictureOrtherWidget();
                shape.ContentPart = part;
                return shape;
            });
        }
    }
}
