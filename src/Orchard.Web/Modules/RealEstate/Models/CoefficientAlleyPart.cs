using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class CoefficientAlleyPartRecord : ContentPartRecord
    {
        public virtual double StreetUnitPrice { get; set; }
        public virtual double CoefficientAlley1Max { get; set; }
        public virtual double CoefficientAlley1Min { get; set; }
        public virtual double CoefficientAlleyMax { get; set; }
        public virtual double CoefficientAlleyMin { get; set; }
        public virtual double CoefficientAlleyEqual { get; set; }
    }

    public class CoefficientAlleyPart : ContentPart<CoefficientAlleyPartRecord>
    {
        public double StreetUnitPrice
        {
            get { return Retrieve(r => r.StreetUnitPrice); }
            set { Store(r => r.StreetUnitPrice, value); }
        }

        public double CoefficientAlley1Max
        {
            get { return Retrieve(r => r.CoefficientAlley1Max); }
            set { Store(r => r.CoefficientAlley1Max, value); }
        }

        public double CoefficientAlley1Min
        {
            get { return Retrieve(r => r.CoefficientAlley1Min); }
            set { Store(r => r.CoefficientAlley1Min, value); }
        }

        public double CoefficientAlleyMax
        {
            get { return Retrieve(r => r.CoefficientAlleyMax); }
            set { Store(r => r.CoefficientAlleyMax, value); }
        }

        public double CoefficientAlleyMin
        {
            get { return Retrieve(r => r.CoefficientAlleyMin); }
            set { Store(r => r.CoefficientAlleyMin, value); }
        }

        public double CoefficientAlleyEqual
        {
            get { return Retrieve(r => r.CoefficientAlleyEqual); }
            set { Store(r => r.CoefficientAlleyEqual, value); }
        }
    }
}