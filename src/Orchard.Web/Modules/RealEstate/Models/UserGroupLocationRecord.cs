namespace RealEstate.Models
{
    public class UserGroupLocationRecord
    {
        public virtual int Id { get; set; }
        public virtual UserGroupPartRecord UserGroupPartRecord { get; set; }
        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual LocationWardPartRecord LocationWardPartRecord { get; set; }
    }
}