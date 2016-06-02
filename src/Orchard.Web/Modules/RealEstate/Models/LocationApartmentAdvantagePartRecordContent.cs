namespace RealEstate.Models
{
    public class LocationApartmentAdvantagePartRecordContent
    {
        public virtual int Id { get; set; }
        public virtual LocationApartmentPartRecord LocationApartmentPartRecord { get; set; }
        public virtual PropertyAdvantagePartRecord PropertyAdvantagePartRecord { get; set; }
    }
}