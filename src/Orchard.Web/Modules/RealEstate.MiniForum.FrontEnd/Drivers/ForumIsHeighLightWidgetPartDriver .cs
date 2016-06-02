using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsHeighLightWidgetPartDriver : ContentPartDriver<ForumIsHeighLightWidgetPart>
    {
        protected override DriverResult Display(ForumIsHeighLightWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsHeighLightWidget", () =>
            {
                var shape = shapeHelper.Parts_ForumIsHeighLightWidget();
                //shape.ContentPart = part;
                return shape;
            });
        }
    }
}