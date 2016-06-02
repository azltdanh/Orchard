using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PropertyFlagViewModel
    {
    }

    public class PropertyFlagCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public float Value { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PropertyFlag { get; set; }
    }

    public class PropertyFlagEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyFlag.As<PropertyFlagPart>().Name; }
            set { PropertyFlag.As<PropertyFlagPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyFlag.As<PropertyFlagPart>().ShortName; }
            set { PropertyFlag.As<PropertyFlagPart>().ShortName = value; }
        }

        public float Value
        {
            get { return PropertyFlag.As<PropertyFlagPart>().Value; }
            set { PropertyFlag.As<PropertyFlagPart>().Value = value; }
        }

        public string CssClass
        {
            get { return PropertyFlag.As<PropertyFlagPart>().CssClass; }
            set { PropertyFlag.As<PropertyFlagPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyFlag.As<PropertyFlagPart>().SeqOrder; }
            set { PropertyFlag.As<PropertyFlagPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyFlag.As<PropertyFlagPart>().IsEnabled; }
            set { PropertyFlag.As<PropertyFlagPart>().IsEnabled = value; }
        }

        public IContent PropertyFlag { get; set; }
    }

    public class PropertyFlagsIndexViewModel
    {
        public IList<PropertyFlagEntry> PropertyFlags { get; set; }
        public PropertyFlagIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyFlagEntry
    {
        public PropertyFlagPartRecord PropertyFlag { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyFlagIndexOptions
    {
        public string Search { get; set; }
        public PropertyFlagsOrder Order { get; set; }
        public PropertyFlagsFilter Filter { get; set; }
        public PropertyFlagsBulkAction BulkAction { get; set; }
    }

    public enum PropertyFlagsOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyFlagsFilter
    {
        All
    }

    public enum PropertyFlagsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
