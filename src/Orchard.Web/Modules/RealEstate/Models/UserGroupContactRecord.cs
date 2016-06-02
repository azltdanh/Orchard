namespace RealEstate.Models
{
    public class UserGroupContactRecord
    {
        public virtual int Id { get; set; }
        public virtual UserGroupPartRecord UserGroupPartRecord { get; set; }
        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual AdsTypePartRecord AdsTypePartRecord { get; set; }
        public virtual PropertyTypeGroupPartRecord PropertyTypeGroupPartRecord { get; set; }
        public virtual string ContactPhone { get; set; }
    }
}