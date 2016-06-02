using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;

namespace RealEstate.Models
{
    public class LocationApartmentPartRecord : ContentPartRecord
    {
        public LocationApartmentPartRecord()
        {
            Files = new List<PropertyFilePartRecord>();
            Advantages = new List<LocationApartmentAdvantagePartRecordContent>();
        }

        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationWardPartRecord Ward { get; set; }
        public virtual LocationStreetPartRecord Street { get; set; }
        public virtual string AddressNumber { get; set; }

        public virtual string Name { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string Block { get; set; }

        public virtual int? Floors { get; set; }
        public virtual double? PriceAverage { get; set; }

        public virtual string StreetAddress { get; set; }
        public virtual string DistanceToCentral { get; set; }
        public virtual string OtherAdvantages { get; set; }

        public virtual string Description { get; set; }

        public virtual string Investors { get; set; }
        public virtual string CurrentBuildingStatus { get; set; }
        public virtual string ManagementFees { get; set; }

        public virtual double? AreaTotal { get; set; }
        public virtual double? AreaConstruction { get; set; }
        public virtual double? AreaGreen { get; set; }

        public virtual int? TradeFloors { get; set; }
        public virtual double? AreaTradeFloors { get; set; }

        public virtual int? Basements { get; set; }
        public virtual double? AreaBasements { get; set; }

        public virtual int? Elevators { get; set; }

        public virtual int SeqOrder { get; set; }
        public virtual bool IsEnabled { get; set; }

        public virtual bool IsHighlight { get; set; }
        public virtual DateTime? HighlightExpiredTime { get; set; }

        public virtual DateTime? CreatedDate { get; set; }
        public virtual DateTime? UpdatedDate { get; set; }

        // Files
        public virtual IList<PropertyFilePartRecord> Files { get; set; }

        #region Advantage & DisAdvantage

        // Advantages
        public virtual IList<LocationApartmentAdvantagePartRecordContent> Advantages { get; set; }

        #endregion
    }

    public class LocationApartmentPart : ContentPart<LocationApartmentPartRecord>
    {
        public LocationProvincePartRecord Province
        {
            get { return Record.Province; }
            set { Record.Province = value; }
        }

        public LocationDistrictPartRecord District
        {
            get { return Record.District; }
            set { Record.District = value; }
        }

        public LocationWardPartRecord Ward
        {
            get { return Record.Ward; }
            set { Record.Ward = value; }
        }

        public LocationStreetPartRecord Street
        {
            get { return Record.Street; }
            set { Record.Street = value; }
        }

        public string AddressNumber
        {
            get { return Retrieve(r => r.AddressNumber); }
            set { Store(r => r.AddressNumber, value); }
        }

        public string Name
        {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShortName
        {
            get { return Retrieve(r => r.ShortName); }
            set { Store(r => r.ShortName, value); }
        }

        public string Block
        {
            get { return Retrieve(r => r.Block); }
            set { Store(r => r.Block, value); }
        }

        public int? Floors
        {
            get { return Retrieve(r => r.Floors); }
            set { Store(r => r.Floors, value); }
        }

        public double? PriceAverage
        {
            get { return Retrieve(r => r.PriceAverage); }
            set { Store(r => r.PriceAverage, value); }
        }

        public string StreetAddress
        {
            get { return Retrieve(r => r.StreetAddress); }
            set { Store(r => r.StreetAddress, value); }
        }

        public string DistanceToCentral
        {
            get { return Retrieve(r => r.DistanceToCentral); }
            set { Store(r => r.DistanceToCentral, value); }
        }

        public string OtherAdvantages
        {
            get { return Retrieve(r => r.OtherAdvantages); }
            set { Store(r => r.OtherAdvantages, value); }
        }

        public string Description
        {
            get { return Retrieve(r => r.Description); }
            set { Store(r => r.Description, value); }
        }

        public string Investors
        {
            get { return Retrieve(r => r.Investors); }
            set { Store(r => r.Investors, value); }
        }

        public string CurrentBuildingStatus
        {
            get { return Retrieve(r => r.CurrentBuildingStatus); }
            set { Store(r => r.CurrentBuildingStatus, value); }
        }

        public string ManagementFees
        {
            get { return Retrieve(r => r.ManagementFees); }
            set { Store(r => r.ManagementFees, value); }
        }

        public double? AreaTotal
        {
            get { return Retrieve(r => r.AreaTotal); }
            set { Store(r => r.AreaTotal, value); }
        }

        public double? AreaConstruction
        {
            get { return Retrieve(r => r.AreaConstruction); }
            set { Store(r => r.AreaConstruction, value); }
        }

        public double? AreaGreen
        {
            get { return Retrieve(r => r.AreaGreen); }
            set { Store(r => r.AreaGreen, value); }
        }

        public int? TradeFloors
        {
            get { return Retrieve(r => r.TradeFloors); }
            set { Store(r => r.TradeFloors, value); }
        }

        public double? AreaTradeFloors
        {
            get { return Retrieve(r => r.AreaTradeFloors); }
            set { Store(r => r.AreaTradeFloors, value); }
        }

        public int? Basements
        {
            get { return Retrieve(r => r.Basements); }
            set { Store(r => r.Basements, value); }
        }

        public double? AreaBasements
        {
            get { return Retrieve(r => r.AreaBasements); }
            set { Store(r => r.AreaBasements, value); }
        }

        public int? Elevators
        {
            get { return Retrieve(r => r.Elevators); }
            set { Store(r => r.Elevators, value); }
        }

        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }

        public bool IsEnabled
        {
            get { return Retrieve(r => r.IsEnabled); }
            set { Store(r => r.IsEnabled, value); }
        }

        public bool IsHighlight
        {
            get { return Retrieve(r => r.IsHighlight); }
            set { Store(r => r.IsHighlight, value); }
        }
        public DateTime? HighlightExpiredTime
        {
            get { return Retrieve(r => r.HighlightExpiredTime); }
            set { Store(r => r.HighlightExpiredTime, value); }
        }

        public DateTime? CreatedDate
        {
            get { return Retrieve(r => r.CreatedDate); }
            set { Store(r => r.CreatedDate, value); }
        }

        public DateTime? UpdatedDate
        {
            get { return Retrieve(r => r.UpdatedDate); }
            set { Store(r => r.UpdatedDate, value); }
        }

        // Files

        public IEnumerable<PropertyFilePartRecord> Files
        {
            get { return Record.Files; }
        }

        #region Display

        public string DisplayForLocationAddress
        {
            get
            {
                var address = new List<string>();

                if (!string.IsNullOrEmpty(AddressNumber)) address.Add(AddressNumber);

                if (Street != null) address.Add(Street.Name);

                if (Ward != null) address.Add(Ward.Name);

                if (District != null) address.Add(District.Name);

                if (Province != null) address.Add(Province.Name);

                return string.Join(", ", address);
            }
        }

        #endregion

        #region Advantage & DisAdvantage

        // Advantages

        public IEnumerable<PropertyAdvantagePartRecord> Advantages
        {
            get
            {
                return
                    Record.Advantages.Where(r => r.PropertyAdvantagePartRecord.ShortName == "apartment-adv")
                        .Select(r => r.PropertyAdvantagePartRecord);
            }
        }

        // DisAdvantages

        public IEnumerable<PropertyAdvantagePartRecord> DisAdvantages
        {
            get
            {
                return
                    Record.Advantages.Where(r => r.PropertyAdvantagePartRecord.ShortName == "apart-disadv")
                        .Select(r => r.PropertyAdvantagePartRecord);
            }
        }

        #endregion
    }
}