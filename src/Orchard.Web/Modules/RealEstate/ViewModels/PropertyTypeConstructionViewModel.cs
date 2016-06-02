using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PropertyTypeConstructionViewModel
    {
    }

    public class PropertyTypeConstructionCreateViewModel
    {
        [Required]
        public int PropertyGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> PropertyGroups { get; set; }

        [Required]
        public int PropertyTypeId { get; set; }
        public IEnumerable<PropertyTypePartRecord> PropertyTypes { get; set; }

        public int? MinFloor { get; set; }

        public int? MaxFloor { get; set; }

        public bool IsDefaultInFloorsRange { get; set; }

        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public float UnitPrice { get; set; }

        public string DefaultImgUrl { get; set; }

        public IContent PropertyTypeConstruction { get; set; }

    }

    public class PropertyTypeConstructionEditViewModel
    {
        [Required]
        public int PropertyGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> PropertyGroups { get; set; }

        [Required]
        public int PropertyTypeId { get; set; }
        public IEnumerable<PropertyTypePartRecord> PropertyTypes { get; set; }

        public int? MinFloor
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().MinFloor; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().MinFloor = value; }
        }

        public int? MaxFloor
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().MaxFloor; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().MaxFloor = value; }
        }

        public bool IsDefaultInFloorsRange
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().IsDefaultInFloorsRange; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().IsDefaultInFloorsRange = value; }
        }

        [Required]
        public string Name
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().Name; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().ShortName; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().CssClass; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().SeqOrder; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().IsEnabled; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().IsEnabled = value; }
        }

        public float UnitPrice
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().UnitPrice; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().UnitPrice = value; }
        }

        public string DefaultImgUrl
        {
            get { return PropertyTypeConstruction.As<PropertyTypeConstructionPart>().DefaultImgUrl; }
            set { PropertyTypeConstruction.As<PropertyTypeConstructionPart>().DefaultImgUrl = value; }
        }

        public IContent PropertyTypeConstruction { get; set; }
    }

    public class PropertyTypeConstructionsIndexViewModel
    {
        public IList<PropertyTypeConstructionEntry> PropertyTypeConstructions { get; set; }
        public PropertyTypeConstructionIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyTypeConstructionEntry
    {
        public PropertyTypeConstructionPartRecord PropertyTypeConstruction { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyTypeConstructionIndexOptions
    {
        public string Search { get; set; }
        public PropertyTypeConstructionsOrder Order { get; set; }
        public PropertyTypeConstructionsFilter Filter { get; set; }
        public PropertyTypeConstructionsBulkAction BulkAction { get; set; }
        public int PropertyGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> PropertyGroups { get; set; }
        public int PropertyTypeId { get; set; }
        public IEnumerable<PropertyTypePartRecord> PropertyTypes { get; set; }
    }

    public enum PropertyTypeConstructionsOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyTypeConstructionsFilter
    {
        All
    }

    public enum PropertyTypeConstructionsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
