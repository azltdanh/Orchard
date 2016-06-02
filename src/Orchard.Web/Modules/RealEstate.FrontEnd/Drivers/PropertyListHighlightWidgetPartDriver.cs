using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyListHighlightWidgetPartDriver : ContentPartDriver<PropertyListHighlightWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyListHighlightWidgetPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyListHighlightWidgetPart part, string displayType,
            dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyListHighlightWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyListHighlightWidget(
                        CountSelling: _fastfilterservice.CountPropertyWidgetByAdsType("highlight", "ad-selling"),
                        CountLeasing: _fastfilterservice.CountPropertyWidgetByAdsType("highlight", "ad-leasing")
                        );
                    shape.ContentPart = part;
                    shape.AdsTypes = _fastfilterservice.GetAdsTypesFromRoute();
                    return shape;
                });
        }
    }
}