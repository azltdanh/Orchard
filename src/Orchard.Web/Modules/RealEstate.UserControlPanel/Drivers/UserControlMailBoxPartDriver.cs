using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealEstate.UserControlPanel.Models;
using RealEstate.Services;
using Orchard.ContentManagement.Drivers;

namespace RealEstate.UserControlPanel.Drivers
{
    public class UserControlMailBoxPartDriver : ContentPartDriver<UserControlMailBoxWidgetPart>
    {
        public UserControlMailBoxPartDriver(IPropertyService propertyservice)
        {
            _propertyservice = propertyservice;
        }

        private IPropertyService _propertyservice;

        protected override DriverResult Display(UserControlMailBoxWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserControlMailBoxWidget",
                () =>
                {
                    var shape = shapeHelper.Parts_UserControlMailBoxWidget();
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}