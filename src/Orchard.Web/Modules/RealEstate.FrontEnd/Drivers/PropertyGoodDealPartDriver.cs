using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyGoodDealPartDriver : ContentPartDriver<PropertyGoodDealWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyGoodDealPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyGoodDealWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyGoodDealWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyGoodDealWidget(
                        CountSelling: _fastfilterservice.CountPropertyWidgetByAdsType("gooddeal", "ad-selling")
                        );
                    shape.ContentPart = part;
                    //shape.AdsTypes = _fastfilterservice.GetAdsTypesFromRoute();
                    return shape;
                });
        }
    }
}