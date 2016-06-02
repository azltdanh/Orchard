using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using Orchard.Security;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class CustomerAdminController : Controller, IUpdateModel
    {
        //private bool _debugIndex = true;
        private readonly bool _debugEdit = true;

        #region Init

        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly IRevisionService _revisionService;
        private readonly IPropertySettingService _settingService;
        private readonly ISiteService _siteService;
        private readonly IPropertyExchangeService _propertyExchangeService;

        private readonly CultureInfo _provider = CultureInfo.InvariantCulture;
        private const string Format = "dd/MM/yyyy";
        private const DateTimeStyles Style = DateTimeStyles.AdjustToUniversal;

        public CustomerAdminController(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            IPropertySettingService settingService,
            IPropertyService propertyService,
            ICustomerService customerService,
            IRevisionService revisionService,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IPropertyExchangeService propertyExchangeService)
        {
            Services = services;

            _addressService = addressService;
            _groupService = groupService;
            _settingService = settingService;
            _propertyService = propertyService;
            _customerService = customerService;
            _revisionService = revisionService;
            _siteService = siteService;
            _propertyExchangeService = propertyExchangeService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;

            if (!Services.Authorizer.Authorize(Permissions.ViewDebugLogEstimateProperties))
            {
                //_debugIndex = false;
                _debugEdit = false;
            }
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion

        #region Index

        public ActionResult Index(CustomerIndexOptions options, PagerParameters pagerParameters)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            DateTime startIndex = DateTime.Now;

            if (!Services.Authorizer.Authorize(Permissions.MetaListOwnCustomers, T("Not authorized to list customers")))
                return new HttpUnauthorizedResult();

            IContentQuery<CustomerPart, CustomerPartRecord> list = _customerService.SearchCustomers(options);

            #region SLICE

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<CustomerPart> results = list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #endregion

            #region BUILD MODEL

            int statusNegotiateId = _customerService.GetStatus("st-negotiate").Id;


            var model = new CustomerIndexViewModel
            {
                Customers = results
                    .Select(x => new CustomerEntry
                    {
                        Customer = x.Record,
                        Purposes = _customerService.GetCustomerPurposes(x),
                        Requirements = _customerService.GetRequirements(x),
                        IsEditable = _customerService.EnableEditCustomer(x.Record, user),
                        ShowContactPhone =
                            !(x.Status.Id == statusNegotiateId &&
                              !Services.Authorizer.Authorize(Permissions.AccessNegotiateCustomers))
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount,
                TotalExecutionTime = (DateTime.Now - startIndex).TotalSeconds,
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

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.BulkActionCustomers,
                    T("Not authorized to bulk action customers")))
                return new HttpUnauthorizedResult();

            var viewModel = new CustomerIndexViewModel
            {
                Customers = new List<CustomerEntry>(),
                Options = new CustomerIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<CustomerEntry> checkedEntries = viewModel.Customers.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case CustomerBulkAction.None:
                    break;

                #region InternalStatus

                case CustomerBulkAction.StatusNew:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-new");
                    }
                    break;
                case CustomerBulkAction.StatusHigh:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-high");
                    }
                    break;
                case CustomerBulkAction.StatusNegotiate:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-negotiate");
                    }
                    break;
                case CustomerBulkAction.StatusTrading:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-trading");
                    }
                    break;
                case CustomerBulkAction.StatusBought:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-bought");
                    }
                    break;
                case CustomerBulkAction.StatusOnhold:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-onhold");
                    }
                    break;
                case CustomerBulkAction.StatusSuspended:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-suspended");
                    }
                    break;
                case CustomerBulkAction.StatusDoubt:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-doubt");
                    }
                    break;
                case CustomerBulkAction.StatusBroker:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-broker");
                    }
                    break;
                case CustomerBulkAction.StatusTrash:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-trash");
                    }
                    break;

                #endregion

                #region Action

                case CustomerBulkAction.Publish:
                    if (Services.Authorizer.Authorize(Permissions.PublishCustomer))
                    {
                        foreach (CustomerEntry entry in checkedEntries)
                        {
                            UpdateStatus(entry.Customer.Id, "st-approved");
                        }
                    }
                    break;
                case CustomerBulkAction.UnPublish:
                    break;
                case CustomerBulkAction.Delete:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        Delete(entry.Customer.Id);
                    }
                    break;
                case CustomerBulkAction.Export:
                    break;
                case CustomerBulkAction.UpdateNegotiateStatus:
                    int pnegotiateStatusId = _propertyService.GetStatus("st-negotiate").Id;
                    int cnegotiateStatusId = _customerService.GetStatus("st-negotiate").Id;
                    DateTime dateToUpdateNegotiateStatus =
                        DateTime.Now.AddDays(
                            -(int.Parse(_settingService.GetSetting("DaysToUpdateNegotiateStatus") ?? "7")));

                    IEnumerable<CustomerPart> customers =
                        Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                            .Where(a => a.Status.Id == cnegotiateStatusId &&
                                        (
                                            (a.StatusChangedDate != null &&
                                             a.StatusChangedDate < dateToUpdateNegotiateStatus)
                                            || a.LastUpdatedDate < dateToUpdateNegotiateStatus
                                            )
                            ).List();

                    foreach (CustomerPart item in customers)
                    {
                        bool haveNegotiateProperties = item.Properties.Any(a => a.Status.Id == pnegotiateStatusId &&
                                                                                (
                                                                                    (a.StatusChangedDate != null &&
                                                                                     a.StatusChangedDate <
                                                                                     dateToUpdateNegotiateStatus)
                                                                                    ||
                                                                                    a.LastUpdatedDate <
                                                                                    dateToUpdateNegotiateStatus
                                                                                    )
                            );
                        if (!haveNegotiateProperties) UpdateStatus(item.Id, "st-new");
                    }
                    break;

                #endregion

                #region ExternalAction

                // Duyệt tin rao
                case CustomerBulkAction.Refresh:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        Refresh(entry.Customer.Id);
                    }
                    break;
                case CustomerBulkAction.AddToAdsVIP:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        AddToAdsVIP(entry.Customer.Id);
                    }
                    break;
                case CustomerBulkAction.RemoveAdsVIP:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        RemoveAdsVIP(entry.Customer.Id);
                    }
                    break;
                case CustomerBulkAction.Approve:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-approved");
                    }
                    break;
                case CustomerBulkAction.NotApprove:
                    foreach (CustomerEntry entry in checkedEntries)
                    {
                        UpdateStatus(entry.Customer.Id, "st-invalid");
                    }
                    break;

                    #endregion
            }

            return this.RedirectLocal(viewModel.ReturnUrl);
            //return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        #endregion

        #region Create

        public ActionResult Create(int? adsTypeId, int? pExchangeId, string returnUrl)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnCustomer, T("Not authorized to create customers")))
                return new HttpUnauthorizedResult();

            int provinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;
            int districtId = 0;
            PropertyExchangePartRecord propertyExchange = null;

            #region Validate PropertyExchange

            if (pExchangeId.HasValue)
            {
                propertyExchange = _propertyService.GetExchangePartRecord(pExchangeId.Value);
                if (propertyExchange == null) return RedirectToAction("Create", new { adsTypeId = adsTypeId, returnUrl = returnUrl });

                if (propertyExchange != null && propertyExchange.Customer != null)
                {
                    return RedirectToAction("Edit", new { id = propertyExchange.Customer.Id, pExchangeId = propertyExchange.Id, returnUrl = returnUrl });
                }
            }

            #endregion

            // Get Default from Group Setting
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var belongGroup = _groupService.GetBelongGroup(user.Id);
            if (belongGroup != null) if (belongGroup.DefaultProvince != null) provinceId = belongGroup.DefaultProvince.Id;

            var createModel = new CustomerCreateViewModel
            {
                ReturnUrl = returnUrl,
                Status = _customerService.GetStatusForInternal(),
                StatusId = _customerService.GetStatus("st-new").Id,
                Purposes = _customerService.GetPurposesEntries().ToList(),
                AdsTypeId = adsTypeId ?? 0,

                PropertyTypeGroups = _propertyService.GetTypeGroups(),
                Directions = _propertyService.GetDirections(),
                Locations = _propertyService.GetLocations(),
                PaymentMethods = _propertyService.GetPaymentMethods(),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvinces(),
                Districts = _addressService.GetDistricts(provinceId),
                Wards = _addressService.GetWards(districtId),
                Streets = _addressService.GetStreets(districtId),
                Apartments = _addressService.GetApartments(districtId)
            };

            if (pExchangeId.HasValue && propertyExchange != null)
            {
                createModel.AdsTypes = _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-exchange");
                createModel.IsRequirementExchange = true;
                createModel.PropertyExchangeId = propertyExchange.Id;
                createModel.ExchangeTypes = _propertyService.GetExchangeTypeParts();

                var property = Services.ContentManager.Get<PropertyPart>(propertyExchange.Property.Id);
                createModel.PropertyLink = Url.Action("Edit", "PropertyAdmin", new { id = property.Id });
                createModel.PropertyAddress = property.DisplayForAddress;
                //createModel.ContactName = property.ContactName;
                //createModel.ContactPhone = property.ContactPhone;
            }
            else
            {
                createModel.IsRequirementExchange = false;
                createModel.PropertyExchangeId = 0;
                createModel.AdsTypes = _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-renting" || a.CssClass == "ad-buying");
            }

            var customer = Services.ContentManager.New<CustomerPart>("Customer");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/Customer.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(customer);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(CustomerCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnCustomer, T("Not authorized to create customers")))
                return new HttpUnauthorizedResult();

            #region VALIDATION

            if (!string.IsNullOrEmpty(createModel.ContactPhone))
            {
                if (!_customerService.VerifyCustomerUnicity(createModel.ContactPhone))
                {
                    CustomerPart r = _customerService.GetCustomerByContactPhone(0, createModel.ContactPhone);
                    AddModelError("ContactPhone",
                        T("Khách hàng với số điện thoại {0} đã có trong dữ liệu.", r.ContactPhone));
                    Services.Notifier.Warning(T("Khách hàng <a href='{0}'>{1} - {2} {3}</a> đã có trong dữ liệu.",
                        Url.Action("Edit", "CustomerAdmin", new { r.Id }), r.Id, r.ContactName, r.ContactPhone));
                }
            }

            if (createModel.PropertyTypeGroupId.HasValue && createModel.PropertyTypeGroupId != 52)
            {
                if (createModel.DistrictIds == null ||
                    (createModel.DistrictIds != null && createModel.DistrictIds.Count() == 0))
                {
                    AddModelError("NullDistrictIds", T("Vui lòng chọn ít nhất một Quận / Huyện."));
                }
            }

            if (createModel.IsRequirementExchange && string.IsNullOrEmpty(createModel.ExchangeTypeClass))
            {
                AddModelError("NullExchangeTypeClass", T("Vui lòng chọn loại trao đổi."));
            }

            #endregion

            #region CREATE RECORD

            DateTime createdDate = DateTime.Now;
            var createdUser = Services.WorkContext.CurrentUser.As<UserPart>();
            var belongGroup = _groupService.GetBelongGroup(createdUser.Id);

            var c = Services.ContentManager.New<CustomerPart>("Customer");
            if (ModelState.IsValid)
            {
                #region RECORD

                // Contact

                c.ContactName = createModel.ContactName;
                c.ContactPhone = createModel.ContactPhone;
                c.ContactAddress = createModel.ContactAddress;
                c.ContactEmail = createModel.ContactEmail;

                // Status
                c.Status = _customerService.GetStatus(createModel.StatusId);
                c.StatusChangedDate = createdDate;
                c.Note = createModel.Note;

                // User
                c.CreatedDate = createdDate;
                c.CreatedUser = createdUser.Record;
                c.LastUpdatedDate = createdDate;
                c.LastUpdatedUser = createdUser.Record;

                // UserGroup
                c.UserGroup = belongGroup;

                // Ads

                // Published
                c.Published = createModel.Published;
                c.AdsExpirationDate = createModel.AdsExpirationDate;
                if (c.Published)
                    c.AdsExpirationDate = _propertyService.GetAddExpirationDate(createModel.AddAdsExpirationDate,
                        c.AdsExpirationDate);
                else c.AdsExpirationDate = null;

                // AdsVIP
                c.AdsVIP = createModel.AdsVIP;
                c.AdsVIPExpirationDate = createModel.AdsVIPExpirationDate;
                if (c.AdsVIP)
                    c.AdsVIPExpirationDate = _propertyService.GetAddExpirationDate(createModel.AddAdsVIPExpirationDate,
                        c.AdsVIPExpirationDate);
                else c.AdsVIPExpirationDate = null;

                // Thời gian đăng tin AdsExpirationDate = MAX(AdsExpirationDate, AdsVIPExpirationDate, AdsGoodDealExpirationDate)
                c.AdsExpirationDate = (c.AdsVIPExpirationDate != null &&
                                       (c.AdsVIPExpirationDate > c.AdsExpirationDate || c.AdsExpirationDate == null))
                    ? c.AdsVIPExpirationDate
                    : c.AdsExpirationDate;

                #endregion

                Services.ContentManager.Create(c);

                // IdStr
                c.IdStr = c.Id.ToString(CultureInfo.InvariantCulture);

                // Purposes
                _customerService.UpdatePurposesForContentItem(c, createModel.Purposes);

                UpdateRequirements(c.Record, _customerService.BuildCustomerEditRequirementViewModel(createModel));

                //TempData["PropertyTypeGroupId"] = createModel.PropertyTypeGroupId;
                TempData["AdsTypeId"] = createModel.AdsTypeId;

                //UpdatePropertyExchange
                if (createModel.IsRequirementExchange)
                {
                    _propertyExchangeService.UpdatePropertyExchange(createModel.PropertyExchangeId.Value, c, createModel.ExchangeTypeClass, createModel.ExchangeValue, createModel.PaymentMethodId);

                    return RedirectToAction("Edit", new { id = c.Id, typeGroupId = createModel.PropertyTypeGroupId, pExchangeId = createModel.PropertyExchangeId });
                }


                return RedirectToAction("Edit", new { id = c.Id, typeGroupId = createModel.PropertyTypeGroupId });
            }

            dynamic model = Services.ContentManager.UpdateEditor(c, this);

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                createModel.Status = _customerService.GetStatusForInternal();
                createModel.Purposes = _customerService.GetPurposesEntries().ToList();


                createModel.PropertyTypeGroups = _propertyService.GetTypeGroups();
                createModel.Directions = _propertyService.GetDirections();
                createModel.Locations = _propertyService.GetLocations();
                createModel.PaymentMethods = _propertyService.GetPaymentMethods();

                createModel.Provinces = _addressService.GetProvinces();
                createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
                createModel.Wards = _addressService.GetWards(createModel.DistrictIds);
                createModel.Streets = _addressService.GetStreets(createModel.DistrictIds);
                createModel.Apartments = _addressService.GetApartments(createModel.DistrictIds);

                if (createModel.IsRequirementExchange)
                {
                    createModel.ExchangeTypes = _propertyService.GetExchangeTypeParts();
                    createModel.AdsTypes = _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-exchange");
                }
                else
                {
                    createModel.AdsTypes =
                    _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-renting" || a.CssClass == "ad-buying");
                }

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/Customer.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            Services.Notifier.Information(T("Khách hàng <a href='{0}'>{1} - {2} {3}</a> đã nhập thành công",
                Url.Action("Edit", new { c.Id, createModel.AdsTypeId }), c.Id, c.ContactName, c.ContactPhone));
            return RedirectToAction("Index");
        }

        #endregion

        #region Edit

        public ActionResult Edit(int id, int? typeGroupId, int? pExchangeId, PagerParameters pagerParameters, string returnUrl)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);
            PropertyExchangePartRecord propertyExchange = null;

            #region Validate PropertyExchange

            if (pExchangeId.HasValue)
            {
                propertyExchange = _propertyService.GetExchangePartRecord(pExchangeId.Value);
                if (propertyExchange == null) return RedirectToAction("Edit", new { id = id, typeGroupId = typeGroupId, returnUrl = returnUrl });

                if (propertyExchange != null && propertyExchange.Customer == null)
                {
                    return RedirectToAction("Create", new { id = id, pExchangeId = pExchangeId, returnUrl = returnUrl });
                }
            }
            else
            {
                var propertyExchangeByCustomer = _propertyService.GetExchangePartRecord(c);

                if (propertyExchangeByCustomer != null)
                    return RedirectToAction("Edit", new { id = id, typeGroupId = typeGroupId, pExchangeId = propertyExchangeByCustomer.Id, returnUrl = returnUrl });
            }

            #endregion

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            #region GET RECORD

            DateTime startGetRecord = DateTime.Now;

            if (_debugEdit)
                Services.Notifier.Information(T("GET RECORD {0}", (DateTime.Now - startGetRecord).TotalSeconds));

            #endregion

            #region SECURITY

            DateTime startCheckSecurity = DateTime.Now;

            if (!_customerService.EnableEditCustomer(c.Record, user))
            {
                if (Services.Authorizer.Authorize(StandardPermissions.AccessAdminPanel))
                {
                    Services.Notifier.Error(T("Not authorized to view this customer {0}", c.Id));
                    return RedirectToAction("Index"); // Redirect to Index
                }
                else
                {
                    Services.Notifier.Error(T("Not authorized to view this customer {0}", c.Id));
                    return new HttpUnauthorizedResult("Not authorized to edit properties");
                }
            }

            if (_debugEdit)
                Services.Notifier.Information(T("SECURITY {0}", (DateTime.Now - startCheckSecurity).TotalSeconds));

            #endregion

            #region BUILD MODEL

            DateTime startBuildModel = DateTime.Now;

            int provinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            // Get Default from Group Setting
            var belongGroup = _groupService.GetBelongGroup(user.Id);
            if (belongGroup != null) if (belongGroup.DefaultProvince != null) provinceId = belongGroup.DefaultProvince.Id;

            CustomerEditViewModel editModel = _customerService.BuildEditViewModel(c);
            editModel.ReturnUrl = returnUrl;

            editModel.PropertyTypeGroups = _propertyService.GetTypeGroups();
            editModel.PropertyTypeGroupId = typeGroupId != null ? typeGroupId : 0;
            editModel.ProvinceId = provinceId;
            editModel.Provinces = _addressService.GetProvinces();
            editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
            editModel.Wards = _addressService.GetWards(editModel.DistrictIds);
            editModel.Streets = _addressService.GetStreets(editModel.DistrictIds);
            editModel.Directions = _propertyService.GetDirections();
            editModel.Locations = _propertyService.GetLocations();
            editModel.PaymentMethods = _propertyService.GetPaymentMethods();
            editModel.Apartments = _addressService.GetApartments(editModel.DistrictIds);

            editModel.Users = _groupService.GetGroupUsers(user);

            //editModel.PropertyTypeGroupId = TempData["PropertyTypeGroupId"] != null
            //    ? Convert.ToInt32(TempData["PropertyTypeGroupId"])
            //    : 0;
            editModel.AdsTypeId = TempData["AdsTypeId"] != null ? Convert.ToInt32(TempData["AdsTypeId"]) : 0;

            if (pExchangeId.HasValue && propertyExchange != null)
            {
                editModel.AdsTypes = _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-exchange");
                editModel.IsRequirementExchange = true;
                editModel.PropertyExchangeId = propertyExchange.Id;
                editModel.ExchangeTypes = _propertyService.GetExchangeTypeParts();
                editModel.ExchangeValue = propertyExchange.ExchangeValues;

                var property = Services.ContentManager.Get<PropertyPart>(propertyExchange.Property.Id);
                editModel.PropertyLink = Url.Action("Edit", "PropertyAdmin", new { id = property.Id });
                editModel.PropertyAddress = property.DisplayForAddress;
            }
            else
            {
                editModel.AdsTypes =
                _propertyService.GetAdsTypes().Where(r => r.CssClass == "ad-renting" || r.CssClass == "ad-buying");
                editModel.IsRequirementExchange = false;
                editModel.PropertyExchangeId = 0;
            }

            if (_debugEdit)
                Services.Notifier.Information(T("BUILD MODEL {0}", (DateTime.Now - startBuildModel).TotalSeconds));

            #endregion

            #region SEARCH PROPERTIES

            //DateTime startSearchProperties = DateTime.Now;

            //// Get all Properties from customer's requirements
            //var listProperties = _customerService.SearchProperties(c);

            //var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            //int totalCount = listProperties.Count();
            //var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
            //editModel.Pager = pagerShape;

            //var addProperties = (listProperties.Skip((pager.Page-1) * pager.PageSize).Take(pager.PageSize)
            //                        .Select(r => new{PropertyPart = r,CustomerPropertyRecord=new CustomerPropertyRecord { PropertyPartRecord = r.Record, CustomerPartRecord = c.Record }}).ToList()
            //                        .Select(r => new CustomerPropertyEntry
            //                        {
            //                            PropertyPart = r.PropertyPart,
            //                            CustomerPropertyRecord = r.CustomerPropertyRecord,
            //                            CustomerFeedbackId = 0,
            //                            ShowContactPhone = !((r.PropertyPart.Status.CssClass == "st-negotiate" || r.PropertyPart.Status.CssClass == "st-trading") && !Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties))
            //                        }).ToList());

            //editModel.Properties.AddRange(addProperties);

            //if (_debugEdit) Services.Notifier.Information(T("SEARCH PROPERTIES {0}", (DateTime.Now - startSearchProperties).TotalSeconds));

            #endregion

            #region BUILD TEMPLATE

            DateTime startBuildTemplate = DateTime.Now;

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/Customer.Edit",
                Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(c);
            model.Content.Add(editor);

            if (_debugEdit)
                Services.Notifier.Information(T("BUILD TEMPLATE {0}", (DateTime.Now - startBuildTemplate).TotalSeconds));

            #endregion

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, PagerParameters pagerParameters, FormCollection frmCollection)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var c = Services.ContentManager.Get<CustomerPart>(id);

            #region SECURITY

            if (!_customerService.EnableEditCustomer(c.Record, user))
            {
                if (Services.Authorizer.Authorize(StandardPermissions.AccessAdminPanel))
                {
                    Services.Notifier.Error(T("Not authorized to view this customer {0}", c.Id));
                    return RedirectToAction("Index"); // Redirect to Index
                }
                else
                {
                    Services.Notifier.Error(T("Not authorized to view this customer {0}", c.Id));
                    return new HttpUnauthorizedResult("Not authorized to edit properties");
                }
            }

            #endregion

            #region Build oldModel

            var oldModel = new CustomerEditViewModel { Customer = c };
            DateTime oldLastUpdatedDate = oldModel.LastUpdatedDate;
            UserPartRecord oldLastUpdatedUser = oldModel.LastUpdatedUser;
            string oldContactName = c.ContactName;
            string oldContactPhone = c.ContactPhone;
            string oldContactAddress = c.ContactAddress;
            string oldContactEmail = c.ContactEmail;
            int oldStatusId = c.Status.Id;

            #endregion

            #region UPDATE MODEL

            dynamic model = Services.ContentManager.UpdateEditor(c, this);

            DateTime lastUpdatedDate = DateTime.Now;
            UserPart lastUpdatedUser = user;
            var belongGroup = _groupService.GetBelongGroup(user.Id);

            var editModel = new CustomerEditViewModel { Customer = c };

            if (TryUpdateModel(editModel))
            {
                #region VALIDATION

                // Chỉ kiểm tra trùng nếu là KH nội bộ

                if (!_customerService.IsExternalCustomer(c))
                {
                    if (!string.IsNullOrEmpty(editModel.ContactPhone))
                    {
                        if (!_customerService.VerifyCustomerUnicity(id, editModel.ContactPhone))
                        {
                            CustomerPart r = _customerService.GetCustomerByContactPhone(0, editModel.ContactPhone);
                            AddModelError("ContactPhone",
                                T("Khách hàng với số điện thoại {0} đã có trong dữ liệu", r.ContactPhone));
                            Services.Notifier.Warning(
                                T("Edi: Khách hàng <a href='{0}'>{1} - {2} {3}</a> đã có trong dữ liệu.",
                                    Url.Action("Edit", "CustomerAdmin", new { r.Id }), r.Id, r.ContactName, r.ContactPhone));
                        }
                    }
                }

                if (!string.IsNullOrEmpty(frmCollection["submit.AddNewRequirement"]))
                {
                    if (editModel.PropertyTypeGroupId.HasValue && editModel.PropertyTypeGroupId != 52)
                    {
                        if (editModel.DistrictIds == null ||
                            (editModel.DistrictIds != null && editModel.DistrictIds.Count() == 0))
                        {
                            AddModelError("NullDistrictIds", T("Vui lòng chọn ít nhất một Quận / Huyện."));
                        }
                    }
                }

                if (editModel.IsRequirementExchange && string.IsNullOrEmpty(editModel.ExchangeTypeClass))
                {
                    AddModelError("NullExchangeTypeClass", T("Vui lòng chọn loại trao đổi."));
                }

                #endregion

                if (ModelState.IsValid)
                {
                    #region UPDATE MODEL

                    // User
                    c.LastUpdatedDate = lastUpdatedDate;
                    c.LastUpdatedUser = lastUpdatedUser.Record;

                    // UserGroup
                    if (c.UserGroup == null) c.UserGroup = belongGroup;

                    // Status
                    c.Status = _customerService.GetStatus(editModel.StatusId);
                    if (c.Status.Id != oldStatusId)
                        c.StatusChangedDate = lastUpdatedDate;

                    // Published
                    if (c.Published)
                        c.AdsExpirationDate = _propertyService.GetAddExpirationDate(editModel.AddAdsExpirationDate,
                            c.AdsExpirationDate);
                    else c.AdsExpirationDate = null;

                    // AdsVIP
                    if (c.AdsVIP)
                        c.AdsVIPExpirationDate = _propertyService.GetAddExpirationDate(
                            editModel.AddAdsVIPExpirationDate, c.AdsVIPExpirationDate);
                    else c.AdsVIPExpirationDate = null;

                    // Thời gian đăng tin AdsExpirationDate = MAX(AdsExpirationDate, AdsVIPExpirationDate, AdsGoodDealExpirationDate)
                    c.AdsExpirationDate = (c.AdsVIPExpirationDate != null &&
                                           (c.AdsVIPExpirationDate > c.AdsExpirationDate || c.AdsExpirationDate == null))
                        ? c.AdsVIPExpirationDate
                        : c.AdsExpirationDate;

                    // Purposes
                    _customerService.UpdatePurposesForContentItem(c, editModel.Purposes);

                    var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

                    //TempData["PropertyTypeGroupId"] = editModel.PropertyTypeGroupId;
                    TempData["AdsTypeId"] = editModel.AdsTypeId;

                    // Requirement
                    if (!string.IsNullOrEmpty(frmCollection["submit.AddNewRequirement"]))
                    {
                        UpdateRequirements(c.Record, _customerService.BuildCustomerEditRequirementViewModel(editModel));
                        return RedirectToAction("Edit", new { id, page = pager.Page });
                    }

                    // Properties
                    if (!string.IsNullOrEmpty(frmCollection["submit.BulkUpdateProperties"]))
                    {
                        CustomerPart customer = c;
                        var feedback = Services.ContentManager.Get<CustomerFeedbackPart>(editModel.FeedbackId ?? 0);

                        var propertyIds = new List<int>();

                        if (editModel.Properties != null)
                        {
                            IEnumerable<CustomerPropertyEntry> checkedEntries =
                                editModel.Properties.Where(a => a.IsChecked);
                            propertyIds =
                                checkedEntries.Select(a => a.CustomerPropertyRecord.PropertyPartRecord.Id).ToList();
                        }

                        if (editModel.PropertyId.HasValue) propertyIds.Add((int)editModel.PropertyId);

                        foreach (int propertyId in propertyIds)
                        {
                            // TODO: validate FeedbackId & UserIds
                            var property = Services.ContentManager.Get<PropertyPart>(propertyId);
                            if (property != null)
                            {
                                // Add property
                                DateTime visitedDate = DateTime.Now;
                                if (!string.IsNullOrEmpty(editModel.VisitedDate))
                                    DateTime.TryParseExact(editModel.VisitedDate, Format, _provider, Style,
                                        out visitedDate);

                                _customerService.UpdateCustomerProperty(customer, property, feedback, editModel.UserIds,
                                    visitedDate, editModel.IsWorkOverTime);

                                Services.Notifier.Information(T("BĐS mã số {0} đã thêm vào.", propertyId));
                            }
                            else
                            {
                                Services.Notifier.Error(T("BĐS mã số {0} không tồn tại.", propertyId));
                            }
                        }

                        return RedirectToAction("Edit", new { Id = id, typeGroupId = editModel.PropertyTypeGroupId, page = pager.Page });
                    }

                    #endregion

                    #region Save Revision

                    if (oldContactName != editModel.ContactName)
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, c.Record, "ContactName",
                            oldContactName, editModel.ContactName);

                    if (oldContactPhone != editModel.ContactPhone)
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, c.Record, "ContactPhone",
                            oldContactPhone, editModel.ContactPhone);

                    if (oldContactAddress != editModel.ContactAddress)
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, c.Record,
                            "ContactAddress", oldContactAddress, editModel.ContactAddress);

                    if (oldContactEmail != editModel.ContactEmail)
                        _revisionService.CreateRevision(oldLastUpdatedDate, oldLastUpdatedUser, c.Record, "ContactEmail",
                            oldContactEmail, editModel.ContactEmail);

                    #endregion

                    Services.Notifier.Information(T("Khách hàng <a href='{0}'>{1} - {2} {3}</a> đã cập nhật thành công",
                        Url.Action("Edit", new { c.Id }), c.Id, c.ContactName, c.ContactPhone));

                    //UpdatePropertyExchange
                    if (editModel.IsRequirementExchange)
                    {
                        _propertyExchangeService.UpdatePropertyExchange(editModel.PropertyExchangeId.Value, editModel.ExchangeTypeClass, editModel.ExchangeValue, editModel.PaymentMethodId);

                        return RedirectToAction("Edit", new { id = c.Id, typeGroupId = editModel.PropertyTypeGroupId, pExchangeId = editModel.PropertyExchangeId });
                    }

                }
            }

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = _customerService.BuildEditViewModel(c);

                editModel.PropertyTypeGroups = _propertyService.GetTypeGroups();
                editModel.Provinces = _addressService.GetProvinces();
                editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
                editModel.Wards = _addressService.GetWards(editModel.DistrictIds);
                editModel.Streets = _addressService.GetStreets(editModel.DistrictIds);
                editModel.Directions = _propertyService.GetDirections();
                editModel.Locations = _propertyService.GetLocations();
                editModel.PaymentMethods = _propertyService.GetPaymentMethods();
                editModel.Apartments = _addressService.GetApartments(editModel.DistrictIds);
                editModel.Users = belongGroup != null ? belongGroup.GroupUsers.Select(a => a.UserPartRecord) : new List<UserPartRecord>();

                if (editModel.IsRequirementExchange)
                {
                    editModel.AdsTypes = _propertyService.GetAdsTypes().Where(a => a.CssClass == "ad-exchange");
                    editModel.ExchangeTypes = _propertyService.GetExchangeTypeParts();
                }
                else
                {
                    editModel.AdsTypes =
                    _propertyService.GetAdsTypes().Where(r => r.CssClass == "ad-renting" || r.CssClass == "ad-buying");
                }

                dynamic editor = Shape.EditorTemplate(
                    TemplateName: "Parts/Customer.Edit", Model: editModel, Prefix: null);
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
            return RedirectToAction("Index",
                new { AdsTypeId = editModel.AdsTypeId.HasValue ? editModel.AdsTypeId : 98465 });
        }

        public ActionResult SearchCustomerProperties(int customerId, PagerParameters pagerParameters)
        {
            var c = Services.ContentManager.Get<CustomerPart>(customerId);

            DateTime startSearchProperties = DateTime.Now;

            var editModel = new CustomerEditViewModel
            {
                Properties = _customerService.GetCustomerSavedPropertiesEntries(customerId).ToList(),
                Feedbacks = _customerService.GetFeedbacks(),
                EnableDeleteCustomerProperty = Services.Authorizer.Authorize(Permissions.DeleteCustomerProperty),
                EnableEditContactPhone = true
            };

            // Get all Properties from customer's saved properties
            // Permission

            // Get all Properties from customer's requirements
            IEnumerable<PropertyPart> listProperties = _customerService.SearchProperties(c).ToList();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            int totalCount = listProperties.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
            editModel.Pager = pagerShape;

            List<CustomerPropertyEntry> addProperties =
                (listProperties.Skip((pager.Page - 1) * pager.PageSize).Take(pager.PageSize)
                    .Select(
                        r =>
                            new
                            {
                                PropertyPart = r,
                                CustomerPropertyRecord =
                                    new CustomerPropertyRecord
                                    {
                                        PropertyPartRecord = r.Record,
                                        CustomerPartRecord = c.Record
                                    }
                            }).ToList()
                    .Select(r => new CustomerPropertyEntry
                    {
                        PropertyPart = r.PropertyPart,
                        CustomerPropertyRecord = r.CustomerPropertyRecord,
                        CustomerFeedbackId = 0,
                        ShowContactPhone =
                            !((r.PropertyPart.Status.CssClass == "st-negotiate" ||
                               r.PropertyPart.Status.CssClass == "st-trading") &&
                              !Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties))
                    }).ToList());

            editModel.Properties.AddRange(addProperties);

            if (_debugEdit)
                Services.Notifier.Information(T("SEARCH PROPERTIES {0}",
                    (DateTime.Now - startSearchProperties).TotalSeconds));

            return PartialView("SearchCustomerProperties", editModel);
        }

        #endregion

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeCustomers,
            string[] excludeCustomers)
        {
            return TryUpdateModel(model, prefix, includeCustomers, excludeCustomers);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Details(int id)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);
            var model = new List<CustomerRevisionEntry>();

            IEnumerable<RevisionPart> revisions = _revisionService.GetCustomerRevisions(id).ToList();
            List<DateTime> dates = revisions.Select(a => a.CreatedDate).Distinct().ToList();

            foreach (DateTime date in dates)
            {
                var entry = new CustomerRevisionEntry();
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
                            int befStatusId = int.Parse(item.ValueBefore);
                            int aftStatusId = int.Parse(item.ValueAfter);
                            entry.StatusName = _propertyService.GetStatus(befStatusId).Name + " => " +
                                               _propertyService.GetStatus(aftStatusId).Name;
                            break;

                        case "ContactName":
                            entry.ContactName = item.ValueBefore + " => " + item.ValueAfter;
                            break;

                        case "ContactPhone":
                            entry.ContactPhone = item.ValueBefore + " => " + item.ValueAfter;
                            break;

                        case "ContactEmail":
                            entry.ContactEmail = item.ValueBefore + " => " + item.ValueAfter;
                            break;

                        case "ContactAddress":
                            entry.ContactAddress = item.ValueBefore + " => " + item.ValueAfter;
                            break;
                    }
                }

                #endregion

                model.Add(entry);
            }

            return PartialView(new CustomerRevisionsViewModel { Customer = c, Revisions = model });
        }

        public ActionResult VisitedProperties(int id)
        {
            var c = Services.ContentManager.Get<CustomerPart>(id);

            IEnumerable<CustomerPropertyRecord> visitedProperties =
                _customerService.GetCustomerPropertiesByContactPhone(c.ContactPhone).ToList();

            IOrderedEnumerable<CustomerVisitedEntry> visitedStreets = visitedProperties.Select(
                a => new CustomerVisitedEntry
                {
                    Province = a.PropertyPartRecord.Province,
                    District = a.PropertyPartRecord.District,
                    Ward = a.PropertyPartRecord.Ward,
                    Street =
                        a.PropertyPartRecord.Street != null
                            ? (a.PropertyPartRecord.Street.RelatedStreet ?? a.PropertyPartRecord.Street)
                            : null,
                    Count = visitedProperties.Count(b =>
                        b.PropertyPartRecord.Province == a.PropertyPartRecord.Province &&
                        b.PropertyPartRecord.District == a.PropertyPartRecord.District &&
                        b.PropertyPartRecord.Ward == a.PropertyPartRecord.Ward &&
                        b.PropertyPartRecord.Street == a.PropertyPartRecord.Street)
                }).Distinct()
                .GroupBy(p => new { p.Province, p.District, p.Ward, p.Street })
                .Select(g => g.First())
                .OrderBy(a => a.Province.SeqOrder).ThenBy(a => a.District.SeqOrder);

            var model = new CustomerVisitedPropertiesViewModel
            {
                Customer = c,
                VisitedStreets = visitedStreets
            };

            return PartialView(model);
        }

        public ActionResult ViewCustomers(int id)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var p = Services.ContentManager.Get<PropertyPart>(id);

            var model = new CustomerIndexViewModel
            {
                Customers = p.Customers.OrderByDescending(a => a.CreatedDate)
                    .Select(x => new CustomerEntry
                    {
                        Customer = x,
                        Purposes = _customerService.GetCustomerPurposes(x),
                        Requirements = _customerService.GetRequirements(x),
                        IsEditable = _customerService.EnableEditCustomer(x, user)
                        //ShowContactPhone = !(x.Status.Id == statusNegotiateId && !Services.Authorizer.Authorize(Permissions.AccessNegotiateCustomers))
                    })
                    .ToList(),
            };

            return PartialView("Index", model);
        }

        public ActionResult CalledByUsers(int id)
        {
            var customer = Services.ContentManager.Get<CustomerPart>(id);

            IEnumerable<UserActivityPartRecord> calledByUsers = _revisionService.GetUserActivitiesCalledByUsers(customer).ToList();

            IEnumerable<CalledByUserEntry> entries = calledByUsers.Select(a => new CalledByUserEntry
            {
                User = a.UserPartRecord,
                CallLogs =
                    calledByUsers.Where(b => a.UserPartRecord == b.UserPartRecord).Select(b => b.CreatedDate).ToList(),
                //Count = calledByUsers.Count(b => a.UserPartRecord == b.UserPartRecord) 
            })
                .Distinct().GroupBy(a => a.User).Select(g => g.First());

            var model = new CustomerCalledByUsers
            {
                Customer = customer,
                CalledByUsers = entries
            };

            return PartialView(model);
        }

        public void UpdateRequirements(CustomerPartRecord customer, CustomerEditRequirementViewModel reqModel)
        {
            #region ADD NEW REQUIREMENT

            if ((reqModel.DistrictIds != null && reqModel.DistrictIds.Length > 0)
                || (reqModel.WardIds != null && reqModel.WardIds.Length > 0)
                || (reqModel.StreetIds != null && reqModel.StreetIds.Length > 0)
                || reqModel.MaxArea.HasValue
                || reqModel.MaxWidth.HasValue
                || reqModel.MaxLength.HasValue
                || (reqModel.DirectionIds != null && reqModel.DirectionIds.Length > 0)
                || reqModel.LocationId.HasValue
                || reqModel.MinAlleyWidth.HasValue
                || reqModel.MaxAlleyTurns.HasValue
                || reqModel.MaxDistanceToStreet.HasValue
                || reqModel.MaxFloors.HasValue
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

                        #region Add record Apartment

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
                }

                //return RedirectToAction("Edit", new { id = id });
            }

            #endregion
        }

        #region BULK ACTION

        [HttpPost]
        public ActionResult UpdateStatus(int id, string statusCssClass)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnCustomer, T("Not authorized to edit customers")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var c = Services.ContentManager.Get<CustomerPart>(id);

            if (c != null)
            {
                if (_customerService.EnableEditCustomer(c.Record, user))
                {
                    c.Status = _customerService.GetStatus(statusCssClass);
                    if (statusCssClass == "st-approved") c.Published = true;

                    Services.Notifier.Information(T("Khách hàng <a href='{0}'>{1}</a> đã cập nhật thành công",
                        Url.Action("Edit", new { c.Id }), c.Id));
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.DeleteOwnCustomer, T("Not authorized to delete customers")))
                return new HttpUnauthorizedResult();

            var p = Services.ContentManager.Get<CustomerPart>(id);

            if (p != null)
            {
                #region SECURITY

                if (p.CreatedUser.Id != Services.WorkContext.CurrentUser.As<UserPart>().Id)
                    if (
                        !Services.Authorizer.Authorize(Permissions.DeleteCustomer,
                            T("Not authorized to delete customers")))
                        return new HttpUnauthorizedResult();

                #endregion

                p.Status = _customerService.GetStatus("st-deleted");
                Services.Notifier.Information(T("Khách hàng {0} đã xóa thành công", p.Id));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Refresh(int id)
        {
            var r = Services.ContentManager.Get<CustomerPart>(id);

            if (r != null)
            {
                r.AdsExpirationDate = DateTime.Now.AddDays(90);
                Services.Notifier.Information(T("Khách hàng <a href='{0}'>{1}</a> đã cập nhật thành công",
                    Url.Action("Edit", new { r.Id }), r.Id));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        // ReSharper disable once InconsistentNaming
        public ActionResult AddToAdsVIP(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsVIP, T("Không có quyền đăng tin VIP")))
                return new HttpUnauthorizedResult();

            var r = Services.ContentManager.Get<CustomerPart>(id);

            if (r != null)
            {
                r.AdsVIP = true;
                r.AdsVIPExpirationDate = DateTime.Now.AddDays(90);
                r.Published = true;
                if (r.AdsExpirationDate < r.AdsVIPExpirationDate) r.AdsExpirationDate = r.AdsVIPExpirationDate;
                Services.Notifier.Information(T("BĐS {0} --> tin VIP", r.Id));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        // ReSharper disable once InconsistentNaming
        public ActionResult RemoveAdsVIP(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.SetAdsVIP, T("Không có quyền loại khỏi tin VIP")))
                return new HttpUnauthorizedResult();

            var r = Services.ContentManager.Get<CustomerPart>(id);

            if (r != null)
            {
                r.AdsVIP = false;
                r.AdsVIPExpirationDate = null;
                Services.Notifier.Information(T("BĐS {0} --> loại khỏi tin VIP", r.Id));
            }

            return RedirectToAction("Index");
        }

        #endregion

    }
}