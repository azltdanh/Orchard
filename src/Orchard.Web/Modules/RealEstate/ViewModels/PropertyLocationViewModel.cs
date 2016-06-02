using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PropertyLocationViewModel
    {
    }

    public class PropertyLocationCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PropertyLocation { get; set; }
    }

    public class PropertyLocationEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyLocation.As<PropertyLocationPart>().Name; }
            set { PropertyLocation.As<PropertyLocationPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyLocation.As<PropertyLocationPart>().ShortName; }
            set { PropertyLocation.As<PropertyLocationPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyLocation.As<PropertyLocationPart>().CssClass; }
            set { PropertyLocation.As<PropertyLocationPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyLocation.As<PropertyLocationPart>().SeqOrder; }
            set { PropertyLocation.As<PropertyLocationPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyLocation.As<PropertyLocationPart>().IsEnabled; }
            set { PropertyLocation.As<PropertyLocationPart>().IsEnabled = value; }
        }

        public IContent PropertyLocation { get; set; }
    }

    public class PropertyLocationsIndexViewModel
    {
        public IList<PropertyLocationEntry> PropertyLocations { get; set; }
        public PropertyLocationIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyLocationEntry
    {
        public PropertyLocationPartRecord PropertyLocation { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyLocationIndexOptions
    {
        public string Search { get; set; }
        public PropertyLocationsOrder Order { get; set; }
        public PropertyLocationsFilter Filter { get; set; }
        public PropertyLocationsBulkAction BulkAction { get; set; }
    }

    public enum PropertyLocationsOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyLocationsFilter
    {
        All
    }

    public enum PropertyLocationsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
