using System;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using System.Web.Routing;
using System.Reflection;
using System.Collections.Generic;
using Orchard;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Settings.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.UI.Navigation;
using Orchard.Mvc.AntiForgery;
using Orchard.Mvc.Extensions;

using Orchard.Users.Models;

using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.Net;
using System.Collections;

namespace RealEstate.Controllers
{
    [ValidateInput(false), Admin]
    public class PropertyTypeLandAdminController : Controller, IUpdateModel
    {

        #region Init

        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertySettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IPropertyService _propertyService;
        private readonly IRevisionService _revisionService;
        private readonly ISiteService _siteService;

        public PropertyTypeLandAdminController(
            IAddressService addressService,
            IUserGroupService groupService,
            IPropertySettingService settingService,
            ICustomerService customerService,
            IPropertyService propertyService,
            IRevisionService revisionService,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IOrchardServices services)
        {
            _addressService = addressService;
            _groupService = groupService;
            _settingService = settingService;
            _customerService = customerService;
            _propertyService = propertyService;
            _revisionService = revisionService;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        TimeSpan scopeTimeout = new TimeSpan(1, 30, 0);

        #endregion

        public ActionResult Index(PropertyIndexOptions options, PagerParameters pagerParameters)
        {
            return View();
        }

        #region Create / Edit House

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            var property = Services.ContentManager.New<PropertyPart>("Property");
            dynamic model = Services.ContentManager.BuildEditor(property);

            var viewModel = _propertyService.BuildLandCreateViewModel();
            if (Request["AdsTypeId"] != null) viewModel.AdsTypeId = int.Parse(Request["AdsTypeId"]);
            if (Request["PaymentMethodId"] != null) viewModel.PaymentMethodId = int.Parse(Request["PaymentMethodId"]);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Property.CreateLand", Model: viewModel, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(LandCreateViewModel createModel, FormCollection frmCollection)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            #region VALIDATION

            #region Province, District, Ward, Street

            // Ward
            if (!createModel.ChkOtherWardName && !createModel.WardId.HasValue)
            {
                AddModelError("WardId", T("Vui lòng chọn Phường / Xã."));
            }
            // OtherWardName
            if (createModel.ChkOtherWardName && string.IsNullOrEmpty(createModel.OtherWardName))
            {
                AddModelError("OtherWardName", T("Vui lòng nhập Phường / Xã."));
            }

            #endregion

            #region AddressNumber

            #endregion

            #region AreaTotal, AreaResidential
            // AreaTotal & AreaTotalWidth + AreaTotalLength
            if (!(createModel.AreaTotal.HasValue || (createModel.AreaTotalWidth.HasValue && createModel.AreaTotalLength.HasValue)))
            {
                AddModelError("AreaTotal", T("Vui lòng nhập diện tích khuôn viên HOẶC nhập chiều ngang và chiều dài khu đất."));
                AddModelError("AreaTotalWidth", T(""));
                AddModelError("AreaTotalLength", T(""));
            }
            // AreaTotal & AreaResidential
            if (createModel.AreaTotal.HasValue && createModel.AreaResidential.HasValue && createModel.AreaTotal < createModel.AreaResidential)
            {
                AddModelError("AreaResidential", T("Diện tích hợp quy hoạch phải nhỏ hơn diện tích khuôn viên."));
            }

            #endregion

            #region Status RAO BÁN

            var _status = _propertyService.GetStatus(createModel.StatusId);

            if (_status.CssClass == "st-new" || _status.CssClass == "st-selling" || _status.CssClass == "st-negotiate" || _status.CssClass == "st-trading" || _status.CssClass == "st-sold")
            {
                if (createModel.PriceProposed <= 0)
                {
                    // chưa có thông tin Hẻm
                    AddModelError("PriceProposed", T("Vui lòng nhập giá rao bán."));
                }
            }

            #endregion

            #endregion

            #region CREATE RECORD

            var p = Services.ContentManager.New<PropertyPart>("Property");
            if (ModelState.IsValid)
            {
                #region RECORD

                var createdDate = DateTime.Now;
                var createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;
                var infoFromUser = _groupService.GetUser(createModel.LastInfoFromUserId);

                #region Type

                // Type
                p.Record.Type = _propertyService.GetType(createModel.TypeId);
                p.Record.TypeGroup = p.Type.Group;

                #endregion

                #region Address

                // Province
                p.Record.Province = _addressService.GetProvince(createModel.ProvinceId);
                //p.Record.OtherProvinceName = createModel.ProvinceId.HasValue ? "" : createModel.OtherProvinceName;

                // District
                p.Record.District = _addressService.GetDistrict(createModel.DistrictId);
                //p.Record.OtherDistrictName = createModel.DistrictId.HasValue ? "" : createModel.OtherDistrictName;

                // Ward
                p.Record.Ward = _addressService.GetWard(createModel.WardId);
                p.Record.OtherWardName = createModel.WardId.HasValue ? "" : createModel.OtherWardName;

                // Street
                p.Record.Street = _addressService.GetStreet(createModel.StreetId);
                p.Record.OtherStreetName = createModel.StreetId.HasValue ? "" : createModel.OtherStreetName;

                // Address
                p.Record.AddressNumber = createModel.AddressNumber;
                p.Record.AlleyNumber = _propertyService.IntParseAddressNumber(createModel.AddressNumber);

                // Street Segment
                var segmentStreet = _addressService.GetStreet(p.Record.Street, p.Record.AlleyNumber);
                if (segmentStreet != null)
                    p.Record.Street = segmentStreet;

                #endregion

                #region Legal, Direction
                // LegalStatus
                p.Record.LegalStatus = _propertyService.GetLegalStatus(createModel.LegalStatusId);

                // Direction
                p.Record.Direction = _propertyService.GetDirection(createModel.DirectionId);
                #endregion

                #region Location
                // Location
                p.Record.Location = _propertyService.GetLocation(createModel.LocationCssClass);
                if(p.Location != null)
                {
                    if (p.Location.CssClass == "h-front")
                    {
                        p.Record.StreetWidth = createModel.StreetWidth;
                        p.Record.DistanceToStreet = null;
                        p.Record.AlleyTurns = null;
                        p.Record.AlleyWidth = null;
                        p.Record.AlleyWidth1 = null;
                        p.Record.AlleyWidth2 = null;
                        p.Record.AlleyWidth3 = null;
                        p.Record.AlleyWidth4 = null;
                        p.Record.AlleyWidth5 = null;
                        p.Record.AlleyWidth6 = null;
                        p.Record.AlleyWidth7 = null;
                        p.Record.AlleyWidth8 = null;
                        p.Record.AlleyWidth9 = null;
                    }
                    else
                    {
                        p.Record.StreetWidth = null;
                        // Alley
                        p.Record.DistanceToStreet = createModel.DistanceToStreet;
                        p.Record.AlleyTurns = createModel.AlleyTurns;
                        p.Record.AlleyWidth1 = createModel.AlleyWidth1;
                        p.Record.AlleyWidth2 = createModel.AlleyWidth2;
                        p.Record.AlleyWidth3 = createModel.AlleyWidth3;
                        p.Record.AlleyWidth4 = createModel.AlleyWidth4;
                        p.Record.AlleyWidth5 = createModel.AlleyWidth5;
                        p.Record.AlleyWidth6 = createModel.AlleyWidth6;
                        p.Record.AlleyWidth7 = createModel.AlleyWidth7;
                        p.Record.AlleyWidth8 = createModel.AlleyWidth8;
                        p.Record.AlleyWidth9 = createModel.AlleyWidth9;
                        if (createModel.AlleyTurns > 0)
                        {
                            p.Record.AlleyWidth = new List<double?> { createModel.AlleyWidth1, createModel.AlleyWidth2, createModel.AlleyWidth3, createModel.AlleyWidth4, createModel.AlleyWidth5, createModel.AlleyWidth6, createModel.AlleyWidth7, createModel.AlleyWidth8, createModel.AlleyWidth9 }[(int)createModel.AlleyTurns - 1];
                        }
                    }
                }
                #endregion

                #region Area
                // AreaTotal
                p.Record.AreaTotal = createModel.AreaTotal;
                p.Record.AreaTotalWidth = createModel.AreaTotalWidth;
                p.Record.AreaTotalLength = createModel.AreaTotalLength;

                // AreaResidential
                p.Record.AreaResidential = createModel.AreaResidential;
                #endregion

                #region Construction
                // Construction
                p.Record.AreaConstruction = createModel.AreaConstruction;
                p.Record.AreaConstructionFloor = createModel.AreaConstructionFloor;
                #endregion

                #region Contact
                // Contact
                p.Record.ContactName = createModel.ContactName;
                p.Record.ContactPhone = createModel.ContactPhone;
                p.Record.ContactPhoneToDisplay = createModel.ContactPhoneToDisplay;
                p.Record.ContactAddress = createModel.ContactAddress;
                p.Record.ContactEmail = createModel.ContactEmail;
                #endregion

                #region Price

                // Price
                p.Record.PriceProposed = createModel.PriceProposed;
                p.Record.PaymentMethod = _propertyService.GetPaymentMethod(createModel.PaymentMethodId);
                p.Record.PaymentUnit = _propertyService.GetPaymentUnit(createModel.PaymentUnitId);

                #endregion

                #region Published, Status, Flag

                // Status
                p.Record.Status = _propertyService.GetStatus(createModel.StatusId);

                // Flag
                p.Record.Flag = _propertyService.GetFlag(createModel.FlagId);

                #endregion

                #region User
                // User
                p.Record.CreatedDate = createdDate;
                p.Record.CreatedUser = createdUser;
                p.Record.LastUpdatedDate = createdDate;
                p.Record.LastUpdatedUser = createdUser;
                p.Record.FirstInfoFromUser = infoFromUser;
                p.Record.LastInfoFromUser = infoFromUser;
                #endregion

                #region Ads
                // Ads Type
                p.Record.AdsType = _propertyService.GetAdsType(createModel.AdsTypeId);

                // Published
                p.Record.Published = createModel.Published;
                p.Record.AdsExpirationDate = createModel.AdsExpirationDate;
                if (p.Published) p.Record.AdsExpirationDate = _propertyService.GetAddExpirationDate(createModel.AddAdsExpirationDate, p.Record.AdsExpirationDate);
                else p.Record.AdsExpirationDate = null;

                // AdsVIP
                p.Record.AdsVIP = createModel.AdsVIP;
                p.Record.AdsVIPExpirationDate = createModel.AdsVIPExpirationDate;
                if (p.AdsVIP) p.Record.AdsVIPExpirationDate = _propertyService.GetAddExpirationDate(createModel.AddAdsVIPExpirationDate, p.Record.AdsVIPExpirationDate);
                else p.Record.AdsVIPExpirationDate = null;

                // AdsGoodDeal
                p.Record.AdsGoodDeal = createModel.AdsGoodDeal;
                // Tự động correct BĐS Giá rẻ hiện trên trang chủ
                if (p.AdsType.CssClass == "ad-selling")
                    if (p.Flag.CssClass != "deal-good" && p.Flag.CssClass != "deal-very-good") p.Record.AdsGoodDeal = false;
                p.Record.AdsGoodDealExpirationDate = createModel.AdsGoodDealExpirationDate;
                if (p.AdsGoodDeal) p.Record.AdsGoodDealExpirationDate = _propertyService.GetAddExpirationDate(createModel.AddAdsGoodDealExpirationDate, p.Record.AdsGoodDealExpirationDate);
                else p.Record.AdsGoodDealExpirationDate = null;

                // Thời gian đăng tin AdsExpirationDate = MAX(AdsExpirationDate, AdsVIPExpirationDate, AdsGoodDealExpirationDate)
                p.Record.AdsExpirationDate = (p.AdsVIPExpirationDate != null && (p.AdsVIPExpirationDate > p.AdsExpirationDate || p.AdsExpirationDate == null)) ? p.AdsVIPExpirationDate : p.AdsExpirationDate;
                p.Record.AdsExpirationDate = (p.AdsGoodDealExpirationDate != null && (p.AdsGoodDealExpirationDate > p.AdsExpirationDate || p.AdsExpirationDate == null)) ? p.AdsGoodDealExpirationDate : p.AdsExpirationDate;

                // Ads Content
                p.Record.Title = createModel.Title;
                p.Record.Description = createModel.Description;
                p.Record.Note = createModel.Note;
                #endregion

                #endregion

                Services.ContentManager.Create(p);

                // IdStr
                p.Record.IdStr = p.Id.ToString();

                // UserGroup
                p.Record.UserGroup = _groupService.GetBelongGroup(p.CreatedUser);

                // Tự động chuyển sang trạng thái RAO BÁN nếu đủ thông tin Địa chỉ, Hẻm, Diện tích, Giá
                if (p.Status.CssClass == "st-new")
                {
                    if (_propertyService.IsValid(p))
                        p.Status = _propertyService.GetStatus("st-selling");
                }

                if (p.Status.CssClass == "st-selling" && p.Published && (p.AdsExpirationDate == null || p.AdsExpirationDate < DateTime.Now)) p.Record.AdsExpirationDate = DateTime.Now.AddDays(90);

                // AreaUsable
                p.Record.AreaUsable = _propertyService.CalcAreaUsable(p);

                // PriceProposedInVND
                p.Record.PriceProposedInVND = _propertyService.CaclPriceProposedInVND(p);

                // Log User Activity
                _revisionService.SaveUserActivityAddNewProperty(createdDate, createdUser, p.Record);

                // Save & Continue
                if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                {
                    return RedirectToAction("Edit", new { id = p.Id });
                }

                // Redirect
                Services.Notifier.Information(T("Property <a href='{0}'>{1}</a> created.", Url.Action("Edit", new { p.Id }), p.DisplayForAddress));
                return RedirectToAction("Create");
            }

            dynamic model = Services.ContentManager.UpdateEditor(p, this);

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Property.CreateLand", Model: _propertyService.BuildLandCreateViewModel(createModel), Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            Services.Notifier.Information(T("Property {0} created", p.Id));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            #region GET RECORD

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region SECURITY

            if (!_propertyService.EnableEditProperty(p.Record, user))
                return new HttpUnauthorizedResult("Not authorized to edit properties");

            #endregion

            var editModel = _propertyService.BuildLandEditViewModel(p);
            editModel.ReturnUrl = returnUrl;
            editModel.LastInfoFromUserId = Services.WorkContext.CurrentUser.Id; // Current User
            
            #endregion

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Property.EditLand", Model: editModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(p);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id, HttpPostedFileBase file, FormCollection frmCollection)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region SECURITY

            if (!_propertyService.EnableEditProperty(p.Record, user))
                return new HttpUnauthorizedResult("Not authorized to edit properties");

            #endregion

            var oldModel = _propertyService.BuildEditViewModel(p);
            var oldLastUpdatedDate = oldModel.LastUpdatedDate;
            var oldLastUpdatedUser = oldModel.LastUpdatedUser;
            var oldAddress = oldModel.DisplayForAddress;
            var oldPriceProposed = p.PriceProposed;
            var oldContactName = p.ContactName;
            var oldContactPhone = p.ContactPhone;
            var oldNote = p.Note;

            dynamic model = Services.ContentManager.UpdateEditor(p, this);

            var lastUpdatedDate = DateTime.Now;
            var lastUpdatedUser = user;

            var editModel = new LandEditViewModel { Property = p };

            if (TryUpdateModel(editModel))
            {
                if (editModel.IsChanged)
                {

                    #region VALIDATION

                    if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                    {
                        #region Province, District, Ward, Street

                        // Province
                        if (!editModel.ChkOtherProvinceName && !editModel.ProvinceId.HasValue)
                        {
                            AddModelError("ProvinceId", T("Vui lòng chọn Tỉnh / Thành phố."));
                        }
                        // OtherProvinceName
                        //if (editModel.ChkOtherProvinceName && string.IsNullOrEmpty(editModel.OtherProvinceName))
                        //{
                        //    AddModelError("OtherProvinceName", T("Vui lòng nhập Tỉnh / Thành phố."));
                        //}
                        // District
                        if (!editModel.ChkOtherDistrictName && !editModel.DistrictId.HasValue)
                        {
                            AddModelError("DistrictId", T("Vui lòng chọn Quận / HUyện."));
                        }
                        // OtherDistrictName
                        //if (editModel.ChkOtherDistrictName && string.IsNullOrEmpty(editModel.OtherDistrictName))
                        //{
                        //    AddModelError("OtherDistrictName", T("Vui lòng nhập Quận / HUyện."));
                        //}
                        // Ward
                        if (!editModel.ChkOtherWardName && !editModel.WardId.HasValue)
                        {
                            AddModelError("WardId", T("Vui lòng chọn Phường / Xã."));
                        }
                        // OtherWardName
                        if (editModel.ChkOtherWardName && string.IsNullOrEmpty(editModel.OtherWardName))
                        {
                            AddModelError("OtherWardName", T("Vui lòng nhập Phường / Xã."));
                        }

                        #endregion

                        #region AddressNumber

                        #endregion
                    }

                    #region AreaTotal, AreaResidential
                    // AreaTotal & AreaTotalWidth + AreaTotalLength
                    if (!(editModel.AreaTotal.HasValue || (editModel.AreaTotalWidth.HasValue && editModel.AreaTotalLength.HasValue)))
                    {
                        AddModelError("AreaTotal", T("Vui lòng nhập diện tích khuôn viên HOẶC nhập chiều ngang và chiều dài khu đất."));
                        AddModelError("AreaTotalWidth", T(""));
                        AddModelError("AreaTotalLength", T(""));
                    }
                    // AreaTotal & AreaResidential
                    if (editModel.AreaTotal.HasValue && editModel.AreaResidential.HasValue && editModel.AreaTotal < editModel.AreaResidential)
                    {
                        AddModelError("AreaResidential", T("Diện tích hợp quy hoạch phải nhỏ hơn diện tích khuôn viên."));
                    }

                    #endregion

                    #region Status RAO BÁN

                    var _status = _propertyService.GetStatus(editModel.StatusId);

                    if (_status.CssClass == "st-new" || _status.CssClass == "st-selling" || _status.CssClass == "st-negotiate" || _status.CssClass == "st-trading" || _status.CssClass == "st-sold")
                    {
                        if (editModel.PriceProposed <= 0)
                        {
                            // chưa có thông tin Hẻm
                            AddModelError("PriceProposed", T("Vui lòng nhập giá rao bán."));
                        }
                    }

                    #endregion

                    #endregion

                    #region UPDATE MODEL

                    var lastInfoFromUser = _groupService.GetUser(editModel.LastInfoFromUserId);

                    // User
                    p.Record.LastUpdatedDate = lastUpdatedDate;
                    p.Record.LastUpdatedUser = lastUpdatedUser;
                    p.Record.LastInfoFromUser = lastInfoFromUser;

                    // Type
                    p.Record.Type = _propertyService.GetType(editModel.TypeId);
                    p.Record.TypeGroup = p.Type.Group;

                    // Address
                    #region Address
                    if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                    {

                        // Province
                        p.Record.Province = _addressService.GetProvince(editModel.ProvinceId);
                        p.Record.OtherProvinceName = editModel.ProvinceId.HasValue ? "" : editModel.OtherProvinceName;

                        // District
                        p.Record.District = _addressService.GetDistrict(editModel.DistrictId);
                        p.Record.OtherDistrictName = editModel.DistrictId.HasValue ? "" : editModel.OtherDistrictName;

                        // Ward
                        p.Record.Ward = _addressService.GetWard(editModel.WardId);
                        p.Record.OtherWardName = editModel.WardId.HasValue ? "" : editModel.OtherWardName;

                        // Street
                        var selectedStreet = _addressService.GetStreet(editModel.StreetId);
                        p.Record.Street = selectedStreet;
                        p.Record.OtherStreetName = editModel.StreetId.HasValue ? "" : editModel.OtherStreetName;

                        p.Record.AlleyNumber = _propertyService.IntParseAddressNumber(editModel.AddressNumber);

                        // Street Segment
                        var segmentStreet = _addressService.GetStreet(selectedStreet, p.Record.AlleyNumber);
                        if (segmentStreet != null)
                            p.Record.Street = segmentStreet;
                    }
                    #endregion

                    // LegalStatus
                    p.Record.LegalStatus = _propertyService.GetLegalStatus(editModel.LegalStatusId);

                    // Direction
                    p.Record.Direction = _propertyService.GetDirection(editModel.DirectionId);

                    // Location
                    #region Location
                    p.Record.Location = _propertyService.GetLocation(editModel.LocationCssClass);
                    if (p.Location != null)
                    {
                        if (p.Location.CssClass == "h-front")
                        {
                            p.Record.DistanceToStreet = editModel.StreetWidth;
                            p.Record.AlleyTurns = null;
                            p.Record.AlleyWidth1 = null;
                            p.Record.AlleyWidth2 = null;
                            p.Record.AlleyWidth3 = null;
                            p.Record.AlleyWidth4 = null;
                            p.Record.AlleyWidth5 = null;
                            p.Record.AlleyWidth6 = null;
                            p.Record.AlleyWidth7 = null;
                            p.Record.AlleyWidth8 = null;
                            p.Record.AlleyWidth9 = null;
                            p.Record.AlleyWidth = null;
                            p.Record.StreetWidth = null;
                        }
                        else
                        {
                            if (editModel.AlleyTurns > 0)
                            {
                                p.Record.AlleyWidth = new List<double?> { editModel.AlleyWidth1, editModel.AlleyWidth2, editModel.AlleyWidth3, editModel.AlleyWidth4, editModel.AlleyWidth5, editModel.AlleyWidth6, editModel.AlleyWidth7, editModel.AlleyWidth8, editModel.AlleyWidth9 }[(int)editModel.AlleyTurns - 1];
                            }
                            p.Record.DistanceToStreet = null;
                        }
                    }
                    #endregion

                    // calc AreaUsable

                    #region Price

                    // PaymentMethod
                    p.Record.PaymentMethod = _propertyService.GetPaymentMethod(editModel.PaymentMethodId);

                    // PaymentUnit
                    p.Record.PaymentUnit = _propertyService.GetPaymentUnit(editModel.PaymentUnitId);

                    #endregion

                    #region Flag & Status

                    // Flag
                    p.Record.Flag = _propertyService.GetFlag(editModel.FlagId);

                    // Status
                    p.Record.Status = _propertyService.GetStatus(editModel.StatusId);
                    if (p.Record.Status.CssClass == "st-estimate")
                    {
                        p.Record.Status = _propertyService.GetStatus("st-new");
                    }

                    // Tự động chuyển sang trạng thái RAO BÁN nếu đủ thông tin Địa chỉ, Hẻm, Diện tích, Giá
                    if (p.Record.Status.CssClass == "st-new")
                    {
                        if (_propertyService.IsValid(p))
                            p.Status = _propertyService.GetStatus("st-selling");
                    }

                    #endregion

                    #region Ads

                    // Ads Type
                    p.Record.AdsType = _propertyService.GetAdsType(editModel.AdsTypeId);

                    // Published
                    if (p.Published) p.Record.AdsExpirationDate = _propertyService.GetAddExpirationDate(editModel.AddAdsExpirationDate, p.Record.AdsExpirationDate);
                    else p.Record.AdsExpirationDate = null;

                    // AdsVIP
                    if (p.AdsVIP) p.Record.AdsVIPExpirationDate = _propertyService.GetAddExpirationDate(editModel.AddAdsVIPExpirationDate, p.Record.AdsVIPExpirationDate);
                    else p.Record.AdsVIPExpirationDate = null;

                    // AdsGoodDeal
                    // Tự động correct BĐS Giá rẻ hiện trên trang chủ
                    if (p.AdsType.CssClass == "ad-selling")
                        if (p.Flag.CssClass != "deal-good" && p.Flag.CssClass != "deal-very-good") p.Record.AdsGoodDeal = false;
                    if (p.AdsGoodDeal) p.Record.AdsGoodDealExpirationDate = _propertyService.GetAddExpirationDate(editModel.AddAdsGoodDealExpirationDate, p.Record.AdsGoodDealExpirationDate);
                    else p.Record.AdsGoodDealExpirationDate = null;

                    // Thời gian đăng tin AdsExpirationDate = MAX(AdsExpirationDate, AdsVIPExpirationDate, AdsGoodDealExpirationDate)
                    p.Record.AdsExpirationDate = (p.AdsVIPExpirationDate != null && (p.AdsVIPExpirationDate > p.AdsExpirationDate || p.AdsExpirationDate == null)) ? p.AdsVIPExpirationDate : p.AdsExpirationDate;
                    p.Record.AdsExpirationDate = (p.AdsGoodDealExpirationDate != null && (p.AdsGoodDealExpirationDate > p.AdsExpirationDate || p.AdsExpirationDate == null)) ? p.AdsGoodDealExpirationDate : p.AdsExpirationDate;

                    if (p.Status.CssClass == "st-selling" && p.Published && (p.AdsExpirationDate == null || p.AdsExpirationDate < DateTime.Now)) p.Record.AdsExpirationDate = DateTime.Now.AddDays(90);

                    #endregion

                    // AreaUsable
                    p.Record.AreaUsable = _propertyService.CalcAreaUsable(p);

                    // PriceProposedInVND
                    p.Record.PriceProposedInVND = _propertyService.CaclPriceProposedInVND(p);

                    #endregion

                    Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật ngày thành công.", Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddress));

                    #region Save Revision

                    bool isChanged = false;

                    var newAddress = p.DisplayForAddress;

                    if (oldModel.StatusId != editModel.StatusId)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "StatusId", oldModel.StatusId, editModel.StatusId);
                        isChanged = true;
                    }

                    if (oldPriceProposed != editModel.PriceProposed || oldModel.PaymentMethodId != editModel.PaymentMethodId || oldModel.PaymentUnitId != editModel.PaymentUnitId)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "PriceProposed", oldPriceProposed, editModel.PriceProposed);
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "PaymentMethodId", oldModel.PaymentMethodId, editModel.PaymentMethodId);
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "PaymentUnitId", oldModel.PaymentUnitId, editModel.PaymentUnitId);
                        isChanged = true;
                    }

                    if (oldAddress != newAddress)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "Address", oldAddress, newAddress);
                        isChanged = true;
                    }

                    if (oldContactName != editModel.ContactName)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "ContactName", oldContactName, editModel.ContactName);
                        isChanged = true;
                    }

                    if (oldContactPhone != editModel.ContactPhone)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "ContactPhone", oldContactPhone, editModel.ContactPhone);
                        isChanged = true;
                    }

                    if (oldNote != editModel.Note)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "Note", oldNote, editModel.Note);
                        isChanged = true;
                    }

                    if (oldModel.LastInfoFromUserId != editModel.LastInfoFromUserId)
                    {
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "LastInfoFromUserId", oldModel.LastInfoFromUserId, editModel.LastInfoFromUserId);
                        isChanged = true;
                    }

                    // Log User Activity
                    if (isChanged)
                        _revisionService.SaveUserActivityUpdateProperty(lastUpdatedDate, lastUpdatedUser, p.Record);

                    #endregion

                }
                else
                {
                    //Services.TransactionManager.Cancel();
                    p.Record.LastUpdatedDate = lastUpdatedDate;
                    p.Record.LastUpdatedUser = lastUpdatedUser;
                    if (p.Status.CssClass == "st-selling" && p.Published && (p.AdsExpirationDate == null || p.AdsExpirationDate < DateTime.Now)) p.Record.AdsExpirationDate = DateTime.Now.AddDays(90);
                    Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật ngày thành công.", Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddress));
                }
            }

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = _propertyService.BuildLandEditViewModel(p);

                var editor = Shape.EditorTemplate(
                    TemplateName: "Parts/Property.EditLand", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            // Save & Continue
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinue"]) || !string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
            {
                return RedirectToAction("Edit", new { id = id, returnUrl = editModel.ReturnUrl });
            }

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index", "PropertyAdmin", new { });
        }

        #endregion

        public ActionResult Details(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            List<PropertyRevisionEntry> model = new List<PropertyRevisionEntry>();

            var revisions = _revisionService.GetPropertyRevisions(id);
            var dates = revisions.Select(a => a.CreatedDate).Distinct().ToList();

            foreach (var date in dates)
            {
                var entry = new PropertyRevisionEntry();
                var records = revisions.Where(a => a.CreatedDate == date);
                    #region GET DATA
                    foreach (var item in records)
                    {
                        entry.CreatedDate = item.CreatedDate;
                        entry.CreatedUser = item.CreatedUser;

                        switch (item.FieldName)
                        {
                            case "StatusId":
                                int StatusId = int.Parse(item.ValueBefore);
                                entry.StatusName = _propertyService.GetStatus(StatusId).Name;
                                break;

                            case "PriceProposed":
                                entry.PriceProposed = item.ValueBefore;
                                break;

                            case "PaymentMethodId":
                                int PaymentMethodId = int.Parse(item.ValueBefore);
                                entry.PaymentMethodName = _propertyService.GetPaymentMethod(PaymentMethodId).ShortName;
                                break;

                            case "PaymentUnitId":
                                int PaymentUnitId = int.Parse(item.ValueBefore);
                                entry.PaymentUnitName = _propertyService.GetPaymentUnit(PaymentUnitId).Name;
                                break;

                            case "Address":
                                entry.Address = item.ValueBefore;
                                break;

                            case "ContactName":
                                entry.ContactName = item.ValueBefore;
                                break;

                            case "ContactPhone":
                                entry.ContactPhone = item.ValueBefore;
                                break;

                            case "Note":
                                entry.Note = item.ValueBefore;
                                break;

                            case "LastInfoFromUserId":
                                int LastInfoFromUserId = int.Parse(item.ValueBefore);
                                entry.LastInfoFromUserName = _groupService.GetUser(LastInfoFromUserId).UserName;
                                break;

                            case "Delete Image":
                            case "Add Image":
                                var file = _propertyService.GetPropertyFile(id, item.ValueAfter);
                                if (!String.IsNullOrEmpty(item.ValueAfter) && file != null)
                                {
                                    entry.ImageId = file.Id;
                                    entry.ImageUrl = file.Path;
                                    entry.ImageName = file.Name;
                                    entry.ImagePublished = file.Published;
                                    entry.Editable = Services.Authorizer.Authorize(Permissions.ManageProperties);
                                }
                                else
                                {
                                    entry.ImageName = item.ValueBefore;
                                    entry.AddImage = item.FieldName + ": " + item.ValueBefore;
                                    entry.Editable = false;
                                }
                                break;

                        }
                    }
                    #endregion
                
                model.Add(entry);
            }

            return PartialView(new PropertyRevisionsViewModel { Property = p, Revisions = model });
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

    }
}
