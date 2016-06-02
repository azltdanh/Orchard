using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationStreetSegmentViewModel
    {
    }

    public class LocationStreetSegmentCreateViewModel
    {
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePartRecord> Provinces { get; set; }

        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPartRecord> Districts { get; set; }

        public int WardId { get; set; }
        public IEnumerable<LocationWardPartRecord> Wards { get; set; }

        [Required]
        public int StreetId { get; set; }
        public IEnumerable<LocationStreetPartRecord> Streets { get; set; }

        [Required]
        public int FromNumber { get; set; }

        [Required]
        public int ToNumber { get; set; }

        public bool IsEnabled { get; set; }

        public float StreetWidth { get; set; }

        public float CoefficientAlley1Max { get; set; }

        public float CoefficientAlley1Min { get; set; }

        public float CoefficientAlleyMax { get; set; }

        public float CoefficientAlleyMin { get; set; }

        public float CoefficientAlleyEqual { get; set; }

        public IContent LocationStreetSegment { get; set; }
    }

    public class LocationStreetSegmentEditViewModel
    {
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePartRecord> Provinces { get; set; }

        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPartRecord> Districts { get; set; }

        public int WardId { get; set; }
        public IEnumerable<LocationWardPartRecord> Wards { get; set; }

        [Required]
        public int StreetId { get; set; }
        public IEnumerable<LocationStreetPartRecord> Streets { get; set; }

        [Required]
        public int FromNumber
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.FromNumber; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.FromNumber = value; }
        }

        [Required]
        public int ToNumber
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.ToNumber; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.ToNumber = value; }
        }

        public bool IsEnabled
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.IsEnabled; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.IsEnabled = value; }
        }

        public float StreetWidth
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.StreetWidth; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.StreetWidth = value; }
        }

        public float CoefficientAlley1Max
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlley1Max; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlley1Max = value; }
        }

        public float CoefficientAlley1Min
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlley1Min; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlley1Min = value; }
        }

        public float CoefficientAlleyMax
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlleyMax; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlleyMax = value; }
        }

        public float CoefficientAlleyMin
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlleyMin; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlleyMin = value; }
        }

        public float CoefficientAlleyEqual
        {
            get { return LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlleyEqual; }
            set { LocationStreetSegment.As<LocationStreetSegmentPart>().Record.CoefficientAlleyEqual = value; }
        }

        public IContent LocationStreetSegment { get; set; }
    }

    public class LocationStreetSegmentsIndexViewModel
    {
        public IList<LocationStreetSegmentEntry> LocationStreetSegments { get; set; }
        public LocationStreetSegmentIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationStreetSegmentEntry
    {
        public LocationStreetSegmentPartRecord LocationStreetSegment { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationStreetSegmentIndexOptions
    {
        public string Search { get; set; }
        public LocationStreetSegmentsOrder Order { get; set; }
        public LocationStreetSegmentsFilter Filter { get; set; }
        public LocationStreetSegmentsBulkAction BulkAction { get; set; }
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePartRecord> Provinces { get; set; }
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPartRecord> Districts { get; set; }
        public int StreetId { get; set; }
        public IEnumerable<LocationStreetPartRecord> Streets { get; set; }
    }

    public enum LocationStreetSegmentsOrder
    {
        Name,
        SeqOrder
    }

    public enum LocationStreetSegmentsFilter
    {
        All
    }

    public enum LocationStreetSegmentsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
