using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class UserInGroupRecord
    {
        public virtual int Id { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual UserGroupPartRecord UserGroupPartRecord { get; set; }
        public virtual AdsTypePartRecord DefaultAdsType { get; set; }
        public virtual PropertyTypeGroupPartRecord DefaultTypeGroup { get; set; }
    }
}