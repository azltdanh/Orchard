using RealEstate.Models;
using System.Collections.Generic;

namespace RealEstate.NewLetter.ViewModels
{
    public class InfomationAddressViewModel
    {
        // AdsType
        public int? AdsTypeId { get; set; }
        public string AdsTypeCssClass { get; set; }
        public string AdsTypeNew { get; set; }
        public string AdsTypeVip { get; set; }
        public AdsTypePartRecord AdsType { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }
        public int? PropertyId { get; set; }
        // TypeGroup
        public int? TypeGroupId { get; set; }
        public PropertyTypeGroupPartRecord TypeGroup { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }
        public int? TypeId { get; set; }
        // Province
        public int? ProvinceId { get; set; }
        public LocationProvincePartRecord Province { get; set; }
        public IEnumerable<LocationProvincePartRecord> Provinces { get; set; }

        // Districts
        public int? DistrictId { get; set; }
        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPartRecord> Districts { get; set; }

        // Wards
        public int? WardId { get; set; }
        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPartRecord> Wards { get; set; }

        // Streets
        public int? StreetId { get; set; }
        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPartRecord> Streets { get; set; }

        // Directions
        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        // Location
        public int? LocationId { get; set; }
        public double? MinAlleyWidth { get; set; }

        public double? MinAreaTotal { get; set; }
        public double? MinAreaUsable { get; set; } // for apartment 
        public double? MinAreaTotalWidth { get; set; }
        public double? MinAreaTotalLength { get; set; }

        public double? MinFloors { get; set; }
        public int? MinBedrooms { get; set; }

        public double? MinPriceProposedInVND { get; set; }
        public double? MaxPriceProposedInVND { get; set; }

        //Them

        public bool FlagCheapPrice { get; set; }
        public bool AdsGoodDeal { get; set; }
        public bool AdsVIP { get; set; }
        public bool AdsNormal { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAuction { get; set; }

        public int? ApartmentFloorTh { get; set; }
        public string OtherProjectName { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        public double? MinPriceProposed { get; set; }
        public double? MaxPriceProposed { get; set; }
        public int[] AnyType { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        public string btSubmit { get; set; }

        public InfomationAddressOrder Order { get; set; }

        public InfomationAddressOrderWalk OrderWalk { get; set; }

        public InfomationAddressOrderApartmentFloorTh OrderApartmentFloorTh { get; set; }

        public bool flagAjax { get; set; }

        public bool flagAsideFirst { get; set; }

        public bool flagRequirment { get; set; }

        public string TitleArticle { get; set; }

        public string SearchPhone { get; set; }
    }

    public enum InfomationAddressOrder
    {
        LastUpdatedDate,
        AddressNumber,
        PriceProposedInVND
    }

    public enum InfomationAddressOrderWalk
    {
        None,
        All,
        AllWalk,
        Alley6,
        Alley5,
        Alley4,
        Alley3,
        Alley2,
        Alley
    }

    public enum InfomationAddressOrderApartmentFloorTh
    {
        None,
        All,
        ApartmentFloorTh1To3,
        ApartmentFloorTh4To7,
        ApartmentFloorTh8To12,
        ApartmentFloorTh12
    }
}