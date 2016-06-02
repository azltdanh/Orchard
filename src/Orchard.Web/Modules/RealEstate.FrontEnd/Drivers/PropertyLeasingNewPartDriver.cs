using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyLeasingNewPartDriver : ContentPartDriver<PropertyLeasingNewWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public PropertyLeasingNewPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(PropertyLeasingNewWidgetPart part, string displayType,
            dynamic shapeHelper)
        {
            return ContentShape("Parts_PropertyLeasingNewWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyLeasingNewWidget(
                        CountLeasing: _fastfilterservice.CountPropertyWidgetByAdsType("new", "ad-leasing")
                        );
                    shape.ContentPart = part;
                    //shape.AdsTypes = _fastfilterservice.GetAdsTypesFromRoute();
                    return shape;
                });
        }
    }
}