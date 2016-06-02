using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class CoefficientApartmentFloorsPartRecord : ContentPartRecord
    {
        public virtual double Floors { get; set; }
        public virtual double CoefficientApartmentFloors { get; set; }
    }

    public class CoefficientApartmentFloorsPart : ContentPart<CoefficientApartmentFloorsPartRecord>
    {
        public double Floors
        {
            get { return Retrieve(r => r.Floors); }
            set { Store(r => r.Floors, value); }
        }

        public double CoefficientApartmentFloors
        {
            get { return Retrieve(r => r.CoefficientApartmentFloors); }
            set { Store(r => r.CoefficientApartmentFloors, value); }
        }
    }
}