using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
namespace RealEstate.Models
{
    public class NewsVideoPartRecord : ContentPartRecord
    {
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual string YoutubeId { get; set; }
        public virtual bool Enable { get; set; }
        public virtual int SeqOrder { get; set; }
    }

    public class NewsVideoPart : ContentPart<NewsVideoPartRecord>
    {
        public string Title
        {
            get { return Retrieve(r => r.Title); }
            set { Store(r => r.Title, value); }
        }
        public string Description
        {
            get { return Retrieve(r => r.Description); }
            set { Store(r => r.Description, value); }
        }
        public string YoutubeId
        {
            get { return Retrieve(r => r.YoutubeId); }
            set { Store(r => r.YoutubeId, value); }
        }
        public bool Enable
        {
            get { return Retrieve(r => r.Enable); }
            set { Store(r => r.Enable, value); }
        }
        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }
    }
}
