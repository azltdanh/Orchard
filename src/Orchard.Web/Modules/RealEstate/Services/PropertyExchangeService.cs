using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.ViewModels;
using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.UI.Notify;

namespace RealEstate.Services
{
    public interface IPropertyExchangeService : IDependency
    {
        CustomerRequirementExchangeEditViewModel BuildCustomerRequirementExchange(int groupid, int pId);
        CustomerRequirementExchangeCreateViewModel BuildCustomerRequirementExchangeCreate(int pId);

        PropertyExchangePartRecord CreatePropertyExchange(PropertyPart p);
        PropertyExchangePartRecord UpdatePropertyExchange(int id, string exchangeTypeClass, double values, int paymentMethodId);
        PropertyExchangePartRecord UpdatePropertyExchange(int id, CustomerPart c, string exchangeTypeClass, double values, int paymentMethodId);

        void DeletePropertyExchange(PropertyPart p, bool isAdmin);
        void DeletePropertyExchange(CustomerPart c, bool isAdmin);

        void UpdateStatusPropertyExchange(PropertyPart p, string statusCssClass);
        void UpdateStatusPropertyExchange(CustomerPart c, string statusCssClass);
        
    }

    public class PropertyExchangeService : IPropertyExchangeService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly ISignals _signals;
        private readonly IAddressService _addressService;
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;
        private readonly ICustomerService _customerService;
        private readonly IRepository<PropertyExchangePartRecord> _propertyExchangeRepository;

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours

