using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationWardViewModel
    {
    }

    public class LocationWardCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationWard { get; set; }
    }

    public class LocationWardEditViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        [Required]
        public string Name
        {
            get { return LocationWard.As<LocationWardPart>().Name; }
            set { LocationWard.As<LocationWardPart>().Name = value; }
        }

        [Required]
        public string ShortName
        {
            get { return LocationWard.As<LocationWardPart>().ShortName; }
            set { LocationWard.As<LocationWardPart>().ShortName = value; }
        }

        public int SeqOrder
        {
            get { return LocationWard.As<LocationWardPart>().SeqOrder; }
            set { LocationWard.As<LocationWardPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return LocationWard.As<LocationWardPart>().IsEnabled; }
            set { LocationWard.As<LocationWardPart>().IsEnabled = value; }
        }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationWard { get; set; }
    }

    public class LocationWardsIndexViewModel
    {
        public IList<LocationWardEntry> LocationWards { get; set; }
        public LocationWardIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationWardEntry
    {
        public LocationWardPart LocationWard { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationWardIndexOptions
    {
        public string Search { get; set; }
        public LocationWardsOrder Order { get; set; }
        public LocationWardsFilter Filter { get; set; }
        public LocationWardsBulkAction BulkAction { get; set; }
        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        public bool IsRestrictedLocations { get; set; }

    }

    public enum LocationWardsOrder
    {
        SeqOrder,
        Name
    }

    public enum LocationWardsFilter
    {
        All
    }

    public enum LocationWardsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
