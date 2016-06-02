using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class PropertyGroupPartRecord : ContentPartRecord
    {
        public virtual int PropertyId { get; set; }
        public virtual int UserGroupId { get; set; }
        public virtual bool? IsApproved { get; set; }
    }

    public class PropertyGroupPart : ContentPart<PropertyGroupPartRecord>
    {
        public int PropertyId
        {
            get { return Retrieve(r => r.PropertyId); }
            set { Store(r => r.PropertyId, value); }
        }

        public int UserGroupId
        {
            get { return Retrieve(r => r.UserGroupId); }
            set { Store(r => r.UserGroupId, value); }
        }

        public bool? IsApproved
        {
            get { return Retrieve(r => r.IsApproved); }
            set { Store(r => r.IsApproved, value); }
        }
    }
}