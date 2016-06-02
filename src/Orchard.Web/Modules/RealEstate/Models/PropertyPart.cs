using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using RealEstate.Helpers;

namespace RealEstate.Models
{
    public class PropertyPartRecord : ContentPartRecord
    {
        public PropertyPartRecord()
        {
            Files = new List<PropertyFilePartRecord>();
            Customers = new List<CustomerPropertyRecord>();
            Advantages = new List<PropertyAdvantagePartRecordContent>();
            UserProperties = new List<PropertyUserPartRecordContent>();
        }

        #region Intial

        public virtual int? PropertyId { get; set; }
        public virtual string IdStr { get; set; }

        public virtual string PlacesAround { get; set; }
        public virtual string YoutubeId { get; set; }
        public virtual bool YoutubePublish { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual bool IsRefresh { get; set; }

        #region Flag
        
        public virtual PropertyFlagPartRecord Flag { get; set; }
        public virtual bool IsExcludeFromPriceEstimation { get; set; }

        #endregion

        #region Status

        public virtual PropertyStatusPartRecord Status { get; set; }
        public virtual DateTime? StatusChangedDate { get; set; }
        public virtual bool IsSoldByGroup { get; set; }

        #endregion

        #region AdsType

        // Ads Type
        public virtual AdsTypePartRecord AdsType { get; set; }

        public virtual bool Published { get; set; }
        public virtual DateTime? AdsExpirationDate { get; set; }

        public virtual bool AdsGoodDeal { get; set; }
        public virtual bool AdsGoodDealRequest { get; set; }
        public virtual DateTime? AdsGoodDealExpirationDate { get; set; }

        // ReSharper disable InconsistentNaming
        public virtual bool AdsVIP { get; set; }
        public virtual bool AdsVIPRequest { get; set; }
        public virtual DateTime? AdsVIPExpirationDate { get; set; }
        // ReSharper restore InconsistentNaming

        public virtual bool AdsHighlight { get; set; }
        public virtual bool AdsHighlightRequest { get; set; }
        public virtual DateTime? AdsHighlightExpirationDate { get; set; }

        #endregion

        #region Type

        // TypeGroup
        public virtual PropertyTypeGroupPartRecord TypeGroup { get; set; }

        // Type
        public virtual PropertyTypePartRecord Type { get; set; }

        // TypeConstruction
        public virtual PropertyTypeConstructionPartRecord TypeConstruction { get; set; }

        #endregion

        #region Address

        // Address
        public virtual LocationProvincePartRecord Province { get; set; }
        public virtual LocationDistrictPartRecord District { get; set; }
        public virtual LocationWardPartRecord Ward { get; set; }
        public virtual LocationStreetPartRecord Street { get; set; }
        public virtual LocationApartmentPartRecord Apartment { get; set; }
        public virtual LocationApartmentBlockPartRecord ApartmentBlock { get; set; }
        public virtual GroupInApartmentBlockPartRecord GroupInApartmentBlock { get; set; }
        public virtual int ApartmentPositionTh { get; set; }
        public virtual ApartmentBlockInfoPartRecord ApartmentBlockInfoPartRecord { get; set; }


        public virtual string OtherProvinceName { get; set; }
        public virtual string OtherDistrictName { get; set; }
        public virtual string OtherWardName { get; set; }
        public virtual string OtherStreetName { get; set; }
        public virtual string OtherProjectName { get; set; }

        public virtual string AddressNumber { get; set; }
        public virtual string AddressCorner { get; set; }
        public virtual int AlleyNumber { get; set; }
        public virtual bool PublishAddress { get; set; }

        #endregion

        #region Legal, Direction, Location

        // LegalStatus
        public virtual PropertyLegalStatusPartRecord LegalStatus { get; set; }
        // Direction
        public virtual DirectionPartRecord Direction { get; set; }
        // Location
        public virtual PropertyLocationPartRecord Location { get; set; }

        #endregion

        #region Alley

        // Alley
        public virtual int? AlleyTurns { get; set; }
        public virtual double? DistanceToStreet { get; set; }
        public virtual double? AlleyWidth { get; set; }
        public virtual double? AlleyWidth1 { get; set; }
        public virtual double? AlleyWidth2 { get; set; }
        public virtual double? AlleyWidth3 { get; set; }
        public virtual double? AlleyWidth4 { get; set; }
        public virtual double? AlleyWidth5 { get; set; }
        public virtual double? AlleyWidth6 { get; set; }
        public virtual double? AlleyWidth7 { get; set; }
        public virtual double? AlleyWidth8 { get; set; }
        public virtual double? AlleyWidth9 { get; set; }
        public virtual double? StreetWidth { get; set; }

        #endregion

        #region Area

        // Area for filter only
        public virtual double? Area { get; set; }

        // AreaTotal
        public virtual double? AreaTotal { get; set; }
        public virtual double? AreaTotalWidth { get; set; }
        public virtual double? AreaTotalLength { get; set; }
        public virtual double? AreaTotalBackWidth { get; set; }

        // AreaLegal
        public virtual double? AreaLegal { get; set; }
        public virtual double? AreaLegalWidth { get; set; }
        public virtual double? AreaLegalLength { get; set; }
        public virtual double? AreaLegalBackWidth { get; set; }
        public virtual double? AreaIlegalRecognized { get; set; }
        public virtual double? AreaIlegalNotRecognized { get; set; }

        // AreaResidential
        public virtual double? AreaResidential { get; set; }

        #endregion

        #region Construction

        // Construction
        public virtual double? AreaConstruction { get; set; }
        public virtual double? AreaConstructionFloor { get; set; }
        public virtual double? AreaUsable { get; set; }

        public virtual double? Floors { get; set; }
        public virtual int? Bedrooms { get; set; }
        public virtual int? Livingrooms { get; set; }
        public virtual int? Bathrooms { get; set; }
        public virtual int? Balconies { get; set; }

        public virtual PropertyInteriorPartRecord Interior { get; set; }
        public virtual double? RemainingValue { get; set; }

        public virtual bool HaveBasement { get; set; }
        public virtual bool HaveMezzanine { get; set; }
        public virtual bool HaveTerrace { get; set; }
        public virtual bool HaveGarage { get; set; }
        public virtual bool HaveElevator { get; set; }
        public virtual bool HaveSwimmingPool { get; set; }
        public virtual bool HaveGarden { get; set; }
        public virtual bool HaveSkylight { get; set; }

        #endregion

        #region Advantage & DisAdvantage

        // Advantage & DisAdvantage
        public virtual IList<PropertyAdvantagePartRecordContent> Advantages { get; set; }

        // OtherAdvantages
        public virtual double? OtherAdvantages { get; set; }

        // OtherAdvantagesDesc
        public virtual string OtherAdvantagesDesc { get; set; }

        // OtherDisAdvantages
        public virtual double? OtherDisAdvantages { get; set; }

        // OtherDisAdvantagesDesc
        public virtual string OtherDisAdvantagesDesc { get; set; }

        #endregion

        #region UserProperties

        public virtual IList<PropertyUserPartRecordContent> UserProperties { get; set; }

        #endregion

        #region Contact

        // Contact
        public virtual string ContactName { get; set; }
        public virtual string ContactPhone { get; set; }
        public virtual string ContactPhoneToDisplay { get; set; }
        public virtual string ContactAddress { get; set; }
        public virtual string ContactEmail { get; set; }
        public virtual bool PublishContact { get; set; }

        #endregion

        #region Price

        // Price
        public virtual double? PriceProposed { get; set; }
        // ReSharper disable InconsistentNaming
        public virtual double? PriceProposedInVND { get; set; }
        public virtual double? PriceEstimatedInVND { get; set; }
        // ReSharper restore InconsistentNaming
        public virtual double? PriceEstimatedByStaff { get; set; }
        public virtual double? PriceEstimatedRatingPoint { get; set; }
        public virtual string PriceEstimatedComment { get; set; }
        public virtual PaymentMethodPartRecord PaymentMethod { get; set; }
        public virtual PaymentUnitPartRecord PaymentUnit { get; set; }

        public virtual bool PriceNegotiable { get; set; }

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

        public virtual bool IsOwner { get; set; }
        public virtual bool NoBroker { get; set; }
        public virtual bool IsAuction { get; set; }
        public virtual bool IsHighlights { get; set; }
        public virtual bool IsAuthenticatedInfo { get; set; }

        #endregion

        #region User

        // Group
        public virtual UserGroupPartRecord UserGroup { get; set; }

        // User
        public virtual DateTime CreatedDate { get; set; }
        public virtual UserPartRecord CreatedUser { get; set; }
        public virtual DateTime LastUpdatedDate { get; set; }
        public virtual UserPartRecord LastUpdatedUser { get; set; }
        public virtual UserPartRecord FirstInfoFromUser { get; set; }
        public virtual UserPartRecord LastInfoFromUser { get; set; }
        public virtual DateTime? LastExportedDate { get; set; }
        public virtual UserPartRecord LastExportedUser { get; set; }

        #endregion

        #region Content

        // Ads Content
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
        public virtual string Note { get; set; }

        #endregion

        #region Apartment

        public virtual string ApartmentNumber { get; set; }
        public virtual int? ApartmentFloorTh { get; set; }

        public virtual int? ApartmentFloors { get; set; }
        public virtual int? ApartmentTradeFloors { get; set; }
        public virtual int? ApartmentElevators { get; set; }
        public virtual int? ApartmentBasements { get; set; }

        #endregion

        #endregion

        #region Related Content

        // Files
        public virtual IList<PropertyFilePartRecord> Files { get; set; }

        // Customers
        public virtual IList<CustomerPropertyRecord> Customers { get; set; }

        // PropertyExchanges
        public virtual IList<PropertyExchangePartRecord> PropertyExchanges { get; set; }

        #endregion

        #region Order by Group

        public bool OrderByGroupPhuoc { get; set; }
        public bool OrderByGroupDatPho  { get; set; }
        public bool OrderByGroupNghia { get; set; }
        public bool OrderByGroupCLBHN { get; set; }
        public bool OrderByGroupDuLieuNhaDat { get; set; }
        public bool OrderByGroupDinhGiaNhaDat { get; set; }

        #endregion
    }

