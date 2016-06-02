using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using Orchard;
using Orchard.Alias;
using Orchard.Alias.Records;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using RealEstate.Helpers;
using Contrib.OnlineUsers.Models;

namespace RealEstate.FrontEnd.Services
{
    public interface IFastFilterService : IDependency
    {
        IEnumerable<AdsTypePartRecord> GetAdsTypesFromRoute();

        IEnumerable<PropertyDisplayEntry> BuildPropertiesEntries(IEnumerable<PropertyPart> pList);
        IEnumerable<PropertyDisplayEntry> BuildPropertiesEntriesFront(IEnumerable<PropertyPart> pList);
        IEnumerable<PropertyDisplayEntry> BuildPropertiesEntriesAlley(IEnumerable<PropertyPart> pList);

        int UpdateRequirements(CustomerPartRecord customer, CustomerEditRequirementViewModel reqModel);

        string GetFrontEndSetting(string name);

        #region Dictionary SEO

        Dictionary<string, string> Metas(string province, string anytype, int flatAdsType, int flatTypeGroup);

        Dictionary<string, string> Metas(string metaTitle, string metaKeyword, string metaDescription, string district,
            string province, string anytype);

        #endregion

        #region Update/Insert AliasMeta

        void UpdateAliasMeta(AliasRecord alias, Dictionary<string, string> value);
        void UpdateAliasMeta(AliasRecord alias, AliasesMetaCreatedOptions options, string district, string province, string anytype);
        void UpdateAliasMeta(AliasRecord alias, AliasesMetaCreatedOptions options, string district, string province);
        Dictionary<string, string> PropertyGetSeoMeta(string path);
        void ClearCacheMeta(string path);

        #endregion

        #region Statistic

        int CountPropertyByType(string adsTypeCssClass, string typeCssClass);
        int CountPropertyByAdsType(string adsTypeCssClass, string typeGroupCssClass);
        int CountPropertyWidgetByAdsType(string typeWidget, string adsTypeCssClass);
        int CountRequirementByAdsType(string adsTypeCssClass);

        #endregion

        #region Customer

        IContentQuery<CustomerPart, CustomerPartRecord> GetCustomers();

        IContentQuery<CustomerPart, CustomerPartRecord> SearchCustomers(PropertyDisplayIndexOptions options);
        IContentQuery<CustomerPart, CustomerPartRecord> SearchCustomersRequirementTheSame(CustomerPartRecord customer);

        CustomerDetailViewModel BuildDetailViewModel(CustomerPart c);
        CustomerDetailViewModel BuildPropertyExchangeDetailViewModel(PropertyPart p);

        bool IsViewable(CustomerPart c);

        #endregion

        #region InitWidget

        PropertyDisplayIndexOptions InitFilterWidget(PropertyDisplayIndexOptions options);
        LocationApartmentDisplayOptions InitFilterApartmentWidget(LocationApartmentDisplayOptions options);
        LocationApartmentIndexDisplayViewModel InitApartmentHighlightsWidget(LocationApartmentDisplayOptions options);
        LocationApartmentDisplayOptions InitCompareApartment(LocationApartmentDisplayOptions options);
        EstimateWidgetViewModel InitEstimateWidget(EstimateWidgetViewModel model);
        PlanningMapIndexOptions InitMapPlanningWidget(PlanningMapIndexOptions model);
        AliasesMetaCreatedOptions InitAliasCreate(AliasesMetaCreatedOptions options);

        //Get userlocaition in realestate detail
        PropertyDisplayEntry InitUserLocationWidget(PropertyDisplayEntry options);

        #endregion

        #region Properties

        // Properties by AdsType
        IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesByAdsType(string adsTypeCssClass);
        IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesNewestByAdsType(string adsTypeCssClass);
        IContentQuery<PropertyPart, PropertyPartRecord> GetIsAuctionPropertiesByAdsType(string adsTypeCssClass);

        // Highlight Properties
        IContentQuery<PropertyPart, PropertyPartRecord> GetHighlightProperties();
        IContentQuery<PropertyPart, PropertyPartRecord> GetHighlightProperties(string adsTypeCssClass);

        IContentQuery<PropertyPart, PropertyPartRecord> SearchPropertiesByAds(string adsTypeCssClass, PropertyDisplayIndexOptions options);

        // Properties
        IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(PropertyDisplayIndexOptions options);

        // Get all Properties from customer's requirements
        IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(CustomerPart customer);

        IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> SearchApartment(LocationApartmentDisplayOptions options);

        bool IsEditable(int id);
        bool IsViewable(PropertyPart p);

        #endregion

        #region Build Model

        PropertyFrontEndCreateBaseViewModel BuildCreateBaseViewModel(string adsTypeCssClass);
        PropertyFrontEndCreateBaseViewModel BuildCreateBaseViewModel(PropertyPart p);
        PropertyFrontEndEditViewModel BuildEditViewModel(PropertyPart p);

        #endregion

    }

    public class FastFilterService : IFastFilterService
    {
        #region Init

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours
        private readonly IAddressService _addressService;
        private readonly IRepository<AliasRecord> _aliasRepository;
        private readonly IAliasService _aliasService;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly ICustomerService _customerService;
        private readonly IUserGroupService _groupService;
        private readonly IMembershipService _membershipService;
        private readonly IRepository<PlanningMapRecord> _planningMapRepository;
        private readonly IPropertyService _propertyService;
        private readonly RequestContext _requestContext;
        private readonly ISignals _signals;
        private readonly IHostNameService _hostNameService;
        private readonly IFacebookApiService _facebookApiSevice;
        private readonly IAdsPaymentHistoryService _adsPaymentService;
        private readonly IPropertyExchangeService _propertyExchangeService;

        public FastFilterService(
            IAddressService addressService,
            ISiteService siteService,
            IUserGroupService groupService,
            IPropertyService propertyService,
            ICustomerService customerService,
            IPropertySettingService settingService,
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            ICacheManager cacheManager,
            IOrchardServices services,
            RequestContext requestContext,
            IRepository<PlanningMapRecord> planningMapRepository,
            IUserPersonalService userRealEstateService,
            IRepository<AliasRecord> aliasRepository,
            IAliasService aliasService,
            IFacebookApiService facebookApiSevice,
            IHostNameService hostNameService,
            IClock clock,
            ISignals signals,
            IAdsPaymentHistoryService adsPaymentService,
            IPropertyExchangeService propertyExchangeService)
        {
            _addressService = addressService;
            _groupService = groupService;
            _propertyService = propertyService;
            _membershipService = membershipService;
            _customerService = customerService;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _planningMapRepository = planningMapRepository;
            _clock = clock;
            _signals = signals;
            _requestContext = requestContext;
            _aliasRepository = aliasRepository;
            _aliasService = aliasService;
            _hostNameService = hostNameService;
            _facebookApiSevice = facebookApiSevice;
            _adsPaymentService = adsPaymentService;
            _propertyExchangeService = propertyExchangeService;

            Shape = shapeFactory;
            Logger = NullLogger.Instance;
            Services = services;
            T = NullLocalizer.Instance;
        }

        private dynamic Shape { get; set; }
        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion

        #region InitWidget

