

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using RealEstate.Models;
using Orchard.ContentManagement;
using System;

namespace RealEstate.ViewModels
{
    public class PropertyExchangeViewModel
    {
    }

    public class CustomerRequirementExchangeCreateViewModel
    {
        public string PropertyDisplayAddress { get; set; }
        public int? PropertyId { get; set; }
        [Required(ErrorMessage = "Vui Lòng nhập giá trị chênh lệch muốn trao đổi")]
        public double Values { get; set; }

        [Required(ErrorMessage = "Vui Lòng chọn loại giao dịch chênh lệch muốn đổi")]
        public string ExchangeTypeClass { get; set; }
        public List<SelectListItem> ExchangeTypes { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

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
    }

    public class CustomerRequirementExchangeEditViewModel
    {
        public int? PropertyId { get; set; }
        public string PropertyDisplayAddress { get; set; }

        [Required(ErrorMessage = "Vui Lòng nhập giá trị chênh lệch muốn trao đổi")]
        public double Values { get; set; }

        [Required(ErrorMessage = "Vui Lòng chọn loại giao dịch chênh lệch muốn đổi")]
        public string ExchangeTypeClass { get; set; }
        public List<SelectListItem> ExchangeTypes { get; set; }

        public int PaymentMethodId { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }
        
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
}
