using System.Collections.Generic;

namespace RealEstate.Models
{
    public class CustomerPropertyRecord
    {
        public virtual int Id { get; set; }
        public virtual PropertyPartRecord PropertyPartRecord { get; set; }
        public virtual CustomerPartRecord CustomerPartRecord { get; set; }
        public virtual CustomerFeedbackPartRecord CustomerFeedbackPartRecord { get; set; }
        public virtual IList<CustomerPropertyUserRecord> Users { get; set; }
    }
}