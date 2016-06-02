using System;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class UserActivityPartRecord
    {
        public virtual int Id { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual UserActionPartRecord UserActionPartRecord { get; set; }
        public virtual PropertyPartRecord PropertyPartRecord { get; set; }
        public virtual CustomerPartRecord CustomerPartRecord { get; set; }
    }
}