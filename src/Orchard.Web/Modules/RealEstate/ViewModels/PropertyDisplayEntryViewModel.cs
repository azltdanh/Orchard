using System.Web.Mvc;
using RealEstate.Models;
using Contrib.OnlineUsers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Users.Models;

namespace RealEstate.ViewModels
{
    #region Properties

    public class PropertyDisplayIndexViewModel
    {
        public IEnumerable<PropertyDisplayEntry> Properties { get; set; }

        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }
        public PropertyDisplayIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }

        // Debug
        public string MsgHideAddressProperties { get; set; }
        public string MsgShowAddressProperties { get; set; }

        public int CountSellingGoodDeal { get; set; }
        public int CountLeasingGoodDeal { get; set; }

        public int CountSellingVip { get; set; }
        public int CountLeasingVip { get; set; }

        public int CountBuyingVip { get; set; }
        public int CountRentingVip { get; set; }

        public int CountSelling { get; set; }
        public int CountLeasing { get; set; }

        public int CountBuying { get; set; }
        public int CountRenting { get; set; }

        public string DisplayForDescription
        {
            get
            {
                List<string> _desc = new List<string>();

                if (Options.AdsType != null)
                {
                    _desc.Add(Options.AdsType.ShortName); // Bán || Cho thuê || Cần mua || Cần thuê
                }

                if (Options.Types != null)
                {
                    foreach (var i in Options.Types)
                    {
                        _desc.Add(i.Name + " ");
                    }
                }
                else
                {
                    if (Options.TypeGroup != null)
                    {
                        _desc.Add(Options.TypeGroup.ShortName);// nhà || căn hộ || đất
                    }
                    else
                    {
                        _desc.Add(" nhà đất");
                    }
                }

                if (Options.AdsGoodDeal == true) _desc.Add(" giá rẻ");
                if (Options.AdsVIP == true) _desc.Add(" giao dịch gấp");
                if (Options.IsOwner == true) _desc.Add(" chính chủ");
                if (Options.IsAuction == true) _desc.Add(" phát mãi");
                
                if (Options.Province != null) _desc.Add(" tại");

                if (Options.Streets != null)
                {
                    _desc.Add("Đường");
                    foreach (var i in Options.Streets)
                    {
                        _desc.Add(i.Name + ",");
                    }
                }

                if (Options.Wards != null)
                {
                    foreach (var i in Options.Wards)
                    {
                        _desc.Add(i.Name + ",");
                    }
                }

                if (Options.Districts != null)
                {
                    foreach (var i in Options.Districts)
                    {
                        _desc.Add(i.Name + ",");
                    }
                }

                if (Options.Province != null)
                {
                    _desc.Add(Options.Province.Name);
                }
                return String.Join(" ", _desc);
            }
        }
    }

    public class PropertyDisplayEntry
    {
        public PropertyPart Property { get; set; }

        #region Advantage & DisAdvantage

        // Advantages
        public IEnumerable<PropertyAdvantagePartRecord> Advantages { get; set; }

        // DisAdvantages
        public IEnumerable<PropertyAdvantagePartRecord> DisAdvantages { get; set; }

        // ApartmentAdvantages
        public IEnumerable<PropertyAdvantagePartRecord> ApartmentAdvantages { get; set; }

        // ApartmentInteriorAdvantages
        public IEnumerable<PropertyAdvantagePartRecord> ApartmentInteriorAdvantages { get; set; }

        #endregion

        public bool IsChecked { get; set; }

        public int UserViews { get; set; }

        public string UserNameDisplay { get; set; }

        public int? DomainGroup { get; set; }

        #region Images

        public virtual IEnumerable<PropertyFilePart> Files { get; set; }

        public string DefaultImgUrl { get; set; }

        #endregion

        public string DisplayForContact { get; set; }

        public List<string> DisplayForPhone { get; set; }
       
        //UserLocation
        public int PropertyId { get; set; }
        public int[] UserPartIds { get; set; }
        public IEnumerable<UserLocationRecord> UserLocations { get; set; }
        public IEnumerable<UserUpdateOptions> UserUpdateOptions { get; set; }

        //ApartmentBlockContentOrther
        public string ApartmentBlockInfoContent { get; set; }
    }

    #endregion

    #region Customers

    public class CustomerDisplayIndexViewModel
    {
        public IEnumerable<CustomerDisplayEntry> Customers { get; set; }

        public PropertyDisplayIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }

        public string DisplayForDescription
        {
            get
            {
                List<string> _desc = new List<string>();

                if (Options.AdsType != null)
                {
                    _desc.Add(Options.AdsType.ShortName); // Cần mua || Cần thuê
                }

                if (Options.Types != null)
                {
                    foreach (var i in Options.Types)
                    {
                        _desc.Add(i.Name + " ");
                    }
                }
                else
                {
                    if (Options.TypeGroup != null)
                    {
                        _desc.Add(" " + Options.TypeGroup.ShortName);
                    }
                    else
                    {
                        _desc.Add(" nhà đất");
                    }
                }
                if (Options.AdsGoodDeal == true) _desc.Add(" giá rẻ");
                if (Options.AdsVIP == true) _desc.Add(" giao dịch gấp");
                if (Options.IsOwner == true) _desc.Add(" chính chủ");
                if (Options.IsAuction == true) _desc.Add(" phát mãi");

                if (Options.Province != null) _desc.Add(" tại");

                if (Options.Streets != null)
                {
                    _desc.Add("Đường");
                    foreach (var i in Options.Streets)
                    {
                        _desc.Add(i.Name + ",");
                    }
                }

                if (Options.Wards != null)
                {
                    foreach (var i in Options.Wards)
                    {
                        _desc.Add(i.Name + ",");
                    }
                }

                if (Options.Districts != null)
                {
                    foreach (var i in Options.Districts)
                    {
                        _desc.Add(i.Name + ",");
                    }
                }

                if (Options.Province != null)
                {
                    _desc.Add(Options.Province.Name);
                }
                return String.Join(" ", _desc);
            }
        }
    }

    public class CustomerDisplayEntry
    {
        public CustomerPart Customer { get; set; }

        public IEnumerable<CustomerPurposePartRecord> Purposes { get; set; }

        public bool IsChecked { get; set; }
    }

    #endregion

    public class PropertyDisplayIndexOptions
    {
        // PropertyId
        public int? PropertyId { get; set; }

        // Province
        public int? ProvinceId { get; set; }
        public LocationProvincePartRecord Province { get; set; }
        public IEnumerable<LocationProvincePart> Provinces { get; set; }
        public List<SelectListItem> ListProvinces { get; set; }

        // Districts
        public int[] DistrictIds { get; set; }
        public IEnumerable<LocationDistrictPart> Districts { get; set; }
        public List<SelectListItem> ListDistricts { get; set; }

        // Wards
        public int[] WardIds { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public List<SelectListItem> ListWards { get; set; }

        // Streets
        public int[] StreetIds { get; set; }
        public IEnumerable<LocationStreetPart> Streets { get; set; }
        public List<SelectListItem> ListStreets { get; set; }

        // Apartments
        public int[] ApartmentIds { get; set; }
        public IEnumerable<LocationApartmentPart> Apartments { get; set; }
        public List<SelectListItem> ListApartments { get; set; }

        // AdsType
        public string AdsTypeCssClass { get; set; }
        public AdsTypePartRecord AdsType { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        // TypeGroup
        public string TypeGroupCssClass { get; set; }
        public PropertyTypeGroupPartRecord TypeGroup { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // TypeGroup
        public int? TypeId { get; set; }
        public string TypeCssClass { get; set; }
        public PropertyTypePartRecord Type { get; set; }
        public int[] TypeIds { get; set; }
        public IEnumerable<PropertyTypePartRecord> Types { get; set; }

        public PropertyStatusPartRecord Status { get; set; }
        public int[] StatusIds { get; set; }
        public IEnumerable<PropertyStatusPartRecord> Statuses { get; set; }

        // Directions
        public int[] DirectionIds { get; set; }
        public IEnumerable<DirectionPartRecord> Directions { get; set; }

        // Location
        public int? LocationId { get; set; }

        public double? MinAlleyWidth { get; set; }

        public double? MinAreaTotal { get; set; }
        public double? MinAreaTotalWidth { get; set; }
        public double? MinAreaTotalLength { get; set; }
        public double? MinAreaUsable { get; set; }

        public double? MinFloors { get; set; }
        public int? MinBedrooms { get; set; }

        public List<PriceData> ListPrice { get; set; }
        public int? PriceDataId { get; set; }//tỷ VNĐ

        public double? MinPriceProposedInVND { get; set; }
        public double? MaxPriceProposedInVND { get; set; }

        //UserLocation
        public UserLocationRecord UserLocation { get; set; }
        public int[] UserLocationIds { get; set; }
        public int[] UserPartIds { get; set; }
        public IEnumerable<UserLocationRecord> UserLocations { get; set; }
        public IEnumerable<UserUpdateOptions> UserUpdateOptions { get; set; }

        //Them
        public bool FlagCheapPrice { get; set; }
        public bool AdsHighlight { get; set; }
        public int AdsHighlightCount { get; set; }
        public bool AdsGoodDeal { get; set; }
        public int AdsGoodDealCount { get; set; }
        public bool AdsVIP { get; set; }
        public int AdsVIPCount { get; set; }
        public string AdsTitle { get; set; }
        public string AdsClass { get; set; }
        public bool AdsNormal { get; set; }
        public bool IsOwner { get; set; }
        public bool IsAuction { get; set; }
        public bool IsAll { get; set; }
        public int? PageCurent { get; set; }

        public int? ApartmentFloorTh { get; set; }
        public string OtherProjectName { get; set; }

        public double? MinPriceProposed { get; set; }
        public double? MaxPriceProposed { get; set; }
        public string PaymentMethodCssClass { get; set; }
        public IEnumerable<PaymentMethodPartRecord> PaymentMethods { get; set; }

        public int[] AnyType { get; set; }
        public int AllAnyType { get; set; }

        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }

        public string btSubmit { get; set; }

        public PropertyDisplayOrder Order { get; set; }

        public PropertyDisplayLocation AlleyTurnsRange { get; set; }

        public PropertyDisplayApartmentFloorTh ApartmentFloorThRange { get; set; }

        public bool flagAjax { get; set; }

        public bool flagAsideFirst { get; set; }

        public bool flagRequirment { get; set; }
        public bool PropertyExchange { get; set; }

        public string TitleArticle { get; set; }
        //public Dictionary<string, string> MetaLayout { get; set; }
        public string MetaTitle { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }

        public string SearchPhone { get; set; }
        
        public int? Create_Id { get; set; }

    }

    public class PriceData
    {
        public int Id { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string Name { get; set; }
    }

    public class UserUpdateOptions
    {
        public UserUpdateProfileRecord UserUpdateProfilePart { get; set; }
        public UserPartRecord UserPart { get; set; }
        public UserLocationRecord UserLocation { get; set; }
    }

    public enum PropertyDisplayOrder
    {
        LastUpdatedDate,
        AddressNumber,
        PriceProposedInVND
    }

    public enum PropertyDisplayLocation
    {
        None,
        All,
        AllWalk,
        Alley6,
        Alley5,
        Alley4,
        Alley3,
        Alley2,
        Alley
    }

    public enum PropertyDisplayApartmentFloorTh
    {
        None,
        All,
        ApartmentFloorTh1To3,
        ApartmentFloorTh4To7,
        ApartmentFloorTh8To12,
        ApartmentFloorTh12
    }

    public enum PropertyDisplayStatus
    {
        None,
        All,
        AdsGoodDeal,
        AdsVIP,
        IsOwner,
        IsAuction
    }
}
