using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Models;
using System.Web.Mvc;

namespace RealEstate.ViewModels
{

    #region INDEX

    public class PropertyIndexViewModel
    {
        public IList<PropertyEntry> Properties { get; set; }
        public PropertyIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public double TotalExecutionTime { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class PropertyEntry
    {
        public PropertyPartRecord Property { get; set; }
        public PropertyPart PropertyPart { get; set; }
        public IEnumerable<PropertyFilePart> Files { get; set; }
        //public IEnumerable<PropertyAdvantagePartRecord> Advantages { get; set; }
        public List<PropertyAdvantageItem> Advantages { get; set; }

        public string DisplayForContact { get; set; }
        public HostNamePart HostNamePart { get; set; }


        public bool IsChecked { get; set; }
        public bool IsEditable { get; set; }
        public bool IsCopyable { get; set; }
        public bool ShowAddressNumber { get; set; }
        public bool ShowContactPhone { get; set; }

        public bool IsExportedRecently { get; set; }
        public bool IsExportedExpired { get; set; }
        public bool IsCheckSavedProperty { get; set; }
        public PropertyExchangePartRecord PropertyExchange { get; set; }
    }

    public class PropertyIndexOptions
    {
        public IEnumerable<IsApprovedEntry> ApproveAllGroup =
            new List<IsApprovedEntry>
            {
                new IsApprovedEntry {Name = "Tất cả BĐS", Value = "none"},
                new IsApprovedEntry {Name = "Tin đã xóa", Value = "NotApproved"},
            };

        public IEnumerable<IsApprovedEntry> NotApproveAllGroup =
            new List<IsApprovedEntry>
            {
                new IsApprovedEntry {Name = "Tin chưa duyệt", Value = "none"},
                new IsApprovedEntry {Name = "Tin đã duyệt", Value = "Approved"},
                new IsApprovedEntry {Name = "Tin đã xóa", Value = "NotApproved"},
            };

        public bool UseAccurateSearch { get; set; }

        public string ReturnUrl { get; set; }

        public string Search { get; set; }
        public PropertyOrder Order { get; set; }
        public PropertyFilter Filter { get; set; }
        public PropertyBulkAction BulkAction { get; set; }
        public IList<PropertyBulkAction> PublishBulkAction { get; set; }

        public DateTime NeedUpdateDate { get; set; }

        public string List { get; set; }
        public int[] SelectedIds { get; set; }

        public int? Id { get; set; }
        public string IdStr { get; set; }

        // Status
        public int? StatusId { get; set; }
        public string StatusCssClass { get; set; }
        public IEnumerable<PropertyStatusPartRecord> Status { get; set; }

        // IsSoldByGroup
        public bool IsSoldByGroup { get; set; }

        // Flag
        public int? FlagId { get; set; }
        public int[] FlagIds { get; set; }
        public IEnumerable<PropertyFlagPartRecord> Flags { get; set; }

        // AdsType
        public int? AdsTypeId { get; set; }
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // TypeGroup
        public int? TypeGroupId { get; set; }
        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Type
        public int[] TypeIds { get; set; }
        public IEnumerable<PropertyTypePartRecord> Types { get; set; }

        // TypeConstruction
        public int[] TypeConstructionIds { get; set; }
        public IEnumerable<PropertyTypeConstructionPartRecord> TypeConstructions { get; set; }

        // Address

        public int? ProvinceId { get; set; }
        public List<SelectListItem> Provinces { get; set; }

        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int? StreetId { get; set; }
        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int? ApartmentId { get; set; }
        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public string AddressNumber { get; set; }

        public string ApartmentNumber { get; set; }

        public string AddressCorner { get; set; }

        // Price

        public double? MinPriceProposed { get; set; }
        public double? MaxPriceProposed { get; set; }

        public int PaymentMethodId { get; set; }
        public string PaymentMethodCssClass { get; set; }
        public string PaymentMethodShortName { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        // Estimation

        public bool ShowEstimation { get; set; }
        public double? PriceEstimatedRatingPoint { get; set; }

        public bool ShowNeedUpdate { get; set; }
        public bool ShowExcludedInEstimation { get; set; }
        public bool ShowIncludedInEstimation { get; set; }

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

        // LegalStatus
        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }

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

        // Interior
        public int? InteriorId { get; set; }
        public IEnumerable<PropertyInteriorPartRecord> Interiors { get; set; }

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

        public int? FirstInfoFromUserId { get; set; }
        public IEnumerable<UserPartRecord> Users { get; set; }

        public string CreatedUserNameOrEmail { get; set; }

        public int? CreatedUserId { get; set; }
        public string CreatedFrom { get; set; }
        public string CreatedTo { get; set; }

        public int? LastUpdatedUserId { get; set; }
        public string LastUpdatedFrom { get; set; }
        public string LastUpdatedTo { get; set; }

        public int? LastExportedUserId { get; set; }
        public string LastExportedFrom { get; set; }
        public string LastExportedTo { get; set; }

        public int? GroupId { get; set; }
        public int[] GroupIds { get; set; }
        public IEnumerable<UserGroupPartRecord> Groups { get; set; }
        public int? BelongGroupId { get; set; }

        // Advantages
        public int[] AdvantageIds { get; set; }
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // DisAdvantages
        public int[] DisAdvantageIds { get; set; }
        public IList<PropertyAdvantageEntry> DisAdvantages { get; set; }

        // ApartmentAdvantages
        public int[] ApartmentAdvantageIds { get; set; }
        public IList<PropertyAdvantageEntry> ApartmentAdvantages { get; set; }

        // ApartmentInteriorAdvantages
        public int[] ApartmentInteriorAdvantageIds { get; set; }
        public IList<PropertyAdvantageEntry> ApartmentInteriorAdvantages { get; set; }

        // UserGroup by Domain
        public UserGroupPartRecord UserGroupDomain { get; set; }
        public IEnumerable<HostNamePartRecord> UserGroupDomains { get; set; }

        // Duyệt tin của group
        public string GroupApproved { get; set; }
        public IEnumerable<IsApprovedEntry> IsApprovedEntries { get; set; }
        public bool IsApproveAllGroup { get; set; }

        public bool AdsGoodDeal { get; set; }
        public bool AdsGoodDealRequest { get; set; }
        // ReSharper disable InconsistentNaming
        public bool AdsVIP { get; set; }
        public bool AdsVIP1 { get; set; }
        public bool AdsVIP2 { get; set; }
        public bool AdsVIP3 { get; set; }
        public bool AdsVIPRequest { get; set; }
        // ReSharper restore InconsistentNaming
        public bool AdsHighlight { get; set; }
        public bool AdsHighlightRequest { get; set; }
        public bool AdsNormal { get; set; }

        public bool AdsRequest { get; set; }

        public bool IsOwner { get; set; }
        public bool NoBroker { get; set; }
        public bool IsAuction { get; set; }

        public bool PublishAddress { get; set; }
        public bool PublishContact { get; set; }

        public bool AdsExpired { get; set; }
        public bool AdsNotExpired { get; set; }

        public bool IsHighlights { get; set; }
        public bool IsAuthenticatedInfo { get; set; }

        public bool IsPropertiesWatchList { get; set; }
        public bool IsPropertiesExchange { get; set; }
    }

    public class IsApprovedEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public enum PropertyOrder
    {
        LastUpdatedDate,
        AddressNumber,
// ReSharper disable InconsistentNaming
        PriceProposedInVND
// ReSharper restore InconsistentNaming
    }

    public enum PropertyFilter
    {
        All
    }

    public enum PropertyBulkAction
    {
        None,

        #region Estimation

        AddToEstimation,
        RemoveFromEstimation,

        #endregion

        #region Listing, Trash, Delete, Export

        Listing,
        Trash,
        Delete,
        Export,

        #endregion

        #region Refresh, Approve, NotApprove, Copy

        Refresh,
        Approve,
        NotApprove,
        Copy,

        #endregion

        #region Publish, PublishAddress, PublishContact

        // Publish
        Publish,
        UnPublish,

        // PublishAddress
        PublishAddress,
        UnPublishAddress,

        // PublishContact
        PublishContact,
        UnPublishContact,

        #endregion

        #region AdsGoodDeal, AdsVIP, AdsHighlight

        // AdsGoodDeal
        AddToAdsGoodDeal,
        RemoveAdsGoodDeal,

        // AdsVIP
        // ReSharper disable InconsistentNaming
        AddToAdsVIP,
        RemoveAdsVIP,
        AddToVIP1,
        AddToVIP2,
        AddToVIP3,
        // ReSharper restore InconsistentNaming

        // AdsHighlight
        AddToAdsHighlight,
        RemoveAdsHighlight,

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsAuthenticatedInfo

        // IsOwner - BĐS chính chủ
        SetIsOwner,
        UnSetIsOwner,

        // NoBroker - BĐS miễn trung gian
        SetNoBroker,
        UnSetNoBroker,

        // IsAuction - BĐS phát mãi
        SetIsAuction,
        UnSetIsAuction,

        // SetIsAuthenticatedInfo - BĐS đã xác thực
        SetIsAuthenticatedInfo,
        UnSetIsAuthenticatedInfo,

        //Xóa khỏi danh sách BĐS theo dõi
        DeleteUserProperties,

        #endregion

        #region Mass-Update Properties

        UpdateNegotiateStatus,
        UpdateMetaDescriptionKeywords,
        TransferPropertyTypeConstruction,
        UpdatePlacesAround,

        #endregion

        #region ShareToGroup Properties

        ShareToGroup,
        NotShareToGroup,
        RemoveShareToGroup,
        UnRemoveShareToGroup,
        CopyToGroup

        #endregion
    }

    public enum ExpirationDate
    {
        None,

        Day10,
        Day20,
        Day30,
        Day60,
        Day90,

        Week1,
        Week2,
        Week3,
        Week4,

        Month1,
        Month2,
        Month3,
    }

// ReSharper disable InconsistentNaming
    public enum AdsTypeVIP
    {
        VIP1 = 3,
        VIP2 = 2,
        VIP3 = 1
    }
    // ReSharper restore InconsistentNaming

    public class ImportActionEntry
    {
        public string Group { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsEnable { get; set; }
    }

    public class PropertyRevisionEntry
    {
        public DateTime CreatedDate { get; set; }
        public UserPartRecord CreatedUser { get; set; }
        public string StatusName { get; set; }
        public string PriceProposed { get; set; }
        public string PaymentMethodName { get; set; }
        public string PaymentUnitName { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string Note { get; set; }
        public string LastInfoFromUserName { get; set; }
        public string AdsOnline { get; set; }
        public string AdsNewspaper { get; set; }
        public string AddImage { get; set; }
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public string ImageName { get; set; }
        public bool ImagePublished { get; set; }
        public bool ImageIsAvatar { get; set; }
        public bool ImageIsDeleted { get; set; }
        public bool Editable { get; set; }
    }

    public class PropertyRevisionsViewModel
    {
        public PropertyPart Property { get; set; }
        public IEnumerable<PropertyRevisionEntry> Revisions { get; set; }
    }

    public class PropertyImagesViewModel
    {
        // Images
        public virtual IEnumerable<PropertyFilePart> Images { get; set; }
        public bool EnableEditImages { get; set; }
    }

    #endregion

    #region ViewModel

    public class PropertyViewModel
    {
        public IContent Property { get; set; }

        public bool EnableEditProperty { get; set; }
        public bool EnableCopyPropertyToGroup { get; set; }
        public bool EnableCopyPropertyToAdsType { get; set; }

        public bool EnableViewAddressNumber { get; set; }
        public bool EnableViewContactPhone { get; set; }

        public string ReturnUrl { get; set; }
        public int VisitedCount { get; set; }
        public PropertyExchangePartRecord PropertyExchange { get; set; }

        public int Id
        {
            get { return Property.As<PropertyPart>().Id; }
        }

        public string DisplayForUrl
        {
            get { return Property.As<PropertyPart>().DisplayForUrl; }
        }

        #region Flag, Status

        // Status
        public PropertyStatusPartRecord Status
        {
            get { return Property.As<PropertyPart>().Status; }
        }

        // StatusChangedDate
        public DateTime? StatusChangedDate
        {
            get { return Property.As<PropertyPart>().StatusChangedDate; }
        }

        // IsSoldByGroup
        public bool IsSoldByGroup
        {
            get { return Property.As<PropertyPart>().IsSoldByGroup; }
        }

        // Flag
        public PropertyFlagPartRecord Flag
        {
            get { return Property.As<PropertyPart>().Flag; }
        }

        // IsExcludeFromPriceEstimation
        public bool IsExcludeFromPriceEstimation
        {
            get { return Property.As<PropertyPart>().IsExcludeFromPriceEstimation; }
        }

        #endregion

        #region AdsType

        // AdsType
        public AdsTypePartRecord AdsType
        {
            get { return Property.As<PropertyPart>().AdsType; }
        }
        // AdsTypeCssClass
        public string AdsTypeCssClass
        {
            get { return Property.As<PropertyPart>().AdsType != null ? Property.As<PropertyPart>().AdsType.CssClass : ""; }
        }

        // Published
        public bool Published
        {
            get { return Property.As<PropertyPart>().Published; }
        }

        public DateTime? AdsExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsExpirationDate; }
        }

        // AdsGoodDeal
        public bool AdsGoodDeal
        {
            get { return Property.As<PropertyPart>().AdsGoodDeal; }
        }

        public bool AdsGoodDealRequest
        {
            get { return Property.As<PropertyPart>().AdsGoodDealRequest; }
        }

        public DateTime? AdsGoodDealExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsGoodDealExpirationDate; }
        }

