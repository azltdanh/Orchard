using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsProjectWidgetPartDriver : ContentPartDriver<ForumIsProjectWidgetPart>
    {
        protected override DriverResult Display(ForumIsProjectWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsProjectWidget", () =>
            {
                var shape = shapeHelper.Parts_ForumIsProjectWidget();
                //shape.ContentPart = part;
                return shape;
            });
        }
    }
}