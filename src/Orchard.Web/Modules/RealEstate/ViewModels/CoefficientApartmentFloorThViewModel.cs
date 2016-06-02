using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CoefficientApartmentFloorThViewModel
    {
    }

    public class CoefficientApartmentFloorThCreateViewModel
    {
        public double? MaxFloors { get; set; }

        [Required]
        public double ApartmentFloorTh { get; set; }

        [Required]
        public double CoefficientApartmentFloorTh { get; set; }

        public IContent CoefficientApartmentFloorThPart { get; set; }
    }

    public class CoefficientApartmentFloorThEditViewModel
    {
        public double? MaxFloors
        {
            get { return CoefficientApartmentFloorThPart.As<CoefficientApartmentFloorThPart>().MaxFloors; }
            set { CoefficientApartmentFloorThPart.As<CoefficientApartmentFloorThPart>().MaxFloors = value; }
        }

        [Required]
        public double ApartmentFloorTh
        {
            get { return CoefficientApartmentFloorThPart.As<CoefficientApartmentFloorThPart>().ApartmentFloorTh; }
            set { CoefficientApartmentFloorThPart.As<CoefficientApartmentFloorThPart>().ApartmentFloorTh = value; }
        }

        [Required]
        public double CoefficientApartmentFloorTh
        {
            get
            {
                return CoefficientApartmentFloorThPart.As<CoefficientApartmentFloorThPart>().CoefficientApartmentFloorTh;
            }
            set
            {
                CoefficientApartmentFloorThPart.As<CoefficientApartmentFloorThPart>().CoefficientApartmentFloorTh = value;
            }
        }

        public IContent CoefficientApartmentFloorThPart { get; set; }
    }

    public class CoefficientApartmentFloorThIndexViewModel
    {
        public IList<CoefficientApartmentFloorThEntry> CoefficientApartmentFloorThs { get; set; }
        public CoefficientApartmentFloorThIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CoefficientApartmentFloorThEntry
    {
        public CoefficientApartmentFloorThPart CoefficientApartmentFloorThPart { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CoefficientApartmentFloorThIndexOptions
    {
        public string Search { get; set; }
        public CoefficientApartmentFloorThOrder Order { get; set; }
        public CoefficientApartmentFloorThFilter Filter { get; set; }
        public CoefficientApartmentFloorThBulkAction BulkAction { get; set; }
    }

    public enum CoefficientApartmentFloorThOrder
    {
        SeqOrder,
        Name
    }

    public enum CoefficientApartmentFloorThFilter
    {
        All
    }

    public enum CoefficientApartmentFloorThBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}