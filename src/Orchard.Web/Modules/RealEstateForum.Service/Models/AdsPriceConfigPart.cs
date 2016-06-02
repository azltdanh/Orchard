using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstateForum.Service.Models
{
    public class AdsPriceConfigPartRecord : ContentPartRecord
    {
        public virtual string Price { get; set; }
        public virtual string CssClass { get; set; }
    }
    public class AdsPriceConfigPart : ContentPart<AdsPriceConfigPartRecord>
    {
        public string Price
        {
            get { return this.Retrieve(r => r.Price); }
            set { this.Store(r=>r.Price,value);}
        }
        public string CssClass
        {
            get { return this.Retrieve(r => r.CssClass); }
            set { this.Store(r => r.CssClass, value); }
        }
    }
}