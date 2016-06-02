using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class ContactPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Address { get; set; }
        public virtual string Email { get; set; }
    }

    public class ContactPart : ContentPart<ContactPartRecord>
    {
        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string Phone
        {
            get { return Retrieve(r => r.Phone); }
            set { Store(r => r.Phone, value); }
        }

        public string Address
        {
            get { return Retrieve(r => r.Address); }
            set { Store(r => r.Address, value); }
        }

        public string Email
        {
            get { return Retrieve(r => r.Email); }
            set { Store(r => r.Email, value); }
        }
    }
}