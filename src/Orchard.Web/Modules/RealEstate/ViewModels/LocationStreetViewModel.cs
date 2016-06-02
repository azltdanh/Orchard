using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationStreetViewModel
    {
    }

    public class LocationStreetCreateViewModel
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

        public string Name { get; set; }

        public string ShortName { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsOneWayStreet { get; set; }

        public double? StreetWidth { get; set; }

        public double? CoefficientAlley1Max { get; set; }

        public double? CoefficientAlley1Min { get; set; }

        public double? CoefficientAlleyMax { get; set; }

        public double? CoefficientAlleyMin { get; set; }

        public double? CoefficientAlleyEqual { get; set; }

        public bool IsRelatedStreet { get; set; }

        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int? FromNumber { get; set; }

        public int? ToNumber { get; set; }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationStreet { get; set; }
    }

    public class LocationStreetEditViewModel
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

        public string Name
        {
            get { return LocationStreet.As<LocationStreetPart>().Name; }
            set { LocationStreet.As<LocationStreetPart>().Name = value; }
        }

        public string ShortName
        {
            get { return LocationStreet.As<LocationStreetPart>().ShortName; }
            set { LocationStreet.As<LocationStreetPart>().ShortName = value; }
        }

        public int SeqOrder
        {
            get { return LocationStreet.As<LocationStreetPart>().SeqOrder; }
            set { LocationStreet.As<LocationStreetPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return LocationStreet.As<LocationStreetPart>().IsEnabled; }
            set { LocationStreet.As<LocationStreetPart>().IsEnabled = value; }
        }

        public bool IsOneWayStreet
        {
            get { return LocationStreet.As<LocationStreetPart>().IsOneWayStreet; }
            set { LocationStreet.As<LocationStreetPart>().IsOneWayStreet = value; }
        }

        public double? StreetWidth
        {
            get { return LocationStreet.As<LocationStreetPart>().StreetWidth; }
            set { LocationStreet.As<LocationStreetPart>().StreetWidth = value; }
        }

        public double? CoefficientAlley1Max
        {
            get { return LocationStreet.As<LocationStreetPart>().CoefficientAlley1Max; }
            set { LocationStreet.As<LocationStreetPart>().CoefficientAlley1Max = value; }
        }

        public double? CoefficientAlley1Min
        {
            get { return LocationStreet.As<LocationStreetPart>().CoefficientAlley1Min; }
            set { LocationStreet.As<LocationStreetPart>().CoefficientAlley1Min = value; }
        }

        public double? CoefficientAlleyMax
        {
            get { return LocationStreet.As<LocationStreetPart>().CoefficientAlleyMax; }
            set { LocationStreet.As<LocationStreetPart>().CoefficientAlleyMax = value; }
        }

        public double? CoefficientAlleyMin
        {
            get { return LocationStreet.As<LocationStreetPart>().CoefficientAlleyMin; }
            set { LocationStreet.As<LocationStreetPart>().CoefficientAlleyMin = value; }
        }

        public double? CoefficientAlleyEqual
        {
            get { return LocationStreet.As<LocationStreetPart>().CoefficientAlleyEqual; }
            set { LocationStreet.As<LocationStreetPart>().CoefficientAlleyEqual = value; }
        }

        public bool IsRelatedStreet { get; set; }

        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int? FromNumber
        {
            get { return LocationStreet.As<LocationStreetPart>().FromNumber; }
            set { LocationStreet.As<LocationStreetPart>().FromNumber = value; }
        }

        public int? ToNumber
        {
            get { return LocationStreet.As<LocationStreetPart>().ToNumber; }
            set { LocationStreet.As<LocationStreetPart>().ToNumber = value; }
        }

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationStreet { get; set; }
    }

    public class LocationStreetsIndexViewModel
    {
        public IList<LocationStreetEntry> LocationStreets { get; set; }
        public LocationStreetIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationStreetEntry
    {
        public LocationStreetPart LocationStreet { get; set; }
        public string DisplayForStreetName { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationStreetIndexOptions
    {
        public string Search { get; set; }
        public LocationStreetsOrder Order { get; set; }
        public LocationStreetsFilter Filter { get; set; }
        public LocationStreetsBulkAction BulkAction { get; set; }
        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
        public bool ShowRelatedStreetOnly { get; set; }
        public bool IsRestrictedLocations { get; set; }
    }

    public enum LocationStreetsOrder
    {
        Name,
        SeqOrder
    }

    public enum LocationStreetsFilter
    {
        All
    }

    public enum LocationStreetsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
