using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class UnEstimatedLocationAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly ISiteService _siteService;
        private readonly IRepository<UnEstimatedLocationRecord> _unEstimatedLocationRepository;

        public UnEstimatedLocationAdminController(
            ISiteService siteService,
            IAddressService addressService,
            IRepository<UnEstimatedLocationRecord> unEstimatedLocationRepository,
            IShapeFactory shapeFactory,
            IOrchardServices services)
        {
            _siteService = siteService;
            _addressService = addressService;
            _unEstimatedLocationRepository = unEstimatedLocationRepository;

            T = NullLocalizer.Instance;
            Services = services;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Index(UnEstimatedLocationIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageProperties,
                    T("Not authorized to list UnEstimatedLocations")))
                return new HttpUnauthorizedResult();

            // default options
            if (options == null)
            {
                options = new UnEstimatedLocationIndexOptions {ProvinceId = 0, DistrictId = 0, WardId = 0, StreetId = 0};
            }

            options.Provinces = _addressService.GetProvinces();
            if (options.ProvinceId == null || options.ProvinceId == 0)
                options.ProvinceId = _addressService.GetProvince("TP. Hồ Chí Minh").Id;
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            options.Wards = _addressService.GetWards(options.DistrictId ?? 0);
            options.Streets = _addressService.GetStreets(options.DistrictId ?? 0).Where(a => a.RelatedStreet == null);

            IQueryable<UnEstimatedLocationRecord> unEstimatedLocations = _unEstimatedLocationRepository.Table;

            // Filter
            switch (options.Filter)
            {
                case UnEstimatedLocationsFilter.All:
                    break;
            }

            // Search
            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                unEstimatedLocations =
                    unEstimatedLocations.Where(
                        u => u.AddressNumber.Contains(options.Search) || u.AddressCorner.Contains(options.Search));
            }

            if (options.ProvinceId > 0)
                unEstimatedLocations =
                    unEstimatedLocations.Where(u => u.LocationProvincePartRecord.Id == options.ProvinceId);
            if (options.DistrictId > 0)
                unEstimatedLocations =
                    unEstimatedLocations.Where(u => u.LocationDistrictPartRecord.Id == options.DistrictId);
            if (options.WardId > 0)
                unEstimatedLocations = unEstimatedLocations.Where(u => u.LocationWardPartRecord.Id == options.WardId);
            if (options.StreetId > 0)
                unEstimatedLocations = unEstimatedLocations.Where(u => u.LocationStreetPartRecord.Id == options.StreetId);

            // Order
            switch (options.Order)
            {
                case UnEstimatedLocationsOrder.CreatedDate:
                    unEstimatedLocations = unEstimatedLocations.OrderByDescending(u => u.CreatedDate);
                    break;
                case UnEstimatedLocationsOrder.Address:
                    unEstimatedLocations = unEstimatedLocations.OrderBy(a => a.LocationProvincePartRecord)
                        .ThenBy(a => a.LocationDistrictPartRecord)
                        .ThenBy(a => a.LocationWardPartRecord)
                        .ThenBy(a => a.LocationStreetPartRecord)
                        .ThenBy(a => a.AddressNumber)
                        .ThenBy(a => a.AddressCorner);
                    break;
            }

            // Pager
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(unEstimatedLocations.Count());

            List<UnEstimatedLocationRecord> results =
                unEstimatedLocations.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            var model = new UnEstimatedLocationsIndexViewModel
            {
                UnEstimatedLocations =
                    results.Select(a => new UnEstimatedLocationEntry {UnEstimatedLocation = a}).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageProperties,
                    T("Not authorized to manage UnEstimatedLocations")))
                return new HttpUnauthorizedResult();

            var viewModel = new UnEstimatedLocationsIndexViewModel
            {
                UnEstimatedLocations = new List<UnEstimatedLocationEntry>(),
                Options = new UnEstimatedLocationIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<UnEstimatedLocationEntry> checkedEntries = viewModel.UnEstimatedLocations.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case UnEstimatedLocationsBulkAction.None:
                    break;

                case UnEstimatedLocationsBulkAction.Delete:
                    foreach (UnEstimatedLocationEntry entry in checkedEntries)
                    {
                        Delete(entry.UnEstimatedLocation.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageProperties,
                    T("Not authorized to manage UnEstimatedLocations")))
                return new HttpUnauthorizedResult();

            UnEstimatedLocationRecord unEstimatedLocation = _unEstimatedLocationRepository.Get(id);

            if (unEstimatedLocation != null)
            {
                _unEstimatedLocationRepository.Delete(unEstimatedLocation);

                Services.Notifier.Information(T("UnEstimatedLocation {0} deleted", unEstimatedLocation.Id));
            }

            return RedirectToAction("Index");
        }
    }
}