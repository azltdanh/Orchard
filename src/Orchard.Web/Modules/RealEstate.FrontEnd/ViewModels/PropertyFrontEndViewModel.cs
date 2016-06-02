using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.ViewModels;
using System.Web.Mvc;

namespace RealEstate.FrontEnd.ViewModels
{

    #region INDEX

    public class CustomerRequirementFilterViewModel
    {
        public string AdsTypeCssClass { get; set; }
        public string ReturnStatus { get; set; }
    }

    public class UserPropertyIndexViewModel
    {
        public IList<UserPropertyEntry> Properties { get; set; }
        public UserPropertyIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class UserPropertyEntry
    {
        public PropertyPart Property { get; set; }
        public bool IsChecked { get; set; }
        public PropertyExchangePartRecord PropertyExchange { get; set; }
    }

    public class UserPropertyIndexOptions
    {
        public string ReturnStatus { get; set; }
        public string ReturnAdstype { get; set; }
        public string ReturnUrl { get; set; }

        public string Search { get; set; }
        public UserPropertyOrder Order { get; set; }
        public PropertyFilter Filter { get; set; }
        public PropertyBulkAction BulkAction { get; set; }

        public DateTime NeedUpdateDate { get; set; }

        public string List { get; set; }
        public int[] SelectedIds { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public int? WardId { get; set; }
        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public int? StreetId { get; set; }
        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
        public string AddressNumber { get; set; }
        public string AddressCorner { get; set; }

        public double? MinPriceProposedInVND { get; set; }
        public double? MaxPriceProposedInVND { get; set; }
        public bool ShowEstimation { get; set; }

        public int? StatusId { get; set; }
        public IEnumerable<PropertyStatusPartRecord> Status { get; set; }
        public int? FlagId { get; set; }
        public int[] FlagIds { get; set; }
        public IEnumerable<PropertyFlagPartRecord> Flags { get; set; }

        public bool ShowAdsOnline { get; set; }
        public bool ShowAdsNewspaper { get; set; }
        public bool ShowNeedUpdate { get; set; }
        public bool ShowExcludedInEstimation { get; set; }
        public bool ShowIncludedInEstimation { get; set; }

        public int? TypeId { get; set; }
        public IEnumerable<PropertyTypePartRecord> Types { get; set; }
        public int? TypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }
        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }
        public string ContactPhone { get; set; }
        public int? FirstInfoFromUserId { get; set; }
        public IEnumerable<UserPartRecord> FirstInfoFromUsers { get; set; }

        public double? MinAreaTotal { get; set; }
        public double? MaxAreaTotal { get; set; }

        public double? MinAreaTotalWidth { get; set; }
        public double? MaxAreaTotalWidth { get; set; }

        public double? MinAreaTotalLength { get; set; }
        public double? MaxAreaTotalLength { get; set; }

        public int? DirectionId { get; set; }
        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }

