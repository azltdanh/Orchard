using System.Linq;
using Orchard.ContentManagement.Drivers;
using RealEstate.UserControlPanel.Models;
using RealEstate.UserControlPanel.Services;
using RealEstate.Services;

namespace RealEstate.UserControlPanel.Drivers
{
    public class UserControlPanelPartDriver : ContentPartDriver<UserControlPanelWidgetPart>
    {
        public UserControlPanelPartDriver(IPropertyService propertyService, IControlPanelService controlpanelservice)
        {
            _propertyService = propertyService;
            _controlpanelservice = controlpanelservice;
        }

        private IPropertyService _propertyService;
        private IControlPanelService _controlpanelservice;

        protected override DriverResult Display(UserControlPanelWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_UserControlPanelWidget",
                () =>
                {
                    var shape = shapeHelper.Parts_UserControlPanelWidget(
                        countDisplayAll: _controlpanelservice.GetOwnProperties("all").Count(),
                        countView: _controlpanelservice.GetOwnProperties("view").Count(),
                        countEstimate: _controlpanelservice.GetOwnProperties("estimate").Count(),
                        countNotDisplay: _controlpanelservice.GetOwnProperties("notdisplay").Count(),
                        countPending: _controlpanelservice.GetOwnProperties("pending").Count(),
                        countStopPublished: _controlpanelservice.GetOwnProperties("stop").Count(),
                        countInvalid: _controlpanelservice.GetOwnProperties("invalid").Count(),
                        countDeleted: _controlpanelservice.GetOwnProperties("del").Count(),
                        countDraft: _controlpanelservice.GetOwnProperties("draft").Count(),
                        countExchange: _propertyService.GetListPropertyExchange().Count()
                        );
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}