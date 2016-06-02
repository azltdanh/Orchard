

using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class ExchangeTypePartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
        public virtual string CssClass { get; set; }
    }

    public class ExchangeTypePart : ContentPart<ExchangeTypePartRecord>
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
    }
}
