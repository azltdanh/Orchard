using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Controllers
{
    [Themed]
    public class HomeController : Controller, IUpdateModel
    {
        #region Init

        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IFastFilterService _fastfilterService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly IAdsPaymentHistoryService _adsPaymentService;
        private readonly IPropertyExchangeService _propertyExchangeService;

        public HomeController(
            IAddressService addressService,
            IPropertyService propertyService,
            IShapeFactory shapeFactory,
            IUserGroupService groupService,
            ICustomerService customerService,
            IFastFilterService fastfilterService,
            IOrchardServices services,
            IAdsPaymentHistoryService adsPaymentService,
            IPropertyExchangeService propertyExchangeService)
        {
            _addressService = addressService;
            _propertyService = propertyService;
            _fastfilterService = fastfilterService;
            _customerService = customerService;
            _groupService = groupService;
            _adsPaymentService = adsPaymentService;
            _propertyExchangeService = propertyExchangeService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

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

        [Authorize]
        public ActionResult Index(UserPropertyIndexOptions options, PagerParameters pagerParameters)
        {

            // ReSharper disable once Mvc.ActionNotResolved
            // ReSharper disable once Mvc.ControllerNotResolved
            // ReSharper disable once Mvc.AreaNotResolved
            return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
        }

        #region Create

        [Authorize]
        public ActionResult Create(string adsTypeCssClass)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            var property = Services.ContentManager.New<PropertyPart>("Property");
            dynamic model = Services.ContentManager.BuildEditor(property);

            var viewModel = _fastfilterService.BuildCreateBaseViewModel(adsTypeCssClass);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyCreate",
                Model: viewModel, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            return View((object)model);
        }

        [Authorize]
        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(PropertyFrontEndCreateBaseViewModel createModel)
        {
            #region VALIDATION

            if (string.IsNullOrEmpty(createModel.AdsTypeCssClass))
            {
                AddModelError("AdsTypeCssClass", T("Vui lòng chọn Loại giao dịch."));
            }
            else
            {
                if (createModel.AdsTypeCssClass == "ad-selling" || createModel.AdsTypeCssClass == "ad-leasing")
                {
                    if (string.IsNullOrEmpty(createModel.TypeGroupCssClass))
                    {
                        AddModelError("TypeGroupCssClass", T("Vui lòng chọn Nhóm BĐS."));
                    }
                    if (!createModel.TypeId.HasValue || createModel.TypeId <= 0)
                    {
                        AddModelError("TypeId", T("Vui lòng chọn Loại BĐS."));
                    }
                }
            }

            #endregion

            #region CREATE RECORD

            var p = Services.ContentManager.New<PropertyPart>("Property");

            if (ModelState.IsValid)
            {
                DateTime createdDate = DateTime.Now;
                UserPartRecord createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;

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

                p.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

                #endregion

                PropertyTypePartRecord type = _propertyService.GetType(createModel.TypeId);
                // Type
                p.Type = type;
                // TypeGroup
                p.TypeGroup = type.Group;
                // AdsType
                p.AdsType = _propertyService.GetAdsType(createModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : createModel.AdsTypeCssClass);

                Services.ContentManager.Create(p);

                #region Update OrderBy Domain (UserGroup)

                if (p.UserGroup != null)
                    _propertyService.UpdateOrderByDomainGroup(p, p.UserGroup.Id);

                #endregion

                // IdStr
                p.IdStr = p.Id.ToString(CultureInfo.InvariantCulture);

                #region Create Property Exchange

                if (createModel.AdsTypeCssClass == "ad-exchange")
                {
                    _propertyExchangeService.CreatePropertyExchange(p);
                }

                #endregion

                Services.ContentManager.UpdateEditor(p, this);

                // Redirect to Edit
                return RedirectToAction("Edit", "Home", new { area = "RealEstate.FrontEnd", id = p.Id });
            }

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                dynamic model = Services.ContentManager.UpdateEditor(p, this);
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyCreate",
                    Model: _fastfilterService.BuildCreateBaseViewModel(p), Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                return View((object)model);
            }

            #endregion

            return RedirectToAction("Index");
        }

        #endregion

        #region Edit

        [Authorize]
        public async System.Threading.Tasks.Task<ActionResult> Edit(int id, string returnUrl)
        {
            #region SECURITY

            if (_fastfilterService.IsEditable(id) == false)
            {
                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
            }

            #endregion

            #region GET RECORD

            var p = Services.ContentManager.Get<PropertyPart>(id);
            if (p.PriceProposed <= 0) p.PriceProposed = null;

            #region ESTIMATE

            if (p.AdsType.CssClass == "ad-selling" && p.TypeGroup.CssClass == "gp-house")
            {
                if (Request.QueryString["estimate"] == "True")
                {
                    if (_propertyService.IsValidForEstimate(p))
                    {
                        var entry = await _propertyService.EstimateProperty(p.Id);
                        p.PriceEstimatedInVND = entry.PriceEstimatedInVND;
                    }
                    else
                    {
                        p.PriceEstimatedInVND = null;
                    }
                }
            }

            #endregion

            PropertyFrontEndEditViewModel editModel = _fastfilterService.BuildEditViewModel(p);
            editModel.ReturnUrl = returnUrl;
            if (Request.QueryString["estimate"] == "True") editModel.SubmitEstimate = true;

            var propertyExchange = _propertyService.GetExchangePartRecordByPropertyId(id);
            editModel.IsPropertyExchange = propertyExchange != null;
            editModel.AdsTypeCssClass = propertyExchange != null ? "ad-exchange" : editModel.AdsTypeCssClass;

            #endregion

            #region RETURN VIEW

            if (p.PriceProposed < 0) p.PriceProposed = null;
            if (p.AreaUsable == 0) p.AreaUsable = null;

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyEdit", Model: editModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(p);
            model.Content.Add(editor);

            #endregion

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [Authorize]
        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, FormCollection frmCollection)
        {
            #region SECURITY

            if (_fastfilterService.IsEditable(id) == false)
            {
                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
            }

            #endregion

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region Get Old Data

            int oldSeqOrder = p.SeqOrder;
            string oldAdsTypeCssClass = p.AdsType.CssClass;
            bool oldIsAdsVip = p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now && !p.AdsVIPRequest;
            string oldStatusCssClass = p.Status.CssClass;
            bool oldIsRefresh = p.IsRefresh;
            int oldDistrictId = p.District != null ? p.District.Id : 0;
            DateTime? oldAdsVipExpirationDate = p.AdsVIPExpirationDate;
            var oldRequestVip = p.AdsVIPRequest;
            //bool isValidate = oldIsAdsVip;// (oldStatusCssClass == "st-pending" || oldStatusCssClass == "st-draft");//oldStatusCssClass != "st-approved" || && oldIsRefresh

            #endregion

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            dynamic model = Services.ContentManager.UpdateEditor(p, this);

            DateTime lastUpdatedDate = DateTime.Now;
            UserPart lastUpdatedUser = user;

            var editModel = new PropertyFrontEndEditViewModel { Property = p };

            if (TryUpdateModel(editModel))
            {
                //if (oldDistrictId != editModel.DistrictId) districtChange = true;

                const string format = "dd/MM/yyyy";
                CultureInfo provider = CultureInfo.InvariantCulture;
                const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                DateTime fromDate, toDate;

                DateTime.TryParseExact(editModel.DateVipFrom, format, provider, style, out fromDate);
                DateTime.TryParseExact(editModel.DateVipTo, format, provider, style, out toDate);

                if (!string.IsNullOrEmpty(frmCollection["submit.Update"]) ||
                    !string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                {
                    #region VALIDATION

                    #region AdsType, TypeGroup, Type

                    // AdsType
                    if (String.IsNullOrEmpty(editModel.AdsTypeCssClass))
                    {
                        AddModelError("AdsTypeCssClass", T("Vui lòng chọn Loại giao dịch."));
                    }

                    if (editModel.AdsTypeCssClass != "ad-exchange" && _propertyService.GetAdsType(editModel.AdsTypeCssClass) == null)
                    {
                        AddModelError("AdsTypeCssClassNull", T("Vui lòng chọn đúng Loại giao dịch."));
                    }
                    // TypeGroup
                    if (String.IsNullOrEmpty(editModel.TypeGroupCssClass))
                    {
                        AddModelError("TypeGroupCssClass", T("Vui lòng chọn Nhóm bất động sản."));
                    }

                    // Type
                    if (!editModel.TypeId.HasValue || editModel.TypeId <= 0)
                    {
                        AddModelError("TypeId", T("Vui lòng chọn Loại bất động sản."));
                    }

                    #endregion

                    #region Province, District, Ward, Street

                    // Province
                    if (!editModel.ProvinceId.HasValue)
                    {
                        AddModelError("ProvinceId", T("Vui lòng chọn Tỉnh / Thành phố."));
                    }

                    // District
                    if (!editModel.DistrictId.HasValue)
                    {
                        AddModelError("DistrictId", T("Vui lòng chọn Quận / HUyện."));
                    }

                    #endregion

                    #region AddressNumber

                    // AddressNumber
                    //if (editModel.WardId.HasValue && editModel.StreetId.HasValue && !string.IsNullOrEmpty(editModel.AddressNumber))
                    //{
                    //    var adsType = _propertyService.GetAdsType(editModel.AdsTypeCssClass);
                    //    if (!_propertyService.VerifyUserPropertyUpdateUnicity(id, editModel.ProvinceId, editModel.DistrictId, editModel.WardId, editModel.StreetId, editModel.ApartmentId, editModel.AddressNumber, editModel.AddressCorner, editModel.ApartmentNumber, adsType.CssClass))
                    //    {
                    //        var r = _propertyService.GetUserPropertyByAddress(id, editModel.ProvinceId, editModel.DistrictId, editModel.WardId, editModel.StreetId, editModel.ApartmentId, editModel.AddressNumber, editModel.AddressCorner, editModel.ApartmentNumber, adsType.CssClass);
                    //        AddModelError("AddressNumber", T("BĐS {0} đã có trong tài sản của bạn.",r.DisplayForAddress));
                    //    }
                    //}

                    #endregion

                    switch (p.TypeGroup.CssClass)
                    {
                        case "gp-house":

                            #region Validate for "gp-house"

                            #region AreaTotal, AreaLegal

                            // AreaTotal & AreaTotalWidth + AreaTotalLength
                            if (
                                !(editModel.AreaTotal > 0 ||
                                  (editModel.AreaTotalWidth > 0 && editModel.AreaTotalLength > 0)))
                            {
                                AddModelError("AreaTotal",
                                    T("Vui lòng nhập Diện tích khuôn viên HOẶC nhập Chiều ngang và Chiều dài khu đất."));
                                AddModelError("AreaTotalWidth", T(""));
                                AddModelError("AreaTotalLength", T(""));
                            }

                            double areaTotal = _propertyService.CalcArea(editModel.AreaTotal, editModel.AreaTotalWidth,
                                editModel.AreaTotalLength, editModel.AreaTotalBackWidth);
                            double areaLegal = _propertyService.CalcArea(editModel.AreaLegal, editModel.AreaLegalWidth,
                                editModel.AreaLegalLength, editModel.AreaLegalBackWidth);

                            // AreaTotal & AreaLegal
                            if (areaTotal > 0 && areaLegal > 0 && areaLegal > areaTotal)
                            {
                                AddModelError("AreaLegal",
                                    T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                            }

                            // AreaConstruction
                            if (editModel.AreaConstruction > 0)
                            {
                                if (areaTotal < editModel.AreaConstruction)
                                {
                                    AddModelError("AreaConstruction",
                                        T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên đất."));
                                }
                            }

                            #endregion

                            // Location
                            if (string.IsNullOrEmpty(editModel.LocationCssClass))
                            {
                                AddModelError("LocationCssClass", T("Vui lòng chọn Vị trí BĐS."));
                            }

                            #endregion

                            break;
                        case "gp-apartment":

                            #region Validate for "gp-apartment"

                            // AreaUsable
                            if (!editModel.AreaUsable.HasValue)
                            {
                                AddModelError("AreaUsable", T("Vui lòng nhập diện tích căn hộ."));
                            }

                            #endregion

                            break;
                        case "gp-land":

                            #region Validate for "gp-land"

                            #region AreaTotal, AreaResidential, AreaConstruction

                            // AreaTotal & AreaTotalWidth + AreaTotalLength
                            if (
                                !(editModel.AreaTotal > 0 ||
                                  (editModel.AreaTotalWidth > 0 && editModel.AreaTotalLength > 0)))
                            {
                                AddModelError("AreaTotal",
                                    T("Vui lòng nhập Diện tích khuôn viên HOẶC nhập Chiều ngang và Chiều dài khu đất."));
                                AddModelError("AreaTotalWidth", T(""));
                                AddModelError("AreaTotalLength", T(""));
                            }

                            double areaTotalLand = _propertyService.CalcArea(editModel.AreaTotal,
                                editModel.AreaTotalWidth, editModel.AreaTotalLength, editModel.AreaTotalBackWidth);

                            // AreaTotal & AreaResidential
                            if (editModel.AreaResidential > 0)
                            {
                                if (areaTotalLand < editModel.AreaResidential)
                                {
                                    AddModelError("AreaResidential",
                                        T("Diện tích đất thổ cư phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                                }
                            }

                            // AreaTotal & AreaConstruction
                            if (editModel.AreaConstruction > 0)
                            {
                                if (areaTotalLand < editModel.AreaConstruction)
                                {
                                    AddModelError("AreaConstruction",
                                        T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên đất."));
                                }
                            }

                            #endregion

                            // Location
                            if (string.IsNullOrEmpty(editModel.LocationCssClass))
                            {
                                AddModelError("LocationCssClass", T("Vui lòng chọn Vị trí BĐS."));
                            }

                            #endregion

                            break;
                    }

                    // ContactPhone
                    if (string.IsNullOrEmpty(editModel.ContactPhone))
                    {
                        AddModelError("ContactPhone", T("Vui lòng nhập Thông tin liên hệ."));
                    }
                    if (!string.IsNullOrEmpty(frmCollection["submit.Update"]))
                    {
                        //Nếu tin cũ ko phải là tin VIP và kiểm tra tin mới có thuộc khu vực thu phí hoặc tin VIP ko?
                        if (!oldIsAdsVip && _adsPaymentService.CheckIsValidVip(editModel.AdsTypeVIP, editModel.DistrictId.Value, editModel.AdsTypeCssClass))
                        {
                            //if (editModel.AdsTypeVIP != 1 && editModel.AdsTypeVIP != 2 && editModel.AdsTypeVIP != 3)//editModel.AdsTypeVIP <0 || editModel.AdsTypeVIP > 3
                            if (editModel.AdsTypeVIP < 0 || editModel.AdsTypeVIP > 3)
                            {
                                AddModelError("NotExistAdsTypeVIP", T("Giá trị tin VIP không đúng."));
                            }
                            else
                            {
                                //if (
                                //    !_adsPaymentService.CheckIsHaveMoney(editModel.AdsTypeVIP, p, oldSeqOrder,
                                //        oldAdsVipExpirationDate, (int) (toDate - fromDate).TotalDays))
                                if (!_adsPaymentService.CheckHaveMoney(oldRequestVip, oldSeqOrder, oldAdsVipExpirationDate, editModel.AdsTypeVIP, (int)(toDate - fromDate).TotalDays))
                                    AddModelError("NotEnoughMoney",
                                        T(
                                            "Tin đăng của bạn nằm trong khu vực có thu phí [Quận 1, Quận 3, Quận 5, Quận 10, Quận 11, Quận Phú Nhuận, Quận Tân Bình, Quận Bình Thạnh, Quận Tân Phú] và số tiền của bạn không đủ để thực hiện tin này, Vui lòng liên hệ BQT hoặc nạp tiền thêm để tiếp tục đăng tin."));
                            }

                            if (toDate <= fromDate)
                            {
                                AddModelError("NotValidDays",
                                    T("Ngày hết hạn tin VIP của bạn phải lớn hơn ngày bắt đầu."));
                            }
                            //Services.Notifier.Information(T("AdsTypeVIP: {0}", editModel.AdsTypeVIP));
                        }
                    }

                    #endregion
                }
                if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                {
                    #region VALIDATION

                    //Ward
                    if (!editModel.ChkOtherWardName && !editModel.WardId.HasValue)
                    {
                        AddModelError("WardId", T("Vui lòng chọn Phường / Xã."));
                    }
                    //OtherWardName
                    if (editModel.ChkOtherWardName && string.IsNullOrEmpty(editModel.OtherWardName))
                    {
                        AddModelError("OtherWardName", T("Vui lòng nhập Phường / Xã."));
                    }
                    //Street
                    if (!editModel.ChkOtherStreetName && !editModel.StreetId.HasValue)
                    {
                        AddModelError("StreetId", T("Vui lòng chọn tên đường."));
                    }
                    //OtherStreetName
                    if (editModel.ChkOtherStreetName && string.IsNullOrEmpty(editModel.OtherStreetName))
                    {
                        AddModelError("OtherStreetName", T("Vui lòng nhập tên đường."));
                    }
                    if (!string.IsNullOrEmpty(editModel.LocationCssClass) &&
                        editModel.LocationCssClass.Contains("h-alley"))
                    {
                        // DistanceToStreet
                        if (editModel.DistanceToStreet.HasValue == false)
                        {
                            AddModelError("DistanceToStreet", T("Vui lòng nhập khoảng cách từ BĐS ra MT đường"));
                        }
                        // AlleyTurns
                        if (!editModel.AlleyTurns.HasValue)
                        {
                            AddModelError("AlleyTurns", T("Vui lòng nhập số lần rẽ (quẹo) từ MT đường vào đến BĐS"));
                        }
                        else
                        {
                            if (editModel.AlleyTurns < 1) editModel.AlleyTurns = 1;
                            if (editModel.AlleyTurns > 6) editModel.AlleyTurns = 6;
                            if (editModel.AlleyTurns >= 1 && !editModel.AlleyWidth1.HasValue)
                                AddModelError("AlleyWidth1", T("Vui lòng nhập Lần rẽ thứ 1 (Hẻm đầu tiên)"));
                            if (editModel.AlleyTurns >= 2 && !editModel.AlleyWidth2.HasValue)
                                AddModelError("AlleyWidth2", T("Vui lòng nhập Lần rẽ thứ 2"));
                            if (editModel.AlleyTurns >= 3 && !editModel.AlleyWidth3.HasValue)
                                AddModelError("AlleyWidth3", T("Vui lòng nhập Lần rẽ thứ 3"));
                            if (editModel.AlleyTurns >= 4 && !editModel.AlleyWidth4.HasValue)
                                AddModelError("AlleyWidth4", T("Vui lòng nhập Lần rẽ thứ 4"));
                            if (editModel.AlleyTurns >= 5 && !editModel.AlleyWidth5.HasValue)
                                AddModelError("AlleyWidth5", T("Vui lòng nhập Lần rẽ thứ 5"));
                            if (editModel.AlleyTurns >= 6 && !editModel.AlleyWidth6.HasValue)
                                AddModelError("AlleyWidth6", T("Vui lòng nhập Lần rẽ thứ 6"));
                        }
                    }

                    #endregion
                }

                if (ModelState.IsValid)
                {
                    #region UPDATE MODEL

                    p.IsRefresh = false;

                    #region User

                    // User
                    p.LastUpdatedDate = lastUpdatedDate;
                    p.LastUpdatedUser = lastUpdatedUser.Record;
                    p.LastInfoFromUser = lastUpdatedUser.Record;

                    #endregion

                    #region Group

                    if (p.UserGroup == null)
                    {
                        p.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
                    }

                    #endregion

                    #region AdsType

                    // Ads Type
                    p.AdsType = _propertyService.GetAdsType(editModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : editModel.AdsTypeCssClass);

                    // AdsExpirationDate
                    /*if (editModel.AdsExpirationDateValue.HasValue)
                    {
                        switch (editModel.AdsExpirationDateValue)
                        {
                            case 1: p.AdsExpirationDate = DateTime.Now.AddDays(10);
                                break;
                            case 2: p.AdsExpirationDate = DateTime.Now.AddDays(20);
                                break;
                            case 3: p.AdsExpirationDate = DateTime.Now.AddDays(30);
                                break;
                            case 4: p.AdsExpirationDate = DateTime.Now.AddDays(60);
                                break;
                            case 5: p.AdsExpirationDate = DateTime.Now.AddDays(90);
                                break;
                        }
                    }*/

                    if (!oldIsAdsVip)
                        p.AdsExpirationDate = DateExtension.GetEndOfDate(toDate);

                    #endregion

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

                    #region Address

                    // Province
                    if (editModel.ProvinceId.HasValue) p.Province = _addressService.GetProvince(editModel.ProvinceId);
                    p.OtherProvinceName = editModel.ProvinceId.HasValue ? "" : editModel.OtherProvinceName;

                    // District
                    if (editModel.DistrictId.HasValue) p.District = _addressService.GetDistrict(editModel.DistrictId);
                    p.OtherDistrictName = editModel.DistrictId.HasValue ? "" : editModel.OtherDistrictName;

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

                    // LegalStatus
                    p.LegalStatus = _propertyService.GetLegalStatus(editModel.LegalStatusId);

                    // Direction
                    p.Direction = _propertyService.GetDirection(editModel.DirectionId);

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
                        }
                    }

                    #endregion

                    // Area for filter only
                    p.Area = _propertyService.CalcAreaForFilter(p);

                    // AreaUsable
                    p.AreaUsable = _propertyService.CalcAreaUsable(p);

                    #region Price

                    // PaymentMethod
                    p.PaymentMethod = _propertyService.GetPaymentMethod(editModel.PaymentMethodId);

                    // PaymentUnit
                    p.PaymentUnit = _propertyService.GetPaymentUnit(editModel.PaymentUnitId);

                    if (p.PriceProposed > 0)
                    {
                        // PriceProposedInVND
                        p.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(p);
                    }

                    #endregion

                    // Status
                    if (!string.IsNullOrEmpty(frmCollection["submit.Update"]) ||
                        !string.IsNullOrEmpty(frmCollection["submit.Captcha"]))
                    {
                        p.Status = _propertyService.GetStatus("st-pending");
                    }
                    else if (!string.IsNullOrEmpty(frmCollection["submit.Save"]))
                    {
                        p.Status = _propertyService.GetStatus("st-draft");
                    }

                    // Advantages
                    _propertyService.UpdatePropertyAdvantages(p, editModel.Advantages);

                    // DisAdvantages
                    _propertyService.UpdatePropertyDisAdvantages(p, editModel.DisAdvantages);

                    // ApartmentAdvantages
                    _propertyService.UpdatePropertyApartmentAdvantages(p, editModel.ApartmentAdvantages);

                    // ApartmentInteriorAdvantages
                    _propertyService.UpdatePropertyApartmentInteriorAdvantages(p, editModel.ApartmentInteriorAdvantages);

                    if (editModel.TypeGroupCssClass == "gp-apartment")
                    {
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

                    #endregion

                    #region Save Meta

                    _propertyService.UpdateMetaDescriptionKeywords(p, true);

                    #endregion

                    #region AdsVIPRequest

                    if (!string.IsNullOrEmpty(frmCollection["submit.Update"]))
                    {
                        if (!oldIsAdsVip && _adsPaymentService.CheckIsValidVip(editModel.AdsTypeVIP, editModel.DistrictId.Value, editModel.AdsTypeCssClass))
                        {
                            p.AdsVIPRequest = true;
                            p.AdsVIPExpirationDate = DateExtension.GetEndOfDate(toDate);
                            p.SeqOrder = editModel.AdsTypeVIP;
                            p.Published = true;

                            //Update Payment History
                            _adsPaymentService.UpdatePaymentHistoryV2(oldStatusCssClass, oldSeqOrder,
                            oldAdsVipExpirationDate, p, user, editModel.AdsTypeVIP, (int)(toDate - fromDate).TotalDays);
                            // send mail to Admin
                        }
                    }
                    #endregion

                    #region PostToFaceBook

                    //PostToFaceBook(p.Id,lastUpdatedUser.Id);
                    Session["PropertyId"] = editModel.AcceptPostToFacebok ? p.Id : 0;

                    #endregion

                    #region PropertyExchange

                    if (editModel.AdsTypeCssClass == "ad-exchange")
                    {
                        if (!editModel.IsPropertyExchange)
                        {
                            //Thêm mới bđs trao đổi
                            _propertyExchangeService.CreatePropertyExchange(p);
                        }
                        Services.Notifier.Information(T("Tin rao <a href='{0}'>{1}</a> cần trao đổi đã được lưu thành công. Giờ bạn cần nhập thông tin về bất động sản muốn được nhận!",
                            Url.Action("Edit", new { p.Id }), p.DisplayForAddress));
                        //Redirect tạo customer
                        return RedirectToAction("RequirementExchangeCreate", "PropertyExchange", new { pId = id });
                    }
                    else
                    {
                        if (editModel.IsPropertyExchange)
                        {
                            //Xóa BĐS trao đổi đi
                            _propertyExchangeService.DeletePropertyExchange(p, false);
                            Services.Notifier.Information(T("Tin rao <a href='{0}'>{1}</a> đã xóa khỏi BĐS trao đổi!", Url.Action("Edit", new { p.Id }), p.DisplayForAddress));
                        }
                    }

                    #endregion

                    #region REDIRECT

                    //if (!string.IsNullOrEmpty(frmCollection["submit.Update"]))
                    //{
                    //    Services.Notifier.Information(T("Tin rao <a href='{0}'>{1}</a> đã đăng thành công. Chúng tôi sẽ duyệt tin và đưa lên trang web trong thời gian 24h.", Url.Action("Edit", new { p.Id }), p.DisplayForAddress));
                    //}
                    //Services.Notifier.Information(T("Tin rao {0} đã được lưu thành công.",p.DisplayForPriceProposed));

                    // Save & Continue
                    if (!string.IsNullOrEmpty(frmCollection["submit.Save"]))
                    {
                        Services.Notifier.Information(T("Tin rao <a href='{0}'>{1}</a> đã được lưu thành công.",
                            Url.Action("Edit", new { p.Id }), p.DisplayForAddress));
                        return RedirectToAction("Edit", new { id, returnUrl = editModel.ReturnUrl });
                    }
                    Services.Notifier.Information(
                        T(
                            "Tin rao <a href='{0}'>{1}</a> đã đăng thành công. Chúng tôi sẽ duyệt tin và đưa lên trang web trong thời gian 24h.",
                            Url.Action("Edit", new { p.Id }), p.DisplayForAddress));

                    #endregion

                    #region ESTIMATE

                    // Estimate
                    if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                    {
                        if (ModelState.IsValid)
                        {
                            if (p.AdsType.CssClass == "ad-selling")
                            {
                                // Clear UnitPrice in Cache
                                _propertyService.ClearApplicationCache(id);
                                return RedirectToAction("Edit",
                                    new { id, estimate = true, returnUrl = editModel.ReturnUrl });
                            }
                        }
                    }

                    #endregion
                }
            }

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = _fastfilterService.BuildEditViewModel(p);
                if (editModel.PriceProposed <= 0) editModel.PriceProposed = null;
                if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"])) editModel.SubmitEstimate = true;

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PropertyEdit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            if (Url.IsLocalUrl(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }

            return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
        }

        #endregion

        #region CreateCustomerRequirements

        //Dang tin can thue , can mua
        [Authorize]
        public ActionResult CreateCustomerRequirements(string adsTypeCssClass)
        {
            var property = Services.ContentManager.New<CustomerPart>("Customer");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerCreate",
                Model: BuildCreateCustomerRequirements(adsTypeCssClass), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(property);
            model.Content.Add(editor);

            return View((object)model);
        }

        [Authorize]
        [HttpPost, ActionName("CreateCustomerRequirements")]
        public ActionResult CreateCustomerRequirements(CustomerFrontEndCreateViewModel options)
        {
            #region VALIDATION

            if (string.IsNullOrEmpty(options.AdsTypeCssClass))
            {
                AddModelError("AdsTypeCssClass", T("Bạn chưa chọn loại giao dịch."));
            }

            if (string.IsNullOrEmpty(options.TypeGroupCssClass))
            {
                AddModelError("TypeGroupCssClass", T("Bạn chưa chọn loại BĐS."));
            }

            if (!options.ProvinceId.HasValue)
            {
                AddModelError("ProvinceId", T("Bạn chưa chọn Tỉnh / Thành Phố."));
            }

            if (string.IsNullOrEmpty(options.Phone))
            {
                AddModelError("Phone", T("Vui lòng nhập Số điện thoại."));
            }

            if (!string.IsNullOrEmpty(options.Email))
            {
                if (!Regex.IsMatch(options.Email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
                {
                    // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                    ModelState.AddModelError("email", T("Bạn vui lòng cung cấp một địa chỉ e-mail hợp lệ."));
                }
            }

            if (string.IsNullOrEmpty(options.Name))
            {
                AddModelError("Name", T("Vui lòng nhập tên."));
            }

            #endregion

            #region Create New Customer

            DateTime createdDate = DateTime.Now;
            UserPartRecord createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            var c = Services.ContentManager.New<CustomerPart>("Customer");

            // Contact
            c.ContactName = options.Name;
            c.ContactPhone = options.Phone;
            c.ContactEmail = options.Email;

            // Status
            c.Status = _customerService.GetStatus("st-pending");
            c.StatusChangedDate = createdDate;

            // User
            c.CreatedDate = createdDate;
            c.CreatedUser = createdUser;
            c.LastUpdatedDate = createdDate;
            c.LastUpdatedUser = createdUser;
            c.Note = options.Note;
            c.Published = true;

            // UserGroup
            c.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            // AdsExpirationDate
            if (options.AdsExpirationDateValue.HasValue)
            {
                switch (options.AdsExpirationDateValue)
                {
                    case 1:
                        c.AdsExpirationDate = DateTime.Now.AddDays(10);
                        break;
                    case 2:
                        c.AdsExpirationDate = DateTime.Now.AddDays(20);
                        break;
                    case 3:
                        c.AdsExpirationDate = DateTime.Now.AddDays(30);
                        break;
                    case 4:
                        c.AdsExpirationDate = DateTime.Now.AddDays(60);
                        break;
                    case 5:
                        c.AdsExpirationDate = DateTime.Now.AddDays(90);
                        break;
                }
            }

            Services.ContentManager.Create(c);

            // Purposes
            _customerService.UpdatePurposesForContentItem(c, options.Purposes);

            #endregion

            var createModel = new CustomerEditRequirementViewModel();

            if (ModelState.IsValid)
            {
                #region Update Customer Requirements

                createModel.AdsTypeId = _propertyService.GetAdsType(options.AdsTypeCssClass).Id;
                createModel.PropertyTypeGroupId = _propertyService.GetTypeGroup(options.TypeGroupCssClass).Id;

                createModel.ProvinceId = options.ProvinceId;
                createModel.DistrictIds = options.DistrictIds;
                createModel.WardIds = options.WardIds;
                createModel.StreetIds = options.StreetIds;

                createModel.ApartmentIds = options.ApartmentIds;
                createModel.OtherProjectName = options.OtherProjectName;

                createModel.DirectionIds = options.DirectionIds;

                createModel.LocationId = options.LocationId;
                createModel.MinAlleyWidth = options.MinAlleyWidth;

                createModel.MinArea = options.MinArea;
                createModel.MinWidth = options.MinWidth;
                createModel.MinLength = options.MinLength;

                createModel.MinFloors = options.MinFloors;
                createModel.MinBedrooms = options.MinBedrooms;

                createModel.MinPrice = options.MinPrice;
                createModel.MaxPrice = options.MaxPrice;
                createModel.PaymentMethodId = _propertyService.GetPaymentMethod(options.PaymentMethodCssClass).Id;

                #region ApartmentFloorThRange

                switch (options.ApartmentFloorThRange)
                {
                    case PropertyDisplayApartmentFloorTh.All:
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                        createModel.MinApartmentFloorTh = 1;
                        createModel.MaxApartmentFloorTh = 3;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                        createModel.MinApartmentFloorTh = 4;
                        createModel.MaxApartmentFloorTh = 7;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                        createModel.MinApartmentFloorTh = 8;
                        createModel.MaxApartmentFloorTh = 12;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                        createModel.MinApartmentFloorTh = 12;
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
                        createModel.LocationId = locationFront;
                        break;
                    case PropertyDisplayLocation.Alley6: // hem 6m tro len
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 6;
                        break;
                    case PropertyDisplayLocation.Alley5:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 5;
                        break;
                    case PropertyDisplayLocation.Alley4:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 4;
                        break;
                    case PropertyDisplayLocation.Alley3:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 3;
                        break;
                    case PropertyDisplayLocation.Alley2:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 2;
                        break;
                    case PropertyDisplayLocation.Alley:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 1;
                        break;
                }

                #endregion

                //Add Requirement
                int id = _fastfilterService.UpdateRequirements(c.Record, createModel);

                Services.Notifier.Information(T("Đăng yêu cầu mã số <a href='{0}'>{1}</a> thành công",
                    Url.Action("EditCustomerRequirements", new { groupId = id }), id));

                #endregion
            }
            else
            {
                #region ERROR HANDLE

                dynamic model = Services.ContentManager.UpdateEditor(c, this);
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(
                    TemplateName: "Parts/CustomerCreate",
                    Model: BuildCreateCustomerRequirements(options.AdsTypeCssClass), Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);

                #endregion
            }

            return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
        }

        public CustomerFrontEndCreateViewModel BuildCreateCustomerRequirements(string adsTypeCssClass)
        {
            var currentUserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            int provinceId = (currentUserGroup != null && currentUserGroup.DefaultProvince != null)
                ? currentUserGroup.DefaultProvince.Id
                : _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            return new CustomerFrontEndCreateViewModel
            {
                AdsTypeCssClass = adsTypeCssClass,
                AdsTypes = _propertyService.GetAdsTypes(),
                TypeGroups = _propertyService.GetTypeGroups(),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvinces(),
                Districts = _addressService.GetDistricts(provinceId),
                Wards = _addressService.GetWards(0),
                Streets = _addressService.GetStreets(0),
                Directions = _propertyService.GetDirections(),
                PaymentMethods = _propertyService.GetPaymentMethods(),
                Purposes = _customerService.GetPurposesEntries().ToList(),
                Apartments = _addressService.GetApartments(0)
            };
        }

        #endregion

        #region EditCustomerRequirements

        [Authorize]
        public ActionResult EditCustomerRequirements(int groupId)
        {
            CustomerFrontEndEditViewModel viewmodel = BuildRequirementEditViewModel(groupId);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/CustomerEdit", Model: viewmodel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(viewmodel.Customer);
            model.Content.Add(editor);
            return View((object)model);
        }

        [Authorize]
        [HttpPost, ActionName("EditCustomerRequirements")]
        public ActionResult EditCustomerRequirementsPost(int groupId)
        {
            IEnumerable<CustomerRequirementRecord> requirements = _customerService.GetRequirements(groupId);
            CustomerRequirementRecord req = requirements.First();
            var customer = Services.ContentManager.Get<CustomerPart>(req.CustomerPartRecord.Id);

            dynamic model = Services.ContentManager.UpdateEditor(customer, this);

            DateTime lastUpdatedDate = DateTime.Now;
            UserPartRecord lastUpdatedUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            var editModel = new CustomerFrontEndEditViewModel { Customer = customer };

            if (TryUpdateModel(editModel))
            {
                #region VALIDATION

                if (String.IsNullOrEmpty(editModel.AdsTypeCssClass))
                {
                    AddModelError("AdsTypeCssClass", T("Bạn chưa chọn loại giao dịch."));
                }

                if (!editModel.ProvinceId.HasValue)
                {
                    AddModelError("ProvinceId", T("Bạn chưa chọn Tỉnh / Thành Phố."));
                }

                if (string.IsNullOrEmpty(editModel.TypeGroupCssClass))
                {
                    AddModelError("TypeGroupCssClass", T("Bạn chưa chọn loại BĐS."));
                }
                if (string.IsNullOrEmpty(editModel.Phone))
                {
                    AddModelError("Phone", T("Vui lòng nhập Số điện thoại."));
                }

                if (!string.IsNullOrEmpty(editModel.Email))
                {
                    if (!Regex.IsMatch(editModel.Email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
                    {
                        // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                        ModelState.AddModelError("Email", T("Bạn vui lòng cung cấp một địa chỉ e-mail hợp lệ."));
                    }
                }

                if (string.IsNullOrEmpty(editModel.Name))
                {
                    AddModelError("Name", T("Vui lòng nhập tên."));
                }

                #endregion

                #region UPDATE MODEL

                // UserGroup
                if (customer.UserGroup == null)
                {
                    customer.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
                }

                // User
                customer.LastUpdatedDate = lastUpdatedDate;
                customer.LastUpdatedUser = lastUpdatedUser;

                customer.Status = _customerService.GetStatus("st-pending");
                customer.Note = editModel.Note;

                customer.ContactName = editModel.Name;
                customer.ContactPhone = editModel.Phone;
                customer.ContactEmail = editModel.Email;

                // AdsExpirationDate
                if (editModel.AdsExpirationDateValue.HasValue)
                {
                    switch (editModel.AdsExpirationDateValue)
                    {
                        case 1:
                            customer.AdsExpirationDate = DateTime.Now.AddDays(10);
                            break;
                        case 2:
                            customer.AdsExpirationDate = DateTime.Now.AddDays(20);
                            break;
                        case 3:
                            customer.AdsExpirationDate = DateTime.Now.AddDays(30);
                            break;
                        case 4:
                            customer.AdsExpirationDate = DateTime.Now.AddDays(60);
                            break;
                        case 5:
                            customer.AdsExpirationDate = DateTime.Now.AddDays(90);
                            break;
                    }
                }

                // Purposes
                _customerService.UpdatePurposesForContentItem(customer, editModel.Purposes);

                var replaceModel = new CustomerEditRequirementViewModel
                {
                    GroupId = groupId,
                    AdsTypeId = _propertyService.GetAdsType(editModel.AdsTypeCssClass).Id,
                    PropertyTypeGroupId = _propertyService.GetTypeGroup(editModel.TypeGroupCssClass).Id,
                    ProvinceId = editModel.ProvinceId ?? _addressService.GetProvince("TP. Hồ Chí Minh").Id,
                    DistrictIds = editModel.DistrictIds,
                    WardIds = editModel.WardIds,
                    StreetIds = editModel.StreetIds,
                    DirectionIds = editModel.DirectionIds,
                    ApartmentIds = editModel.ApartmentIds,
                    OtherProjectName = editModel.OtherProjectName,
                    LocationId = editModel.LocationId,
                    MinFloors = editModel.MinFloors,
                    MinBedrooms = editModel.MinBedrooms,
                    MinArea = editModel.MinArea,
                    MinWidth = editModel.MinWidth,
                    MinLength = editModel.MinLength,
                    MinAlleyWidth = editModel.MinAlleyWidth
                };

                #region Update Customer Requirements

                replaceModel.MinFloors = editModel.MinFloors;
                replaceModel.MinPrice = editModel.MinPrice;
                replaceModel.MaxPrice = editModel.MaxPrice;
                replaceModel.PaymentMethodId = _propertyService.GetPaymentMethod(editModel.PaymentMethodCssClass).Id;

                #region ApartmentFloorThRange

                switch (editModel.ApartmentFloorThRange)
                {
                    case PropertyDisplayApartmentFloorTh.All:
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                        replaceModel.MinApartmentFloorTh = 1;
                        replaceModel.MaxApartmentFloorTh = 3;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                        replaceModel.MinApartmentFloorTh = 4;
                        replaceModel.MaxApartmentFloorTh = 7;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                        replaceModel.MinApartmentFloorTh = 8;
                        replaceModel.MaxApartmentFloorTh = 12;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                        replaceModel.MinApartmentFloorTh = 12;
                        break;
                }

                #endregion

                #region AlleyTurnsRange

                int locationFront = _propertyService.GetLocation("h-front").Id;
                int locationAlley = _propertyService.GetLocation("h-alley").Id;

                switch (editModel.AlleyTurnsRange)
                {
                    case PropertyDisplayLocation.All: // Tat ca cac vi tri
                        break;
                    case PropertyDisplayLocation.AllWalk: // Mat Tien
                        replaceModel.LocationId = locationFront;
                        break;
                    case PropertyDisplayLocation.Alley6: // hem 6m tro len
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 6;
                        break;
                    case PropertyDisplayLocation.Alley5:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 5;
                        break;
                    case PropertyDisplayLocation.Alley4:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 4;
                        break;
                    case PropertyDisplayLocation.Alley3:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 3;
                        break;
                    case PropertyDisplayLocation.Alley2:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 2;
                        break;
                    case PropertyDisplayLocation.Alley:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 1;
                        break;
                }

                #endregion

                //Add Requirement
                int id = _fastfilterService.UpdateRequirements(customer.Record, replaceModel);

                Services.Notifier.Information(T("Cập nhật yêu cầu mã số <a href='{0}'>{1}</a> thành công",
                    Url.Action("EditCustomerRequirements", new { groupId = id }), id));

                #endregion

                #endregion
            }

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = BuildRequirementEditViewModel(groupId);

                dynamic editor = Shape.EditorTemplate(
                    TemplateName: "Parts/CustomerEdit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
        }

        public CustomerFrontEndEditViewModel BuildRequirementEditViewModel(int groupid)
        {
            var currentUserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            IEnumerable<CustomerRequirementRecord> requirements = _customerService.GetRequirements(groupid).ToList();
            CustomerRequirementRecord req = requirements.First();
            PropertyTypeGroupPartRecord typeGroup = req.PropertyTypeGroupPartRecord;

            int provinceId = req.LocationProvincePartRecord != null
                ? req.LocationProvincePartRecord.Id
                : (currentUserGroup != null && currentUserGroup.DefaultProvince != null)
                    ? currentUserGroup.DefaultProvince.Id
                    : _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            int[] districtIds =
                requirements.Where(a => a.LocationDistrictPartRecord != null)
                    .Select(a => a.LocationDistrictPartRecord.Id)
                    .ToArray();

            var customer = Services.ContentManager.Get<CustomerPart>(req.CustomerPartRecord.Id);
            IEnumerable<int> purposeIds = _customerService.GetCustomerPurposes(customer).Select(a => a.Id);
            //var _purposeIds = c.Purposes.Select(r => r.Id).ToList();

            #region CustomerFrontEndEditViewModel

            var viewmodel = new CustomerFrontEndEditViewModel
            {
                //GroupId = req.GroupId,

                AdsTypeCssClass = req.AdsTypePartRecord != null ? req.AdsTypePartRecord.CssClass : "",
                AdsTypes =
                    _propertyService.GetAdsTypes().Where(p => p.CssClass == "ad-buying" || p.CssClass == "ad-renting"),
                TypeGroupCssClass = typeGroup != null ? typeGroup.CssClass : "",
                TypeGroups = _propertyService.GetTypeGroups(),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvinces(),
                DistrictIds = districtIds,
                Districts = _addressService.GetDistricts(provinceId),
                WardIds =
                    requirements.Where(a => a.LocationWardPartRecord != null)
                        .Select(a => a.LocationWardPartRecord.Id)
                        .ToArray(),
                Wards = _addressService.GetWards(districtIds),
                StreetIds =
                    requirements.Where(a => a.LocationStreetPartRecord != null)
                        .Select(a => a.LocationStreetPartRecord.Id)
                        .ToArray(),
                Streets = _addressService.GetStreets(districtIds),
                ApartmentIds =
                    requirements.Where(a => a.LocationApartmentPartRecord != null)
                        .Select(a => a.LocationApartmentPartRecord.Id)
                        .ToArray(),
                Apartments = _addressService.GetApartments(districtIds),
                ChkOtherProjectName = requirements.Any(r => r.OtherProjectName != null),
                DirectionIds =
                    requirements.Where(a => a.DirectionPartRecord != null)
                        .Select(a => a.DirectionPartRecord.Id)
                        .ToArray(),
                Directions = _propertyService.GetDirections(),
                OtherProjectName = req.OtherProjectName,
                //ApartmentFloorThRange

                LocationId = req.PropertyLocationPartRecord != null ? req.PropertyLocationPartRecord.Id : 0,
                //AlleyTurnsRange

                MinAlleyWidth = req.MinAlleyWidth,
                MinArea = req.MinArea,
                MinWidth = req.MinWidth,
                MinLength = req.MinLength,
                MinFloors = req.MinFloors,
                MinBedrooms = req.MinBedrooms,
                MinBathrooms = req.MinBathrooms,
                MinPrice = req.MinPrice,
                MaxPrice = req.MaxPrice,
                PaymentMethodCssClass = req.PaymentMethodPartRecord.CssClass,
                PaymentMethods = _propertyService.GetPaymentMethods(),
                Note = customer.Note,
                AdsExpirationDate = customer.AdsExpirationDate,
                Name = customer.ContactName,
                Phone = customer.ContactPhone,
                Email = customer.ContactEmail,
                Purposes =
                    _customerService.GetPurposes()
                        .Select(r => new CustomerPurposeEntry { Purpose = r, IsChecked = purposeIds.Contains(r.Id) })
                        .ToList(),
                Customer = customer
            };

            #endregion

            #region AlleyTurnsRange

            if (req.MinAlleyWidth.HasValue)
            {
                switch (Int32.Parse(req.MinAlleyWidth.ToString()))
                {
                    case 1:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley;
                        break;
                    case 2:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley2;
                        break;
                    case 3:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley3;
                        break;
                    case 4:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley4;
                        break;
                    case 5:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley5;
                        break;
                    case 6:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley6;
                        break;
                }
            }
            else
            {
                if (req.PropertyLocationPartRecord != null && req.PropertyLocationPartRecord.CssClass == "h-front")
                {
                    viewmodel.AlleyTurnsRange = PropertyDisplayLocation.AllWalk;
                }
                else
                {
                    viewmodel.AlleyTurnsRange = PropertyDisplayLocation.All;
                }
            }

            #endregion

            #region ApartmentFloorThRange

            if (req.MinApartmentFloorTh.HasValue)
            {
                switch (req.MinApartmentFloorTh)
                {
                    case 1:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3;
                        break;
                    case 4:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7;
                        break;
                    case 8:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12;
                        break;
                    case 12:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh12;
                        break;
                    default:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.All;
                        break;
                }
            }

            #endregion

            return viewmodel;
        }

        #endregion

    }
}