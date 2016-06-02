using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsLawHousingWidgetPartDriver : ContentPartDriver<ForumIsLawHousingWidgetPart>
    {
        protected override DriverResult Display(ForumIsLawHousingWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsLawHousingWidget", () =>
            {
                var shape = shapeHelper.Parts_ForumIsLawHousingWidget();
                //shape.ContentPart = part;
                return shape;
            });
        }
    }
}