    public class PropertyPart : ContentPart<PropertyPartRecord>
    {
        #region Intial 

        public int? PropertyId
        {
            get { return Retrieve(r => r.PropertyId); }
            set { Store(r => r.PropertyId, value); }
        }

        public string IdStr
        {
            get { return Retrieve(r => r.IdStr); }
            set { Store(r => r.IdStr, value); }
        }

        public string PlacesAround
        {
            get { return Retrieve(r => r.PlacesAround); }
            set { Store(r => r.PlacesAround, value); }
        }

        public string YoutubeId
        {
            get { return Retrieve(r => r.YoutubeId); }
            set { Store(r => r.YoutubeId, value); }
        }
        public bool YoutubePublish
        {
            get { return Retrieve(r => r.YoutubePublish); }
            set { Store(r => r.YoutubePublish, value); }
        }

        public int SeqOrder
        {
            get { return Retrieve(r => r.SeqOrder); }
            set { Store(r => r.SeqOrder, value); }
        }

        public virtual bool IsRefresh
        {
            get { return Retrieve(r => r.IsRefresh); }
            set { Store(r => r.IsRefresh, value); }
        }

        #region Flag

        public PropertyFlagPartRecord Flag
        {
            get { return Record.Flag; }
            set { Record.Flag = value; }
        }

        public bool IsExcludeFromPriceEstimation
        {
            get { return Retrieve(r => r.IsExcludeFromPriceEstimation); }
            set { Store(r => r.IsExcludeFromPriceEstimation, value); }
        }

        #endregion

        #region Status

        public PropertyStatusPartRecord Status
        {
            get { return Record.Status; }
            set { Record.Status = value; }
        }

        public DateTime? StatusChangedDate
        {
            get { return Retrieve(r => r.StatusChangedDate); }
            set { Store(r => r.StatusChangedDate, value); }
        }

        public bool IsSoldByGroup
        {
            get { return Retrieve(r => r.IsSoldByGroup); }
            set { Store(r => r.IsSoldByGroup, value); }
        }

        #endregion
        
        #region AdsType

        // AdsType

        public AdsTypePartRecord AdsType
        {
            get { return Record.AdsType; }
            set { Record.AdsType = value; }
        }

        public bool Published
        {
            get { return Retrieve(r => r.Published); }
            set { Store(r => r.Published, value); }
        }

        public DateTime? AdsExpirationDate
        {
            get { return Retrieve(r => r.AdsExpirationDate); }
            set { Store(r => r.AdsExpirationDate, value); }
        }

        public bool AdsGoodDeal
        {
            get { return Retrieve(r => r.AdsGoodDeal); }
            set { Store(r => r.AdsGoodDeal, value); }
        }

        public bool AdsGoodDealRequest
        {
            get { return Retrieve(r => r.AdsGoodDealRequest); }
            set { Store(r => r.AdsGoodDealRequest, value); }
        }

        public DateTime? AdsGoodDealExpirationDate
        {
            get { return Retrieve(r => r.AdsGoodDealExpirationDate); }
            set { Store(r => r.AdsGoodDealExpirationDate, value); }
        }

        // ReSharper disable InconsistentNaming
        public bool AdsVIP
        {
            get { return Retrieve(r => r.AdsVIP); }
            set { Store(r => r.AdsVIP, value); }
        }

        public bool AdsVIPRequest
        {
            get { return Retrieve(r => r.AdsVIPRequest); }
            set { Store(r => r.AdsVIPRequest, value); }
        }

        public DateTime? AdsVIPExpirationDate
        {
            get { return Retrieve(r => r.AdsVIPExpirationDate); }
            set { Store(r => r.AdsVIPExpirationDate, value); }
        }
        // ReSharper restore InconsistentNaming

        public bool AdsHighlight
        {
            get { return Retrieve(r => r.AdsHighlight); }
            set { Store(r => r.AdsHighlight, value); }
        }

        public bool AdsHighlightRequest
        {
            get { return Retrieve(r => r.AdsHighlightRequest); }
            set { Store(r => r.AdsHighlightRequest, value); }
        }

        public DateTime? AdsHighlightExpirationDate
        {
            get { return Retrieve(r => r.AdsHighlightExpirationDate); }
            set { Store(r => r.AdsHighlightExpirationDate, value); }
        }

        #endregion

        #region Type

        // TypeGroup
        public PropertyTypeGroupPartRecord TypeGroup
        {
            get { return Record.TypeGroup; }
            set { Record.TypeGroup = value; }
        }

        // Type
        public PropertyTypePartRecord Type
        {
            get { return Record.Type; }
            set { Record.Type = value; }
        }

        // TypeConstruction
        public PropertyTypeConstructionPartRecord TypeConstruction
        {
            get { return Record.TypeConstruction; }
            set { Record.TypeConstruction = value; }
        }

        #endregion

        #region Address

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

        public LocationApartmentPartRecord Apartment
        {
            get { return Record.Apartment; }
            set { Record.Apartment = value; }
        }

        public LocationApartmentBlockPartRecord ApartmentBlock
        {
            get { return Record.ApartmentBlock; }
            set { Record.ApartmentBlock = value; }
        }

        public GroupInApartmentBlockPartRecord GroupInApartmentBlock 
        {
            get { return Record.GroupInApartmentBlock; }
            set { Record.GroupInApartmentBlock = value; }
        }

        public int ApartmentPositionTh
        {
            get { return Retrieve(r => r.ApartmentPositionTh); }
            set { Store(r => r.ApartmentPositionTh, value); }
        }

        public ApartmentBlockInfoPartRecord ApartmentBlockInfoPartRecord
        {
            get { return Record.ApartmentBlockInfoPartRecord; }
            set { Record.ApartmentBlockInfoPartRecord = value; }
        }

        public string OtherProvinceName
        {
            get { return Retrieve(r => r.OtherProvinceName); }
            set { Store(r => r.OtherProvinceName, value); }
        }

