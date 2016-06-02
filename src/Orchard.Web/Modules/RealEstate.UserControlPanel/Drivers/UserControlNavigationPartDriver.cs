using Orchard.ContentManagement.Drivers;
using RealEstate.UserControlPanel.Models;
using RealEstate.Services;

namespace RealEstate.UserControlPanel.Drivers
{
    public class UserControlNavigationPartDriver : ContentPartDriver<UserControlNavigationWidgetPart>
    {
        public UserControlNavigationPartDriver(IPropertyService propertyservice)
        {
            _propertyservice = propertyservice;
        }

        private IPropertyService _propertyservice;

        protected override DriverResult Display(UserControlNavigationWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserControlNavigationWidget",
                () =>
                {
                    var shape = shapeHelper.Parts_UserControlNavigationWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}