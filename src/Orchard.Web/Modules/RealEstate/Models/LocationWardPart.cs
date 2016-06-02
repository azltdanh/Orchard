using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class LocationWardPartRecord : ContentPartRecord
    {
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }

        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }
    }

    public class LocationWardPart : ContentPart<LocationWardPartRecord>
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
    }
}