        public string OtherDistrictName
        {
            get { return Retrieve(r => r.OtherDistrictName); }
            set { Store(r => r.OtherDistrictName, value); }
        }

        public string OtherWardName
        {
            get { return Retrieve(r => r.OtherWardName); }
            set { Store(r => r.OtherWardName, value); }
        }

        public string OtherStreetName
        {
            get { return Retrieve(r => r.OtherStreetName); }
            set { Store(r => r.OtherStreetName, value); }
        }

        public string OtherProjectName
        {
            get { return Retrieve(r => r.OtherProjectName); }
            set { Store(r => r.OtherProjectName, value); }
        }

        public string AddressNumber
        {
            get { return Retrieve(r => r.AddressNumber); }
            set { Store(r => r.AddressNumber, value); }
        }

        public string AddressCorner
        {
            get { return Retrieve(r => r.AddressCorner); }
            set { Store(r => r.AddressCorner, value); }
        }

        public int AlleyNumber
        {
            get { return Retrieve(r => r.AlleyNumber); }
            set { Store(r => r.AlleyNumber, value); }
        }

        public bool PublishAddress
        {
            get { return Retrieve(r => r.PublishAddress); }
            set { Store(r => r.PublishAddress, value); }
        }

        #endregion

        #region Legal, Direction, Location

        // LegalStatus

        public PropertyLegalStatusPartRecord LegalStatus
        {
            get { return Record.LegalStatus; }
            set { Record.LegalStatus = value; }
        }

        // Direction

        public DirectionPartRecord Direction
        {
            get { return Record.Direction; }
            set { Record.Direction = value; }
        }

        // Location

        public string DisplayForLocation
        {
            get
            {
                string location = "";

                try
                {
                    if (Location != null)
                    {
                        if (Location.CssClass == "h-front")
                        {
                            location = Location.ShortName;
                        }
                        else
                        {
                            location += Location.ShortName + " " + (AlleyWidth.HasValue ? AlleyWidth.ToString() : "?") +
                                         "m";
                        }
                    }
                }
                catch
                {
                }

                return location;
            }
        }

        public PropertyLocationPartRecord Location
        {
            get { return Record.Location; }
            set { Record.Location = value; }
        }

        #endregion

        #region Alley

        // Alley

        public string DisplayForAlley
        {
            get
            {
                string alley = "";

                try
                {
                    if (Location != null)
                    {
                        if (Location.CssClass == "h-front")
                        {
                            alley = Location.ShortName;
                        }
                        else
                        {
                            double? alleyWidth = AlleyWidth;
                            if (!(alleyWidth > 0))
                            {
                                int alleyTurns = AlleyTurns ?? 1;
                                alleyWidth =
                                    new List<double?>
                                    {
                                        AlleyWidth1,
                                        AlleyWidth2,
                                        AlleyWidth3,
                                        AlleyWidth4,
                                        AlleyWidth5,
                                        AlleyWidth6,
                                        AlleyWidth7,
                                        AlleyWidth8,
                                        AlleyWidth9
                                    }[alleyTurns - 1];
                            }

                            alley += Location.ShortName + " " + (alleyWidth > 0 ? alleyWidth.ToString() : "?") + "m";
                            if (DistanceToStreet > 0) alley += ", cách MT " + DistanceToStreet + "m";
                            if (AlleyTurns > 0) alley += ", " + AlleyTurns + " lần rẽ";
                        }
                    }
                }
                catch
                {
                }

                return alley;
            }
        }

        public string DisplayForTurns
        {
            get
            {
                var turns = new List<string>();

                try
                {
                    if (AlleyTurns.HasValue)
                    {
                        var widths = new List<string>
                        {
                            AlleyWidth1 != null ? AlleyWidth1.ToString() : "?",
                            AlleyWidth2 != null ? AlleyWidth2.ToString() : "?",
                            AlleyWidth3 != null ? AlleyWidth3.ToString() : "?",
                            AlleyWidth4 != null ? AlleyWidth4.ToString() : "?",
                            AlleyWidth5 != null ? AlleyWidth5.ToString() : "?",
                            AlleyWidth6 != null ? AlleyWidth6.ToString() : "?",
                            AlleyWidth7 != null ? AlleyWidth7.ToString() : "?",
                            AlleyWidth8 != null ? AlleyWidth8.ToString() : "?",
                            AlleyWidth9 != null ? AlleyWidth9.ToString() : "?"
                        };
                        for (int i = 0; i < AlleyTurns; i++)
                        {
                            turns.Add("h" + (i + 1) + " " + widths[i] + "m");
                        }
                    }
                }
                catch
                {
                }

                return string.Join(", ", turns);
            }
        }

        public int? AlleyTurns
        {
            get { return Retrieve(r => r.AlleyTurns); }
            set { Store(r => r.AlleyTurns, value); }
        }

        public double? DistanceToStreet
        {
            get { return Retrieve(r => r.DistanceToStreet); }
            set { Store(r => r.DistanceToStreet, value); }
        }

        public double? AlleyWidth
        {
            get { return Retrieve(r => r.AlleyWidth); }
            set { Store(r => r.AlleyWidth, value); }
        }

        public double? AlleyWidth1
        {
            get { return Retrieve(r => r.AlleyWidth1); }
            set { Store(r => r.AlleyWidth1, value); }
        }

        public double? AlleyWidth2
        {
            get { return Retrieve(r => r.AlleyWidth2); }
            set { Store(r => r.AlleyWidth2, value); }
        }

        public double? AlleyWidth3
        {
            get { return Retrieve(r => r.AlleyWidth3); }
            set { Store(r => r.AlleyWidth3, value); }
        }

        public double? AlleyWidth4
        {
            get { return Retrieve(r => r.AlleyWidth4); }
            set { Store(r => r.AlleyWidth4, value); }
        }

        public double? AlleyWidth5
        {
            get { return Retrieve(r => r.AlleyWidth5); }
            set { Store(r => r.AlleyWidth5, value); }
        }

        public double? AlleyWidth6
        {
            get { return Retrieve(r => r.AlleyWidth6); }
            set { Store(r => r.AlleyWidth6, value); }
        }

        public double? AlleyWidth7
        {
            get { return Retrieve(r => r.AlleyWidth7); }
            set { Store(r => r.AlleyWidth7, value); }
        }

        public double? AlleyWidth8
        {
            get { return Retrieve(r => r.AlleyWidth8); }
            set { Store(r => r.AlleyWidth8, value); }
        }

        public double? AlleyWidth9
        {
            get { return Retrieve(r => r.AlleyWidth9); }
            set { Store(r => r.AlleyWidth9, value); }
        }

        public double? StreetWidth
        {
            get { return Retrieve(r => r.StreetWidth); }
            set { Store(r => r.StreetWidth, value); }
        }

        #endregion

        #region Area

        // Area for filter only
        public double? Area
        {
            get { return Retrieve(r => r.Area); }
            set { Store(r => r.Area, value); }
        }

        // AreaTotal

        public string DisplayForAreaTotal
        {
            get
            {
                var area = new List<string>();

                try
                {
                    if (AreaTotal > 0)
                        area.Add(String.Format("{0:#,0.## m<sup>2</sup>}", AreaTotal));
                    if (AreaTotalWidth > 0 && AreaTotalLength > 0)
                        area.Add(String.Format("({0:#,0.##m} x {1:#,0.##m})", AreaTotalWidth, AreaTotalLength));
                    if (AreaTotalBackWidth > 0 && AreaTotalBackWidth != AreaTotalWidth)
                        area.Add(String.Format("{0:mặt hậu #,0.##m}", AreaTotalBackWidth));
                }
                catch
                {
                }

                return String.Join(" ", area);
            }
        }

        public double? AreaTotal
        {
            get { return Retrieve(r => r.AreaTotal); }
            set { Store(r => r.AreaTotal, value); }
        }

        public double? AreaTotalWidth
        {
            get { return Retrieve(r => r.AreaTotalWidth); }
            set { Store(r => r.AreaTotalWidth, value); }
        }

