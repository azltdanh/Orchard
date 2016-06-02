using System;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class UserLocationRecord
    {
        public virtual int Id { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual LocationWardPartRecord LocationWardPartRecord { get; set; }
        public virtual bool RetrictedAccessGroupProperties { get; set; }
        public virtual bool EnableAccessProperties { get; set; }
        public virtual bool EnableEditLocations { get; set; }
        public virtual bool EnableIsAgencies { get; set; }
        public virtual string AreaAgencies { get; set; }
        public virtual DateTime? EndDateAgencing { get; set; }
        public virtual UserGroupPartRecord UserGroupRecord { get; set; }
        public virtual bool IsSelling { get; set; }
        public virtual bool IsLeasing { get; set; }
    }
}