using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PropertyInteriorViewModel
    {
    }

    public class PropertyInteriorCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public float UnitPrice { get; set; }

        public IContent PropertyInterior { get; set; }
    }

    public class PropertyInteriorEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyInterior.As<PropertyInteriorPart>().Name; }
            set { PropertyInterior.As<PropertyInteriorPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyInterior.As<PropertyInteriorPart>().ShortName; }
            set { PropertyInterior.As<PropertyInteriorPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyInterior.As<PropertyInteriorPart>().CssClass; }
            set { PropertyInterior.As<PropertyInteriorPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyInterior.As<PropertyInteriorPart>().SeqOrder; }
            set { PropertyInterior.As<PropertyInteriorPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyInterior.As<PropertyInteriorPart>().IsEnabled; }
            set { PropertyInterior.As<PropertyInteriorPart>().IsEnabled = value; }
        }

        public float UnitPrice
        {
            get { return PropertyInterior.As<PropertyInteriorPart>().UnitPrice; }
            set { PropertyInterior.As<PropertyInteriorPart>().UnitPrice = value; }
        }

        public IContent PropertyInterior { get; set; }
    }

    public class PropertyInteriorsIndexViewModel
    {
        public IList<PropertyInteriorEntry> PropertyInteriors { get; set; }
        public PropertyInteriorIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyInteriorEntry
    {
        public PropertyInteriorPartRecord PropertyInterior { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyInteriorIndexOptions
    {
        public string Search { get; set; }
        public PropertyInteriorsOrder Order { get; set; }
        public PropertyInteriorsFilter Filter { get; set; }
        public PropertyInteriorsBulkAction BulkAction { get; set; }
    }

    public enum PropertyInteriorsOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyInteriorsFilter
    {
        All
    }

    public enum PropertyInteriorsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
