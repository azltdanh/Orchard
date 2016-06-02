using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class UserProfileAgencyWidgetPartDriver : ContentPartDriver<UserProfileAgencyWidgetPart>
    {
        protected override DriverResult Display(UserProfileAgencyWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserProfileAgencyWidget", () =>
            {
                var shape = shapeHelper.Parts_UserProfileAgencyWidget();
                //shape.ContentPart = part;
                return shape;
            });
        }
    }
}
