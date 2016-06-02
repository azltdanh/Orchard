using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CoefficientAlleyDistanceViewModel
    {
    }

    public class CoefficientAlleyDistanceCreateViewModel
    {
        [Required]
        public double LastAlleyWidth { get; set; }

        [Required]
        public double MaxAlleyDistance { get; set; }

        [Required]
        public double CoefficientDistance { get; set; }

        public IContent CoefficientAlleyDistance { get; set; }
    }

    public class CoefficientAlleyDistanceEditViewModel
    {
        [Required]
        public double LastAlleyWidth
        {
            get { return CoefficientAlleyDistance.As<CoefficientAlleyDistancePart>().LastAlleyWidth; }
            set { CoefficientAlleyDistance.As<CoefficientAlleyDistancePart>().LastAlleyWidth = value; }
        }

        [Required]
        public double MaxAlleyDistance
        {
            get { return CoefficientAlleyDistance.As<CoefficientAlleyDistancePart>().MaxAlleyDistance; }
            set { CoefficientAlleyDistance.As<CoefficientAlleyDistancePart>().MaxAlleyDistance = value; }
        }

        [Required]
        public double CoefficientDistance
        {
            get { return CoefficientAlleyDistance.As<CoefficientAlleyDistancePart>().CoefficientDistance; }
            set { CoefficientAlleyDistance.As<CoefficientAlleyDistancePart>().CoefficientDistance = value; }
        }

        public IContent CoefficientAlleyDistance { get; set; }
    }

    public class CoefficientAlleyDistancesIndexViewModel
    {
        public IList<CoefficientAlleyDistanceEntry> CoefficientAlleyDistances { get; set; }
        public CoefficientAlleyDistanceIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CoefficientAlleyDistanceEntry
    {
        public CoefficientAlleyDistancePartRecord CoefficientAlleyDistance { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CoefficientAlleyDistanceIndexOptions
    {
        public string Search { get; set; }
        public CoefficientAlleyDistancesOrder Order { get; set; }
        public CoefficientAlleyDistancesFilter Filter { get; set; }
        public CoefficientAlleyDistancesBulkAction BulkAction { get; set; }
    }

    public enum CoefficientAlleyDistancesOrder
    {
        SeqOrder,
        Name
    }

    public enum CoefficientAlleyDistancesFilter
    {
        All
    }

    public enum CoefficientAlleyDistancesBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}