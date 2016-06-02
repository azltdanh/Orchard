using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.ViewModels
{
    public class EstimateCreateViewModel
    {
        #region Type

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

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố.")]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận / Huyện.")]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường / Xã.")]
        public int WardId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Đường / Phố.")]
        public int StreetId { get; set; }

        public int ApartmentId { get; set; }


        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        [StringLength(255)]
        [Required(ErrorMessage = "Vui lòng nhập Số nhà.")]
        public string AddressNumber { get; set; }

        [StringLength(255)]
        public string AddressCorner { get; set; }

        [StringLength(255)]
        public string ApartmentNumber { get; set; }

        public int AlleyNumber { get; set; }

        #endregion

        #region LegalStatus, Direction, Location

        // LegalStatus

        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }

        // Direction

        public int? DirectionId { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        // Location

        [Required(ErrorMessage = "Vui lòng chọn Vị trí BĐS.")]
        public string LocationCssClass { get; set; }

        public IEnumerable<PropertyLocationPartRecord> Locations { get; set; }

        #endregion

        #region Alley

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

        // AreaTotal
        public double? AreaTotal { get; set; }

        //[Required(ErrorMessage = "Vui lòng nhập Chiều ngang mặt tiền diện tích khuôn viên.")]
        public double? AreaTotalWidth { get; set; }

        //[Required(ErrorMessage = "Vui lòng nhập Chiều sâu diện tích khuôn viên.")]
        public double? AreaTotalLength { get; set; }

        public double? AreaTotalBackWidth { get; set; }

        // AreaLegal
        public double? AreaLegal { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Chiều ngang mặt tiền diện tích phù hợp quy hoạch.")]
        public double? AreaLegalWidth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Chiều sâu diện tích phù hợp quy hoạch.")]
        public double? AreaLegalLength { get; set; }

        public double? AreaLegalBackWidth { get; set; }
        public double? AreaIlegal { get; set; }

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
        public string ContactAddress { get; set; }

        [StringLength(255)]
        public string ContactEmail { get; set; }

        #endregion

        #region Price

        // Price

        public double? PriceProposed { get; set; }
        public double? PriceProposedInVND { get; set; }
        public double? PriceEstimatedInVND { get; set; }
        public double? PriceEstimatedRatingPoint { get; set; }

        [StringLength(500)]
        public string PriceEstimatedComment { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        public int PaymentUnitId { get; set; }
        public IEnumerable<PaymentUnitPartRecord> PaymentUnits { get; set; }

        #endregion

        public IContent Property { get; set; }
    }

    public class EstimateEditViewModel
    {
        public int Id
        {
            get { return Property.As<PropertyPart>().Id; }
        }

        public bool IsValidForEstimate { get; set; }

        public IEnumerable<PropertyDisplayEntry> ReferencedProperties { get; set; }

        public int AdsTypeVIP { get; set; }
        public string AdsTypeCssClass { get; set; }

        public string DateVipFrom { get; set; }

        public string DateVipTo { get; set; }

        public long Amount { get; set; }
        public string AmountVND { get; set; }
        public long[] UnitArray { get; set; }

        // Files
        public virtual IEnumerable<PropertyFilePart> Files { get; set; }

        public IContent Property { get; set; }

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

        #endregion

        #region AdsContent

        // Published
        public bool Published
        {
            get { return Property.As<PropertyPart>().Published; }
            set { Property.As<PropertyPart>().Published = value; }
        }

        // AdsExpirationDate
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

        #endregion

        #region Type

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

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh / Thành phố.")]
        public int ProvinceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận / Huyện.")]
        public int DistrictId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Phường / Xã.")]
        public int WardId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Đường / Phố.")]
        public int StreetId { get; set; }

        public int ApartmentId { get; set; }

        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        [StringLength(255)]
        [Required(ErrorMessage = "Vui lòng nhập Số nhà.")]
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

        [StringLength(255)]
        public string ApartmentNumber
        {
            get { return Property.As<PropertyPart>().ApartmentNumber; }
            set { Property.As<PropertyPart>().ApartmentNumber = value; }
        }

        public int AlleyNumber
        {
            get { return Property.As<PropertyPart>().AlleyNumber; }
            set { Property.As<PropertyPart>().AlleyNumber = value; }
        }

        // PublishAddress
        public bool PublishAddress
        {
            get { return Property.As<PropertyPart>().PublishAddress; }
            set { Property.As<PropertyPart>().PublishAddress = value; }
        }

        public string DisplayForAddress
        {
            get { return Property.As<PropertyPart>().DisplayForAddress; }
        }

        public bool UnPublishAddress { get; set; }

        #endregion

        #region LegalStatus, Direction, Location

        // LegalStatus

        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyLegalStatusPartRecord> LegalStatus { get; set; }

        // Direction

        public int? DirectionId { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        // Location

        public int LocationId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Vị trí BĐS.")]
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

        //[Required(ErrorMessage = "Vui lòng nhập Chiều ngang mặt tiền diện tích khuôn viên.")]
        public double? AreaTotalWidth
        {
            get { return Property.As<PropertyPart>().AreaTotalWidth; }
            set { Property.As<PropertyPart>().AreaTotalWidth = value; }
        }

        //[Required(ErrorMessage = "Vui lòng nhập Chiều sâu diện tích khuôn viên.")]
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

        [Required(ErrorMessage = "Vui lòng nhập Chiều ngang mặt tiền diện tích phù hợp quy hoạch.")]
        public double? AreaLegalWidth
        {
            get { return Property.As<PropertyPart>().AreaLegalWidth; }
            set { Property.As<PropertyPart>().AreaLegalWidth = value; }
        }

        [Required(ErrorMessage = "Vui lòng nhập Chiều sâu diện tích phù hợp quy hoạch.")]
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

        public double? AreaIlegal { get; set; }

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

        #region Advantages & DisAdvantages

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
    }


    public class EstimateWidgetViewModel
    {
        // Province
        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        // Districts
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        // Wards
        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        // Streets
        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public string AddressNumber { get; set; }

        public string AddressCorner { get; set; }
    }
}