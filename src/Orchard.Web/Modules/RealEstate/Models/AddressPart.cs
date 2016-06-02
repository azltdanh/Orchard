using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class AddressPartRecord : ContentPartRecord
    {
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationWardPartRecord Ward { get; set; }
        public virtual LocationStreetPartRecord Street { get; set; }

        public virtual string AddressNumber { get; set; }
        public virtual string AddressCorner { get; set; }
        public virtual int AlleyNumber { get; set; }

        public virtual string OtherProvinceName { get; set; }
        public virtual string OtherDistrictName { get; set; }
        public virtual string OtherWardName { get; set; }
        public virtual string OtherStreetName { get; set; }
    }

    public class AddressPart : ContentPart<AddressPartRecord>
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

        public string AddressNumber
        {
            get { return Retrieve(r => r.AddressNumber); }
            set { Store(r => r.AddressNumber, value); }
        }

        public string AddressCorner
        {
            get { return Retrieve(r => r.AddressCorner); }
            set { Store(r => r.AddressCorner, value); }
        }

        public int AlleyNumber
        {
            get { return Retrieve(r => r.AlleyNumber); }
            set { Store(r => r.AlleyNumber, value); }
        }

        public string OtherProvinceName
        {
            get { return Retrieve(r => r.OtherProvinceName); }
            set { Store(r => r.OtherProvinceName, value); }
        }

        public string OtherDistrictName
        {
            get { return Retrieve(r => r.OtherDistrictName); }
            set { Store(r => r.OtherDistrictName, value); }
        }

        public string OtherWardName
        {
            get { return Retrieve(r => r.OtherWardName); }
            set { Store(r => r.OtherWardName, value); }
        }

        public string OtherStreetName
        {
            get { return Retrieve(r => r.OtherStreetName); }
            set { Store(r => r.OtherStreetName, value); }
        }
    }
}