using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationApartmentRelationViewModel
    {
    }

    public class LocationApartmentRelationCreateViewModel
    {

        public int? Id { get; set; }
        public string ReturnUrl { get; set; }

        #region LocationApartment

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        [Required]
        public int ApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        #endregion

        [Required]
        public double? RelatedValue { get; set; }

        #region RelatedLocationApartment

        [Required]
        public int RelatedProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> RelatedProvinces { get; set; }

        [Required]
        public int RelatedDistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> RelatedDistricts { get; set; }

        [Required]
        public int RelatedApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> RelatedApartments { get; set; }

        #endregion

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationApartmentRelation { get; set; }
    }

    public class LocationApartmentRelationEditViewModel
    {

        public int Id
        {
            get { return LocationApartmentRelation.As<LocationApartmentRelationPart>().Id; }
            set { }
        }
        public string ReturnUrl { get; set; }

        #region LocationApartment

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        [Required]
        public int ApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        #endregion

        [Required]
        public double RelatedValue
        {
            get { return LocationApartmentRelation.As<LocationApartmentRelationPart>().RelatedValue; }
            set { LocationApartmentRelation.As<LocationApartmentRelationPart>().RelatedValue = value; }
        }

        #region RelatedLocationApartment

        [Required]
        public int RelatedProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> RelatedProvinces { get; set; }

        [Required]
        public int RelatedDistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> RelatedDistricts { get; set; }

        [Required]
        public int RelatedApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> RelatedApartments { get; set; }

        #endregion

        public bool IsRestrictedLocations { get; set; }

        public IContent LocationApartmentRelation { get; set; }
    }

    public class LocationApartmentRelationsIndexViewModel
    {
        public IList<LocationApartmentRelationEntry> LocationApartmentRelations { get; set; }
        public LocationApartmentRelationIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationApartmentRelationEntry
    {
        public LocationApartmentRelationPartRecord LocationApartmentRelation { get; set; }
        public double AssociatedValue { get; set; }
        public bool IsAncestor { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationApartmentRelationIndexOptions
    {
        public LocationApartmentRelationsOrder Order { get; set; }
        public LocationApartmentRelationsFilter Filter { get; set; }
        public LocationApartmentRelationsBulkAction BulkAction { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public int? ApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }

        public int? RelatedProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> RelatedProvinces { get; set; }
        public int? RelatedDistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> RelatedDistricts { get; set; }
        public int? RelatedApartmentId { get; set; }
        public IEnumerable<LocationApartmentPart> RelatedApartments { get; set; }

        public bool IsRestrictedLocations { get; set; }

    }

    public enum LocationApartmentRelationsOrder
    {
        Province,
        District,
        LocationApartment,
        RelatedProvince,
        RelatedDistrict,
        RelatedLocationApartment
    }

    public enum LocationApartmentRelationsFilter
    {
        All
    }

    public enum LocationApartmentRelationsBulkAction
    {
        None,
        Delete,
    }
}
