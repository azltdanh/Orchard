using System;
using Orchard.Users.Models;

namespace RealEstate.Models
{
    public class CustomerPropertyUserRecord
    {
        public virtual int Id { get; set; }
        public virtual CustomerPropertyRecord CustomerPropertyRecord { get; set; }
        public virtual UserPartRecord UserPartRecord { get; set; }
        public virtual DateTime VisitedDate { get; set; }
        public virtual bool IsWorkOverTime { get; set; }
    }
}