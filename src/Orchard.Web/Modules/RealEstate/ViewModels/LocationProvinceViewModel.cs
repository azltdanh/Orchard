using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class LocationProvinceViewModel
    {
    }

    public class LocationProvinceCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent LocationProvince { get; set; }
    }

    public class LocationProvinceEditViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        public string Name
        {
            get { return LocationProvince.As<LocationProvincePart>().Name; }
            set { LocationProvince.As<LocationProvincePart>().Name = value; }
        }

        [Required]
        public string ShortName
        {
            get { return LocationProvince.As<LocationProvincePart>().ShortName; }
            set { LocationProvince.As<LocationProvincePart>().ShortName = value; }
        }

        public int SeqOrder
        {
            get { return LocationProvince.As<LocationProvincePart>().SeqOrder; }
            set { LocationProvince.As<LocationProvincePart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return LocationProvince.As<LocationProvincePart>().IsEnabled; }
            set { LocationProvince.As<LocationProvincePart>().IsEnabled = value; }
        }

        public IContent LocationProvince { get; set; }
    }

    public class LocationProvincesIndexViewModel
    {
        public IList<LocationProvinceEntry> LocationProvinces { get; set; }
        public LocationProvinceIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class LocationProvinceEntry
    {
        public LocationProvincePart LocationProvince { get; set; }
        public bool IsChecked { get; set; }
    }

    public class LocationProvinceIndexOptions
    {
        public string Search { get; set; }
        public LocationProvincesOrder Order { get; set; }
        public LocationProvincesFilter Filter { get; set; }
        public LocationProvincesBulkAction BulkAction { get; set; }
    }

    public enum LocationProvincesOrder
    {
        SeqOrder,
        Name
    }

    public enum LocationProvincesFilter
    {
        All
    }

    public enum LocationProvincesBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
