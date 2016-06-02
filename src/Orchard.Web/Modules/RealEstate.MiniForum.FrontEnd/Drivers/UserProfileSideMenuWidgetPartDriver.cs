using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class UserProfileSideMenuWidgetPartDriver : ContentPartDriver<UserProfileSideMenuWidgetPart>
    {
        protected override DriverResult Display(UserProfileSideMenuWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserProfileSideMenuWidget", () =>
            {
                var shape = shapeHelper.Parts_UserProfileSideMenuWidget();
                shape.ContentPart = part;
                return shape;
            });
        }
    }
}
