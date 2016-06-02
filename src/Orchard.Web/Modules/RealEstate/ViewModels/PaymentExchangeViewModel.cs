using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class PaymentExchangeViewModel
    {
    }

    public class PaymentExchangeCreateViewModel
    {
        [Required]
        public float USD { get; set; }

        [Required]
        public float SJC { get; set; }

        [Required]
        public System.DateTime Date { get; set; }

        public IContent PaymentExchange { get; set; }
    }

    public class PaymentExchangeEditViewModel
    {
        [Required]
        public float USD
        {
            get { return PaymentExchange.As<PaymentExchangePart>().USD; }
            set { PaymentExchange.As<PaymentExchangePart>().USD = value; }
        }

        public float SJC
        {
            get { return PaymentExchange.As<PaymentExchangePart>().SJC; }
            set { PaymentExchange.As<PaymentExchangePart>().SJC = value; }
        }

        public System.DateTime Date
        {
            get { return PaymentExchange.As<PaymentExchangePart>().Date; }
            set { PaymentExchange.As<PaymentExchangePart>().Date = value; }
        }

        public IContent PaymentExchange { get; set; }
    }

    public class PaymentExchangesIndexViewModel
    {
        public IList<PaymentExchangeEntry> PaymentExchanges { get; set; }
        public PaymentExchangeIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class PaymentExchangeEntry
    {
        public PaymentExchangePartRecord PaymentExchange { get; set; }
        public bool IsChecked { get; set; }
    }

    public class PaymentExchangeIndexOptions
    {
        public string Search { get; set; }
        public PaymentExchangesOrder Order { get; set; }
        public PaymentExchangesFilter Filter { get; set; }
        public PaymentExchangesBulkAction BulkAction { get; set; }
    }

    public enum PaymentExchangesOrder
    {
        SeqOrder,
        Name
    }

    public enum PaymentExchangesFilter
    {
        All
    }

    public enum PaymentExchangesBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
