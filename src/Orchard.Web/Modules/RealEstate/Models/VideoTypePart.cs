using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.Models
{
    public class VideoTypePartRecord : ContentPartRecord
    {
        public virtual string Name { get; set; }
    }

    public class VideoTypePart : ContentPart<VideoTypePartRecord>
    {
        public string Name 
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }
    }
}