        public PropertyDisplayIndexOptions InitFilterWidget(PropertyDisplayIndexOptions options)
        {
            var routeData = _requestContext.RouteData.Values;
            var requestData = _requestContext.HttpContext.Request;

            #region DEFAULT OPTIONS

            // default options
            if (options == null)
            {
                options = new PropertyDisplayIndexOptions { ProvinceId = 0, LocationId = 0 };
            }

            #endregion

            #region Provinces, Districts, Wards, Streets

            // Provinces
            options.Provinces = _addressService.GetProvinces();
            options.ListProvinces = _addressService.GetSelectListProvinces();

            // ProvinceId
            if (routeData["ProvinceId"] != null)
                options.ProvinceId = int.Parse(routeData["ProvinceId"].ToString());
            else if (!String.IsNullOrEmpty(requestData["ProvinceId"]))
                options.ProvinceId = int.Parse(requestData["ProvinceId"]);

            // Districts
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            options.ListDistricts = _addressService.GetSelectListDistricts(options.ProvinceId);

            // DistrictIds
            if (routeData["DistrictIds"] != null)
                options.DistrictIds = routeData["DistrictIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["DistrictIds"]))
                options.DistrictIds = requestData["DistrictIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            // Wards
            if (options.DistrictIds != null && options.DistrictIds.Count() > 0)
                options.Wards = _addressService.GetWards(options.DistrictIds);
            else
                options.Wards = _addressService.GetWards(0);

            // WardIds
            if (routeData["WardIds"] != null)
                options.WardIds = routeData["WardIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["WardIds"]))
                options.WardIds = requestData["WardIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            // Streets
            if (options.DistrictIds != null && options.DistrictIds.Count() > 0)
                options.Streets = _addressService.GetStreets(options.DistrictIds);
            else
                options.Streets = _addressService.GetStreetsByProvince(0);

            // StreetIds
            if (routeData["StreetIds"] != null)
                options.StreetIds = routeData["StreetIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["StreetIds"]))
                options.StreetIds = requestData["StreetIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            #endregion

            #region AdsTypes

            var adsTypeCssClass = new List<string> { "ad-buying", "ad-renting" };//Cần mua , Cần thuê

            // AdsTypes
            options.AdsTypes = _propertyService.GetAdsTypes().Where(r => !adsTypeCssClass.Contains(r.CssClass));

            // AdsTypeCssClass
            if (routeData["AdsTypeCssClass"] != null)
                options.AdsTypeCssClass = routeData["AdsTypeCssClass"].ToString();
            else if (!String.IsNullOrEmpty(requestData["AdsTypeCssClass"]))
                options.AdsTypeCssClass = requestData["AdsTypeCssClass"];

            #endregion

            #region TypeGroups

            // TypeGroups
            options.TypeGroups = _propertyService.GetTypeGroups();

            // TypeGroupCssClass
            if (routeData["TypeGroupCssClass"] != null)
                options.TypeGroupCssClass = routeData["TypeGroupCssClass"].ToString();
            else if (!String.IsNullOrEmpty(requestData["TypeGroupCssClass"]))
                options.TypeGroupCssClass = requestData["TypeGroupCssClass"];

            #endregion

            #region Types

            // Types
            options.Types = _propertyService.GetTypes();

            // TypeIds
            if (routeData["TypeIds"] != null)
                options.TypeIds = routeData["TypeIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["TypeIds"]))
                options.TypeIds = requestData["TypeIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            #endregion

            #region Directions

            // Directions
            options.Directions = _propertyService.GetDirections();

            // DirectionIds
            if (routeData["DirectionIds"] != null)
                options.DirectionIds = routeData["DirectionIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["DirectionIds"]))
                options.DirectionIds = requestData["DirectionIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            #endregion

            // Apartments
            if (options.DistrictIds != null && options.DistrictIds.Count() > 0)
                options.Apartments = _addressService.GetApartments(options.DistrictIds);
            else
                options.Apartments = _addressService.GetApartments(0);

            // ApartmentIds
            if (routeData["ApartmentIds"] != null)
                options.ApartmentIds = routeData["ApartmentIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["ApartmentIds"]))
                options.ApartmentIds = requestData["ApartmentIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            // OtherProjectName
            if (routeData["OtherProjectName"] != null)
                options.OtherProjectName = routeData["OtherProjectName"].ToString();
            else if (!String.IsNullOrEmpty(requestData["OtherProjectName"]))
                options.OtherProjectName = requestData["OtherProjectName"];

            // ApartmentFloorThRange
            //if (routeData["ApartmentFloorThRange"] != null) options.ApartmentFloorThRange = routeData["ApartmentFloorThRange"].ToString();
            //else if (!String.IsNullOrEmpty(requestData["ApartmentFloorThRange"])) options.ApartmentFloorThRange = requestData["ApartmentFloorThRange"];

            // AlleyTurnsRange
            //if (routeData["AlleyTurnsRange"] != null) options.AlleyTurnsRange = routeData["AlleyTurnsRange"].ToString();
            //else if (!String.IsNullOrEmpty(requestData["AlleyTurnsRange"])) options.AlleyTurnsRange = requestData["AlleyTurnsRange"];

            // MinFloors
            if (routeData["MinFloors"] != null) options.MinFloors = double.Parse(routeData["MinFloors"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinFloors"]))
                options.MinFloors = double.Parse(requestData["MinFloors"]);

            // MinBedrooms
            if (routeData["MinBedrooms"] != null) options.MinBedrooms = int.Parse(routeData["MinBedrooms"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinBedrooms"]))
                options.MinBedrooms = int.Parse(requestData["MinBedrooms"]);

            #region Area

            // MinAreaTotal
            if (routeData["MinAreaTotal"] != null)
                options.MinAreaTotal = double.Parse(routeData["MinAreaTotal"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinAreaTotal"]))
                options.MinAreaTotal = double.Parse(requestData["MinAreaTotal"]);

            // MinAreaTotalWidth
            if (routeData["MinAreaTotalWidth"] != null)
                options.MinAreaTotalWidth = double.Parse(routeData["MinAreaTotalWidth"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinAreaTotalWidth"]))
                options.MinAreaTotalWidth = double.Parse(requestData["MinAreaTotalWidth"]);

            // MinAreaTotalLength
            if (routeData["MinAreaTotalLength"] != null)
                options.MinAreaTotalLength = double.Parse(routeData["MinAreaTotalLength"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinAreaTotalLength"]))
                options.MinAreaTotalLength = double.Parse(requestData["MinAreaTotalLength"]);

            // MinAreaUsable
            if (routeData["MinAreaUsable"] != null)
                options.MinAreaUsable = double.Parse(routeData["MinAreaUsable"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinAreaUsable"]))
                options.MinAreaUsable = double.Parse(requestData["MinAreaUsable"]);

            #endregion

            #region PriceProposed

            options.ListPrice = BuildPriceData();

            // MinPriceProposed
            if (routeData["MinPriceProposed"] != null)
                options.MinPriceProposed = double.Parse(routeData["MinPriceProposed"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MinPriceProposed"]))
                options.MinPriceProposed = double.Parse(requestData["MinPriceProposed"]);

            // MaxPriceProposed
            if (routeData["MaxPriceProposed"] != null)
                options.MaxPriceProposed = double.Parse(routeData["MaxPriceProposed"].ToString());
            else if (!String.IsNullOrEmpty(requestData["MaxPriceProposed"]))
                options.MaxPriceProposed = double.Parse(requestData["MaxPriceProposed"]);

            // PaymentMethod
            options.PaymentMethods = _propertyService.GetPaymentMethods();

            // PaymentMethodCssClass
            if (routeData["PaymentMethodCssClass"] != null)
                options.PaymentMethodCssClass = routeData["PaymentMethodCssClass"].ToString();
            else if (!String.IsNullOrEmpty(requestData["PaymentMethodCssClass"]))
                options.PaymentMethodCssClass = requestData["PaymentMethodCssClass"];

            if (String.IsNullOrEmpty(options.PaymentMethodCssClass))
            {
                options.PaymentMethodCssClass = options.AdsTypeCssClass == "ad-leasing" ? "pm-vnd-m" : "pm-vnd-b";
            }

            #endregion

            return options;
        }

        public LocationApartmentDisplayOptions InitFilterApartmentWidget(LocationApartmentDisplayOptions options)
        {
            RouteValueDictionary routeData = _requestContext.RouteData.Values;
            HttpRequestBase requestData = _requestContext.HttpContext.Request;

            #region DEFAULT OPTIONS

            // default options
            if (options == null)
            {
                options = new LocationApartmentDisplayOptions { ApartmentProvinceId = 0 };
            }

            #endregion

            // Provinces
            options.Provinces = _addressService.GetProvinces();

            // ProvinceId
            if (routeData["ApartmentProvinceId"] != null)
                options.ApartmentProvinceId = int.Parse(routeData["ApartmentProvinceId"].ToString());
            else if (!String.IsNullOrEmpty(requestData["ApartmentProvinceId"]))
                options.ApartmentProvinceId = int.Parse(requestData["ApartmentProvinceId"]);

            // Districts
            options.Districts = _addressService.GetDistricts(options.ApartmentProvinceId);

            // DistrictIds
            if (routeData["ApartmentDistrictIds"] != null)
                options.ApartmentDistrictIds =
                    routeData["ApartmentDistrictIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["ApartmentDistrictIds"]))
                options.ApartmentDistrictIds =
                    requestData["ApartmentDistrictIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            // Apartments
            if (options.ApartmentDistrictIds != null && options.ApartmentDistrictIds.Count() > 0)
                options.Apartments = _addressService.GetApartments(options.ApartmentDistrictIds);
            else
                options.Apartments = _addressService.GetApartments(0);

            // ApartmentIds
            if (routeData["ApartmentApartmentIds"] != null)
                options.ApartmentApartmentIds =
                    routeData["ApartmentApartmentIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["ApartmentApartmentIds"]))
                options.ApartmentApartmentIds =
                    requestData["ApartmentApartmentIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            return options;
        }

        public LocationApartmentIndexDisplayViewModel InitApartmentHighlightsWidget(LocationApartmentDisplayOptions options)
        {
            IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> aList = SearchApartment(options);

            DateTime startBuildModel = DateTime.Now;
            var model = new LocationApartmentIndexDisplayViewModel
            {
                LocationApartments = aList.List().OrderBy(r => Guid.NewGuid()).Select(r => new LocationApartmentDisplayEntry
                {
                    LocationApartment = r,
                    DisplayUrl = r.Name.ToSlug(),
                    DefaultImgUrl = _propertyService.GetDefaultImageUrl(r)
                }).Take(9),
            };

            return model;
        }

        public LocationApartmentDisplayOptions InitCompareApartment(LocationApartmentDisplayOptions options)
        {
            RouteValueDictionary routeData = _requestContext.RouteData.Values;
            HttpRequestBase requestData = _requestContext.HttpContext.Request;

            #region DEFAULT OPTIONS

            // default options
            if (options == null)
            {
                options = new LocationApartmentDisplayOptions { ApartmentProvinceId = 0, WithApartmentProvinceId = 0 };
            }

            #endregion

            // Provinces
            List<LocationProvincePart> provinces = _addressService.GetProvinces().ToList();
            options.Provinces = provinces;

            // AparmentApartmentId
            if (routeData["AparmentApartmentId"] != null)
                options.ApartmentApartmentId = int.Parse(routeData["AparmentApartmentId"].ToString());
            if (!String.IsNullOrEmpty(requestData["AparmentApartmentId"]))
                options.ApartmentApartmentId = int.Parse(requestData["AparmentApartmentId"]);

            // WidthAparmentApartmentId
            if (routeData["WithApartmentApartmentId"] != null)
                options.WithApartmentApartmentId = int.Parse(routeData["WithApartmentApartmentId"].ToString());
            if (!String.IsNullOrEmpty(requestData["WithApartmentApartmentId"]))
                options.WithApartmentApartmentId = int.Parse(requestData["WithApartmentApartmentId"]);

            if (options.ApartmentApartmentId.HasValue)
            {
                var p1 = Services.ContentManager.Get<LocationApartmentPart>(options.ApartmentApartmentId.Value);
                options.ApartmentProvinceId = p1.Province.Id;
                options.Districts = _addressService.GetDistricts(p1.Province.Id);
                options.ApartmentDistrictId = p1.District.Id;
                options.Apartments = _addressService.GetApartments(options.ApartmentDistrictId);
            }
            else
            {
                options.Districts = _addressService.GetDistricts(options.ApartmentProvinceId);
                options.Apartments = _addressService.GetApartments(0);
            }

            if (options.WithApartmentApartmentId.HasValue)
            {
                var p2 = Services.ContentManager.Get<LocationApartmentPart>(options.WithApartmentApartmentId.Value);
                options.WithApartmentProvinceId = p2.Province.Id;
                options.WithDistricts = _addressService.GetDistricts(p2.Province.Id);
                options.WithApartmentDistrictId = p2.District.Id;
                options.WithApartments = _addressService.GetApartments(options.WithApartmentDistrictId);
            }
            else
            {
                options.WithDistricts = _addressService.GetDistricts(options.WithApartmentProvinceId);
                options.WithApartments = _addressService.GetApartments(0);
            }

            return options;
        }

        public EstimateWidgetViewModel InitEstimateWidget(EstimateWidgetViewModel options)
        {
            if (options.ProvinceId == null || options.ProvinceId == 0)
                options.ProvinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;
            options.Provinces = _addressService.GetProvincesForEstimate();
            options.Districts = _addressService.GetDistrictsForEstimate();
            options.Wards = new List<LocationWardPart>();
            options.Streets = new List<LocationStreetPart>();
            return options;
        }

        public PlanningMapIndexOptions InitMapPlanningWidget(PlanningMapIndexOptions options)
        {
            if (options == null)
            {
                options = new PlanningMapIndexOptions { ProvinceId = 0, DistrictId = 0, WardId = 0 };
            }

            List<int> provinceIds =
                _planningMapRepository.Table.Select(a => a.LocationProvincePartRecord.Id).Distinct().ToList();
            options.Provinces = _addressService.GetProvinces().Where(a => provinceIds.Contains(a.Id));

            if (options.ProvinceId == null || options.ProvinceId == 0)
                options.ProvinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            List<int> districtIds =
                _planningMapRepository.Fetch(a => a.LocationProvincePartRecord.Id == options.ProvinceId)
                    .Select(a => a.LocationDistrictPartRecord.Id)
                    .Distinct()
                    .ToList();
            options.Districts = _addressService.GetDistricts().Where(a => districtIds.Contains(a.Id));

            List<int> wardIds =
                _planningMapRepository.Fetch(a => a.LocationDistrictPartRecord.Id == options.DistrictId)
                    .Select(a => a.LocationWardPartRecord.Id)
                    .Distinct()
                    .ToList();
            options.Wards = _addressService.GetWards().Where(a => wardIds.Contains(a.Id)).List();

            return options;
        }

        #endregion

        #region Properties

        // Properties by AdsType
        public IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesByAdsType(string adsTypeCssClass)
        {
            return
                _propertyService.GetProperties()
                    .Where(p => p.AdsType != null && p.AdsType == _propertyService.GetAdsType(adsTypeCssClass));
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesNewestByAdsType(string adsTypeCssClass)
        {
            return
                _propertyService.GetPropertiesNewest()
                    .Where(p => p.AdsType != null && p.AdsType == _propertyService.GetAdsType(adsTypeCssClass));
        }

        // Highlight Properties
        public IContentQuery<PropertyPart, PropertyPartRecord> GetHighlightProperties()
        {
            return
                _propertyService.GetProperties()
                    .Where(p => p.AdsHighlight && p.AdsHighlightExpirationDate > DateTime.Now);
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetHighlightProperties(string adsTypeCssClass)
        {
            return
                GetPropertiesByAdsType(adsTypeCssClass)
                    .Where(p => p.AdsHighlight && p.AdsHighlightExpirationDate > DateTime.Now);
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetIsAuctionPropertiesByAdsType(string adsTypeCssClass)
        {
            return
               _propertyService.GetIsAuctionProperties()
                   .Where(p => p.AdsType != null && p.AdsType == _propertyService.GetAdsType(adsTypeCssClass));
        }
        public IContentQuery<PropertyPart, PropertyPartRecord> SearchPropertiesByAds(string adsTypeCssClass,
            PropertyDisplayIndexOptions options)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> pList;
            List<int> lstIdVip;
            List<int> lstIdHighight;

            switch (adsTypeCssClass)
            {
                case "vip":
                    pList =
                        _propertyService.GetProperties().Where(r => r.AdsVIP && r.AdsVIPExpirationDate > DateTime.Now);
                    break;
                case "highlight":
                    lstIdVip =
                        _propertyService.GetProperties()
                            .Where(r => r.AdsVIP && r.AdsVIPExpirationDate > DateTime.Now)
                            .List()
                            .Select(r => r.Id)
                            .ToList();
                    pList =
                        _propertyService.GetProperties()
                            .Where(
                                r =>
                                    r.AdsHighlight && r.AdsHighlightExpirationDate > DateTime.Now &&
                                    !lstIdVip.Contains(r.Id));
                    break;
                case "gooddeal":
                    lstIdVip =
                        _propertyService.GetProperties()
                            .Where(r => r.AdsVIP && r.AdsVIPExpirationDate > DateTime.Now)
                            .List()
                            .Select(r => r.Id)
                            .ToList();
                    lstIdHighight =
                        _propertyService.GetProperties()
                            .Where(
                                r =>
                                    r.AdsHighlight && r.AdsHighlightExpirationDate > DateTime.Now &&
                                    !lstIdVip.Contains(r.Id))
                            .List()
                            .Select(r => r.Id)
                            .ToList();
                    pList =
                        _propertyService.GetProperties()
                            .Where(
                                r =>
                                    r.AdsGoodDeal && r.AdsGoodDealExpirationDate > DateTime.Now &&
                                    !lstIdVip.Contains(r.Id) && !lstIdHighight.Contains(r.Id));
                    break;
                default:
                    lstIdVip =
                        _propertyService.GetProperties()
                            .Where(r => r.AdsVIP && r.AdsVIPExpirationDate > DateTime.Now)
                            .List()
                            .Select(r => r.Id)
                            .ToList();
                    lstIdHighight =
                        _propertyService.GetProperties()
                            .Where(
                                r =>
                                    r.AdsHighlight && r.AdsHighlightExpirationDate > DateTime.Now &&
                                    !lstIdVip.Contains(r.Id))
                            .List()
                            .Select(r => r.Id)
                            .ToList();
                    List<int> lstIdGoodDeal = _propertyService.GetProperties()
                        .Where(
                            r =>
                                r.AdsGoodDeal && r.AdsGoodDealExpirationDate > DateTime.Now &&
                                !lstIdVip.Contains(r.Id) && !lstIdHighight.Contains(r.Id))
                        .List()
                        .Select(r => r.Id)
                        .ToList();
                    pList =
                        _propertyService.GetProperties()
                            .Where(
                                r =>
                                    !lstIdVip.Contains(r.Id) && !lstIdHighight.Contains(r.Id) &&
                                    !lstIdGoodDeal.Contains(r.Id));
                    break;
            }

            #region FILTER

            #region AdsType

            if (!String.IsNullOrEmpty(options.AdsTypeCssClass))
                pList = pList.Where(p => p.AdsType == _propertyService.GetAdsType(options.AdsTypeCssClass));

            #endregion

            #region TypeGroup

            if (!String.IsNullOrEmpty(options.TypeGroupCssClass))
            {
                if (options.TypeGroupCssClass == "gp-land")
                {
                    // Lấy cả Đất chưa xây dựng trong Đất ở và các loại nhà
                    pList =
                        pList.Where(
                            a =>
                                a.TypeGroup == _propertyService.GetTypeGroup(options.TypeGroupCssClass) ||
                                a.Type == _propertyService.GetType("tp-residential-land"));
                }
                else
                {
                    pList = pList.Where(a => a.TypeGroup == _propertyService.GetTypeGroup(options.TypeGroupCssClass));
                }
            }

            #endregion

            #region Address

            // Province
            if (options.ProvinceId > 0)
            {
                pList = pList.Where(p => p.Province.Id == options.ProvinceId);
            }

            // Districts
            if (options.DistrictIds != null && options.DistrictIds.Count() > 0)
            {
                pList = pList.Where(p => options.DistrictIds.Contains(p.District.Id));
            }

            // Wards
            if (options.WardIds != null && options.WardIds.Count() > 0)
            {
                pList = pList.Where(p => options.WardIds.Contains(p.Ward.Id));
            }

            // Streets
            if (options.StreetIds != null && options.StreetIds.Count() > 0)
            {
                // Lấy tất cả BĐS trên Đường và Đoạn Đường
                List<int> listRelatedStreetIds =
                    _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                        .Where(a => options.StreetIds.Contains(a.RelatedStreet.Id))
                        .List()
                        .Select(a => a.Id)
                        .ToList();
                listRelatedStreetIds.AddRange(options.StreetIds);
                pList = pList.Where(p => listRelatedStreetIds.Contains(p.Street.Id));
            }

            #endregion

            // Directions
            if (options.DirectionIds != null)
                if (options.DirectionIds.Count() > 0)
                    pList = pList.Where(p => options.DirectionIds.Contains(p.Direction.Id) || p.Direction == null);

            #region Location & AlleyWidth

            // Location
            if (options.LocationId > 0) pList = pList.Where(p => p.Location.Id == options.LocationId);

            #region AlleyWidth

            int locationFront = _propertyService.GetLocation("h-front").Id;
            int locationAlley = _propertyService.GetLocation("h-alley").Id;

            //options.AlleyTurnsRange = options.AlleyTurnsRange == PropertyDisplayLocation.None ? PropertyDisplayLocation.All : options.AlleyTurnsRange;

            switch (options.AlleyTurnsRange)
            {
                case PropertyDisplayLocation.All: // Tat ca cac vi tri
                    pList = pList.Where(p => p.Location.Id == locationFront || p.Location.Id == locationAlley);
                    break;
                case PropertyDisplayLocation.AllWalk: // Mat Tien
                    pList = pList.Where(p => p.Location.Id == locationFront);
                    break;
                case PropertyDisplayLocation.Alley6: // hem 6m tro len
                    pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 6);
                    break;
                case PropertyDisplayLocation.Alley5:
                    pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 5);
                    break;
                case PropertyDisplayLocation.Alley4:
                    pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 4);
                    break;
                case PropertyDisplayLocation.Alley3:
                    pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 3);
                    break;
                case PropertyDisplayLocation.Alley2:
                    pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 2);
                    break;
                case PropertyDisplayLocation.Alley:
                    pList = pList.Where(p => p.Location.Id == locationAlley);
                    break;
            }

            #endregion

            #endregion

            // Area & Floor
            if (options.MinAreaTotal > 0)
                pList = pList.Where(p => p.AreaTotal >= options.MinAreaTotal || p.Area >= options.MinAreaTotal);
            if (options.MinAreaUsable > 0)
                pList = pList.Where(p => p.AreaUsable >= options.MinAreaUsable || p.Area >= options.MinAreaUsable);
            if (options.MinAreaTotalWidth > 0) pList = pList.Where(p => p.AreaTotalWidth >= options.MinAreaTotalWidth);
            if (options.MinAreaTotalLength > 0)
                pList = pList.Where(p => p.AreaTotalLength >= options.MinAreaTotalLength);
            if (options.MinFloors > 0) pList = pList.Where(p => p.Floors >= options.MinFloors);
            if (options.MinBedrooms > 0) pList = pList.Where(p => p.Bedrooms >= options.MinBedrooms);

            #region Price

            // Convert Price to VND

            double minPriceVnd = options.MinPriceProposed.HasValue
                ? _propertyService.ConvertToVndB(options.MinPriceProposed ?? 0, options.PaymentMethodCssClass)
                : 0;
            double maxPriceVnd = options.MaxPriceProposed.HasValue
                ? _propertyService.ConvertToVndB(options.MaxPriceProposed ?? 0, options.PaymentMethodCssClass)
                : 0;

            if (minPriceVnd > 0) pList = pList.Where(p => p.PriceProposedInVND >= minPriceVnd);
            if (maxPriceVnd > 0) pList = pList.Where(p => p.PriceProposedInVND <= maxPriceVnd);

            #endregion

            #region Apartment

            // OtherProjectName
            if (!String.IsNullOrEmpty(options.OtherProjectName))
                pList = pList.Where(p => p.OtherProjectName.Contains(options.OtherProjectName));

            // ApartmentFloorThRange
            switch (options.ApartmentFloorThRange)
            {
                case PropertyDisplayApartmentFloorTh.All:
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                    pList = pList.Where(p => p.ApartmentFloorTh >= 1 && p.ApartmentFloorTh <= 3);
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                    pList = pList.Where(p => p.ApartmentFloorTh >= 4 && p.ApartmentFloorTh <= 7);
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                    pList = pList.Where(p => p.ApartmentFloorTh >= 8 && p.ApartmentFloorTh <= 12);
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                    pList = pList.Where(p => p.ApartmentFloorTh >= 12);
                    break;
            }

            #endregion

            #region CHÍNH CHỦ, PHÁT MÃI

            if (options.AdsNormal)
            {
                options.TitleArticle = "Tin mới đăng";
            }

            if (options.IsOwner)
            {
                // BĐS Chính chủ
                pList = pList.Where(p => p.PublishAddress || p.IsOwner);
            }
            if (options.IsAuction)
            {
                // Tìm BĐS phát mãi
                pList = pList.Where(p => p.IsAuction);
            }

            if (string.IsNullOrEmpty(options.TitleArticle))
            {
                options.TitleArticle = "Kết quả tìm kiếm";
            }
            if (options.Create_Id.HasValue)
            {
                pList = pList.Where(p => p.CreatedUser.Id == options.Create_Id.Value);
            }

            #endregion

            if (!string.IsNullOrEmpty(options.SearchPhone))
            {
                pList = pList.Where(p => p.ContactPhone.Contains(options.SearchPhone)
                                         || p.ContactPhoneToDisplay.Contains(options.SearchPhone));
            }

            #endregion

            return pList;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(PropertyDisplayIndexOptions options)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> pList;

            if (options.PropertyId.HasValue)
            {
                pList = GetProperties(options.PropertyId);
            }
            else
            {
                if (options.PropertyExchange)
                    pList = _propertyService.GetListPropertyExchangeQueryFrontEnd();
                else if (options.IsAuction)
                {
                    pList = _propertyService.GetIsAuctionProperties();
                    var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

                    if (currentDomainGroup != null && currentDomainGroup.Id != 847241)//847241(nhabandaugia.com) lấy tất cả BĐS phát mãi 
                        pList = pList.Where(p => p.UserGroup == currentDomainGroup);
                }
                else
                    pList = _propertyService.GetProperties();

                #region FILTER

                #region AdsType, Type

                if (!String.IsNullOrEmpty(options.AdsTypeCssClass))
                    pList = pList.Where(p => p.AdsType == _propertyService.GetAdsType(options.AdsTypeCssClass));

                if (options.TypeIds != null && options.TypeIds.Any())
                    pList = pList.Where(p => options.TypeIds.Contains(p.Type.Id));

                if (options.TypeId.HasValue) pList = pList.Where(r => r.Type.Id == options.TypeId);

                #endregion

                #region TypeGroup

                if (!String.IsNullOrEmpty(options.TypeGroupCssClass))
                {
                    switch (options.TypeGroupCssClass)
                    {
                        case "gp-house-land":
                            pList =
                            pList.Where(
                                a =>
                                    (a.TypeGroup == _propertyService.GetTypeGroup("gp-land") ||
                                     a.TypeGroup == _propertyService.GetTypeGroup("gp-house")) ||
                                    a.Type == _propertyService.GetType("tp-residential-land"));
                            break;
                        case "gp-land":
                            // Lấy cả Đất chưa xây dựng trong Đất ở và các loại nhà
                            pList =
                                pList.Where(
                                    a =>
                                        a.TypeGroup == _propertyService.GetTypeGroup(options.TypeGroupCssClass) ||
                                        a.Type == _propertyService.GetType("tp-residential-land"));
                            break;
                        default:
                            pList = pList.Where(a => a.TypeGroup == _propertyService.GetTypeGroup(options.TypeGroupCssClass));
                            break;

                    }
                }

                #endregion

                #region Address

                // Province
                if (options.ProvinceId > 0) pList = pList.Where(p => p.Province.Id == options.ProvinceId);

                // Districts
                if (options.DistrictIds != null && options.DistrictIds.Count() > 0) pList = pList.Where(p => options.DistrictIds.Contains(p.District.Id));

                // Wards
                if (options.WardIds != null && options.WardIds.Count() > 0) pList = pList.Where(p => options.WardIds.Contains(p.Ward.Id));

                // Streets
                if (options.StreetIds != null && options.StreetIds.Count() > 0)
                {
                    // Lấy tất cả BĐS trên Đường và Đoạn Đường
                    List<int> listRelatedStreetIds =
                        _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                            .Where(a => a.RelatedStreet != null && options.StreetIds.Contains(a.RelatedStreet.Id))
                            .List()
                            .Select(a => a.Id)
                            .ToList();
                    listRelatedStreetIds.AddRange(options.StreetIds);
                    pList = pList.Where(p => listRelatedStreetIds.Contains(p.Street.Id));
                }

                #endregion

                // Directions
                if (options.DirectionIds != null && options.DirectionIds.Count() > 0)
                    pList = pList.Where(p => options.DirectionIds.Contains(p.Direction.Id) || p.Direction == null);

                #region Location & AlleyWidth

                // Location
                if (options.LocationId > 0) pList = pList.Where(p => p.Location.Id == options.LocationId);

                #region AlleyWidth

                int locationFront = _propertyService.GetLocation("h-front").Id;
                int locationAlley = _propertyService.GetLocation("h-alley").Id;

                //options.AlleyTurnsRange = options.AlleyTurnsRange == PropertyDisplayLocation.None ? PropertyDisplayLocation.All : options.AlleyTurnsRange;

                switch (options.AlleyTurnsRange)
                {
                    case PropertyDisplayLocation.All: // Tat ca cac vi tri
                        pList = pList.Where(p => p.Location.Id == locationFront || p.Location.Id == locationAlley);
                        break;
                    case PropertyDisplayLocation.AllWalk: // Mat Tien
                        pList = pList.Where(p => p.Location.Id == locationFront);
                        break;
                    case PropertyDisplayLocation.Alley6: // hem 6m tro len
                        pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 6);
                        break;
                    case PropertyDisplayLocation.Alley5:
                        pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 5);
                        break;
                    case PropertyDisplayLocation.Alley4:
                        pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 4);
                        break;
                    case PropertyDisplayLocation.Alley3:
                        pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 3);
                        break;
                    case PropertyDisplayLocation.Alley2:
                        pList = pList.Where(p => p.Location.Id == locationAlley && p.AlleyWidth >= 2);
                        break;
                    case PropertyDisplayLocation.Alley:
                        pList = pList.Where(p => p.Location.Id == locationAlley);
                        break;
                }

                #endregion

                #endregion

                // Area & Floor
                if (options.IsAll)
                {
                    if (options.MinAreaTotal > 0)
                        pList = pList.Where(p =>
                            ((p.TypeGroup != _propertyService.GetTypeGroup("gp-apartment") &&
                              (p.AreaTotal >= options.MinAreaTotal || p.Area >= options.MinAreaTotal))
                             ||
                             (p.TypeGroup == _propertyService.GetTypeGroup("gp-apartment") &&
                              (p.AreaUsable >= options.MinAreaTotal || p.Area >= options.MinAreaTotal))));
                }
                else
                {
                    // Area
                    if (options.TypeGroupCssClass != "gp-apartment")
                    {
                        if (options.MinAreaTotal > 0)
                            pList =
                                pList.Where(p => p.AreaTotal >= options.MinAreaTotal || p.Area >= options.MinAreaTotal);
                        if (options.MinAreaTotalWidth > 0)
                            pList = pList.Where(p => p.AreaTotalWidth >= options.MinAreaTotalWidth);
                        if (options.MinAreaTotalLength > 0)
                            pList = pList.Where(p => p.AreaTotalLength >= options.MinAreaTotalLength);
                    }

                    // AreaUsable
                    if (options.TypeGroupCssClass == "gp-apartment")
                    {
                        if (options.MinAreaUsable > 0) pList = pList.Where(p => p.AreaUsable >= options.MinAreaUsable);
                    }
                    if (options.MinFloors > 0) pList = pList.Where(p => p.Floors >= options.MinFloors);
                    if (options.MinBedrooms > 0) pList = pList.Where(p => p.Bedrooms >= options.MinBedrooms);
                }

                #region Price

                if (options.PriceDataId.HasValue && options.PriceDataId > 0)
                {
                    var priceData = BuildPriceData().Where(r => r.Id == options.PriceDataId.Value).FirstOrDefault();

                    if (priceData != null)
                    {
                        double minPriceDataVnd = _propertyService.ConvertToVndB(priceData.MinValue, "pm-vnd-b");
                        double maxPriceDataVnd = _propertyService.ConvertToVndB(priceData.MaxValue, "pm-vnd-b");


                        pList = pList.Where(p => p.PriceProposedInVND >= minPriceDataVnd);
                        pList = pList.Where(p => p.PriceProposedInVND <= maxPriceDataVnd);
                    }
                }
                else
                {
                    // Convert Price to VND

                    double minPriceVnd = options.MinPriceProposed.HasValue
                        ? _propertyService.ConvertToVndB((double)options.MinPriceProposed, options.PaymentMethodCssClass)
                        : 0;
                    double maxPriceVnd = options.MaxPriceProposed.HasValue
                        ? _propertyService.ConvertToVndB((double)options.MaxPriceProposed, options.PaymentMethodCssClass)
                        : 0;

                    if (minPriceVnd > 0) pList = pList.Where(p => p.PriceProposedInVND >= minPriceVnd);
                    if (maxPriceVnd > 0) pList = pList.Where(p => p.PriceProposedInVND <= maxPriceVnd);
                }


                #endregion

                #region Apartment

                if (options.ApartmentIds != null && options.ApartmentIds.Count() > 0) pList = pList.Where(p => options.ApartmentIds.Contains(p.Apartment.Id));

                // OtherProjectName
                if (!String.IsNullOrEmpty(options.OtherProjectName)) pList = pList.Where(p => p.OtherProjectName.Contains(options.OtherProjectName));

                // ApartmentFloorThRange
                switch (options.ApartmentFloorThRange)
                {
                    case PropertyDisplayApartmentFloorTh.All:
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 1 && p.ApartmentFloorTh <= 3);
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 4 && p.ApartmentFloorTh <= 7);
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 8 && p.ApartmentFloorTh <= 12);
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 12);
                        break;
                }

                #endregion

                #region BĐS GIÁ RẺ, GIAO DỊCH GẤP, NỔI BẬT, CHÍNH CHỦ, PHÁT MÃI

                if (options.AdsHighlight)
                {
                    options.TitleArticle = "Nhà đất nổi bật";
                    pList = pList.Where(p => p.AdsHighlight && p.AdsHighlightExpirationDate > DateTime.Now);
                }
                if (options.AdsGoodDeal)
                {
                    options.TitleArticle = "Nhà đất giá rẻ";
                    pList = pList
                        .Where(p =>
                            ((p.Flag.Id == 32 || p.Flag.Id == 33) && p.IsExcludeFromPriceEstimation != true) || // Nhà rẻ (deal-good) và Nhà rất rẻ (deal-very-good) nhưng không bị loại khỏi định giá
                            (p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now)); // Nhà quảng cáo BĐS giá rẻ
                }
                //if (options.AdsVIP)
                //{
                //    options.TitleArticle = "Nhà đất giao dịch gấp";
                //    pList = pList.Where(p => p.AdsVIP == true && p.AdsVIPExpirationDate >= DateTime.Now);
                //}
                if (options.AdsNormal)
                {
                    options.TitleArticle = "Tin mới đăng";
                }

                if (options.IsOwner)
                {
                    // BĐS Chính chủ
                    pList = pList.Where(p => p.PublishAddress || p.IsOwner);
                }
                if (options.IsAuction)
                {
                    // Tìm BĐS phát mãi
                    pList = pList.Where(p => p.IsAuction);
                }
                if (options.PropertyExchange)
                {
                    options.TitleArticle = "Trao đổi bất động sản";
                }

                if (string.IsNullOrEmpty(options.TitleArticle))
                {
                    options.TitleArticle = "Kết quả tìm kiếm";
                }
                if (options.Create_Id.HasValue)
                {
                    pList = pList.Where(p => p.CreatedUser.Id == options.Create_Id.Value);
                }

                #endregion

                if (!string.IsNullOrEmpty(options.SearchPhone))
                {
                    pList = pList.Where(p => p.ContactPhone.Contains(options.SearchPhone)
                                             || p.ContactPhoneToDisplay.Contains(options.SearchPhone));
                }

                #endregion
            }

            return pList;
        }

        // Get all Properties from customer's requirements
        public IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(CustomerPart customer)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> pList = _propertyService.GetProperties();

            #region Get all Properties from customer's requirements

            List<int?> groupIds = customer.Requirements.Where(r => r.IsEnabled).Select(r => r.GroupId).ToList();

            foreach (var groupId in groupIds)
            {
                List<CustomerRequirementRecord> reqs = customer.Requirements.Where(r => r.GroupId == groupId).ToList();
                CustomerRequirementRecord req = reqs.First();

                #region Build Options

                var options = new PropertyIndexOptions
                {
                    DistrictIds = reqs.Where(r => r.LocationDistrictPartRecord != null)
                        .Select(r => r.LocationDistrictPartRecord.Id)
                        .ToArray(),
                    WardIds =
                        reqs.Where(r => r.LocationWardPartRecord != null)
                            .Select(r => r.LocationWardPartRecord.Id)
                            .ToArray(),
                    StreetIds = reqs.Where(r => r.LocationStreetPartRecord != null)
                        .Select(r => r.LocationStreetPartRecord.Id)
                        .ToArray(),
                    DirectionIds =
                        reqs.Where(r => r.DirectionPartRecord != null).Select(r => r.DirectionPartRecord.Id).ToArray()
                };

                if (req.PropertyLocationPartRecord != null) options.LocationId = req.PropertyLocationPartRecord.Id;

                options.MinAlleyTurns = req.MinAlleyTurns;
                options.MaxAlleyTurns = req.MaxAlleyTurns;
                options.MinAlleyWidth = req.MinAlleyWidth;
                options.MaxAlleyWidth = req.MaxAlleyWidth;
                options.MinDistanceToStreet = req.MinDistanceToStreet;
                options.MaxDistanceToStreet = req.MaxDistanceToStreet;

                options.MinAreaTotal = req.MinArea;
                options.MaxAreaTotal = req.MaxArea;
                options.MinAreaTotalWidth = req.MinWidth;
                options.MaxAreaTotalWidth = req.MaxWidth;
                options.MinAreaTotalLength = req.MinLength;
                options.MaxAreaTotalLength = req.MaxLength;

                options.MinFloors = req.MinFloors;
                options.MaxFloors = req.MaxFloors;
                options.MinBedrooms = req.MinBedrooms;
                options.MaxBedrooms = req.MaxBedrooms;
                options.MinBathrooms = req.MinBathrooms;
                options.MaxBathrooms = req.MaxBathrooms;

                options.MinPriceProposed = req.MinPrice.HasValue
                    ? _propertyService.ConvertToVndB((double)req.MinPrice, req.PaymentMethodPartRecord.CssClass)
                    : 0;
                options.MaxPriceProposed = req.MaxPrice.HasValue
                    ? _propertyService.ConvertToVndB((double)req.MaxPrice, req.PaymentMethodPartRecord.CssClass)
                    : 0;

                #endregion

                #region Filter

                if (options.DistrictIds.Count() != 0) pList = pList.Where(p => options.DistrictIds.Contains(p.District.Id));

                if (options.WardIds.Count() != 0) pList = pList.Where(p => options.WardIds.Contains(p.Ward.Id));

                if (options.StreetIds.Count() != 0) pList = pList.Where(p => options.StreetIds.Contains(p.Street.Id));

                if (options.DirectionIds.Count() != 0) pList = pList.Where(p => options.DirectionIds.Contains(p.Direction.Id));

                if (options.LocationId.HasValue) pList = pList.Where(p => p.Location.Id == options.LocationId);

                if (options.MinAlleyTurns.HasValue) pList = pList.Where(p => p.AlleyTurns >= options.MinAlleyTurns);

                if (options.MinAreaTotal.HasValue) pList = pList.Where(p => p.AreaTotal >= options.MinAreaTotal || p.Area >= options.MinAreaTotal);

                if (options.MaxAreaTotal.HasValue) pList = pList.Where(p => p.AreaTotal <= options.MaxAreaTotal || p.Area <= options.MaxAreaTotal);

                if (options.MinAreaTotalWidth.HasValue) pList = pList.Where(p => p.AreaTotalWidth >= options.MinAreaTotalWidth);

                if (options.MaxAreaTotalWidth.HasValue) pList = pList.Where(p => p.AreaTotalWidth <= options.MaxAreaTotalWidth);

                if (options.MinAreaTotalLength.HasValue) pList = pList.Where(p => p.AreaTotalLength >= options.MinAreaTotalLength);

                if (options.MaxAreaTotalLength.HasValue) pList = pList.Where(p => p.AreaTotalLength <= options.MaxAreaTotalLength);

                if (options.MinFloors.HasValue) pList = pList.Where(p => p.Floors >= options.MinFloors);

                if (options.MaxFloors.HasValue) pList = pList.Where(p => p.Floors <= options.MaxFloors);

                if (options.MinBedrooms.HasValue) pList = pList.Where(p => p.Bedrooms >= options.MinBedrooms);

                if (options.MaxBedrooms.HasValue) pList = pList.Where(p => p.Bedrooms <= options.MaxBedrooms);

                if (options.MinBathrooms.HasValue) pList = pList.Where(p => p.Bathrooms >= options.MinBathrooms);

                if (options.MaxBathrooms.HasValue) pList = pList.Where(p => p.Bathrooms <= options.MaxBathrooms);

                if (options.MinPriceProposed != null && options.MinPriceProposed != 0) pList = pList.Where(p => p.PriceProposed >= options.MinPriceProposed);

                if (options.MaxPriceProposed != null && options.MaxPriceProposed != 0) pList = pList.Where(p => p.PriceProposed <= options.MaxPriceProposed);

                #endregion

                break;
            }

            #endregion

            return pList;
        }

        public IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> SearchApartment(
            LocationApartmentDisplayOptions options)
        {
            IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> aList =
                _contentManager.Query<LocationApartmentPart, LocationApartmentPartRecord>();

            if (options.ApartmentProvinceId.HasValue)
                aList = aList.Where(r => r.Province != null && r.Province.Id == options.ApartmentProvinceId.Value);

            if (options.ApartmentDistrictIds != null && options.ApartmentDistrictIds.Count() > 0)
                aList = aList.Where(r => r.District != null && options.ApartmentDistrictIds.Contains(r.District.Id));

            if (options.ApartmentApartmentIds != null && options.ApartmentApartmentIds.Count() > 0)
                aList = aList.Where(r => options.ApartmentApartmentIds.Contains(r.Id));

            return aList;
        }


        public bool IsEditable(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            if (_propertyService.IsExternalProperty(p))
            {
                IUser currentUser = Services.WorkContext.CurrentUser;
                if (currentUser == null && p.Status.CssClass == "st-estimate")
                    currentUser = _membershipService.GetUser("daiphuhung");
                if (currentUser != null)
                    return (p.CreatedUser.Id == currentUser.Id);
            }
            return false;
        }

        public bool IsViewable(PropertyPart p)
        {
            bool isViewable = true;

            if (p != null)
            {
                if (p.Published != true) isViewable = false;
                if (p.Status != null)
                {
                    string[] listStatus =
                    {
                        "st-new", "st-selling", "st-approved", "st-pending", "st-onhold", "st-sold",
                        "st-no-contact", "st-deleted", "st-trashed"
                    }; //, "st-onhold", "st-sold", "st-no-contact"

                    if (!listStatus.Contains(p.Status.CssClass))
                    {
                        isViewable = false;
                    }
                    else
                    {
                        if (p.Status.CssClass == "st-pending")
                        {
                            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
                            if (currentUser == null)
                                isViewable = false;
                            else if (currentUser.Record != p.CreatedUser) isViewable = false;
                        }
                    }
                }
                else
                {
                    isViewable = false;
                }
            }
            else
            {
                isViewable = false;
            }

            return isViewable;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetProperties(int? propertyId)
        {
            var statusCssClass = new List<string>
            {
                "st-new",
                "st-selling",
                "st-approved",
                "st-pending",
                "st-onhold",
                "st-sold",
                "st-no-contact",
                "st-deleted",
                "st-trashed"
            }; // -- Đang rao bán -- (RAO BÁN, KH RAO BÁN) (Đủ thông tin - Đã duyệt - Chưa hoàn chỉnh)
            List<int> statusIds =
                _propertyService.GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

            IContentQuery<PropertyPart, PropertyPartRecord> pList = Services.ContentManager
                .Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Published)
                .Where(p => statusIds.Contains(p.Status.Id) && p.Id == propertyId)
                .OrderByDescending(p => p.LastUpdatedDate);

            return pList;
        }

        #endregion

        #region Form Init

        public AliasesMetaCreatedOptions InitAliasCreate(AliasesMetaCreatedOptions options)
        {
            RouteValueDictionary routeData = _requestContext.RouteData.Values;
            HttpRequestBase requestData = _requestContext.HttpContext.Request;

            // default options
            if (options == null)
            {
                options = new AliasesMetaCreatedOptions
                {
                    AdsVIP = false,
                    AdsGoodDeal = false,
                    IsAuction = false,
                    IsOwner = false,
                    IsCheckProvince = false,
                    IsCheckUpdateMeta = false
                };
                //options.AllAnyType = false;
            }

            #region Provinces

            // Provinces
            options.Provinces = _addressService.GetProvinces();

            // ProvinceIds
            if (routeData["ProvinceIds"] != null)
                options.ProvinceIds =
                    routeData["ProvinceIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["ProvinceIds"]))
                options.ProvinceIds = requestData["ProvinceIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            #endregion

            #region AdsTypes

            // AdsTypes
            options.AdsTypes = _propertyService.GetAdsTypes();

            // AdsTypeCssClass
            if (routeData["AdsTypeCssClass"] != null) options.AdsTypeCssClass = routeData["AdsTypeCssClass"].ToString();
            else if (!String.IsNullOrEmpty(requestData["AdsTypeCssClass"]))
                options.AdsTypeCssClass = requestData["AdsTypeCssClass"];

            #endregion

            #region TypeGroups

            // TypeGroups
            List<PropertyTypeGroupPartRecord> typeGroups = _propertyService.GetTypeGroups().ToList();
            typeGroups.Insert(0, new PropertyTypeGroupPartRecord { CssClass = "gp-house-land", Name = "Nhà phố và đất" });
            options.TypeGroups = typeGroups;

            // TypeGroupCssClass
            if (routeData["TypeGroupCssClass"] != null)
                options.TypeGroupCssClass = routeData["TypeGroupCssClass"].ToString();
            else if (!String.IsNullOrEmpty(requestData["TypeGroupCssClass"]))
                options.TypeGroupCssClass = requestData["TypeGroupCssClass"];

            #endregion

            #region AnyType

            if (routeData["AdsGoodDeal"] != null) options.AdsGoodDeal = (bool)routeData["AdsGoodDeal"];
            if (routeData["AdsVIP"] != null) options.AdsVIP = (bool)routeData["AdsVIP"];
            if (routeData["IsOwner"] != null) options.IsOwner = (bool)routeData["IsOwner"];
            if (routeData["IsAuction"] != null) options.IsAuction = (bool)routeData["IsAuction"];
            //if (routeData["AllAnyType"] != null) options.AllAnyType = (bool)routeData["AllAnyType"];
            if (routeData["IsCheckProvince"] != null) options.IsCheckProvince = (bool)routeData["IsCheckProvince"];
            if (routeData["IsCheckUpdateMeta"] != null)
                options.IsCheckUpdateMeta = (bool)routeData["IsCheckUpdateMeta"];

            #endregion

            return options;
        }

        public PropertyDisplayEntry InitUserLocationWidget(PropertyDisplayEntry options)
        {
            RouteValueDictionary routeData = _requestContext.RouteData.Values;
            HttpRequestBase requestData = _requestContext.HttpContext.Request;

            // ProvinceId
            if (routeData["id"] != null) options.PropertyId = int.Parse(routeData["id"].ToString());
            else if (!String.IsNullOrEmpty(requestData["id"])) options.PropertyId = int.Parse(requestData["id"]);

            if (string.IsNullOrEmpty(options.PropertyId.ToString()))
                options.PropertyId = 0;

            var p = Services.ContentManager.Get<PropertyPart>(options.PropertyId);

            if (p != null)
            {
                if (p.Province != null)
                    options.UserLocations = _groupService.GetAgencyUserLocationProvinces(p.Province.Id);
                // UserLocations ProvinceID & DistrictID
                options.UserLocations =
                    options.UserLocations.Where(
                        a => a.LocationDistrictPartRecord != null && a.LocationDistrictPartRecord.Id == p.District.Id ||
                        (a.LocationDistrictPartRecord == null && a.LocationProvincePartRecord != null && a.LocationProvincePartRecord.Id == p.Province.Id));
                // UserLocations DistrictID
                var userLocationRecords = options.UserLocations as IList<UserLocationRecord> ?? options.UserLocations.ToList();
                if (options.UserLocations == null || !userLocationRecords.Any() && p.Province != null)
                    options.UserLocations = _groupService.GetAgencyUserLocationProvinces(p.Province.Id);
                //IsSelling or IsLeasing
                if (p.AdsType.CssClass == "ad-selling")
                    options.UserPartIds = userLocationRecords.Where(a => a.IsSelling).Select(a => a.UserPartRecord.Id).Distinct().ToArray();
                else if (p.AdsType.CssClass == "ad-leasing")
                    options.UserPartIds = userLocationRecords.Where(a => a.IsLeasing).Select(a => a.UserPartRecord.Id).Distinct().ToArray();
                else
                    options.UserPartIds = userLocationRecords.Select(a => a.UserPartRecord.Id).ToArray();
            }

            IEnumerable<UserUpdateProfileRecord> userUpdateProfiles;
            if (options.UserPartIds != null && options.UserPartIds.Any())
                userUpdateProfiles = _groupService.GetUserUpdateProfiles(options.UserPartIds);
            else
                userUpdateProfiles = _groupService.GetUserUpdateProfiles(0);

            options.UserUpdateOptions = userUpdateProfiles
                .OrderBy(r => Guid.NewGuid())
                .Select(a => new UserUpdateOptions
                {
                    UserUpdateProfilePart = a,
                    UserLocation = _groupService.GetAgencyUserLocations().Where(i => i.UserPartRecord.Id == a.Id).FirstOrDefault(),
                    UserPart = _contentManager.Get<UserPart>(a.Id) != null ? _contentManager.Get<UserPart>(a.Id).Record : null
                }).Take(5);

            return options;
        }

        #endregion

        public IEnumerable<AdsTypePartRecord> GetAdsTypesFromRoute()
        {
            RouteValueDictionary routeData = _requestContext.RouteData.Values;
            HttpRequestBase requestData = _requestContext.HttpContext.Request;

            string adsTypeCssClass = "";

            if (routeData["AdsTypeCssClass"] != null) adsTypeCssClass = routeData["AdsTypeCssClass"].ToString();
            else if (!String.IsNullOrEmpty(requestData["AdsTypeCssClass"]))
                adsTypeCssClass = requestData["AdsTypeCssClass"];

            IEnumerable<AdsTypePartRecord> adsTypes = _propertyService.GetAdsTypes();

            if (adsTypeCssClass == "ad-selling" || adsTypeCssClass == "ad-buying")
            {
                // Mua bán nhà đất
                adsTypes = adsTypes.Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-buying");
            }
            else if (adsTypeCssClass == "ad-leasing" || adsTypeCssClass == "ad-renting")
            {
                // Nhà đất cho thuê
                adsTypes = adsTypes.Where(a => a.CssClass == "ad-leasing" || a.CssClass == "ad-renting");
            }

            return adsTypes;
        }

        public int UpdateRequirements(CustomerPartRecord customer, CustomerEditRequirementViewModel reqModel)
        {
            int groupId = 0;

            #region ADD NEW REQUIREMENT

            if ((reqModel.DistrictIds != null && reqModel.DistrictIds.Length > 0) || reqModel.ProvinceId != null
                || (reqModel.WardIds != null && reqModel.WardIds.Length > 0)
                || (reqModel.StreetIds != null && reqModel.StreetIds.Length > 0)
                || reqModel.MinArea.HasValue || reqModel.MaxArea.HasValue
                || reqModel.MinWidth.HasValue || reqModel.MaxWidth.HasValue
                || reqModel.MinLength.HasValue || reqModel.MaxLength.HasValue
                || (reqModel.DirectionIds != null && reqModel.DirectionIds.Length > 0)
                || reqModel.LocationId.HasValue
                || reqModel.MinAlleyWidth.HasValue
                || reqModel.MaxAlleyTurns.HasValue
                || reqModel.MaxDistanceToStreet.HasValue
                || reqModel.MinFloors.HasValue || reqModel.MaxFloors.HasValue
                || reqModel.MinPrice.HasValue || reqModel.MaxPrice.HasValue
                )
            {
                LocationProvincePartRecord province = _addressService.GetProvince(reqModel.ProvinceId);
                PropertyLocationPartRecord location = _propertyService.GetLocation(reqModel.LocationId);

                PaymentMethodPartRecord paymentMethod = _propertyService.GetPaymentMethod(reqModel.PaymentMethodId);
                AdsTypePartRecord adstype = _propertyService.GetAdsType(reqModel.AdsTypeId);
                PropertyTypeGroupPartRecord propertyTypeGroup =
                    _propertyService.GetTypeGroup(reqModel.PropertyTypeGroupId);

                #region REMOVE OLD REQUIREMENT

                if (reqModel.GroupId.HasValue) // !string.IsNullOrEmpty(btnSaveRequirement)
                {
                    _customerService.DeleteCustomerRequirements((int)reqModel.GroupId);
                }

                #endregion

                var customerRequirements = new List<CustomerRequirementRecord>();

                if (reqModel.DistrictIds != null)
                {
                    // Add record for each District
                    foreach (int districtId in reqModel.DistrictIds)
                    {
                        LocationDistrictPartRecord district = _addressService.GetDistrict(districtId);
                        List<int> districtStreetIds = reqModel.StreetIds != null
                            ? _addressService.GetStreets(districtId)
                                .Where(a => reqModel.StreetIds.Contains(a.Id))
                                .Select(a => a.Id)
                                .ToList()
                            : null;
                        List<int> districtWardIds = reqModel.WardIds != null
                            ? _addressService.GetWards(districtId)
                                .Where(a => reqModel.WardIds.Contains(a.Id))
                                .Select(a => a.Id)
                                .ToList()
                            : null;
                        List<int> districtApartmentIds = reqModel.ApartmentIds != null
                            ? _addressService.GetApartments(districtId)
                                .Where(r => reqModel.ApartmentIds.Contains(r.Id))
                                .Select(r => r.Id)
                                .ToList()
                            : null;

                        #region Add record Aparment

                        if (reqModel.PropertyTypeGroupId.HasValue && reqModel.PropertyTypeGroupId.Value == 52)
                        // if is Apartment
                        {
                            if (districtApartmentIds != null && districtApartmentIds.Count > 0)
                            {
                                foreach (int apartmentId in districtApartmentIds)
                                {
                                    if (reqModel.DirectionIds != null)
                                    {
                                        foreach (int directionId in reqModel.DirectionIds)
                                        {
                                            CustomerRequirementRecord r =
                                                _customerService.NewCustomerRequirementRecord(reqModel, customer,
                                                    location, paymentMethod, adstype, propertyTypeGroup);
                                            r.LocationProvincePartRecord = province;
                                            r.LocationDistrictPartRecord = district;
                                            r.LocationApartmentPartRecord = _addressService.GetApartment(apartmentId);
                                            r.DirectionPartRecord = _propertyService.GetDirection(directionId);

                                            customerRequirements.Add(r);
                                        }
                                    }
                                    else
                                    {
                                        CustomerRequirementRecord r =
                                            _customerService.NewCustomerRequirementRecord(reqModel, customer, location,
                                                paymentMethod, adstype, propertyTypeGroup);
                                        r.LocationProvincePartRecord = province;
                                        r.LocationDistrictPartRecord = district;
                                        r.LocationApartmentPartRecord = _addressService.GetApartment(apartmentId);

                                        customerRequirements.Add(r);
                                    }
                                }
                            }
                        }

                        #endregion

                        if (districtStreetIds != null && districtStreetIds.Count > 0)
                        {
                            #region Add record for each Street

                            // Add record for each Street
                            foreach (int streetId in districtStreetIds)
                            {
                                LocationStreetPartRecord street = _addressService.GetStreet(streetId);
                                if (street.District.Id == district.Id) // Chỉ lưu record nếu Street thuộc district
                                {
                                    if (reqModel.DirectionIds != null)
                                    {
                                        foreach (int directionId in reqModel.DirectionIds)
                                        {
                                            CustomerRequirementRecord r =
                                                _customerService.NewCustomerRequirementRecord(reqModel, customer,
                                                    location, paymentMethod, adstype, propertyTypeGroup);
                                            r.LocationProvincePartRecord = province;
                                            r.LocationDistrictPartRecord = district;
                                            r.LocationStreetPartRecord = street;
                                            r.DirectionPartRecord = _propertyService.GetDirection(directionId);

                                            customerRequirements.Add(r);
                                        }
                                    }
                                    else
                                    {
                                        CustomerRequirementRecord r =
                                            _customerService.NewCustomerRequirementRecord(reqModel, customer, location,
                                                paymentMethod, adstype, propertyTypeGroup);
                                        r.LocationProvincePartRecord = province;
                                        r.LocationDistrictPartRecord = district;
                                        r.LocationStreetPartRecord = street;

                                        customerRequirements.Add(r);
                                    }
                                }
                            }

                            #endregion
                        }
                        else // No Street is selected
                        {
                            if (districtWardIds != null && districtWardIds.Count > 0)
                            {
                                #region Add record for each Ward

                                // Add record for each Ward
                                foreach (int wardId in districtWardIds)
                                {
                                    LocationWardPartRecord ward = _addressService.GetWard(wardId);
                                    if (ward.District.Id == district.Id) // Chỉ lưu record nếu Ward thuộc district
                                    {
                                        if (reqModel.DirectionIds != null)
                                        {
                                            foreach (int directionId in reqModel.DirectionIds)
                                            {
                                                CustomerRequirementRecord r =
                                                    _customerService.NewCustomerRequirementRecord(reqModel, customer,
                                                        location, paymentMethod, adstype, propertyTypeGroup);
                                                r.LocationProvincePartRecord = province;
                                                r.LocationDistrictPartRecord = district;
                                                r.LocationWardPartRecord = ward;
                                                r.DirectionPartRecord = _propertyService.GetDirection(directionId);

                                                customerRequirements.Add(r);
                                            }
                                        }
                                        else
                                        {
                                            CustomerRequirementRecord r =
                                                _customerService.NewCustomerRequirementRecord(reqModel, customer,
                                                    location, paymentMethod, adstype, propertyTypeGroup);
                                            r.LocationProvincePartRecord = province;
                                            r.LocationDistrictPartRecord = district;
                                            r.LocationWardPartRecord = ward;

                                            customerRequirements.Add(r);
                                        }
                                    }
                                }

                                #endregion
                            }
                            else // No Ward & No Street is selected
                            {
                                if (reqModel.DirectionIds != null)
                                {
                                    foreach (int directionId in reqModel.DirectionIds)
                                    {
                                        CustomerRequirementRecord r =
                                            _customerService.NewCustomerRequirementRecord(reqModel, customer, location,
                                                paymentMethod, adstype, propertyTypeGroup);
                                        r.LocationProvincePartRecord = province;
                                        r.LocationDistrictPartRecord = district;
                                        r.DirectionPartRecord = _propertyService.GetDirection(directionId);

                                        customerRequirements.Add(r);
                                    }
                                }
                                else
                                {
                                    CustomerRequirementRecord r = _customerService.NewCustomerRequirementRecord(
                                        reqModel, customer, location, paymentMethod, adstype, propertyTypeGroup);
                                    r.LocationProvincePartRecord = province;
                                    r.LocationDistrictPartRecord = district;
                                    customerRequirements.Add(r);
                                }
                            }
                        }
                    }
                }
                else // No District is selected
                {
                    if (reqModel.DirectionIds != null)
                    {
                        foreach (int directionId in reqModel.DirectionIds)
                        {
                            CustomerRequirementRecord r = _customerService.NewCustomerRequirementRecord(reqModel,
                                customer, location, paymentMethod, adstype, propertyTypeGroup);
                            r.LocationProvincePartRecord = province;
                            r.DirectionPartRecord = _propertyService.GetDirection(directionId);

                            customerRequirements.Add(r);
                        }
                    }
                    else
                    {
                        CustomerRequirementRecord r = _customerService.NewCustomerRequirementRecord(reqModel, customer,
                            location, paymentMethod, adstype, propertyTypeGroup);
                        r.LocationProvincePartRecord = province;

                        customerRequirements.Add(r);
                    }
                }

                // Update GroupId
                if (customerRequirements.Count > 0)
                {
                    _customerService.UpdateCustomerRequirements(customerRequirements);
                    groupId = customerRequirements.First().GroupId.Value;
                }

                //return RedirectToAction("Edit", new { id = id });
            }

            #endregion

            return groupId;
        }

        #region Build

        public IEnumerable<PropertyDisplayEntry> BuildPropertiesEntries(IEnumerable<PropertyPart> pList)
        {
            // UserView is Count on BuildDisplay
            IList<PropertyPart> propertyParts = pList as IList<PropertyPart> ?? pList.ToList();

            foreach (PropertyPart p in propertyParts)
            {
                _contentManager.BuildDisplay(p);
            }

            return propertyParts.Select(p => _propertyService.BuildPropertyEntryFrontEnd(p)).ToList();
        }

        public IEnumerable<PropertyDisplayEntry> BuildPropertiesEntriesFront(IEnumerable<PropertyPart> pList)
        {
            int locationFront = _propertyService.GetLocation("h-front").Id;
            // UserView is Count on BuildDisplay
            IList<PropertyPart> propertyParts = pList as IList<PropertyPart> ?? pList.ToList();
            foreach (PropertyPart p in propertyParts)
            {
                _contentManager.BuildDisplay(p);
            }

            return
                propertyParts.Where(p => p.Location != null && p.Location.Id == locationFront)
                    .Select(p => _propertyService.BuildPropertyEntryFrontEnd(p))
                    .ToList();
        }

        public IEnumerable<PropertyDisplayEntry> BuildPropertiesEntriesAlley(IEnumerable<PropertyPart> pList)
        {
            int locationAlley = _propertyService.GetLocation("h-alley").Id;
            // UserView is Count on BuildDisplay
            IList<PropertyPart> propertyParts = pList as IList<PropertyPart> ?? pList.ToList();
            foreach (PropertyPart p in propertyParts)
            {
                _contentManager.BuildDisplay(p);
            }

            return
                propertyParts.Where(p => p.Location != null && p.Location.Id == locationAlley)
                    .Select(p => _propertyService.BuildPropertyEntry(p))
                    .ToList();
        }

        #endregion

        #region Build ViewModel

        // Build
        public PropertyFrontEndCreateBaseViewModel BuildCreateBaseViewModel(string adsTypeCssClass)
        {
            return new PropertyFrontEndCreateBaseViewModel
            {
                AdsTypeCssClass = adsTypeCssClass,
                AdsTypes = _propertyService.GetAdsTypes(),
                TypeGroups = _propertyService.GetTypeGroups(),
                Types = _propertyService.GetTypes(null, null)
            };
        }

        public PropertyFrontEndCreateBaseViewModel BuildCreateBaseViewModel(PropertyPart p)
        {
            string adsTypeCssClass = p.AdsType != null ? p.AdsType.CssClass : "";
            string typeGroupCssClass = p.TypeGroup != null ? p.TypeGroup.CssClass : "";

            return new PropertyFrontEndCreateBaseViewModel
            {
                Property = p,
                AdsTypeCssClass = adsTypeCssClass,
                AdsTypes = _propertyService.GetAdsTypes(),
                TypeGroupCssClass = typeGroupCssClass,
                TypeGroups = _propertyService.GetTypeGroups(),
                TypeId = p.Type != null ? p.Type.Id : 0,
                Types = _propertyService.GetTypes(adsTypeCssClass, typeGroupCssClass)
            };
        }

        public PropertyFrontEndEditViewModel BuildEditViewModel(PropertyPart p)
        {
            int currentUserId = Services.WorkContext.CurrentUser != null ? Services.WorkContext.CurrentUser.Id : 0;
            var currentUserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            string adsTypeCssClass = p.AdsType != null ? p.AdsType.CssClass : "";
            string typeGroupCssClass = p.Type != null ? p.Type.Group.CssClass : "";

            int provinceId = p.Province != null
                ? p.Province.Id
                : (currentUserGroup != null && currentUserGroup.DefaultProvince != null)
                    ? currentUserGroup.DefaultProvince.Id
                    : _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            int districtId = p.District != null ? p.District.Id : 0;
            int apartmentId = p.Apartment != null ? p.Apartment.Id : 0;

            IEnumerable<int> advantageIds = _propertyService.GetPropertyAdvantages(p).Select(a => a.Id);
            IEnumerable<int> disadvantageIds = _propertyService.GetPropertyDisAdvantages(p).Select(a => a.Id);
            IEnumerable<int> apartmentAdvantageIds =
                _propertyService.GetPropertyApartmentAdvantages(p).Select(a => a.Id);
            IEnumerable<int> apartmentInteriorAdvantageIds =
                _propertyService.GetPropertyApartmentInteriorAdvantages(p).Select(a => a.Id);
            AdsPaymentHistoryPart amount =
                _adsPaymentService.GetPaymentHistoryLasted(Services.WorkContext.CurrentUser.As<UserPart>());
            AdsPaymentHistoryPart oldHistory = _adsPaymentService.GetPaymentHistoryByProperty(p);

            return new PropertyFrontEndEditViewModel
            {
                Property = p,

                // AdsTypes
                AdsTypeCssClass = adsTypeCssClass,
                AdsTypes =
                    _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing"),

                // TypeGroups
                TypeGroupCssClass = typeGroupCssClass,
                TypeGroups = _propertyService.GetTypeGroups(),

                // Types
                TypeId = p.Type != null ? p.Type.Id : 0,
                Types = _propertyService.GetTypes(adsTypeCssClass, typeGroupCssClass),

                // TypeConstructions
                TypeConstructionId = p.TypeConstruction != null ? p.TypeConstruction.Id : 0,
                TypeConstructions = _propertyService.GetTypeConstructions(p.Type != null ? p.Type.Id : 0, p.Floors),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvinces(),
                DistrictId = districtId,
                Districts = _addressService.GetDistricts(provinceId),
                WardId = p.Ward != null ? p.Ward.Id : 0,
                Wards = _addressService.GetWards(districtId),
                StreetId =
                    p.Street != null ? (p.Street.RelatedStreet != null ? p.Street.RelatedStreet.Id : p.Street.Id) : 0,
                Streets = _addressService.GetStreets(districtId),
                ApartmentId = apartmentId,
                Apartments = _addressService.GetApartments(districtId),
                UnPublishAddress = !p.PublishAddress,

                // LegalStatus
                LegalStatusId = p.LegalStatus != null ? p.LegalStatus.Id : 0,
                LegalStatus = _propertyService.GetLegalStatus(),

                // Directions
                DirectionId = p.Direction != null ? p.Direction.Id : 0,
                Directions = _propertyService.GetDirections(),

                // Locations
                LocationCssClass = p.Location != null ? p.Location.CssClass : "",
                Locations = _propertyService.GetLocations(),
                FloorsCount = p.Floors > 10 ? -1 : p.Floors,

                // Interiors
                InteriorId = p.Interior != null ? p.Interior.Id : 0,
                Interiors = _propertyService.GetInteriors(),

                // PaymentMethods
                PaymentMethodId = p.PaymentMethod != null ? p.PaymentMethod.Id : 0,
                PaymentMethods = _propertyService.GetPaymentMethods(),
                AdsTypeVIP = p.AdsVIP && p.AdsVIPExpirationDate > DateTime.Now || p.AdsVIPRequest ? p.SeqOrder : 0,
                IsAdsVIP = p.AdsVIP && p.AdsVIPExpirationDate > DateTime.Now && !p.AdsVIPRequest,
                // PaymentUnits
                PaymentUnitId = p.PaymentUnit != null ? p.PaymentUnit.Id : 0,
                PaymentUnits = _propertyService.GetPaymentUnits(),

                // Advantages & DisAdvantages
                Advantages =
                    _propertyService.GetAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = advantageIds.Contains(r.Id) })
                        .ToList(),
                DisAdvantages =
                    _propertyService.GetDisAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = disadvantageIds.Contains(r.Id) })
                        .ToList(),
                ApartmentAdvantages =
                    _propertyService.GetApartmentAdvantages()
                        .Select(
                            r =>
                                new PropertyAdvantageEntry
                                {
                                    Advantage = r,
                                    IsChecked = apartmentAdvantageIds.Contains(r.Id)
                                })
                        .ToList(),
                ApartmentInteriorAdvantages =
                    _propertyService.GetApartmentInteriorAdvantages()
                        .Select(
                            r =>
                                new PropertyAdvantageEntry
                                {
                                    Advantage = r,
                                    IsChecked = apartmentInteriorAdvantageIds.Contains(r.Id)
                                })
                        .ToList(),
                Files = _propertyService.GetPropertyFiles(p).Where(a => a.CreatedUser.Id == currentUserId),
                IsEstimateable = _propertyService.IsValidForEstimate(p),
                DateVipFrom = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy"),
                DateVipTo =
                    Convert.ToDateTime(p.AdsVIPRequest || p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now
                        ? p.AdsVIPExpirationDate
                        : DateTime.Now.AddDays(30)).ToString("dd/MM/yyyy"),
                Amount = amount != null ? amount.EndBalance : 0,
                AmountVND = _adsPaymentService.ConvertoVND(amount != null ? amount.EndBalance : 0),
                UnitArray =
                    _adsPaymentService.GetPaymentConfigsAsVip().OrderBy(r => r.Value).Select(r => r.Value).ToArray(),
                PostingDates =
                    oldHistory != null && p.AdsVIPExpirationDate.HasValue
                        ? (int)(p.AdsVIPExpirationDate.Value - DateTime.Now).TotalDays
                        : 1,
                HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                AcceptPostToFacebok = true
            };
        }

        #endregion

        public string GetFrontEndSetting(string name)
        {
            return _cacheManager.Get("CacheFrontEndSetting_" + name, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("CacheFrontEndSetting_" + name + "_Changed"));

                if (string.IsNullOrEmpty(name)) return null;
                FrontEndSettingPart item =
                    _contentManager.Query<FrontEndSettingPart, FrontEndSettingPartRecord>()
                        .Where(a => a.Name == name)
                        .Slice(1)
                        .FirstOrDefault();

                return item == null ? null : item.Value;
            });
        }

        #region Metas

        /// <summary>
        /// </summary>
        /// <param name="province">Tên tỉnh/tp</param>
        /// {TaiTinhThanh}
        /// <param name="anytype">Giá rẻ/Giao dịch gấp/Chính chủ/phát mãi</param>
        /// {LoaiGiaoDich}
        /// <param name="flagAdsType">1. Nhà bán/ 2. Nhà đất cho thuê/ 3. Nhà cần bán/ 4. Nhà cần thuê</param>
        /// {NhomBDS}
        /// <param name="flagTypeGroup">1. Đất ở và các loại nhà/ 2. Chung cư, căn hộ/ 3. Các loại đất khác/ 4. Tất cả</param>
        /// {LoaiBDS}
        /// <returns></returns>
        public Dictionary<string, string> Metas(string province, string anytype, int flagAdsType, int flagTypeGroup)
        {
            var meta = new Dictionary<string, string>();
            string buy, leasing;

            if (flagAdsType == 1 || flagAdsType == 2) // bán  & cho thuê
            {
                buy = "Bán";
                leasing = "Cho thuê";
            }
            else
            {
                buy = "Cần bán";
                leasing = "Cần thuê";
            }


            if (flagAdsType == 1 || flagAdsType == 3)
            {
                switch (flagTypeGroup)
                {
                    case 1:
                        // nha pho
                        meta["Title"] = buy + " đất ở " + anytype.ToLower() + ", nhà phố " + anytype.ToLower() +
                                         ", biệt thự " + anytype.ToLower() + ", cao ốc văn phòng " + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = buy + ", đất, ở, bán, nhà, phố, biệt, thự, bán, cao, ốc, văn phòng " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " ,tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam. ";
                        meta["Description"] = buy + " nhà đất " + anytype.ToLower() + " tại " + province +
                                                " với các loại diện tích, giá bán, vị trí, đặc điểm khác nhau. " + buy +
                                                " đất ở và bán nhà, bán biệt thự, bán cao ốc văn phòng, đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                    case 2: // can ho
                        meta["Title"] = buy + " căn hộ " + anytype.ToLower() + ", " + buy.ToLower() + " chung cư " +
                                         anytype.ToLower() + ", " + buy.ToLower() + " căn hộ cao cấp " +
                                         anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = buy.ToLower() +
                                            ", đất, ở, bán, nhà, phố, biệt, thự, bán, cao, ốc, văn phòng " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " ,tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam. ";
                        meta["Description"] = buy + " căn hộ " + anytype.ToLower() + ", " + buy.ToLower() +
                                                " chung cư " + anytype.ToLower() + ", " + buy.ToLower() +
                                                " căn hộ cao cấp " + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "") +
                                                " với các loại diện tích, giá bán, vị trí, đặc điểm khác nhau. " + buy +
                                                " căn hộ, chung cư, đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                    case 3: // dat khac
                        meta["Title"] = buy + " các loại đất " + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = buy.ToLower() + ", đất, vườn, đất, công, nghiệp, đất, nông, nghiệp " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " ,tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam.";
                        meta["Description"] = buy + " đất " + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "") +
                                                " với các loại diện tích, giá bán, vị trí, đặc điểm khác nhau. " + buy +
                                                " đất đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                    case 4:
                    case 5: // tat ca
                        meta["Title"] = "Nhà đất " + buy.ToLower() + " " + anytype.ToLower() + ", nhà đất " +
                                         buy.ToLower() + " " + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = "bất, động, sản, nhà, đất, " + buy.ToLower() +
                                            ", đất, thổ, cư, đất, vườn, đất, nông, nghiệp " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam.";
                        meta["Description"] = "Nhà đất " + buy.ToLower() + " " + anytype.ToLower() + ", nhà đất " +
                                                buy.ToLower() + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "") +
                                                " với các loại diện tích, giá bán, vị trí, đặc điểm khác nhau. Bán nhà đất đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                }
            }
            else
            {
                switch (flagTypeGroup)
                {
                    case 1:
                        //nha pho
                        meta["Title"] = leasing + " đất ở " + anytype.ToLower() + ", nhà phố " + anytype.ToLower() +
                                         ", biệt thự " + anytype.ToLower() + ", cao ốc văn phòng " + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = string.Join(",", leasing.ToLower().Split(' ')) +
                                            ", đất, ở, nhà, phố, cho, thuê, biệt, thự, cho, thuê, cao, ốc, văn phòng " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " ,tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam. ";
                        meta["Description"] = leasing + " nhà đất " + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "") +
                                                " với các loại diện tích, giá thuê, vị trí, đặc điểm khác nhau. " +
                                                leasing + " đất ở và " + leasing.ToLower() + " nhà, " +
                                                leasing.ToLower() + " biệt thự, " + leasing.ToLower() +
                                                " cao ốc văn phòng, đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                    case 2: //can ho
                        meta["Title"] = leasing + " căn hộ " + anytype.ToLower() + ", cho thuê chung cư " +
                                         anytype.ToLower() + ", cho thuê căn hộ cao cấp" + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = string.Join(",", leasing.ToLower().Split(' ')) +
                                            ", căn, hộ, cho, thuê, chung, cư, cho, thuê, căn, hộ, cao, cấp " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam. ";
                        meta["Description"] = leasing + " căn hộ, " + leasing.ToLower() + " chung cư, " +
                                                leasing.ToLower() + " căn hộ cao cấp " + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "") +
                                                " với các loại diện tích, giá " + leasing.ToLower() +
                                                ", vị trí, đặc điểm khác nhau. " + leasing +
                                                " căn hộ, chung cư, đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                    case 3: // cac loai dat khac
                        meta["Title"] = leasing + " đất " + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = string.Join(",", leasing.ToLower().Split(' ')) +
                                            ", đất, thổ, cư, đất, vườn, đất, công, nghiệp " +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Việt, Nam";
                        meta["Description"] = leasing + " đất " + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "") +
                                                " với các loại diện tích, giá " + leasing.ToLower() +
                                                ", vị trí, đặc điểm khác nhau. thuê và " + leasing +
                                                " nhà đất, đầy đủ, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        break;
                    case 4:
                    case 5: // tat ca
                        meta["Title"] = "Nhà đất " + leasing.ToLower() + " " + anytype.ToLower() + ", nhà đất " +
                                         leasing.ToLower() + " " + anytype.ToLower() +
                                         (!string.IsNullOrEmpty(province) ? " tại " + province : "");
                        meta["Keywords"] = "nhà, đất " + string.Join(",", leasing.ToLower().Split(' ')) +
                                            ", nhà, đất, đất, vườn, đất, công, nghiệp" +
                                            string.Join(", ", anytype.ToLower().Split(' ')) +
                                            (!string.IsNullOrEmpty(province)
                                                ? " tại " + string.Join(", ", province.Split(' '))
                                                : "") + " Viet, Nam ";
                        meta["Description"] = leasing + " nhà đất " + anytype.ToLower() +
                                                (!string.IsNullOrEmpty(province)
                                                    ? " tại " + string.Join(", ", province.Split(' '))
                                                    : "") + " với các loại diện tích, giá " + leasing.ToLower() +
                                                ", vị trí, đặc điểm khác nhau. " + leasing +
                                                " nhà đất, đầy đủ nhất, cập nhật nhanh nhất" +
                                                (!string.IsNullOrEmpty(province)
                                                    ? " tại " + string.Join(", ", province.Split(' '))
                                                    : "");
                        break;
                }
            }

            return meta;
        }

        /// <summary>
        /// </summary>
        /// <param name="district"></param>
        /// <param name="province">Tên tỉnh/tp</param>
        /// {TaiTinhThanh}
        /// <param name="anytype">Giá rẻ/Giao dịch gấp/Chính chủ/phát mãi</param>
        /// {LoaiGiaoDich}
        /// <param name="metaTitle">Meta tite cho alias</param>
        /// <param name="metaKeyword">Meta keyword cho alias</param>
        /// ///
        /// <param name="metaDescription">Meta Description cho alias</param>
        /// <returns></returns>
        public Dictionary<string, string> Metas(string metaTitle, string metaKeyword, string metaDescription,
            string district, string province, string anytype)
        {
            var meta = new Dictionary<string, string>();
            var autoMeta = new Dictionary<string, string>();
            autoMeta["{TaiQuanHuyen}"] = district;
            autoMeta["{TaiTinhThanh}"] = province;
            autoMeta["{LoaiGiaoDich}"] = anytype;

            meta["Title"] = AutoReplaceMeta(metaTitle, autoMeta);
            meta["Keywords"] = AutoReplaceMeta(metaKeyword, autoMeta);
            meta["Description"] = AutoReplaceMeta(metaDescription, autoMeta);

            return meta;
        }

        public void UpdateAliasMeta(AliasRecord alias, Dictionary<string, string> value)
        {
            AliasesMetaPart aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                .Where(a => a.Alias_Id == alias.Id).Slice(1).FirstOrDefault();

            if (aliasesMeta == null)
            {
                var aliasesMetaCreate = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");
                aliasesMetaCreate.Title = value["Title"];
                aliasesMetaCreate.Keywords = value["Keywords"];
                aliasesMetaCreate.Description = value["Description"];
                aliasesMetaCreate.Alias_Id = alias.Id;
                Services.ContentManager.Create(aliasesMetaCreate);
                Services.Notifier.Information(T("Vừa thêm Meta, Description, Title cho alias {0}", alias.Path));
            }
            else
            {
                // Xóa cache theo path khi cập nhật Meta mới
                ClearCacheMeta(alias.Path);

                aliasesMeta.Title = value["Title"];
                aliasesMeta.Keywords = value["Keywords"];
                aliasesMeta.Description = value["Description"];
                Services.Notifier.Information(T("Vừa cập nhật Meta, Description, Title cho alias {0}", alias.Path));
            }
        }

        public void UpdateAliasMeta(AliasRecord alias, AliasesMetaCreatedOptions options, string district,
            string province, string anytype)
        {
            AliasesMetaPart aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                .Where(a => a.Alias_Id == alias.Id).Slice(1).FirstOrDefault();

            var autoMeta = new Dictionary<string, string>();
            autoMeta["{TaiQuanHuyen}"] = district;
            autoMeta["{TaiTinhThanh}"] = province;
            autoMeta["{LoaiGiaoDich}"] = anytype;

            if (aliasesMeta == null)
            {
                var aliasesMetaCreate = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");
                if (options.IsCheckUpdateTitle)
                {
                    aliasesMetaCreate.Title = AutoReplaceMeta(options.MetaTitle, autoMeta);
                    Services.Notifier.Information(T("Vừa thêm Title cho alias {0}", alias.Path));
                }
                if (options.IsCheckUpdateMetaKeyword)
                {
                    aliasesMetaCreate.Keywords = AutoReplaceMeta(options.MetaKeyword, autoMeta);
                    Services.Notifier.Information(T("Vừa thêm Keyword cho alias {0}", alias.Path));
                }
                if (options.IsCheckUpdateMetaDescription)
                {
                    aliasesMetaCreate.Description = AutoReplaceMeta(options.MetaDescription, autoMeta);
                    Services.Notifier.Information(T("Vừa thêm Description cho alias {0}", alias.Path));
                }
                aliasesMetaCreate.Alias_Id = alias.Id;
                Services.ContentManager.Create(aliasesMetaCreate);
            }
            else
            {
                if (options.IsCheckUpdateTitle || options.IsCheckUpdateMetaKeyword ||
                    options.IsCheckUpdateMetaDescription)
                {
                    // Xóa cache theo path khi cập nhật Meta mới
                    ClearCacheMeta(alias.Path);
                }
                if (options.IsCheckUpdateTitle)
                {
                    aliasesMeta.Title = AutoReplaceMeta(options.MetaTitle, autoMeta);
                    Services.Notifier.Information(T("Vừa cập nhật Title cho alias {0}", alias.Path));
                }
                if (options.IsCheckUpdateMetaKeyword)
                {
                    aliasesMeta.Keywords = AutoReplaceMeta(options.MetaKeyword, autoMeta);
                    Services.Notifier.Information(T("Vừa cập nhật Keyword cho alias {0}", alias.Path));
                }
                if (options.IsCheckUpdateMetaDescription)
                {
                    aliasesMeta.Description = AutoReplaceMeta(options.MetaDescription, autoMeta);
                    Services.Notifier.Information(T("Vừa cập nhật Description cho alias {0}", alias.Path));
                }
            }
        }

        public void UpdateAliasMeta(AliasRecord alias, AliasesMetaCreatedOptions options, string district,
            string province)
        {
            AliasesMetaPart aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                .Where(a => a.Alias_Id == alias.Id).Slice(1).FirstOrDefault();

            var autoMeta = new Dictionary<string, string>();
            autoMeta["{TaiQuanHuyen}"] = district;
            autoMeta["{TaiTinhThanh}"] = province;

            string title = "";
            string keywords = !string.IsNullOrEmpty(province) ? "Dự án " + province : "";
            string description = "";
            if (options.IsCheckUpdateMeta) // Nếu không tự điền Meta: Title, Keywords, Description
            {
                title = "Dự án " + district + " " + province;
                keywords += !string.IsNullOrEmpty(district) ? "Dự án " + district : "";
                description = "Dự án " + district + " " + province;
            }

            if (aliasesMeta == null)
            {
                var aliasesMetaCreate = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");

                aliasesMetaCreate.Title = AutoReplaceMeta(!options.IsCheckUpdateMeta ? options.MetaTitle : title,
                    autoMeta);
                Services.Notifier.Information(T("Vừa thêm Title cho alias {0}", alias.Path));

                aliasesMetaCreate.Keywords = AutoReplaceMeta(
                    !options.IsCheckUpdateMeta ? options.MetaKeyword : keywords, autoMeta);
                Services.Notifier.Information(T("Vừa thêm Keyword cho alias {0}", alias.Path));

                aliasesMetaCreate.Description =
                    AutoReplaceMeta(!options.IsCheckUpdateMeta ? options.MetaDescription : description, autoMeta);
                Services.Notifier.Information(T("Vừa thêm Description cho alias {0}", alias.Path));

                aliasesMetaCreate.Alias_Id = alias.Id;
                Services.ContentManager.Create(aliasesMetaCreate);
            }
            else
            {
                if (options.IsCheckUpdateTitle || options.IsCheckUpdateMetaKeyword ||
                    options.IsCheckUpdateMetaDescription)
                {
                    // Xóa cache theo path khi cập nhật Meta mới
                    ClearCacheMeta(alias.Path);
                }
                if (options.IsCheckUpdateTitle)
                {
                    aliasesMeta.Title = AutoReplaceMeta(!options.IsCheckUpdateMeta ? options.MetaTitle : title,
                        autoMeta);
                    Services.Notifier.Information(T("Vừa cập nhật Title cho alias {0}", alias.Path));
                }
                if (options.IsCheckUpdateMetaKeyword)
                {
                    aliasesMeta.Keywords = AutoReplaceMeta(!options.IsCheckUpdateMeta ? options.MetaKeyword : keywords,
                        autoMeta);
                    Services.Notifier.Information(T("Vừa cập nhật Keyword cho alias {0}", alias.Path));
                }
                if (options.IsCheckUpdateMetaDescription)
                {
                    aliasesMeta.Description =
                        AutoReplaceMeta(!options.IsCheckUpdateMeta ? options.MetaDescription : description, autoMeta);
                    Services.Notifier.Information(T("Vừa cập nhật Description cho alias {0}", alias.Path));
                }
            }
        }

        public Dictionary<string, string> PropertyGetSeoMeta(string path)
        {
            //Get Current DomainGroupId
            var defaultDomainGroup = _groupService.GetDefaultDomainGroup();
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            return _cacheManager.Get("CacheMeta_" + currentDomainGroup.Id + "_" + path, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("CacheMeta_" + currentDomainGroup.Id + "_" + path + "_Changed"));

                var meta = new Dictionary<string, string>();
                meta["Title"] = "";
                meta["Keywords"] = "";
                meta["Description"] = "";

                if (path == "/")
                {
                    path = String.Empty;
                }

                RouteValueDictionary aliasGet = _aliasService.Get(path);

                if (aliasGet != null)
                {
                    AliasRecord aliasRecord = _aliasRepository.Table.Single(a => a.Path == path);
                    AliasesMetaPart aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                                                .Where(r => r.Alias_Id == aliasRecord.Id && r.DomainGroupId == currentDomainGroup.Id).Slice(1).FirstOrDefault()
                                                ??
                                                  _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                                                .Where(r => r.Alias_Id == aliasRecord.Id && r.DomainGroupId == defaultDomainGroup.Id).Slice(1).FirstOrDefault();

                    if (aliasesMeta != null)
                    {
                        meta["Title"] = !string.IsNullOrEmpty(aliasesMeta.Title) ? aliasesMeta.Title : "";
                        meta["Keywords"] = !string.IsNullOrEmpty(aliasesMeta.Keywords) ? aliasesMeta.Keywords : "";
                        meta["Description"] = !string.IsNullOrEmpty(aliasesMeta.Description)
                            ? aliasesMeta.Description
                            : "";

                        return meta;
                    }
                }
                return meta;
            });
        }

        public void ClearCacheMeta(string path)
        {
            //Get Current DomainGroupId
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            _signals.Trigger("CacheMeta_" + currentDomainGroup.Id + "_" + path + "_Changed");
        }

        public string AutoReplaceMeta(string stringReplace, Dictionary<string, string> autoContent)
        {
            List<string> replaceRules = autoContent.Keys.ToList();
            // {"{TaiQuanHuyen}", "{TaiTinhThanh}", "{LoaiGiaoDich}" };

            return replaceRules.Aggregate(stringReplace,
                (current, r) => !string.IsNullOrEmpty(current) ? current.Replace(r, autoContent[r]) : "");
        }

        #endregion

        #region Statistic

        public int CountPropertyByType(string adsTypeCssClass, string typeCssClass)
        {
            var hostname = _hostNameService.GetHostNameSite();
            return _cacheManager.Get("CountProperty-" + adsTypeCssClass + "-" + typeCssClass + "-" + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));

                AdsTypePartRecord adsType = _propertyService.GetAdsType(adsTypeCssClass);
                PropertyTypePartRecord pType = _propertyService.GetType(typeCssClass);

                return
                    _propertyService.GetProperties()
                        .Where(p => p.AdsType == adsType && p.Type == pType)
                        .Count();
            });
        }

        public int CountPropertyByAdsType(string adsTypeCssClass, string typeGroupCssClass)
        {
            var hostname = _hostNameService.GetHostNameSite();
            return _cacheManager.Get("CountProperty-" + adsTypeCssClass + "-" + typeGroupCssClass + "-" + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));

                var adsType = _propertyService.GetAdsType(adsTypeCssClass);
                var typeGroup = _propertyService.GetTypeGroup(typeGroupCssClass);

                if (typeGroupCssClass == "gp-land")
                {
                    var typeResidentialLand = _propertyService.GetType("tp-residential-land"); // Đất ở
                    return
                        _propertyService.GetProperties()
                            .Where(p => p.AdsType == adsType && (p.TypeGroup == typeGroup || p.Type == typeResidentialLand))
                            .Count();
                }
                else
                {
                    return
                        _propertyService.GetProperties()
                            .Where(p => p.AdsType == adsType && p.TypeGroup == typeGroup)
                            .Count();
                }
            });
        }

        public int CountPropertyWidgetByAdsType(string typeWidget, string adsTypeCssClass)
        {
            var hostname = _hostNameService.GetHostNameSite();
            return _cacheManager.Get("CountProperty-" + typeWidget + "-" + adsTypeCssClass + "-" + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));

                int count = 0;

                int adsTypeSellingId = _propertyService.GetAdsType("ad-selling").Id;
                int adsTypeLeasingId = _propertyService.GetAdsType("ad-leasing").Id;
                int adsTypeBuyingId = _propertyService.GetAdsType("ad-buying").Id;
                int adsTypeRentingId = _propertyService.GetAdsType("ad-renting").Id;
                int statusApprovedIdCustomer = _customerService.GetStatus("st-approved").Id;

                switch (typeWidget)
                {
                    case "gooddeal":
                        switch (adsTypeCssClass)
                        {
                            case "ad-selling":
                                count = _propertyService.GetProperties()
                                    .Where(p => p.AdsType.Id == adsTypeSellingId)
                                    .Where(p =>
                                        ((p.Flag.Id == 32 || p.Flag.Id == 33) && p.IsExcludeFromPriceEstimation != true) || // Nhà rẻ (deal-good) và Nhà rất rẻ (deal-very-good) nhưng không bị loại khỏi định giá
                                        (p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now)) // Nhà quảng cáo BĐS giá rẻ;
                                    .Count();
                                break;
                            case "ad-leasing":
                                count = _propertyService.GetProperties()
                                    .Where(p => p.AdsType.Id == adsTypeLeasingId)
                                    .Where(p =>
                                        ((p.Flag.Id == 32 || p.Flag.Id == 33) && p.IsExcludeFromPriceEstimation != true) || // Nhà rẻ (deal-good) và Nhà rất rẻ (deal-very-good) nhưng không bị loại khỏi định giá
                                        (p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now)) // Nhà quảng cáo BĐS giá rẻ;
                                    .Count();
                                break;
                        }
                        break;
                    case "vip":
                        switch (adsTypeCssClass)
                        {
                            case "ad-selling":
                                count = _propertyService.GetProperties()
                                    .Where(
                                        p =>
                                            p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now &&
                                            p.AdsType.Id == adsTypeSellingId).Count();
                                break;
                            case "ad-leasing":
                                count = _propertyService.GetProperties()
                                    .Where(
                                        p =>
                                            p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now &&
                                            p.AdsType.Id == adsTypeLeasingId).Count();
                                break;
                            case "ad-buying":
                                count = Services.ContentManager
                                    .Query<CustomerPart, CustomerPartRecord>()
                                    .Where(
                                        p =>
                                            p.Status.Id == statusApprovedIdCustomer && p.AdsVIP &&
                                            p.AdsVIPExpirationDate >= DateTime.Now &&
                                            p.Requirements.Any(a => a.AdsTypePartRecord.Id == adsTypeBuyingId)).Count();
                                break;
                            case "ad-renting":
                                count = Services.ContentManager
                                    .Query<CustomerPart, CustomerPartRecord>()
                                    .Where(
                                        p =>
                                            p.Status.Id == statusApprovedIdCustomer && p.AdsVIP &&
                                            p.AdsVIPExpirationDate >= DateTime.Now &&
                                            p.Requirements.Any(a => a.AdsTypePartRecord.Id == adsTypeRentingId)).Count();
                                break;
                        }
                        break;
                    case "new":
                        switch (adsTypeCssClass)
                        {
                            case "ad-selling":
                                count =
                                    _propertyService.GetProperties().Where(p => p.AdsType.Id == adsTypeSellingId).Count();
                                break;
                            case "ad-leasing":
                                count =
                                    _propertyService.GetProperties().Where(p => p.AdsType.Id == adsTypeLeasingId).Count();
                                break;
                            case "ad-buying":
                                count = Services.ContentManager
                                    .Query<CustomerPart, CustomerPartRecord>()
                                    .Where(
                                        p =>
                                            p.Status.Id == statusApprovedIdCustomer && p.Published &&
                                            p.Requirements.Any(a => a.AdsTypePartRecord.Id == adsTypeBuyingId)).Count();
                                break;
                            case "ad-renting":
                                count = Services.ContentManager
                                    .Query<CustomerPart, CustomerPartRecord>()
                                    .Where(
                                        p =>
                                            p.Status.Id == statusApprovedIdCustomer && p.Published &&
                                            p.Requirements.Any(a => a.AdsTypePartRecord.Id == adsTypeRentingId)).Count();
                                break;
                        }
                        break;
                    case "highlight":
                        var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
                        switch (adsTypeCssClass)
                        {
                            case "ad-selling":
                                count = _propertyService.GetProperties()
                                .Where(
                                    p =>
                                        p.AdsVIP && p.SeqOrder == 3 && p.AdsVIPExpirationDate >= DateTime.Now &&
                                        (p.AdsType != null && p.AdsType.Id == adsTypeSellingId) &&
                                        p.UserGroup == currentDomainGroup).Count();
                                break;
                            case "ad-leasing":
                                count = _propertyService.GetProperties()
                                .Where(
                                    p =>
                                        p.AdsVIP && p.SeqOrder == 3 && p.AdsVIPExpirationDate >= DateTime.Now &&
                                        (p.AdsType != null && p.AdsType.Id == adsTypeLeasingId) &&
                                        p.UserGroup == currentDomainGroup).Count();
                                break;
                        }
                        break;
                    default:
                        count = 0;
                        break;
                }

                return count;
            });
        }

        public int CountRequirementByAdsType(string adsTypeCssClass)
        {
            return _cacheManager.Get("CountRequirement-" + adsTypeCssClass, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                //ctx.Monitor(_signals.When("FrontEndProperties_Changed"));

                int adsTypeId = _propertyService.GetAdsType(adsTypeCssClass).Id;
                return _contentManager.Query<CustomerPart, CustomerPartRecord>()
                    .Where(
                        p =>
                            p.Status == _customerService.GetStatus("st-approved") &&
                            p.Requirements.Any(a => a.AdsTypePartRecord.Id == adsTypeId)).Count();
            });
        }

        #endregion

        #region Customer

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomers()
        {
            var statusCssClass = new List<string> { "st-new", "st-high" }; // KH MỚI, CẦN MUA GẤP, ĐÃ DUYỆT
            List<int> statusIds =
                _customerService.GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            IContentQuery<CustomerPart, CustomerPartRecord> cList = Services.ContentManager
                .Query<CustomerPart, CustomerPartRecord>()
                // LastUpdate 29-05-2014 - Trước đó là lấy những khách hàng { "st-new", "st-high" , "st-approved"} và  p.Published == true
                .Where(
                    p =>
                        (statusIds.Contains(p.Status.Id) && p.Published) ||
                        p.Status == _customerService.GetStatus("st-approved"))
                .OrderByDescending(p => p.LastUpdatedDate);

            return cList;
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> SearchCustomers(PropertyDisplayIndexOptions options)
        {
            IContentQuery<CustomerPart, CustomerPartRecord> cList = GetCustomers();

            #region FILTER

            if (!string.IsNullOrEmpty(options.SearchPhone))
                cList = cList.Where(a => a.ContactPhone.Contains(options.SearchPhone));

            // AdsType
            if (!string.IsNullOrEmpty(options.AdsTypeCssClass))
                cList =
                    cList.Where(
                        p =>
                            p.Requirements.Any(
                                a => a.AdsTypePartRecord == _propertyService.GetAdsType(options.AdsTypeCssClass)));
            //Services.Notifier.Information(T("cListCount 2: {0} - {1} - {2}", cList.Count(), options.AdsTypeCssClass, _propertyService.GetAdsType(options.AdsTypeCssClass).Id));

            // TypeGroup
            if (!string.IsNullOrEmpty(options.TypeGroupCssClass))
                cList =
                    cList.Where(
                        p =>
                            p.Requirements.Any(
                                a =>
                                    a.PropertyTypeGroupPartRecord ==
                                    _propertyService.GetTypeGroup(options.TypeGroupCssClass)));

            #region Address

            // Province
            if (options.ProvinceId > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.LocationProvincePartRecord.Id == options.ProvinceId));

            // DistrictIds
            if (options.DistrictIds != null)
                cList =
                    cList.Where(
                        p => p.Requirements.Any(a => options.DistrictIds.Contains(a.LocationDistrictPartRecord.Id)));

            // WardIds
            if (options.WardIds != null)
                cList = cList.Where(p => p.Requirements.Any(a => options.WardIds.Contains(a.LocationWardPartRecord.Id)));

            // StreetIds
            if (options.StreetIds != null)
                cList =
                    cList.Where(p => p.Requirements.Any(a => options.StreetIds.Contains(a.LocationStreetPartRecord.Id)));

            #endregion

            // DirectionIds
            if (options.DirectionIds != null)
                cList =
                    cList.Where(p => p.Requirements.Any(a => options.DirectionIds.Contains(a.DirectionPartRecord.Id)));

            // OtherProjectName
            if (!string.IsNullOrEmpty(options.OtherProjectName))
                cList = cList.Where(p => p.Requirements.Any(a => a.OtherProjectName.Contains(options.OtherProjectName)));

            #region ApartmentFloorThRange

            int minApartmentFloorTh = 0;
            int maxApartmentFloorTh = 0;

            switch (options.ApartmentFloorThRange)
            {
                case PropertyDisplayApartmentFloorTh.All:
                case PropertyDisplayApartmentFloorTh.None:
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                    minApartmentFloorTh = 1;
                    maxApartmentFloorTh = 3;
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                    minApartmentFloorTh = 4;
                    maxApartmentFloorTh = 7;
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                    minApartmentFloorTh = 8;
                    maxApartmentFloorTh = 12;
                    break;
                case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                    minApartmentFloorTh = 12;
                    break;
            }

            if (minApartmentFloorTh > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinApartmentFloorTh >= minApartmentFloorTh));
            if (maxApartmentFloorTh > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.MaxApartmentFloorTh <= maxApartmentFloorTh));

            #endregion

            #region Location - MinAlleyWidth

            switch (options.AlleyTurnsRange)
            {
                case PropertyDisplayLocation.All:
                case PropertyDisplayLocation.None:
                    break;
                case PropertyDisplayLocation.AllWalk:
                    options.LocationId = _propertyService.GetLocation("h-front").Id;
                    options.MinAlleyWidth = 0;
                    break;
                case PropertyDisplayLocation.Alley6:
                    options.LocationId = _propertyService.GetLocation("h-alley").Id;
                    options.MinAlleyWidth = 6;
                    break;
                case PropertyDisplayLocation.Alley5:
                    options.LocationId = _propertyService.GetLocation("h-alley").Id;
                    options.MinAlleyWidth = 5;
                    break;
                case PropertyDisplayLocation.Alley4:
                    options.LocationId = _propertyService.GetLocation("h-alley").Id;
                    options.MinAlleyWidth = 4;
                    break;
                case PropertyDisplayLocation.Alley3:
                    options.LocationId = _propertyService.GetLocation("h-alley").Id;
                    options.MinAlleyWidth = 3;
                    break;
                case PropertyDisplayLocation.Alley2:
                    options.LocationId = _propertyService.GetLocation("h-alley").Id;
                    options.MinAlleyWidth = 2;
                    break;
                case PropertyDisplayLocation.Alley:
                    options.LocationId = _propertyService.GetLocation("h-alley").Id;
                    options.MinAlleyWidth = 1;
                    break;
            }

            if (options.LocationId > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.PropertyLocationPartRecord.Id == options.LocationId));

            if (options.MinAlleyWidth > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinAlleyWidth >= options.MinAlleyWidth));
            //if (options.MaxAlleyWidth > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MaxAlleyWidth <= options.MaxAlleyWidth));

            //if (options.MinAlleyTurns > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MinAlleyTurns >= options.MinAlleyTurns));
            //if (options.MaxAlleyTurns > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MaxAlleyTurns <= options.MaxAlleyTurns));

            //if (options.MinDistanceToStreet > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MinDistanceToStreet >= options.MinDistanceToStreet));
            //if (options.MaxDistanceToStreet > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MaxDistanceToStreet <= options.MaxDistanceToStreet));

            #endregion

            #region Area

            if (options.MinAreaTotal.HasValue)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinArea >= options.MinAreaTotal));
            //if (options.MaxAreaTotal.HasValue) cList = cList.Where(p => p.Requirements.Any(a => a.MaxArea <= options.MaxAreaTotal));

            if (options.MinAreaTotalWidth.HasValue)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinWidth >= options.MinAreaTotalWidth));
            //if (options.MaxAreaTotalWidth.HasValue) cList = cList.Where(p => p.Requirements.Any(a => a.MaxWidth <= options.MaxAreaTotalWidth));

            if (options.MinAreaTotalLength.HasValue)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinLength >= options.MinAreaTotalLength));
            //if (options.MaxAreaTotalLength.HasValue) cList = cList.Where(p => p.Requirements.Any(a => a.MaxLength <= options.MaxAreaTotalLength));

            #endregion

            // Floors
            if (options.MinFloors > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinFloors >= options.MinFloors));
            //if (options.MaxFloors > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MaxFloors <= options.MaxFloors));

            // Bedrooms
            if (options.MinBedrooms > 0)
                cList = cList.Where(p => p.Requirements.Any(a => a.MinBedrooms >= options.MinBedrooms));
            //if (options.MaxBedrooms > 0) cList = cList.Where(p => p.Requirements.Any(a => a.MaxBedrooms <= options.MaxBedrooms));

            #region Price

            // Convert Price to VND

            //double minPriceVnd = options.MinPriceProposed.HasValue
            //    ? _propertyService.ConvertToVndB((double) options.MinPriceProposed, options.PaymentMethodCssClass)
            //    : 0;
            //double maxPriceVnd = options.MaxPriceProposed.HasValue
            //    ? _propertyService.ConvertToVndB((double) options.MaxPriceProposed, options.PaymentMethodCssClass)
            //    : 0;

            PaymentMethodPartRecord paymentMethod = _propertyService.GetPaymentMethod(options.PaymentMethodCssClass);

            if (options.MinPriceProposed > 0)
                cList =
                    cList.Where(
                        p =>
                            p.Requirements.Any(
                                a =>
                                    a.PaymentMethodPartRecord == paymentMethod && a.MinPrice >= options.MinPriceProposed));
            if (options.MaxPriceProposed > 0)
                cList =
                    cList.Where(
                        p =>
                            p.Requirements.Any(
                                a =>
                                    a.PaymentMethodPartRecord == paymentMethod && a.MaxPrice <= options.MaxPriceProposed));

            #endregion

            if (options.AdsVIP)
            {
                cList = cList.Where(p => p.AdsVIP);
                cList = cList.Where(p => p.AdsVIPExpirationDate > DateTime.Now);
            }

            #endregion

            return cList;
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> SearchCustomersRequirementTheSame(
            CustomerPartRecord customer)
        {
            IContentQuery<CustomerPart, CustomerPartRecord> cList = GetCustomers();
            List<int?> groupIds = customer.Requirements.Where(r => r.IsEnabled).Select(r => r.GroupId).ToList();
            foreach (var groupId in groupIds)
            {
                List<CustomerRequirementRecord> reqs = customer.Requirements.Where(r => r.GroupId == groupId).ToList();
                CustomerRequirementRecord req = reqs.First();

                var options = new PropertyDisplayIndexOptions
                {
                    AdsTypeCssClass = req.AdsTypePartRecord.CssClass,
                    ProvinceId = req.LocationProvincePartRecord != null ? req.LocationProvincePartRecord.Id : 0,
                    DistrictIds = reqs.Any(r => r.LocationDistrictPartRecord != null)
                        ? reqs.Where(r => r.LocationDistrictPartRecord != null)
                            .Select(r => r.LocationDistrictPartRecord.Id)
                            .ToArray()
                        : null,
                    MinPriceProposed = req.MinPrice.HasValue
                        ? _propertyService.ConvertToVndB((double)req.MinPrice, req.PaymentMethodPartRecord.CssClass)
                        : 0,
                    MaxPriceProposed = req.MaxPrice.HasValue
                        ? _propertyService.ConvertToVndB((double)req.MaxPrice, req.PaymentMethodPartRecord.CssClass)
                        : 0,
                    PaymentMethodCssClass = req.PaymentMethodPartRecord.CssClass
                };

                #region Filter

                // Province
                if (options.ProvinceId > 0)
                    cList =
                        cList.Where(p => p.Requirements.Any(a => a.LocationProvincePartRecord.Id == options.ProvinceId));
                // DistrictIds
                if (options.DistrictIds != null)
                    cList =
                        cList.Where(
                            p => p.Requirements.Any(a => options.DistrictIds.Contains(a.LocationDistrictPartRecord.Id)));

                // Convert Price to VND

                //double minPriceVnd = options.MinPriceProposed.HasValue
                //    ? _propertyService.ConvertToVndB((double) options.MinPriceProposed, options.PaymentMethodCssClass)
                //    : 0;
                //double maxPriceVnd = options.MaxPriceProposed.HasValue
                //    ? _propertyService.ConvertToVndB((double) options.MaxPriceProposed, options.PaymentMethodCssClass)
                //    : 0;

                PaymentMethodPartRecord paymentMethod = _propertyService.GetPaymentMethod(options.PaymentMethodCssClass);

                if (options.MinPriceProposed > 0)
                    cList =
                        cList.Where(
                            p =>
                                p.Requirements.Any(
                                    a =>
                                        a.PaymentMethodPartRecord == paymentMethod &&
                                        a.MinPrice >= options.MinPriceProposed));
                if (options.MaxPriceProposed > 0)
                    cList =
                        cList.Where(
                            p =>
                                p.Requirements.Any(
                                    a =>
                                        a.PaymentMethodPartRecord == paymentMethod &&
                                        a.MaxPrice <= options.MaxPriceProposed));
                if (!string.IsNullOrEmpty(options.AdsTypeCssClass))
                    cList.Where(r => r.Requirements.Any(s => s.AdsTypePartRecord.CssClass == options.AdsTypeCssClass));

                #endregion
            }

            return cList.Where(r => r.Id != customer.Id);
        }

        public CustomerDetailViewModel BuildDetailViewModel(CustomerPart c)
        {
            IEnumerable<int> purposeIds = _customerService.GetCustomerPurposes(c).Select(a => a.Id);
            //var _purposeIds = c.Purposes.Select(r => r.Id).ToList();

            var displayPhone = new List<string>();
            if (!String.IsNullOrEmpty(c.ContactPhone))
            {
                var find = new Regex(@"\d+");
                MatchCollection str = find.Matches(c.ContactPhone.Replace(".", ""));
                displayPhone.AddRange(from object i in str select i.ToString());
            }

            return new CustomerDetailViewModel
            {
                Customer = c,
                DisplayPhone = displayPhone,
                Purposes =
                    _customerService.GetPurposes()
                        .Select(r => new CustomerPurposeEntry { Purpose = r, IsChecked = purposeIds.Contains(r.Id) })
                        .ToList(),
                Requirements =
                    _customerService.GetRequirements(c.Record)
                        .Select(r => new CustomerRequirementEntry { Requirement = r })
                        .ToList(),
            };
        }

        public CustomerDetailViewModel BuildPropertyExchangeDetailViewModel(PropertyPart p)
        {
            var propertyExchange = _propertyService.GetExchangePartRecord(p);
            var c = _contentManager.Get<CustomerPart>(propertyExchange.Customer.Id);

            IEnumerable<int> purposeIds = _customerService.GetCustomerPurposes(c).Select(a => a.Id);
            //var _purposeIds = c.Purposes.Select(r => r.Id).ToList();

            var displayPhone = new List<string>();
            if (!String.IsNullOrEmpty(c.ContactPhone))
            {
                var find = new Regex(@"\d+");
                MatchCollection str = find.Matches(c.ContactPhone.Replace(".", ""));
                displayPhone.AddRange(from object i in str select i.ToString());
            }

            return new CustomerDetailViewModel
            {
                Customer = c,
                DisplayPhone = displayPhone,
                Purposes =
                    _customerService.GetPurposes()
                        .Select(r => new CustomerPurposeEntry { Purpose = r, IsChecked = purposeIds.Contains(r.Id) })
                        .ToList(),
                Requirements =
                    _customerService.GetRequirements(c.Record)
                        .Select(r => new CustomerRequirementEntry { Requirement = r })
                        .ToList(),
                PropertyExchange = propertyExchange
            };
        }

        public bool IsViewable(CustomerPart c)
        {
            bool isViewable = true;

            if (c != null)
            {
                if (c.Published != true) isViewable = false;

                string[] listStatus = { "st-new", "st-high", "st-approved", "st-pending" };

                if (!listStatus.Contains(c.Status.CssClass))
                {
                    isViewable = false;
                }
                else
                {
                    if (c.Status.CssClass == "st-pending")
                    {
                        IUser currentuser = Services.WorkContext.CurrentUser;
                        if (currentuser != null && currentuser.Id == c.CreatedUser.Id)
                        {
                            // owner
                        }
                        else
                        {
                            isViewable = false;
                        }
                    }
                }
            }
            else
            {
                isViewable = false;
            }

            return isViewable;
        }

        #endregion

        public List<PriceData> BuildPriceData()
        {
            //Đơn vị tỉ đồng
            var model = new List<PriceData>()
            {
                new PriceData{ Id =-1, MinValue = 0, MaxValue = 0, Name = "Chọn mức giá"},
                new PriceData{ Id = 0, MinValue = 0, MaxValue = 0, Name = "Thỏa thuận"},
                new PriceData{ Id =1, MinValue = 0, MaxValue = 0.5, Name = "< 500 triệu"},
                new PriceData{ Id =2, MinValue = 0.5, MaxValue = 0.8, Name = "500 - 800 triệu"},
                new PriceData{ Id =3, MinValue = 0.8, MaxValue = 1, Name = "800 triệu - 1 tỷ"},
                new PriceData{ Id =5, MinValue = 1, MaxValue = 2, Name = "1 - 2 tỷ"},
                new PriceData{ Id =6, MinValue = 2, MaxValue = 3, Name = "2 - 3 tỷ"},
                new PriceData{ Id =7, MinValue = 3, MaxValue = 5, Name = "3 - 5 tỷ"},
                new PriceData{ Id =8, MinValue = 5, MaxValue = 7, Name = "5 - 7 tỷ"},
                new PriceData{ Id =9, MinValue = 7, MaxValue = 10, Name = "7 - 10 tỷ"},
                new PriceData{ Id =10, MinValue = 10, MaxValue = 20, Name = "10 - 20 tỷ"},
                new PriceData{ Id =11, MinValue = 20, MaxValue = 30, Name = "20 - 30 tỷ"},
                new PriceData{ Id =12, MinValue = 30, MaxValue = 0, Name = "> 30 tỷ"}
            };

            return model;
        }
    }
}