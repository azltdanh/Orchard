using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class AdsPaymentConfigPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string CssClass { get; set; }
        public virtual string Description { get; set; }
        public virtual long Value { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual int VipValue { get; set; }
    }

    public class AdsPaymentConfigPart : ContentPart<AdsPaymentConfigPartRecord>
    {
        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string CssClass
        {
            get { return Retrieve(r => r.CssClass); }
            set { Store(r => r.CssClass, value); }
        }

        public string Description
        {
            get { return Retrieve(r => r.Description); }
            set { Store(r => r.Description, value); }
        }

        public long Value
        {
            get { return Retrieve(r => r.Value); }
            set { Store(r => r.Value, value); }
        }

        public bool IsEnabled
        {
            get { return Retrieve(r => r.IsEnabled); }
            set { Store(r => r.IsEnabled, value); }
        }

        public int VipValue
        {
            get { return Retrieve(r => r.VipValue); }
            set { Store(r => r.VipValue, value); }
        }
    }
}