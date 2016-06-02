using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Contrib.OnlineUsers.Models;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Places.Response;
using ImageResizer;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Services;
using Orchard.Tags.Helpers;
using Orchard.Tags.Services;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.ViewModels;
using Vandelay.Industries.Models;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace RealEstate.Services
{
    public interface IPropertyService : IDependency
    {
        #region Repository

        #region Status, Flag

        PropertyStatusPartRecord GetStatus(int? statusId);
        PropertyStatusPartRecord GetStatus(string statusCssClass);
        IEnumerable<PropertyStatusPartRecord> GetStatus();
        IEnumerable<PropertyStatusPartRecord> GetStatusWithDefault(int? value, string name);

        IEnumerable<PropertyStatusPartRecord> GetStatusForInternal();
        IEnumerable<PropertyStatusPartRecord> GetStatusForExternal();
        IEnumerable<PropertyStatusPartRecord> GetStatusForTargetCssClass(string targetStatusCssClass);
        List<string> ExternalStatusCssClass();

        PropertyFlagPartRecord GetFlag(int? flagId);
        PropertyFlagPartRecord GetFlag(string flagCssClass);
        IEnumerable<PropertyFlagPartRecord> GetFlags();

        #endregion

        #region AdsType

        AdsTypePartRecord GetAdsType(int? adsTypeId);
        AdsTypePartRecord GetAdsType(string adsTypeCssClass);
        IEnumerable<AdsTypePartRecord> GetAdsTypes();

        #endregion

        #region PropertyType, TypeGroup, PropertyGroup

        PropertyTypePartRecord GetType(int? typeId);
        PropertyTypePartRecord GetType(string typeCssClass);
        IEnumerable<PropertyTypePartRecord> GetTypes();
        IEnumerable<PropertyTypePartRecord> GetTypes(string adsTypeCssClass, string typeGroupCssClass);

        List<String> GetTypeDefaultImageUrls(PropertyTypePartRecord type);
        List<String> GetTypeDefaultImageUrls(PropertyTypePartRecord type, double floors);
        String GetTypeRandomImageUrl(PropertyTypePartRecord type, double floors);
        String GetAvatarImageUrl(PropertyPart p);
        String GetDefaultImageUrl(PropertyPart p);
        String GetAvatarImageUrl(LocationApartmentPart p);
        String GetDefaultImageUrl(LocationApartmentPart p);

        IContentQuery<PropertyPart, PropertyPartRecord> GetProperties();
        IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesNewest();
        IContentQuery<PropertyPart, PropertyPartRecord> GetIsAuctionProperties();

        IEnumerable<LocationApartmentPartRecord> GetListApartmentsByProperty();
        void ClearCacheApartmentsByProperty();

        PropertyTypeGroupPartRecord GetTypeGroup(int? typeGroupId);
        PropertyTypeGroupPartRecord GetTypeGroup(string typeGroupCssClass);
        IEnumerable<PropertyTypeGroupPartRecord> GetTypeGroups();

        PropertyTypeConstructionPartRecord GetTypeConstruction(int? typeConstructionId);
        PropertyTypeConstructionPartRecord GetTypeConstruction(string typeConstructionCssClass);
        IEnumerable<PropertyTypeConstructionPartRecord> GetTypeConstructions();
        IEnumerable<PropertyTypeConstructionPartRecord> GetTypeConstructions(int? propertyTypeId, double? floor);
        PropertyTypeConstructionPartRecord GetTypeConstructionDefaultInFloorsRange(int? propertyTypeId, double? floor);

        IEnumerable<PropertyGroupPart> GetPropertyGroup();
        IEnumerable<PropertyGroupPart> GetPropertyGroup(int? groupId);
        IEnumerable<PropertyGroupPart> GetPropertyGroup(UserGroupPartRecord group);

        #endregion

        #region LegalStatus, Interior

        PropertyLegalStatusPartRecord GetLegalStatus(int? legalStatusId);
        PropertyLegalStatusPartRecord GetLegalStatus(string legalStatusName);
        IEnumerable<PropertyLegalStatusPartRecord> GetLegalStatus();

        PropertyInteriorPartRecord GetInterior(int? interiorId);
        IEnumerable<PropertyInteriorPartRecord> GetInteriors();

        #endregion

        #region Direction, Location, UserGroupDomains

        DirectionPartRecord GetDirection(int? directionId);
        IEnumerable<DirectionPartRecord> GetDirections();

        PropertyLocationPartRecord GetLocation(int? locationId);
        PropertyLocationPartRecord GetLocation(string locationCssClass);
        IEnumerable<PropertyLocationPartRecord> GetLocations();

        IEnumerable<HostNamePartRecord> GetUserGroupDomains();

        #endregion

        #region PaymentMethod, PaymentMethod, PaymentExchange

        PaymentMethodPartRecord GetPaymentMethod(int? paymentMethodId);
        PaymentMethodPartRecord GetPaymentMethod(string paymentMethodCssClass);
        IEnumerable<PaymentMethodPartRecord> GetPaymentMethods();

        PaymentUnitPartRecord GetPaymentUnit(int? paymentUnitId);
        PaymentUnitPartRecord GetPaymentUnit(string paymentUnitCssClass);
        IEnumerable<PaymentUnitPartRecord> GetPaymentUnits();

        PaymentExchangePartRecord GetPaymentExchange(int? paymentExchangeId);
        IEnumerable<PaymentExchangePartRecord> GetPaymentExchanges();

        #endregion

        #region ExchangeRates

        double Convert(double input, string fromPaymentMethod, string toPaymentMethod);

        double ConvertToVndB(double input, string paymentMethod);
        double ConvertToVndM(double input, string paymentMethod);
        double ConvertToVndK(double input, string paymentMethod);
        double ConvertToUsd(double input, string paymentMethod);
        double ConvertToGold(double input, string paymentMethod);

        double ConvertToVndB(double input, string paymentMethod, double areaTotal);
        double ConvertToVndM(double input, string paymentMethod, double areaTotal);
        double ConvertToVndK(double input, string paymentMethod, double areaTotal);
        double ConvertToUsd(double input, string paymentMethod, double areaTotal);
        double ConvertToGold(double input, string paymentMethod, double areaTotal);

        #endregion

        #region Advantages, DisAdvantages

        void AddAdvantages(PropertyPart item, IEnumerable<PropertyAdvantagePartRecordContent> advantages);

        void UpdateApartmentAdvantagesForContentItem(LocationApartmentPart item,
            IEnumerable<PropertyAdvantageEntry> advantages);

        IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForApartment(LocationApartmentPart item);

        #endregion

        #region UserProperties

        bool SaveUserProperties(PropertyPart p);
        bool CheckIsSavedProperty(int propertyId);
        IEnumerable<PropertyUserPartRecordContent> GetListPropertyUserContent();
        IContentQuery<PropertyPart, PropertyPartRecord> GetListPropertyUserFrontEnd();
        void DeleteUserProperty(int propertyId);

        #endregion

        #region GetPropertyAdvantages

        // Advantages
        IEnumerable<PropertyAdvantagePartRecord> GetAdvantages();
        IList<PropertyAdvantageEntry> GetAdvantagesEntries();

        // DisAdvantages
        IEnumerable<PropertyAdvantagePartRecord> GetDisAdvantages();
        IList<PropertyAdvantageEntry> GetDisAdvantagesEntries();

        // Apartment Advantages
        IEnumerable<PropertyAdvantagePartRecord> GetApartmentAdvantages();
        IList<PropertyAdvantageEntry> GetApartmentAdvantagesEntries();

        // Apartment Interior Advantages
        IEnumerable<PropertyAdvantagePartRecord> GetApartmentInteriorAdvantages();
        IList<PropertyAdvantageEntry> GetApartmentInteriorAdvantagesEntries();

        // Construction Advantages
        IEnumerable<PropertyAdvantagePartRecord> GetConstructionAdvantages();
        IList<PropertyAdvantageEntry> GetConstructionAdvantagesEntries();

        #endregion

        #region GetAdvantagesForProperty

        //IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForProperty(PropertyPart p);
        List<PropertyAdvantageItem> GetAdvantagesForProperty(PropertyPart p);
        //IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForProperty(PropertyPart p, String advShortName);

        IEnumerable<PropertyAdvantagePartRecord> GetPropertyAdvantages(PropertyPart p);
        IEnumerable<PropertyAdvantagePartRecord> GetPropertyDisAdvantages(PropertyPart p);
        IEnumerable<PropertyAdvantagePartRecord> GetPropertyApartmentAdvantages(PropertyPart p);
        IEnumerable<PropertyAdvantagePartRecord> GetPropertyApartmentInteriorAdvantages(PropertyPart p);
        IEnumerable<PropertyAdvantagePartRecord> GetPropertyConstructionAdvantages(PropertyPart p);

        #endregion

        #region GetAdvantagesForLocationApartment

        IEnumerable<PropertyAdvantagePartRecord> GetPropertyApartmentAdvantages(LocationApartmentPart p);

        string GetApartmentBlockInfo(PropertyPart p);


        #endregion

        #region UpdateAdvantagesForProperty

        void UpdatePropertyAdvantages(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages);
        void UpdatePropertyDisAdvantages(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages);
        void UpdatePropertyApartmentAdvantages(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages);

        void UpdatePropertyApartmentInteriorAdvantages(PropertyPart property,
            IEnumerable<PropertyAdvantageEntry> advantages);

        void UpdatePropertyConstructionAdvantages(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages);

        #endregion

        #endregion

        #region Property

        PropertyPartRecord GetProperty(int? id);

        PropertyFilePart GetPropertyFile(int propertyId, string filePath);
        IEnumerable<PropertyFilePart> GetPropertyFiles(PropertyPart p);
        IEnumerable<PropertyFilePart> GetApartmentBlockInfoFiles(ApartmentBlockInfoPart p);
        void ClearCacheGetApartmentBlockInfoFiles(ApartmentBlockInfoPart p);

        IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetPropertyApartmentFiles(PropertyPart p);
        IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetAllPropertyFiles(PropertyPart p);
        IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetAllLocationApartmentFiles(LocationApartmentPart p);

        int IntParseAddressNumber(string addressNumber);

        void UploadImages(IEnumerable<HttpPostedFileBase> files, PropertyPart p, bool isPublished);
        PropertyFilePart UploadPropertyImage(HttpPostedFileBase file, PropertyPart p, UserPart createdUser, bool isPublished);

        void UploadImagesForBlockInfo(IEnumerable<HttpPostedFileBase> files, ApartmentBlockInfoPart p, bool isPublished);
        PropertyFilePart UploadPropertyImageForBlockInfo(HttpPostedFileBase file, ApartmentBlockInfoPart p, UserPart createdUser, bool isPublished);
        string UploadAvatarBlockInfo(HttpPostedFileBase file, ApartmentBlockInfoPart blockInfo);

        void ResizeAllImages();

        PropertyPart CopyToAdsType(PropertyPart property, string adsTypeCssClass, bool publishedCopy,
            double priceProposedCopy, int paymentMethodIdCopy, int paymentUnitIdCopy);

        PropertyPart Copy(PropertyPart property);
        PropertyPart CopyToGroup(PropertyPart property);
        PropertyPart CopyToApproved(PropertyPart property);
        PropertyGroupPart NotShareToGroup(int id, int groupId);

        #endregion

        #region Mass-Update Properties

        void UpdateNegotiateStatus();

        void UpdateMetaDescriptionKeywords();
        void UpdateMetaDescriptionKeywords(PropertyPart p, bool overwrite);

        void TransferPropertyTypeConstruction();
        void TransferPropertyTypeConstruction(PropertyPart p);

        void UpdatePlacesAroundForProperty();
        void UpdatePlacesAroundForProperty(PropertyPart p);
        void UpdatePlacesAroundForProperty(PropertyPart p, double lat, double lng);

        #endregion

        #region Search Propertes

        PropertyIndexOptions BuildIndexOptions(PropertyIndexOptions options);
        PropertyIndexOptions BuildGroupIndexOptions(PropertyIndexOptions options);
        IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(PropertyIndexOptions options);
        IContentQuery<PropertyPart, PropertyPartRecord> SearchGroupProperties(PropertyIndexOptions options);

        #endregion

        #region Group Properties

        IContentQuery<PropertyPart, PropertyPartRecord> GetUserProperties(UserPart user);
        IContentQuery<PropertyPart, PropertyPartRecord> GetGroupProperties(UserGroupPartRecord group);

        #endregion

        #region Helper

        string GetDisplayName(PropertyPart p);
        string GetDisplayForContact(PropertyPart p);
        double CalcArea(double? areaTotal, double? areaTotalWidth, double? areaTotalLength, double? areaTotalBackWidth);
        double CalcAreaTotal(PropertyPart p);
        double CalcAreaLegal(PropertyPart p);
        double CalcAreaConstructionFloor(PropertyPart p);
        double CalcAreaForFilter(PropertyPart p);
        double CalcAreaUsable(PropertyPart p);
        double? CaclPriceProposedInVnd(PropertyPart p);
        double? CalPriceProposedInVndRealArea(double realAreaUse, PropertyPart p);//Tinh tổng giá theo diện tích thông thủy
        int CountVisitedCustomer(PropertyPart p);

        #endregion

        #region Build ViewModel

        PropertyCreateViewModel BuildCreateViewModel(string adsTypeCssClass, string typeGroupCssClass);
        PropertyCreateViewModel BuildCreateViewModel(PropertyCreateViewModel model);
        PropertyEditViewModel BuildEditViewModel(PropertyPart p);
        PropertyViewModel BuildViewModel(PropertyPart p);

        PropertyDisplayEntry BuildPropertyEntry(PropertyPart p);
        PropertyDisplayEntry BuildPropertyEntryFrontEnd(PropertyPart p);
        IEnumerable<PropertyDisplayEntry> BuildPropertiesEntries(IEnumerable<PropertyPart> pList);

        LocationApartmentDisplayEntry BuildLocationApartmentEntry(LocationApartmentPart p);
        LocationApartmentCartCreateViewModel BuildApartmentCartCreate(int? apartmentId);

        LocationApartmentCartIndexViewModel BuildApartmentCartIndex(int apartmentId, bool isFrontEnd);
        LocationApartmentBlockItem BuildApartmentCartByBlockId(int blockId);
        LocationApartmentBlockItem BuildApartmentCartByBlockId(int apartmentId, string blockShortName);
        bool VerifyApartmentGroupInBlock(int apartmentBlockId, int groupPositionInBlock);
        GroupInApartmentBlockPart CheckApartmentGroupInBlock(int blockId, int groupPosition);
        void UpdatePropertyInfoByApartmentInfo(int apartmentBlockInfo);

        #endregion

        #region Verify

        IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(int? provinceId, int? districtId, int? wardId,
            int? streetId, int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber);

        bool EnableViewProperty(PropertyPart p, UserPart user);
        bool EnableViewAddressNumber(PropertyPart p, UserPart user);

        bool EnableEditProperty(PropertyPart p, UserPart user);
        bool EnableEditStatus(PropertyPart p, UserPart user);
        bool EnableEditAddressNumber(PropertyPart p, UserPart user);
        bool EnableEditContactPhone(PropertyPart p, UserPart user);
        bool EnableEditPropertyImages(PropertyPart p, UserPart user);
        bool EnableAddAdsGoodDeal(UserPart user);
        bool EnableAddAdsGoodDeal(UserGroupPartRecord group);
        //bool EnableAddAdsVIP(UserPartRecord user);
        //bool EnableAddAdsVIP(UserGroupPartRecord group);
        bool EnableAddAdsHighlight(UserPart user);
        bool EnableAddAdsHighlight(UserGroupPartRecord group);

        bool IsValid(PropertyPart p);
        bool IsValidForCopyToGroup(PropertyPart p);
        bool IsValidToPublish(PropertyPart p);
        bool IsExternalProperty(PropertyPart p);
        bool IsValidForEstimate(PropertyPart p);

        #region Internal Properties

        IContentQuery<PropertyPart, PropertyPartRecord> GetInternalPropertiesByAddress(int? provinceId, int? districtId,
            int? wardId, int? streetId, int? apartmentId, string addressNumber, string addressCorner,
            string apartmentNumber);

        PropertyPart GetPropertyByAddress(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass);

        bool VerifyPropertyUnicity(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass);

        bool VerifyPropertyGroupExist(int propertyId, int userGroupId);

        #endregion

        #region External Properties

        IContentQuery<PropertyPart, PropertyPartRecord> GetExternalPropertiesByAddress(int? provinceId, int? districtId,
            int? wardId, int? streetId, int? apartmentId, string addressNumber, string addressCorner,
            string apartmentNumber);

        PropertyPart GetUserPropertyByAddress(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass);

        PropertyPart GetUserPropertyByAddress(int userId, int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass);

        // VerifyUserPropertyUnicity

        bool VerifyUserPropertyUnicity(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass);

        bool VerifyUserPropertyUnicity(int userId, int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass);

        #endregion

        #endregion

        void UpdateOrderByDomainGroup(PropertyPart p, int userGroupId);

        DateTime? GetAddExpirationDate(ExpirationDate input, DateTime? expirationDate);

        #region Orchard Tags

        void AutoUpdateTags(PropertyPart p);

        #endregion

        #region PropertiesExchange

        int PropertyExchangeCount();
        bool IsEditableProperty(int id);
        bool IsEditablePropertyExchange(int id, int pId);
        bool IsEditablePropertyExchange(int pId);
        bool IsPropertyExchange(int propertyId);

        PropertyExchangePartRecord GetExchangePartRecord(PropertyPart p);
        PropertyExchangePartRecord GetExchangePartRecord(CustomerPart c);
        PropertyExchangePartRecord GetExchangePartRecord(int id);
        PropertyExchangePartRecord GetExchangePartRecordByPropertyId(int PropertyId);
        //Exchange Type
        List<SelectListItem> GetExchangeTypeParts();
        ExchangeTypePart GetExchangeType(int id);
        ExchangeTypePart GetExchangeType(string cssClass);
        IEnumerable<PropertyExchangePartRecord> GetListPropertyExchange();
        IContentQuery<PropertyPart, PropertyPartRecord> GetListPropertyExchangeQueryFrontEnd();

        List<int> ListOwnPropertyIdsExchange();
        List<int> ListOwnCustomerIdsExchange();
        List<int> ListPropertyIdsExchange();
        void ClearCachePropertyExchange();

        #endregion


        #region Statistics
        string BuildMessageString(object data);

        object CountPendingProperties(bool countPendingInDetails);
        object CountPendingCustomers(bool countPendingInDetails);

        #endregion

        #region Estimate

        Task<PropertyEstimateEntry> EstimateProperty(int id);
        Task<List<int>> GetListPropertiesUseToEstimate(string key);
        Task<bool> ClearApplicationCache(int id);
        Task<bool> IsEstimateable(int districtId, int wardId, int streetId, string addressNumber, string addressCorner);

        #endregion
    }

    public class PropertyService : IPropertyService
    {
        private static readonly Random Random = new Random();
        private static readonly object SyncLock = new object();

        // Init

        private readonly List<string> _externalStatusCssClass = new List<string>
        {
            "st-pending",
            "st-estimate",
            "st-approved",
            "st-invalid",
            "st-draft"
        }; //, "st-trashed"

        // Các loại BĐS khi rao bán: Đất thổ cư; Nhà phố; Biệt thự; Khách sạn; Cao ốc Văn Phòng; Kho xưởng
        private readonly List<string> _sellingPropertyTypeCssClass = new List<string>
        {
            "tp-residential-land",
            "tp-house",
            "tp-concrete-house",
            "tp-villa",
            "tp-office-building",
            "tp-hotel",
            "tp-warehouse-workshop"
        };

        private readonly List<string> _statusGroupCssClass = new List<string> { "st-new", "st-selling", "st-approved" };

        public static int RandomNumber(int max)
        {
            // synchronize
            lock (SyncLock)
            {
                return Random.Next(max);
            }
        }

        #region Init

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours
        private readonly IAddressService _addressService;
        //private readonly IAdsPaymentHistoryService _adsPaymentService;
        private readonly IRepository<PropertyAdvantagePartRecordContent> _advantagesContentRepository;
        private readonly IRepository<LocationApartmentAdvantagePartRecordContent> _apartmentAdvantagesContentRepository;

        private readonly IRepository<PropertyUserPartRecordContent> _userPropertiesContentRepository;

        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IRepository<CustomerPropertyRecord> _customerpropertyRepository;
        private readonly IFacebookApiService _facebookApiSevice;
        private readonly IGoogleApiService _googleApiService;
        private readonly IRepository<UserGroupContactRecord> _groupContactRepository;
        private readonly IUserGroupService _groupService;
        private readonly IHostNameService _hostNameService;
        private readonly IRevisionService _revisionService;
        private readonly IPropertySettingService _settingService;
        private readonly ISignals _signals;
        private readonly ITagService _tagService;
        private readonly ILocationApartmentService _apartmentService;
        private readonly IRepository<PropertyExchangePartRecord> _propertyExchangeRepository;

        //private bool _debugIndex = true;

        public PropertyService(
            IAddressService addressService,
            IRevisionService revisionService,
            IUserGroupService groupService,
            IPropertySettingService settingService,
            IHostNameService hostNameService,
            IRepository<UserGroupContactRecord> groupContactRepository,
            IRepository<CustomerPropertyRecord> customerpropertyRepository,
            IRepository<PropertyAdvantagePartRecordContent> advantagesContentRepository,
            IRepository<LocationApartmentAdvantagePartRecordContent> apartmentAdvantagesContentRepository,
            IRepository<PropertyUserPartRecordContent> userPropertiesContentRepository,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IGoogleApiService googleApiService,
            IOrchardServices services,
            //IAdsPaymentHistoryService adsPaymentService,
            IFacebookApiService facebookApiSevice,
            ITagService tagService,
            ILocationApartmentService apartmentService,
            IRepository<PropertyExchangePartRecord> propertyExchangeRepository
            )
        {
            _addressService = addressService;
            _revisionService = revisionService;
            _groupService = groupService;
            _settingService = settingService;
            _hostNameService = hostNameService;

            _groupContactRepository = groupContactRepository;
            _customerpropertyRepository = customerpropertyRepository;
            _advantagesContentRepository = advantagesContentRepository;
            _apartmentAdvantagesContentRepository = apartmentAdvantagesContentRepository;
            _userPropertiesContentRepository = userPropertiesContentRepository;

            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;
            _googleApiService = googleApiService;
            //_adsPaymentService = adsPaymentService;
            _facebookApiSevice = facebookApiSevice;
            _tagService = tagService;
            _apartmentService = apartmentService;
            _propertyExchangeRepository = propertyExchangeRepository;

            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }

        #endregion

        #region Repository

        #region Status

        public PropertyStatusPartRecord GetStatus(int? statusId)
        {
            if (statusId > 0)
            {
                var partRecord = _contentManager.Get<PropertyStatusPart>((int)statusId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyStatusPartRecord GetStatus(string statusCssClass)
        {
            if (!String.IsNullOrEmpty(statusCssClass))
            {
                PropertyStatusPart partRecord =
                    _contentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                        .Where(a => a.CssClass == statusCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyStatusPartRecord> GetStatus()
        {
            return _cacheManager.Get("Status", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Status_Changed"));

                return
                    _contentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        public IEnumerable<PropertyStatusPartRecord> GetStatusWithDefault(int? value, string name)
        {
            IEnumerable<PropertyStatusPartRecord> statuses = GetStatus();

            // user không có quyền xóa BĐS
            if (!Services.Authorizer.Authorize(Permissions.DeleteProperty))
                statuses = statuses.Where(a => a.CssClass != "st-deleted");

            // user không có quyền duyệt BĐS
            if (!Services.Authorizer.Authorize(Permissions.ApproveProperty))
                statuses = statuses.Where(a => !_externalStatusCssClass.Contains(a.CssClass));

            List<PropertyStatusPartRecord> listStatus = statuses.ToList();

            listStatus.Insert(0,
                new PropertyStatusPartRecord
                {
                    Id = value ?? 0,
                    Name = String.IsNullOrEmpty(name) ? "-- Tất cả --" : name
                });

            return listStatus;
        }

        public IEnumerable<PropertyStatusPartRecord> GetStatusForInternal()
        {
            IEnumerable<PropertyStatusPartRecord> internalStatus = GetStatus();//.Where(a => !_externalStatusCssClass.Contains(a.CssClass));

            // User không có quyền duyệt BĐS
            if (!Services.Authorizer.Authorize(Permissions.ApproveProperty))
                internalStatus = internalStatus.Where(a => !_externalStatusCssClass.Contains(a.CssClass));

            // User không có quyền xóa BĐS
            if (!Services.Authorizer.Authorize(Permissions.DeleteProperty))
                internalStatus = internalStatus.Where(a => a.CssClass != "st-deleted");

            // User không có quyền set BĐS "Đang thương lượng" ("Đặt cọc giữ chỗ")
            if (!Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties))
                internalStatus = internalStatus.Where(a => a.CssClass != "st-negotiate");

            // User không có quyền set BĐS "Chờ giao dịch" ("Đặt cọc mua bán")
            if (!Services.Authorizer.Authorize(Permissions.AccessTradingProperties))
                internalStatus = internalStatus.Where(a => a.CssClass != "st-trading");

            // User không có quyền set BĐS "Đã giao dịch"
            if (!Services.Authorizer.Authorize(Permissions.AccessSoldProperties))
                internalStatus = internalStatus.Where(a => a.CssClass != "st-sold");

            // User không có quyền set BĐS "Tạm ngưng"
            if (!Services.Authorizer.Authorize(Permissions.AccessOnHoldProperties))
                internalStatus = internalStatus.Where(a => a.CssClass != "st-onhold");

            return internalStatus;
        }

        public IEnumerable<PropertyStatusPartRecord> GetStatusForExternal()
        {
            return GetStatus().Where(a => _externalStatusCssClass.Contains(a.CssClass));
        }

        public IEnumerable<PropertyStatusPartRecord> GetStatusForTargetCssClass(string targetStatusCssClass)
        {
            if (_externalStatusCssClass.Contains(targetStatusCssClass))
                return GetStatusForExternal();
            return GetStatusForInternal();
        }

        public List<string> ExternalStatusCssClass()
        {
            return _externalStatusCssClass;
        }

        #endregion

        #region Flags

        public PropertyFlagPartRecord GetFlag(int? flagId)
        {
            if (flagId > 0)
            {
                var partRecord = _contentManager.Get<PropertyFlagPart>((int)flagId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyFlagPartRecord GetFlag(string flagCssClass)
        {
            if (!String.IsNullOrEmpty(flagCssClass))
            {
                PropertyFlagPart partRecord =
                    _contentManager.Query<PropertyFlagPart, PropertyFlagPartRecord>()
                        .Where(a => a.CssClass == flagCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyFlagPartRecord> GetFlags()
        {
            return _cacheManager.Get("Flags", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Flags_Changed"));

                return
                    _contentManager.Query<PropertyFlagPart, PropertyFlagPartRecord>()
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region AdsTypes

        public AdsTypePartRecord GetAdsType(int? adsTypeId)
        {
            if (adsTypeId > 0)
            {
                var partRecord = _contentManager.Get<AdsTypePart>((int)adsTypeId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public AdsTypePartRecord GetAdsType(string adsTypeCssClass)
        {
            if (!String.IsNullOrEmpty(adsTypeCssClass))
            {
                AdsTypePart partRecord =
                    _contentManager.Query<AdsTypePart, AdsTypePartRecord>()
                        .Where(a => a.CssClass == adsTypeCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<AdsTypePartRecord> GetAdsTypes()
        {
            return _cacheManager.Get("AdsTypes", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("AdsTypes_Changed"));

                return
                    _contentManager.Query<AdsTypePart, AdsTypePartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region Types

        // Type
        public PropertyTypePartRecord GetType(int? typeId)
        {
            if (typeId > 0)
            {
                var partRecord = _contentManager.Get<PropertyTypePart>((int)typeId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyTypePartRecord GetType(string typeCssClass)
        {
            if (!String.IsNullOrEmpty(typeCssClass))
            {
                PropertyTypePart partRecord =
                    _contentManager.Query<PropertyTypePart, PropertyTypePartRecord>()
                        .Where(a => a.CssClass == typeCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyTypePartRecord> GetTypes()
        {
            return _cacheManager.Get("Types", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Types_Changed"));

                return
                    _contentManager.Query<PropertyTypePart, PropertyTypePartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        public IEnumerable<PropertyTypePartRecord> GetTypes(string adsTypeCssClass, string typeGroupCssClass)
        {
            IEnumerable<PropertyTypePartRecord> list = GetTypes().Where(a => a.Group.CssClass == typeGroupCssClass);
            if (typeGroupCssClass == "gp-house")
            {
                // Các loại BĐS khi rao bán: Đất thổ cư; Nhà phố; Biệt thự; Khách sạn; Cao ốc Văn Phòng; Kho xưởng
                if (adsTypeCssClass == "ad-selling" || adsTypeCssClass == "ad-buying")
                {
                    list = list.Where(a => _sellingPropertyTypeCssClass.Contains(a.CssClass));
                }
            }
            return list;
        }

        // TypeGroup
        public PropertyTypeGroupPartRecord GetTypeGroup(int? typeGroupId)
        {
            if (typeGroupId > 0)
            {
                var partRecord = _contentManager.Get<PropertyTypeGroupPart>((int)typeGroupId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyTypeGroupPartRecord GetTypeGroup(string typeGroupCssClass)
        {
            if (!String.IsNullOrEmpty(typeGroupCssClass))
            {
                PropertyTypeGroupPart partRecord =
                    _contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>()
                        .Where(a => a.CssClass == typeGroupCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyTypeGroupPartRecord> GetTypeGroups()
        {
            //return _cacheManager.Get("TypeGroups", ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
            //    ctx.Monitor(_signals.When("TypeGroups_Changed"));

            return
                _contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>()
                    .Where(a => a.IsEnabled)
                    .OrderBy(a => a.SeqOrder)
                    .List()
                    .Select(a => a.Record);
            //});
        }

        // TypeConstruction
        public PropertyTypeConstructionPartRecord GetTypeConstruction(int? typeConstructionId)
        {
            if (typeConstructionId > 0)
            {
                var partRecord = _contentManager.Get<PropertyTypeConstructionPart>((int)typeConstructionId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyTypeConstructionPartRecord GetTypeConstruction(string typeConstructionCssClass)
        {
            if (!String.IsNullOrEmpty(typeConstructionCssClass))
            {
                PropertyTypeConstructionPart partRecord =
                    _contentManager.Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                        .Where(a => a.CssClass == typeConstructionCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyTypeConstructionPartRecord> GetTypeConstructions()
        {
            return _cacheManager.Get("TypeConstructions", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("TypeConstructions_Changed"));

                return
                    _contentManager.Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        public IEnumerable<PropertyTypeConstructionPartRecord> GetTypeConstructions(int? propertyTypeId, double? floor)
        {
            var list = new List<PropertyTypeConstructionPartRecord>();
            PropertyTypePartRecord propertyType = GetType(propertyTypeId);
            if (propertyType != null)
            {
                IContentQuery<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord> filter =
                    _contentManager.Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                        .Where(a => a.IsEnabled)
                        .Where(a => a.PropertyType.Id == propertyTypeId);
                //var filter = GetTypeConstructions().Where(a => a.PropertyType == propertyType);
                switch (propertyType.CssClass)
                {
                    case "tp-house":
                    case "tp-concrete-house":
                    case "tp-villa":
                    case "tp-office-building":
                        if (floor > 0)
                        {
                            filter =
                                filter.Where(
                                    a =>
                                        (a.MinFloor <= floor || a.MinFloor == null) &&
                                        (a.MaxFloor >= floor || a.MaxFloor == null));
                        }
                        else if (floor <= 0 || floor == null)
                        {
                            // Nhà phố liền kề trệt
                            filter = filter.Where(a => a.MaxFloor == 0);
                        }
                        break;
                    case "tp-hotel":
                        break;
                    case "tp-warehouse-workshop":
                        break;
                }
                list = filter.OrderBy(a => a.SeqOrder).List().Select(a => a.Record).ToList();
            }
            return list;
        }

        public PropertyTypeConstructionPartRecord GetTypeConstructionDefaultInFloorsRange(int? propertyTypeId,
            double? floor)
        {
            IEnumerable<PropertyTypeConstructionPartRecord> list =
                GetTypeConstructions(propertyTypeId, floor).Where(a => a.IsDefaultInFloorsRange).ToList();
            return list.Any() ? list.FirstOrDefault() : null;
        }

        #endregion

        #region LegalStatus

        public PropertyLegalStatusPartRecord GetLegalStatus(int? legalStatusId)
        {
            if (legalStatusId > 0)
            {
                var partRecord = _contentManager.Get<PropertyLegalStatusPart>((int)legalStatusId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyLegalStatusPartRecord GetLegalStatus(string legalStatusName)
        {
            if (!String.IsNullOrEmpty(legalStatusName))
            {
                PropertyLegalStatusPart partRecord =
                    _contentManager.Query<PropertyLegalStatusPart, PropertyLegalStatusPartRecord>()
                        .Where(a => a.Name == legalStatusName)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null)
                {
                    return partRecord.Record;
                }
                partRecord =
                    _contentManager.Query<PropertyLegalStatusPart, PropertyLegalStatusPartRecord>()
                        .Where(a => a.Name == "Khác")
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyLegalStatusPartRecord> GetLegalStatus()
        {
            return _cacheManager.Get("LegalStatus", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("LegalStatus_Changed"));

                return
                    _contentManager.Query<PropertyLegalStatusPart, PropertyLegalStatusPartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region Interiors

        public PropertyInteriorPartRecord GetInterior(int? interiorId)
        {
            if (interiorId > 0)
            {
                var partRecord = _contentManager.Get<PropertyInteriorPart>((int)interiorId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyInteriorPartRecord> GetInteriors()
        {
            return _cacheManager.Get("Interiors", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Interiors_Changed"));

                return
                    _contentManager.Query<PropertyInteriorPart, PropertyInteriorPartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region Directions

        public DirectionPartRecord GetDirection(int? directionId)
        {
            if (directionId > 0)
            {
                var partRecord = _contentManager.Get<DirectionPart>((int)directionId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<DirectionPartRecord> GetDirections()
        {
            return _cacheManager.Get("Directions", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Directions_Changed"));

                return
                    _contentManager.Query<DirectionPart, DirectionPartRecord>()
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region UserGroup by Domain, PropertyGroup

        public IEnumerable<HostNamePartRecord> GetUserGroupDomains()
        {
            return _cacheManager.Get("UserGroupDomains", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("UserGroupDomains_Changed"));

                return
                    _contentManager.Query<HostNamePart, HostNamePartRecord>()
                        .Where(a => a.IsEnabled && a.Name != null)
                        .List()
                        .Select(a => a.Record);
            });
        }

        public IEnumerable<PropertyGroupPart> GetPropertyGroup()
        {
            return _cacheManager.Get("PropertyGroup", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PropertyGroup_Changed"));

                return _contentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>().List();
            });
        }

        public IEnumerable<PropertyGroupPart> GetPropertyGroup(int? groupId)
        {
            return _cacheManager.Get("PropertyGroup_" + "_" + groupId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PropertyGroup_" + "_" + groupId + "_Changed"));

                return
                    _contentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>()
                        .Where(a => a.UserGroupId == groupId)
                        .List();
            });
        }

        public IEnumerable<PropertyGroupPart> GetPropertyGroup(UserGroupPartRecord group)
        {
            return _cacheManager.Get("PropertyGroup_" + "_" + group.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PropertyGroup_" + "_" + group.Id + "_Changed"));

                return
                    _contentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>()
                        .Where(a => a.UserGroupId == group.Id)
                        .List();
            });
        }

        #endregion

        #region Locations

        public PropertyLocationPartRecord GetLocation(int? locationId)
        {
            if (locationId > 0)
            {
                var partRecord = _contentManager.Get<PropertyLocationPart>((int)locationId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyLocationPartRecord GetLocation(string locationCssClass)
        {
            if (!String.IsNullOrEmpty(locationCssClass))
            {
                PropertyLocationPart partRecord =
                    _contentManager.Query<PropertyLocationPart, PropertyLocationPartRecord>()
                        .Where(a => a.CssClass == locationCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PropertyLocationPartRecord> GetLocations()
        {
            return _cacheManager.Get("Locations", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Locations_Changed"));

                return
                    _contentManager.Query<PropertyLocationPart, PropertyLocationPartRecord>()
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region PaymentMethod, PaymentUnit, PaymentExchange

        // PaymentMethod
        public PaymentMethodPartRecord GetPaymentMethod(int? paymentMethodId)
        {
            if (paymentMethodId > 0)
            {
                var partRecord = _contentManager.Get<PaymentMethodPart>((int)paymentMethodId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PaymentMethodPartRecord GetPaymentMethod(string paymentMethodCssClass)
        {
            if (!String.IsNullOrEmpty(paymentMethodCssClass))
            {
                PaymentMethodPart partRecord =
                    _contentManager.Query<PaymentMethodPart, PaymentMethodPartRecord>()
                        .Where(a => a.CssClass == paymentMethodCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PaymentMethodPartRecord> GetPaymentMethods()
        {
            return _cacheManager.Get("PaymentMethods", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PaymentMethods_Changed"));

                return
                    _contentManager.Query<PaymentMethodPart, PaymentMethodPartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        // PaymentUnit
        public PaymentUnitPartRecord GetPaymentUnit(int? paymentUnitId)
        {
            if (paymentUnitId > 0)
            {
                var partRecord = _contentManager.Get<PaymentUnitPart>((int)paymentUnitId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PaymentUnitPartRecord GetPaymentUnit(string paymentUnitCssClass)
        {
            if (!String.IsNullOrEmpty(paymentUnitCssClass))
            {
                PaymentUnitPart partRecord =
                    _contentManager.Query<PaymentUnitPart, PaymentUnitPartRecord>()
                        .Where(a => a.CssClass == paymentUnitCssClass)
                        .List()
                        .FirstOrDefault();
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PaymentUnitPartRecord> GetPaymentUnits()
        {
            return _cacheManager.Get("PaymentUnits", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PaymentUnits_Changed"));

                return
                    _contentManager.Query<PaymentUnitPart, PaymentUnitPartRecord>()
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        // PaymentExchange
        public PaymentExchangePartRecord GetPaymentExchange(int? paymentExchangeId)
        {
            if (paymentExchangeId > 0)
            {
                var partRecord = _contentManager.Get<PaymentExchangePart>((int)paymentExchangeId);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public IEnumerable<PaymentExchangePartRecord> GetPaymentExchanges()
        {
            return _cacheManager.Get("PaymentExchanges", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PaymentExchanges_Changed"));

                return
                    _contentManager.Query<PaymentExchangePart, PaymentExchangePartRecord>()
                        .OrderByDescending(a => a.Date)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region ExchangeRate

        public double GetRateUsd()
        {
            double rate = 0;
            if (GetPaymentExchanges().Any(ex => ex.USD > 0))
                rate = GetPaymentExchanges().First(ex => ex.USD > 0).USD;
            return rate;
        }

        public double GetRateGold()
        {
            double rate = 0;
            if (GetPaymentExchanges().Any(ex => ex.SJC > 0))
                rate = GetPaymentExchanges().First(ex => ex.SJC > 0).SJC;
            return rate;
        }

        #endregion

        #region Advantages & DisAdvantages

        public void AddAdvantages(PropertyPart property, IEnumerable<PropertyAdvantagePartRecordContent> advantages)
        {
            IEnumerable<PropertyAdvantagePartRecordContent> oldAdvantages =
                _advantagesContentRepository.Fetch(r => r.PropertyPartRecord.Id == property.Id);
            Dictionary<PropertyAdvantagePartRecord, bool> lookupNew =
                advantages.Select(e => e.PropertyAdvantagePartRecord).ToDictionary(r => r, r => false);
            // Delete the advantages that are no longer there and mark the ones that should stay
            foreach (PropertyAdvantagePartRecordContent content in oldAdvantages)
            {
                if (lookupNew.ContainsKey(content.PropertyAdvantagePartRecord))
                {
                    lookupNew[content.PropertyAdvantagePartRecord] = true;
                }
                else
                {
                    _advantagesContentRepository.Delete(content);
                }
            }
            // Add the new advantages
            foreach (PropertyAdvantagePartRecord advantage in lookupNew.Where(kvp => !kvp.Value).Select(kvp => kvp.Key))
            {
                _advantagesContentRepository.Create(new PropertyAdvantagePartRecordContent
                {
                    PropertyPartRecord = property.Record,
                    PropertyAdvantagePartRecord = advantage
                });
            }
        }

        public void UpdateApartmentAdvantagesForContentItem(LocationApartmentPart apartment,
            IEnumerable<PropertyAdvantageEntry> advantages)
        {
            if (advantages != null)
            {
                IEnumerable<LocationApartmentAdvantagePartRecordContent> oldAdvantages =
                    _apartmentAdvantagesContentRepository.Fetch(
                        r =>
                            r.LocationApartmentPartRecord.Id == apartment.Id &&
                            r.PropertyAdvantagePartRecord.ShortName == "apartment-adv");
                Dictionary<PropertyAdvantagePartRecord, bool> lookupNew =
                    advantages.Where(e => e.IsChecked).Select(e => e.Advantage).ToDictionary(r => r, r => false);
                // Delete the advantages that are no longer there and mark the ones that should stay
                foreach (LocationApartmentAdvantagePartRecordContent content in oldAdvantages)
                {
                    if (lookupNew.ContainsKey(content.PropertyAdvantagePartRecord))
                    {
                        lookupNew[content.PropertyAdvantagePartRecord] = true;
                    }
                    else
                    {
                        _apartmentAdvantagesContentRepository.Delete(content);
                    }
                }
                // Add the new advantages
                foreach (
                    PropertyAdvantagePartRecord advantage in lookupNew.Where(kvp => !kvp.Value).Select(kvp => kvp.Key))
                {
                    _apartmentAdvantagesContentRepository.Create(new LocationApartmentAdvantagePartRecordContent
                    {
                        LocationApartmentPartRecord = apartment.Record,
                        PropertyAdvantagePartRecord = advantage
                    });
                }
                _apartmentAdvantagesContentRepository.Flush();
            }
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForApartment(LocationApartmentPart apartment)
        {
            var result = new List<PropertyAdvantagePartRecord>();
            IEnumerable<LocationApartmentAdvantagePartRecordContent> query =
                _apartmentAdvantagesContentRepository.Fetch(r => r.LocationApartmentPartRecord.Id == apartment.Id)
                    .ToList();
            if (query.Any()) result = query.Select(a => a.PropertyAdvantagePartRecord).ToList();
            return result;
        }

        #endregion

        #region UserProperties (BĐS lưu theo dõi)

        public bool SaveUserProperties(PropertyPart p)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (_userPropertiesContentRepository.Fetch(r => r.PropertyPartRecord == p.Record && r.UserPartRecord == user.Record).Any())
                return false;//BĐS này đã có trong danh sách theo dõi

            _userPropertiesContentRepository.Create(new PropertyUserPartRecordContent
            {
                PropertyPartRecord = p.Record,
                UserPartRecord = user.Record
            });

            return true;
        }

        public bool CheckIsSavedProperty(int propertyId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            return _userPropertiesContentRepository.Fetch(r => r.PropertyPartRecord.Id == propertyId && r.UserPartRecord == user.Record).Any();
        }

        public IEnumerable<PropertyUserPartRecordContent> GetListPropertyUserContent()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            return _userPropertiesContentRepository.Fetch(p => p.UserPartRecord.Id == user.Id);
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetListPropertyUserFrontEnd()
        {
            var listPropertiesId = GetListPropertyUserContent().Select(r => r.PropertyPartRecord.Id).ToList();

            return GetProperties().Where(p => listPropertiesId.Contains(p.Id));
        }

        public void DeleteUserProperty(int propertyId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var userProperty = _userPropertiesContentRepository.Fetch(r => r.PropertyPartRecord.Id == propertyId && r.UserPartRecord.Id == user.Id).FirstOrDefault();
            if (userProperty != null)
                _userPropertiesContentRepository.Delete(userProperty);
        }

        #endregion

        #region GetPropertyAdvantages

        // Advantages
        public IEnumerable<PropertyAdvantagePartRecord> GetAdvantages()
        {
            return GetPropertyAdvantages("adv").Where(r => r.Id != 85731);
            //Loại bỏ: Có đặc điểm (khác) làm tăng giá trị BĐS
        }

        public IList<PropertyAdvantageEntry> GetAdvantagesEntries()
        {
            return GetAdvantages().Select(x => new PropertyAdvantageEntry { Advantage = x }).ToList();
        }

        // DisAdvantages
        public IEnumerable<PropertyAdvantagePartRecord> GetDisAdvantages()
        {
            return GetPropertyAdvantages("disadv").Where(r => r.Id != 85795).ToList();
            //Loại bỏ: Nằm trong khu phức tạp (khu vực xấu);
        }

        public IList<PropertyAdvantageEntry> GetDisAdvantagesEntries()
        {
            return GetDisAdvantages().Select(x => new PropertyAdvantageEntry { Advantage = x }).ToList();
        }

        // Apartment Advantages
        public IEnumerable<PropertyAdvantagePartRecord> GetApartmentAdvantages()
        {
            return GetPropertyAdvantages("apartment-adv");
        }

        public IList<PropertyAdvantageEntry> GetApartmentAdvantagesEntries()
        {
            return GetApartmentAdvantages().Select(x => new PropertyAdvantageEntry { Advantage = x }).ToList();
        }

        // Apartment Interior Advantages
        public IEnumerable<PropertyAdvantagePartRecord> GetApartmentInteriorAdvantages()
        {
            return GetPropertyAdvantages("apartment-interior-adv");
        }

        public IList<PropertyAdvantageEntry> GetApartmentInteriorAdvantagesEntries()
        {
            return GetApartmentInteriorAdvantages().Select(x => new PropertyAdvantageEntry { Advantage = x }).ToList();
        }

        // Construction Advantages
        public IEnumerable<PropertyAdvantagePartRecord> GetConstructionAdvantages()
        {
            return GetPropertyAdvantages("construction-adv");
        }

        public IList<PropertyAdvantageEntry> GetConstructionAdvantagesEntries()
        {
            return GetConstructionAdvantages().Select(x => new PropertyAdvantageEntry { Advantage = x }).ToList();
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyAdvantages(String advShortName)
        {
            return _cacheManager.Get("PropertyAdvantages-" + advShortName, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Advantages_Changed"));

                return
                    _contentManager.Query<PropertyAdvantagePart, PropertyAdvantagePartRecord>()
                        .Where(a => a.IsEnabled && a.ShortName == advShortName)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region GetAdvantagesForProperty

        //public IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForProperty(PropertyPart p)
        //{
        //    return _cacheManager.Get("PropertyAdvantages_" + p.Id, ctx =>
        //    {
        //        ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
        //        ctx.Monitor(_signals.When("PropertyAdvantages_Changed_" + p.Id));

        //        var result = new List<PropertyAdvantagePartRecord>();
        //        IEnumerable<PropertyAdvantagePartRecordContent> query =
        //            _advantagesContentRepository.Fetch(r => r.PropertyPartRecord == p.Record).ToList();
        //        if (query.Any()) result = query.Select(a => a.PropertyAdvantagePartRecord).ToList();
        //        return result;
        //    });
        //}

        public List<PropertyAdvantageItem> GetAdvantagesForProperty(PropertyPart p)
        {
            return _cacheManager.Get("PropertyAdvantages_" + p.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PropertyAdvantages_Changed_" + p.Id));

                var result = new List<PropertyAdvantageItem>();
                IEnumerable<PropertyAdvantagePartRecordContent> query =
                    _advantagesContentRepository.Fetch(r => r.PropertyPartRecord == p.Record).ToList();
                if (query.Any())
                    result = query.Select(a => new PropertyAdvantageItem
                    {
                        Name = a.PropertyAdvantagePartRecord.Name,
                        CssClass = a.PropertyAdvantagePartRecord.CssClass,
                        ShortName = a.PropertyAdvantagePartRecord.ShortName
                    }).ToList();

                return result;
            });
        }


        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyAdvantages(PropertyPart p)
        {
            // p.Advantages.Select(r => r.Id).ToList();
            return GetAdvantagesForProperty(p, "adv");
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyDisAdvantages(PropertyPart p)
        {
            // p.DisAdvantages.Select(r => r.Id).ToList();
            return GetAdvantagesForProperty(p, "disadv");
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyApartmentAdvantages(PropertyPart p)
        {
            // p.ApartmentAdvantages.Select(r => r.Id).ToList();
            return GetAdvantagesForProperty(p, "apartment-adv");
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyApartmentInteriorAdvantages(PropertyPart p)
        {
            // p.ApartmentInteriorAdvantages.Select(r => r.Id).ToList();
            return GetAdvantagesForProperty(p, "apartment-interior-adv");
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyConstructionAdvantages(PropertyPart p)
        {

            // p.ConstructionAdvantages.Select(r => r.Id).ToList();

            return GetAdvantagesForProperty(p, "construction-adv");
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForProperty(PropertyPart p, String advShortName)
        {
            var result = new List<PropertyAdvantagePartRecord>();
            IEnumerable<PropertyAdvantagePartRecordContent> query =
                _advantagesContentRepository.Fetch(
                    r => r.PropertyPartRecord == p.Record && r.PropertyAdvantagePartRecord.ShortName == advShortName)
                    .ToList();
            if (query.Any()) result = query.Select(a => a.PropertyAdvantagePartRecord).ToList();
            return result;
        }

        #endregion

        #region GetAdvantagesForLocationApartment

        public IEnumerable<PropertyAdvantagePartRecord> GetPropertyApartmentAdvantages(LocationApartmentPart p)
        {
            return GetAdvantagesForLocationApartment(p, "apartment-adv");
        }

        public IEnumerable<PropertyAdvantagePartRecord> GetAdvantagesForLocationApartment(LocationApartmentPart p,
            String advShortName)
        {
            var result = new List<PropertyAdvantagePartRecord>();
            IEnumerable<LocationApartmentAdvantagePartRecordContent> query =
                _apartmentAdvantagesContentRepository.Fetch(
                    r =>
                        r.LocationApartmentPartRecord == p.Record &&
                        r.PropertyAdvantagePartRecord.ShortName == advShortName).ToList();
            if (query.Any()) result = query.Select(a => a.PropertyAdvantagePartRecord).ToList();
            return result;
        }

        public string GetApartmentBlockInfo(PropertyPart p)
        {
            if (p != null)
            {
                var part = _contentManager.Query<ApartmentBlockInfoPart, ApartmentBlockInfoPartRecord>().Where(a => a.ApartmentBlock == p.ApartmentBlock).List().FirstOrDefault();
                if (part != null) return part.OrtherContent;
            }
            return "";
        }
        #endregion

        #region UpdateAdvantagesForProperty

        public void UpdatePropertyAdvantages(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages)
        {
            UpdateAdvantagesForProperty(property, advantages, "adv");
        }

        public void UpdatePropertyDisAdvantages(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages)
        {
            UpdateAdvantagesForProperty(property, advantages, "disadv");
        }

        public void UpdatePropertyApartmentAdvantages(PropertyPart property,
            IEnumerable<PropertyAdvantageEntry> advantages)
        {
            UpdateAdvantagesForProperty(property, advantages, "apartment-adv");
        }

        public void UpdatePropertyApartmentInteriorAdvantages(PropertyPart property,
            IEnumerable<PropertyAdvantageEntry> advantages)
        {
            UpdateAdvantagesForProperty(property, advantages, "apartment-interior-adv");
        }

        public void UpdatePropertyConstructionAdvantages(PropertyPart property,
            IEnumerable<PropertyAdvantageEntry> advantages)
        {
            UpdateAdvantagesForProperty(property, advantages, "construction-adv");
        }

        public void UpdateAdvantagesForProperty(PropertyPart property, IEnumerable<PropertyAdvantageEntry> advantages,
            String advShortName)
        {
            if (advantages != null)
            {
                PropertyPartRecord record = property.Record;
                IEnumerable<PropertyAdvantagePartRecordContent> oldAdvantages =
                    _advantagesContentRepository.Fetch(
                        r => r.PropertyPartRecord == record && r.PropertyAdvantagePartRecord.ShortName == advShortName);
                Dictionary<PropertyAdvantagePartRecord, bool> lookupNew =
                    advantages.Where(e => e.IsChecked).Select(e => e.Advantage).ToDictionary(r => r, r => false);
                // Delete the advantages that are no longer there and mark the ones that should stay
                foreach (PropertyAdvantagePartRecordContent content in oldAdvantages)
                {
                    if (lookupNew.ContainsKey(content.PropertyAdvantagePartRecord))
                    {
                        lookupNew[content.PropertyAdvantagePartRecord] = true;
                    }
                    else
                    {
                        _advantagesContentRepository.Delete(content);
                    }
                }
                // Add the new advantages
                foreach (
                    PropertyAdvantagePartRecord advantage in lookupNew.Where(kvp => !kvp.Value).Select(kvp => kvp.Key))
                {
                    _advantagesContentRepository.Create(new PropertyAdvantagePartRecordContent
                    {
                        PropertyPartRecord = record,
                        PropertyAdvantagePartRecord = advantage
                    });
                }
                _advantagesContentRepository.Flush();
            }
        }

        #endregion

        #endregion

        #region Property

        public PropertyPartRecord GetProperty(int? id)
        {
            if (id > 0)
            {
                var partRecord = _contentManager.Get<PropertyPart>((int)id);
                if (partRecord != null) return partRecord.Record;
            }
            return null;
        }

        public PropertyFilePart GetPropertyFile(int propertyId, string filePath)
        {
            return
                _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(
                        a => a.PropertyPartRecord != null && a.PropertyPartRecord.Id == propertyId && a.Path == filePath)
                    .List()
                    .FirstOrDefault();
        }

        public IEnumerable<PropertyFilePart> GetPropertyFiles(PropertyPart p)
        {
            return _cacheManager.Get("PropertyFiles_" + p.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("PropertyFiles_Changed_" + p.Id));

                return _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(a => a.IsDeleted != true && a.PropertyPartRecord != null && a.PropertyPartRecord == p.Record)
                    .List();
            });
        }

        public IEnumerable<PropertyFilePart> GetApartmentBlockInfoFiles(ApartmentBlockInfoPart p)
        {
            return _cacheManager.Get("ApartmentBlockInfoFiles_" + p.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("ApartmentBlockInfoFiles_Changed_" + p.Id));

                return _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(a => a.IsDeleted != true && a.ApartmentBlockInfoPartRecord != null && a.ApartmentBlockInfoPartRecord == p.Record)
                    .List();
            });
        }
        public void ClearCacheGetApartmentBlockInfoFiles(ApartmentBlockInfoPart p)
        {
            _signals.Trigger("ApartmentBlockInfoFiles_Changed_" + p.Id);
        }


        public IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetPropertyApartmentFiles(PropertyPart p)
        {
            return
                _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(
                        a =>
                            a.IsDeleted != true && a.LocationApartmentPartRecord != null &&
                            a.LocationApartmentPartRecord == p.Apartment);
        }

        public IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetAllPropertyFiles(PropertyPart p)
        {
            return _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                .Where(a => a.IsDeleted != true &&
                            ((a.PropertyPartRecord != null && a.PropertyPartRecord == p.Record) ||
                             (a.LocationApartmentPartRecord != null && a.LocationApartmentPartRecord == p.Apartment) ||
                             (a.ApartmentBlockInfoPartRecord != null && a.ApartmentBlockInfoPartRecord == p.ApartmentBlockInfoPartRecord))
                );
        }

        public IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetAllLocationApartmentFiles(
            LocationApartmentPart p)
        {
            return _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                .Where(
                    a =>
                        a.IsDeleted != true && a.LocationApartmentPartRecord != null &&
                        a.LocationApartmentPartRecord == p.Record);
        }

        public void UploadImages(IEnumerable<HttpPostedFileBase> files, PropertyPart p, bool isPublished)
        {
            var createdUser = Services.WorkContext.CurrentUser.As<UserPart>();

            // Verify that the user selected a file
            IList<HttpPostedFileBase> httpPostedFileBases = files as IList<HttpPostedFileBase> ?? files.ToList();
            if (files != null && httpPostedFileBases.Any())
            {
                foreach (HttpPostedFileBase file in httpPostedFileBases)
                {
                    UploadPropertyImage(file, p, createdUser, isPublished);
                }
            }
        }

        public PropertyFilePart UploadPropertyImage(HttpPostedFileBase fileBase, PropertyPart p, UserPart createdUser,
            bool isPublished)
        {
            var pFile = Services.ContentManager.New<PropertyFilePart>("PropertyFile");

            if (fileBase != null && fileBase.ContentLength > 0)
            {
                DateTime createdDate = DateTime.Now;

                var fileLocation = fileBase.SaveAsUserFiles(p.Id);

                // Image Record
                pFile.CreatedDate = createdDate;
                pFile.CreatedUser = createdUser.Record;
                pFile.Property = p.Record;
                pFile.Name = fileBase.FileName;
                pFile.Type = "image";
                pFile.Size = fileBase.ContentLength;
                pFile.Path = fileLocation;
                pFile.Published = isPublished;

                Services.ContentManager.Create(pFile);

                // Revision Record
                var rev = Services.ContentManager.New<RevisionPart>("Revision");
                rev.CreatedDate = createdDate;
                rev.CreatedUser = createdUser.Record;
                rev.FieldName = "Add Image";
                rev.ValueBefore = pFile.Name;
                rev.ValueAfter = pFile.Path;
                rev.ContentType = "Property";
                rev.ContentTypeRecordId = p.Id;

                Services.ContentManager.Create(rev);

                // UserAction Record
                _revisionService.SaveUserActivityUploadPropertyImages(createdDate, createdUser, p);

                //Clear cache UploadFile
                _signals.Trigger("PropertyFiles_Changed_" + p.Id);

                Services.Notifier.Information(T("File {0} uploaded successfully.", fileBase.FileName));
            }

            return pFile;
        }

        public void UploadImagesForBlockInfo(IEnumerable<HttpPostedFileBase> files, ApartmentBlockInfoPart p, bool isPublished)
        {
            var createdUser = Services.WorkContext.CurrentUser.As<UserPart>();

            // Verify that the user selected a file
            IList<HttpPostedFileBase> httpPostedFileBases = files as IList<HttpPostedFileBase> ?? files.ToList();
            if (files != null && httpPostedFileBases.Any())
            {
                foreach (HttpPostedFileBase file in httpPostedFileBases)
                {
                    UploadPropertyImageForBlockInfo(file, p, createdUser, isPublished);
                }
            }
        }

        public PropertyFilePart UploadPropertyImageForBlockInfo(HttpPostedFileBase fileBase, ApartmentBlockInfoPart p, UserPart createdUser, bool isPublished)
        {
            var pFile = Services.ContentManager.New<PropertyFilePart>("PropertyFile");

            if (fileBase != null && fileBase.ContentLength > 0)
            {
                DateTime createdDate = DateTime.Now;

                var fileLocation = fileBase.SaveAsUserFiles(p.Id);

                // Image Record
                pFile.CreatedDate = createdDate;
                pFile.CreatedUser = createdUser.Record;
                pFile.Property = null;
                pFile.ApartmentBlockInfoPartRecord = p.Record;
                pFile.Name = fileBase.FileName;
                pFile.Type = "image";
                pFile.Size = fileBase.ContentLength;
                pFile.Path = fileLocation;
                pFile.Published = isPublished;

                Services.ContentManager.Create(pFile);

                //Clear cache UploadFile
                ClearCacheGetApartmentBlockInfoFiles(p);

                Services.Notifier.Information(T("File {0} uploaded successfully.", fileBase.FileName));
            }

            return pFile;
        }

        public string UploadAvatarBlockInfo(HttpPostedFileBase fileBase, ApartmentBlockInfoPart blockInfo)
        {
            if (fileBase != null && fileBase.ContentLength > 0)
            {
                var fileLocation = fileBase.SaveAsUserAvatar(blockInfo.Id);
                return fileLocation;
            }
            return null;
        }

        public void ResizeAllImages()
        {
            IEnumerable<PropertyFilePart> files =
                Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(a => a.Size > 250000)
                    .List().ToList();
            foreach (
                ImageJob i in
                    files.Select(filePart => "~" + filePart.Path)
                        .Select(
                            fileLocation =>
                                new ImageJob(fileLocation, fileLocation,
                                    new Instructions("maxwidth=1024;format=jpg;mode=max"))))
            {
                i.CreateParentDirectory = true; //Auto-create the uploads directory.
                i.Build();
            }
            Services.Notifier.Information(T("Resize {0} file(s) successfully.", files.Count()));
        }

        // Default Property Images
        public List<String> GetTypeDefaultImageUrls(PropertyTypePartRecord type)
        {
            return _cacheManager.Get("TypeDefaultImages" + type.CssClass, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Types_Changed"));

                var imgUrl = new List<string>();
                string defaultPath =
                    type.DefaultImgUrl.Substring(0, type.DefaultImgUrl.IndexOf(".", StringComparison.Ordinal)) + "/";

                var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/" + defaultPath));
                if (folder.Exists)
                {
                    string[] files = Directory.GetFiles(folder.FullName, "*.jpg");
                    imgUrl.AddRange(
                        files.Select(filePath => new FileInfo(filePath)).Select(file => defaultPath + file.Name));
                }

                return imgUrl;
            });
        }

        public List<String> GetTypeDefaultImageUrls(PropertyTypePartRecord type, double floors)
        {
            return _cacheManager.Get("TypeDefaultImages" + type.CssClass + "_floors" + floors, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Types_Changed"));

                var imgUrl = new List<string>();
                string defaultPath =
                    type.DefaultImgUrl.Substring(0, type.DefaultImgUrl.IndexOf(".", StringComparison.Ordinal)) + "/";

                if (floors <= 0)
                {
                    imgUrl.Add(GetType("tp-lv4-house").DefaultImgUrl);
                }
                else
                {
                    double floorRounded = Math.Round(floors) > 2 ? 3 : Math.Round(floors);
                    defaultPath = defaultPath + floorRounded + "/";

                    var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/" + defaultPath));
                    if (folder.Exists)
                    {
                        string[] files = Directory.GetFiles(folder.FullName, "*.jpg");
                        imgUrl.AddRange(
                            files.Select(filePath => new FileInfo(filePath)).Select(file => defaultPath + file.Name));
                    }
                }

                return imgUrl;
            });
        }

        public String GetTypeRandomImageUrl(PropertyTypePartRecord type, double floors)
        {
            String imgUrl = "";
            List<String> imageUrls = type.CssClass == "tp-concrete-house"
                ? GetTypeDefaultImageUrls(type, floors)
                : GetTypeDefaultImageUrls(type);
            if (imageUrls.Count > 0)
            {
                imgUrl = imageUrls[RandomNumber(imageUrls.Count)];
            }
            return imgUrl;
        }

        public String GetAvatarImageUrl(PropertyPart p)
        {
            string imgUrl = "";
            IEnumerable<PropertyFilePart> propertyFiles = GetAllPropertyFiles(p).Where(r => r.Published).List().ToList();

            if (propertyFiles.Any())
            {
                imgUrl = propertyFiles.Any(a => a.IsAvatar)
                    ? propertyFiles.First(a => a.IsAvatar).Path
                    : propertyFiles.First().Path;
            }
            return imgUrl;
        }

        public String GetDefaultImageUrl(PropertyPart p)
        {
            string imgUrl = GetAvatarImageUrl(p);

            // check if image is exists
            var imgFile = new FileInfo(HttpContext.Current.Server.MapPath("~/" + imgUrl));
            if (!imgFile.Exists) imgUrl = "";

            if (String.IsNullOrEmpty(imgUrl))
            {
                if (p.Type != null)
                {
                    // get Random image form PropertyType images
                    imgUrl = GetTypeRandomImageUrl(p.Type, p.Floors ?? 0);

                    // get Default image of PropertyType
                    if (String.IsNullOrEmpty(imgUrl))
                        imgUrl = p.Type.DefaultImgUrl;
                }
            }
            return imgUrl;
        }

        public List<String> GetLocationApartmentDefaultImageUrl()
        {
            return _cacheManager.Get("LocationDefaultImages", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("LocationDefaultImages_Changed"));

                var imgUrl = new List<string>();
                const string defaultPath = "/Themes/Bootstrap/styles/images/location-apartment-default-images/";

                var folder = new DirectoryInfo(HttpContext.Current.Server.MapPath("~" + defaultPath));
                if (folder.Exists)
                {
                    string[] files = Directory.GetFiles(folder.FullName, "*.jpg");
                    imgUrl.AddRange(
                        files.Select(filePath => new FileInfo(filePath)).Select(file => defaultPath + file.Name));
                }

                return imgUrl;
            });
        }

        #endregion

        #region Property AdsVIP

        public DateTime? GetAddExpirationDate(ExpirationDate selectedOption, DateTime? expirationDate)
        {
            double daysToAdd;
            switch (selectedOption)
            {
                case ExpirationDate.None:
                    daysToAdd = 0;
                    break;

                case ExpirationDate.Day10:
                    daysToAdd = 10;
                    break;

                case ExpirationDate.Day20:
                    daysToAdd = 20;
                    break;

                case ExpirationDate.Day30:
                    daysToAdd = 30;
                    break;

                case ExpirationDate.Day60:
                    daysToAdd = 60;
                    break;

                case ExpirationDate.Day90:
                    daysToAdd = 90;
                    break;

                case ExpirationDate.Week1:
                    daysToAdd = 7;
                    break;
                case ExpirationDate.Week2:
                    daysToAdd = 7 * 2;
                    break;
                case ExpirationDate.Week3:
                    daysToAdd = 7 * 3;
                    break;
                case ExpirationDate.Week4:
                    daysToAdd = 7 * 4;
                    break;

                case ExpirationDate.Month1:
                    daysToAdd = (DateTime.Now.AddMonths(1) - DateTime.Now).TotalDays;
                    break;
                case ExpirationDate.Month2:
                    daysToAdd = (DateTime.Now.AddMonths(2) - DateTime.Now).TotalDays;
                    break;
                case ExpirationDate.Month3:
                    daysToAdd = (DateTime.Now.AddMonths(3) - DateTime.Now).TotalDays;
                    break;
                default:
                    daysToAdd = 0;
                    break;
            }

            if (daysToAdd > 0)
            {
                DateTime newExpirationDate;
                if (expirationDate.HasValue && expirationDate > DateTime.Now)
                {
                    newExpirationDate = expirationDate.Value.AddDays(daysToAdd);
                }
                else
                {
                    newExpirationDate = DateTime.Now.AddDays(daysToAdd);
                }
                return newExpirationDate;
            }

            return expirationDate;
        }

        #endregion

        #region Location Apartment Image Default

        //
        public String GetAvatarImageUrl(LocationApartmentPart p)
        {
            string imgUrl = "";
            IEnumerable<PropertyFilePart> apartmentFiles =
                GetAllLocationApartmentFiles(p).Where(r => r.Published).List().ToList();

            if (apartmentFiles.Any())
            {
                imgUrl = apartmentFiles.Any(a => a.IsAvatar)
                    ? apartmentFiles.First(a => a.IsAvatar).Path
                    : apartmentFiles.First().Path;
            }
            return imgUrl;
        }

        //
        public String GetDefaultImageUrl(LocationApartmentPart p)
        {
            string imgUrl = GetAvatarImageUrl(p);

            // check if image is exists
            var imgFile = new FileInfo(HttpContext.Current.Server.MapPath("~/" + imgUrl));
            if (!imgFile.Exists) imgUrl = "";

            if (String.IsNullOrEmpty(imgUrl))
            {
                // get Random image Location Apartment images
                List<String> imageUrls = GetLocationApartmentDefaultImageUrl();
                if (imageUrls.Count > 0)
                {
                    imgUrl = imageUrls[RandomNumber(imageUrls.Count)];
                }
                return imgUrl;
            }
            return imgUrl;
        }

        #endregion

        #region Property FrontEnd

        public IContentQuery<PropertyPart, PropertyPartRecord> GetFrontEndProperties()
        {
            var statusSelling = GetStatus("st-selling");
            var statusNew = GetStatus("st-new");
            var statusApproved = GetStatus("st-approved");

            // -- Đang rao bán -- (RAO BÁN, KH RAO BÁN, CHƯA HOÀN CHỈNH) (Đủ thông tin - Đã duyệt - Chưa hoàn chỉnh)
            IContentQuery<PropertyPart, PropertyPartRecord> pList = Services.ContentManager
                .Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Published == true)
                .Where(p => p.Status == statusSelling || p.Status == statusNew || (p.Status == statusApproved && p.AdsExpirationDate > DateTime.Now));

            return pList;
        }

        /// <summary>
        /// Lấy BĐS theo Domain và Group Config
        /// </summary>
        /// <returns></returns>
        public IContentQuery<PropertyPart, PropertyPartRecord> GetFrontEndDomainProperties()
        {
            var pList = GetFrontEndProperties();

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            //var gpList = GetPropertyGroup(currentDomainGroup);

            if (currentDomainGroup.ApproveAllGroup)
            {
                // Cho phép hiện tất cả BĐS từ group khác
                // => lấy tất cả BĐS Published ngoại trừ các BĐS không cho phép hiện (Black List)
                List<int> groupExistIds = GetPropertyGroup(currentDomainGroup).Where(a => a.IsApproved == false).Select(a => a.PropertyId).ToList(); // Danh sách BĐS từ group khác nhưng không cho phép hiện
                pList.Where(p => !groupExistIds.Contains(p.Id));
                //pList.Where(p => 
                //    !_contentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>()
                //        .Where(a => a.UserGroupId == currentDomainGroup.Id && a.PropertyId == p.Id && a.IsApproved == false).List().Any());

            }
            else
            {
                // Chỉ lấy các BĐS đã duyệt từ group khác
                // => lấy tất cả BĐS của group hiện tại + BĐS đã duyệt từ group khác (White List)
                List<int> groupExistIds = GetPropertyGroup(currentDomainGroup).Where(a => a.IsApproved == true).Select(a => a.PropertyId).ToList(); // Danh sách BĐS từ group khác đã được duyệt
                pList.Where(p => p.UserGroup == currentDomainGroup || groupExistIds.Contains(p.Id));
            }

            return pList;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetProperties()
        {
            var pList = GetFrontEndDomainProperties();

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            //List<int> preferences = new List<int> { currentDomainGroup.Id };

            //pList.OrderByDescending(item => preferences.IndexOf(item.UserGroup.Id));

            //pList.OrderByDescending(item => item.UserGroup != null);

            pList.OrderBy(item => item.UserGroup != null && item.UserGroup.Id == currentDomainGroup.Id);

            //// Sorting
            //switch (currentDomainGroup.Id)
            //{
            //    case 742406:
            //        pList.OrderByDescending(r => r.OrderByGroupPhuoc);
            //        break;

            //    case 611671:
            //        pList.OrderByDescending(r => r.OrderByGroupDatPho);
            //        break;

            //    case 749499:
            //        pList.OrderByDescending(r => r.OrderByGroupNghia);
            //        break;

            //    case 880256:
            //        pList.OrderByDescending(r => r.OrderByGroupCLBHN);
            //        break;

            //    case 816826://768363
            //        pList.OrderByDescending(r => r.OrderByGroupDuLieuNhaDat);
            //        break;

            //    default:
            //        pList.OrderByDescending(r => r.OrderByGroupDinhGiaNhaDat);
            //        break;
            //}

            pList.OrderByDescending(a => a.SeqOrder)
                .OrderByDescending(a => a.AdsVIP)
                .OrderByDescending(a => a.LastUpdatedDate)
              ;

            return pList;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetIsAuctionProperties()
        {
            var pList = GetFrontEndProperties().Where(p => p.IsAuction);

            return pList.OrderByDescending(a => a.LastUpdatedDate);
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesNewest()
        {
            var pList = GetFrontEndDomainProperties();

            pList.OrderByDescending(a => a.LastUpdatedDate);

            return pList;
        }

        #region Update Order by Domain

        public void UpdateOrderByDomainGroup(PropertyPart p, int userGroupId)
        {
            switch (userGroupId)
            {
                case 742406:
                    p.OrderByGroupPhuoc = true;
                    break;

                case 611671:
                    p.OrderByGroupDatPho = true;
                    break;

                case 749499:
                    p.OrderByGroupNghia = true;
                    break;

                case 880256:
                    p.OrderByGroupCLBHN = true;
                    break;

                case 816826://768363
                    p.OrderByGroupDuLieuNhaDat = true;
                    break;
                default:
                    p.OrderByGroupDinhGiaNhaDat = true;
                    break;
            }
        }

        #endregion

        #endregion

        #region PropertyExchange

        public bool IsEditableProperty(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null)
                return false;

            //if (p.Status.CssClass != "st-pending" && p.Status.CssClass != "st-draft")
            //return false;

            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            return p.CreatedUser == currentUser.Record;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">PropertyExchangeId</param>
        /// <param name="pId">PropertyId</param>
        /// <returns>True/False</returns>
        public bool IsEditablePropertyExchange(int id, int pId)
        {
            var p = GetExchangePartRecord(id);

            if (p == null) return false;
            //var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            return false;//p.User == currentUser.Record && IsEditableProperty(pId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pId">PropertyId</param>
        /// <returns>True/False</returns>
        public bool IsEditablePropertyExchange(int pId)
        {
            var propertyExchange = GetExchangePartRecordByPropertyId(pId);

            if (propertyExchange == null) return false;

            return true;
        }

        public int PropertyExchangeCount()
        {
            return GetListPropertyExchangeQueryFrontEnd().Count();
        }

        public bool IsPropertyExchange(int propertyId)
        {
            return _propertyExchangeRepository.Fetch(r => r.Property != null && r.Property.Id == propertyId).Any();
        }

        public PropertyExchangePartRecord GetExchangePartRecord(PropertyPart p)
        {
            return _propertyExchangeRepository.Fetch(r => r.Property == p.Record).FirstOrDefault();
        }
        public PropertyExchangePartRecord GetExchangePartRecord(CustomerPart c)
        {
            return _propertyExchangeRepository.Fetch(r => r.Customer != null && r.Customer == c.Record).FirstOrDefault();
        }

        public PropertyExchangePartRecord GetExchangePartRecordByPropertyId(int propertyId)
        {
            return _propertyExchangeRepository.Fetch(r => r.Property.Id == propertyId).FirstOrDefault();
        }

        public PropertyExchangePartRecord GetExchangePartRecord(int id)
        {
            return _propertyExchangeRepository.Fetch(r => r.Id == id).FirstOrDefault();
        }

        public List<SelectListItem> GetExchangeTypeParts()
        {
            return _cacheManager.Get("ExchangeTypes", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("ExchangeTypes_Changed"));

                return _contentManager.Query<ExchangeTypePart, ExchangeTypePartRecord>().List().Select(r => new SelectListItem
                {
                    Value = r.CssClass.ToString(CultureInfo.InvariantCulture),
                    Text = r.Name
                }).ToList();
            });
        }

        public ExchangeTypePart GetExchangeType(int id)
        {
            return _contentManager.Get<ExchangeTypePart>(id);
        }

        public ExchangeTypePart GetExchangeType(string cssClass)
        {
            return _contentManager.Query<ExchangeTypePart, ExchangeTypePartRecord>().Where(r => r.CssClass == cssClass.ToLower()).Slice(1).FirstOrDefault();
        }

        public IEnumerable<PropertyExchangePartRecord> GetListPropertyExchange()
        {
            var userCurrent = Services.WorkContext.CurrentUser.As<UserPart>();
            return _cacheManager.Get("ListPropertyExchangeByUser_" + userCurrent.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("ListPropertyExchangeByUser_" + userCurrent.Id + "_Changed"));

                return _propertyExchangeRepository.Fetch(r => r.User == userCurrent.Record);
            });
        }


        public IContentQuery<PropertyPart, PropertyPartRecord> GetListPropertyExchangeQueryFrontEnd()
        {
            var listPropertiesId = ListPropertyIdsExchange();

            var pList = GetFrontEndDomainProperties().Where(p => listPropertiesId.Contains(p.Id));

            return pList;
        }

        public List<int> ListPropertyIdsExchange()
        {
            return _cacheManager.Get("ListPropertyIdsExchange", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("ListPropertyIdsExchange_Changed"));

                return _propertyExchangeRepository.Fetch(r => r.Property != null).Select(r => r.Property.Id).ToList();
            });
        }

        public List<int> ListOwnPropertyIdsExchange()
        {
            return GetListPropertyExchange().Select(r => r.Property.Id).ToList();
        }

        public List<int> ListOwnCustomerIdsExchange()
        {
            return GetListPropertyExchange().Where(r => r.Customer != null).Select(r => r.Customer.Id).ToList();
        }

        public void ClearCachePropertyExchange()
        {
            var userCurrent = Services.WorkContext.CurrentUser.As<UserPart>();
            _signals.Trigger("ListPropertyExchangeByUser_" + userCurrent.Id + "_Changed");
            _signals.Trigger("ListPropertyIdsExchange_Changed");
        }

        #endregion

        #region Location Apartment

        public IEnumerable<LocationApartmentPartRecord> GetListApartmentsByProperty()
        {
            var statusCssClass = new List<string> { "st-trash", "st-deleted" };
            List<int> statusIds = GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord belongGroup = _groupService.GetBelongGroup(user.Id);


            return _cacheManager.Get("ListApartmentsByProperty_" + belongGroup.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("ListApartmentsByProperty_" + belongGroup.Id + "_Changed"));


                if (belongGroup.Id == 103993)//Default Group DPH
                {
                    var list = _contentManager.Query<PropertyPart, PropertyPartRecord>()
                   .Where(r => !statusIds.Contains(r.Status.Id) && r.ApartmentBlock != null)
                   .List().Select(r => r.Apartment);

                    list = list.Distinct();

                    return list;
                }
                else
                {
                    var list = _contentManager.Query<PropertyPart, PropertyPartRecord>()
                   .Where(r => r.UserGroup == belongGroup && !statusIds.Contains(r.Status.Id) && r.ApartmentBlock != null)
                   .List().Select(r => r.Apartment);

                    list = list.Distinct();

                    return list;
                }
            });
        }

        public void ClearCacheApartmentsByProperty()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord belongGroup = _groupService.GetBelongGroup(user.Id);

            _signals.Trigger("ListApartmentsByProperty_" + belongGroup.Id + "_Changed");
        }

        #endregion

        #region Mass-Update Properties

        public void UpdateNegotiateStatus()
        {
            PropertyStatusPartRecord negotiateStatus = GetStatus("st-negotiate");
            PropertyStatusPartRecord sellingStatus = GetStatus("st-selling");
            DateTime dateToUpdateNegotiateStatus =
                DateTime.Now.AddDays(-(int.Parse(_settingService.GetSetting("DaysToUpdateNegotiateStatus") ?? "7")));

            IEnumerable<PropertyPart> properties = Services.ContentManager
                .Query<PropertyPart, PropertyPartRecord>()
                .Where(a => a.Status == negotiateStatus &&
                            (
                                (a.StatusChangedDate != null && a.StatusChangedDate < dateToUpdateNegotiateStatus)
                                || a.LastUpdatedDate < dateToUpdateNegotiateStatus
                                )
                ).List();

            foreach (PropertyPart item in properties)
            {
                item.Status = sellingStatus;
                item.StatusChangedDate = DateTime.Now;
            }
        }

        public void UpdateMetaDescriptionKeywords()
        {
            IEnumerable<PropertyPart> pList = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().List();
            foreach (PropertyPart item in pList)
            {
                UpdateMetaDescriptionKeywords(item, false);
            }
        }

        public void UpdateMetaDescriptionKeywords(PropertyPart p, bool overwrite)
        {
            var pMeta = p.As<MetaPart>();
            if (overwrite || String.IsNullOrEmpty(pMeta.Keywords))
            {
                pMeta.Keywords = p.DisplayForTitle;
            }
            if (overwrite || String.IsNullOrEmpty(pMeta.Description))
            {
                pMeta.Description = !String.IsNullOrEmpty(p.Content)
                    ? WebUtility.HtmlDecode(p.Content).StripHtml()
                    : p.DisplayForAreaConstructionLocationInfo.StripHtml();
            }
        }

        public void TransferPropertyTypeConstruction()
        {
            IEnumerable<PropertyPart> pList = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().List();
            foreach (PropertyPart item in pList)
            {
                TransferPropertyTypeConstruction(item);
            }
        }

        public void TransferPropertyTypeConstruction(PropertyPart p)
        {
            if (p.TypeGroup != null && p.TypeGroup.CssClass == "gp-house")
            {
                if (p.Type != null)
                {
                    PropertyTypePartRecord typeHouse = GetType("tp-concrete-house");

                    switch (p.Type.CssClass)
                    {
                        case "tp-broken-house":
                            // chuyển thành Nhà phố liền kề trệt; Cột gỗ; mái lá hoặc giấy dầu; vách tôn + gỗ; nền láng xi măng.
                            p.Type = typeHouse;
                            p.TypeConstruction =
                                GetTypeConstructions(typeHouse.Id, 0).FirstOrDefault(a => a.Name.Contains(
                                    "Nhà phố liền kề trệt; Cột gỗ; mái lá hoặc giấy dầu; vách tôn + gỗ; nền láng xi măng."));
                            break;
                        case "tp-lv4-house ":
                            // chuyển thành Nhà phố liền kề trệt Default
                            p.Type = typeHouse;
                            p.TypeConstruction = GetTypeConstructionDefaultInFloorsRange(typeHouse.Id, 0);
                            break;
                        case "tp-wooden-house":
                            // chuyển thành Nhà phố liền kề ≤ 4 tầng; Cột gỗ; sàn gỗ; mái lợp tôn có trần; vách ván; nền lát gạch ceramic hoặc tương đương.
                            p.Type = typeHouse;
                            p.TypeConstruction =
                                GetTypeConstructions(typeHouse.Id, 2).FirstOrDefault(a => a.Name.Contains(
                                    "Nhà phố liền kề ≤ 4 tầng; Cột gỗ; sàn gỗ; mái lợp tôn có trần; vách ván; nền lát gạch ceramic hoặc tương đương."));
                            break;
                        case "tp-drywall-house":
                            // chuyển thành Nhà phố liền kề ≤ 4 tầng; Cột BTCT hoặc gạch; sàn đúc giả hoặc sàn gỗ; mái lợp tôn hay ngói có trần; tường gạch; nền lát gạch ceramic hoặc tương đương.
                            p.Type = typeHouse;
                            p.TypeConstruction =
                                GetTypeConstructions(typeHouse.Id, 2).FirstOrDefault(a => a.Name.Contains(
                                    "Nhà phố liền kề ≤ 4 tầng; Cột BTCT hoặc gạch; sàn đúc giả hoặc sàn gỗ; mái lợp tôn hay ngói có trần; tường gạch; nền lát gạch ceramic hoặc tương đương."));
                            break;
                        case "tp-concrete-house":
                            // chọn lại Loại công trình Nhà phố Default theo số tầng
                            if (p.TypeConstruction == null)
                                p.TypeConstruction = GetTypeConstructionDefaultInFloorsRange(p.Type.Id, p.Floors);
                            break;
                        case "tp-villa":
                            // chọn lại Loại công trình Biệt thự Default theo số tầng
                            if (p.TypeConstruction == null)
                                p.TypeConstruction = GetTypeConstructionDefaultInFloorsRange(p.Type.Id, p.Floors);
                            break;
                        case "tp-office-building":
                            // chọn lại Loại công trình Cao ốc văn phòng Default theo số tầng
                            if (p.TypeConstruction == null)
                                p.TypeConstruction = GetTypeConstructionDefaultInFloorsRange(p.Type.Id, p.Floors);
                            break;
                        case "tp-hotel":
                            // chọn lại Loại công trình Khách sạn Default theo số tầng
                            if (p.TypeConstruction == null)
                                p.TypeConstruction = GetTypeConstructionDefaultInFloorsRange(p.Type.Id, p.Floors);
                            break;
                        case "tp-warehouse-workshop":
                            // chọn lại Loại công trình Nhà xưởng Default theo số tầng
                            if (p.TypeConstruction == null)
                                p.TypeConstruction = GetTypeConstructionDefaultInFloorsRange(p.Type.Id, p.Floors);
                            break;
                    }
                }
            }
        }

        public void UpdatePlacesAroundForProperty()
        {
            IEnumerable<PropertyPart> pList = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().List();
            foreach (PropertyPart item in pList)
            {
                UpdatePlacesAroundForProperty(item);
            }
        }

        public void UpdatePlacesAroundForProperty(PropertyPart p)
        {
            try
            {
                const int rsCount = 5;
                int kCount = 4;
                //var mapPart = _contentManager.Get<
                string address = p.DisplayForAddressPlacesRound;

                if (address.Trim().Any())
                {
                    string[] keyword = _settingService.GetSetting("Google_Maps_Keyword_Search").Split(';');
                    double radius;
                    string placesAround = "";
                    Location curentAddress = _googleApiService.GeoCode(address);

                    #region options filter

                    switch (p.Flag.Id)
                    {
                        case 31: // bình thường
                            radius = 600; // 600, 2
                            kCount = 2; // lấy 2 tiêu chí: chợ, trường mầm non
                            break;
                        case 32: // nhà rẻ
                            radius = 800; // 800, 3
                            kCount = 3; // lấy 3 tiêu chí: chợ, trường mầm non, siêu thị
                            break;
                        case 33: // nhà rất rẻ
                            radius = 1000; // 1000 ,4 
                            kCount = 4; // lấy 4 tiêu chí: chợ, trường mầm non, siêu thị, công viên
                            break;
                        case 34: // nhà giá cao// 500, 
                            radius = 500;
                            kCount = 1; // lấy 1tiêu chí: chợ
                            break;
                        case 35: // chưa định giá dc
                            radius = 500; // 500, 1
                            kCount = 1; // lấy 1tiêu chí: chợ
                            break;
                        default:
                            radius = 1000;
                            break;
                    }

                    #endregion

                    for (int i = 0; i < kCount; i++)
                    {
                        IEnumerable<Result> result = _googleApiService.ResultSearch(address, keyword[i], radius,
                            _settingService.GetSetting("Google_Maps_API_KEY")).ToList();

                        if (result.Any())
                        {
                            placesAround += "<strong>- Gần " + keyword[i] + ": </strong>";

                            List<GoogleAPIViewModel> model = result.Take(rsCount).Select(c => new GoogleAPIViewModel
                            {
                                Name = "</br>+ " + c.Name,
                                Vicinity = c.Vicinity,
                                Distance =
                                    _googleApiService.DistanceTwoPoint(curentAddress.Latitude,
                                        c.Geometry.Location.Latitude, curentAddress.Longitude,
                                        c.Geometry.Location.Longitude)
                            }).ToList();


                            placesAround = model.Aggregate(placesAround,
                                (current, g) => current + g.Name + " (~" + g.Distance + "m) - " + g.Vicinity);
                            placesAround += "</br>";
                        }
                    }

                    p.PlacesAround = placesAround;
                }
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("Error UpdatePlacesAroundForProperty: {0}", ex.Message));
            }
        }

        public void UpdatePlacesAroundForProperty(PropertyPart p, double lat, double lng)
        {
            try
            {
                const int rsCount = 5;
                int kCount = 4;
                //var mapPart = _contentManager.Get<
                string address = p.DisplayForAddressPlacesRound;

                if (address.Trim().Any())
                {
                    string[] keyword = _settingService.GetSetting("Google_Maps_Keyword_Search").Split(';');
                    double radius;
                    string placesAround = "";

                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    Location curentAddress = (lat == 0 || lng == 0)
                        ? _googleApiService.GeoCode(address)
                        : new Location(lat, lng);

                    #region options filter

                    switch (p.Flag.Id)
                    {
                        case 31: // bình thường
                            radius = 600; // 600, 2
                            kCount = 2; // lấy 2 tiêu chí: chợ, trường mầm non
                            break;
                        case 32: // nhà rẻ
                            radius = 800; // 800, 3
                            kCount = 3; // lấy 3 tiêu chí: chợ, trường mầm non, siêu thị
                            break;
                        case 33: // nhà rất rẻ
                            radius = 1000; // 1000 ,4 
                            kCount = 4; // lấy 4 tiêu chí: chợ, trường mầm non, siêu thị, công viên
                            break;
                        case 34: // nhà giá cao// 500, 
                            radius = 500;
                            kCount = 1; // lấy 1tiêu chí: chợ
                            break;
                        case 35: // chưa định giá dc
                            radius = 500; // 500, 1
                            kCount = 1; // lấy 1tiêu chí: chợ
                            break;
                        default:
                            radius = 1000;
                            break;
                    }

                    #endregion

                    for (int i = 0; i < kCount; i++)
                    {
                        IEnumerable<Result> result = (lat == 0 || lng == 0)
                            ? _googleApiService.ResultSearch(address, keyword[i], radius,
                                _settingService.GetSetting("Google_Maps_API_KEY")).ToList()
                            : _googleApiService.ResultSearch(lng, lat, keyword[i], radius,
                                _settingService.GetSetting("Google_Maps_API_KEY")).ToList();

                        if (result.Any())
                        {
                            placesAround += "<strong>- Gần " + keyword[i] + ": </strong>";

                            List<GoogleAPIViewModel> model = result.Take(rsCount).Select(c => new GoogleAPIViewModel
                            {
                                Name = "</br>+ " + c.Name,
                                Vicinity = c.Vicinity,
                                Distance =
                                    _googleApiService.DistanceTwoPoint(curentAddress.Latitude,
                                        c.Geometry.Location.Latitude, curentAddress.Longitude,
                                        c.Geometry.Location.Longitude)
                            }).ToList();


                            placesAround = model.Aggregate(placesAround,
                                (current, g) => current + g.Name + " (~" + g.Distance + "m) - " + g.Vicinity);
                            placesAround += "</br>";
                        }
                    }
                    // ReSharper restore CompareOfFloatsByEqualityOperator

                    p.PlacesAround = placesAround;
                }
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("Error UpdatePlacesAroundForProperty: {0}", ex.Message));
            }
        }

        #endregion

        #region COPY

        /// <summary>
        /// Copy BĐS từ Rao bán sang Cho thuê hoặc ngược lại
        /// </summary>
        /// <param name="property"></param>
        /// <param name="adsTypeCssClass"></param>
        /// <param name="publishedCopy"></param>
        /// <param name="priceProposedCopy"></param>
        /// <param name="paymentMethodIdCopy"></param>
        /// <param name="paymentUnitIdCopy"></param>
        /// <returns></returns>
        public PropertyPart CopyToAdsType(PropertyPart property, string adsTypeCssClass, bool publishedCopy,
            double priceProposedCopy, int paymentMethodIdCopy, int paymentUnitIdCopy)
        {
            PropertyPart clone = Copy(property);

            if (String.IsNullOrEmpty(adsTypeCssClass))
                adsTypeCssClass = property.AdsType.CssClass == "ad-selling" ? "ad-leasing" : "ad-selling";

            clone.AdsType = GetAdsType(adsTypeCssClass);
            clone.Published = publishedCopy;

            clone.PriceProposed = priceProposedCopy;
            clone.PaymentMethod = GetPaymentMethod(paymentMethodIdCopy);
            clone.PaymentUnit = GetPaymentUnit(paymentUnitIdCopy);
            clone.PriceProposedInVND = CaclPriceProposedInVnd(clone);

            return clone;
        }

        /// <summary>
        /// Copy BĐS từ Group khác vào BĐS khách
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public PropertyPart CopyToApproved(PropertyPart property)
        {

            var clone = CopyToGroup(property);

            // Status
            clone.Status = GetStatus("st-approved");

            return clone;
        }

        /// <summary>
        /// Copy BĐS từ Group khác vào BĐS nội bộ
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public PropertyPart CopyToGroup(PropertyPart property)
        {
            var clone = Copy(property);

            #region Address

            // Address
            if (!property.PublishAddress)
            {
                clone.AddressNumber = "";
                clone.AddressCorner = "";

                // AlleyNumber
                clone.AlleyNumber = -1;

                // ApartmentNumber
                clone.ApartmentNumber = "";
            }

            #endregion

            #region Contact

            // Contact
            if (!property.PublishContact)
            {
                //lấy thông tin liên hệ của nhóm
                string displayForContact = GetDisplayForContact(property);
                clone.ContactPhone = displayForContact;
                clone.ContactPhoneToDisplay = property.ContactPhoneToDisplay;

                clone.ContactName = "";
                clone.ContactAddress = "";
                clone.ContactEmail = "";
            }

            #endregion

            #region After Created

            // Tự động chuyển sang trạng thái RAO BÁN nếu đủ thông tin Địa chỉ, Hẻm, Diện tích, Giá
            if (clone.Status.CssClass == "st-new" || clone.Status.CssClass == "st-estimate")
            {
                if (IsValid(clone))
                    clone.Status = GetStatus("st-selling");
            }
            if (clone.Status.CssClass == "st-selling" && clone.Published)
                clone.AdsExpirationDate = DateTime.Now.AddDays(90);

            #endregion

            return clone;
        }

        public PropertyPart Copy(PropertyPart property)
        {
            DateTime createdDate = DateTime.Now;
            var createdUser = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord belongGroup = _groupService.GetBelongGroup(createdUser.Id);

            var clone = Services.ContentManager.New<PropertyPart>("Property");

            #region RECORD

            #region Type

            // Type
            clone.Type = property.Type;
            clone.TypeGroup = property.Type.Group;

            #endregion

            #region Address

            // Province
            clone.Province = property.Province;

            // District
            clone.District = property.District;

            // Ward
            clone.Ward = property.Ward;
            clone.OtherWardName = property.OtherWardName;

            // Street
            clone.Street = property.Street;
            clone.OtherStreetName = property.OtherStreetName;

            // Apartment
            clone.Apartment = property.Apartment;
            clone.OtherProjectName = property.OtherProjectName;

            // Address
            clone.AddressNumber = property.AddressNumber;
            clone.AddressCorner = property.AddressCorner;

            // AlleyNumber
            clone.AlleyNumber = property.AlleyNumber;

            // ApartmentNumber
            clone.ApartmentNumber = property.ApartmentNumber;

            #endregion

            #region Legal, Direction, Location

            // LegalStatus
            clone.LegalStatus = property.LegalStatus;

            // Direction
            clone.Direction = property.Direction;

            // Location
            clone.Location = property.Location;

            #endregion

            #region Alley

            if (property.TypeGroup.CssClass != "gp-apartment")
            {
                if (property.Location.CssClass == "h-front")
                {
                    // StreetWidth
                    clone.StreetWidth = property.StreetWidth;
                }
                else
                {
                    // Alley
                    clone.DistanceToStreet = property.DistanceToStreet;

                    clone.AlleyTurns = property.AlleyTurns;
                    clone.AlleyWidth = property.AlleyWidth;
                    clone.AlleyWidth1 = property.AlleyWidth1;
                    clone.AlleyWidth2 = property.AlleyWidth2;
                    clone.AlleyWidth3 = property.AlleyWidth3;
                    clone.AlleyWidth4 = property.AlleyWidth4;
                    clone.AlleyWidth5 = property.AlleyWidth5;
                    clone.AlleyWidth6 = property.AlleyWidth6;
                    clone.AlleyWidth7 = property.AlleyWidth7;
                    clone.AlleyWidth8 = property.AlleyWidth8;
                    clone.AlleyWidth9 = property.AlleyWidth9;
                }
            }
            #endregion

            #region Area

            // Area for filter only
            clone.Area = property.Area;

            // AreaTotal
            clone.AreaTotal = property.AreaTotal;
            clone.AreaTotalWidth = property.AreaTotalWidth;
            clone.AreaTotalLength = property.AreaTotalLength;
            clone.AreaTotalBackWidth = property.AreaTotalBackWidth;

            // AreaLegal
            clone.AreaLegal = property.AreaLegal;
            clone.AreaLegalWidth = property.AreaLegalWidth;
            clone.AreaLegalLength = property.AreaLegalLength;
            clone.AreaLegalBackWidth = property.AreaLegalBackWidth;
            clone.AreaIlegalRecognized = property.AreaIlegalRecognized;
            clone.AreaIlegalNotRecognized = property.AreaIlegalNotRecognized;

            // AreaResidential
            clone.AreaResidential = property.AreaResidential;

            #endregion

            #region Construction

            // Construction
            clone.AreaConstruction = property.AreaConstruction;
            clone.AreaConstructionFloor = property.AreaConstructionFloor;
            clone.AreaUsable = property.AreaUsable;

            clone.Floors = property.Floors;
            clone.Bedrooms = property.Bedrooms;
            clone.Livingrooms = property.Livingrooms;
            clone.Bathrooms = property.Bathrooms;
            clone.Balconies = property.Balconies;

            clone.HaveBasement = property.HaveBasement;
            clone.HaveElevator = property.HaveElevator;
            clone.HaveGarage = property.HaveGarage;
            clone.HaveGarden = property.HaveGarden;
            clone.HaveMezzanine = property.HaveMezzanine;
            clone.HaveSkylight = property.HaveSkylight;
            clone.HaveSwimmingPool = property.HaveSwimmingPool;
            clone.HaveTerrace = property.HaveTerrace;

            // Interior
            clone.Interior = property.Interior;
            clone.RemainingValue = property.RemainingValue;
            clone.TypeConstruction = property.TypeConstruction;

            #endregion

            #region Apartment Info

            // Apartment Info
            clone.ApartmentFloors = property.ApartmentFloors;
            clone.ApartmentFloorTh = property.ApartmentFloorTh;
            clone.ApartmentElevators = property.ApartmentElevators;
            clone.ApartmentBasements = property.ApartmentBasements;

            #endregion

            #region OtherAdvantages & OtherDisAdvantages

            clone.OtherAdvantages = property.OtherAdvantages;
            clone.OtherAdvantagesDesc = property.OtherAdvantagesDesc;
            clone.OtherDisAdvantages = property.OtherDisAdvantages;
            clone.OtherDisAdvantagesDesc = property.OtherDisAdvantagesDesc;

            #endregion

            #region Contact

            // Contact
            clone.ContactName = property.ContactName;
            clone.ContactPhone = property.ContactPhone;
            clone.ContactPhoneToDisplay = property.ContactPhoneToDisplay;
            clone.ContactAddress = property.ContactAddress;
            clone.ContactEmail = property.ContactEmail;

            #endregion

            #region PublishAddress && PublishContact

            clone.PublishAddress = property.PublishAddress;
            clone.PublishContact = property.PublishContact;

            #endregion

            #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

            clone.IsOwner = property.IsOwner;
            clone.NoBroker = property.NoBroker;
            clone.IsAuction = property.IsAuction;
            clone.IsHighlights = property.IsHighlights;
            clone.IsAuthenticatedInfo = property.IsAuthenticatedInfo;

            #endregion

            #region Price

            // Price
            clone.PriceProposed = property.PriceProposed;
            clone.PaymentMethod = property.PaymentMethod;
            clone.PaymentUnit = property.PaymentUnit;

            clone.PriceProposedInVND = property.PriceProposedInVND;
            clone.PriceEstimatedByStaff = property.PriceEstimatedByStaff;

            clone.PriceNegotiable = property.PriceNegotiable;

            #endregion

            #region Ads Content

            // Ads Content
            clone.Title = property.Title;
            clone.Content = property.Content;
            clone.Note = property.Note;

            #endregion

            #region Add Youtube VideoId

            clone.YoutubeId = property.YoutubeId;

            #endregion

            #region AdsType

            // Ads Type
            clone.AdsType = property.AdsType;
            clone.AdsGoodDeal = false;
            clone.AdsHighlight = false;
            clone.AdsVIP = false;
            clone.SeqOrder = 0;

            // Published
            clone.Published = false;
            clone.AdsExpirationDate = property.AdsExpirationDate; // 90 days ?

            #endregion

            #region Status, Flag

            // Status
            clone.Status = GetStatus("st-new");
            clone.StatusChangedDate = createdDate;

            // Flag
            clone.Flag = GetFlag("deal-normal");
            clone.IsExcludeFromPriceEstimation = false;

            #endregion

            #region User

            // User
            clone.CreatedDate = createdDate;
            clone.CreatedUser = createdUser.Record;
            clone.LastUpdatedDate = createdDate;
            clone.LastUpdatedUser = createdUser.Record;
            clone.FirstInfoFromUser = createdUser.Record;
            clone.LastInfoFromUser = createdUser.Record;

            // UserGroup
            clone.UserGroup = belongGroup;

            #endregion

            #endregion

            Services.ContentManager.Create(clone);

            #region After Created

            // IdStr
            clone.IdStr = clone.Id.ToString(CultureInfo.InvariantCulture);

            // Tự động chuyển sang trạng thái RAO BÁN nếu đủ thông tin Địa chỉ, Hẻm, Diện tích, Giá
            if (clone.Status.CssClass == "st-new" || clone.Status.CssClass == "st-estimate")
            {
                if (IsValid(clone))
                    clone.Status = GetStatus("st-selling");
            }
            if (clone.Status.CssClass == "st-selling" && clone.Published)
                clone.AdsExpirationDate = DateTime.Now.AddDays(90);

            #region Advantages && DisAdvantages

            var advantages = new List<PropertyAdvantagePartRecord>();
            IEnumerable<PropertyAdvantagePartRecordContent> query =
                _advantagesContentRepository.Fetch(r => r.PropertyPartRecord == property.Record).ToList();
            if (query.Any()) advantages = query.Select(a => a.PropertyAdvantagePartRecord).ToList();

            // Add the new advantages
            foreach (PropertyAdvantagePartRecord advantage in advantages)
            {
                _advantagesContentRepository.Create(new PropertyAdvantagePartRecordContent
                {
                    PropertyPartRecord = clone.Record,
                    PropertyAdvantagePartRecord = advantage
                });
            }

            _advantagesContentRepository.Flush();

            #endregion

            // Log User Activity
            _revisionService.SaveUserActivityAddNewProperty(createdDate, createdUser, clone);

            // Upload Images
            //UploadImages(files, clone, false);

            #endregion

            #region Update OrderBy Domain (UserGroup)

            if (belongGroup != null)
                UpdateOrderByDomainGroup(clone, belongGroup.Id);

            #endregion

            return clone;
        }

        public PropertyGroupPart NotShareToGroup(int id, int groupId)
        {
            var p = Services.ContentManager.New<PropertyGroupPart>("PropertyGroup");

            if (!VerifyPropertyGroupExist(id, groupId))
            {
                p.PropertyId = id;
                p.UserGroupId = groupId;
                p.IsApproved = false;

                Services.ContentManager.Create(p);
                Services.Notifier.Information(T("BĐS {0} ({1} - {2}) đã chuyển sang không duyêt", p.Id, id, groupId));
            }
            else
            {
                Services.Notifier.Information(T("BĐS ({1} - {2}) đã tồn tại trong danh sách không duyệt", id, groupId));
            }
            return p;
        }

        #endregion

        #region Search Properties

        public PropertyIndexOptions BuildIndexOptions(PropertyIndexOptions options)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var belongGroup = _groupService.GetBelongGroup(user.Id);

            #region DEFAULT OPTIONS

            // default options
            if (options == null)
            {
                options = new PropertyIndexOptions
                {
                    StatusId = 0,
                    FlagId = 0,
                    AdsTypeId = 0,
                    TypeGroupId = 0,
                    ProvinceId = 0,
                    PaymentMethodId = 0,
                    PaymentUnitId = 0,
                    DirectionId = 0,
                    LegalStatusId = 0,
                    LocationId = 0,
                    InteriorId = 0,
                    CreatedUserId = 0,
                    LastUpdatedUserId = 0,
                    FirstInfoFromUserId = 0,
                    GroupId = 0,
                    BelongGroupId = 0
                };
            }

            // Status
            options.Status = GetStatusWithDefault(null, null);
            if (options.StatusId > 0) options.StatusCssClass = GetStatus(options.StatusId).CssClass;

            // Flags
            options.Flags = GetFlags();

            // AdsTypes
            options.AdsTypes = GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing");
            if (!options.AdsTypeId.HasValue || options.AdsTypeId == 0)
                options.AdsTypeId = _groupService.GetUserDefaultAdsType(user).Id;

            // TypeGroups
            options.TypeGroups = GetTypeGroups();
            if (!options.TypeGroupId.HasValue || options.TypeGroupId == 0)
                options.TypeGroupId = _groupService.GetUserDefaultTypeGroup(user).Id;
            if (options.TypeGroupId > 0) options.TypeGroupCssClass = GetTypeGroup(options.TypeGroupId).CssClass;

            // Types
            options.Types = GetTypes();
            //if (options.TypeIds == null || options.TypeIds.Count() == 0) options.TypeIds = GetTypes().Where(a => a.Group.CssClass == "gp-house").Select(a => a.Id).ToArray();

            #region Provinces, Districts, Wards, Streets, Apartments

            // Provinces
            options.Provinces = _addressService.GetSelectListProvinces();
            if (options.Provinces[0].Value != "0") options.Provinces.Insert(0, new SelectListItem { Text = "-- Chọn tất cả Tỉnh/TP --", Value = "0" });
            if (!options.ProvinceId.HasValue) options.ProvinceId = _groupService.GetUserDefaultProvinceId(user);

            // Districts
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            if (options.DistrictIds == null || !options.DistrictIds.Any())
            {
                LocationDistrictPartRecord defaultDistrict = _groupService.GetUserDefaultDistrict(user);
                if (defaultDistrict != null && defaultDistrict.Province.Id == options.ProvinceId) options.DistrictIds = new[] { defaultDistrict.Id };
            }

            // Wards
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Wards = _addressService.GetWards(options.DistrictIds);
            else
                options.Wards = _addressService.GetWards(0);

            // Streets
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Streets = _addressService.GetStreets(options.DistrictIds);
            else
                options.Streets = _addressService.GetStreetsByProvince(0);

            // Apartments
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Apartments = _addressService.GetApartments(options.DistrictIds);
            else
                options.Apartments = _addressService.GetApartments(0);

            #endregion

            // PaymentMethod
            options.PaymentMethods = GetPaymentMethods();
            if (options.PaymentMethodId == 0)
            {
                AdsTypePartRecord adsTypeLeasing = GetAdsType("ad-leasing");
                if (options.AdsTypeId == adsTypeLeasing.Id || options.AdsTypeCssClass == adsTypeLeasing.CssClass)
                    options.PaymentMethodId = GetPaymentMethod("pm-vnd-m").Id;
                else
                    options.PaymentMethodId = GetPaymentMethod("pm-vnd-b").Id;
            }
            options.PaymentMethodCssClass = GetPaymentMethod(options.PaymentMethodId).CssClass;
            options.PaymentMethodShortName = GetPaymentMethod(options.PaymentMethodId).ShortName;

            // PaymentUnit
            options.PaymentUnits = GetPaymentUnits();
            if (options.PaymentUnitId == 0) options.PaymentUnitId = GetPaymentUnit("unit-total").Id;

            // Directions, LegalStatus, Locations, Interiors
            options.Directions = GetDirections();
            options.LegalStatus = GetLegalStatus();
            options.Locations = GetLocations();
            options.Interiors = GetInteriors();

            // Advantages, DisAdvantages
            options.Advantages = GetAdvantagesEntries();
            options.DisAdvantages = GetDisAdvantagesEntries();
            options.ApartmentAdvantages = GetApartmentAdvantagesEntries();
            options.ApartmentInteriorAdvantages = GetApartmentInteriorAdvantagesEntries();

            // Groups
            if (belongGroup != null && !options.GroupId.HasValue) options.GroupId = belongGroup.Id;
            if (belongGroup != null) options.BelongGroupId = belongGroup.Id;

            //UserGroupDomain
            options.UserGroupDomain = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            // Other
            options.NeedUpdateDate =
                DateTime.Now.AddDays(-double.Parse(_settingService.GetSetting("DaysToUpdatePrice") ?? "90"));

            #endregion

            return options;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(PropertyIndexOptions options)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            //DateTime startBuildIndexOptions = DateTime.Now;
            options = BuildIndexOptions(options);

            //if (_debugIndex) Services.Notifier.Information(T("BuildIndexOptions {0}", (DateTime.Now - startBuildIndexOptions).TotalSeconds));

            // Get BĐS theo Group và các quyền liên quan
            IContentQuery<PropertyPart, PropertyPartRecord> list = GetUserProperties(user);

            #region SECURITY


            // List PropertyUserPartRecordContent
            if (options.IsPropertiesWatchList)
            {
                var listPropertiesId = GetListPropertyUserContent().Select(r => r.PropertyPartRecord.Id).ToList();
                list = list.Where(p => listPropertiesId.Contains(p.Id));
            }
            else if (options.IsPropertiesExchange)
            {
                var listPropertiesId = ListPropertyIdsExchange();
                list = list.Where(p => listPropertiesId.Contains(p.Id));
            }
            else
            {

                // BĐS của khách
                List<int> statusExternal = GetStatusForExternal().Select(a => a.Id).ToList();
                if (statusExternal.Contains(options.StatusId ?? 0) && (Services.Authorizer.Authorize(Permissions.ApproveProperty)))
                {
                    list = list.Where(a => statusExternal.Contains(a.Status.Id));
                }
                // BĐS nội bộ
                else
                {
                    #region Permissions "deal-good", "deal-very-good", "st-trading", "st-sold", "st-onhold"

                    if (!Services.Authorizer.Authorize(Permissions.MetaListDealGoodProperties))
                    {
                        // Remove all properties have flag 'deal-good'
                        PropertyFlagPartRecord flagDealGood = GetFlag("deal-good");
                        list = list.Where(p => p.Flag != flagDealGood); // || subordinateUserIds.Contains(p.CreatedUser.Id)
                        //options.Flags = options.Flags.Where(a => a.CssClass != "deal-good");
                    }

                    if (!Services.Authorizer.Authorize(Permissions.MetaListDealVeryGoodProperties))
                    {
                        // Remove all properties have flag 'deal-very-good'
                        PropertyFlagPartRecord flagDealVeryGood = GetFlag("deal-very-good");
                        list =
                            list.Where(p => p.Flag != flagDealVeryGood); // || subordinateUserIds.Contains(p.CreatedUser.Id)
                        //options.Flags = options.Flags.Where(a => a.CssClass != "deal-very-good");
                    }

                    if (!Services.Authorizer.Authorize(Permissions.AccessTradingProperties))
                    {
                        // Remove all properties have status 'st-trading'
                        PropertyStatusPartRecord statusTrading = GetStatus("st-trading");
                        list =
                            list.Where(p => p.Status != statusTrading); // || subordinateUserIds.Contains(p.CreatedUser.Id)
                        //options.Status = options.Status.Where(a => a.CssClass != "st-trading");
                    }

                    if (!Services.Authorizer.Authorize(Permissions.AccessSoldProperties))
                    {
                        // Remove all properties have status 'st-sold'
                        PropertyStatusPartRecord statusSold = GetStatus("st-sold");
                        list =
                            list.Where(p => p.Status != statusSold); // || subordinateUserIds.Contains(p.CreatedUser.Id)
                        //options.Status = options.Status.Where(a => a.CssClass != "st-sold");
                    }

                    if (!Services.Authorizer.Authorize(Permissions.AccessOnHoldProperties))
                    {
                        // Remove all properties have status 'st-onhold'
                        PropertyStatusPartRecord statusOnHold = GetStatus("st-onhold");
                        list =
                            list.Where(p => p.Status != statusOnHold); // || subordinateUserIds.Contains(p.CreatedUser.Id)
                        //options.Status = options.Status.Where(a => a.CssClass != "st-onhold");
                    }

                    #endregion
                }
            }

            #endregion

            #region FILTER

            if (options.Id > 0 || !String.IsNullOrEmpty(options.IdStr))
            {
                #region Id, IdStr

                // Id
                if (options.Id > 0) list = list.Where(p => p.Id == options.Id);

                // IdStr
                list = list.Where(p => p.IdStr.Contains(options.IdStr.Trim()));

                #endregion
            }
            else
            {
                #region Status
                int statusDeleted = GetStatus("st-deleted").Id;
                var statusCssClass = new List<string> { "st-new", "st-selling", "st-negotiate" };
                // -- Đang rao bán -- (MỚI, RAO BÁN, THƯƠNG LƯỢNG)
                List<int> statusIds =
                    GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

                // Status
                if (options.IsSoldByGroup == true)
                {
                    var statusSold = GetStatus("st-sold");
                    options.StatusId = statusSold.Id;
                    list = list.Where(p => p.Status == statusSold && p.IsSoldByGroup == true && p.UserGroup.Id == options.GroupId);
                }
                else if (options.StatusId == null)
                    list = list.Where(p => statusIds.Contains(p.Status.Id)); // -- Đang rao bán --
                else if (options.StatusId == 0)
                    list = list.Where(p => p.Status.Id != statusDeleted); // -- Tất cả --
                else if (options.StatusId > 0)
                    list = list.Where(p => p.Status.Id == options.StatusId); // selected Status

                #endregion

                #region Flag

                // Flag
                if (options.FlagIds != null)
                    if (options.FlagIds.Any()) list = list.Where(p => options.FlagIds.Contains(p.Flag.Id));

                #endregion

                #region AdsType, TypeGroup, Type

                // AdsType
                if (options.AdsTypeId > 0) list = list.Where(p => p.AdsType.Id == options.AdsTypeId);

                // TypeGroup
                if (options.TypeGroupId > 0) list = list.Where(p => p.TypeGroup.Id == options.TypeGroupId);

                // Type
                if (options.TypeIds != null)
                    if (options.TypeIds.Any()) list = list.Where(p => options.TypeIds.Contains(p.Type.Id));

                // TypeConstruction
                if (options.TypeConstructionIds != null)
                    if (options.TypeConstructionIds.Any())
                        list = list.Where(p => options.TypeConstructionIds.Contains(p.TypeConstruction.Id));

                #endregion

                #region Address

                // Provinces
                if (options.ProvinceId > 0) list = list.Where(p => p.Province.Id == options.ProvinceId);

                // Districts
                if (options.DistrictIds != null)
                    if (options.DistrictIds.Any())
                        list = list.Where(p => options.DistrictIds.Contains(p.District.Id));

                // Wards
                if (options.WardIds != null)
                    if (options.WardIds.Any()) list = list.Where(p => options.WardIds.Contains(p.Ward.Id));

                // Streets
                if (options.StreetIds != null)
                    if (options.StreetIds.Any())
                    {
                        // Lấy tất cả BĐS trên Đường và Đoạn Đường
                        List<int> listRelatedStreetIds =
                            _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                                .Where(a => options.StreetIds.Contains(a.RelatedStreet.Id))
                                .List()
                                .Select(a => a.Id)
                                .ToList();
                        listRelatedStreetIds.AddRange(options.StreetIds);
                        list = list.Where(p => listRelatedStreetIds.Contains(p.Street.Id));
                    }

                // Street
                if (options.StreetId > 0)
                {
                    // Lấy tất cả BĐS trên Đường và Đoạn Đường
                    List<int> listRelatedStreetIds =
                        _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                            .Where(a => a.RelatedStreet.Id == options.StreetId)
                            .List()
                            .Select(a => a.Id)
                            .ToList();
                    listRelatedStreetIds.Add(options.StreetId ?? 0);
                    list = list.Where(p => listRelatedStreetIds.Contains(p.Street.Id));
                }

                // Apartments
                if (options.TypeGroupCssClass == "gp-apartment")
                {
                    if (options.ApartmentId > 0)
                    {
                        list = list.Where(p => p.Apartment != null && p.Apartment.Id == options.ApartmentId);
                    }
                    if (options.ApartmentIds != null)
                        if (options.ApartmentIds.Any())
                            list =
                                list.Where(p => p.Apartment != null && options.ApartmentIds.Contains(p.Apartment.Id));

                    if (!String.IsNullOrEmpty(options.ApartmentNumber))
                        list =
                            list.Where(
                                p =>
                                    p.AddressNumber.Contains(options.ApartmentNumber.Trim()) ||
                                    p.ApartmentNumber.Contains(options.ApartmentNumber.Trim()));
                }

                // AddressNumber
                if (!String.IsNullOrEmpty(options.AddressNumber))
                {
                    if (options.UseAccurateSearch)
                        list =
                            list.Where(
                                p =>
                                    p.AddressNumber == options.AddressNumber.Trim() ||
                                    p.ApartmentNumber == options.AddressNumber.Trim());
                    else
                        list =
                            list.Where(
                                p =>
                                    p.AddressNumber.Contains(options.AddressNumber.Trim()) ||
                                    p.ApartmentNumber.Contains(options.AddressNumber.Trim()));
                }
                if (!String.IsNullOrEmpty(options.AddressCorner))
                {
                    list = options.UseAccurateSearch
                        ? list.Where(p => p.AddressCorner == options.AddressCorner.Trim())
                        : list.Where(p => p.AddressCorner.Contains(options.AddressCorner.Trim()));
                }
                if (!String.IsNullOrEmpty(options.Search))
                    list =
                        list.Where(
                            p =>
                                p.AddressNumber.Contains(options.Search.Trim()) ||
                                p.ApartmentNumber.Contains(options.Search.Trim()));

                #endregion

                #region Direction, LegalStatus, Location

                // Direction
                if (options.DirectionIds != null) // Khi chọn hướng thì phải ra cả những nhà chưa nhập hướng
                    if (options.DirectionIds.Any())
                        list =
                            list.Where(
                                p =>
                                    options.DirectionIds.Contains(p.Direction.Id) ||
                                    (options.UseAccurateSearch == false && p.Direction == null));

                // LegalStatus
                if (options.LegalStatusId > 0) list = list.Where(p => p.LegalStatus.Id == options.LegalStatusId);

                // Location
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.LocationId > 0) list = list.Where(p => p.Location.Id == options.LocationId);
                }

                #endregion

                #region Alley

                // Alley
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (!(options.LocationId > 0))
                    {
                        // Tìm tất cà nhà Hẻm và MT
                        PropertyLocationPartRecord frontLocation = GetLocation("h-front");

                        if (options.MinAlleyWidth > 0)
                            list =
                                list.Where(
                                    p =>
                                        (p.AlleyWidth >= options.MinAlleyWidth ||
                                         (options.UseAccurateSearch == false && p.AlleyWidth == null)) ||
                                        p.Location == frontLocation);
                        if (options.MaxAlleyWidth > 0)
                            list =
                                list.Where(
                                    p =>
                                        (p.AlleyWidth <= options.MaxAlleyWidth ||
                                         (options.UseAccurateSearch == false && p.AlleyWidth == null)) ||
                                        p.Location == frontLocation);

                        if (options.MinAlleyTurns > 0)
                            list =
                                list.Where(
                                    p =>
                                        (p.AlleyTurns >= options.MinAlleyTurns ||
                                         (options.UseAccurateSearch == false && p.AlleyTurns == null)) ||
                                        p.Location == frontLocation);
                        if (options.MaxAlleyTurns > 0)
                            list =
                                list.Where(
                                    p =>
                                        (p.AlleyTurns <= options.MaxAlleyTurns ||
                                         (options.UseAccurateSearch == false && p.AlleyTurns == null)) ||
                                        p.Location == frontLocation);

                        if (options.MinDistanceToStreet > 0)
                            list =
                                list.Where(
                                    p =>
                                        (p.DistanceToStreet >= options.MinDistanceToStreet ||
                                         (options.UseAccurateSearch == false && p.DistanceToStreet == null)) ||
                                        p.Location == frontLocation);
                        if (options.MaxDistanceToStreet > 0)
                            list =
                                list.Where(
                                    p =>
                                        (p.DistanceToStreet <= options.MaxDistanceToStreet ||
                                         (options.UseAccurateSearch == false && p.DistanceToStreet == null)) ||
                                        p.Location == frontLocation);
                    }
                    else
                    {
                        if (options.MinAlleyWidth > 0)
                            list =
                                list.Where(
                                    p =>
                                        p.AlleyWidth >= options.MinAlleyWidth ||
                                        (options.UseAccurateSearch == false && p.AlleyWidth == null));
                        if (options.MaxAlleyWidth > 0)
                            list =
                                list.Where(
                                    p =>
                                        p.AlleyWidth <= options.MaxAlleyWidth ||
                                        (options.UseAccurateSearch == false && p.AlleyWidth == null));

                        if (options.MinAlleyTurns > 0)
                            list =
                                list.Where(
                                    p =>
                                        p.AlleyTurns >= options.MinAlleyTurns ||
                                        (options.UseAccurateSearch == false && p.AlleyTurns == null));
                        if (options.MaxAlleyTurns > 0)
                            list =
                                list.Where(
                                    p =>
                                        p.AlleyTurns <= options.MaxAlleyTurns ||
                                        (options.UseAccurateSearch == false && p.AlleyTurns == null));

                        if (options.MinDistanceToStreet > 0)
                            list =
                                list.Where(
                                    p =>
                                        p.DistanceToStreet >= options.MinDistanceToStreet ||
                                        (options.UseAccurateSearch == false && p.DistanceToStreet == null));
                        if (options.MaxDistanceToStreet > 0)
                            list =
                                list.Where(
                                    p =>
                                        p.DistanceToStreet <= options.MaxDistanceToStreet ||
                                        (options.UseAccurateSearch == false && p.DistanceToStreet == null));
                    }
                }

                #endregion

                #region Area

                // Area
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinAreaTotal > 0)
                        list = list.Where(p => p.AreaTotal >= options.MinAreaTotal || p.Area >= options.MinAreaTotal);
                    if (options.MaxAreaTotal > 0)
                        list = list.Where(p => p.AreaTotal <= options.MaxAreaTotal || p.Area <= options.MaxAreaTotal);

                    if (options.MinAreaTotalWidth > 0)
                        list = list.Where(p => p.AreaTotalWidth >= options.MinAreaTotalWidth);
                    if (options.MaxAreaTotalWidth > 0)
                        list = list.Where(p => p.AreaTotalWidth <= options.MaxAreaTotalWidth);

                    if (options.MinAreaTotalLength > 0)
                        list = list.Where(p => p.AreaTotalLength >= options.MinAreaTotalLength);
                    if (options.MaxAreaTotalLength > 0)
                        list = list.Where(p => p.AreaTotalLength <= options.MaxAreaTotalLength);
                }

                // AreaUsable
                if (options.TypeGroupCssClass == "gp-apartment")
                {
                    if (options.MinAreaUsable > 0)
                        list = list.Where(p => p.AreaUsable >= options.MinAreaUsable || p.Area >= options.MinAreaTotal);
                    if (options.MaxAreaUsable > 0)
                        list = list.Where(p => p.AreaUsable <= options.MaxAreaUsable || p.Area <= options.MaxAreaTotal);

                    // ApartmentFloorTh
                    if (options.MinApartmentFloorTh > 0)
                        list = list.Where(p => p.ApartmentFloorTh >= options.MinApartmentFloorTh);
                    if (options.MaxApartmentFloorTh > 0)
                        list = list.Where(p => p.ApartmentFloorTh <= options.MaxApartmentFloorTh);
                }

                #endregion

                #region Construction

                // Construction

                if (options.InteriorId > 0) list = list.Where(p => p.Interior.Id == options.InteriorId);

                // Floors
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinFloors > 0) list = list.Where(p => p.Floors >= options.MinFloors);
                    if (options.MaxFloors > 0) list = list.Where(p => p.Floors <= options.MaxFloors);
                }

                // Bedrooms
                if (options.MinBedrooms > 0) list = list.Where(p => p.Bedrooms >= options.MinBedrooms);
                if (options.MaxBedrooms > 0) list = list.Where(p => p.Bedrooms <= options.MaxBedrooms);

                // Bathrooms
                if (options.MinBathrooms > 0) list = list.Where(p => p.Bathrooms >= options.MinBathrooms);
                if (options.MaxBathrooms > 0) list = list.Where(p => p.Bathrooms <= options.MaxBathrooms);

                #endregion

                #region Price

                // Price

                // Convert Price to VND
                double minPriceVnd = options.MinPriceProposed > 0
                    ? ConvertToVndB((double)options.MinPriceProposed, options.PaymentMethodCssClass)
                    : 0;
                double maxPriceVnd = options.MaxPriceProposed > 0
                    ? ConvertToVndB((double)options.MaxPriceProposed, options.PaymentMethodCssClass)
                    : 0;


                if (minPriceVnd > 0) list = list.Where(p => p.PriceProposedInVND >= minPriceVnd);
                if (maxPriceVnd > 0) list = list.Where(p => p.PriceProposedInVND <= maxPriceVnd);
                if (minPriceVnd > 0 || maxPriceVnd > 0) list.Where(p => p.PaymentUnit.Id == options.PaymentUnitId);

                #endregion

                #region ContactPhone

                if (!String.IsNullOrWhiteSpace(options.ContactPhone))
                    list =
                        list.Where(
                            p =>
                                p.ContactPhone.Contains(options.ContactPhone) ||
                                p.ContactPhoneToDisplay.Contains(options.ContactPhone));

                #endregion

                #region Users

                if (!String.IsNullOrWhiteSpace(options.CreatedUserNameOrEmail))
                {
                    IContentQuery<UserPart, UserPartRecord> users =
                        _contentManager.Query<UserPart, UserPartRecord>()
                            .Where(
                                a =>
                                    a.UserName.Contains(options.CreatedUserNameOrEmail) ||
                                    a.Email.Contains(options.CreatedUserNameOrEmail));
                    if (users.Count() > 0)
                    {
                        List<int> userIds = users.List().Select(a => a.Id).ToList();
                        list = list.Where(p => userIds.Contains(p.CreatedUser.Id));
                    }
                }

                // Get date value

                const string format = "dd/MM/yyyy";
                CultureInfo provider = CultureInfo.InvariantCulture;
                const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                DateTime createdFrom, createdTo, lastUpdatedFrom, lastUpdatedTo, lastExportedFrom, lastExportedTo;

                DateTime.TryParseExact(options.CreatedFrom, format, provider, style, out createdFrom);
                DateTime.TryParseExact(options.CreatedTo, format, provider, style, out createdTo);

                DateTime.TryParseExact(options.LastUpdatedFrom, format, provider, style, out lastUpdatedFrom);
                DateTime.TryParseExact(options.LastUpdatedTo, format, provider, style, out lastUpdatedTo);

                DateTime.TryParseExact(options.LastExportedFrom, format, provider, style, out lastExportedFrom);
                DateTime.TryParseExact(options.LastExportedTo, format, provider, style, out lastExportedTo);

                if (options.CreatedUserId > 0) list = list.Where(p => p.CreatedUser.Id == options.CreatedUserId);
                if (createdFrom.Year > 1) list = list.Where(p => p.CreatedDate >= createdFrom);
                if (createdTo.Year > 1) list = list.Where(p => p.CreatedDate <= createdTo.AddHours(24));

                if (options.LastUpdatedUserId > 0)
                    list = list.Where(p => p.LastUpdatedUser.Id == options.LastUpdatedUserId);
                if (lastUpdatedFrom.Year > 1) list = list.Where(p => p.LastUpdatedDate >= lastUpdatedFrom);
                if (lastUpdatedTo.Year > 1) list = list.Where(p => p.LastUpdatedDate <= lastUpdatedTo.AddHours(24));

                if (options.LastExportedUserId > 0)
                    list = list.Where(p => p.LastExportedUser.Id == options.LastExportedUserId);
                if (lastExportedFrom.Year > 1) list = list.Where(p => p.LastExportedDate >= lastExportedFrom);
                if (lastExportedTo.Year > 1)
                    list = list.Where(p => p.LastExportedDate <= lastExportedTo.AddHours(24));

                if (options.FirstInfoFromUserId > 0)
                    list = list.Where(p => p.FirstInfoFromUser.Id == options.FirstInfoFromUserId);

                #endregion

                #region Group

                // GroupId
                if (options.GroupId > 0)
                {
                    list = list.Where(p => p.UserGroup.Id == options.GroupId);
                }

                // GroupIds
                if (options.GroupIds != null)
                    if (options.GroupIds.Any()) list = list.Where(p => options.GroupIds.Contains(p.UserGroup.Id));

                #endregion

                #region Other

                // PriceEstimatedRatingPoint
                if (options.StatusCssClass == "st-estimate" && options.PriceEstimatedRatingPoint > 0)
                    list = list.Where(p => p.PriceEstimatedRatingPoint == options.PriceEstimatedRatingPoint);

                // ShowNeedUpdate
                if (options.ShowNeedUpdate) list = list.Where(p => p.LastUpdatedDate <= options.NeedUpdateDate);

                if (options.ShowExcludedInEstimation) list = list.Where(p => p.IsExcludeFromPriceEstimation);
                if (options.ShowIncludedInEstimation) list = list.Where(p => !p.IsExcludeFromPriceEstimation);

                #endregion

                #region Advantages, DisAdvantages

                //// Advantages

                //if (options.AdvantageIds != null)
                //    if (options.AdvantageIds.Any())
                //        foreach (var advId in options.AdvantageIds)
                //        {
                //            //_list = _list.Where(p => p.Advantages.Any(a => a.PropertyAdvantagePartRecord.Id == advId));
                //        }

                //// DisAdvantages

                //if (options.DisAdvantageIds != null)
                //    if (options.DisAdvantageIds.Any())
                //        foreach (var advId in options.DisAdvantageIds)
                //        {
                //            //_list = _list.Where(p => p.Advantages.Any(a => a.PropertyAdvantagePartRecord.Id == advId));
                //        }

                //// ApartmentAdvantages

                //if (options.ApartmentAdvantageIds != null)
                //    if (options.ApartmentAdvantageIds.Any())
                //        foreach (var advId in options.ApartmentAdvantageIds)
                //        {
                //            //_list = _list.Where(p => p.ApartmentAdvantages.Any(a => a.PropertyAdvantagePartRecord.Id == advId));
                //        }

                #endregion

                #region Ads Content

                // AdsType

                if (options.AdsHighlight) list = list.Where(p => p.AdsHighlight);
                if (options.AdsHighlightRequest) list = list.Where(p => p.AdsHighlightRequest);

                if (options.AdsGoodDeal) list = list.Where(p => p.AdsGoodDeal);
                if (options.AdsGoodDealRequest) list = list.Where(p => p.AdsGoodDealRequest);

                if (options.AdsVIP) list = list.Where(p => p.AdsVIP);

                // VIP1, VIP2, VIP3
                if (options.AdsVIP1 || options.AdsVIP2 || options.AdsVIP3)
                {
                    var adsViPs = new List<int>();

                    if (options.AdsVIP1) adsViPs.Add(3);
                    if (options.AdsVIP2) adsViPs.Add(2);
                    if (options.AdsVIP3) adsViPs.Add(1);

                    list = list.Where(p => adsViPs.Contains(p.SeqOrder));
                }

                if (options.AdsVIPRequest) list = list.Where(p => p.AdsVIPRequest);

                if (options.AdsNormal)
                    list =
                        list.Where(
                            p => p.Published && p.AdsVIP == false && p.AdsHighlight == false && p.AdsGoodDeal == false);

                if (options.AdsRequest)
                    list = list.Where(p => p.AdsGoodDealRequest || p.AdsVIPRequest || p.AdsHighlightRequest);

                #endregion

                #region IsOwner, NoBroker, IsAuction, IsHighlights

                if (options.IsOwner) list = list.Where(p => p.IsOwner);
                if (options.NoBroker) list = list.Where(p => p.NoBroker);
                if (options.IsAuction) list = list.Where(p => p.IsAuction);
                if (options.IsHighlights) list = list.Where(p => p.IsHighlights);
                if (options.IsAuthenticatedInfo) list = list.Where(p => p.IsAuthenticatedInfo);

                #endregion

                #region AdsExpired, AdsNotExpired

                DateTime dateNow = DateTime.Now;

                if (options.AdsExpired) list = list.Where(p => p.AdsExpirationDate < dateNow);
                if (options.AdsNotExpired) list = list.Where(p => p.AdsExpirationDate >= dateNow);

                #endregion

                #region PublishAddress, PublishContact

                if (options.PublishAddress) list = list.Where(p => p.PublishAddress);
                if (options.PublishContact) list = list.Where(p => p.PublishContact);

                #endregion
            }

            #endregion

            #region ORDER

            switch (options.Order)
            {
                case PropertyOrder.LastUpdatedDate:
                    list = list.OrderByDescending(u => u.LastUpdatedDate);
                    break;
                case PropertyOrder.AddressNumber:
                    list = list.OrderBy(u => u.AlleyNumber);
                    break;
                case PropertyOrder.PriceProposedInVND:
                    list = list.OrderBy(u => u.PriceProposedInVND);
                    break;
            }

            #endregion

            return list;
        }

        #endregion

        #region Search Group Properties

        public PropertyIndexOptions BuildGroupIndexOptions(PropertyIndexOptions options)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            #region DEFAULT OPTIONS

            // default options
            if (options == null)
            {
                options = new PropertyIndexOptions
                {
                    StatusId = 0,
                    FlagId = 0,
                    AdsTypeId = 0,
                    TypeGroupId = 0,
                    ProvinceId = 0,
                    PaymentMethodId = 0,
                    PaymentUnitId = 0,
                    DirectionId = 0,
                    LegalStatusId = 0,
                    LocationId = 0,
                    InteriorId = 0,
                    CreatedUserId = 0,
                    LastUpdatedUserId = 0,
                    FirstInfoFromUserId = 0,
                    GroupId = 0,
                    GroupApproved = "none"
                };
            }

            // Status
            options.Status = GetStatusWithDefault(null, null).Where(a => _statusGroupCssClass.Contains(a.CssClass));
            if (options.StatusId > 0) options.StatusCssClass = GetStatus(options.StatusId).CssClass;

            // Flags
            options.Flags = GetFlags();

            // AdsTypes
            options.AdsTypes = GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing");
            if (!options.AdsTypeId.HasValue || options.AdsTypeId == 0)
                options.AdsTypeId = _groupService.GetUserDefaultAdsType(user).Id;

            // TypeGroups
            options.TypeGroups = GetTypeGroups();
            if (!options.TypeGroupId.HasValue || options.TypeGroupId == 0)
                options.TypeGroupId = _groupService.GetUserDefaultTypeGroup(user).Id;
            if (options.TypeGroupId > 0) options.TypeGroupCssClass = GetTypeGroup(options.TypeGroupId).CssClass;

            // Types
            options.Types = GetTypes();
            //if (options.TypeIds == null || options.TypeIds.Count() == 0) options.TypeIds = GetTypes().Where(a => a.Group.CssClass == "gp-house").Select(a => a.Id).ToArray();

            #region Provinces, Districts, Wards, Streets, Apartments

            // Provinces
            options.Provinces = _addressService.GetSelectListProvinces();
            if (options.Provinces[0].Value != "0") options.Provinces.Insert(0, new SelectListItem { Text = "-- Chọn tất cả Tỉnh/TP --", Value = "0" });
            if (!options.ProvinceId.HasValue) options.ProvinceId = _groupService.GetUserDefaultProvinceId(user);

            // Districts
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            if (options.DistrictIds == null || !options.DistrictIds.Any())
            {
                LocationDistrictPartRecord defaultDistrict = _groupService.GetUserDefaultDistrict(user);
                if (defaultDistrict != null) options.DistrictIds = new[] { defaultDistrict.Id };
            }

            // Wards
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Wards = _addressService.GetWards(options.DistrictIds);
            else
                options.Wards = _addressService.GetWards(0);

            // Streets
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Streets = _addressService.GetStreets(options.DistrictIds);
            else
                options.Streets = _addressService.GetStreetsByProvince(0);

            // Apartments
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Apartments = _addressService.GetApartments(options.DistrictIds);
            else
                options.Apartments = _addressService.GetApartments(0);

            #endregion

            // PaymentMethod
            options.PaymentMethods = GetPaymentMethods();
            if (options.PaymentMethodId == 0)
            {
                AdsTypePartRecord adsTypeLeasing = GetAdsType("ad-leasing");
                if (options.AdsTypeId == adsTypeLeasing.Id || options.AdsTypeCssClass == adsTypeLeasing.CssClass)
                    options.PaymentMethodId = GetPaymentMethod("pm-vnd-m").Id;
                else
                    options.PaymentMethodId = GetPaymentMethod("pm-vnd-b").Id;
            }
            options.PaymentMethodCssClass = GetPaymentMethod(options.PaymentMethodId).CssClass;
            options.PaymentMethodShortName = GetPaymentMethod(options.PaymentMethodId).ShortName;

            // PaymentUnit
            options.PaymentUnits = GetPaymentUnits();
            if (options.PaymentUnitId == 0) options.PaymentUnitId = GetPaymentUnit("unit-total").Id;

            // Directions, LegalStatus, Locations, Interiors
            options.Directions = GetDirections();
            options.LegalStatus = GetLegalStatus();
            options.Locations = GetLocations();
            options.Interiors = GetInteriors();

            // Advantages, DisAdvantages
            options.Advantages = GetAdvantagesEntries();
            options.DisAdvantages = GetDisAdvantagesEntries();
            options.ApartmentAdvantages = GetApartmentAdvantagesEntries();
            options.ApartmentInteriorAdvantages = GetApartmentInteriorAdvantagesEntries();

            // Users
            options.Users = _groupService.GetGroupUsers(user);

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            if (currentDomainGroup != null)
            {
                //ApproveAllGroup
                options.IsApproveAllGroup = currentDomainGroup.ApproveAllGroup;

                // Groups
                options.Groups = _groupService.GetGroups().Where(a => a.Id != currentDomainGroup.Id);

                //UserGroup by Domain
                options.UserGroupDomains = GetUserGroupDomains().Where(a => a.Id != currentDomainGroup.Id);
            }
            options.IsApprovedEntries = options.IsApproveAllGroup ? options.ApproveAllGroup : options.NotApproveAllGroup;

            // Other
            options.NeedUpdateDate =
                DateTime.Now.AddDays(-double.Parse(_settingService.GetSetting("DaysToUpdatePrice") ?? "90"));

            //Group Approved

            #endregion

            return options;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> SearchGroupProperties(PropertyIndexOptions options)
        {

            var statusCssClass = new List<string> { "st-selling", "st-new" };
            // -- Đang rao bán -- (RAO BÁN, KH RAO BÁN) (Đủ thông tin - Đã duyệt - Chưa hoàn chỉnh)
            List<int> statusIds = GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            int statusApproved = GetStatus().Where(r => r.CssClass.Contains("st-approved")).Select(r => r.Id).First();

            IContentQuery<PropertyPart, PropertyPartRecord> pList = Services.ContentManager
                .Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Published)
                .Where(p => statusIds.Contains(p.Status.Id) || (p.Status.Id == statusApproved && p.AdsExpirationDate > DateTime.Now));

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            if (currentDomainGroup.ApproveAllGroup)
            {
                List<int> groupExistIds = GetPropertyGroup(currentDomainGroup).Where(a => a.IsApproved == false).Select(a => a.PropertyId).ToList();
                if (options.GroupApproved == "NotApproved")
                {
                    pList = pList.Where(p => groupExistIds.Contains(p.Id) && p.UserGroup != currentDomainGroup);
                }
                else
                {
                    pList = pList.Where(p => !groupExistIds.Contains(p.Id) && p.UserGroup != currentDomainGroup);
                }
            }
            else
            {
                if (options.GroupApproved == "Approved")
                {
                    List<int> groupExistIds = GetPropertyGroup(currentDomainGroup).Where(a => a.IsApproved != false).Select(a => a.PropertyId).ToList();
                    pList = pList.Where(p => groupExistIds.Contains(p.Id));
                }
                else if (options.GroupApproved == "NotApproved")
                {
                    List<int> groupExistIds = GetPropertyGroup(currentDomainGroup).Where(a => a.IsApproved == false).Select(a => a.PropertyId).ToList();
                    pList = pList.Where(p => groupExistIds.Contains(p.Id));
                }
                else
                {
                    List<int> groupExistIds = GetPropertyGroup(currentDomainGroup).Select(a => a.PropertyId).ToList();
                    pList = pList.Where(p => !groupExistIds.Contains(p.Id) && p.UserGroup != currentDomainGroup);
                }
            }

            options = BuildGroupIndexOptions(options);

            #region FILTER

            if (options.Id > 0 || !String.IsNullOrEmpty(options.IdStr))
            {
                #region Id, IdStr

                // Id
                if (options.Id > 0) pList = pList.Where(p => p.Id == options.Id);

                // IdStr
                pList = pList.Where(p => p.IdStr.Contains(options.IdStr.Trim()));

                #endregion
            }
            else
            {
                #region Status

                // Status
                if (options.StatusId > 0)
                    pList = pList.Where(p => p.Status.Id == options.StatusId); // selected Status

                #endregion

                #region Flag

                // Flag
                if (options.FlagIds != null)
                    if (options.FlagIds.Any()) pList = pList.Where(p => options.FlagIds.Contains(p.Flag.Id));

                #endregion

                #region AdsType, TypeGroup, Type

                // AdsType
                if (options.AdsTypeId > 0) pList = pList.Where(p => p.AdsType.Id == options.AdsTypeId);

                // TypeGroup
                if (options.TypeGroupId > 0) pList = pList.Where(p => p.TypeGroup.Id == options.TypeGroupId);

                // Type
                if (options.TypeIds != null)
                    if (options.TypeIds.Any()) pList = pList.Where(p => options.TypeIds.Contains(p.Type.Id));

                // TypeConstruction
                if (options.TypeConstructionIds != null)
                    if (options.TypeConstructionIds.Any())
                        pList = pList.Where(p => options.TypeConstructionIds.Contains(p.TypeConstruction.Id));

                #endregion

                #region Address

                // Provinces
                if (options.ProvinceId > 0) pList = pList.Where(p => p.Province.Id == options.ProvinceId);

                // Districts
                if (options.DistrictIds != null)
                    if (options.DistrictIds.Any())
                        pList = pList.Where(p => options.DistrictIds.Contains(p.District.Id));

                // Wards
                if (options.WardIds != null)
                    if (options.WardIds.Any()) pList = pList.Where(p => options.WardIds.Contains(p.Ward.Id));

                // Streets
                if (options.StreetIds != null)
                    if (options.StreetIds.Any())
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

                // Street
                if (options.StreetId > 0)
                {
                    // Lấy tất cả BĐS trên Đường và Đoạn Đường
                    List<int> listRelatedStreetIds =
                        _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                            .Where(a => a.RelatedStreet.Id == options.StreetId)
                            .List()
                            .Select(a => a.Id)
                            .ToList();
                    listRelatedStreetIds.Add(options.StreetId ?? 0);
                    pList = pList.Where(p => listRelatedStreetIds.Contains(p.Street.Id));
                }

                // Apartments
                if (options.TypeGroupCssClass == "gp-apartment")
                {
                    if (options.ApartmentId > 0)
                    {
                        pList = pList.Where(p => p.Apartment != null && p.Apartment.Id == options.ApartmentId);
                    }
                    if (options.ApartmentIds != null)
                        if (options.ApartmentIds.Any())
                            pList =
                                pList.Where(p => p.Apartment != null && options.ApartmentIds.Contains(p.Apartment.Id));

                    if (!String.IsNullOrEmpty(options.ApartmentNumber))
                        pList =
                            pList.Where(
                                p =>
                                    p.AddressNumber.Contains(options.ApartmentNumber.Trim()) ||
                                    p.ApartmentNumber.Contains(options.ApartmentNumber.Trim()));
                }

                // AddressNumber
                if (!String.IsNullOrEmpty(options.AddressNumber))
                {
                    if (options.UseAccurateSearch)
                        pList =
                            pList.Where(
                                p =>
                                    p.AddressNumber == options.AddressNumber.Trim() ||
                                    p.ApartmentNumber == options.AddressNumber.Trim());
                    else
                        pList =
                            pList.Where(
                                p =>
                                    p.AddressNumber.Contains(options.AddressNumber.Trim()) ||
                                    p.ApartmentNumber.Contains(options.AddressNumber.Trim()));
                }
                if (!String.IsNullOrEmpty(options.AddressCorner))
                {
                    pList = options.UseAccurateSearch
                        ? pList.Where(p => p.AddressCorner == options.AddressCorner.Trim())
                        : pList.Where(p => p.AddressCorner.Contains(options.AddressCorner.Trim()));
                }
                if (!String.IsNullOrEmpty(options.Search))
                    pList =
                        pList.Where(
                            p =>
                                p.AddressNumber.Contains(options.Search.Trim()) ||
                                p.ApartmentNumber.Contains(options.Search.Trim()));

                #endregion

                #region Direction, LegalStatus, Location

                // Direction
                if (options.DirectionIds != null) // Khi chọn hướng thì phải ra cả những nhà chưa nhập hướng
                    if (options.DirectionIds.Any())
                        pList =
                            pList.Where(
                                p =>
                                    options.DirectionIds.Contains(p.Direction.Id) ||
                                    (options.UseAccurateSearch == false && p.Direction == null));

                // LegalStatus
                if (options.LegalStatusId > 0) pList = pList.Where(p => p.LegalStatus.Id == options.LegalStatusId);

                // Location
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.LocationId > 0) pList = pList.Where(p => p.Location.Id == options.LocationId);
                }

                #endregion

                #region Alley

                // Alley
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (!(options.LocationId > 0))
                    {
                        // Tìm tất cà nhà Hẻm và MT
                        PropertyLocationPartRecord frontLocation = GetLocation("h-front");

                        if (options.MinAlleyWidth > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        (p.AlleyWidth >= options.MinAlleyWidth ||
                                         (options.UseAccurateSearch == false && p.AlleyWidth == null)) ||
                                        p.Location == frontLocation);
                        if (options.MaxAlleyWidth > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        (p.AlleyWidth <= options.MaxAlleyWidth ||
                                         (options.UseAccurateSearch == false && p.AlleyWidth == null)) ||
                                        p.Location == frontLocation);

                        if (options.MinAlleyTurns > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        (p.AlleyTurns >= options.MinAlleyTurns ||
                                         (options.UseAccurateSearch == false && p.AlleyTurns == null)) ||
                                        p.Location == frontLocation);
                        if (options.MaxAlleyTurns > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        (p.AlleyTurns <= options.MaxAlleyTurns ||
                                         (options.UseAccurateSearch == false && p.AlleyTurns == null)) ||
                                        p.Location == frontLocation);

                        if (options.MinDistanceToStreet > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        (p.DistanceToStreet >= options.MinDistanceToStreet ||
                                         (options.UseAccurateSearch == false && p.DistanceToStreet == null)) ||
                                        p.Location == frontLocation);
                        if (options.MaxDistanceToStreet > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        (p.DistanceToStreet <= options.MaxDistanceToStreet ||
                                         (options.UseAccurateSearch == false && p.DistanceToStreet == null)) ||
                                        p.Location == frontLocation);
                    }
                    else
                    {
                        if (options.MinAlleyWidth > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        p.AlleyWidth >= options.MinAlleyWidth ||
                                        (options.UseAccurateSearch == false && p.AlleyWidth == null));
                        if (options.MaxAlleyWidth > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        p.AlleyWidth <= options.MaxAlleyWidth ||
                                        (options.UseAccurateSearch == false && p.AlleyWidth == null));

                        if (options.MinAlleyTurns > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        p.AlleyTurns >= options.MinAlleyTurns ||
                                        (options.UseAccurateSearch == false && p.AlleyTurns == null));
                        if (options.MaxAlleyTurns > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        p.AlleyTurns <= options.MaxAlleyTurns ||
                                        (options.UseAccurateSearch == false && p.AlleyTurns == null));

                        if (options.MinDistanceToStreet > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        p.DistanceToStreet >= options.MinDistanceToStreet ||
                                        (options.UseAccurateSearch == false && p.DistanceToStreet == null));
                        if (options.MaxDistanceToStreet > 0)
                            pList =
                                pList.Where(
                                    p =>
                                        p.DistanceToStreet <= options.MaxDistanceToStreet ||
                                        (options.UseAccurateSearch == false && p.DistanceToStreet == null));
                    }
                }

                #endregion

                #region Area

                // Area
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinAreaTotal > 0)
                        pList = pList.Where(p => p.AreaTotal >= options.MinAreaTotal || p.Area >= options.MinAreaTotal);
                    if (options.MaxAreaTotal > 0)
                        pList = pList.Where(p => p.AreaTotal <= options.MaxAreaTotal || p.Area <= options.MaxAreaTotal);

                    if (options.MinAreaTotalWidth > 0)
                        pList = pList.Where(p => p.AreaTotalWidth >= options.MinAreaTotalWidth);
                    if (options.MaxAreaTotalWidth > 0)
                        pList = pList.Where(p => p.AreaTotalWidth <= options.MaxAreaTotalWidth);

                    if (options.MinAreaTotalLength > 0)
                        pList = pList.Where(p => p.AreaTotalLength >= options.MinAreaTotalLength);
                    if (options.MaxAreaTotalLength > 0)
                        pList = pList.Where(p => p.AreaTotalLength <= options.MaxAreaTotalLength);
                }

                // AreaUsable
                if (options.TypeGroupCssClass == "gp-apartment")
                {
                    if (options.MinAreaUsable > 0)
                        pList = pList.Where(p => p.AreaUsable >= options.MinAreaUsable || p.Area >= options.MinAreaTotal);
                    if (options.MaxAreaUsable > 0)
                        pList = pList.Where(p => p.AreaUsable <= options.MaxAreaUsable || p.Area <= options.MaxAreaTotal);

                    // ApartmentFloorTh
                    if (options.MinApartmentFloorTh > 0)
                        pList = pList.Where(p => p.ApartmentFloorTh >= options.MinApartmentFloorTh);
                    if (options.MaxApartmentFloorTh > 0)
                        pList = pList.Where(p => p.ApartmentFloorTh <= options.MaxApartmentFloorTh);
                }

                #endregion

                #region Construction

                // Construction

                if (options.InteriorId > 0) pList = pList.Where(p => p.Interior.Id == options.InteriorId);

                // Floors
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinFloors > 0) pList = pList.Where(p => p.Floors >= options.MinFloors);
                    if (options.MaxFloors > 0) pList = pList.Where(p => p.Floors <= options.MaxFloors);
                }

                // Bedrooms
                if (options.MinBedrooms > 0) pList = pList.Where(p => p.Bedrooms >= options.MinBedrooms);
                if (options.MaxBedrooms > 0) pList = pList.Where(p => p.Bedrooms <= options.MaxBedrooms);

                // Bathrooms
                if (options.MinBathrooms > 0) pList = pList.Where(p => p.Bathrooms >= options.MinBathrooms);
                if (options.MaxBathrooms > 0) pList = pList.Where(p => p.Bathrooms <= options.MaxBathrooms);

                #endregion

                #region Price

                // Price

                // Convert Price to VND
                double minPriceVnd = options.MinPriceProposed > 0
                    ? ConvertToVndB((double)options.MinPriceProposed, options.PaymentMethodCssClass)
                    : 0;
                double maxPriceVnd = options.MaxPriceProposed > 0
                    ? ConvertToVndB((double)options.MaxPriceProposed, options.PaymentMethodCssClass)
                    : 0;


                if (minPriceVnd > 0) pList = pList.Where(p => p.PriceProposedInVND >= minPriceVnd);
                if (maxPriceVnd > 0) pList = pList.Where(p => p.PriceProposedInVND <= maxPriceVnd);
                if (minPriceVnd > 0 || maxPriceVnd > 0) pList.Where(p => p.PaymentUnit.Id == options.PaymentUnitId);

                #endregion

                #region ContactPhone

                if (!String.IsNullOrWhiteSpace(options.ContactPhone))
                    pList =
                        pList.Where(
                            p =>
                                p.ContactPhone.Contains(options.ContactPhone) ||
                                p.ContactPhoneToDisplay.Contains(options.ContactPhone));

                #endregion

                #region Users

                // Users

                if (!String.IsNullOrWhiteSpace(options.CreatedUserNameOrEmail))
                {
                    IContentQuery<UserPart, UserPartRecord> users =
                        _contentManager.Query<UserPart, UserPartRecord>()
                            .Where(
                                a =>
                                    a.UserName.Contains(options.CreatedUserNameOrEmail) ||
                                    a.Email.Contains(options.CreatedUserNameOrEmail));
                    if (users.Count() > 0)
                    {
                        List<int> userIds = users.List().Select(a => a.Id).ToList();
                        pList = pList.Where(p => userIds.Contains(p.CreatedUser.Id));
                    }
                }

                // Get date value

                const string format = "dd/MM/yyyy";
                CultureInfo provider = CultureInfo.InvariantCulture;
                const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                DateTime createdFrom, createdTo, lastUpdatedFrom, lastUpdatedTo, lastExportedFrom, lastExportedTo;

                DateTime.TryParseExact(options.CreatedFrom, format, provider, style, out createdFrom);
                DateTime.TryParseExact(options.CreatedTo, format, provider, style, out createdTo);

                DateTime.TryParseExact(options.LastUpdatedFrom, format, provider, style, out lastUpdatedFrom);
                DateTime.TryParseExact(options.LastUpdatedTo, format, provider, style, out lastUpdatedTo);

                DateTime.TryParseExact(options.LastExportedFrom, format, provider, style, out lastExportedFrom);
                DateTime.TryParseExact(options.LastExportedTo, format, provider, style, out lastExportedTo);

                if (options.CreatedUserId > 0) pList = pList.Where(p => p.CreatedUser.Id == options.CreatedUserId);
                if (createdFrom.Year > 1) pList = pList.Where(p => p.CreatedDate >= createdFrom);
                if (createdTo.Year > 1) pList = pList.Where(p => p.CreatedDate <= createdTo.AddHours(24));

                if (options.LastUpdatedUserId > 0)
                    pList = pList.Where(p => p.LastUpdatedUser.Id == options.LastUpdatedUserId);
                if (lastUpdatedFrom.Year > 1) pList = pList.Where(p => p.LastUpdatedDate >= lastUpdatedFrom);
                if (lastUpdatedTo.Year > 1) pList = pList.Where(p => p.LastUpdatedDate <= lastUpdatedTo.AddHours(24));

                if (options.LastExportedUserId > 0)
                    pList = pList.Where(p => p.LastExportedUser.Id == options.LastExportedUserId);
                if (lastExportedFrom.Year > 1) pList = pList.Where(p => p.LastExportedDate >= lastExportedFrom);
                if (lastExportedTo.Year > 1)
                    pList = pList.Where(p => p.LastExportedDate <= lastExportedTo.AddHours(24));

                if (options.FirstInfoFromUserId > 0)
                    pList = pList.Where(p => p.FirstInfoFromUser.Id == options.FirstInfoFromUserId);

                #endregion

                #region Group

                // GroupId
                if (options.GroupId > 0) pList = pList.Where(p => p.UserGroup.Id == options.GroupId);

                // GroupIds
                if (options.GroupIds != null)
                    if (options.GroupIds.Any()) pList = pList.Where(p => options.GroupIds.Contains(p.UserGroup.Id));

                #endregion

                #region Other

                // PriceEstimatedRatingPoint
                if (options.StatusCssClass == "st-estimate" && options.PriceEstimatedRatingPoint > 0)
                    pList = pList.Where(p => p.PriceEstimatedRatingPoint == options.PriceEstimatedRatingPoint);

                // ShowNeedUpdate
                if (options.ShowNeedUpdate) pList = pList.Where(p => p.LastUpdatedDate <= options.NeedUpdateDate);

                if (options.ShowExcludedInEstimation) pList = pList.Where(p => p.IsExcludeFromPriceEstimation);
                if (options.ShowIncludedInEstimation) pList = pList.Where(p => !p.IsExcludeFromPriceEstimation);

                #endregion

                #region Advantages, DisAdvantages

                //// Advantages

                //if (options.AdvantageIds != null)
                //    if (options.AdvantageIds.Any())
                //        foreach (var advId in options.AdvantageIds)
                //        {
                //            //_list = _list.Where(p => p.Advantages.Any(a => a.PropertyAdvantagePartRecord.Id == advId));
                //        }

                //// DisAdvantages

                //if (options.DisAdvantageIds != null)
                //    if (options.DisAdvantageIds.Any())
                //        foreach (var advId in options.DisAdvantageIds)
                //        {
                //            //_list = _list.Where(p => p.Advantages.Any(a => a.PropertyAdvantagePartRecord.Id == advId));
                //        }

                //// ApartmentAdvantages

                //if (options.ApartmentAdvantageIds != null)
                //    if (options.ApartmentAdvantageIds.Any())
                //        foreach (var advId in options.ApartmentAdvantageIds)
                //        {
                //            //_list = _list.Where(p => p.ApartmentAdvantages.Any(a => a.PropertyAdvantagePartRecord.Id == advId));
                //        }

                #endregion

                #region Ads Content

                // AdsType

                if (options.AdsHighlight) pList = pList.Where(p => p.AdsHighlight);
                if (options.AdsHighlightRequest) pList = pList.Where(p => p.AdsHighlightRequest);

                if (options.AdsGoodDeal) pList = pList.Where(p => p.AdsGoodDeal);
                if (options.AdsGoodDealRequest) pList = pList.Where(p => p.AdsGoodDealRequest);

                if (options.AdsVIP) pList = pList.Where(p => p.AdsVIP);

                // VIP1, VIP2, VIP3
                if (options.AdsVIP1 || options.AdsVIP2 || options.AdsVIP3)
                {
                    var adsVips = new List<int>();

                    if (options.AdsVIP1) adsVips.Add(3);
                    if (options.AdsVIP2) adsVips.Add(2);
                    if (options.AdsVIP3) adsVips.Add(1);

                    pList = pList.Where(p => adsVips.Contains(p.SeqOrder));
                }

                if (options.AdsVIPRequest) pList = pList.Where(p => p.AdsVIPRequest);

                if (options.AdsNormal)
                    pList =
                        pList.Where(
                            p => p.Published && p.AdsVIP == false && p.AdsHighlight == false && p.AdsGoodDeal == false);

                if (options.AdsRequest)
                    pList = pList.Where(p => p.AdsGoodDealRequest || p.AdsVIPRequest || p.AdsHighlightRequest);

                #endregion

                #region IsOwner, NoBroker, IsAuction, IsHighlights

                if (options.IsOwner) pList = pList.Where(p => p.IsOwner);
                if (options.NoBroker) pList = pList.Where(p => p.NoBroker);
                if (options.IsAuction) pList = pList.Where(p => p.IsAuction);
                if (options.IsHighlights) pList = pList.Where(p => p.IsHighlights);
                if (options.IsAuthenticatedInfo) pList = pList.Where(p => p.IsAuthenticatedInfo);

                #endregion

                #region AdsExpired, AdsNotExpired

                DateTime dateNow = DateTime.Now;

                if (options.AdsExpired) pList = pList.Where(p => p.AdsExpirationDate < dateNow);
                if (options.AdsNotExpired) pList = pList.Where(p => p.AdsExpirationDate >= dateNow);

                #endregion

                #region PublishAddress, PublishContact

                if (options.PublishAddress) pList = pList.Where(p => p.PublishAddress);
                if (options.PublishContact) pList = pList.Where(p => p.PublishContact);

                #endregion
            }

            #endregion

            #region ORDER

            switch (options.Order)
            {
                case PropertyOrder.LastUpdatedDate:
                    pList = pList.OrderByDescending(u => u.LastUpdatedDate);
                    break;
                case PropertyOrder.AddressNumber:
                    pList = pList.OrderBy(u => u.AlleyNumber);
                    break;
                case PropertyOrder.PriceProposedInVND:
                    pList = pList.OrderBy(u => u.PriceProposedInVND);
                    break;
            }

            #endregion

            return pList;
        }

        #endregion

        #region Group Properties

        public IContentQuery<PropertyPart, PropertyPartRecord> GetUserProperties(UserPart user)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> list =
                _contentManager.Query<PropertyPart, PropertyPartRecord>();

            // Show all properties
            if (Services.Authorizer.Authorize(Permissions.ManageProperties)) return list;

            UserGroupPartRecord jointGroup = _groupService.GetJointGroup(user.Id);
            UserGroupPartRecord ownGroup = _groupService.GetOwnGroup(user.Id);

            if (Services.Authorizer.Authorize(Permissions.MetaListProperties) && _settingService.CheckAllowedIPs() &&
                jointGroup != null)
            {
                // Show jointGroup properties
                list = GetGroupProperties(jointGroup);
            }
            else
            {
                if (ownGroup != null)
                {
                    // Show ownGroup properties
                    list = GetGroupProperties(ownGroup);
                }
                else
                {
                    List<int> includeLocationProvinceIds = _groupService.GetUserLocationProvinceIds(user);
                    List<int> includeLocationDistrictIds = _groupService.GetUserLocationDistrictIds(user);
                    List<int> includeLocationWardIds = _groupService.GetUserLocationWardIds(user);

                    // Show own properties only
                    list = list.Where(a => a.CreatedUser.Id == user.Id ||
                                           includeLocationProvinceIds.Contains(a.Province.Id) ||
                                           includeLocationDistrictIds.Contains(a.District.Id) ||
                                           includeLocationWardIds.Contains(a.Ward.Id));

                }
            }
            return list;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetGroupProperties(UserGroupPartRecord group)
        {

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            // Build query string
            Expression<Func<PropertyPartRecord, bool>> predicate = PredicateBuilder.False<PropertyPartRecord>();
            //predicate = predicate.Or(p => p.CreatedUser.Id == user.Id);

            // Group Users
            //List<int> relevantUserIds = _groupService.GetRelevantUserIds(group);
            //relevantUserIds.Add(group.GroupAdminUser.Id);
            //predicate = predicate.Or(p => relevantUserIds.Contains(p.CreatedUser.Id));

            #region Relevant GroupIds

            List<int> relevantGroupIds = _groupService.GetRelevantGroupIds(group);

            predicate = predicate.Or(p => relevantGroupIds.Contains(p.UserGroup.Id) || p.CreatedUser.Id == group.GroupAdminUser.Id);

            #endregion

            #region Access Locations

            // Group Locations
            List<int> includeGroupLocationProvinceIds = _groupService.GetGroupLocationProvinceIds(group);
            List<int> includeGroupLocationDistrictIds = _groupService.GetGroupLocationDistrictIds(group);
            List<int> includeGroupLocationWardIds = _groupService.GetGroupLocationWardIds(group);

            predicate = predicate.Or(p => includeGroupLocationProvinceIds.Contains(p.Province.Id));
            predicate = predicate.Or(p => includeGroupLocationDistrictIds.Contains(p.District.Id));
            predicate = predicate.Or(p => includeGroupLocationWardIds.Contains(p.Ward.Id));

            // User Locations
            List<int> includeUserLocationProvinceIds = _groupService.GetUserLocationProvinceIds(user);
            List<int> includeUserLocationDistrictIds = _groupService.GetUserLocationDistrictIds(user);
            List<int> includeUserLocationWardIds = _groupService.GetUserLocationWardIds(user);

            predicate = predicate.Or(p => includeUserLocationProvinceIds.Contains(p.Province.Id));
            predicate = predicate.Or(p => includeUserLocationDistrictIds.Contains(p.District.Id));
            predicate = predicate.Or(p => includeUserLocationWardIds.Contains(p.Ward.Id));

            #endregion

            #region Shared Locations

            // Group Shared Locations
            List<UserGroupSharedLocationRecord> sharedLocationRecords = _groupService.GetGroupSharedLocations(group);

            foreach (UserGroupSharedLocationRecord record in sharedLocationRecords)
            {
                if (record.LocationDistrictPartRecord != null &&
                    record.LocationWardPartRecord != null)
                {
                    predicate =
                        predicate.Or(
                            p =>
                                p.UserGroup == record.SeederUserGroupPartRecord &&
                                p.Province == record.LocationProvincePartRecord &&
                                p.District == record.LocationDistrictPartRecord &&
                                p.Ward == record.LocationWardPartRecord);
                }
                else if (record.LocationDistrictPartRecord != null)
                {
                    predicate =
                        predicate.Or(
                            p =>
                                p.UserGroup == record.SeederUserGroupPartRecord &&
                                p.Province == record.LocationProvincePartRecord &&
                                p.District == record.LocationDistrictPartRecord);
                }
                else
                {
                    predicate =
                        predicate.Or(
                            p =>
                                p.UserGroup == record.SeederUserGroupPartRecord &&
                                p.Province == record.LocationProvincePartRecord);
                }
            }

            #endregion

            var query = _contentManager.Query<PropertyPart, PropertyPartRecord>().Where(predicate);

            #region Retricted Locations

            // Retricted Locations
            List<UserLocationRecord> retrictedLocations = _groupService.GetUserRetrictedAccessGroupLocations(user);
            Expression<Func<PropertyPartRecord, bool>> predicateRetrictedLocations = PredicateBuilder.False<PropertyPartRecord>();
            //predicateRetrictedLocations = predicateRetrictedLocations.Or(p => p.CreatedUser.Id == user.Id);

            if (retrictedLocations.Count > 0)
            {
                foreach (UserLocationRecord record in retrictedLocations)
                {
                    if (record.LocationDistrictPartRecord != null && record.LocationWardPartRecord != null)
                    {
                        predicateRetrictedLocations = predicateRetrictedLocations.Or(p => p.Province == record.LocationProvincePartRecord && p.District == record.LocationDistrictPartRecord && p.Ward == record.LocationWardPartRecord);
                    }
                    else if (record.LocationDistrictPartRecord != null)
                    {
                        predicateRetrictedLocations = predicateRetrictedLocations.Or(p => p.Province == record.LocationProvincePartRecord && p.District == record.LocationDistrictPartRecord);
                    }
                    else
                    {
                        predicateRetrictedLocations = predicateRetrictedLocations.Or(p => p.Province == record.LocationProvincePartRecord);
                    }
                }


                // User Locations

                predicateRetrictedLocations = predicateRetrictedLocations.Or(p => includeUserLocationProvinceIds.Contains(p.Province.Id));
                predicateRetrictedLocations = predicateRetrictedLocations.Or(p => includeUserLocationDistrictIds.Contains(p.District.Id));
                predicateRetrictedLocations = predicateRetrictedLocations.Or(p => includeUserLocationWardIds.Contains(p.Ward.Id));

                query = query.Where(predicateRetrictedLocations);
            }

            #endregion

            // Filter
            return query;
        }

        #endregion

        #region Helper

        public string GetDisplayName(PropertyPart p)
        {
            if (p != null)
            {
                string displayName = p.CreatedUser.UserName;
                var userUpdate = Services.ContentManager.Get(p.CreatedUser.Id).As<UserUpdateProfilePart>();
                if (userUpdate != null && !string.IsNullOrEmpty(userUpdate.DisplayName))
                    displayName = userUpdate.DisplayName;

                return displayName;
            }
            return "congty"; //Default null
        }

        public string GetDisplayForContact(PropertyPart p)
        {
            string displayContact = "";

            if (p != null)
            {
                if (p.PublishContact == false)
                {
                    // Show ContactPhoneToDisplay
                    if (!String.IsNullOrEmpty(p.ContactPhoneToDisplay))
                        displayContact = p.ContactPhoneToDisplay;
                    else if (p.UserGroup != null)
                    {
                        // Get from table GroupContact
                        List<UserGroupContactRecord> contacts =
                            _groupContactRepository.Fetch(r => r.UserGroupPartRecord == p.UserGroup).ToList();
                        if (contacts.Any())
                        {
                            #region AdsType, TypeGroup

                            // check Province, District, AdsType, TypeGroup
                            if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == p.District && r.AdsTypePartRecord == p.AdsType &&
                                        r.PropertyTypeGroupPartRecord == p.TypeGroup))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == p.District &&
                                                        r.AdsTypePartRecord == p.AdsType &&
                                                        r.PropertyTypeGroupPartRecord == p.TypeGroup).ContactPhone;
                            }
                            // check Province, District = null, AdsType, TypeGroup
                            if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == null && r.AdsTypePartRecord == p.AdsType &&
                                        r.PropertyTypeGroupPartRecord == p.TypeGroup))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == p.AdsType &&
                                                        r.PropertyTypeGroupPartRecord == p.TypeGroup).ContactPhone;
                            }
                            // check Province = null, District = null, AdsType, TypeGroup
                            if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == null && r.LocationDistrictPartRecord == null &&
                                        r.AdsTypePartRecord == p.AdsType && r.PropertyTypeGroupPartRecord == p.TypeGroup))
                            {
                                displayContact =
                                    contacts.First(
                                        r =>
                                            r.LocationProvincePartRecord == null && r.LocationDistrictPartRecord == null &&
                                            r.AdsTypePartRecord == p.AdsType &&
                                            r.PropertyTypeGroupPartRecord == p.TypeGroup).ContactPhone;
                            }

                            #endregion

                            #region AdsType, TypeGroup = null

                            // check Province, District, AdsType, TypeGroup = null
                            if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == p.District && r.AdsTypePartRecord == p.AdsType &&
                                        r.PropertyTypeGroupPartRecord == null))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == p.District &&
                                                        r.AdsTypePartRecord == p.AdsType &&
                                                        r.PropertyTypeGroupPartRecord == null)
                                        .ContactPhone;
                            }
                            // check Province, District = null, AdsType, TypeGroup = null
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == null && r.AdsTypePartRecord == p.AdsType &&
                                        r.PropertyTypeGroupPartRecord == null))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == p.AdsType &&
                                                        r.PropertyTypeGroupPartRecord == null).ContactPhone;
                            }
                            // check Province = null, District = null, AdsType, TypeGroup = null
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == null &&
                                        r.LocationDistrictPartRecord == null && r.AdsTypePartRecord == p.AdsType &&
                                        r.PropertyTypeGroupPartRecord == null))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == null &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == p.AdsType &&
                                                        r.PropertyTypeGroupPartRecord == null).ContactPhone;
                            }
                            #endregion

                            #region AdsType = null, TypeGroup
                            // check Province, District, AdsType = null, TypeGroup
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == p.District &&
                                        r.AdsTypePartRecord == null &&
                                        r.PropertyTypeGroupPartRecord == p.TypeGroup))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == p.District &&
                                                        r.AdsTypePartRecord == null &&
                                                        r.PropertyTypeGroupPartRecord == p.TypeGroup)
                                        .ContactPhone;
                            }
                            // check Province, District = null, AdsType = null, TypeGroup
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == null &&
                                        r.AdsTypePartRecord == null &&
                                        r.PropertyTypeGroupPartRecord == p.TypeGroup))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == null &&
                                                        r.PropertyTypeGroupPartRecord == p.TypeGroup)
                                        .ContactPhone;
                            }
                            // check Province = null, District = null, AdsType = null, TypeGroup
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == null &&
                                        r.LocationDistrictPartRecord == null &&
                                        r.AdsTypePartRecord == null &&
                                        r.PropertyTypeGroupPartRecord == p.TypeGroup))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == null &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == null &&
                                                        r.PropertyTypeGroupPartRecord == p.TypeGroup)
                                        .ContactPhone;
                            }
                            #endregion

                            #region AdsType = null, TypeGroup = null
                            // check Province, District, AdsType = null, TypeGroup = null
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == p.District &&
                                        r.AdsTypePartRecord == null &&
                                        r.PropertyTypeGroupPartRecord == null))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == p.District &&
                                                        r.AdsTypePartRecord == null &&
                                                        r.PropertyTypeGroupPartRecord == null)
                                        .ContactPhone;
                            }
                            // check Province, District = null, AdsType = null, TypeGroup = null
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == p.Province &&
                                        r.LocationDistrictPartRecord == null &&
                                        r.AdsTypePartRecord == null &&
                                        r.PropertyTypeGroupPartRecord == null))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == p.Province &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == null &&
                                                        r.PropertyTypeGroupPartRecord == null)
                                        .ContactPhone;
                            }
                            // check Province = null, District = null, AdsType = null, TypeGroup = null
                            else if (
                                contacts.Any(
                                    r =>
                                        r.LocationProvincePartRecord == null &&
                                        r.LocationDistrictPartRecord == null &&
                                        r.AdsTypePartRecord == null &&
                                        r.PropertyTypeGroupPartRecord == null))
                            {
                                displayContact =
                                    contacts.First(r => r.LocationProvincePartRecord == null &&
                                                        r.LocationDistrictPartRecord == null &&
                                                        r.AdsTypePartRecord == null &&
                                                        r.PropertyTypeGroupPartRecord == null)
                                        .ContactPhone;
                            }

                            #endregion
                        }
                        // Show ContactNumber of District
                        else if (p.UserGroup.ShortName == "DPH" && p.District != null)
                        {
                            if (!String.IsNullOrEmpty(p.District.ContactPhone))
                                displayContact = p.District.ContactPhone;
                        }
                        // Show ContactNumber of group
                        else if (!String.IsNullOrEmpty(p.UserGroup.ContactPhone))
                        {
                            displayContact = p.UserGroup.ContactPhone;
                        }
                    }
                }

                if (String.IsNullOrEmpty(displayContact))
                    displayContact = p.ContactPhone;
            }

            return displayContact;
        }

        public double CalcArea(double? areaTotal, double? areaTotalWidth, double? areaTotalLength,
            double? areaTotalBackWidth)
        {
            double frontWidth = areaTotalWidth ?? 0;
            double backWidth = areaTotalBackWidth ?? frontWidth;
            double length = areaTotalLength ?? 0;

            return areaTotal ?? (frontWidth + backWidth) / 2 * length;
        }

        // Tính Diện tích khuôn viên
        public double CalcAreaTotal(PropertyPart p)
        {
            return CalcArea(p.AreaTotal, p.AreaTotalWidth, p.AreaTotalLength, p.AreaTotalBackWidth);
        }

        // Tính Diện tích hợp quy hoạch
        public double CalcAreaLegal(PropertyPart p)
        {
            return CalcArea(p.AreaLegal, p.AreaLegalWidth, p.AreaLegalLength, p.AreaLegalBackWidth);
        }

        // Tính Tổng diện tích sàn xây dựng
        public double CalcAreaConstructionFloor(PropertyPart p)
        {
            double areaConstruction = p.AreaConstruction ?? CalcAreaLegal(p);
            if (areaConstruction <= 0) areaConstruction = CalcAreaTotal(p);
            double areaConstructionFloor = p.AreaConstructionFloor ?? 0;

            if (areaConstructionFloor <= 0)
            {
                double constructionAreaCoeff = p.Floors ?? 0;
                // Tầng trệt (mặt đất) ground
                constructionAreaCoeff += 1; // ground
                // HẦM, LỬNG, SÂN THƯỢNG
                if (p.HaveBasement)
                    constructionAreaCoeff += double.Parse(_settingService.GetSetting("Co_Tang_Ham") ?? "2");
                if (p.HaveMezzanine)
                    constructionAreaCoeff += double.Parse(_settingService.GetSetting("Co_Gac_Lung") ?? "0.5");
                if (p.HaveTerrace)
                    constructionAreaCoeff += double.Parse(_settingService.GetSetting("Co_San_Thuong") ?? "1");

                areaConstructionFloor = areaConstruction * constructionAreaCoeff;
            }

            return areaConstructionFloor;
        }

        // Tính Diện tích dùng filter
        public double CalcAreaForFilter(PropertyPart p)
        {
            if (p.TypeGroup.CssClass == "gp-apartment")
                return p.AreaUsable ?? 0;

            return CalcAreaTotal(p);
        }

        // Tính Diện tích sử dụng
        public double CalcAreaUsable(PropertyPart p)
        {
            if (p.TypeGroup.CssClass == "gp-apartment")
                return p.AreaUsable ?? 0;

            double areaTotal = CalcAreaTotal(p);
            double areaConstruction = p.AreaConstruction ?? CalcAreaLegal(p);
            if (areaConstruction <= 0) areaConstruction = areaTotal;
            double areaConstructionFloor = CalcAreaConstructionFloor(p);

            double areaUsable = areaTotal - areaConstruction + areaConstructionFloor;

            if (areaUsable > 0)
                return areaUsable;
            return areaTotal;
        }

        // Tính Diện tích dùng tính giá rao / Tổng diện tích
        public double CalcAreaForPriceProposedInVnd(PropertyPart p)
        {
            if (p.TypeGroup.CssClass == "gp-apartment")
                return p.AreaUsable ?? 0;

            double areaLegal = CalcAreaLegal(p);
            if (areaLegal <= 0) areaLegal = CalcAreaTotal(p);

            return areaLegal;
        }

        public double? CaclPriceProposedInVnd(PropertyPart p)
        {
            if (p.PriceProposed > 0)
            {
                double priceProposedInVnd = 0;
                PaymentUnitPartRecord paymentUnitTotal = GetPaymentUnit("unit-total");
                PaymentUnitPartRecord paymentUnitM2 = GetPaymentUnit("unit-m2");
                PaymentMethodPartRecord paymentMethod = GetPaymentMethod(p.PaymentMethod.Id);

                if (p.PaymentUnit.Id == paymentUnitTotal.Id)
                {
                    priceProposedInVnd = ConvertToVndB(p.PriceProposed ?? 0, paymentMethod.CssClass);
                }
                else if (p.PaymentUnit.Id == paymentUnitM2.Id)
                {
                    double calcArea = CalcAreaForPriceProposedInVnd(p);
                    priceProposedInVnd = ConvertToVndB(p.PriceProposed ?? 0, paymentMethod.CssClass, calcArea);
                }
                return priceProposedInVnd;
            }
            return null;
        }

        public double? CalPriceProposedInVndRealArea(double realAreaUse, PropertyPart p)
        {
            PaymentMethodPartRecord paymentMethod = GetPaymentMethod(p.PaymentMethod.Id);
            return ConvertToVndB(p.PriceProposed ?? 0, paymentMethod.CssClass, realAreaUse);
        }

        /// <summary>
        ///     Thống kê trong từng BĐS đã có bao nhiêu khách xem (chỉ đưa ra số thống kê, không nêu cụ thể khách)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int CountVisitedCustomer(PropertyPart p)
        {
            int visitCount = 0;
            try
            {
                if (p.TypeGroup.CssClass == "gp-house")
                {
                    IEnumerable<PropertyPart> similarProperties =
                        GetInternalPropertiesByAddress(p.Province.Id, p.District.Id, p.Ward.Id, p.Street.Id, null,
                            p.AddressNumber, p.AddressCorner, null).Where(a => a.AdsType == p.AdsType).List();
                    visitCount +=
                        similarProperties.Sum(
                            item => _customerpropertyRepository.Fetch(a => a.PropertyPartRecord == item.Record).Count());
                }
                else if (p.TypeGroup.CssClass == "gp-apartment")
                {
                    IEnumerable<PropertyPart> similarProperties =
                        GetInternalPropertiesByAddress(p.Province.Id, p.District.Id, null, null, p.Apartment.Id, null,
                            null, p.ApartmentNumber).Where(a => a.AdsType == p.AdsType).List();
                    visitCount +=
                        similarProperties.Sum(
                            item => _customerpropertyRepository.Fetch(a => a.PropertyPartRecord == item.Record).Count());
                }
            }
            catch (Exception e)
            {
                Services.Notifier.Error(T("{0}", e.Message));
            }

            return visitCount;
        }

        #endregion

        #region VerifyAddress

        public IContentQuery<PropertyPart, PropertyPartRecord> SearchProperties(int? provinceId, int? districtId,
            int? wardId, int? streetId, int? apartmentId, string addressNumber, string addressCorner,
            string apartmentNumber)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> list =
                _contentManager.Query<PropertyPart, PropertyPartRecord>();

            // Province
            if (provinceId > 0) list = list.Where(a => a.Province != null && a.Province.Id == provinceId);

            // District
            if (districtId > 0) list = list.Where(a => a.District != null && a.District.Id == districtId);

            // Ward
            if (wardId > 0) list = list.Where(a => a.Ward != null && a.Ward.Id == wardId);

            // Street
            if (streetId > 0)
            {
                // Kiểm tra cả các đoạn đường
                var streetIds = new List<int> { (int)streetId };
                IEnumerable<LocationStreetPart> relatedStreets =
                    _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                        .Where(a => a.RelatedStreet.Id == streetId)
                        .List();
                if (relatedStreets != null) streetIds.AddRange(relatedStreets.Select(a => a.Id).ToList());

                list = list.Where(a => a.Street != null && streetIds.Contains(a.Street.Id));
            }

            // Apartment
            if (apartmentId > 0) list = list.Where(a => a.Apartment != null && a.Apartment.Id == apartmentId);

            // AddressNumber
            if (!String.IsNullOrEmpty(addressNumber)) list = list.Where(a => a.AddressNumber == addressNumber);

            // AddressCorner
            if (!String.IsNullOrEmpty(addressCorner)) list = list.Where(a => a.AddressCorner == addressCorner);

            // ApartmentNumber
            if (!String.IsNullOrEmpty(apartmentNumber)) list = list.Where(a => a.ApartmentNumber == apartmentNumber);

            return list;
        }

        #region Internal Properties

        public IContentQuery<PropertyPart, PropertyPartRecord> GetInternalPropertiesByAddress(int? provinceId,
            int? districtId, int? wardId, int? streetId, int? apartmentId, string addressNumber, string addressCorner,
            string apartmentNumber)
        {
            // Loại các property-status của khách hàng
            List<int> statusIds = GetStatusForExternal().Select(a => a.Id).ToList();
            int deletedStatusId = GetStatus("st-deleted").Id;
            statusIds.Add(deletedStatusId);

            return
                SearchProperties(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber)
                .Where(a => !statusIds.Contains(a.Status.Id));
        }

        public PropertyPart GetPropertyByAddress(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            AdsTypePartRecord adsType = GetAdsType(adsTypeCssClass);
            var pList = GetInternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId, addressNumber,
                    addressCorner, apartmentNumber)
                    .Where(a => a.AdsType == adsType)
                    .Where(a => a.Id != pId);
            if (pList.Count() > 0) return pList.Slice(1).FirstOrDefault();
            return null;
        }

        public bool VerifyPropertyUnicity(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            AdsTypePartRecord adsType = GetAdsType(adsTypeCssClass);
            var pList = GetInternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId, addressNumber,
                    addressCorner, apartmentNumber)
                    .Where(a => a.AdsType == adsType)
                    .Where(a => a.Id != pId);

            IUser currentUser = Services.WorkContext.CurrentUser;
            if (currentUser != null)
            {
                // Chỉ kiểm tra các bđs của group và các group con
                List<int> relevantGroupIds = _groupService.GetRelevantGroupIds(currentUser.As<UserPart>());
                if (pList.Where(a => relevantGroupIds.Contains(a.UserGroup.Id)).Count() > 0)
                    return false;
            }
            else
            {
                //dùng cho phần định giá khi user chưa đăng nhập.
                if (pList.Count() > 0)
                    return false;
            }
            return true;
        }

        #endregion

        #region External Properties

        public IContentQuery<PropertyPart, PropertyPartRecord> GetExternalPropertiesByAddress(int? provinceId,
            int? districtId, int? wardId, int? streetId, int? apartmentId, string addressNumber, string addressCorner,
            string apartmentNumber)
        {
            // Lấy các property-status của khách hàng
            List<int> statusIds = GetStatusForExternal().Select(a => a.Id).ToList();

            return
                SearchProperties(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber)
                .Where(a => statusIds.Contains(a.Status.Id));
        }

        public PropertyPart GetUserPropertyByAddress(int pId, int? provinceId, int? districtId, int? wardId,
            int? streetId, int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber,
            string adsTypeCssClass)
        {
            IUser currentUser = Services.WorkContext.CurrentUser;
            return GetUserPropertyByAddress(currentUser.Id, pId, provinceId, districtId, wardId, streetId,
                        apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);
        }

        public PropertyPart GetUserPropertyByAddress(int userId, int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            AdsTypePartRecord adsType = GetAdsType(adsTypeCssClass);
            var pList = GetExternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber)
                        .Where(a => a.AdsType == adsType)
                        .Where(a => a.Id != pId)
                        .Where(a => a.CreatedUser.Id == userId);

            if (pList.Count() > 0)
                return pList.Slice(1).FirstOrDefault();

            return null;
        }

        public bool VerifyUserPropertyUnicity(int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            IUser currentUser = Services.WorkContext.CurrentUser;
            return VerifyUserPropertyUnicity(currentUser.Id, pId, provinceId, districtId, wardId, streetId,
                        apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);
        }

        public bool VerifyUserPropertyUnicity(int userId, int pId, int? provinceId, int? districtId, int? wardId, int? streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            AdsTypePartRecord adsType = GetAdsType(adsTypeCssClass);
            var pList = GetExternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber)
                        .Where(a => a.AdsType == adsType)
                        .Where(a => a.Id != pId)
                        .Where(a => a.CreatedUser.Id == userId);

            if (pList.Count() > 0)
                return false;

            return true;
        }

        #endregion

        #endregion

        #region Permissions

        public bool EnableViewProperty(PropertyPart p, UserPart user)
        {
            // BĐS của user
            if (p.CreatedUser.Id == user.Id) return true;

            // Permissions.ManageProperties
            if (Services.Authorizer.Authorize(Permissions.ManageProperties) &&
                Services.Authorizer.Authorize(Permissions.NoRestrictedIP))
                return true;

            // Check if user is Supervisor of this Property's Group
            if (_groupService.IsSupervisor(p.UserGroup, user)) return true; // user is Supervisor

            // BĐS trong vùng cho phép chỉ được view, không được edit
            // Check if user is GroupAdmin, and property is in GroupLocations range
            if (_groupService.IsInGroupLocations(p, user)) return true;

            // BĐS trong vùng cho phép của user
            if (_groupService.IsInUserLocations(p, user)) return true;

            // Trong vùng IP cho phép
            if (_settingService.CheckAllowedIPs()) // user client nằm trong IP cho phép
            {
                UserGroupPartRecord jointGroup = _groupService.GetJointGroup(user.Id);

                // user chỉ được view BĐS của group hoặc các group con
                if (_groupService.IsSupervisor(p.UserGroup, jointGroup))
                {
                    if (p.Status.CssClass == "st-trading") // BĐS đang giao dịch
                        return Services.Authorizer.Authorize(Permissions.AccessTradingProperties,
                            T("Not authorized to edit properties"));

                    if (p.Status.CssClass == "st-sold") // BĐS đã giao dịch
                        return Services.Authorizer.Authorize(Permissions.AccessSoldProperties,
                            T("Not authorized to edit properties"));

                    if (p.Status.CssClass == "st-onhold") // BĐS ngưng giao dịch
                        return Services.Authorizer.Authorize(Permissions.AccessOnHoldProperties,
                            T("Not authorized to edit properties"));

                    if (p.Flag.CssClass == "deal-very-good") // BĐS giá rất rẻ
                        return Services.Authorizer.Authorize(Permissions.MetaListDealVeryGoodProperties,
                            T("Not authorized to edit properties"));

                    if (p.Flag.CssClass == "deal-good") // BĐS giá rẻ
                        return Services.Authorizer.Authorize(Permissions.MetaListDealGoodProperties,
                            T("Not authorized to edit properties"));

                    return true;
                }

                // BĐS trong vùng cho phép chỉ được view, không được edit

                // BĐS trong vùng cho phép của group
                if (_groupService.IsInGroupLocations(p, jointGroup)) return true;

            }

            return false;
        }

        public bool EnableEditProperty(PropertyPart p, UserPart user)
        {
            // BĐS của khách đăng
            if (p.Status != null && _externalStatusCssClass.Contains(p.Status.CssClass))
                return Services.Authorizer.Authorize(Permissions.ApproveProperty);

            // BĐS của user
            if (p.CreatedUser.Id == user.Id)
                return Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties"));

            // Permissions.ManageProperties
            if (Services.Authorizer.Authorize(Permissions.ManageProperties) &&
                Services.Authorizer.Authorize(Permissions.NoRestrictedIP))
                return true;

            // Check if user is Supervisor of this Property's Group
            if (_groupService.IsSupervisor(p.UserGroup, user)) return true; // user is Supervisor

            // BĐS trong vùng cho phép của user
            if (_groupService.IsInUserLocations(p, user)) return true;

            // Trong vùng IP cho phép
            if (_settingService.CheckAllowedIPs()) // user client nằm trong IP cho phép
            {
                UserGroupPartRecord jointGroup = _groupService.GetJointGroup(user.Id);

                // Check if BĐS nằm trong khu vực hoạt động của user (bị giới hạn)
                if (_groupService.IsInRetrictedAccessGroupLocations(p, user))
                {

                    // user chỉ được edit BĐS của group hoặc các group con
                    if (_groupService.IsSupervisor(p.UserGroup, jointGroup)
                        || _groupService.IsInGroupSharedLocations(p, user)) // Shared Location
                    {
                        if (p.Status != null && p.Status.CssClass == "st-trading") // BĐS đang giao dịch
                            return Services.Authorizer.Authorize(Permissions.AccessTradingProperties,
                                T("Not authorized to edit properties"));

                        if (p.Status != null && p.Status.CssClass == "st-sold") // BĐS đã giao dịch
                            return Services.Authorizer.Authorize(Permissions.AccessSoldProperties,
                                T("Not authorized to edit properties"));

                        if (p.Status != null && p.Status.CssClass == "st-onhold") // BĐS ngưng giao dịch
                            return Services.Authorizer.Authorize(Permissions.AccessOnHoldProperties,
                                T("Not authorized to edit properties"));

                        if (p.Flag.CssClass == "deal-very-good") // BĐS giá rất rẻ
                            return Services.Authorizer.Authorize(Permissions.MetaListDealVeryGoodProperties,
                                T("Not authorized to edit properties"));

                        if (p.Flag.CssClass == "deal-good") // BĐS giá rẻ
                            return Services.Authorizer.Authorize(Permissions.MetaListDealGoodProperties,
                                T("Not authorized to edit properties"));

                        if (p.CreatedUser.Id == user.Id) // BĐS của user
                            return Services.Authorizer.Authorize(Permissions.EditOwnProperty,
                                T("Not authorized to edit properties"));
                        return Services.Authorizer.Authorize(Permissions.EditProperty,
                            T("Not authorized to edit properties"));
                    }
                }
            }

            return false;
        }

        public bool EnableEditStatus(PropertyPart p, UserPart user)
        {
            if (p.Status != null)
            {
                // BĐS của user
                //if (p.CreatedUser == user) return Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties"));

                // Admin
                if (Services.Authorizer.Authorize(Permissions.ManageProperties)) return true;

                // user is Supervisor
                if (_groupService.IsSupervisor(p.UserGroup, user)) return true;

                // BĐS đang thương lượng (đặt cọc giữ chỗ)
                if (p.Status.CssClass == "st-negotiate") return Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties);

                // BĐS đang giao dịch (đặt cọc mua bán)
                if (p.Status.CssClass == "st-trading") return Services.Authorizer.Authorize(Permissions.AccessTradingProperties);

                // BĐS đã giao dịch
                if (p.Status.CssClass == "st-sold") return Services.Authorizer.Authorize(Permissions.AccessSoldProperties);

                // BĐS ngưng giao dịch
                if (p.Status.CssClass == "st-onhold") return Services.Authorizer.Authorize(Permissions.AccessOnHoldProperties);
            }
            return true;
        }

        public bool EnableViewAddressNumber(PropertyPart p, UserPart user)
        {
            return _cacheManager.Get("EnableViewAddressNumber_" + p.Id + "_" + user.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("EnableViewAddressNumber_Changed"));

                // BĐS của user
                if (p.CreatedUser.Id == user.Id)
                    return Services.Authorizer.Authorize(Permissions.EditOwnProperty,
                        T("Not authorized to edit properties"));

                // user is Supervisor
                if (_groupService.IsSupervisor(p.UserGroup, user)) return true;

                // Check if user is GroupAdmin, and property is in GroupLocations range
                if (_groupService.IsInGroupLocations(p, user)) return true;

                return _settingService.CheckAllowedIPs();
            });
        }

        public bool EnableEditAddressNumber(PropertyPart p, UserPart user)
        {
            // BĐS của user
            if (p.CreatedUser.Id == user.Id)
                return Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties"));

            // user is Supervisor
            if (_groupService.IsSupervisor(p.UserGroup, user)) return true;

            return Services.Authorizer.Authorize(Permissions.EditAddressNumber);
        }

        public bool EnableEditContactPhone(PropertyPart p, UserPart user)
        {
            // BĐS của user
            if (p.CreatedUser.Id == user.Id)
                return Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties"));

            // user is Supervisor
            if (_groupService.IsSupervisor(p.UserGroup, user)) return true;

            if (p.Status != null)
            {
                if (p.Status.CssClass == "st-negotiate")
                    return Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties);
                if (p.Status.CssClass == "st-trading")
                    return Services.Authorizer.Authorize(Permissions.AccessTradingProperties);
                if (p.Status.CssClass == "st-sold")
                    return Services.Authorizer.Authorize(Permissions.AccessSoldProperties);
                if (p.Status.CssClass == "st-onhold")
                    return Services.Authorizer.Authorize(Permissions.AccessOnHoldProperties);
            }
            return true;
        }

        public bool EnableEditPropertyImages(PropertyPart p, UserPart user)
        {
            // BĐS của user
            if (p.CreatedUser.Id == user.Id)
                return Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties"));

            // user is Supervisor
            if (_groupService.IsSupervisor(p.UserGroup, user)) return true;

            return Services.Authorizer.Authorize(Permissions.EditPropertyImages);
        }

        public bool EnableAddAdsGoodDeal(UserPart user)
        {
            return EnableAddAdsGoodDeal(_groupService.GetBelongGroup(user.Id));
        }

        public bool EnableAddAdsGoodDeal(UserGroupPartRecord group)
        {
            if (Services.Authorizer.Authorize(Permissions.SetAdsGoodDeal))
            {
                if (group != null)
                {
                    if (group.NumberOfAdsGoodDeal == -1) return true;

                    int usedAdsGoodDeal = _contentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.UserGroup == group && a.AdsGoodDeal && a.AdsGoodDealExpirationDate > DateTime.Now)
                        .Count();
                    if (group.NumberOfAdsGoodDeal > usedAdsGoodDeal)
                        return true;
                    Services.Notifier.Warning(
                        T(
                            "Bạn đã hết hạn mức đăng tin \"BĐS giá rẻ\"; Hạn mức của {0} là {1} tin. Để tăng thêm hạn mức vui lòng liên hệ ban quản trị.",
                            @group.Name, @group.NumberOfAdsGoodDeal));
                    return false;
                }
                return true;
            }

            return false;
        }

        //public bool EnableAddAdsVIP(UserPartRecord user)
        //{
        //    return EnableAddAdsVIP(_groupService.GetBelongGroup(user));
        //}
        //public bool EnableAddAdsVIP(UserGroupPartRecord group)
        //{
        //    if (Services.Authorizer.Authorize(Permissions.SetAdsVIP))
        //    {
        //        if (group != null)
        //        {
        //            if (group.NumberOfAdsVIP == -1) return true;

        //            int usedAdsVIP = _contentManager.Query<PropertyPart, PropertyPartRecord>()
        //                .Where(a => a.UserGroup == group && a.AdsVIP == true && a.AdsVIPExpirationDate > DateTime.Now).Count();
        //            if (group.NumberOfAdsVIP > usedAdsVIP)
        //                return true;
        //            else
        //            {
        //                Services.Notifier.Warning(T("Bạn đã hết hạn mức đăng tin \"BĐS giao dịch gấp\"; Hạn mức của {0} là {1} tin. Để tăng thêm hạn mức vui lòng liên hệ ban quản trị.", group.Name, group.NumberOfAdsVIP));
        //                return false;
        //            }
        //        }
        //        return true;
        //    }

        //    return false;
        //}

        public bool EnableAddAdsHighlight(UserPart user)
        {
            return EnableAddAdsHighlight(_groupService.GetBelongGroup(user.Id));
        }

        public bool EnableAddAdsHighlight(UserGroupPartRecord group)
        {
            if (Services.Authorizer.Authorize(Permissions.SetAdsHighlight))
            {
                if (group != null)
                {
                    if (group.NumberOfAdsHighlight == -1) return true;

                    int usedAdsHighlight = _contentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(
                            a => a.UserGroup == group && a.AdsHighlight && a.AdsHighlightExpirationDate > DateTime.Now)
                        .Count();
                    if (group.NumberOfAdsHighlight > usedAdsHighlight)
                        return true;
                    Services.Notifier.Warning(
                        T(
                            "Bạn đã hết hạn mức đăng tin \"BĐS nổi bật\"; Hạn mức của {0} là {1} tin. Để tăng thêm hạn mức vui lòng liên hệ ban quản trị.",
                            @group.Name, @group.NumberOfAdsHighlight));
                    return false;
                }
                return true;
            }

            return false;
        }

        #endregion

        #region IsValid

        /// <summary>
        /// BĐS đủ thông tin
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsValid(PropertyPart p)
        {
            bool isValid = false;

            if (p.TypeGroup != null)
            {
                switch (p.TypeGroup.CssClass)
                {
                    case "gp-house":
                        bool validAddress = p.District != null &&
                                            (p.Ward != null || !String.IsNullOrEmpty(p.OtherWardName)) &&
                                            (p.Street != null || !String.IsNullOrEmpty(p.OtherStreetName)) &&
                                            !String.IsNullOrEmpty(p.AddressNumber);
                        bool validAlley = (p.Location.CssClass == "h-front") ||
                                          (p.Location.CssClass == "h-alley" && p.AlleyTurns > 0 &&
                                           (p.AlleyWidth1 > 0 || p.AlleyWidth2 > 0 || p.AlleyWidth3 > 0 ||
                                            p.AlleyWidth4 > 0 || p.AlleyWidth5 > 0 || p.AlleyWidth6 > 0 ||
                                            p.AlleyWidth7 > 0 || p.AlleyWidth8 > 0 || p.AlleyWidth9 > 0));
                        bool validArea = (p.AreaTotal > 0 || (p.AreaTotalLength > 0 && p.AreaTotalWidth > 0)) &&
                                         (p.AreaLegal > 0 || (p.AreaLegalLength > 0 && p.AreaLegalWidth > 0));
                        bool validPrice = p.PriceProposed > 0 || p.PriceNegotiable;
                        bool validContact = !String.IsNullOrEmpty(p.ContactPhone);

                        isValid = validAddress && validAlley && validArea && validPrice && validContact;
                        break;
                    case "gp-apartment":
                        validAddress = p.District != null &&
                                       (p.Apartment != null || !String.IsNullOrEmpty(p.OtherProjectName));
                        validArea = p.AreaUsable > 0;
                        validPrice = p.PriceProposed > 0 || p.PriceNegotiable;
                        validContact = !String.IsNullOrEmpty(p.ContactPhone);

                        isValid = validAddress && validArea && validPrice && validContact;
                        break;
                    case "gp-land":
                        validAddress = p.District != null && (p.Ward != null || !String.IsNullOrEmpty(p.OtherWardName));
                        validArea = (p.AreaTotal > 0 || (p.AreaTotalLength > 0 && p.AreaTotalWidth > 0));
                        validPrice = p.PriceProposed > 0 || p.PriceNegotiable;
                        validContact = !String.IsNullOrEmpty(p.ContactPhone);

                        isValid = validAddress && validArea && validPrice && validContact;
                        break;
                }
            }

            return isValid;
        }

        // Copy BĐS từ Group khác
        public bool IsValidForCopyToGroup(PropertyPart p)
        {
            // BĐS của Group

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord jointGroup = _groupService.GetJointGroup(user.Id);
            UserGroupPartRecord ownGroup = _groupService.GetOwnGroup(user.Id);

            // BĐS của khách đăng thì đc phép copy vào BĐS nội bộ
            if (p.Status != null && !_externalStatusCssClass.Contains(p.Status.CssClass))
            {
                if (p.UserGroup != null)
                {
                    if (p.UserGroup == jointGroup || p.UserGroup == ownGroup)
                        return false;
                }
            }

            return IsValidToPublish(p);
        }

        public bool IsValidForCopyToAdsType(PropertyPart p)
        {
            string copyToAdsTypeCssClass = p.AdsType.CssClass == "ad-selling" ? "ad-leasing" : "ad-selling";

            return IsValidToPublish(p, copyToAdsTypeCssClass);
        }

        public bool IsValidToPublish(PropertyPart p)
        {
            return IsValidToPublish(p, p.AdsType.CssClass);
        }

        public bool IsValidToPublish(PropertyPart p, string adsTypeCssClass)
        {
            bool isValid = true;

            if (String.IsNullOrEmpty(adsTypeCssClass) && p.AdsType != null) adsTypeCssClass = p.AdsType.CssClass;

            if (p.TypeGroup.CssClass == "gp-house")
            {
                #region VALIDATION

                #region Province, District, Ward, Street, AddressNumber

                // Province
                if (p.Province == null) isValid = false;

                // District
                if (p.District == null) isValid = false;

                // Ward
                if (p.Ward == null && String.IsNullOrEmpty(p.OtherWardName)) isValid = false;

                // Street
                if (p.Street == null && String.IsNullOrEmpty(p.OtherStreetName)) isValid = false;

                // AddressNumber
                if (String.IsNullOrEmpty(p.AddressNumber)) isValid = false;

                #endregion

                #region AreaTotal, AreaLegal

                // AreaTotal
                if (!(p.AreaTotal > 0 || (p.AreaTotalLength > 0 && p.AreaTotalWidth > 0))) return false;

                // AreaLegal
                if (!(p.AreaLegal > 0 || (p.AreaLegalLength > 0 && p.AreaLegalWidth > 0))) return false;

                double areaTotal = CalcAreaTotal(p);
                double areaLegal = CalcAreaLegal(p);

                // AreaTotal & AreaLegal
                if (areaTotal > 0 && areaLegal > 0 && areaTotal < areaLegal) return false;

                // AreaIlegalRecognized
                if (p.AreaIlegalRecognized > 0 && areaTotal > 0 && areaLegal > 0 && (areaTotal - areaLegal + 0.1) < p.AreaIlegalRecognized) return false;

                // AreaConstruction
                if (p.AreaConstruction > 0 && areaTotal > 0 && areaTotal < p.AreaConstruction) return false;

                // AreaResidential
                if (p.AreaResidential > 0 && areaTotal > 0 && areaTotal < p.AreaResidential) return false;

                #endregion

                #endregion

                if (p.Province != null && p.District != null && p.Ward != null && p.Street != null &&
                    !String.IsNullOrEmpty(p.AddressNumber))
                {
                    if (
                        !VerifyPropertyUnicity(0, p.Province.Id, p.District.Id, p.Ward.Id, p.Street.Id, null,
                            p.AddressNumber, p.AddressCorner, null, adsTypeCssClass))
                        isValid = false; // Nếu không unique thì return false
                }
            }
            else if (p.TypeGroup.CssClass == "gp-apartment")
            {
                #region VALIDATION

                #region Province, District, Apartment, ApartmentNumber

                // Province
                if (p.Province == null) isValid = false;

                // District
                if (p.District == null) isValid = false;

                // Apartment
                if (p.Apartment == null && String.IsNullOrEmpty(p.OtherProjectName)) isValid = false;

                // ApartmentNumber
                if (String.IsNullOrEmpty(p.ApartmentNumber)) isValid = false;

                #endregion

                #region AreaUsable

                // AreaUsable
                if (!p.AreaUsable.HasValue) isValid = false;

                #endregion

                #endregion

                if (p.Province != null && p.District != null && p.Apartment != null &&
                    !String.IsNullOrEmpty(p.ApartmentNumber))
                {
                    if (
                        !VerifyPropertyUnicity(0, p.Province.Id, p.District.Id, null, null, p.Apartment.Id, null, null,
                            p.ApartmentNumber, adsTypeCssClass))
                        isValid = false; // Nếu không unique thì return false
                }
            }

            return isValid;
        }

        /// <summary>
        /// BĐS đủ thông tin để định giá
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool IsValidForEstimate(PropertyPart p)
        {
            // TypeGroup
            if (p.TypeGroup == null || p.TypeGroup.CssClass != "gp-house") return false;

            // Address
            if (p.Province == null) return false;
            if (p.District == null) return false;
            if (p.Ward == null) return false;
            if (p.Street == null) return false;
            if (String.IsNullOrEmpty(p.AddressNumber)) return false;

            // Location
            if (p.Location == null) return false;
            if (p.Location.CssClass == "h-alley")
            {
                if (p.AlleyTurns >= 1 && (!p.AlleyWidth1.HasValue || p.AlleyWidth1 <= 0)) return false;
                if (p.AlleyTurns >= 2 && (!p.AlleyWidth2.HasValue || p.AlleyWidth2 <= 0)) return false;
                if (p.AlleyTurns >= 3 && (!p.AlleyWidth3.HasValue || p.AlleyWidth3 <= 0)) return false;
                if (p.AlleyTurns >= 4 && (!p.AlleyWidth4.HasValue || p.AlleyWidth4 <= 0)) return false;
                if (p.AlleyTurns >= 5 && (!p.AlleyWidth5.HasValue || p.AlleyWidth5 <= 0)) return false;
                if (p.AlleyTurns >= 6 && (!p.AlleyWidth6.HasValue || p.AlleyWidth6 <= 0)) return false;
                if (p.AlleyTurns >= 7 && (!p.AlleyWidth7.HasValue || p.AlleyWidth7 <= 0)) return false;
                if (p.AlleyTurns >= 8 && (!p.AlleyWidth8.HasValue || p.AlleyWidth8 <= 0)) return false;
                if (p.AlleyTurns >= 9 && (!p.AlleyWidth9.HasValue || p.AlleyWidth9 <= 0)) return false;
            }

            // AreaTotal 
            //if (!(p.AreaTotal > 0 || (p.AreaTotalLength > 0 && p.AreaTotalWidth > 0))) return false;

            // AreaLegal
            if (!(p.AreaLegal > 0 || (p.AreaLegalLength > 0 && p.AreaLegalWidth > 0))) return false;

            //double areaTotal = CalcAreaTotal(p);
            double areaLegal = CalcAreaLegal(p);

            // AreaTotal & AreaLegal
            //if (areaTotal > 0 && areaLegal > 0 && areaTotal < areaLegal) return false;

            // AreaIlegalRecognized
            //if (p.AreaIlegalRecognized > 0 && areaTotal > 0 && areaLegal > 0 && areaTotal > areaLegal && (areaTotal - areaLegal + 0.1) < p.AreaIlegalRecognized) return false;

            // AreaConstruction
            //if (p.AreaConstruction > 0 && areaTotal > 0 && areaTotal < p.AreaConstruction) return false;

            return true;
        }

        public bool IsExternalProperty(PropertyPart p)
        {
            if (p == null) return false;
            if (p.Status == null) return false;
            return _externalStatusCssClass.Contains(p.Status.CssClass);
        }

        public bool VerifyPropertyGroupExist(int propertyId, int userGroupId)
        {
            return
                _contentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>()
                    .Where(a => a.PropertyId == propertyId && a.UserGroupId == userGroupId)
                    .List()
                    .Any();
        }

        #endregion

        #region BuildViewModel

        public PropertyCreateViewModel BuildCreateViewModel(string adsTypeCssClass, string typeGroupCssClass)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            int paymentMethodId = 0;
            int provinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;
            int districtId = 0;
            int apartmentId = 0;

            // Get Default from Group Setting
            UserGroupPartRecord group = _groupService.GetBelongGroup(user.Id);
            if (group != null)
            {
                if (group.DefaultProvince != null) provinceId = group.DefaultProvince.Id;
                if (group.DefaultDistrict != null) districtId = group.DefaultDistrict.Id;
                if (String.IsNullOrEmpty(adsTypeCssClass))
                {
                    if (group.DefaultAdsType != null)
                    {
                        adsTypeCssClass = group.DefaultAdsType.CssClass;
                        if (group.DefaultAdsType.CssClass == "ad-leasing")
                        {
                            paymentMethodId = GetPaymentMethod("pm-vnd-m").Id;
                        }
                    }
                }
                if (String.IsNullOrEmpty(typeGroupCssClass))
                {
                    if (group.DefaultTypeGroup != null) typeGroupCssClass = group.DefaultTypeGroup.CssClass;
                }
            }

            if (String.IsNullOrEmpty(adsTypeCssClass)) adsTypeCssClass = "ad-selling";

            //AdsPaymentHistoryPart amount = _adsPaymentService.GetPaymentHistoryLasted(user);

            return new PropertyCreateViewModel
            {
                LastInfoFromUserId = user.Id, // Current User
                Users = _groupService.GetGroupUsers(user),
                AdsTypeCssClass = adsTypeCssClass,
                AdsTypes = GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing"),
                AddAdsExpirationDate = ExpirationDate.Day90,
                TypeGroups = GetTypeGroups(),
                TypeGroupCssClass = typeGroupCssClass,
                Types = GetTypes(adsTypeCssClass, typeGroupCssClass),
                TypeConstructions = GetTypeConstructions(null, null),
                ProvinceId = provinceId,
                Provinces = _addressService.GetSelectListProvinces(),
                Districts = _addressService.GetDistricts(provinceId),
                Wards = _addressService.GetWards(districtId),
                Streets = _addressService.GetStreets(districtId),
                Apartments = _addressService.GetApartments(districtId),
                ApartmentBlocks = _apartmentService.LocationApartmentBlockParts(apartmentId),
                LegalStatus = GetLegalStatus(),
                Directions = GetDirections(),
                Locations = GetLocations(),
                Interiors = GetInteriors(),
                FloorsCount = 0,
                PaymentMethodId = paymentMethodId,
                PaymentMethods = GetPaymentMethods(),
                PaymentUnits = GetPaymentUnits(),
                Flags = GetFlags(),
                Status = GetStatusForInternal(),
                ChkOtherDistrictName = false,
                ChkOtherProvinceName = false,
                ChkOtherStreetName = false,
                ChkOtherWardName = false,
                Published = true,
                Advantages = GetAdvantagesEntries(),
                DisAdvantages = GetDisAdvantagesEntries(),
                ApartmentAdvantages = GetApartmentAdvantagesEntries(),
                ApartmentInteriorAdvantages = GetApartmentInteriorAdvantagesEntries(),

                // Permissions
                EnableSetAdsGoodDeal = Services.Authorizer.Authorize(Permissions.SetAdsGoodDeal),
                EnableSetAdsVIP = Services.Authorizer.Authorize(Permissions.SetAdsVIP),
                EnableSetAdsHighlight = Services.Authorizer.Authorize(Permissions.SetAdsHighlight),
                PriceNegotiable = false,

                IsManageAddAdsPayment = Services.Authorizer.Authorize(Permissions.ManageAddAdsPayment),

                //HaveFacebookUserId = _facebookApiSevice.HaveFacebookUserId(),
                AcceptPostToFacebok = true,

                //Default Property VIP
                AdsVIP = true,
                AdsTypeVIPId = AdsTypeVIP.VIP3,
                AddAdsVIPExpirationDate = ExpirationDate.Day30
            };
        }

        public PropertyCreateViewModel BuildCreateViewModel(PropertyCreateViewModel createModel)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            //AdsPaymentHistoryPart amount = _adsPaymentService.GetPaymentHistoryLasted(user);

            createModel.Users = _groupService.GetGroupUsers(user);

            createModel.AdsTypes = GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing");

            createModel.TypeGroups = GetTypeGroups();
            createModel.Types = GetTypes(createModel.AdsTypeCssClass, createModel.TypeGroupCssClass);
            createModel.TypeConstructions = GetTypeConstructions(createModel.TypeId, createModel.Floors);

            createModel.Provinces = _addressService.GetSelectListProvinces();
            createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
            createModel.Wards = _addressService.GetWards(createModel.DistrictId);
            createModel.Streets = _addressService.GetStreets(createModel.DistrictId);
            createModel.Apartments = _addressService.GetApartments(createModel.DistrictId);
            createModel.ApartmentBlocks = _apartmentService.LocationApartmentBlockParts(createModel.ApartmentId != null ? createModel.ApartmentId.Value : 0);

            createModel.LegalStatus = GetLegalStatus();
            createModel.Directions = GetDirections();
            createModel.Locations = GetLocations();

            createModel.Interiors = GetInteriors();

            createModel.PaymentMethods = GetPaymentMethods();
            createModel.PaymentUnits = GetPaymentUnits();

            createModel.Flags = GetFlags();
            createModel.Status = GetStatusForInternal();

            createModel.Advantages = GetAdvantagesEntries();
            createModel.DisAdvantages = GetDisAdvantagesEntries();
            createModel.ApartmentAdvantages = GetApartmentAdvantagesEntries();
            createModel.ApartmentInteriorAdvantages = GetApartmentInteriorAdvantagesEntries();

            createModel.IsManageAddAdsPayment = Services.Authorizer.Authorize(Permissions.ManageAddAdsPayment);

            //Default Property VIP
            createModel.AdsVIP = true;
            createModel.AdsTypeVIPId = AdsTypeVIP.VIP3;
            createModel.AddAdsVIPExpirationDate = ExpirationDate.Day30;

            return createModel;
        }

        public PropertyEditViewModel BuildEditViewModel(PropertyPart p)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            string adsTypeCssClass = p.AdsType != null ? p.AdsType.CssClass : "ad-selling";
            string typeGroupCssClass = p.Type != null ? p.Type.Group.CssClass : "";
            string statusCssClass = p.Status != null ? p.Status.CssClass : "";

            int provinceId = p.Province != null ? p.Province.Id : 0;
            int districtId = p.District != null ? p.District.Id : 0;
            int wardId = p.Ward != null ? p.Ward.Id : 0;
            int streetId = p.Street != null
                ? (p.Street.RelatedStreet != null ? p.Street.RelatedStreet.Id : p.Street.Id)
                : 0;
            int apartmentId = p.Apartment != null ? p.Apartment.Id : 0;
            int apartmentBlockId = p.Apartment != null && p.ApartmentBlock != null ? p.ApartmentBlock.Id : 0;

            IEnumerable<int> advantageIds = GetPropertyAdvantages(p).Select(a => a.Id);
            IEnumerable<int> disadvantageIds = GetPropertyDisAdvantages(p).Select(a => a.Id);
            IEnumerable<int> apartmentAdvantageIds = GetPropertyApartmentAdvantages(p).Select(a => a.Id);
            IEnumerable<int> apartmentInteriorAdvantageIds = GetPropertyApartmentInteriorAdvantages(p)
                .Select(a => a.Id);

            //AdsPaymentHistoryPart amount = _adsPaymentService.GetPaymentHistoryLasted(user);

            return new PropertyEditViewModel
            {
                Property = p,
                AdsTypeCssClass = adsTypeCssClass,
                AdsTypes = GetAdsTypes().Where(a => a.CssClass == "ad-selling" || a.CssClass == "ad-leasing"),
                TypeGroupCssClass = typeGroupCssClass, // "gp-house", "gp-apartment", "gp-land"
                TypeGroups = GetTypeGroups(),//.Where(a => a.CssClass == typeGroupCssClass),
                TypeId = p.Type != null ? p.Type.Id : 0,
                Types = GetTypes(adsTypeCssClass, typeGroupCssClass),
                TypeConstructionId = p.TypeConstruction != null ? p.TypeConstruction.Id : 0,
                TypeConstructions = GetTypeConstructions(p.Type != null ? p.Type.Id : 0, p.Floors),
                FlagId = p.Flag != null ? p.Flag.Id : 0,
                FlagCssClass = p.Flag != null ? p.Flag.CssClass : "",
                Flags = GetFlags(),
                StatusCssClass = p.Status != null ? p.Status.CssClass : "",
                StatusName = p.Status != null ? p.Status.Name : "",
                Status = GetStatusForTargetCssClass(statusCssClass),
                LastInfoFromUserId = p.LastInfoFromUser != null ? p.LastInfoFromUser.Id : 0,
                Users = _groupService.GetGroupUsers(user),
                ProvinceId = provinceId,
                Provinces = _addressService.GetSelectListProvinces(),
                DistrictId = districtId,
                Districts = _addressService.GetDistricts(provinceId),
                WardId = wardId,
                Wards = _addressService.GetWards(districtId),
                StreetId = streetId,
                Streets = _addressService.GetStreets(districtId),
                ApartmentId = apartmentId,
                ApartmentBlockId = apartmentBlockId,
                Apartments = _addressService.GetApartments(districtId),
                ApartmentBlocks = _apartmentService.LocationApartmentBlockParts(apartmentId),
                ChkOtherProvinceName = p.Province == null && !String.IsNullOrEmpty(p.OtherProvinceName),
                ChkOtherDistrictName = p.District == null && !String.IsNullOrEmpty(p.OtherDistrictName),
                ChkOtherWardName = p.Ward == null && !String.IsNullOrEmpty(p.OtherWardName),
                ChkOtherStreetName = p.Street == null && !String.IsNullOrEmpty(p.OtherStreetName),
                ChkOtherProjectName = p.Apartment == null && !String.IsNullOrEmpty(p.OtherProjectName),
                LegalStatusId = p.LegalStatus != null ? p.LegalStatus.Id : 0,
                LegalStatus = GetLegalStatus(),
                DirectionId = p.Direction != null ? p.Direction.Id : 0,
                Directions = GetDirections(),
                LocationCssClass = p.Location != null ? p.Location.CssClass : "",
                Locations = GetLocations(),
                FloorsCount = p.Floors > 10 ? -1 : p.Floors,
                InteriorId = p.Interior != null ? p.Interior.Id : 0,
                Interiors = GetInteriors(),
                Advantages =
                    GetAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = advantageIds.Contains(r.Id) })
                        .ToList(),
                DisAdvantages =
                    GetDisAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = disadvantageIds.Contains(r.Id) })
                        .ToList(),
                ApartmentAdvantages =
                    GetApartmentAdvantages()
                        .Select(
                            r =>
                                new PropertyAdvantageEntry
                                {
                                    Advantage = r,
                                    IsChecked = apartmentAdvantageIds.Contains(r.Id)
                                })
                        .ToList(),
                ApartmentInteriorAdvantages =
                    GetApartmentInteriorAdvantages()
                        .Select(
                            r =>
                                new PropertyAdvantageEntry
                                {
                                    Advantage = r,
                                    IsChecked = apartmentInteriorAdvantageIds.Contains(r.Id)
                                })
                        .ToList(),
                PaymentMethodId = p.PaymentMethod != null ? p.PaymentMethod.Id : 0,
                PaymentMethods = GetPaymentMethods(),
                PaymentUnitId = p.PaymentUnit != null ? p.PaymentUnit.Id : 0,
                PaymentUnits = GetPaymentUnits(),

                // CopyToAdsType
                AdsTypeCssClassCopy = adsTypeCssClass == "ad-selling" ? "ad-leasing" : "ad-selling",
                PublishedCopy = p.Published,
                PaymentMethodIdCopy = GetPaymentMethod(adsTypeCssClass == "ad-selling" ? "pm-vnd-m" : "pm-vnd-b").Id,
                IsExternal = IsExternalProperty(p),

                // Permissions
                EnableEditProperty = true,
                EnableCopyPropertyToGroup = IsValidForCopyToGroup(p) && IsValidToPublish(p),
                EnableCopyPropertyToAdsType = IsValidForCopyToAdsType(p),
                EnableEditStatus = EnableEditStatus(p, user),
                EnableEditAddressNumber = EnableEditAddressNumber(p, user),
                EnableEditContactPhone = EnableEditContactPhone(p, user),
                EnableEditImages = EnableEditPropertyImages(p, user),
                EnableSetAdsGoodDeal = Services.Authorizer.Authorize(Permissions.SetAdsGoodDeal),
                EnableSetAdsVIP = Services.Authorizer.Authorize(Permissions.SetAdsVIP),
                EnableSetAdsHighlight = Services.Authorizer.Authorize(Permissions.SetAdsHighlight),
                Files = GetPropertyFiles(p),
                VisitedCount = CountVisitedCustomer(p),
                UpdateMeta = true,
                YoutubeId = p.YoutubeId,
                AdsTypeVIPId = (AdsTypeVIP)p.SeqOrder,
                AdsVIP = (p.AdsVIPRequest || (p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now)),

                IsManageAddAdsPayment = Services.Authorizer.Authorize(Permissions.ManageAddAdsPayment)
            };
        }

        public PropertyViewModel BuildViewModel(PropertyPart p)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            IEnumerable<int> advantageIds = GetPropertyAdvantages(p).Select(a => a.Id);
            IEnumerable<int> disadvantageIds = GetPropertyDisAdvantages(p).Select(a => a.Id);
            IEnumerable<int> apartmentAdvantageIds = GetPropertyApartmentAdvantages(p).Select(a => a.Id);
            IEnumerable<int> apartmentInteriorAdvantageIds = GetPropertyApartmentInteriorAdvantages(p)
                .Select(a => a.Id);

            return new PropertyViewModel
            {
                Property = p,
                EnableEditProperty = false,
                EnableCopyPropertyToGroup = IsValidForCopyToGroup(p) && IsValidToPublish(p),
                EnableCopyPropertyToAdsType = IsValidForCopyToAdsType(p),
                EnableViewContactPhone = _groupService.IsInRetrictedAccessGroupLocations(p, user),
                VisitedCount = CountVisitedCustomer(p),
                Advantages =
                    GetAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = advantageIds.Contains(r.Id) })
                        .ToList(),
                DisAdvantages =
                    GetDisAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = disadvantageIds.Contains(r.Id) })
                        .ToList(),
                ApartmentAdvantages =
                    GetApartmentAdvantages()
                        .Select(
                            r =>
                                new PropertyAdvantageEntry
                                {
                                    Advantage = r,
                                    IsChecked = apartmentAdvantageIds.Contains(r.Id)
                                })
                        .ToList(),
                ApartmentInteriorAdvantages =
                    GetApartmentInteriorAdvantages()
                        .Select(
                            r =>
                                new PropertyAdvantageEntry
                                {
                                    Advantage = r,
                                    IsChecked = apartmentInteriorAdvantageIds.Contains(r.Id)
                                })
                        .ToList(),
            };
        }

        public IEnumerable<PropertyDisplayEntry> BuildPropertiesEntries(IEnumerable<PropertyPart> pList)
        {
            // UserView is Count on BuildDisplay
            IList<PropertyPart> propertyParts = pList as IList<PropertyPart> ?? pList.ToList();
            foreach (PropertyPart p in propertyParts)
            {
                _contentManager.BuildDisplay(p);
            }

            return propertyParts.Select(BuildPropertyEntry).ToList();
        }

        public PropertyDisplayEntry BuildPropertyEntry(PropertyPart p)
        {
            return new PropertyDisplayEntry
            {
                Property = p,
                Advantages = GetPropertyAdvantages(p),
                DisAdvantages = GetPropertyDisAdvantages(p),
                ApartmentAdvantages = GetPropertyApartmentAdvantages(p),
                ApartmentInteriorAdvantages = GetPropertyApartmentInteriorAdvantages(p),
                UserNameDisplay = GetDisplayName(p),
                DisplayForContact = GetDisplayForContact(p),
                UserViews = GetUserViews(p.ContentItem),
                Files = GetAllPropertyFiles(p).Where(a => a.Published).List(),
                DefaultImgUrl = GetDefaultImageUrl(p),
                ApartmentBlockInfoContent = GetApartmentBlockInfo(p),
            };
        }

        public PropertyDisplayEntry BuildPropertyEntryFrontEnd(PropertyPart p)
        {
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            return new PropertyDisplayEntry
            {
                Property = p,
                DisplayForContact = GetDisplayForContact(p),
                DefaultImgUrl = GetDefaultImageUrl(p),
                DomainGroup = currentDomainGroup != null ? currentDomainGroup.Id : 0,
            };
        }

        public LocationApartmentDisplayEntry BuildLocationApartmentEntry(LocationApartmentPart p)
        {
            return new LocationApartmentDisplayEntry
            {
                LocationApartment = p,
                DefaultImgUrl = GetDefaultImageUrl(p),
                Files = GetAllLocationApartmentFiles(p).Where(a => a.Published).List(),
                LocationApartmentAdvantages = GetPropertyApartmentAdvantages(p)
            };
        }


        #region LocationApartmentCart

        public LocationApartmentCartCreateViewModel BuildApartmentCartCreate(int? apartmentId)
        {
            apartmentId = apartmentId ?? 0;
            var model = new LocationApartmentCartCreateViewModel
            {
                LocationApartmentBlocks = _apartmentService.LocationApartmentBlockParts(apartmentId),
                //LocationApartment = _contentManager.Get<LocationApartmentPart>(apartmentId),
                LocationApartments = _addressService.GetApartments().List(),
                ApartmentId = (int)apartmentId,
                PaymentMethods = GetPaymentMethods(),
                PaymentMethodId = 57,//Triệu dồng
                PaymentUnits = GetPaymentUnits(),
                PaymentUnitId = 90//m2
            };

            return model;
        }

        public LocationApartmentCartIndexViewModel BuildApartmentCartIndex(int apartmentId, bool isFrontEnd)
        {
            var model = new LocationApartmentCartIndexViewModel();
            model.LocationApartment = _contentManager.Get<LocationApartmentPart>(apartmentId);

            model.LocationApartmentBlocks = _apartmentService.LocationApartmentBlockParts(apartmentId)
            .Select(r => new LocationApartmentBlockItem
            {
                ApartmentBlockPart = r,
                GroupInApartmentBlockParts = _contentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                                            .Where(s => s.IsActive && s.ApartmentBlock != null && s.ApartmentBlock == r.Record).OrderBy(s => s.SeqOrder).List(),
                Properties = GetPropertiesForApartmentCart(isFrontEnd, r.Id),
            }).ToList();

            var statusSelling = GetStatus("st-selling");
            var statusNew = GetStatus("st-new");
            model.CountSelling = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Apartment.Id == apartmentId && (p.Status == statusSelling || p.Status == statusNew)).Count();

            var statusOnHold = GetStatus("st-onhold");
            model.CountOnHold = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Apartment.Id == apartmentId && p.Status == statusOnHold).Count();


            var statusNegotiate = GetStatus("st-negotiate");
            model.CountNegotiate = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Apartment.Id == apartmentId && p.Status == statusNegotiate).Count();


            var statusTrading = GetStatus("st-trading");
            model.CountTrading = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Apartment.Id == apartmentId && p.Status == statusTrading).Count();


            var statusSold = GetStatus("st-sold");
            model.CountSold = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                .Where(p => p.Apartment.Id == apartmentId && p.Status == statusSold).Count();

            return model;
        }

        public LocationApartmentBlockItem BuildApartmentCartByBlockId(int blockId)
        {
            var blockPart = Services.ContentManager.Get<LocationApartmentBlockPart>(blockId);
            if (blockPart != null)
                return BuildApartmentCartByBlockId(blockPart.LocationApartment.Id, blockPart.ShortName);

            return null;
        }

        public LocationApartmentBlockItem BuildApartmentCartByBlockId(int apartmentId, string blockShortName)
        {
            var block = Services.ContentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>()
                    .Where(r => r.LocationApartment != null && r.LocationApartment.Id == apartmentId && r.ShortName == blockShortName).Slice(1).FirstOrDefault();

            if (block == null) return null;

            var model = new LocationApartmentBlockItem
            {
                ApartmentBlockPart = block,
                GroupInApartmentBlockParts = _contentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                                             .Where(s => s.IsActive && s.ApartmentBlock != null && s.ApartmentBlock == block.Record).OrderBy(r => r.SeqOrder).List(),
                Properties = GetPropertiesForApartmentCart(true, block.Id)
            };

            return model;
        }

        public IEnumerable<PropertyPart> GetPropertiesForApartmentCart(bool isFrontEnd, int blockId)
        {
            List<PropertyPart> pList;
            int timeReservationLimit = 2; // Hours // TODO: config theo dự án

            if (isFrontEnd)
                pList = GetPropertiesApartment().Where(p => p.Published == true && p.ApartmentBlock != null && p.ApartmentBlock.Id == blockId).List().Select(p => p).ToList();
            else
                pList = GetPropertiesApartment().Where(p => p.ApartmentBlock != null && p.ApartmentBlock.Id == blockId).List().Select(p => p).ToList();

            // Tự động chuyển các giữ chỗ về rao bán sau 2h
            foreach (var p in pList)
            {
                if (p.Status.CssClass == "st-onhold")
                {
                    if (p.StatusChangedDate == null || (DateTime.Now - ((DateTime)p.StatusChangedDate).ToLocalTime()).TotalHours > timeReservationLimit)
                    {
                        p.Status = GetStatus("st-selling"); // Giữ chỗ -> Rao bán
                    }
                }
            }

            return pList;
        }

        public bool VerifyApartmentGroupInBlock(int apartmentBlockId, int groupPositionInBlock)
        {
            var groupInApartmentBlock = _contentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                .Where(r => r.ApartmentBlock == null && r.ApartmentBlock.Id == apartmentBlockId && r.ApartmentGroupPosition == groupPositionInBlock).Slice(1).FirstOrDefault();

            if (groupInApartmentBlock == null)
                return false;

            var properties = GetPropertiesApartment()
                .Where(p => p.ApartmentBlock != null && p.ApartmentBlock.Id == apartmentBlockId && p.GroupInApartmentBlock != null && p.GroupInApartmentBlock == groupInApartmentBlock.Record);

            return properties.Count() > 0;
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> GetPropertiesApartment()
        {
            var statusCssClass = new List<string> { "st-selling", "st-new", "st-sold", "st-onhold", "st-negotiate", "st-trading" };
            // -- Đang rao bán -- (RAO BÁN, KH RAO BÁN) (Đủ thông tin - Đã duyệt - Chưa hoàn chỉnh),
            List<int> statusIds = GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            int statusApproved = GetStatus().Where(r => r.CssClass.Contains("st-approved")).Select(r => r.Id).First();

            IContentQuery<PropertyPart, PropertyPartRecord> pList = Services.ContentManager
                .Query<PropertyPart, PropertyPartRecord>()
                .Where(p => statusIds.Contains(p.Status.Id) || (p.Status.Id == statusApproved && p.AdsExpirationDate > DateTime.Now));

            return pList;
        }

        public GroupInApartmentBlockPart CheckApartmentGroupInBlock(int blockId, int groupPosition)
        {
            var query = Services.ContentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                .Where(r => r.IsActive && r.ApartmentBlock != null && r.ApartmentBlock.Id == blockId && r.ApartmentGroupPosition == groupPosition);

            if (query.Count() > 0)
                return query.Slice(1).Select(r => r).FirstOrDefault();

            return null;
        }

        public void UpdatePropertyInfoByApartmentInfo(int apartmentBlockInfo)
        {
            var apBlockInfoPart = Services.ContentManager.Get<ApartmentBlockInfoPart>(apartmentBlockInfo);

            if (apBlockInfoPart != null)
            {
                var properties = GetPropertiesApartment().Where(r => r.ApartmentBlockInfoPartRecord != null && r.ApartmentBlockInfoPartRecord == apBlockInfoPart.Record).List();
                foreach (var property in properties)
                {
                    property.Area = apBlockInfoPart.ApartmentArea;
                    property.Bedrooms = apBlockInfoPart.ApartmentBedrooms;
                    property.Bathrooms = apBlockInfoPart.ApartmentBathrooms;
                    property.Content = apBlockInfoPart.OrtherContent;
                }
            }
        }

        #endregion

        public int GetUserViews(ContentItem part)
        {
            //var resultRecord = _votingService.GetResult(part.Id, "sum", Constants.Dimension);
            //return resultRecord == null ? 0 : (int)resultRecord.Value;
            return 0;
        }

        #endregion

        #region AddressNumber FUNCTION

        public int IntParseAddressNumber(string addressNumber)
        {
            if (String.IsNullOrEmpty(addressNumber)) return -1;
            try
            {
                string[] list = addressNumber.Split('/');
                Match firstmatchedValue = Regex.Match(list[0], @"\d+", RegexOptions.IgnorePatternWhitespace);
                return int.Parse(firstmatchedValue.Value);
            }
            catch
            {
                return -1;
            }
        }

        #endregion

        #region CONVERT CURRENCY

        public double Convert(double input, string fromPaymentMethod, string toPaymentMethod)
        {
            switch (toPaymentMethod)
            {
                case "pm-vnd-b":
                    return ConvertToVndB(input, fromPaymentMethod);
                case "pm-vnd-m":
                    return ConvertToVndM(input, fromPaymentMethod);
                case "pm-vnd-k":
                    return ConvertToVndK(input, fromPaymentMethod);
                case "pm-usd":
                    return ConvertToUsd(input, fromPaymentMethod);
                case "pm-usd-k":
                    return ConvertToUsdK(input, fromPaymentMethod);
                case "pm-usd-m":
                    return ConvertToUsdM(input, fromPaymentMethod);
                case "pm-sjc":
                    return ConvertToGold(input, fromPaymentMethod);
                default:
                    return 0;
            }
        }

        public double ConvertToVndB(double input, string paymentMethod)
        {
            return ConvertToVndB(input, paymentMethod, 1);
        }

        public double ConvertToVndM(double input, string paymentMethod)
        {
            return ConvertToVndM(input, paymentMethod, 1);
        }

        public double ConvertToVndK(double input, string paymentMethod)
        {
            return ConvertToVndK(input, paymentMethod, 1);
        }

        public double ConvertToUsd(double input, string paymentMethod)
        {
            return ConvertToUsd(input, paymentMethod, 1);
        }

        public double ConvertToGold(double input, string paymentMethod)
        {
            return ConvertToGold(input, paymentMethod, 1);
        }

        public double ConvertToVndB(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = input;
                    break;
                case "pm-vnd-m":
                    output = input * 1000000 / 1000000000;
                    break;
                case "pm-vnd-k":
                    output = input * 1000 / 1000000000;
                    break;
                case "pm-usd":
                    output = input * rateUsd / 1000000000;
                    break;
                case "pm-usd-k":
                    output = input * 1000 * rateUsd / 1000000000;
                    break;
                case "pm-usd-m":
                    output = input * 1000000 * rateUsd / 1000000000;
                    break;
                case "pm-sjc":
                    output = input * rateGold / 1000000000;
                    break;
            }

            return output * areaTotal;
        }

        public double ConvertToVndM(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = input * 1000000000 / 1000000;
                    break;
                case "pm-vnd-m":
                    output = input;
                    break;
                case "pm-vnd-k":
                    output = input * 1000 / 1000000;
                    break;
                case "pm-usd":
                    output = input * rateUsd / 1000000;
                    break;
                case "pm-usd-k":
                    output = input * 1000 * rateUsd / 1000000;
                    break;
                case "pm-usd-m":
                    output = input * 1000000 * rateUsd / 1000000;
                    break;
                case "pm-sjc":
                    output = input * rateGold / 1000000;
                    break;
            }

            return output * areaTotal;
        }

        public double ConvertToVndK(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = input * 1000000000 / 1000;
                    break;
                case "pm-vnd-m":
                    output = input * 1000000 / 1000;
                    break;
                case "pm-vnd-k":
                    output = input;
                    break;
                case "pm-usd":
                    output = input * rateUsd / 1000;
                    break;
                case "pm-usd-k":
                    output = input * 1000 * rateUsd / 1000;
                    break;
                case "pm-usd-m":
                    output = input * 1000000 * rateUsd / 1000;
                    break;
                case "pm-sjc":
                    output = input * rateGold / 1000;
                    break;
            }

            return output * areaTotal;
        }

        public double ConvertToUsd(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = input * 1000000000 / rateUsd;
                    break;
                case "pm-vnd-m":
                    output = input * 1000000 / rateUsd;
                    break;
                case "pm-vnd-k":
                    output = input * 1000 / rateUsd;
                    break;
                case "pm-usd":
                    output = input;
                    break;
                case "pm-usd-k":
                    output = input * 1000;
                    break;
                case "pm-usd-m":
                    output = input * 1000000;
                    break;
                case "pm-sjc":
                    output = input * rateGold / rateUsd;
                    break;
            }

            return output * areaTotal;
        }

        public double ConvertToGold(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = input * 1000000000 / rateGold;
                    break;
                case "pm-vnd-m":
                    output = input * 1000000 / rateGold;
                    break;
                case "pm-vnd-k":
                    output = input * 1000 / rateGold;
                    break;
                case "pm-usd":
                    output = input * rateUsd / rateGold;
                    break;
                case "pm-usd-k":
                    output = input * 1000 * rateUsd / rateGold;
                    break;
                case "pm-usd-m":
                    output = input * 1000000 * rateUsd / rateGold;
                    break;
                case "pm-sjc":
                    output = input;
                    break;
            }

            return output * areaTotal;
        }

        public double ConvertToUsdK(double input, string paymentMethod)
        {
            return ConvertToUsdK(input, paymentMethod, 1);
        }

        public double ConvertToUsdM(double input, string paymentMethod)
        {
            return ConvertToUsdM(input, paymentMethod, 1);
        }

        public double ConvertToUsdK(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = (input * 1000000000 / rateUsd) / 1000;
                    break;
                case "pm-vnd-m":
                    output = (input * 1000000 / rateUsd) / 1000;
                    break;
                case "pm-vnd-k":
                    output = (input * 1000 / rateUsd) / 1000;
                    break;
                case "pm-usd":
                    output = input / 1000;
                    break;
                case "pm-usd-k":
                    output = input;
                    break;
                case "pm-usd-m":
                    output = input * 1000;
                    break;
                case "pm-sjc":
                    output = (input * rateGold / rateUsd) / 1000;
                    break;
            }

            return output * areaTotal;
        }

        public double ConvertToUsdM(double input, string paymentMethod, double areaTotal)
        {
            double output = input;
            double rateUsd = GetRateUsd();
            double rateGold = GetRateGold();

            if (String.IsNullOrWhiteSpace(paymentMethod)) paymentMethod = "pm-vnd-b";

            switch (paymentMethod.ToLower())
            {
                case "pm-vnd-b":
                    output = (input * 1000000000 / rateUsd) / 1000000;
                    break;
                case "pm-vnd-m":
                    output = (input * 1000000 / rateUsd) / 1000000;
                    break;
                case "pm-vnd-k":
                    output = (input * 1000 / rateUsd) / 1000000;
                    break;
                case "pm-usd":
                    output = input / 1000000;
                    break;
                case "pm-usd-k":
                    output = input / 1000;
                    break;
                case "pm-usd-m":
                    output = input;
                    break;
                case "pm-sjc":
                    output = (input * rateGold / rateUsd) / 1000000;
                    break;
            }

            return output * areaTotal;
        }

        #endregion

        #region Orchard Tags

        public void AutoUpdateTags(PropertyPart p)
        {
            char[] disalowedChars = { '<', '>', '*', '%', ':', '&', '\\', '"', '|', '/' };

            string tags = p.Province.Name;
            if (p.District != null)
            {
                tags += ", " + p.District.Name + " " + p.Province.Name;

                if (p.Ward != null)
                    tags += ", " + p.Ward.Name + " " + p.District.Name;

                if (p.Street != null)
                    tags += ", " + p.Street.Name + " " + p.District.Name;
            }

            List<string> tagNames = TagHelpers.ParseCommaSeparatedTagNames(tags);

            List<string> disallowedTags = tagNames.Where(x => disalowedChars.Intersect(x).Any()).ToList();

            if (disallowedTags.Any())
            {
                tagNames = tagNames.Where(x => !disallowedTags.Contains(x)).ToList();
            }

            if (p.ContentItem.Id != 0)
            {
                _tagService.UpdateTagsForContentItem(p.ContentItem, tagNames);
            }
        }

        #endregion

        #region Statistics

        public string BuildMessageString(object data)
        {

            var sb = new System.Text.StringBuilder();

            {

                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();

                var serializedObject = ser.Serialize(data);

                sb.AppendFormat("data: {0}\n\n", serializedObject);

            }

            return sb.ToString();

        }
        public object CountPendingProperties(bool countPendingInDetails)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            var belongGroup = _groupService.GetBelongGroup(user.Id);

            // status
            PropertyStatusPartRecord statusPending = GetStatus("st-pending");
            PropertyStatusPartRecord statusApproved = GetStatus("st-approved");

            // adsType
            AdsTypePartRecord adsTypeSelling = GetAdsType("ad-selling");
            AdsTypePartRecord adsTypeLeasing = GetAdsType("ad-leasing");
            //var adsTypeBuying = GetAdsType("ad-buying");
            //var adsTypeRenting = GetAdsType("ad-renting");

            // typeGroup
            PropertyTypeGroupPartRecord typeGroupHouse = GetTypeGroup("gp-house");
            PropertyTypeGroupPartRecord typeGroupApartment = GetTypeGroup("gp-apartment");
            PropertyTypeGroupPartRecord typeGroupLand = GetTypeGroup("gp-land");

            int countPendingSellingHouse = 0,
                countPendingSellingApartment = 0,
                countPendingSellingLand = 0,
                countPendingLeasingHouse = 0,
                countPendingLeasingApartment = 0,
                countPendingLeasingLand = 0,
                countPendingProperties = 0,
                countAdsRequestProperties = 0;

            #region Pending

            if (Services.Authorizer.Authorize(Permissions.ManageProperties) || belongGroup != null)//(currentDomainGroup != null && belongGroup != null && belongGroup == currentDomainGroup))
            {
                countPendingProperties = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                    .Where(a => a.Status == statusPending && a.UserGroup == belongGroup)
                    .Count();

                if (countPendingInDetails)
                {
                    #region Pending Selling

                    countPendingSellingHouse = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.AdsType == adsTypeSelling)
                        .Where(a => a.TypeGroup == typeGroupHouse)
                        .Count();

                    countPendingSellingApartment = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.AdsType == adsTypeSelling)
                        .Where(a => a.TypeGroup == typeGroupApartment)
                        .Count();

                    countPendingSellingLand = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.AdsType == adsTypeSelling)
                        .Where(a => a.TypeGroup == typeGroupLand)
                        .Count();

                    #endregion

                    #region Pending Leasing

                    countPendingLeasingHouse = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.AdsType == adsTypeLeasing)
                        .Where(a => a.TypeGroup == typeGroupHouse)
                        .Where(a => a.UserGroup == belongGroup)
                        .Count();

                    countPendingLeasingApartment = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.AdsType == adsTypeLeasing)
                        .Where(a => a.TypeGroup == typeGroupApartment)
                        .Where(a => a.UserGroup == belongGroup)
                        .Count();

                    countPendingLeasingLand = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.AdsType == adsTypeLeasing)
                        .Where(a => a.TypeGroup == typeGroupLand)
                        .Where(a => a.UserGroup == belongGroup)
                        .Count();

                    #endregion

                    #region Pending AdsRequest

                    countAdsRequestProperties = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.Status == statusPending || a.Status == statusApproved)
                        .Where(p => p.AdsVIPRequest) //p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
                        .Where(a => a.UserGroup == belongGroup)
                        .Count();

                    #endregion
                }
            }

            #endregion

            #region AdsRequest

            //    countAdsRequestSellingHouse = 0,
            //    countAdsRequestSellingApartment = 0,
            //    countAdsRequestSellingLand = 0,
            //    countAdsRequestLeasingHouse = 0,
            //    countAdsRequestLeasingApartment = 0,
            //    countAdsRequestLeasingLand = 0;

            //int countAdsRequestProperties = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.UserGroup.Id == hostGroupId)
            //    .Count();

            //if (countAdsRequestInDetails == true)
            //{

            #region AdsRequest Selling

            //countAdsRequestSellingHouse = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.AdsType == adsTypeSelling)
            //    .Where(a => a.TypeGroup == typeGroupHouse)
            //    .Count();

            //countAdsRequestSellingApartment = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.AdsType == adsTypeSelling)
            //    .Where(a => a.TypeGroup == typeGroupApartment)
            //    .Count();

            //countAdsRequestSellingLand = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.AdsType == adsTypeSelling)
            //    .Where(a => a.TypeGroup == typeGroupLand)
            //    .Count();

            #endregion

            #region AdsRequest Leasing

            //countAdsRequestLeasingHouse = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.AdsType == adsTypeLeasing)
            //    .Where(a => a.TypeGroup == typeGroupHouse)
            //    .Count();

            //countAdsRequestLeasingApartment = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.AdsType == adsTypeLeasing)
            //    .Where(a => a.TypeGroup == typeGroupApartment)
            //    .Count();

            //countAdsRequestLeasingLand = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //    .Where(a => a.Status == statusPending || a.Status == statusApproved)
            //    .Where(p => p.AdsVIPRequest)//p.AdsGoodDealRequest == true || p.AdsHighlightRequest == true
            //    .Where(a => a.AdsType == adsTypeLeasing)
            //    .Where(a => a.TypeGroup == typeGroupLand)
            //    .Count();

            #endregion

            //}

            #endregion

            var data = new
            {
                userId = user != null ? user.Id : 0,
                groupId = group != null ? group.Id : 0,
                belongGroupId = belongGroup != null ? belongGroup.Id : 0,

                countPendingProperties,

                countPendingSellingHouse,
                countPendingSellingApartment,
                countPendingSellingLand,

                countPendingLeasingHouse,
                countPendingLeasingApartment,
                countPendingLeasingLand,

                countAdsRequestProperties

                //countAdsRequestSellingHouse,
                //countAdsRequestSellingApartment,
                //countAdsRequestSellingLand,
                //countAdsRequestLeasingHouse,
                //countAdsRequestLeasingApartment,
                //countAdsRequestLeasingLand
            };

            return data;
        }
        public object CountPendingCustomers(bool countPendingInDetails)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            var belongGroup = _groupService.GetBelongGroup(user.Id);

            // status
            CustomerStatusPartRecord statusPending = _contentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>().Where(a => a.CssClass == "st-pending").List().FirstOrDefault().Record;

            // typeGroup
            PropertyTypeGroupPartRecord typeGroupHouse = GetTypeGroup("gp-house");
            PropertyTypeGroupPartRecord typeGroupApartment = GetTypeGroup("gp-apartment");
            PropertyTypeGroupPartRecord typeGroupLand = GetTypeGroup("gp-land");

            int countPendingBuyingHouse = 0,
                countPendingBuyingApartment = 0,
                countPendingBuyingLand = 0,
                countPendingRentingHouse = 0,
                countPendingRentingApartment = 0,
                countPendingRentingLand = 0,
                countPendingExchangeHouse = 0,
                countPendingExchangeApartment = 0,
                countPendingExchangeLand = 0,
                countPendingCustomers = 0;


            if (Services.Authorizer.Authorize(Permissions.ManageCustomers) || belongGroup != null)//(currentDomainGroup != null && belongGroup != null && belongGroup == currentDomainGroup))
            {
                countPendingCustomers = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                    .Where(a => a.UserGroup == belongGroup)
                    .Where(a => a.Status == statusPending)
                    .Where(a => a.Requirements.Any())
                    .Count();

                if (countPendingInDetails)
                {
                    #region Pending Buying

                    AdsTypePartRecord adsTypeBuying = GetAdsType("ad-buying");

                    countPendingBuyingHouse = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeBuying))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupHouse))
                        .Count();

                    countPendingBuyingApartment = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeBuying))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupApartment))
                        .Count();

                    countPendingBuyingLand = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeBuying))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupLand))
                        .Count();

                    #endregion

                    #region Pending Renting

                    AdsTypePartRecord adsTypeRenting = GetAdsType("ad-renting");

                    countPendingRentingHouse = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeRenting))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupHouse))
                        .Count();

                    countPendingRentingApartment = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeRenting))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupApartment))
                        .Count();

                    countPendingRentingLand = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeRenting))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupLand))
                        .Count();

                    #endregion

                    #region Pending Exchange

                    AdsTypePartRecord adsTypeExchange = GetAdsType("ad-exchange");

                    countPendingExchangeHouse = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeExchange))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupHouse))
                        .Count();

                    countPendingExchangeApartment = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeExchange))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupApartment))
                        .Count();

                    countPendingExchangeLand = Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                        .Where(a => a.UserGroup == belongGroup)
                        .Where(a => a.Status == statusPending)
                        .Where(a => a.Requirements.Any(b => b.AdsTypePartRecord == adsTypeExchange))
                        .Where(a => a.Requirements.Any(b => b.PropertyTypeGroupPartRecord == typeGroupLand))
                        .Count();

                    #endregion
                }
            }

            var data = new
            {
                userId = user != null ? user.Id : 0,
                groupId = group != null ? group.Id : 0,
                belongGroupId = belongGroup != null ? belongGroup.Id : 0,

                countPendingCustomers,

                countPendingBuyingHouse,
                countPendingBuyingApartment,
                countPendingBuyingLand,

                countPendingRentingHouse,
                countPendingRentingApartment,
                countPendingRentingLand,

                countPendingExchangeHouse,
                countPendingExchangeApartment,
                countPendingExchangeLand,
            };

            return data;
        }
        #endregion

        #region Estimate

        private string apiUri = "http://api.dinhgianhadat.local/";

        public async Task<PropertyEstimateEntry> EstimateProperty(int id)
        {
            // Clear UnitPrice in Cache
            await ClearApplicationCache(id);

            PropertyEstimateEntry entry = null;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/estimate/" + id);
                if (response.IsSuccessStatusCode)
                {
                    entry = await response.Content.ReadAsAsync<PropertyEstimateEntry>();
                    Services.Notifier.Error(T("ESTIMATE {0}", entry.PriceEstimatedInVND));
                }

                //// HTTP POST
                //var gizmo = new Product() { Name = "Gizmo", Price = 100, Category = "Widget" };
                //response = await client.PostAsJsonAsync("api/products", gizmo);
                //if (response.IsSuccessStatusCode)
                //{
                //    Uri gizmoUrl = response.Headers.Location;

                //    // HTTP PUT
                //    gizmo.Price = 80;   // Update price
                //    response = await client.PutAsJsonAsync(gizmoUrl, gizmo);

                //    // HTTP DELETE
                //    response = await client.DeleteAsync(gizmoUrl);
                //}
            }
            return entry;
        }

        public async Task<List<int>> GetListPropertiesUseToEstimate(string key)
        {
            List<int> list = new List<int>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/estimate?key=" + key);
                if (response.IsSuccessStatusCode)
                {
                    list = await response.Content.ReadAsAsync<List<int>>();
                }
            }
            return list;
        }

        public async Task<bool> ClearApplicationCache(int id)
        {
            bool success = false;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/estimate?propertyId=" + id);
                if (response.IsSuccessStatusCode)
                {
                    success = await response.Content.ReadAsAsync<bool>();
                }
            }
            return success;
        }

        public async Task<bool> IsEstimateable(int districtId, int wardId, int streetId, string addressNumber, string addressCorner)
        {
            bool IsEstimateable = false;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/estimate?districtId=" + districtId + "&wardId=" + wardId + "&streetId=" + streetId + "&addressNumber=" + addressNumber + "&addressCorner=" + addressCorner);
                if (response.IsSuccessStatusCode)
                {
                    IsEstimateable = await response.Content.ReadAsAsync<bool>();
                }
            }
            return IsEstimateable;
        }

        #endregion
    }
}