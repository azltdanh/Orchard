using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Drawing;

using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;

using Maps.Models;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using Maps.Services;
using RealEstate.Helpers;


namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class LocationApartmentAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly ILocationApartmentService _apartmentService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly IMapService _mapService;

        public LocationApartmentAdminController(
            IOrchardServices services,
            IPropertyService propertyService,
            IAddressService addressService,
            IUserGroupService groupService,
            ILocationApartmentService apartmentService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService,
            IMapService mapService)
        {
            Services = services;
            _propertyService = propertyService;
            _addressService = addressService;
            _groupService = groupService;
            _apartmentService = apartmentService;
            _signals = signals;
            _siteService = siteService;
            _mapService = mapService;

            T = NullLocalizer.Instance;
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

        public ActionResult Index(LocationApartmentIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var apartments = Services.ContentManager
                .Query<LocationApartmentPart, LocationApartmentPartRecord>();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int provinceId = _groupService.GetUserDefaultProvinceId(user);
            int districtId = _groupService.GetUserDefaultDistrictId(user);

            // default options
            if (options == null)
            {
                options = new LocationApartmentIndexOptions { ProvinceId = provinceId, DistrictId = districtId };
            }

            // default setting from Group setting
            if (!options.ProvinceId.HasValue) options.ProvinceId = provinceId;
            if (!options.DistrictId.HasValue) options.DistrictId = districtId;

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                options.IsRestrictedLocations = true;
                options.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                options.Districts = _groupService.GetUserEnableEditLocationDistricts(user, options.ProvinceId);

                int[] enableProvinceIds = options.Provinces.Select(a => a.Id).ToArray();
                apartments = apartments.Where(u => enableProvinceIds.Contains(u.Province.Id));

                int[] enableDistrictIds = options.Districts.Select(a => a.Id).ToArray();
                apartments = apartments.Where(u => enableDistrictIds.Contains(u.District.Id));
            }
            else
            {
                options.Provinces = _addressService.GetProvinces();
                options.Districts = _addressService.GetDistricts(options.ProvinceId);
            }

            switch (options.Filter)
            {
                case LocationApartmentsFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                apartments =
                    apartments.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            if (options.ProvinceId > 0) apartments = apartments.Where(u => u.Province.Id == options.ProvinceId);
            if (options.DistrictId > 0) apartments = apartments.Where(u => u.District.Id == options.DistrictId);

            if (options.IsHighlight) apartments = apartments.Where(r => r.IsHighlight && r.HighlightExpiredTime != null && r.HighlightExpiredTime >= DateTime.Now);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(apartments.Count());

            switch (options.Order)
            {
                case LocationApartmentsOrder.SeqOrder:
                    apartments = apartments.OrderBy(u => u.District).OrderBy(u => u.SeqOrder);
                    break;
                case LocationApartmentsOrder.Name:
                    apartments = apartments.OrderBy(u => u.District).OrderBy(u => u.Name);
                    break;
            }

            List<LocationApartmentPart> results = apartments
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationApartmentsIndexViewModel
            {
                LocationApartments = results
                    .Select(x => new LocationApartmentEntry { LocationApartment = x.Record })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.ProvinceId", options.ProvinceId);
            routeData.Values.Add("Options.DistrictId", options.DistrictId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationApartmentsIndexViewModel
            {
                LocationApartments = new List<LocationApartmentEntry>(),
                Options = new LocationApartmentIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationApartmentEntry> checkedEntries = viewModel.LocationApartments.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationApartmentsBulkAction.None:
                    break;
                case LocationApartmentsBulkAction.Enable:
                    foreach (LocationApartmentEntry entry in checkedEntries)
                    {
                        Enable(entry.LocationApartment.Id);
                    }
                    break;
                case LocationApartmentsBulkAction.Disable:
                    foreach (LocationApartmentEntry entry in checkedEntries)
                    {
                        Disable(entry.LocationApartment.Id);
                    }
                    break;
                case LocationApartmentsBulkAction.Delete:
                    foreach (LocationApartmentEntry entry in checkedEntries)
                    {
                        Delete(entry.LocationApartment.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult ApartmentWithCartIndex(LocationApartmentIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.EditOwnProperty,
                    T("Not authorized EditOwnProperty")))
                return new HttpUnauthorizedResult();

            var apartments = _propertyService.GetListApartmentsByProperty();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
            {
                options = new LocationApartmentIndexOptions();
            }


            options.Provinces = _addressService.GetProvinces();
            options.Districts = _addressService.GetDistricts(options.ProvinceId);

            switch (options.Filter)
            {
                case LocationApartmentsFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                apartments =
                    apartments.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            if (options.ProvinceId > 0) apartments = apartments.Where(u => u.Province.Id == options.ProvinceId);
            if (options.DistrictId > 0) apartments = apartments.Where(u => u.District.Id == options.DistrictId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(apartments.Count());

            var results = apartments
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
                //.Slice(pager.GetStartIndex(), pager.PageSize)
            .ToList();

            var model = new LocationApartmentsIndexViewModel
            {
                LocationApartments = results
                    .Select(x => new LocationApartmentEntry
                    {
                        LocationApartment = x,
                        LocationApartmentBlock = _apartmentService.LocationApartmentBlockParts(x.Id)
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.ProvinceId", options.ProvinceId);
            routeData.Values.Add("Options.DistrictId", options.DistrictId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        public ActionResult Create(string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var createModel = new LocationApartmentCreateViewModel
            {
                Advantages = _propertyService.GetApartmentAdvantagesEntries(),
                IsEnabled = true,
                ReturnUrl = returnUrl
            };

            // default setting from Group setting
            if (createModel.ProvinceId <= 0) createModel.ProvinceId = _groupService.GetUserDefaultProvinceId(user);
            if (createModel.DistrictId <= 0) createModel.DistrictId = _groupService.GetUserDefaultDistrictId(user);

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                createModel.IsRestrictedLocations = true;
                createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                createModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, createModel.ProvinceId);
            }
            else
            {
                createModel.Provinces = _addressService.GetProvinces();
                createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
            }

            createModel.Wards = _addressService.GetWards(createModel.DistrictId);
            createModel.Streets = _addressService.GetStreets(createModel.DistrictId);

            var apartment = Services.ContentManager.New<LocationApartmentPart>("LocationApartment");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationApartment.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartment);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(LocationApartmentCreateViewModel createModel, FormCollection frmCollection,
            IEnumerable<HttpPostedFileBase> files)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (!string.IsNullOrEmpty(createModel.Name))
            {
                if (
                    !_apartmentService.VerifyApartmentUnicity(createModel.Name, createModel.Block,
                        createModel.DistrictId))
                {
                    AddModelError("NotUniqueApartmentName", T("Apartment with that name already exists."));
                }
            }
            if(createModel.IsHighlight && !createModel.HighlightExpiredTime.HasValue)
            {
                AddModelError("NullHighlightExpiredTime", T("Vui lòng chọn ngày hết hạn cho dự án nổi bật."));
            }

            var apartment = Services.ContentManager.New<LocationApartmentPart>("LocationApartment");
            if (ModelState.IsValid)
            {
                LocationDistrictPartRecord district = _addressService.GetDistrict(createModel.DistrictId);

                apartment.Province = district.Province;
                apartment.District = district;
                apartment.Ward = _addressService.GetWard(createModel.WardId);
                apartment.Street = _addressService.GetStreet(createModel.StreetId);
                apartment.AddressNumber = createModel.AddressNumber;

                apartment.Name = createModel.Name;
                apartment.ShortName = createModel.ShortName;
                apartment.Block = createModel.Block;

                apartment.StreetAddress = createModel.StreetAddress;
                apartment.DistanceToCentral = createModel.DistanceToCentral;

                apartment.Description = createModel.Description;

                apartment.Investors = createModel.Investors;
                apartment.CurrentBuildingStatus = createModel.CurrentBuildingStatus;
                apartment.ManagementFees = createModel.ManagementFees;

                apartment.Floors = createModel.Floors;

                apartment.AreaTotal = createModel.AreaTotal;
                apartment.AreaConstruction = createModel.AreaConstruction;
                apartment.AreaGreen = createModel.AreaGreen;

                apartment.TradeFloors = createModel.TradeFloors;
                apartment.AreaTradeFloors = createModel.AreaTradeFloors;

                apartment.Basements = createModel.Basements;
                apartment.AreaBasements = createModel.AreaBasements;

                apartment.Elevators = createModel.Elevators;

                apartment.SeqOrder = createModel.SeqOrder;
                apartment.IsEnabled = createModel.IsEnabled;

                apartment.IsHighlight = createModel.IsHighlight;
                apartment.HighlightExpiredTime = createModel.IsHighlight ? RealEstate.Helpers.DateExtension.GetEndOfDate(createModel.HighlightExpiredTime.Value) : (DateTime?)null;

                apartment.CreatedDate = DateTime.Now;
                apartment.UpdatedDate = DateTime.Now;
                    
                Services.ContentManager.Create(apartment);

                // Advantages
                _propertyService.UpdateApartmentAdvantagesForContentItem(apartment, createModel.Advantages);

                // Upload Images
                _addressService.UploadApartmentImages(files, apartment, true);
            }

            dynamic model = Services.ContentManager.UpdateEditor(apartment, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
                {
                    // User chỉ có quyền trong User's Location from User setting
                    createModel.IsRestrictedLocations = true;
                    createModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                    createModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user,
                        createModel.ProvinceId);
                }
                else
                {
                    createModel.Provinces = _addressService.GetProvinces();
                    createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
                }

                createModel.Wards = _addressService.GetWards(createModel.DistrictId);
                createModel.Streets = _addressService.GetStreets(createModel.DistrictId);

                createModel.Advantages = _propertyService.GetApartmentAdvantagesEntries();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationApartment.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            _signals.Trigger("Apartments_Changed");

            Services.Notifier.Information(T("<a href='{0}'>{1} - {2}</a> created.",
                Url.Action("Edit", new { apartment.Id }), apartment.Name, apartment.District.Name));

            // Create New
            return RedirectToAction("Create", new { createModel.ReturnUrl });

            //return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var apartment = Services.ContentManager.Get<LocationApartmentPart>(id);
            IEnumerable<int> advantageIds =
                _propertyService.GetAdvantagesForApartment(apartment)
                    .Where(a => a.ShortName == "apartment-adv")
                    .Select(a => a.Id);
            var editModel = new LocationApartmentEditViewModel
            {
                LocationApartment = apartment,
                ProvinceId = apartment.District.Province.Id,
                DistrictId = apartment.District.Id,
                WardId = apartment.Ward != null ? apartment.Ward.Id : 0,
                StreetId = apartment.Street != null ? apartment.Street.Id : 0,
                Advantages =
                    _propertyService.GetApartmentAdvantages()
                        .Select(
                            r => new PropertyAdvantageEntry { Advantage = r, IsChecked = advantageIds.Contains(r.Id) })
                        .ToList(),
                Files = _addressService.GetApartmentFiles(apartment).List(),
                ReturnUrl = returnUrl
            };

            if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
            {
                // User chỉ có quyền trong User's Location from User setting
                editModel.IsRestrictedLocations = true;
                editModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                editModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, editModel.ProvinceId);
            }
            else
            {
                editModel.Provinces = _addressService.GetProvinces();
                editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
            }

            editModel.Wards = _addressService.GetWards(editModel.DistrictId);
            editModel.Streets = _addressService.GetStreets(editModel.DistrictId);

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationApartment.Edit",
                Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartment);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var apartment = Services.ContentManager.Get<LocationApartmentPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(apartment, this);

            var editModel = new LocationApartmentEditViewModel { LocationApartment = apartment };

            if (editModel.IsHighlight && !editModel.HighlightExpiredTime.HasValue)
            {
                AddModelError("NullHighlightExpiredTime", T("Vui lòng chọn ngày hết hạn cho dự án nổi bật."));
            }

            try
            {

            }catch(Exception)
            { 
            }

            if (TryUpdateModel(editModel))
            {
                if (!_apartmentService.VerifyApartmentUnicity(id, editModel.Name, editModel.Block, editModel.DistrictId))
                {
                    AddModelError("NotUniqueApartmentName", T("Apartment with that name already exists."));
                }
                else
                {
                    LocationDistrictPartRecord district = _addressService.GetDistrict(editModel.DistrictId);
                    apartment.District = district;
                    apartment.Province = district.Province;
                    apartment.Ward = _addressService.GetWard(editModel.WardId);
                    apartment.Street = _addressService.GetStreet(editModel.StreetId);

                    // Advantages
                    _propertyService.UpdateApartmentAdvantagesForContentItem(apartment, editModel.Advantages);
                }

                apartment.HighlightExpiredTime = editModel.IsHighlight ? RealEstate.Helpers.DateExtension.GetEndOfDate(editModel.HighlightExpiredTime.Value) : (DateTime?)null;
                //apartment.HighlightExpiredTime = editModel.IsHighlight ? editModel.HighlightExpiredTime : default(DateTime);
                apartment.UpdatedDate = DateTime.Now;

            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                if (!Services.Authorizer.Authorize(Permissions.ManageAddressLocations))
                {
                    // User chỉ có quyền trong User's Location from User setting
                    editModel.IsRestrictedLocations = true;
                    editModel.Provinces = _groupService.GetUserEnableEditLocationProvinces(user);
                    editModel.Districts = _groupService.GetUserEnableEditLocationDistricts(user, editModel.ProvinceId);
                }
                else
                {
                    editModel.Provinces = _addressService.GetProvinces();
                    editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
                }

                editModel.Wards = _addressService.GetWards(editModel.DistrictId);
                editModel.Streets = _addressService.GetStreets(editModel.DistrictId);

                IEnumerable<int> advantageIds =
                    _propertyService.GetAdvantagesForApartment(apartment)
                        .Where(a => a.ShortName == "apartment-adv")
                        .Select(a => a.Id);
                editModel.Advantages =
                    _propertyService.GetApartmentAdvantages()
                        .Select(r => new PropertyAdvantageEntry { Advantage = r, IsChecked = advantageIds.Contains(r.Id) })
                        .ToList();
                editModel.Files = _addressService.GetApartmentFiles(apartment).List();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationApartment.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            _signals.Trigger("Apartments_Changed");

            Services.Notifier.Information(T("<a href='{0}'>{1} - {2}</a> updated.",
                Url.Action("Edit", new { apartment.Id }), apartment.Name, apartment.District.Name));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var apartment = Services.ContentManager.Get<LocationApartmentPart>(id);

            if (apartment != null)
            {
                apartment.IsEnabled = true;

                _signals.Trigger("Apartments_Changed");

                Services.Notifier.Information(T("Apartment {0} updated", apartment.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var apartment = Services.ContentManager.Get<LocationApartmentPart>(id);

            if (apartment != null)
            {
                apartment.IsEnabled = false;

                _signals.Trigger("Apartments_Changed");

                Services.Notifier.Information(T("Apartment {0} updated", apartment.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var apartment = Services.ContentManager.Get<LocationApartmentPart>(id);

            if (apartment != null)
            {
                Services.ContentManager.Remove(apartment.ContentItem);

                _signals.Trigger("Apartments_Changed");

                Services.Notifier.Information(T("Apartment {0} deleted", apartment.Name));
            }

            return RedirectToAction("Index");
        }

        #region LocationApartmentBlock

        public ActionResult ApartmentBlockIndex(LocationApartmentBlockIndexOptions options,
            PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var apartmentsBlock =
                Services.ContentManager
                    .Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>().Where(r => r.IsActive);

            options.LocationApartments = _addressService.GetApartments().List();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            switch (options.Filter)
            {
                case LocationApartmentsFilter.All:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                apartmentsBlock = apartmentsBlock.Where(u => u.BlockName.Contains(options.Search));
            }

            if (options.ApartmentId > 0)
                apartmentsBlock = apartmentsBlock.Where(u => u.LocationApartment.Id == options.ApartmentId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(apartmentsBlock.Count());

            apartmentsBlock = apartmentsBlock.OrderByDescending(r => r.LocationApartment.Id);
            switch (options.Order)
            {
                //case LocationApartmentsOrder.SeqOrder:
                //    apartmentsBlock = apartmentsBlock.OrderByDescending(u => u.SeqOrder);
                //    break;
                case LocationApartmentsOrder.Name:
                    apartmentsBlock = apartmentsBlock.OrderByDescending(u => u.BlockName);
                    break;
            }

            List<LocationApartmentBlockPart> results = apartmentsBlock
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationApartmentBlockIndexViewModel
            {
                LocationApartmentBlocks = results
                    .Select(x => new LocationApartmentBlockEntry { LocationApartmentBlock = x.Record })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.ApartmentId", options.ApartmentId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ApartmentBlockIndex(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationApartmentBlockIndexViewModel
            {
                LocationApartmentBlocks = new List<LocationApartmentBlockEntry>(),
                Options = new LocationApartmentBlockIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationApartmentBlockEntry> checkedEntries =
                viewModel.LocationApartmentBlocks.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationApartmentsBulkAction.None:
                    break;
                case LocationApartmentsBulkAction.Delete:
                    foreach (LocationApartmentBlockEntry entry in checkedEntries)
                    {
                        ApartmentBlockDelete(entry.LocationApartmentBlock.Id);
                    }
                    break;
            }

            return RedirectToAction("ApartmentBlockIndex", ControllerContext.RouteData.Values);
        }

        public ActionResult ApartmentBlockCreate(int? apartmentId, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var createModel = new LocationApartmentBlockCreateViewModel
            {
                LocationApartments = _addressService.GetApartments().List(),
                ApartmentId = apartmentId.HasValue ? apartmentId.Value : 0,
                ReturnUrl = returnUrl
            };

            var apartmentBlock = Services.ContentManager.New<LocationApartmentBlockPart>("LocationApartmentBlock");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationApartmentBlock.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartmentBlock);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("ApartmentBlockCreate")]
        public ActionResult ApartmentBlockCreatePost(LocationApartmentBlockCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();


            if (!string.IsNullOrEmpty(createModel.BlockName))
            {
                if (_apartmentService.VerifyApartmentBlockUnicity(createModel.BlockName, createModel.ApartmentId))
                {
                    AddModelError("NotUniqueApartmentBlockName", T("Block Name đã tồn tại trong dự án này! Vui lòng nhập tên khác."));
                }
            }

            //if (!string.IsNullOrEmpty(createModel.ShortName))
            //{
            //    if (_apartmentService.VerifyApartmentBlockShortNameUnicity(createModel.ShortName,
            //        createModel.ApartmentId))
            //    {
            //        AddModelError("NotUniqueApartmentBlockShortName", T("Block ShortName đã tồn tại trong dự án này! Vui lòng nhập tên khác."));
            //    }
            //}

            var apartmentBlock = Services.ContentManager.New<LocationApartmentBlockPart>("LocationApartmentBlock");
            if (ModelState.IsValid)
            {
                apartmentBlock.BlockName = createModel.BlockName;
                apartmentBlock.ShortName = createModel.BlockName.ToSlug();// createModel.ShortName;
                apartmentBlock.LocationApartment =
                    _apartmentService.LocationApartmentPart(createModel.ApartmentId).Record;
                apartmentBlock.FloorTotal = createModel.FloorTotal;
                //apartmentBlock.GroupFloorInBlockTotal = createModel.FloorGroupTotal;
                apartmentBlock.PriceAverage = createModel.PriceAverage;

                //apartmentBlock.ApartmentEachFloor = createModel.ApartmentEachFloor;
                //apartmentBlock.SeqOrder = 1;
                apartmentBlock.IsActive = true;
                Services.ContentManager.Create(apartmentBlock);

                //ClearCache
                _apartmentService.ClearCacheApartmentBlocks();
            }

            dynamic model = Services.ContentManager.UpdateEditor(apartmentBlock, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();


                createModel.LocationApartments = _addressService.GetApartments().List();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationApartmentBlock.Create",
                    Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("<a href='{0}'>{1}</a> created.",
                Url.Action("ApartmentBlockEdit", new { apartmentBlock.Id }), apartmentBlock.BlockName));

            if (!string.IsNullOrEmpty(createModel.ReturnUrl))
                return Redirect(createModel.ReturnUrl);

            return RedirectToAction("ApartmentBlockCreate");
        }


        public ActionResult ApartmentBlockEdit(int id, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();


            var apartmentBlock = Services.ContentManager.Get<LocationApartmentBlockPart>(id);
            var editModel = new LocationApartmentBlockEditViewModel
            {
                LocationApartmentBlock = apartmentBlock,
                ApartmentId = apartmentBlock.LocationApartment.Id,
                LocationApartments = _addressService.GetApartments().List(),
                ReturnUrl = returnUrl
            };


            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationApartmentBlock.Edit",
                Model: editModel,
                Prefix: null);

            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartmentBlock);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("ApartmentBlockEdit")]
        public ActionResult ApartmentBlockEditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var apartmentBlock = Services.ContentManager.Get<LocationApartmentBlockPart>(id);
            dynamic model = Services.ContentManager.UpdateEditor(apartmentBlock, this);

            var editModel = new LocationApartmentBlockEditViewModel { LocationApartmentBlock = apartmentBlock };
            if (TryUpdateModel(editModel))
            {
                if (_apartmentService.VerifyApartmentBlockUnicity(id, editModel.BlockName, editModel.ApartmentId))
                {
                    AddModelError("NotUniqueApartmentBlockName", T("Block Name đã tồn tại trong dự án này! Vui lòng nhập tên khác."));
                }
                //else if (_apartmentService.VerifyApartmentBlockShortNameUnicity(id, editModel.ShortName,
                //    editModel.ApartmentId))
                //{
                //    AddModelError("NotUniqueApartmentBlockShortName",
                //        T("Apartment with that ShortName already exists."));
                //}
                else
                {
                    apartmentBlock.ShortName = editModel.BlockName.ToSlug();
                    apartmentBlock.LocationApartment =
                        _apartmentService.LocationApartmentPart(editModel.ApartmentId).Record;

                    //ClearCache
                    _apartmentService.ClearCacheApartmentBlocks();
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.LocationApartments = _addressService.GetApartments().List();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationApartmentBlock.Edit",
                    Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("<a href='{0}'>{1}</a> updated.",
                Url.Action("ApartmentBlockEditPost", new { apartmentBlock.Id }), apartmentBlock.BlockName));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("ApartmentBlockIndex");
        }

        public ActionResult ApartmentBlockDelete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var apartmentBlock = Services.ContentManager.Get<LocationApartmentBlockPart>(id);

            if (apartmentBlock == null) return RedirectToAction("ApartmentBlockIndex");

            List<int> checkIsDelete = _apartmentService.CheckIsDeleteApartmentBlock(id);
            if (!checkIsDelete.Any())
            {
                Services.ContentManager.Remove(apartmentBlock.ContentItem);
                Services.Notifier.Information(T("Apartment Block {0} deleted", apartmentBlock.BlockName));

                //ClearCache
                _apartmentService.ClearCacheApartmentBlocks();
            }
            else
            {
                //LocationApartmentBlockPartRecord
                apartmentBlock.IsActive = false;
                Services.Notifier.Error(T("Apartment Block {0} tồn tại {1} bđs thuộc Block",
                                            apartmentBlock.BlockName, checkIsDelete.Count()));

                foreach (int item in checkIsDelete)
                {
                    var property = Services.ContentManager.Get<PropertyPart>(item);
                    property.Status = _propertyService.GetStatus("st-deleted");
                    Services.Notifier.Error(T("BĐS {0} thuộc block {1} đã chuyển trạng thái \"đã xóa\"", item, apartmentBlock.BlockName));
                }
            }

            //GroupInApartmentBlockPartRecord
            var groupApartmentBlockInfo = Services.ContentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                .Where(r => r.ApartmentBlock != null && r.ApartmentBlock == apartmentBlock.Record);
            if (groupApartmentBlockInfo.Count() > 0)
            {
                Services.Notifier.Error(T("Apartment Block {0} tồn tại {1} GroupInApartmentBlock thuộc Block",
                                            apartmentBlock.BlockName, groupApartmentBlockInfo.Count()));

                foreach (var group in groupApartmentBlockInfo.List())
                {
                    group.IsActive = false;
                }
            }

            return RedirectToAction("ApartmentBlockIndex");
        }

        #endregion

        #region LocationApartmentBlockInfo

        public ActionResult ApartmentBlockInfoIndex(int blockId, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var apartmentInfos = Services.ContentManager.Query<ApartmentBlockInfoPart, ApartmentBlockInfoPartRecord>().Where(r => r.ApartmentBlock != null && r.ApartmentBlock.Id == blockId);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(apartmentInfos.Count());


            var results = apartmentInfos
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var blockPart = Services.ContentManager.Get<LocationApartmentBlockPart>(blockId);
            var model = new ApartmentBlockInfoIndex
            {
                ApartmentBlockInfoParts = results.Select(r => new ApartmentBlockInfoEntry
                {
                    ApartmentBlockInfoPart = r
                }).ToList(),
                ApartmentBlockPart = blockPart,
                ApartmentName = blockPart.LocationApartment.Name,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ApartmentBlockInfoIndex(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to manage apartments")))
                return new HttpUnauthorizedResult();

            var viewModel = new ApartmentBlockInfoIndex
            {
                ApartmentBlockInfoParts = new List<ApartmentBlockInfoEntry>(),
                Options = new ApartmentBlockInfoOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<ApartmentBlockInfoEntry> checkedEntries = viewModel.ApartmentBlockInfoParts.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationApartmentsBulkAction.None:
                    break;
                case LocationApartmentsBulkAction.Delete:
                    foreach (ApartmentBlockInfoEntry entry in checkedEntries)
                    {
                        //Delete(entry.LocationApartment.Id);
                        BlockInfoDelete(entry.ApartmentBlockInfoPart.Id);
                    }
                    break;
            }

            return RedirectToAction("ApartmentBlockInfoIndex", ControllerContext.RouteData.Values);
        }

        public ActionResult ApartmentBlockInfoCreate(int blockId)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var apartmentBlockInfo = Services.ContentManager.New<ApartmentBlockInfoPart>("ApartmentBlockInfo");
            var viewModel = new ApartmentBlockInfoCreateViewmodel();
            viewModel.ApartmentBlockId = blockId;


            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/BlockInfo.Create",
                Model: viewModel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartmentBlockInfo);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("ApartmentBlockInfoCreate")]
        public ActionResult ApartmentBlockInfoCreatePost(ApartmentBlockInfoCreateViewmodel createModel, IEnumerable<HttpPostedFileBase> files)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();


            var apartmentBlockInfo = Services.ContentManager.New<ApartmentBlockInfoPart>("ApartmentBlockInfo");
            var apartmentBlock = Services.ContentManager.Get<LocationApartmentBlockPart>(createModel.ApartmentBlockId);
            var userCurrent = Services.WorkContext.CurrentUser.As<UserPart>();
            var fileAvatar = Request.Files["AvatarFile"];

            if (fileAvatar == null || fileAvatar.ContentLength <= 0)
                AddModelError("NullAvatar", T("Vui lòng chọn file avatar cho vị trí này"));
            else Services.Notifier.Information(T("Image: {0}", fileAvatar.FileName));


            if (ModelState.IsValid)
            {
                apartmentBlockInfo.ApartmentName = createModel.ApartmentName;
                apartmentBlockInfo.ApartmentArea = createModel.ApartmentArea;
                apartmentBlockInfo.ApartmentBathrooms = createModel.ApartmentBathrooms;
                apartmentBlockInfo.ApartmentBedrooms = createModel.ApartmentBedrooms;
                //apartmentBlockInfo.PriceAverage = createModel.PriceAverage;
                apartmentBlockInfo.RealAreaUse = createModel.RealAreaUse;
                apartmentBlockInfo.OrtherContent = createModel.OrtherContent;
                apartmentBlockInfo.ApartmentBlock = apartmentBlock.Record;
                apartmentBlockInfo.IsActive = true;

                Services.ContentManager.Create(apartmentBlockInfo);

                // Upload Images 
                _propertyService.UploadImagesForBlockInfo(files, apartmentBlockInfo, true);
                //Upload Avatar
                string avatar = _propertyService.UploadAvatarBlockInfo(fileAvatar, apartmentBlockInfo);
                apartmentBlockInfo.Avatar = avatar;
            }

            dynamic model = Services.ContentManager.UpdateEditor(apartmentBlockInfo, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/BlockInfo.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Thông tin {0} đã được tạo.", apartmentBlockInfo.ApartmentName));

            return RedirectToAction("ApartmentBlockInfoIndex", new { blockId = createModel.ApartmentBlockId });
        }

        public ActionResult ApartmentBlockInfoEdit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var apartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(id);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/BlockInfo.Edit",
                Model: new ApartmentBlockInfoEditViewmodel
                {
                    ApartmentBlockInfo = apartmentBlockInfo,
                    ApartmentBlockId = apartmentBlockInfo.ApartmentBlock.Id,
                    Files = _propertyService.GetApartmentBlockInfoFiles(apartmentBlockInfo),
                    EnableEditImages = Services.Authorizer.Authorize(StandardPermissions.SiteOwner)
                }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(apartmentBlockInfo);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("ApartmentBlockInfoEdit")]
        public ActionResult ApartmentBlockInfoEditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageLocationApartments,
                    T("Not authorized to list apartments")))
                return new HttpUnauthorizedResult();

            var apartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(apartmentBlockInfo, this);

            var editModel = new ApartmentBlockInfoEditViewmodel { ApartmentBlockInfo = apartmentBlockInfo };

            var fileAvatar = Request.Files["AvatarFile"];

            if (fileAvatar != null && fileAvatar.ContentLength > 0)
            {
                //Upload Avatar
                string avatar = _propertyService.UploadAvatarBlockInfo(fileAvatar, apartmentBlockInfo);
                apartmentBlockInfo.Avatar = avatar;
            }

            TryUpdateModel(editModel);
            //Update For Property
            _propertyService.UpdatePropertyInfoByApartmentInfo(id);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.Files = _propertyService.GetApartmentBlockInfoFiles(apartmentBlockInfo);
                editModel.EnableEditImages = Services.Authorizer.Authorize(StandardPermissions.SiteOwner);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/BlockInfo.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Thông tin {0} đã được cập nhật", apartmentBlockInfo.ApartmentName));

            return RedirectToAction("ApartmentBlockInfoIndex", new { blockId = apartmentBlockInfo.ApartmentBlock.Id });
        }

        public ActionResult ApartmentBlockInfoDelete(int id)
        {
            var apartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(id);

            BlockInfoDelete(id);

            return RedirectToAction("ApartmentBlockInfoIndex", new { blockId = apartmentBlockInfo.ApartmentBlock.Id });
        }

        public void BlockInfoDelete(int id)
        {
            var apartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(id);
            apartmentBlockInfo.IsActive = false;
        }

        #endregion

        #region Location Apartment Cart

        //Id = ApartmentId
        public ActionResult ApartmentCartIndex(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.EditOwnProperty,
                    T("Not authorized to EditOwnProperty")))
                return new HttpUnauthorizedResult();

            LocationApartmentCartIndexViewModel model = _propertyService.BuildApartmentCartIndex(id, false);

            return View(model);
        }

        public ActionResult SafeDownload(int id)
        {
            using (var package = new ExcelPackage())
            {
                package.Workbook.Worksheets.Add("Test");
                ExcelWorksheet ws = package.Workbook.Worksheets[1];
                ws.Name = string.Format("Export-{0:yyyy-MM-dd-HH-mm-ss}.xlsx", DateTime.Now); //Setting Sheet's name
                ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

                int rowIndex = 1;

                //Merging cells and create a center heading for out table
                ws.Cells[rowIndex, 1].Value = "DANH SÁCH CĂN HỘ BLOCK C - CC1.JOVITA - ĐỢT 1"; // Heading Name
                ws.Cells[rowIndex, 1, rowIndex, 10].Merge = true; //Merge columns start and end range
                ws.Cells[rowIndex, 1, rowIndex, 10].Style.Font.Bold = true; //Font should be bold
                ws.Cells[rowIndex, 1, rowIndex, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Aligmnet is center
                rowIndex++;
                rowIndex++;

                LocationApartmentCartIndexViewModel model = _propertyService.BuildApartmentCartIndex(id, false);

                ws.Cells[rowIndex, 1].Value = "Đang bán (" + model.CountSelling + ")";
                ws.Cells[rowIndex, 1, rowIndex, 2].Merge = true; //Merge columns start and end range
                ws.Cells[rowIndex, 1, rowIndex, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 1, rowIndex, 2].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#018A3A"));

                ws.Cells[rowIndex, 3].Value = "Giữ chỗ (" + model.CountOnHold + ")";
                ws.Cells[rowIndex, 3, rowIndex, 4].Merge = true; //Merge columns start and end range
                ws.Cells[rowIndex, 3, rowIndex, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 3, rowIndex, 4].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#9B9898"));

                ws.Cells[rowIndex, 5].Value = "Đặt cọc giữ chỗ (" + model.CountNegotiate + ")";
                ws.Cells[rowIndex, 5, rowIndex, 6].Merge = true; //Merge columns start and end range
                ws.Cells[rowIndex, 5, rowIndex, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 5, rowIndex, 6].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FEFB00"));

                ws.Cells[rowIndex, 7].Value = "Đặt cọc mua bán (" + model.CountTrading + ")";
                ws.Cells[rowIndex, 7, rowIndex, 8].Merge = true; //Merge columns start and end range
                ws.Cells[rowIndex, 7, rowIndex, 8].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 7, rowIndex, 8].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FEA500"));

                ws.Cells[rowIndex, 9].Value = " Đã giao dịch (" + model.CountSold + ")";
                ws.Cells[rowIndex, 9, rowIndex, 10].Merge = true; //Merge columns start and end range
                ws.Cells[rowIndex, 9, rowIndex, 10].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells[rowIndex, 9, rowIndex, 10].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#C61B21"));

                ws.Cells[rowIndex, 1, rowIndex, 10].Style.Font.Color.SetColor(Color.White);
                ws.Cells[rowIndex, 1, rowIndex, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                rowIndex++;
                rowIndex++;

                foreach (var block in model.LocationApartmentBlocks.Where(item => item.Properties.Any(r => r.GroupInApartmentBlock != null)))
                {
                    // Block Name
                    ws.Cells[rowIndex, 1].Value = block.ApartmentBlockPart.BlockName;
                    ws.Cells[rowIndex, 1].Style.Font.Bold = true; //Font should be bold
                    rowIndex++;
                    rowIndex++;

                    foreach (var groupAp in block.GroupInApartmentBlockParts)
                    {
                        // Group Name
                        ws.Cells[rowIndex, 1].Value = "Nhóm " + groupAp.ApartmentGroupPosition;
                        ws.Cells[rowIndex, 1].Style.Font.Bold = true; //Font should be bold
                        rowIndex++;
                        rowIndex++;

                        for (int i = groupAp.FloorFrom; i <= groupAp.FloorTo; i++)
                        {
                            // Floor Name
                            ws.Cells[rowIndex, 2].Value = "Tầng " + i;
                            ws.Cells[rowIndex, 2, rowIndex + 1, 2].Merge = true; //Merge columns start and end range
                            ws.Cells[rowIndex, 2, rowIndex + 1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center; // Aligmnet is center
                            ws.Cells[rowIndex, 2, rowIndex + 1, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            var floorProperties = block.Properties.Where(r => r.ApartmentFloorTh == i && !String.IsNullOrWhiteSpace(r.ApartmentNumber)).ToList();
                            for (int j = 0; j < floorProperties.Count(); j++)
                            {
                                // Apartment
                                var cell = ws.Cells[rowIndex + (j % 2), j / 2 + 3];
                                cell.Value = floorProperties[j].ApartmentNumber;
                                cell.Style.Font.Color.SetColor(Color.White);
                                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;

                                Color bgColor;
                                switch (floorProperties[j].Status.CssClass)
                                {
                                    case "st-onhold":
                                        bgColor = ColorTranslator.FromHtml("#9B9898");
                                        break;
                                    case "st-negotiate":
                                        bgColor = ColorTranslator.FromHtml("#FEFB00");
                                        break;
                                    case "st-trading":
                                        bgColor = ColorTranslator.FromHtml("#FEA500");
                                        break;
                                    case "st-sold":
                                        bgColor = ColorTranslator.FromHtml("#C61B21");
                                        break;
                                    default:
                                        bgColor = ColorTranslator.FromHtml("#018A3A");
                                        break;
                                }

                                cell.Style.Fill.BackgroundColor.SetColor(bgColor);
                            }
                            rowIndex++;
                            rowIndex++;
                        }
                        rowIndex++;
                    }
                }
                //for (var i = 1; i < 11; i++)
                //{
                //    for (var j = 2; j < 45; j++)
                //    {
                //        var cell = ws.Cells[j, i];

                //        //Setting Value in cell
                //        cell.Value = i * (j - 1);
                //    }
                //}

                //var chart = ws.Drawings.AddChart("chart1", eChartType.AreaStacked);
                ////Set position and size
                //chart.SetPosition(0, 630);
                //chart.SetSize(800, 600);

                //// Add the data series.
                //var series = chart.Series.Add(ws.Cells["A2:A46"], ws.Cells["B2:B46"]);

                var memoryStream = package.GetAsByteArray();
                var fileName = string.Format("Export-{0:yyyy-MM-dd-HH-mm-ss}.xlsx", DateTime.Now);
                // mimetype from http://stackoverflow.com/questions/4212861/what-is-a-correct-mime-type-for-docx-pptx-etc
                return base.File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public FileContentResult ApartmentCartIndexDownloadCSV(int id)
        {
            LocationApartmentCartIndexViewModel model = _propertyService.BuildApartmentCartIndex(id, false);

            StringBuilder sb = new StringBuilder();

            sb.Append("<table style='border:1px solid black; font-size:12px;'>");
            sb.Append("<tr>");
            sb.Append("<td style='width:120px;'><b>City Name</b></td>");
            sb.Append("<td style='width:300px;'><b>City Code</b></td>");
            sb.Append("</tr>");

            foreach (var block in model.LocationApartmentBlocks.Where(item => item.Properties.Any(r => r.GroupInApartmentBlock != null)))
            {
                sb.Append("<tr>");
                sb.Append("<td>" + block.ApartmentBlockPart.BlockName + "</td>");
                sb.Append("<td>" + block.ApartmentBlockPart.FloorTotal + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            foreach (var item in model.LocationApartmentBlocks.Where(item => item.Properties.Any(r => r.GroupInApartmentBlock != null)))
            {
                //item.ApartmentBlockPart.BlockName

                foreach (var groupAp in item.GroupInApartmentBlockParts)
                {
                    //Nhóm @groupAp.ApartmentGroupPosition
                    sb.Append("<table class='apartment-cart-table'>");
                    for (int i = groupAp.FloorFrom; i <= groupAp.FloorTo; i++)
                    {
                        sb.Append("<tr>");
                        sb.Append("<td rowspan='2'>" + "Tầng" + i + "</td>");


                        for (int j = 1; j <= groupAp.ApartmentPerFloor; j += 2)
                        {
                            var property = item.Properties.FirstOrDefault(r =>
                                        r.ApartmentBlock != null && r.ApartmentBlock.Id == item.ApartmentBlockPart.Id &&
                                        r.GroupInApartmentBlock.Id == groupAp.Id &&
                                        r.ApartmentFloorTh != null && r.ApartmentFloorTh == i &&
                                        r.ApartmentPositionTh == j);
                            sb.Append("<td>" + property.ApartmentNumber + "</td>");
                        }
                        sb.Append("</tr>");

                        sb.Append("<tr>");
                        for (int j = 2; j <= groupAp.ApartmentPerFloor; j += 2)
                        {
                            var property = item.Properties.FirstOrDefault(r =>
                                        r.ApartmentBlock != null && r.ApartmentBlock.Id == item.ApartmentBlockPart.Id &&
                                        r.GroupInApartmentBlock.Id == groupAp.Id &&
                                        r.ApartmentFloorTh != null && r.ApartmentFloorTh == i &&
                                        r.ApartmentPositionTh == j);
                            sb.Append("<td>" + property.ApartmentNumber + "</td>");
                        }
                        sb.Append("</tr>");
                    }
                    sb.Append("</table>");
                }
            }
            Response.AddHeader("content-disposition", string.Format("attachment; filename=Export-{0}.xls", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
            Response.ContentType = "application/vnd.ms-excel";
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            Response.Charset = "UTF-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "application/vnd.ms-excel");
        }
        public void Export(int id)
        {
            try
            {
                ExportAsCSV(id);
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("{0}", ex.Message));
                //Logger.WriteLog(LogLevel.Error, exception);
            }
        }
        private void ExportAsCSV(int id)
        {

            LocationApartmentCartIndexViewModel model = _propertyService.BuildApartmentCartIndex(id, false);

            StringBuilder sb = new StringBuilder();

            sb.Append("<table style='1px solid black; font-size:12px;'>");
            sb.Append("<tr>");
            sb.Append("<td style='width:120px;'><b>City Name</b></td>");
            sb.Append("<td style='width:300px;'><b>City Code</b></td>");
            sb.Append("</tr>");

            foreach (var block in model.LocationApartmentBlocks.Where(item => item.Properties.Any(r => r.GroupInApartmentBlock != null)))
            {
                sb.Append("<tr>");
                sb.Append("<td>" + block.ApartmentBlockPart.BlockName + "</td>");
                sb.Append("<td>" + block.ApartmentBlockPart.FloorTotal + "</td>");
                sb.Append("</tr>");
            }

            foreach (var block in model.LocationApartmentBlocks.Where(item => item.Properties.Any(r => r.GroupInApartmentBlock != null)))
            {
                // Block C
                //products.Rows.Add(8,block.ApartmentBlockPart.BlockName);

                //foreach (var groupAp in block.GroupInApartmentBlockParts)
                //{
                //    // Nhóm 1 Từng Group trong block
                //    products.Rows.Add(groupAp.ApartmentGroupPosition);
                //    for (int i = groupAp.FloorFrom; i <= groupAp.FloorTo; i++)
                //    {
                //        // Từng tầng trong group

                //        for (int j = 1; j <= groupAp.ApartmentPerFloor; j += 2)
                //        {
                //            // Từng căn trong tầng
                //            var property = block.Properties.FirstOrDefault(r =>
                //                        r.ApartmentBlock != null && r.ApartmentBlock.Id == block.ApartmentBlockPart.Id &&
                //                        r.GroupInApartmentBlock.Id == groupAp.Id &&
                //                        r.ApartmentFloorTh != null && r.ApartmentFloorTh == i &&
                //                        r.ApartmentPositionTh == j);
                //            products.Rows.Add(property.ApartmentNumber);

                //        }

                //        for (int j = 2; j <= groupAp.ApartmentPerFloor; j += 2)
                //        {
                //            var property = block.Properties.FirstOrDefault(r =>
                //                        r.ApartmentBlock != null && r.ApartmentBlock.Id == block.ApartmentBlockPart.Id &&
                //                        r.GroupInApartmentBlock.Id == groupAp.Id &&
                //                        r.ApartmentFloorTh != null && r.ApartmentFloorTh == i &&
                //                        r.ApartmentPositionTh == j);
                //            products.Rows.Add(property.ApartmentNumber);
                //        }
                //    }
                //}
            }

            var grid = new GridView();
            grid.DataSource = model.LocationApartmentBlocks.ToList();
            grid.DataBind();

            Response.ClearContent();
            Response.Buffer = true;

            Response.AddHeader("content-disposition", string.Format("attachment; filename=Export-{0}.xls", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
            //Response.ContentType = "application/vnd.ms-excel";
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            Response.Charset = "UTF-8";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);

            grid.RenderControl(htw);

            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            //var sw = new StringWriter();
            //if (Services.Authorizer.Authorize(Permissions.ExportProperties))
            //{


            //write the header
            //sw.WriteLine(String.Format("<span style='background-color:red;'>{0}</span>,{1},{2},{3},{4},{5},{6},{7},{8},{9}", "Đang bán", model.CountSelling, "Giữ chỗ", model.CountOnHold, "Đặt cọc giữ chỗ", model.CountNegotiate, "Đặt cọc mua bán", model.CountTrading, "Đã giao dịch", model.CountSold));

            //write every subscriber to the file
            //var resourceManager = new ResourceManager(typeof(CMSMessages));
            //foreach (var record in filterRecords.Select(x => x.First().Subscriber))
            //{
            //    sw.WriteLine(String.Format("{0},{1},{2},{3}", record.EmailAddress, record.Gender.HasValue ? resourceManager.GetString(record.Gender.ToString()) : "", record.FirstName, record.LastName));
            //}
            //}

            //Response.Clear();
            //Response.Charset = "UTF-8";
            //Response.ContentEncoding = System.Text.Encoding.UTF8;
            //Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());
            //Response.AddHeader("Content-Disposition",string.Format("attachment; filename=Export-{0}.csv", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
            //Response.ContentType = "text/csv";
            //Response.Write(sw);
            //Response.End();
        }

        //Id = ApartmentId
        public ActionResult ApartmentCartCreate(int? apartmentId, int? groupFloorPosition, int? apartmentBlockId, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageApartmentCart,
                    T("Not authorized to ManageApartmentCart")))
                return new HttpUnauthorizedResult();

            var createModel = _propertyService.BuildApartmentCartCreate(apartmentId);
            createModel.GroupFloorPosition = groupFloorPosition;
            createModel.ReturnUrl = returnUrl;
            createModel.ApartmentBlockId = apartmentBlockId != null ? apartmentBlockId.Value : 0;
            createModel.Published = true;

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)createModel);
        }

        [HttpPost, ActionName("ApartmentCartCreate")]
        public ActionResult ApartmentCartCreatePost(int? apartmentId, LocationApartmentCartCreateViewModel createModel, FormCollection frm)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageApartmentCart,
                    T("Not authorized to ManageApartmentCart")))
                return new HttpUnauthorizedResult();

            #region Validate

            if (createModel.FloorFrom == 0)
                AddModelError("FloorFromNotZero", T("Vui lòng nhập vị trí \"Từ tầng \" phải khác 0"));
            if (createModel.FloorTo == 0)
                AddModelError("FloorToNotZero", T("Vui lòng nhập vị trí tầng \" đến \" phải khác 0"));
            if (createModel.FloorFrom > createModel.FloorTo)
                AddModelError("FloorFromNotGreaterFloorTo", T("Vị trí \" đến \" phải lớn hơn vị trí \" Từ tầng \""));
            if (createModel.RoomInFloor == 0)
                AddModelError("RoomInFloorNotZero", T("Số căn hộ trên tầng phải khác 0"));

            if (createModel.FloorFrom > createModel.FloorsNumber || createModel.FloorTo > createModel.FloorsNumber)
                AddModelError("FloorFromOrFloorToNotGreaterFloorsNumber", T("Vị trí tầng phải nhỏ hơn tổng số tầng của Block!"));

            int groupFloorPosition = createModel.GroupFloorPosition != null && createModel.GroupFloorPosition.Value != 0 ? createModel.GroupFloorPosition.Value : 1;

            if (groupFloorPosition > createModel.FloorGroupTotal)
                AddModelError("GroupFloorPositionNotGreaterFloorGroupTotal", T("Vị trí nhóm không được lớn hơn \" Tổng nhóm tầng\""));

            var apartmentBlockPart = Services.ContentManager.Get<LocationApartmentBlockPart>(createModel.ApartmentBlockId);
            if (!(apartmentBlockPart.PriceAverage > 0))
            {
                AddModelError("PriceAverageNull", T("{0} bị NULL giá trung bình cho căn hộ, vui lòng vào thông tin Block để cập nhật Giá trung bình.", apartmentBlockPart.BlockName));
                //Services.Notifier.Error(T("{0} bị NULL giá trung bình cho căn hộ, vui lòng vào thông tin Block để cập nhật Giá trung bình.", apartmentBlockPart.BlockName));
            }
            //kiểm tra xem nhóm tầng đã được tạo chưa (ApartmentBlockId,groupFloorPosition)
            if (ModelState.IsValid && _propertyService.VerifyApartmentGroupInBlock(createModel.ApartmentBlockId, groupFloorPosition))
            {
                AddModelError("GroupInBlockExist", T("Nhóm tầng thứ {0} đã tồn tại, vui lòng click <a href=\"{1}\"> vào đây </a> để tạo cho nhóm tầng kế tiếp", groupFloorPosition, Url.Action("ApartmentCartCreate", new { apartmentId = apartmentId, groupFloorPosition = groupFloorPosition + 1 })));
            }


            #endregion

            if (ModelState.IsValid)
            {
                try
                {
                    //GroupInApartmentBlockPartRecord: nếu createModel.GroupFloorPosition == 0 ? 1 : createModel.GroupFloorPosition

                    var apartment = Services.ContentManager.Get<LocationApartmentPart>(createModel.ApartmentId);
                    apartmentBlockPart.GroupFloorInBlockTotal = createModel.FloorGroupTotal;

                    //Save vào table: GroupInApartmentBlockPartRecord(ApartmentBlock, FloorFrom,  FloorTo, ApartmentPerFloor)

                    #region Check & Insert GroupInApartmentBlockPart

                    //Check xem đã có nhóm này chưa?
                    var chkApartmentGroupInBlock = _propertyService.CheckApartmentGroupInBlock(apartmentBlockPart.Id, groupFloorPosition);

                    if (chkApartmentGroupInBlock == null)
                    {
                        var groupInApartmentBlockPart = Services.ContentManager.New<GroupInApartmentBlockPart>("GroupInApartmentBlock");
                        groupInApartmentBlockPart.ApartmentBlock = apartmentBlockPart.Record;
                        groupInApartmentBlockPart.FloorFrom = createModel.FloorFrom;
                        groupInApartmentBlockPart.FloorTo = createModel.FloorTo;
                        groupInApartmentBlockPart.ApartmentPerFloor = createModel.RoomInFloor;
                        groupInApartmentBlockPart.ApartmentGroupPosition = groupFloorPosition;
                        groupInApartmentBlockPart.IsActive = true;

                        Services.ContentManager.Create(groupInApartmentBlockPart);

                        Services.Notifier.Information(T("GroupInApartmentBlockPart đã được tạo"));

                        chkApartmentGroupInBlock = groupInApartmentBlockPart;
                    }

                    #endregion

                    var dictionary = new Dictionary<string, double>();

                    //Create Apartment Property
                    for (int i = createModel.FloorFrom; i <= createModel.FloorTo; i++)
                    {
                        //Hệ số chênh lệch
                        //var floorDifferenceTo = Convert.ToInt32(frm["ToPosition" + i]);
                        var floorCoefficient = !string.IsNullOrEmpty(frm["Coefficient" + i]) ? Convert.ToDouble(frm["Coefficient" + i]) : 0;

                        for (int j = 1; j <= createModel.RoomInFloor; j++)
                        {
                            //ApartmentInfo
                            var apNameId = Convert.ToInt32(frm["ApartmentName" + j]);
                            var apTotalCoefficient = Convert.ToDouble(frm["ApartmentCoefficient" + j]);//Tổng hệ số chênh lệch
                            var apartmentBlockInfo = Services.ContentManager.Get<ApartmentBlockInfoPart>(apNameId);


                            if (!(apartmentBlockPart.PriceAverage > 0))
                                Services.Notifier.Error(T("{0} bị NULL giá trung bình cho căn hộ, vui lòng vào thông tin Block để cập nhật Giá trung bình.", apartmentBlockPart.BlockName));

                            var priceAverage = apartmentBlockPart.PriceAverage;
                            var apBedroom = apartmentBlockInfo.ApartmentBedrooms;
                            var apBathroom = apartmentBlockInfo.ApartmentBathrooms;
                            var apArea = apartmentBlockInfo.ApartmentArea;

                            //Lưu giá trị đầu tiên lại để làm chuẩn
                            if (i == createModel.FloorFrom)
                            {
                                dictionary["ApartmentCoefficient" + j] = apTotalCoefficient;
                            }

                            // tầng i + 1 = tầng i + (tầng i * (% chênh lệch giữa tầng i + 1 và tầng i))
                            // tầng i + 1 = giá chuẩn * (tổng hệ số + hệ số chênh lệch tầng i+1 và tầng i)
                            // Căn đầu tiên trong nhóm làm chuẩn
                            double apPrice = priceAverage * apTotalCoefficient;

                            if (floorCoefficient != 1 && floorCoefficient != 0)
                            {
                                apPrice = priceAverage * (dictionary["ApartmentCoefficient" + j] + floorCoefficient);
                                Services.Notifier.Information(T("Giá tầng {0} = {1} * ( {2} + {3} ) = {4} triệu/m2", i, priceAverage, dictionary["ApartmentCoefficient" + j], floorCoefficient, apPrice));
                            }

                            if (string.IsNullOrEmpty(apartmentBlockInfo.ApartmentName)) Services.Notifier.Error(T("Tên căn hộ vị trí {0} bị NULL", j));

                            string apartmentNumber = apartmentBlockInfo.ApartmentName + "-" + i + "." + j;
                            //A131 => Tên A, Nhóm 1, tầng 3, vị trí 1

                            var p = Services.ContentManager.New<PropertyPart>("Property");

                            DateTime createdDate = DateTime.Now;
                            var createdUser = Services.WorkContext.CurrentUser.As<UserPart>();
                            UserGroupPartRecord belongGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();//_groupService.GetBelongGroup(createdUser.Id);

                            #region Type

                            // Type
                            p.Type = _propertyService.GetType(243); //243: Chung cư
                            p.TypeGroup = p.Type.Group;

                            #endregion

                            #region Address

                            // Province
                            p.Province = apartment.Province;

                            // District
                            p.District = apartment.District;

                            // Ward
                            p.Ward = apartment.Ward;
                            p.OtherWardName = null;

                            // Address
                            p.AddressNumber = apartment.AddressNumber;
                            p.AddressCorner = apartment.AddressNumber;

                            // Street
                            p.Street = apartment.Street;

                            // Apartment
                            p.Apartment = apartment.Record;
                            p.OtherProjectName = null;

                            p.ApartmentBlock = apartmentBlockPart.Record;
                            p.GroupInApartmentBlock = chkApartmentGroupInBlock.Record;
                            p.ApartmentPositionTh = j;

                            // ApartmentNumber
                            p.ApartmentNumber = apartmentNumber;

                            #endregion

                            #region Legal, Direction, Location

                            //// LegalStatus
                            //p.LegalStatus = _propertyService.GetLegalStatus(createModel.LegalStatusId);

                            //// Direction
                            //p.Direction = _propertyService.GetDirection(createModel.DirectionId);

                            //// Location
                            //p.Location = _propertyService.GetLocation(createModel.LocationCssClass);

                            #endregion

                            #region Area

                            // AreaTotal
                            p.Area = apArea;
                            p.AreaTotal = apartment.AreaTotal;

                            #endregion

                            #region Construction

                            // Construction
                            p.AreaConstruction = apartment.AreaConstruction;
                            p.AreaUsable = apArea;

                            #endregion

                            #region Apartment Info

                            // Apartment Info
                            p.ApartmentFloors = apartment.Floors;
                            p.ApartmentFloorTh = i;
                            p.ApartmentElevators = apartment.Elevators;
                            p.ApartmentBasements = apartment.Basements;

                            p.ApartmentTradeFloors = apartment.TradeFloors;
                            p.Bedrooms = apBedroom;
                            p.Bathrooms = apBathroom;

                            #endregion

                            #region Contact -----

                            // Contact
                            //p.ContactName = createModel.ContactName;
                            p.ContactPhone = createModel.ContactPhone;
                            //p.ContactPhoneToDisplay = createModel.ContactPhoneToDisplay;
                            //p.ContactAddress = createModel.ContactAddress;
                            //p.ContactEmail = createModel.ContactEmail;

                            #endregion

                            #region Price

                            //Price
                            p.PriceProposed = apPrice;
                            p.PaymentMethod =
                                _propertyService.GetPaymentMethod(57);//createModel.PaymentMethodId - triệu đồng
                            p.PaymentUnit =
                                _propertyService.GetPaymentUnit(90);//createModel.PaymentUnitId - m2

                            #endregion

                            #region User

                            // User
                            p.CreatedDate = createdDate;
                            p.CreatedUser = createdUser.Record;
                            p.LastUpdatedDate = createdDate;
                            p.LastUpdatedUser = createdUser.Record;
                            p.FirstInfoFromUser = createdUser.Record;
                            p.LastInfoFromUser = createdUser.Record;

                            // UserGroup
                            p.UserGroup = belongGroup;

                            #endregion

                            //status
                            p.Status = _propertyService.GetStatus(21); //21: Chưa hoàn chỉnh

                            // Flag
                            p.Flag = _propertyService.GetFlag(31); //31 Bình Thường

                            #region AdsType

                            // AdsType
                            p.AdsType = _propertyService.GetAdsType("ad-selling");

                            // Published
                            p.Published = createModel.Published;

                            p.AdsVIP = false;
                            p.AdsVIPExpirationDate = null;
                            p.SeqOrder = 0;

                            p.AdsExpirationDate = DateTime.Now.AddDays(90);

                            #endregion

                            Services.ContentManager.Create(p);

                            p.ApartmentBlockInfoPartRecord = apartmentBlockInfo.Record;

                            #region Advantages

                            List<PropertyAdvantageEntry> advantageCssClass =
                                _propertyService.GetAdvantagesForApartment(apartment)
                                    .Where(a => a.ShortName == "apartment-adv")
                                    .Select(a => new PropertyAdvantageEntry
                                    {
                                        Advantage = a,
                                        IsChecked = true
                                    }).ToList();
                            // ApartmentAdvantages
                            _propertyService.UpdatePropertyApartmentAdvantages(p, advantageCssClass);

                            // PriceProposedInVND
                            p.PriceProposedInVND = apartmentBlockInfo.RealAreaUse != 0 ? _propertyService.CalPriceProposedInVndRealArea(apartmentBlockInfo.RealAreaUse, p) : _propertyService.CaclPriceProposedInVnd(p);

                            //UpdateMap
                            var mapPart = apartment.As<MapPart>();
                            if (mapPart != null)
                                _mapService.UpdateMapPart(p.Id, mapPart.Latitude, mapPart.Longitude, mapPart.PlanMapLatitude, mapPart.PlanMapLongitude);

                            #endregion
                            Services.Notifier.Information(T("Căn hộ {0} - {1} đã được tạo.", p.ApartmentNumber, apartment.Name));
                        }
                    }

                    //Clear cache ListApartmentsByProperty
                    _propertyService.ClearCacheApartmentsByProperty();

                    if (groupFloorPosition + 1 > createModel.FloorGroupTotal)
                        return RedirectToAction("ApartmentCartIndex", new { id = apartmentId });

                    return RedirectToAction("ApartmentCartCreate", new { apartmentId = apartmentId, groupFloorPosition = groupFloorPosition + 1, apartmentBlockId = createModel.ApartmentBlockId });
                }
                catch (Exception ex)
                {
                    Services.Notifier.Information(T("Error: {0}", ex.Message));
                }

                if (!ModelState.IsValid)
                {
                    return View(createModel);
                }
                return RedirectToAction("ApartmentCartIndex", new { id = apartmentId });
            }

            LocationApartmentCartCreateViewModel model = _propertyService.BuildApartmentCartCreate(apartmentId);

            return View(model);
        }

        public ActionResult ApartmentCartDelete(int apartmentId, string returnUrl)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageApartmentCart,
                    T("Not authorized to ManageApartmentCart")))
                return new HttpUnauthorizedResult();

            var lstApartmentBlocks =
                Services.ContentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>()
                    .Where(r => r.IsActive && r.LocationApartment.Id == apartmentId).List();

            if (Services.Authorizer.Authorize(Permissions.ManageApartmentCart))
            {
                bool isAdmin = Services.Authorizer.Authorize(StandardPermissions.SiteOwner);
                if (lstApartmentBlocks.Any())
                {
                    foreach (var item in lstApartmentBlocks)
                    {
                        //item.IsActive = false;

                        //PropertyPartRecord
                        var properties = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                            .Where(r => r.ApartmentBlock != null && r.ApartmentBlock.Id == item.Id);

                        if (properties.Count() <= 0) continue;
                        foreach (var p in properties.List())
                        {
                            //Chuyển trạng thái Property
                            p.Status = isAdmin ? _propertyService.GetStatus("st-deleted") : _propertyService.GetStatus("st-trash");
                        }

                        //GroupInApartmentBlockPartRecord
                        var groupApartmentBlockInfo = Services.ContentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                            .Where(r => r.ApartmentBlock != null && r.ApartmentBlock == item.Record);
                        if (groupApartmentBlockInfo.Count() <= 0) continue;

                        foreach (var group in groupApartmentBlockInfo.List())
                        {
                            group.IsActive = false;
                        }
                    }

                    _propertyService.ClearCacheApartmentsByProperty();
                    _apartmentService.ClearCacheApartmentBlocks();
                }
            }
            return Redirect(returnUrl);
        }


        public ActionResult ApartmentCartGroupApartmentDelete(int? apartmentId, int groupFloorPosition, int apartmentBlockId)
        {

            if (
                !Services.Authorizer.Authorize(Permissions.ManageApartmentCart,
                    T("Not authorized to ManageApartmentCart")))
                return new HttpUnauthorizedResult();

            //Delete Group
            if (groupFloorPosition > 0 && apartmentBlockId > 0)
            {
                var groupInApartmentBlock = Services.ContentManager.Query<GroupInApartmentBlockPart, GroupInApartmentBlockPartRecord>()
                    .Where(r => r.ApartmentBlock != null && r.ApartmentBlock.Id == apartmentBlockId && r.ApartmentGroupPosition == groupFloorPosition).Slice(1).FirstOrDefault();

                if (groupInApartmentBlock != null)
                {
                    bool isAdmin = Services.Authorizer.Authorize(StandardPermissions.SiteOwner);
                    //PropertyPartRecord
                    var properties = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(r => r.GroupInApartmentBlock != null && r.GroupInApartmentBlock == groupInApartmentBlock.Record);

                    foreach (var p in properties.List())
                    {
                        //Chuyển trạng thái Property
                        p.Status = isAdmin ? _propertyService.GetStatus("st-deleted") : _propertyService.GetStatus("st-trash");
                    }

                    Services.Notifier.Information(T("Đã xóa {0} bđs thuộc nhóm {1}.", properties.Count(), groupFloorPosition));
                }
            }

            return RedirectToAction("ApartmentCartCreate", new { apartmentId = apartmentId, groupFloorPosition = groupFloorPosition, apartmentBlockId = apartmentBlockId });
        }

        #endregion
    }
}