        public double? MinAlleyTurns { get; set; }
        public double? MaxAlleyTurns { get; set; }

        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        public int? InteriorId { get; set; }
        public IEnumerable<PropertyInteriorPartRecord> Interiors { get; set; }

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }

        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }

        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        public DateTime? AdsOnlineFrom { get; set; }
        public DateTime? AdsOnlineTo { get; set; }

        public DateTime? AdsNewspaperFrom { get; set; }
        public DateTime? AdsNewspaperTo { get; set; }

        public string CreatedFrom { get; set; }
        public string CreatedTo { get; set; }

        public int? CreatedUserId { get; set; }
        public IEnumerable<UserPartRecord> CreatedUsers { get; set; }

        public string LastUpdatedFrom { get; set; }
        public string LastUpdatedTo { get; set; }

        public int? LastUpdatedUserId { get; set; }
        public IEnumerable<UserPartRecord> LastUpdatedUsers { get; set; }

        public string LastExportedFrom { get; set; }
        public string LastExportedTo { get; set; }

        public int? LastExportedUserId { get; set; }
        public IEnumerable<UserPartRecord> LastExportedUsers { get; set; }

        // Advantages
        public int[] AdvantageIds { get; set; }
        public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // DisAdvantages
        public int[] DisAdvantageIds { get; set; }
        public IList<PropertyAdvantageEntry> DisAdvantages { get; set; }

        // Advantage
        public bool AdvCornerStreet { get; set; }
        public bool AdvCornerStreetAlley { get; set; }
        public bool AdvCornerAlley { get; set; }
        public bool AdvCornerAlleySmall { get; set; }
        public bool AdvDoubleFront { get; set; }
        public bool AdvNearSuperMarket { get; set; }
        public bool AdvNearTradeCenter { get; set; }
        public bool AdvNearPark { get; set; }
        public bool AdvSecurityArea { get; set; }
        public bool AdvLuxuryResidential { get; set; }

        // DisAdvantage
        public bool DAdvFacingAlley { get; set; }
        public bool DAdvFacingTemple { get; set; }
        public bool DAdvFacingChurch { get; set; }
        public bool DadvFacingFuneral { get; set; }
        public bool DadvUnderBridge { get; set; }
        public bool DAdvFacingDrain { get; set; }
        public bool DAdvFacingBigTree { get; set; }
        public bool DAdvFacingElectricityCylindrical { get; set; }
        public bool DAdvUnderHighVoltageLines { get; set; }
        public bool DadvShareWall { get; set; }
        public bool DAdvBuildingHeightRestriction { get; set; }
        public bool DAdvPlanningSuspended { get; set; }
        public bool DAdvComplexSecurityArea { get; set; }

        public int? AdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }
        public bool AdsVIP { get; set; }
        public bool AdsNormal { get; set; }
        public bool flagAjax { get; set; }
    }

    public enum UserPropertyOrder
    {
        LastUpdatedDate,
        AddressNumber,
        PriceProposedInVND
    }

    public enum PropertyFilter
    {
        All
    }

    public enum PropertyBulkAction
    {
        None,
        Publish,
        UnPublish,
        AdsOnline,
        RemoveAdsOnline,
        AdsNewspaper,
        RemoveAdsNewspaper,
        AddToEstimation,
        RemoveFromEstimation,
        Listing,
        Delete,
        Export,
    }

    #endregion

    #region HouseViewModel

    public class SelectListFrontEnd
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string CssClass { get; set; }
    }

    public class PropertyFrontEndCreateBaseViewModel
    {
        public string ReturnUrl { get; set; }

        // AdsType
        [Required(ErrorMessage = "Vui lòng chọn Loại giao dịch.")]
        public string AdsTypeCssClass { get; set; }

        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // TypeGroup
        [Required(ErrorMessage = "Vui lòng chọn Nhóm BĐS.")]
        public string TypeGroupCssClass { get; set; }

        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Type
        [Required(ErrorMessage = "Vui lòng chọn Loại BĐS.")]
        public int? TypeId { get; set; }

        public IEnumerable<PropertyTypePartRecord> Types { get; set; }

        //Use for REST API
        public int? UserId { get; set; }
        public int? DomainGroupId { get; set; }

        public IContent Property { get; set; }
    }

    public class PropertyFrontEndEditViewModel
    {
        public string ReturnUrl { get; set; }
        public bool IsPropertyExchange { get; set; }
        //public bool ChkPropertyExchange { get; set; }

        public int Id
        {
            get { return Property.Id; }
        }

        // Files
        public virtual IEnumerable<PropertyFilePart> Files { get; set; }

        // Customers
        public virtual IEnumerable<CustomerPropertyRecord> Customers { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        //Use for REST API
        public int? UserId { get; set; }
        public int? DomainGroupId { get; set; }

        public IContent Property { get; set; }

        #region Type

        // TypeGroup
        [Required(ErrorMessage = "Vui lòng chọn Nhóm BĐS.")]
        public string TypeGroupCssClass { get; set; }

        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Type
        [Required(ErrorMessage = "Vui lòng chọn Loại BĐS.")]
        public int? TypeId { get; set; }

        public IEnumerable<PropertyTypePartRecord> Types { get; set; }

        // TypeConstruction
        public int? TypeConstructionId { get; set; }
        public IEnumerable<PropertyTypeConstructionPartRecord> TypeConstructions { get; set; }

        #endregion

        #region Address

        // Address

        public int? ProvinceId { get; set; }

        public LocationProvincePartRecord Province
        {
            get { return Property.As<PropertyPart>().Province; }
        }

        public int? DistrictId { get; set; }

        public LocationDistrictPartRecord District
        {
            get { return Property.As<PropertyPart>().District; }
        }

        public int? WardId { get; set; }

        public LocationWardPartRecord Ward
        {
            get { return Property.As<PropertyPart>().Ward; }
        }

        public int? StreetId { get; set; }

        public LocationStreetPartRecord Street
        {
            get { return Property.As<PropertyPart>().Street; }
        }

        public int? ApartmentId { get; set; }

        public string DisplayForAddress
        {
            get { return Property.As<PropertyPart>().DisplayForAddress; }
        }

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

        //PublishAddress
        public bool PublishAddress
        {
            get { return Property.As<PropertyPart>().PublishAddress; }
            set { Property.As<PropertyPart>().PublishAddress = value; }
        }

        public bool UnPublishAddress { get; set; }

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

        public bool ChkOtherProvinceName
        {
            get
            {
                return Property.As<PropertyPart>().Province == null &&
                       !string.IsNullOrEmpty(Property.As<PropertyPart>().OtherProvinceName);
            }
        }

        public bool ChkOtherDistrictName
        {
            get
            {
                return Property.As<PropertyPart>().District == null &&
                       !string.IsNullOrEmpty(Property.As<PropertyPart>().OtherDistrictName);
            }
        }

        public bool ChkOtherWardName
        {
            get
            {
                return Property.As<PropertyPart>().Ward == null &&
                       !string.IsNullOrEmpty(Property.As<PropertyPart>().OtherWardName);
            }
        }

        public bool ChkOtherStreetName
        {
            get
            {
                return Property.As<PropertyPart>().Street == null &&
                       !string.IsNullOrEmpty(Property.As<PropertyPart>().OtherStreetName);
            }
        }

        public bool ChkOtherProjectName
        {
            get
            {
                return Property.As<PropertyPart>().Apartment == null &&
                       !string.IsNullOrEmpty(Property.As<PropertyPart>().OtherProjectName);
            }
        }

        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        #endregion

        #region LegalStatus, Direction, Location

        // LegalStatus
        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }

        // Direction
        public int? DirectionId { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        // Location
        public string LocationCssClass { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        #endregion

        #region Alley

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

        #endregion

        #region Construction

        // AreaUsable
        public double? AreaUsable
        {
            get { return Property.As<PropertyPart>().AreaUsable; }
            set { Property.As<PropertyPart>().AreaUsable = value; }
        }

        // AreaResidential
        public double? AreaResidential
        {
            get { return Property.As<PropertyPart>().AreaResidential; }
            set { Property.As<PropertyPart>().AreaResidential = value; }
        }

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

        #region Advantages,DisAdvantages

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
        public string ContactPhone
        {
            get { return Property.As<PropertyPart>().ContactPhone; }
            set { Property.As<PropertyPart>().ContactPhone = value; }
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

        #endregion

        #region IsOwner, NoBroker, IsAuction

        // IsOwner
        public bool IsOwner
        {
            get { return Property.As<PropertyPart>().IsOwner; }
            set { Property.As<PropertyPart>().IsOwner = value; }
        }

        // NoBroker
        public bool NoBroker
        {
            get { return Property.As<PropertyPart>().NoBroker; }
            set { Property.As<PropertyPart>().NoBroker = value; }
        }

        // IsAuction
        public bool IsAuction
        {
            get { return Property.As<PropertyPart>().IsAuction; }
            set { Property.As<PropertyPart>().IsAuction = value; }
        }

        #endregion

        #region Price

        // Price

        public double? PriceProposed
        {
            get { return Property.As<PropertyPart>().PriceProposed; }
            set { Property.As<PropertyPart>().PriceProposed = value; }
        }

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

        public bool IsEstimateable { get; set; }
        public bool SubmitEstimate { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        #endregion

        #region User

        // User
        public DateTime CreatedDate
        {
            get { return Property.As<PropertyPart>().CreatedDate; }
            set { Property.As<PropertyPart>().CreatedDate = value; }
        }

        public DateTime LastUpdatedDate
        {
            get { return Property.As<PropertyPart>().LastUpdatedDate; }
            set { Property.As<PropertyPart>().LastUpdatedDate = value; }
        }

        #endregion

        #region Ads Content

        // AdsType
        [Required(ErrorMessage = "Vui lòng chọn Loại giao dịch.")]
        public string AdsTypeCssClass { get; set; }

        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // Published
        public bool Published
        {
            get { return Property.As<PropertyPart>().Published; }
            set { Property.As<PropertyPart>().Published = value; }
        }

        //AdsExpirationDate
        public int? AdsExpirationDateValue { get; set; }

        public bool AdsGoodDealRequest
        {
            get { return Property.As<PropertyPart>().AdsGoodDealRequest; }
            set { Property.As<PropertyPart>().AdsGoodDealRequest = value; }
        }

        public bool AdsVIPRequest
        {
            get { return Property.As<PropertyPart>().AdsVIPRequest; }
            set { Property.As<PropertyPart>().AdsVIPRequest = value; }
        }

        public int AdsTypeVIP { get; set; }
        public bool IsAdsVIP { get; set; }

        public bool AdsHighlightRequest
        {
            get { return Property.As<PropertyPart>().AdsHighlightRequest; }
            set { Property.As<PropertyPart>().AdsHighlightRequest = value; }
        }

        // Ads Content

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

        [StringLength(255)]
        public string Note
        {
            get { return Property.As<PropertyPart>().Note; }
            set { Property.As<PropertyPart>().Note = value; }
        }

        public string StatusCssClass
        {
            get { return Property.As<PropertyPart>().Status.CssClass; }
        }

        public bool IsRefresh
        {
            get { return Property.As<PropertyPart>().IsRefresh; }
        }

        public string DateVipFrom { get; set; }
        public string DateVipTo { get; set; }
        public long Amount { get; set; }
        public string AmountVND { get; set; }
        public long[] UnitArray { get; set; }
        public int PostingDates { get; set; }

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

        // Apartment Advantages
        public IList<PropertyAdvantageEntry> ApartmentAdvantages { get; set; }

        // Apartment Interior Advantages
        public IList<PropertyAdvantageEntry> ApartmentInteriorAdvantages { get; set; }

        #endregion
    }

    #endregion

    #region CustomerViewModel

    public class CustomerFrontEndCreateViewModel
    {
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        public bool ChkOtherProjectName { get; set; }

        [StringLength(255)]
        public string OtherProjectName { get; set; }

        public PropertyDisplayApartmentFloorTh ApartmentFloorThRange { get; set; }

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        public PropertyDisplayLocation AlleyTurnsRange { get; set; }

        public int? MinAlleyTurns { get; set; }
        public int? MaxAlleyTurns { get; set; }

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }

        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        public double? MinArea { get; set; }
        public double? MaxArea { get; set; }

        public double? MinWidth { get; set; }
        public double? MaxWidth { get; set; }

        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }

        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }

        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }

        public string PaymentMethodCssClass { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        [StringLength(255)]
        public string Note { get; set; }

        public int? AdsExpirationDateValue { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        public IList<CustomerPurposeEntry> Purposes { get; set; }

        public string ReturnUrl { get; set; }


        public IContent Customer { get; set; }
    }

    public class CustomerFrontEndEditViewModel
    {
        public string AdsTypeCssClass { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public string TypeGroupCssClass { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        public bool ChkOtherProjectName { get; set; }

        [StringLength(255)]
        public string OtherProjectName { get; set; }

        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public PropertyDisplayApartmentFloorTh ApartmentFloorThRange { get; set; }

        public int? LocationId { get; set; }
        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        public PropertyDisplayLocation AlleyTurnsRange { get; set; }

        public int? MinAlleyTurns { get; set; }
        public int? MaxAlleyTurns { get; set; }

        public double? MinAlleyWidth { get; set; }
        public double? MaxAlleyWidth { get; set; }

        public double? MinDistanceToStreet { get; set; }
        public double? MaxDistanceToStreet { get; set; }

        public double? MinArea { get; set; }
        public double? MaxArea { get; set; }

        public double? MinWidth { get; set; }
        public double? MaxWidth { get; set; }

        public double? MinLength { get; set; }
        public double? MaxLength { get; set; }

        public double? MinFloors { get; set; }
        public double? MaxFloors { get; set; }

        public int? MinBedrooms { get; set; }
        public int? MaxBedrooms { get; set; }

        public int? MinBathrooms { get; set; }
        public int? MaxBathrooms { get; set; }

        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }

        public string PaymentMethodCssClass { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        [StringLength(255)]
        public string Note { get; set; }

        public int? AdsExpirationDateValue { get; set; }
        public DateTime? AdsExpirationDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        public IList<CustomerPurposeEntry> Purposes { get; set; }

        public string ReturnUrl { get; set; }


        public IContent Customer { get; set; }
    }

    public class CustomerDetailViewModel
    {
        public CustomerPart Customer { get; set; }

        public PropertyExchangePartRecord PropertyExchange {get; set;}

        public List<string> DisplayPhone { get; set; }

        public IList<CustomerPurposeEntry> Purposes { get; set; }

        public IList<CustomerRequirementEntry> Requirements { get; set; }

        public IEnumerable<PropertyDisplayEntry> Properties { get; set; }

        public dynamic Pager { get; set; }
    }

    #endregion
    
}