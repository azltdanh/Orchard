using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
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
    public class EstimateController : Controller, IUpdateModel
    {
        #region Init

        private readonly IAddressService _addressService;
        private readonly IAdsPaymentHistoryService _adsPaymentService;
        private readonly IFastFilterService _fastfilterService;
        private readonly IMembershipService _membershipService;
        private readonly IPropertyService _propertyService;
        private readonly IUserGroupService _groupService;

        public EstimateController(
            IAddressService addressService,
            IPropertyService propertyService,
            IUserGroupService groupService,
            IFastFilterService fastfilterService,
            IShapeFactory shapeFactory,
            IMembershipService membershipService,
            IAdsPaymentHistoryService adsPaymentService,
            IOrchardServices services)
        {
            _addressService = addressService;
            _propertyService = propertyService;
            _groupService = groupService;
            _fastfilterService = fastfilterService;
            _membershipService = membershipService;
            _adsPaymentService = adsPaymentService;

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

        #region ViewModel

        public EstimateCreateViewModel BuildCreateViewModel(int? districtId, int? wardId, int? streetId,
            string addressNumber, string addressCorner)
        {
            int provinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            return new EstimateCreateViewModel
            {
                Types = _propertyService.GetTypes("ad-selling", "gp-house"),
                TypeConstructions = _propertyService.GetTypeConstructions(null, null),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvincesForEstimate(),
                Districts = _addressService.GetDistrictsForEstimate(),
                DistrictId = districtId ?? 0,
                Wards = _addressService.GetWards(districtId ?? 0),
                WardId = wardId ?? 0,
                Streets = _addressService.GetStreets(districtId ?? 0),
                StreetId = streetId ?? 0,
                AddressNumber = addressNumber,
                AddressCorner = addressCorner,
                LegalStatus = _propertyService.GetLegalStatus(),
                Directions = _propertyService.GetDirections(),
                Locations = _propertyService.GetLocations(),
                Interiors = _propertyService.GetInteriors(),
                FloorsCount = 0,
                PaymentMethods = _propertyService.GetPaymentMethods(),
                PaymentUnits = _propertyService.GetPaymentUnits(),
                Advantages = _propertyService.GetAdvantagesEntries(),
                DisAdvantages = _propertyService.GetDisAdvantagesEntries(),
            };
        }

        public EstimateCreateViewModel BuildCreateViewModel(EstimateCreateViewModel createModel)
        {
            createModel.Types = _propertyService.GetTypes("ad-selling", "gp-house");
            createModel.TypeConstructions = _propertyService.GetTypeConstructions(createModel.TypeId, createModel.Floors);

            createModel.Provinces = _addressService.GetProvincesForEstimate();
            createModel.Districts = _addressService.GetDistrictsForEstimate();
            createModel.Wards = _addressService.GetWards(createModel.DistrictId);
            createModel.Streets = _addressService.GetStreets(createModel.DistrictId);

            createModel.LegalStatus = _propertyService.GetLegalStatus();
            createModel.Directions = _propertyService.GetDirections();
            createModel.Locations = _propertyService.GetLocations();
            createModel.Interiors = _propertyService.GetInteriors();

            createModel.PaymentMethods = _propertyService.GetPaymentMethods();
            createModel.PaymentUnits = _propertyService.GetPaymentUnits();

            createModel.Advantages = _propertyService.GetAdvantagesEntries();
            createModel.DisAdvantages = _propertyService.GetDisAdvantagesEntries();

            return createModel;
        }

        public EstimateEditViewModel BuildEditViewModel(PropertyPart p)
        {
            IEnumerable<PropertyFilePart> files = new List<PropertyFilePart>();
            long amountEndBalance = 0;

            IUser currentUser = Services.WorkContext.CurrentUser;

            if (currentUser != null)
            {
                files = _propertyService.GetPropertyFiles(p).Where(a => a.CreatedUser.Id == currentUser.Id);

                AdsPaymentHistoryPart amount = _adsPaymentService.GetPaymentHistoryLasted(currentUser.As<UserPart>());
                amountEndBalance = amount != null ? amount.EndBalance : 0;
            }

            int provinceId = p.Province != null ? p.Province.Id : 0;
            int districtId = p.District != null ? p.District.Id : 0;

            IEnumerable<int> advantageIds = _propertyService.GetPropertyAdvantages(p).Select(a => a.Id);
            IEnumerable<int> disadvantageIds = _propertyService.GetPropertyDisAdvantages(p).Select(a => a.Id);

            //var _oldHistory = _adsPaymentService.GetPaymentHistoryByProperty(p);

            return new EstimateEditViewModel
            {
                Property = p,
                IsValidForEstimate = _propertyService.IsValidForEstimate(p),
                TypeId = p.Type != null ? p.Type.Id : 0,
                Types = _propertyService.GetTypes("ad-selling", "gp-house"),
                TypeConstructionId = p.TypeConstruction != null ? p.TypeConstruction.Id : 0,
                TypeConstructions = _propertyService.GetTypeConstructions(p.Type != null ? p.Type.Id : 0, p.Floors),

                ProvinceId = provinceId,
                Provinces = _addressService.GetProvincesForEstimate(),
                DistrictId = districtId,
                Districts = _addressService.GetDistrictsForEstimate(),
                WardId = p.Ward != null ? p.Ward.Id : 0,
                Wards = _addressService.GetWards(districtId),
                StreetId =
                    p.Street != null ? (p.Street.RelatedStreet != null ? p.Street.RelatedStreet.Id : p.Street.Id) : 0,
                Streets = _addressService.GetStreets(districtId),

                LegalStatusId = p.LegalStatus != null ? p.LegalStatus.Id : 0,
                LegalStatus = _propertyService.GetLegalStatus(),
                DirectionId = p.Direction != null ? p.Direction.Id : 0,
                Directions = _propertyService.GetDirections(),
                LocationCssClass = p.Location != null ? p.Location.CssClass : "",
                Locations = _propertyService.GetLocations(),

                AreaIlegal = p.AreaTotal - p.AreaLegal,

                FloorsCount = p.Floors > 10 ? -1 : p.Floors,
                InteriorId = p.Interior != null ? p.Interior.Id : 0,
                Interiors = _propertyService.GetInteriors(),

                PaymentMethodId = p.PaymentMethod != null ? p.PaymentMethod.Id : 0,
                PaymentMethods = _propertyService.GetPaymentMethods(),
                PaymentUnitId = p.PaymentUnit != null ? p.PaymentUnit.Id : 0,
                PaymentUnits = _propertyService.GetPaymentUnits(),

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

                Files = files,
                DateVipFrom = Convert.ToDateTime(DateTime.Now).ToString("dd/MM/yyyy"),
                DateVipTo =
                    Convert.ToDateTime(p.AdsVIPExpirationDate.HasValue
                        ? p.AdsVIPExpirationDate
                        : DateTime.Now.AddDays(30)).ToString("dd/MM/yyyy"),
                AdsTypeCssClass = p.AdsType.CssClass,
                Amount = amountEndBalance,
                AmountVND = _adsPaymentService.ConvertoVND(amountEndBalance),
                UnitArray =
                    _adsPaymentService.GetPaymentConfigsAsVip().OrderBy(r => r.Value).Select(r => r.Value).ToArray()
            };
        }

        #endregion

        public ActionResult Index()
        {
            return View("Index");
        }

        #region Create / Edit 

        public ActionResult Create(int? districtId, int? wardId, int? streetId, string addressNumber,
            string addressCorner)
        {
            var property = Services.ContentManager.New<PropertyPart>("Property");
            dynamic model = Services.ContentManager.BuildEditor(property);
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/EstimateCreate",
                Model: BuildCreateViewModel(districtId, wardId, streetId, addressNumber, addressCorner), Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(EstimateCreateViewModel createModel, FormCollection frmCollection)
        {

            var user = Services.WorkContext.CurrentUser;

            if (user != null)
            {
                #region Validate User Properties
                PropertyPart r = _propertyService.GetUserPropertyByAddress(0, createModel.ProvinceId, createModel.DistrictId, createModel.WardId, createModel.StreetId, 
                                createModel.ApartmentId, createModel.AddressNumber, createModel.AddressCorner, createModel.ApartmentNumber, "ad-selling");
                if (r != null)
                {
                    AddModelError("AddressNumber", T("BĐS <a href='{0}'>{1}</a> đã có trong tài sản của bạn.", Url.Action("Edit", new { r.Id }), r.DisplayForAddress));
                }
                #endregion
            }
            else
            {
                user = _membershipService.GetUser("daiphuhung");
            }

            #region VALIDATION

            #region AreaTotal, AreaLegal

            //double areaTotal = _propertyService.CalcArea(createModel.AreaTotal, createModel.AreaTotalWidth,
            //createModel.AreaTotalLength, createModel.AreaTotalBackWidth);

            double areaLegal = _propertyService.CalcArea(createModel.AreaLegal, createModel.AreaLegalWidth,
                createModel.AreaLegalLength, createModel.AreaLegalBackWidth);

            double areaTotal = areaLegal;
            if (createModel.AreaIlegal > 0) areaTotal += (double)createModel.AreaIlegal;

            // AreaTotal & AreaLegal
            //if (areaTotal > 0 && areaLegal > 0 && areaLegal > areaTotal)
            //{
            //    AddModelError("AreaLegal", T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
            //}

            // AreaIlegalRecognized
            //if (createModel.AreaIlegalRecognized.HasValue)
            //{
            //    if ((areaTotal - areaLegal) < createModel.AreaIlegalRecognized)
            //    {
            //        AddModelError("AreaIlegalRecognized",
            //            T(
            //                "Diện tích đất vi phạm lộ giới (quy hoạch) phải nhỏ hơn (Diện tích khuôn viên - Diện tích hợp quy hoạch)."));
            //    }
            //}

            // AreaConstruction
            if (createModel.AreaConstruction > 0)
            {
                if (areaTotal < createModel.AreaConstruction)
                {
                    AddModelError("AreaConstruction",
                        T("Diện tích xây dựng phải nhỏ hơn hoặc bằng Diện tích khuôn viên đất."));
                }
            }

            #endregion

            #region Location

            if (!string.IsNullOrEmpty(createModel.LocationCssClass) && createModel.LocationCssClass == "h-alley")
            {
                // DistanceToStreet
                if (createModel.DistanceToStreet.HasValue == false)
                {
                    AddModelError("DistanceToStreet", T("Vui lòng nhập khoảng cách từ BĐS ra MT đường"));
                }
                // AlleyTurns
                if (!createModel.AlleyTurns.HasValue)
                {
                    AddModelError("AlleyTurns", T("Vui lòng nhập số lần rẽ (quẹo) từ MT đường vào đến BĐS"));
                }
                else
                {
                    if (createModel.AlleyTurns < 1) createModel.AlleyTurns = 1;
                    if (createModel.AlleyTurns > 6) createModel.AlleyTurns = 6;
                    if (createModel.AlleyTurns >= 1 && !createModel.AlleyWidth1.HasValue)
                        AddModelError("AlleyWidth1", T("Vui lòng nhập Lần rẽ thứ 1 (Hẻm đầu tiên)"));
                    if (createModel.AlleyTurns >= 2 && !createModel.AlleyWidth2.HasValue)
                        AddModelError("AlleyWidth2", T("Vui lòng nhập Lần rẽ thứ 2"));
                    if (createModel.AlleyTurns >= 3 && !createModel.AlleyWidth3.HasValue)
                        AddModelError("AlleyWidth3", T("Vui lòng nhập Lần rẽ thứ 3"));
                    if (createModel.AlleyTurns >= 4 && !createModel.AlleyWidth4.HasValue)
                        AddModelError("AlleyWidth4", T("Vui lòng nhập Lần rẽ thứ 4"));
                    if (createModel.AlleyTurns >= 5 && !createModel.AlleyWidth5.HasValue)
                        AddModelError("AlleyWidth5", T("Vui lòng nhập Lần rẽ thứ 5"));
                    if (createModel.AlleyTurns >= 6 && !createModel.AlleyWidth6.HasValue)
                        AddModelError("AlleyWidth6", T("Vui lòng nhập Lần rẽ thứ 6"));
                }
            }

            #endregion

            #endregion

            #region CREATE RECORD

            var p = Services.ContentManager.New<PropertyPart>("Property");

            if (ModelState.IsValid)
            {
                #region RECORD

                DateTime createdDate = DateTime.Now;

                UserPartRecord createdUser = user.As<UserPart>().Record;

                #region Type

                // Ads Type
                p.AdsType = _propertyService.GetAdsType("ad-selling");

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
                p.Ward = _addressService.GetWard(createModel.WardId);

                // Street
                LocationStreetPartRecord street = _addressService.GetStreet(createModel.StreetId);
                p.Street = street;

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

                // Street Segment
                LocationStreetPartRecord segmentStreet = _addressService.GetStreet(street, p.AlleyNumber);
                if (segmentStreet != null)
                    p.Street = segmentStreet;

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

                if (p.Location.CssClass == "h-front")
                {
                    p.StreetWidth = createModel.StreetWidth;
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

                #endregion

                #region Area

                // AreaLegal
                p.AreaLegal = createModel.AreaLegal;
                p.AreaLegalWidth = createModel.AreaLegalWidth;
                p.AreaLegalLength = createModel.AreaLegalLength;
                p.AreaLegalBackWidth = createModel.AreaLegalBackWidth;

                // AreaTotal
                p.AreaTotal = createModel.AreaLegal;
                if (createModel.AreaIlegal > 0) p.AreaTotal += createModel.AreaIlegal;

                #endregion

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
                    p.AreaConstruction = createModel.AreaConstruction;
                    p.AreaConstructionFloor = createModel.AreaConstructionFloor;

                    p.Floors = createModel.Floors;
                    p.Bedrooms = createModel.Bedrooms;
                    p.Livingrooms = createModel.Livingrooms;
                    p.Bathrooms = createModel.Bathrooms;
                    p.Balconies = createModel.Balconies;

                    p.TypeConstruction = _propertyService.GetTypeConstruction(createModel.TypeConstructionId);
                    p.Interior = _propertyService.GetInterior(createModel.InteriorId);
                    p.RemainingValue = createModel.RemainingValue;

                    p.HaveBasement = createModel.HaveBasement;
                    p.HaveMezzanine = createModel.HaveMezzanine;
                    p.HaveTerrace = createModel.HaveTerrace;
                    p.HaveGarage = createModel.HaveGarage;
                    p.HaveElevator = createModel.HaveElevator;
                    p.HaveSwimmingPool = createModel.HaveSwimmingPool;
                    p.HaveGarden = createModel.HaveGarden;
                    p.HaveSkylight = createModel.HaveSkylight;
                }

                #endregion

                #region Contact

                // Contact
                p.ContactName = createModel.ContactName;
                p.ContactPhone = createModel.ContactPhone;
                p.ContactAddress = createModel.ContactAddress;
                p.ContactEmail = createModel.ContactEmail;

                #endregion

                #region Price

                //// Price
                p.PriceProposed = -1;
                p.PriceProposedInVND = -1;
                p.PaymentMethod = _propertyService.GetPaymentMethod("pm-vnd-b");
                p.PaymentUnit = _propertyService.GetPaymentUnit("unit-total");

                #endregion

                #region Flag & Status

                p.Published = false;
                p.Status = _propertyService.GetStatus("st-estimate");
                p.Flag = _propertyService.GetFlag("deal-unknow");
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

                #region OtherAdvantages & OtherDisAdvantages

                p.OtherAdvantages = createModel.OtherAdvantages;
                p.OtherAdvantagesDesc = createModel.OtherAdvantagesDesc;
                p.OtherDisAdvantages = createModel.OtherDisAdvantages;
                p.OtherDisAdvantagesDesc = createModel.OtherDisAdvantagesDesc;

                #endregion

                #endregion

                Services.ContentManager.Create(p);

                // IdStr
                p.IdStr = p.Id.ToString(CultureInfo.InvariantCulture);

                // Area for filter only
                p.Area = _propertyService.CalcAreaForFilter(p);

                // AreaUsable
                p.AreaUsable = _propertyService.CalcAreaUsable(p);

                // Advantages
                _propertyService.UpdatePropertyAdvantages(p, createModel.Advantages);

                // DisAdvantages
                _propertyService.UpdatePropertyDisAdvantages(p, createModel.DisAdvantages);

                Services.ContentManager.UpdateEditor(p, this);

            }

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                dynamic model = Services.ContentManager.UpdateEditor(p, this);
                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/EstimateCreate",
                    Model: BuildCreateViewModel(createModel), Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            return RedirectToAction("Edit", new { p.Id });
        }

        #region Edit

        public async System.Threading.Tasks.Task<ActionResult> Edit(int id)
        {
            #region GET RECORD

            var p = Services.ContentManager.Get<PropertyPart>(id);

            // transfer to current login user
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            if (user != null)
            {
                if (p.CreatedUser.UserName == "daiphuhung")
                {
                    p.CreatedUser = user.Record;
                    p.LastUpdatedUser = user.Record;
                    p.FirstInfoFromUser = user.Record;
                    p.LastInfoFromUser = user.Record;
                }
            }

            #region SECURITY

            if (_fastfilterService.IsEditable(id) == false)
            {
                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
            }

            #endregion

            EstimateEditViewModel editModel = BuildEditViewModel(p);

            #endregion

            #region ESTIMATE

            if (editModel.IsValidForEstimate)
            {
                var entry = await _propertyService.EstimateProperty(p.Id);
                p.PriceEstimatedInVND = entry.PriceEstimatedInVND;
            }
            else
            {
                p.PriceEstimatedInVND = null;
            }

            #endregion

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/EstimateEdit", Model: editModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(p);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id, FormCollection frmCollection, IEnumerable<HttpPostedFileBase> files)
        {

            var user = Services.WorkContext.CurrentUser;

            var p = Services.ContentManager.Get<PropertyPart>(id);

            #region SECURITY

            // Đăng tin
            if (!string.IsNullOrEmpty(frmCollection["submit.Create"]))
            {
                if (user == null)
                {
                    return RedirectToAction("AccessDenied", "Account", new { area = "Orchard.Users", returnUrl = Request.RawUrl });
                }
            }

            if (_fastfilterService.IsEditable(id) == false)
            {
                if (user == null)
                {
                    return RedirectToAction("AccessDenied", "Account", new { area = "Orchard.Users", returnUrl = Request.RawUrl });
                }
                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
            }

            #endregion

            dynamic model = Services.ContentManager.UpdateEditor(p, this);

            var editModel = new EstimateEditViewModel { Property = p };

            if (TryUpdateModel(editModel))
            {
                #region VALIDATION

                #region AddressNumber

                // AddressNumber
                if (!string.IsNullOrEmpty(frmCollection["submit.Estimate"]))
                {
                    if (user != null)
                    {
                        if (
                            !_propertyService.VerifyUserPropertyUnicity(id, editModel.ProvinceId, editModel.DistrictId,
                                editModel.WardId, editModel.StreetId, editModel.ApartmentId, editModel.AddressNumber,
                                editModel.AddressCorner, editModel.ApartmentNumber, p.AdsType.CssClass))
                        {
                            PropertyPart r = _propertyService.GetUserPropertyByAddress(id, editModel.ProvinceId,
                                editModel.DistrictId, editModel.WardId, editModel.StreetId, editModel.ApartmentId,
                                editModel.AddressNumber, editModel.AddressCorner, editModel.ApartmentNumber,
                                p.AdsType.CssClass);
                            AddModelError("AddressNumber",
                                T("BĐS <a href='{0}'>{1}</a> đã có trong tài sản của bạn.",
                                    Url.Action("Edit", new { r.Id }), r.DisplayForAddress));
                        }
                    }
                }

                #endregion

                #region AreaTotal, AreaLegal

                //double areaTotal = _propertyService.CalcArea(editModel.AreaTotal, editModel.AreaTotalWidth,
                //    editModel.AreaTotalLength, editModel.AreaTotalBackWidth);

                double areaLegal = _propertyService.CalcArea(editModel.AreaLegal, editModel.AreaLegalWidth,
                    editModel.AreaLegalLength, editModel.AreaLegalBackWidth);

                double areaTotal = areaLegal;
                if (editModel.AreaIlegal > 0) areaTotal += (double)editModel.AreaIlegal;

                // AreaTotal & AreaLegal
                //if (areaTotal > 0 && areaLegal > 0 && areaLegal > areaTotal)
                //{
                //    AddModelError("AreaLegal", T("Diện tích hợp quy hoạch phải nhỏ hơn hoặc bằng Diện tích khuôn viên."));
                //}

                // AreaIlegalRecognized
                //if (editModel.AreaIlegalRecognized.HasValue)
                //{
                //    if ((areaTotal - areaLegal) < editModel.AreaIlegalRecognized)
                //    {
                //        AddModelError("AreaIlegalRecognized",
                //            T(
                //                "Diện tích đất vi phạm lộ giới (quy hoạch) phải nhỏ hơn (Diện tích khuôn viên - Diện tích hợp quy hoạch)."));
                //    }
                //}

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

                #region Location

                if (!string.IsNullOrEmpty(editModel.LocationCssClass) && editModel.LocationCssClass.Contains("h-alley"))
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

                #endregion

                #region UPDATE RECORD

                if (ModelState.IsValid)
                {
                    #region Type

                    // Type
                    p.Type = _propertyService.GetType(editModel.TypeId);
                    p.TypeGroup = p.Type.Group;

                    #endregion

                    #region Address

                    // Province
                    p.Province = _addressService.GetProvince(editModel.ProvinceId);

                    // District
                    p.District = _addressService.GetDistrict(editModel.DistrictId);

                    // Ward
                    p.Ward = _addressService.GetWard(editModel.WardId);

                    // Street
                    LocationStreetPartRecord street = _addressService.GetStreet(editModel.StreetId);
                    p.Street = street;

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

                    // Street Segment
                    LocationStreetPartRecord segmentStreet = _addressService.GetStreet(street, p.AlleyNumber);
                    if (segmentStreet != null)
                        p.Street = segmentStreet;

                    #endregion

                    #region Legal, Direction, Location

                    // LegalStatus
                    p.LegalStatus = _propertyService.GetLegalStatus(editModel.LegalStatusId);

                    // Direction
                    p.Direction = _propertyService.GetDirection(editModel.DirectionId);

                    // Location
                    p.Location = _propertyService.GetLocation(editModel.LocationCssClass);

                    #endregion

                    #region Alley

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

                    #endregion

                    #region Area

                    // AreaTotal
                    p.AreaTotal = editModel.AreaLegal;
                    if (editModel.AreaIlegal > 0) p.AreaTotal += editModel.AreaIlegal;

                    #endregion

                    #region Construction

                    if (p.Type.CssClass == "tp-residential-land")
                    {
                        p.AreaConstruction = null;
                        p.AreaConstructionFloor = null;

                        p.Floors = null;
                        p.Bedrooms = null;
                        p.Bathrooms = null;
                        p.Livingrooms = null;
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
                        p.TypeConstruction = _propertyService.GetTypeConstruction(editModel.TypeConstructionId);
                        p.Interior = _propertyService.GetInterior(editModel.InteriorId);
                    }

                    #endregion

                    #region Price

                    // PaymentMethod
                    p.PaymentMethod = _propertyService.GetPaymentMethod(editModel.PaymentMethodId);

                    // PaymentUnit`
                    p.PaymentUnit = _propertyService.GetPaymentUnit(editModel.PaymentUnitId);

                    if (p.PriceProposed > 0)
                    {
                        // PriceProposedInVND
                        p.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(p);
                    }

                    #endregion

                    #region User

                    // User
                    p.LastUpdatedDate = DateTime.Now;
                    if (user != null)
                    {
                        var lastUpdatedUser = user.As<UserPart>();
                        p.LastUpdatedUser = lastUpdatedUser.Record;
                        p.LastInfoFromUser = lastUpdatedUser.Record;
                    }

                    #endregion

                    #region Group

                    if (p.UserGroup == null)
                    {
                        p.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
                    }

                    #endregion

                    // Area for filter only
                    p.Area = _propertyService.CalcAreaForFilter(p);

                    // AreaUsable
                    p.AreaUsable = _propertyService.CalcAreaUsable(p);

                    // Advantages
                    _propertyService.UpdatePropertyAdvantages(p, editModel.Advantages);

                    // DisAdvantages
                    _propertyService.UpdatePropertyDisAdvantages(p, editModel.DisAdvantages);

                    #region Đăng tin

                    // Đăng tin
                    if (!string.IsNullOrEmpty(frmCollection["submit.Create"]))
                    {
                        const string format = "dd/MM/yyyy";
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                        DateTime adsDateFrom, adsDateTo;

                        DateTime.TryParseExact(editModel.DateVipFrom, format, provider, style, out adsDateFrom);
                        DateTime.TryParseExact(editModel.DateVipTo, format, provider, style, out adsDateTo);

                        #region Create Validation

                        if (_adsPaymentService.CheckIsValidVip(editModel.AdsTypeVIP, editModel.DistrictId, "ad-selling")) // editModel.AdsVIPRequest
                        {
                            if (editModel.AdsTypeVIP < 0 && editModel.AdsTypeVIP > 3)
                            {
                                AddModelError("NotExistAdsTypeVIP", T("Giá trị tin VIP không đúng."));
                            }
                            else
                            {
                                if (!_adsPaymentService.CheckHaveMoney(false, 0, null, editModel.AdsTypeVIP,
                                        (int)(adsDateTo - adsDateFrom).TotalDays))
                                    AddModelError("NotEnoughMoney",
                                        T(
                                            "Số tiền của bạn không đủ để thực hiện tin này, Vui lòng liên hệ BQT hoặc nạp tiền thêm để tiếp tục đăng tin."));
                            }

                            if (adsDateTo <= adsDateFrom)
                            {
                                AddModelError("NotValidDays", T("Ngày hết hạn tin VIP của bạn phải lớn hơn ngày bắt đầu."));
                            }
                        }

                        #endregion

                        if (ModelState.IsValid)
                        {
                            if (user != null)
                            {
                                p.Published = true;
                                p.PublishAddress = true;
                                p.Status = _propertyService.GetStatus("st-pending");

                                p.AdsExpirationDate = DateExtension.GetEndOfDate(adsDateTo);

                                p.IsOwner = editModel.IsOwner;
                                p.NoBroker = editModel.NoBroker;

                                #region AdsVIPRequest

                                if (_adsPaymentService.CheckIsValidVip(editModel.AdsTypeVIP, editModel.DistrictId, "st-estimate"))
                                {
                                    p.AdsVIPRequest = true;
                                    p.AdsVIPExpirationDate = DateExtension.GetEndOfDate(adsDateTo);
                                    p.SeqOrder = editModel.AdsTypeVIP;

                                    // Update Payment History
                                    //_adsPaymentService.UpdatePaymentHistoryV2("st-estimate", 0, false, null, p,
                                    //    user.As<UserPart>(), editModel.AdsTypeVIP, (int)(adsDateTo - adsDateFrom).TotalDays);

                                    _adsPaymentService.UpdatePaymentHistoryV2("st-estimate", 0,// false,
                                        null, p, user.As<UserPart>(), editModel.AdsTypeVIP, (int)(adsDateTo - adsDateFrom).TotalDays);
                                }

                                if (p.UserGroup != null)
                                    _propertyService.UpdateOrderByDomainGroup(p, p.UserGroup.Id);

                                Services.Notifier.Information(
                                    T(
                                        "Tin rao <a href='{0}'>{1}</a> đã đăng thành công. Chúng tôi sẽ duyệt tin và đưa lên trang web trong thời gian 24h.",
                                        Url.Action("Edit", new { p.Id }), p.DisplayForAddress));

                                #endregion

                                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });
                            }
                        }
                    }

                    #endregion

                    Services.Notifier.Information(T("BĐS <a href='{0}'>{1}</a> cập nhật thành công.",
                        Url.Action("Edit", new { p.Id }), p.DisplayForAddress));

                    #region ESTIMATE

                    // Clear UnitPrice in Cache
                    _propertyService.ClearApplicationCache(id);

                    #endregion
                }

                #endregion
            }

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = BuildEditViewModel(p);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/EstimateEdit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            return RedirectToAction("Edit", new { id });
        }

        #endregion

        [HttpPost]
        public ActionResult AjaxRating(int id, double ratingPoint, string comment)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);
            Services.ContentManager.UpdateEditor(p, this);
            p.PriceEstimatedRatingPoint = ratingPoint;
            p.PriceEstimatedComment = comment;
            return Json(new { point = p.PriceEstimatedRatingPoint, description = p.PriceEstimatedComment });
        }

        #endregion

    }
}