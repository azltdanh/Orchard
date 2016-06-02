
using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumPostOfUserWidgetPartDriver : ContentPartDriver<ForumPostOfUserWidgetPart>
    {
        protected override DriverResult Display(ForumPostOfUserWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumPostOfUserWidget",
                () =>
                {
                    var shape = shapeHelper.Parts_ForumPostOfUserWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}