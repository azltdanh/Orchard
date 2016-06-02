using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.FrontEnd.Models
{
    public class FrontEndSettingPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
        public virtual string Description { get; set; }
        public virtual int SeqOrder { get; set; }
    }

    public class FrontEndSettingPart : ContentPart<FrontEndSettingPartRecord>
    {
        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string Value
        {
            get { return Retrieve(r => r.Value); }
            set { Store(r => r.Value, value); }
        }

        public string Description
        {
            get { return Retrieve(r => r.Description); }
            set { Store(r => r.Description, value); }
        }

        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }
    }
}