        public PropertyExchangeService(
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IOrchardServices services,
            IAddressService addressService,
            IPropertyService propertyService,
            IUserGroupService groupService,
            ICustomerService customerService,
            IRepository<PropertyExchangePartRecord> propertyExchangeRepository)
        {
            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;
            _addressService = addressService;
            _propertyService = propertyService;
            _groupService = groupService;
            _customerService = customerService;
            _propertyExchangeRepository = propertyExchangeRepository;

            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public CustomerRequirementExchangeEditViewModel BuildCustomerRequirementExchange(int groupid, int pId)
        {
            var currentUserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            IEnumerable<CustomerRequirementRecord> requirements = _customerService.GetRequirements(groupid).ToList();
            CustomerRequirementRecord req = requirements.First();
            PropertyTypeGroupPartRecord typeGroup = req.PropertyTypeGroupPartRecord;

            int provinceId = req.LocationProvincePartRecord != null
                ? req.LocationProvincePartRecord.Id
                : (currentUserGroup != null && currentUserGroup.DefaultProvince != null)
                    ? currentUserGroup.DefaultProvince.Id
                    : _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            int[] districtIds =
                requirements.Where(a => a.LocationDistrictPartRecord != null)
                    .Select(a => a.LocationDistrictPartRecord.Id)
                    .ToArray();

            var customer = Services.ContentManager.Get<CustomerPart>(req.CustomerPartRecord.Id);
            IEnumerable<int> purposeIds = _customerService.GetCustomerPurposes(customer).Select(a => a.Id);
            //var _purposeIds = c.Purposes.Select(r => r.Id).ToList();
            var p = Services.ContentManager.Get<PropertyPart>(pId);

            #region CustomerFrontEndEditViewModel

            var viewmodel = new CustomerRequirementExchangeEditViewModel
            {
                //GroupId = req.GroupId,

                AdsTypeCssClass = req.AdsTypePartRecord != null ? req.AdsTypePartRecord.CssClass : "ad-exchange",
                AdsTypes = _propertyService.GetAdsTypes().Where(r => r.CssClass == "ad-exchange"),
                TypeGroupCssClass = typeGroup != null ? typeGroup.CssClass : "",
                TypeGroups = _propertyService.GetTypeGroups(),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvinces(),
                DistrictIds = districtIds,
                Districts = _addressService.GetDistricts(provinceId),
                WardIds =
                    requirements.Where(a => a.LocationWardPartRecord != null)
                        .Select(a => a.LocationWardPartRecord.Id)
                        .ToArray(),
                Wards = _addressService.GetWards(districtIds),
                StreetIds =
                    requirements.Where(a => a.LocationStreetPartRecord != null)
                        .Select(a => a.LocationStreetPartRecord.Id)
                        .ToArray(),
                Streets = _addressService.GetStreets(districtIds),
                ApartmentIds =
                    requirements.Where(a => a.LocationApartmentPartRecord != null)
                        .Select(a => a.LocationApartmentPartRecord.Id)
                        .ToArray(),
                Apartments = _addressService.GetApartments(districtIds),
                ChkOtherProjectName = requirements.Any(r => r.OtherProjectName != null),
                DirectionIds =
                    requirements.Where(a => a.DirectionPartRecord != null)
                        .Select(a => a.DirectionPartRecord.Id)
                        .ToArray(),
                Directions = _propertyService.GetDirections(),
                OtherProjectName = req.OtherProjectName,
                //ApartmentFloorThRange

                LocationId = req.PropertyLocationPartRecord != null ? req.PropertyLocationPartRecord.Id : 0,
                //AlleyTurnsRange

                MinAlleyWidth = req.MinAlleyWidth,
                MinArea = req.MinArea,
                MinWidth = req.MinWidth,
                MinLength = req.MinLength,
                MinFloors = req.MinFloors,
                MinBedrooms = req.MinBedrooms,
                MinBathrooms = req.MinBathrooms,
                MinPrice = req.MinPrice,
                MaxPrice = req.MaxPrice,
                Note = customer.Note,
                AdsExpirationDate = customer.AdsExpirationDate,
                Name = customer.ContactName,
                Phone = customer.ContactPhone,
                Email = customer.ContactEmail,
                Purposes =
                    _customerService.GetPurposes()
                        .Select(r => new CustomerPurposeEntry { Purpose = r, IsChecked = purposeIds.Contains(r.Id) })
                        .ToList(),
                Customer = customer,
                ExchangeTypes = _propertyService.GetExchangeTypeParts(),
                PaymentMethods = _propertyService.GetPaymentMethods(),
                //PaymentUnits = _propertyService.GetPaymentUnits()
                PropertyDisplayAddress = p != null ? p.DisplayForAddress : "Địa chỉ",
                PropertyId = pId
            };

            #endregion

            #region AlleyTurnsRange

            if (req.MinAlleyWidth.HasValue)
            {
                switch (Int32.Parse(req.MinAlleyWidth.ToString()))
                {
                    case 1:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley;
                        break;
                    case 2:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley2;
                        break;
                    case 3:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley3;
                        break;
                    case 4:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley4;
                        break;
                    case 5:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley5;
                        break;
                    case 6:
                        viewmodel.AlleyTurnsRange = PropertyDisplayLocation.Alley6;
                        break;
                }
            }
            else
            {
                if (req.PropertyLocationPartRecord != null && req.PropertyLocationPartRecord.CssClass == "h-front")
                {
                    viewmodel.AlleyTurnsRange = PropertyDisplayLocation.AllWalk;
                }
                else
                {
                    viewmodel.AlleyTurnsRange = PropertyDisplayLocation.All;
                }
            }

            #endregion

            #region ApartmentFloorThRange

            if (req.MinApartmentFloorTh.HasValue)
            {
                switch (req.MinApartmentFloorTh)
                {
                    case 1:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3;
                        break;
                    case 4:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7;
                        break;
                    case 8:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12;
                        break;
                    case 12:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.ApartmentFloorTh12;
                        break;
                    default:
                        viewmodel.ApartmentFloorThRange = PropertyDisplayApartmentFloorTh.All;
                        break;
                }
            }

            #endregion

            return viewmodel;
        }

        public CustomerRequirementExchangeCreateViewModel BuildCustomerRequirementExchangeCreate(int pId)
        {
            var currentUserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            var p = Services.ContentManager.Get<PropertyPart>(pId);
            int provinceId = (currentUserGroup != null && currentUserGroup.DefaultProvince != null)
                ? currentUserGroup.DefaultProvince.Id
                : _addressService.GetProvince("TP. Hồ Chí Minh").Id;

            return new CustomerRequirementExchangeCreateViewModel
            {
                AdsTypeCssClass = "ad-exchange",
                AdsTypes = _propertyService.GetAdsTypes().Where(r => r.CssClass == "ad-exchange"),
                TypeGroups = _propertyService.GetTypeGroups(),
                ProvinceId = provinceId,
                Provinces = _addressService.GetProvinces(),
                Districts = _addressService.GetDistricts(provinceId),
                Wards = _addressService.GetWards(0),
                Streets = _addressService.GetStreets(0),
                Directions = _propertyService.GetDirections(),
                Purposes = _customerService.GetPurposesEntries().ToList(),
                Apartments = _addressService.GetApartments(0),
                ExchangeTypes = _propertyService.GetExchangeTypeParts(),
                PaymentMethods = _propertyService.GetPaymentMethods(),
                //PaymentUnits = _propertyService.GetPaymentUnits()
                PropertyDisplayAddress = p != null ? p.DisplayForAddress : "Địa chỉ",
                PropertyId = pId
            };
        }


        public PropertyExchangePartRecord CreatePropertyExchange(PropertyPart p)
        {
            try
            {
                var user = Services.WorkContext.CurrentUser.As<UserPart>();
                if (!_propertyExchangeRepository.Fetch(r => r.Property == p.Record && r.User == user.Record).Any())
                {
                    var exchangeType = _propertyService.GetExchangeType("exchange-parity");
                    var record = new PropertyExchangePartRecord
                    {
                        User = user.Record,
                        Property = p.Record,
                        Customer = null,
                        ExchangeType = exchangeType.Record,
                        ExchangeValues = 0,
                        PaymentMethod = null
                    };

                    _propertyExchangeRepository.Create(record);
                    //_signals.Trigger("ListPropertyExchangeByUser_" + user.Id + "_Changed");
                    _propertyService.ClearCachePropertyExchange();

                    return record;
                }

                Services.Notifier.Information(T("BĐS <a href=\"dang-tin/bat-dong-san-can-trao-doi/{0}\">{1}</a> đã có trong ds BĐS trao đổi của bạn.", p.Id, p.DisplayForAddress));

                return null;
            }
            catch (Exception exi)
            {
                Services.Notifier.Information(T("Error: {1}", exi.Message));
                return null;
            }
        }

        public PropertyExchangePartRecord UpdatePropertyExchange(int id, string exchangeTypeClass, double values, int paymentMethodId)
        {
            var pExchange = _propertyService.GetExchangePartRecord(id);
            var exchangeType = _propertyService.GetExchangeType(exchangeTypeClass);

            if (pExchange != null)
            {
                pExchange.ExchangeType = exchangeType.Record;
                pExchange.ExchangeValues = exchangeType.CssClass != "exchange-parity" ? values : 0;
                pExchange.PaymentMethod = _propertyService.GetPaymentMethod(paymentMethodId);
                //_signals.Trigger("ListPropertyExchangeByUser_" + pExchange.User.Id + "_Changed");
                _propertyService.ClearCachePropertyExchange();
            }
            return pExchange;
        }

        public PropertyExchangePartRecord UpdatePropertyExchange(int id, CustomerPart c, string exchangeTypeClass,
            double values, int paymentMethodId)
        {
            var pExchange = _propertyService.GetExchangePartRecord(id);
            var exchangeType = _propertyService.GetExchangeType(exchangeTypeClass);

            if (pExchange != null)
            {
                pExchange.ExchangeType = exchangeType.Record;
                pExchange.ExchangeValues = exchangeType.CssClass != "exchange-parity" ? values : 0;
                pExchange.PaymentMethod = _propertyService.GetPaymentMethod(paymentMethodId);
                pExchange.Customer = c.Record;
                //_signals.Trigger("ListPropertyExchangeByUser_" + pExchange.User.Id + "_Changed");
                _propertyService.ClearCachePropertyExchange();
            }
            return pExchange;
        }

        public void DeletePropertyExchange(PropertyPart p, bool isAdmin)
        {
            var record = _propertyExchangeRepository.Fetch(r => r.Property == p.Record).FirstOrDefault();
            if (record != null)
            {
                //_signals.Trigger("ListPropertyExchangeByUser_" + user.Id + "_Changed");
                _propertyService.ClearCachePropertyExchange();

                if (record.Customer != null)
                {
                    if (isAdmin) DeleteCustomerAdmin(record.Customer);
                    else DeleteCustomerFrontEnd(record.Customer);
                }

                _propertyExchangeRepository.Delete(record);
            }
        }
        
        public void DeletePropertyExchange(CustomerPart c, bool isAdmin)
        {
            var record = _propertyExchangeRepository.Fetch(r => r.Customer == c.Record).FirstOrDefault();
            if (record != null)
            {
                //_signals.Trigger("ListPropertyExchangeByUser_" + user.Id + "_Changed");
                _propertyService.ClearCachePropertyExchange();

                if (record.Customer != null)
                {
                    if (isAdmin) DeleteCustomerAdmin(record.Customer);
                    else DeleteCustomerFrontEnd(record.Customer);
                }

                _propertyExchangeRepository.Delete(record);
            }
        }

        public void DeleteCustomerAdmin(CustomerPartRecord c)
        {
            var customer = _contentManager.Get<CustomerPart>(c.Id);
            var statusDelete = _customerService.GetStatus("st-trash");
            customer.Status = statusDelete;
        }

        public void DeleteCustomerFrontEnd(CustomerPartRecord c)
        {
            var customer = _contentManager.Get<CustomerPart>(c.Id);
            var statusDelete = _customerService.GetStatus("st-trashed");
            customer.Status = statusDelete;
        }

        public void UpdateStatusPropertyExchange(CustomerPart c, string statusCssClass)
        {
            var record = _propertyExchangeRepository.Fetch(r => r.Customer != null && r.Customer == c.Record).FirstOrDefault();
            if (record != null && record.Property != null)
            {
                var status = _propertyService.GetStatus(statusCssClass);
                var property = _contentManager.Get<PropertyPart>(record.Property.Id);
                property.Status = status;
            }
        }

        public void UpdateStatusPropertyExchange(PropertyPart p, string statusCssClass)
        {
            var record = _propertyExchangeRepository.Fetch(r => r.Property == p.Record).FirstOrDefault();
            if (record != null && record.Customer != null)
            {
                var status = _customerService.GetStatus(statusCssClass);
                var customer = _contentManager.Get<CustomerPart>(record.Customer.Id);
                customer.Status = status;
            }
        }
    }
}
