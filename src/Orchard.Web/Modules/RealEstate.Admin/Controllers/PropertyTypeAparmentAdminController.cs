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
    public class PropertyTypeApartmentAdminController : Controller, IUpdateModel
    {
        #region Init

        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertySettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IPropertyService _propertyService;
        private readonly IRevisionService _revisionService;
        private readonly ISiteService _siteService;

        public PropertyTypeApartmentAdminController(
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

        #region Create / Edit Apartment

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            var property = Services.ContentManager.New<PropertyPart>("Property");
            dynamic model = Services.ContentManager.BuildEditor(property);

            var viewModel = _propertyService.BuildApartmentCreateViewModel();
            if (Request["AdsTypeId"] != null) viewModel.AdsTypeId = int.Parse(Request["AdsTypeId"]);
            if (Request["PaymentMethodId"] != null) viewModel.PaymentMethodId = int.Parse(Request["PaymentMethodId"]);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Property.CreateApartment", Model: viewModel, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(PropertyApartmentCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            #region VALIDATION

            #region Province, District, Ward, Street

            // Street
            if (string.IsNullOrEmpty(createModel.OtherProjectName))
            {
                if (!createModel.ChkOtherStreetName && !createModel.StreetId.HasValue)
                {
                    AddModelError("StreetId", T("Vui lòng chọn tên đường hoặc nhập Tên dự án / chung cư."));
                    AddModelError("OtherProjectName", T(""));
                }
                // OtherStreetName
                if (createModel.ChkOtherStreetName && string.IsNullOrEmpty(createModel.OtherStreetName))
                {
                    AddModelError("OtherStreetName", T("Vui lòng nhập tên đường hoặc nhập Tên dự án / chung cư."));
                    AddModelError("OtherProjectName", T(""));
                }
            }

            #endregion

            #endregion

            #region CREATE RECORD

            var p = Services.ContentManager.New<PropertyPart>("Property");
            if (ModelState.IsValid)
            {
                p.Record.Type = _propertyService.GetType(createModel.TypeId);
                p.Record.TypeGroup = p.Type.Group;

                var createdDate = DateTime.Now;
                var createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;
                var infoFromUser = _groupService.GetUser(createModel.LastInfoFromUserId);

                #region Address

                // Province
                p.Record.Province = _addressService.GetProvince(createModel.ProvinceId);

                // District
                p.Record.District = _addressService.GetDistrict(createModel.DistrictId);

                // Ward
                if (createModel.WardId.HasValue)
                    p.Record.Ward = _addressService.GetWard(createModel.WardId);
                p.Record.OtherWardName = createModel.WardId.HasValue ? "" : createModel.OtherWardName;

                // Street
                if (createModel.StreetId.HasValue)
                    p.Record.Street = _addressService.GetStreet(createModel.StreetId);
                p.Record.OtherStreetName = createModel.StreetId.HasValue ? "" : createModel.OtherStreetName;

                // ProjectName
                p.Record.OtherProjectName = createModel.OtherProjectName;

                // Address
                p.Record.AddressNumber = createModel.AddressNumber;
                p.Record.AlleyNumber = _propertyService.IntParseAddressNumber(createModel.AddressNumber);
                p.Record.ApartmentNumber = createModel.ApartmentNumber;

                #endregion

                #region Apartment Info

                // Apartment Info
                p.Record.ApartmentFloors = createModel.ApartmentFloors;
                p.Record.ApartmentFloorTh = createModel.ApartmentFloorTh;
                p.Record.ApartmentElevators = createModel.ApartmentElevators;
                p.Record.ApartmentBasements = createModel.ApartmentBasements;

                p.Record.ApartmentHaveChildcare = createModel.ApartmentHaveChildcare;
                p.Record.ApartmentHavePark = createModel.ApartmentHavePark;
                p.Record.ApartmentHaveSwimmingPool = createModel.ApartmentHaveSwimmingPool;
                p.Record.ApartmentHaveSuperMarket = createModel.ApartmentHaveSuperMarket;
                p.Record.ApartmentHaveSportCenter = createModel.ApartmentHaveSportCenter;

                #endregion

                #region Construction

                // Construction
                p.Record.AreaUsable = createModel.AreaUsable;
                p.Record.Bedrooms = createModel.Bedrooms;
                p.Record.Bathrooms = createModel.Bathrooms;
                p.Record.Balconies = createModel.Balconies;
                p.Record.RemainingValue = createModel.RemainingValue;

                #endregion

                #region Interior

                // Interior
                p.Record.InteriorHaveWoodFloor = createModel.InteriorHaveWoodFloor;
                p.Record.InteriorHaveToiletEquipment = createModel.InteriorHaveToiletEquipment;
                p.Record.InteriorHaveKitchenEquipment = createModel.InteriorHaveKitchenEquipment;
                p.Record.InteriorHaveBedCabinets = createModel.InteriorHaveBedCabinets;
                p.Record.InteriorHaveAirConditioner = createModel.InteriorHaveAirConditioner;

                #endregion

                #region Direction, LegalStatus

                // Direction
                p.Record.Direction = _propertyService.GetDirection(createModel.DirectionId);

                // LegalStatus
                p.Record.LegalStatus = _propertyService.GetLegalStatus(createModel.LegalStatusId);

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

                // PriceProposedInVND
                if (p.Record.PaymentUnit.CssClass == "unit-total")
                {
                    p.Record.PriceProposedInVND = _propertyService.ConvertToVNDB((double)p.Record.PriceProposed, p.Record.PaymentMethod.CssClass);
                }
                else if (p.Record.PaymentUnit.CssClass == "unit-m2")
                {
                    double _area = p.AreaUsable.HasValue ? (double)p.AreaUsable : 0;
                    p.Record.PriceProposedInVND = _propertyService.ConvertToVNDB((double)p.Record.PriceProposed, p.Record.PaymentMethod.CssClass, _area);
                }
                #endregion

                #region Status, Flag

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

                // Log User Activity
                _revisionService.SaveUserActivityAddNewProperty(createdDate, createdUser, p.Record);

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

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Property.CreateApartment", Model: _propertyService.BuildApartmentCreateViewModel(createModel), Prefix: null);
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

            var editModel = _propertyService.BuildApartmentEditViewModel(p);
            editModel.ReturnUrl = returnUrl;
            editModel.LastInfoFromUserId = Services.WorkContext.CurrentUser.Id; // Current User

            #endregion

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Property.EditApartment", Model: editModel, Prefix: null);
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

            var oldModel = _propertyService.BuildApartmentEditViewModel(p);
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

            var editModel = new PropertyApartmentEditViewModel { Property = p };

            if (TryUpdateModel(editModel))
            {
                if (editModel.IsChanged)
                {

                    #region UPDATE MODEL

                    p.Record.Type = _propertyService.GetType(editModel.TypeId);
                    p.Record.TypeGroup = p.Type.Group;

                    var lastInfoFromUser = _groupService.GetUser(editModel.LastInfoFromUserId);

                    // User
                    p.Record.LastUpdatedDate = lastUpdatedDate;
                    p.Record.LastUpdatedUser = lastUpdatedUser;
                    p.Record.LastInfoFromUser = lastInfoFromUser;

                    // Address
                    if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                    {
                        // Province
                        p.Record.Province = _addressService.GetProvince(editModel.ProvinceId);

                        // District
                        p.Record.District = _addressService.GetDistrict(editModel.DistrictId);

                        // Ward
                        if (editModel.WardId.HasValue)
                            p.Record.Ward = _addressService.GetWard(editModel.WardId);
                        p.Record.OtherWardName = editModel.WardId.HasValue ? "" : editModel.OtherWardName;

                        // Street
                        if (editModel.StreetId.HasValue)
                            p.Record.Street = _addressService.GetStreet(editModel.StreetId);
                        p.Record.OtherStreetName = editModel.StreetId.HasValue ? "" : editModel.OtherStreetName;

                        p.Record.AlleyNumber = _propertyService.IntParseAddressNumber(editModel.AddressNumber);
                    }

                    // LegalStatus
                    p.Record.LegalStatus = _propertyService.GetLegalStatus(editModel.LegalStatusId);

                    // Direction
                    p.Record.Direction = _propertyService.GetDirection(editModel.DirectionId);

                    #region Price
                    // PaymentMethod
                    p.Record.PaymentMethod = _propertyService.GetPaymentMethod(editModel.PaymentMethodId);

                    // PaymentUnit
                    p.Record.PaymentUnit = _propertyService.GetPaymentUnit(editModel.PaymentUnitId);

                    // PriceProposedInVND
                    if (p.Record.PaymentUnit.CssClass == "unit-total")
                    {
                        p.Record.PriceProposedInVND = _propertyService.ConvertToVNDB((double)p.Record.PriceProposed, p.Record.PaymentMethod.CssClass);
                    }
                    else if (p.Record.PaymentUnit.CssClass == "unit-m2")
                    {
                        double _area = p.AreaUsable.HasValue ? (double)p.AreaUsable : 0;
                        p.Record.PriceProposedInVND = _propertyService.ConvertToVNDB((double)p.Record.PriceProposed, p.Record.PaymentMethod.CssClass, _area);
                    }
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

                    #endregion

                    Services.Notifier.Information(T("Property <a href='{0}'>{1}</a> updated successfully.", Url.Action("Edit", new { p.Id }), p.DisplayForAddress));

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

                editModel = _propertyService.BuildApartmentEditViewModel(p);

                var editor = Shape.EditorTemplate(
                    TemplateName: "Parts/Property.EditApartment", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            // Save & Continue
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinue"]))
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
