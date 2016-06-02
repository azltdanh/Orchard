namespace RealEstate.Models
{
    public class PropertyAdvantagePartRecordContent
    {
        public virtual int Id { get; set; }
        public virtual PropertyPartRecord PropertyPartRecord { get; set; }
        public virtual PropertyAdvantagePartRecord PropertyAdvantagePartRecord { get; set; }
    }
}