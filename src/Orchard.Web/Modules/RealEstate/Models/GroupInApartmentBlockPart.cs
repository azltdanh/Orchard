
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
namespace RealEstate.Models
{
    public class GroupInApartmentBlockPartRecord : ContentPartRecord
    {
        public virtual LocationApartmentBlockPartRecord ApartmentBlock { get; set; }
        public virtual int FloorFrom { get; set; }
        public virtual int FloorTo { get; set; }
        public virtual int ApartmentPerFloor { get; set; }
        public virtual int ApartmentGroupPosition { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsActive { get; set; }
    }
    public class GroupInApartmentBlockPart : ContentPart<GroupInApartmentBlockPartRecord>
    {
        public LocationApartmentBlockPartRecord ApartmentBlock
        {
            get { return Record.ApartmentBlock; }
            set { Record.ApartmentBlock = value; }
        }

        public int FloorFrom
        {
            get { return Retrieve(r => r.FloorFrom); }
            set { Store(r => r.FloorFrom, value); }
        }

        public int FloorTo
        {
            get { return Retrieve(r => r.FloorTo); }
            set { Store(r => r.FloorTo, value); }
        }

        public int ApartmentPerFloor
        {
            get { return Retrieve(r => r.ApartmentPerFloor); }
            set { Store(r => r.ApartmentPerFloor, value); }
        }

        public int ApartmentGroupPosition
        {
            get { return Retrieve(r => r.ApartmentGroupPosition); }
            set { Store(r => r.ApartmentGroupPosition, value); }
        }
        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }
        public bool IsActive
        {
            get { return Retrieve(r => r.IsActive); }
            set { Store(r => r.IsActive, value); }
        }
    }
}
