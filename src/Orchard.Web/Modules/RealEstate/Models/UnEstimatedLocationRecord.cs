using System;

namespace RealEstate.Models
{
    public class UnEstimatedLocationRecord
    {
        public virtual int Id { get; set; }
        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual LocationWardPartRecord LocationWardPartRecord { get; set; }
        public virtual LocationStreetPartRecord LocationStreetPartRecord { get; set; }
        public virtual string AddressNumber { get; set; }
        public virtual string AddressCorner { get; set; }
        public virtual DateTime CreatedDate { get; set; }
    }
}