        public double? AreaTotalLength
        {
            get { return Retrieve(r => r.AreaTotalLength); }
            set { Store(r => r.AreaTotalLength, value); }
        }

        public double? AreaTotalBackWidth
        {
            get { return Retrieve(r => r.AreaTotalBackWidth); }
            set { Store(r => r.AreaTotalBackWidth, value); }
        }

        // AreaLegal

        public string DisplayForAreaLegal
        {
            get
            {
                var area = new List<string>();

                try
                {
                    if (AreaLegal > 0)
                        area.Add(String.Format("{0:#,0.## m<sup>2</sup>}", AreaLegal));
                    if (AreaLegalWidth > 0 && AreaLegalLength > 0)
                        area.Add(String.Format("({0:#,0.##m} x {1:#,0.##m})", AreaLegalWidth, AreaLegalLength));
                    if (AreaLegalBackWidth > 0 && AreaLegalBackWidth != AreaLegalWidth)
                        area.Add(String.Format("{0:mặt hậu #,0.##m}", AreaLegalBackWidth));
                }
                catch
                {
                }

                return String.Join(" ", area);
            }
        }

        public double? AreaLegal
        {
            get { return Retrieve(r => r.AreaLegal); }
            set { Store(r => r.AreaLegal, value); }
        }

        public double? AreaLegalWidth
        {
            get { return Retrieve(r => r.AreaLegalWidth); }
            set { Store(r => r.AreaLegalWidth, value); }
        }

        public double? AreaLegalLength
        {
            get { return Retrieve(r => r.AreaLegalLength); }
            set { Store(r => r.AreaLegalLength, value); }
        }

        public double? AreaLegalBackWidth
        {
            get { return Retrieve(r => r.AreaLegalBackWidth); }
            set { Store(r => r.AreaLegalBackWidth, value); }
        }

        public double? AreaIlegalRecognized
        {
            get { return Retrieve(r => r.AreaIlegalRecognized); }
            set { Store(r => r.AreaIlegalRecognized, value); }
        }

        public double? AreaIlegalNotRecognized
        {
            get { return Retrieve(r => r.AreaIlegalNotRecognized); }
            set { Store(r => r.AreaIlegalNotRecognized, value); }
        }

        // AreaResidential

        public double? AreaResidential
        {
            get { return Retrieve(r => r.AreaResidential); }
            set { Store(r => r.AreaResidential, value); }
        }

        #endregion

        #region Construction

        // Construction

        public double? AreaConstruction
        {
            get { return Retrieve(r => r.AreaConstruction); }
            set { Store(r => r.AreaConstruction, value); }
        }

        public double? AreaConstructionFloor
        {
            get { return Retrieve(r => r.AreaConstructionFloor); }
            set { Store(r => r.AreaConstructionFloor, value); }
        }

        public double? AreaUsable
        {
            get { return Retrieve(r => r.AreaUsable); }
            set { Store(r => r.AreaUsable, value); }
        }

        public double? Floors
        {
            get { return Retrieve(r => r.Floors); }
            set { Store(r => r.Floors, value); }
        }

        public int? Bedrooms
        {
            get { return Retrieve(r => r.Bedrooms); }
            set { Store(r => r.Bedrooms, value); }
        }

        public int? Livingrooms
        {
            get { return Retrieve(r => r.Livingrooms); }
            set { Store(r => r.Livingrooms, value); }
        }

        public int? Bathrooms
        {
            get { return Retrieve(r => r.Bathrooms); }
            set { Store(r => r.Bathrooms, value); }
        }

        public int? Balconies
        {
            get { return Retrieve(r => r.Balconies); }
            set { Store(r => r.Balconies, value); }
        }

        public PropertyInteriorPartRecord Interior
        {
            get { return Record.Interior; }
            set { Record.Interior = value; }
        }

        public double? RemainingValue
        {
            get { return Retrieve(r => r.RemainingValue); }
            set { Store(r => r.RemainingValue, value); }
        }

        public bool HaveBasement
        {
            get { return Retrieve(r => r.HaveBasement); }
            set { Store(r => r.HaveBasement, value); }
        }

        public bool HaveMezzanine
        {
            get { return Retrieve(r => r.HaveMezzanine); }
            set { Store(r => r.HaveMezzanine, value); }
        }

        public bool HaveElevator
        {
            get { return Retrieve(r => r.HaveElevator); }
            set { Store(r => r.HaveElevator, value); }
        }

        public bool HaveSwimmingPool
        {
            get { return Retrieve(r => r.HaveSwimmingPool); }
            set { Store(r => r.HaveSwimmingPool, value); }
        }

        public bool HaveGarage
        {
            get { return Retrieve(r => r.HaveGarage); }
            set { Store(r => r.HaveGarage, value); }
        }

        public bool HaveGarden
        {
            get { return Retrieve(r => r.HaveGarden); }
            set { Store(r => r.HaveGarden, value); }
        }

        public bool HaveTerrace
        {
            get { return Retrieve(r => r.HaveTerrace); }
            set { Store(r => r.HaveTerrace, value); }
        }

        public bool HaveSkylight
        {
            get { return Retrieve(r => r.HaveSkylight); }
            set { Store(r => r.HaveSkylight, value); }
        }

        #endregion

        #region Advantage & DisAdvantage

        // Advantages

        public IEnumerable<PropertyAdvantagePartRecord> Advantages {
            //get { return Record.Advantages.Where(r => r.PropertyAdvantagePartRecord.ShortName == "adv").Select(r => r.PropertyAdvantagePartRecord); }
            get; set; }

        // OtherAdvantages
        public double? OtherAdvantages
        {
            get { return Retrieve(r => r.OtherAdvantages); }
            set { Store(r => r.OtherAdvantages, value); }
        }

        // OtherAdvantagesDesc
        public string OtherAdvantagesDesc
        {
            get { return Retrieve(r => r.OtherAdvantagesDesc); }
            set { Store(r => r.OtherAdvantagesDesc, value); }
        }

        // DisAdvantages

        public IEnumerable<PropertyAdvantagePartRecord> DisAdvantages {
            //get { return Record.Advantages.Where(r => r.PropertyAdvantagePartRecord.ShortName == "disadv").Select(r => r.PropertyAdvantagePartRecord); }
            get; set; }

        // OtherDisAdvantages
        public double? OtherDisAdvantages
        {
            get { return Retrieve(r => r.OtherDisAdvantages); }
            set { Store(r => r.OtherDisAdvantages, value); }
        }

        // OtherDisAdvantagesDesc
        public string OtherDisAdvantagesDesc
        {
            get { return Retrieve(r => r.OtherDisAdvantagesDesc); }
            set { Store(r => r.OtherDisAdvantagesDesc, value); }
        }

        #endregion

        #region Contact

        // Contact

        public string ContactName
        {
            get { return Retrieve(r => r.ContactName); }
            set { Store(r => r.ContactName, value); }
        }

        public string ContactPhone
        {
            get { return Retrieve(r => r.ContactPhone); }
            set { Store(r => r.ContactPhone, value); }
        }

        public string ContactPhoneToDisplay
        {
            get { return Retrieve(r => r.ContactPhoneToDisplay); }
            set { Store(r => r.ContactPhoneToDisplay, value); }
        }

        public string ContactAddress
        {
            get { return Retrieve(r => r.ContactAddress); }
            set { Store(r => r.ContactAddress, value); }
        }

        public string ContactEmail
        {
            get { return Retrieve(r => r.ContactEmail); }
            set { Store(r => r.ContactEmail, value); }
        }

        public bool PublishContact
        {
            get { return Retrieve(r => r.PublishContact); }
            set { Store(r => r.PublishContact, value); }
        }

        #endregion

        #region Price

        // Price

