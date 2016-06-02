using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;


namespace RealEstateForum.Service.Models
{
    public class SupportOnlineConfigPartRecord : ContentPartRecord
    {
        public virtual string NumberPhone { get; set; }
        public virtual string YahooNick { get; set; }
        public virtual string SkypeNick { get; set; }
    }
    public class SupportOnlineConfigPart : ContentPart<SupportOnlineConfigPartRecord>
    {
        public string NumberPhone
        {
            get { return Retrieve(r=>r.NumberPhone); }
            set { Retrieve(r => r.NumberPhone, value); }
        }
        public string YahooNick
        {
            get { return Retrieve(r=>r.YahooNick); }
            set { Store(r=>r.YahooNick,value); }
        }
        public string SkypeNick
        {
            get { return Retrieve(r=>r.SkypeNick); }
            set { Store(r=>r.SkypeNick,value); }
        }
    }
}