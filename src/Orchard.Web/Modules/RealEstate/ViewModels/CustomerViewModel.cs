using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Models;
using System.Web.Mvc;

namespace RealEstate.ViewModels
{
    public class CustomerViewModel
    {
    }

    #region INDEX

    public class CustomerIndexViewModel
    {
        public List<CustomerEntry> Customers { get; set; }
        public CustomerIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public double TotalExecutionTime { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class CustomerEntry
    {
        public CustomerPartRecord Customer { get; set; }
        public IEnumerable<CustomerRequirementRecord> Requirements { get; set; }
        public IEnumerable<CustomerPurposePartRecord> Purposes { get; set; }
        public bool IsChecked { get; set; }
        public bool IsEditable { get; set; }
        public bool ShowContactPhone { get; set; }
    }

    public class CustomerIndexOptions
    {
        public string ReturnUrl { get; set; }

        public string Search { get; set; }
        public CustomerOrder Order { get; set; }
        public CustomerFilter Filter { get; set; }
        public CustomerBulkAction BulkAction { get; set; }

        public DateTime NeedUpdateDate { get; set; }

        // Filter

        public string List { get; set; }

        public int? Id { get; set; }
        public string IdStr { get; set; }

        // Status
        public int? StatusId { get; set; }
        public string StatusCssClass { get; set; }
        public IEnumerable<CustomerStatusPartRecord> Status { get; set; }

        // Purposes
        public int[] PurposeIds { get; set; }
        public IEnumerable<CustomerPurposePartRecord> Purposes { get; set; }

        // AdsType
        public int? AdsTypeId { get; set; }
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // PropertyTypeGroup
        public int? TypeGroupId { get; set; }
        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Address

        public int? ProvinceId { get; set; }
        public List<SelectListItem> Provinces { get; set; }

        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int? ApartmentId { get; set; }
        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        // Apartment

        public string OtherProjectName { get; set; }

        // Price

        public double? MinPriceProposed { get; set; }
        public double? MaxPriceProposed { get; set; }

        public int PaymentMethodId { get; set; }
        public string PaymentMethodCssClass { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        // Area

        public double? MinAreaTotal { get; set; }
        public double? MaxAreaTotal { get; set; }

        public double? MinAreaTotalWidth { get; set; }
        public double? MaxAreaTotalWidth { get; set; }

        public double? MinAreaTotalLength { get; set; }
        public double? MaxAreaTotalLength { get; set; }

        // AreaUsable
        public double? MinAreaUsable { get; set; }
        public double? MaxAreaUsable { get; set; }

        // ApartmentFloorTh
        public double? MinApartmentFloorTh { get; set; }
        public double? MaxApartmentFloorTh { get; set; }

        // Direction

        public int? DirectionId { get; set; }
        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        // Location

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        // Alley

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }

        public double? MinAlleyTurns { get; set; }
        public double? MaxAlleyTurns { get; set; }

        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        // Construction

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }

        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }

        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        // Contact

        public string ContactName { get; set; }
        public string ContactPhone { get; set; }

        // Users

        public int[] ServedUserIds { get; set; }
        public string VisitedFrom { get; set; }
        public string VisitedTo { get; set; }
        public IEnumerable<UserPartRecord> ServedUsers { get; set; }

        public int? CreatedUserId { get; set; }
        public string CreatedFrom { get; set; }
        public string CreatedTo { get; set; }

        public int? LastUpdatedUserId { get; set; }
        public string LastUpdatedFrom { get; set; }
        public string LastUpdatedTo { get; set; }

        public int? GroupId { get; set; }
        public int[] GroupIds { get; set; }
        public IEnumerable<UserGroupPartRecord> Groups { get; set; }

        public IEnumerable<UserPartRecord> Users { get; set; }
    }

    public enum CustomerOrder
    {
        LastUpdatedDate,
        CreatedDate,
        ContactName
    }

    public enum CustomerFilter
    {
        All
    }

    public enum CustomerBulkAction
    {
        None,

