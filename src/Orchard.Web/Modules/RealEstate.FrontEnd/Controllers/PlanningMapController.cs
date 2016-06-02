using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Navigation;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Controllers
{
    [Themed]
    public class PlanningMapController : Controller, IUpdateModel
    {
        #region Init

        private readonly IAddressService _addressService;

        private readonly IPropertyService _propertyService;
        private readonly IRepository<PlanningMapRecord> _planningMapRepository;

        public PlanningMapController(IAddressService addressService,
            IPropertyService propertyService,
            IRepository<PlanningMapRecord> planningMapRepository,
            IShapeFactory shapeFactory,
            IOrchardServices services)
        {
            _addressService = addressService;
            _propertyService = propertyService;
            _planningMapRepository = planningMapRepository;

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

        public ActionResult Index(PlanningMapIndexOptions options, PagerParameters pagerParameters)
        {
            // default options
            if (options == null)
            {
                options = new PlanningMapIndexOptions { ProvinceId = 0, DistrictId = 0, WardId = 0 };
            }

            List<int> provinceIds =
                _planningMapRepository.Table.Select(a => a.LocationProvincePartRecord.Id).Distinct().ToList();
            options.Provinces = _addressService.GetProvinces().Where(a => provinceIds.Contains(a.Id));

            if (options.ProvinceId == null || options.ProvinceId == 0)
                options.ProvinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            List<int> districtIds =
                _planningMapRepository.Fetch(a => a.LocationProvincePartRecord.Id == options.ProvinceId)
                    .Select(a => a.LocationDistrictPartRecord.Id)
                    .Distinct()
                    .ToList();
            options.Districts = _addressService.GetDistricts().Where(a => districtIds.Contains(a.Id));

            List<int> wardIds =
                _planningMapRepository.Fetch(a => a.LocationDistrictPartRecord.Id == options.DistrictId)
                    .Select(a => a.LocationWardPartRecord.Id)
                    .Distinct()
                    .ToList();
            options.Wards = _addressService.GetWards().Where(a => wardIds.Contains(a.Id)).List();

            //var planningMaps = _planningMapRepository.Table;

            //if (options.ProvinceId > 0) planningMaps = planningMaps.Where(u => u.LocationProvincePartRecord.Id == options.ProvinceId);
            //if (options.DistrictId > 0) planningMaps = planningMaps.Where(u => u.LocationDistrictPartRecord.Id == options.DistrictId);
            //if (options.WardId > 0) planningMaps = planningMaps.Where(u => u.LocationWardPartRecord.Id == options.WardId);

            // Order
            //planningMaps = planningMaps.OrderBy(a => a.LocationProvincePartRecord)
            //                            .ThenBy(a => a.LocationDistrictPartRecord)
            //                            .ThenBy(a => a.LocationWardPartRecord);


            //var results = planningMaps.Take(1).ToList();

            //var model = new PlanningMapsIndexViewModel
            //{
            //    PlanningMaps = results.Select(a => new PlanningMapEntry { PlanningMap = a }).ToList(),
            //    Options = options
            //};

            return View(options);
        }

        [HttpPost]
        public ActionResult Index(FormCollection frmCollection)
        {
            var routeValues = new RouteValueDictionary();

            // process params
            foreach (string key in frmCollection.AllKeys)
            {
                if (key.Contains("RequestVerificationToken"))
                {
                }
                routeValues.Add(key, frmCollection[key]);
            }

            // general url
            string url = Url.Action("Index", routeValues);

            // redirect
            return Redirect(url);
        }

        public ActionResult GetDistrictsForJson(int provinceId)
        {
            List<SelectListItem> lstDistricts =
                _planningMapRepository.Fetch(a => a.LocationProvincePartRecord.Id == provinceId)
                    .Select(a => a.LocationDistrictPartRecord).Distinct().OrderBy(a => a.SeqOrder)
                    .Select(a => new { a.Name, a.Id }).ToList()
                    .Select(a => new SelectListItem { Text = a.Name, Value = a.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            return Json(new { list = lstDistricts }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWardsForJson(int districtId)
        {
            List<SelectListItem> lstWards =
                _planningMapRepository.Fetch(a => a.LocationDistrictPartRecord.Id == districtId)
                    .Select(a => a.LocationWardPartRecord).Distinct().OrderBy(a => a.SeqOrder)
                    .Select(a => new { WardName = a.Name + " - " + a.District.ShortName, a.Id }).ToList()
                    .Select(a => new SelectListItem { Text = a.WardName, Value = a.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            return Json(new { list = lstWards }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPlanningMapForJson(int wardId)
        {
            PlanningMapRecord m =
                _planningMapRepository.Fetch(a => a.LocationWardPartRecord.Id == wardId).FirstOrDefault();
            if (m != null)
                return
                    Json(
                        new
                        {
                            success = true,
                            wardId,
                            name =
                                String.Join(", ", m.LocationWardPartRecord.Name, m.LocationDistrictPartRecord.Name,
                                    m.LocationProvincePartRecord.Name),
                            m.ImagesPath,
                            m.Width,
                            m.Height,
                            m.MinZoom,
                            m.MaxZoom,
                            m.Ratio
                        }, JsonRequestBehavior.AllowGet);
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNearbyPropertiesForJson(int pId)
        {
            var p = Services.ContentManager.Get<PropertyPart>(pId);

            if (p != null) { 

            // Lấy các BĐS cùng Phường
            IContentQuery<PropertyPart, PropertyPartRecord> pList = _propertyService.GetProperties()
                .Where(a => a.AdsType == p.AdsType && a.TypeGroup == p.TypeGroup && a.Province == p.Province)
                .Where(a => a.Id != p.Id)
                .Where(a => a.Ward != null && a.Ward == p.Ward && a.PublishAddress == true);

            if (pList.Count() > 0)
                return
                    Json(
                        new
                        {
                            success = true,
                            properties = pList.List()
                            .Where(a => a.As<Maps.Models.MapPart>().PlanMapLatitude != 0 && a.As<Maps.Models.MapPart>().PlanMapLongitude != 0)
                            .Select(a => new
                            {
                                a.Id,
                                a.DisplayForTitle,
                                a.DisplayForTitleSEO,
                                a.DisplayForTitleWithPriceProposed,
                                EditUrl = Url.Action("Edit", "PropertyAdmin", new { area = "RealEstate.Admin", id = a.Id }).ToString(),
                                ViewUrl = Url.Action("RealEstateDetail", "PropertySearch", new { area = "RealEstate.FrontEnd", id = a.Id, title = a.DisplayForUrl }).ToString(),
                                a.As<Maps.Models.MapPart>().PlanMapLatitude,
                                a.As<Maps.Models.MapPart>().PlanMapLongitude
                            }).ToList()
                        }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
    }
}