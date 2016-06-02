using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyGoodDealIsAuctionPartDriver : ContentPartDriver<PropertyGoodDealIsAuctionWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyGoodDealIsAuctionPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyGoodDealIsAuctionWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyGoodDealIsAuctionWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyGoodDealIsAuctionWidget(
                        CountSelling: _fastfilterservice.CountPropertyWidgetByAdsType("gooddeal", "ad-selling")
                        );
                    shape.ContentPart = part;
                    //shape.AdsTypes = _fastfilterservice.GetAdsTypesFromRoute();
                    return shape;
                });
        }
    }
}