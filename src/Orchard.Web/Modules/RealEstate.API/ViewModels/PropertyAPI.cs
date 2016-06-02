using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.API.ViewModels
{

    public class PropertyAPICreateViewModel
    {
        #region Note
        
        //public int TypeId { get; set; }
        //public int ProvinceId { get; set; }
        //public int DistrictId { get; set; }
        //public int WardId { get; set; }
        //public int StreetId { get; set; }
        //public string AddressNumber { get; set; }
        //public int AlleyNumber { get; set; }
        //public string OtherProvinceName { get; set; }
        //public string OtherDistrictName { get; set; }
        //public string OtherWardName { get; set; }
        //public string OtherStreetName { get; set; }
        //public int LegalStatusId { get; set; }
        //public int DirectionId { get; set; }
        //public int LocationId { get; set; }
        //public int AlleyTurns { get; set; }
        //public double? DistanceToStreet { get; set; }
        //public double? AlleyWidth { get; set; }
        //public double? AlleyWidth1 { get; set; }
        //public double? StreetWidth { get; set; }
        //public double? AreaTotal { get; set; }
        //public double? AreaTotalWidth { get; set; }
        //public double? AreaTotalLength { get; set; }
        //public double? AreaTotalBackWidth { get; set; }
        //public double? AreaLegal { get; set; }
        //public double? AreaLegalWidth { get; set; }
        //public double? AreaLegalLength { get; set; }
        //public double? AreaLegalBackWidth { get; set; }
        //public double? AreaIlegalRecognized { get; set; }
        //public double? AreaIlegalNotRecognized { get; set; }
        //public double? AreaConstruction { get; set; }
        //public double? AreaUsable { get; set; }
        //public double? Floors { get; set; }
        //public int? Bedrooms { get; set; }
        //public int? Livingrooms { get; set; }
        //public int? Bathrooms { get; set; }
        //public int? Balconies { get; set; }
        //public int? InteriorId { get; set; }
        //public double? RemainingValue { get; set; }
        //public bool? HaveBasement { get; set; }
        //public bool? HaveMezzanine { get; set; }
        //public bool? HaveElevator { get; set; }
        //public bool? HaveSwimmingPool { get; set; }
        //public bool? HaveGarage { get; set; }
        //public bool? HaveGarden { get; set; }
        //public bool? HaveTerrace { get; set; }
        //public bool? HaveSkylight { get; set; }
        //public string ContactName { get; set; }
        //public string ContactPhone { get; set; }
        //public string ContactAddress { get; set; }
        //public string ContactEmail { get; set; }
        //public double? PriceProposed { get; set; }
        //public double? PriceProposedInVND { get; set; }
        //public double? PriceEstimatedInVND { get; set; }
        //public int? PaymentMethodId { get; set; }
        //public int? PaymentUnitId { get; set; }

        #endregion

        public string ReturnUrl { get; set; }
        public string apiKey { get; set; }
        public int IsSaveDraft { get; set; }

        public int DomainGroupId { get; set; }
        public int UserId { get; set; }

        // AdsType
        [Required(ErrorMessage = "Vui lòng chọn loại giao dịch.")]
        public string AdsTypeCssClass { get; set; }

        public IEnumerable<PropertyBase> AdsTypes { get; set; }

        // TypeGroup
        [Required(ErrorMessage = "Vui lòng chọn Nhóm BĐS.")]
        public string TypeGroupCssClass { get; set; }
        public int TypeGroupId { get; set; }

        public IEnumerable<PropertyBase> TypeGroups { get; set; }

        // Type
        [Required(ErrorMessage = "Vui lòng chọn loại BĐS.")]
        public int? TypeId { get; set; }

        public IEnumerable<PropertyBase> Types { get; set; }

        #region Ads Content

        public string Title { get; set; }


        //AdsExpirationDate
        public int? AdsExpirationDateValue { get; set; }
        public bool AdsGoodDealRequest { get; set; }
        public bool AdsVIPRequest { get; set; }

        public string DateVipFrom { get; set; }
        public string DateVipTo { get; set; }
        public int AdsTypeVIP { get; set; }
        public bool IsAdsVIP { get; set; }

        #endregion
    }

    //Cập nhật tin đăng
    public class PropertyAPIEditViewModel
    {
        public string ReturnUrl { get; set; }
        public bool IsPropertyExchange { get; set; }

        public int PropertyId { get; set; }//
        public int? DomainGroupId { get; set; }//
        public int UserId { get; set; }//

        // Files
        //public virtual IEnumerable<PropertyFile> Files { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        #region Type

        // TypeGroup
        public string TypeGroupCssClass { get; set; }
        //[Required(ErrorMessage = "Vui lòng chọn Nhóm BĐS.")]
        //public int TypeGroupId { get; set; }

        public IEnumerable<PropertyBase> TypeGroups { get; set; }

        // Type
        //[Required(ErrorMessage = "Vui lòng chọn Loại BĐS.")]
        public int? TypeId { get; set; }

        public IEnumerable<PropertyBase> Types { get; set; }

        // TypeConstruction
        public int TypeConstructionId { get; set; }
        public IEnumerable<PropertyBase> TypeConstructions { get; set; }

        #endregion

        #region Address

        // Address

        public int? ProvinceId { get; set; }

        public PropertyBase Province { get; set; }

        public int? DistrictId { get; set; }

        public PropertyBase District { get; set; }

        public int? WardId { get; set; }

        public PropertyBase Ward { get; set; }

        public int? StreetId { get; set; }

        public PropertyBase Street { get; set; }

        public int? ApartmentId { get; set; }

        public string DisplayForAddress { get; set; }

        [StringLength(255)]
        public string AddressNumber { get; set; }

        public string ApartmentNumber { get; set; }

        [StringLength(255)]
        public string AddressCorner { get; set; }

        public int AlleyNumber { get; set; }

        //PublishAddress
        public bool PublishAddress { get; set; }

        public bool UnPublishAddress { get; set; }

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

        public IEnumerable<PropertyBase> Provinces { get; set; }
        public IEnumerable<PropertyBase> Districts { get; set; }
        public IEnumerable<PropertyBase> Wards { get; set; }
        public IEnumerable<PropertyBase> Streets { get; set; }
        public IEnumerable<PropertyBase> Apartments { get; set; }

        #endregion

        #region LegalStatus, Direction, Location

        // LegalStatus
        public int? LegalStatusId { get; set; }
        public IEnumerable<PropertyBase> LegalStatus { get; set; }

        // Direction
        public int? DirectionId { get; set; }
        public IEnumerable<PropertyBase> Directions { get; set; }

        // Location
        public string LocationCssClass { get; set; }
        public int? LocationId { get; set; }
        public IEnumerable<PropertyBase> Locations { get; set; }

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

        #endregion

        #region Construction

        // AreaUsable
        public double? AreaUsable { get; set; }

        // AreaResidential
        public double? AreaResidential { get; set; }

        // Construction

        public double? AreaConstruction { get; set; }

        public double? AreaConstructionFloor { get; set; }

        public int? Floors { get; set; }

        public double? FloorsCount { get; set; }

        public int? Bedrooms { get; set; }
        public int? Livingrooms { get; set; }

        public int? Bathrooms { get; set; }
        public int? Balconies { get; set; }

        public int? ApartmentBedrooms { get; set; }
        public int? ApartmentBathrooms { get; set; }

        public int? InteriorId { get; set; }
        public IEnumerable<PropertyBase> Interiors { get; set; }

        public double? RemainingValue { get; set; }

        public bool HaveBasement { get; set; }

        public bool HaveMezzanine { get; set; }

        public bool HaveElevator { get; set; }

        public bool HaveSwimmingPool { get; set; }

        public bool HaveGarage { get; set; }

        public bool HaveGarden { get; set; }

        public bool HaveTerrace { get; set; }

        public bool HaveSkylight { get; set; }

        public int? ApartmentFloorTh { get; set; }

        public int? ApartmentFloors { get; set; }

        public int? ApartmentTradeFloors { get; set; }

        public int? ApartmentElevators { get; set; }

        public int? ApartmentBasements { get; set; }


        #endregion

        #region Advantages,DisAdvantages

        // Apartment Advantages
        public IList<PropertyAdvantageEntry> ApartmentAdvantages { get; set; }

        // Apartment Interior Advantages
        public IList<PropertyAdvantageEntry> ApartmentInteriorAdvantages { get; set; }

        // Advantages
        //public IList<PropertyAdvantageEntry> Advantages { get; set; }

        // OtherAdvantages
        public double? OtherAdvantages { get; set; }

        // OtherAdvantagesDesc
        [StringLength(255)]
        public string OtherAdvantagesDesc { get; set; }

        // DisAdvantages
        //public IList<PropertyAdvantageEntry> DisAdvantages { get; set; }

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
        public string ContactPhone { get; set; }

        [StringLength(255)]
        public string ContactAddress { get; set; }
        [StringLength(255)]
        public string ContactEmail { get; set; }

        #endregion

        #region IsOwner, NoBroker, IsAuction

        // IsOwner
        public bool IsOwner { get; set; }

        // NoBroker
        public bool NoBroker { get; set; }

        // IsAuction
        public bool IsAuction { get; set; }

        #endregion

        #region Price

        // Price

        public double? PriceProposed { get; set; }

        public double? PriceProposedInVND { get; set; }
        public double? PriceEstimatedInVND { get; set; }

        public bool IsEstimateable { get; set; }
        public bool SubmitEstimate { get; set; }

        public int? PaymentMethodId { get; set; }
        public IEnumerable<PropertyBase> PaymentMethods { get; set; }
        public int? PaymentUnitId { get; set; }
        public IEnumerable<PropertyBase> PaymentUnits { get; set; }

        #endregion

        #region User

        // User
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        #endregion

        #region Ads Content

        // AdsType
        public string AdsTypeCssClass { get; set; }
        //[Required(ErrorMessage = "Vui lòng chọn Loại giao dịch.")]
        public int AdsTypeId { get; set; }

        public IEnumerable<PropertyBase> AdsTypes { get; set; }

        // Published
        public bool Published { get; set; }

        //AdsExpirationDate
        public int? AdsExpirationDateValue { get; set; }

        public bool AdsGoodDealRequest { get; set; }

        public bool AdsVIPRequest { get; set; }

        public int AdsTypeVIP { get; set; }
        public bool IsAdsVIP { get; set; }

        public bool AdsHighlightRequest { get; set; }

        // Ads Content

        [StringLength(255)]
        public string Title { get; set; }
        public string Content { get; set; }

        [StringLength(255)]
        public string Note { get; set; }

        public string StatusCssClass { get; set; }

        public bool IsRefresh { get; set; }

        public string DateVipFrom { get; set; }
        public string DateVipTo { get; set; }
        public long Amount { get; set; }
        public string AmountVND { get; set; }
        public long[] UnitArray { get; set; }
        public int PostingDates { get; set; }

        #endregion

        #region Map

        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public float PlanMapLatitude { get; set; }
        public float PlanMapLongitude { get; set; }

        #endregion
    }

    public class PropertyAdvantageEntry
    {
        public bool IsChecked { get; set; }
        public PropertyBase Advantage { get; set; }
    }

    public class PropertyBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string CssClass { get; set; }
        public string ContactPhone { get; set; }

        //TypeContruction
        public int? GroupId { get; set; }
        public int? TypeId { get; set; }
        public int? MinFloor { get; set; }
        public int? MaxFloor { get; set; }
        public int? SeqOrder { get; set; }
    }

    #region Estimation

    public class EstimateViewModel
    {
        public string ReturnUrl { get; set; }

        public string apiKey { get; set; }
        public int UserId { get; set; }
        public int DomainGroupId { get; set; }

        public int PropertyId { get; set; }
        public int ContentItemId { get; set; }

        // Files
        // public virtual IEnumerable<PropertyFile.PropertyFile> Files { get; set; }

        // Accept post facebook
        public bool HaveFacebookUserId { get; set; }
        public bool AcceptPostToFacebok { get; set; }

        #region Type

        // AdsType
        //[Required(ErrorMessage = "Vui lòng chọn loại giao dịch.")]
        public string AdsTypeCssClass { get; set; }

        // TypeGroup
        //[Required(ErrorMessage = "Vui lòng chọn Nhóm BĐS.")]
        public string TypeGroupCssClass { get; set; }
        public int? TypeGroupId { get; set; }

        // Type
        [Required(ErrorMessage = "Vui lòng chọn Loại BĐS.")]
        public int? TypeId { get; set; }

        // TypeConstruction
        public int? TypeConstructionId { get; set; }
        public IEnumerable<PropertyBase> TypeConstructions { get; set; }

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

        [StringLength(255)]
        [Required(ErrorMessage = "Vui lòng nhập Số nhà.")]
        public string AddressNumber { get; set; }

        [StringLength(255)]
        public string AddressCorner { get; set; }

        public int AlleyNumber { get; set; }

        public int ApartmentId { get; set; }

        [StringLength(255)]
        public string ApartmentNumber { get; set; }

        public string DisplayForAddress { get; set; }

        public bool PublishAddress { get; set; }

        public bool UnPublishAddress { get; set; }

        #endregion

        #region LegalStatus, Direction, Location

        // LegalStatus
        public int? LegalStatusId { get; set; }

        // Direction
        public int? DirectionId { get; set; }

        // Location
        [Required(ErrorMessage = "Vui lòng chọn Vị trí BĐS.")]
        public string LocationCssClass { get; set; }
        public int? LocationId { get; set; }

        #endregion

        #region Alley

        public double? DistanceToStreet { get; set; }
        public int? AlleyTurns { get; set; }
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

        //[Required(ErrorMessage = "Vui lòng nhập Chiều ngang mặt tiền diện tích phù hợp quy hoạch.")]
        public double? AreaLegalWidth { get; set; }

        //[Required(ErrorMessage = "Vui lòng nhập Chiều sâu diện tích phù hợp quy hoạch.")]
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

        #region IsOwner, NoBroker, IsAuction

        // IsOwner
        public bool IsOwner { get; set; }

        // NoBroker
        public bool NoBroker { get; set; }

        // IsAuction
        public bool IsAuction { get; set; }

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
        public int PaymentUnitId { get; set; }

        #endregion

        #region User

        // User
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        #endregion

        #region Ads Content

        // Published
        public bool Published { get; set; }

        // Ads Content

        [StringLength(255)]
        public string Title { get; set; }
        public string Content { get; set; }

        public string StatusName { get; set; }
        public string StatusCssClass { get; set; }

        #endregion

        #region Map

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public double PlanMapLatitude { get; set; }
        public double PlanMapLongitude { get; set; }

        #endregion
    }

    #endregion
}