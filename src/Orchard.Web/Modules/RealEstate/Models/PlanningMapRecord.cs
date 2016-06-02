namespace RealEstate.Models
{
    public class PlanningMapRecord
    {
        public virtual int Id { get; set; }
        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual LocationWardPartRecord LocationWardPartRecord { get; set; }
        public virtual string ImagesPath { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual int MinZoom { get; set; }
        public virtual int MaxZoom { get; set; }
        public virtual double Ratio { get; set; }
        public virtual bool IsEnabled { get; set; }
    }
}