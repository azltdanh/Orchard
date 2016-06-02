namespace RealEstate.Models
{
    public class UserGroupSharedLocationRecord
    {
        public virtual int Id { get; set; }
        public virtual UserGroupPartRecord SeederUserGroupPartRecord { get; set; }
        public virtual UserGroupPartRecord LeecherUserGroupPartRecord { get; set; }
        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual LocationWardPartRecord LocationWardPartRecord { get; set; }
    }
}