using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;

namespace RealEstate.FrontEnd.Drivers
{
    public class BreadcrumbPartDriver : ContentPartDriver<BreadcrumbWidgetPart>
    {
        protected override DriverResult Display(BreadcrumbWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_BreadcrumbWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_BreadcrumbWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}