        // AdsVIP
        // ReSharper disable InconsistentNaming
        public bool AdsVIP
        {
            get { return Property.As<PropertyPart>().AdsVIP; }
        }

        public bool AdsVIPRequest
        {
            get { return Property.As<PropertyPart>().AdsVIPRequest; }
        }

        public DateTime? AdsVIPExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsVIPExpirationDate; }
        }

        // ReSharper restore InconsistentNaming

        // AdsHighlight
        public bool AdsHighlight
        {
            get { return Property.As<PropertyPart>().AdsHighlight; }
        }

        public bool AdsHighlightRequest
        {
            get { return Property.As<PropertyPart>().AdsHighlightRequest; }
        }

        public DateTime? AdsHighlightExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsHighlightExpirationDate; }
        }

        #endregion

        #region Type

        // TypeGroup
        public PropertyTypeGroupPartRecord TypeGroup
        {
            get { return Property.As<PropertyPart>().TypeGroup; }
        }

        // Type
        public PropertyTypePartRecord Type
        {
            get { return Property.As<PropertyPart>().Type; }
        }

        // TypeConstruction
        public PropertyTypeConstructionPartRecord TypeConstruction
        {
            get { return Property.As<PropertyPart>().TypeConstruction; }
        }

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

        public bool IsOwner
        {
            get { return Property.As<PropertyPart>().IsOwner; }
        }

        public bool NoBroker
        {
            get { return Property.As<PropertyPart>().NoBroker; }
        }

        public bool IsAuction
        {
            get { return Property.As<PropertyPart>().IsAuction; }
        }

        public bool IsHighlights
        {
            get { return Property.As<PropertyPart>().IsHighlights; }
        }

        public bool IsAuthenticatedInfo
        {
            get { return Property.As<PropertyPart>().IsAuthenticatedInfo; }
        }

        #endregion

        #region Address

        // Address

        public string DisplayForAddress
        {
            get { return Property.As<PropertyPart>().DisplayForAddressForOwner; }
        }

        public LocationProvincePartRecord Province
        {
            get { return Property.As<PropertyPart>().Province; }
        }

        public LocationDistrictPartRecord District
        {
            get { return Property.As<PropertyPart>().District; }
        }

        public LocationWardPartRecord Ward
        {
            get { return Property.As<PropertyPart>().Ward; }
        }

        public LocationStreetPartRecord Street
        {
            get { return Property.As<PropertyPart>().Street; }
        }

        public LocationApartmentPartRecord Apartment
        {
            get { return Property.As<PropertyPart>().Apartment; }
        }

        public string OtherProvinceName
        {
            get { return Property.As<PropertyPart>().OtherProvinceName; }
        }

        public string OtherDistrictName
        {
            get { return Property.As<PropertyPart>().OtherDistrictName; }
        }

        public string OtherWardName
        {
            get { return Property.As<PropertyPart>().OtherWardName; }
        }

        public string OtherStreetName
        {
            get { return Property.As<PropertyPart>().OtherStreetName; }
        }

        public string OtherProjectName
        {
            get { return Property.As<PropertyPart>().OtherProjectName; }
        }

        public string AddressNumber
        {
            get { return Property.As<PropertyPart>().AddressNumber; }
        }

        public string AddressCorner
        {
            get { return Property.As<PropertyPart>().AddressCorner; }
        }

        public int AlleyNumber
        {
            get { return Property.As<PropertyPart>().AlleyNumber; }
        }

        public bool PublishAddress
        {
            get { return Property.As<PropertyPart>().PublishAddress; }
        }

        #endregion

        #region LegalStatus, Direction, Location

        // LegalStatus
        public PropertyLegalStatusPartRecord LegalStatus
        {
            get { return Property.As<PropertyPart>().LegalStatus; }
        }

        // Direction
        public DirectionPartRecord Direction
        {
            get { return Property.As<PropertyPart>().Direction; }
        }

        // Location
        public PropertyLocationPartRecord Location
        {
            get { return Property.As<PropertyPart>().Location; }
        }

        public string DisplayForLocation
        {
            get { return Property.As<PropertyPart>().DisplayForLocation; }
        }

        #endregion

        #region  Alley

        // Alley
        public string DisplayForAlley
        {
            get { return Property.As<PropertyPart>().DisplayForAlley; }
        }

        public string DisplayForTurns
        {
            get { return Property.As<PropertyPart>().DisplayForTurns; }
        }

        public int? AlleyTurns
        {
            get { return Property.As<PropertyPart>().AlleyTurns; }
        }

        public double? DistanceToStreet
        {
            get { return Property.As<PropertyPart>().DistanceToStreet; }
        }

        public double? AlleyWidth
        {
            get { return Property.As<PropertyPart>().AlleyWidth; }
        }

        public double? AlleyWidth1
        {
            get { return Property.As<PropertyPart>().AlleyWidth1; }
        }

        public double? AlleyWidth2
        {
            get { return Property.As<PropertyPart>().AlleyWidth2; }
        }

        public double? AlleyWidth3
        {
            get { return Property.As<PropertyPart>().AlleyWidth3; }
        }

        public double? AlleyWidth4
        {
            get { return Property.As<PropertyPart>().AlleyWidth4; }
        }

        public double? AlleyWidth5
        {
            get { return Property.As<PropertyPart>().AlleyWidth5; }
        }

        public double? AlleyWidth6
        {
            get { return Property.As<PropertyPart>().AlleyWidth6; }
        }

        public double? AlleyWidth7
        {
            get { return Property.As<PropertyPart>().AlleyWidth7; }
        }

        public double? AlleyWidth8
        {
            get { return Property.As<PropertyPart>().AlleyWidth8; }
        }

        public double? AlleyWidth9
        {
            get { return Property.As<PropertyPart>().AlleyWidth9; }
        }

        public double? StreetWidth
        {
            get { return Property.As<PropertyPart>().StreetWidth; }
        }

        #endregion

        #region Area

        // AreaTotal
        public string DisplayForAreaTotal
        {
            get { return Property.As<PropertyPart>().DisplayForAreaTotal; }
        }

        public double? AreaTotal
        {
            get { return Property.As<PropertyPart>().AreaTotal; }
        }

        public double? AreaTotalWidth
        {
            get { return Property.As<PropertyPart>().AreaTotalWidth; }
        }

        public double? AreaTotalLength
        {
            get { return Property.As<PropertyPart>().AreaTotalLength; }
        }

        public double? AreaTotalBackWidth
        {
            get { return Property.As<PropertyPart>().AreaTotalBackWidth; }
        }

        // AreaLegal
        public string DisplayForAreaLegal
        {
            get { return Property.As<PropertyPart>().DisplayForAreaLegal; }
        }

        public double? AreaLegal
        {
            get { return Property.As<PropertyPart>().AreaLegal; }
        }

        public double? AreaLegalWidth
        {
            get { return Property.As<PropertyPart>().AreaLegalWidth; }
        }

        public double? AreaLegalLength
        {
            get { return Property.As<PropertyPart>().AreaLegalLength; }
        }

        public double? AreaLegalBackWidth
        {
            get { return Property.As<PropertyPart>().AreaLegalBackWidth; }
        }

        public double? AreaIlegalRecognized
        {
            get { return Property.As<PropertyPart>().AreaIlegalRecognized; }
        }

        public double? AreaIlegalNotRecognized
        {
            get { return Property.As<PropertyPart>().AreaIlegalNotRecognized; }
        }

        // AreaResidential
        public double? AreaResidential
        {
            get { return Property.As<PropertyPart>().AreaResidential; }
        }

        #endregion

        #region Construction

        // Construction

        public double? AreaConstruction
        {
            get { return Property.As<PropertyPart>().AreaConstruction; }
        }

        public double? AreaConstructionFloor
        {
            get { return Property.As<PropertyPart>().AreaConstructionFloor; }
        }

        public double? AreaUsable
        {
            get { return Property.As<PropertyPart>().AreaUsable; }
        }

        public double? Floors
        {
            get { return Property.As<PropertyPart>().Floors; }
        }

        public int? Bedrooms
        {
            get { return Property.As<PropertyPart>().Bedrooms; }
        }

        public int? Livingrooms
        {
            get { return Property.As<PropertyPart>().Livingrooms; }
        }

        public int? Bathrooms
        {
            get { return Property.As<PropertyPart>().Bathrooms; }
        }

        public int? Balconies
        {
            get { return Property.As<PropertyPart>().Balconies; }
        }

        public PropertyInteriorPartRecord Interior
        {
            get { return Property.As<PropertyPart>().Interior; }
        }

        public double? RemainingValue
        {
            get { return Property.As<PropertyPart>().RemainingValue; }
        }

        public bool HaveBasement
        {
            get { return Property.As<PropertyPart>().HaveBasement; }
        }

        public bool HaveMezzanine
        {
            get { return Property.As<PropertyPart>().HaveMezzanine; }
        }

        public bool HaveElevator
        {
            get { return Property.As<PropertyPart>().HaveElevator; }
        }

        public bool HaveSwimmingPool
        {
            get { return Property.As<PropertyPart>().HaveSwimmingPool; }
        }

        public bool HaveGarage
        {
            get { return Property.As<PropertyPart>().HaveGarage; }
        }

        public bool HaveGarden
        {
            get { return Property.As<PropertyPart>().HaveGarden; }
        }

        public bool HaveTerrace
        {
            get { return Property.As<PropertyPart>().HaveTerrace; }
        }

        public bool HaveSkylight
        {
            get { return Property.As<PropertyPart>().HaveSkylight; }
        }

        #endregion

        #region Advantages, DisAdvantages

        // Advantages
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // OtherAdvantages
        public double? OtherAdvantages
        {
            get { return Property.As<PropertyPart>().OtherAdvantages; }
        }

        // OtherAdvantagesDesc
        public string OtherAdvantagesDesc
        {
            get { return Property.As<PropertyPart>().OtherAdvantagesDesc; }
        }

        // DisAdvantages
        public IList<PropertyAdvantageEntry> DisAdvantages { get; set; }

        // OtherDisAdvantages
        public double? OtherDisAdvantages
        {
            get { return Property.As<PropertyPart>().OtherDisAdvantages; }
        }

        // OtherDisAdvantagesDesc
        public string OtherDisAdvantagesDesc
        {
            get { return Property.As<PropertyPart>().OtherDisAdvantagesDesc; }
        }

        #endregion

        #region Contact

        // Contact

        public string ContactName
        {
            get { return Property.As<PropertyPart>().ContactName; }
        }

        public string ContactPhone
        {
            get { return Property.As<PropertyPart>().ContactPhone; }
        }

        public string ContactPhoneToDisplay
        {
            get { return Property.As<PropertyPart>().ContactPhoneToDisplay; }
        }

        public string ContactAddress
        {
            get { return Property.As<PropertyPart>().ContactAddress; }
        }

        public string ContactEmail
        {
            get { return Property.As<PropertyPart>().ContactEmail; }
        }

        public bool PublishContact
        {
            get { return Property.As<PropertyPart>().PublishContact; }
        }

        #endregion

        #region Price

        // Price

        public double? PriceProposed
        {
            get { return Property.As<PropertyPart>().PriceProposed; }
        }

        // ReSharper disable InconsistentNaming
        public double? PriceProposedInVND
        {
            get { return Property.As<PropertyPart>().PriceProposedInVND; }
        }

        public double? PriceEstimatedInVND
        {
            get { return Property.As<PropertyPart>().PriceEstimatedInVND; }
        }
        // ReSharper restore InconsistentNaming

        public double? PriceEstimatedByStaff
        {
            get { return Property.As<PropertyPart>().PriceEstimatedByStaff; }
        }

        public double? PriceEstimatedRatingPoint
        {
            get { return Property.As<PropertyPart>().PriceEstimatedRatingPoint; }
        }

        public string PriceEstimatedComment
        {
            get { return Property.As<PropertyPart>().PriceEstimatedComment; }
        }

        public PaymentMethodPartRecord PaymentMethod
        {
            get { return Property.As<PropertyPart>().PaymentMethod; }
        }

        public PaymentUnitPartRecord PaymentUnit
        {
            get { return Property.As<PropertyPart>().PaymentUnit; }
        }

        public bool PriceNegotiable
        {
            get { return Property.As<PropertyPart>().PriceNegotiable; }
        }

        #endregion

        #region User

        // CreatedDate
        public DateTime CreatedDate
        {
            get { return Property.As<PropertyPart>().CreatedDate; }
        }

        // CreatedUser
        public UserPartRecord CreatedUser
        {
            get { return Property.As<PropertyPart>().CreatedUser; }
        }

        // LastUpdatedDate
        public DateTime LastUpdatedDate
        {
            get { return Property.As<PropertyPart>().LastUpdatedDate; }
        }

        // LastUpdatedUser
        public UserPartRecord LastUpdatedUser
        {
            get { return Property.As<PropertyPart>().LastUpdatedUser; }
        }

        // FirstInfoFromUser
        public UserPartRecord FirstInfoFromUser
        {
            get { return Property.As<PropertyPart>().FirstInfoFromUser; }
        }

        // LastInfoFromUser
        public UserPartRecord LastInfoFromUser
        {
            get { return Property.As<PropertyPart>().LastInfoFromUser; }
        }

        // LastExportedDate
        public DateTime? LastExportedDate
        {
            get { return Property.As<PropertyPart>().LastExportedDate; }
        }

        // LastExportedUser
        public UserPartRecord LastExportedUser
        {
            get { return Property.As<PropertyPart>().LastExportedUser; }
        }

        #endregion

        #region Content

        // Ads Content

        public string Title
        {
            get { return Property.As<PropertyPart>().Title; }
        }

        public string Content
        {
            get { return Property.As<PropertyPart>().Content; }
        }

        public string Note
        {
            get { return Property.As<PropertyPart>().Note; }
        }

        #endregion

        #region Apartment

        public string ApartmentNumber
        {
            get { return Property.As<PropertyPart>().ApartmentNumber; }
        }

        public int? ApartmentFloorTh
        {
            get { return Property.As<PropertyPart>().ApartmentFloorTh; }
        }

        public int? ApartmentFloors
        {
            get { return Property.As<PropertyPart>().ApartmentFloors; }
        }

        public int? ApartmentTradeFloors
        {
            get { return Property.As<PropertyPart>().ApartmentTradeFloors; }
        }

        public int? ApartmentElevators
        {
            get { return Property.As<PropertyPart>().ApartmentElevators; }
        }

        public int? ApartmentBasements
        {
            get { return Property.As<PropertyPart>().ApartmentBasements; }
        }

        // ApartmentAdvantages
        public IList<PropertyAdvantageEntry> ApartmentAdvantages { get; set; }

        // ApartmentInteriorAdvantages
        public IList<PropertyAdvantageEntry> ApartmentInteriorAdvantages { get; set; }

        #endregion

        #region Related Content

        // Files
        public virtual IEnumerable<PropertyFilePart> Files { get; set; }

        // Customers
        public virtual IEnumerable<CustomerPropertyRecord> Customers { get; set; }

        #endregion
    }

    public class PropertyCreateViewModel
    {
        public bool EnableSetAdsGoodDeal { get; set; }
        // ReSharper disable InconsistentNaming
        public bool EnableSetAdsVIP { get; set; }
        // ReSharper restore InconsistentNaming
        public bool EnableSetAdsHighlight { get; set; }

        public IContent Property { get; set; }

        public string ReturnUrl { get; set; }

        public int? Id { get; set; }

        public string YoutubeId { get; set; }
        public bool YoutubePublish { get; set; }
        public int? SeqOrder { get; set; }

        public bool IsManageAddAdsPayment { get; set; }

        // Accept post facebook
        //public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        //public bool IsPropertyExchange { get; set; }

        #region AdsType

        // AdsType
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // Published
        public bool Published { get; set; }
        public DateTime? AdsExpirationDate { get; set; }
        public ExpirationDate AddAdsExpirationDate { get; set; }

        // AdsGoodDeal
        public bool AdsGoodDeal { get; set; }
        public bool AdsGoodDealRequest { get; set; }
        public DateTime? AdsGoodDealExpirationDate { get; set; }
        public ExpirationDate AddAdsGoodDealExpirationDate { get; set; }

        // AdsVIP
        // ReSharper disable InconsistentNaming
        public bool AdsVIP { get; set; }
        public bool AdsVIPRequest { get; set; }
        public DateTime? AdsVIPExpirationDate { get; set; }
        public ExpirationDate AddAdsVIPExpirationDate { get; set; }
        public AdsTypeVIP AdsTypeVIPId { get; set; }
        // ReSharper restore InconsistentNaming

        // AdsHighlight
        public bool AdsHighlight { get; set; }
        public bool AdsHighlightRequest { get; set; }
        public DateTime? AdsHighlightExpirationDate { get; set; }
        public ExpirationDate AddAdsHighlightExpirationDate { get; set; }

        #endregion

        #region Type

        // TypeGroup
        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Type
        [Required(ErrorMessage = "Vui lòng chọn Loại bất động sản.")]
        public int? TypeId { get; set; }
        public string TypeCssClass { get; set; }

        public IEnumerable<PropertyTypePartRecord> Types { get; set; }

        // TypeConstruction
        public int? TypeConstructionId { get; set; }
        public IEnumerable<PropertyTypeConstructionPartRecord> TypeConstructions { get; set; }

        #endregion

        #region Address

        // Address

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố.")]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận / Huyện.")]
        public int DistrictId { get; set; }

        public int? WardId { get; set; }
        public int? StreetId { get; set; }
        public int? ApartmentId { get; set; }
        public int? ApartmentBlockId { get; set; }

        [StringLength(255)]
        public string AddressNumber { get; set; }

        [StringLength(255)]
        public string AddressCorner { get; set; }

        public int AlleyNumber { get; set; }
        public bool PublishAddress { get; set; }

        [StringLength(255)]
        public string OtherProvinceName { get; set; }

        [StringLength(255)]
        public string OtherDistrictName { get; set; }

        [StringLength(255)]
        public string OtherWardName { get; set; }

        [StringLength(255)]
        public string OtherStreetName { get; set; }

        [StringLength(255)]
        public string OtherProjectName { get; set; }

        public bool ChkOtherProvinceName { get; set; }
        public bool ChkOtherDistrictName { get; set; }
        public bool ChkOtherWardName { get; set; }
        public bool ChkOtherStreetName { get; set; }
        public bool ChkOtherProjectName { get; set; }

        public List<SelectListItem> Provinces { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }
        public IEnumerable<LocationApartmentBlockPart> ApartmentBlocks { get; set; }

        #endregion

        #region LegalStatus, Direction

        // LegalStatus
        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }

        // Direction
        public int? DirectionId { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        #endregion

        #region Location, Alley

        // Location
        public string LocationCssClass { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        // Alley
        public int? AlleyTurns { get; set; }
        public double? DistanceToStreet { get; set; }
        public double? AlleyWidth { get; set; }
        public double? AlleyWidth1 { get; set; }
        public double? AlleyWidth2 { get; set; }
        public double? AlleyWidth3 { get; set; }
        public double? AlleyWidth4 { get; set; }
        public double? AlleyWidth5 { get; set; }
        public double? AlleyWidth6 { get; set; }
        public double? AlleyWidth7 { get; set; }
        public double? AlleyWidth8 { get; set; }
        public double? AlleyWidth9 { get; set; }
        public double? StreetWidth { get; set; }

        #endregion

        #region Area

        //// Area for filter only
        //public double? Area { get; set; }

        // AreaTotal
        public double? AreaTotal { get; set; }
        public double? AreaTotalWidth { get; set; }
        public double? AreaTotalLength { get; set; }
        public double? AreaTotalBackWidth { get; set; }

        // AreaLegal
        public double? AreaLegal { get; set; }
        public double? AreaLegalWidth { get; set; }
        public double? AreaLegalLength { get; set; }
        public double? AreaLegalBackWidth { get; set; }
        public double? AreaIlegalRecognized { get; set; }
        public double? AreaIlegalNotRecognized { get; set; }

        // AreaResidential
        public double? AreaResidential { get; set; }

        #endregion

        #region Construction

        // Construction
        public double? AreaConstruction { get; set; }
        public double? AreaConstructionFloor { get; set; }

        public double? Floors { get; set; }
        public double? FloorsCount { get; set; }
        public int? Bedrooms { get; set; }
        public int? Livingrooms { get; set; }
        public int? Bathrooms { get; set; }
        public int? Balconies { get; set; }

        public int? InteriorId { get; set; }
        public IEnumerable<PropertyInteriorPartRecord> Interiors { get; set; }

        public double? RemainingValue { get; set; }

        public bool HaveBasement { get; set; }
        public bool HaveMezzanine { get; set; }
        public bool HaveElevator { get; set; }
        public bool HaveSwimmingPool { get; set; }
        public bool HaveGarage { get; set; }
        public bool HaveGarden { get; set; }
        public bool HaveTerrace { get; set; }
        public bool HaveSkylight { get; set; }

        #endregion

        #region Advantages, DisAdvantages

        // Advantages
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // OtherAdvantages
        public double? OtherAdvantages { get; set; }

        // OtherAdvantagesDesc
        [StringLength(255)]
        public string OtherAdvantagesDesc { get; set; }

        // DisAdvantages
        public IList<PropertyAdvantageEntry> DisAdvantages { get; set; }

        // OtherDisAdvantages
        public double? OtherDisAdvantages { get; set; }

        // OtherDisAdvantagesDesc
        [StringLength(255)]
        public string OtherDisAdvantagesDesc { get; set; }

        #endregion

        #region Contact

        // Contact
        [StringLength(255)]
        public string ContactName { get; set; }

        [StringLength(255)]
        [Required(ErrorMessage = "Vui lòng nhập Thông tin liên hệ.")]
        public string ContactPhone { get; set; }

        [StringLength(255)]
        public string ContactPhoneToDisplay { get; set; }

        [StringLength(255)]
        public string ContactAddress { get; set; }

        [StringLength(255)]
        public string ContactEmail { get; set; }

        public bool PublishContact { get; set; }

        #endregion

        #region Price

        // Price
        public double? PriceProposed { get; set; }
        // ReSharper disable InconsistentNaming
        public double? PriceProposedInVND { get; set; }
        public double? PriceEstimatedInVND { get; set; }
        // ReSharper restore InconsistentNaming
        public double? PriceEstimatedByStaff { get; set; }

        public double? PriceEstimatedRatingPoint { get; set; }

        [StringLength(500)]
        public string PriceEstimatedComment { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        public bool PriceNegotiable { get; set; }

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

        public bool IsOwner { get; set; }
        public bool NoBroker { get; set; }
        public bool IsAuction { get; set; }
        public bool IsHighlights { get; set; }
        public bool IsAuthenticatedInfo { get; set; }

        #endregion

        #region Flag, Status

        // Flag & Status

        //public int StatusId { get; set; }
        public string StatusCssClass { get; set; }
        public IEnumerable<PropertyStatusPartRecord> Status { get; set; }
        public DateTime? StatusChangedDate { get; set; }
        public bool IsSoldByGroup { get; set; }

        public int FlagId { get; set; }
        public IEnumerable<PropertyFlagPartRecord> Flags { get; set; }
        public bool IsExcludeFromPriceEstimation { get; set; }

        #endregion

        #region User

        // User
        public DateTime CreatedDate { get; set; }
        public int CreatedUserId { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int LastUpdatedUserId { get; set; }
        public int FirstInfoFromUserId { get; set; }
        public int LastInfoFromUserId { get; set; }
        public DateTime? LastExportedDate { get; set; }
        public int? LastExportedUserId { get; set; }
        public IEnumerable<UserPartRecord> Users { get; set; }

        #endregion

        #region AdsContent

        // Ads Content
        [StringLength(255)]
        public string Title { get; set; }

        public string Content { get; set; }

        [StringLength(250, ErrorMessage = "Số ký tự tối đa là 250")]
        public string Note { get; set; }

        #endregion

        #region AreaUsable

        // AreaUsable
        public double? AreaUsable { get; set; }

        #endregion

        #region Apartment

        [StringLength(255)]
        public string ApartmentNumber { get; set; }

        public int? ApartmentFloorTh { get; set; }

        public int? ApartmentFloors { get; set; }
        public int? ApartmentTradeFloors { get; set; }
        public int? ApartmentElevators { get; set; }
        public int? ApartmentBasements { get; set; }

        // ApartmentAdvantages
        public IList<PropertyAdvantageEntry> ApartmentAdvantages { get; set; }

        // ApartmentInteriorAdvantages
        public IList<PropertyAdvantageEntry> ApartmentInteriorAdvantages { get; set; }

        #endregion
    }

    public class PropertyEditViewModel
    {
        public bool IsChanged { get; set; }
        public bool IsExternal { get; set; }

        public bool EnableEditProperty { get; set; }
        public bool EnableCopyPropertyToGroup { get; set; }
        public bool EnableCopyPropertyToAdsType { get; set; }

        public bool EnableEditStatus { get; set; }
        public bool EnableEditAddressNumber { get; set; }
        public bool EnableEditContactPhone { get; set; }
        public bool EnableEditImages { get; set; }

        public bool EnableSetAdsGoodDeal { get; set; }
        // ReSharper disable InconsistentNaming
        public bool EnableSetAdsVIP { get; set; }
        // ReSharper restore InconsistentNaming
        public bool EnableSetAdsHighlight { get; set; }

        public bool AcceptPostToFacebok { get; set; }
        public bool IsPropertyExchange { get; set; }
        //public bool ChkPropertyExchange { get; set; }

        public string ReturnUrl { get; set; }
        public int VisitedCount { get; set; }
        public PropertyExchangePartRecord PropertyExchange { get; set; }

        public int Id
        {
            get { return Property.As<PropertyPart>().Id; }
        }

        public string DisplayForUrl
        {
            get { return Property.As<PropertyPart>().DisplayForUrl; }
        }

        public string YoutubeId
        {
            get { return Property.As<PropertyPart>().YoutubeId; }
            set { Property.As<PropertyPart>().YoutubeId = value; }
        }

        public bool YoutubePublish
        {
            get { return Property.As<PropertyPart>().YoutubePublish; }
            set { Property.As<PropertyPart>().YoutubePublish = value; }
        }

        public int SeqOrder
        {
            get { return Property.As<PropertyPart>().SeqOrder; }
            set { Property.As<PropertyPart>().SeqOrder = value; }
        }

        public bool IsManageAddAdsPayment { get; set; }

        public IContent Property { get; set; }

        #region DEBUG

        // DEBUG

        public double DebugAreaLegal { get; set; }
        public double DebugFrontWidth { get; set; }
        public double DebugBackWidth { get; set; }
        public double DebugLength { get; set; }

        public double DebugAreaStandard { get; set; }
        public double DebugAreaExcess { get; set; }

        public double DebugAreaIlegalRecognized { get; set; }
        public double DebugAreaIlegalNotRecognized { get; set; }

        public string DebugAlleyUnitPrice { get; set; }
        public double DebugAlleyCoeff { get; set; }
        public double DebugLengthCoeff { get; set; }
        public double DebugWidthCoeff { get; set; }
        public double DebugAreaWidth { get; set; }

        public double DebugUnitPrice { get; set; }
        public double DebugUnitPriceEstimate { get; set; }
        public double DebugUnitPriceOnStreet { get; set; }

        public double DebugPriceHouseEstimated { get; set; }
        public double DebugPriceLandProposed { get; set; }
        public double DebugPriceLandEstimated { get; set; }
        public double DebugPriceChangedInPercent { get; set; }

        public double DebugPercent { get; set; }
        public string DebugPercentMsg { get; set; }

        public string DebugEstimationMsg { get; set; }
        public List<int> DebugEstimationList { get; set; }

        #endregion

        #region Apartment

        [StringLength(255)]
        public string ApartmentNumber
        {
            get { return Property.As<PropertyPart>().ApartmentNumber; }
            set { Property.As<PropertyPart>().ApartmentNumber = value; }
        }

        public int? ApartmentFloorTh
        {
            get { return Property.As<PropertyPart>().ApartmentFloorTh; }
            set { Property.As<PropertyPart>().ApartmentFloorTh = value; }
        }

        public int? ApartmentFloors
        {
            get { return Property.As<PropertyPart>().ApartmentFloors; }
            set { Property.As<PropertyPart>().ApartmentFloors = value; }
        }

        public int? ApartmentTradeFloors
        {
            get { return Property.As<PropertyPart>().ApartmentTradeFloors; }
            set { Property.As<PropertyPart>().ApartmentTradeFloors = value; }
        }

        public int? ApartmentElevators
        {
            get { return Property.As<PropertyPart>().ApartmentElevators; }
            set { Property.As<PropertyPart>().ApartmentElevators = value; }
        }

        public int? ApartmentBasements
        {
            get { return Property.As<PropertyPart>().ApartmentBasements; }
            set { Property.As<PropertyPart>().ApartmentBasements = value; }
        }

        // ApartmentAdvantages
        public IList<PropertyAdvantageEntry> ApartmentAdvantages { get; set; }

        // ApartmentInteriorAdvantages
        public IList<PropertyAdvantageEntry> ApartmentInteriorAdvantages { get; set; }

        #endregion

        #region AdsType

        // AdsType
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // Published
        public bool Published
        {
            get { return Property.As<PropertyPart>().Published; }
            set { Property.As<PropertyPart>().Published = value; }
        }

        public DateTime? AdsExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsExpirationDate; }
            set { Property.As<PropertyPart>().AdsExpirationDate = value; }
        }

        public ExpirationDate AddAdsExpirationDate { get; set; }

        // AdsGoodDeal
        public bool AdsGoodDeal
        {
            get { return Property.As<PropertyPart>().AdsGoodDeal; }
            set { Property.As<PropertyPart>().AdsGoodDeal = value; }
        }

        public bool AdsGoodDealRequest
        {
            get { return Property.As<PropertyPart>().AdsGoodDealRequest; }
            set { Property.As<PropertyPart>().AdsGoodDealRequest = value; }
        }

        public DateTime? AdsGoodDealExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsGoodDealExpirationDate; }
            set { Property.As<PropertyPart>().AdsGoodDealExpirationDate = value; }
        }

        public ExpirationDate AddAdsGoodDealExpirationDate { get; set; }

        // AdsVIP
        // ReSharper disable InconsistentNaming
        public bool AdsVIP
        {
            get { return Property.As<PropertyPart>().AdsVIP; }
            set { Property.As<PropertyPart>().AdsVIP = value; }
        }

        public bool AdsVIPRequest
        {
            get { return Property.As<PropertyPart>().AdsVIPRequest; }
            set { Property.As<PropertyPart>().AdsVIPRequest = value; }
        }

        public DateTime? AdsVIPExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsVIPExpirationDate; }
            set { Property.As<PropertyPart>().AdsVIPExpirationDate = value; }
        }

        public ExpirationDate AddAdsVIPExpirationDate { get; set; }
        public AdsTypeVIP AdsTypeVIPId { get; set; }
        // ReSharper restore InconsistentNaming

        // AdsHighlight
        public bool AdsHighlight
        {
            get { return Property.As<PropertyPart>().AdsHighlight; }
            set { Property.As<PropertyPart>().AdsHighlight = value; }
        }

        public bool AdsHighlightRequest
        {
            get { return Property.As<PropertyPart>().AdsHighlightRequest; }
            set { Property.As<PropertyPart>().AdsHighlightRequest = value; }
        }

        public DateTime? AdsHighlightExpirationDate
        {
            get { return Property.As<PropertyPart>().AdsHighlightExpirationDate; }
            set { Property.As<PropertyPart>().AdsHighlightExpirationDate = value; }
        }

        public ExpirationDate AddAdsHighlightExpirationDate { get; set; }

        #endregion

        #region Type

        // TypeGroup
        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Type
        [Required(ErrorMessage = "Vui lòng chọn Loại bất động sản.")]
        public int? TypeId { get; set; }

        public IEnumerable<PropertyTypePartRecord> Types { get; set; }

        // TypeConstruction
        public int? TypeConstructionId { get; set; }
        public IEnumerable<PropertyTypeConstructionPartRecord> TypeConstructions { get; set; }

        #endregion

        #region Address

        // Address
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố.")]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận / Huyện.")]
        public int DistrictId { get; set; }

        public int? WardId { get; set; }
        public int? StreetId { get; set; }
        public int? ApartmentId { get; set; }
        public int? ApartmentBlockId { get; set; }

        [StringLength(255)]
        public string AddressNumber
        {
            get { return Property.As<PropertyPart>().AddressNumber; }
            set { Property.As<PropertyPart>().AddressNumber = value; }
        }

        [StringLength(255)]
        public string AddressCorner
        {
            get { return Property.As<PropertyPart>().AddressCorner; }
            set { Property.As<PropertyPart>().AddressCorner = value; }
        }

        public int AlleyNumber
        {
            get { return Property.As<PropertyPart>().AlleyNumber; }
            set { Property.As<PropertyPart>().AlleyNumber = value; }
        }

        public bool PublishAddress
        {
            get { return Property.As<PropertyPart>().PublishAddress; }
            set { Property.As<PropertyPart>().PublishAddress = value; }
        }

        public string DisplayForAddress
        {
            get { return Property.As<PropertyPart>().DisplayForAddressForOwner; }
        }

        [StringLength(255)]
        public string OtherProvinceName
        {
            get { return Property.As<PropertyPart>().OtherProvinceName; }
            set { Property.As<PropertyPart>().OtherProvinceName = value; }
        }

        [StringLength(255)]
        public string OtherDistrictName
        {
            get { return Property.As<PropertyPart>().OtherDistrictName; }
            set { Property.As<PropertyPart>().OtherDistrictName = value; }
        }

        [StringLength(255)]
        public string OtherWardName
        {
            get { return Property.As<PropertyPart>().OtherWardName; }
            set { Property.As<PropertyPart>().OtherWardName = value; }
        }

        [StringLength(255)]
        public string OtherStreetName
        {
            get { return Property.As<PropertyPart>().OtherStreetName; }
            set { Property.As<PropertyPart>().OtherStreetName = value; }
        }

        [StringLength(255)]
        public string OtherProjectName
        {
            get { return Property.As<PropertyPart>().OtherProjectName; }
            set { Property.As<PropertyPart>().OtherProjectName = value; }
        }

        public bool ChkOtherProvinceName { get; set; }
        public bool ChkOtherDistrictName { get; set; }
        public bool ChkOtherWardName { get; set; }
        public bool ChkOtherStreetName { get; set; }
        public bool ChkOtherProjectName { get; set; }

        public List<SelectListItem> Provinces { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }
        public IEnumerable<LocationApartmentBlockPart> ApartmentBlocks { get; set; }

        #endregion

        #region LegalStatus, Direction

        // LegalStatus
        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }

        // Direction
        public int? DirectionId { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        #endregion

        #region Location, Alley

        // Location
        public string LocationCssClass { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        // Alley
        public int? AlleyTurns
        {
            get { return Property.As<PropertyPart>().AlleyTurns; }
            set { Property.As<PropertyPart>().AlleyTurns = value; }
        }

        public double? DistanceToStreet
        {
            get { return Property.As<PropertyPart>().DistanceToStreet; }
            set { Property.As<PropertyPart>().DistanceToStreet = value; }
        }

        public double? AlleyWidth
        {
            get { return Property.As<PropertyPart>().AlleyWidth; }
            set { Property.As<PropertyPart>().AlleyWidth = value; }
        }

        public double? AlleyWidth1
        {
            get { return Property.As<PropertyPart>().AlleyWidth1; }
            set { Property.As<PropertyPart>().AlleyWidth1 = value; }
        }

        public double? AlleyWidth2
        {
            get { return Property.As<PropertyPart>().AlleyWidth2; }
            set { Property.As<PropertyPart>().AlleyWidth2 = value; }
        }

        public double? AlleyWidth3
        {
            get { return Property.As<PropertyPart>().AlleyWidth3; }
            set { Property.As<PropertyPart>().AlleyWidth3 = value; }
        }

        public double? AlleyWidth4
        {
            get { return Property.As<PropertyPart>().AlleyWidth4; }
            set { Property.As<PropertyPart>().AlleyWidth4 = value; }
        }

        public double? AlleyWidth5
        {
            get { return Property.As<PropertyPart>().AlleyWidth5; }
            set { Property.As<PropertyPart>().AlleyWidth5 = value; }
        }

        public double? AlleyWidth6
        {
            get { return Property.As<PropertyPart>().AlleyWidth6; }
            set { Property.As<PropertyPart>().AlleyWidth6 = value; }
        }

        public double? AlleyWidth7
        {
            get { return Property.As<PropertyPart>().AlleyWidth7; }
            set { Property.As<PropertyPart>().AlleyWidth7 = value; }
        }

        public double? AlleyWidth8
        {
            get { return Property.As<PropertyPart>().AlleyWidth8; }
            set { Property.As<PropertyPart>().AlleyWidth8 = value; }
        }

        public double? AlleyWidth9
        {
            get { return Property.As<PropertyPart>().AlleyWidth9; }
            set { Property.As<PropertyPart>().AlleyWidth9 = value; }
        }

        public double? StreetWidth
        {
            get { return Property.As<PropertyPart>().StreetWidth; }
            set { Property.As<PropertyPart>().StreetWidth = value; }
        }

        #endregion

        #region Area

        //// Area for filter only
        //public double? Area
        //{
        //    get { return Property.As<PropertyPart>().Area; }
        //    set { Property.As<PropertyPart>().Area = value; }
        //}

        // AreaTotal
        public double? AreaTotal
        {
            get { return Property.As<PropertyPart>().AreaTotal; }
            set { Property.As<PropertyPart>().AreaTotal = value; }
        }

        public double? AreaTotalWidth
        {
            get { return Property.As<PropertyPart>().AreaTotalWidth; }
            set { Property.As<PropertyPart>().AreaTotalWidth = value; }
        }

        public double? AreaTotalLength
        {
            get { return Property.As<PropertyPart>().AreaTotalLength; }
            set { Property.As<PropertyPart>().AreaTotalLength = value; }
        }

        public double? AreaTotalBackWidth
        {
            get { return Property.As<PropertyPart>().AreaTotalBackWidth; }
            set { Property.As<PropertyPart>().AreaTotalBackWidth = value; }
        }

        // AreaLegal
        public double? AreaLegal
        {
            get { return Property.As<PropertyPart>().AreaLegal; }
            set { Property.As<PropertyPart>().AreaLegal = value; }
        }

        public double? AreaLegalWidth
        {
            get { return Property.As<PropertyPart>().AreaLegalWidth; }
            set { Property.As<PropertyPart>().AreaLegalWidth = value; }
        }

        public double? AreaLegalLength
        {
            get { return Property.As<PropertyPart>().AreaLegalLength; }
            set { Property.As<PropertyPart>().AreaLegalLength = value; }
        }

        public double? AreaLegalBackWidth
        {
            get { return Property.As<PropertyPart>().AreaLegalBackWidth; }
            set { Property.As<PropertyPart>().AreaLegalBackWidth = value; }
        }

        public double? AreaIlegalRecognized
        {
            get { return Property.As<PropertyPart>().AreaIlegalRecognized; }
            set { Property.As<PropertyPart>().AreaIlegalRecognized = value; }
        }

        public double? AreaIlegalNotRecognized
        {
            get { return Property.As<PropertyPart>().AreaIlegalNotRecognized; }
            set { Property.As<PropertyPart>().AreaIlegalNotRecognized = value; }
        }

        // AreaResidential
        public double? AreaResidential
        {
            get { return Property.As<PropertyPart>().AreaResidential; }
            set { Property.As<PropertyPart>().AreaResidential = value; }
        }

        #endregion

        #region Construction

        // Construction
        public double? AreaConstruction
        {
            get { return Property.As<PropertyPart>().AreaConstruction; }
            set { Property.As<PropertyPart>().AreaConstruction = value; }
        }

        public double? AreaConstructionFloor
        {
            get { return Property.As<PropertyPart>().AreaConstructionFloor; }
            set { Property.As<PropertyPart>().AreaConstructionFloor = value; }
        }

        // AreaUsable
        public double? AreaUsable
        {
            get { return Property.As<PropertyPart>().AreaUsable; }
            set { Property.As<PropertyPart>().AreaUsable = value; }
        }

        public double? Floors
        {
            get { return Property.As<PropertyPart>().Floors; }
            set { Property.As<PropertyPart>().Floors = value; }
        }

        public double? FloorsCount { get; set; }

        public int? Bedrooms
        {
            get { return Property.As<PropertyPart>().Bedrooms; }
            set { Property.As<PropertyPart>().Bedrooms = value; }
        }

        public int? Livingrooms
        {
            get { return Property.As<PropertyPart>().Livingrooms; }
            set { Property.As<PropertyPart>().Livingrooms = value; }
        }

        public int? Bathrooms
        {
            get { return Property.As<PropertyPart>().Bathrooms; }
            set { Property.As<PropertyPart>().Bathrooms = value; }
        }

        public int? Balconies
        {
            get { return Property.As<PropertyPart>().Balconies; }
            set { Property.As<PropertyPart>().Balconies = value; }
        }

        public int? InteriorId { get; set; }
        public IEnumerable<PropertyInteriorPartRecord> Interiors { get; set; }

        public double? RemainingValue
        {
            get { return Property.As<PropertyPart>().RemainingValue; }
            set { Property.As<PropertyPart>().RemainingValue = value; }
        }

        public bool HaveBasement
        {
            get { return Property.As<PropertyPart>().HaveBasement; }
            set { Property.As<PropertyPart>().HaveBasement = value; }
        }

        public bool HaveMezzanine
        {
            get { return Property.As<PropertyPart>().HaveMezzanine; }
            set { Property.As<PropertyPart>().HaveMezzanine = value; }
        }

        public bool HaveElevator
        {
            get { return Property.As<PropertyPart>().HaveElevator; }
            set { Property.As<PropertyPart>().HaveElevator = value; }
        }

        public bool HaveSwimmingPool
        {
            get { return Property.As<PropertyPart>().HaveSwimmingPool; }
            set { Property.As<PropertyPart>().HaveSwimmingPool = value; }
        }

        public bool HaveGarage
        {
            get { return Property.As<PropertyPart>().HaveGarage; }
            set { Property.As<PropertyPart>().HaveGarage = value; }
        }

        public bool HaveGarden
        {
            get { return Property.As<PropertyPart>().HaveGarden; }
            set { Property.As<PropertyPart>().HaveGarden = value; }
        }

        public bool HaveTerrace
        {
            get { return Property.As<PropertyPart>().HaveTerrace; }
            set { Property.As<PropertyPart>().HaveTerrace = value; }
        }

        public bool HaveSkylight
        {
            get { return Property.As<PropertyPart>().HaveSkylight; }
            set { Property.As<PropertyPart>().HaveSkylight = value; }
        }

        #endregion

        #region Advantages, DisAdvantages

        // Advantages
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // OtherAdvantages
        public double? OtherAdvantages
        {
            get { return Property.As<PropertyPart>().OtherAdvantages; }
            set { Property.As<PropertyPart>().OtherAdvantages = value; }
        }

        // OtherAdvantagesDesc
        [StringLength(255)]
        public string OtherAdvantagesDesc
        {
            get { return Property.As<PropertyPart>().OtherAdvantagesDesc; }
            set { Property.As<PropertyPart>().OtherAdvantagesDesc = value; }
        }

        // DisAdvantages
        public IList<PropertyAdvantageEntry> DisAdvantages { get; set; }

        // OtherDisAdvantages
        public double? OtherDisAdvantages
        {
            get { return Property.As<PropertyPart>().OtherDisAdvantages; }
            set { Property.As<PropertyPart>().OtherDisAdvantages = value; }
        }

        // OtherDisAdvantagesDesc
        [StringLength(255)]
        public string OtherDisAdvantagesDesc
        {
            get { return Property.As<PropertyPart>().OtherDisAdvantagesDesc; }
            set { Property.As<PropertyPart>().OtherDisAdvantagesDesc = value; }
        }

        #endregion

        #region Contact

        // Contact
        [StringLength(255)]
        public string ContactName
        {
            get { return Property.As<PropertyPart>().ContactName; }
            set { Property.As<PropertyPart>().ContactName = value; }
        }

        [StringLength(255)]
        [Required(ErrorMessage = "Vui lòng nhập Thông tin liên hệ.")]
        public string ContactPhone
        {
            get { return Property.As<PropertyPart>().ContactPhone; }
            set { Property.As<PropertyPart>().ContactPhone = value; }
        }

        [StringLength(255)]
        public string ContactPhoneToDisplay
        {
            get { return Property.As<PropertyPart>().ContactPhoneToDisplay; }
            set { Property.As<PropertyPart>().ContactPhoneToDisplay = value; }
        }

        [StringLength(255)]
        public string ContactAddress
        {
            get { return Property.As<PropertyPart>().ContactAddress; }
            set { Property.As<PropertyPart>().ContactAddress = value; }
        }

        [StringLength(255)]
        public string ContactEmail
        {
            get { return Property.As<PropertyPart>().ContactEmail; }
            set { Property.As<PropertyPart>().ContactEmail = value; }
        }

        public bool PublishContact
        {
            get { return Property.As<PropertyPart>().PublishContact; }
            set { Property.As<PropertyPart>().PublishContact = value; }
        }

        #endregion

        #region Price

        // Price
        public double? PriceProposed
        {
            get { return Property.As<PropertyPart>().PriceProposed; }
            set { Property.As<PropertyPart>().PriceProposed = value; }
        }

        // ReSharper disable InconsistentNaming
        public double? PriceProposedInVND
        {
            get { return Property.As<PropertyPart>().PriceProposedInVND; }
            set { Property.As<PropertyPart>().PriceProposedInVND = value; }
        }

        public double? PriceEstimatedInVND
        {
            get { return Property.As<PropertyPart>().PriceEstimatedInVND; }
            set { Property.As<PropertyPart>().PriceEstimatedInVND = value; }
        }
        // ReSharper restore InconsistentNaming

        public double? PriceEstimatedByStaff
        {
            get { return Property.As<PropertyPart>().PriceEstimatedByStaff; }
            set { Property.As<PropertyPart>().PriceEstimatedByStaff = value; }
        }

        public double? PriceEstimatedRatingPoint
        {
            get { return Property.As<PropertyPart>().PriceEstimatedRatingPoint; }
            set { Property.As<PropertyPart>().PriceEstimatedRatingPoint = value; }
        }

        [StringLength(500)]
        public string PriceEstimatedComment
        {
            get { return Property.As<PropertyPart>().PriceEstimatedComment; }
            set { Property.As<PropertyPart>().PriceEstimatedComment = value; }
        }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        public bool PriceNegotiable
        {
            get { return Property.As<PropertyPart>().PriceNegotiable; }
            set { Property.As<PropertyPart>().PriceNegotiable = value; }
        }

        // CopyToAdsType
        public string AdsTypeCssClassCopy { get; set; }
        public bool PublishedCopy { get; set; }
        public double? PriceProposedCopy { get; set; }
        public int PaymentMethodIdCopy { get; set; }
        public int PaymentUnitIdCopy { get; set; }

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

        public bool IsOwner
        {
            get { return Property.As<PropertyPart>().IsOwner; }
            set { Property.As<PropertyPart>().IsOwner = value; }
        }

        public bool NoBroker
        {
            get { return Property.As<PropertyPart>().NoBroker; }
            set { Property.As<PropertyPart>().NoBroker = value; }
        }

        public bool IsAuction
        {
            get { return Property.As<PropertyPart>().IsAuction; }
            set { Property.As<PropertyPart>().IsAuction = value; }
        }

        public bool IsHighlights
        {
            get { return Property.As<PropertyPart>().IsHighlights; }
            set { Property.As<PropertyPart>().IsHighlights = value; }
        }

        public bool IsAuthenticatedInfo
        {
            get { return Property.As<PropertyPart>().IsAuthenticatedInfo; }
            set { Property.As<PropertyPart>().IsAuthenticatedInfo = value; }
        }

        #endregion

        #region Flag, Status

        // Status
        //public int StatusId { get; set; }
        public string StatusCssClass { get; set; }
        public string StatusName { get; set; }
        public IEnumerable<PropertyStatusPartRecord> Status { get; set; }

        public DateTime? StatusChangedDate
        {
            get { return Property.As<PropertyPart>().StatusChangedDate; }
            set { Property.As<PropertyPart>().StatusChangedDate = value; }
        }

        // IsSoldByGroup
        public bool IsSoldByGroup
        {
            get { return Property.As<PropertyPart>().IsSoldByGroup; }
            set { Property.As<PropertyPart>().IsSoldByGroup = value; }
        }

        // Flag
        public int FlagId { get; set; }
        public string FlagCssClass { get; set; }
        public IEnumerable<PropertyFlagPartRecord> Flags { get; set; }

        public bool IsExcludeFromPriceEstimation
        {
            get { return Property.As<PropertyPart>().IsExcludeFromPriceEstimation; }
            set { Property.As<PropertyPart>().IsExcludeFromPriceEstimation = value; }
        }

        #endregion

        #region User

        public DateTime CreatedDate
        {
            get { return Property.As<PropertyPart>().CreatedDate; }
            set { Property.As<PropertyPart>().CreatedDate = value; }
        }

        public UserPartRecord CreatedUser
        {
            get { return Property.As<PropertyPart>().CreatedUser; }
        }

        public DateTime LastUpdatedDate
        {
            get { return Property.As<PropertyPart>().LastUpdatedDate; }
            set { Property.As<PropertyPart>().LastUpdatedDate = value; }
        }

        public UserPartRecord LastUpdatedUser
        {
            get { return Property.As<PropertyPart>().LastUpdatedUser; }
        }

        public UserPartRecord FirstInfoFromUser
        {
            get { return Property.As<PropertyPart>().FirstInfoFromUser; }
        }

        public UserPartRecord LastInfoFromUser
        {
            get { return Property.As<PropertyPart>().LastInfoFromUser; }
        }

        public DateTime? LastExportedDate
        {
            get { return Property.As<PropertyPart>().LastExportedDate; }
            set { Property.As<PropertyPart>().LastExportedDate = value; }
        }

        public UserPartRecord LastExportedUser
        {
            get { return Property.As<PropertyPart>().LastExportedUser; }
        }

        public UserGroupPartRecord UserGroup
        {
            get { return Property.As<PropertyPart>().UserGroup; }
        }

        public IEnumerable<UserPartRecord> Users { get; set; }
        public int LastInfoFromUserId { get; set; }

        #endregion

        #region AdsContent

        // AdsContent
        [StringLength(255)]
        public string Title
        {
            get { return Property.As<PropertyPart>().Title; }
            set { Property.As<PropertyPart>().Title = value; }
        }

        public string Content
        {
            get { return Property.As<PropertyPart>().Content; }
            set { Property.As<PropertyPart>().Content = value; }
        }

        [StringLength(250, ErrorMessage = "Số ký tự tối đa là 250")]
        public string Note
        {
            get { return Property.As<PropertyPart>().Note; }
            set { Property.As<PropertyPart>().Note = value; }
        }

        public bool UpdateMeta { get; set; }

        #endregion

        #region Related Content

        // Files
        public virtual IEnumerable<PropertyFilePart> Files { get; set; }

        // Customers
        public virtual IEnumerable<CustomerPropertyRecord> Customers { get; set; }

        #endregion
    }

    #endregion

    public class PropertyEstimateEntry
    {
        public double? PriceEstimatedInVND { get; set; }
        public string FlagCssClass { get; set; }

        #region DEBUG

        // DEBUG

        public double DebugAreaLegal { get; set; }
        public double DebugFrontWidth { get; set; }
        public double DebugBackWidth { get; set; }
        public double DebugLength { get; set; }

        public double DebugAreaStandard { get; set; }
        public double DebugAreaExcess { get; set; }

        public double DebugAreaIlegalRecognized { get; set; }
        public double DebugAreaIlegalNotRecognized { get; set; }

        public string DebugAlleyUnitPrice { get; set; }
        public double DebugAlleyCoeff { get; set; }
        public double DebugLengthCoeff { get; set; }
        public double DebugWidthCoeff { get; set; }
        public double DebugAreaWidth { get; set; }

        public double DebugUnitPrice { get; set; }
        public double DebugUnitPriceEstimate { get; set; }
        public double DebugUnitPriceOnStreet { get; set; }

        public double DebugPriceHouseEstimated { get; set; }
        public double DebugPriceLandProposed { get; set; }
        public double DebugPriceLandEstimated { get; set; }
        public double DebugPriceChangedInPercent { get; set; }

        public double DebugPercent { get; set; }
        public string DebugPercentMsg { get; set; }

        public string DebugEstimationMsg { get; set; }
        public List<int> DebugEstimationList { get; set; }

        #endregion

    }
}