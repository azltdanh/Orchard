using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyNewIsAuctionPartDriver : ContentPartDriver<PropertyNewIsAuctionWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyNewIsAuctionPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyNewIsAuctionWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyNewIsAuctionWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyNewIsAuctionWidget(
                        CountSelling: _fastfilterservice.CountPropertyWidgetByAdsType("new", "ad-selling"),
                        CountLeasing: _fastfilterservice.CountPropertyWidgetByAdsType("new", "ad-leasing")
                        //CountBuying: _fastfilterservice.CountPropertyWidgetByAdsType("new", "ad-buying"),
                        //CountRenting: _fastfilterservice.CountPropertyWidgetByAdsType("new", "ad-renting")
                        );
                    shape.ContentPart = part;
                    shape.AdsTypes = _fastfilterservice.GetAdsTypesFromRoute();
                    return shape;
                });
        }
    }
}