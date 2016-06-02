using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class AsideFirstLinkPartDriver : ContentPartDriver<AsideFirstLinkWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;
        private readonly IPropertyService _propertyService;

        public AsideFirstLinkPartDriver(IPropertyService propertyService, IFastFilterService fastfilterservice)
        {
            _propertyService = propertyService;
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(AsideFirstLinkWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_AsideFirstLinkWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_AsideFirstLinkWidget(
                        AdsTypes: _propertyService.GetAdsTypes(),
                        TypeGroups: _propertyService.GetTypeGroups(),
                        Types: _propertyService.GetTypes(),

                        CountSellingHouse: _fastfilterservice.CountPropertyByAdsType("ad-selling", "gp-house"),
                        CountSellingApartment: _fastfilterservice.CountPropertyByAdsType("ad-selling", "gp-apartment"),
                        CountSellingLand: _fastfilterservice.CountPropertyByAdsType("ad-selling", "gp-land"),

                        CountLeasingHouse: _fastfilterservice.CountPropertyByAdsType("ad-leasing", "gp-house"),
                        CountLeasingApartment: _fastfilterservice.CountPropertyByAdsType("ad-leasing", "gp-apartment"),
                        CountLeasingLand: _fastfilterservice.CountPropertyByAdsType("ad-leasing", "gp-land"),

                        CountBuying: _fastfilterservice.CountRequirementByAdsType("ad-buying"),
                        CountRenting: _fastfilterservice.CountRequirementByAdsType("ad-renting"),
                        CountPropertyExchange: _propertyService.PropertyExchangeCount(),

                        CountSellingResidentialLand: _fastfilterservice.CountPropertyByType("ad-selling", "tp-residential-land"),
                        CountSellingConcreteHouse: _fastfilterservice.CountPropertyByType("ad-selling", "tp-concrete-house"),
                        CountSellingVilla: _fastfilterservice.CountPropertyByType("ad-selling", "tp-villa"),
                        CountSellingOfficeBuilding: _fastfilterservice.CountPropertyByType("ad-selling", "tp-office-building"),
                        CountSellingHotel: _fastfilterservice.CountPropertyByType("ad-selling", "tp-hotel"),
                        CountSellingResortLand: _fastfilterservice.CountPropertyByType("ad-selling", "tp-resort-land"),
                        CountSellingWarehouseWorkshop: _fastfilterservice.CountPropertyByType("ad-selling", "tp-warehouse-workshop"),
                        CountSellingFarmLand: _fastfilterservice.CountPropertyByType("ad-selling", "tp-farm-land"),

                        CountLeasingVilla: _fastfilterservice.CountPropertyByType("ad-leasing", "tp-villa"),
                        CountLeasingGroundFloor: _fastfilterservice.CountPropertyByType("ad-leasing", "tp-ground-floor"),
                        CountLeasingRoom: _fastfilterservice.CountPropertyByType("ad-leasing", "tp-room"),
                        CountLeasingWarehouseWorkshop: _fastfilterservice.CountPropertyByType("ad-leasing", "tp-warehouse-workshop")
                        );
                    shape.ContentPart = part;
                    return shape;
                });
        }
    }
}