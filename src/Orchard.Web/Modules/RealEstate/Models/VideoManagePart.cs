using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class VideoManagePartRecord : ContentPartRecord
    {
        public virtual VideoTypePartRecord VideoType { get; set; }
        public virtual string Title { get; set; }
        public virtual string Keyword { get; set; }
        public virtual string Description { get; set; }
        public virtual string YoutubeId { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool Enable { get; set; }
        public virtual bool Publish { get; set; }
        public virtual int DomainGroupId { get; set; }
        public virtual string Image { get; set; }

    }

    public class VideoManagePart : ContentPart<VideoManagePartRecord>
    {
        public VideoTypePartRecord VideoType
        {
            get { return Record.VideoType; }
            set { Record.VideoType = value; }
        }
        public string Title
        {
            get { return Retrieve(r => r.Title); }
            set { Store(r => r.Title, value); }
        }
        public string Keyword
        {
            get { return Retrieve(r => r.Keyword); }
            set { Store(r => r.Keyword, value); }
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
        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }
        public bool Enable
        {
            get { return Retrieve(r => r.Enable); }
            set { Store(r => r.Enable, value); }
        }
        public bool Publish
        {
            get { return Retrieve(r => r.Publish); }
            set { Store(r => r.Publish, value); }
        }

        public int DomainGroupId
        {
            get { return Retrieve(r => r.DomainGroupId); }
            set { Store(r => r.DomainGroupId, value); }
        }
        public string Image
        {
            get { return Retrieve(r => r.Image); }
            set { Store(r => r.Image, value); }
        }
    }
}
