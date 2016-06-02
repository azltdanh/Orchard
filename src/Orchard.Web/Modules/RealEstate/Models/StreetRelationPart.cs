using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class StreetRelationPartRecord : ContentPartRecord
    {
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationWardPartRecord Ward { get; set; }
        public virtual LocationStreetPartRecord Street { get; set; }
        public virtual double? StreetWidth { get; set; }

        public virtual double RelatedValue { get; set; }
        public virtual double? RelatedAlleyValue { get; set; }

        public virtual LocationProvincePartRecord RelatedProvince { get; set; }
        public virtual LocationDistrictPartRecord RelatedDistrict { get; set; }
        public virtual LocationWardPartRecord RelatedWard { get; set; }
        public virtual LocationStreetPartRecord RelatedStreet { get; set; }
        public virtual double? RelatedStreetWidth { get; set; }

        public virtual double? CoefficientAlley1Max { get; set; }
        public virtual double? CoefficientAlley1Min { get; set; }
        public virtual double? CoefficientAlleyEqual { get; set; }
        public virtual double? CoefficientAlleyMax { get; set; }
        public virtual double? CoefficientAlleyMin { get; set; }
    }

    public class StreetRelationPart : ContentPart<StreetRelationPartRecord>
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

        public LocationStreetPartRecord Street
        {
            get { return Record.Street; }
            set { Record.Street = value; }
        }

        public double? StreetWidth
        {
            get { return Retrieve(r => r.StreetWidth); }
            set { Store(r => r.StreetWidth, value); }
        }

        public double RelatedValue
        {
            get { return Retrieve(r => r.RelatedValue); }
            set { Store(r => r.RelatedValue, value); }
        }

        public double? RelatedAlleyValue
        {
            get { return Retrieve(r => r.RelatedAlleyValue); }
            set { Store(r => r.RelatedAlleyValue, value); }
        }

        public LocationProvincePartRecord RelatedProvince
        {
            get { return Record.RelatedProvince; }
            set { Record.RelatedProvince = value; }
        }

        public LocationDistrictPartRecord RelatedDistrict
        {
            get { return Record.RelatedDistrict; }
            set { Record.RelatedDistrict = value; }
        }

        public LocationWardPartRecord RelatedWard
        {
            get { return Record.RelatedWard; }
            set { Record.RelatedWard = value; }
        }

        public LocationStreetPartRecord RelatedStreet
        {
            get { return Record.RelatedStreet; }
            set { Record.RelatedStreet = value; }
        }

        public double? RelatedStreetWidth
        {
            get { return Retrieve(r => r.RelatedStreetWidth); }
            set { Store(r => r.RelatedStreetWidth, value); }
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

        public string DisplayForStreetName
        {
            get
            {
                string address = "";
                address += Street != null
                    ? (Street.Name + string.Format("{0: (0}", Street.FromNumber) +
                       string.Format("{0: - 0)}", Street.ToNumber) + ", ")
                    : "";
                address += Ward != null ? (Ward.Name + ", ") : "";
                address += District != null ? District.Name : "";
                return address;
            }
        }

        public string DisplayForRelatedStreetName
        {
            get
            {
                string address = "";
                address += RelatedStreet != null
                    ? (RelatedStreet.Name + string.Format("{0: (0}", RelatedStreet.FromNumber) +
                       string.Format("{0: - 0)}", RelatedStreet.ToNumber) + ", ")
                    : "";
                address += RelatedWard != null ? (RelatedWard.Name + ", ") : "";
                address += RelatedDistrict != null ? RelatedDistrict.Name : "";
                return address;
            }
        }
    }
}