using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationDistrictViewModel
    {
    }

    public class LocationDistrictCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceShortName { get; set; }

        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public string ContactPhone { get; set; }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationDistrict { get; set; }
    }

    public class LocationDistrictEditViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceShortName { get; set; }

        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public string Name
        {
            get { return LocationDistrict.As<LocationDistrictPart>().Name; }
            set { LocationDistrict.As<LocationDistrictPart>().Name = value; }
        }

        [Required]
        public string ShortName
        {
            get { return LocationDistrict.As<LocationDistrictPart>().ShortName; }
            set { LocationDistrict.As<LocationDistrictPart>().ShortName = value; }
        }

        public int SeqOrder
        {
            get { return LocationDistrict.As<LocationDistrictPart>().SeqOrder; }
            set { LocationDistrict.As<LocationDistrictPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return LocationDistrict.As<LocationDistrictPart>().IsEnabled; }
            set { LocationDistrict.As<LocationDistrictPart>().IsEnabled = value; }
        }

        public string ContactPhone
        {
            get { return LocationDistrict.As<LocationDistrictPart>().ContactPhone; }
            set { LocationDistrict.As<LocationDistrictPart>().ContactPhone = value; }
        }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationDistrict { get; set; }
    }

    public class LocationDistrictsIndexViewModel
    {
        public IList<LocationDistrictEntry> LocationDistricts { get; set; }
        public LocationDistrictIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationDistrictEntry
    {
        public LocationDistrictPart LocationDistrict { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationDistrictIndexOptions
    {
        public string Search { get; set; }
        public LocationDistrictsOrder Order { get; set; }
        public LocationDistrictsFilter Filter { get; set; }
        public LocationDistrictsBulkAction BulkAction { get; set; }
        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        public bool IsRestrictedLocations { get; set; }

    }

    public enum LocationDistrictsOrder
    {
        SeqOrder,
        Name
    }

    public enum LocationDistrictsFilter
    {
        All
    }

    public enum LocationDistrictsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
