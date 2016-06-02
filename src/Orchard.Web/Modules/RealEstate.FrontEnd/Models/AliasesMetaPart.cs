using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace RealEstate.FrontEnd.Models
{
    public class AliasesMetaPartRecord : ContentPartRecord
    {
        public virtual string Title { get; set; }
        public virtual string Keywords { get; set; }
        public virtual string Description { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual int Alias_Id { get; set; }
        public virtual int DomainGroupId { get; set; }
    }

    public class AliasesMetaPart : ContentPart<AliasesMetaPartRecord>
    {
        public string Title
        {
            get { return Retrieve(r => r.Title); }
            set { Store(r => r.Title, value); }
        }

        public string Keywords
        {
            get { return Retrieve(r => r.Keywords); }
            set { Store(r => r.Keywords, value); }
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

        public int Alias_Id
        {
            get { return Retrieve(r => r.Alias_Id); }
            set { Store(r => r.Alias_Id, value); }
        }

        public int DomainGroupId
        {
            get { return Retrieve(r => r.DomainGroupId); }
            set { Store(r => r.DomainGroupId, value); }
        }
    }
}