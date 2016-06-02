using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Estimation.Controllers
{
    [ValidateInput(false), Admin]
    public class ImportController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IEstimationService _estimationService;
        //private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly IRevisionService _revisionService;

        public ImportController(
            //IUserGroupService groupService,
            IAddressService addressService,
            IPropertyService propertyService,
            ICustomerService customerService,
            IRevisionService revisionService,
            IEstimationService estimationService,
            IOrchardServices services)
        {
            //_groupService = groupService;
            _addressService = addressService;
            _propertyService = propertyService;
            _customerService = customerService;
            _revisionService = revisionService;
            _estimationService = estimationService;
            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index()
        {
            return View("Index");
        }

        #region IMPORT DATA

        public ActionResult CalcAlleyCoefficient()
        {
            LocationProvincePartRecord province = _addressService.GetProvince("TP. Hồ Chí Minh");
            IEnumerable<StreetRelationPart> provinceStreets = Services.ContentManager
                .Query<StreetRelationPart, StreetRelationPartRecord>()
                .Where(a => a.Province.Id == province.Id)
                .List();

            //List<int> _districtIds = _streets.Select(a => a.District.Id).Distinct().ToList();

            //foreach (int districtId in _districtIds)
            //{
            List<StreetRelationPart> streets = provinceStreets.Where(a => a.District.Id == 330).ToList();
            foreach (StreetRelationPart street in streets)
            {
                _estimationService.UnitPriceAvgOnStreet(street.Record);
            }
            Services.Notifier.Information(T("Calculated {0} streets of district {1}", streets.Count,
                streets.First().District.Name));
            //}

            return View();
        }

        public ActionResult Import()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to import data.")))
                return new HttpUnauthorizedResult();

            IEnumerable<ImportActionEntry> model = new List<ImportActionEntry>
            {
                // Locations
                new ImportActionEntry
                {
                    Group = "Locations",
                    Name = "Provinces",
                    Value = "ImportLocationProvinces",
                    IsEnable =
                        (Services.ContentManager.Query<LocationProvincePart, LocationProvincePartRecord>().Count() == 0)
                },
                new ImportActionEntry
                {
                    Group = "Locations",
                    Name = "Districts",
                    Value = "ImportLocationDistricts",
                    IsEnable =
                        (Services.ContentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>().Count() == 0) &&
                        (Services.ContentManager.Query<LocationProvincePart, LocationProvincePartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Locations",
                    Name = "Wards",
                    Value = "ImportLocationWards",
                    IsEnable =
                        (Services.ContentManager.Query<LocationWardPart, LocationWardPartRecord>().Count() == 0) &&
                        (Services.ContentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Locations",
                    Name = "Streets",
                    Value = "ImportLocationStreets",
                    IsEnable =
                        (Services.ContentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Count() == 0) &&
                        (Services.ContentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Locations",
                    Name = "Street Relations",
                    Value = "ImportStreetRelations",
                    IsEnable =
                        (Services.ContentManager.Query<StreetRelationPart, StreetRelationPartRecord>().Count() == 0) &&
                        (Services.ContentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Count() > 0)
                },
                // Properties
                new ImportActionEntry
                {
                    Group = "Properties",
                    Name = "List",
                    Value = "ImportPropertyList",
                    IsEnable =
                        (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() == 0) &&
                        (Services.ContentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Properties",
                    Name = "Files",
                    Value = "ImportPropertyFiles",
                    IsEnable =
                        (Services.ContentManager.Query<PropertyFilePart, PropertyFilePartRecord>().Count() == 0) &&
                        (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Properties",
                    Name = "Revisions",
                    Value = "ImportPropertyRevisions",
                    IsEnable =
                        (Services.ContentManager.Query<RevisionPart, RevisionPartRecord>()
                            .Where(a => a.ContentType == "Property")
                            .Count() == 0) &&
                        (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Properties",
                    Name = "ConvertPrice",
                    Value = "ConvertPriceProposedInVND",
                    IsEnable = (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Properties",
                    Name = "ConvertAdvantages",
                    Value = "ConvertAdvantages",
                    IsEnable = (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() > 0)
                },
                // Customers
                new ImportActionEntry
                {
                    Group = "Customers",
                    Name = "List",
                    Value = "ImportCustomerList",
                    IsEnable = (Services.ContentManager.Query<CustomerPart, CustomerPartRecord>().Count() == 0)
                },
                new ImportActionEntry
                {
                    Group = "Customers",
                    Name = "Requirements",
                    Value = "ImportCustomerRequirements",
                    IsEnable =
                        !_customerService.GetRequirements().Any() &&
                        (Services.ContentManager.Query<CustomerPart, CustomerPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Customers",
                    Name = "Revisions",
                    Value = "ImportCustomerRevisions",
                    IsEnable =
                        (Services.ContentManager.Query<RevisionPart, RevisionPartRecord>()
                            .Where(a => a.ContentType == "Customer")
                            .Count() == 0) &&
                        (Services.ContentManager.Query<CustomerPart, CustomerPartRecord>().Count() > 0)
                },
                new ImportActionEntry
                {
                    Group = "Customers",
                    Name = "Properties",
                    Value = "ImportCustomerProperties",
                    IsEnable =
                        !_customerService.GetCustomerSavedProperties().Any() &&
                        (Services.ContentManager.Query<CustomerPart, CustomerPartRecord>().Count() > 0) &&
                        (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() > 0)
                },
                // Users 
                new ImportActionEntry
                {
                    Group = "Users",
                    Name = "Activities",
                    Value = "ImportUserActivities",
                    IsEnable =
                        !_revisionService.GetUserActivities().Any() &&
                        (Services.ContentManager.Query<CustomerPart, CustomerPartRecord>().Count() > 0) &&
                        (Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().Count() > 0)
                },
                // Revisions
                new ImportActionEntry
                {
                    Group = "Revisions",
                    Name = "Data",
                    Value = "ImportRevisionData",
                    IsEnable = (Services.ContentManager.Query<RevisionPart, RevisionPartRecord>().Count() > 0)
                },
                // Resize Images
                new ImportActionEntry
                {
                    Group = "Files",
                    Name = "ResizeAllImages",
                    Value = "ResizeAllImages",
                    IsEnable = true
                },
            };

            return View(model);
        }

        [HttpPost, ActionName("Import")]
        public ActionResult ImportPost(string importAction)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to import data.")))
                return new HttpUnauthorizedResult();

            switch (importAction)
            {
                    // Locations
                    //case "ImportLocationProvinces":
                    //    ImportLocationProvinces();
                    //    break;
                    //case "ImportLocationDistricts":
                    //    ImportLocationDistricts();
                    //    break;
                    //case "ImportLocationWards":
                    //    ImportLocationWards();
                    //    break;
                    //case "ImportLocationStreets":
                    //    ImportLocationStreets();
                    //    break;
                    //case "ImportStreetRelations":
                    //    ImportStreetRelations();
                    //    break;

                    // Properties
                    //case "ImportPropertyList":
                    //    ImportPropertyList();
                    //    break;
                    //case "ImportPropertyFiles":
                    //    ImportPropertyFiles();
                    //    break;
                    //case "ImportPropertyRevisions":
                    //    ImportPropertyRevisions();
                    //    break;
                case "ConvertPriceProposedInVND":
                    ConvertPriceProposedInVnd();
                    break;
                case "ConvertAdvantages":
                    ConvertAdvantages();
                    break;

                    // Customers
                    //case "ImportCustomerList":
                    //    ImportCustomerList();
                    //    break;
                    //case "ImportCustomerRequirements":
                    //    ImportCustomerRequirements();
                    //    break;
                    //case "ImportCustomerRevisions":
                    //    ImportCustomerRevisions();
                    //    break;
                    //case "ImportCustomerProperties":
                    //    ImportCustomerProperties();
                    //    break;

                    // Users 
                    //case "ImportUserActivities":
                    //    ImportUserActivities();
                    //    break;

                    // Revisions
                    //case "ImportRevisionData":
                    //    ImportRevisionData();
                    //    break;


                    // ResizeAllImages
                case "ResizeAllImages":
                    _propertyService.ResizeAllImages();
                    break;
            }

            //Services.Notifier.Information(T("Action: {0} successfully", ImportAction));
            return RedirectToAction("Import");
        }

        /*
        
        public void ImportLocationProvinces()
        {
            int dataCount = Services.ContentManager.Query<LocationProvincePart, LocationProvincePartRecord>().Count();
            if (dataCount == 0)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
                {
                    int itemsCount = 0, tranferCount = 0, errorId = 0;
                    string errorStep = "", errorMsg = "";

                    try
                    {
                        dgndDataContext storeDB = new dgndDataContext();
                        var provinces = storeDB.LocationCities.Where(a => a.Id > 0).OrderBy(a => a.SeqOrder).ToList();
                        itemsCount = provinces.Count;
                        foreach (var record in provinces)
                        {
                            errorId = record.Id;

                            #region COLLECT DATA

                            var province = Services.ContentManager.New<LocationProvincePart>("LocationProvince");

                            province.Name = record.Name;
                            province.ShortName = record.ShortName;
                            province.SeqOrder = provinces.IndexOf(record) + 1;
                            province.IsEnabled = true;

                            Services.ContentManager.Create(province);

                            #endregion

                            tranferCount++;
                        }

                        if (itemsCount == tranferCount)
                        {
                            Services.Notifier.Information(T("Import {0} province records successfully", itemsCount));
                        }
                    }
                    catch (Exception error)
                    {
                        Services.TransactionManager.Cancel();
                        errorMsg = error.Message;
                        Services.Notifier.Error(T("Import {0} province records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                    }
                }
            }
            else
            {
                Services.Notifier.Information(T("Data have {0} province records.", dataCount));
            }
        }

        public void ImportLocationDistricts()
        {
            int dataCount = Services.ContentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>().Count();
            if (dataCount == 0 && Services.ContentManager.Query<LocationProvincePart, LocationProvincePartRecord>().Count() > 0)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
                {
                    int itemsCount = 0, tranferCount = 0, errorId = 0;
                    string errorStep = "", errorMsg = "";

                    try
                    {
                        dgndDataContext storeDB = new dgndDataContext();
                        var list = storeDB.LocationDistricts.Where(a => a.Id > 0).OrderBy(a => a.LocationCity.SeqOrder).ThenBy(a => a.SeqOrder).ToList();
                        itemsCount = list.Count;

                        var provinceNames = list.Select(a => a.LocationCity.Name).Distinct().ToList();

                        foreach (string provinceName in provinceNames)
                        {
                            var _province = _addressService.GetProvince(provinceName);
                            var districts = list.Where(a => a.LocationCity.Name == provinceName).ToList();

                            foreach (var record in districts)
                            {
                                errorId = record.Id;

                                #region COLLECT DATA

                                var district = Services.ContentManager.New<LocationDistrictPart>("LocationDistrict");

                                district.Province = _province;
                                district.Name = record.Name;
                                district.ShortName = record.ShortName;
                                district.SeqOrder = districts.IndexOf(record) + 1;
                                district.IsEnabled = true;

                                Services.ContentManager.Create(district);

                                #endregion

                                tranferCount++;
                            }
                        }

                        if (itemsCount == tranferCount)
                        {
                            Services.Notifier.Information(T("Import {0} district records successfully", itemsCount));
                        }
                    }
                    catch (Exception error)
                    {
                        Services.TransactionManager.Cancel();
                        errorMsg = error.Message;
                        Services.Notifier.Error(T("Import {0} district records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                    }
                }
            }
            else
            {
                Services.Notifier.Information(T("Data have {0} district records.", dataCount));
            }
        }

        public void ImportLocationWards()
        {
            int dataCount = Services.ContentManager.Query<LocationWardPart, LocationWardPartRecord>().Count();
            if (dataCount == 0 && Services.ContentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>().Count() > 0)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
                {
                    int itemsCount = 0, tranferCount = 0, errorId = 0;
                    string errorStep = "", errorMsg = "";

                    try
                    {
                        dgndDataContext storeDB = new dgndDataContext();
                        var list = storeDB.LocationWards.Where(a => a.Id > 0).OrderBy(a => a.LocationDistrict.LocationCity.SeqOrder).ThenBy(a => a.LocationDistrict.SeqOrder).ThenBy(a => a.SeqOrder).ToList();
                        itemsCount = list.Count;

                        var provinceNames = list.Select(a => a.LocationDistrict.LocationCity.Name).Distinct().ToList();

                        foreach (string provinceName in provinceNames)
                        {
                            var _province = _addressService.GetProvince(provinceName);
                            var districts = _addressService.GetDistricts(_province.Id);

                            foreach (var _district in districts)
                            {
                                var wards = list.Where(a => a.LocationDistrict.LocationCity.Name == provinceName && a.LocationDistrict.Name == _district.Name).OrderBy(a => a.Name).ToList();

                                foreach (var record in wards)
                                {
                                    errorId = record.Id;

                                    #region COLLECT DATA

                                    var ward = Services.ContentManager.New<LocationWardPart>("LocationWard");

                                    ward.Province = _province;
                                    ward.District = _district;
                                    ward.Name = record.Name;
                                    ward.ShortName = record.ShortName;
                                    ward.SeqOrder = wards.IndexOf(record) + 1;
                                    ward.IsEnabled = true;

                                    Services.ContentManager.Create(ward);

                                    #endregion

                                    tranferCount++;
                                }
                            }
                        }

                        if (itemsCount == tranferCount)
                        {
                            Services.Notifier.Information(T("Import {0} ward records successfully", itemsCount));
                        }
                    }
                    catch (Exception error)
                    {
                        Services.TransactionManager.Cancel();
                        errorMsg = error.Message;
                        Services.Notifier.Error(T("Import {0} ward records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                    }
                }
            }
            else
            {
                Services.Notifier.Information(T("Data have {0} ward records.", dataCount));
            }
        }

        public void ImportLocationStreets()
        {
            int dataCount = Services.ContentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Count();
            if (dataCount == 0 && Services.ContentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>().Count() > 0)
            {
                using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
                {
                    int itemsCount = 0, tranferCount = 0, errorId = 0;
                    string errorStep = "", errorMsg = "";

                    try
                    {
                        dgndDataContext storeDB = new dgndDataContext();
                        var list = storeDB.LocationStreets.Where(a => a.Id > 0).OrderBy(a => a.LocationDistrict.LocationCity.SeqOrder).ThenBy(a => a.LocationDistrict.SeqOrder).ThenBy(a => a.SeqOrder).ToList();
                        itemsCount = list.Count;

                        var provinceNames = list.Select(a => a.LocationDistrict.LocationCity.Name).Distinct().ToList();

                        foreach (string provinceName in provinceNames)
                        {
                            var _province = _addressService.GetProvince(provinceName);
                            var districts = _addressService.GetDistricts(_province.Id);

                            foreach (var _district in districts)
                            {
                                var streets = list.Where(a => a.LocationDistrict.LocationCity.Name == provinceName && a.LocationDistrict.Name == _district.Name).OrderBy(a => a.Name).ToList();
                                var wards = _addressService.GetWards(_district.Id);

                                foreach (var record in streets)
                                {
                                    errorId = record.Id;

                                    #region COLLECT DATA

                                    var street = Services.ContentManager.New<LocationStreetPart>("LocationStreet");

                                    street.Province = _province;
                                    street.District = _district;
                                    if (record.WardId > 0)
                                        street.Ward = wards.FirstOrDefault(a => a.Name == record.LocationWard.Name);
                                    street.Name = record.Name;
                                    street.ShortName = record.ShortName;
                                    street.SeqOrder = streets.IndexOf(record) + 1;
                                    street.IsEnabled = true;
                                    street.StreetWidth = record.Width;
                                    street.IsOneWayStreet = record.IsOneWayStreet;
                                    street.CoefficientAlley1Max = record.CoefficientAlley1Max;
                                    street.CoefficientAlley1Min = record.CoefficientAlley1Min;
                                    street.CoefficientAlleyEqual = record.CoefficientAlleyEqual;
                                    street.CoefficientAlleyMax = record.CoefficientAlleyMax;
                                    street.CoefficientAlleyMin = record.CoefficientAlleyMin;

                                    Services.ContentManager.Create(street);

                                    #endregion

                                    tranferCount++;
                                }
                            }
                        }

                        if (itemsCount == tranferCount)
                        {
                            Services.Notifier.Information(T("Import {0} street records successfully", itemsCount));
                        }
                    }
                    catch (Exception error)
                    {
                        Services.TransactionManager.Cancel();
                        errorMsg = error.Message;
                        Services.Notifier.Error(T("Import {0} street records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                    }
                }
            }
            else
            {
                Services.Notifier.Information(T("Data have {0} street records.", dataCount));
            }
        }

        public void ImportStreetRelations()
        {
            int dataCount = Services.ContentManager.Query<StreetRelationPart, StreetRelationPartRecord>().Count();
            if (dataCount == 0 && (Services.ContentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Count() > 0))
            {
                using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
                {
                    int itemsCount = 0, tranferCount = 0, errorId = 0;
                    string errorStep = "", errorMsg = "";

                    try
                    {
                        dgndDataContext storeDB = new dgndDataContext();
                        var streetRelationList = storeDB.LocationRelatedStreets.OrderBy(a => a.DistrictId).ThenBy(a => a.WardId).ThenBy(a => a.LocationStreet.Name).ToList();
                        itemsCount = streetRelationList.Count;

                        foreach (var r in streetRelationList)
                        {
                            errorId = r.Id;

                            var p = _addressService.GetProvinces().FirstOrDefault(a => a.Name == r.LocationDistrict.LocationCity.Name);
                            var d = _addressService.GetDistricts(p.Id).FirstOrDefault(a => a.Name == r.LocationDistrict.Name);
                            var w = _addressService.GetWards(d.Id).FirstOrDefault(a => a.Name == r.LocationWard.Name);
                            var s = _addressService.GetStreets(d.Id).FirstOrDefault(a => a.Name == r.LocationStreet.Name);

                            var p1 = _addressService.GetProvinces().FirstOrDefault(a => a.Name == r.LocationDistrict1.LocationCity.Name);
                            var d1 = _addressService.GetDistricts(p1.Id).FirstOrDefault(a => a.Name == r.LocationDistrict1.Name);
                            var w1 = _addressService.GetWards(d1.Id).FirstOrDefault(a => a.Name == r.LocationWard1.Name);
                            var s1 = _addressService.GetStreets(d1.Id).FirstOrDefault(a => a.Name == r.LocationStreet1.Name);

                            StreetRelationPartRecord streetRelationRecord = new StreetRelationPartRecord
                            {
                                Province = p,
                                District = d,
                                Ward = w,
                                Street = s,
                                StreetWidth = r.StreetWidth,
                                RelatedProvince = p1,
                                RelatedDistrict = d1,
                                RelatedWard = w1,
                                RelatedStreet = s1,
                                RelatedStreetWidth = r.RelatedStreetWidth,
                                RelatedValue = r.RelatedValue
                            };

                            var model = Services.ContentManager.New<StreetRelationPart>("StreetRelation");
                            model.Record = streetRelationRecord;
                            Services.ContentManager.Create(model);

                            tranferCount++;
                        }

                        if (itemsCount == tranferCount)
                        {
                            Services.Notifier.Information(T("Import {0} street relation records successfully", itemsCount));
                        }
                    }
                    catch (Exception error)
                    {
                        Services.TransactionManager.Cancel();
                        errorMsg = error.Message;
                        Services.Notifier.Error(T("Import {0} street relation records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                    }
                }
            }
            else
            {
                Services.Notifier.Information(T("Data have {0} street relation records.", dataCount));
            }
        }

        public void ImportPropertyList()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var pList = storeDB.Properties.OrderBy(a => a.Id).ToList();
                    itemsCount = pList.Count;

                    foreach (var p in pList)
                    {
                        var property = Services.ContentManager.New<PropertyPart>("Property");
                        errorId = p.Id;

                        #region COLLECT DATA

                        property.PropertyId = p.Id;

                        // Type
                        #region Type

                        errorStep = "Type";
                        string _typeName = "";

                        if (p.ConstructionTypeId == null)
                        {
                            switch (p.PropertyType.Name)
                            {
                                case "Căn hộ, chung cư":
                                    _typeName = "Căn hộ, chung cư";
                                    break;
                                case "Đất nền dự án":
                                    _typeName = "Đất dự án phân lô";
                                    break;
                                case "Các loại đất khác":
                                    _typeName = "Các loại đất khác";
                                    break;
                                case "Kho bãi":
                                    _typeName = "Các loại đất khác";
                                    break;
                                case "Nhà chung nhiều hộ":
                                    _typeName = "Một phần nhà, đất";
                                    break;
                            }
                        }
                        else
                        {
                            switch (p.PropertyConstructionType.Name)
                            {
                                case "Chưa xây dựng":
                                    _typeName = "Đất ở";
                                    break;
                                case "Nhà tạm (nhà nát)":
                                    _typeName = "Nhà nát";
                                    break;
                                case "Nhà cấp 4 (trệt)":
                                    _typeName = "Nhà cấp 4";
                                    break;
                                case "Nhà gác gỗ":
                                    _typeName = "Nhà gác gỗ";
                                    break;
                                case "Nhà đúc giả":
                                    _typeName = "Nhà phố đúc giả";
                                    break;
                                case "Nhà đúc kiên cố":
                                    _typeName = "Nhà phố đúc kiên cố";
                                    break;
                                case "Villa - Biệt thự":
                                    _typeName = "Biệt thự";
                                    break;
                                case "Cao ốc VP":
                                    _typeName = "Cao ốc văn phòng";
                                    break;
                                case "Căn hộ cao cấp":
                                    _typeName = "Căn hộ, chung cư";
                                    break;
                                case "Chung cư":
                                    _typeName = "Căn hộ, chung cư";
                                    break;
                                case "Nhà chung nhiều hộ":
                                    _typeName = "Một phần nhà, đất";
                                    break;
                                case "Khách sạn":
                                    _typeName = "Khách sạn";
                                    break;
                                case "Nhà hàng":
                                    _typeName = "Nhà hàng";
                                    break;
                                case "Quán Cafe":
                                    _typeName = "Quán cafe";
                                    break;
                                case "Kho xưởng":
                                    _typeName = "Kho xưởng";
                                    break;
                                case "Nhà xây phòng trọ":
                                    _typeName = "Khách sạn";
                                    break;
                            }
                        }

                        try
                        {
                            property.Type = _propertyService.GetType(_typeName);
                            if (property.Type.CssClass == "tp-residential-land")
                            {
                                property.AreaConstruction = null;
                                property.AreaUsable = null;

                                property.Floors = null;
                                property.Bedrooms = null;
                                property.Bathrooms = null;
                                property.Livingrooms = null;
                                property.Balconies = null;

                                property.HaveBasement = false;
                                property.HaveElevator = false;
                                property.HaveGarage = false;
                                property.HaveGarden = false;
                                property.HaveMezzanine = false;
                                property.HaveSkylight = false;
                                property.HaveSwimmingPool = false;
                                property.HaveTerrace = false;

                                property.RemainingValue = null;
                                property.Interior = null;
                            }
                            else
                            {
                                // Construction
                                property.AreaConstruction = p.AreaConstruction;
                                property.AreaUsable = p.AreaUsable;

                                property.Floors = p.Floors;
                                property.Bedrooms = p.Bedrooms;
                                property.Livingrooms = p.Livingrooms;
                                property.Bathrooms = p.Bathrooms;
                                property.Balconies = p.Balconies;

                                property.HaveBasement = p.HaveBasement;
                                property.HaveElevator = p.HaveElevator;
                                property.HaveGarage = p.HaveGarage;
                                property.HaveGarden = p.HaveGarden;
                                property.HaveMezzanine = p.HaveMezzanine;
                                property.HaveSkylight = p.HaveSkylight;
                                property.HaveSwimmingPool = p.HaveSwimmingPool;
                                property.HaveTerrace = p.HaveTerrace;

                                property.RemainingValue = p.RemainingValue;

                                // Interior
                                errorStep = "Interior";
                                if (p.InteriorTypeId.HasValue)
                                    property.Interior = _propertyService.GetInteriors().Single(a => a.Name == p.PropertyInteriorType.Name);
                                else
                                    property.Interior = null;
                            }
                        }
                        catch
                        {
                            errorId = p.Id;
                            break;
                        }

                        #endregion

                        // Province
                        errorStep = "Province";
                        property.Province = _addressService.GetProvinces().Single(a => a.Name == p.LocationCity.Name);
                        property.OtherProvinceName = null;

                        // District
                        errorStep = "District";
                        if (p.DistrictId > 0)
                        {
                            property.District = _addressService.GetDistricts(property.Province.Id).Single(a => a.Name == p.LocationDistrict.Name);
                            property.OtherDistrictName = "";

                            // Ward
                            errorStep = "Ward";
                            if (p.WardId > 0)
                            {
                                property.Ward = _addressService.GetWards(property.District.Id).Single(a => a.Name == p.LocationWard.Name);
                                property.OtherWardName = "";
                            }
                            else
                            {
                                property.Ward = null;
                                property.OtherWardName = p.OtherWardName;
                            }

                            // Street
                            errorStep = "Street";
                            if (p.StreetId > 0)
                            {
                                property.Street = _addressService.GetStreets(property.District.Id).Single(a => a.Name == p.LocationStreet.Name);
                                property.OtherStreetName = "";
                            }
                            else
                            {
                                property.Street = null;
                                property.OtherStreetName = p.OtherStreetName;
                            }
                        }
                        else
                        {
                            property.District = null;
                            property.OtherDistrictName = p.OtherDistrictName;
                            property.Ward = null;
                            property.OtherWardName = p.OtherWardName;
                            property.Street = null;
                            property.OtherStreetName = p.OtherStreetName;
                        }

                        // Address
                        property.AddressNumber = p.AddressNumber;
                        property.AlleyNumber = _propertyService.IntParseAddressNumber(property.AddressNumber);

                        // LegalStatus
                        errorStep = "LegalStatus";
                        if (p.LegalStatusId > 0)
                            property.LegalStatus = _propertyService.GetLegalStatus(p.PropertyLegalStatuse.Name);
                        else
                            property.LegalStatus = null;

                        // Direction
                        errorStep = "Direction";
                        if (p.DirectionId > 0)
                            property.Direction = _propertyService.GetDirections().Single(a => a.Name == p.PropertyDirection.Name);
                        else
                            property.Direction = null;


                        // Location
                        errorStep = "Location";
                        string locationCssClass = (p.LocationId == null || p.LocationId == 1) ? "h-front" : "h-alley";
                        property.Location = _propertyService.GetLocation(locationCssClass);
                        if (property.Location.CssClass == "h-front")
                        {
                            property.AlleyTurns = null;
                            property.AlleyWidth = null;
                            property.AlleyWidth1 = null;
                            property.AlleyWidth2 = null;
                            property.AlleyWidth3 = null;
                            property.AlleyWidth4 = null;
                            property.AlleyWidth5 = null;
                            property.AlleyWidth6 = null;
                            property.AlleyWidth7 = null;
                            property.AlleyWidth8 = null;
                            property.AlleyWidth9 = null;
                            property.StreetWidth = null;
                            property.DistanceToStreet = null;
                        }
                        else
                        {
                            // Alley
                            property.AlleyTurns = p.AlleyTurns;
                            property.AlleyWidth1 = p.AlleyWidth1;
                            property.AlleyWidth2 = p.AlleyWidth2;
                            property.AlleyWidth3 = p.AlleyWidth3;
                            property.AlleyWidth4 = p.AlleyWidth4;
                            property.AlleyWidth5 = p.AlleyWidth5;
                            property.AlleyWidth6 = p.AlleyWidth6;
                            property.AlleyWidth7 = p.AlleyWidth7;
                            property.AlleyWidth8 = p.AlleyWidth8;
                            property.AlleyWidth9 = p.AlleyWidth9;
                            if (p.AlleyTurns > 0)
                                property.AlleyWidth = new List<double?> { p.AlleyWidth1, p.AlleyWidth2, p.AlleyWidth3, p.AlleyWidth4, p.AlleyWidth5, p.AlleyWidth6, p.AlleyWidth7, p.AlleyWidth8, p.AlleyWidth9 }[(int)p.AlleyTurns - 1];
                            property.StreetWidth = p.StreetWidth;
                            property.DistanceToStreet = p.DistanceToStreet;
                        }

                        // AreaTotal
                        property.AreaTotal = p.AreaTotal;
                        property.AreaTotalWidth = p.AreaTotalWidth;
                        property.AreaTotalLength = p.AreaTotalLength;
                        property.AreaTotalBackWidth = p.AreaTotalBackWidth;

                        // AreaLegal
                        property.AreaLegal = p.AreaLegal;
                        property.AreaLegalWidth = p.AreaLegalWidth;
                        property.AreaLegalLength = p.AreaLegalLength;
                        property.AreaLegalBackWidth = p.AreaLegalBackWidth;
                        property.AreaIlegalRecognized = p.AreaIlegalRecognized;
                        property.AreaIlegalNotRecognized = p.AreaIlegalNotRecognized;
                        
                        // Contact
                        property.ContactName = p.ContactName;
                        property.ContactPhone = p.ContactPhone;
                        property.ContactAddress = p.ContactAddress;
                        property.ContactEmail = p.ContactEmail;

                        // Price
                        property.PriceProposed = p.PriceProposed;
                        property.PaymentMethod = _propertyService.GetPaymentMethods().Single(a => a.Name == p.PaymentMethod.Name);
                        property.PaymentUnit = _propertyService.GetPaymentUnits().Single(a => a.Name == p.PaymentUnit.Name);

                        // Status
                        errorStep = "Status";
                        property.Published = true;
                        property.Status = _propertyService.GetStatus().Single(a => a.Name == p.PropertyStatuse.Name);

                        // Flag
                        errorStep = "Flag";
                        property.Flag = _propertyService.GetFlags().Single(a => a.Name == p.PropertyFlag.Name);
                        property.IsExcludeFromPriceEstimation = p.IsExcludeFromPriceEstimation;

                        // User
                        errorStep = "CreatedUser";
                        property.CreatedDate = p.CreatedDate;
                        property.CreatedUser = _groupService.GetUser(p.CreatedUser.UserName);
                        errorStep = "LastUpdatedUser";
                        property.LastUpdatedDate = p.LastUpdatedDate;
                        property.LastUpdatedUser = _groupService.GetUser(p.LastUpdatedUser.UserName);
                        errorStep = "FirstInfoFromUser";
                        property.FirstInfoFromUser = _groupService.GetUser(p.InfoFromUserFirst.UserName);
                        errorStep = "LastInfoFromUser";
                        property.LastInfoFromUser = _groupService.GetUser(p.InfoFromUser.UserName);

                        // Ads Type
                        property.AdsType = null;

                        // Ads Content
                        property.Title = "";
                        property.Content = p.Content;
                        property.Note = p.Notes;

                        #endregion

                        Services.ContentManager.Create(property);
                        
                        // AreaUsable
                        property.AreaUsable = _propertyService.CalcAreaUsable(property);

                        // PriceProposedInVND
                        property.PriceProposedInVND = _propertyService.CaclPriceProposedInVND(property);

                        tranferCount++;
                    }

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} property records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} property records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportPropertyFiles()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var fList = storeDB.PropertyFiles.Where(a => a.PropertyId.HasValue).OrderBy(a => a.Id).ToList();
                    itemsCount = fList.Count;

                    foreach (var f in fList)
                    {
                        var file = Services.ContentManager.New<PropertyFilePart>("PropertyFile");
                        errorId = f.Id;

                        #region COLLECT DATA

                        file.Name = f.FileName;
                        file.Type = f.FileType;
                        file.Size = (int)f.FileSize;

                        file.CreatedDate = (DateTime)f.CreatedDate;
                        file.CreatedUser = _groupService.GetUser(storeDB.aspnet_Users.Where(a => a.UserId == (Guid)f.CreatedUserId).Select(a => a.UserName).FirstOrDefault());

                        file.Property = _propertyService.GetPropertyById(f.PropertyId);
                        file.Path = f.FilePath.Replace(f.PropertyId.ToString(), file.Property.Id.ToString());

                        string _uploadsFolder = Server.MapPath("~/UserFiles/" + file.Property.Id);
                        DirectoryInfo Folder = new DirectoryInfo(_uploadsFolder);
                        if (!Folder.Exists) Folder.Create();

                        try
                        {
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile("http://dinhgianhadat.vn" + f.FilePath, Server.MapPath("~" + file.Path));
                            Services.ContentManager.Create(file);
                            tranferCount++;
                        }
                        catch (System.Net.WebException e)
                        {
                            // Skip this record
                            Services.Notifier.Information(T("Skip file {0}, Exception: {1}", f.FilePath, e.Message));
                        }

                        #endregion

                    }

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} property file records successfully", itemsCount));
                    }
                    else
                    {
                        Services.Notifier.Information(T("Import {0} / {1} property file records successfully", tranferCount, itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} property file records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportPropertyRevisions()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                //DB work here, with 1 hour transaction timeout

                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var revList = storeDB.PropertyRevisions.OrderBy(a => a.Date).ToList();
                    //var revList = storeDB.PropertyRevisions.OrderByDescending(a => a.Date).Take(12950).ToList();
                    itemsCount = revList.Count;

                    #region COLLECT DATA

                    foreach (var rev in revList)
                    {
                        var records = rev.PropertyRevisionRecords;
                        foreach (var record in records)
                        {
                            var revision = Services.ContentManager.New<RevisionPart>("Revision");
                            errorId = rev.Id;

                            revision.CreatedDate = rev.Date;
                            revision.CreatedUser = _groupService.GetUser(rev.aspnet_User.UserName);
                            revision.ContentType = "Property";
                            revision.ContentTypeRecordId = _propertyService.GetPropertyById(rev.PropertyId).Id;
                            revision.FieldName = record.FieldName;
                            revision.ValueBefore = record.FieldValue;

                            Services.ContentManager.Create(revision);
                        }
                        tranferCount++;
                    }

                    #endregion

                    //if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} property revision records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} property revision records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportCustomerList()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var cList = storeDB.Customers.OrderBy(a => a.CustomerId).ToList();
                    itemsCount = cList.Count;
                    foreach (var c in cList)
                    {
                        var customer = Services.ContentManager.New<CustomerPart>("Customer");
                        errorId = c.CustomerId;

                        #region COLLECT DATA

                        customer.CustomerId = c.CustomerId;

                        customer.ContactName = c.CustomerName;
                        customer.ContactPhone = c.CustomerPhone;
                        customer.ContactAddress = c.CustomerAddress;
                        customer.ContactEmail = c.CustomerEmail;
                        customer.Note = c.CustomerNotes;

                        errorStep = "CreatedUser";
                        customer.CreatedDate = c.CreatedDate;
                        customer.CreatedUser = _groupService.GetUser(c.CreatedUser.UserName);

                        errorStep = "LastUpdatedUser";
                        customer.LastUpdatedDate = c.LastUpdatedDate;
                        if (c.LastUpdatedUserId != null)
                            customer.LastUpdatedUser = _groupService.GetUser(storeDB.aspnet_Users.Where(a => a.UserId == (Guid)c.LastUpdatedUserId).Select(a => a.UserName).FirstOrDefault());

                        errorStep = "LastCallUser";
                        customer.LastCallDate = c.LastCallDate;
                        if (c.LastCallByUserId != null)
                            customer.LastCallUser = _groupService.GetUser(storeDB.aspnet_Users.Where(a => a.UserId == (Guid)c.LastCallByUserId).Select(a => a.UserName).FirstOrDefault());

                        if (c.CustomerPriorityId.HasValue)
                        {
                            errorStep = "Status";
                            customer.Status = _customerService.GetStatus(c.CustomerPriority.CssClass);
                        }
                        else
                        {
                            customer.Status = _customerService.GetStatus("st-new");
                        }
                        if (c.IsPotential) customer.Status = _customerService.GetStatus("st-negotiate");

                        Services.ContentManager.Create(customer);

                        if (c.CustomerPurposeId.HasValue)
                        {
                            errorStep = "Purpose";
                            var purposes = _customerService.GetPurposes().Where(r => r.Name == c.CustomerPurpose.Name)
                                .Select(r => new CustomerPurposeEntry { IsChecked = true, Purpose = r }).ToList();

                            _customerService.UpdatePurposesForContentItem(customer, purposes);
                        }

                        #endregion

                        tranferCount++;
                    }

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} customer records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} customer records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportCustomerRequirements()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var reqList = storeDB.CustomerRequirements.OrderBy(a => a.Id).ToList();
                    itemsCount = reqList.Count;

                    foreach (var req in reqList)
                    {
                        var requirement = new CustomerRequirementRecord();
                        errorId = req.Id;

                        #region COLLECT DATA

                        errorStep = "CustomerPartRecord";
                        requirement.CustomerPartRecord = _customerService.GetCustomerById(req.CustomerId);

                        requirement.GroupId = req.GroupId;
                        requirement.IsEnabled = req.IsEnabled;

                        if (req.CityId.HasValue)
                        {
                            errorStep = "LocationProvincePartRecord";
                            requirement.LocationProvincePartRecord = _addressService.GetProvince(req.LocationCity.Name);
                            if (req.DistrictId.HasValue)
                            {
                                errorStep = "LocationDistrictPartRecord";
                                requirement.LocationDistrictPartRecord = _addressService.GetDistricts(requirement.LocationProvincePartRecord.Id).FirstOrDefault(a => a.Name == req.LocationDistrict.Name);
                                if (req.WardId.HasValue)
                                {
                                    errorStep = "LocationWardPartRecord";
                                    requirement.LocationWardPartRecord = _addressService.GetWards(requirement.LocationDistrictPartRecord.Id).FirstOrDefault(a => a.Name == req.LocationWard.Name);
                                }
                                if (req.StreetId.HasValue)
                                {
                                    errorStep = "LocationStreetPartRecord";
                                    requirement.LocationStreetPartRecord = _addressService.GetStreets(requirement.LocationDistrictPartRecord.Id).FirstOrDefault(a => a.Name == req.LocationStreet.Name);
                                }
                            }
                        }

                        requirement.MinArea = req.MinArea;
                        requirement.MaxArea = req.MaxArea;
                        requirement.MinWidth = req.MinWidth;
                        requirement.MaxWidth = req.MaxWidth;
                        requirement.MinLength = req.MinLength;
                        requirement.MaxLength = req.MaxLength;

                        errorStep = "DirectionPartRecord";
                        if (req.DirectionId.HasValue)
                            requirement.DirectionPartRecord = _propertyService.GetDirections().FirstOrDefault(a => a.Name == req.PropertyDirection.Name);

                        errorStep = "PropertyLocationPartRecord";
                        if (req.LocationId.HasValue)
                        {
                            string locationCssClass = (req.LocationId == 1) ? "h-front" : "h-alley";
                            requirement.PropertyLocationPartRecord = _propertyService.GetLocation(locationCssClass);
                        }

                        requirement.MinAlleyWidth = req.MinAlleyWidth;
                        requirement.MaxAlleyWidth = req.MaxAlleyWidth;
                        //requirement.MinAlleyTurns
                        requirement.MaxAlleyTurns = req.MaxAlleyTurns;
                        //requirement.MinDistanceToStreet
                        requirement.MaxDistanceToStreet = req.MaxDistanceToStreet;
                        requirement.MinFloors = req.MinFloors;
                        requirement.MaxFloors = req.MaxFloors;
                        requirement.MinBedrooms = req.MinBedrooms;
                        requirement.MaxBedrooms = req.MaxBedrooms;
                        //requirement.MinBathrooms
                        //requirement.MaxBathrooms
                        requirement.MinPrice = req.MinPrice;
                        requirement.MaxPrice = req.MaxPrice;

                        errorStep = "PaymentMethodPartRecord";
                        requirement.PaymentMethodPartRecord = _propertyService.GetPaymentMethods().SingleOrDefault(a => a.Name == req.PaymentMethod.Name);

                        #endregion

                        _customerService.CreateCustomerRequirement(requirement);

                        tranferCount++;
                    }

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} customer requirement records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} customer requirement records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportCustomerRevisions()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                //DB work here, with 1 hour transaction timeout

                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var revList = storeDB.CustomerRevisions.OrderBy(a => a.ChangedDate).ToList();
                    itemsCount = revList.Count;

                    #region COLLECT DATA

                    foreach (var rev in revList)
                    {
                        var records = rev.CustomerRevisionRecords;
                        foreach (var record in records)
                        {
                            var revision = Services.ContentManager.New<RevisionPart>("Revision");
                            errorId = rev.CustomerRevisionId;

                            revision.CreatedDate = rev.ChangedDate;
                            revision.CreatedUser = _groupService.GetUser(rev.aspnet_User.UserName);
                            revision.ContentType = "Customer";
                            revision.ContentTypeRecordId = _customerService.GetCustomerById(rev.CustomerId).Id;
                            revision.FieldName = record.FieldName;
                            revision.ValueBefore = record.FieldValue;

                            Services.ContentManager.Create(revision);
                        }
                        tranferCount++;
                    }

                    #endregion

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} user revision records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} user revision records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportCustomerProperties()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var cList = storeDB.CustomerProperties.OrderBy(a => a.Id).ToList();
                    itemsCount = cList.Count;
                    foreach (var c in cList)
                    {
                        errorId = c.CustomerId;

                        #region COLLECT DATA

                        var customer = _customerService.GetCustomerById(c.CustomerId);
                        var property = _propertyService.GetPropertyById(c.PropertyId);
                        var feedback = _customerService.GetFeedback(c.CustomerFeedback.CssClass);

                        var cp = _customerService.CreateCustomerProperty(customer, property, feedback);

                        foreach (var staff in c.CustomerStaffs)
                        {
                            var user = _groupService.GetUser(staff.aspnet_User.UserName);
                            _customerService.CreateCustomerPropertyUser(cp, user, (DateTime)staff.Date, staff.IsOverTimeWork);
                        }

                        #endregion

                        tranferCount++;
                    }

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} user property records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} user property records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportUserActivities()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();
                    var list = storeDB.UserActivities.OrderBy(a => a.Id).ToList();
                    itemsCount = list.Count;

                    foreach (var record in list)
                    {
                        errorId = record.Id;

                        #region COLLECT DATA

                        var createdDate = (DateTime)record.Date;
                        var createdUser = _groupService.GetUser(record.aspnet_User.UserName);
                        var property = _propertyService.GetPropertyById(record.PropertyId);
                        var customer = _customerService.GetCustomerById(record.CustomerId);

                        switch (record.ActionId)
                        {
                            case 1: //Thêm mới BĐS
                                _revisionService.SaveUserActivityAddNewProperty(createdDate, createdUser, property);
                                break;
                            case 2: //Sửa thông tin BĐS
                                _revisionService.SaveUserActivityUpdateProperty(createdDate, createdUser, property);
                                break;
                            case 3: //Dẫn khách
                                _revisionService.SaveUserActivityServeCustomer(createdDate, createdUser, customer);
                                break;
                            case 4: //BĐS nguồn chờ xóa
                                _revisionService.SaveUserActivityOwnPropertyDeleted(createdDate, createdUser, property);
                                break;
                        }

                        #endregion

                        tranferCount++;
                    }

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} user activity records successfully", itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} user activity records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        public void ImportRevisionData()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, scopeTimeout))
            {
                int itemsCount = 0, tranferCount = 0, errorId = 0;
                string errorStep = "", errorMsg = "";

                try
                {
                    dgndDataContext storeDB = new dgndDataContext();

                    #region COLLECT DATA

                    // StatusId
                    var statusList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "StatusId").List();
                    itemsCount += statusList.Count();
                    foreach (var item in statusList)
                    {
                        errorId = item.Id;
                        string statusCssClass = "";
                        switch (item.ValueBefore)
                        {
                            case "1":
                                statusCssClass = "st-new";
                                break;
                            case "2":
                                statusCssClass = "st-selling";
                                break;
                            case "3":
                                statusCssClass = "st-negotiate";
                                break;
                            case "4":
                                statusCssClass = "st-sold";
                                break;
                            case "5":
                                statusCssClass = "st-onhold";
                                break;
                            case "6":
                                statusCssClass = "st-trash";
                                break;
                            case "7":
                                statusCssClass = "st-deleted";
                                break;
                            case "8":
                                statusCssClass = "st-selling";
                                break;
                            case "9":
                                statusCssClass = "st-pending";
                                break;
                            case "10":
                                statusCssClass = "st-trading";
                                break;
                            case "11":
                                statusCssClass = "st-no-contact";
                                break;
                        }
                        if (!string.IsNullOrEmpty(statusCssClass))
                        {
                            item.ValueBefore = _propertyService.GetStatus(statusCssClass).Id.ToString();
                            tranferCount++;
                        }
                    }

                    // PaymentMethodId
                    var paymentMethodList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "PaymentMethodId").List();
                    itemsCount += paymentMethodList.Count();
                    foreach (var item in paymentMethodList)
                    {
                        errorId = item.Id;
                        int id = int.Parse(item.ValueBefore);
                        if (id <= 5)
                        {
                            string name = storeDB.PaymentMethods.Single(a => a.Id == id).Name;
                            item.ValueBefore = _propertyService.GetPaymentMethods().Single(a => a.Name == name).Id.ToString();
                            tranferCount++;
                        }
                    }

                    // PaymentUnitId
                    var paymentUnitList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "PaymentUnitId").List();
                    itemsCount += paymentUnitList.Count();
                    foreach (var item in paymentUnitList)
                    {
                        errorId = item.Id;
                        int id = int.Parse(item.ValueBefore);
                        if (id <= 2)
                        {
                            string name = storeDB.PaymentUnits.Single(a => a.Id == id).Name;
                            item.ValueBefore = _propertyService.GetPaymentUnits().Single(a => a.Name == name).Id.ToString();
                            tranferCount++;
                        }
                    }

                    // InfoFromUserId
                    var infoFromList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "InfoFromUserId").List();
                    itemsCount += infoFromList.Count();
                    foreach (var item in infoFromList)
                    {
                        errorId = item.Id;
                        int test;
                        if (!int.TryParse(item.ValueBefore, out test))
                        {
                            Guid userId = Guid.Parse(item.ValueBefore);
                            string userName = "";
                            if (storeDB.aspnet_Users.Any(a => a.UserId == userId))
                            {
                                userName = storeDB.aspnet_Users.Single(a => a.UserId == userId).UserName;
                            }
                            else
                            {
                                userName = "congty";
                            }
                            item.ValueBefore = _groupService.GetUser(userName).Id.ToString();
                            tranferCount++;
                        }
                    }

                    // LastInfoFromUserId
                    var lastInfoFromList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "LastInfoFromUserId").List();
                    itemsCount += lastInfoFromList.Count();
                    foreach (var item in lastInfoFromList)
                    {
                        errorId = item.Id;
                        int test;
                        if (!int.TryParse(item.ValueBefore, out test))
                        {
                            Guid userId = Guid.Parse(item.ValueBefore);
                            string userName = "";
                            if (storeDB.aspnet_Users.Any(a => a.UserId == userId))
                            {
                                userName = storeDB.aspnet_Users.Single(a => a.UserId == userId).UserName;
                            }
                            else
                            {
                                userName = "congty";
                            }
                            item.ValueBefore = _groupService.GetUser(userName).Id.ToString();
                            tranferCount++;
                        }
                    }

                    // AdsNewspaperUserId
                    var adsNewspaperList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "AdsNewspaperUserId").List();
                    itemsCount += adsNewspaperList.Count();
                    foreach (var item in adsNewspaperList)
                    {
                        errorId = item.Id;
                        int test;
                        if (!int.TryParse(item.ValueBefore, out test))
                        {
                            Guid userId = Guid.Parse(item.ValueBefore);
                            string userName = "";
                            if (storeDB.aspnet_Users.Any(a => a.UserId == userId))
                            {
                                userName = storeDB.aspnet_Users.Single(a => a.UserId == userId).UserName;
                            }
                            else
                            {
                                userName = "congty";
                            }
                            item.ValueBefore = _groupService.GetUser(userName).Id.ToString();
                            tranferCount++;
                        }
                    }

                    // AdsOnlineUserId
                    var adsOnlineList = Services.ContentManager
                        .Query<RevisionPart, RevisionPartRecord>().Where(a => a.FieldName == "AdsOnlineUserId").List();
                    itemsCount += adsOnlineList.Count();
                    foreach (var item in adsOnlineList)
                    {
                        errorId = item.Id;
                        int test;
                        if (!int.TryParse(item.ValueBefore, out test))
                        {
                            Guid userId = Guid.Parse(item.ValueBefore);
                            string userName = "";
                            if (storeDB.aspnet_Users.Any(a => a.UserId == userId))
                            {
                                userName = storeDB.aspnet_Users.Single(a => a.UserId == userId).UserName;
                            }
                            else
                            {
                                userName = "congty";
                            }
                            item.ValueBefore = _groupService.GetUser(userName).Id.ToString();
                            tranferCount++;
                        }
                    }

                    #endregion

                    if (itemsCount == tranferCount)
                    {
                        Services.Notifier.Information(T("Import {0} revision records successfully", itemsCount));
                    }
                    else
                    {
                        Services.Notifier.Information(T("Import {0} / {1} revision file records successfully", tranferCount, itemsCount));
                    }
                }
                catch (Exception error)
                {
                    Services.TransactionManager.Cancel();
                    errorMsg = error.Message;
                    Services.Notifier.Error(T("Import {0} revision records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount, tranferCount, errorId, errorStep, errorMsg));
                }
            }
        }

        */

        public void ConvertPriceProposedInVnd()
        {
            int itemsCount = 0, tranferCount = 0, errorId = 0;
            string errorStep = "";

            try
            {
                IEnumerable<PropertyPart> pList =
                    Services.ContentManager.Query<PropertyPart, PropertyPartRecord>().List().ToList();
                itemsCount = pList.Count();

                foreach (PropertyPart item in pList)
                {
                    errorId = item.Id;
                    errorStep = "CaclPriceProposedInVnd";
                    item.PriceProposedInVND = _propertyService.CaclPriceProposedInVnd(item);
                    tranferCount++;
                }

                Services.Notifier.Information(itemsCount == tranferCount
                    ? T("Convert {0} records successfully", itemsCount)
                    : T("Convert {0} / {1} records successfully", tranferCount, itemsCount));
            }
            catch (Exception error)
            {
                Services.TransactionManager.Cancel();
                string errorMsg = error.Message;
                Services.Notifier.Error(
                    T("Import {0} revision records failed at record {1} Id: {2} Step: {3} Error: {4}", itemsCount,
                        tranferCount, errorId, errorStep, errorMsg));
            }
        }

        public void ConvertAdvantages()
        {
            int itemsCount = 0, tranferCount = 0, errorId = 0;
            string errorStep = "";

            try
            {
                IEnumerable<PropertyPart> pList = Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                    .Where(a =>
                        a.HaveBasement ||
                        a.HaveMezzanine ||
                        a.HaveTerrace ||
                        a.HaveGarage ||
                        a.HaveElevator ||
                        a.HaveSwimmingPool ||
                        a.HaveGarden ||
                        a.HaveSkylight
                    )
                    .List().ToList();

                itemsCount = pList.Count();

                //var apartmentAdvantages = _propertyService.GetApartmentAdvantages();
                //var apartmentInteriorAdvantages = _propertyService.GetApartmentInteriorAdvantages();
                IEnumerable<PropertyAdvantagePartRecord> apartmentConstructionAdvantages =
                    _propertyService.GetConstructionAdvantages().ToList();

                foreach (PropertyPart item in pList)
                {
                    errorId = item.Id;

                    #region Advantages

                    errorStep = "Advantages";

                    //List<PropertyAdvantageEntry> advantages = new List<PropertyAdvantageEntry>();

                    //if (item.AdvCornerStreet) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Nhà có 2 mặt đường chính"), IsChecked = true });
                    //if (item.AdvCornerStreetAlley) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Nhà có 1 mặt đường chính và 1 mặt hẻm"), IsChecked = true });
                    //if (item.AdvCornerAlley) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Nhà có 2 mặt hẻm"), IsChecked = true });
                    //if (item.AdvDoubleFront) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Nhà có hẻm sau lưng"), IsChecked = true });
                    //if (item.AdvNearSuperMarket) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Gần chợ, siêu thị (<500m)"), IsChecked = true });
                    //if (item.AdvNearPark) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Gần công viên, trung tâm giải trí (<500m)"), IsChecked = true });
                    //if (item.AdvLuxuryResidential) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Khu dân cư cao cấp có cổng bảo vệ"), IsChecked = true });
                    //if (item.AdvNearTradeCenter) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Khu trung tâm thương mại"), IsChecked = true });
                    //if (item.AdvSecurityArea) advantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetAdvantages().FirstOrDefault(a => a.Name == "Tiện làm quán cafe, nhà hàng, khách sạn"), IsChecked = true });

                    //_propertyService.UpdateAdvantagesForContentItem(item, advantages);

                    #endregion

                    #region DisAdvantages

                    errorStep = "DisAdvantages";

                    //List<PropertyAdvantageEntry> disadvantages = new List<PropertyAdvantageEntry>();

                    //if (item.DAdvFacingAlley) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Đường, hẻm đâm thẳng vào nhà"), IsChecked = true });
                    //if (item.DAdvFacingTemple) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Đối diện hoặc gần sát chùa, nhà thờ"), IsChecked = true });
                    //if (item.DadvFacingFuneral) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Đối diện hoặc gần sát nhà tang lễ, nhà xác"), IsChecked = true });
                    //if (item.DadvUnderBridge) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Dưới hoặc gần chân cầu, đường điện cao thế"), IsChecked = true });
                    //if (item.DAdvFacingDrain) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Có cống trước nhà"), IsChecked = true });
                    //if (item.DAdvFacingElectricityCylindrical) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Có trụ điện trước nhà"), IsChecked = true });
                    //if (item.DAdvFacingBigTree) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Có cây lớn trước nhà"), IsChecked = true });
                    //if (item.DadvShareWall) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Tường chung không thể xây mới"), IsChecked = true });
                    //if (item.DAdvPlanningSuspended) disadvantages.Add(new PropertyAdvantageEntry { Advantage = _propertyService.GetDisAdvantages().FirstOrDefault(a => a.Name == "Khu quy hoạch treo"), IsChecked = true });

                    //_propertyService.UpdateDisAdvantagesForContentItem(item, disadvantages);

                    #endregion

                    #region ApartmentAdvantages

                    errorStep = "ApartmentAdvantages";

                    //List<PropertyAdvantageEntry> apartmentAdvantagesToAdd = new List<PropertyAdvantageEntry>();

                    //if (item.ApartmentHaveChildcare == true) apartmentAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentAdvantages.Where(a => a.CssClass == "apartment-adv-Childcare").First(), IsChecked = true });
                    //if (item.ApartmentHavePark == true) apartmentAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentAdvantages.Where(a => a.CssClass == "apartment-adv-Park").First(), IsChecked = true });
                    //if (item.ApartmentHaveSwimmingPool == true) apartmentAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentAdvantages.Where(a => a.CssClass == "apartment-adv-SwimmingPool").First(), IsChecked = true });
                    //if (item.ApartmentHaveSuperMarket == true) apartmentAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentAdvantages.Where(a => a.CssClass == "apartment-adv-SuperMarket").First(), IsChecked = true });
                    //if (item.ApartmentHaveSportCenter == true) apartmentAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentAdvantages.Where(a => a.CssClass == "apartment-adv-SportCenter").First(), IsChecked = true });

                    //_propertyService.UpdatePropertyApartmentAdvantages(item, apartmentAdvantagesToAdd);

                    #endregion

                    #region ApartmentInteriorAdvantages

                    errorStep = "ApartmentInteriorAdvantages";

                    //List<PropertyAdvantageEntry> apartmentInteriorAdvantagesToAdd = new List<PropertyAdvantageEntry>();

                    //if (item.InteriorHaveWoodFloor == true) apartmentInteriorAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentInteriorAdvantages.Where(a => a.CssClass == "apartment-interior-adv-WoodFloor").First(), IsChecked = true });
                    //if (item.InteriorHaveToiletEquipment == true) apartmentInteriorAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentInteriorAdvantages.Where(a => a.CssClass == "apartment-interior-adv-ToiletEquipment").First(), IsChecked = true });
                    //if (item.InteriorHaveKitchenEquipment == true) apartmentInteriorAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentInteriorAdvantages.Where(a => a.CssClass == "apartment-interior-adv-KitchenEquipment").First(), IsChecked = true });
                    //if (item.InteriorHaveBedCabinets == true) apartmentInteriorAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentInteriorAdvantages.Where(a => a.CssClass == "apartment-interior-adv-BedCabinets").First(), IsChecked = true });
                    //if (item.InteriorHaveAirConditioner == true) apartmentInteriorAdvantagesToAdd.Add(new PropertyAdvantageEntry { Advantage = apartmentInteriorAdvantages.Where(a => a.CssClass == "apartment-interior-adv-AirConditioner").First(), IsChecked = true });

                    //_propertyService.UpdatePropertyApartmentInteriorAdvantages(item, apartmentInteriorAdvantagesToAdd);

                    #endregion

                    #region ConstructionAdvantages

                    errorStep = "ConstructionAdvantages";

                    var constructionAdvantages = new List<PropertyAdvantageEntry>();

                    if (item.HaveBasement)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Basement"),
                            IsChecked = true
                        });
                    if (item.HaveMezzanine)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Mezzanine"),
                            IsChecked = true
                        });
                    if (item.HaveTerrace)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Terrace"),
                            IsChecked = true
                        });
                    if (item.HaveGarage)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Garage"),
                            IsChecked = true
                        });
                    if (item.HaveElevator)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Elevator"),
                            IsChecked = true
                        });
                    if (item.HaveSwimmingPool)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-SwimmingPool"),
                            IsChecked = true
                        });
                    if (item.HaveGarden)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Garden"),
                            IsChecked = true
                        });
                    if (item.HaveSkylight)
                        constructionAdvantages.Add(new PropertyAdvantageEntry
                        {
                            Advantage =
                                apartmentConstructionAdvantages.First(a => a.CssClass == "construction-adv-Skylight"),
                            IsChecked = true
                        });

                    _propertyService.UpdatePropertyConstructionAdvantages(item, constructionAdvantages);

                    #endregion

                    tranferCount++;
                }

                Services.Notifier.Information(itemsCount == tranferCount
                    ? T("Convert {0} records successfully", itemsCount)
                    : T("Convert {0} / {1} records successfully", tranferCount, itemsCount));
            }
            catch (Exception error)
            {
                Services.TransactionManager.Cancel();
                string errorMsg = error.Message;
                Services.Notifier.Error(T("Import {0} records failed at record {1} Id: {2} Step: {3} Error: {4}",
                    itemsCount, tranferCount, errorId, errorStep, errorMsg));
            }
        }

        #endregion
    }
}