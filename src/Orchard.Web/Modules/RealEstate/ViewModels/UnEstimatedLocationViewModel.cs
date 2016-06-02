using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class UnEstimatedLocationsIndexViewModel
    {
        public IList<UnEstimatedLocationEntry> UnEstimatedLocations { get; set; }
        public UnEstimatedLocationIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class UnEstimatedLocationEntry
    {
        public UnEstimatedLocationRecord UnEstimatedLocation { get; set; }
        public bool IsChecked { get; set; }
    }

    public class UnEstimatedLocationIndexOptions
    {
        public string Search { get; set; }
        public UnEstimatedLocationsOrder Order { get; set; }
        public UnEstimatedLocationsFilter Filter { get; set; }
        public UnEstimatedLocationsBulkAction BulkAction { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
    }

    public enum UnEstimatedLocationsOrder
    {
        CreatedDate,
        Address
    }

    public enum UnEstimatedLocationsFilter
    {
        All
    }

    public enum UnEstimatedLocationsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