        // Internal Actions
        Publish,
        UnPublish,
        Delete,
        Export,
        UpdateNegotiateStatus,

        StatusNew,
        StatusHigh,
        StatusNegotiate,
        StatusTrading,
        StatusBought,
        StatusOnhold,
        StatusSuspended,
        StatusDoubt,
        StatusBroker,
        StatusTrash,

        // External Actions
        Refresh,
        AddToAdsVIP,
        RemoveAdsVIP,
        Approve,
        NotApprove,
    }

    #endregion

    #region Create / Edit

    public class CustomerCreateViewModel
    {
        public string ReturnUrl { get; set; }

        public int Id { get; set; }

        public bool IsExternalCustomer { get; set; }

        // Status
        public int StatusId { get; set; }
        public IEnumerable<CustomerStatusPartRecord> Status { get; set; }
        public DateTime? StatusChangedDate { get; set; }
        public string Note { get; set; }

        //PropertyExchange
        public bool IsRequirementExchange { get; set; }
        public int? PropertyExchangeId { get; set; }
        //[Required(ErrorMessage = "Vui Lòng chọn loại giao dịch chênh lệch muốn đổi")]
        public string ExchangeTypeClass { get; set; }
        public List<SelectListItem> ExchangeTypes { get; set; }
        public double ExchangeValue { get; set; }
        public string PropertyLink { get; set; }
        public string PropertyAddress { get; set; }

        // Purposes
        public List<CustomerPurposeEntry> Purposes { get; set; }

        // Requirements
        public List<CustomerRequirementEntry> Requirements { get; set; }

        public IContent Customer { get; set; }

        #region CustomerRequirement

