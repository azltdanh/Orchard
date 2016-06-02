using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PlanningMapViewModel
    {
    }

    public class PlanningMapCreateViewModel
    {
        public string ReturnUrl { get; set; }

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
        public string ImagesPath { get; set; }

        [Required]
        public int? Width { get; set; }
        [Required]
        public int? Height { get; set; }
        [Required]
        public int? MinZoom { get; set; }
        [Required]
        public int? MaxZoom { get; set; }
        [Required]
        public double? Ratio { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PlanningMap { get; set; }
    }

    public class PlanningMapEditViewModel
    {
        public string ReturnUrl { get; set; }

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
        public string ImagesPath
        {
            get { return PlanningMap.ImagesPath; }
            set { PlanningMap.ImagesPath = value; }
        }

        [Required]
        public int Width
        {
            get { return PlanningMap.Width; }
            set { PlanningMap.Width = value; }
        }
        [Required]
        public int Height
        {
            get { return PlanningMap.Height; }
            set { PlanningMap.Height = value; }
        }
        [Required]
        public int MinZoom
        {
            get { return PlanningMap.MinZoom; }
            set { PlanningMap.MinZoom = value; }
        }
        [Required]
        public int MaxZoom
        {
            get { return PlanningMap.MaxZoom; }
            set { PlanningMap.MaxZoom = value; }
        }
        [Required]
        public double Ratio
        {
            get { return PlanningMap.Ratio; }
            set { PlanningMap.Ratio = value; }
        }

        public bool IsEnabled
        {
            get { return PlanningMap.IsEnabled; }
            set { PlanningMap.IsEnabled = value; }
        }

        public PlanningMapRecord PlanningMap { get; set; }
    }

    public class PlanningMapsIndexViewModel
    {
        public IList<PlanningMapEntry> PlanningMaps { get; set; }
        public PlanningMapIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PlanningMapEntry
    {
        public PlanningMapRecord PlanningMap { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PlanningMapIndexOptions
    {
        public string Search { get; set; }
        public PlanningMapBulkAction BulkAction { get; set; }
        public int? ProvinceId { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
    }

    public enum PlanningMapBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
