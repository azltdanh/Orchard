using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class LocationStreetPartRecord : ContentPartRecord
    {
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationWardPartRecord Ward { get; set; }

        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual int SeqOrder { get; set; }

        public virtual bool IsEnabled { get; set; }
        public virtual bool IsOneWayStreet { get; set; }
        public virtual double? StreetWidth { get; set; }

        public virtual double? CoefficientAlley1Max { get; set; }
        public virtual double? CoefficientAlley1Min { get; set; }
        public virtual double? CoefficientAlleyEqual { get; set; }
        public virtual double? CoefficientAlleyMax { get; set; }
        public virtual double? CoefficientAlleyMin { get; set; }

        public virtual LocationStreetPartRecord RelatedStreet { get; set; }
        public virtual int? FromNumber { get; set; }
        public virtual int? ToNumber { get; set; }
    }

    public class LocationStreetPart : ContentPart<LocationStreetPartRecord>
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

        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShortName
        {
            get { return Retrieve(r => r.ShortName); }
            set { Store(r => r.ShortName, value); }
        }

        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }

        public bool IsEnabled
        {
            get { return Retrieve(r => r.IsEnabled); }
            set { Store(r => r.IsEnabled, value); }
        }

        public bool IsOneWayStreet
        {
            get { return Retrieve(r => r.IsOneWayStreet); }
            set { Store(r => r.IsOneWayStreet, value); }
        }

        public double? StreetWidth
        {
            get { return Retrieve(r => r.StreetWidth); }
            set { Store(r => r.StreetWidth, value); }
        }

        public double? CoefficientAlley1Max
        {
            get { return Retrieve(r => r.CoefficientAlley1Max); }
            set { Store(r => r.CoefficientAlley1Max, value); }
        }

        public double? CoefficientAlley1Min
        {
            get { return Retrieve(r => r.CoefficientAlley1Min); }
            set { Store(r => r.CoefficientAlley1Min, value); }
        }

        public double? CoefficientAlleyEqual
        {
            get { return Retrieve(r => r.CoefficientAlleyEqual); }
            set { Store(r => r.CoefficientAlleyEqual, value); }
        }

        public double? CoefficientAlleyMax
        {
            get { return Retrieve(r => r.CoefficientAlleyMax); }
            set { Store(r => r.CoefficientAlleyMax, value); }
        }

        public double? CoefficientAlleyMin
        {
            get { return Retrieve(r => r.CoefficientAlleyMin); }
            set { Store(r => r.CoefficientAlleyMin, value); }
        }

        public LocationStreetPartRecord RelatedStreet
        {
            get { return Record.RelatedStreet; }
            set { Record.RelatedStreet = value; }
        }

        public int? FromNumber
        {
            get { return Retrieve(r => r.FromNumber); }
            set { Store(r => r.FromNumber, value); }
        }

        public int? ToNumber
        {
            get { return Retrieve(r => r.ToNumber); }
            set { Store(r => r.ToNumber, value); }
        }

        public string DisplayForStreetName
        {
            get
            {
                if (RelatedStreet != null)
                    return RelatedStreet.Name + " (" + FromNumber + " - " + ToNumber + ")";
                return Name;
            }
        }
    }
}