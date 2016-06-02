using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyNewPartDriver : ContentPartDriver<PropertyNewWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyNewPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyNewWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyNewWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyNewWidget(
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