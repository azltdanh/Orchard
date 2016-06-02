using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Orchard;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstate.Services;
using System.Globalization;
using RealEstate.API.ViewModels;
using Maps.Services;
using RealEstate.Helpers;
using RealEstate.ViewModels;
using System.Threading.Tasks;
using Orchard.Data;

namespace RealEstate.API.Controllers
{
    public class PropertyAPIController : CrossSiteController, IUpdateModel
    {
        // GET: PropertyAPI


        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly IAddressService _addressService;
        private readonly IMapService _mapService;
        private readonly IAdsPaymentHistoryService _adsPaymentHistoryService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<UnEstimatedLocationRecord> _unEstimatedLocationRepository;
        private readonly IPropertySettingService _settingService;

        public PropertyAPIController(
            IOrchardServices services,
            IUserGroupService groupService,
            IMapService mapService,
            ICustomerService customerService,
            IPropertyService propertyService,
            IAddressService addressService,
            IAdsPaymentHistoryService adsPaymentHistoryService,
            IRepository<UnEstimatedLocationRecord> unEstimatedLocationRepository,
            IPropertySettingService settingService)
        {
            Services = services;
            _propertyService = propertyService;
            _groupService = groupService;
            _mapService = mapService;
            _addressService = addressService;
            _adsPaymentHistoryService = adsPaymentHistoryService;
            _customerService = customerService;
            _unEstimatedLocationRepository = unEstimatedLocationRepository;
            _settingService = settingService;
        }
        public IOrchardServices Services { get; set; }