        public int? GroupId { get; set; }
        public bool IsEnabled { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Loại tin rao.")]
        public int? AdsTypeId { get; set; }

        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Nhóm BĐS.")]
        public int? PropertyTypeGroupId { get; set; }

        public IEnumerable<PropertyTypeGroupPartRecord> PropertyTypeGroups { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố.")]
        public int? ProvinceId { get; set; }

        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        //[Required(ErrorMessage = "Vui lòng chọn ít nhất một Quận / Huyện.")]
        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public double? MinArea { get; set; }
        public double? MaxArea { get; set; }
        public double? MinWidth { get; set; }
        public double? MaxWidth { get; set; }
        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }
        public int? MinAlleyTurns { get; set; }
        public int? MaxAlleyTurns { get; set; }
        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        public string OtherProjectName { get; set; }
        public bool ChkOtherProjectName { get; set; }
        public int? MinApartmentFloorTh { get; set; }
        public int? MaxApartmentFloorTh { get; set; }

        #endregion

        #region User

        // User
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int LastUpdatedUserId { get; set; }
        public DateTime? LastCallDate { get; set; }
        public int LastCallUserId { get; set; }

        #endregion

        #region Ads

        // Published
        public bool Published { get; set; }
        public DateTime? AdsExpirationDate { get; set; }
        public ExpirationDate AddAdsExpirationDate { get; set; }

        // AdsVIP
        public bool AdsVIP { get; set; }
        public DateTime? AdsVIPExpirationDate { get; set; }
        public ExpirationDate AddAdsVIPExpirationDate { get; set; }

        #endregion

        #region Contact

        // Contact
        [Required(ErrorMessage = "Vui lòng nhập Tên khách hàng.")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Điện thoại liên hệ.")]
        public string ContactPhone { get; set; }

        public string ContactAddress { get; set; }
        public string ContactEmail { get; set; }

        #endregion
    }

    public class CustomerEditViewModel
    {
        public string ReturnUrl { get; set; }

        public int Id
        {
            get { return Customer.As<CustomerPart>().Id; }
        }

        public bool IsExternalCustomer { get; set; }

        public bool EnableEditContactPhone { get; set; }
        public bool EnableDeleteCustomerProperty { get; set; }

        //PropertyExchange
        public bool IsRequirementExchange { get; set; }
        public int? PropertyExchangeId { get; set; }
        //[Required(ErrorMessage = "Vui Lòng chọn loại giao dịch chênh lệch muốn đổi")]
        public string ExchangeTypeClass { get; set; }
        public List<SelectListItem> ExchangeTypes { get; set; }
        public double ExchangeValue { get; set; }
        public string PropertyLink { get; set; }
        public string PropertyAddress { get; set; }

        // Status

        public int StatusId { get; set; }
        public IEnumerable<CustomerStatusPartRecord> Status { get; set; }

        public DateTime? StatusChangedDate
        {
            get { return Customer.As<CustomerPart>().StatusChangedDate; }
            set { Customer.As<CustomerPart>().StatusChangedDate = value; }
        }

        public string Note
        {
            get { return Customer.As<CustomerPart>().Note; }
            set { Customer.As<CustomerPart>().Note = value; }
        }

        // Purposes
        public List<CustomerPurposeEntry> Purposes { get; set; }

        // Requirements
        public List<CustomerRequirementEntry> Requirements { get; set; }

        // Properties
        public List<CustomerPropertyEntry> Properties { get; set; }
        public dynamic Pager { get; set; }

        public IContent Customer { get; set; }

        #region CustomerProperties

        public int? PropertyId { get; set; }
        public int? FeedbackId { get; set; }
        public IEnumerable<CustomerFeedbackPartRecord> Feedbacks { get; set; }
        public int[] UserIds { get; set; }
        public IEnumerable<UserPartRecord> Users { get; set; }
        public string VisitedDate { get; set; }
        public bool IsWorkOverTime { get; set; }

        #endregion

        #region User

        public DateTime CreatedDate
        {
            get { return Customer.As<CustomerPart>().CreatedDate; }
            set { Customer.As<CustomerPart>().CreatedDate = value; }
        }

        public UserPartRecord CreatedUser
        {
            get { return Customer.As<CustomerPart>().CreatedUser; }
        }

        public DateTime LastUpdatedDate
        {
            get { return Customer.As<CustomerPart>().LastUpdatedDate; }
            set { Customer.As<CustomerPart>().LastUpdatedDate = value; }
        }

        public UserPartRecord LastUpdatedUser
        {
            get { return Customer.As<CustomerPart>().LastUpdatedUser; }
        }

        public DateTime? LastCallDate
        {
            get { return Customer.As<CustomerPart>().LastCallDate; }
            set { Customer.As<CustomerPart>().LastCallDate = value; }
        }

        public UserPartRecord LastCallUser
        {
            get { return Customer.As<CustomerPart>().LastCallUser; }
        }

        #endregion

        #region Ads

        // Published

        public bool Published
        {
            get { return Customer.As<CustomerPart>().Published; }
            set { Customer.As<CustomerPart>().Published = value; }
        }

        public DateTime? AdsExpirationDate
        {
            get { return Customer.As<CustomerPart>().AdsExpirationDate; }
            set { Customer.As<CustomerPart>().AdsExpirationDate = value; }
        }

        public ExpirationDate AddAdsExpirationDate { get; set; }

        // AdsVIP

        public bool AdsVIP
        {
            get { return Customer.As<CustomerPart>().AdsVIP; }
            set { Customer.As<CustomerPart>().AdsVIP = value; }
        }

        public DateTime? AdsVIPExpirationDate
        {
            get { return Customer.As<CustomerPart>().AdsVIPExpirationDate; }
            set { Customer.As<CustomerPart>().AdsVIPExpirationDate = value; }
        }

        public ExpirationDate AddAdsVIPExpirationDate { get; set; }

        #endregion

        #region CustomerRequirement

        public int? GroupId { get; set; }
        public bool IsEnabled { get; set; }

        public int? AdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? PropertyTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> PropertyTypeGroups { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public double? MinArea { get; set; }
        public double? MaxArea { get; set; }
        public double? MinWidth { get; set; }
        public double? MaxWidth { get; set; }
        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }
        public int? MinAlleyTurns { get; set; }
        public int? MaxAlleyTurns { get; set; }
        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        public string OtherProjectName { get; set; }
        public bool ChkOtherProjectName { get; set; }
        public int? MinApartmentFloorTh { get; set; }
        public int? MaxApartmentFloorTh { get; set; }

        #endregion

        #region Contact

        [Required(ErrorMessage = "Vui lòng nhập Tên khách hàng.")]
        public string ContactName
        {
            get { return Customer.As<CustomerPart>().ContactName; }
            set { Customer.As<CustomerPart>().ContactName = value; }
        }

        [Required(ErrorMessage = "Vui lòng nhập Điện thoại liên hệ.")]
        public string ContactPhone
        {
            get { return Customer.As<CustomerPart>().ContactPhone; }
            set { Customer.As<CustomerPart>().ContactPhone = value; }
        }

        public string ContactAddress
        {
            get { return Customer.As<CustomerPart>().ContactAddress; }
            set { Customer.As<CustomerPart>().ContactAddress = value; }
        }

        public string ContactEmail
        {
            get { return Customer.As<CustomerPart>().ContactEmail; }
            set { Customer.As<CustomerPart>().ContactEmail = value; }
        }

        #endregion
    }

    public class CustomerEditRequirementViewModel
    {
        #region CustomerRequirement

        public int? GroupId { get; set; }
        public bool IsEnabled { get; set; }

        public int? AdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? PropertyTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> PropertyTypeGroups { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public double? MinArea { get; set; }
        public double? MaxArea { get; set; }
        public double? MinWidth { get; set; }
        public double? MaxWidth { get; set; }
        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }
        public int? MinAlleyTurns { get; set; }
        public int? MaxAlleyTurns { get; set; }
        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }
        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }
        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        public string OtherProjectName { get; set; }
        public int? MinApartmentFloorTh { get; set; }
        public int? MaxApartmentFloorTh { get; set; }

