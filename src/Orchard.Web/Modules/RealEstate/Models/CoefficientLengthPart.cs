using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class CoefficientLengthPartRecord : ContentPartRecord
    {
        public virtual double WidthRange { get; set; }
        public virtual double MinLength { get; set; }
        public virtual double MaxLength { get; set; }
    }

    public class CoefficientLengthPart : ContentPart<CoefficientLengthPartRecord>
    {
        public double WidthRange
        {
            get { return Retrieve(r => r.WidthRange); }
            set { Store(r => r.WidthRange, value); }
        }

        public double MinLength
        {
            get { return Retrieve(r => r.MinLength); }
            set { Store(r => r.MinLength, value); }
        }

        public double MaxLength
        {
            get { return Retrieve(r => r.MaxLength); }
            set { Store(r => r.MaxLength, value); }
        }
    }
}