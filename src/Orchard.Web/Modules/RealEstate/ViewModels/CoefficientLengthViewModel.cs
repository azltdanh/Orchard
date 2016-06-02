using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CoefficientLengthViewModel
    {
    }

    public class CoefficientLengthCreateViewModel
    {
        [Required]
        public double WidthRange { get; set; }

        [Required]
        public double MinLength { get; set; }

        [Required]
        public double MaxLength { get; set; }

        public IContent CoefficientLength { get; set; }
    }

    public class CoefficientLengthEditViewModel
    {
        [Required]
        public double WidthRange
        {
            get { return CoefficientLength.As<CoefficientLengthPart>().WidthRange; }
            set { CoefficientLength.As<CoefficientLengthPart>().WidthRange = value; }
        }

        [Required]
        public double MinLength
        {
            get { return CoefficientLength.As<CoefficientLengthPart>().MinLength; }
            set { CoefficientLength.As<CoefficientLengthPart>().MinLength = value; }
        }

        [Required]
        public double MaxLength
        {
            get { return CoefficientLength.As<CoefficientLengthPart>().MaxLength; }
            set { CoefficientLength.As<CoefficientLengthPart>().MaxLength = value; }
        }

        public IContent CoefficientLength { get; set; }
    }

    public class CoefficientLengthsIndexViewModel
    {
        public IList<CoefficientLengthEntry> CoefficientLengths { get; set; }
        public CoefficientLengthIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CoefficientLengthEntry
    {
        public CoefficientLengthPartRecord CoefficientLength { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CoefficientLengthIndexOptions
    {
        public string Search { get; set; }
        public CoefficientLengthsOrder Order { get; set; }
        public CoefficientLengthsFilter Filter { get; set; }
        public CoefficientLengthsBulkAction BulkAction { get; set; }
    }

    public enum CoefficientLengthsOrder
    {
        SeqOrder,
        Name
    }

    public enum CoefficientLengthsFilter
    {
        All
    }

    public enum CoefficientLengthsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}