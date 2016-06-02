namespace RealEstate.Models
{
    public class CustomerPurposePartRecordContent
    {
        public virtual int Id { get; set; }
        public virtual CustomerPartRecord CustomerPartRecord { get; set; }
        public virtual CustomerPurposePartRecord CustomerPurposePartRecord { get; set; }
    }
}