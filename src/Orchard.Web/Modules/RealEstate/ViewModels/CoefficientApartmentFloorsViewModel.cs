using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CoefficientApartmentFloorsViewModel
    {
    }

    public class CoefficientApartmentFloorsCreateViewModel
    {
        [Required]
        public double Floors { get; set; }

        [Required]
        public double CoefficientApartmentFloors { get; set; }

        public IContent CoefficientApartmentFloorsPart { get; set; }
    }

    public class CoefficientApartmentFloorsEditViewModel
    {
        [Required]
        public double Floors
        {
            get { return CoefficientApartmentFloorsPart.As<CoefficientApartmentFloorsPart>().Floors; }
            set { CoefficientApartmentFloorsPart.As<CoefficientApartmentFloorsPart>().Floors = value; }
        }

        [Required]
        public double CoefficientApartmentFloors
        {
            get
            {
                return CoefficientApartmentFloorsPart.As<CoefficientApartmentFloorsPart>().CoefficientApartmentFloors;
            }
            set
            {
                CoefficientApartmentFloorsPart.As<CoefficientApartmentFloorsPart>().CoefficientApartmentFloors = value;
            }
        }

        public IContent CoefficientApartmentFloorsPart { get; set; }
    }

    public class CoefficientApartmentFloorsIndexViewModel
    {
        public IList<CoefficientApartmentFloorsEntry> CoefficientApartmentFloors { get; set; }
        public CoefficientApartmentFloorsIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CoefficientApartmentFloorsEntry
    {
        public CoefficientApartmentFloorsPart CoefficientApartmentFloorsPart { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CoefficientApartmentFloorsIndexOptions
    {
        public string Search { get; set; }
        public CoefficientApartmentFloorsOrder Order { get; set; }
        public CoefficientApartmentFloorsFilter Filter { get; set; }
        public CoefficientApartmentFloorsBulkAction BulkAction { get; set; }
    }

    public enum CoefficientApartmentFloorsOrder
    {
        SeqOrder,
        Name
    }

    public enum CoefficientApartmentFloorsFilter
    {
        All
    }

    public enum CoefficientApartmentFloorsBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}