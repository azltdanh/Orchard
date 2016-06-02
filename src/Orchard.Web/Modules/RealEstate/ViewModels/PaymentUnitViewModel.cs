using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class PaymentUnitViewModel
    {
    }

    public class PaymentUnitCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent PaymentUnit { get; set; }
    }

    public class PaymentUnitEditViewModel
    {
        [Required]
        public string Name
        {
            get { return PaymentUnit.As<PaymentUnitPart>().Name; }
            set { PaymentUnit.As<PaymentUnitPart>().Name = value; }
        }

        public string ShortName
        {
            get { return PaymentUnit.As<PaymentUnitPart>().ShortName; }
            set { PaymentUnit.As<PaymentUnitPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return PaymentUnit.As<PaymentUnitPart>().CssClass; }
            set { PaymentUnit.As<PaymentUnitPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return PaymentUnit.As<PaymentUnitPart>().SeqOrder; }
            set { PaymentUnit.As<PaymentUnitPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return PaymentUnit.As<PaymentUnitPart>().IsEnabled; }
            set { PaymentUnit.As<PaymentUnitPart>().IsEnabled = value; }
        }

        public IContent PaymentUnit { get; set; }
    }

    public class PaymentUnitsIndexViewModel
    {
        public IList<PaymentUnitEntry> PaymentUnits { get; set; }
        public PaymentUnitIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PaymentUnitEntry
    {
        public PaymentUnitPartRecord PaymentUnit { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PaymentUnitIndexOptions
    {
        public string Search { get; set; }
        public PaymentUnitsOrder Order { get; set; }
        public PaymentUnitsFilter Filter { get; set; }
        public PaymentUnitsBulkAction BulkAction { get; set; }
    }

    public enum PaymentUnitsOrder
    {
        SeqOrder,
        Name
    }

    public enum PaymentUnitsFilter
    {
        All
    }

    public enum PaymentUnitsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
