using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class LocationApartmentBlockPartRecord : ContentPartRecord
    {
        public virtual LocationApartmentPartRecord LocationApartment { get; set; }
        public virtual string BlockName { get; set; }
        public virtual string ShortName { get; set; }
        public virtual int ApartmentEachFloor { get; set; }
        //public virtual int SeqOrder { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual int FloorTotal { get; set; }
        public virtual int GroupFloorInBlockTotal { get; set; }
        public virtual double PriceAverage { get; set; }
    }
    public class LocationApartmentBlockPart : ContentPart<LocationApartmentBlockPartRecord>
    {
        public LocationApartmentPartRecord LocationApartment 
        {
            get { return Record.LocationApartment; }
            set { Record.LocationApartment = value; }
        }

        public string BlockName
        {
            get { return Retrieve(r => r.BlockName); }
            set { Store(r => r.BlockName, value); }
        }
        public string ShortName
        {
            get { return Retrieve(r => r.ShortName); }
            set { Store(r => r.ShortName, value); }
        }

        public int ApartmentEachFloor
        {
            get { return Retrieve(r => r.ApartmentEachFloor); }
            set { Store(r => r.ApartmentEachFloor, value); }
        }

        //public int SeqOrder
        //{
        //    get { return Retrieve(r => r.SeqOrder); }
        //    set { Store(r => r.SeqOrder, value); }
        //}

        public bool IsActive
        {
            get { return Retrieve(r => r.IsActive); }
            set { Store(r => r.IsActive, value); }
        }

        public int FloorTotal
        {
            get { return Retrieve(r => r.FloorTotal); }
            set { Store(r => r.FloorTotal, value); }
        }

        public int GroupFloorInBlockTotal
        {
            get { return Retrieve(r => r.GroupFloorInBlockTotal); }
            set { Store(r => r.GroupFloorInBlockTotal, value); }
        }

        public double PriceAverage
        {
            get { return Retrieve(r => r.PriceAverage); }
            set { Store(r => r.PriceAverage, value); }
        }
    }
}
