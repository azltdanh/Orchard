using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PropertyLegalStatusViewModel
    {
    }

    public class PropertyLegalStatusCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PropertyLegalStatus { get; set; }
    }

    public class PropertyLegalStatusEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PropertyLegalStatus.As<PropertyLegalStatusPart>().Name; }
            set { PropertyLegalStatus.As<PropertyLegalStatusPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PropertyLegalStatus.As<PropertyLegalStatusPart>().ShortName; }
            set { PropertyLegalStatus.As<PropertyLegalStatusPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PropertyLegalStatus.As<PropertyLegalStatusPart>().CssClass; }
            set { PropertyLegalStatus.As<PropertyLegalStatusPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PropertyLegalStatus.As<PropertyLegalStatusPart>().SeqOrder; }
            set { PropertyLegalStatus.As<PropertyLegalStatusPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PropertyLegalStatus.As<PropertyLegalStatusPart>().IsEnabled; }
            set { PropertyLegalStatus.As<PropertyLegalStatusPart>().IsEnabled = value; }
        }

        public IContent PropertyLegalStatus { get; set; }
    }

    public class PropertyLegalStatusIndexViewModel
    {
        public IList<PropertyLegalStatusEntry> PropertyLegalStatus { get; set; }
        public PropertyLegalStatusIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PropertyLegalStatusEntry
    {
        public PropertyLegalStatusPartRecord PropertyLegalStatus { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PropertyLegalStatusIndexOptions
    {
        public string Search { get; set; }
        public PropertyLegalStatusOrder Order { get; set; }
        public PropertyLegalStatusFilter Filter { get; set; }
        public PropertyLegalStatusBulkAction BulkAction { get; set; }
    }

    public enum PropertyLegalStatusOrder
    {
        SeqOrder,
        Name
    }

    public enum PropertyLegalStatusFilter
    {
        All
    }

    public enum PropertyLegalStatusBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
