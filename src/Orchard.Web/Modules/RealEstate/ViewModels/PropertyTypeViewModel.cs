using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PropertyTypeViewModel
    {
    }

    public class PropertyTypeCreateViewModel
    {
        [Required]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupShortName { get; set; }

        public IEnumerable<PropertyTypeGroupPartRecord> Groups { get; set; }

        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public float UnitPrice { get; set; }

        public string DefaultImgUrl { get; set; }

        public IContent PropertyType { get; set; }

    }

    public class PropertyTypeEditViewModel
    {
        [Required]
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupShortName { get; set; }

        public IEnumerable<PropertyTypeGroupPartRecord> Groups { get; set; }

        [Required]
        public string Name
        {
            get { return PropertyType.As<PropertyTypePart>().Name; }
            set { PropertyType.As<PropertyTypePart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyType.As<PropertyTypePart>().ShortName; }
            set { PropertyType.As<PropertyTypePart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyType.As<PropertyTypePart>().CssClass; }
            set { PropertyType.As<PropertyTypePart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyType.As<PropertyTypePart>().SeqOrder; }
            set { PropertyType.As<PropertyTypePart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyType.As<PropertyTypePart>().IsEnabled; }
            set { PropertyType.As<PropertyTypePart>().IsEnabled = value; }
        }

        public float UnitPrice
        {
            get { return PropertyType.As<PropertyTypePart>().UnitPrice; }
            set { PropertyType.As<PropertyTypePart>().UnitPrice = value; }
        }

        public string DefaultImgUrl
        {
            get { return PropertyType.As<PropertyTypePart>().DefaultImgUrl; }
            set { PropertyType.As<PropertyTypePart>().DefaultImgUrl = value; }
        }

        public IContent PropertyType { get; set; }
    }

    public class PropertyTypesIndexViewModel
    {
        public IList<PropertyTypeEntry> PropertyTypes { get; set; }
        public PropertyTypeIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyTypeEntry
    {
        public PropertyTypePartRecord PropertyType { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyTypeIndexOptions
    {
        public string Search { get; set; }
        public PropertyTypesOrder Order { get; set; }
        public PropertyTypesFilter Filter { get; set; }
        public PropertyTypesBulkAction BulkAction { get; set; }
        public int GroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> Groups { get; set; }
    }

    public enum PropertyTypesOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyTypesFilter
    {
        All
    }

    public enum PropertyTypesBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
