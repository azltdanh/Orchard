using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsMarketWidgetPartDriver : ContentPartDriver<ForumIsMarketWidgetPart>
    {
        protected override DriverResult Display(ForumIsMarketWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsMarketWidget", () =>
            {
                var shape = shapeHelper.Parts_ForumIsMarketWidget();
                //shape.ContentPart = part;
                return shape;
            });
        }
    }
}