        #endregion
    }

    public class CustomerRequirementEntry
    {
        public CustomerRequirementRecord Requirement { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CustomerPropertyEntry
    {
        public CustomerPropertyRecord CustomerPropertyRecord { get; set; }
        public PropertyPart PropertyPart { get; set; }
        public IEnumerable<PropertyAdvantagePartRecord> Advantages { get; set; }
        public bool IsChecked { get; set; }
        public int Id { get; set; }
        public int CustomerFeedbackId { get; set; }
        public string CustomerFeedbackCssClass { get; set; }
        public string CustomerStaff { get; set; }
        public DateTime? CustomerVisitedDate { get; set; }
        public bool IsWorkOverTime { get; set; }
        public bool ShowContactPhone { get; set; }
        public bool IsEditable { get; set; }
    }

    public class CustomerRevisionEntry
    {
        public DateTime CreatedDate { get; set; }
        public UserPartRecord CreatedUser { get; set; }
        public string StatusName { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string ContactAddress { get; set; }
    }

    public class CustomerRevisionsViewModel
    {
        public CustomerPart Customer { get; set; }
        public IEnumerable<CustomerRevisionEntry> Revisions { get; set; }
    }

    public class CustomerVisitedEntry
    {
        public LocationProvincePartRecord Province { get; set; }
        public LocationDistrictPartRecord District { get; set; }
        public LocationWardPartRecord Ward { get; set; }
        public LocationStreetPartRecord Street { get; set; }
        public int Count { get; set; }
    }

    public class CustomerVisitedPropertiesViewModel
    {
        public CustomerPart Customer { get; set; }
        public IEnumerable<CustomerVisitedEntry> VisitedStreets { get; set; }
    }

    public class CalledByUserEntry
    {
        public UserPartRecord User { get; set; }
        public List<DateTime> CallLogs { get; set; }
        public int Count { get; set; }
    }

    public class CustomerCalledByUsers
    {
        public CustomerPart Customer { get; set; }
        public IEnumerable<CalledByUserEntry> CalledByUsers { get; set; }
    }

    #endregion
    
}