        //xử lý từ Ajax
        [HttpPost]
        public JsonResult Create(PropertyFrontEndCreateBaseViewModel createModel, FormCollection frm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var p = Services.ContentManager.New<PropertyPart>("Property");

                    DateTime createdDate = DateTime.Now;
                    int userId = int.Parse(frm["UserId"]);
                    int domainGroupId = int.Parse(frm["DomainGroupId"]);

                    UserPartRecord createdUser = _groupService.GetUser(userId).Record;// Services.WorkContext.CurrentUser.As<UserPart>().Record;

                    #region Published, Status, Flag

                    p.Status = _propertyService.GetStatus("st-draft");
                    p.Flag = _propertyService.GetFlag("deal-unknow");

                    p.Published = true;
                    p.PublishAddress = true;
                    p.PublishContact = true;

                    p.IsExcludeFromPriceEstimation = true;

                    #endregion

                    #region User

                    // User
                    p.CreatedDate = createdDate;
                    p.CreatedUser = createdUser;
                    p.LastUpdatedDate = createdDate;
                    p.LastUpdatedUser = createdUser;
                    p.FirstInfoFromUser = createdUser;
                    p.LastInfoFromUser = createdUser;

                    #endregion

                    #region Group

                    p.UserGroup = _groupService.GetGroup(domainGroupId);

                    #endregion

                    PropertyTypePartRecord type = _propertyService.GetType(createModel.TypeId);

                    // Type
                    p.Type = type;
                    // TypeGroup
                    p.TypeGroup = type.Group;
                    // AdsType
                    p.AdsType = _propertyService.GetAdsType(createModel.AdsTypeCssClass);//createModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : createModel.AdsTypeCssClass

                    Services.ContentManager.Create(p);

                    #region Update OrderBy Domain (UserGroup)

                    if (p.UserGroup != null)
                        _propertyService.UpdateOrderByDomainGroup(p, p.UserGroup.Id);

                    #endregion

                    // IdStr
                    p.IdStr = p.Id.ToString(CultureInfo.InvariantCulture);

                    Services.ContentManager.UpdateEditor(p, this);

                    return Json(new { status = true, PropertyId = p.Id });
                }

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = ex.Message
                });
            }

            return Json(new { status = false, message = "Tạo mới bđs ko thành công!" });
        }

        #region Xử lý từ WebRequest

        /// <summary>
        /// Dùng cho trường hợp chỉ tạo 3 field trước
        /// </summary>
        /// <param name="createModel"></param>
        /// <param name="frm"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CreateRest(PropertyFrontEndCreateBaseViewModel createModel, FormCollection frm)
        {
            string test = "";
            try
            {
                if (!string.IsNullOrEmpty(frm["apiKey"]) && !_settingService.GetSetting("API_Key_DGND").Equals(frm["apiKey"]))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }
                test += "=>1";
                var p = Services.ContentManager.New<PropertyPart>("Property");
                test += "=>2";

                DateTime createdDate = DateTime.Now;
                int userId = createModel.UserId.Value;// int.Parse(frm["UserId"]);
                test += "=>3.1";
                int domainGroupId = createModel.DomainGroupId.Value;//int.Parse(frm["DomainGroupId"]);
                test += "=>3.2";

                UserPartRecord createdUser = _groupService.GetUser(userId).Record;
                test += "=>4";

                #region Published, Status, Flag

                p.Status = _propertyService.GetStatus("st-draft");
                p.Flag = _propertyService.GetFlag("deal-unknow");
                test += "=>5";

                p.Published = true;
                p.PublishAddress = true;
                p.PublishContact = true;

                p.IsExcludeFromPriceEstimation = true;

                #endregion

                #region User

                // User
                p.CreatedDate = createdDate;
                p.CreatedUser = createdUser;
                p.LastUpdatedDate = createdDate;
                p.LastUpdatedUser = createdUser;
                p.FirstInfoFromUser = createdUser;
                p.LastInfoFromUser = createdUser;

                #endregion

                #region Group

                p.UserGroup = _groupService.GetGroup(domainGroupId);
                test += "=>6";

                #endregion

                PropertyTypePartRecord type = _propertyService.GetType(createModel.TypeId);
                test += "=>7";

                // Type
                p.Type = type;
                // TypeGroup
                p.TypeGroup = type.Group;
                // AdsType
                p.AdsType = _propertyService.GetAdsType(createModel.AdsTypeCssClass);//createModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : createModel.AdsTypeCssClass
                test += "=>8";

                Services.ContentManager.Create(p);
                test += "=>9";

                #region Update OrderBy Domain (UserGroup)

                if (p.UserGroup != null)
                    _propertyService.UpdateOrderByDomainGroup(p, p.UserGroup.Id);

                #endregion

                // IdStr
                p.IdStr = p.Id.ToString(CultureInfo.InvariantCulture);

                #region Create Property Exchange

                //if (createModel.AdsTypeCssClass == "ad-exchange")
                //{
                //    _propertyExchangeService.CreatePropertyExchange(p);
                //}

                #endregion

                Services.ContentManager.UpdateEditor(p, this);

                return Json(new { status = true, PropertyId = p.Id });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message =
                        ex.Message + " - " + test
                });
            }
        }

        [HttpPost]
        public JsonResult CreateRestFull(PropertyAPICreateViewModel createModel)
        {
            string test = "";
            try
            {
                if (!string.IsNullOrEmpty(createModel.apiKey) && !_settingService.GetSetting("API_Key_DGND").Equals(createModel.apiKey))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                #region Validation


                #endregion

                if (ModelState.IsValid)
                {
                    var p = Services.ContentManager.New<PropertyPart>("Property");

                    ///IsSaveDraft == 1(Đăng tin)/0(Lưu tạm)

                    const string format = "dd/MM/yyyy";
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                    DateTime fromDate, toDate;

                    DateTime.TryParseExact(createModel.DateVipFrom, format, provider, style, out fromDate);
                    DateTime.TryParseExact(createModel.DateVipTo, format, provider, style, out toDate);

                    DateTime createdDate = DateTime.Now;
                    int userId = createModel.UserId;
                    int domainGroupId = createModel.DomainGroupId;

                    UserPart createdUser = _groupService.GetUser(userId);

                    #region Published, Status, Flag

                    p.Status = _propertyService.GetStatus("st-draft");
                    p.Flag = _propertyService.GetFlag("deal-unknow");
                    test += "=>5";

                    p.Published = true;
                    p.PublishAddress = true;
                    p.PublishContact = true;


                    p.IsExcludeFromPriceEstimation = true;

                    #endregion

                    p.Title = createModel.Title;

                    #region User

                    // User
                    p.CreatedDate = createdDate;
                    p.CreatedUser = createdUser.Record;
                    p.LastUpdatedDate = createdDate;
                    p.LastUpdatedUser = createdUser.Record;
                    p.FirstInfoFromUser = createdUser.Record;
                    p.LastInfoFromUser = createdUser.Record;

                    #endregion

                    #region Group

                    p.UserGroup = _groupService.GetGroup(domainGroupId);
                    test += "=>6";

                    #endregion

                    PropertyTypePartRecord type = _propertyService.GetType(createModel.TypeId);
                    test += "=>7";

                    // Type
                    p.Type = type;
                    // TypeGroup
                    p.TypeGroup = type.Group;
                    // AdsType
                    p.AdsType = _propertyService.GetAdsType(createModel.AdsTypeCssClass);//createModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : createModel.AdsTypeCssClass
                    test += "=>8";

                    Services.ContentManager.Create(p);
                    test += "=>9";

                    #region Update OrderBy Domain (UserGroup)

                    if (p.UserGroup != null)
                        _propertyService.UpdateOrderByDomainGroup(p, p.UserGroup.Id);

                    #endregion

                    // IdStr
                    p.IdStr = p.Id.ToString(CultureInfo.InvariantCulture);

                    #region Create Property Exchange

                    //if (createModel.AdsTypeCssClass == "ad-exchange")
                    //{
                    //    _propertyExchangeService.CreatePropertyExchange(p);
                    //}

                    #endregion

                    #region AdsVIPRequest

                    p.AdsVIPRequest = true;
                    p.AdsVIPExpirationDate = DateExtension.GetEndOfDate(toDate);
                    p.AdsExpirationDate = DateExtension.GetEndOfDate(toDate);
                    p.SeqOrder = createModel.AdsTypeVIP;
                    p.Published = true;

                    //Update Payment History
                    _adsPaymentHistoryService.UpdatePaymentHistoryV2("", 0,
                    null, p, createdUser, createModel.AdsTypeVIP, (int)(toDate - fromDate).TotalDays);

                    #endregion

                    Services.ContentManager.UpdateEditor(p, this);

                    /*
                     * Update lại ContentId trong table PropertyFilePart vì khi tạo thì up load hình sẽ dùng 1 ID ngẫu nhiên (createModel.ContentItemId) 
                     * => Khi người click tạo mới tin thì sẽ update lại ID tạm đó**/
                    //Task.Run(() => _propertyService.UploadPropertyForImage(createModel.ContentItemId,p.Id));

                    return Json(new { status = true, PropertyId = p.Id });
                }

                return Json(new { status = false, message = "Model not valid" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message =
                        ex.Message
                });
            }
        }

        #endregion

        //xử lý từ WebRequest
        [HttpPost, ValidateInput(false)]
        public JsonResult Edit(PropertyAPIEditViewModel editModel, FormCollection frmCollection)
        {
            string test = "";
            try
            {
                if (!string.IsNullOrEmpty(frmCollection["apiKey"]) && !_settingService.GetSetting("API_Key_DGND").Equals(frmCollection["apiKey"]))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }
                ///IsSaveDraft == 1(Đăng tin)/0(Lưu tạm)
                int saveDraft = int.Parse(frmCollection["IsSaveDraft"]);
                test += "=>1";

                //Xử lý cập nhật bds => Continue
                var p = Services.ContentManager.Get<PropertyPart>(editModel.PropertyId);
                test += "=>2";

                #region Get Old Data

                test += "=>2.1";
                int oldSeqOrder = p.SeqOrder;
                string oldAdsTypeCssClass = p.AdsType.CssClass;

                test += "=>2.2";
                bool oldIsAdsVip = p.AdsVIP && p.AdsVIPExpirationDate.HasValue && p.AdsVIPExpirationDate >= DateTime.Now && !p.AdsVIPRequest;

                test += "=>2.3";
                string oldStatusCssClass = p.Status.CssClass;

                test += "=>2.4";
                bool oldIsRefresh = p.IsRefresh;

                test += "=>2.5";
                int oldDistrictId = p.District != null ? p.District.Id : 0;

                test += "=>2.6";
                DateTime? oldAdsVipExpirationDate = p.AdsVIPExpirationDate;

                test += "=>2.7";
                var oldRequestVip = p.AdsVIPRequest;
                test += "=>3";

                #endregion

                int userId = editModel.UserId;
                int domainGroupId = editModel.DomainGroupId.Value;
                test += "=>4";

                const string format = "dd/MM/yyyy";
                CultureInfo provider = CultureInfo.InvariantCulture;
                const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                DateTime fromDate, toDate;

                DateTime.TryParseExact(editModel.DateVipFrom, format, provider, style, out fromDate);
                DateTime.TryParseExact(editModel.DateVipTo, format, provider, style, out toDate);

                var user = _groupService.GetUser(userId);//Services.WorkContext.CurrentUser.As<UserPart>().Record;
                DateTime lastUpdatedDate = DateTime.Now;
                UserPart lastUpdatedUser = user;
                test += "=>5";

                //var updateModel = new PropertyFrontEndEditViewModel { Property = p };

                if (ModelState.IsValid)
                {
                    #region UPDATE MODEL

                    p.IsRefresh = false;

                    #region User

                    // User
                    p.LastUpdatedDate = lastUpdatedDate;
                    p.LastUpdatedUser = lastUpdatedUser.Record;
                    p.LastInfoFromUser = lastUpdatedUser.Record;
                    test += "=>6";

                    #endregion

                    #region Group

                    if (p.UserGroup == null)
                    {
                        p.UserGroup = _groupService.GetGroup(domainGroupId);
                    }

                    #endregion
                    test += "=>7";

                    #region AdsType

                    // Ads Type
                    p.AdsType = _propertyService.GetAdsType(editModel.AdsTypeCssClass);

                    if (p.AdsExpirationDate == null)
                        p.AdsExpirationDate = DateExtension.GetEndOfDate(DateTime.Now.AddDays(1));//30

                    #endregion

                    test += "=>8";

                    #region Type

                    if (editModel.TypeId > 0)
                    {
                        p.Type = _propertyService.GetType(editModel.TypeId);
                        p.TypeGroup = p.Type.Group;

                        if (p.Type.CssClass == "tp-residential-land")
                        {
                            p.AreaConstruction = null;
                            p.AreaConstructionFloor = null;

                            p.Floors = null;
                            p.Bedrooms = null;
                            p.Bathrooms = null;
                            p.Livingrooms = null;
                            p.Balconies = null;

                            p.HaveBasement = false;
                            p.HaveElevator = false;
                            p.HaveGarage = false;
                            p.HaveGarden = false;
                            p.HaveMezzanine = false;
                            p.HaveSkylight = false;
                            p.HaveSwimmingPool = false;
                            p.HaveTerrace = false;

                            p.RemainingValue = null;
                            p.Interior = null;
                            p.TypeConstruction = null;
                        }
                        else
                        {
                            // Interior
                            p.Interior = _propertyService.GetInterior(editModel.InteriorId);
                            p.TypeConstruction = _propertyService.GetTypeConstruction(editModel.TypeConstructionId);
                        }
                    }

                    #endregion

                    test += "=>9";

                    #region Address

                    // Province
                    p.Province = _addressService.GetProvince(editModel.ProvinceId);

                    // District
                    p.District = _addressService.GetDistrict(editModel.DistrictId);

                    // Ward
                    if (editModel.WardId.HasValue) p.Ward = _addressService.GetWard(editModel.WardId);
                    p.OtherWardName = editModel.WardId.HasValue ? "" : editModel.OtherWardName;

                    // Street
                    if (editModel.StreetId.HasValue)
                    {
                        LocationStreetPartRecord selectedStreet = _addressService.GetStreet(editModel.StreetId);
                        p.Street = selectedStreet;

                        // Street Segment
                        LocationStreetPartRecord segmentStreet = _addressService.GetStreet(selectedStreet, p.AlleyNumber);
                        if (segmentStreet != null) p.Street = segmentStreet;
                    }
                    p.OtherStreetName = editModel.StreetId.HasValue ? "" : editModel.OtherStreetName;

                    //Custom
                    p.OtherProjectName = editModel.OtherProjectName;
                    p.AddressNumber = editModel.AddressNumber;
                    p.AddressCorner = editModel.AddressCorner;


                    // AlleyNumber
                    if (p.Province.Name == "Hà Nội")
                    {
                        p.AlleyNumber = _propertyService.IntParseAddressNumber(editModel.AddressCorner);
                    }
                    else
                    {
                        p.AlleyNumber = _propertyService.IntParseAddressNumber(editModel.AddressNumber);
                        p.AddressCorner = null;
                    }

                    // PublishAddress
                    p.PublishAddress = !editModel.UnPublishAddress;

                    #endregion

                    test += "=>10";

                    // LegalStatus
                    p.LegalStatus = _propertyService.GetLegalStatus(editModel.LegalStatusId);
                    test += "=>11";

                    // Direction
                    p.Direction = _propertyService.GetDirection(editModel.DirectionId);
                    test += "=>12";

                    #region Location

                    if (!string.IsNullOrEmpty(editModel.LocationCssClass))
                    {
                        p.Location = _propertyService.GetLocation(editModel.LocationCssClass);
                        if (p.Location.CssClass == "h-front")
                        {
                            p.DistanceToStreet = null;
                            p.AlleyTurns = null;
                            p.AlleyWidth = null;
                            p.AlleyWidth1 = null;
                            p.AlleyWidth2 = null;
                            p.AlleyWidth3 = null;
                            p.AlleyWidth4 = null;
                            p.AlleyWidth5 = null;
                            p.AlleyWidth6 = null;
                            p.AlleyWidth7 = null;
                            p.AlleyWidth8 = null;
                            p.AlleyWidth9 = null;

                            //Custom
                            p.StreetWidth = editModel.StreetWidth;
                        }
                        else
                        {
                            if (editModel.AlleyTurns > 0)
                            {
                                p.AlleyWidth =
                                    new List<double?>
                                    {
                                        editModel.AlleyWidth1,
                                        editModel.AlleyWidth2,
                                        editModel.AlleyWidth3,
                                        editModel.AlleyWidth4,
                                        editModel.AlleyWidth5,
                                        editModel.AlleyWidth6,
                                        editModel.AlleyWidth7,
                                        editModel.AlleyWidth8,
                                        editModel.AlleyWidth9
                                    }[(int)editModel.AlleyTurns - 1];
                            }

                            //Custom
                            p.AlleyWidth1 = editModel.AlleyWidth1;//Hẻm rộng
                        }
                    }

                    #endregion
                    test += "=>13";

                    //Custom
                    p.AreaTotal = editModel.AreaTotal;
                    p.AreaTotalWidth = editModel.AreaTotalWidth;
                    p.AreaTotalLength = editModel.AreaTotalLength;
                    p.AreaTotalBackWidth = editModel.AreaTotalBackWidth;
                    p.AreaConstruction = editModel.AreaConstruction;
                    p.AreaConstructionFloor = editModel.AreaConstructionFloor;
                    test += "=>14";

                    p.Floors = editModel.Floors;
                    p.Bedrooms = editModel.Bedrooms;
                    p.Bathrooms = editModel.Bathrooms;
                    p.HaveBasement = editModel.HaveBasement;
                    p.HaveMezzanine = editModel.HaveMezzanine;
                    p.HaveTerrace = editModel.HaveTerrace;
                    p.HaveGarage = editModel.HaveGarage;
                    p.HaveElevator = editModel.HaveElevator;
                    p.HaveSwimmingPool = editModel.HaveSwimmingPool;

                    p.ContactPhone = editModel.ContactPhone;
                    p.ContactEmail = editModel.ContactEmail;
                    p.Title = editModel.Title;
                    p.Content = editModel.Content;
                    test += "=>15";

                    // Area for filter only
                    p.Area = _propertyService.CalcAreaForFilter(p);

                    // AreaUsable
                    p.AreaUsable = _propertyService.CalcAreaUsable(p);
                    test += "=>16";

                    #region Price

                    //Custom
                    p.PriceProposed = editModel.PriceProposed;
                    p.NoBroker = editModel.NoBroker;

                    // PaymentMethod
                    p.PaymentMethod = _propertyService.GetPaymentMethod(editModel.PaymentMethodId);

                    // PaymentUnit
                    p.PaymentUnit = _propertyService.GetPaymentUnit(editModel.PaymentUnitId);
                    test += "=>17";

                    if (p.PriceProposed > 0)
                    {
                        // PriceProposedInVND
                        p.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(p);
                    }
                    test += "=>18";

                    #endregion

                    // Status
                    if (saveDraft == 1)
                    {
                        p.Status = _propertyService.GetStatus("st-pending");
                    }
                    else if (saveDraft == 0)
                    {
                        p.Status = _propertyService.GetStatus("st-draft");
                    }

                    // Advantages
                    //_propertyService.UpdatePropertyAdvantages(p, editModel.Advantages);

                    // DisAdvantages
                    // _propertyService.UpdatePropertyDisAdvantages(p, editModel.DisAdvantages);

                    test += "=>19";
                    // ApartmentAdvantages
                    if (editModel.ApartmentAdvantages != null && editModel.ApartmentAdvantages.Count > 0)
                    {
                        var advs = editModel.ApartmentAdvantages.Select(r => new RealEstate.ViewModels.PropertyAdvantageEntry
                        {
                            IsChecked = r.IsChecked,
                            Advantage = new PropertyAdvantagePartRecord
                            {
                                Id = r.Advantage.Id,
                                Name = r.Advantage.Name,
                                CssClass = r.Advantage.CssClass
                            }
                        });
                        _propertyService.UpdatePropertyApartmentAdvantages(p, advs);
                    }

                    test += "=>20";

                    if (editModel.ApartmentInteriorAdvantages != null && editModel.ApartmentInteriorAdvantages.Count > 0)
                    {
                        // ApartmentInteriorAdvantages
                        var advInteriors = editModel.ApartmentInteriorAdvantages.Select(r => new RealEstate.ViewModels.PropertyAdvantageEntry
                        {
                            IsChecked = r.IsChecked,
                            Advantage = new PropertyAdvantagePartRecord
                            {
                                Id = r.Advantage.Id,
                                Name = r.Advantage.Name,
                                CssClass = r.Advantage.CssClass
                            }
                        });
                        _propertyService.UpdatePropertyApartmentInteriorAdvantages(p, advInteriors);
                    }

                    test += "=>21";

                    if (editModel.TypeGroupCssClass == "gp-apartment")
                    {
                        //Custom
                        p.ApartmentNumber = editModel.ApartmentNumber;
                        p.ApartmentFloors = editModel.ApartmentFloors;
                        p.ApartmentTradeFloors = editModel.ApartmentTradeFloors;
                        p.ApartmentElevators = editModel.ApartmentElevators;
                        p.ApartmentBasements = editModel.ApartmentBasements;
                        p.ApartmentFloorTh = editModel.ApartmentFloorTh;
                        p.AreaUsable = editModel.AreaUsable;//Diện tích căn hộ
                        p.Balconies = editModel.Balconies;//Ban công
                        p.RemainingValue = editModel.RemainingValue;//chất lượng còn lại
                        p.Bedrooms = editModel.ApartmentBedrooms;
                        p.Bathrooms = editModel.ApartmentBathrooms;


                        if (editModel.ApartmentId.HasValue)
                        {
                            p.Apartment = _addressService.GetApartment(editModel.ApartmentId);
                            p.OtherProjectName = null;
                        }
                        else
                        {
                            p.Apartment = null;
                            p.OtherProjectName = editModel.OtherProjectName;
                        }
                    }

                    _mapService.UpdateMapPart(p.Id, (float)editModel.Latitude, (float)editModel.Longitude, (float)editModel.PlanMapLatitude, (float)editModel.PlanMapLongitude);
                    test += "=>22";

                    #endregion

                    #region Save Meta

                    _propertyService.UpdateMetaDescriptionKeywords(p, true);

                    #endregion
                    test += "=>23";

                    #region AdsVIPRequest

                    //Xem lại bước này, nếu sửa lại tin vip cũ thì ...
                    if (saveDraft == 1 && !oldIsAdsVip)
                    //&& _adsPaymentHistoryService.CheckIsValidVip(editModel.AdsTypeVIP, editModel.DistrictId.Value, editModel.AdsTypeCssClass))
                    {
                        p.AdsVIPRequest = true;
                        p.AdsVIPExpirationDate = DateExtension.GetEndOfDate(toDate);
                        p.AdsExpirationDate = DateExtension.GetEndOfDate(toDate);
                        p.SeqOrder = editModel.AdsTypeVIP;
                        p.Published = true;

                        //Update Payment History
                        _adsPaymentHistoryService.UpdatePaymentHistoryV2(oldStatusCssClass, oldSeqOrder,
                        oldAdsVipExpirationDate, p, user, editModel.AdsTypeVIP, (int)(toDate - fromDate).TotalDays);
                    }
                    test += "=>24";

                    #endregion

                    return Json(new { status = true, message = test });//
                }

                string _error = "Error: ";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        _error += error.ErrorMessage;
                    }
                }

                return Json(new { status = false, message = "Vui lòng nhập đầy đủ thông tin! - " + _error });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });// + " - "  + test 
            }
        }

        //Chưa sử dụng
        public JsonResult UpdatePaymentHistoryV2(string oldStatusCssClass, int oldSeqOrder, DateTime? oldAdsVipExpirationDate,
                                int propertyId, int userId, int newVip, int newDays)
        {
            try
            {
                var p = Services.ContentManager.Get<PropertyPart>(propertyId);
                var user = _groupService.GetUser(userId);

                _adsPaymentHistoryService.UpdatePaymentHistoryV2(oldStatusCssClass, oldSeqOrder, oldAdsVipExpirationDate, p, user, newVip, newDays);

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        public JsonResult UploadImage1(HttpPostedFileBase file, int contentItemId, int userId)
        {
            try
            {
                if (file == null || file.ContentLength <= 0) file = Request.Files[0];

                if (file == null || file.ContentLength <= 0) return Json(new { success = false, message = "File is null" });

                var pFile = new PropertyFilePart();

                UserPart createdUser = _groupService.GetUser(userId);

                // contentItem is a Property
                var contentItemProperty = Services.ContentManager.Get<PropertyPart>(contentItemId);
                if (contentItemProperty != null)
                {
                    pFile = _propertyService.UploadPropertyImage(file, contentItemProperty, createdUser, false);
                }
                else
                {
                    // contentItem is a Apartment
                    var contentItemApartment = Services.ContentManager.Get<LocationApartmentPart>(contentItemId);
                    if (contentItemApartment != null)
                        pFile = _addressService.UploadApartmentImage(file, contentItemApartment, createdUser.Record, true);
                    // always set isPublished = true for Apartment
                    //pFile = _addressService.UploadApartmentImage(file, contentItemApartment, createdUser, isPublished);
                    else
                    {
                        var contentItemApartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(contentItemId);
                        if (contentItemApartmentBlockInfo != null)
                            pFile = _propertyService.UploadPropertyImageForBlockInfo(file, contentItemApartmentBlockInfo, createdUser, true);
                    }
                }

                return
                    Json(
                        new
                        {
                            success = true,
                            fileId = pFile.Id,
                            fileName = pFile.Name,
                            path = pFile.Path,
                            published = pFile.Published,
                            contentItemId
                        });

            }
            catch (Exception ex)
            {

                return Json(new { success = false, message = ex.Message });
            }

        }

        #region Ajax Delete

        //xử lý từ WebRequest
        [HttpPost]
        public JsonResult AjaxDelete(int userId, int propertyId, string apiKey)
        {
            try
            {
                if (apiKey != _settingService.GetSetting("API_Key_DGND"))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                var p = Services.ContentManager.Get<PropertyPart>(propertyId);

                if (p != null)
                {
                    if (p.CreatedUser.Id != userId)
                    {
                        return Json(new { status = false, message = "Tài khoản của bạn không có quyền xóa tin đăng này." });
                    }

                    Delete(p);

                    return Json(new { status = true, message = "success" });
                }

                return Json(new { status = false, message = "failer" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult AjaxTrashed(int userId, int propertyId, string apiKey)
        {
            try
            {
                if (apiKey != _settingService.GetSetting("API_Key_DGND"))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                var p = Services.ContentManager.Get<PropertyPart>(propertyId);

                if (p != null)
                {
                    if (p.CreatedUser.Id != userId)
                    {
                        return Json(new { status = false, message = "Tài khoản của bạn không có quyền xóa tin đăng này." });
                    }

                    TrashedDeleted(p);

                    return Json(new { status = true, message = "success" });
                }

                return Json(new { status = false, message = "failer" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion

        #region Quản lý giao dịch

        public JsonResult DGNDPayment(int payType, int userId, long amount, string note, string key)
        {
            try
            {
                if (!_settingService.GetSetting("API_Key_DGND").Equals(key))
                {
                    return Json(new { status = false, message = "Không thể truy cập." });
                }

                var user = _groupService.GetUser(userId);
                _adsPaymentHistoryService.AddPaymentHistory((PayType)payType, user, amount, note);

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion

        #region QUản lý tin cần mua, cần thuê

        public JsonResult AjaxDeleteRequirementByGroup(int groupId, int userId, string apiKey)
        {
            try
            {
                if (apiKey != _settingService.GetSetting("API_Key_DGND"))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                _customerService.DeleteCustomerRequirements(groupId);
                return Json(new { status = true, message = "thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }

        }
        #endregion

        #region Quản lý tin rao

        #endregion

        #region Void

        public void Delete(PropertyPart p)
        {
            var statusDeleted = _propertyService.GetStatus("st-trashed");
            p.Status = statusDeleted;
            CancelAdsVIP(p);

            //Update StatusPropertyExchange
            //_propertyExchangeService.UpdateStatusPropertyExchange(p, "st-trashed");
        }
        public void TrashedDeleted(PropertyPart p)
        {
            var statusDeleted = _propertyService.GetStatus("st-deleted");
            p.Status = statusDeleted;
            CancelAdsVIP(p);

            //Update StatusPropertyExchange
            //_propertyExchangeService.UpdateStatusPropertyExchange(p, "st-deleted");
        }
        public void StopPublished(PropertyPart p)
        {
            p.Published = false;
            CancelAdsVIP(p);
        }
        public void StartPublished(PropertyPart p)
        {
            var statusApproved = _propertyService.GetStatus("st-approved");
            p.Published = true;
            p.Status = statusApproved;
            p.LastUpdatedDate = DateTime.Now;
            p.AdsExpirationDate = DateTime.Now.AddDays(1);//30
            p.AdsVIP = false;
            p.SeqOrder = 0;
            p.IsRefresh = false;

            //Update StatusPropertyExchange
            //_propertyExchangeService.UpdateStatusPropertyExchange(p, "st-approved");
        }
        public void RePending(PropertyPart p)
        {
            var statusPending = _propertyService.GetStatus("st-pending");
            p.Published = true;
            p.Status = statusPending;
            p.LastUpdatedDate = DateTime.Now;
            p.IsRefresh = false;

            //Update StatusPropertyExchange
            //_propertyExchangeService.UpdateStatusPropertyExchange(p, "st-pending");
        }
        public void CancelAdsVIP(PropertyPart p)
        {
            // Hủy đăng tin VIP
            p.AdsVIP = false;
            p.AdsVIPExpirationDate = null;
            p.SeqOrder = 0;
        }

        #endregion

        #region Implement IUpdateModel

        public void AddModelError(string key, Orchard.Localization.LocalizedString errorMessage)
        {
            throw new NotImplementedException();
        }

        public new bool TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) where TModel : class
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Estimation

        public PropertyPart UpdatePropertyRECORD(PropertyPart p, EstimateViewModel model, ref int step)
        {
            int userId = model.UserId;
            int domainGroupId = model.DomainGroupId;
            var lastUpdatedUser = userId > 0 ? _groupService.GetUser(userId) : _groupService.GetUser("daiphuhung");

            var lastUpdatedDate = DateTime.Now;

            #region RECORD

            step = 2;
            #region Type

            // Ads Type
            p.AdsType = _propertyService.GetAdsType("ad-selling");

            // Type
            p.Type = _propertyService.GetType(model.TypeId);
            p.TypeGroup = p.Type.Group;

            #endregion

            step = 3;
            #region Address

            // Province
            p.Province = _addressService.GetProvince(model.ProvinceId);

            // District
            p.District = _addressService.GetDistrict(model.DistrictId);

            // Ward
            p.Ward = _addressService.GetWard(model.WardId);

            // Street
            LocationStreetPartRecord street = _addressService.GetStreet(model.StreetId);
            p.Street = street;

            // Address
            p.AddressNumber = model.AddressNumber;
            p.AddressCorner = model.AddressCorner;

            // AlleyNumber
            if (p.Province.Name == "Hà Nội")
            {
                p.AlleyNumber = _propertyService.IntParseAddressNumber(model.AddressCorner);
            }
            else
            {
                p.AlleyNumber = _propertyService.IntParseAddressNumber(model.AddressNumber);
                p.AddressCorner = null;
            }

            // Street Segment
            LocationStreetPartRecord segmentStreet = _addressService.GetStreet(street, p.AlleyNumber);
            if (segmentStreet != null)
                p.Street = segmentStreet;

            #endregion

            step = 4;
            #region Legal, Direction, Location

            // LegalStatus
            p.LegalStatus = _propertyService.GetLegalStatus(model.LegalStatusId);

            // Direction
            p.Direction = _propertyService.GetDirection(model.DirectionId);

            // Location
            p.Location = _propertyService.GetLocation(model.LocationCssClass);

            #endregion

            step = 5;
            #region Alley

            if (p.Location.CssClass == "h-front")
            {
                p.StreetWidth = model.StreetWidth;
                p.DistanceToStreet = null;
                p.AlleyTurns = null;
                p.AlleyWidth = null;
                p.AlleyWidth1 = null;
                p.AlleyWidth2 = null;
                p.AlleyWidth3 = null;
                p.AlleyWidth4 = null;
                p.AlleyWidth5 = null;
                p.AlleyWidth6 = null;
                p.AlleyWidth7 = null;
                p.AlleyWidth8 = null;
                p.AlleyWidth9 = null;
            }
            else
            {
                // Alley
                p.StreetWidth = null;
                p.DistanceToStreet = model.DistanceToStreet;
                p.AlleyTurns = model.AlleyTurns;
                p.AlleyWidth1 = model.AlleyWidth1;
                p.AlleyWidth2 = model.AlleyWidth2;
                p.AlleyWidth3 = model.AlleyWidth3;
                p.AlleyWidth4 = model.AlleyWidth4;
                p.AlleyWidth5 = model.AlleyWidth5;
                p.AlleyWidth6 = model.AlleyWidth6;
                p.AlleyWidth7 = model.AlleyWidth7;
                p.AlleyWidth8 = model.AlleyWidth8;
                p.AlleyWidth9 = model.AlleyWidth9;
                if (model.AlleyTurns > 0)
                {
                    p.AlleyWidth =
                        new List<double?>
                        {
                                model.AlleyWidth1,
                                model.AlleyWidth2,
                                model.AlleyWidth3,
                                model.AlleyWidth4,
                                model.AlleyWidth5,
                                model.AlleyWidth6,
                                model.AlleyWidth7,
                                model.AlleyWidth8,
                                model.AlleyWidth9
                        }[(int)model.AlleyTurns - 1];
                }
            }

            #endregion

            step = 6;
            #region Area

            // AreaLegal
            p.AreaLegal = model.AreaLegal;
            p.AreaLegalWidth = model.AreaLegalWidth;
            p.AreaLegalLength = model.AreaLegalLength;
            p.AreaLegalBackWidth = model.AreaLegalBackWidth;

            // AreaTotal
            p.AreaTotal = model.AreaLegal;
            if (model.AreaIlegal > 0) p.AreaTotal += model.AreaIlegal;

            #endregion

            step = 7;
            #region Construction

            if (p.Type.CssClass == "tp-residential-land")
            {
                p.AreaConstruction = null;
                p.AreaConstructionFloor = null;

                p.Floors = null;
                p.Bedrooms = null;
                p.Livingrooms = null;
                p.Bathrooms = null;
                p.Balconies = null;

                p.TypeConstruction = null;
                p.Interior = null;
                p.RemainingValue = null;

                p.HaveBasement = false;
                p.HaveMezzanine = false;
                p.HaveTerrace = false;
                p.HaveGarage = false;
                p.HaveElevator = false;
                p.HaveSwimmingPool = false;
                p.HaveGarden = false;
                p.HaveSkylight = false;
            }
            else
            {
                p.AreaConstruction = model.AreaConstruction;
                p.AreaConstructionFloor = model.AreaConstructionFloor;

                p.Floors = model.Floors;
                p.Bedrooms = model.Bedrooms;
                p.Livingrooms = model.Livingrooms;
                p.Bathrooms = model.Bathrooms;
                p.Balconies = model.Balconies;

                p.TypeConstruction = _propertyService.GetTypeConstruction(model.TypeConstructionId);
                p.Interior = _propertyService.GetInterior(model.InteriorId);
                p.RemainingValue = model.RemainingValue;

                p.HaveBasement = model.HaveBasement;
                p.HaveMezzanine = model.HaveMezzanine;
                p.HaveTerrace = model.HaveTerrace;
                p.HaveGarage = model.HaveGarage;
                p.HaveElevator = model.HaveElevator;
                p.HaveSwimmingPool = model.HaveSwimmingPool;
                p.HaveGarden = model.HaveGarden;
                p.HaveSkylight = model.HaveSkylight;
            }

            #endregion

            step = 8;
            #region Contact

            // Contact
            p.ContactName = model.ContactName;
            p.ContactPhone = model.ContactPhone;
            p.ContactAddress = model.ContactAddress;
            p.ContactEmail = model.ContactEmail;

            #endregion

            step = 9;
            #region Price

            // Price
            p.PriceProposed = -1;
            p.PriceProposedInVND = -1;
            p.PaymentMethod = _propertyService.GetPaymentMethod("pm-vnd-b");
            p.PaymentUnit = _propertyService.GetPaymentUnit("unit-total");

            //// PaymentMethod
            //p.PaymentMethod = _propertyService.GetPaymentMethod(model.PaymentMethodId);

            //// PaymentUnit`
            //p.PaymentUnit = _propertyService.GetPaymentUnit(model.PaymentUnitId);

            p.PriceEstimatedRatingPoint = model.PriceEstimatedRatingPoint;
            p.PriceEstimatedComment = model.PriceEstimatedComment;

            //if (p.PriceProposed > 0)
            //{
            //    // PriceProposedInVND
            //    p.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(p);
            //}

            #endregion

            step = 10;
            #region Flag & Status

            p.Published = false;
            p.Status = _propertyService.GetStatus("st-estimate");
            p.Flag = _propertyService.GetFlag("deal-unknow");
            p.IsExcludeFromPriceEstimation = true;

            #endregion

            step = 11;
            #region User

            // User
            if (p.CreatedUser == null)
            {
                p.CreatedDate = lastUpdatedDate;
                p.CreatedUser = lastUpdatedUser.Record;
                p.FirstInfoFromUser = lastUpdatedUser.Record;
            }
            p.LastUpdatedDate = lastUpdatedDate;
            p.LastUpdatedUser = lastUpdatedUser.Record;
            p.LastInfoFromUser = lastUpdatedUser.Record;

            // transfer to current login user
            if (p.CreatedUser.UserName == "daiphuhung" && lastUpdatedUser.UserName != "daiphuhung")
            {
                p.CreatedUser = lastUpdatedUser.Record;
                p.LastUpdatedUser = lastUpdatedUser.Record;
                p.FirstInfoFromUser = lastUpdatedUser.Record;
                p.LastInfoFromUser = lastUpdatedUser.Record;
            }

            #endregion

            step = 12;
            #region Group

            if (p.UserGroup == null)
            {
                p.UserGroup = _groupService.GetGroup(domainGroupId) ?? _groupService.GetDefaultDomainGroup();
            }

            #endregion

            step = 13;
            #region OtherAdvantages & OtherDisAdvantages

            p.OtherAdvantages = model.OtherAdvantages;
            p.OtherAdvantagesDesc = model.OtherAdvantagesDesc;
            p.OtherDisAdvantages = model.OtherDisAdvantages;
            p.OtherDisAdvantagesDesc = model.OtherDisAdvantagesDesc;

            #endregion

            #endregion

            return p;
        }

        public PropertyPart UpdatePropertyPART(PropertyPart p, EstimateViewModel model, ref int step)
        {

            #region UPDATE PART

            // IdStr
            p.IdStr = p.Id.ToString();

            // Area for filter only
            p.Area = _propertyService.CalcAreaForFilter(p);

            // AreaUsable
            p.AreaUsable = _propertyService.CalcAreaUsable(p);

            step = 15;

            // Advantages
            if (model.Advantages != null && model.Advantages.Count > 0)
            {
                var advs = model.Advantages.Select(r => new RealEstate.ViewModels.PropertyAdvantageEntry
                {
                    IsChecked = r.IsChecked,
                    Advantage = new PropertyAdvantagePartRecord
                    {
                        Id = r.Advantage.Id,
                        Name = r.Advantage.Name,
                        CssClass = r.Advantage.CssClass
                    }
                });
                _propertyService.UpdatePropertyAdvantages(p, advs);
            }

            step = 16;

            // DisAdvantages
            if (model.DisAdvantages != null && model.DisAdvantages.Count > 0)
            {
                var disadvs = model.DisAdvantages.Select(r => new RealEstate.ViewModels.PropertyAdvantageEntry
                {
                    IsChecked = r.IsChecked,
                    Advantage = new PropertyAdvantagePartRecord
                    {
                        Id = r.Advantage.Id,
                        Name = r.Advantage.Name,
                        CssClass = r.Advantage.CssClass
                    }
                });
                _propertyService.UpdatePropertyDisAdvantages(p, disadvs);
            }

            step = 17;

            // Map
            _mapService.UpdateMapPart(p.Id, (float)model.Latitude, (float)model.Longitude, (float)model.PlanMapLatitude, (float)model.PlanMapLongitude);

            #endregion

            return p;
        }

        public async Task<PropertyPart> UpdatePropertyESTIMATE(PropertyPart p)
        {

            //step = 18;
            #region ESTIMATE

            // Clear UnitPrice in Cache
            //await _propertyService.ClearApplicationCache(p.Id);

            //step = 19;

            if (true) // editModel.IsValidForEstimate
            {
                var entry = await _propertyService.EstimateProperty(p.Id);
                if (entry != null) p.PriceEstimatedInVND = entry.PriceEstimatedInVND;
            }
            else
            {
                p.PriceEstimatedInVND = null;
            }

            #endregion

            return p;
        }

        [HttpPost]
        public async Task<JsonResult> EstimatePrice(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            var isValidForEstimate = _propertyService.IsValidForEstimate(p);
            double priceEstimatedInVND = -1;
            string priceEstimatedStr = "";

            if (isValidForEstimate)
            {
                var entry = await _propertyService.EstimateProperty(id);
                if (entry != null)
                {
                    priceEstimatedInVND = Math.Round((double)(entry.PriceEstimatedInVND ?? 0) * 1000, 0) * 1000000;
                    priceEstimatedStr = string.Format("{0:#,0 đ}", priceEstimatedInVND);
                }
            }

            return Json(new
            {
                Id = id,
                Message = "",
                IsValidForEstimate = isValidForEstimate,
                PriceEstimatedInVND = priceEstimatedInVND,
                PriceEstimatedStr = priceEstimatedStr,
            });
        }

        [HttpPost]
        public async Task<JsonResult> IsEstimateable(int districtId, int wardId, int streetId, string addressNumber, string addressCorner)
        {
            bool isEstimateable = await _propertyService.IsEstimateable(districtId, wardId, streetId, addressNumber, addressCorner);
            if (isEstimateable) return Json(new { isEstimateable = true });

            // Not Estimateable
            List<UnEstimatedLocationRecord> records = _unEstimatedLocationRepository.Fetch(a =>
                a.LocationDistrictPartRecord.Id == districtId &&
                a.LocationWardPartRecord.Id == wardId &&
                a.LocationStreetPartRecord.Id == streetId &&
                a.AddressNumber == addressNumber &&
                a.AddressCorner == addressCorner
                ).ToList();

            if (records.Any())
            {
                // Record đã có trong db, cập nhật nếu cần thiết

                //var record = records.First();
            }
            else
            {
                // Record chưa có trong db, tạo mới

                LocationDistrictPartRecord district = _addressService.GetDistrict(districtId);

                var record = new UnEstimatedLocationRecord
                {
                    LocationProvincePartRecord = district.Province,
                    LocationDistrictPartRecord = district,
                    LocationWardPartRecord = _addressService.GetWard(wardId),
                    LocationStreetPartRecord = _addressService.GetStreet(streetId),
                    AddressNumber = addressNumber,
                    AddressCorner = addressCorner,
                    CreatedDate = DateTime.Now
                };

                _unEstimatedLocationRepository.Create(record);
            }
            return Json(new { isEstimateable = false });
        }

        [HttpPost]
        public JsonResult IsValidUserAddress(int userId, int pId, int provinceId, int districtId, int wardId, int streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            int step = 0;

            if (string.IsNullOrEmpty(adsTypeCssClass)) adsTypeCssClass = "ad-selling";
            if (string.IsNullOrEmpty(apartmentNumber)) apartmentNumber = null;
            if (string.IsNullOrEmpty(addressCorner)) addressCorner = null;

            bool exists = false;
            bool propertyEstimate = false;
            //bool propertyExchange = false;

            try
            {
                // Kiểm tra BĐS đã có trong danh sách của user hay chưa
                step = 1;
                exists = !_propertyService.VerifyUserPropertyUnicity(userId, pId, provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

                if (apartmentId > 0 && string.IsNullOrEmpty(apartmentNumber) && exists)
                {
                    step = 2;
                    exists = false;
                } //Nếu là căn hộ chung cư mà ko nhập mã số căn hộ

                if (exists)
                {
                    step = 3;
                    PropertyPart p = _propertyService.GetUserPropertyByAddress(userId, pId, provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

                    string address = p.DisplayForAddressForOwner;

                    string info = "Giá " + string.Format("{0:0.##}", p.PriceProposedInVND) + " Tỷ, "
                                  + "Diện tích " + string.Format("{0:0.##}", p.AreaTotalWidth) + "x" +
                                  string.Format("{0:0.##}", p.AreaTotalLength);
                    info += ", Điện thoại " + p.ContactPhone;

                    step = 4;
                    string link;

                    step = 5;
                    switch (p.Status.CssClass)
                    {
                        case "st-estimate":
                            link = Url.Action("EstimationEdit", "Home", new { area = "", p.Id });
                            propertyEstimate = true;
                            break;
                        default:
                            link = Url.Action("Update", "ManageProperty", new { area = "Backend", p.Id });
                            break;
                            //propertyExchange = _propertyService.ListOwnPropertyIdsExchange().Contains(p.Id);
                    }

                    step = 6;
                    var result =
                        new
                        {
                            exists = true,
                            propertyEstimate,
                            //propertyExchange,
                            id = p.Id,
                            address,
                            link,
                            info,
                            statusCssClass = p.Status.CssClass,
                            statusName = p.Status.Name
                        };
                    return Json(result);
                }

                step = 7;
                bool existsInternal = !_propertyService.VerifyPropertyUnicity(pId, provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

                if (existsInternal)
                {
                    step = 8;
                    var pList = _propertyService.GetInternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber);
                    if (pList == null || pList.Count() == 0)
                    {
                        pList = _propertyService.GetExternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId, addressNumber, addressCorner, apartmentNumber);
                    }

                    PropertyPart p = null;

                    step = 9;
                    if (pList != null && pList.Count() > 0)
                        p = pList.OrderByDescending(a => a.LastUpdatedDate).Slice(1).First();

                    if (p != null)
                    {
                        step = 10;
                        #region return json

                        return Json(new
                        {
                            exists = false,
                            existsInternal = true,


                            // Type
                            TypeId = p.Type != null ? p.Type.Id.ToString() : "",

                            // LegalStatus, Direction, Location
                            LegalStatusId = p.LegalStatus != null ? p.LegalStatus.Id.ToString() : "",
                            DirectionId = p.Direction != null ? p.Direction.Id.ToString() : "",
                            LocationCssClass = p.Location != null ? p.Location.CssClass : "",

                            // Alley
                            DistanceToStreet = p.DistanceToStreet > 0 ? p.DistanceToStreet.ToString() : "",
                            AlleyTurns = p.AlleyTurns > 0 ? p.AlleyTurns.ToString() : "",
                            AlleyWidth1 = p.AlleyWidth1 > 0 ? p.AlleyWidth1.ToString() : "",
                            AlleyWidth2 = p.AlleyWidth2 > 0 ? p.AlleyWidth2.ToString() : "",
                            AlleyWidth3 = p.AlleyWidth3 > 0 ? p.AlleyWidth3.ToString() : "",
                            AlleyWidth4 = p.AlleyWidth4 > 0 ? p.AlleyWidth4.ToString() : "",
                            AlleyWidth5 = p.AlleyWidth5 > 0 ? p.AlleyWidth5.ToString() : "",
                            AlleyWidth6 = p.AlleyWidth6 > 0 ? p.AlleyWidth6.ToString() : "",
                            StreetWidth = p.StreetWidth > 0 ? p.StreetWidth.ToString() : "",

                            // AreaTotal
                            AreaTotal = p.AreaTotal > 0 ? p.AreaTotal.ToString() : "",
                            AreaTotalWidth = p.AreaTotalWidth > 0 ? p.AreaTotalWidth.ToString() : "",
                            AreaTotalLength = p.AreaTotalLength > 0 ? p.AreaTotalLength.ToString() : "",
                            AreaTotalBackWidth = p.AreaTotalBackWidth > 0 ? p.AreaTotalBackWidth.ToString() : "",

                            // AreaLegal
                            AreaLegal = p.AreaLegal > 0 ? p.AreaLegal.ToString() : "",
                            AreaLegalWidth = p.AreaLegalWidth > 0 ? p.AreaLegalWidth.ToString() : "",
                            AreaLegalLength = p.AreaLegalLength > 0 ? p.AreaLegalLength.ToString() : "",
                            AreaLegalBackWidth = p.AreaLegalBackWidth > 0 ? p.AreaLegalBackWidth.ToString() : "",
                            AreaIlegalRecognized = p.AreaIlegalRecognized > 0 ? p.AreaIlegalRecognized.ToString() : "",

                            // AreaUsable
                            AreaUsable = p.AreaUsable > 0 ? p.AreaUsable.ToString() : "",

                            // AreaResidential
                            AreaResidential = p.AreaResidential > 0 ? p.AreaResidential.ToString() : "",

                            // Construction
                            AreaConstruction = p.AreaConstruction > 0 ? p.AreaConstruction.ToString() : "",
                            AreaConstructionFloor = p.AreaConstructionFloor > 0 ? p.AreaConstructionFloor.ToString() : "",
                            Floors = p.Floors > 0 ? p.Floors.ToString() : "",
                            FloorsCount = p.Floors > 10 ? "-1" : (p.Floors > 0 ? p.Floors.ToString() : ""),
                            Bedrooms = p.Bedrooms > 0 ? p.Bedrooms.ToString() : "",
                            Bathrooms = p.Bathrooms > 0 ? p.Bathrooms.ToString() : "",
                            TypeConstructionId = p.TypeConstruction != null ? p.TypeConstruction.Id.ToString() : "",
                            InteriorId = p.Interior != null ? p.Interior.Id.ToString() : "",
                            RemainingValue = p.RemainingValue > 0 ? p.RemainingValue.ToString() : "",

                            HaveBasement = p.HaveBasement ? true : false,
                            HaveMezzanine = p.HaveMezzanine ? true : false,
                            HaveTerrace = p.HaveTerrace ? true : false,
                            HaveGarage = p.HaveGarage ? true : false,
                            HaveElevator = p.HaveElevator ? true : false,
                            HaveSwimmingPool = p.HaveSwimmingPool ? true : false,

                            // Advantages, 
                            Advantages = string.Join(",", _propertyService.GetPropertyAdvantages(p).Select(a => a.CssClass)),

                            // DisAdvantages
                            DisAdvantages = string.Join(",", _propertyService.GetPropertyDisAdvantages(p).Select(a => a.CssClass)),

                            // ApartmentAdvantages
                            ApartmentAdvantages = string.Join(",", _propertyService.GetPropertyApartmentAdvantages(p).Select(a => a.CssClass)),

                            // ApartmentInteriorAdvantages
                            ApartmentInteriorAdvantages = string.Join(",", _propertyService.GetPropertyApartmentInteriorAdvantages(p).Select(a => a.CssClass)),

                            // Apartment
                            ApartmentFloors = p.ApartmentFloors > 0 ? p.ApartmentFloors.ToString() : "",
                            ApartmentElevators = p.ApartmentElevators > 0 ? p.ApartmentElevators.ToString() : "",
                            ApartmentBasements = p.ApartmentBasements > 0 ? p.ApartmentBasements.ToString() : "",
                        });
                        #endregion
                    }
                }

                return Json(new { exists = false, existsInternal = false });
            }
            catch (Exception ex)
            {
                return Json(new { exists = false, existsInternal = false, info = step.ToString() + ": " + ex.Message });
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult EstimationCreatePost(EstimateViewModel createModel, FormCollection frm)
        {
            int step = 0;
            try
            {
                if (!string.IsNullOrEmpty(frm["apiKey"]) && !_settingService.GetSetting("API_Key_DGND").Equals(frm["apiKey"]))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                #region VALIDATION

                #region AddressNumber

                //if (user != null)
                //{
                //    #region Validate User Properties
                //    if (
                //        !_propertyService.VerifyUserPropertyUnicity(createModel.ProvinceId, createModel.DistrictId,
                //            createModel.WardId, createModel.StreetId, createModel.ApartmentId, createModel.AddressNumber,
                //            createModel.AddressCorner, createModel.ApartmentNumber, "ad-selling"))
                //    {
                //        PropertyPart r = _propertyService.GetUserPropertyByAddress(createModel.ProvinceId,
                //            createModel.DistrictId, createModel.WardId, createModel.StreetId, createModel.ApartmentId,
                //            createModel.AddressNumber, createModel.AddressCorner, createModel.ApartmentNumber,
                //            "ad-selling");
                //        AddModelError("AddressNumber",
                //            T("BĐS <a href='{0}'>{1}</a> đã có trong tài sản của bạn.", Url.Action("Edit", new { r.Id }),
                //                r.DisplayForAddress));
                //    }
                //    #endregion
                //}
                //else
                //{
                //    user = _membershipService.GetUser("daiphuhung");
                //}

                #endregion

                #endregion

                #region CREATE RECORD

                step = 1;
                var p = Services.ContentManager.New<PropertyPart>("Property");

                if (ModelState.IsValid)
                {
                    // Create Record
                    p = UpdatePropertyRECORD(p, createModel, ref step);

                    step = 14;
                    Services.ContentManager.Create(p);

                    // Update Part
                    p = UpdatePropertyPART(p, createModel, ref step);

                    // Estimate
                    step = 18;
                    //p = await UpdatePropertyESTIMATE(p);

                    step = 20;

                    Services.ContentManager.UpdateEditor(p, this);

                    return Json(new { status = true, message = "Finished: " + step.ToString(), PropertyId = p.Id });
                }

                #endregion

                string _error = "Error: ";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        _error += error.ErrorMessage;
                    }
                }

                return Json(new { status = false, message = "Vui lòng nhập đầy đủ thông tin! - " + step.ToString() + " " + _error });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Exception: " + step.ToString() + " " + ex.Message });
            }
        }

        [HttpPost, ValidateInput(false)]
        public JsonResult EstimationEditPost(EstimateViewModel editModel, FormCollection frmCollection)
        {
            int step = 0;
            try
            {
                if (!string.IsNullOrEmpty(frmCollection["apiKey"]) && !_settingService.GetSetting("API_Key_DGND").Equals(frmCollection["apiKey"]))
                {
                    return Json(new { status = false, message = "Không thể truy cập" });
                }

                #region VALIDATION

                #region AddressNumber

                //if (user != null)
                //{
                //    if (
                //        !_propertyService.VerifyUserPropertyUnicity(id, editModel.ProvinceId, editModel.DistrictId,
                //            editModel.WardId, editModel.StreetId, editModel.ApartmentId, editModel.AddressNumber,
                //            editModel.AddressCorner, editModel.ApartmentNumber, p.AdsType.CssClass))
                //    {
                //        PropertyPart r = _propertyService.GetUserPropertyByAddress(id, editModel.ProvinceId,
                //            editModel.DistrictId, editModel.WardId, editModel.StreetId, editModel.ApartmentId,
                //            editModel.AddressNumber, editModel.AddressCorner, editModel.ApartmentNumber,
                //            p.AdsType.CssClass);
                //        AddModelError("AddressNumber",
                //            T("BĐS <a href='{0}'>{1}</a> đã có trong tài sản của bạn.",
                //                Url.Action("Edit", new { r.Id }), r.DisplayForAddress));
                //    }
                //}

                #endregion

                #endregion

                #region UPDATE RECORD

                step = 1;
                var p = Services.ContentManager.Get<PropertyPart>(editModel.PropertyId);

                dynamic model = Services.ContentManager.UpdateEditor(p, this);

                if (TryUpdateModel(editModel))
                {
                    if (ModelState.IsValid)
                    {
                        // Update Record
                        p = UpdatePropertyRECORD(p, editModel, ref step);

                        step = 14;
                        //Services.ContentManager.Create(p);

                        // Update Part
                        p = UpdatePropertyPART(p, editModel, ref step);

                        // Estimate
                        step = 18;
                        //p = await UpdatePropertyESTIMATE(p);

                        step = 20;

                        Services.ContentManager.UpdateEditor(p, this);

                        return Json(new { status = true, message = "Finished: " + step.ToString(), PropertyId = p.Id });
                    }

                }

                #endregion

                string _error = "Error: ";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        _error += error.ErrorMessage;
                    }
                }

                return Json(new { status = false, message = "Vui lòng nhập đầy đủ thông tin! - " + step.ToString() + " " + _error });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Exception: " + step.ToString() + " " + ex.Message });
            }
        }

        #endregion

    }
}