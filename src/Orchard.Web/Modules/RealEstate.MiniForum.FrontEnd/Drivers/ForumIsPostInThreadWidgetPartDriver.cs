using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsPostInThreadWidgetPartDriver : ContentPartDriver<ForumIsPostInThreadWidgetPart>
    {
        protected override DriverResult Display(ForumIsPostInThreadWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsPostInThreadWidget", () =>
            {
                var shape = shapeHelper.Parts_ForumIsPostInThreadWidget();
                //shape.ContentPart = part;
                return shape;
            });
        }
    }
}