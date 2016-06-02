using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class CoefficientApartmentFloorThPartRecord : ContentPartRecord
    {
        public virtual double? MaxFloors { get; set; }
        public virtual double ApartmentFloorTh { get; set; }
        public virtual double CoefficientApartmentFloorTh { get; set; }
    }

    public class CoefficientApartmentFloorThPart : ContentPart<CoefficientApartmentFloorThPartRecord>
    {
        public double? MaxFloors
        {
            get { return Retrieve(r => r.MaxFloors); }
            set { Store(r => r.MaxFloors, value); }
        }

        public double ApartmentFloorTh
        {
            get { return Retrieve(r => r.ApartmentFloorTh); }
            set { Store(r => r.ApartmentFloorTh, value); }
        }

        public double CoefficientApartmentFloorTh
        {
            get { return Retrieve(r => r.CoefficientApartmentFloorTh); }
            set { Store(r => r.CoefficientApartmentFloorTh, value); }
        }
    }
}