        public string DisplayForPriceProposed
        {
            get
            {
                string price = "Giá thương lượng";

                try
                {
                    if (PriceProposed > 0)
                    {
                        if (AdsType != null)
                        {
                            switch (AdsType.CssClass)
                            {
                                case "ad-leasing":
                                    price = "Giá thuê: ";
                                    break;
                                default:
                                    price = "Giá bán: ";
                                    break;
                            }
                        }
                        if (PaymentUnit.CssClass == "unit-m2")
                        {
                            price += String.Format("{0:#,0.##} {1} / {2}", PriceProposed, PaymentMethod.ShortName,
                                PaymentUnit.Name);
                        }
                        else
                        {
                            price += String.Format("{0:#,0.##} {1}", PriceProposed, PaymentMethod.ShortName);
                        }
                    }
                }
                catch
                {
                }

                return price;
            }
        }

        public double? PriceProposed
        {
            get { return Retrieve(r => r.PriceProposed); }
            set { Store(r => r.PriceProposed, value); }
        }

        public double? PriceProposedInVND
        {
            get { return Retrieve(r => r.PriceProposedInVND); }
            set { Store(r => r.PriceProposedInVND, value); }
        }

        public double? PriceEstimatedInVND
        {
            get { return Retrieve(r => r.PriceEstimatedInVND); }
            set { Store(r => r.PriceEstimatedInVND, value); }
        }

        public double? PriceEstimatedByStaff
        {
            get { return Retrieve(r => r.PriceEstimatedByStaff); }
            set { Store(r => r.PriceEstimatedByStaff, value); }
        }

        public double? PriceEstimatedRatingPoint
        {
            get { return Retrieve(r => r.PriceEstimatedRatingPoint); }
            set { Store(r => r.PriceEstimatedRatingPoint, value); }
        }

        public string PriceEstimatedComment
        {
            get { return Retrieve(r => r.PriceEstimatedComment); }
            set { Store(r => r.PriceEstimatedComment, value); }
        }

        public PaymentMethodPartRecord PaymentMethod
        {
            get { return Record.PaymentMethod; }
            set { Record.PaymentMethod = value; }
        }

        public PaymentUnitPartRecord PaymentUnit
        {
            get { return Record.PaymentUnit; }
            set { Record.PaymentUnit = value; }
        }

        public bool PriceNegotiable
        {
            get { return Retrieve(r => r.PriceNegotiable); }
            set { Store(r => r.PriceNegotiable, value); }
        }

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

        public bool IsOwner
        {
            get { return Retrieve(r => r.IsOwner); }
            set { Store(r => r.IsOwner, value); }
        }

        public bool NoBroker
        {
            get { return Retrieve(r => r.NoBroker); }
            set { Store(r => r.NoBroker, value); }
        }

        public bool IsAuction
        {
            get { return Retrieve(r => r.IsAuction); }
            set { Store(r => r.IsAuction, value); }
        }

        public bool IsHighlights
        {
            get { return Retrieve(r => r.IsHighlights); }
            set { Store(r => r.IsHighlights, value); }
        }

        public bool IsAuthenticatedInfo
        {
            get { return Retrieve(r => r.IsAuthenticatedInfo); }
            set { Store(r => r.IsAuthenticatedInfo, value); }
        }

        #endregion

        #region User

        // Group

        public UserGroupPartRecord UserGroup
        {
            get { return Record.UserGroup; }
            set { Record.UserGroup = value; }
        }

        // User

        public DateTime CreatedDate
        {
            get { return Retrieve(r => r.CreatedDate); }
            set { Store(r => r.CreatedDate, value); }
        }

        public UserPartRecord CreatedUser
        {
            get { return Record.CreatedUser; }
            set { Record.CreatedUser = value; }
        }

        public DateTime LastUpdatedDate
        {
            get { return Retrieve(r => r.LastUpdatedDate); }
            set { Store(r => r.LastUpdatedDate, value); }
        }

        public UserPartRecord LastUpdatedUser
        {
            get { return Record.LastUpdatedUser; }
            set { Record.LastUpdatedUser = value; }
        }

        public UserPartRecord FirstInfoFromUser
        {
            get { return Record.FirstInfoFromUser; }
            set { Record.FirstInfoFromUser = value; }
        }

        public UserPartRecord LastInfoFromUser
        {
            get { return Record.LastInfoFromUser; }
            set { Record.LastInfoFromUser = value; }
        }

        public DateTime? LastExportedDate
        {
            get { return Retrieve(r => r.LastExportedDate); }
            set { Store(r => r.LastExportedDate, value); }
        }

        public UserPartRecord LastExportedUser
        {
            get { return Record.LastExportedUser; }
            set { Record.LastExportedUser = value; }
        }

        #endregion

        #region Content

        // Ads Content

        public string Title
        {
            get { return Retrieve(r => r.Title); }
            set { Store(r => r.Title, value); }
        }

        public string Content
        {
            get { return Retrieve(r => r.Content); }
            set { Store(r => r.Content, value); }
        }

        public string Note
        {
            get { return Retrieve(r => r.Note); }
            set { Store(r => r.Note, value); }
        }

        #endregion

        #region Apartment

        public string ApartmentNumber
        {
            get { return Retrieve(r => r.ApartmentNumber); }
            set { Store(r => r.ApartmentNumber, value); }
        }

        public int? ApartmentFloorTh
        {
            get { return Retrieve(r => r.ApartmentFloorTh); }
            set { Store(r => r.ApartmentFloorTh, value); }
        }

        public int? ApartmentFloors
        {
            get { return Retrieve(r => r.ApartmentFloors); }
            set { Store(r => r.ApartmentFloors, value); }
        }

        public int? ApartmentTradeFloors
        {
            get { return Retrieve(r => r.ApartmentTradeFloors); }
            set { Store(r => r.ApartmentTradeFloors, value); }
        }

        public int? ApartmentElevators
        {
            get { return Retrieve(r => r.ApartmentElevators); }
            set { Store(r => r.ApartmentElevators, value); }
        }

        public int? ApartmentBasements
        {
            get { return Retrieve(r => r.ApartmentBasements); }
            set { Store(r => r.ApartmentBasements, value); }
        }

        // ApartmentAdvantages
        public IEnumerable<PropertyAdvantagePartRecord> ApartmentAdvantages
        {
            get
            {
                return
                    Record.Advantages.Where(r => r.PropertyAdvantagePartRecord.ShortName == "apartment-adv")
                        .Select(r => r.PropertyAdvantagePartRecord);
            }
        }

        // ApartmentInteriorAdvantages
        public IEnumerable<PropertyAdvantagePartRecord> ApartmentInteriorAdvantages
        {
            get
            {
                return
                    Record.Advantages.Where(r => r.PropertyAdvantagePartRecord.ShortName == "apartment-interior-adv")
                        .Select(r => r.PropertyAdvantagePartRecord);
            }
        }

        #endregion

        #endregion

        #region Related Content

        // Files

        public IEnumerable<PropertyFilePartRecord> Files
        {
            get { return Record.Files; }
        }

        // Customers

        public IEnumerable<CustomerPartRecord> Customers
        {
            get { return Record.Customers.Select(r => r.CustomerPartRecord); }
        }

        //UserProperties
        public IEnumerable<UserPartRecord> UserProperties 
        {
            get { return Record.UserProperties.Select(r => r.UserPartRecord); }
        }

        //PropertyExchanges
        public IEnumerable<CustomerPartRecord> PropertyExchanges
        {
            get { return Record.PropertyExchanges.Select(r => r.Customer); }
        }
        #endregion

        #region Display

        #region Title & Address

        public string DisplayForUrl
        {
            get
            {
                string url = DisplayForTitleSEO.ToLower().RemoveSign4VietnameseString();

                // invalid chars, make into spaces
                url = Regex.Replace(url, @"[^a-z0-9.\s-]", "");
                // convert multiple spaces/hyphens into one space       
                url = Regex.Replace(url, @"[.\s-]+", " ").Trim();
                // cut and trim it
                //str = str.Substring(0, str.Length <= maxLength ? str.Length : maxLength).Trim();
                // hyphens
                url = Regex.Replace(url, @"\s", "-");

                // url sẽ bị lỗi 404 Not Found nếu có "tin-"
                return url.Replace("tin-", "tn-");
            }
        }

