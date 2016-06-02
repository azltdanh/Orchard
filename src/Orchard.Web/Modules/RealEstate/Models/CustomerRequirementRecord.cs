namespace RealEstate.Models
{
    public class CustomerRequirementRecord
    {
        public virtual int Id { get; set; }
        public virtual int? GroupId { get; set; }
        public virtual CustomerPartRecord CustomerPartRecord { get; set; }
        public virtual bool IsEnabled { get; set; }

        public virtual AdsTypePartRecord AdsTypePartRecord { get; set; }
        public virtual PropertyTypeGroupPartRecord PropertyTypeGroupPartRecord { get; set; }

        public virtual LocationProvincePartRecord LocationProvincePartRecord { get; set; }
        public virtual LocationDistrictPartRecord LocationDistrictPartRecord { get; set; }
        public virtual LocationWardPartRecord LocationWardPartRecord { get; set; }
        public virtual LocationStreetPartRecord LocationStreetPartRecord { get; set; }
        public virtual LocationApartmentPartRecord LocationApartmentPartRecord { get; set; }

        public virtual double? MinArea { get; set; }
        public virtual double? MaxArea { get; set; }
        public virtual double? MinWidth { get; set; }
        public virtual double? MaxWidth { get; set; }
        public virtual double? MinLength { get; set; }
        public virtual double? MaxLength { get; set; }

        public virtual DirectionPartRecord DirectionPartRecord { get; set; }
        public virtual PropertyLocationPartRecord PropertyLocationPartRecord { get; set; }

        public virtual double? MinAlleyWidth { get; set; }
        public virtual double? MaxAlleyWidth { get; set; }
        public virtual int? MinAlleyTurns { get; set; }
        public virtual int? MaxAlleyTurns { get; set; }
        public virtual double? MinDistanceToStreet { get; set; }
        public virtual double? MaxDistanceToStreet { get; set; }
        public virtual double? MinFloors { get; set; }
        public virtual double? MaxFloors { get; set; }
        public virtual int? MinBedrooms { get; set; }
        public virtual int? MaxBedrooms { get; set; }
        public virtual int? MinBathrooms { get; set; }
        public virtual int? MaxBathrooms { get; set; }

        public virtual double? MinPrice { get; set; }
        public virtual double? MaxPrice { get; set; }
        public virtual PaymentMethodPartRecord PaymentMethodPartRecord { get; set; }

        // Apartment
        public virtual string OtherProjectName { get; set; }
        public virtual int? MinApartmentFloorTh { get; set; }
        public virtual int? MaxApartmentFloorTh { get; set; }
    }
}