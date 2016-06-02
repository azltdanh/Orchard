using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class UserProfileSideMenuOrtherWidgetPartDriver : ContentPartDriver<UserProfileSideMenuOrtherWidgetPart>
    {
        protected override DriverResult Display(UserProfileSideMenuOrtherWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserProfileSideMenuOrtherWidget", () =>
            {
                var shape = shapeHelper.Parts_UserProfileSideMenuOrtherWidget();
                shape.ContentPart = part;
                return shape;
            });
        }
    }
}
