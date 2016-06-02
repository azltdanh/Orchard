using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CustomerStatusViewModel
    {
    }

    public class CustomerStatusCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent CustomerStatus { get; set; }
    }

    public class CustomerStatusEditViewModel
    {
        [Required]
        public string Name
        {
            get { return CustomerStatus.As<CustomerStatusPart>().Name; }
            set { CustomerStatus.As<CustomerStatusPart>().Name = value; }
        }

        public string ShortName
        {
            get { return CustomerStatus.As<CustomerStatusPart>().ShortName; }
            set { CustomerStatus.As<CustomerStatusPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return CustomerStatus.As<CustomerStatusPart>().CssClass; }
            set { CustomerStatus.As<CustomerStatusPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return CustomerStatus.As<CustomerStatusPart>().SeqOrder; }
            set { CustomerStatus.As<CustomerStatusPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return CustomerStatus.As<CustomerStatusPart>().IsEnabled; }
            set { CustomerStatus.As<CustomerStatusPart>().IsEnabled = value; }
        }

        public IContent CustomerStatus { get; set; }
    }

    public class CustomerStatusIndexViewModel
    {
        public IList<CustomerStatusEntry> CustomerStatus { get; set; }
        public CustomerStatusIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CustomerStatusEntry
    {
        public CustomerStatusPartRecord CustomerStatus { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CustomerStatusIndexOptions
    {
        public string Search { get; set; }
        public CustomerStatusOrder Order { get; set; }
        public CustomerStatusFilter Filter { get; set; }
        public CustomerStatusBulkAction BulkAction { get; set; }
    }

    public enum CustomerStatusOrder
    {
        SeqOrder,
        Name
    }

    public enum CustomerStatusFilter
    {
        All
    }

    public enum CustomerStatusBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}