using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
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

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class LocationStreetAdminController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly ILocationStreetService _streetService;

        public LocationStreetAdminController(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            ILocationStreetService streetService,
            IShapeFactory shapeFactory,
            ISignals signals,
            ISiteService siteService)
        {
            Services = services;
            _addressService = addressService;
            _groupService = groupService;
            _streetService = streetService;
            _signals = signals;
            _siteService = siteService;

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

        public ActionResult Index(LocationStreetIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to list streets")))
                return new HttpUnauthorizedResult();

            //ImportData();

            IContentQuery<LocationStreetPart, LocationStreetPartRecord> streets = Services.ContentManager
                .Query<LocationStreetPart, LocationStreetPartRecord>();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int provinceId = _groupService.GetUserDefaultProvinceId(user);
            int districtId = _groupService.GetUserDefaultDistrictId(user);

            // default options
            if (options == null)
            {
                options = new LocationStreetIndexOptions
                {
                    ProvinceId = provinceId,
                    DistrictId = districtId,
                    WardId = 0,
                    StreetId = 0
                };
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
                streets = streets.Where(u => enableProvinceIds.Contains(u.Province.Id));

                int[] enableDistrictIds = options.Districts.Select(a => a.Id).ToArray();
                streets = streets.Where(u => enableDistrictIds.Contains(u.District.Id));
            }
            else
            {
                options.Provinces = _addressService.GetProvinces();
                options.Districts = _addressService.GetDistricts(options.ProvinceId);
            }

            options.Wards = _addressService.GetWards(options.DistrictId);
            options.Streets = _addressService.GetStreets(options.DistrictId).Where(a => a.RelatedStreet == null);

            switch (options.Filter)
            {
                case LocationStreetsFilter.All:
                    //streets = streets.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                streets = streets.Where(u => u.Name.Contains(options.Search) || u.ShortName.Contains(options.Search));
            }

            if (options.ProvinceId > 0) streets = streets.Where(u => u.Province.Id == options.ProvinceId);
            if (options.DistrictId > 0) streets = streets.Where(u => u.District.Id == options.DistrictId);
            if (options.WardId > 0) streets = streets.Where(u => u.Ward.Id == options.WardId);
            if (options.StreetId > 0)
                streets = streets.Where(u => u.RelatedStreet != null && u.RelatedStreet.Id == options.StreetId);
            if (options.ShowRelatedStreetOnly) streets = streets.Where(u => u.RelatedStreet != null);

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(streets.Count());

            switch (options.Order)
            {
                case LocationStreetsOrder.SeqOrder:
                    streets = streets.OrderBy(u => u.District).OrderBy(u => u.SeqOrder);
                    break;
                case LocationStreetsOrder.Name:
                    streets = streets.OrderBy(u => u.District).OrderBy(u => u.Name).OrderBy(s => s.FromNumber);
                    break;
            }

            List<LocationStreetPart> results = streets
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new LocationStreetsIndexViewModel
            {
                LocationStreets = results
                    .Select(
                        x => new LocationStreetEntry {LocationStreet = x, DisplayForStreetName = x.DisplayForStreetName})
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
            routeData.Values.Add("Options.WardId", options.WardId);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var viewModel = new LocationStreetsIndexViewModel
            {
                LocationStreets = new List<LocationStreetEntry>(),
                Options = new LocationStreetIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<LocationStreetEntry> checkedEntries = viewModel.LocationStreets.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case LocationStreetsBulkAction.None:
                    break;
                case LocationStreetsBulkAction.Enable:
                    foreach (LocationStreetEntry entry in checkedEntries)
                    {
                        Enable(entry.LocationStreet.Id);
                    }
                    break;
                case LocationStreetsBulkAction.Disable:
                    foreach (LocationStreetEntry entry in checkedEntries)
                    {
                        Disable(entry.LocationStreet.Id);
                    }
                    break;
                case LocationStreetsBulkAction.Delete:
                    foreach (LocationStreetEntry entry in checkedEntries)
                    {
                        Delete(entry.LocationStreet.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var createModel = new LocationStreetCreateViewModel
            {
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

            var street = Services.ContentManager.New<LocationStreetPart>("LocationStreet");
            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationStreet.Create",
                Model: createModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(street);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePost(LocationStreetCreateViewModel createModel, FormCollection frmCollection)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if (createModel.IsRelatedStreet)
            {
                if (!createModel.StreetId.HasValue)
                {
                    AddModelError("StreetId", T("Please select related street."));
                }

                if (!(createModel.FromNumber.HasValue && createModel.ToNumber.HasValue) ||
                    (createModel.FromNumber >= createModel.ToNumber) ||
                    ((createModel.FromNumber + createModel.ToNumber)%2 != 0))
                {
                    AddModelError("FromNumber", T("FromNumber and ToNumber not valid."));
                    AddModelError("ToNumber", T("FromNumber and ToNumber not valid."));
                }

                if (createModel.StreetId.HasValue && createModel.FromNumber.HasValue && createModel.ToNumber.HasValue)
                {
                    if (
                        !_streetService.VerifyStreetSegmentUnicity((int) createModel.StreetId,
                            (int) createModel.FromNumber, (int) createModel.ToNumber, createModel.DistrictId))
                    {
                        AddModelError("NotUniqueStreetSegmentName", T("StreetSegment with that range already exists."));
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(createModel.Name))
                {
                    if (!_streetService.VerifyStreetUnicity(createModel.Name, createModel.DistrictId))
                    {
                        AddModelError("NotUniqueStreetName", T("Street with that name already exists."));
                    }
                }
                else
                {
                    AddModelError("NotUniqueStreetName", T("Please enter street name."));
                }
            }

            var street = Services.ContentManager.New<LocationStreetPart>("LocationStreet");
            if (ModelState.IsValid)
            {
                LocationDistrictPartRecord district = _addressService.GetDistrict(createModel.DistrictId);
                //street.Name = createModel.Name;
                //street.ShortName = createModel.ShortName;
                street.SeqOrder = createModel.SeqOrder;
                street.IsEnabled = createModel.IsEnabled;
                street.Province = district.Province;
                street.District = district;
                street.Ward = _addressService.GetWard(createModel.WardId);

                if (createModel.IsRelatedStreet)
                {
                    LocationStreetPartRecord relatedStreet = _addressService.GetStreet(createModel.StreetId);
                    street.RelatedStreet = relatedStreet;
                    street.Name = relatedStreet.Name;
                    street.FromNumber = createModel.FromNumber;
                    street.ToNumber = createModel.ToNumber;
                }
                else
                {
                    street.Name = createModel.Name;
                    street.RelatedStreet = null;
                    street.FromNumber = null;
                    street.ToNumber = null;
                }

                street.StreetWidth = createModel.StreetWidth;
                street.CoefficientAlley1Max = createModel.CoefficientAlley1Max;
                street.CoefficientAlley1Min = createModel.CoefficientAlley1Min;
                street.CoefficientAlleyEqual = createModel.CoefficientAlleyEqual;
                street.CoefficientAlleyMax = createModel.CoefficientAlleyMax;
                street.CoefficientAlleyMin = createModel.CoefficientAlleyMin;

                Services.ContentManager.Create(street);

                UpdatePropertyLocationStreet(street);
            }

            dynamic model = Services.ContentManager.UpdateEditor(street, this);

            if (street.RelatedStreet != null)
                street.Name = street.RelatedStreet.Name;
            else
            {
                street.FromNumber = null;
                street.ToNumber = null;
            }

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

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationStreet.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Streets_Changed");

            Services.Notifier.Information(T("Đường <a href='{0}'>{1} - {2}</a> created.",
                Url.Action("Edit", new {street.Id}), street.DisplayForStreetName, street.District.Name));

            return RedirectToAction("Create", new {createModel.ReturnUrl});
            //return RedirectToAction("Index");
        }

        public ActionResult Edit(int id, string returnUrl)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var street = Services.ContentManager.Get<LocationStreetPart>(id);
            var editModel = new LocationStreetEditViewModel
            {
                LocationStreet = street,
                ProvinceId = street.Province.Id,
                DistrictId = street.District.Id,
                Wards = _addressService.GetWards(street.District.Id),
                Streets = _addressService.GetStreets(street.District.Id),
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

            if (street.Ward != null)
                editModel.WardId = street.Ward.Id;
            if (street.RelatedStreet != null)
            {
                editModel.IsRelatedStreet = true;
                editModel.StreetId = street.RelatedStreet.Id;
            }

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationStreet.Edit",
                Model: editModel,
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(street);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            var street = Services.ContentManager.Get<LocationStreetPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(street, this);

            var editModel = new LocationStreetEditViewModel {LocationStreet = street};
            if (TryUpdateModel(editModel))
            {
                bool valid = true;

                if (editModel.IsRelatedStreet)
                {
                    if (!editModel.StreetId.HasValue)
                    {
                        valid = false;
                        AddModelError("StreetId", T("Please select related street."));
                    }

                    if (!(editModel.FromNumber.HasValue && editModel.ToNumber.HasValue) ||
                        (editModel.FromNumber >= editModel.ToNumber) ||
                        ((editModel.FromNumber + editModel.ToNumber)%2 != 0))
                    {
                        valid = false;
                        AddModelError("FromNumber", T("FromNumber and ToNumber not valid."));
                        AddModelError("ToNumber", T("FromNumber and ToNumber not valid."));
                    }

                    if (editModel.StreetId.HasValue && editModel.FromNumber.HasValue && editModel.ToNumber.HasValue)
                    {
                        if (
                            !_streetService.VerifyStreetSegmentUnicity(id, (int) editModel.StreetId,
                                (int) editModel.FromNumber, (int) editModel.ToNumber, editModel.DistrictId))
                        {
                            AddModelError("NotUniqueStreetSegmentName",
                                T("StreetSegment with that range already exists."));
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(editModel.Name))
                    {
                        if (!_streetService.VerifyStreetUnicity(id, editModel.Name, editModel.DistrictId))
                        {
                            valid = false;
                            AddModelError("NotUniqueStreetName", T("Street with that name already exists."));
                        }
                    }
                    else
                    {
                        valid = false;
                        AddModelError("NotUniqueStreetName", T("Please enter street name."));
                    }
                }

                if (valid)
                {
                    LocationDistrictPartRecord district = _addressService.GetDistrict(editModel.DistrictId);
                    street.Province = district.Province;
                    street.District = district;
                    street.Ward = _addressService.GetWard(editModel.WardId);

                    if (editModel.IsRelatedStreet)
                    {
                        street.RelatedStreet = _addressService.GetStreet(editModel.StreetId);
                        street.Name = street.RelatedStreet.Name;
                    }
                    else
                    {
                        street.Name = editModel.Name;
                        street.RelatedStreet = null;
                        street.FromNumber = null;
                        street.ToNumber = null;
                    }
                    UpdatePropertyLocationStreet(street);
                }
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

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/LocationStreet.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            _signals.Trigger("Streets_Changed");

            Services.Notifier.Information(T("Đường <a href='{0}'>{1} - {2}</a> updated.",
                Url.Action("Edit", new {street.Id}), street.DisplayForStreetName, street.District.Name));

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Enable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetPart>(id);

            if (street != null)
            {
                street.IsEnabled = true;

                _signals.Trigger("Streets_Changed");

                Services.Notifier.Information(T("Street {0} updated", street.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Disable(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetPart>(id);

            if (street != null)
            {
                street.IsEnabled = false;

                _signals.Trigger("Streets_Changed");

                Services.Notifier.Information(T("Street {0} updated", street.Name));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageLocationStreets, T("Not authorized to manage streets")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.Get<LocationStreetPart>(id);

            if (street != null)
            {
                if (street.RelatedStreet != null)
                {
                    // Đưa các BĐS trở lại đường chính
                    IEnumerable<PropertyPart> toRemoveList =
                        Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                            .Where(a => a.Street.Id == street.Id)
                            .List();
                    foreach (PropertyPart item in toRemoveList)
                    {
                        item.Street = street.RelatedStreet;
                    }
                }
                else if (
                    Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a => a.Street.Id == street.Id)
                        .Count() > 0)
                {
                    Services.Notifier.Information(T("Cannot delete Street {0}", street.Name));
                    return RedirectToAction("Index");
                }

                // Xóa các records StreetRelation 
                IEnumerable<StreetRelationPart> relations =
                    Services.ContentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                        .Where(a => a.Street.Id == street.Id || a.RelatedStreet.Id == street.Id)
                        .List();
                foreach (StreetRelationPart relation in relations)
                {
                    Services.ContentManager.Remove(relation.ContentItem);
                }

                // Xóa record Street
                Services.ContentManager.Remove(street.ContentItem);

                _signals.Trigger("Streets_Changed");

                Services.Notifier.Information(T("Street {0} deleted", street.Name));
            }

            return RedirectToAction("Index");
        }

        public void UpdatePropertyLocationStreet(LocationStreetPart street)
        {
            if (street.RelatedStreet != null)
            {
                // Loại tất cả các BĐS không thỏa đk ra khỏi đoạn đường
                IEnumerable<PropertyPart> toRemoveListOutOfRange = Services.ContentManager
                    .Query<PropertyPart, PropertyPartRecord>()
                    .Where(
                        a =>
                            a.Street.Id == street.Id &&
                            (a.AlleyNumber < street.FromNumber || a.AlleyNumber > street.ToNumber)).List().ToList();
                foreach (PropertyPart item in toRemoveListOutOfRange)
                {
                    item.Street = street.RelatedStreet;
                }
                IEnumerable<PropertyPart> toRemoveListNotInRange = Services.ContentManager
                    .Query<PropertyPart, PropertyPartRecord>()
                    .Where(
                        a =>
                            a.Street.Id == street.Id && a.AlleyNumber >= street.FromNumber &&
                            a.AlleyNumber <= street.ToNumber).List()
                    .Where(a => (a.AlleyNumber + street.FromNumber)%2 != 0).ToList();
                foreach (PropertyPart item in toRemoveListNotInRange)
                {
                    item.Street = street.RelatedStreet;
                }

                // Lấy tất cả các BĐS thỏa đk cho vào đoạn đường vửa tạo
                IEnumerable<PropertyPart> toAddList = Services.ContentManager
                    .Query<PropertyPart, PropertyPartRecord>()
                    .Where(
                        a =>
                            a.Street == street.RelatedStreet && a.AlleyNumber >= street.FromNumber &&
                            a.AlleyNumber <= street.ToNumber).List()
                    .Where(a => (a.AlleyNumber + street.FromNumber)%2 == 0).ToList();
                foreach (PropertyPart item in toAddList)
                {
                    item.Street = street.Record;
                }
                Services.Notifier.Information(T("Remove {0} properties",
                    toRemoveListOutOfRange.Count() + toRemoveListNotInRange.Count()));
                Services.Notifier.Information(T("Add {0} properties", toAddList.Count()));
            }
        }
    }
}