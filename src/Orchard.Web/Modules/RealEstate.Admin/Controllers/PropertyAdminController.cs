using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Maps.Services;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class PropertyAdminController : Controller, IUpdateModel
    {
        private static System.Collections.Concurrent.BlockingCollection<string> _data = new System.Collections.Concurrent.BlockingCollection<string>();
        static PropertyAdminController()
        {
            _data.Add("started");
            for (int i = 0; i < 100; i++)
            {
                _data.Add("item" + i.ToString());
            }
            _data.Add("ended");
        }

        private readonly bool _debugEdit = true;
        private readonly bool _debugIndex = true;

        List<string> priceRequiredStatus = new List<string> { "st-new", "st-selling", "st-negotiate", "st-trading", "st-sold" };

        #region Init

        private readonly IAddressService _addressService;
        private readonly IFacebookApiService _facebookApiSevice;
        private readonly IGoogleApiService _googleApiService;
        private readonly IUserGroupService _groupService;
        private readonly IHostNameService _hostNameService;
        private readonly IMapService _mapService;
        private readonly IAdsPaymentHistoryService _paymentHistory;
        private readonly IPropertyService _propertyService;
        private readonly IRevisionService _revisionService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly ILocationApartmentService _apartmentService;
        private readonly IPropertyExchangeService _propertyExchangeService;
        private readonly IPropertySettingService _settingService;

        public PropertyAdminController(
            IAddressService addressService,
            IPropertyService propertyService,
            IRevisionService revisionService,
            IUserGroupService groupService,
            IHostNameService hostNameService,
            ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IGoogleApiService googleApiService,
            IAdsPaymentHistoryService paymentHistory,
            IFacebookApiService facebookApiSevice,
            IMapService mapService,
            ILocationApartmentService apartmentService,
            IPropertySettingService settingService,
            IPropertyExchangeService propertyExchangeService)
        {
            _addressService = addressService;
            _propertyService = propertyService;
            _revisionService = revisionService;
            _groupService = groupService;
            _hostNameService = hostNameService;
            _signals = signals;
            _siteService = siteService;
            _googleApiService = googleApiService;
            _mapService = mapService;
            _paymentHistory = paymentHistory;
            _facebookApiSevice = facebookApiSevice;
            _apartmentService = apartmentService;
            _propertyExchangeService = propertyExchangeService;
            _settingService = settingService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;

            if (!Services.Authorizer.Authorize(Permissions.ViewDebugLogEstimateProperties))
            {
                _debugIndex = false;
                _debugEdit = false;
            }
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #endregion

        public ActionResult Message()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var group = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            var result = string.Empty;

            var sb = new System.Text.StringBuilder();

            if (_data.TryTake(out result, TimeSpan.FromMilliseconds(1000)))
            {

                JavaScriptSerializer ser = new JavaScriptSerializer();

                var serializedObject = ser.Serialize(new
                {
                    userId = user != null ? user.Id : 0,
                    groupId = group != null ? group.Id : 0,
                    item = result,
                    message = "hello"
                });

                sb.AppendFormat("data: {0}\n\n", serializedObject);

            }

            return Content(sb.ToString(), "text/event-stream");

        }

        #region Index

        public async Task<ActionResult> Index(PropertyIndexOptions options, PagerParameters pagerParameters)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            DateTime startIndex = DateTime.Now;

            if (
                !Services.Authorizer.Authorize(Permissions.MetaListOwnProperties, T("Not authorized to list properties")))
                return new HttpUnauthorizedResult();

            IContentQuery<PropertyPart, PropertyPartRecord> list;

            #region FILTER

            DateTime startFilter = DateTime.Now;

            if (!string.IsNullOrEmpty(options.List))
            {
                // Quyền xem nhà dùng tham chiếu định giá
                if (
                    !Services.Authorizer.Authorize(Permissions.ViewReferencedProperties,
                        T("Not authorized to list properties")))
                    return new HttpUnauthorizedResult();

                options = _propertyService.BuildIndexOptions(options);
                List<int> listIds = await _propertyService.GetListPropertiesUseToEstimate(options.List);
                list = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Where(p => listIds.Contains(p.Id));
            }
            else
            {
                list = _propertyService.SearchProperties(options);
            }

            if (_debugIndex) Services.Notifier.Information(T("FILTER {0}", (DateTime.Now - startFilter).TotalSeconds));

            #endregion

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<PropertyPart> results = list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region ESTIMATION

            if (options.ShowEstimation)
            {
                //DateTime startEstimation = DateTime.Now;

                //foreach (
                //    PropertyPart p in
                //        results.Where(item => item.TypeGroup != null && item.TypeGroup.CssClass == "gp-house"))
                //{
                //    var entry = await _propertyService.EstimateProperty(p.Id);
                //    p.PriceEstimatedInVND = entry.PriceEstimatedInVND;
                //}

                //Services.Notifier.Information(T("ESTIMATE {0}", (DateTime.Now - startEstimation).TotalSeconds));
            }

            #endregion

            #region BUILD MODEL

            DateTime startBuildModel = DateTime.Now;

            PropertyStatusPartRecord statusNegotiate = _propertyService.GetStatus("st-negotiate");
            bool accessNegotiateProperties = Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties);

            DateTime dateExportRecently = DateTime.Now.AddDays(-10);
            DateTime dateExportExpired = DateTime.Now.AddDays(-30);

            var model = new PropertyIndexViewModel
            {
                Properties = results
                    .Select(x => new PropertyEntry
                    {
                        PropertyPart = x,
                        Files = _propertyService.GetPropertyFiles(x),
                        Advantages = _propertyService.GetAdvantagesForProperty(x),
                        ShowAddressNumber = _propertyService.EnableViewAddressNumber(x, user),
                        ShowContactPhone = !(x.Status.Id == statusNegotiate.Id && !accessNegotiateProperties),
                        // vừa xuất tin trong vòng 10 ngày gần đây
                        IsExportedRecently = (x.LastExportedDate.HasValue && x.LastExportedDate > dateExportRecently),
                        // đã xuất tin 30 ngày trước
                        IsExportedExpired = (x.LastExportedDate.HasValue && x.LastExportedDate < dateExportExpired),
                        IsCheckSavedProperty = _propertyService.CheckIsSavedProperty(x.Id),
                        PropertyExchange = _propertyService.GetExchangePartRecordByPropertyId(x.Id)
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount,
                TotalExecutionTime = (DateTime.Now - startIndex).TotalSeconds,
            };

            if (_debugIndex)
                Services.Notifier.Information(T("MODEL {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            //if (_debugIndex) Services.Notifier.Information(T("COMPLETE {0}", (DateTime.Now - startIndex).TotalSeconds));

            #region Clear Cache Action

            if (TempData["PropertyClearCacheId"] != null)
            {
                try
                {
                    var pId = (int)TempData["PropertyClearCacheId"];
                    //Task.Run(() => ClearCacheClientWhenUpdate(pId));
                    ClearCacheClientWhenUpdate(pId);
                }
                catch (Exception ex)
                {
                    Services.Notifier.Error(T("1. Clear Cache Action Error: {0}", ex.Message));
                }
            }

            if (TempData["checkedEntries"] != null)
            {
                try
                {
                    var checkedEntries = (List<PropertyEntry>)TempData["checkedEntries"];

                    var propertIds = checkedEntries.Select(r => r.Property.Id).ToList();
                    //Task.Run(() => ClearCaceWhenActionWithProperties(string.Join(",",propertIds)));
                    ClearCaceWhenActionWithProperties(string.Join(",", propertIds));
                }
                catch (Exception ex)
                {
                    Services.Notifier.Error(T("2. Clear Cache Action Error: {0}", ex.Message));
                }
            }

            #endregion

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.BulkActionProperties,
                    T("Not authorized to bulk action properties")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyIndexViewModel
            {
                Properties = new List<PropertyEntry>(),
                Options = new PropertyIndexOptions()
            };
            UpdateModel(viewModel);

            List<PropertyEntry> checkedEntries = viewModel.Properties.Where(c => c.IsChecked).ToList();

            var publishBulkActions = new List<PropertyBulkAction>();
            if (viewModel.Options.PublishBulkAction != null)
                publishBulkActions.AddRange(viewModel.Options.PublishBulkAction);
            publishBulkActions.Add(viewModel.Options.BulkAction);

            foreach (PropertyBulkAction bulkAction in publishBulkActions)
            {
                switch (bulkAction)
                {
                    case PropertyBulkAction.None:
                        break;

                    #region Estimation

                    case PropertyBulkAction.AddToEstimation:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            AddToEstimation(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.RemoveFromEstimation:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            RemoveFromEstimation(entry.Property.Id);
                        }
                        break;

                    #endregion

                    #region Listing, Trash, Delete, Export

                    case PropertyBulkAction.Listing:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Listing(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.Trash:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Trash(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.Delete:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Delete(entry.Property.Id);
                        }
                        break;

                    case PropertyBulkAction.Export:
                        if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner) ||
                            Services.Authorizer.Authorize(Permissions.ExportProperties))
                        {
                            List<int> selectedIds = checkedEntries.Select(a => a.Property.Id).ToList();
                            List<PropertyEntry> properties = Services.ContentManager
                                .Query<PropertyPart, PropertyPartRecord>().Where(a => selectedIds.Contains(a.Id)).List()
                                .Select(x => new PropertyEntry
                                {
                                    Property = x.Record,
                                    PropertyPart = x,
                                    DisplayForContact = _propertyService.GetDisplayForContact(x)
                                }).ToList();
                            foreach (PropertyEntry item in properties)
                            {
                                DateTime lastExportedDate = DateTime.Now;
                                var user = Services.WorkContext.CurrentUser.As<UserPart>();
                                item.Property.LastExportedDate = lastExportedDate;
                                item.Property.LastExportedUser = user.Record;
                            }
                            return View("Print", properties);
                        }
                        break;

                    #endregion

                    #region Refresh, Approve, NotApprove, Copy

                    // Duyệt BĐS
                    case PropertyBulkAction.Refresh:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Refresh(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.Approve:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Approve(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.NotApprove:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            NotApprove(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.Copy:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Copy(entry.Property.Id);
                        }
                        break;

                    #endregion

                    #region Publish, PublishAddress, PublishContact

                    // Cho phép tin rao hiện trên trang chủ
                    case PropertyBulkAction.Publish:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            Publish(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnPublish:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnPublish(entry.Property.Id);
                        }
                        break;

                    // Cho phép hiện địa chỉ
                    case PropertyBulkAction.PublishAddress:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            PublishAddress(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnPublishAddress:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnPublishAddress(entry.Property.Id);
                        }
                        break;

                    // Cho phép hiện liên hệ
                    case PropertyBulkAction.PublishContact:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            PublishContact(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnPublishContact:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnPublishContact(entry.Property.Id);
                        }
                        break;

                    #endregion

                    #region AdsGoodDeal, AdsVIP, AdsHighlight

                    case PropertyBulkAction.AddToAdsGoodDeal:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            AddToAdsGoodDeal(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.RemoveAdsGoodDeal:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            RemoveAdsGoodDeal(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.AddToVIP1:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            AddToAdsVip(entry.Property.Id, 3);
                        }
                        break;
                    case PropertyBulkAction.AddToVIP2:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            AddToAdsVip(entry.Property.Id, 2);
                        }
                        break;
                    case PropertyBulkAction.AddToVIP3:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            AddToAdsVip(entry.Property.Id, 1);
                        }
                        break;
                    case PropertyBulkAction.RemoveAdsVIP:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            RemoveAdsVip(entry.Property.Id);
                        }
                        break;
                    /*case PropertyBulkAction.AddToAdsHighlight:
                    foreach (PropertyEntry entry in checkedEntries)
                    {
                        AddToAdsHighlight(entry.Property.Id);
                    }
                    break;
                case PropertyBulkAction.RemoveAdsHighlight:
                    foreach (PropertyEntry entry in checkedEntries)
                    {
                        RemoveAdsHighlight(entry.Property.Id);
                    }
                    break;
                    */

                    #endregion

                    #region IsOwner, NoBroker, IsAuction, IsAuthenticatedInfo

                    // BĐS chính chủ
                    case PropertyBulkAction.SetIsOwner:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            SetIsOwner(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnSetIsOwner:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnSetIsOwner(entry.Property.Id);
                        }
                        break;

                    // BĐS miễn trung gian
                    case PropertyBulkAction.SetNoBroker:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            SetNoBroker(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnSetNoBroker:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnSetNoBroker(entry.Property.Id);
                        }
                        break;

                    // BĐS phát mãi
                    case PropertyBulkAction.SetIsAuction:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            SetIsAuction(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnSetIsAuction:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnSetIsAuction(entry.Property.Id);
                        }
                        break;

                    // BĐS đã xác thực
                    case PropertyBulkAction.SetIsAuthenticatedInfo:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            SetIsAuthenticatedInfo(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.UnSetIsAuthenticatedInfo:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnSetIsAuthenticatedInfo(entry.Property.Id);
                        }
                        break;
                    case PropertyBulkAction.DeleteUserProperties:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            DeleteUserProperty(entry.Property.Id);
                        }
                        break;

                    #endregion

                    #region Mass-Update Properties

                    case PropertyBulkAction.UpdateNegotiateStatus:
                        if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                        {
                            _propertyService.UpdateNegotiateStatus();
                        }
                        break;

                    case PropertyBulkAction.UpdateMetaDescriptionKeywords:
                        if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                        {
                            _propertyService.UpdateMetaDescriptionKeywords();
                        }
                        break;

                    case PropertyBulkAction.TransferPropertyTypeConstruction:
                        if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner) ||
                            Services.Authorizer.Authorize(Permissions.ManageProperties))
                        {
                            _propertyService.TransferPropertyTypeConstruction();
                        }
                        break;

                        #endregion
                }
            }

            #region Clear Cache client

            if(checkedEntries.Any())
            {
                TempData["checkedEntries"] = checkedEntries;                
            }

	        #endregion 
            

            return this.RedirectLocal(viewModel.ReturnUrl);
            //return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        #endregion

        #region UserProperty

        public ActionResult UserPropertyIndex()
        {

            return View();
        }

        #endregion

        #region Create

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            #region Facebook

            if (Session["PropertyAdminId"] != null)
            {
                try
                {
                    var pId = (int)Session["PropertyAdminId"];
                    ActionPostFacebook(pId);
                }
                catch (Exception ex)
                {
                    Services.Notifier.Error(T("PostToYourFacebook Error: {0}", ex.Message));
                }
            }

            #endregion
                       

            var property = Services.ContentManager.New<PropertyPart>("Property");
            dynamic model = Services.ContentManager.BuildEditor(property);

            string adsTypeCssClass = "";
            if (Request["AdsTypeCssClass"] != null)
            {
                adsTypeCssClass = Request["AdsTypeCssClass"];
            }

            else if (Request["AdsTypeId"] != null)
            {
                AdsTypePartRecord adsType = _propertyService.GetAdsType(int.Parse(Request["AdsTypeId"]));
                if (adsType != null) adsTypeCssClass = adsType.CssClass;
            }

            string typeGroupCssClass = "gp-house";
            if (Request["TypeGroupCssClass"] != null) typeGroupCssClass = Request["TypeGroupCssClass"];

            PropertyCreateViewModel viewModel = _propertyService.BuildCreateViewModel(adsTypeCssClass, typeGroupCssClass);

            if (Request["PaymentMethodId"] != null) viewModel.PaymentMethodId = int.Parse(Request["PaymentMethodId"]);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Property.Create", Model: viewModel, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ValidateInput(false), ActionName("Create")]
        public ActionResult CreatePost(PropertyCreateViewModel createModel, FormCollection frmCollection,
            IEnumerable<HttpPostedFileBase> files)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to create properties")))
                return new HttpUnauthorizedResult();

            #region VALIDATION

            // "gp-house"
            if (createModel.TypeGroupCssClass == "gp-house")
            {
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
                // Street
                if (!createModel.ChkOtherStreetName && !createModel.StreetId.HasValue)
                {
                    AddModelError("StreetId", T("Vui lòng chọn tên đường."));
                }
                // OtherStreetName
                if (createModel.ChkOtherStreetName && string.IsNullOrEmpty(createModel.OtherStreetName))
                {
                    AddModelError("OtherStreetName", T("Vui lòng nhập tên đường."));
                }

                #endregion

                #region AddressNumber

                // AddressNumber
                if (Services.Authorizer.Authorize(Permissions.RequireAddressNumber))
                {
                    if (string.IsNullOrEmpty(createModel.AddressNumber))
                    {
                        var pType = _propertyService.GetType(createModel.TypeId);
                        if (pType != null && pType.CssClass == "tp-residential-land")
                        {
                            // "đất thổ cư" không cần nhập số nhà
                        }
                        else
                        {
                            // Bắt buộc nhập số nhà (trừ "đất thổ cư" và "các loại đất khác")
                            AddModelError("AddressNumber", T("Vui lòng nhập số nhà."));
                        }
                    }
                    if (!string.IsNullOrEmpty(createModel.AddressNumber))
                    {
                        if (createModel.WardId > 0 && createModel.StreetId > 0)
                        {
                            if (
                                !_propertyService.VerifyPropertyUnicity(0, createModel.ProvinceId, createModel.DistrictId,
                                    createModel.WardId, createModel.StreetId, createModel.ApartmentId,
                                    createModel.AddressNumber, createModel.AddressCorner, createModel.ApartmentNumber,
                                    createModel.AdsTypeCssClass))
                            {
                                PropertyPart r = _propertyService.GetPropertyByAddress(0, createModel.ProvinceId,
                                    createModel.DistrictId, createModel.WardId, createModel.StreetId,
                                    createModel.ApartmentId, createModel.AddressNumber, createModel.AddressCorner,
                                    createModel.ApartmentNumber, createModel.AdsTypeCssClass);
                                AddModelError("AddressNumber",
                                    T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã có trong dữ liệu.",
                                        Url.Action("Edit", new { r.Id }), r.Id, r.DisplayForAddressForOwner));
                                Services.Notifier.Warning(
                                    T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã có trong dữ liệu.",
                                        Url.Action("Edit", new { r.Id }), r.Id, r.DisplayForAddressForOwner));
                            }
                        }
                    }
                }

                #endregion

                #region Location

                if (String.IsNullOrEmpty(createModel.LocationCssClass))
                {
                    AddModelError("LocationCssClass", T("Vui lòng chọn vị trí bất động sản."));
                }

                #endregion

                #region AreaTotal, AreaLegal

                // AreaTotal & AreaTotalWidth + AreaTotalLength
                if (
                    !(createModel.AreaTotal > 0 ||
                      (createModel.AreaTotalWidth > 0 && createModel.AreaTotalLength > 0)))
                {
                    AddModelError("AreaTotal",
                        T("Vui lòng nhập Diện tích khuôn viên HOẶC nhập Chiều ngang và Chiều dài khu đất."));
                    AddModelError("AreaTotalWidth", T(""));
                    AddModelError("AreaTotalLength", T(""));
                }

                // AreaLegal & AreaLegalWidth + AreaLegalLength
                if (createModel.AreaLegalWidth > 0 || createModel.AreaLegalLength > 0)
                    if (
                        !(createModel.AreaLegal > 0 ||
                          (createModel.AreaLegalWidth > 0 && createModel.AreaLegalLength > 0)))
                    {
                        AddModelError("AreaLegal",
                            T("Vui lòng nhập Diện tích hợp quy hoạch HOẶC nhập Chiều ngang và Chiều dài hợp quy hoạch."));
                        AddModelError("AreaLegalWidth", T(""));
                        AddModelError("AreaLegalLength", T(""));
                    }

                #endregion

                #region Area logic

                double areaTotal = _propertyService.CalcArea(createModel.AreaTotal, createModel.AreaTotalWidth,
                    createModel.AreaTotalLength, createModel.AreaTotalBackWidth);

                if (createModel.AreaLegal > 0
                    || (createModel.AreaLegalWidth > 0 && createModel.AreaLegalLength > 0)
                    || createModel.AreaIlegalRecognized > 0
                    )
                {
                    double areaLegal = _propertyService.CalcArea(createModel.AreaLegal, createModel.AreaLegalWidth,
                        createModel.AreaLegalLength, createModel.AreaLegalBackWidth);

                    // DT hợp quy hoạch phải nhỏ hơn DTKV
                    if (areaLegal > areaTotal)
                    {
                        // AreaTotal
                        if (createModel.AreaTotal.HasValue)
                            AddModelError("AreaTotal",
                                T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                        else
                        {
                            AddModelError("AreaTotalWidth",
                                T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                            AddModelError("AreaTotalLength", T(""));
                        }
                        // AreaLegal
                        if (createModel.AreaLegal.HasValue)
                            AddModelError("AreaLegal", T(""));
                        else
                        {
                            AddModelError("AreaLegalWidth", T(""));
                            AddModelError("AreaLegalLength", T(""));
                        }
                    }

                    // AreaIlegalRecognized
                    if (createModel.AreaIlegalRecognized > 0)
                    {
                        if ((areaTotal - areaLegal + 0.1) < createModel.AreaIlegalRecognized)
                        {
                            AddModelError("AreaIlegalRecognized",
                                T(
                                    "Diện tích đất vi phạm lộ giới (quy hoạch) phải nhỏ hơn (Diện tích khuôn viên - Diện tích hợp quy hoạch)."));
                        }
                    }
                }

                // AreaConstruction
                if (createModel.AreaConstruction > 0)
                {
                    if (areaTotal < createModel.AreaConstruction)
                    {
                        AddModelError("AreaConstruction",
                            T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                    }
                }

                #endregion

                #region Status RAO BÁN

                if (createModel.StatusCssClass == "st-selling")
                {

                    // AreaLegal & AreaLegalWidth + AreaLegalLength
                    if (
                        !(createModel.AreaLegal > 0 ||
                          (createModel.AreaLegalWidth > 0 && createModel.AreaLegalLength > 0)))
                    {
                        AddModelError("AreaLegal",
                            T("Vui lòng nhập Diện tích hợp quy hoạch HOẶC nhập Chiều ngang và Chiều dài hợp quy hoạch."));
                        AddModelError("AreaLegalWidth", T(""));
                        AddModelError("AreaLegalLength", T(""));
                    }

                    // Location
                    if (createModel.LocationCssClass != "h-front")
                    {
                        if (!createModel.AlleyTurns.HasValue ||
                            !(createModel.AlleyWidth1.HasValue || createModel.AlleyWidth2.HasValue ||
                              createModel.AlleyWidth3.HasValue || createModel.AlleyWidth4.HasValue ||
                              createModel.AlleyWidth5.HasValue || createModel.AlleyWidth6.HasValue ||
                              createModel.AlleyWidth7.HasValue || createModel.AlleyWidth8.HasValue ||
                              createModel.AlleyWidth9.HasValue))
                        {
                            // chưa có thông tin Hẻm
                            AddModelError("AlleyTurns", T("Vui lòng nhập thông tin Số lần rẽ & Độ rộng hẻm"));
                        }
                    }
                }

                if (string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                {
                    if (priceRequiredStatus.Contains(createModel.StatusCssClass))
                    {
                        if (createModel.PriceNegotiable == false) // Giá thương lượng - không cần nhập Giá rao
                        {
                            if (createModel.PriceProposed == null || createModel.PriceProposed <= 0) // Giá rao phải > 0
                            {
                                AddModelError("PriceProposed", T("Vui lòng nhập giá rao bán / cho thuê."));
                            }
                        }
                    }
                }

                #endregion
            }
            // "gp-apartment"
            if (createModel.TypeGroupCssClass == "gp-apartment")
            {
                #region Apartment

                //// Apartment
                //if (!createModel.ChkOtherProjectName && !createModel.ApartmentId.HasValue)
                //{
                //    AddModelError("ApartmentId", T("Vui lòng chọn Tên dự án / chung cư."));
                //}
                //// OtherProjectName
                //if (createModel.ChkOtherProjectName && string.IsNullOrEmpty(createModel.OtherProjectName))
                //{
                //    AddModelError("OtherProjectName", T("Vui lòng nhập Tên dự án / chung cư."));
                //}

                #endregion

                if (!createModel.AreaUsable.HasValue)
                {
                    AddModelError("AreaUsable", T("Vui lòng nhập Diện tích căn hộ."));
                }
            }
            // "gp-land"
            if (createModel.TypeGroupCssClass == "gp-land")
            {
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

                #region Location

                if (String.IsNullOrEmpty(createModel.LocationCssClass))
                {
                    AddModelError("LocationCssClass", T("Vui lòng chọn vị trí bất động sản."));
                }

                #endregion

                #region AreaTotal, AreaResidential

                // AreaTotal & AreaTotalWidth + AreaTotalLength
                if (
                    !(createModel.AreaTotal.HasValue ||
                      (createModel.AreaTotalWidth.HasValue && createModel.AreaTotalLength.HasValue)))
                {
                    AddModelError("AreaTotal",
                        T("Vui lòng nhập Diện tích khuôn viên HOẶC nhập Chiều ngang và Chiều dài khu đất."));
                    AddModelError("AreaTotalWidth", T(""));
                    AddModelError("AreaTotalLength", T(""));
                }

                double areaTotal = _propertyService.CalcArea(createModel.AreaTotal, createModel.AreaTotalWidth,
                    createModel.AreaTotalLength, createModel.AreaTotalBackWidth);

                // AreaTotal & AreaResidential
                if (createModel.AreaResidential > 0)
                {
                    if (areaTotal < createModel.AreaResidential)
                    {
                        AddModelError("AreaResidential",
                            T("Diện tích đất thổ cư phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                    }
                }

                // AreaTotal & AreaConstruction
                if (createModel.AreaConstruction > 0)
                {
                    if (areaTotal < createModel.AreaConstruction)
                    {
                        AddModelError("AreaConstruction",
                            T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                    }
                }

                #endregion

                #region Status RAO BÁN

                if (priceRequiredStatus.Contains(createModel.StatusCssClass))
                {
                    if (createModel.PriceNegotiable == false) // Giá thương lượng - không cần nhập Giá rao
                    {
                        if (createModel.PriceProposed == null || createModel.PriceProposed <= 0) // Giá rao phải > 0
                        {
                            AddModelError("PriceProposed", T("Vui lòng nhập giá rao bán / cho thuê."));
                        }
                    }
                }

                #endregion
            }

            #endregion


            #region CREATE RECORD

            var p = Services.ContentManager.New<PropertyPart>("Property");
            if (ModelState.IsValid)
            {
                DateTime createdDate = DateTime.Now;
                var createdUser = Services.WorkContext.CurrentUser.As<UserPart>();
                UserPart infoFromUser = _groupService.GetUser(createModel.LastInfoFromUserId);
                UserGroupPartRecord belongGroup = _groupService.GetBelongGroup(createdUser.Id);

                #region RECORD

                #region Type

                // Type
                p.Type = _propertyService.GetType(createModel.TypeId);
                p.TypeGroup = p.Type.Group;

                #endregion

                #region Address

                // Province
                p.Province = _addressService.GetProvince(createModel.ProvinceId);

                // District
                p.District = _addressService.GetDistrict(createModel.DistrictId);

                // Ward
                if (createModel.ChkOtherWardName && !String.IsNullOrEmpty(createModel.OtherWardName))
                {
                    p.Ward = null;
                    p.OtherWardName = createModel.OtherWardName;
                }
                else
                {
                    p.Ward = _addressService.GetWard(createModel.WardId);
                    p.OtherWardName = null;
                }

                // Address
                p.AddressNumber = createModel.AddressNumber;
                p.AddressCorner = createModel.AddressCorner;

                // AlleyNumber
                if (p.Province.Name == "Hà Nội")
                {
                    p.AlleyNumber = _propertyService.IntParseAddressNumber(createModel.AddressCorner);
                }
                else
                {
                    p.AlleyNumber = _propertyService.IntParseAddressNumber(createModel.AddressNumber);
                    p.AddressCorner = null;
                }

                // Street
                if (createModel.ChkOtherStreetName && !String.IsNullOrEmpty(createModel.OtherStreetName))
                {
                    p.Street = null;
                    p.OtherStreetName = createModel.OtherStreetName;
                }
                else if (createModel.StreetId > 0)
                {
                    LocationStreetPartRecord selectedStreet = _addressService.GetStreet(createModel.StreetId);
                    p.Street = selectedStreet;
                    p.OtherStreetName = null;

                    // Street Segment
                    LocationStreetPartRecord segmentStreet = _addressService.GetStreet(selectedStreet, p.AlleyNumber);
                    if (segmentStreet != null)
                        p.Street = segmentStreet;
                }
                else
                {
                    p.Street = null;
                }

                // Apartment
                if (createModel.ChkOtherProjectName && !String.IsNullOrEmpty(createModel.OtherProjectName))
                {
                    p.Apartment = null;
                    p.OtherProjectName = createModel.OtherProjectName;
                }
                else
                {
                    p.Apartment = _addressService.GetApartment(createModel.ApartmentId);
                    p.OtherProjectName = null;
                }

                p.ApartmentBlock = createModel.ApartmentBlockId != null ? _apartmentService.LocationApartmentBlockPart(createModel.ApartmentBlockId.Value).Record : null;


                // ApartmentNumber
                p.ApartmentNumber = createModel.ApartmentNumber;

                #endregion

                #region Legal, Direction, Location

                // LegalStatus
                p.LegalStatus = _propertyService.GetLegalStatus(createModel.LegalStatusId);

                // Direction
                p.Direction = _propertyService.GetDirection(createModel.DirectionId);

                // Location
                p.Location = _propertyService.GetLocation(createModel.LocationCssClass);

                #endregion

                #region Alley

                if (p.Location != null)
                {
                    if (p.Location.CssClass == "h-front")
                    {
                        p.StreetWidth = createModel.StreetWidth;

                        // Alley
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
                        p.StreetWidth = null;

                        // Alley
                        p.DistanceToStreet = createModel.DistanceToStreet;
                        p.AlleyTurns = createModel.AlleyTurns;
                        p.AlleyWidth1 = createModel.AlleyWidth1;
                        p.AlleyWidth2 = createModel.AlleyWidth2;
                        p.AlleyWidth3 = createModel.AlleyWidth3;
                        p.AlleyWidth4 = createModel.AlleyWidth4;
                        p.AlleyWidth5 = createModel.AlleyWidth5;
                        p.AlleyWidth6 = createModel.AlleyWidth6;
                        p.AlleyWidth7 = createModel.AlleyWidth7;
                        p.AlleyWidth8 = createModel.AlleyWidth8;
                        p.AlleyWidth9 = createModel.AlleyWidth9;
                        if (createModel.AlleyTurns > 0)
                        {
                            p.AlleyWidth =
                                new List<double?>
                                {
                                    createModel.AlleyWidth1,
                                    createModel.AlleyWidth2,
                                    createModel.AlleyWidth3,
                                    createModel.AlleyWidth4,
                                    createModel.AlleyWidth5,
                                    createModel.AlleyWidth6,
                                    createModel.AlleyWidth7,
                                    createModel.AlleyWidth8,
                                    createModel.AlleyWidth9
                                }[(int)createModel.AlleyTurns - 1];
                        }
                    }
                }

                #endregion

                #region Area

                // AreaTotal
                p.AreaTotal = createModel.AreaTotal;
                p.AreaTotalWidth = createModel.AreaTotalWidth;
                p.AreaTotalLength = createModel.AreaTotalLength;
                p.AreaTotalBackWidth = createModel.AreaTotalBackWidth;

                // AreaLegal
                p.AreaLegal = createModel.AreaLegal;
                p.AreaLegalWidth = createModel.AreaLegalWidth;
                p.AreaLegalLength = createModel.AreaLegalLength;
                p.AreaLegalBackWidth = createModel.AreaLegalBackWidth;
                p.AreaIlegalRecognized = createModel.AreaIlegalRecognized;
                p.AreaIlegalNotRecognized = createModel.AreaIlegalNotRecognized;

                // AreaResidential
                p.AreaResidential = createModel.AreaResidential;

                #endregion

                #region Construction

                if (p.Type.CssClass == "tp-residential-land")
                {
                    // Construction
                    p.AreaConstruction = null;
                    p.AreaConstructionFloor = null;
                    p.AreaUsable = null;

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

                    // Interior
                    p.Interior = null;
                    p.RemainingValue = null;
                    p.TypeConstruction = null;
                }
                else
                {
                    // Construction
                    p.AreaConstruction = createModel.AreaConstruction;
                    p.AreaConstructionFloor = createModel.AreaConstructionFloor;
                    p.AreaUsable = createModel.AreaUsable;

                    p.Floors = createModel.Floors;
                    p.Bedrooms = createModel.Bedrooms;
                    p.Livingrooms = createModel.Livingrooms;
                    p.Bathrooms = createModel.Bathrooms;
                    p.Balconies = createModel.Balconies;

                    p.HaveBasement = createModel.HaveBasement;
                    p.HaveElevator = createModel.HaveElevator;
                    p.HaveGarage = createModel.HaveGarage;
                    p.HaveGarden = createModel.HaveGarden;
                    p.HaveMezzanine = createModel.HaveMezzanine;
                    p.HaveSkylight = createModel.HaveSkylight;
                    p.HaveSwimmingPool = createModel.HaveSwimmingPool;
                    p.HaveTerrace = createModel.HaveTerrace;

                    // Interior
                    p.Interior = _propertyService.GetInterior(createModel.InteriorId);
                    p.RemainingValue = createModel.RemainingValue;
                    p.TypeConstruction = _propertyService.GetTypeConstruction(createModel.TypeConstructionId);
                }

                #endregion

                #region Apartment Info

                // Apartment Info
                p.ApartmentFloors = createModel.ApartmentFloors;
                p.ApartmentFloorTh = createModel.ApartmentFloorTh;
                p.ApartmentElevators = createModel.ApartmentElevators;
                p.ApartmentBasements = createModel.ApartmentBasements;

                #endregion

                #region OtherAdvantages & OtherDisAdvantages

                p.OtherAdvantages = createModel.OtherAdvantages;
                p.OtherAdvantagesDesc = createModel.OtherAdvantagesDesc;
                p.OtherDisAdvantages = createModel.OtherDisAdvantages;
                p.OtherDisAdvantagesDesc = createModel.OtherDisAdvantagesDesc;

                #endregion

                #region Contact

                // Contact
                p.ContactName = createModel.ContactName;
                p.ContactPhone = createModel.ContactPhone;
                p.ContactPhoneToDisplay = createModel.ContactPhoneToDisplay;
                p.ContactAddress = createModel.ContactAddress;
                p.ContactEmail = createModel.ContactEmail;

                #endregion

                #region PublishAddress && PublishContact

                p.PublishAddress = createModel.PublishAddress;
                p.PublishContact = createModel.PublishContact;

                #endregion

                #region IsOwner, NoBroker, IsAuction, IsHighlights, IsAuthenticatedInfo

                p.IsOwner = createModel.IsOwner;
                p.NoBroker = createModel.NoBroker;
                p.IsAuction = createModel.IsAuction;
                p.IsHighlights = createModel.IsHighlights;
                p.IsAuthenticatedInfo = createModel.IsAuthenticatedInfo;

                #endregion

                #region Price

                // Price
                p.PriceProposed = createModel.PriceProposed;
                p.PaymentMethod = _propertyService.GetPaymentMethod(createModel.PaymentMethodId);
                p.PaymentUnit = _propertyService.GetPaymentUnit(createModel.PaymentUnitId);

                p.PriceEstimatedByStaff = createModel.PriceEstimatedByStaff;

                p.PriceNegotiable = createModel.PriceNegotiable;

                #endregion

                #region Ads Content

                // Ads Content
                p.Title = createModel.Title;
                p.Content = HttpUtility.HtmlDecode(createModel.Content);
                p.Note = createModel.Note;

                #endregion

                #region AdsType

                // AdsType
                p.AdsType = _propertyService.GetAdsType(createModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : createModel.AdsTypeCssClass);

                // Published
                p.Published = createModel.Published;

                // AdsExpirationDate
                p.AdsExpirationDate = createModel.AdsExpirationDate;
                if (p.Published)
                    p.AdsExpirationDate = _propertyService.GetAddExpirationDate(createModel.AddAdsExpirationDate,
                        p.AdsExpirationDate);
                else p.AdsExpirationDate = null;

                // AdsVIP

                #region BĐS Đăng VIP

                if (createModel.AdsVIP)
                {
                    int adsVipSeqOrder = 0;
                    switch (createModel.AdsTypeVIPId)
                    {
                        case AdsTypeVIP.VIP1:
                            adsVipSeqOrder = 3;
                            break;
                        case AdsTypeVIP.VIP2:
                            adsVipSeqOrder = 2;
                            break;
                        case AdsTypeVIP.VIP3:
                            adsVipSeqOrder = 1;
                            break;
                    }

                    if (createModel.AdsVIPExpirationDate.HasValue)
                        p.AdsVIPExpirationDate = DateExtension.GetEndOfDate(createModel.AdsVIPExpirationDate.Value);
                    DateTime? adsVipExpirationDate =
                        _propertyService.GetAddExpirationDate(createModel.AddAdsVIPExpirationDate,
                            p.AdsVIPExpirationDate);

                    // Kiểm tra quyền đăng tin VIP
                    if (Services.Authorizer.Authorize(Permissions.ManageAddAdsPayment))
                    {
                        p.AdsVIP = true;
                        p.AdsVIPExpirationDate = adsVipExpirationDate;
                        p.SeqOrder = adsVipSeqOrder;
                    }
                    // Kiểm tra số tiền trong tài khoản
                    //else if (_paymentHistory.CheckIsHaveMoney(adsVipSeqOrder, null, 0, null,
                    //    (int) (adsVipExpirationDate.Value - DateTime.Now).TotalDays))
                    //{
                    //    p.AdsVIP = true;
                    //    p.AdsVIPExpirationDate = adsVipExpirationDate;
                    //    p.SeqOrder = adsVipSeqOrder;

                    //    // Update Payment History
                    //    _paymentHistory.UpdatePaymentHistoryAdmin(0, null, adsVipSeqOrder, createdUser,
                    //        (int) (adsVipExpirationDate.Value - DateTime.Now).TotalDays);
                    //}
                    // Không đủ tiền đăng tin VIP
                    else
                    {
                        Services.Notifier.Error(
                            T("Tài khoản bạn không có quyền để đăng tin VIP này."));
                    }
                }
                else
                {
                    // Hủy đăng tin VIP
                    p.AdsVIP = false;
                    p.AdsVIPExpirationDate = null;
                    p.SeqOrder = 0;
                }

                #endregion

                // AdsHighlight
                /*if (createModel.AdsHighlight) p.AdsHighlight = _propertyService.EnableAddAdsHighlight(belongGroup);
                p.AdsHighlightExpirationDate = createModel.AdsHighlightExpirationDate;
                if (p.AdsHighlight)
                    p.AdsHighlightExpirationDate =
                        _propertyService.GetAddExpirationDate(createModel.AddAdsHighlightExpirationDate,
                            p.AdsHighlightExpirationDate);
                else p.AdsHighlightExpirationDate = null;*/

                // AdsGoodDeal
                if (createModel.AdsGoodDeal) p.AdsGoodDeal = _propertyService.EnableAddAdsGoodDeal(belongGroup);
                p.AdsGoodDealExpirationDate = createModel.AdsGoodDealExpirationDate;
                if (p.AdsGoodDeal)
                    p.AdsGoodDealExpirationDate =
                        _propertyService.GetAddExpirationDate(createModel.AddAdsGoodDealExpirationDate,
                            p.AdsGoodDealExpirationDate);
                else p.AdsGoodDealExpirationDate = null;

                // Thời gian đăng tin AdsExpirationDate = MAX(AdsExpirationDate, AdsVIPExpirationDate, AdsHighlightExpirationDate, AdsGoodDealExpirationDate)
                p.AdsExpirationDate = (p.AdsVIPExpirationDate != null &&
                                       (p.AdsVIPExpirationDate > p.AdsExpirationDate || p.AdsExpirationDate == null))
                    ? p.AdsVIPExpirationDate
                    : p.AdsExpirationDate;
                p.AdsExpirationDate = (p.AdsHighlightExpirationDate != null &&
                                       (p.AdsHighlightExpirationDate > p.AdsExpirationDate ||
                                        p.AdsExpirationDate == null))
                    ? p.AdsHighlightExpirationDate
                    : p.AdsExpirationDate;
                p.AdsExpirationDate = (p.AdsGoodDealExpirationDate != null &&
                                       (p.AdsGoodDealExpirationDate > p.AdsExpirationDate || p.AdsExpirationDate == null))
                    ? p.AdsGoodDealExpirationDate
                    : p.AdsExpirationDate;

                #endregion

                #region Status, Flag

                // Status
                if (createModel.PriceNegotiable == false &&
                    (createModel.PriceProposed == null || createModel.PriceProposed <= 0))
                {
                    bool isExternalProperty = _propertyService.ExternalStatusCssClass().Contains(createModel.StatusCssClass);
                    if (!isExternalProperty)
                    {
                        // BĐS khách không bắt buộc Giá, BĐS nội bộ nếu không nhập Giá sẽ bị chuyển vào BĐS Định giá
                        p.Status = _propertyService.GetStatus("st-estimate");
                    }
                }
                else
                {
                    p.Status = _propertyService.GetStatus(createModel.StatusCssClass);
                }
                p.StatusChangedDate = createdDate;
                if (p.Status.CssClass == "st-sold") p.IsSoldByGroup = createModel.IsSoldByGroup;
                else p.IsSoldByGroup = false;

                // Flag
                p.Flag = _propertyService.GetFlag(createModel.FlagId);
                p.IsExcludeFromPriceEstimation = createModel.IsExcludeFromPriceEstimation;

                #endregion

                #region User

                // User
                p.CreatedDate = createdDate;
                p.CreatedUser = createdUser.Record;
                p.LastUpdatedDate = createdDate;
                p.LastUpdatedUser = createdUser.Record;
                p.FirstInfoFromUser = infoFromUser.Record;
                p.LastInfoFromUser = infoFromUser.Record;

                // UserGroup
                p.UserGroup = belongGroup;

                #endregion

                #endregion

                Services.ContentManager.Create(p);

                #region After Created

                // IdStr
                p.IdStr = p.Id.ToString(CultureInfo.InvariantCulture);

                // Tự động chuyển sang trạng thái RAO BÁN nếu đủ thông tin Địa chỉ, Hẻm, Diện tích, Giá
                if (p.Status.CssClass == "st-new" || p.Status.CssClass == "st-estimate")
                {
                    if (_propertyService.IsValid(p))
                        p.Status = _propertyService.GetStatus("st-selling");
                }

                if (p.Status.CssClass == "st-selling" && p.Published &&
                    (p.AdsExpirationDate == null || p.AdsExpirationDate < DateTime.Now))
                    p.AdsExpirationDate = DateTime.Now.AddDays(90);

                // Area for filter only
                p.Area = _propertyService.CalcAreaForFilter(p);

                // AreaUsable
                p.AreaUsable = _propertyService.CalcAreaUsable(p);

                // PriceProposedInVND
                p.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(p);

                // Advantages
                _propertyService.UpdatePropertyAdvantages(p, createModel.Advantages);

                // DisAdvantages
                _propertyService.UpdatePropertyDisAdvantages(p, createModel.DisAdvantages);

                // ApartmentAdvantages
                _propertyService.UpdatePropertyApartmentAdvantages(p, createModel.ApartmentAdvantages);

                // ApartmentInteriorAdvantages
                _propertyService.UpdatePropertyApartmentInteriorAdvantages(p, createModel.ApartmentInteriorAdvantages);

                // Log User Activity
                _revisionService.SaveUserActivityAddNewProperty(createdDate, createdUser, p);

                // Upload Images 
                _propertyService.UploadImages(files, p, false);

                //Upload Video
                p.YoutubeId = createModel.YoutubeId;

                Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật thành công.",
                    Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));

                // Save & Continue
                if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                {
                    return RedirectToAction("Edit", new { id = p.Id });
                }

                #region Update OrderBy Domain (UserGroup)

                if (belongGroup != null)
                    _propertyService.UpdateOrderByDomainGroup(p, belongGroup.Id);

                #endregion

                #endregion

                #region Update MapPart

                _mapService.UpdateMapPart(p.Id,
                    Convert.ToSingle(frmCollection["MapPart.Latitude"]),
                    Convert.ToSingle(frmCollection["MapPart.Longitude"]),
                    Convert.ToSingle(frmCollection["MapPart.PlanMapLatitude"]),
                    Convert.ToSingle(frmCollection["MapPart.PlanMapLongitude"]));

                #endregion

                #region Save Meta

                _propertyService.UpdateMetaDescriptionKeywords(p, true);

                #endregion

                #region Post to Facebook

                Session["PropertyAdminId"] = createModel.AcceptPostToFacebok ? p.Id : 0;

                #endregion

                #region Update PlacesArround

                _propertyService.UpdatePlacesAroundForProperty(p, Convert.ToSingle(frmCollection["MapPart.Latitude"]),
                    Convert.ToSingle(frmCollection["MapPart.Longitude"]));

                #endregion

                #region Create PropertyExchange

                if (createModel.AdsTypeCssClass == "ad-exchange")
                {
                    var propertyExchange = _propertyExchangeService.CreatePropertyExchange(p);

                    return RedirectToAction("Create", "CustomerAdmin", new { adsTypeId = 98465, pExchangeId = propertyExchange.Id });//98465: Cần mua
                }

                #endregion

                // Save & Continue
                if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinue"]))
                {
                    return RedirectToAction("Edit", new { p.Id, returnUrl = createModel.ReturnUrl });
                }

                // Estimate this property before estimate
                if (p.TypeGroup != null && p.TypeGroup.CssClass == "gp-house")
                {
                    // Clear Estimate Cache
                    ClearEstimateCache(p, false);
                    _propertyService.EstimateProperty(p.Id);
                }

                // Redirect
                return RedirectToAction("Create");
            }

            dynamic model = Services.ContentManager.UpdateEditor(p, this);

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Property.Create",
                    Model: _propertyService.BuildCreateViewModel(createModel), Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật thành công.",
                Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));

            return RedirectToAction("Index");
        }

        #endregion

        #region Edit

        public async Task<ActionResult> Edit(int id, string returnUrl)
        {
            #region Facebook

            if (Session["PropertyAdminId"] != null)
            {
                try
                {
                    var pId = (int)Session["PropertyAdminId"];
                    ActionPostFacebook(pId);
                }
                catch (Exception ex)
                {
                    Services.Notifier.Error(T("PostToYourFacebook Error: {0}", ex.Message));
                }
            }

            #endregion

            #region Clear Cache Action

            if (TempData["PropertyClearCacheId"] != null)
            {
                try
                {
                    var pId = (int)TempData["PropertyClearCacheId"];
                    //Task.Run(() => ClearCacheClientWhenUpdate(pId));
                    ClearCacheClientWhenUpdate(pId);
                }
                catch (Exception ex)
                {
                    Services.Notifier.Error(T("PostToYourFacebook Error: {0}", ex.Message));
                }
            }

            #endregion

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            #region GET RECORD

            DateTime startGetRecord = DateTime.Now;

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region SECURITY

            if (!_propertyService.EnableEditProperty(p, user))
                return RedirectToAction("View", new { p.Id, returnUrl }); // Redirect to View
            //return new HttpUnauthorizedResult("Not authorized to edit properties");

            #endregion

            if (_debugEdit)
                Services.Notifier.Information(T("GET RECORD {0}", (DateTime.Now - startGetRecord).TotalSeconds));

        
            #endregion

            //Kiểm tra xem có phải là bđs trao đổi ko
            //=> hiện link đến yêu cầu để chỉnh sửa
            //=> Xóa BĐS trao đổi

            #region BUILD MODEL

            DateTime startBuildModel = DateTime.Now;

            PropertyEditViewModel editModel = _propertyService.BuildEditViewModel(p);
            editModel.ReturnUrl = returnUrl;
            editModel.LastInfoFromUserId = Services.WorkContext.CurrentUser.Id; //Current User

            DateTime start = DateTime.Now;
            var propertyExchange = _propertyService.GetExchangePartRecordByPropertyId(id);
            editModel.PropertyExchange = propertyExchange;
            Services.Notifier.Information(T("GetExchangePartRecordByPropertyId: {0}", (DateTime.Now - start).TotalSeconds));

            editModel.IsPropertyExchange = propertyExchange != null;
            editModel.AdsTypeCssClass = propertyExchange != null ? "ad-exchange" : editModel.AdsTypeCssClass;

            if (_debugEdit)
                Services.Notifier.Information(T("BUILD MODEL {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            #endregion

            #region ESTIMATE

            if (p.TypeGroup != null && p.TypeGroup.CssClass == "gp-house")
            {
                DateTime startEstimate = DateTime.Now;
                PropertyEstimateEntry entry = await _propertyService.EstimateProperty(p.Id);
                p.PriceEstimatedInVND = entry.PriceEstimatedInVND;
                p.Flag = _propertyService.GetFlag(entry.FlagCssClass);
                if (_debugEdit) Services.Notifier.Information(T("ESTIMATE {0}", (DateTime.Now - startEstimate).TotalSeconds));

                #region BUILD DEBUG MODEL

                {
                    DateTime startBuildDebugModel = DateTime.Now;

                    string key = id.ToString(CultureInfo.InvariantCulture);

                    editModel.DebugAreaLegal = entry.DebugAreaLegal;
                    editModel.DebugFrontWidth = entry.DebugFrontWidth;
                    editModel.DebugBackWidth = entry.DebugBackWidth;
                    editModel.DebugLength = entry.DebugLength;

                    editModel.DebugAreaStandard = entry.DebugAreaStandard;
                    editModel.DebugAreaExcess = entry.DebugAreaExcess;

                    editModel.DebugAreaIlegalRecognized = entry.DebugAreaIlegalRecognized;
                    editModel.DebugAreaIlegalNotRecognized = entry.DebugAreaIlegalNotRecognized;

                    editModel.DebugAlleyUnitPrice = entry.DebugAlleyUnitPrice;
                    editModel.DebugAlleyCoeff = entry.DebugAlleyCoeff;
                    editModel.DebugLengthCoeff = entry.DebugLengthCoeff;
                    editModel.DebugWidthCoeff = entry.DebugWidthCoeff;

                    editModel.DebugAreaWidth = entry.DebugAreaWidth;

                    editModel.DebugPriceHouseEstimated = entry.DebugPriceHouseEstimated;
                    editModel.DebugPriceLandProposed = entry.DebugPriceLandProposed;
                    editModel.DebugPriceLandEstimated = entry.DebugPriceLandEstimated;

                    editModel.DebugPriceChangedInPercent = entry.DebugPriceChangedInPercent;

                    editModel.DebugUnitPrice = entry.DebugUnitPrice;
                    editModel.DebugUnitPriceEstimate = entry.DebugUnitPriceEstimate;
                    editModel.DebugUnitPriceOnStreet = entry.DebugUnitPriceOnStreet;

                    editModel.DebugPercent = entry.DebugPercent;
                    editModel.DebugPercentMsg = entry.DebugPercentMsg;

                    editModel.DebugEstimationMsg = entry.DebugEstimationMsg;
                    editModel.DebugEstimationList = entry.DebugEstimationList;

                    if (_debugEdit)
                        Services.Notifier.Information(T("BUILD DEBUG MODEL {0}",
                            (DateTime.Now - startBuildDebugModel).TotalSeconds));
                }

                #endregion

            }

            #endregion

            #region BUILD TEMPLATE

            DateTime startBuildTemplate = DateTime.Now;

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Property.Edit", Model: editModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(p);
            model.Content.Add(editor);

            if (_debugEdit)
                Services.Notifier.Information(T("BUILD TEMPLATE {0}", (DateTime.Now - startBuildTemplate).TotalSeconds));

            #endregion

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ValidateInput(false), ActionName("Edit")]
        public ActionResult EditPost(int id, HttpPostedFileBase file, FormCollection frmCollection)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region SECURITY

            if (!_propertyService.EnableEditProperty(p, user))
                return RedirectToAction("View", new { p.Id }); // Redirect to View
            //return new HttpUnauthorizedResult("Not authorized to edit properties");

            #endregion

            #region BUILD OLD-MODEL

            PropertyEditViewModel oldModel = _propertyService.BuildEditViewModel(p);
            DateTime oldLastUpdatedDate = p.LastUpdatedDate;
            UserPartRecord oldLastUpdatedUser = p.LastUpdatedUser;
            string oldAddress = p.DisplayForAddressForOwner;
            string oldLocationCssClass = oldModel.LocationCssClass;
            double? oldPriceProposed = p.PriceProposed;
            string oldContactName = p.ContactName;
            string oldContactPhone = p.ContactPhone;
            string oldNote = p.Note;

            bool oldAdsGoodDeal = p.AdsGoodDeal;
            DateTime? oldAdsGoodDealExpirationDate = p.AdsGoodDealExpirationDate;

            bool oldAdsVip = p.AdsVIP;
            DateTime? oldAdsVipExpirationDate = p.AdsVIPExpirationDate;
            int oldSeqOrder = p.SeqOrder;

            //bool oldAdsHighlight = p.AdsHighlight;
            //DateTime? oldAdsHighlightExpirationDate = p.AdsHighlightExpirationDate;

            string oldTypeGroupCssClass = p.TypeGroup != null ? p.TypeGroup.CssClass : "";

            #endregion

            dynamic model = Services.ContentManager.UpdateEditor(p, this);

            DateTime lastUpdatedDate = DateTime.Now;
            UserPart lastUpdatedUser = user;
            UserGroupPartRecord belongGroup = _groupService.GetBelongGroup(p.CreatedUser.Id);

            var editModel = new PropertyEditViewModel { Property = p };

            if (TryUpdateModel(editModel))
            {
                if (editModel.IsChanged)
                {
                    if (editModel.TypeGroupCssClass != oldTypeGroupCssClass)
                    {
                        // If change TypeGroup, skip validation for ONE

                        p.TypeGroup = _propertyService.GetTypeGroup(editModel.TypeGroupCssClass);
                        p.Type = _propertyService.GetType(editModel.TypeId);
                    }

                    #region VALIDATION

                    bool isExternalProperty = _propertyService.IsExternalProperty(p);

                    #region "gp-house"

                    if (editModel.TypeGroupCssClass == "gp-house")
                    {
                        #region Address

                        if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                        {
                            if (!isExternalProperty) // BĐS khách không bắt buộc Phường, Đường, Số nhà
                            {
                                #region Ward, Street

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
                                // Street
                                if (!editModel.ChkOtherStreetName && !editModel.StreetId.HasValue)
                                {
                                    AddModelError("StreetId", T("Vui lòng chọn tên đường."));
                                }
                                // OtherStreetName
                                if (editModel.ChkOtherStreetName && string.IsNullOrEmpty(editModel.OtherStreetName))
                                {
                                    AddModelError("OtherStreetName", T("Vui lòng nhập tên đường."));
                                }

                                #endregion

                                #region AddressNumber

                                if (Services.Authorizer.Authorize(Permissions.RequireAddressNumber))
                                {
                                    if (string.IsNullOrEmpty(editModel.AddressNumber))
                                    {
                                        var pType = _propertyService.GetType(editModel.TypeId);
                                        if (pType != null && pType.CssClass == "tp-residential-land")
                                        {
                                            // "đất thổ cư" không cần nhập số nhà
                                        }
                                        else
                                        {
                                            // Bắt buộc nhập số nhà (trừ "đất thổ cư" và "các loại đất khác")
                                            AddModelError("AddressNumber", T("Vui lòng nhập số nhà."));
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(editModel.AddressNumber))
                                    {
                                        if (editModel.WardId > 0 && editModel.StreetId > 0)
                                        {
                                            if (
                                                !_propertyService.VerifyPropertyUnicity(id, editModel.ProvinceId,
                                                    editModel.DistrictId, editModel.WardId, editModel.StreetId,
                                                    editModel.ApartmentId, editModel.AddressNumber,
                                                    editModel.AddressCorner, editModel.ApartmentNumber,
                                                    editModel.AdsTypeCssClass))
                                            {
                                                PropertyPart r = _propertyService.GetPropertyByAddress(id,
                                                    editModel.ProvinceId, editModel.DistrictId, editModel.WardId,
                                                    editModel.StreetId, editModel.ApartmentId, editModel.AddressNumber,
                                                    editModel.AddressCorner, editModel.ApartmentNumber,
                                                    editModel.AdsTypeCssClass);
                                                AddModelError("AddressNumber",
                                                    T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã có trong dữ liệu.",
                                                        Url.Action("Edit", new { r.Id }), r.Id,
                                                        r.DisplayForAddressForOwner));
                                                Services.Notifier.Warning(
                                                    T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã có trong dữ liệu.",
                                                        Url.Action("Edit", new { r.Id }), r.Id,
                                                        r.DisplayForAddressForOwner));
                                            }
                                        }
                                    }
                                }

                                #endregion
                            }
                        }

                        #endregion

                        #region Location

                        if (String.IsNullOrEmpty(editModel.LocationCssClass))
                        {
                            AddModelError("LocationCssClass", T("Vui lòng chọn vị trí bất động sản."));
                        }

                        #endregion

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

                        if (!isExternalProperty) // BĐS khách không bắt buộc Chiều ngang mặt hậu, DT hợp quy hoạch
                        {

                            // AreaLegal & AreaLegalWidth + AreaLegalLength
                            if (editModel.AreaLegalWidth > 0 || editModel.AreaLegalLength > 0)
                                if (!(editModel.AreaLegal > 0 ||
                                      (editModel.AreaLegalWidth > 0 && editModel.AreaLegalLength > 0)))
                                {
                                    AddModelError("AreaLegal",
                                        T("Vui lòng nhập Diện tích hợp quy hoạch HOẶC nhập Chiều ngang và Chiều dài hợp quy hoạch."));
                                    AddModelError("AreaLegalWidth", T(""));
                                    AddModelError("AreaLegalLength", T(""));
                                }

                        }

                        #endregion

                        #region Area logic

                        double areaTotal = _propertyService.CalcArea(editModel.AreaTotal, editModel.AreaTotalWidth,
                            editModel.AreaTotalLength, editModel.AreaTotalBackWidth);

                        if (editModel.AreaLegal > 0
                            || (editModel.AreaLegalWidth > 0 && editModel.AreaLegalLength > 0)
                            || editModel.AreaIlegalRecognized > 0)
                        {
                            double areaLegal = _propertyService.CalcArea(editModel.AreaLegal, editModel.AreaLegalWidth,
                                editModel.AreaLegalLength, editModel.AreaLegalBackWidth);

                            // DT hợp quy hoạch phải nhỏ hơn DTKV
                            if (areaLegal > areaTotal)
                            {
                                // AreaTotal
                                if (editModel.AreaTotal.HasValue)
                                    AddModelError("AreaTotal",
                                        T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                                else
                                {
                                    AddModelError("AreaTotalWidth",
                                        T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                                    AddModelError("AreaTotalLength", T(""));
                                }
                                // AreaLegal
                                if (editModel.AreaLegal.HasValue)
                                    AddModelError("AreaLegal", T(""));
                                else
                                {
                                    AddModelError("AreaLegalWidth", T(""));
                                    AddModelError("AreaLegalLength", T(""));
                                }
                            }

                            // AreaIlegalRecognized
                            if (editModel.AreaIlegalRecognized > 0)
                            {
                                if ((areaTotal - areaLegal + 0.1) < editModel.AreaIlegalRecognized)
                                {
                                    AddModelError("AreaIlegalRecognized",
                                        T(
                                            "Diện tích đất vi phạm lộ giới (quy hoạch) phải nhỏ hơn (Diện tích khuôn viên - Diện tích hợp quy hoạch)."));
                                }
                            }
                        }

                        // AreaConstruction
                        if (editModel.AreaConstruction > 0)
                        {
                            if (areaTotal < editModel.AreaConstruction)
                            {
                                AddModelError("AreaConstruction",
                                    T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                            }
                        }

                        #endregion

                        #region Status RAO BÁN

                        if (editModel.StatusCssClass == "st-selling")
                        {

                            // AreaLegal & AreaLegalWidth + AreaLegalLength
                            if (
                                !(editModel.AreaLegal > 0 ||
                                  (editModel.AreaLegalWidth > 0 && editModel.AreaLegalLength > 0)))
                            {
                                AddModelError("AreaLegal",
                                    T("Vui lòng nhập Diện tích hợp quy hoạch HOẶC nhập Chiều ngang và Chiều dài hợp quy hoạch."));
                                AddModelError("AreaLegalWidth", T(""));
                                AddModelError("AreaLegalLength", T(""));
                            }

                            // Location
                            if (editModel.LocationCssClass != "h-front")
                            {
                                if (!editModel.AlleyTurns.HasValue ||
                                    !(editModel.AlleyWidth1.HasValue || editModel.AlleyWidth2.HasValue ||
                                      editModel.AlleyWidth3.HasValue || editModel.AlleyWidth4.HasValue ||
                                      editModel.AlleyWidth5.HasValue || editModel.AlleyWidth6.HasValue ||
                                      editModel.AlleyWidth7.HasValue || editModel.AlleyWidth8.HasValue ||
                                      editModel.AlleyWidth9.HasValue))
                                {
                                    // chưa có thông tin Hẻm
                                    AddModelError("AlleyTurns", T("Vui lòng nhập thông tin Số lần rẽ & Độ rộng hẻm"));
                                }
                            }
                        }

                        #endregion
                    }
                    #endregion

                    #region "gp-apartment"

                    else if (editModel.TypeGroupCssClass == "gp-apartment")
                    {
                        #region Apartment

                        //if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                        //{
                        //    if (!isExternalProperty) // BĐS khách không bắt buộc Tên dự án / chung cư
                        //    {
                        //        // Apartment
                        //        if (!editModel.ChkOtherProjectName && !editModel.ApartmentId.HasValue)
                        //        {
                        //            AddModelError("ApartmentId", T("Vui lòng chọn Tên dự án / chung cư."));
                        //        }
                        //        // OtherProjectName
                        //        if (editModel.ChkOtherProjectName && string.IsNullOrEmpty(editModel.OtherProjectName))
                        //        {
                        //            AddModelError("OtherProjectName", T("Vui lòng nhập Tên dự án / chung cư."));
                        //        }
                        //    }
                        //}

                        #endregion

                        #region AreaUsable

                        if (!editModel.AreaUsable.HasValue)
                        {
                            AddModelError("AreaUsable", T("Vui lòng nhập diện tích căn hộ."));
                        }

                        #endregion
                    }

                    #endregion

                    #region "gp-land"

                    else if (editModel.TypeGroupCssClass == "gp-land")
                    {
                        #region Address

                        if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                        {
                            if (!isExternalProperty) // BĐS khách không bắt buộc Phường, Đường, Số nhà
                            {
                                #region Ward, Street

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
                        }

                        #endregion

                        #region Location

                        if (String.IsNullOrEmpty(editModel.LocationCssClass))
                        {
                            AddModelError("LocationCssClass", T("Vui lòng chọn vị trí bất động sản."));
                        }

                        #endregion

                        #region AreaTotal, AreaResidential

                        // AreaTotal & AreaTotalWidth + AreaTotalLength
                        if (
                            !(editModel.AreaTotal.HasValue ||
                              (editModel.AreaTotalWidth.HasValue && editModel.AreaTotalLength.HasValue)))
                        {
                            AddModelError("AreaTotal",
                                T("Vui lòng nhập Diện tích khuôn viên HOẶC nhập Chiều ngang và Chiều dài khu đất."));
                            AddModelError("AreaTotalWidth", T(""));
                            AddModelError("AreaTotalLength", T(""));
                        }

                        double areaTotal = _propertyService.CalcArea(editModel.AreaTotal, editModel.AreaTotalWidth,
                            editModel.AreaTotalLength, editModel.AreaTotalBackWidth);

                        // AreaTotal & AreaResidential
                        if (editModel.AreaResidential > 0)
                        {
                            if (areaTotal < editModel.AreaResidential)
                            {
                                AddModelError("AreaResidential",
                                    T("Diện tích đất thổ cư phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                            }
                        }

                        // AreaTotal & AreaConstruction
                        if (editModel.AreaConstruction > 0)
                        {
                            if (areaTotal < editModel.AreaConstruction)
                            {
                                AddModelError("AreaConstruction",
                                    T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                            }
                        }

                        #endregion
                    }

                    #endregion

                    #region PriceProposed

                    if (priceRequiredStatus.Contains(editModel.StatusCssClass))
                    {
                        if (editModel.PriceNegotiable == false) // Giá thương lượng - không cần nhập Giá rao
                        {
                            if (editModel.PriceProposed == null || editModel.PriceProposed <= 0) // Giá rao phải > 0
                            {
                                AddModelError("PriceProposed", T("Vui lòng nhập giá rao bán / cho thuê."));
                            }
                        }
                    }

                    #endregion

                    #endregion

                    #region UPDATE RECORD

                    if (ModelState.IsValid)
                    {
                        #region UPDATE MODEL

                        UserPart lastInfoFromUser = _groupService.GetUser(editModel.LastInfoFromUserId);

                        // User
                        p.LastUpdatedDate = lastUpdatedDate;
                        p.LastUpdatedUser = lastUpdatedUser.Record;
                        p.LastInfoFromUser = lastInfoFromUser.Record;

                        // UserGroup
                        if (p.UserGroup == null) p.UserGroup = belongGroup;

                        #region Address

                        if (Services.Authorizer.Authorize(Permissions.EditAddressNumber))
                        {
                            // Province
                            p.Province = _addressService.GetProvince(editModel.ProvinceId);

                            // District
                            p.District = _addressService.GetDistrict(editModel.DistrictId);

                            // Ward
                            if (editModel.ChkOtherWardName && !String.IsNullOrEmpty(editModel.OtherWardName))
                            {
                                p.Ward = null;
                                p.OtherWardName = editModel.OtherWardName;
                            }
                            else
                            {
                                p.Ward = _addressService.GetWard(editModel.WardId);
                                p.OtherWardName = null;
                            }

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

                            // Street
                            if (editModel.ChkOtherStreetName && !String.IsNullOrEmpty(editModel.OtherStreetName))
                            {
                                p.Street = null;
                                p.OtherStreetName = editModel.OtherStreetName;
                            }
                            else if (editModel.StreetId > 0)
                            {
                                LocationStreetPartRecord selectedStreet =
                                    _addressService.GetStreet(editModel.StreetId);
                                p.Street = selectedStreet;
                                p.OtherStreetName = null;

                                // Street Segment
                                LocationStreetPartRecord segmentStreet = _addressService.GetStreet(selectedStreet,
                                    p.AlleyNumber);
                                if (segmentStreet != null)
                                    p.Street = segmentStreet;
                            }
                            else
                            {
                                p.Street = null;
                            }

                            // Apartment
                            if (editModel.ChkOtherProjectName && !String.IsNullOrEmpty(editModel.OtherProjectName))
                            {
                                p.Apartment = null;
                                p.OtherProjectName = editModel.OtherProjectName;
                            }
                            else
                            {
                                p.Apartment = _addressService.GetApartment(editModel.ApartmentId);
                                p.OtherProjectName = null;
                            }

                            p.ApartmentBlock = editModel.ApartmentBlockId != null
                                ? _apartmentService.LocationApartmentBlockPart(editModel.ApartmentBlockId.Value).Record
                                : p.ApartmentBlock;//nếu editModel.ApartmentBlockId = null => lấy giá trị cũ
                        }

                        #endregion

                        #region LegalStatus, Direction, Location

                        // LegalStatus
                        p.LegalStatus = _propertyService.GetLegalStatus(editModel.LegalStatusId);

                        // Direction
                        p.Direction = _propertyService.GetDirection(editModel.DirectionId);

                        // Location
                        p.Location = _propertyService.GetLocation(editModel.LocationCssClass);

                        #endregion

                        #region Alley

                        if (p.Location != null)
                        {
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
                                p.StreetWidth = null;
                            }
                        }

                        #endregion

                        #region Type

                        p.Type = _propertyService.GetType(editModel.TypeId);
                        p.TypeGroup = p.Type.Group;

                        if (p.Type.CssClass == "tp-residential-land")
                        {
                            p.AreaConstruction = null;
                            p.AreaConstructionFloor = null;
                            p.AreaUsable = null;

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

                        #endregion

                        #region Price

                        // PaymentMethod
                        p.PaymentMethod = _propertyService.GetPaymentMethod(editModel.PaymentMethodId);

                        // PaymentUnit
                        p.PaymentUnit = _propertyService.GetPaymentUnit(editModel.PaymentUnitId);

                        #endregion

                        #region Flag & Status

                        // Flag
                        p.Flag = _propertyService.GetFlag(editModel.FlagId);

                        // Status

                        bool allowChangeStatus = true;

                        if (editModel.StatusCssClass == "st-deleted")
                        {
                            if (!Services.Authorizer.Authorize(Permissions.DeleteOwnProperty))
                                allowChangeStatus = false;
                            if (p.CreatedUser.Id != user.Id)
                                if (!Services.Authorizer.Authorize(Permissions.DeleteProperty))
                                    allowChangeStatus = false;
                            if (!allowChangeStatus)
                                Services.Notifier.Error(T(
                                    "Bạn không thể xóa BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a>.",
                                    Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));
                        }

                        if (allowChangeStatus)
                        {
                            p.Status = _propertyService.GetStatus(editModel.StatusCssClass);

                            if (p.Status.CssClass == "st-sold") p.IsSoldByGroup = editModel.IsSoldByGroup;
                            else p.IsSoldByGroup = false;

                        }

                        #region Property VIP Approved

                        if (editModel.AdsVIPRequest && p.Status.CssClass == "st-approved")
                        {
                            _paymentHistory.ApprovedPaymentHistory(p);

                            Services.Notifier.Information(T("{0} đã duyệt tin VIP", p.Id));
                        }
                        else if (editModel.AdsVIPRequest && p.Status.CssClass == "st-invalid")
                        {
                            _paymentHistory.NotApprovedPaymentHistory(p);

                            Services.Notifier.Information(T("{0} Không hợp lệ tin VIP", p.Id));
                        }

                        #endregion

                        // Tự động chuyển sang trạng thái RAO BÁN nếu đủ thông tin Địa chỉ, Hẻm, Diện tích, Giá
                        if (p.Status.CssClass == "st-new" || p.Status.CssClass == "st-estimate")
                        {
                            if (_propertyService.IsValid(p))
                                p.Status = _propertyService.GetStatus("st-selling");
                        }

                        if (p.PriceNegotiable == false && (p.PriceProposed == null || p.PriceProposed <= 0))
                        {
                            if (!isExternalProperty)
                            // BĐS khách không bắt buộc Giá, BĐS nội bộ nếu không nhập Giá sẽ bị chuyển vào BĐS Định giá
                            {
                                p.Status = _propertyService.GetStatus("st-estimate");
                            }
                        }

                        // Hủy tin VIP đối với BĐS không hiện trên trang chủ
                        var allowStatusCssClass = new List<string>
                        {
                            "st-new",
                            "st-selling",
                            "st-negotiate",
                            "st-approved"
                        };
                        if ((p.AdsVIP && p.AdsVIPExpirationDate >= DateTime.Now || p.AdsVIPRequest) &&
                            !allowStatusCssClass.Contains(p.Status.CssClass))
                        {
                            p.AdsVIPRequest = false;
                            p.AdsVIP = false;
                            p.AdsVIPExpirationDate = null;
                            p.SeqOrder = 0;
                        }

                        #endregion

                        #region AdsType

                        // AdsType
                        p.AdsType = _propertyService.GetAdsType(editModel.AdsTypeCssClass == "ad-exchange" ? "ad-selling" : editModel.AdsTypeCssClass);

                        // AdsExpirationDate
                        if (p.Published)
                            p.AdsExpirationDate =
                                _propertyService.GetAddExpirationDate(editModel.AddAdsExpirationDate,
                                    p.AdsExpirationDate);
                        else p.AdsExpirationDate = null;

                        // AdsVIP

                        #region BĐS Đăng VIP

                        if (editModel.AdsVIP)
                        {
                            int adsVipSeqOrder = 0;
                            switch (editModel.AdsTypeVIPId)
                            {
                                case AdsTypeVIP.VIP1:
                                    adsVipSeqOrder = 3;
                                    break;
                                case AdsTypeVIP.VIP2:
                                    adsVipSeqOrder = 2;
                                    break;
                                case AdsTypeVIP.VIP3:
                                    adsVipSeqOrder = 1;
                                    break;
                            }

                            DateTime? adsVipExpirationDate =
                                _propertyService.GetAddExpirationDate(editModel.AddAdsVIPExpirationDate,
                                    p.AdsVIPExpirationDate);

                            if (
                                oldAdsVip == false // chuyển từ tin thường sang tin VIP
                                || (oldAdsVip && oldAdsVipExpirationDate < adsVipExpirationDate) // gia hạn tin VIP
                                || oldSeqOrder != adsVipSeqOrder // thay đổi loại tin VIP
                                )
                            {
                                // Kiểm tra quyền đăng tin VIP
                                if (Services.Authorizer.Authorize(Permissions.ManageAddAdsPayment))
                                {
                                    p.AdsVIP = true;
                                    p.AdsVIPExpirationDate = adsVipExpirationDate;
                                    p.SeqOrder = adsVipSeqOrder;
                                }
                                //// Kiểm tra số tiền trong tài khoản
                                //else if (_paymentHistory.CheckIsHaveMoney(adsVipSeqOrder, p, oldSeqOrder,
                                //    oldAdsVipExpirationDate,
                                //    (int) (adsVipExpirationDate.Value - DateTime.Now).TotalDays))
                                //{
                                //    p.AdsVIP = true;
                                //    p.AdsVIPExpirationDate = adsVipExpirationDate;
                                //    p.SeqOrder = adsVipSeqOrder;

                                //    //Update History Here
                                //    _paymentHistory.UpdatePaymentHistoryAdmin(oldSeqOrder, p, adsVipSeqOrder,
                                //        lastUpdatedUser, (int) (adsVipExpirationDate.Value - DateTime.Now).TotalDays);
                                //}
                                // Không đủ tiền đăng tin VIP
                                else
                                {
                                    Services.Notifier.Error(
                                        T("Tài khoản của bạn không có quyền hoặc không đủ tiền để đăng tin VIP"));
                                }
                            }
                        }
                        else
                        {
                            // Hủy đăng tin VIP
                            p.AdsVIP = false;
                            p.AdsVIPExpirationDate = null;
                            p.SeqOrder = 0;
                        }

                        p.IsRefresh = false;

                        #endregion

                        // AdsHighlight
                        /*if (oldAdsHighlight == false && editModel.AdsHighlight)
                        p.AdsHighlight = _propertyService.EnableAddAdsHighlight(p.UserGroup);
                    if (p.AdsHighlight)
                        p.AdsHighlightExpirationDate =
                            _propertyService.GetAddExpirationDate(editModel.AddAdsHighlightExpirationDate,
                                p.AdsHighlightExpirationDate);
                    else p.AdsHighlightExpirationDate = null;*/

                        // AdsGoodDeal
                        if (oldAdsGoodDeal == false && editModel.AdsGoodDeal)
                            p.AdsGoodDeal = _propertyService.EnableAddAdsGoodDeal(p.UserGroup);
                        if (p.AdsGoodDeal)
                            p.AdsGoodDealExpirationDate =
                                _propertyService.GetAddExpirationDate(editModel.AddAdsGoodDealExpirationDate,
                                    p.AdsGoodDealExpirationDate);
                        else p.AdsGoodDealExpirationDate = null;

                        // Thời gian đăng tin AdsExpirationDate = MAX(AdsExpirationDate, AdsVIPExpirationDate, AdsHighlightExpirationDate, AdsGoodDealExpirationDate)
                        p.AdsExpirationDate = (p.AdsVIPExpirationDate != null &&
                                                (p.AdsVIPExpirationDate > p.AdsExpirationDate ||
                                                p.AdsExpirationDate == null))
                            ? p.AdsVIPExpirationDate
                            : p.AdsExpirationDate;
                        /*p.AdsExpirationDate = (p.AdsHighlightExpirationDate != null &&
                                            (p.AdsHighlightExpirationDate > p.AdsExpirationDate ||
                                            p.AdsExpirationDate == null))
                        ? p.AdsHighlightExpirationDate
                        : p.AdsExpirationDate;*/
                        p.AdsExpirationDate = (p.AdsGoodDealExpirationDate != null &&
                                                (p.AdsGoodDealExpirationDate > p.AdsExpirationDate ||
                                                p.AdsExpirationDate == null))
                            ? p.AdsGoodDealExpirationDate
                            : p.AdsExpirationDate;

                        if (p.Status.CssClass == "st-selling" && p.Published &&
                            (p.AdsExpirationDate == null || p.AdsExpirationDate < DateTime.Now))
                            p.AdsExpirationDate = DateTime.Now.AddDays(90);

                        #endregion

                        // Area for filter only
                        p.Area = _propertyService.CalcAreaForFilter(p);

                        // AreaUsable
                        p.AreaUsable = _propertyService.CalcAreaUsable(p);

                        // PriceProposedInVND
                        p.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(p);

                        #region Advantages & DisAdvantages

                        // Advantages
                        _propertyService.UpdatePropertyAdvantages(p, editModel.Advantages);

                        // DisAdvantages
                        _propertyService.UpdatePropertyDisAdvantages(p, editModel.DisAdvantages);

                        // ApartmentAdvantages
                        _propertyService.UpdatePropertyApartmentAdvantages(p, editModel.ApartmentAdvantages);

                        // ApartmentInteriorAdvantages
                        _propertyService.UpdatePropertyApartmentInteriorAdvantages(p,
                            editModel.ApartmentInteriorAdvantages);

                        #endregion

                        #endregion

                        #region AdsRequest

                        // AdsHighlightRequest
                        /*if (p.AdsHighlightRequest)
                        {
                            if (p.AdsHighlight && oldAdsHighlightExpirationDate != p.AdsHighlightExpirationDate)
                            {
                                p.AdsHighlightRequest = false;
                                if (isExternalProperty)
                                {
                                    p.Status = _propertyService.GetStatus("st-approved");
                                    p.StatusChangedDate = DateTime.Now;
                                }
                            }
                        }*/

                        // AdsGoodDealRequest
                        if (p.AdsGoodDealRequest)
                        {
                            if (p.AdsGoodDeal && oldAdsGoodDealExpirationDate != p.AdsGoodDealExpirationDate)
                            {
                                p.AdsGoodDealRequest = false;
                                if (isExternalProperty)
                                {
                                    p.Status = _propertyService.GetStatus("st-approved");
                                    p.StatusChangedDate = DateTime.Now;
                                }
                            }
                        }

                        // AdsVIPRequest
                        if (p.AdsVIPRequest)
                        {
                            if (p.AdsVIP && oldAdsVipExpirationDate != p.AdsVIPExpirationDate)
                            {
                                p.AdsVIPRequest = false;
                                if (isExternalProperty)
                                {
                                    p.Status = _propertyService.GetStatus("st-approved");
                                    p.StatusChangedDate = DateTime.Now;
                                }
                            }
                        }

                        #endregion

                        #region Save Revision

                        bool isChanged = false;

                        string newAddress = p.DisplayForAddressForOwner;

                        if (oldModel.StatusCssClass != editModel.StatusCssClass)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "StatusCssClass",
                                oldModel.StatusCssClass, editModel.StatusCssClass);
                            p.StatusChangedDate = DateTime.Now;
                            isChanged = true;
                        }

                        if (oldPriceProposed != editModel.PriceProposed ||
                            oldModel.PaymentMethodId != editModel.PaymentMethodId ||
                            oldModel.PaymentUnitId != editModel.PaymentUnitId)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record,
                                "PriceProposed", oldPriceProposed, editModel.PriceProposed);
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record,
                                "PaymentMethodId", oldModel.PaymentMethodId, editModel.PaymentMethodId);
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record,
                                "PaymentUnitId", oldModel.PaymentUnitId, editModel.PaymentUnitId);
                            isChanged = true;
                        }

                        if (oldAddress != newAddress)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "Address",
                                oldAddress, newAddress);
                            isChanged = true;
                        }

                        if (oldContactName != editModel.ContactName)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record,
                                "ContactName", oldContactName, editModel.ContactName);
                            isChanged = true;
                        }

                        if (oldContactPhone != editModel.ContactPhone)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record,
                                "ContactPhone", oldContactPhone, editModel.ContactPhone);
                            isChanged = true;
                        }

                        if (oldNote != editModel.Note)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record, "Note",
                                oldNote, editModel.Note);
                            isChanged = true;
                        }

                        if (oldModel.LastInfoFromUserId != editModel.LastInfoFromUserId)
                        {
                            _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, p.Record,
                                "LastInfoFromUserId", oldModel.LastInfoFromUserId, editModel.LastInfoFromUserId);
                            isChanged = true;
                        }

                        // Log User Activity
                        if (isChanged)
                            _revisionService.SaveUserActivityUpdateProperty(lastUpdatedDate, lastUpdatedUser, p);

                        #endregion

                        #region Save Meta

                        _propertyService.UpdateMetaDescriptionKeywords(p, editModel.UpdateMeta);

                        #endregion

                        #region Update PlacesArround

                        _propertyService.UpdatePlacesAroundForProperty(p,
                            Convert.ToSingle(frmCollection["MapPart.Latitude"]),
                            Convert.ToSingle(frmCollection["MapPart.Longitude"]));
                        //_propertyService.UpdatePlacesAroundForProperty(p);

                        #endregion

                        #region Post to Facebook

                        Session["PropertyAdminId"] = editModel.AcceptPostToFacebok ? p.Id : 0;

                        #endregion

                        #region Clear Cache

                        TempData["PropertyClearCacheId"] = p.Id;

                        #endregion

                        #region PropertyExchange

                        if (editModel.AdsTypeCssClass == "ad-exchange")
                        {
                            if (!editModel.IsPropertyExchange)
                            {
                                //Thêm mới bđs trao đổi
                                var propertyExchange = _propertyExchangeService.CreatePropertyExchange(p);

                                Services.Notifier.Information(T("Thông tin BĐS trao đổi <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật thành công. Vui lòng thêm thông tin yêu cầu bđs muốn nhận!",
                                    Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));

                                // Redirect
                                if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinueRequirement"]))
                                {
                                    return RedirectToAction("Create", "CustomerAdmin", new { adsTypeId = 98465, pExchangeId = propertyExchange.Id });//98465: Cần mua
                                }
                            }
                            // Redirect
                            if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinueRequirement"]))
                            {
                                var propertyExchange = _propertyService.GetExchangePartRecordByPropertyId(id);
                                return RedirectToAction("Edit", "CustomerAdmin", new { id = propertyExchange.Customer.Id, pExchangeId = propertyExchange.Id });//98465: Cần mua
                            }
                        }
                        else
                        {
                            if (editModel.IsPropertyExchange)
                            {
                                //Xóa BĐS trao đổi đi
                                _propertyExchangeService.DeletePropertyExchange(p, true);
                                Services.Notifier.Information(T("Tin rao <a href='{0}'>{1}</a> đã xóa khỏi BĐS trao đổi!", Url.Action("Edit", new { p.Id }), p.DisplayForAddressForOwner));
                            }
                        }

                        #endregion

                        // Clear Estimate Cache
                        ClearEstimateCache(p, (oldLocationCssClass != editModel.LocationCssClass));                        
                    }

                    #endregion
                }
            }

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = _propertyService.BuildEditViewModel(p);
                editModel.PropertyExchange = _propertyService.GetExchangePartRecordByPropertyId(id);


                dynamic editor = Shape.EditorTemplate(
                    TemplateName: "Parts/Property.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            // Copy
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveCopy"]))
            {
                Copy(id);
                return RedirectToAction("Edit", new { id, returnUrl = editModel.ReturnUrl });
            }

            // CopyToAdsType
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveCopyToAdsType"]))
            {
                if (editModel.PriceProposedCopy > 0 && editModel.PaymentMethodIdCopy > 0 &&
                    editModel.PaymentUnitIdCopy > 0)
                {
                    PropertyPart clone = _propertyService.CopyToAdsType(p, editModel.AdsTypeCssClassCopy,
                        editModel.PublishedCopy, (double)editModel.PriceProposedCopy, editModel.PaymentMethodIdCopy,
                        editModel.PaymentUnitIdCopy);
                    if (clone != null && clone.Id > 0 && clone.Id != id)
                    {
                        Services.Notifier.Information(
                            T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> copy sang {3} thành công.",
                                Url.Action("Edit", new { clone.Id }), clone.Id, clone.DisplayForAddressForOwner,
                                clone.AdsType.Name));
                    }
                    else
                    {
                        Services.Notifier.Error(T("Copy failed"));
                    }
                    return RedirectToAction("Edit", new { id, returnUrl = editModel.ReturnUrl });
                }
            }

            // UPDATE DATE
            if (!editModel.IsChanged)
            {
                //Services.TransactionManager.Cancel();
                p.LastUpdatedDate = lastUpdatedDate;
                p.LastUpdatedUser = lastUpdatedUser.Record;
                if (p.Status.CssClass == "st-selling" && p.Published &&
                    (p.AdsExpirationDate == null || p.AdsExpirationDate < DateTime.Now))
                    p.AdsExpirationDate = DateTime.Now.AddDays(90);
                Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật ngày thành công.",
                    Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));
            }
            else
            {
                Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> cập nhật thành công.",
                    Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));                
            }

            // Estimate 
            if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
            {
                // Clear Estimate Cache
                ClearEstimateCache(p, (oldLocationCssClass != editModel.LocationCssClass));
                return RedirectToAction("Edit", new { id, returnUrl = editModel.ReturnUrl });
            }

            // Save & Continue
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveContinue"]))
            {
                return RedirectToAction("Edit", new { id, returnUrl = editModel.ReturnUrl });
            }

            #region ESTIMATE

            if (p.TypeGroup != null && p.TypeGroup.CssClass == "gp-house")
            {
                _propertyService.EstimateProperty(p.Id);
            }

            #endregion

            // Back to returnUrl
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        public void CorrectAdsGoodDeal(int propertyId)
        {
            var p = Services.ContentManager.Get<PropertyPart>(propertyId);

            // Tự động correct BĐS Giá rẻ hiện trên trang chủ
            if (p.AdsType.CssClass != "ad-selling") return;
            if (p.Flag.CssClass == "deal-unknow")
            {
                if (p.PriceEstimatedByStaff > 0 && p.PriceProposedInVND > 0 &&
                    p.PriceEstimatedByStaff > p.PriceProposedInVND)
                {
                    // BĐS có thể quảng cáo vào BĐS giá rẻ
                }
                else
                {
                    p.AdsGoodDeal = false;
                    p.AdsGoodDealExpirationDate = null;
                    Services.Notifier.Warning(
                        T(
                            "BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> không thể đưa vào BĐS giá rẻ do giá rao bán cao hơn giá định giá của nhân viên.",
                            Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));
                }
            }
            else if (p.Flag.CssClass != "deal-good" && p.Flag.CssClass != "deal-very-good")
            {
                p.AdsGoodDeal = false;
                p.AdsGoodDealExpirationDate = null;
                Services.Notifier.Warning(
                    T(
                        "BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> không thể đưa vào BĐS giá rẻ do kết quả Định giá tự động không cho phép.",
                        Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));
            }
        }

        public void ClearEstimateCache(PropertyPart p, bool locationChanged)
        {
            // Clear UnitPrice in Cache
            _propertyService.ClearApplicationCache(p.Id);
        }

        #endregion

        #region View

        public ActionResult View(int id, string returnUrl)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region SECURITY

            if (!_propertyService.EnableViewProperty(p, user))
            {
                if (Services.Authorizer.Authorize(StandardPermissions.AccessAdminPanel))
                {
                    Services.Notifier.Error(T("Not authorized to view this property {0}", p.Id));
                    return RedirectToAction("Index"); // Redirect to Index
                }
                else
                {
                    Services.Notifier.Error(T("Not authorized to view this property {0}", p.Id));
                    return new HttpUnauthorizedResult("Not authorized to view this property");
                }
            }

            #endregion

            PropertyViewModel viewModel = _propertyService.BuildViewModel(p);
            viewModel.ReturnUrl = returnUrl;

            return View(viewModel);
        }

        [HttpPost, ActionName("View")]
        public ActionResult ViewPost(int id, string returnUrl, FormCollection frmCollection)
        {
            // Copy
            if (!string.IsNullOrEmpty(frmCollection["submit.SaveCopy"]))
            {
                Copy(id);
            }
            return RedirectToAction("View", new { id, returnUrl });
        }

        #endregion

        #region Ajax Details

        public ActionResult Details(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            var model = new List<PropertyRevisionEntry>();

            IEnumerable<RevisionPart> revisions = _revisionService.GetPropertyRevisions(id).ToList();
            List<DateTime> dates = revisions.Select(a => a.CreatedDate).Distinct().ToList();

            foreach (DateTime date in dates)
            {
                var entry = new PropertyRevisionEntry();
                DateTime date1 = date;
                IEnumerable<RevisionPart> records = revisions.Where(a => a.CreatedDate == date1);

                #region GET DATA

                foreach (RevisionPart item in records)
                {
                    entry.CreatedDate = item.CreatedDate;
                    entry.CreatedUser = item.CreatedUser;

                    switch (item.FieldName)
                    {
                        case "StatusId":
                            int statusId = int.Parse(item.ValueBefore);
                            entry.StatusName = _propertyService.GetStatus(statusId).Name;
                            break;

                        case "StatusCssClass":
                            entry.StatusName = _propertyService.GetStatus(item.ValueBefore).Name;
                            break;

                        case "PriceProposed":
                            entry.PriceProposed = item.ValueBefore;
                            break;

                        case "PaymentMethodId":
                            int paymentMethodId = int.Parse(item.ValueBefore);
                            entry.PaymentMethodName = _propertyService.GetPaymentMethod(paymentMethodId).ShortName;
                            break;

                        case "PaymentUnitId":
                            int paymentUnitId = int.Parse(item.ValueBefore);
                            entry.PaymentUnitName = _propertyService.GetPaymentUnit(paymentUnitId).Name;
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
                            int lastInfoFromUserId = int.Parse(item.ValueBefore);
                            entry.LastInfoFromUserName = _groupService.GetUser(lastInfoFromUserId).UserName;
                            break;

                        case "Delete Image":
                        case "Add Image":
                            PropertyFilePart file = _propertyService.GetPropertyFile(id, item.ValueAfter);
                            if (!String.IsNullOrEmpty(item.ValueAfter) && file != null)
                            {
                                entry.ImageId = file.Id;
                                entry.ImageUrl = file.Path;
                                entry.ImageName = file.Name;
                                entry.ImagePublished = file.Published;
                                entry.ImageIsAvatar = file.IsAvatar;
                                entry.ImageIsDeleted = file.IsDeleted;
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

            // ReSharper disable once Mvc.PartialViewNotResolved
            return PartialView(new PropertyRevisionsViewModel { Property = p, Revisions = model });
        }

        #endregion

        #region BULK ACTION

        #region Estimation

        [HttpPost]
        public ActionResult AddToEstimation(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Not authorized to edit properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsExcludeFromPriceEstimation = false;
            Services.Notifier.Information(T("BĐS {0} --> BĐS dùng định giá", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveFromEstimation(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Not authorized to edit properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsExcludeFromPriceEstimation = true;
            Services.Notifier.Information(T("BĐS {0} --> BĐS loại khỏi định giá", p.Id));

            return RedirectToAction("Index");
        }

        #endregion

        #region Listing, Trash, Delete

        [HttpPost]
        public ActionResult Listing(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Not authorized to edit properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Status = _propertyService.GetStatus("st-selling");
            p.StatusChangedDate = DateTime.Now;
            Services.Notifier.Information(T("BĐS {0} --> BĐS đủ thông tin", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Trash(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Not authorized to edit properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Not authorized to edit properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Status = _propertyService.GetStatus("st-trash");
            p.StatusChangedDate = DateTime.Now;
            Services.Notifier.Information(T("BĐS {0} --> chờ xóa", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.DeleteOwnProperty, T("Not authorized to delete properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (
                    !Services.Authorizer.Authorize(Permissions.DeleteProperty,
                        T("Not authorized to delete properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Status = _propertyService.GetStatus("st-deleted");
            p.StatusChangedDate = DateTime.Now;
            Services.Notifier.Information(T("BĐS {0} --> đã xóa", p.Id));

            return RedirectToAction("Index");
        }

        #endregion

        #region Refresh, Approve, NotApprove

        [HttpPost]
        public ActionResult Refresh(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");
            p.AdsExpirationDate = DateTime.Now.AddDays(90);
            Services.Notifier.Information(T("BĐS {0} đã được làm mới", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Approve(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ApproveOwnProperty, T("Not authorized to approve properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (
                    !Services.Authorizer.Authorize(Permissions.ApproveProperty,
                        T("Not authorized to approve properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Status = _propertyService.GetStatus("st-approved");
            p.StatusChangedDate = DateTime.Now;

            if (p.AdsVIPRequest)
            {
                p.AdsVIP = true;
                _paymentHistory.ApprovedPaymentHistory(p);

                Services.Notifier.Information(T("BĐS {0} đã được duyệt tin VIP {1}", p.Id,
                    p.SeqOrder == 1 ? 3 : p.SeqOrder == 3 ? 1 : p.SeqOrder));
            }

            Services.Notifier.Information(T("BĐS {0} đã được duyệt", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult NotApprove(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ApproveOwnProperty, T("Not authorized to approve properties")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (
                    !Services.Authorizer.Authorize(Permissions.ApproveProperty,
                        T("Not authorized to approve properties")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Status = _propertyService.GetStatus("st-invalid");
            p.StatusChangedDate = DateTime.Now;

            if (p.AdsVIPRequest)
            {
                _paymentHistory.NotApprovedPaymentHistory(p);
            }
            Services.Notifier.Information(T("BĐS {0} không hợp lệ", p.Id));

            return RedirectToAction("Index");
        }

        #endregion

        #region Copy

        [HttpPost]
        public ActionResult Copy(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.CopyOwnProperty, T("Not authorized to copy properties")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyPart>(id);

            if (record != null)
            {
                #region SECURITY

                if (record.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.CopyProperty, T("Not authorized to copy properties")))
                        return new HttpUnauthorizedResult();

                #endregion

                bool isValidForCopyToGroup = _propertyService.IsValidForCopyToGroup(record);
                bool isValidToPublish = _propertyService.IsValidToPublish(record);

                if (isValidForCopyToGroup && isValidToPublish)
                {
                    PropertyPart clone = _propertyService.Copy(record);

                    Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã sao chép thành công.",
                        Url.Action("Edit", new { clone.Id }), clone.Id, clone.DisplayForAddressForOwner));
                }
                else
                {
                    if (!isValidForCopyToGroup)
                    {
                        Services.Notifier.Error(
                        T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> không thể sao chép do đã có trong dữ liệu.",
                            Url.Action("Edit", new { record.Id }), record.Id, record.DisplayForAddressForOwner));
                    }
                    if (!isValidToPublish)
                    {
                        Services.Notifier.Error(
                        T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> không thể sao chép do không đủ thông tin.",
                            Url.Action("Edit", new { record.Id }), record.Id, record.DisplayForAddressForOwner));
                    }
                }
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Publish, PublishAddress, PublishContact

        // Cho phép tin rao hiện trên trang chủ
        [HttpPost]
        public ActionResult Publish(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.PublishOwnProperty,
                    T("Không có quyền đưa BĐS ra trang chính")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (
                    !Services.Authorizer.Authorize(Permissions.PublishProperty,
                        T("Không có quyền đưa BĐS ra trang chính")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Published = true;
            p.AdsExpirationDate = DateTime.Now.AddDays(90);

            Services.Notifier.Information(T("BĐS {0} --> đăng lên trang chính", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnPublish(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.PublishOwnProperty,
                    T("Không có quyền ngừng đăng BĐS trên trang chính")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (
                    !Services.Authorizer.Authorize(Permissions.PublishProperty,
                        T("Không có quyền ngừng đăng BĐS trên trang chính")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.Published = false;
            Services.Notifier.Information(T("BĐS {0} --> ngừng đăng trên trang chính", p.Id));

            return RedirectToAction("Index");
        }

        // Cho phép hiện địa chỉ
        [HttpPost]
        public ActionResult PublishAddress(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            if (p == null) return RedirectToAction("Index");
            p.PublishAddress = true;
            Services.Notifier.Information(T("BĐS {0} --> Cho phép hiện địa chỉ", p.Id));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnPublishAddress(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            if (p == null) return RedirectToAction("Index");
            p.PublishAddress = false;
            Services.Notifier.Information(T("BĐS {0} --> Dấu địa chỉ", p.Id));
            return RedirectToAction("Index");
        }

        // Cho phép hiện liên hệ
        [HttpPost]
        public ActionResult PublishContact(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            if (p == null) return RedirectToAction("Index");
            p.PublishContact = true;
            Services.Notifier.Information(T("BĐS {0} --> Cho phép hiện liên hệ", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnPublishContact(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            if (p == null) return RedirectToAction("Index");
            p.PublishContact = false;
            Services.Notifier.Information(T("BĐS {0} --> Dấu liên hệ", p.Id));
            return RedirectToAction("Index");
        }

        #endregion

        #region AdsGoodDeal, AdsVIP, AdsHighlight

        [HttpPost]
        public ActionResult AddToAdsGoodDeal(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsGoodDeal, T("Không có quyền đưa BĐS vào BĐS giá rẻ")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            if (p.AdsType.CssClass == "ad-selling")
            {
                if (p.Flag.CssClass != "deal-good" && p.Flag.CssClass != "deal-very-good")
                {
                    p.AdsGoodDeal = false;
                    p.AdsGoodDealExpirationDate = null;

                    Services.Notifier.Error(T("BĐS {0} không thể đưa vào BĐS giá rẻ", p.Id));
                    return RedirectToAction("Index");
                }
            }

            if (p.AdsGoodDeal == false) p.AdsGoodDeal = _propertyService.EnableAddAdsGoodDeal(p.UserGroup);
            if (p.AdsGoodDeal)
            {
                p.AdsGoodDealExpirationDate = DateTime.Now.AddDays(90);
                p.Published = true;
                p.AdsGoodDealRequest = false;
                if (_propertyService.IsExternalProperty(p))
                {
                    p.Status = _propertyService.GetStatus("st-approved");
                    p.StatusChangedDate = DateTime.Now;
                }
                if (p.AdsExpirationDate < p.AdsGoodDealExpirationDate)
                    p.AdsExpirationDate = p.AdsGoodDealExpirationDate;
                Services.Notifier.Information(T("BĐS {0} --> BĐS giá rẻ", p.Id));
            }
            else
            {
                p.AdsGoodDealExpirationDate = null;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveAdsGoodDeal(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsGoodDeal, T("Không có quyền loại BĐS khỏi BĐS giá rẻ")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            p.AdsGoodDeal = false;
            p.AdsGoodDealExpirationDate = null;
            Services.Notifier.Information(T("BĐS {0} --> loại khỏi BĐS giá rẻ", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddToAdsVip(int id, int seqOrder)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsVIP, T("Không có quyền đưa BĐS vào BĐS VIP")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            p.AdsVIPRequest = false;

            p.AdsVIP = true;
            p.AdsVIPExpirationDate = p.AdsVIPExpirationDate.HasValue
                ? p.AdsVIPExpirationDate
                : DateTime.Now.AddDays(30);
            p.SeqOrder = seqOrder;

            p.Published = true;
            if (p.AdsExpirationDate < p.AdsVIPExpirationDate) p.AdsExpirationDate = p.AdsVIPExpirationDate;

            if (_propertyService.IsExternalProperty(p))
            {
                p.Status = _propertyService.GetStatus("st-approved");
                p.StatusChangedDate = DateTime.Now;
            }

            //
            _paymentHistory.ApprovedPaymentHistory(p);

            Services.Notifier.Information(T("BĐS {0} --> BĐS VIP {1}", p.Id,
                seqOrder == 1 ? 3 : seqOrder == 3 ? 1 : seqOrder));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveAdsVip(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsVIP, T("Không có quyền loại BĐS khỏi BĐS VIP")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            p.AdsVIPRequest = false;

            p.AdsVIP = false;
            p.AdsVIPExpirationDate = null;
            p.SeqOrder = 0;

            Services.Notifier.Information(T("BĐS {0} --> loại khỏi BĐS VIP", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddToAdsHighlight(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsHighlight, T("Không có quyền đưa BĐS vào BĐS nổi bật")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            bool enableAddAdsHighlight = _propertyService.EnableAddAdsHighlight(p.UserGroup);
            if (enableAddAdsHighlight && p.AdsHighlight != true)
            {
                p.AdsHighlight = true;
            }

            // Update ExpirationDate if property is already AdsHighlight
            if (p.AdsHighlight)
            {
                p.AdsHighlightExpirationDate = DateTime.Now.AddDays(90);
                p.Published = true;
                p.AdsHighlightRequest = false;
                if (_propertyService.IsExternalProperty(p))
                {
                    p.Status = _propertyService.GetStatus("st-approved");
                    p.StatusChangedDate = DateTime.Now;
                }
                if (p.AdsExpirationDate < p.AdsHighlightExpirationDate)
                    p.AdsExpirationDate = p.AdsHighlightExpirationDate;
                Services.Notifier.Information(T("BĐS {0} --> BĐS nổi bật", p.Id));
            }
            else
            {
                p.AdsHighlightExpirationDate = null;
                // Notifier
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RemoveAdsHighlight(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.SetAdsHighlight,
                    T("Không có quyền loại BĐS khỏi BĐS nổi bật")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            p.AdsHighlight = false;
            p.AdsHighlightExpirationDate = null;
            Services.Notifier.Information(T("BĐS {0} --> loại khỏi BĐS nổi bật", p.Id));

            return RedirectToAction("Index");
        }

        #endregion

        #region IsOwner, NoBroker, IsAuction, IsAuthenticatedInfo

        // BĐS chính chủ
        [HttpPost]
        public ActionResult SetIsOwner(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsOwner = true;
            Services.Notifier.Information(T("BĐS {0} --> chính chủ", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnSetIsOwner(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsOwner = false;
            Services.Notifier.Information(T("BĐS {0} --> xóa chính chủ", p.Id));

            return RedirectToAction("Index");
        }

        // BĐS miễn trung gian
        [HttpPost]
        public ActionResult SetNoBroker(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.NoBroker = true;
            Services.Notifier.Information(T("BĐS {0} --> miễn trung gian", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnSetNoBroker(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.NoBroker = false;
            Services.Notifier.Information(T("BĐS {0} --> xóa miễn trung gian", p.Id));

            return RedirectToAction("Index");
        }

        // BĐS phát mãi
        [HttpPost]
        public ActionResult SetIsAuction(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsAuction = true;
            Services.Notifier.Information(T("BĐS {0} --> phát mãi", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnSetIsAuction(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsAuction = false;
            Services.Notifier.Information(T("BĐS {0} --> xóa phát mãi", p.Id));

            return RedirectToAction("Index");
        }

        // BĐS đã xác thực
        [HttpPost]
        public ActionResult SetIsAuthenticatedInfo(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsAuthenticatedInfo = true;
            Services.Notifier.Information(T("BĐS {0} --> đã xác thực", p.Id));

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UnSetIsAuthenticatedInfo(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty, T("Không có quyền chỉnh sửa BĐS")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<PropertyPart>(id);

            if (p == null) return RedirectToAction("Index");

            #region SECURITY

            if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                if (!Services.Authorizer.Authorize(Permissions.EditProperty, T("Không có quyền chỉnh sửa BĐS")))
                    return new HttpUnauthorizedResult();

            #endregion

            p.IsAuthenticatedInfo = false;
            Services.Notifier.Information(T("BĐS {0} --> chưa xác thực", p.Id));

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult DeleteUserProperty(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);

            _propertyService.DeleteUserProperty(id);

            return RedirectToAction("Index");
        }

        #endregion

        #endregion

        #region Group

        public ActionResult Group(PropertyIndexOptions options, PagerParameters pagerParameters)
        {
            DateTime startIndex = DateTime.Now;

            if (
                !Services.Authorizer.Authorize(Permissions.MetaListOwnProperties, T("Not authorized to list properties")))
                return new HttpUnauthorizedResult();

            #region FILTER

            DateTime startFilter = DateTime.Now;

            IContentQuery<PropertyPart, PropertyPartRecord> list = _propertyService.SearchGroupProperties(options);

            if (_debugIndex) Services.Notifier.Information(T("FILTER {0}", (DateTime.Now - startFilter).TotalSeconds));

            #endregion

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<PropertyPart> results = list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region BUILD MODEL

            DateTime startBuildModel = DateTime.Now;

            var model = new PropertyIndexViewModel
            {
                Properties = results
                    .Select(x => new PropertyEntry
                    {
                        Property = x.Record,
                        PropertyPart = x,
                        DisplayForContact = _propertyService.GetDisplayForContact(x),
                        HostNamePart = _hostNameService.GetHostNameByProperty(x)
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount,
                TotalExecutionTime = (DateTime.Now - startIndex).TotalSeconds,
            };

            if (_debugIndex)
                Services.Notifier.Information(T("MODEL {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            //if (_debugIndex) Services.Notifier.Information(T("COMPLETE {0}", (DateTime.Now - startIndex).TotalSeconds));

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Group(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.BulkActionProperties,
                    T("Not authorized to bulk action properties")))
                return new HttpUnauthorizedResult();

            var viewModel = new PropertyIndexViewModel
            {
                Properties = new List<PropertyEntry>(),
                Options = new PropertyIndexOptions()
            };
            UpdateModel(viewModel);

            List<PropertyEntry> checkedEntries = viewModel.Properties.Where(c => c.IsChecked).ToList();

            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            var publishBulkActions = new List<PropertyBulkAction>();
            if (viewModel.Options.PublishBulkAction != null)
                publishBulkActions.AddRange(viewModel.Options.PublishBulkAction);
            publishBulkActions.Add(viewModel.Options.BulkAction);

            foreach (PropertyBulkAction bulkAction in publishBulkActions)
            {
                switch (bulkAction)
                {
                    case PropertyBulkAction.None:
                        break;

                    #region Shared group

                    case PropertyBulkAction.ShareToGroup:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            ShareToGroup(entry.Property.Id, currentDomainGroup.Id);
                        }
                        _signals.Trigger("PropertyGroup_" + "_" + currentDomainGroup.Id + "_Changed");
                        break;
                    case PropertyBulkAction.NotShareToGroup:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            NotShareToGroup(entry.Property.Id, currentDomainGroup.Id);
                        }
                        _signals.Trigger("PropertyGroup_" + "_" + currentDomainGroup.Id + "_Changed");
                        break;
                    case PropertyBulkAction.RemoveShareToGroup:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            RemoveShareToGroup(entry.Property.Id, currentDomainGroup.Id);
                        }
                        _signals.Trigger("PropertyGroup_" + "_" + currentDomainGroup.Id + "_Changed");
                        break;
                    case PropertyBulkAction.UnRemoveShareToGroup:
                        foreach (PropertyEntry entry in checkedEntries)
                        {
                            UnRemoveShareToGroup(entry.Property.Id, currentDomainGroup.Id);
                        }
                        _signals.Trigger("PropertyGroup_" + "_" + currentDomainGroup.Id + "_Changed");
                        break;

                        #endregion
                }
            }

            return this.RedirectLocal(viewModel.ReturnUrl);
        }

        #endregion

        #region ShareToGroup

        [HttpPost]
        public ActionResult ShareToGroup(int id, int groupId)
        {
            //if (!Services.Authorizer.Authorize(Permissions.ShareOwnProperty, T("Not authorized to share properties")))
            //    return new HttpUnauthorizedResult();

            var p = Services.ContentManager.New<PropertyGroupPart>("PropertyGroup");

            if (!_propertyService.VerifyPropertyGroupExist(id, groupId))
            {
                p.PropertyId = id;
                p.UserGroupId = groupId;
                p.IsApproved = true;

                Services.ContentManager.Create(p);
                Services.Notifier.Information(T("BĐS {0} ({1} - {2}) đã duyệt hiển thị thành công", p.Id, id, groupId));
            }
            else
            {
                Services.Notifier.Information(T("BĐS ({1} - {2}) đã tồn tại", id, groupId));
            }

            return RedirectToAction("Group");
        }

        #endregion

        #region NotShareToGroup

        [HttpPost]
        public ActionResult NotShareToGroup(int id, int groupId)
        {
            //if (!Services.Authorizer.Authorize(Permissions.ShareOwnProperty, T("Not authorized to share properties")))
            //    return new HttpUnauthorizedResult();

            PropertyGroupPart clone = _propertyService.NotShareToGroup(id, groupId);

            return RedirectToAction("Group");
        }

        #endregion

        #region RemoveShareToGroup

        [HttpPost]
        public ActionResult RemoveShareToGroup(int id, int groupId)
        {
            //////if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
            ////return new HttpUnauthorizedResult();

            PropertyGroupPart pGroup =
                Services.ContentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>()
                    .Where(a => a.PropertyId == id && a.UserGroupId == groupId)
                    .List()
                    .FirstOrDefault();

            if (pGroup != null)
            {
                pGroup.IsApproved = false;
                Services.Notifier.Information(T("Propety {0} removed", pGroup.PropertyId));
            }

            return RedirectToAction("Group");
        }

        #endregion

        #region UnRemoveShareToGroup

        [HttpPost]
        public ActionResult UnRemoveShareToGroup(int id, int groupId)
        {
            //////if (!Services.Authorizer.Authorize(Permissions.ManageAdsTypes, T("Not authorized to manage adsTypes")))
            ////return new HttpUnauthorizedResult();

            PropertyGroupPart pGroup =
                Services.ContentManager.Query<PropertyGroupPart, PropertyGroupPartRecord>()
                    .Where(a => a.PropertyId == id && a.UserGroupId == groupId)
                    .List()
                    .FirstOrDefault();

            if (pGroup != null)
            {
                pGroup.IsApproved = true;
                Services.Notifier.Information(T("Propety {0} removed", pGroup.PropertyId));
            }

            return RedirectToAction("Group");
        }

        #endregion

        #region CopyToGroup

        public ActionResult CopyToGroup(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.CopyOwnProperty, T("Not authorized to copy properties")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyPart>(id);

            if (record != null)
            {
                #region SECURITY

                if (record.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.CopyProperty, T("Not authorized to copy properties")))
                        return new HttpUnauthorizedResult();

                #endregion
                // Bỏ qua báo trùng khi copy BĐS nội bộ
                //bool isValidForCopy = _propertyService.IsValidForCopyToGroup(record);

                PropertyPart clone = _propertyService.CopyToGroup(record);

                if (clone != null)
                {
                    // khi copy xong chuyển BĐS sang chế độ không hiện copy nữa

                    var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

                    PropertyGroupPart p = _propertyService.NotShareToGroup(id, currentDomainGroup.Id);

                    Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã sao chép thành công.",
                        Url.Action("Edit", new { clone.Id }), clone.Id, clone.DisplayForAddressForOwner));

                    _signals.Trigger("PropertyGroup_" + "_" + currentDomainGroup.Id + "_Changed");

                    return RedirectToAction("Edit", new { clone.Id, returnUrl = "" });
                }

                //Services.Notifier.Error(
                //    T(
                //        "BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> không thể sao chép do đã có trong dữ liệu hoặc không đủ thông tin.",
                //        Url.Action("Edit", new { record.Id }), record.Id, record.DisplayForAddressForOwner));
            }

            return RedirectToAction("Group");
        }

        public ActionResult CopyToApproved(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.CopyOwnProperty, T("Not authorized to copy properties")))
                return new HttpUnauthorizedResult();

            var record = Services.ContentManager.Get<PropertyPart>(id);

            if (record != null)
            {
                #region SECURITY

                if (record.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (!Services.Authorizer.Authorize(Permissions.CopyProperty, T("Not authorized to copy properties")))
                        return new HttpUnauthorizedResult();

                #endregion

                PropertyPart clone = _propertyService.CopyToApproved(record);

                if (clone != null)
                {
                    // khi copy xong chuyển BĐS sang chế độ không hiện copy nữa

                    var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

                    PropertyGroupPart p = _propertyService.NotShareToGroup(id, currentDomainGroup.Id);

                    Services.Notifier.Information(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> đã sao chép thành công.",
                        Url.Action("Edit", new { clone.Id }), clone.Id, clone.DisplayForAddressForOwner));

                    _signals.Trigger("PropertyGroup_" + "_" + currentDomainGroup.Id + "_Changed");
                }

                return RedirectToAction("Edit", new { clone.Id, returnUrl = "" });
            }

            return RedirectToAction("Group");
        }

        #endregion

        #region Post Facebook

        private void ActionPostFacebook(int propertyId)
        {
            Session["PropertyAdminId"] = null;
            var p = Services.ContentManager.Get<PropertyPart>(propertyId);
            // ReSharper disable once Mvc.ActionNotResolved
            // ReSharper disable once Mvc.ControllerNotResolved
            // ReSharper disable once Mvc.AreaNotResolved
            string linkdetail = Url.Action("RealEstateDetail", "PropertySearch",
                new { area = "RealEstate.FrontEnd", id = p.Id, title = p.DisplayForUrl });

            int userCurentId = Services.WorkContext.CurrentUser.As<UserPart>().Id;

            PropertyDisplayEntry entry = _propertyService.BuildPropertyEntryFrontEnd(p);

            string titlecontent = p.DisplayForTitle;

            var interior = new List<string> { entry.Property.DisplayForAreaConstructionLocationInfo };
            if (!String.IsNullOrEmpty(entry.Property.Content)) interior.Add(entry.Property.Content);
            if (entry.Property.IsOwner) interior.Add("Tin chính chủ");
            if (entry.Property.NoBroker) interior.Add("Miễn trung gian");
            if (entry.Property.IsAuction) interior.Add("BĐS phát mãi");
            string summarydetail = String.Join(", ", interior);

            string defaultAvatar = entry.DefaultImgUrl;

            _facebookApiSevice.PostToYourFacebook(linkdetail, titlecontent + "<br>" + summarydetail,
                p.DisplayForTitle + " - " + p.DisplayForPriceProposed,
                titlecontent + " - " + p.DisplayForPriceProposed, defaultAvatar, userCurentId);
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

        #region WebClient

        private void ClearCacheClientWhenUpdate(int propertyId)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Timeout = 600 * 60 * 1000;
                    // set the user agent to IE6
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)");

                    // actually execute the GET request
                    string url = _settingService.GetSetting("URL_CLEAR_CACHE_WHEN_UPDATE");
                    string key = _settingService.GetSetting("API_CLEARCACHE_KEY");
                    string responseString = client.DownloadString(string.Format("{0}?key={1}&propertyId={2}", url, key, propertyId));

                    Services.Notifier.Information(T("1. Clear cache client response: {0}", responseString));
                }
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("1. Clear cache client response Error: {0}", ex.Message));
            };
        }

        private void ClearCaceWhenActionWithProperties(string propertyIds)
        {
            try{
                using (WebClient client = new WebClient())
                {
                    client.Timeout = 600 * 60 * 1000;
                    // set the user agent to IE6
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)");

                    // actually execute the GET request
                    string url = _settingService.GetSetting("URL_CLEAR_CACHE_ACTION_WITH_PROPERTIES");
                    string key = _settingService.GetSetting("API_CLEARCACHE_KEY");
                    string ret = client.DownloadString(string.Format("{0}?key={1}&propertyIds={2}", url, key, propertyIds));

                    Services.Notifier.Information(T("propertyIds: {0}", propertyIds));

                    Services.Notifier.Information(T("2. Clear cache client response: {0}", ret));
                }
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("2. Clear cache client response Error: {0}", ex.Message));
            };
        }

        private class WebClient : System.Net.WebClient
        {
            public int Timeout { get; set; }

            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest lWebRequest = base.GetWebRequest(uri);
                lWebRequest.Timeout = Timeout;
                ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
                return lWebRequest;
            }
        }

        #endregion
        
    }
}