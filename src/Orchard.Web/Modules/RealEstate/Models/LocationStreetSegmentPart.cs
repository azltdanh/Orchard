using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models {

    public class LocationStreetSegmentPartRecord : ContentPartRecord
    {
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationWardPartRecord Ward { get; set; }
        public virtual LocationStreetPartRecord Street { get; set; }
        public virtual int FromNumber { get; set; }
        public virtual int ToNumber { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual float StreetWidth { get; set; }
        public virtual float CoefficientAlley1Max { get; set; }
        public virtual float CoefficientAlley1Min { get; set; }
        public virtual float CoefficientAlleyMax { get; set; }
        public virtual float CoefficientAlleyMin { get; set; }
        public virtual float CoefficientAlleyEqual { get; set; }

    }

    public class LocationStreetSegmentPart : ContentPart<LocationStreetSegmentPartRecord> 
    {
        public LocationProvincePartRecord Province
        {
            get { return Record.Province; }
            set { Record.Province = value; }
        }
        public LocationDistrictPartRecord District
        {
            get { return Record.District; }
            set { Record.District = value; }
        }
        public LocationWardPartRecord Ward
        {
            get { return Record.Ward; }
            set { Record.Ward = value; }
        }
        public LocationStreetPartRecord Street {
            get { return Record.Street; }
            set { Record.Street = value; }
        }
        public int FromNumber
        {
            get { return Record.FromNumber; }
            set { Record.FromNumber = value; }
        }
        public int ToNumber
        {
            get { return Record.ToNumber; }
            set { Record.ToNumber = value; }
        }
        public bool IsEnabled {
            get { return Record.IsEnabled; }
            set { Record.IsEnabled = value; }
        }
        public float StreetWidth {
            get { return Record.StreetWidth; }
            set { Record.StreetWidth = value; }
        }
        public float CoefficientAlley1Max
        {
            get { return Record.CoefficientAlley1Max; }
            set { Record.CoefficientAlley1Max = value; }
        }
        public float CoefficientAlley1Min
        {
            get { return Record.CoefficientAlley1Min; }
            set { Record.CoefficientAlley1Min = value; }
        }
        public float CoefficientAlleyMax
        {
            get { return Record.CoefficientAlleyMax; }
            set { Record.CoefficientAlleyMax = value; }
        }
        public float CoefficientAlleyMin
        {
            get { return Record.CoefficientAlleyMin; }
            set { Record.CoefficientAlleyMin = value; }
        }
        public float CoefficientAlleyEqual
        {
            get { return Record.CoefficientAlleyEqual; }
            set { Record.CoefficientAlleyEqual = value; }
        }
    }
}
