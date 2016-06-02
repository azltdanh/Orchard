using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PaymentMethodViewModel
    {
    }

    public class PaymentMethodCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PaymentMethod { get; set; }
    }

    public class PaymentMethodEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PaymentMethod.As<PaymentMethodPart>().Name; }
            set { PaymentMethod.As<PaymentMethodPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PaymentMethod.As<PaymentMethodPart>().ShortName; }
            set { PaymentMethod.As<PaymentMethodPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PaymentMethod.As<PaymentMethodPart>().CssClass; }
            set { PaymentMethod.As<PaymentMethodPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PaymentMethod.As<PaymentMethodPart>().SeqOrder; }
            set { PaymentMethod.As<PaymentMethodPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PaymentMethod.As<PaymentMethodPart>().IsEnabled; }
            set { PaymentMethod.As<PaymentMethodPart>().IsEnabled = value; }
        }

        public IContent PaymentMethod { get; set; }
    }

    public class PaymentMethodsIndexViewModel
    {
        public IList<PaymentMethodEntry> PaymentMethods { get; set; }
        public PaymentMethodIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PaymentMethodEntry
    {
        public PaymentMethodPartRecord PaymentMethod { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PaymentMethodIndexOptions
    {
        public string Search { get; set; }
        public PaymentMethodsOrder Order { get; set; }
        public PaymentMethodsFilter Filter { get; set; }
        public PaymentMethodsBulkAction BulkAction { get; set; }
    }

    public enum PaymentMethodsOrder
    {
        SeqOrder,
        Name
    }

    public enum PaymentMethodsFilter
    {
        All
    }

    public enum PaymentMethodsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
