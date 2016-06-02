using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class PaymentUnitPartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string CssClass { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }
    }

    public class PaymentUnitPart : ContentPart<PaymentUnitPartRecord>
    {
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

        public string CssClass
        {
            get { return Retrieve(r => r.CssClass); }
            set { Store(r => r.CssClass, value); }
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