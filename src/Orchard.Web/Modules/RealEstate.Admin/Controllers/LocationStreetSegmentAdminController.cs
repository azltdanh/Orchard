using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Reflection;
using System.Collections.Generic;
using Orchard;
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

using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

using System.Transactions;
using System.Data.SqlClient;
using System.Data;

namespace RealEstate.Controllers
{
    [ValidateInput(false), Admin]
    public class LocationStreetSegmentAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly ILocationStreetSegmentService _streetService;
        private readonly ISiteService _siteService;

        public LocationStreetSegmentAdminController(
            IOrchardServices services,
            IAddressService addressService,
            ILocationStreetSegmentService streetService,
            IShapeFactory shapeFactory,
            ISiteService siteService)
        {
            Services = services;
            _addressService = addressService;
            _streetService = streetService;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public bool GetOdd(int number)
        {
            if (number % 2 != 0)
                return true;
            else
                return false;
        }

        public bool GetEven(int number)
        {
            if (number % 2 == 0)
                return true;
            else
                return false;
        }

        public ActionResult Index(LocationStreetSegmentIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to list streets")))
                return new HttpUnauthorizedResult();

            //ImportData();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
            {
                options = new LocationStreetSegmentIndexOptions();
                options.ProvinceId = 0;
                options.DistrictId = 0;
                options.StreetId = 0;
            }

            options.Provinces = _addressService.GetProvinces();
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            options.Streets = _addressService.GetStreets(options.DistrictId);

            var streets = Services.ContentManager
                .Query<LocationStreetSegmentPart, LocationStreetSegmentPartRecord>();

            switch (options.Filter)
            {
                case LocationStreetSegmentsFilter.All:
                    //streets = streets.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            //if (!String.IsNullOrWhiteSpace(options.Search))
            //{
            //    streets = streets.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            //}

            if (options.ProvinceId > 0)
            {
                var districtIds = _addressService.GetDistricts(options.ProvinceId).Select(a => a.Id).ToList();
                var streetIds = new List<int>();
                foreach (var id in districtIds)
                {
                    streetIds.AddRange(_addressService.GetStreets(id).Select(a => a.Id).ToList());
                }
                streets = streets.Where(u => streetIds.Contains(u.Street.Id));
            }

            if (options.DistrictId > 0)
            {
                var streetIds = _addressService.GetStreets(options.DistrictId).Select(a => a.Id).ToList();
                streets = streets.Where(u => streetIds.Contains(u.Street.Id));
            }

            if (options.StreetId > 0)
            {
                streets = streets.Where(u => u.Street.Id == options.StreetId);
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(streets.Count());

            //streets = streets.OrderBy(u => u.Street).OrderBy(u => u.FromNumber);

            //switch (options.Order)
            //{
            //    case LocationStreetSegmentsOrder.SeqOrder:
            //        streets = streets.OrderBy(u => u.District).OrderBy(u => u.SeqOrder);
            //        break;
            //    case LocationStreetSegmentsOrder.Name:
            //        streets = streets.OrderBy(u => u.District).OrderBy(u => u.Name);
            //        break;
            //}

            var results = streets
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList().OrderBy(s => s.Street.Name).ThenBy(u => u.FromNumber);

            var model = new LocationStreetSegmentsIndexViewModel
            {
                LocationStreetSegments = results
                    .Select(x => new LocationStreetSegmentEntry { LocationStreetSegment = x.Record })
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
            routeData.Values.Add("Options.StreetId", options.StreetId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }
        
        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationStreetSegmentsIndexViewModel { LocationStreetSegments = new List<LocationStreetSegmentEntry>(), Options = new LocationStreetSegmentIndexOptions() };
            UpdateModel(viewModel);

            var checkedEntries = viewModel.LocationStreetSegments.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationStreetSegmentsBulkAction.None:
                    break;
                case LocationStreetSegmentsBulkAction.Enable:
                    foreach (var entry in checkedEntries)
                    {
                        Enable(entry.LocationStreetSegment.Id);
                    }
                    break;
                case LocationStreetSegmentsBulkAction.Disable:
                    foreach (var entry in checkedEntries)
                    {
                        Disable(entry.LocationStreetSegment.Id);
                    }
                    break;
                case LocationStreetSegmentsBulkAction.Delete:
                    foreach (var entry in checkedEntries)
                    {
                        Delete(entry.LocationStreetSegment.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create()
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.New<LocationStreetSegmentPart>("LocationStreetSegment");
            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationStreetSegment.Create", 
                Model: new LocationStreetSegmentCreateViewModel { 
                    Provinces = _addressService.GetProvinces(), 
                    Districts = _addressService.GetDistricts(-1),
                    Streets = _addressService.GetStreets(-1)
                }, 
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(street);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(LocationStreetSegmentCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            if ((createModel.FromNumber >= createModel.ToNumber) || ((createModel.FromNumber + createModel.ToNumber) % 2 != 0))
            {
                AddModelError("FromNumber", T("FromNumber and ToNumber not valid."));
                AddModelError("ToNumber", T("FromNumber and ToNumber not valid."));
            }

            if (!_streetService.VerifyStreetSegmentUnicity(createModel.StreetId, createModel.FromNumber, createModel.ToNumber))
            {
                AddModelError("NotUniqueStreetSegmentName", T("StreetSegment with that range already exists."));
            }

            var street = Services.ContentManager.New<LocationStreetSegmentPart>("LocationStreetSegment");
            if (ModelState.IsValid)
            {
                street.Record.Street = _addressService.GetStreet(createModel.StreetId);
                street.Record.FromNumber = createModel.FromNumber;
                street.Record.ToNumber = createModel.ToNumber;
                street.Record.IsEnabled = createModel.IsEnabled;
                street.Record.StreetWidth = createModel.StreetWidth;
                street.Record.CoefficientAlley1Max = createModel.CoefficientAlley1Max;
                street.Record.CoefficientAlley1Min = createModel.CoefficientAlley1Min;
                street.Record.CoefficientAlleyEqual = createModel.CoefficientAlleyEqual;
                street.Record.CoefficientAlleyMax = createModel.CoefficientAlleyMax;
                street.Record.CoefficientAlleyMin = createModel.CoefficientAlleyMin;

                Services.ContentManager.Create(street);
            }

            dynamic model = Services.ContentManager.UpdateEditor(street, this);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                createModel.Provinces = _addressService.GetProvinces();
                createModel.Districts = _addressService.GetDistricts(createModel.ProvinceId);
                createModel.Streets = _addressService.GetStreets(createModel.DistrictId);

                var editor = Shape.EditorTemplate(TemplateName: "Parts/LocationStreetSegment.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("StreetSegment from number {0} to number {1} created", street.FromNumber, street.ToNumber));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetSegmentPart>(id);
            int _streetId = street.Street.Id;
            int _districtId = street.Street.District.Id;
            int _provinceId = street.Street.District.Province.Id;

            var editModel = new LocationStreetSegmentEditViewModel {
                LocationStreetSegment = street,
                ProvinceId = _provinceId,
                Provinces = _addressService.GetProvinces(),
                DistrictId = _districtId,
                Districts = _addressService.GetDistricts(_provinceId),
                StreetId = _streetId,
                Streets = _addressService.GetStreets(_districtId),
            };

            var editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationStreetSegment.Edit",
                Model: editModel, 
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(street);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetSegmentPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(street, this);

            var editModel = new LocationStreetSegmentEditViewModel { LocationStreetSegment = street };
            if (TryUpdateModel(editModel))
            {

                if ((editModel.FromNumber >= editModel.ToNumber) || ((editModel.FromNumber + editModel.ToNumber) % 2 != 0))
                {
                    AddModelError("FromNumber", T("FromNumber and ToNumber not valid."));
                    AddModelError("ToNumber", T("FromNumber and ToNumber not valid."));
                }

                if (!_streetService.VerifyStreetSegmentUnicity(id, editModel.StreetId, editModel.FromNumber, editModel.ToNumber))
                {
                    AddModelError("NotUniqueStreetSegmentName", T("StreetSegment with that range already exists."));
                }                
                else
                {
                    street.Record.Street = _addressService.GetStreet(editModel.StreetId);
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel.Provinces = _addressService.GetProvinces();
                editModel.Districts = _addressService.GetDistricts(editModel.ProvinceId);
                editModel.Streets = _addressService.GetStreets(editModel.DistrictId);

                var editor = Shape.EditorTemplate(TemplateName: "Parts/LocationStreetSegment.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("StreetSegment from number {0} to number {1} updated", street.FromNumber, street.ToNumber));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetSegmentPart>(id);

            if (street != null)
            {
                street.IsEnabled = true;
                Services.Notifier.Information(T("StreetSegment from number {0} to number {1} updated", street.FromNumber, street.ToNumber));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetSegmentPart>(id);

            if (street != null)
            {
                street.IsEnabled = false;
                Services.Notifier.Information(T("StreetSegment from number {0} to number {1} updated", street.FromNumber, street.ToNumber));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetSegmentPart>(id);

            if (street != null)
            {
                    Services.ContentManager.Remove(street.ContentItem);
                    Services.Notifier.Information(T("StreetSegment from number {0} to number {1} deleted", street.FromNumber, street.ToNumber));
            }

            return RedirectToAction("Index");
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
