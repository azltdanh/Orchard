using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyTagsDriver : ContentPartDriver<PropertyTagsWidgetPart>
    {
        protected override DriverResult Display(PropertyTagsWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyTagsWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyTagsWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}