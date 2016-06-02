using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using Orchard.Settings;
using Orchard.Security;
using Orchard;
using Orchard.Users.Models;

using RealEstate.UserControlPanel.Models;
using RealEstate.Services;
using RealEstate.UserControlPanel.Services;
using RealEstate.NewLetter.Services;

namespace RealEstate.UserControlPanel.Drivers
{
    public class UserControlWishListPartDriver : ContentPartDriver<UserControlWishListWidgetPart>
    {
        public UserControlWishListPartDriver(IPropertyService propertyservice, IControlPanelService controlpanelservice, IMessageInboxService messageInboxService)
        {
            _propertyservice = propertyservice;
            _controlpanelservice = controlpanelservice;
            _messageInboxService = messageInboxService;

        }
        private IPropertyService _propertyservice;
        private IControlPanelService _controlpanelservice;
        private IMessageInboxService _messageInboxService;

        protected override DriverResult Display(UserControlWishListWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserControlWishListWidget",
                () =>
                {
                    var shape = shapeHelper.Parts_UserControlWishListWidget(
                        CountUserProperties: _controlpanelservice.CountUserProperties(),
                        CoutMessageInbox: _messageInboxService.CountMessageInboxFromAdmin()
                        );
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}