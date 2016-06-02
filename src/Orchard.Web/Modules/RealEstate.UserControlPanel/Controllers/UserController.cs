using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Security.Cryptography;
using System.Collections.Generic;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Themes;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.UI.Navigation;
using Orchard.Users.Models;

using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using RealEstate.UserControlPanel.ViewModels;
using RealEstate.FrontEnd.Services;
using RealEstate.UserControlPanel.Services;
using RealEstate.FrontEnd.ViewModels;
using Newtonsoft.Json.Linq;
using RealEstate.Helpers;
using Contrib.OnlineUsers.Models;

namespace RealEstate.FrontEnd.Controllers
{
    [Themed]
    public class UserController : Controller, IUpdateModel
    {
        #region Init

        private readonly IFastFilterService _fastfilterService;
        private readonly IPropertySettingService _settingService;
        private readonly IControlPanelService _controlpanelservice;
        private readonly ISiteService _siteService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly RequestContext _requestContext;
        private readonly IPropertyService _propertyService;
        private readonly IAdsPaymentHistoryService _adsPaymentService;
        private readonly IContentManager _contentManager;
        private readonly IFacebookApiService _facebookApiSevice;
        private readonly IPropertyExchangeService _propertyExchangeService;

        public UserController(
            IFastFilterService fastfilterService,
            IControlPanelService controlpanelservice,
            IPropertySettingService settingService,
            ICustomerService customerService,
            ISiteService siteService,
            IAddressService addressService,
            IPropertyService propertyService,
            IUserGroupService groupService,
            IShapeFactory shapeFactory,
            RequestContext requestContext,
            IOrchardServices services,
            IAdsPaymentHistoryService adsPaymentService,
            IFacebookApiService facebookApiSevice,
            IContentManager contentManager,
            IPropertyExchangeService propertyExchangeService)
        {
            _fastfilterService = fastfilterService;
            _controlpanelservice = controlpanelservice;
            _settingService = settingService;
            _siteService = siteService;
            _addressService = addressService;
            _groupService = groupService;
            _propertyService = propertyService;
            _customerService = customerService;
            _requestContext = requestContext;
            _contentManager = contentManager;
            _adsPaymentService = adsPaymentService;
            _facebookApiSevice = facebookApiSevice;
            _propertyExchangeService = propertyExchangeService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        private readonly List<string> lstString = new List<string>() { "ins-money", "ins-admin-money", "ins-promotion-money", "ex-deduction-money", };

        //string format = "dd/MM/yyyy";
        //System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
        //System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.AdjustToUniversal;

        TimeSpan scopeTimeout = new TimeSpan(1, 30, 0);

        #endregion

        [Authorize]
        public ActionResult Index(UserPropertyIndexOptions options)
        {
            #region Facebook

            if (Session["PropertyId"] != null)
            {
                try
                {
                    int pId = (int)Session["PropertyId"];
                    var p = Services.ContentManager.Get<PropertyPart>(pId);
                    string linkdetail = Url.Action("RealEstateDetail", "PropertySearch", new { area = "RealEstate.FrontEnd", id = p.Id, title = p.DisplayForUrl });

                    var userCurentId = Services.WorkContext.CurrentUser.As<UserPart>().Id;
                    //_fastfilterService.PostToFaceBook(p, linkdetail, Services.WorkContext.CurrentUser.As<UserPart>().Id, comment);

                    var entry = _propertyService.BuildPropertyEntryFrontEnd(p);

                    string titlecontent = p.DisplayForTitle;

                    List<string> _interior = new List<string>();
                    _interior.Add(entry.Property.DisplayForAreaConstructionLocationInfo);
                    if (!String.IsNullOrEmpty(entry.Property.Content)) _interior.Add(entry.Property.Content);
                    if (entry.Property.IsOwner) _interior.Add("Tin chính chủ");
                    if (entry.Property.NoBroker) _interior.Add("Miễn trung gian");
                    if (entry.Property.IsAuction) _interior.Add("BĐS phát mãi");
                    string summarydetail = String.Join(", ", _interior);

                    string default_avatar = entry.DefaultImgUrl;

                    _facebookApiSevice.PostToYourFacebook(linkdetail, titlecontent + "<br>" + summarydetail, p.DisplayForTitle + " - " + p.DisplayForPriceProposed, titlecontent + " - " + p.DisplayForPriceProposed, default_avatar, userCurentId);
                }
                catch
                {
                    //Services.Notifier.Information(T("Message: {0}", ex.Message));
                }
            }

            #endregion

            var routeData = _requestContext.RouteData.Values;
            var requestData = _requestContext.HttpContext.Request;
            // Provinces
            var provinces = _addressService.GetProvinces().ToList();
            //_provinces.Insert(0, new LocationProvincePartRecord() { Id = 0, Name = "Chọn tất cả Tỉnh / TP" });
            options.Provinces = provinces;

            // ProvinceId
            if (routeData["ProvinceId"] != null) options.ProvinceId = int.Parse(routeData["ProvinceId"].ToString());
            else if (!String.IsNullOrEmpty(requestData["ProvinceId"])) options.ProvinceId = int.Parse(requestData["ProvinceId"]);

            // Districts
            options.Districts = _addressService.GetDistricts(options.ProvinceId);

            // DistrictIds
            if (routeData["DistrictIds"] != null) options.DistrictIds = routeData["DistrictIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["DistrictIds"])) options.DistrictIds = requestData["DistrictIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            // Streets
            if (options.DistrictIds != null && options.DistrictIds.Count() > 0)
                options.Streets = _addressService.GetStreets(options.DistrictIds);
            else
                options.Streets = _addressService.GetStreetsByProvince(0);

            // StreetIds
            if (routeData["StreetIds"] != null) options.StreetIds = routeData["StreetIds"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
            if (!String.IsNullOrEmpty(requestData["StreetIds"])) options.StreetIds = requestData["StreetIds"].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

            options.AdsTypes = _propertyService.GetAdsTypes();// use
            options.TypeGroups = _propertyService.GetTypeGroups();// use
            
            ViewData["ad-selling"] = _controlpanelservice.Count(options, "ad-selling");
            ViewData["ad-leasing"] = _controlpanelservice.Count(options, "ad-leasing");
            ViewData["ad-buying"] = _controlpanelservice.CountCustomerRequirement(options.ReturnStatus, "ad-buying");
            ViewData["ad-renting"] = _controlpanelservice.CountCustomerRequirement(options.ReturnStatus, "ad-renting");
            ViewData["ad-all"] = (int)ViewData["ad-selling"] + (int)ViewData["ad-leasing"];//_controlpanelservice.Count(options, "ad-selling") + _controlpanelservice.Count(options, "ad-leasing");

            #region Build Title

            if (options.ReturnStatus == "all" || options.ReturnStatus == null) ViewData["Title"] = "Tất cả tài sản";
            if (options.ReturnStatus == "view") ViewData["Title"] = "Tài sản đang hiển thị";
            if (options.ReturnStatus == "notdisplay") ViewData["Title"] = "Tài sản hết hạn hiển thị";
            if (options.ReturnStatus == "pending") ViewData["Title"] = "Tài sản đang chờ duyệt";
            if (options.ReturnStatus == "stop") ViewData["Title"] = "Tài sản ngừng đăng";
            if (options.ReturnStatus == "del") ViewData["Title"] = "Tài sản đã xóa";
            if (options.ReturnStatus == "estimate") ViewData["Title"] = "Tài sản định giá";
            if (options.ReturnStatus == "invalid") ViewData["Title"] = "Tài sản chưa hợp lệ";
            if (options.ReturnStatus == "draft") ViewData["Title"] = "Tin đang soạn";
            if (options.ReturnStatus == "userproperty") ViewData["Title"] = "BĐS lưu theo dõi";
            if (options.ReturnStatus == "exchange") ViewData["Title"] = "BĐS trao đổi";

            #endregion

            var endblance = _adsPaymentService.GetPaymentHistoryLasted(Services.WorkContext.CurrentUser.As<UserPart>());
            ViewData["TotalAmount"] = _adsPaymentService.ConvertoVND(endblance != null ? endblance.EndBalance : 0);

            var model = new UserPropertyIndexViewModel
            {
                Options = options,
            };
            return View(model);
        }

        public ActionResult ViewIndexAjax(UserPropertyIndexOptions options, PagerParameters pagerParameters)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            if (pager.PageSize != _siteService.GetSiteSettings().PageSize)
                pager.PageSize = _siteService.GetSiteSettings().PageSize;

            var pList = _controlpanelservice.GetOwnProperties(options.ReturnStatus);

            #region Build

            int AdsTypeId = 0;
            if (options.ReturnAdstype != "ad-all" && options.ReturnAdstype != null)
            {
                switch (options.ReturnAdstype) {
                    case "ad-all%2C": options.ReturnAdstype = "ad-all"; break;
                    case "ad-selling%2C": options.ReturnAdstype = "ad-selling"; break;
                    case "ad-leasing%2C": options.ReturnAdstype = "ad-leasing"; break;
                    case "ad-buying%2C": options.ReturnAdstype = "ad-buying"; break;
                    case "ad-renting%2C": options.ReturnAdstype = "ad-renting"; break;
                }
                AdsTypeId = _propertyService.GetAdsType(options.ReturnAdstype).Id;
                pList = pList.Where(p => p.AdsType.Id == AdsTypeId);
            }

            #endregion

            int totalCount = pList.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            if(options.ReturnStatus != "userproperty")
                pList = pList.OrderByDescending(u => u.LastUpdatedDate);

            var results = pList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            #region BUILD MODEL

            var model = new UserPropertyIndexViewModel
            {
                Properties = results.Select(x => new UserPropertyEntry 
                { 
                    Property = x,
                    PropertyExchange = _propertyService.GetExchangePartRecordByPropertyId(x.Id)
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount,
                ReturnUrl = options.ReturnUrl
            };

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView("AjaxViewIndex", model);
        }

        #region Post VIP

        [Authorize]
        public ActionResult AjaxPostVIP(int Id)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var p = _contentManager.Get<PropertyPart>(Id);

            var _Amount = _adsPaymentService.GetPaymentHistoryLasted(user);
            var _oldHistory = _adsPaymentService.GetPaymentHistoryByProperty(p);

            var model = new PostVIPViewModel();
            model.PropertyId = Id;
            model.Amount = _Amount != null ? _Amount.EndBalance : 0;
            model.AmountVND = _adsPaymentService.ConvertoVND(_Amount != null ? _Amount.EndBalance : 0);
            model.UserId = user.Id;
            model.DateVipFrom = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy");
            model.DateVipTo = Convert.ToDateTime(DateTime.Now.AddDays(30)).ToString("dd/MM/yyyy");
            model.UnitArray = _adsPaymentService.GetPaymentConfigsAsVip().OrderBy(r => r.Value).Select(r => r.Value).ToArray();
            model.DistrictId = p.District.Id;
            model.AdsTypeCssClass = p.AdsType.CssClass;

            return PartialView(model);
        }
        public ActionResult AjaxSubmitPostVIP(PostVIPViewModel model)
        {
            string format = "dd/MM/yyyy";
            System.Globalization.CultureInfo provider = System.Globalization.CultureInfo.InvariantCulture;
            System.Globalization.DateTimeStyles style = System.Globalization.DateTimeStyles.AdjustToUniversal;

            DateTime _adsDateFrom, _adsDateTo;

            DateTime.TryParseExact(model.DateVipFrom, format, provider, style, out _adsDateFrom);
            DateTime.TryParseExact(model.DateVipTo, format, provider, style, out _adsDateTo);

            var p = _contentManager.Get<PropertyPart>(model.PropertyId);
            // Kiểm tra số tiền có đủ đăng tin vip không
            if (model.AdsTypeVIP != 0 && model.AdsTypeVIP != 1 && model.AdsTypeVIP != 2 && model.AdsTypeVIP != 3)
            {
                return Json(new { status = false, message = "Giá trị tin VIP không đúng." });
            }
            else
            {
                if (!_adsPaymentService.CheckIsHaveMoney(model.AdsTypeVIP, p, 0, null, (int)(_adsDateTo - _adsDateFrom).TotalDays))
                    return Json(new { status = false, message = "Số tiền của bạn không đủ để thực hiện tin VIP này, Vui lòng liên hệ BQT hoặc nạp tiền thêm để tiếp tục đăng tin." });
            }

            if (_adsDateTo <= _adsDateFrom)
            {
                return Json(new { status = false, message = "Ngày hết hạn tin VIP của bạn phải lớn hơn ngày bắt đầu." });
            }

            //Xử lý đăng tin vip
            p.IsRefresh = false;
            p.AdsVIP = true;
            p.AdsExpirationDate = DateExtension.GetEndOfDate(_adsDateTo);
            p.AdsVIPExpirationDate = DateExtension.GetEndOfDate(_adsDateTo);
            p.SeqOrder = model.AdsTypeVIP;
            _adsPaymentService.UpdatePaymentHistory("st-approved", 0, true,null, p, model.AdsTypeVIP, Services.WorkContext.CurrentUser.As<UserPart>(), (_adsDateTo - _adsDateFrom).TotalDays);
            _adsPaymentService.ApprovedPaymentHistory(p);

            return Json(new { status = true, vip = model.AdsTypeVIP == 1 ? 3 : model.AdsTypeVIP == 3 ? 1 : model.AdsTypeVIP });
        }

        #endregion

        public ActionResult AjaxPanelSearch(FormCollection frmCollection,UserPropertyIndexOptions options, PagerParameters pagerParameters)
        {
            //Phải tìm theo danh sách hiện tại( tin bình thường, tin theo dõi hoặc tin trao đổi)
            string resturnStatus = frmCollection["ReturnStatusForm"];
            var pList = _controlpanelservice.GetOwnProperties(resturnStatus);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            if (pager.PageSize != _siteService.GetSiteSettings().PageSize)
                pager.PageSize = _siteService.GetSiteSettings().PageSize;

            #region TypeGroup

            if (!string.IsNullOrEmpty(frmCollection["options.TypeGroupId"]))
            {
                var lisTypeIds = Services.ContentManager.Query<PropertyTypePart, PropertyTypePartRecord>().Where(p => p.Group.Id == Convert.ToInt32(frmCollection["options.TypeGroupId"])).List().Select(a => a.Id).ToList();
                pList = pList.Where(p => lisTypeIds.Contains(p.Type.Id));
            }

            #endregion

            #region AdsType

            options.ReturnAdstype = frmCollection["FrmReturnStatus"];
            int AdsTypeId = 0;
            if (options.ReturnAdstype != "ad-all" && options.ReturnAdstype != null)
            {
                AdsTypeId = _propertyService.GetAdsType(options.ReturnAdstype).Id;
                pList = pList.Where(p => p.AdsType.Id == AdsTypeId);
            }

            #endregion

            #region Province

            if (!string.IsNullOrEmpty(frmCollection["Options.ProvinceId"]))
            {
                options.ProvinceId = Convert.ToInt32(frmCollection["Options.ProvinceId"]);
                if (options.ProvinceId.HasValue && options.ProvinceId > 0) pList = pList.Where(p => p.Province.Id == options.ProvinceId);
            }

            #endregion

            #region Districts

            if (frmCollection["Options.DistrictIds"] != null)
            {
                string[] FrmDistricts = frmCollection["Options.DistrictIds"].Split(',');
                int[] FrmDistrictIds = Array.ConvertAll<string, int>(FrmDistricts, int.Parse);

                pList = pList.Where(p => FrmDistrictIds.Contains(p.District.Id));
            }

            #endregion

            #region Streets

            if (frmCollection["Options.StreetIds"] != null)
            {
                string[] FrmStreets = frmCollection["Options.StreetIds"].Split(',');
                int[] FrmStreetIds = Array.ConvertAll<string, int>(FrmStreets, int.Parse);

                pList = pList.Where(p => FrmStreetIds.Contains(p.Street.Id));
            }

            #endregion

            int totalCount = pList.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = pList
                .OrderByDescending(r=>r.LastUpdatedDate)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            #region BUILD MODEL

            var model = new UserPropertyIndexViewModel
            {
                Properties = results.Select(x => new UserPropertyEntry { Property = x }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            return PartialView("AjaxViewIndex", model);
        }
        
        public ActionResult Submit(int acId)
        {
            object results = new { acId = acId, success = true, message = "Xóa thành công" };
            return Json(results);
        }

        #region Chọn nhiều tin
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Properties"></param>
        /// <param name="flag">1.Delete 2.Refresh 3.Stop 4.Start 5. Deleted 6. Restore 7. UserProperty Delete</param>
        /// <returns></returns>
        public ActionResult AjaxExcuteCollection(string Properties, int flag)
        {
            try
            {
                string[] PropertiesArray = Properties.Split(',');
                switch (flag)
                {
                    case 1://ajax Delete
                        for (int i = 0; i < PropertiesArray.Length; i++)
                        {
                            var p = Services.ContentManager.Get<PropertyPart>(Convert.ToInt32(PropertiesArray[i]));
                            if (p != null)
                            {
                                #region SECURITY

                                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty))
                                        return Json(new { status = false, message = "Not authorized to publish properties" });

                                #endregion
                                //p.Status = statusDeleted;
                                Delete(p);
                            }
                        }
                        return Json(new { status = true });
                    //case 2:// ajax refresh
                    //    for (int i = 0; i < PropertiesArray.Length; i++)
                    //    {
                    //        var p = Services.ContentManager.Get<PropertyPart>(Convert.ToInt32(PropertiesArray[i]));
                    //        if (p != null)
                    //        {
                    //            p.LastUpdatedDate = DateTime.Now;
                    //        }
                    //    }
                    //    return Json(new { status = true });
                    case 3:// ajajx stop
                        for (int i = 0; i < PropertiesArray.Length; i++)
                        {
                            var p = Services.ContentManager.Get<PropertyPart>(Convert.ToInt32(PropertiesArray[i]));
                            if (p != null)
                            {
                                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty))
                                        return Json(new { status = false, message = "Not authorized to publish properties" });

                                //p.Published = false;
                                StopPublished(p);
                            }
                            else
                            {
                                return Json(new { status = false, message = "Ko dc!" });
                            }
                        }
                        return Json(new { status = true});
                    case 4:// ajax start
                        //for (int i = 0; i < PropertiesArray.Length; i++)
                        //{
                        //    var p = Services.ContentManager.Get<PropertyPart>(Convert.ToInt32(PropertiesArray[i]));

                        //    if (p != null)
                        //    {
                        //        if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                        //            if (!Services.Authorizer.Authorize(Permissions.PublishProperty))
                        //                return Json(new { status = false, message = "Not authorized to publish properties" });

                        //        StartPublished(p);
                        //    }
                        //}
                        return Json(new { status = true });
                    case 5://ajax trashed deleted
                        for (int i = 0; i < PropertiesArray.Length; i++)
                        {
                            var p = Services.ContentManager.Get<PropertyPart>(Convert.ToInt32(PropertiesArray[i]));

                            if (p != null)
                            {
                                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty))
                                        return Json(new { status = false, message = "Not authorized to publish properties" });

                                //p.Status = statusTrasheDdeleted;
                                TrashedDeleted(p);
                            }
                        }
                        return Json(new { status = true });
                    case 6:// ajax restore trashed
                        for (int i = 0; i < PropertiesArray.Length; i++)
                        {
                            var p = Services.ContentManager.Get<PropertyPart>(Convert.ToInt32(PropertiesArray[i]));

                            if (p != null)
                            {
                                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty))
                                        return Json(new { status = false, message = "Not authorized to publish properties" });

                                RePending(p);
                            }
                        }
                        return Json(new { status = true });
                    case 7:// ajax delete UserProperty
                        for (int i = 0; i < PropertiesArray.Length; i++)
                        {
                            _propertyService.DeleteUserProperty(Convert.ToInt32(PropertiesArray[i]));
                        }
                        return Json(new { status = true });
                    default:
                        return Json(new { status = false, message = "default error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false, message = e.Message });
            }
        }
        #endregion

