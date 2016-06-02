using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class LocationApartmentRelationPartRecord : ContentPartRecord
    {
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationApartmentPartRecord LocationApartment { get; set; }

        public virtual double RelatedValue { get; set; }

        public virtual LocationProvincePartRecord RelatedProvince { get; set; }
        public virtual LocationDistrictPartRecord RelatedDistrict { get; set; }
        public virtual LocationApartmentPartRecord RelatedLocationApartment { get; set; }
    }

    public class LocationApartmentRelationPart : ContentPart<LocationApartmentRelationPartRecord>
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

        public LocationApartmentPartRecord LocationApartment
        {
            get { return Record.LocationApartment; }
            set { Record.LocationApartment = value; }
        }

        public double RelatedValue
        {
            get { return Retrieve(r => r.RelatedValue); }
            set { Store(r => r.RelatedValue, value); }
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

        public LocationApartmentPartRecord RelatedLocationApartment
        {
            get { return Record.RelatedLocationApartment; }
            set { Record.RelatedLocationApartment = value; }
        }
    }
}