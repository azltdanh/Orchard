using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class CoefficientAlleyDistancePartRecord : ContentPartRecord
    {
        public virtual double LastAlleyWidth { get; set; }
        public virtual double MaxAlleyDistance { get; set; }
        public virtual double CoefficientDistance { get; set; }
    }

    public class CoefficientAlleyDistancePart : ContentPart<CoefficientAlleyDistancePartRecord>
    {
        public double LastAlleyWidth
        {
            get { return Retrieve(r => r.LastAlleyWidth); }
            set { Store(r => r.LastAlleyWidth, value); }
        }

        public double MaxAlleyDistance
        {
            get { return Retrieve(r => r.MaxAlleyDistance); }
            set { Store(r => r.MaxAlleyDistance, value); }
        }

        public double CoefficientDistance
        {
            get { return Retrieve(r => r.CoefficientDistance); }
            set { Store(r => r.CoefficientDistance, value); }
        }
    }
}