        public string DisplayForTitleSEO
        {
            get
            {
                var title = new List<string>();

                try
                {
                    if (!String.IsNullOrEmpty(Title))
                    {
                        title.Add(Title);
                    }
                    else
                    {
                        string prefix = "";
                        if (Status != null && Status.CssClass == "st-estimate")
                        {
                            prefix = "Định giá ";
                        }
                        else
                        {
                            prefix = (AdsType != null ? AdsType.ShortName + " " : "");
                        }

                        // AdsType + Address
                        title.Add(prefix + DisplayForLocationShortAddress); // Bán, Cho thuê

                        if (Apartment != null)
                        {
                            title.Add(String.Format("{0:#,0.## m2}", AreaUsable));
                        }
                        else
                        {
                            // Area
                            if (AreaTotal > 0)
                            {
                                title.Add(String.Format("{0:#,0.## m2}", AreaTotal));
                            }
                            else if (AreaTotalWidth > 0 && AreaTotalLength > 0)
                            {
                                title.Add(String.Format("{0:#,0.## m2}", AreaTotalWidth * AreaTotalLength));
                            }
                            else if (AreaUsable > 0)
                            {
                                title.Add(String.Format("{0:#,0.## m2}", AreaUsable));
                            }
                        }

                        // Price
                        if (PriceProposed > 0)
                        {
                            if (PaymentUnit.CssClass == "unit-m2")
                            {
                                title.Add(String.Format("{0:#,0.##} {1} / {2}", PriceProposed, PaymentMethod.ShortName,
                                    PaymentUnit.Name));
                            }
                            else
                            {
                                title.Add(String.Format("{0:#,0.##} {1}", PriceProposed, PaymentMethod.ShortName));
                            }
                        }
                        else
                        {
                            title.Add("giá thương lượng");
                        }
                    }
                }
                catch
                {
                }

                return String.Join(", ", title);
            }
        }

        public string DisplayForTitle
        {
            get
            {
                var title = new List<string>();

                try
                {
                    if (!String.IsNullOrEmpty(Title))
                    {
                        title.Add(Title);
                    }
                    else
                    {
                        string prefix;
                        if (Status != null && Status.CssClass == "st-estimate")
                        {
                            prefix = "Định giá ";
                        }
                        else
                        {
                            prefix = (AdsType != null ? AdsType.ShortName + " " : "");
                        }

                        // AdsType + Address
                        title.Add(prefix + DisplayForLocationFullAddress); // Bán, Cho thuê

                        // AreaTotal
                        //if (!String.IsNullOrEmpty(DisplayForAreaTotal)) _title.Add(DisplayForAreaTotal);

                        if (Apartment != null)
                        {
                            title.Add(String.Format("{0:#,0.## m2}", AreaUsable));
                        }
                        else
                        {
                            // Area
                            if (AreaTotal > 0)
                            {
                                title.Add(String.Format("{0:#,0.## m2}", AreaTotal));
                            }
                            else if (AreaTotalWidth > 0 && AreaTotalLength > 0)
                            {
                                title.Add(String.Format("{0:#,0.## m2}", AreaTotalWidth * AreaTotalLength));
                            }
                            else if (AreaUsable > 0)
                            {
                                title.Add(String.Format("{0:#,0.## m2}", AreaUsable));
                            }
                        }
                    }
                }
                catch
                {
                }

                return String.Join(", ", title);
            }
        }

        public string DisplayForTitleWithPriceProposed
        {
            get
            {
                var title = new List<string>();
                string price = "";

                try
                {
                    title.Add(DisplayForTitle);

                    // Floors
                    if (Floors > 0) title.Add(Floors + " lầu");
                    // Bedrooms
                    if (Bedrooms > 0) title.Add(Bedrooms + " PN");
                    // Bathrooms
                    if (Bathrooms > 0) title.Add(Bathrooms + " WC");

                    // PriceProposed
                    if (Status != null && Status.CssClass != "st-estimate")
                        if (!String.IsNullOrEmpty(DisplayForPriceProposed))
                            price =
                                (String.Format("<div class='text-danger text-bold'>{0}</div>", DisplayForPriceProposed));
                }
                catch
                {
                }

                return "<div>" + String.Join(", ", title) + "</div>" + price;
            }
        }

        public string DisplayForLocationFullAddress
        {
            get
            {
                string title = "";

                try
                {
                    if (Type != null) title += Type.ShortName + " "; // nhà

                    // District
                    //if (District != null)
                    //{
                    //    title += District.Name + ", ";
                    //}
                    //else if (!string.IsNullOrEmpty(OtherDistrictName))
                    //{
                    //    title += OtherDistrictName + ", ";
                    //}

                    if (Location != null) title += Location.ShortName + " "; // MT

                    if (Location != null && Location.CssClass.Equals("h-alley"))
                    {
                        if (AlleyWidth > 0)
                        {
                            title += AlleyWidth + "m, ";
                        }
                    }

                    title += DisplayForAddress;
                }
                catch
                {
                }

                return title;
            }
        }

        public string DisplayForLocationShortAddress
        {
            get
            {
                string title = "";

                try
                {
                    if (Type != null) title += Type.ShortName + " "; // nhà

                    // District
                    if (District != null)
                    {
                        title += District.Name + ", ";
                    }
                    else if (!string.IsNullOrEmpty(OtherDistrictName))
                    {
                        title += OtherDistrictName + ", ";
                    }

                    if (Location != null) title += Location.ShortName + " "; // MT

                    if (Location != null && Location.CssClass.Equals("h-alley"))
                    {
                        if (AlleyWidth > 0)
                        {
                            title += AlleyWidth + "m ";
                        }
                    }

                    title += DisplayForAddressInShort;
                }
                catch
                {
                }

                return title;
            }
        }

        public string DisplayForAddress
        {
            get
            {
                var address = new List<string>();

                try
                {
                    // Street
                    if (Street != null)
                    {
                        address.Add(DisplayForAddressNumber + Street.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherStreetName))
                    {
                        address.Add(DisplayForAddressNumber + OtherStreetName);
                    }
                    else
                    {
                        address.Add(DisplayForAddressNumber);
                    }

                    // Ward
                    if (Ward != null)
                    {
                        address.Add(Ward.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherWardName))
                    {
                        address.Add(OtherWardName);
                    }

                    // Ward
                    if (District != null)
                    {
                        address.Add(District.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherDistrictName))
                    {
                        address.Add(OtherDistrictName);
                    }

                    // Province
                    if (Province != null)
                    {
                        address.Add(!String.IsNullOrEmpty(Province.ShortName) ? Province.ShortName : Province.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherProvinceName))
                    {
                        address.Add(OtherProvinceName);
                    }
                }
                catch
                {
                }

                return string.Join(", ", address);
            }
        }

        public string DisplayForAddressForOwner
        {
            get
            {
                var address = new List<string>();

                try
                {
                    // Street
                    if (Street != null)
                    {
                        address.Add(DisplayForAddressNumberForOwner + Street.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherStreetName))
                    {
                        address.Add(DisplayForAddressNumberForOwner + OtherStreetName);
                    }
                    else
                    {
                        address.Add(DisplayForAddressNumberForOwner);
                    }

                    // Ward
                    if (Ward != null)
                    {
                        address.Add(Ward.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherWardName))
                    {
                        address.Add(OtherWardName);
                    }

                    // District
                    if (District != null)
                    {
                        address.Add(District.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherDistrictName))
                    {
                        address.Add(OtherDistrictName);
                    }

                    // Province
                    if (Province != null)
                    {
                        address.Add(!String.IsNullOrEmpty(Province.ShortName) ? Province.ShortName : Province.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherProvinceName))
                    {
                        address.Add(OtherProvinceName);
                    }
                }
                catch
                {
                }

                return string.Join(", ", address);
            }
        }

