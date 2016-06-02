using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;
using System;

namespace RealEstate.ViewModels
{
    public class LocationApartmentViewModel
    {
    }

    public class LocationApartmentCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        [StringLength(255)]
        public string AddressNumber { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string ShortName { get; set; }

        [StringLength(255)]
        public string Block { get; set; }

        [Required]
        public int? Floors { get; set; }

        public double? PriceAverage { get; set; }

        [StringLength(255)]
        public string StreetAddress { get; set; }

        [StringLength(255)]
        public string DistanceToCentral { get; set; }

        [StringLength(255)]
        public string OtherAdvantages { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(255)]
        public string Investors { get; set; }

        [StringLength(255)]
        public string CurrentBuildingStatus { get; set; }

        [StringLength(255)]
        public string ManagementFees { get; set; }

        public double? AreaTotal { get; set; }
        public double? AreaConstruction { get; set; }
        public double? AreaGreen { get; set; }

        public int? TradeFloors { get; set; }
        public double? AreaTradeFloors { get; set; }

        public int? Basements { get; set; }
        public double? AreaBasements { get; set; }

        public int? Elevators { get; set; }

        public int SeqOrder { get; set; }

        public bool IsHighlight { get; set; }
        public DateTime? HighlightExpiredTime { get; set; }

        public bool IsEnabled { get; set; }

        // Advantages
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationApartment { get; set; }
    }

    public class LocationApartmentEditViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        [StringLength(255)]
        public string AddressNumber
        {
            get { return LocationApartment.As<LocationApartmentPart>().AddressNumber; }
            set { LocationApartment.As<LocationApartmentPart>().AddressNumber = value; }
        }

        [Required]
        [StringLength(255)]
        public string Name
        {
            get { return LocationApartment.As<LocationApartmentPart>().Name; }
            set { LocationApartment.As<LocationApartmentPart>().Name = value; }
        }

        [StringLength(255)]
        public string ShortName
        {
            get { return LocationApartment.As<LocationApartmentPart>().ShortName; }
            set { LocationApartment.As<LocationApartmentPart>().ShortName = value; }
        }

        [StringLength(255)]
        public string Block
        {
            get { return LocationApartment.As<LocationApartmentPart>().Block; }
            set { LocationApartment.As<LocationApartmentPart>().Block = value; }
        }

        [Required]
        public int? Floors
        {
            get { return LocationApartment.As<LocationApartmentPart>().Floors; }
            set { LocationApartment.As<LocationApartmentPart>().Floors = value; }
        }

        public double? PriceAverage
        {
            get { return LocationApartment.As<LocationApartmentPart>().PriceAverage; }
            set { LocationApartment.As<LocationApartmentPart>().PriceAverage = value; }
        }

        [StringLength(255)]
        public string StreetAddress
        {
            get { return LocationApartment.As<LocationApartmentPart>().StreetAddress; }
            set { LocationApartment.As<LocationApartmentPart>().StreetAddress = value; }
        }

        [StringLength(255)]
        public string DistanceToCentral
        {
            get { return LocationApartment.As<LocationApartmentPart>().DistanceToCentral; }
            set { LocationApartment.As<LocationApartmentPart>().DistanceToCentral = value; }
        }

        [StringLength(255)]
        public string OtherAdvantages
        {
            get { return LocationApartment.As<LocationApartmentPart>().OtherAdvantages; }
            set { LocationApartment.As<LocationApartmentPart>().OtherAdvantages = value; }
        }

        [StringLength(1000)]
        public string Description
        {
            get { return LocationApartment.As<LocationApartmentPart>().Description; }
            set { LocationApartment.As<LocationApartmentPart>().Description = value; }
        }

        [StringLength(255)]
        public string Investors
        {
            get { return LocationApartment.As<LocationApartmentPart>().Investors; }
            set { LocationApartment.As<LocationApartmentPart>().Investors = value; }
        }

        [StringLength(255)]
        public string CurrentBuildingStatus
        {
            get { return LocationApartment.As<LocationApartmentPart>().CurrentBuildingStatus; }
            set { LocationApartment.As<LocationApartmentPart>().CurrentBuildingStatus = value; }
        }

        [StringLength(255)]
        public string ManagementFees
        {
            get { return LocationApartment.As<LocationApartmentPart>().ManagementFees; }
            set { LocationApartment.As<LocationApartmentPart>().ManagementFees = value; }
        }

        public double? AreaTotal
        {
            get { return LocationApartment.As<LocationApartmentPart>().AreaTotal; }
            set { LocationApartment.As<LocationApartmentPart>().AreaTotal = value; }
        }
        public double? AreaConstruction
        {
            get { return LocationApartment.As<LocationApartmentPart>().AreaConstruction; }
            set { LocationApartment.As<LocationApartmentPart>().AreaConstruction = value; }
        }
        public double? AreaGreen
        {
            get { return LocationApartment.As<LocationApartmentPart>().AreaGreen; }
            set { LocationApartment.As<LocationApartmentPart>().AreaGreen = value; }
        }

        public int? TradeFloors
        {
            get { return LocationApartment.As<LocationApartmentPart>().TradeFloors; }
            set { LocationApartment.As<LocationApartmentPart>().TradeFloors = value; }
        }
        public double? AreaTradeFloors
        {
            get { return LocationApartment.As<LocationApartmentPart>().AreaTradeFloors; }
            set { LocationApartment.As<LocationApartmentPart>().AreaTradeFloors = value; }
        }

