using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PropertyStatusViewModel
    {
    }

    public class PropertyStatusCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PropertyStatus { get; set; }
    }

    public class PropertyStatusEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyStatus.As<PropertyStatusPart>().Name; }
            set { PropertyStatus.As<PropertyStatusPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyStatus.As<PropertyStatusPart>().ShortName; }
            set { PropertyStatus.As<PropertyStatusPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyStatus.As<PropertyStatusPart>().CssClass; }
            set { PropertyStatus.As<PropertyStatusPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyStatus.As<PropertyStatusPart>().SeqOrder; }
            set { PropertyStatus.As<PropertyStatusPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyStatus.As<PropertyStatusPart>().IsEnabled; }
            set { PropertyStatus.As<PropertyStatusPart>().IsEnabled = value; }
        }

        public IContent PropertyStatus { get; set; }
    }

    public class PropertyStatusIndexViewModel
    {
        public IList<PropertyStatusEntry> PropertyStatus { get; set; }
        public PropertyStatusIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyStatusEntry
    {
        public PropertyStatusPartRecord PropertyStatus { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyStatusIndexOptions
    {
        public string Search { get; set; }
        public PropertyStatusOrder Order { get; set; }
        public PropertyStatusFilter Filter { get; set; }
        public PropertyStatusBulkAction BulkAction { get; set; }
    }

    public enum PropertyStatusOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyStatusFilter
    {
        All
    }

    public enum PropertyStatusBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
