using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CustomerPurposeViewModel
    {
    }

    public class CustomerPurposeCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent CustomerPurpose { get; set; }
    }

    public class CustomerPurposeEditViewModel
    {
        [Required]
        public string Name
        {
            get { return CustomerPurpose.As<CustomerPurposePart>().Name; }
            set { CustomerPurpose.As<CustomerPurposePart>().Name = value; }
        }

        public string ShortName
        {
            get { return CustomerPurpose.As<CustomerPurposePart>().ShortName; }
            set { CustomerPurpose.As<CustomerPurposePart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return CustomerPurpose.As<CustomerPurposePart>().CssClass; }
            set { CustomerPurpose.As<CustomerPurposePart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return CustomerPurpose.As<CustomerPurposePart>().SeqOrder; }
            set { CustomerPurpose.As<CustomerPurposePart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return CustomerPurpose.As<CustomerPurposePart>().IsEnabled; }
            set { CustomerPurpose.As<CustomerPurposePart>().IsEnabled = value; }
        }

        public IContent CustomerPurpose { get; set; }
    }

    public class CustomerPurposeIndexViewModel
    {
        public IList<CustomerPurposeEntry> Purposes { get; set; }
        public CustomerPurposeIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CustomerPurposeEntry
    {
        public CustomerPurposePartRecord Purpose { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CustomerPurposeIndexOptions
    {
        public string Search { get; set; }
        public CustomerPurposeOrder Order { get; set; }
        public CustomerPurposeFilter Filter { get; set; }
        public CustomerPurposeBulkAction BulkAction { get; set; }
    }

    public enum CustomerPurposeOrder
    {
        SeqOrder,
        Name
    }

    public enum CustomerPurposeFilter
    {
        All
    }

    public enum CustomerPurposeBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}