using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml.Linq;
using Orchard;
using Orchard.Alias.Implementation.Holder;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.Services;

namespace RealEstate.Admin.Controllers
{
    [Themed]
    public class HomeController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly IAliasHolder _aliasHolder;
        private readonly ICustomerService _customerService;
        private readonly IRepository<CustomerPropertyRecord> _customerpropertyRepository;
        private readonly IUserGroupService _groupService;
        private readonly IPostAdminService _postAdminService;
        private readonly IPropertyService _propertyService;
        private readonly IRevisionService _revisionService;
        private readonly ISignals _signals;
        private readonly IStreetRelationService _streetRelationService;
        private readonly IThreadAdminService _threadService;
        private readonly IRepository<UnEstimatedLocationRecord> _unEstimatedLocationRepository;
        private readonly ILocationApartmentService _apartmentService;
        private readonly IRepository<UserLocationRecord> _userLocationRepository;
        private readonly IHostNameService _hostNameService;
        private readonly IGoogleApiService _googleApiService;

        public HomeController(
            IAddressService addressService,
            IUserGroupService groupService,
            IPropertyService propertyService,
            ICustomerService customerService,
            IRevisionService revisionService,
            IStreetRelationService streetRelationService,
            IRepository<UnEstimatedLocationRecord> unEstimatedLocationRepository,
            IRepository<CustomerPropertyRecord> customerpropertyRepository,
            IRepository<UserLocationRecord> userLocationRepository,
            IHostNameService hostNameService,
            IPostAdminService postAdminService,
            IThreadAdminService threadService,
            ISignals signals,
            IAliasHolder aliasHolder,
            IOrchardServices services,
            IGoogleApiService googleApiService,
            ILocationApartmentService apartmentService)
        {
            _groupService = groupService;
            _addressService = addressService;
            _propertyService = propertyService;
            _customerService = customerService;
            _revisionService = revisionService;
            _streetRelationService = streetRelationService;
            _unEstimatedLocationRepository = unEstimatedLocationRepository;
            _customerpropertyRepository = customerpropertyRepository;
            _userLocationRepository = userLocationRepository;
            _postAdminService = postAdminService;
            _threadService = threadService;
            _hostNameService = hostNameService;
            _aliasHolder = aliasHolder;
            _signals = signals;
            _apartmentService = apartmentService;
            _googleApiService = googleApiService;

            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index()
        {
            return View("Index");
        }

        #region SITEMAP

        public ContentResult Sitemap(int? count)
        {
            IOrderedEnumerable<AliasInfo> aliases =
                _aliasHolder.GetMaps().SelectMany(x => x.GetAliases()).OrderBy(a => a.Path);

            IEnumerable<AliasInfo> result = count > 0 ? aliases.Take((int)count) : aliases.ToList();

            List<SitemapExtension.SitemapUrl> urls = result.Select(a => new SitemapExtension.SitemapUrl
            {
                loc = a.Path,
                lastmod = String.Format("{0:yyyy-MM-dd}", DateTime.Now),
                changefreq = "daily",
                priority = "1"
            }).ToList();

            XDocument sitemap = SitemapExtension.GenerateSitemap(urls);

            return Content(sitemap.ToString(), "text/xml");
        }

        public ContentResult SitemapProperties(int? count)
        {
            IContentQuery<PropertyPart, PropertyPartRecord> items = _propertyService.GetProperties();

            IEnumerable<PropertyPart> result = count > 0 ? items.Slice((int)count) : items.List();

            List<SitemapExtension.SitemapUrl> urls = result.Select(a => new SitemapExtension.SitemapUrl
            {
                loc = "tin-" + a.DisplayForUrl + "-" + a.Id,
                lastmod = String.Format("{0:yyyy-MM-dd}", a.LastUpdatedDate),
                changefreq = "daily",
                priority = "0.69"
            }).ToList();

            XDocument sitemap = SitemapExtension.GenerateSitemap(urls);

            return Content(sitemap.ToString(), "text/xml");

            //XElement root = XElement.Parse(
            //@"<Canvas a='1'>
            //  <Grid>
            //    <TextBlock>
            //      <Run Text='r'/>
            //      <Run Text='u'/>
            //      <Run Text='n'/>
            //    </TextBlock>
            //    <TextBlock>
            //      <Run Text='far a'/>
            //      <Run Text='way'/>
            //      <Run Text=' from me'/>
            //    </TextBlock>
            //  </Grid>
            //  <Grid>
            //    <TextBlock>
            //      <Run Text='I'/>
            //      <Run Text=' '/>
            //      <Run Text='want'/>
            //      <LineBreak/>
            //    </TextBlock>
            //    <TextBlock>
            //      <LineBreak/>
            //      <Run Text='...thi'/>
            //      <Run Text='s to'/>
            //      <LineBreak/>
            //      <Run Text=' work'/>
            //    </TextBlock>
            //  </Grid>
            //</Canvas>");
            //return Content(SitemapExtension.ToStringWithCustomWhiteSpace(sitemap.Root), "text/xml");
        }

        public ActionResult SitemapForum(int? count)
        {
            IContentQuery<ForumPostPart, ForumPostPartRecord> items =
                _postAdminService.GetListPostQueryByHostName("dinhgianhadat.vn");

            IEnumerable<ForumPostPart> result = count > 0 ? items.Slice((int)count) : items.List();

            List<SitemapExtension.SitemapUrl> urls =
                result.Select(a => a.Thread.ParentThreadId != null
                    ? new SitemapExtension.SitemapUrl
                    {
                        loc =
                            "dien-dan/" +
                            _threadService.GetThreadInfoById("dinhgianhadat.vn", a.Thread.ParentThreadId.Value)
                                .ShortName + "/" + a.DisplayForUrl + "-" + a.Id,
                        lastmod =
                            String.Format("{0:yyyy-MM-dd}", a.DateUpdated.HasValue ? a.DateUpdated : a.DateCreated),
                        changefreq = "daily",
                        priority = "0.69"
                    }
                    : null).ToList();

            XDocument sitemap = SitemapExtension.GenerateSitemap(urls);

            return Content(sitemap.ToString(), "text/xml");
        }

        public ActionResult SitemapBlogs(int? count)
        {
            IEnumerable<UserPart> items = _groupService.GetUsers();

            IEnumerable<UserPart> result = count > 0 ? items.Take((int)count) : items;

            List<SitemapExtension.SitemapUrl> urls =
                result.Select(a => new SitemapExtension.SitemapUrl //blog-ca-nhan/{UserName}-{UserId}
                {
                    loc = "blog-ca-nhan/" + a.UserName.ToSlug() + "-" + a.Id,
                    lastmod = String.Format("{0:yyyy-MM-dd}", DateTime.Now),
                    changefreq = "daily",
                    priority = "0.69"
                }).ToList();

            XDocument sitemap = SitemapExtension.GenerateSitemap(urls);

            return Content(sitemap.ToString(), "text/xml");
        }

        #endregion

        #region GET JSON

        public ActionResult GetPropertyTypeIdsForJson(int typeGroupId)
        {
            List<SelectListItem> lstTypes =
                _propertyService.GetTypes().Where(a => a.Group.Id == typeGroupId).OrderBy(a => a.SeqOrder)
                    .Select(a => new { a.Name, a.Id }).ToList()
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                    .ToList();
            return Json(new { list = lstTypes }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPropertyTypesForJson(string adsTypeCssClass, string typeGroupCssClass)
        {
            List<SelectListItem> lstTypes =
                _propertyService.GetTypes(adsTypeCssClass, typeGroupCssClass).OrderBy(a => a.SeqOrder)
                    .Select(a => new { a.Name, a.Id }).ToList()
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                    .ToList();
            return Json(new { list = lstTypes }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPropertyTypeConstructionsForJson(int? typeId, double? floors)
        {
            List<SelectListItem> lstTypeConstructions =
                _propertyService.GetTypeConstructions(typeId, floors).OrderBy(a => a.SeqOrder)
                    .Select(a => new { a.Name, a.Id }).ToList()
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                    .ToList();
            return
                Json(
                    new
                    {
                        list = lstTypeConstructions,
                        count = _propertyService.GetTypeConstructions(typeId, floors).Count()
                    }, JsonRequestBehavior.AllowGet);
        }

        // District

        public ActionResult GetDistrictsForJson(int provinceId, int? userId)
        {
            IEnumerable<LocationDistrictPart> lstDistricts = new List<LocationDistrictPart>();

            if (userId > 0)
            {
                var currentUser = Services.ContentManager.Get<UserPart>((int)userId);
                if (currentUser != null)
                {
                    lstDistricts =
                        _groupService.GetUserEnableEditLocationDistricts(currentUser, provinceId)
                            .OrderBy(a => a.SeqOrder);
                }
            }
            else
            {
                lstDistricts = _addressService.GetDistricts(provinceId).OrderBy(a => a.SeqOrder);
            }

            List<SelectListItem> result = lstDistricts.Select(a => new { a.Name, a.Id })
                .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                .ToList();

            return Json(new { list = result }, JsonRequestBehavior.AllowGet);
        }

        // Ward

        public ActionResult GetWardsForJson(int districtId)
        {
            return Json(new { list = _addressService.GetSelectListWards(districtId) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWardsByDistrictsForJson(int[] districtIds)
        {
            return Json(new { list = _addressService.GetSelectListWards(districtIds) }, JsonRequestBehavior.AllowGet);
        }

        // Street

        public ActionResult GetStreetsForJson(int districtId)
        {
            return Json(new { list = _addressService.GetSelectListStreets(districtId) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStreetsByDistrictsForJson(int[] districtIds)
        {
            return Json(new { list = _addressService.GetSelectListStreets(districtIds) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStreetsByProvinceForJson(int provinceId, string selectedStreetId)
        {
            return Json(new { list = _addressService.GetSelectListStreetsByProvince(provinceId) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllStreetsForJson(int districtId)
        {
            return Json(new { list = _addressService.GetSelectListAllStreets(districtId) }, JsonRequestBehavior.AllowGet);
        }

        // Apartment

        public ActionResult GetApartmentsForJson(int districtId)
        {
            return Json(new { list = _addressService.GetSelectListApartments(districtId) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetApartmentsByDistrictsForJson(int[] districtIds)
        {
            return Json(new { list = _addressService.GetSelectListApartments(districtIds) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetApartmentBlocksForJson(int apartmentId)
        {
            List<SelectListItem> lstApartmentBlocks = _apartmentService.LocationApartmentBlockParts(apartmentId).OrderBy(a => a.BlockName)
                .Select(
                    a =>
                        new SelectListItem { Text = a.BlockName, Value = a.Id.ToString(CultureInfo.InvariantCulture) })
                .ToList();
            return Json(new { list = lstApartmentBlocks }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SearchApartmentsForJson(string term)
        {
            List<string> lstApartments =
                Services.ContentManager.Query<LocationApartmentPart, LocationApartmentPartRecord>()
                    .Where(a => a.Name.Contains(term)).OrderBy(a => a.Name).List().Select(a => a.Name).ToList();
            return Json(new { list = lstApartments }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetApartmentInfoForJson(int apartmentId, int propertyId = 0)
        {
            var apartment = Services.ContentManager.Get<LocationApartmentPart>(apartmentId);
            var property = Services.ContentManager.Get<PropertyPart>(propertyId);

            if (apartment == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            List<string> advantageCssClass =
                _propertyService.GetAdvantagesForApartment(apartment)
                    .Where(a => a.ShortName == "apartment-adv")
                    .Select(a => a.CssClass).ToList();

            var avatarImage = _addressService.GetApartmentFiles(apartment, property).List()
                .Select(r => new { r.Path, r.Id, r.IsAvatar }).ToList();

            string note = "";
            if (!string.IsNullOrEmpty(apartment.Investors)) note += "\n- Chủ đầu tư: " + apartment.Investors;
            if (!string.IsNullOrEmpty(apartment.CurrentBuildingStatus))
                note += "\n- Hiện trạng: " + apartment.CurrentBuildingStatus;
            if (!string.IsNullOrEmpty(apartment.ManagementFees))
                note += "\n- Phí quản lý: " + apartment.ManagementFees;
            if (apartment.AreaTotal.HasValue) note += "\n- Diện tích khuôn viên: " + apartment.AreaTotal;
            if (apartment.AreaConstruction.HasValue)
                note += "\n- Diện tích sàn xây dựng: " + apartment.AreaConstruction;
            if (apartment.AreaGreen.HasValue) note += "\n- Diện tích công viên cây xanh: " + apartment.AreaGreen;
            if (apartment.AreaTradeFloors.HasValue)
                note += "\n- Diện tích sàn thương mại: " + apartment.AreaTradeFloors;
            if (apartment.AreaBasements.HasValue) note += "\n- Diện tích tầng hầm: " + apartment.AreaBasements;

            return Json(new
            {
                success = true,
                apartmentWardId = apartment.Ward != null ? apartment.Ward.Id : 0,
                apartmentStreetId = apartment.Street != null ? apartment.Street.Id : 0,
                apartmentAddressNumber = apartment.AddressNumber,
                apartmentList = avatarImage,
                apartmentFloors = apartment.Floors,
                apartmentTradeFloors = apartment.TradeFloors,
                apartmentElevators = apartment.Elevators,
                apartmentBasements = apartment.Basements,
                apartmentDescription = apartment.Description + note,
                apartmentHaveChildcare = advantageCssClass.Contains("apartment-adv-Childcare"),
                apartmentHavePark = advantageCssClass.Contains("apartment-adv-Park"),
                apartmentHaveSwimmingPool = advantageCssClass.Contains("apartment-adv-SwimmingPool"),
                apartmentHaveSuperMarket = advantageCssClass.Contains("apartment-adv-SuperMarket"),
                apartmentHaveSportCenter = advantageCssClass.Contains("apartment-adv-SportCenter"),
                apartmentHaveHospital = advantageCssClass.Contains("apartment-adv-Hospital"),
                apartmentHavePublicArea = advantageCssClass.Contains("apartment-adv-PublicArea"),
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AjaxGetApartmentBlockInfo(int apartmentBlockId)
        {
            var apartmentBlock = _apartmentService.LocationApartmentBlockPart(apartmentBlockId);
            return Json(new { floor = apartmentBlock.FloorTotal, groupFloor = apartmentBlock.GroupFloorInBlockTotal }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetInfoAgenciesForJson(int? userId, int? groupId)
        {
            if (string.IsNullOrEmpty(userId.ToString()) || string.IsNullOrEmpty(groupId.ToString()))
                return Json(new { part = "null" }, JsonRequestBehavior.AllowGet);

            UserLocationRecord userLocations =
               _userLocationRepository.Fetch(a => a.UserPartRecord.Id == userId && a.UserGroupRecord.Id == groupId).FirstOrDefault();

            if (userLocations != null)
                return Json(new { part = new { EnableIsAgencies = userLocations.EnableIsAgencies, EndDateAgencing = String.Format("{0: dd/MM/yyyy}", userLocations.EndDateAgencing), AreaAgencies = userLocations.AreaAgencies, IsSelling = userLocations.IsSelling, IsLeasing = userLocations.IsLeasing } }, JsonRequestBehavior.AllowGet);
            return Json(new { part = "null" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AjaxGetListApartmentBlockInfo(int blockId)
        {
            return Json(new { list = _apartmentService.GetListApartmentBlockInfo(blockId) }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region STREET RELATION

        // Validate Street Relation
        [HttpPost]
        public ActionResult AjaxValidateStreetRelation(int id, int districtId, int wardId, int streetId)
        {
            bool exists;

            if (id > 0)
                exists = !_streetRelationService.VerifyStreetUnicity(id, streetId, wardId, districtId);
            else
                exists = !_streetRelationService.VerifyStreetUnicity(streetId, wardId, districtId);

            if (exists)
            {
                StreetRelationPart p = _streetRelationService.GetStreetRelation(districtId, wardId, streetId);
                string name = p.DisplayForStreetName;
                string link = Url.Action("Edit", "StreetRelationAdmin", new { p.Id });

                var result = new { exists = true, id = p.Id, name, link };
                return Json(result);
            }

            return Json(new { exists = false });
        }

        // Validate Street Relation Related
        [HttpPost]
        public ActionResult AjaxIsValidStreetRelation(int districtId, int wardId, int streetId, int relatedDistrictId,
            int relatedWardId, int relatedStreetId)
        {
            bool isValid = _streetRelationService.IsValidRelation(streetId, wardId, districtId, relatedStreetId,
                relatedWardId, relatedDistrictId);

            if (isValid) return Json(new { valid = true });

            StreetRelationPart p = _streetRelationService.GetStreetRelation(districtId, wardId, streetId);
            string name = p.DisplayForStreetName;
            string link = Url.Action("Edit", "StreetRelationAdmin", new { p.Id });

            var result = new { isvalid = false, id = p.Id, name, link };
            return Json(result);
        }

        // Get CoefficientAlley
        [HttpPost]
        public ActionResult AjaxGetCoefficientAlley(int id)
        {
            var c = Services.ContentManager.Get<CoefficientAlleyPart>(id);
            if (c == null) return Json(new { id, success = false });
            object results =
                new
                {
                    id,
                    success = true,
                    H1max = c.CoefficientAlley1Max,
                    H1min = c.CoefficientAlley1Min,
                    Hequal = c.CoefficientAlleyEqual,
                    Hmax = c.CoefficientAlleyMax,
                    Hmin = c.CoefficientAlleyMin
                };
            return Json(results);
        }

        #endregion

        #region ESTIMATE

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> IsEstimateable(int districtId, int wardId, int streetId, string addressNumber,
            string addressCorner)
        {
            bool isEstimateable = await _propertyService.IsEstimateable(districtId, wardId, streetId, addressNumber, addressCorner);
            if (isEstimateable) return Json(new { isEstimateable = true });
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

        #endregion

        #region PROPERTY FUNCTIONS

        // Validate Address

        [HttpPost]
        public ActionResult AjaxValidateAddress(int id, int provinceId, int districtId, int wardId, int streetId,
            int apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            bool exists;
            addressCorner = !string.IsNullOrEmpty(addressCorner) ? addressCorner : null;
            apartmentNumber = !string.IsNullOrEmpty(apartmentNumber) ? apartmentNumber : null;

            var property = Services.ContentManager.Get<PropertyPart>(id);
            if (property != null)
            {
                if (_propertyService.IsExternalProperty(property))
                {
                    // BĐS khách đăng không cần kt trùng
                    return Json(new { exists = false });
                }
            }
            exists =
                !_propertyService.VerifyPropertyUnicity(id, provinceId, districtId, wardId, streetId, apartmentId,
                    addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

            if (apartmentId > 0 && string.IsNullOrEmpty(apartmentNumber) && exists)
            {
                exists = false;
            }//Nếu là căn hộ chung cư mà ko nhập mã số căn hộ


            //var ward = Services.ContentManager.Get(wardId).As<LocationWardPart>();
            //var street = Services.ContentManager.Get(streetId).As<LocationStreetPart>();
            //address = string.Join(", ", addressNumber, addressCorner, street.Name, ward.Name, street.District.Name, street.Province.Name)
            if (!exists) return Json(new { exists = false });

            PropertyPart p = _propertyService.GetPropertyByAddress(id, provinceId, districtId, wardId, streetId, apartmentId,
                addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

            //var enableEditAddressNumber = Services.Authorizer.Authorize(Permissions.EditAddressNumber);
            bool enableEditContactPhone = true;
            string statusName = String.Empty;
            string statusCssClass = String.Empty;
            if (p.Status != null)
            {
                statusName = p.Status.Name;
                statusCssClass = p.Status.CssClass;
                if (statusCssClass == "st-negotiate")
                    enableEditContactPhone = Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties);
                if (statusCssClass == "st-trading")
                    enableEditContactPhone = Services.Authorizer.Authorize(Permissions.AccessTradingProperties);
            }

            string address = p.DisplayForAddressForOwner;

            // ReSharper disable once Mvc.AreaNotResolved
            string link = Url.Action("Edit", "PropertyAdmin", new { area = "RealEstate.Admin", p.Id });

            string info = "Trạng thái <strong class=" + statusCssClass + ">" + statusName + "</strong> " + "Giá " +
                          String.Format("{0:0.##}", p.PriceProposedInVND) + " Tỷ, "
                          + "Diện tích " + String.Format("{0:0.##}", p.AreaTotalWidth) + "x" +
                          String.Format("{0:0.##}", p.AreaTotalLength);
            if (enableEditContactPhone) info += ", Điện thoại " + p.ContactPhone;

            var result = new { exists = true, id = p.Id, address, link, info, statusCssClass };
            return Json(result);
        }

        [HttpPost]
        public ActionResult AjaxUserValidateAddress(int id, int provinceId, int districtId, int wardId, int streetId,
            int? apartmentId, string addressNumber, string addressCorner, string apartmentNumber, string adsTypeCssClass)
        {
            if (String.IsNullOrEmpty(adsTypeCssClass)) adsTypeCssClass = "ad-selling";
            addressCorner = !string.IsNullOrEmpty(addressCorner) ? addressCorner : null;
            apartmentNumber = !string.IsNullOrEmpty(apartmentNumber) ? apartmentNumber : null;

            bool propertyEstimate = false;
            bool propertyExchange = false;

            bool exists =
                    !_propertyService.VerifyUserPropertyUnicity(id, provinceId, districtId, wardId, streetId,
                        apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

            if (apartmentId > 0 && string.IsNullOrEmpty(apartmentNumber) && exists)
            {
                exists = false;
            } //Nếu là căn hộ chung cư mà ko nhập mã số căn hộ

            if (exists)
            {
                PropertyPart p = _propertyService.GetUserPropertyByAddress(id, provinceId, districtId, wardId, streetId,
                    apartmentId, addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

                string address = p.DisplayForAddressForOwner;

                string info = "Giá " + String.Format("{0:0.##}", p.PriceProposedInVND) + " Tỷ, "
                              + "Diện tích " + String.Format("{0:0.##}", p.AreaTotalWidth) + "x" +
                              String.Format("{0:0.##}", p.AreaTotalLength);
                info += ", Điện thoại " + p.ContactPhone;

                // ReSharper disable Mvc.ActionNotResolved
                // ReSharper disable Mvc.ControllerNotResolved
                // ReSharper disable Mvc.AreaNotResolved
                string link;
                if (p.Status.CssClass == "st-deleted")
                {
                    // Redirect to control-panel Đã xóa
                    link = Url.Action("Index", "User",
                        new
                        {
                            area = "RealEstate.UserControlPanel",
                            ReturnStatus = "del",
                            ProvinceId = provinceId,
                            DistrictIds = districtId,
                            StreetId = streetId
                        });
                }
                else
                {
                    switch (p.Status.CssClass)
                    {
                        case "st-estimate":
                            link = Url.Action("Edit", "Estimate", new { area = "RealEstate.FrontEnd", p.Id });
                            propertyEstimate = true;
                            break;
                        default:
                            link = Url.Action("Edit", "Home", new { area = "RealEstate.FrontEnd", p.Id });
                            break;
                    }
                    propertyExchange = _propertyService.ListOwnPropertyIdsExchange().Contains(id);
                }
                // ReSharper restore Mvc.AreaNotResolved
                // ReSharper restore Mvc.ControllerNotResolved
                // ReSharper restore Mvc.ActionNotResolved

                var result =
                    new
                    {
                        propertyEstimate,
                        propertyExchange,
                        exists = true,
                        id = p.Id,
                        address,
                        link,
                        info,
                        statusCssClass = p.Status.CssClass,
                        statusName = p.Status.Name
                    };
                return Json(result);
            }
            bool existsInternal =
                    !_propertyService.VerifyPropertyUnicity(id, provinceId, districtId, wardId, streetId, apartmentId,
                        addressNumber, addressCorner, apartmentNumber, adsTypeCssClass);

            return existsInternal
                ? Json(new { exists = false, existsInternal = true })
                : Json(new { exists = false, existsInternal = false });

            //var ward = Services.ContentManager.Get(wardId).As<LocationWardPart>();
            //var street = Services.ContentManager.Get(streetId).As<LocationStreetPart>();
            //address = string.Join(", ", addressNumber, addressCorner, street.Name, ward.Name, street.District.Name, street.Province.Name) 
            //return Json(new { exists });
        }

        [HttpPost]
        public ActionResult AjaxIsCopyable(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);

            bool isCopyable = _propertyService.IsValidForCopyToGroup(p) && _propertyService.IsValidToPublish(p);

            return Json(new { id, isCopyable });
        }

        public ActionResult AjaxCountPendingProperties(bool countPendingInDetails)
        {
            var data = _propertyService.CountPendingProperties(countPendingInDetails);
            return Content(_propertyService.BuildMessageString(data), "text/event-stream");
        }

        public ActionResult AjaxCountPendingCustomers(bool countPendingInDetails)
        {
            var data = _propertyService.CountPendingCustomers(countPendingInDetails);
            return Content(_propertyService.BuildMessageString(data), "text/event-stream");
        }

        public ActionResult AjaxDelYoutube(int propertyId)
        {
            try
            {
                var p = Services.ContentManager.Get<PropertyPart>(propertyId);
                if (p != null)
                    p.YoutubeId = null;

                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }

        public ActionResult AjaxUpdateSeqOrderGroupApartment(int seqOrder, int groupApartmentId)
        {
            try
            {
                var groupApartment = Services.ContentManager.Get<GroupInApartmentBlockPart>(groupApartmentId);

                if (groupApartment != null)
                {
                    groupApartment.SeqOrder = seqOrder;

                    return Json(new { status = true });
                }
                return Json(new { status = false, message = "Không tìm thấy group này!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        public ActionResult AjaxBookReservationApartment(int id)
        {
            string message = "Đặt giữ chỗ không thành công!";
            string status = "";

            try
            {
                var p = Services.ContentManager.Get<PropertyPart>(id);
                var user = Services.WorkContext.CurrentUser.As<UserPart>();
                int timeReservationLimit = 2; // Hours // TODO: config theo dự án
                int timeReservationLimitExtend = timeReservationLimit;

                if (p != null)
                {
                    status = p.Status.CssClass;

                    if (p.Status.CssClass != "st-sold") // căn hộ chưa bán
                    {
                        // Giữ chỗ trong vòng 2h + 1h
                        if (p.LastUpdatedUser.Id == user.Id)
                        {
                            timeReservationLimitExtend += 1; // Mỗi user được phép giữ chỗ liên tục cách nhau 1h
                            message = "Không được phép giữ chỗ liên tục trong vòng " + timeReservationLimitExtend + " giờ!";
                        }

                        var currentTime = DateTime.Now;

                        // Kiểm tra đủ điều kiện để giữ chỗ
                        if (p.Status.CssClass != "st-onhold")
                        {
                            if (p.StatusChangedDate == null || p.LastUpdatedUser.Id != user.Id || (currentTime - ((DateTime)p.StatusChangedDate).ToLocalTime()).TotalHours > timeReservationLimitExtend)
                            {
                                // được phép giữ chỗ
                                p.Status = _propertyService.GetStatus("st-onhold"); // Giữ chỗ
                                p.StatusChangedDate = currentTime;
                                p.LastUpdatedUser = user.Record;

                                message = "Đặt giữ chỗ thành công! Chỗ sẽ được giữ trong vòng " + timeReservationLimit + " giờ. Hết hạn vào lúc " + currentTime.AddHours(timeReservationLimit);
                                status = p.Status.CssClass;

                                return Json(new { success = true, message, status });
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, status });
            }

            return Json(new { success = false, message, status });
        }

        public ActionResult AjaxCancelReservationApartment(int id)
        {
            string message = "HỦY giữ chỗ không thành công!";
            string status = "";

            try
            {
                var p = Services.ContentManager.Get<PropertyPart>(id);
                var user = Services.WorkContext.CurrentUser.As<UserPart>();

                if (p != null)
                {
                    status = p.Status.CssClass;

                    // Kiểm tra đủ điều kiện để HỦY giữ chỗ
                    if (p.Status.CssClass == "st-onhold" && p.LastUpdatedUser.Id == user.Id)
                    {
                        var currentTime = DateTime.Now;

                        p.Status = _propertyService.GetStatus("st-selling"); // Rao bán
                        p.StatusChangedDate = currentTime;
                        p.LastUpdatedUser = user.Record;

                        message = "HỦY giữ chỗ thành công!";
                        status = p.Status.CssClass;

                        return Json(new { success = true, message, status });
                    }
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, status });
            }

            return Json(new { success = false, message, status });
        }

        #endregion

        #region FILES

        #region Upload Image

        [HttpPost]
        public ActionResult Upload(string token, HttpPostedFileBase fileBase, int contentItemId, int userId,
            bool isPublished)
        {
            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(token);
            if (ticket == null) throw new InvalidOperationException("The user is not authenticated.");

            var identity = new FormsIdentity(ticket);
            if (!identity.IsAuthenticated) throw new InvalidOperationException("The user is not authenticated.");

            if (fileBase == null || fileBase.ContentLength <= 0) fileBase = Request.Files[0];

            if (fileBase != null && fileBase.ContentLength > 0)
            {
                var pFile = new PropertyFilePart();

                UserPart createdUser = _groupService.GetUser(userId);

                // contentItem is a Property
                var contentItemProperty = Services.ContentManager.Get<PropertyPart>(contentItemId);
                if (contentItemProperty != null)
                {
                    pFile = _propertyService.UploadPropertyImage(fileBase, contentItemProperty, createdUser, isPublished);
                }
                else
                {
                    // contentItem is a Apartment
                    var contentItemApartment = Services.ContentManager.Get<LocationApartmentPart>(contentItemId);
                    if (contentItemApartment != null)
                    {
                        // always set isPublished = true for Apartment
                        pFile = _addressService.UploadApartmentImage(fileBase, contentItemApartment, createdUser.Record, true);
                    }
                    else
                    {
                        var contentItemApartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(contentItemId);
                        if (contentItemApartmentBlockInfo != null)
                            pFile = _propertyService.UploadPropertyImageForBlockInfo(fileBase, contentItemApartmentBlockInfo, createdUser, true);
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

            return Json(new { success = false });
        }

        #endregion

        #region Upload Video

        [HttpPost]
        public ActionResult UploadYoutube(HttpPostedFileBase file, string videoTitle, string videoKeyword, string videoDescription) //
        {
            if (string.IsNullOrEmpty(videoTitle) || string.IsNullOrEmpty(videoKeyword) || string.IsNullOrEmpty(videoDescription))
            {
                return Json(new { status = false, message = "Vui lòng nhập đầy đủ thông tin!" + videoTitle + " - " + videoKeyword + " - " + videoDescription });
            }
            if (file == null || file.ContentLength <= 0) file = Request.Files["file"];
            if (file == null || file.ContentLength <= 0)
                return Json(new { status = false, message = "Vui lòng chọn file video để upload!" });
            try
            {
                string videoId = _googleApiService.VideoIdUpload(file, "VideoTitle", "VideoKeyword",
                    "VideoDescription");
                return !string.IsNullOrEmpty(videoId)
                    ? Json(new { status = true, VideoId = videoId })
                    : Json(new { status = false, message = "Upload thất bại!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion

        [HttpPost]
        public ActionResult AjaxDeletePropertyImage(int id)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                //var fileInfo = new System.IO.FileInfo(Server.MapPath("~" + file.Path));
                //if (fileInfo.Exists) fileInfo.Delete();

                //Services.ContentManager.Remove(file.ContentItem);

                file.IsDeleted = true;

                //Clear cache UploadFile
                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return Json(new { id, success = true, message = "Xóa thành công" });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        [HttpPost]
        public ActionResult AjaxPermanentlyDeletePropertyImage(int id)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                var fileInfo = new FileInfo(Server.MapPath("~" + file.Path));
                if (fileInfo.Exists) fileInfo.Delete();

                Services.ContentManager.Remove(file.ContentItem);

                return Json(new { id, success = true, message = "Xóa thành công" });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        [HttpPost]
        public ActionResult AjaxPublishPropertyImage(int id, bool isPublished)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                file.Published = isPublished;

                //Clear cache UploadFile
                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return Json(new { id, success = true, isPublished = file.Published, message = "Cập nhật thành công" });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        [HttpPost]
        public ActionResult AjaxSetAvatarPropertyImage(int id, bool isAvatar)
        {
            var file = Services.ContentManager.Get<PropertyFilePart>(id);
            if (file != null)
            {
                IEnumerable<PropertyFilePart> files = new List<PropertyFilePart>();

                if (file.Property != null)
                    files =
                        Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.PropertyPartRecord == file.Property)
                            .List();
                else if (file.Apartment != null)
                    files =
                        Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.LocationApartmentPartRecord == file.Apartment)
                            .List();

                foreach (PropertyFilePart item in files)
                {
                    item.IsAvatar = false;
                }
                file.IsAvatar = true;
                file.Published = true;

                //Clear cache UploadFile
                if (file.Property != null)
                    _signals.Trigger("PropertyFiles_Changed_" + file.Property.Id);

                return
                    Json(
                        new
                        {
                            id,
                            success = true,
                            isAvatar = file.IsAvatar,
                            isPublished = file.Published,
                            message = "Cập nhật thành công"
                        });
            }
            return Json(new { id, success = false, message = "Không tìm thấy dữ liệu" });
        }

        #endregion

        #region CUSTOMER FUNCTIONS

        // Customer

        // Validate Customer ContactPhone

        [HttpPost]
        public ActionResult AjaxValidateContactPhone(int id, string contactPhone)
        {
            bool exists;

            if (id > 0)
                exists = !_customerService.VerifyCustomerUnicity(id, contactPhone);
            else
                exists = !_customerService.VerifyCustomerUnicity(contactPhone);

            if (!exists) return Json(new { exists = false });
            CustomerPart c = _customerService.GetCustomerByContactPhone(id, contactPhone);
            string link = Url.Action("Edit", "CustomerAdmin", new { c.Id });

            var result =
                new { exists = true, id = c.Id, contactName = c.ContactName, contactPhone = c.ContactPhone, link };
            return Json(result);
        }

        [HttpPost]
        public ActionResult AjaxGetCustomerContactPhone(int id)
        {
            var customer = Services.ContentManager.Get<CustomerPart>(id);
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            // update last call user
            customer.LastCallDate = DateTime.Now;
            customer.LastCallUser = user.Record;

            // save user activity record
            _revisionService.SaveUserActivityCallCustomer((DateTime)customer.LastCallDate, user, customer);

            object results =
                new
                {
                    id,
                    contactName = customer.ContactName,
                    contactPhone = customer.ContactPhone,
                    userName = customer.LastCallUser.UserName,
                    success = true,
                    message = "Xóa thành công"
                };
            return Json(results);
        }

        // Customer Requirements

        [HttpPost]
        public ActionResult AjaxEnabledGroupRequirements(int groupId)
        {
            _customerService.EnableCustomerRequirements(groupId);
            return Json(new { groupId, success = true, message = "Saved" });
        }

        [HttpPost]
        public ActionResult AjaxDisabledGroupRequirements(int groupId)
        {
            _customerService.DisableCustomerRequirements(groupId);
            return Json(new { groupId, success = true, message = "Saved" });
        }

        [HttpPost]
        public ActionResult AjaxDeleteGroupRequirements(int groupId)
        {
            _customerService.DeleteCustomerRequirements(groupId);
            return Json(new { groupId, success = true, message = "Xóa thành công" });
        }

        [HttpPost]
        public ActionResult AjaxEditGroupRequirements(int groupId)
        {
            List<CustomerRequirementRecord> model = _customerService.GetRequirements(groupId).ToList();
            CustomerRequirementRecord req = model.First();

            object results = new
            {
                groupId,
                adsTypeId = req.AdsTypePartRecord != null ? req.AdsTypePartRecord.Id : 0,
                propertyTypeGroupId = req.PropertyTypeGroupPartRecord != null ? req.PropertyTypeGroupPartRecord.Id : 0,
                provinceId = req.LocationProvincePartRecord.Id,
                districtIds =
                    model.Where(a => a.LocationDistrictPartRecord != null)
                        .Select(a => a.LocationDistrictPartRecord.Id)
                        .Distinct(),
                wardIds =
                    model.Where(a => a.LocationWardPartRecord != null)
                        .Select(a => a.LocationWardPartRecord.Id)
                        .Distinct(),
                streetIds =
                    model.Where(a => a.LocationStreetPartRecord != null)
                        .Select(a => a.LocationStreetPartRecord.Id)
                        .Distinct(),
                apartmentIds =
                    model.Where(a => a.LocationApartmentPartRecord != null)
                        .Select(a => a.LocationApartmentPartRecord.Id)
                        .Distinct(),
                directionIds =
                    model.Where(a => a.DirectionPartRecord != null).Select(a => a.DirectionPartRecord.Id).Distinct(),
                minArea = req.MinArea,
                maxArea = req.MaxArea,
                minWidth = req.MinWidth,
                maxWidth = req.MaxWidth,
                minLength = req.MinLength,
                maxLength = req.MaxLength,
                locationId = req.PropertyLocationPartRecord != null ? req.PropertyLocationPartRecord.Id : 0,
                minAlleyWidth = req.MinAlleyWidth,
                maxAlleyWidth = req.MaxAlleyWidth,
                maxAlleyTurns = req.MaxAlleyTurns,
                maxDistanceToStreet = req.MaxDistanceToStreet,
                minFloors = req.MinFloors,
                maxFloors = req.MaxFloors,
                minPrice = req.MinPrice,
                maxPrice = req.MaxPrice,
                paymentMethodId = req.PaymentMethodPartRecord != null ? req.PaymentMethodPartRecord.Id : 0,
                otherProjectName = req.OtherProjectName,
                minApartmentFloorth = req.MinApartmentFloorTh,
                maxApartmentFloorth = req.MaxApartmentFloorTh,
                success = true,
                message = "?"
            };
            return Json(results);
        }

        // Customer Properties

        [HttpPost]
        public ActionResult AjaxUpdateCustomerPropertyFeedback(int? cpId, int cId, int pId, int feedbackId,
            int[] selectedStaff, DateTime? visitedDate, bool isWorkOverTime)
        {
            // Save Customer's Properties Feedback

            var customer = Services.ContentManager.Get<CustomerPart>(cId);
            var property = Services.ContentManager.Get<PropertyPart>(pId);
            var feedback = Services.ContentManager.Get<CustomerFeedbackPart>(feedbackId);
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            CustomerPropertyRecord cp = _customerService.UpdateCustomerProperty(customer, property, feedback,
                selectedStaff, visitedDate, isWorkOverTime);

            //cpId = cp.Id;
            string staffs = "";
            string visited = "";
            if (cp.Users != null && cp.Users.Count > 0)
            {
                staffs = string.Join(", ", cp.Users.Select(a => a.UserPartRecord.UserName).Distinct().ToArray());
                visited = cp.Users.Min(a => a.VisitedDate).ToString("dd/MM/yyyy");
            }

            // Save Customer LastUpdatedDate

            var c = Services.ContentManager.Get<CustomerPart>(cId);
            c.LastUpdatedDate = DateTime.Now;
            c.LastUpdatedUser = user.Record;

            // Return

            object results =
                new
                {
                    cpId,
                    staffs,
                    visited,
                    isWorkOverTime,
                    css = cp.CustomerFeedbackPartRecord.CssClass,
                    success = true,
                    message = "Cập nhật thành công"
                };
            return Json(results);
        }

        [HttpPost]
        public ActionResult AjaxDeleteCustomerProperty(int cpId)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.DeleteCustomerProperty,
                    T("Not authorized to delete customer's property")))
                return Json(new { cpId, success = false, message = "Không thể xóa dữ liệu này." });

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            CustomerPropertyRecord cp = _customerpropertyRepository.Get(cpId);

            // Save Customer LastUpdatedDate

            CustomerPartRecord c = cp.CustomerPartRecord;
            c.LastUpdatedDate = DateTime.Now;
            c.LastUpdatedUser = user.Record;

            // Delete Customer Property

            _customerService.DeleteCustomerProperty(cpId);

            // Return

            object results = new { cpId, success = true, message = "Xóa thành công" };
            return Json(results);
        }

        #endregion

        #region USER FUNCTIONS

        public ActionResult SearchUsersForJson(string q, int page)
        {
            var items =
                _groupService.SearchUsers(q).Select(a => new { name = a.UserName, id = a.Id }).ToList();

            return Json(new { total_count = items.Count, incomplete_results = false, items }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchGroupUsersForJson(string q, int page, int g)
        {
            var currentGroup = _groupService.GetGroup(g);
            //var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            var users = currentGroup.GroupUsers.Select(a => a.UserPartRecord).ToList();
            users.Add(currentGroup.GroupAdminUser);

            var items = users.Where(a => a.NormalizedUserName.Contains(q.ToLower())).OrderBy(a => a.UserName)
                .Select(a => new { name = a.UserName, id = a.Id }).ToList();

            return Json(new { total_count = items.Count, incomplete_results = false, items }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchAvailableGroupMemberUsersForJson(string q, int page, int g)
        {
            var currentGroup = _groupService.GetGroup(g);
            var items =
                _groupService.SearchAvailableGroupMemberUsers(currentGroup, q).Select(a => new { name = a.UserName, id = a.Id }).ToList();

            return Json(new { total_count = items.Count, incomplete_results = false, items }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SearchAvailableGroupAdminUsersForJson(string q, int page, int g)
        {
            var currentGroup = _groupService.GetGroup(g);
            var items =
                _groupService.SearchAvailableGroupAdminUsers(currentGroup, q).Select(a => new { name = a.UserName, id = a.Id }).ToList();

            return Json(new { total_count = items.Count, incomplete_results = false, items }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUserForJson(int id)
        {
            var item = _groupService.GetUser(id);
            if (item != null)
                return Json(new { name = item.UserName, id = item.Id }, JsonRequestBehavior.AllowGet);
            return Json(new { name = "", id = 0 }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AjaxDeleteUserInGroup(int id)
        {
            if (_groupService.RemoveUserFromGroup(id))
            {
                return Json(new { id, success = true, message = "Xóa thành công." });
            }
            return Json(new { id, success = false, message = "Không thể xóa dữ liệu này." });
        }

        [HttpPost]
        public ActionResult AjaxDeleteUserLocation(int id, int? provinceId)
        {
            //var group = Services.ContentManager.Get<UserGroupPart>(id);
            //var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

            //bool currentUserIsGroupAdmin = group.GroupAdminUser.Id == currentUser.Id;
            //bool enableGroupAddUserAgencies = Services.Authorizer.Authorize(Permissions.ManageUsers) ||
            //                                 currentUserIsGroupAdmin;

            //if (!enableGroupAddUserAgencies)
            //    return Json(new {id, success = false, message = "Không thể xóa dữ liệu này."});

            _groupService.DeleteUserLocation(id);

            //clear cache
            _groupService.ClearUserLocationCache();
            if (provinceId.HasValue)
                _groupService.ClearUserLocationCache(provinceId);

            // Return
            object results = new { id, success = true, message = "Xóa thành công" };
            return Json(results);
        }

        [HttpPost]
        public ActionResult AjaxDeleteGroupContact(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to delete group contact")))
                return Json(new { id, success = false, message = "Không thể xóa dữ liệu này." });

            _groupService.DeleteGroupContact(id);

            // Return

            object results = new { id, success = true, message = "Xóa thành công" };
            return Json(results);
        }

        [HttpPost]
        public ActionResult AjaxDeleteGroupLocation(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to delete user")))
                return Json(new { id, success = false, message = "Không thể xóa dữ liệu này." });

            _groupService.DeleteGroupLocation(id);

            // Return

            object results = new { id, success = true, message = "Xóa thành công" };
            return Json(results);
        }

        [HttpPost]
        public ActionResult AjaxDeleteGroupSharedLocation(int id)
        {
            //if (!Services.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to delete user")))
            //return Json(new { id = id, success = false, message = "Không thể xóa dữ liệu này." });

            _groupService.DeleteGroupSharedLocation(id);

            // Return

            object results = new { id, success = true, message = "Xóa thành công" };
            return Json(results);
        }

        #endregion

        #region GROUP FUNCTIONS

        /// <summary>
        /// Search Groups by name
        /// </summary>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult SearchGroupsForJson(string q, int page)
        {
            var items =
                _groupService.SearchGroups(q).Select(a => new { name = a.Name, id = a.Id }).ToList();

            items.Insert(0, new { name = "-- Tất cả --", id = 0 });

            return Json(new { total_count = items.Count, incomplete_results = false, items }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Search Groups which shared properties to current Group
        /// </summary>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult SearchSeederGroupsForJson(string q, int page, int g)
        {
            var leecherGroup = _groupService.GetGroup(g);
            var items =
                _groupService.GetGroupSharedLocations(leecherGroup).Select(a => a.SeederUserGroupPartRecord).Distinct()
                //.Where(a => a.Name.Contains(q)).OrderBy(a => a.Name)
                .Select(a => new { name = a.Name, id = a.Id }).ToList();

            // add belongGroup to the list
            if (leecherGroup != null) items.Insert(0, new { name = leecherGroup.Name, id = leecherGroup.Id });
            items.Insert(0, new { name = "-- Tất cả --", id = 0 });

            return Json(new { total_count = items.Count, incomplete_results = false, items }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGroupForJson(int id)
        {
            if (id == 0)
                return Json(new { name = "-- Tất cả --", id = 0 }, JsonRequestBehavior.AllowGet);

            if (id > 0)
            {
                var item = _groupService.GetGroup(id);

                if (item != null)
                    return Json(new { name = item.Name, id = item.Id }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { name = "", id = 0 }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}