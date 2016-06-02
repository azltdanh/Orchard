using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class StreetRelationViewModel
    {
    }

    public class StreetRelationCreateViewModel
    {

        public int Id { get; set; }
        public string ReturnUrl { get; set; }

        #region Street

        [Required]
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string ProvinceShortName { get; set; }

        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        [Required]
        public int WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        [Required]
        public int StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public double? StreetWidth { get; set; }
        
        #endregion

        [Required]
        public double? RelatedValue { get; set; }
        public double? RelatedAlleyValue { get; set; }

        #region RelatedStreet

        [Required]
        public int RelatedProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> RelatedProvinces { get; set; }

        [Required]
        public int RelatedDistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> RelatedDistricts { get; set; }

        [Required]
        public int RelatedWardId { get; set; }
        public IEnumerable<LocationWardPart> RelatedWards { get; set; }

        [Required]
        public int RelatedStreetId { get; set; }
        public IEnumerable<LocationStreetPart> RelatedStreets { get; set; }

        public double? RelatedStreetWidth { get; set; }

        #endregion

        #region Coefficient

        public double? CoefficientAlley1Max { get; set; }
        public double? CoefficientAlley1Min { get; set; }
        public double? CoefficientAlleyEqual { get; set; }
        public double? CoefficientAlleyMax { get; set; }
        public double? CoefficientAlleyMin { get; set; }

        public int? CoefficientAlleyId { get; set; }
        public IEnumerable<CoefficientAlleyPartRecord> CoefficientAlleys { get; set; }

        #endregion

        public bool IsRestrictedLocations { get; set; }

        public IContent StreetRelation { get; set; }
    }

    public class StreetRelationEditViewModel
    {

        public int Id
        {
            get { return StreetRelation.As<StreetRelationPart>().Id; }
            set { }
        }
        public string ReturnUrl { get; set; }

        #region Street

        [Required]
        public int ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }

        [Required]
        public int DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }

        [Required]
        public int WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }

        [Required]
        public int StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public double? StreetWidth 
        {
            get { return StreetRelation.As<StreetRelationPart>().StreetWidth; }
            set { StreetRelation.As<StreetRelationPart>().StreetWidth = value; }
        }

        #endregion

        [Required]
        public double RelatedValue
        {
            get { return StreetRelation.As<StreetRelationPart>().RelatedValue; }
            set { StreetRelation.As<StreetRelationPart>().RelatedValue = value; }
        }
        public double? RelatedAlleyValue
        {
            get { return StreetRelation.As<StreetRelationPart>().RelatedAlleyValue; }
            set { StreetRelation.As<StreetRelationPart>().RelatedAlleyValue = value; }
        }

        #region RelatedStreet

        [Required]
        public int RelatedProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> RelatedProvinces { get; set; }

        [Required]
        public int RelatedDistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> RelatedDistricts { get; set; }

        [Required]
        public int RelatedWardId { get; set; }
        public IEnumerable<LocationWardPart> RelatedWards { get; set; }

        [Required]
        public int RelatedStreetId { get; set; }
        public IEnumerable<LocationStreetPart> RelatedStreets { get; set; }

        public double? RelatedStreetWidth
        {
            get { return StreetRelation.As<StreetRelationPart>().RelatedStreetWidth; }
            set { StreetRelation.As<StreetRelationPart>().RelatedStreetWidth = value; }
        }

        #endregion

        #region Coefficient

        public double? CoefficientAlley1Max
        {
            get { return StreetRelation.As<StreetRelationPart>().CoefficientAlley1Max; }
            set { StreetRelation.As<StreetRelationPart>().CoefficientAlley1Max = value; }
        }

        public double? CoefficientAlley1Min
        {
            get { return StreetRelation.As<StreetRelationPart>().CoefficientAlley1Min; }
            set { StreetRelation.As<StreetRelationPart>().CoefficientAlley1Min = value; }
        }

        public double? CoefficientAlleyMax
        {
            get { return StreetRelation.As<StreetRelationPart>().CoefficientAlleyMax; }
            set { StreetRelation.As<StreetRelationPart>().CoefficientAlleyMax = value; }
        }

        public double? CoefficientAlleyMin
        {
            get { return StreetRelation.As<StreetRelationPart>().CoefficientAlleyMin; }
            set { StreetRelation.As<StreetRelationPart>().CoefficientAlleyMin = value; }
        }

        public double? CoefficientAlleyEqual
        {
            get { return StreetRelation.As<StreetRelationPart>().CoefficientAlleyEqual; }
            set { StreetRelation.As<StreetRelationPart>().CoefficientAlleyEqual = value; }
        }

        public int? CoefficientAlleyId { get; set; }
        public IEnumerable<CoefficientAlleyPartRecord> CoefficientAlleys { get; set; }

        #endregion

        public bool IsRestrictedLocations { get; set; }

        public IContent StreetRelation { get; set; }
    }

    public class StreetRelationsIndexViewModel
    {
        public IList<StreetRelationEntry> StreetRelations { get; set; }
        public StreetRelationIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class StreetRelationEntry
    {
        public StreetRelationPartRecord StreetRelation { get; set; }
        public double AssociatedValue { get; set; }
        public bool IsAncestor { get; set; }
        public bool IsChecked { get; set; }
    }

    public class StreetRelationIndexOptions
    {
        public StreetRelationsOrder Order { get; set; }
        public StreetRelationsFilter Filter { get; set; }
        public StreetRelationsBulkAction BulkAction { get; set; }

        public bool IsRestrictedLocations { get; set; }

        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public int? StreetId { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }

        public int? RelatedProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> RelatedProvinces { get; set; }
        public int? RelatedDistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> RelatedDistricts { get; set; }
        public int? RelatedWardId { get; set; }
        public IEnumerable<LocationWardPart> RelatedWards { get; set; }
        public int? RelatedStreetId { get; set; }
        public IEnumerable<LocationStreetPart> RelatedStreets { get; set; }
    }

    public enum StreetRelationsOrder
    {
        Province,
        District,
        Ward,
        Street,
        RelatedProvince,
        RelatedDistrict,
        RelatedWard,
        RelatedStreet
    }

    public enum StreetRelationsFilter
    {
        All
    }

    public enum StreetRelationsBulkAction
    {
        None,
        Delete,
    }
}
