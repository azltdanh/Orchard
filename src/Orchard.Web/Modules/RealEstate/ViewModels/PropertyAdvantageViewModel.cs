using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PropertyAdvantageViewModel
    {
    }

    public class PropertyAdvantageCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public double AddedValue { get; set; }

        public IContent PropertyAdvantage { get; set; }
    }

    public class PropertyAdvantageEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyAdvantage.As<PropertyAdvantagePart>().Name; }
            set { PropertyAdvantage.As<PropertyAdvantagePart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyAdvantage.As<PropertyAdvantagePart>().ShortName; }
            set { PropertyAdvantage.As<PropertyAdvantagePart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyAdvantage.As<PropertyAdvantagePart>().CssClass; }
            set { PropertyAdvantage.As<PropertyAdvantagePart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyAdvantage.As<PropertyAdvantagePart>().SeqOrder; }
            set { PropertyAdvantage.As<PropertyAdvantagePart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyAdvantage.As<PropertyAdvantagePart>().IsEnabled; }
            set { PropertyAdvantage.As<PropertyAdvantagePart>().IsEnabled = value; }
        }

        public double AddedValue
        {
            get { return PropertyAdvantage.As<PropertyAdvantagePart>().AddedValue; }
            set { PropertyAdvantage.As<PropertyAdvantagePart>().AddedValue = value; }
        }

        public IContent PropertyAdvantage { get; set; }
    }

    public class PropertyAdvantagesIndexViewModel
    {
        public IList<PropertyAdvantageEntry> PropertyAdvantages { get; set; }
        public PropertyAdvantageIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyAdvantageEntry
    {
        public PropertyAdvantagePartRecord Advantage { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyAdvantageItem
    {
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }
    }

    public class PropertyAdvantageIndexOptions
    {
        public string Search { get; set; }
        public PropertyAdvantagesOrder Order { get; set; }
        public PropertyAdvantagesFilter Filter { get; set; }
        public PropertyAdvantagesBulkAction BulkAction { get; set; }
    }

    public enum PropertyAdvantagesOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyAdvantagesFilter
    {
        Advantages,
        DisAdvantages,
        ApartmentAdvantages,
        ApartmentInteriorAdvantages,
        ConstructionAdvantages,
        All
    }

    public enum PropertyAdvantagesBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
