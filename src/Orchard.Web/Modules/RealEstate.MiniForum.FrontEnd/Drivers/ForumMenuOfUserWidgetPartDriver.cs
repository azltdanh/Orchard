using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumMenuOfUserWidgetPartDriver : ContentPartDriver<ForumMenuOfUserWidgetPart>
    {
        protected override DriverResult Display(ForumMenuOfUserWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumMenuOfUserWidget",
                () =>
                {
                    var shape = shapeHelper.Parts_ForumMenuOfUserWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}