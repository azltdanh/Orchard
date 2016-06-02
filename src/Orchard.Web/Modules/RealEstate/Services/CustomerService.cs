using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.ViewModels;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace RealEstate.Services
{
    public interface ICustomerService : IDependency
    {
        CustomerStatusPartRecord GetStatus(int? statusId);
        CustomerStatusPartRecord GetStatus(string statusCssClass);
        IEnumerable<CustomerStatusPartRecord> GetStatus();

        IEnumerable<CustomerStatusPartRecord> GetStatusForInternal();
        IEnumerable<CustomerStatusPartRecord> GetStatusForExternal();
        IEnumerable<CustomerStatusPartRecord> GetStatusForTargetCssClass(string targetStatusCssClass);

        IEnumerable<CustomerPurposePartRecord> GetPurposes();
        IEnumerable<CustomerPurposeEntry> GetPurposesEntries();
        IEnumerable<CustomerPurposePartRecord> GetCustomerPurposes(CustomerPart c);
        IEnumerable<CustomerPurposePartRecord> GetCustomerPurposes(CustomerPartRecord c);
        void UpdatePurposesForContentItem(CustomerPart item, IEnumerable<CustomerPurposeEntry> purposes);

        IEnumerable<CustomerFeedbackPartRecord> GetFeedbacks();
        CustomerFeedbackPartRecord GetFeedback(int id);
        CustomerFeedbackPartRecord GetFeedback(string feedbackCssClass);

        // Customer Requirements

        IEnumerable<CustomerRequirementRecord> GetRequirements();
        IEnumerable<CustomerRequirementRecord> GetRequirements(int groupId);
        IEnumerable<CustomerRequirementRecord> GetRequirements(CustomerPart c);
        IEnumerable<CustomerRequirementRecord> GetRequirements(CustomerPartRecord c);

        CustomerRequirementRecord NewCustomerRequirementRecord(CustomerEditRequirementViewModel reqModel,
            CustomerPartRecord customer, PropertyLocationPartRecord location, PaymentMethodPartRecord paymentMethod,
            AdsTypePartRecord adsType, PropertyTypeGroupPartRecord propertyTypeGroup);

        void UpdateCustomerRequirements(List<CustomerRequirementRecord> customerRequirements);
        void CreateCustomerRequirement(CustomerRequirementRecord record);
        int CreateCustomerRequirementId(CustomerRequirementRecord record);
        void DeleteCustomerRequirements(int groupId);
        void EnableCustomerRequirements(int groupId);
        void DisableCustomerRequirements(int groupId);

        // Customer Properties

        IEnumerable<CustomerPropertyRecord> GetCustomerSavedProperties();
        IEnumerable<CustomerPropertyRecord> GetCustomerSavedProperties(int customerId);
        IEnumerable<CustomerPropertyEntry> GetCustomerSavedPropertiesEntries(int customerId);

        CustomerPropertyRecord CreateCustomerProperty(CustomerPart customer, PropertyPart property,
            CustomerFeedbackPart feedback);

        void CreateCustomerPropertyUser(CustomerPropertyRecord cp, UserPartRecord user, DateTime visitedDate,
            bool isWorkOverTime);

        CustomerPropertyRecord UpdateCustomerProperty(CustomerPart customer, PropertyPart property,
            CustomerFeedbackPart feedback, int[] userIds, DateTime? visitedDate, bool isWorkOverTime);

        void DeleteCustomerProperty(int id);

        // Served Users

        IEnumerable<UserPartRecord> GetServedUsers(UserPart user);

        // Customer

        CustomerPart GetCustomer(int? id);
        CustomerPart GetCustomerById(int? customerId);
        CustomerPart GetCustomerByContactPhone(int id, string contactPhone);

        // Search Customers

        CustomerIndexOptions BuildIndexOptions(CustomerIndexOptions options);
        IContentQuery<CustomerPart, CustomerPartRecord> SearchCustomers(CustomerIndexOptions options);

        // Verify

        bool VerifyCustomerUnicity(string contactPhone);
        bool VerifyCustomerUnicity(int id, string contactPhone);
        IEnumerable<CustomerPropertyRecord> GetCustomerPropertiesByContactPhone(string contactPhone);

        // Permissions
        bool EnableEditCustomer(CustomerPartRecord c, UserPart user);
        bool IsExternalCustomer(CustomerPart c);

        // Build
        CustomerEditViewModel BuildEditViewModel(CustomerPart p);
        CustomerEditRequirementViewModel BuildCustomerEditRequirementViewModel(CustomerCreateViewModel viewModel);
        CustomerEditRequirementViewModel BuildCustomerEditRequirementViewModel(CustomerEditViewModel viewModel);

        // Search Properties
        IEnumerable<PropertyPart> SearchProperties(CustomerPart customer);

        #region Group Customers

        IContentQuery<CustomerPart, CustomerPartRecord> GetUserCustomers(UserPart user, bool getAllCustomers);
        IContentQuery<CustomerPart, CustomerPartRecord> GetGroupCustomers(UserGroupPartRecord group);

        #endregion
    }

    public class CustomerService : ICustomerService
    {
        #region INIT

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours
        private readonly IAddressService _addressService;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IRepository<CustomerPurposePartRecordContent> _contentPurposeRepository;
        private readonly IRepository<CustomerPropertyRecord> _customerpropertyRepository;
        private readonly IRepository<CustomerPropertyUserRecord> _customerpropertyUserRepository;
        private readonly IUserGroupService _groupService;
        private readonly IPropertyService _propertyService;

        private readonly IRepository<CustomerRequirementRecord> _requirementRepository;

        private readonly IRevisionService _revisionService;
        private readonly IPropertySettingService _settingService;

        private readonly ISignals _signals;

        public CustomerService(
            IOrchardServices services,
            IAddressService addressService,
            IUserGroupService groupService,
            IPropertySettingService settingService,
            IPropertyService propertyService,
            IRepository<CustomerRequirementRecord> requirementRepository,
            IRepository<CustomerPropertyRecord> customerpropertyRepository,
            IRepository<CustomerPropertyUserRecord> customerpropertyUserRepository,
            IRepository<CustomerPurposePartRecordContent> contentPurposeRepository,
            IRevisionService revisionService,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager)
        {
            _addressService = addressService;
            _groupService = groupService;
            _settingService = settingService;
            _propertyService = propertyService;

            _requirementRepository = requirementRepository;
            _customerpropertyRepository = customerpropertyRepository;
            _customerpropertyUserRepository = customerpropertyUserRepository;
            _contentPurposeRepository = contentPurposeRepository;

            _revisionService = revisionService;

            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;

            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #endregion

        #region Status

        public CustomerStatusPartRecord GetStatus(int? statusId)
        {
            if (!statusId.HasValue) return null;
            return _contentManager.Get<CustomerStatusPart>((int)statusId).Record;
        }

        public CustomerStatusPartRecord GetStatus(string statusCssClass)
        {
            if (string.IsNullOrEmpty(statusCssClass)) return null;
            return
                _contentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>()
                    .Where(a => a.CssClass == statusCssClass)
                    .List()
                    .First()
                    .Record;
        }

        public IEnumerable<CustomerStatusPartRecord> GetStatus()
        {
            return _cacheManager.Get("CustomerStatus", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("CustomerStatus_Changed"));

                return
                    _contentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        public IEnumerable<CustomerStatusPartRecord> GetStatusForInternal()
        {
            List<string> externalStatusCssClass = ExternalStatusCssClass();
            return GetStatus().Where(a => !externalStatusCssClass.Contains(a.CssClass)).OrderBy(a => a.SeqOrder);
        }

        public IEnumerable<CustomerStatusPartRecord> GetStatusForExternal()
        {
            List<string> externalStatusCssClass = ExternalStatusCssClass();
            return GetStatus().Where(a => externalStatusCssClass.Contains(a.CssClass)).OrderBy(a => a.SeqOrder);
        }

        public IEnumerable<CustomerStatusPartRecord> GetStatusForTargetCssClass(string targetStatusCssClass)
        {
            List<string> externalStatusCssClass = ExternalStatusCssClass();
            if (externalStatusCssClass.Contains(targetStatusCssClass))
                return GetStatusForExternal();
            return GetStatusForInternal();
        }

        public List<string> ExternalStatusCssClass()
        {
            return new List<string> { "st-pending", "st-approved", "st-invalid", "st-draft", "st-trashed" };
        }

        #endregion

        #region Purposes

        public IEnumerable<CustomerPurposePartRecord> GetPurposes()
        {
            return _cacheManager.Get("Purposes", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Purposes_Changed"));

                return
                    _contentManager.Query<CustomerPurposePart, CustomerPurposePartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        public IEnumerable<CustomerPurposeEntry> GetPurposesEntries()
        {
            return GetPurposes().Select(x => new CustomerPurposeEntry { Purpose = x }).ToList();
        }

        public IEnumerable<CustomerPurposePartRecord> GetCustomerPurposes(CustomerPart c)
        {
            return GetCustomerPurposes(c.Record);
        }

        public IEnumerable<CustomerPurposePartRecord> GetCustomerPurposes(CustomerPartRecord c)
        {
            var result = new List<CustomerPurposePartRecord>();
            IEnumerable<CustomerPurposePartRecordContent> query =
                _contentPurposeRepository.Fetch(r => r.CustomerPartRecord == c).ToList();
            if (query.Any()) result = query.Select(a => a.CustomerPurposePartRecord).ToList();
            return result;
        }

        public void UpdatePurposesForContentItem(CustomerPart item, IEnumerable<CustomerPurposeEntry> purposes)
        {
            CustomerPartRecord record = item.As<CustomerPart>().Record;
            IEnumerable<CustomerPurposePartRecordContent> oldPurposes =
                _contentPurposeRepository.Fetch(r => r.CustomerPartRecord == record);
            Dictionary<CustomerPurposePartRecord, bool> lookupNew =
                purposes.Where(e => e.IsChecked).Select(e => e.Purpose).ToDictionary(r => r, r => false);
            // Delete the purposes that are no longer there and mark the ones that should stay
            foreach (CustomerPurposePartRecordContent content in oldPurposes)
            {
                if (lookupNew.ContainsKey(content.CustomerPurposePartRecord))
                {
                    lookupNew[content.CustomerPurposePartRecord] = true;
                }
                else
                {
                    _contentPurposeRepository.Delete(content);
                }
            }
            // Add the new purposes
            foreach (CustomerPurposePartRecord purpose in lookupNew.Where(kvp => !kvp.Value).Select(kvp => kvp.Key))
            {
                _contentPurposeRepository.Create(new CustomerPurposePartRecordContent
                {
                    CustomerPartRecord = record,
                    CustomerPurposePartRecord = purpose
                });
            }
        }

        #endregion

        #region Feedbacks

        public CustomerFeedbackPartRecord GetFeedback(int id)
        {
            return _contentManager.Get(id).As<CustomerFeedbackPart>().Record;
        }

        public CustomerFeedbackPartRecord GetFeedback(string feedbackCssClass)
        {
            if (string.IsNullOrEmpty(feedbackCssClass)) return null;
            return
                _contentManager.Query<CustomerFeedbackPart, CustomerFeedbackPartRecord>()
                    .Where(a => a.CssClass == feedbackCssClass)
                    .List()
                    .First()
                    .Record;
        }

        public IEnumerable<CustomerFeedbackPartRecord> GetFeedbacks()
        {
            return _cacheManager.Get("Feedbacks", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Feedbacks_Changed"));

                return
                    _contentManager.Query<CustomerFeedbackPart, CustomerFeedbackPartRecord>()
                        .Where(a => a.IsEnabled)
                        .OrderBy(a => a.SeqOrder)
                        .List()
                        .Select(a => a.Record);
            });
        }

        #endregion

        #region Customer Requirements

        public IEnumerable<CustomerRequirementRecord> GetRequirements()
        {
            return _requirementRepository.Table.ToList();
        }

        public IEnumerable<CustomerRequirementRecord> GetRequirements(int groupId)
        {
            return _requirementRepository.Fetch(r => r.GroupId == groupId).ToList();
        }

        public IEnumerable<CustomerRequirementRecord> GetRequirements(CustomerPart c)
        {
            return GetRequirements(c.Record);
        }

        public IEnumerable<CustomerRequirementRecord> GetRequirements(CustomerPartRecord c)
        {
            var result = new List<CustomerRequirementRecord>();
            IEnumerable<CustomerRequirementRecord> query =
                _requirementRepository.Fetch(r => r.CustomerPartRecord == c).ToList();
            if (query.Any()) result = query.Select(a => a).ToList();
            return result;
        }

        public CustomerRequirementRecord NewCustomerRequirementRecord(CustomerEditRequirementViewModel reqModel,
            CustomerPartRecord customer, PropertyLocationPartRecord location, PaymentMethodPartRecord paymentMethod,
            AdsTypePartRecord adsType, PropertyTypeGroupPartRecord propertyTypeGroup)
        {
            var r = new CustomerRequirementRecord
            {
                CustomerPartRecord = customer,
                IsEnabled = true,
                MinArea = reqModel.MinArea,
                MaxArea = reqModel.MaxArea,
                MinWidth = reqModel.MinWidth,
                MaxWidth = reqModel.MaxWidth,
                MinLength = reqModel.MinLength,
                MaxLength = reqModel.MaxLength,
                PropertyLocationPartRecord = location,
                MinAlleyWidth = reqModel.MinAlleyWidth,
                MaxAlleyWidth = reqModel.MaxAlleyWidth,
                MinAlleyTurns = reqModel.MinAlleyTurns,
                MaxAlleyTurns = reqModel.MaxAlleyTurns,
                MinDistanceToStreet = reqModel.MinDistanceToStreet,
                MaxDistanceToStreet = reqModel.MaxDistanceToStreet,
                MinFloors = reqModel.MinFloors,
                MaxFloors = reqModel.MaxFloors,
                MinBedrooms = reqModel.MinBedrooms,
                MaxBedrooms = reqModel.MaxBedrooms,
                MinBathrooms = reqModel.MinBathrooms,
                MaxBathrooms = reqModel.MaxBathrooms,
                MinPrice = reqModel.MinPrice,
                MaxPrice = reqModel.MaxPrice,
                PaymentMethodPartRecord = paymentMethod,
                AdsTypePartRecord = adsType,
                PropertyTypeGroupPartRecord = propertyTypeGroup,
                OtherProjectName = reqModel.OtherProjectName,
                MinApartmentFloorTh = reqModel.MinApartmentFloorTh,
                MaxApartmentFloorTh = reqModel.MaxApartmentFloorTh
            };

            // Apartment
            return r;
        }

        public void UpdateCustomerRequirements(List<CustomerRequirementRecord> customerRequirements)
        {
            int groupId = 0;
            foreach (CustomerRequirementRecord record in customerRequirements)
            {
                if (groupId != 0) record.GroupId = groupId;
                _requirementRepository.Create(record);
                if (groupId == 0)
                {
                    groupId = record.Id;
                    record.GroupId = groupId;
                }
            }
        }

        public void CreateCustomerRequirement(CustomerRequirementRecord record)
        {
            _requirementRepository.Create(record);
        }
        public int CreateCustomerRequirementId(CustomerRequirementRecord record)
        {
            _requirementRepository.Create(record);
            record.GroupId = record.Id;

            return record.Id;
        }

        public void DeleteCustomerRequirements(int groupId)
        {
            IEnumerable<CustomerRequirementRecord> requirements = _requirementRepository.Fetch(r => r.GroupId == groupId);
            foreach (CustomerRequirementRecord item in requirements)
            {
                _requirementRepository.Delete(item);
            }
        }

        public void EnableCustomerRequirements(int groupId)
        {
            IEnumerable<CustomerRequirementRecord> requirements = _requirementRepository.Fetch(r => r.GroupId == groupId);
            foreach (CustomerRequirementRecord item in requirements)
            {
                item.IsEnabled = true;
            }
        }

        public void DisableCustomerRequirements(int groupId)
        {
            IEnumerable<CustomerRequirementRecord> requirements = _requirementRepository.Fetch(r => r.GroupId == groupId);
            foreach (CustomerRequirementRecord item in requirements)
            {
                item.IsEnabled = false;
            }
        }

        #endregion

        #region Customer Saved Properties

        public IEnumerable<CustomerPropertyRecord> GetCustomerSavedProperties()
        {
            return _customerpropertyRepository.Table.ToList();
        }

        public IEnumerable<CustomerPropertyRecord> GetCustomerSavedProperties(int customerId)
        {
            return _customerpropertyRepository.Fetch(r => r.CustomerPartRecord.Id == customerId).ToList();
        }

        public IEnumerable<CustomerPropertyEntry> GetCustomerSavedPropertiesEntries(int customerId)
        {
            return GetCustomerSavedProperties(customerId)
                .Select(r => new CustomerPropertyEntry
                {
                    Id = r.Id,
                    PropertyPart = _contentManager.Get<PropertyPart>(r.PropertyPartRecord.Id),
                    CustomerPropertyRecord = r,
                    CustomerFeedbackId = r.CustomerFeedbackPartRecord.Id,
                    CustomerFeedbackCssClass = r.CustomerFeedbackPartRecord.CssClass,
                    CustomerStaff =
                        (r.Users != null && r.Users.Count > 0)
                            ? string.Join(", ", r.Users.Select(a => a.UserPartRecord.UserName).ToArray())
                            : "",
                    CustomerVisitedDate =
                        (r.Users != null && r.Users.Count > 0) ? r.Users.Min(a => a.VisitedDate) : DateTime.Now,
                    IsWorkOverTime =
                        (r.Users != null && r.Users.Count > 0) &&
                        r.Users.OrderBy(a => a.VisitedDate).First().IsWorkOverTime,
                    ShowContactPhone =
                        !((r.PropertyPartRecord.Status.CssClass == "st-negotiate" ||
                           r.PropertyPartRecord.Status.CssClass == "st-trading") &&
                          !Services.Authorizer.Authorize(Permissions.AccessNegotiateProperties))
                }).OrderByDescending(r => r.CustomerVisitedDate);
        }

        public CustomerPropertyRecord CreateCustomerProperty(CustomerPart customer, PropertyPart property,
            CustomerFeedbackPart feedback)
        {
            CustomerPropertyRecord r;

            if (
                _customerpropertyRepository.Fetch(
                    a => a.CustomerPartRecord.Id == customer.Id && a.PropertyPartRecord.Id == property.Id).Any())
            {
                r =
                    _customerpropertyRepository.Fetch(
                        a => a.CustomerPartRecord.Id == customer.Id && a.PropertyPartRecord.Id == property.Id).First();
                r.CustomerFeedbackPartRecord = feedback.Record;
            }
            else
            {
                r = new CustomerPropertyRecord
                {
                    PropertyPartRecord = property.Record,
                    CustomerPartRecord = customer.Record,
                    CustomerFeedbackPartRecord = feedback.Record
                };
                _customerpropertyRepository.Create(r);
            }

            UpdateCustomerAndPropertyStatus(customer, property, feedback);

            return r;
        }

        public CustomerPropertyRecord UpdateCustomerProperty(CustomerPart customer, PropertyPart property,
            CustomerFeedbackPart feedback, int[] userIds, DateTime? visitedDate, bool isWorkOverTime)
        {
            CustomerPropertyRecord r;

            if (
                _customerpropertyRepository.Fetch(
                    a => a.CustomerPartRecord.Id == customer.Id && a.PropertyPartRecord.Id == property.Id).Any())
            {
                r =
                    _customerpropertyRepository.Fetch(
                        a => a.CustomerPartRecord.Id == customer.Id && a.PropertyPartRecord.Id == property.Id).First();
                r.CustomerFeedbackPartRecord = feedback.Record;
            }
            else
            {
                r = new CustomerPropertyRecord
                {
                    PropertyPartRecord = property.Record,
                    CustomerPartRecord = customer.Record,
                    CustomerFeedbackPartRecord = feedback.Record
                };
                _customerpropertyRepository.Create(r);
            }

            // Status
            UpdateCustomerAndPropertyStatus(customer, property, feedback);

            // Staff
            if (!_customerpropertyUserRepository.Fetch(a => a.CustomerPropertyRecord == r).Any())
            {
                DateTime visitedDateValue = visitedDate ?? DateTime.Now;
                if (feedback.CssClass != "fb-wait-visit")
                {
                    //DateTime startDate = DateExtension.GetStartOfMonth(visitedDateValue);
                    //DateTime endDate = DateExtension.GetEndOfMonth(visitedDateValue);

                    r.Users = new List<CustomerPropertyUserRecord>();
                    foreach (int userId in userIds)
                    {
                        UserPart user = _groupService.GetUser(userId);
                        var s = new CustomerPropertyUserRecord
                        {
                            CustomerPropertyRecord = r,
                            UserPartRecord = user.Record,
                            VisitedDate = visitedDateValue,
                            IsWorkOverTime = isWorkOverTime
                        };

                        _customerpropertyUserRepository.Create(s);
                        r.Users.Add(s);

                        // Log User Activity
                        // Trong một tháng, dẫn 1 khách bao nhiêu lần cũng tính là 1 activity

                        _revisionService.SaveUserActivityServeCustomer(visitedDateValue, user, customer);
                    }
                }
            }

            return r;
        }

        public void CreateCustomerPropertyUser(CustomerPropertyRecord cp, UserPartRecord user, DateTime visitedDate,
            bool isWorkOverTime)
        {
            var s = new CustomerPropertyUserRecord
            {
                CustomerPropertyRecord = cp,
                UserPartRecord = user,
                VisitedDate = visitedDate,
                IsWorkOverTime = isWorkOverTime
            };

            _customerpropertyUserRepository.Create(s);
        }

        public void DeleteCustomerProperty(int id)
        {
            CustomerPropertyRecord r = _customerpropertyRepository.Get(id);
            foreach (CustomerPropertyUserRecord item in r.Users)
            {
                _customerpropertyUserRepository.Delete(item);
            }
            _customerpropertyRepository.Delete(r);
        }

        /// <summary>
        ///     Update Customer Status and Property Status base on Customer feedback on Property
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="property"></param>
        /// <param name="feedback"></param>
        public void UpdateCustomerAndPropertyStatus(CustomerPart customer, PropertyPart property,
            CustomerFeedbackPart feedback)
        {
            switch (feedback.CssClass)
            {
                case "fb-dealing":
                    if (customer.Status.CssClass != "st-trading") customer.Status = GetStatus("st-negotiate");
                    if (property.Status.CssClass != "st-trading") property.Status = _propertyService.GetStatus("st-negotiate");
                    break;
                case "fb-wait-deposit":
                case "fb-deposited":
                    customer.Status = GetStatus("st-trading");
                    property.Status = _propertyService.GetStatus("st-trading");
                    break;
                case "fb-bought-successful":
                    //if (customer.Status.CssClass != "st-trading" && customer.Status.CssClass != "st-negotiate")
                    customer.Status = GetStatus("st-bought");
                    property.Status = _propertyService.GetStatus("st-sold");
                    break;
            }
        }

        #endregion

        #region Customer

        // Served Users

        public IEnumerable<UserPartRecord> GetServedUsers(UserPart user)
        {
            List<int> listIds =
                _customerpropertyUserRepository.Table.Select(a => a.UserPartRecord.Id).Distinct().ToList();
            //.Select(a => a.Id).ToList();
            return
                _groupService.GetGroupUsers(user).Where(a => listIds.Contains(a.Id)).OrderBy(a => a.UserName).ToList();
        }

        public CustomerPart GetCustomer(int? id)
        {
            if (!id.HasValue) return null;
            return _contentManager.Get((int)id).As<CustomerPart>();
        }

        public CustomerPart GetCustomerById(int? customerId)
        {
            if (!customerId.HasValue) return null;
            return _contentManager.Query<CustomerPart, CustomerPartRecord>()
                .Where(a => a.CustomerId == customerId).List().FirstOrDefault();
        }

        public CustomerPart GetCustomerByContactPhone(int id, string contactPhone)
        {
            int statusId = GetStatus("st-deleted").Id;
            return _contentManager.Query<CustomerPart, CustomerPartRecord>()
                .Where(r => r.Id != id && r.ContactPhone.Contains(contactPhone) && r.Status.Id != statusId)
                .List().FirstOrDefault();
        }

        #endregion

        #region Search Customers

        public CustomerIndexOptions BuildIndexOptions(CustomerIndexOptions options)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var belongGroup = _groupService.GetBelongGroup(user.Id);

            #region DEFAULT OPTIONS

            if (options == null)
            {
                options = new CustomerIndexOptions
                {
                    StatusId = 0,
                    AdsTypeId = 0,
                    TypeGroupId = 0,
                    ProvinceId = 0,
                    PaymentMethodId = 0,
                    PaymentUnitId = 0,
                    DirectionId = 0,
                    LocationId = 0,
                    CreatedUserId = 0,
                    LastUpdatedUserId = 0,
                    GroupId = 0
                };
            }

            // Status
            options.Status = GetStatusWithDefault(null, null);
            if (options.StatusId > 0) options.StatusCssClass = GetStatus(options.StatusId).CssClass;

            // Purposes
            options.Purposes = GetPurposes();

            // AdsTypes
            options.AdsTypes =
                _propertyService.GetAdsTypes().Where(a => a.CssClass != "ad-selling" && a.CssClass != "ad-leasing");
            if (!options.AdsTypeId.HasValue || options.AdsTypeId == 0)
            {
                AdsTypePartRecord defaultAdsType = _groupService.GetUserDefaultAdsType(user);
                if (defaultAdsType.CssClass == "ad-selling")
                {
                    options.AdsTypeId = options.AdsTypes.First(a => a.CssClass == "ad-buying").Id;
                }
                else if (defaultAdsType.CssClass == "ad-leasing")
                {
                    options.AdsTypeId = options.AdsTypes.First(a => a.CssClass == "ad-renting").Id;
                }
            }

            // TypeGroups
            options.TypeGroups = _propertyService.GetTypeGroups();
            if (!options.TypeGroupId.HasValue || options.TypeGroupId == 0)
                options.TypeGroupId = _groupService.GetUserDefaultTypeGroup(user).Id;
            options.TypeGroupCssClass = _propertyService.GetTypeGroup(options.TypeGroupId).CssClass;

            #region Provinces, Districts, Wards, Streets, Apartments

            // Provinces
            options.Provinces = _addressService.GetSelectListProvinces();
            if (options.Provinces[0].Value != "0") options.Provinces.Insert(0, new SelectListItem { Text = "-- Chọn tất cả Tỉnh/TP --", Value = "0" });
            if (!options.ProvinceId.HasValue) options.ProvinceId = _groupService.GetUserDefaultProvinceId(user);

            // Districts
            options.Districts = _addressService.GetDistricts(options.ProvinceId);
            if (options.DistrictIds == null || !options.DistrictIds.Any())
            {
                LocationDistrictPartRecord defaultDistrict = _groupService.GetUserDefaultDistrict(user);
                if (defaultDistrict != null) options.DistrictIds = new[] { defaultDistrict.Id };
            }

            // Wards
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Wards = _addressService.GetWards(options.DistrictIds);
            else
                options.Wards = _addressService.GetWards(0);

            // Streets
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Streets = _addressService.GetStreets(options.DistrictIds);
            else
                options.Streets = _addressService.GetStreetsByProvince(0);

            // Apartments
            if (options.DistrictIds != null && options.DistrictIds.Any())
                options.Apartments = _addressService.GetApartments(options.DistrictIds);
            else
                options.Apartments = _addressService.GetApartments(0);

            #endregion

            // PaymentMethod
            options.PaymentMethods = _propertyService.GetPaymentMethods();
            if (options.PaymentMethodId == 0)
            {
                AdsTypePartRecord adsTypeRenting = _propertyService.GetAdsType("ad-renting");
                if (options.AdsTypeId == adsTypeRenting.Id || options.AdsTypeCssClass == adsTypeRenting.CssClass)
                    options.PaymentMethodId = _propertyService.GetPaymentMethod("pm-vnd-m").Id;
                else
                    options.PaymentMethodId = _propertyService.GetPaymentMethod("pm-vnd-b").Id;
            }
            options.PaymentMethodCssClass = _propertyService.GetPaymentMethod(options.PaymentMethodId).CssClass;

            // PaymentUnit
            options.PaymentUnits = _propertyService.GetPaymentUnits();
            if (options.PaymentUnitId == 0) options.PaymentUnitId = _propertyService.GetPaymentUnit("unit-total").Id;

            // Directions
            options.Directions = _propertyService.GetDirections();

            // Locations
            options.Locations = _propertyService.GetLocations();

            // ServedUsers
            options.ServedUsers = GetServedUsers(user);

            // Groups
            if (belongGroup != null && !options.GroupId.HasValue) options.GroupId = belongGroup.Id;

            // Other
            options.NeedUpdateDate =
                DateTime.Now.AddDays(-double.Parse(_settingService.GetSetting("DaysToUpdatePrice") ?? "90"));

            #endregion

            return options;
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> SearchCustomers(CustomerIndexOptions options)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            options = BuildIndexOptions(options);

            IContentQuery<CustomerPart, CustomerPartRecord> list = GetUserCustomers(user, false);

            #region SECURITY

            // Permissions "st-trading"

            if (!Services.Authorizer.Authorize(Permissions.AccessTradingCustomers))
            {
                // Remove all customers have status 'st-trading'
                CustomerStatusPartRecord statusTrading = GetStatus("st-trading");
                list = list.Where(r => r.Status != statusTrading); // || subordinateUserIds.Contains(r.CreatedUser.Id)
            }

            #endregion

            #region FILTER

            if (options.Id > 0 || !String.IsNullOrEmpty(options.IdStr))
            {
                #region Id, IdStr

                // Id
                if (options.Id > 0) list = list.Where(p => p.Id == options.Id);

                // IdStr
                if (!String.IsNullOrEmpty(options.IdStr)) list = list.Where(p => p.IdStr.Contains(options.IdStr));

                #endregion
            }
            else
            {
                #region Status

                int statusDeleted = GetStatus("st-deleted").Id;
                var statusCssClass = new List<string> { "st-new", "st-high", "st-negotiate", "st-approved" };
                // -- Đang rao -- (KH MỚI, CẦN MUA GẤP, THƯƠNG LƯỢNG, Đã Duyệt)
                List<int> statusIds =
                    GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
                // Status
                if (options.StatusId == null)
                    list = list.Where(p => statusIds.Contains(p.Status.Id)); // -- Đang rao --
                else if (options.StatusId == 0) list = list.Where(p => p.Status.Id != statusDeleted); // -- Tất cả --
                else if (options.StatusId > 0 && options.StatusId == GetStatus("st-pending").Id)
                    list = list.Where(p => p.Status.Id == options.StatusId && p.Requirements.Any()); //
                else if (options.StatusId > 0)
                    list = list.Where(p => p.Status.Id == options.StatusId); // selected Status

                #endregion

                //Purpose
                //if (options.PurposeIds != null) { _list = _list.Where(p => p.Purposes.Any(a => options.PurposeIds.Contains(a.CustomerPurposePartRecord.Id))); }

                #region AdsType, TypeGroup, Type

                // AdsType
                if (options.AdsTypeId != null)
                    list = list.Where(p => p.Requirements.Any(a => a.AdsTypePartRecord.Id == options.AdsTypeId));
                // PropertyTypeGroup
                if (options.TypeGroupId != null)
                    list =
                        list.Where(
                            p => p.Requirements.Any(a => a.PropertyTypeGroupPartRecord.Id == options.TypeGroupId));

                #endregion

                #region Address

                // Provinces
                if (options.ProvinceId > 0)
                    list =
                        list.Where(p => p.Requirements.Any(a => a.LocationProvincePartRecord.Id == options.ProvinceId));
                // Districts
                if (options.DistrictIds != null)
                    list =
                        list.Where(
                            p => p.Requirements.Any(a => options.DistrictIds.Contains(a.LocationDistrictPartRecord.Id)));
                // Wards
                if (options.WardIds != null)
                    list =
                        list.Where(p => p.Requirements.Any(a => options.WardIds.Contains(a.LocationWardPartRecord.Id)));
                // Street
                if (options.StreetIds != null)
                    list =
                        list.Where(
                            p => p.Requirements.Any(a => options.StreetIds.Contains(a.LocationStreetPartRecord.Id)));

                // Apartments
                if (options.TypeGroupCssClass == "gp-apartment")
                {
                    if (options.ApartmentIds != null)
                        list =
                            list.Where(
                                p =>
                                    p.Requirements.Any(
                                        a => options.ApartmentIds.Contains(a.LocationApartmentPartRecord.Id)));
                }

                #endregion

                #region Direction, LegalStatus, Location

                // Direction
                if (options.DirectionIds != null)
                    list =
                        list.Where(
                            p => p.Requirements.Any(a => options.DirectionIds.Contains(a.DirectionPartRecord.Id)));


                // Location
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.LocationId > 0)
                        list =
                            list.Where(
                                p => p.Requirements.Any(a => a.PropertyLocationPartRecord.Id == options.LocationId));
                }

                #endregion

                #region Alley

                // Alley
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinAlleyWidth.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MinAlleyWidth >= options.MinAlleyWidth));
                    if (options.MaxAlleyWidth.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MaxAlleyWidth <= options.MaxAlleyWidth));

                    if (options.MinAlleyTurns.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MinAlleyTurns >= options.MinAlleyTurns));
                    if (options.MaxAlleyTurns.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MaxAlleyTurns <= options.MaxAlleyTurns));

                    if (options.MinDistanceToStreet.HasValue)
                        list =
                            list.Where(
                                p => p.Requirements.Any(a => a.MinDistanceToStreet >= options.MinDistanceToStreet));
                    if (options.MaxDistanceToStreet.HasValue)
                        list =
                            list.Where(
                                p => p.Requirements.Any(a => a.MaxDistanceToStreet <= options.MaxDistanceToStreet));
                }

                #endregion

                #region Area

                if (options.MinAreaTotal.HasValue)
                    list = list.Where(p => p.Requirements.Any(a => a.MinArea >= options.MinAreaTotal));
                if (options.MaxAreaTotal.HasValue)
                    list = list.Where(p => p.Requirements.Any(a => a.MaxArea <= options.MaxAreaTotal));

                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinAreaTotalWidth.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MinWidth >= options.MinAreaTotalWidth));
                    if (options.MaxAreaTotalWidth.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MaxWidth <= options.MaxAreaTotalWidth));

                    if (options.MinAreaTotalLength.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MinLength >= options.MinAreaTotalLength));
                    if (options.MaxAreaTotalLength.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MaxLength <= options.MaxAreaTotalLength));
                }

                #endregion

                #region Construction

                // Construction

                // Floors
                if (options.TypeGroupCssClass != "gp-apartment")
                {
                    if (options.MinFloors.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MinFloors >= options.MinFloors));
                    if (options.MaxFloors.HasValue)
                        list = list.Where(p => p.Requirements.Any(a => a.MaxFloors <= options.MaxFloors));
                }

                // Bedrooms
                if (options.MinBedrooms.HasValue)
                    list = list.Where(p => p.Requirements.Any(a => a.MinBedrooms >= options.MinBedrooms));
                if (options.MaxBedrooms.HasValue)
                    list = list.Where(p => p.Requirements.Any(a => a.MaxBedrooms <= options.MaxBedrooms));

                // Bathrooms
                if (options.MinBathrooms.HasValue)
                    list = list.Where(p => p.Requirements.Any(a => a.MinBathrooms >= options.MinBathrooms));
                if (options.MaxBathrooms.HasValue)
                    list = list.Where(p => p.Requirements.Any(a => a.MaxBathrooms <= options.MaxBathrooms));

                #endregion

                #region Apartemnt

                // OtherProjectName
                if (!String.IsNullOrWhiteSpace(options.OtherProjectName))
                    list = list.Where(p => p.ContactName.Contains(options.OtherProjectName));

                // ApartmentFloorTh
                if (options.MinApartmentFloorTh.HasValue)
                    list =
                        list.Where(p => p.Requirements.Any(a => a.MinApartmentFloorTh >= options.MinApartmentFloorTh));
                if (options.MaxApartmentFloorTh.HasValue)
                    list =
                        list.Where(p => p.Requirements.Any(a => a.MaxApartmentFloorTh <= options.MaxApartmentFloorTh));

                #endregion

                #region Price

                // Price

                // Convert Price to VND
                //double MinPriceVND = options.MinPriceProposed.HasValue ? options.AdsTypeId == 98465 ? _propertyService.ConvertToVNDB((double)options.MinPriceProposed.Value, options.PaymentMethodCssClass) : _propertyService.ConvertToVNDM((double)options.MinPriceProposed.Value, options.PaymentMethodCssClass) : 0;
                //double MaxPriceVND = options.MaxPriceProposed.HasValue ? options.AdsTypeId == 98465 ? _propertyService.ConvertToVNDB((double)options.MaxPriceProposed.Value, options.PaymentMethodCssClass) : _propertyService.ConvertToVNDM((double)options.MaxPriceProposed.Value, options.PaymentMethodCssClass) : 0;

                double minPriceVnd = options.MinPriceProposed.HasValue
                    ? _propertyService.Convert((double)options.MinPriceProposed, options.PaymentMethodCssClass,
                        options.PaymentMethodCssClass)
                    : 0;
                double maxPriceVnd = options.MaxPriceProposed.HasValue
                    ? _propertyService.Convert((double)options.MaxPriceProposed, options.PaymentMethodCssClass,
                        options.PaymentMethodCssClass)
                    : 0;

                if (minPriceVnd > 0) list = list.Where(p => p.Requirements.Any(a => a.MinPrice >= minPriceVnd));
                if (maxPriceVnd > 0) list = list.Where(p => p.Requirements.Any(a => a.MaxPrice <= maxPriceVnd));

                #endregion

                #region ContactPhone

                if (!String.IsNullOrWhiteSpace(options.ContactPhone))
                    list =
                        list.Where(
                            p =>
                                p.ContactPhone.Contains(options.ContactPhone) ||
                                p.ContactName.Contains(options.ContactPhone));

                #endregion

                #region Users

                // Users

                // Get date value

                const string format = "dd/MM/yyyy";
                CultureInfo provider = CultureInfo.InvariantCulture;
                const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

                DateTime createdFrom, createdTo, lastUpdatedFrom, lastUpdatedTo, visitedFrom, visitedTo;

                DateTime.TryParseExact(options.CreatedFrom, format, provider, style, out createdFrom);
                DateTime.TryParseExact(options.CreatedTo, format, provider, style, out createdTo);

                DateTime.TryParseExact(options.LastUpdatedFrom, format, provider, style, out lastUpdatedFrom);
                DateTime.TryParseExact(options.LastUpdatedTo, format, provider, style, out lastUpdatedTo);

                DateTime.TryParseExact(options.VisitedFrom, format, provider, style, out visitedFrom);
                DateTime.TryParseExact(options.VisitedTo, format, provider, style, out visitedTo);

                if (options.CreatedUserId > 0) list = list.Where(p => p.CreatedUser.Id == options.CreatedUserId);
                if (createdFrom.Year > 1) list = list.Where(p => p.CreatedDate >= createdFrom);
                if (createdTo.Year > 1) list = list.Where(p => p.CreatedDate <= createdTo.AddHours(24));

                if (options.LastUpdatedUserId > 0)
                    list = list.Where(p => p.LastUpdatedUser.Id == options.LastUpdatedUserId);
                if (lastUpdatedFrom.Year > 1) list = list.Where(p => p.LastUpdatedDate >= lastUpdatedFrom);
                if (lastUpdatedTo.Year > 1) list = list.Where(p => p.LastUpdatedDate <= lastUpdatedTo.AddHours(24));

                if (options.ServedUserIds != null)
                {
                    list =
                        list.Where(
                            p =>
                                p.Properties.Any(
                                    a => a.Users.Any(b => options.ServedUserIds.Contains(b.UserPartRecord.Id))));
                }
                if (visitedFrom.Year > 1)
                    list = list.Where(p => p.Properties.Any(a => a.Users.Any(b => b.VisitedDate >= visitedFrom)));
                if (visitedTo.Year > 1)
                    list =
                        list.Where(
                            p => p.Properties.Any(a => a.Users.Any(b => b.VisitedDate <= visitedTo.AddHours(24))));

                #endregion
            }

            #endregion

            #region Group

            // GroupId
            if (options.GroupId > 0) list = list.Where(a => a.UserGroup.Id == options.GroupId);

            // GroupIds
            if (options.GroupIds != null)
                if (options.GroupIds.Any()) list = list.Where(a => options.GroupIds.Contains(a.UserGroup.Id));

            #endregion

            #region ORDER

            switch (options.Order)
            {
                case CustomerOrder.LastUpdatedDate:
                    list = list.OrderByDescending(u => u.LastUpdatedDate);
                    break;
                case CustomerOrder.CreatedDate:
                    list = list.OrderBy(u => u.CreatedDate);
                    break;
                case CustomerOrder.ContactName:
                    list = list.OrderBy(u => u.ContactName);
                    break;
            }

            #endregion

            return list;
        }

        public IEnumerable<CustomerStatusPartRecord> GetStatusWithDefault(int? value, string name)
        {
            IEnumerable<CustomerStatusPartRecord> statuses = GetStatus();

            List<CustomerStatusPartRecord> listStatus = statuses.ToList();

            listStatus.Insert(0,
                new CustomerStatusPartRecord
                {
                    Id = value ?? 0,
                    Name = String.IsNullOrEmpty(name) ? "-- Tất cả --" : name
                });

            return listStatus;
        }

        #endregion

        #region Group Customers

        public IContentQuery<CustomerPart, CustomerPartRecord> GetUserCustomers(UserPart user, bool getAllCustomers)
        {
            IContentQuery<CustomerPart, CustomerPartRecord> list =
                _contentManager.Query<CustomerPart, CustomerPartRecord>();

            // Show all customers
            if (Services.Authorizer.Authorize(Permissions.ManageCustomers))
            {
                return list;
            }

            UserGroupPartRecord jointGroup = _groupService.GetJointGroup(user.Id);
            UserGroupPartRecord ownGroup = _groupService.GetOwnGroup(user.Id);

            if (
                jointGroup != null &&
                (getAllCustomers ||
                 (Services.Authorizer.Authorize(Permissions.MetaListCustomers) && _settingService.CheckAllowedIPsCustomer()))
                )
            {
                list = GetGroupCustomers(jointGroup);
            }
            else if (ownGroup != null)
            {
                list = GetGroupCustomers(ownGroup);
            }
            else
            {
                list = list.Where(p => p.CreatedUser.Id == user.Id);
            }

            return list;
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetGroupCustomers(UserGroupPartRecord group)
        {
            //var relevantUserIds = _groupService.GetRelevantUserIds(group);
            //relevantUserIds.Add(group.GroupAdminUser.Id);

            //var groupLocationProvinceIds = _groupService.GetGroupLocationProvinceIds(group);
            //var groupLocationDistrictIds = _groupService.GetGroupLocationDistrictIds(group);
            //var groupLocationWardIds = _groupService.GetGroupLocationWardIds(group);

            IContentQuery<CustomerPart, CustomerPartRecord> list =
                _contentManager.Query<CustomerPart, CustomerPartRecord>().Where(a => a.UserGroup == group);
            //relevantUserIds.Contains(a.CreatedUser.Id) // Get all properties from relevant group

            return list;
        }

        #endregion

        #region VerifyContactPhone

        public bool VerifyCustomerUnicity(string contactPhone)
        {
            if (GetInternalCustomersByContactPhone(contactPhone).List().Any())
            {
                return false;
            }

            return true;
        }

        public bool VerifyCustomerUnicity(int id, string contactPhone)
        {
            if (GetInternalCustomersByContactPhone(contactPhone).List().Any(r => r.Id != id))
            {
                return false;
            }

            return true;
        }

        public IEnumerable<CustomerPropertyRecord> GetCustomerPropertiesByContactPhone(string contactPhone)
        {
            IEnumerable<CustomerPropertyRecord> list =
                _customerpropertyRepository.Fetch(a => a.CustomerPartRecord.ContactPhone.Contains(contactPhone));
            return list;
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetInternalCustomersByContactPhone(string contactPhone)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            // Loại các customer-status của Khách Hàng
            List<int> statusIds = GetStatusForExternal().Select(a => a.Id).ToList();
            int deletedStatusId = GetStatus("st-deleted").Id;
            statusIds.Add(deletedStatusId);

            return
                GetUserCustomers(user, true)
                    .Where(r => r.ContactPhone.Contains(contactPhone) && !statusIds.Contains(r.Status.Id));
        }

        #endregion

        #region Permissions

        public bool EnableEditCustomer(CustomerPartRecord c, UserPart user)
        {
            // KH của user
            if (c.CreatedUser.Id == user.Id)
                return Services.Authorizer.Authorize(Permissions.EditOwnCustomer, T("Not authorized to edit customers"));

            // Check if current user is Supervisor of this Property
            if (_groupService.IsSupervisor(c.UserGroup, user)) return true; // user is Supervisor

            if (_settingService.CheckAllowedIPsCustomer()) // user client nằm trong IP cho phép
            {
                if (c.Status.CssClass == "st-trading") // KH đang giao dịch
                    return Services.Authorizer.Authorize(Permissions.AccessTradingCustomers,
                        T("Not authorized to edit customers"));

                if (c.CreatedUser.Id == user.Id) // KH của user
                    return Services.Authorizer.Authorize(Permissions.EditOwnCustomer,
                        T("Not authorized to edit customers"));
                return Services.Authorizer.Authorize(Permissions.EditCustomer, T("Not authorized to edit customers"));
            }

            return false;
        }

        #endregion

        #region IsValid

        public bool IsExternalCustomer(CustomerPart c)
        {
            if (c == null) return false;
            if (c.Status == null) return false;
            List<string> externalStatusCssClass = ExternalStatusCssClass();
            return externalStatusCssClass.Contains(c.Status.CssClass);
        }

        #endregion

        #region BuildViewModel

        public CustomerEditViewModel BuildEditViewModel(CustomerPart c)
        {
            bool isExternalCustomer = IsExternalCustomer(c);

            IEnumerable<int> purposeIds = GetCustomerPurposes(c).Select(a => a.Id);
            //var _purposeIds = c.Purposes.Select(r => r.Id).ToList();

            // Permission
            bool enableDeleteCustomerProperty = Services.Authorizer.Authorize(Permissions.DeleteCustomerProperty);
            bool enableEditContactPhone = true;
            if (c.Status != null)
            {
                if (c.Status.CssClass == "st-negotiate")
                    enableEditContactPhone = Services.Authorizer.Authorize(Permissions.AccessNegotiateCustomers);
                if (c.Status.CssClass == "st-trading")
                    enableEditContactPhone = Services.Authorizer.Authorize(Permissions.AccessTradingCustomers);
            }

            return new CustomerEditViewModel
            {
                Customer = c,
                StatusId = c.Status != null ? c.Status.Id : 0,
                Status = isExternalCustomer ? GetStatusForExternal() : GetStatusForInternal(),
                IsExternalCustomer = isExternalCustomer,
                Purposes =
                    GetPurposes()
                        .Select(r => new CustomerPurposeEntry { Purpose = r, IsChecked = purposeIds.Contains(r.Id) })
                        .ToList(),
                Requirements =
                    GetRequirements()
                        .Where(r => r.CustomerPartRecord == c.Record)
                        .Select(r => new CustomerRequirementEntry { Requirement = r })
                        .ToList(),
                FeedbackId = 0,
                Feedbacks = GetFeedbacks(),
                Properties = GetCustomerSavedPropertiesEntries(c.Id).ToList(),

                // Permission
                EnableDeleteCustomerProperty = enableDeleteCustomerProperty,
                EnableEditContactPhone = enableEditContactPhone,
            };
        }

        public CustomerEditRequirementViewModel BuildCustomerEditRequirementViewModel(CustomerCreateViewModel viewModel)
        {
            return new CustomerEditRequirementViewModel
            {
                GroupId = viewModel.GroupId,
                IsEnabled = viewModel.IsEnabled,
                AdsTypeId = viewModel.AdsTypeId,
                PropertyTypeGroupId = viewModel.PropertyTypeGroupId,
                ProvinceId = viewModel.ProvinceId,
                DistrictIds = viewModel.DistrictIds,
                WardIds = viewModel.WardIds,
                StreetIds = viewModel.StreetIds,
                ApartmentIds = viewModel.ApartmentIds,
                MinArea = viewModel.MinArea,
                MaxArea = viewModel.MaxArea,
                MinWidth = viewModel.MinWidth,
                MaxWidth = viewModel.MaxWidth,
                MinLength = viewModel.MinLength,
                MaxLength = viewModel.MaxLength,
                DirectionIds = viewModel.DirectionIds,
                LocationId = viewModel.LocationId,
                MinAlleyWidth = viewModel.MinAlleyWidth,
                MaxAlleyWidth = viewModel.MaxAlleyWidth,
                MinAlleyTurns = viewModel.MinAlleyTurns,
                MaxAlleyTurns = viewModel.MaxAlleyTurns,
                MinDistanceToStreet = viewModel.MinDistanceToStreet,
                MaxDistanceToStreet = viewModel.MaxDistanceToStreet,
                MinFloors = viewModel.MinFloors,
                MaxFloors = viewModel.MaxFloors,
                MinBedrooms = viewModel.MinBedrooms,
                MaxBedrooms = viewModel.MaxBedrooms,
                MinBathrooms = viewModel.MinBathrooms,
                MaxBathrooms = viewModel.MaxBathrooms,
                MinPrice = viewModel.MinPrice,
                MaxPrice = viewModel.MaxPrice,
                PaymentMethodId = viewModel.PaymentMethodId,
                OtherProjectName = viewModel.OtherProjectName,
                MinApartmentFloorTh = viewModel.MinApartmentFloorTh,
                MaxApartmentFloorTh = viewModel.MaxApartmentFloorTh,
            };
        }

        public CustomerEditRequirementViewModel BuildCustomerEditRequirementViewModel(CustomerEditViewModel viewModel)
        {
            return new CustomerEditRequirementViewModel
            {
                GroupId = viewModel.GroupId,
                IsEnabled = viewModel.IsEnabled,
                AdsTypeId = viewModel.AdsTypeId,
                PropertyTypeGroupId = viewModel.PropertyTypeGroupId,
                ProvinceId = viewModel.ProvinceId,
                DistrictIds = viewModel.DistrictIds,
                WardIds = viewModel.WardIds,
                StreetIds = viewModel.StreetIds,
                ApartmentIds = viewModel.ApartmentIds,
                MinArea = viewModel.MinArea,
                MaxArea = viewModel.MaxArea,
                MinWidth = viewModel.MinWidth,
                MaxWidth = viewModel.MaxWidth,
                MinLength = viewModel.MinLength,
                MaxLength = viewModel.MaxLength,
                DirectionIds = viewModel.DirectionIds,
                LocationId = viewModel.LocationId,
                MinAlleyWidth = viewModel.MinAlleyWidth,
                MaxAlleyWidth = viewModel.MaxAlleyWidth,
                MinAlleyTurns = viewModel.MinAlleyTurns,
                MaxAlleyTurns = viewModel.MaxAlleyTurns,
                MinDistanceToStreet = viewModel.MinDistanceToStreet,
                MaxDistanceToStreet = viewModel.MaxDistanceToStreet,
                MinFloors = viewModel.MinFloors,
                MaxFloors = viewModel.MaxFloors,
                MinBedrooms = viewModel.MinBedrooms,
                MaxBedrooms = viewModel.MaxBedrooms,
                MinBathrooms = viewModel.MinBathrooms,
                MaxBathrooms = viewModel.MaxBathrooms,
                MinPrice = viewModel.MinPrice,
                MaxPrice = viewModel.MaxPrice,
                PaymentMethodId = viewModel.PaymentMethodId,
                OtherProjectName = viewModel.OtherProjectName,
                MinApartmentFloorTh = viewModel.MinApartmentFloorTh,
                MaxApartmentFloorTh = viewModel.MaxApartmentFloorTh,
            };
        }

        #endregion

        // Search Customers

        // Build

        public IEnumerable<PropertyPart> SearchProperties(CustomerPart customer)
        {
            // Get all Properties from customer's saved properties
            var savedPropertyIds = new List<int>();
            IEnumerable<CustomerPropertyRecord> savedProperties = GetCustomerSavedProperties(customer.Id).ToList();
            if (savedProperties.Any()) savedPropertyIds = savedProperties.Select(r => r.PropertyPartRecord.Id).ToList();

            // Get all Properties from customer's requirements
            var listProperties = new List<PropertyPart>();

            IEnumerable<CustomerRequirementRecord> customerRequirements = GetRequirements(customer).ToList();

            List<int?> groupIds = customerRequirements.Where(r => r.IsEnabled).Select(r => r.GroupId).ToList();

            foreach (var groupId in groupIds)
            {
                List<CustomerRequirementRecord> reqs = customerRequirements.Where(r => r.GroupId == groupId).ToList();
                CustomerRequirementRecord req = reqs.First();

                var options = new PropertyIndexOptions
                {
                    ProvinceId = req.LocationProvincePartRecord.Id,
                    DistrictIds = reqs.Where(r => r.LocationDistrictPartRecord != null)
                        .Select(r => r.LocationDistrictPartRecord.Id)
                        .ToArray(),
                    WardIds =
                        reqs.Where(r => r.LocationWardPartRecord != null)
                            .Select(r => r.LocationWardPartRecord.Id)
                            .ToArray(),
                    StreetIds = reqs.Where(r => r.LocationStreetPartRecord != null)
                        .Select(r => r.LocationStreetPartRecord.Id)
                        .ToArray(),
                    DirectionIds =
                        reqs.Where(r => r.DirectionPartRecord != null).Select(r => r.DirectionPartRecord.Id).ToArray(),
                    ApartmentIds = reqs.Where(r => r.LocationApartmentPartRecord != null)
                        .Select(r => r.LocationApartmentPartRecord.Id)
                        .ToArray()
                };

                if (req.AdsTypePartRecord != null)
                {
                    if (req.AdsTypePartRecord.CssClass == "ad-buying")
                    {
                        options.AdsTypeId = _propertyService.GetAdsType("ad-selling").Id;
                    }
                    else if (req.AdsTypePartRecord.CssClass == "ad-renting")
                    {
                        options.AdsTypeId = _propertyService.GetAdsType("ad-leasing").Id;
                    }
                    else
                    {
                        options.AdsTypeId = req.AdsTypePartRecord.Id;
                    }
                    options.AdsTypeCssClass = req.AdsTypePartRecord.CssClass;
                }
                if (req.PropertyTypeGroupPartRecord != null)
                {
                    options.TypeGroupId = req.PropertyTypeGroupPartRecord.Id;
                    options.TypeGroupCssClass = req.PropertyTypeGroupPartRecord.CssClass;
                }
                if (req.PropertyLocationPartRecord != null) options.LocationId = req.PropertyLocationPartRecord.Id;

                options.MinAlleyTurns = req.MinAlleyTurns;
                options.MaxAlleyTurns = req.MaxAlleyTurns;
                options.MinAlleyWidth = req.MinAlleyWidth;
                options.MaxAlleyWidth = req.MaxAlleyWidth;
                options.MinDistanceToStreet = req.MinDistanceToStreet;
                options.MaxDistanceToStreet = req.MaxDistanceToStreet;

                options.MinAreaTotal = req.MinArea;
                options.MaxAreaTotal = req.MaxArea;
                options.MinAreaTotalWidth = req.MinWidth;
                options.MaxAreaTotalWidth = req.MaxWidth;
                options.MinAreaTotalLength = req.MinLength;
                options.MaxAreaTotalLength = req.MaxLength;

                options.MinFloors = req.MinFloors;
                options.MaxFloors = req.MaxFloors;
                options.MinBedrooms = req.MinBedrooms;
                options.MaxBedrooms = req.MaxBedrooms;
                options.MinBathrooms = req.MinBathrooms;
                options.MaxBathrooms = req.MaxBathrooms;

                options.MinPriceProposed = req.MinPrice.HasValue ? req.MinPrice : 0;
                options.MaxPriceProposed = req.MaxPrice.HasValue ? req.MaxPrice : 0;
                options.PaymentMethodCssClass = req.PaymentMethodPartRecord.CssClass;
                options.PaymentMethodId = req.PaymentMethodPartRecord.Id;

                IContentQuery<PropertyPart, PropertyPartRecord> temp =
                    _propertyService.SearchProperties(options).Where(a => !savedPropertyIds.Contains(a.Id));

                if (temp.Count() > 0)
                    listProperties.AddRange(temp.List());
            }

            listProperties =
                listProperties.Distinct()
                    .OrderByDescending(a => a.Flag.Value)
                    .ThenBy(a => a.PriceProposedInVND)
                    .ToList();

            return listProperties;
        }
    }
}