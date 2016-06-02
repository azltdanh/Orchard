using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PropertyTypeGroupViewModel
    {
    }

    public class PropertyTypeGroupCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PropertyTypeGroup { get; set; }
    }

    public class PropertyTypeGroupEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyTypeGroup.As<PropertyTypeGroupPart>().Name; }
            set { PropertyTypeGroup.As<PropertyTypeGroupPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyTypeGroup.As<PropertyTypeGroupPart>().ShortName; }
            set { PropertyTypeGroup.As<PropertyTypeGroupPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyTypeGroup.As<PropertyTypeGroupPart>().CssClass; }
            set { PropertyTypeGroup.As<PropertyTypeGroupPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyTypeGroup.As<PropertyTypeGroupPart>().SeqOrder; }
            set { PropertyTypeGroup.As<PropertyTypeGroupPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyTypeGroup.As<PropertyTypeGroupPart>().IsEnabled; }
            set { PropertyTypeGroup.As<PropertyTypeGroupPart>().IsEnabled = value; }
        }

        public IContent PropertyTypeGroup { get; set; }
    }

    public class PropertyTypeGroupsIndexViewModel
    {
        public IList<PropertyTypeGroupEntry> PropertyTypeGroups { get; set; }
        public PropertyTypeGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyTypeGroupEntry
    {
        public PropertyTypeGroupPartRecord PropertyTypeGroup { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyTypeGroupIndexOptions
    {
        public string Search { get; set; }
        public PropertyTypeGroupsOrder Order { get; set; }
        public PropertyTypeGroupsFilter Filter { get; set; }
        public PropertyTypeGroupsBulkAction BulkAction { get; set; }
    }

    public enum PropertyTypeGroupsOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyTypeGroupsFilter
    {
        All
    }

    public enum PropertyTypeGroupsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