        public string DisplayForAddressInShort
        {
            get
            {
                var address = new List<string>();

                try
                {
                    // District
                    //if (District != null)
                    //{
                    //    if (!String.IsNullOrEmpty(District.ShortName))
                    //    {
                    //        _address.Add(District.ShortName);
                    //    }
                    //    else
                    //    {
                    //        _address.Add(District.Name);
                    //    }
                    //}
                    //else if (!string.IsNullOrEmpty(OtherDistrictName))
                    //{
                    //    _address.Add(OtherDistrictName);
                    //}

                    // Street
                    if (Street != null)
                    {
                        address.Add(DisplayForAddressNumber + Street.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherStreetName))
                    {
                        address.Add(DisplayForAddressNumber + OtherStreetName);
                    }


                    // Province
                    if (Province != null)
                    {
                        address.Add(!String.IsNullOrEmpty(Province.ShortName) ? Province.ShortName : Province.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherProvinceName))
                    {
                        address.Add(OtherProvinceName);
                    }
                }
                catch
                {
                }

                return string.Join(", ", address);
            }
        }

        public string DisplayForAddressNumber
        {
            get
            {
                string addressNumber = "";

                try
                {
                    bool isExpires = false;
                    string[] listStatus = new string[] { "st-onhold", "st-sold", "st-no-contact", "st-deleted", "st-pending", "st-trashed", "st-draft" };
                    if (Status.CssClass == "st-approved")
                    {
                        if (AdsExpirationDate < DateTime.Now) { isExpires = true; }
                    }
                    else if(listStatus.Contains(Status.CssClass)) { isExpires = true; }

                    if (TypeGroup != null && TypeGroup.CssClass == "gp-apartment")
                    {
                        // PublishAddress
                        if (PublishAddress && !isExpires)
                        {
                            addressNumber += !String.IsNullOrEmpty(ApartmentNumber) ? (ApartmentNumber + " ") : "";
                        }

                        if (Apartment != null)
                        {
                            addressNumber += !String.IsNullOrEmpty(Apartment.Name) ? (Apartment.Name + ", ") : "";
                        }
                        else
                        {
                            addressNumber += !String.IsNullOrEmpty(OtherProjectName) ? (OtherProjectName + ", ") : "";
                        }

                        addressNumber += DisplayForAddressNumberWithCorner;
                    }
                    else
                    {
                        // PublishAddress
                        if (PublishAddress && !isExpires)
                        {
                            addressNumber += DisplayForAddressNumberWithCorner;
                        }

                        addressNumber += !String.IsNullOrEmpty(OtherProjectName) ? (OtherProjectName + ", ") : "";
                    }
                }
                catch
                {
                }

                return addressNumber;
            }
        }

        public string DisplayForAddressNumberForOwner
        {
            get
            {
                string addressNumber = "";

                try
                {
                    if (TypeGroup != null && TypeGroup.CssClass == "gp-apartment")
                    {
                        addressNumber += !String.IsNullOrEmpty(ApartmentNumber) ? (ApartmentNumber + " ") : "";

                        if (Apartment != null)
                        {
                            addressNumber += !String.IsNullOrEmpty(Apartment.Name) ? (Apartment.Name + ", ") : "";
                        }
                        else
                        {
                            addressNumber += !String.IsNullOrEmpty(OtherProjectName) ? (OtherProjectName + ", ") : "";
                        }

                        addressNumber += DisplayForAddressNumberWithCorner;
                    }
                    else
                    {
                        addressNumber += DisplayForAddressNumberWithCorner;
                        addressNumber += !String.IsNullOrEmpty(OtherProjectName) ? (OtherProjectName + ", ") : "";
                    }
                }
                catch
                {
                }

                return addressNumber;
            }
        }

        public string DisplayForAddressNumberWithCorner
        {
            get
            {
                string addressNumber = "";

                try
                {
                    if (Province != null && Province.Name == "Hà Nội")
                    {
                        addressNumber += !String.IsNullOrEmpty(AddressNumber) ? ("số " + AddressNumber + " ") : "";
                        addressNumber += !String.IsNullOrEmpty(AddressCorner)
                            ? ((AddressCorner.IndexOf('/') > 0 ? "ngách " : "ngõ ") + AddressCorner + " ")
                            : "";
                    }
                    else
                    {
                        addressNumber += !String.IsNullOrEmpty(AddressNumber) ? ("số " + AddressNumber + " ") : "";
                    }
                }
                catch
                {
                }

                return addressNumber;
            }
        }

        public string DisplayForAddressPlacesRound
        {
            get
            {
                var address = new List<string>();

                try
                {
                    // Street
                    if (Street != null)
                    {
                        address.Add(AddressNumber + " " + Street.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherStreetName))
                    {
                        address.Add(AddressNumber + " " + OtherStreetName);
                    }

                    // Ward
                    if (Ward != null)
                    {
                        address.Add(Ward.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherWardName))
                    {
                        address.Add(OtherWardName);
                    }

                    // District
                    if (District != null)
                    {
                        address.Add(District.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherDistrictName))
                    {
                        address.Add(OtherDistrictName);
                    }

                    // Province
                    if (Province != null)
                    {
                        address.Add(!String.IsNullOrEmpty(Province.ShortName) ? Province.ShortName : Province.Name);
                    }
                    else if (!string.IsNullOrEmpty(OtherProvinceName))
                    {
                        address.Add(OtherProvinceName);
                    }
                }
                catch
                {
                }

                return string.Join(", ", address);
            }
        }

        #endregion

        #region PropertyInfo

        public string DisplayForAreaConstructionLocationInfo
        {
            get
            {
                var propertyInfo = new List<string>();

                try
                {
                    string displayForAreaInfo = DisplayForAreaInfo;
                    if (!String.IsNullOrEmpty(displayForAreaInfo)) propertyInfo.Add(displayForAreaInfo);

                    string displayForConstructionInfo = DisplayForConstructionInfo;
                    if (!String.IsNullOrEmpty(displayForConstructionInfo)) propertyInfo.Add(displayForConstructionInfo);

                    string displayForLocationInfo = DisplayForLocationInfo;
                    if (!String.IsNullOrEmpty(displayForLocationInfo)) propertyInfo.Add(displayForLocationInfo);
                }
                catch
                {
                }

                return String.Join(", ", propertyInfo);
            }
        }

        public string DisplayForAreaConstructionInfo
        {
            get
            {
                var propertyInfo = new List<string>();

                try
                {
                    string displayForAreaInfo = DisplayForAreaInfo;
                    if (!String.IsNullOrEmpty(displayForAreaInfo)) propertyInfo.Add(displayForAreaInfo);

                    string displayForConstructionInfo = DisplayForConstructionInfo;
                    if (!String.IsNullOrEmpty(displayForConstructionInfo)) propertyInfo.Add(displayForConstructionInfo);
                }
                catch
                {
                }

                return String.Join(", ", propertyInfo);
            }
        }