        #region Ajax

        //[HttpPost]
        public ActionResult AjaxRefresh(int acId, string returnUrl)
        {
            var p = Services.ContentManager.Get<PropertyPart>(acId);
            if (p != null)
            {
                //var oldIsAsVIP = p.AdsVIP && p.AdsVIPExpirationDate <= DateTime.Now && !p.AdsVIPRequest;
                p.LastUpdatedDate = DateTime.Now;
                p.IsRefresh = true;

                Services.Notifier.Information(T("{0} đã được làm mới thành công.", p.DisplayForTitle));
            }
            return RedirectToAction("Index");
            //return RedirectToAction("Edit", "Home", new { area = "RealEstate.FrontEnd", id = acId });
        }

        public ActionResult AjaxDelete(int pId, string returnUrl)
        {
            var p = Services.ContentManager.Get<PropertyPart>(pId);

            if (p != null)
            {
                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty, T("Not authorized to publish properties")))
                        return new HttpUnauthorizedResult();

                Delete(p);
            }
            Services.Notifier.Information(T("{0} đã được xoá thành công.", p.DisplayForTitle));
            //return this.RedirectLocal(returnUrl);
            return RedirectToAction("Index");
        }
        public ActionResult AjaxTrashedDeleted(int pId, string returnUrl)
        {
            var p = Services.ContentManager.Get<PropertyPart>(pId);

            if (p != null)
            {
                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty, T("Not authorized to publish properties")))
                        return new HttpUnauthorizedResult();

                TrashedDeleted(p);
            }
            Services.Notifier.Information(T("Tin rao {0} đã được xoá vĩnh viễn.", p.DisplayForTitle));
            //return this.RedirectLocal(returnUrl);
            return RedirectToAction("Index");
        }
        
        public ActionResult AjaxStopPublished(int pId, string returnUrl)
        {
            var p = Services.ContentManager.Get<PropertyPart>(pId);

            if (p != null)
            {
                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty, T("Not authorized to publish properties")))
                        return new HttpUnauthorizedResult();

                StopPublished(p);
                Services.Notifier.Information(T("{0} đã ngừng đăng.", p.DisplayForTitle));

            }
            //return this.RedirectLocal(returnUrl);
            return RedirectToAction("Index");
        }
        
        public ActionResult AjaxStartPublished(int pId, string returnUrl)
        {
            var p = Services.ContentManager.Get<PropertyPart>(pId);

            //if (p != null)
            //{
            //    if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
            //        if (!Services.Authorizer.Authorize(Permissions.PublishProperty, T("Not authorized to publish properties")))
            //            return new HttpUnauthorizedResult();

            //    StartPublished(p);

            //    Services.Notifier.Information(T("{0} đã được chuyển vào danh sách chờ duyệt để tiếp tục đăng.", p.DisplayForTitle));
            //}
            //return this.RedirectLocal(returnUrl);

            return RedirectToAction("Edit", "Home", new { area = "RealEstate.FrontEnd", id = pId});
        }

        public ActionResult AjaxRePending(int pId, string returnUrl)
        {
            var p = Services.ContentManager.Get<PropertyPart>(pId);
            if (p != null)
            {
                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.PublishProperty, T("Not authorized to publish properties")))
                        return new HttpUnauthorizedResult();

                RePending(p);
                //Services.Notifier.Information(T("Property {0} updated", p.Id));
            }
            //return this.RedirectLocal(returnUrl);
            return RedirectToAction("Index");
        }

        public ActionResult RealEstateDetailUC(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            var detailModel = _propertyService.BuildPropertyEntry(p);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/RealEstateDetailUC", Model: detailModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildDisplay(p);
            model.Content.Add(editor);

            return View((object)model);
        }

        public ActionResult ListCustomerRequirement(CustomerRequirementFilterViewModel _options, PagerParameters pagerParameters)
        {
            CustomerIndexOptions options = new CustomerIndexOptions();
            options.AdsTypeCssClass = _options.AdsTypeCssClass;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            #region DEFAULT OPTIONS

            //// default options
            //if (options == null)
            //{
            //    options = new CustomerIndexOptions();

            //    options.StatusId = 0;
            //    options.ProvinceId = 0;
            //    options.DistrictId = 0;
            //    options.WardId = 0;
            //    options.StreetId = 0;

            //    options.DirectionId = 0;
            //    options.LocationId = 0;

            //    options.CreatedUserId = 0;
            //    options.LastUpdatedUserId = 0;
            //}

            //options.Status = _customerService.GetStatus();
            //options.Purposes = _customerService.GetPurposes();
            //options.Provinces = _addressService.GetProvinces();
            //if (!options.ProvinceId.HasValue || options.ProvinceId == 0) options.ProvinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;
            //options.Districts = _addressService.GetDistricts(options.ProvinceId);

            //if (options.DistrictIds != null && options.DistrictIds.Count() == 1)
            //{
            //    options.DistrictId = options.DistrictIds[0];
            //}
            //options.Wards = _addressService.GetWards(options.DistrictId);
            //options.Streets = _addressService.GetStreets(options.DistrictId);


            //options.Directions = _propertyService.GetDirections();
            //options.Locations = _propertyService.GetLocations();

            //options.ServedUsers = _customerService.GetServedUsers();

            //options.Users = _groupService.GetUsers();

            //options.NeedUpdateDate = DateTime.Now.AddDays(-double.Parse(_settingService.GetSetting("DaysToUpdateCustomer") ?? "60"));

            #endregion

            var cList = _controlpanelservice.GetOwnCustomerRequirement();

            #region SECURITY

            if (!Services.Authorizer.Authorize(Permissions.MetaListCustomers))
            {
                // Show own properties only
                cList = cList.Where(p => p.CreatedUser.Id == Services.WorkContext.CurrentUser.As<UserPart>().Id);
            }

            #endregion

            #region FILTER

            //Lay yeu cau ma user do tao
            cList = cList.Where(p => p.CreatedUser.Id == Services.WorkContext.CurrentUser.As<UserPart>().Id);
            switch (_options.ReturnStatus)
            {
                case "all": break;
                case "view":
                    options.StatusId = _customerService.GetStatus("st-approved").Id;
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    cList = cList.Where(p => p.AdsExpirationDate >= DateTime.Now);
                    break;
                case "notdisplay": 
                    options.StatusId = _customerService.GetStatus("st-approved").Id; 
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    cList = cList.Where(p => p.AdsExpirationDate < DateTime.Now);
                    break;
                case "pending":
                    options.StatusId = _customerService.GetStatus("st-pending").Id;
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    break;
                case "invalid": 
                    options.StatusId = _customerService.GetStatus("st-invalid").Id;
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    break;
                case "stop": 
                    options.StatusId = _customerService.GetStatus("st-onhold").Id; 
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    break;
                case "draft": 
                    options.StatusId = _customerService.GetStatus("st-draft").Id; 
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    break;
                case "del":
                    options.StatusId = _customerService.GetStatus("st-deleted").Id; 
                    cList = cList.Where(p => p.Status.Id == options.StatusId);
                    break;
                default: break;
            }

            if (!string.IsNullOrEmpty(options.AdsTypeCssClass)) options.AdsTypeId = _propertyService.GetAdsType(options.AdsTypeCssClass).Id;
            if (options.AdsTypeId.HasValue) cList = cList.Where(p => p.Requirements.Any(a => a.AdsTypePartRecord.Id == options.AdsTypeId));
            #endregion

            int totalCount = cList.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            #region ORDER

            switch (options.Order)
            {
                case CustomerOrder.LastUpdatedDate:
                    cList = cList.OrderByDescending(u => u.LastUpdatedDate);
                    break;
                case CustomerOrder.CreatedDate:
                    cList = cList.OrderBy(u => u.CreatedDate);
                    break;
                case CustomerOrder.ContactName:
                    cList = cList.OrderBy(u => u.ContactName);
                    break;
            }

            #endregion

            #region BUILD MODEL

            var results = cList
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new CustomerIndexViewModel
            {
                Customers = results.Select(x => new CustomerEntry { 
                    Customer = x.Record,
                    Purposes = _customerService.GetCustomerPurposes(x)
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
            //routeData.Values.Add("Options.ProvinceId", options.ProvinceId);

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView("ListCustomerRequirement", model);
        }

        #endregion

        #region Void

        public void Delete(PropertyPart p)
        {
            var statusDeleted = _propertyService.GetStatus("st-trashed");
            p.Status = statusDeleted;
            CancelAdsVIP(p);

            //Update StatusPropertyExchange
            _propertyExchangeService.UpdateStatusPropertyExchange(p, "st-trashed");
        }
        public void TrashedDeleted(PropertyPart p)
        {
            var statusDeleted = _propertyService.GetStatus("st-deleted");
            p.Status = statusDeleted;
            CancelAdsVIP(p);

            //Update StatusPropertyExchange
            _propertyExchangeService.UpdateStatusPropertyExchange(p, "st-deleted");
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
            p.AdsExpirationDate = DateTime.Now.AddDays(30);
            p.AdsVIP = false;
            p.SeqOrder = 0;
            p.IsRefresh = false;

            //Update StatusPropertyExchange
            _propertyExchangeService.UpdateStatusPropertyExchange(p, "st-approved");
        }
        public void RePending(PropertyPart p)
        {
            var statusPending = _propertyService.GetStatus("st-pending");
            p.Published = true;
            p.Status = statusPending;
            p.LastUpdatedDate = DateTime.Now;
            p.IsRefresh = false;

            //Update StatusPropertyExchange
            _propertyExchangeService.UpdateStatusPropertyExchange(p, "st-pending");
        }
        public void CancelAdsVIP(PropertyPart p)
        {
            // Hủy đăng tin VIP
            p.AdsVIP = false;
            p.AdsVIPExpirationDate = null;
            p.SeqOrder = 0;
        }

        #endregion

        #region Quản lý lịch sử giao dịch

        [Authorize]
        public ActionResult PaymentHistory(AdsPaymentFrontEndOptions options, PagerParameters pagerParameters)
        {
            #region Init Options

            options.ListAdsPaymentConfig = _adsPaymentService.GetPaymentConfigs();

            #endregion

            #region Filter

            IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> _list = _adsPaymentService.PaymentHistoryFilter(options);

            #endregion

            #region Slice

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = _list.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
            var _endblance = _adsPaymentService.GetPaymentHistoryLasted(Services.WorkContext.CurrentUser.As<UserPart>());
            options.TotalAmount = _adsPaymentService.ConvertoVND(_endblance != null ? _endblance.EndBalance : 0);
            var results = _list.OrderByDescending(r => r.DateTrading).Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #endregion

            #region build Model
            var model = new AdsPaymentHistoryFrontEndIndexViewModel
            {
                AdsPaymentEntry = results.Select(r => new AdsPaymentFrontEndEntry()
                {
                    AdsPaymentHistory = r.Record,
                    PropertyPartEntry = r.Property != null ? _contentManager.Get<PropertyPart>(r.Property.Id) : null,
                    AmountVND = _adsPaymentService.ConvertoVND(lstString.Contains(r.PaymentConfig.CssClass) ? r.EndBalance - r.StartBalance : r.PaymentConfig.Value * r.PostingDates),
                    EndBlanceVND = _adsPaymentService.ConvertoVND(r.EndBalance)
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };
            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        //Nap tien vao tai khoan qua thẻ cào

        [Authorize]
        public ActionResult CardPayment()
        {
            var model = new PaymentCardCreate
            {
                GetPromotion = _adsPaymentService.GetPaymentConfigValue("ins-promotion-money")
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult CardPayment(PaymentCardCreate createModel)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (ModelState.IsValid)
            {
                string urlRequest = "";
                string accessKey = _settingService.GetSetting("1Pay_Card_Access_Key");
                string appSecret = _settingService.GetSetting("1Pay_Card_Secret"); 
                //int timerequest = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;// DateTime.Now.Ticks;DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                string cardCode = createModel.Code;//1pay.vn: pin
                string cardSeri = createModel.Serial;//1pay.vn: serial
                string provider = createModel.Provider;//1pay.vn: type

                string data = "access_key=" + accessKey + "&pin=" + cardCode + "&serial=" + cardSeri + "&type=" + provider;
                string sign = Hash_Mac(data, appSecret);
                try
                {
                    urlRequest = string.Format("https://api.1pay.vn/card-charging/v2/topup?access_key={0}&type={1}&pin={2}&serial={3}&signature={4}",//&ext={}
                                                                    accessKey, provider, cardCode, cardSeri, sign);

                    HttpWebRequest request = WebRequest.Create(urlRequest) as HttpWebRequest;

                    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                    {
                        StreamReader reader = new StreamReader(response.GetResponseStream());

                        string vals = reader.ReadToEnd();

                        dynamic json = JObject.Parse(vals);
                        int status = (int)json.status;
                        if (status != 0)
                        {
                            string mesage = json.description.ToString();
                            Services.Notifier.Information(T("Lỗi: {0}", mesage));

                            return View(createModel);
                        }
                        else
                        {
                            long amount = (long)json.amount;
                            string note = createModel.Note + "\n Số seri: " + createModel.Code + "\n Mã thẻ: " + createModel.Serial + "\n Provider: " + createModel.Provider ;
                            //Services.Notifier.Information(T("Success: {0}", note));
                            _adsPaymentService.AddPaymentHistory(PayType.PayByCard, user, amount, note);
                        }
                    }
                }
                catch(Exception e)
                {
                    Services.Notifier.Information(T("Exception Error 2: {0} - {1}", e.Message, e.Source));
                }
                //return View(createModel);
                return RedirectToAction("PaymentHistory");
            }
            else
            {
                return View(createModel);
            }
        }

        #region Pay.tocdo.vn

        public ActionResult PhonePayment(string appid, string sign, long time, string smsid, string sender, string momsg, string serviceid)
        {
            string app_secret = _settingService.GetSetting("PayTocDo_APP_Secret"); //"mS60RRTrhzcEfzrYwtRd05c5Gjwh03Oc";
            string dgnd_hash_mac = Hash_Mac(appid + time + smsid + sender + momsg + serviceid, app_secret);
            int cdr = 2;
            string msg = "";
            int status = 1;

            if (dgnd_hash_mac == sign)
            {
                // Tiến hành xử lý business tại đây
                //Lấy User ID từ SMS
                int UserID = Convert.ToInt32(momsg.Split(' ').Last());

                // Kiểm tra UserID có đúng ko?
                var user = _contentManager.Get<UserPart>(UserID);
                if (user != null)
                {
                    //If đúng 
                    long amount = 15000;
                    string note = "Tài khoản vừa được nạp thêm " + amount + " VNĐ bằng tin nhắn, từ số đt: " + sender;
                    _adsPaymentService.AddPaymentHistory(PayType.PayByPhone, user, amount, note);
                    cdr = 1;
                    msg = "dinhgianhadat.vn: Tai khoan " + UserID + " vua duoc nap them " + amount + " VNĐ bang tin nhan, tu so dt: " + sender;
                }
                else
                {
                    //Ngược lại
                    cdr = 2;
                    msg = "Ma so khach hang khong dung!";
                }
            }
            else
            {
                status = -1;
                msg = "Yeu cau khong hop le!";
            }
            return Json(new { status = status, mt = new { cdr = cdr, mtmsg = msg, receiver = sender, type = 0 } }, JsonRequestBehavior.AllowGet);
        }
        
        #endregion

        #region 1pay.vn Phone Payment
        
        public ActionResult OnePayPhonePayment(string access_key, string command, string mo_message, string msisdn, string request_id, string request_time, string short_code, string signature)
        {
            var appSecret = _settingService.GetSetting("1Pay_SMS_Secret"); //"mS60RRTrhzcEfzrYwtRd05c5Gjwh03Oc";

            var data = "access_key=" + access_key + "&command=" + command + "&mo_message=" + mo_message + "&msisdn=" + msisdn + "&request_id=" + request_id + "&request_time=" + request_time + "&short_code=" + short_code;
            
            var dgndHashMac = Hash_Mac(data, appSecret);

            var msg = "";
            var status = 0;//0. Khong tinh pi - 1. Tinh phi

            if (dgndHashMac == signature)
            {
                //Lấy User ID từ SMS
                var userId = Convert.ToInt32(mo_message.Split(' ').Last());
                
                // Kiểm tra UserID có đúng ko?
                var user = _contentManager.Get<UserPart>(userId);
                if (user != null)
                {
                    //If đúng 
                    long amount = 15000;
                    int subShortCode = Convert.ToInt32(short_code.Substring(1, 1));
                    switch (subShortCode)
                    {
                        case 0:
                            amount = 500;
                            break;
                        case 1:
                            amount = 1000;
                            break;
                        case 2:
                            amount = 2000;
                            break;
                        case 3:
                            amount = 3000;
                            break;
                        case 4:
                            amount = 4000;
                            break;
                        case 5:
                            amount = 5000;
                            break;
                        case 6:
                            amount = 10000;
                            break;
                        case 7:
                            amount = 15000;
                            break;
                        default:
                            amount = 15000;
                            break;
                    }
                    var note = "Tài khoản vừa được nạp thêm " + amount + " VNĐ bằng tin nhắn, từ số đt: " + msisdn;
                    _adsPaymentService.AddPaymentHistory(PayType.PayByPhone, user, amount, note);
                    msg = "dinhgianhadat.vn: Tai khoan " + userId + " vua duoc nap them " + amount + " VND bang tin nhan, tu so dt: " + msisdn;
                    status = 1;
                }
                else
                {
                    //Ngược lại
                    msg = "Ma so khach hang khong dung!";
                }
            }
            else
            {
                msg = "Yeu cau khong hop le!";
            }
            return Json(new { status = status, sms = msg, type = "text" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Hash Mac

        private static readonly Encoding encoding = Encoding.UTF8;
        private string Hash_Mac(string message,string app_secret)
        {
            byte[] keyByte = encoding.GetBytes(app_secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                hmacsha256.ComputeHash(messageBytes);

                return ByteToString(hmacsha256.Hash);
            }
        }
        
        private string ByteToString(byte[] buff)
        {
            string sbinary = "";
            for (int i = 0; i < buff.Length; i++)
                sbinary += buff[i].ToString("x2"); /* hex format */
            return sbinary;
        }

        #endregion

        #endregion

        #region Profile

        [HttpPost]
        public ActionResult GetProfile()
        {
            IUser user = Services.WorkContext.CurrentUser;
            if (user != null)
            {
                UserUpdateProfilePart personal = user.As<UserUpdateProfilePart>();

                List<string> fullname = new List<string>();
                if (!string.IsNullOrEmpty(personal.FirstName)) fullname.Add(personal.FirstName);
                if (!string.IsNullOrEmpty(personal.LastName)) fullname.Add(personal.LastName);

                var _name = "";

                if (!string.IsNullOrEmpty(personal.DisplayName))
                {
                    _name = personal.DisplayName;
                }
                else if (fullname.Count > 0)
                {
                    _name = String.Join(" ", fullname);
                }
                else
                {
                    _name = user.UserName;
                }

                var _number = personal.Phone ?? "";
                var _email = user.As<UserPart>().Email ?? "";
                var _address = personal.Address ?? "";

                return Json(new { success = true, _name, _number, _email, _address });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        [Authorize]
        public ActionResult EditProfile()
        {
            var userId = Services.WorkContext.CurrentUser.Id;
            var userprofile = Services.ContentManager.Get<UserUpdateProfilePart>(userId);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/UserProfile.Edit", Model: new UserProfileViewModel { UserProfile = userprofile }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(userprofile);
            model.Content.Add(editor);
            return View((object)model);
        }

        [Authorize]
        [HttpPost, ActionName("EditProfile")]
        public ActionResult EditProfilePOST()
        {
            if (Services.WorkContext.CurrentUser == null)
            {
                return HttpNotFound();
            }

            IUser user = Services.WorkContext.CurrentUser;

            dynamic shape = Services.ContentManager.UpdateEditor(user.ContentItem, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                return View("Edit", (object)shape);
            }

            Services.Notifier.Information(T("Thông tin tài khoản của bạn đã được lưu."));

            return RedirectToAction("EditProfile");
        }

        #endregion

        #region UpdateModel

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        #endregion

        public ActionResult AjaxLoadUserMenu()
        {
            return PartialView();
        }
    }
}