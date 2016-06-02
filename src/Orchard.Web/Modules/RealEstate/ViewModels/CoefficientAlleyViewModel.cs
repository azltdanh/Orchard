using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CoefficientAlleyViewModel
    {
    }

    public class CoefficientAlleyCreateViewModel
    {
        [Required]
        public double StreetUnitPrice { get; set; }

        [Required]
        public double CoefficientAlley1Max { get; set; }

        [Required]
        public double CoefficientAlley1Min { get; set; }

        [Required]
        public double CoefficientAlleyMax { get; set; }

        [Required]
        public double CoefficientAlleyMin { get; set; }

        [Required]
        public double CoefficientAlleyEqual { get; set; }

        public IContent CoefficientAlley { get; set; }
    }

    public class CoefficientAlleyEditViewModel
    {
        [Required]
        public double StreetUnitPrice
        {
            get { return CoefficientAlley.As<CoefficientAlleyPart>().StreetUnitPrice; }
            set { CoefficientAlley.As<CoefficientAlleyPart>().StreetUnitPrice = value; }
        }

        [Required]
        public double CoefficientAlley1Max
        {
            get { return CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlley1Max; }
            set { CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlley1Max = value; }
        }

        [Required]
        public double CoefficientAlley1Min
        {
            get { return CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlley1Min; }
            set { CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlley1Min = value; }
        }

        [Required]
        public double CoefficientAlleyMax
        {
            get { return CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlleyMax; }
            set { CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlleyMax = value; }
        }

        [Required]
        public double CoefficientAlleyMin
        {
            get { return CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlleyMin; }
            set { CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlleyMin = value; }
        }

        [Required]
        public double CoefficientAlleyEqual
        {
            get { return CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlleyEqual; }
            set { CoefficientAlley.As<CoefficientAlleyPart>().CoefficientAlleyEqual = value; }
        }

        public IContent CoefficientAlley { get; set; }
    }

    public class CoefficientAlleysIndexViewModel
    {
        public IList<CoefficientAlleyEntry> CoefficientAlleys { get; set; }
        public CoefficientAlleyIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CoefficientAlleyEntry
    {
        public CoefficientAlleyPartRecord CoefficientAlley { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CoefficientAlleyIndexOptions
    {
        public string Search { get; set; }
        public CoefficientAlleysOrder Order { get; set; }
        public CoefficientAlleysFilter Filter { get; set; }
        public CoefficientAlleysBulkAction BulkAction { get; set; }
    }

    public enum CoefficientAlleysOrder
    {
        SeqOrder,
        Name
    }

    public enum CoefficientAlleysFilter
    {
        All
    }

    public enum CoefficientAlleysBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}