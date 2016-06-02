using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyListCompactPartDriver : ContentPartDriver<PropertyListCompactWidgetPart>
    {
        protected override DriverResult Display(PropertyListCompactWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyListCompactWidget",
              () =>
              {
                  dynamic shape = shapeHelper.Parts_PropertyListCompactWidget();
                  shape.ContentPart = part;
                  return shape;
              });
        }
    }
}