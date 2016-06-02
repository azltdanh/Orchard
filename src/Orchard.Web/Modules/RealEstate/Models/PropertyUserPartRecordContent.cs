using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class PropertyUserPartRecordContent
    {
        public virtual int Id { get; set; }
        public virtual PropertyPartRecord PropertyPartRecord { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
    }
}
