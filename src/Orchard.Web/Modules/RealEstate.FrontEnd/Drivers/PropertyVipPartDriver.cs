using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyVipPartDriver : ContentPartDriver<PropertyVipWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyVipPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyVipWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyVipWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyVipWidget(
                        CountSelling: _fastfilterservice.CountPropertyWidgetByAdsType("vip", "ad-selling"),
                        CountLeasing: _fastfilterservice.CountPropertyWidgetByAdsType("vip", "ad-leasing"),
                        CountBuying: _fastfilterservice.CountPropertyWidgetByAdsType("vip", "ad-buying"),
                        CountRenting: _fastfilterservice.CountPropertyWidgetByAdsType("vip", "ad-renting")
                        );
                    shape.ContentPart = part;
                    shape.AdsTypes = _fastfilterservice.GetAdsTypesFromRoute();
                    return shape;
                });
        }
    }
}