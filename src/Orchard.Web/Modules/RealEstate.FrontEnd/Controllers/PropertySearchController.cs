using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Comments.Models;
using Orchard.Comments.Services;
using Orchard.ContentManagement;
using Orchard.Core.Containers.Extensions;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.NewLetter.Services;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Controllers
{
    [Themed]
    public class PropertySearchController : Controller, IUpdateModel
    {
        #region Init

        private readonly IAddressService _addressService;
        private readonly ICommentService _commentService;
        private readonly IContentManager _contentManager;
        private readonly ICustomerService _customerService;
        private readonly IFastFilterService _fastfilterService;
        private readonly IUserGroupService _groupService;
        private readonly INewCustomerService _newcustomerservice;
        private readonly IPropertyService _propertyService;
        private readonly ISiteService _siteService;

        private bool _debugFilter = false;
        //private IFacebookApiService _facebookApiSevice;

        public PropertySearchController(
            IFastFilterService fastfilterService,
            IAddressService addressService,
            ICustomerService customerService,
            IPropertyService propertyService,
            IUserGroupService groupService,
            INewCustomerService newcustomerservice,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            ICommentService commentService,
            //IFacebookApiService facebookApiSevice,
            IOrchardServices services)
        {
            _fastfilterService = fastfilterService;
            _addressService = addressService;
            _customerService = customerService;
            _newcustomerservice = newcustomerservice;
            _propertyService = propertyService;
            _contentManager = contentManager;
            _groupService = groupService;
            _siteService = siteService;

            _commentService = commentService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion

        #region Homepage

        public ActionResult Index()
        {
            ViewBag.Metas = _fastfilterService.PropertyGetSeoMeta("/");

            return View();
        }

        [HttpPost]
        public ActionResult AjaxGetPropertyHightLight(int? skipcount, string adsTypeCssClass)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> pList =
                _fastfilterService.GetHighlightProperties(adsTypeCssClass);

            if (skipcount < 0) skipcount = 0;

            IEnumerable<PropertyPart> result = pList.Slice(skipcount ?? 0, 1).ToList();

            if (result.Any())
            {
                PropertyDisplayEntry viewModel = _propertyService.BuildPropertyEntryFrontEnd(result.Select(p => p).First());

                return PartialView("RealEstateDetail.Compact", viewModel);
            }
            return Json("");
        }

        public ActionResult SearchProperties(PropertyDisplayIndexOptions options, string adsTypeCssClass, string flagAds,
            PagerParameters pagerParameters)
        {

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            if (string.IsNullOrEmpty(adsTypeCssClass)) adsTypeCssClass = "ad-selling";

            options.AdsTypeCssClass = flagAds;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            IContentQuery<PropertyPart, PropertyPartRecord> pList;

            switch (flagAds)
            {
                case "new":
                case "new-leasing":
                    pList = _fastfilterService.GetPropertiesNewestByAdsType(adsTypeCssClass);
                    switch (adsTypeCssClass)
                    {
                        case "ad-leasing":
                            options.TitleArticle = "Nhà đất cho thuê";
                            break;
                        case "ad-selling":
                            options.TitleArticle = "Nhà đất rao bán";
                            break;
                        default:
                            options.TitleArticle = "Tin mới đăng";
                            break;
                    }
                    break;
                case "new-auction":
                    pList = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass);
                    options.TitleArticle = "Nhà đấu giá mới đăng";
                    break;
                case "new-auction-group":
                    pList = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass).Where(a => a.UserGroup == currentDomainGroup);
                    options.TitleArticle = "Nhà đấu giá mới đăng";
                    break;
                case "gooddeal-auction":
                    pList = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now);
                    options.TitleArticle = "Nhà đấu giá rẻ";
                    break;
                case "gooddeal-auction-group":
                    pList = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now).Where(a => a.UserGroup == currentDomainGroup);
                    options.TitleArticle = "Nhà đấu giá rẻ";
                    break;
                case "gooddeal":
                    pList = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass)
                        .Where(p =>
                            ((p.Flag.Id == 32 || p.Flag.Id == 33) && p.IsExcludeFromPriceEstimation != true) || // Nhà rẻ (deal-good) và Nhà rất rẻ (deal-very-good) nhưng không bị loại khỏi định giá
                            (p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now)); // Nhà quảng cáo BĐS giá rẻ
                    options.TitleArticle = "Nhà đất giá rẻ";
                    break;
                case "vip":
                    pList = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now);
                    options.TitleArticle = "Nhà đất giao dịch gấp";
                    break;
                case "highlight":
                    pList = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now).Where(a => a.UserGroup == currentDomainGroup);
                    options.TitleArticle = "Nhà đất nổi bật";
                    break;
                default:
                    return new HttpNotFoundResult();
            }


            #region Shuffle HideAddress & ShowAddress

            // Get ShowAddress Properties

            int totalCount = pList.Count();
            IEnumerable<PropertyPart> results =
                pList.Slice(pager.GetStartIndex(), pager.PageSize)
                    .OrderByDescending(r => r.LastUpdatedDate)
                    .ToList();

            #endregion

            #region Get SEO

            string pathcurent = Request.RawUrl.Substring(1);

            var metaLayout = _fastfilterService.PropertyGetSeoMeta(pathcurent);
            options.MetaTitle = metaLayout["Title"];
            options.MetaKeywords = metaLayout["Keywords"];
            options.MetaDescription = metaLayout["Description"];

            #endregion

            #region BUILD MODEL

            dynamic pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);

            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntries(results),
                Pager = pagerShape,
                TotalCount = totalCount,
                Options = options
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        public ActionResult AjaxProperties(string adsTypeCssClass, string flagAds, PagerParameters pagerParameters)
        {

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            if (string.IsNullOrEmpty(adsTypeCssClass)) adsTypeCssClass = "ad-selling";

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            IContentQuery<PropertyPart, PropertyPartRecord> pListProperties;

            int pageSizeConfig = 10;

            // get Properties
            switch (flagAds)
            {
                case "new":
                case "new-leasing":
                    pListProperties = _fastfilterService.GetPropertiesNewestByAdsType(adsTypeCssClass);
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_New_PageSize") ?? "10");
                    if (currentDomainGroup != null && currentDomainGroup.ShortName == "clbbds") pageSizeConfig = 10;
                    break;
                case "new-auction":
                    pListProperties = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass);
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_New_PageSize") ?? "10");
                    break;
                case "new-auction-group":
                    pListProperties = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass).Where(a => a.UserGroup == currentDomainGroup);
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_New_PageSize") ?? "10");
                    break;
                case "gooddeal-auction":
                    pListProperties = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now);
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_GoodDeal_PageSize") ?? "6");
                    break;
                case "gooddeal-auction-group":
                    pListProperties = _fastfilterService.GetIsAuctionPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now).Where(a => a.UserGroup == currentDomainGroup);
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_GoodDeal_PageSize") ?? "6");
                    break;
                case "gooddeal":
                    pListProperties = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass)
                        .Where(p =>
                            ((p.Flag.Id == 32 || p.Flag.Id == 33) && p.IsExcludeFromPriceEstimation != true) || // Nhà rẻ (deal-good) và Nhà rất rẻ (deal-very-good) nhưng không bị loại khỏi định giá
                            (p.AdsGoodDeal && p.AdsGoodDealExpirationDate >= DateTime.Now)); // Nhà quảng cáo BĐS giá rẻ
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_GoodDeal_PageSize") ?? "6");
                    break;
                case "vip":
                    pListProperties = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsVIP && p.SeqOrder == 3 && p.AdsVIPExpirationDate >= DateTime.Now); //Get VIP 1
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_Leasing_Vip_PageSize") ?? "6");
                    break;
                case "highlight":
                    pListProperties = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass).Where(p => p.AdsVIP && p.SeqOrder == 3 && p.AdsVIPExpirationDate >= DateTime.Now).Where(a => a.UserGroup == currentDomainGroup);
                    pageSizeConfig = int.Parse(_fastfilterService.GetFrontEndSetting("Property_ListHighlight_PageSize") ?? "6");
                    break;
                default:
                    pListProperties = _fastfilterService.GetPropertiesByAdsType(adsTypeCssClass);


                    //var rnd = new Random();


                    // Get ShowAddress Properties
                    //var temp1 = _contentManager.Query<PropertyPart,PropertyPartRecord>().OrderBy(r=> Guid.NewGuid());

                    //Tạm thời lấy mặc định random trong 50 bđs => 
                    //results = pListProperties.Slice(50).OrderBy(r => Guid.NewGuid()).Take(pager.PageSize);

                    // Shuffle List<T>
                    //for (int i = 1; i < results.Length; i++)
                    //{
                    //    int pos = rnd.Next(i + 1);
                    //    var x = results[i];
                    //    results[i] = results[pos];
                    //    results[pos] = x;
                    //}
                    break;
            }

            pager.PageSize = pageSizeConfig;

            if (pListProperties.Count() == 0)//Nếu ko có kết quả thì lấy bđs mới nhất: (08/08/2015)
                pListProperties = _fastfilterService.GetPropertiesNewestByAdsType(adsTypeCssClass);

            //int totalCount = 0;
            IEnumerable<PropertyPart> results = pListProperties.Slice(pager.PageSize);

            #region BUILD MODEL

            var options = new PropertyDisplayIndexOptions { AdsTypeCssClass = adsTypeCssClass };
            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntries(results),
                //TotalCount = totalCount,
                Options = options
            };

            #endregion

            #region ROUTE DATA

            #endregion

            switch (flagAds)
            {
                case "gooddeal":
                    return PartialView("AjaxListPropertiesDeal", model);
                case "gooddeal-auction":
                    return PartialView("AjaxListPropertiesDeal", model);
                case "vip":
                    return PartialView("AjaxListPropertiesVip", model);
                case "new-leasing":
                    return PartialView("AjaxListPropertiesNewLeasing", model);
                case "new":
                    return PartialView("AjaxListPropertiesNew", model);
                case "new-auction":
                    return PartialView("AjaxListPropertiesNewAuction", model);
                case "highlight":
                    return PartialView("AjaxListPropertiesHighlight", model);
                default:
                    return PartialView("AjaxListPropertiesDeal", model);
            }
        }

        public async System.Threading.Tasks.Task<ActionResult> ViewReferencedProperties(int id, PagerParameters pagerParameters)
        {
            List<int> propertyIds = await _propertyService.GetListPropertiesUseToEstimate(id + "_list");

            //var pList = _fastfilterService.GetProperties().Where(a => propertyIds.Contains(a.Id)); // Chỉ lấy các BĐS cho phép hiện trên FrontEnd
            IContentQuery<PropertyPart, PropertyPartRecord> pList =
                Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Where(a => propertyIds.Contains(a.Id));
            // Lấy tất cả các BĐS đã dùng làm định giá

            int totalCount = pList.Count();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<PropertyPart> results = pList.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #region BUILD MODEL

            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntries(results),
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView("ListReferencedProperties", model);
        }

        #endregion

        #region Details

        public ActionResult RealEstateDetail(int? id)
        {
            if (id > 0)
            {
                var p = Services.ContentManager.Get<PropertyPart>((int)id);
                if (p != null && _fastfilterService.IsViewable(p))
                {
                    // build ViewModel
                    PropertyDisplayEntry detailModel = _propertyService.BuildPropertyEntry(p);
                    ViewData["current_user"] = detailModel.Property.CreatedUser.UserName;

                    detailModel.DisplayForPhone = new List<string>();
                    if (!String.IsNullOrEmpty(detailModel.DisplayForContact))
                    {
                        // search by ContactPhone
                        var find = new Regex(@"\d+");
                        MatchCollection str = find.Matches(detailModel.DisplayForContact.Replace(".", ""));
                        foreach (object i in str)
                        {
                            detailModel.DisplayForPhone.Add(i.ToString());
                        }
                    }
                    string template = _propertyService.IsPropertyExchange((int)id) ? "Parts/PropertyExchangeDetail" : "Parts/RealEstateDetail";

                    dynamic editor = Shape.DisplayTemplate(TemplateName: template, Model: detailModel,
                        Prefix: null);
                    editor.Metadata.Position = "2";

                    dynamic model = Services.ContentManager.BuildDisplay(p);
                    model.Content.Add(editor);

                    return View((object)model);
                }
            }

            return View("NotViewDetail");
        }

        public ActionResult AjaxPropertyTags(int? propertyId)
        {
            if (propertyId.HasValue)
            {
                var p = Services.ContentManager.Get<PropertyPart>(propertyId.Value);
                PropertyDisplayEntry tagModel = _propertyService.BuildPropertyEntryFrontEnd(p);
                return PartialView("AjaxPropertyTags", tagModel);
            }
            return PartialView("AjaxPropertyTags", null);
        }

        public ActionResult LoadProvinceById(int propertyId)
        {
            var p = Services.ContentManager.Get<PropertyPart>(propertyId);
            var districtIds = new List<int> { p.District.Id };

            var model = new PropertyDisplayIndexOptions
            {
                AdsTypes = _propertyService.GetAdsTypes(),
                TypeGroups = _propertyService.GetTypeGroups(),
                ProvinceId = p.Province.Id,
                Districts = _addressService.GetDistricts(p.Province.Id),
                DistrictIds = districtIds.ToArray(),
                Wards = _addressService.GetWards(p.District.Id)
            };

            // ReSharper disable once Mvc.PartialViewNotResolved
            return PartialView("FooterLinks", model);
        }

        //AjaxBreadcrumb
        public ActionResult AjaxBreadcrumb(int? propertyId)
        {
            if (propertyId.HasValue)
            {
                var p = Services.ContentManager.Get<PropertyPart>(propertyId.Value);
                PropertyDisplayEntry tagModel = _propertyService.BuildPropertyEntryFrontEnd(p);
                return PartialView("AjaxBreadcrumb", tagModel);
            }
            return PartialView("AjaxBreadcrumb", null);
        }

        public ActionResult AjaxBreadcrumbAdsType(int? adsTypeId, int? typeGroupId)
        {
            string typeGroupName = "";
            string adsTypeName = "";
            if (adsTypeId.HasValue)
            {
                adsTypeName = adsTypeId != 0 ? _propertyService.GetAdsType(adsTypeId).Name : "";
            }
            if (typeGroupId.HasValue)
            {
                typeGroupName = typeGroupId != 0 ? _propertyService.GetTypeGroup(typeGroupId).ShortName : "";
            }
            return Json(new { AdsTypeName = adsTypeName, TypeGroupName = typeGroupName });
        }

        //View Same Property
        public ActionResult ViewSameProperty(int id, PagerParameters pagerParameters)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);

            //Lấy các BĐS cùng Đường
            IContentQuery<PropertyPart, PropertyPartRecord> pList =
                _propertyService.GetProperties()
                    .Where(c => c.AdsType == p.AdsType && c.TypeGroup == p.TypeGroup && c.Province == p.Province)
                    .Where(a => a.Id != id)
                    .Where(c => p.Street != null && c.Street != null && c.Street == p.Street);

            if (pList.Count() < 5)
            {
                // Lấy các BĐS cùng Phường
                pList = _propertyService.GetProperties()
                        .Where(c => c.AdsType == p.AdsType && c.TypeGroup == p.TypeGroup && c.Province == p.Province)
                        .Where(a => a.Id != id)
                        .Where(c => p.Ward != null && c.Ward != null && c.Ward == p.Ward);

                if (pList.Count() < 5)
                {
                    // Lấy các BĐS cùng Quận
                    pList = _propertyService.GetProperties()
                            .Where(c => c.AdsType == p.AdsType && c.TypeGroup == p.TypeGroup && c.Province == p.Province)
                            .Where(a => a.Id != id)
                            .Where(c => p.District != null && c.District != null && c.District == p.District);

                    if (pList.Count() == 0)
                        pList = _propertyService.GetProperties()
                                .Where(c => c.AdsType == p.AdsType && c.TypeGroup == p.TypeGroup && c.Province == p.Province)
                                .Where(a => a.Id != id);
                }
            }

            int totalCount = pList.Count();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
            List<PropertyPart> results = pList.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #region BUILD MODEL

            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntries(results),
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView("ListSameProperties", model);
        }

        [HttpPost]
        public ActionResult LoadSameProperty(int provinceId, int districtId, int wardId, int streetId, int apartmentId,
            string addressNumber, string addressCorner, string apartmentNumber)
        {
            addressCorner = !string.IsNullOrEmpty(addressCorner) ? addressCorner : null;
            IContentQuery<PropertyPart, PropertyPartRecord> pList =
                _propertyService.GetInternalPropertiesByAddress(provinceId, districtId, wardId, streetId, apartmentId,
                    addressNumber, addressCorner, apartmentNumber);
            if (pList == null || pList.Count() == 0)
            {
                pList = _propertyService.GetExternalPropertiesByAddress(provinceId, districtId, wardId, streetId,
                    apartmentId, addressNumber, addressCorner, apartmentNumber);
            }

            PropertyPart selectedProperty = null;

            if (pList != null && pList.Count() > 0)
                selectedProperty = pList.OrderByDescending(a => a.LastUpdatedDate).Slice(1).First();

            if (selectedProperty != null)
            {
                PropertyPart p = selectedProperty;

                #region return json

                return Json(new
                {
                    success = true,

                    // Type
                    TypeId = p.Type != null ? p.Type.Id : 0,

                    // LegalStatus, Direction, Location
                    LegalStatusId = p.LegalStatus != null ? p.LegalStatus.Id : 0,
                    DirectionId = p.Direction != null ? p.Direction.Id : 0,
                    LocationCssClass = p.Location != null ? p.Location.CssClass : "",

                    // Alley
                    DistanceToStreet = p.DistanceToStreet > 0 ? p.DistanceToStreet : 0,
                    AlleyTurns = p.AlleyTurns > 0 ? p.AlleyTurns : 0,
                    AlleyWidth1 = p.AlleyWidth1 > 0 ? p.AlleyWidth1 : 0,
                    AlleyWidth2 = p.AlleyWidth2 > 0 ? p.AlleyWidth2 : 0,
                    AlleyWidth3 = p.AlleyWidth3 > 0 ? p.AlleyWidth3 : 0,
                    AlleyWidth4 = p.AlleyWidth4 > 0 ? p.AlleyWidth4 : 0,
                    AlleyWidth5 = p.AlleyWidth5 > 0 ? p.AlleyWidth5 : 0,
                    AlleyWidth6 = p.AlleyWidth6 > 0 ? p.AlleyWidth6 : 0,
                    StreetWidth = p.StreetWidth > 0 ? p.StreetWidth : 0,

                    // AreaTotal
                    AreaTotal = p.AreaTotal > 0 ? p.AreaTotal : 0,
                    AreaTotalWidth = p.AreaTotalWidth > 0 ? p.AreaTotalWidth : 0,
                    AreaTotalLength = p.AreaTotalLength > 0 ? p.AreaTotalLength : 0,
                    AreaTotalBackWidth = p.AreaTotalBackWidth > 0 ? p.AreaTotalBackWidth : 0,

                    // AreaLegal
                    AreaLegal = p.AreaLegal > 0 ? p.AreaLegal : 0,
                    AreaLegalWidth = p.AreaLegalWidth > 0 ? p.AreaLegalWidth : 0,
                    AreaLegalLength = p.AreaLegalLength > 0 ? p.AreaLegalLength : 0,
                    AreaLegalBackWidth = p.AreaLegalBackWidth > 0 ? p.AreaLegalBackWidth : 0,
                    AreaIlegalRecognized = p.AreaIlegalRecognized > 0 ? p.AreaIlegalRecognized : 0,

                    // AreaUsable
                    AreaUsable = p.AreaUsable > 0 ? p.AreaUsable : 0,

                    // AreaResidential
                    AreaResidential = p.AreaResidential > 0 ? p.AreaResidential : 0,

                    // Construction
                    AreaConstruction = p.AreaConstruction > 0 ? p.AreaConstruction : 0,
                    AreaConstructionFloor = p.AreaConstructionFloor > 0 ? p.AreaConstructionFloor : 0,
                    Floors = p.Floors > 0 ? p.Floors : 0,
                    FloorsCount = p.Floors > 10 ? -1 : (p.Floors > 0 ? p.Floors : 0),
                    Bedrooms = p.Bedrooms > 0 ? p.Bedrooms : 0,
                    Bathrooms = p.Bathrooms > 0 ? p.Bathrooms : 0,
                    TypeConstructionId = p.TypeConstruction != null ? p.TypeConstruction.Id : 0,
                    InteriorId = p.Interior != null ? p.Interior.Id : 0,
                    RemainingValue = p.RemainingValue > 0 ? p.RemainingValue : 0,
                    p.HaveBasement,
                    p.HaveMezzanine,
                    p.HaveTerrace,
                    p.HaveGarage,
                    p.HaveElevator,
                    p.HaveSwimmingPool,

                    // Advantages, 
                    Advantages = _propertyService.GetPropertyAdvantages(p).Select(a => a.CssClass),

                    // DisAdvantages
                    DisAdvantages = _propertyService.GetPropertyDisAdvantages(p).Select(a => a.CssClass),

                    // ApartmentAdvantages
                    ApartmentAdvantages = _propertyService.GetPropertyApartmentAdvantages(p).Select(a => a.CssClass),

                    // ApartmentInteriorAdvantages
                    ApartmentInteriorAdvantages =
                        _propertyService.GetPropertyApartmentInteriorAdvantages(p).Select(a => a.CssClass),

                    // Apartment
                    ApartmentFloors = p.ApartmentFloors > 0 ? p.ApartmentFloors : 0,
                    ApartmentElevators = p.ApartmentElevators > 0 ? p.ApartmentElevators : 0,
                    ApartmentBasements = p.ApartmentBasements > 0 ? p.ApartmentBasements : 0,
                });

                #endregion
            }
            return Json(new
            {
                success = false,
                ProvinceId = provinceId,
                DistrictId = districtId,
                WardId = wardId,
                StreetId = streetId,
                AddressNumber = addressNumber,
                AddressCorner = addressCorner,
                pList = 0
            });
        }

        #endregion

        #region ResultFilter

        public ActionResult ResultFilter(PropertyDisplayIndexOptions options, PagerParameters pagerParameters)
        {
            #region Filter Requirement

            if (options.flagRequirment || options.AdsTypeCssClass == "ad-buying" ||
                options.AdsTypeCssClass == "ad-renting")
            {
                if (Request.Url != null)
                {
                    string url = Request.Url.AbsoluteUri;
                    string[] str = url.Split('?');

                    return Redirect(Url.Action("ResultFilterRequirement", "PropertySearch") + "?" + str[1]);
                }
            }

            #endregion

            DateTime start1 = DateTime.Now;
            IContentQuery<PropertyPart, PropertyPartRecord> pList = _fastfilterService.SearchProperties(options);
            if (_debugFilter) Services.Notifier.Information(T("Start 1: {0}", (DateTime.Now - start1).TotalSeconds));

            if (options.PropertyId > 0)
            {
                // Search by Property Id
                if (pList.Count() == 1)
                {
                    PropertyPart property = pList.Slice(1).FirstOrDefault();
                    if (property != null)
                        return RedirectToAction("RealEstateDetail",
                            new { id = options.PropertyId, title = property.DisplayForUrl });
                }
                return View("NotViewDetail");
            }

            var start2 = DateTime.Now;

            int totalCount = pList.Count();

            if (_debugFilter) Services.Notifier.Information(T("Start 2: {0}", (DateTime.Now - start2).TotalSeconds));

            #region

            var start3 = DateTime.Now;
            string pathcurent = Request.RawUrl.Substring(1);

            if (pathcurent.Contains("?")) pathcurent = pathcurent.Split('?')[0];
            var metaLayout = _fastfilterService.PropertyGetSeoMeta(pathcurent);
            options.MetaTitle = metaLayout["Title"];
            options.MetaKeywords = metaLayout["Keywords"];
            options.MetaDescription = metaLayout["Description"];

            if (_debugFilter) Services.Notifier.Information(T("Start 3: {0}", (DateTime.Now - start3).TotalSeconds));

            //if (pathcurent == "nha-dat-ban" || pathcurent == "nha-dat-cho-thue")
            //{
            //    pager.PageSize = Convert.ToInt32(_settingService.GetSetting("Widget_Nha_Dat_Ban_Cho_Thue_PageSize"));
            //    options.TitleArticle = "Kết quả tìm kiếm";
            //}

            #endregion

            var start4 = DateTime.Now;

            #region Build Title

            if (!String.IsNullOrEmpty(options.AdsTypeCssClass))
            {
                options.AdsType = _propertyService.GetAdsType(options.AdsTypeCssClass);
            }
            if (!String.IsNullOrEmpty(options.TypeGroupCssClass) && options.TypeGroupCssClass != "gp-house-land")
            {
                options.TypeGroup = _propertyService.GetTypeGroup(options.TypeGroupCssClass);
            }
            if (options.ProvinceId > 0)
            {
                options.Province = _addressService.GetProvince(options.ProvinceId);
            }
            if (options.TypeIds != null)
            {
                options.Types = options.TypeIds.Select(i => _propertyService.GetType(i)).ToList();
            }
            else if (options.TypeId.HasValue)
            {
                options.Types = new List<PropertyTypePartRecord> { _propertyService.GetType(options.TypeId) };
            }
            if (options.DistrictIds != null)
            {
                options.Districts =
                    options.DistrictIds.Select(i => _contentManager.Get<LocationDistrictPart>(i)).ToList();
            }
            if (options.WardIds != null)
            {
                options.Wards = options.WardIds.Select(i => _contentManager.Get<LocationWardPart>(i)).ToList();
            }
            if (options.StreetIds != null)
            {
                options.Streets = options.StreetIds.Select(i => _contentManager.Get<LocationStreetPart>(i)).ToList();
            }

            #endregion

            if (_debugFilter) Services.Notifier.Information(T("Start 4: {0}", (DateTime.Now - start4).TotalSeconds));
            #region BUILD MODEL

            DateTime startPager = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            if (_debugFilter) Services.Notifier.Information(T("startPager: {0}", (DateTime.Now - startPager).TotalSeconds));

            DateTime startSlice = DateTime.Now;
            List<PropertyPart> results = pList.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            if (_debugFilter) Services.Notifier.Information(T("startSlice: {0}", (DateTime.Now - startSlice).TotalSeconds));

            DateTime startBuildModel = DateTime.Now;
            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntries(results),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };
            if (_debugFilter)
                Services.Notifier.Information(T("startBuildModel 2: {0} - Total: {1}", (DateTime.Now - startBuildModel).TotalSeconds, (DateTime.Now - start1).TotalSeconds));

            #endregion

            if (Request.IsAjaxRequest())
            {
                return PartialView("ListFilter", model);
            }

            return View(model);
        }

        [HttpPost, ActionName("ResultFilter")]
        public ActionResult ResultFilterPost(PropertyDisplayIndexOptions options, PagerParameters pagerParameters,
            FormCollection frmCollection)
        {
            var listParams = new List<string>();
            var routeValues = new RouteValueDictionary();

            // process params
            foreach (string key in frmCollection.AllKeys.Where(key => !key.Contains("RequestVerificationToken")))
            {
                if (key.Contains("WardIds") || key.Contains("StreetIds") || key.Contains("ApartmentIds") ||
                    key.Contains("DirectionIds") || key.Contains("TypeIds"))
                {
                    string[] listValues = frmCollection[key].Split(',');
                    listParams.AddRange(listValues.Select(value => key + "=" + value));
                }
                else
                {
                    if (key.Contains("DistrictIds") && frmCollection["DistrictIds"].Split(',').Count() > 1)
                    {
                        string[] listDistricts = frmCollection["DistrictIds"].Split(',');
                        listParams.AddRange(listDistricts.Select(value => key + "=" + value));
                        //Services.Notifier.Information(T("D {0}", frmCollection["DistrictIds"]));
                    }
                    else
                    {
                        routeValues.Add(key, frmCollection[key]);
                        //Services.Notifier.Information(T("R: Key: {0} - Value: {1}", key, frmCollection[key]));
                    }
                }
            }

            // general url
            string url = Url.Action("ResultFilter", routeValues);

            if (options.flagRequirment || options.AdsTypeCssClass == "ad-buying" ||
                options.AdsTypeCssClass == "ad-renting")
            {
                url = Url.Action("ResultFilterRequirement", routeValues);
            }

            string addparams = String.Join("&", listParams);
            if (url != null && (!url.Contains("?") && addparams.Length > 0))
            {
                url = url + "?" + addparams;
                //Services.Notifier.Information(T("1? {0}",url));
            }
            else if (url != null && (url.Contains("?") && addparams.Length > 0))
            {
                url = url + "&" + addparams;
                //Services.Notifier.Information(T("2& {0}",url));
            }

            // redirect
            //Services.Notifier.Information(T("{0}", url));
            return Redirect(url);
        }

        // Xem các BĐS MT tương đương
        public ActionResult ViewFrontProperty(PropertyDisplayIndexOptions options, PagerParameters pagerParameters)
        {
            int locationFront = _propertyService.GetLocation("h-front").Id;
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            IContentQuery<PropertyPart, PropertyPartRecord> pList = _fastfilterService.SearchProperties(options);
            pList = pList.Where(p => p.Location.Id == locationFront);
            int totalCount = pList.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<PropertyPart> results = pList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .OrderBy(p => p.Flag.SeqOrder)
                .ThenByDescending(p => p.LastUpdatedDate)
                .ToList();

            #region BUILD MODEL

            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntriesFront(results),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView("ListFrontProperties", model);
        }

        public ActionResult RegisterRequirmentCustomer(PropertyDisplayIndexOptions options)
        {
            if (Services.WorkContext.CurrentUser != null)
            {
                #region VALIDATION

                if (!options.ProvinceId.HasValue)
                {
                    AddModelError("ProvinceId", T("Bạn chưa tìm kiếm theo Tỉnh /TP."));
                }

                if (string.IsNullOrEmpty(options.ContactPhone))
                {
                    AddModelError("ContactPhone", T("Vui lòng nhập Số điện thoại."));
                }

                if (string.IsNullOrEmpty(options.ContactEmail))
                {
                    AddModelError("ContactEmail", T("Vui lòng nhập E-mail."));
                }
                else
                {
                    if (!Regex.IsMatch(options.ContactEmail, UserPart.EmailPattern, RegexOptions.IgnoreCase))
                    {
                        // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                        ModelState.AddModelError("ContactEmail", T("Bạn vui lòng cung cấp một địa chỉ e-mail hợp lệ."));
                    }
                }

                if (string.IsNullOrEmpty(options.ContactName))
                {
                    AddModelError("ContactName", T("Vui lòng nhập tên."));
                }

                #endregion

                if (ModelState.IsValid)
                {
                    #region Create New Customer

                    DateTime createdDate = DateTime.Now;
                    UserPartRecord createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;
                    var c = Services.ContentManager.New<CustomerPart>("Customer");

                    // Contact
                    c.ContactName = options.ContactName;
                    c.ContactPhone = options.ContactPhone;
                    c.ContactEmail = options.ContactEmail;

                    // Status
                    c.Status = _customerService.GetStatus("st-pending");
                    c.StatusChangedDate = createdDate;

                    // User
                    c.CreatedDate = createdDate;
                    c.CreatedUser = createdUser;
                    c.LastUpdatedDate = createdDate;
                    c.LastUpdatedUser = createdUser;
                    c.AdsExpirationDate = DateTime.Now.AddDays(90);
                    c.Published = true;
                    Services.ContentManager.Create(c);

                    // Updated by thanhtuank9 - 08/05/2013
                    // kiem tra xem da co email trong table EmailException và Status = true hay chua => de set lai = false( la dc nhan email);
                    _newcustomerservice.SetStatusEmailException(options.ContactEmail);

                    #endregion

                    #region Update Customer Requirements

                    if (options.AdsTypeCssClass == "ad-selling") options.AdsTypeCssClass = "ad-buying";
                    else if (options.AdsTypeCssClass == "ad-leasing") options.AdsTypeCssClass = "ad-renting";

                    var editModel = new CustomerEditRequirementViewModel
                    {
                        ProvinceId = options.ProvinceId ?? _addressService.GetProvince("TP. Hồ Chí Minh").Id,
                        DistrictIds = options.DistrictIds,
                        WardIds = options.WardIds,
                        StreetIds = options.StreetIds,
                        MinArea = options.MinAreaTotal,
                        MinWidth = options.MinAreaTotalWidth,
                        MinLength = options.MinAreaTotalLength,
                        DirectionIds = options.DirectionIds,
                        LocationId = options.LocationId,
                        MinAlleyWidth = options.MinAlleyWidth,
                        MinFloors = options.MinFloors,
                        MinPrice = options.MinPriceProposed,
                        MaxPrice = options.MaxPriceProposed,
                        OtherProjectName = options.OtherProjectName,
                        MinBedrooms = options.MinBedrooms,
                        AdsTypeId = _propertyService.GetAdsType(options.AdsTypeCssClass).Id,
                        PaymentMethodId = _propertyService.GetPaymentMethod(options.PaymentMethodCssClass).Id,
                        PropertyTypeGroupId = !string.IsNullOrEmpty(options.TypeGroupCssClass)
                            ? _propertyService.GetTypeGroup(options.TypeGroupCssClass).Id
                            : _propertyService.GetTypeGroup("gp-house").Id
                    };
                    //Add Requirement

                    #region ApartmentFloorThRange

                    switch (options.ApartmentFloorThRange)
                    {
                        case PropertyDisplayApartmentFloorTh.All:
                            break;
                        case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                            editModel.MinApartmentFloorTh = 1;
                            editModel.MaxApartmentFloorTh = 3;
                            break;
                        case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                            editModel.MinApartmentFloorTh = 4;
                            editModel.MaxApartmentFloorTh = 7;
                            break;
                        case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                            editModel.MinApartmentFloorTh = 8;
                            editModel.MaxApartmentFloorTh = 12;
                            break;
                        case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                            editModel.MinApartmentFloorTh = 12;
                            break;
                    }

                    #endregion

                    #region AlleyTurnsRange

                    int locationFront = _propertyService.GetLocation("h-front").Id;
                    int locationAlley = _propertyService.GetLocation("h-alley").Id;

                    switch (options.AlleyTurnsRange)
                    {
                        case PropertyDisplayLocation.All: // Tat ca cac vi tri
                            break;
                        case PropertyDisplayLocation.AllWalk: // Mat Tien
                            editModel.LocationId = locationFront;
                            break;
                        case PropertyDisplayLocation.Alley6: // hem 6m tro len
                            editModel.LocationId = locationAlley;
                            editModel.MinAlleyWidth = 6;
                            break;
                        case PropertyDisplayLocation.Alley5:
                            editModel.LocationId = locationAlley;
                            editModel.MinAlleyWidth = 5;
                            break;
                        case PropertyDisplayLocation.Alley4:
                            editModel.LocationId = locationAlley;
                            editModel.MinAlleyWidth = 4;
                            break;
                        case PropertyDisplayLocation.Alley3:
                            editModel.LocationId = locationAlley;
                            editModel.MinAlleyWidth = 3;
                            break;
                        case PropertyDisplayLocation.Alley2:
                            editModel.LocationId = locationAlley;
                            editModel.MinAlleyWidth = 2;
                            break;
                        case PropertyDisplayLocation.Alley:
                            editModel.LocationId = locationAlley;
                            editModel.MinAlleyWidth = 1;
                            break;
                    }

                    #endregion

                    int id = _fastfilterService.UpdateRequirements(c.Record, editModel);
                    Services.Notifier.Information(T("Yêu cầu mã số {0} đã được tạo.", id));

                    #endregion

                    return View("RegisterCustomerSuccess");
                }
            }
            // ReSharper disable once Mvc.AreaNotResolved
            return RedirectToAction("AccessDenied", "Account", new { area = "Orchard.Users" });
        }

        #endregion

        #region ResultFilterRequirement

        public ActionResult ResultFilterRequirement(PropertyDisplayIndexOptions options, PagerParameters pagerParameters)
        {
            IContentQuery<CustomerPart, CustomerPartRecord> cList = _fastfilterService.SearchCustomers(options);

            int totalCount = cList.Count();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters)
            {
                PageSize = Request.IsAjaxRequest()
                    ? int.Parse(_fastfilterService.GetFrontEndSetting("Property_New_Buying_PageSize") ?? "5")
                    : _siteService.GetSiteSettings().PageSize
            };


            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            #region Build Title

            if (!String.IsNullOrEmpty(options.AdsTypeCssClass))
            {
                options.AdsType = _propertyService.GetAdsType(options.AdsTypeCssClass);
            }
            if (!String.IsNullOrEmpty(options.TypeGroupCssClass))
            {
                options.TypeGroup = _propertyService.GetTypeGroup(options.TypeGroupCssClass);
            }
            if (options.ProvinceId > 0)
            {
                options.Province = _addressService.GetProvince(options.ProvinceId);
            }
            if (options.DistrictIds != null)
            {
                options.Districts =
                    options.DistrictIds.Select(i => _contentManager.Get<LocationDistrictPart>(i)).ToList();
            }
            if (options.WardIds != null)
            {
                options.Wards = options.WardIds.Select(i => _contentManager.Get<LocationWardPart>(i)).ToList();
            }
            if (options.StreetIds != null)
            {
                options.Streets = options.StreetIds.Select(i => _contentManager.Get<LocationStreetPart>(i)).ToList();
            }

            #endregion

            #region BUILD MODEL

            List<CustomerPart> results = cList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CustomerDisplayIndexViewModel
            {
                Customers = results
                    .Select(x => new CustomerDisplayEntry
                    {
                        Customer = x,
                        Purposes = _customerService.GetCustomerPurposes(x),
                    }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            //routeData.Values.Add("Options.Filter", options.Filter);
            //routeData.Values.Add("Options.Search", options.Search);
            //routeData.Values.Add("Options.Order", options.Order);

            //routeData.Values.Add("Options.ProvinceId", options.ProvinceId);
            //routeData.Values.Add("Options.DistrictIds", options.DistrictIds);
            //routeData.Values.Add("Options.WardIds", options.WardIds);
            //routeData.Values.Add("Options.StreetIds", options.StreetIds);

            pagerShape.RouteData(routeData);

            #endregion

            if (Request.IsAjaxRequest())
            {
                return PartialView("ListRequirements", model);
            }

            return View(model);
        }

        public ActionResult AjaxResultFilterRequirement(PropertyDisplayIndexOptions options)
        {
            IContentQuery<CustomerPart, CustomerPartRecord> cList = _fastfilterService.SearchCustomers(options);

            int pageTake = int.Parse(_fastfilterService.GetFrontEndSetting("Property_New_Buying_PageSize") ?? "5");

            #region Build Title

            if (!String.IsNullOrEmpty(options.AdsTypeCssClass))
            {
                options.AdsType = _propertyService.GetAdsType(options.AdsTypeCssClass);
            }

            #endregion

            #region BUILD MODEL

            List<CustomerPart> results = cList
                .Slice(pageTake)
                .ToList();

            var model = new CustomerDisplayIndexViewModel
            {
                Customers = results
                    .Select(x => new CustomerDisplayEntry
                    {
                        Customer = x,
                        Purposes = _customerService.GetCustomerPurposes(x),
                    }).ToList(),
                Options = options
            };

            #endregion

            return PartialView(model);
        }

        public ActionResult NewRequirmentDetail(int id, PagerParameters pagerParameters, string returnUrl)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);


            if (_fastfilterService.IsViewable(c))
            {
                CustomerDetailViewModel editModel = _fastfilterService.BuildDetailViewModel(c);
                return View("RequirmentDetail", editModel);
            }
            return View("NotViewDetail");
        }

        public ActionResult RequirmentDetail(int id, PagerParameters pagerParameters, string returnUrl)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);

            return RedirectToAction("NewRequirmentDetail", "PropertySearch",
                // ReSharper disable once Mvc.AreaNotResolved
                new { area = "RealEstate.FrontEnd", id, url = c.DisplayForUrl });
        }

        public ActionResult AjaxSearchPropertiesByCustomer(int id, PagerParameters pagerParameters)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);
            IContentQuery<PropertyPart, PropertyPartRecord> pList = _fastfilterService.SearchProperties(c);

            int totalCount = pList.Count();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<PropertyPart> results = pList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new PropertyDisplayIndexViewModel
            {
                Properties = _fastfilterService.BuildPropertiesEntries(results),
                Pager = pagerShape,
                TotalCount = totalCount
            };

            return PartialView(model);
        }

        public ActionResult AjaxLoadTheSameCustomer(int id, PagerParameters pagerParameters)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);
            IContentQuery<CustomerPart, CustomerPartRecord> cList =
                _fastfilterService.SearchCustomersRequirementTheSame(c.Record);

            int totalCount = cList.Count();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
            List<CustomerPart> results = cList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CustomerDisplayIndexViewModel
            {
                Customers = results.OrderByDescending(r => r.LastUpdatedDate)
                    .Select(x => new CustomerDisplayEntry
                    {
                        Customer = x,
                        Purposes = _customerService.GetCustomerPurposes(x),
                    }).ToList(),
                Pager = pagerShape,
                TotalCount = totalCount
            };

            return PartialView("ListRequirements", model);
        }

        #endregion

        #region Results Filter Apartment

        public ActionResult ResultFilterApartment(LocationApartmentDisplayOptions options,
            PagerParameters pagerParameters)
        {
            DateTime startSearch = DateTime.Now;
            IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> aList =
                _fastfilterService.SearchApartment(options);
            if (_debugFilter)
                Services.Notifier.Information(T("startSearch: {0}", (DateTime.Now - startSearch).TotalSeconds));

            #region Get SEO

            string pathcurent = Request.RawUrl.Substring(1);
            if (pathcurent.Contains("?")) pathcurent = pathcurent.Split('?')[0];
            var metaLayout = _fastfilterService.PropertyGetSeoMeta(pathcurent);
            options.MetaTitle = metaLayout["Title"];
            options.MetaKeywords = metaLayout["Keywords"];
            options.MetaDescription = metaLayout["Description"];

            #endregion

            int totalCount = aList.Count();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            DateTime startSlice = DateTime.Now;
            List<LocationApartmentPart> results = aList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .OrderByDescending(p => p.Name)
                .ToList();
            if (_debugFilter)
                Services.Notifier.Information(T("startSlice: {0}", (DateTime.Now - startSlice).TotalSeconds));

            DateTime startBuildModel = DateTime.Now;
            var model = new LocationApartmentIndexDisplayViewModel
            {
                LocationApartments = results.Select(r => new LocationApartmentDisplayEntry
                {
                    LocationApartment = r,
                    DefaultImgUrl = _propertyService.GetDefaultImageUrl(r)
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };
            if (_debugFilter)
                Services.Notifier.Information(T("startBuildModel: {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            if (_debugFilter)
                Services.Notifier.Information(T("startSearch 2: {0}", (DateTime.Now - startSearch).TotalSeconds));

            return View(model);
        }

        [HttpPost, ActionName("ResultFilterApartment")]
        public ActionResult ResultFilterApartmentPost(LocationApartmentDisplayOptions options,
            PagerParameters pagerParameters, FormCollection frmCollection)
        {
            var listParams = new List<string>();
            var routeValues = new RouteValueDictionary();


            // process params
            foreach (string key in frmCollection.AllKeys.Where(key => !key.Contains("RequestVerificationToken")))
            {
                if (key.Contains("ApartmentApartmentIds"))
                {
                    string[] listValues = frmCollection[key].Split(',');
                    if (frmCollection["ApartmentApartmentIds"].Split(',').Count() == 1)
                    {
                        int apartmentId = Convert.ToInt32(frmCollection["ApartmentApartmentIds"]);
                        var p = Services.ContentManager.Get<LocationApartmentPart>(apartmentId);

                        return RedirectToAction("LocationApartmentDetail",
                            new { id = apartmentId, title = p.Name.ToSlug() });
                    }

                    listParams.AddRange(listValues.Select(value => key + "=" + value));
                }
                else
                {
                    if (key.Contains("ApartmentDistrictIds") &&
                        frmCollection["ApartmentDistrictIds"].Split(',').Count() > 1)
                    {
                        string[] listDistricts = frmCollection["ApartmentDistrictIds"].Split(',');
                        listParams.AddRange(listDistricts.Select(value => key + "=" + value));
                    }
                    else
                    {
                        routeValues.Add(key, frmCollection[key]);
                    }
                }
            }

            // general url
            string url = Url.Action("ResultFilterApartment", routeValues);


            string addparams = String.Join("&", listParams);
            if (url != null && (!url.Contains("?") && addparams.Length > 0))
            {
                url = url + "?" + addparams;
            }
            else if (url != null && (url.Contains("?") && addparams.Length > 0))
            {
                url = url + "&" + addparams;
            }

            // redirect
            return Redirect(url);
        }

        public ActionResult LocationApartmentDetail(int id)
        {
            var p = Services.ContentManager.Get<LocationApartmentPart>(id);

            if (p != null)
            {
                // build ViewModel
                LocationApartmentDisplayEntry detailModel = _propertyService.BuildLocationApartmentEntry(p);

                dynamic editor = Shape.DisplayTemplate(TemplateName: "Parts/LocationApartment.Detail",
                    Model: detailModel, Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildDisplay(p);
                model.Content.Add(editor);
                return View((object)model);
            }
            return View("NotViewDetail");
        }

        public ActionResult LocationApartmentCart(int id)
        {
            var model = _propertyService.BuildApartmentCartIndex(id, true);
            return View(model);
        }

        #endregion

        #region Compare between Apartment

        public ActionResult CompareApartment()
        {
            var options = new LocationApartmentDisplayOptions();
            // ReSharper disable once Mvc.AreaNotResolved
            string path = Url.Action("CompareApartment", "PropertySearch", new { area = "RealEstate.FrontEnd" });

            var metaLayout = _fastfilterService.PropertyGetSeoMeta(path);
            options.MetaTitle = metaLayout["Title"];
            options.MetaKeywords = metaLayout["Keywords"];
            options.MetaDescription = metaLayout["Description"];

            return View(_fastfilterService.InitCompareApartment(options));
        }

        public ActionResult WithCompareApartment(int? apId, string apName)
        {
            var p = _contentManager.Get<LocationApartmentPart>(apId ?? 0);
            if (p == null)
                return Redirect("/");

            var options = new LocationApartmentDisplayOptions { ApartmentApartmentId = apId };
            LocationApartmentDisplayOptions model = _fastfilterService.InitCompareApartment(options);

            options.MetaTitle = "So sánh chung cư " + p.Name;
            options.MetaKeywords = !string.IsNullOrEmpty(p.Description) ? p.Description : p.Name;
            options.MetaDescription = p.Name;


            return View("CompareApartment", model);
        }

        [HttpPost, ActionName("CompareApartment")]
        public ActionResult CompareApartmentPost(LocationApartmentDisplayOptions model)
        {
            if (!model.ApartmentApartmentId.HasValue || !model.WithApartmentApartmentId.HasValue)
            {
                ModelState.AddModelError("NotNullApartmentApartmentId", "Vui lòng chọn dự án cần so sánh");
            }
            if (ModelState.IsValid)
            {
                var p1 = _contentManager.Get<LocationApartmentPart>(model.ApartmentApartmentId ?? 0);
                var p2 = _contentManager.Get<LocationApartmentPart>(model.WithApartmentApartmentId ?? 0);

                if (p1 == null || p2 == null)
                    return Redirect("/");

                return RedirectToAction("CompareDetail",
                    new
                    {
                        apId = model.ApartmentApartmentId,
                        apName = p1.Name.ToSlug(),
                        wId = p2.Id,
                        wName = p2.Name.ToSlug()
                    });
            }
            return View(_fastfilterService.InitCompareApartment(new LocationApartmentDisplayOptions()));
        }

        public ActionResult CompareDetail(int? apId, string apName, int? wId, string wName)
        {
            var options = new LocationApartmentDisplayOptions
            {
                ApartmentApartmentId = apId,
                WithApartmentApartmentId = wId
            };

            var p1 = _contentManager.Get<LocationApartmentPart>(apId ?? 0);
            var p2 = _contentManager.Get<LocationApartmentPart>(wId ?? 0);

            if (p1 == null || p2 == null)
                return Redirect("/");

            var model = new LocationApartmentCompareDisplayViewModel
            {
                LocationApartments = _propertyService.BuildLocationApartmentEntry(p1),
                WithLocationApartments = _propertyService.BuildLocationApartmentEntry(p2),
                Options = _fastfilterService.InitCompareApartment(options)
            };
            return View(model);
        }

        #endregion

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult RedirectUserProfile(string username)
        {
            int userid = _groupService.GetUser(username).Id;
            // ReSharper disable once Mvc.ActionNotResolved
            // ReSharper disable once Mvc.ControllerNotResolved
            return RedirectToAction("FriendPage", "PersonalPage",
                // ReSharper disable once Mvc.AreaNotResolved
                new { area = "RealEstate.MiniForum.FrontEnd", UserId = userid, UserName = username });
        }

        #region JSON

        [HttpPost]
        public ActionResult SaveProperty(int propertyid)
        {
            if (Services.WorkContext.CurrentUser != null)
            {
                var p = Services.ContentManager.Get<PropertyPart>(propertyid);

                var isSave = _propertyService.SaveUserProperties(p);
                return Json(new { success = isSave, flaguser = true });
            }
            return Json(new { success = false, flaguser = false });
        }

        [HttpPost]
        public ActionResult DeleteUserProperty(int propertyid)
        {
            if (Services.WorkContext.CurrentUser != null)
            {
                try
                {
                    _propertyService.DeleteUserProperty(propertyid);
                    return Json(new { success = true, flaguser = true });
                }
                catch
                {
                    return Json(new { success = false, flaguser = true });
                }
            }
            return Json(new { success = false, flaguser = false });
        }

        #endregion

        #region Comments

        [HttpPost]
        public ActionResult DeleteComment(int id)
        {
            CommentPart commentPart = _commentService.GetComment(id);
            if (commentPart == null) return Json(new { success = false });
            _commentService.DeleteComment(id);
            return Json(new { success = true });
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult AddComment(string name, string commentText, string email, int commentedOn)
        {
            try
            {
                IUser currentuser = Services.WorkContext.CurrentUser;

                var comment = Services.ContentManager.New<CommentPart>("Comment");
                comment.Author = !string.IsNullOrEmpty(name) ? name : currentuser.UserName;
                comment.SiteName = Services.WorkContext.CurrentSite.BaseUrl;
                comment.UserName = currentuser.UserName;
                comment.Email = email;
                comment.CommentDateUtc = DateTime.UtcNow;
                comment.CommentText = commentText;
                comment.CommentedOn = commentedOn;
                comment.CommentedOnContentItem = Services.ContentManager.Get(commentedOn);
                comment.Status = CommentStatus.Approved;

                Services.ContentManager.Create(comment);

                return Json(new { success = true, Name = comment.Author, comment.CommentText, comment.Id });
            }
            catch
            {
                TempData["CreateCommentContext.Name"] = name;
                TempData["CreateCommentContext.CommentText"] = commentText;
                TempData["CreateCommentContext.Email"] = email;
                TempData["CreateCommentContext.SiteName"] = Services.WorkContext.CurrentSite.BaseUrl;

                return Json(new { success = false });
            }
            /*
            if (ModelState.IsValid)
            {
                Services.ContentManager.Create(comment);

                var commentPart = comment.As<CommentPart>();

                if (commentPart.Status == CommentStatus.Pending)
                {
                    // if the user who submitted the comment has the right to moderate, don't make this comment moderated
                    if (Services.Authorizer.Authorize(Orchard.Comments.Permissions.ManageComments))
                        commentPart.Status = CommentStatus.Approved;
                    else
                        Services.Notifier.Information(T("Your comment will appear after the site administrator approves it."));
                }
                return Json(new { success = true, Name = commentPart.Author, CommentText = commentPart.CommentText, Id = commentPart.Id });
            }
            else
            {
                TempData["CreateCommentContext.Name"] = comment.Author;
                TempData["CreateCommentContext.CommentText"] = comment.CommentText;
                TempData["CreateCommentContext.Email"] = comment.Email;
                TempData["CreateCommentContext.SiteName"] = comment.SiteName;
            }
            return Json(new { success = false });
             * */
        }

        //public ActionResult ReloadComments(int id)
        //{
        //    IContentQuery<CommentPart, CommentPartRecord> commentsForCommentedContent =
        //        _commentService.GetCommentsForCommentedContent(id);
        //    List<CommentPart> comments =
        //        commentsForCommentedContent.Where(x => x.Status == CommentStatus.Approved)
        //            .OrderBy(x => x.Position)
        //            .List()
        //            .ToList();
        //    return PartialView("Part.ListOfComments", comments);
        //}

        [HttpPost]
        public ActionResult AjaxEditComment(int id, string name, string email, string commentText)
        {
            try
            {
                IUser currentuser = Services.WorkContext.CurrentUser;
                if (string.IsNullOrEmpty(name))
                {
                    name = currentuser.UserName;
                }
                //_commentService.UpdateComment(Id, Name, Email, "dinhgianhadat.vn", CommentText, CommentStatus.Approved);
                var comment = Services.ContentManager.Get<CommentPart>(id);
                comment.CommentText = commentText;
                comment.Author = name;
                comment.Email = email;
                comment.SiteName = "dinhgianhadat.vn";
                comment.Status = CommentStatus.Approved;
                comment.CommentDateUtc = DateTime.UtcNow;

                return Json(new { success = true, CommentText = commentText });
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = e.Message });
            }
        }

        //[HttpPost]
        //        public ActionResult AjaxCommentToFaceBook(int pId, string comment)
        //        {
        //            try
        //            {
        //                var p = Services.ContentManager.Get<PropertyPart>(pId);
        //                string linkdetail = Url.Action("RealEstateDetail", "PropertySearch",
        //// ReSharper disable once Mvc.AreaNotResolved
        //                    new {area = "RealEstate.FrontEnd", id = p.Id, title = p.DisplayForUrl});

        //                int userCurentId = Services.WorkContext.CurrentUser.As<UserPart>().Id;
        //                //_fastfilterService.PostToFaceBook(p, linkdetail, Services.WorkContext.CurrentUser.As<UserPart>().Id, comment);

        //                PropertyDisplayEntry entry = _propertyService.BuildPropertyEntryFrontEnd(p);

        //                string titlecontent = p.DisplayForTitle;

        //                var interior = new List<string> {entry.Property.DisplayForAreaConstructionLocationInfo};
        //                if (!String.IsNullOrEmpty(entry.Property.Content)) interior.Add(entry.Property.Content);
        //                if (entry.Property.IsOwner) interior.Add("Tin chính chủ");
        //                if (entry.Property.NoBroker) interior.Add("Miễn trung gian");
        //                if (entry.Property.IsAuction) interior.Add("BĐS phát mãi");
        //                string summarydetail = String.Join(", ", interior);

        //                string defaultAvatar = entry.DefaultImgUrl;

        //                _facebookApiSevice.PostToYourFacebook(linkdetail, titlecontent + "  " + summarydetail,
        //                    p.DisplayForPriceProposed, titlecontent + " " + p.DisplayForPriceProposed, defaultAvatar,
        //                    userCurentId);

        //                return Json(new {status = true});
        //            }
        //            catch (Exception e)
        //            {
        //                return Json(new {status = false, msg = e.Message + " - " + e.Data});
        //            }
        //        }

        #endregion

    }
}