        public int? Basements
        {
            get { return LocationApartment.As<LocationApartmentPart>().Basements; }
            set { LocationApartment.As<LocationApartmentPart>().Basements = value; }
        }
        public double? AreaBasements
        {
            get { return LocationApartment.As<LocationApartmentPart>().AreaBasements; }
            set { LocationApartment.As<LocationApartmentPart>().AreaBasements = value; }
        }

        public int? Elevators
        {
            get { return LocationApartment.As<LocationApartmentPart>().Elevators; }
            set { LocationApartment.As<LocationApartmentPart>().Elevators = value; }
        }

        public int SeqOrder
        {
            get { return LocationApartment.As<LocationApartmentPart>().SeqOrder; }
            set { LocationApartment.As<LocationApartmentPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return LocationApartment.As<LocationApartmentPart>().IsEnabled; }
            set { LocationApartment.As<LocationApartmentPart>().IsEnabled = value; }
        }

        public bool IsHighlight
        {
            get { return LocationApartment.As<LocationApartmentPart>().IsHighlight; }
            set { LocationApartment.As<LocationApartmentPart>().IsHighlight = value; }
        }

        public DateTime? HighlightExpiredTime
        {
            get { return LocationApartment.As<LocationApartmentPart>().HighlightExpiredTime; }
            set { LocationApartment.As<LocationApartmentPart>().HighlightExpiredTime = value; }
        }

        // Advantages
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // Files
        public virtual IEnumerable<PropertyFilePart> Files { get; set; }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationApartment { get; set; }
    }

    public class LocationApartmentsIndexViewModel
    {
        public IList<LocationApartmentEntry> LocationApartments { get; set; }
        public LocationApartmentIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationApartmentEntry
    {
        public LocationApartmentPartRecord LocationApartment { get; set; }
        public IEnumerable<LocationApartmentBlockPart> LocationApartmentBlock { get; set; }
        public IEnumerable<PropertyFilePart> Files { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationApartmentIndexOptions
    {
        public string Search { get; set; }
        public LocationApartmentsOrder Order { get; set; }
        public LocationApartmentsFilter Filter { get; set; }
        public LocationApartmentsBulkAction BulkAction { get; set; }
        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public bool IsRestrictedLocations { get; set; }
        public bool IsHighlight { get; set; }

    }

    public class LocationApartmentCartCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage="Vui lòng chọn dự án / chung cư")]
        public int ApartmentId { get; set; }
        public LocationApartmentPart LocationApartment { get; set; }
        public IEnumerable<LocationApartmentPart> LocationApartments { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Block dự án / chung cư")]
        public int ApartmentBlockId { get; set; }
        public IEnumerable<LocationApartmentBlockPart> LocationApartmentBlocks { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổng số tầng")]
        public int FloorsNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổng số nhóm tầng")]
        public int FloorGroupTotal { get; set; }
        public int? GroupFloorPosition { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập vị trí tầng cho nhóm tầng")]
        public int FloorFrom { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập vị trí tầng cho nhóm tầng")]
        public int FloorTo { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số căn hộ mỗi tầng")]
        public int RoomInFloor { get; set; }

        public string ContactPhone { get; set; }
        public bool Published { get; set; }

        //[Required]
        public int RoomNumber { get; set; }
    }

    public class LocationApartmentCartIndexViewModel
    {
        public LocationApartmentPart LocationApartment { get; set; }
        public List<LocationApartmentBlockItem> LocationApartmentBlocks { get; set; }
        public int CountSelling { get; set; }
        public int CountOnHold { get; set; }
        public int CountNegotiate { get; set; }
        public int CountTrading { get; set; }
        public int CountSold { get; set; }
    }

    public class LocationApartmentBlockItem
    {
        public LocationApartmentBlockPart ApartmentBlockPart { get; set; }
        public IEnumerable<GroupInApartmentBlockPart> GroupInApartmentBlockParts { get; set; }
        public IEnumerable<PropertyPart> Properties { get; set; }
    }

    #region FrontEnd

    public class LocationApartmentIndexDisplayViewModel
    {
        public dynamic Pager { get; set; }
        public IEnumerable<LocationApartmentDisplayEntry> LocationApartments { get; set; }
        public LocationApartmentDisplayOptions Options { get; set; }
        public int TotalCount { get; set; }
    }
    public class LocationApartmentCompareDisplayViewModel
    {
        public LocationApartmentDisplayEntry LocationApartments { get; set; }
        public LocationApartmentDisplayEntry WithLocationApartments { get; set; }
        public LocationApartmentDisplayOptions Options { get; set; }
    }
    public class LocationApartmentDisplayEntry
    {
        public LocationApartmentPart LocationApartment { get; set; }
        public IEnumerable<PropertyFilePart> Files { get; set; }
        public string DefaultImgUrl { get; set; }
        public string DisplayUrl { get; set; }
        public IEnumerable<PropertyAdvantagePartRecord> LocationApartmentAdvantages { get; set; }
    }
    public class LocationApartmentDisplayOptions
    {
        //public Dictionary<string, string> MetaLayout { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }

        public int? ApartmentProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        public int[] ApartmentDistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] ApartmentApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }


        public int? WithApartmentProvinceId { get; set; }

        public IEnumerable<LocationDistrictPart> WithDistricts { get; set; }
        public int? ApartmentDistrictId { get; set; }
        public int? WithApartmentDistrictId { get; set; }

        public IEnumerable<LocationApartmentPart> WithApartments { get; set; }
        public int? ApartmentApartmentId { get; set; }
        public int? WithApartmentApartmentId { get; set; }

    }

    #endregion

    public enum LocationApartmentsOrder
    {
        Name,
        SeqOrder
    }

    public enum LocationApartmentsFilter
    {
        All
    }

    public enum LocationApartmentsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