        public string DisplayForAreaInfo
        {
            get
            {
                var areaInfo = new List<string>();

                try
                {
                    string displayForAreaTotal = DisplayForAreaTotal;
                    string displayForAreaLegal = DisplayForAreaLegal;
                    switch (TypeGroup.CssClass)
                    {
                        case "gp-house":
                            // DisplayForAreaTotal
                            if (!String.IsNullOrEmpty(displayForAreaTotal))
                                areaInfo.Add("Diện tích khuôn viên " + displayForAreaTotal);
                            // DisplayForAreaLegal
                            if (!String.IsNullOrEmpty(displayForAreaLegal) && displayForAreaLegal != displayForAreaTotal)
                                areaInfo.Add("Diện tích hợp quy hoạch " + displayForAreaLegal);

                            // AreaConstruction
                            if (AreaConstruction > 0)
                                areaInfo.Add("Diện tích xây dựng " + String.Format("{0:#,0.##}", AreaConstruction) +
                                             "m<sup>2</sup>");

                            // AreaConstructionFloor
                            if (AreaConstructionFloor > 0)
                                areaInfo.Add("Tổng diện tích sàn xây dựng " + String.Format("{0:#,0.##}", AreaConstructionFloor) +
                                             "m<sup>2</sup>");
                            // AreaUsable
                            if (AreaUsable > 0 && AreaUsable != AreaTotal)
                                areaInfo.Add("Tổng diện tích sử dụng " + String.Format("{0:#,0.##}", AreaUsable) +
                                             "m<sup>2</sup>");
                            break;
                        case "gp-apartment":
                            // AreaUsable
                            if (AreaUsable > 0)
                                areaInfo.Add("Tổng diện tích sử dụng " + String.Format("{0:#,0.##}", AreaUsable) +
                                             "m<sup>2</sup>");
                            break;
                        case "gp-land":
                            // DisplayForAreaTotal
                            if (!String.IsNullOrEmpty(displayForAreaTotal))
                                areaInfo.Add("Diện tích khuôn viên " + displayForAreaTotal);
                            // DisplayForAreaLegal
                            if (!String.IsNullOrEmpty(displayForAreaLegal) && displayForAreaLegal != displayForAreaTotal)
                                areaInfo.Add("Diện tích hợp quy hoạch " + displayForAreaLegal);
                            // AreaResidential
                            if (AreaResidential > 0 && AreaResidential != AreaTotal)
                                areaInfo.Add("Diện tích đất thổ cư " + String.Format("{0:#,0.##}", AreaResidential) +
                                             "m<sup>2</sup>");
                            // AreaConstruction
                            if (AreaConstruction > 0)
                                areaInfo.Add("Diện tích xây dựng " + String.Format("{0:#,0.##}", AreaConstruction) +
                                             "m<sup>2</sup>");
                            // AreaUsable
                            if (AreaUsable > 0 && AreaUsable != AreaTotal)
                                areaInfo.Add("Tổng diện tích sử dụng " + String.Format("{0:#,0.##}", AreaUsable) +
                                             "m<sup>2</sup>");
                            break;
                    }
                }
                catch
                {
                }

                return String.Join(", ", areaInfo);
            }
        }

        public string DisplayForConstructionInfo
        {
            get
            {
                var constructionInfo = new List<string>();

                try
                {
                    switch (TypeGroup.CssClass)
                    {
                        case "gp-house":
                            //if (item.Interior != null) { constructionInfo.Add(Interior.Name); }
                            if (Floors > 0) constructionInfo.Add(Floors + " lầu");
                            if (Bedrooms > 0) constructionInfo.Add(Bedrooms + " PN");
                            if (Bathrooms > 0) constructionInfo.Add(Bathrooms + " WC");

                            if (HaveBasement) constructionInfo.Add("có tầng hầm");
                            if (HaveMezzanine) constructionInfo.Add("có lửng");
                            if (HaveTerrace) constructionInfo.Add("có sân thượng");
                            if (HaveSkylight) constructionInfo.Add("có giếng trời");
                            if (HaveGarage) constructionInfo.Add("có gara");
                            if (HaveGarden) constructionInfo.Add("có sân vườn");
                            if (HaveElevator) constructionInfo.Add("có thang máy");
                            if (HaveSwimmingPool) constructionInfo.Add("có bể bơi");

                            if (RemainingValue > 0)
                                constructionInfo.Add("chất lượng nhà còn lại " + RemainingValue + "%");
                            if (Direction != null) constructionInfo.Add("hướng " + Direction.Name);
                            if (LegalStatus != null) constructionInfo.Add(LegalStatus.Name);

                            break;

                        case "gp-apartment":

                            string apartmentName = "";
                            if (Apartment != null) apartmentName = Apartment.Name;
                            if (ApartmentFloorTh.HasValue)
                                constructionInfo.Add("căn hộ thuộc tầng " + ApartmentFloorTh);
                            if (Balconies > 0) constructionInfo.Add(Balconies + " ban công (view)");
                            if (Bedrooms > 0) constructionInfo.Add(Bedrooms + " PN");
                            if (Bathrooms > 0) constructionInfo.Add(Bathrooms + " WC");

                            if (RemainingValue > 0)
                                constructionInfo.Add("chất lượng nhà còn lại " + RemainingValue + "%");
                            if (Direction != null) constructionInfo.Add("hướng " + Direction.Name);
                            if (LegalStatus != null) constructionInfo.Add(LegalStatus.Name);

                            if (ApartmentFloors > 0)
                                constructionInfo.Add("Chung cư " + apartmentName + " có " + ApartmentFloors + " tầng");
                            if (ApartmentElevators > 0) constructionInfo.Add(ApartmentElevators + " thang máy");
                            if (ApartmentBasements > 0) constructionInfo.Add(ApartmentBasements + " tầng hầm");
                            if (Apartment != null) constructionInfo.Add(Apartment.Name);

                            break;

                        case "gp-land":

                            if (Direction != null) constructionInfo.Add("hướng " + Direction.Name);
                            if (LegalStatus != null) constructionInfo.Add(LegalStatus.Name);

                            break;
                    }
                }
                catch
                {
                }

                return String.Join(", ", constructionInfo);
            }
        }

        public string DisplayForLocationInfo
        {
            get
            {
                var locationInfo = new List<string>();

                try
                {
                    switch (TypeGroup.CssClass)
                    {
                        case "gp-house":
                        case "gp-land":
                            if (Location != null)
                            {
                                switch (Location.CssClass)
                                {
                                    case "h-front":
                                        if (StreetWidth > 0)
                                            locationInfo.Add("đường trước nhà rộng " + StreetWidth + "m");
                                        break;
                                    case "h-alley":
                                        if (DistanceToStreet > 0) locationInfo.Add("cách MT " + DistanceToStreet + "m");
                                        if (AlleyTurns > 0) locationInfo.Add(AlleyTurns + " lần rẽ");

                                        var widths = new List<string>
                                        {
                                            AlleyWidth1 != null ? AlleyWidth1.ToString() : "?",
                                            AlleyWidth2 != null ? AlleyWidth2.ToString() : "?",
                                            AlleyWidth3 != null ? AlleyWidth3.ToString() : "?",
                                            AlleyWidth4 != null ? AlleyWidth4.ToString() : "?",
                                            AlleyWidth5 != null ? AlleyWidth5.ToString() : "?",
                                            AlleyWidth6 != null ? AlleyWidth6.ToString() : "?",
                                            AlleyWidth7 != null ? AlleyWidth7.ToString() : "?",
                                            AlleyWidth8 != null ? AlleyWidth8.ToString() : "?",
                                            AlleyWidth9 != null ? AlleyWidth9.ToString() : "?"
                                        };
                                        for (int i = 0; i < AlleyTurns; i++)
                                        {
                                            if ((i + 1) == AlleyTurns)
                                                locationInfo.Add("hẻm trước nhà rộng " + widths[i] + "m");
                                            else
                                                locationInfo.Add("lần rẽ " + (i + 1) + " rộng " + widths[i] + "m");
                                        }
                                        break;
                                }
                            }
                            break;
                        case "gp-apartment":

                            break;
                    }
                }
                catch
                {
                }

                return String.Join(", ", locationInfo);
            }
        }

        #endregion

        #endregion

        #region Order by Group

        public bool OrderByGroupPhuoc
        {
            get { return Retrieve(r => r.OrderByGroupPhuoc); }
            set { Store(r => r.OrderByGroupPhuoc, value); }
        }
        public bool OrderByGroupDatPho
        {
            get { return Retrieve(r => r.OrderByGroupDatPho); }
            set { Store(r => r.OrderByGroupDatPho, value); }
        }
        public bool OrderByGroupNghia
        {
            get { return Retrieve(r => r.OrderByGroupNghia); }
            set { Store(r => r.OrderByGroupNghia, value); }
        }
        public bool OrderByGroupCLBHN
        {
            get { return Retrieve(r => r.OrderByGroupCLBHN); }
            set { Store(r => r.OrderByGroupCLBHN, value); }
        }
        public bool OrderByGroupDuLieuNhaDat
        {
            get { return Retrieve(r => r.OrderByGroupDuLieuNhaDat); }
            set { Store(r => r.OrderByGroupDuLieuNhaDat, value); }
        }

        public bool OrderByGroupDinhGiaNhaDat
        {
            get { return Retrieve(r => r.OrderByGroupDinhGiaNhaDat); }
            set { Store(r => r.OrderByGroupDinhGiaNhaDat, value); }
        }

        #endregion
    }
}