using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Collections;

using Orchard;
using Orchard.Data;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;
using Orchard.Messaging.Services;
using Orchard.Environment.Configuration;
using Orchard.UI.Navigation;
using Orchard.Users.Models;
using Orchard.Caching;
using Orchard.Localization;
using Orchard.UI.Notify;

using RealEstate.Models;
using RealEstate.ViewModels;
using RealEstate.Services;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.FrontEnd.Services;
using RealEstate.UserControlPanel.ViewModels;

namespace RealEstate.UserControlPanel.Services
{
    public interface IControlPanelService : IDependency
    {
        IContentQuery<CustomerPart, CustomerPartRecord> GetOwnCustomerRequirement();
        IContentQuery<PropertyPart, PropertyPartRecord> GetOwnProperties(string ViewStatus);
        IContentQuery<CustomerPart, CustomerPartRecord> GetOwnCustomerRequirement(string ViewStatus);
        int Count(UserPropertyIndexOptions options, string AdsTypeCssClass);
        int CountCustomerRequirement(string Status, string AdsTypeCssClass);
        int CountUserProperties();

    }

    public class ControlPanelService : IControlPanelService
    {
        #region Init

        private readonly IContentManager _contentManager;
        private readonly ICustomerService _customerService;
        private readonly IUserGroupService _groupService;
        private readonly IFastFilterService _fastfilterServices;
        private readonly IPropertyService _propertyService;
        private readonly IPropertySettingService _settingService;

        public ControlPanelService(
            IContentManager contentManager,
            IPropertyService propertyService,
            ICustomerService customerService,
            IFastFilterService fastfilterServices,
            IUserGroupService groupService,
            IPropertySettingService settingService,
            IOrchardServices services)
        {
            _contentManager = contentManager;
            _propertyService = propertyService;
            _fastfilterServices = fastfilterServices;
            _customerService = customerService;
            _groupService = groupService;
            _settingService = settingService;
            Services = services;
            Logger = NullLogger.Instance;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion

        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnProperties()
        {
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            List<string> externalStatusCssClass = new List<string> { "st-pending", "st-estimate", "st-approved", "st-invalid", "st-draft", "st-trashed" };
            var statusIds = _propertyService.GetStatus().Where(a => externalStatusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            //var lstPropertyIds = _propertyService.ListOwnPropertyIdsExchange();

            return Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                //.Where(r => !lstPropertyIds.Contains(r.Id))
                .Where(p => p.UserGroup != null
                        && p.UserGroup == currentDomainGroup
                        && p.CreatedUser.Id == currentUser.Id
                        && statusIds.Contains(p.Status.Id));
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetOwnCustomerRequirement()
        {
            var externalStatusCssClass = new List<string> { "st-pending", "st-approved", "st-invalid", "st-deleted" };
            var statusIds = _customerService.GetStatus().Where(a => externalStatusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();
            var lstCustomerIds = _propertyService.ListOwnCustomerIdsExchange();

            return Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                .Where(r => !lstCustomerIds.Contains(r.Id))
                .Where(p => p.CreatedUser.Id == currentUser.Id && statusIds.Contains(p.Status.Id));
        }

        

        #region Private        

        // Tất cả
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesAll()
        {
            var statusDeleted = _propertyService.GetStatus("st-trashed");
            return GetOwnProperties().Where(p => p.Status != null && p.Status != statusDeleted);
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementAll()
        {
            //var statusDeleted = _customerService.GetStatus("st-deleted");
            return GetOwnCustomerRequirement();//.Where(p => p.Status != statusDeleted);
        }

        // Đang hiển thị
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesDisplay()
        {
            var statusApproved = _propertyService.GetStatus("st-approved");
            return GetOwnProperties().Where(p => p.Status == statusApproved && DateTime.Now < p.AdsExpirationDate);//&& p.Published == true
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementDisplay()
        {
            var statusApproved = _customerService.GetStatus("st-approved");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusApproved && DateTime.Now < p.AdsExpirationDate);//&& p.Published == true
        }

        // Hết hạn hiển thị 
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesNotDisplay()
        {
            var statusApproved = _propertyService.GetStatus("st-approved");
            return GetOwnProperties().Where(p => p.Status == statusApproved && DateTime.Now > p.AdsExpirationDate);//&& p.Published == true
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementNotDisplay()
        {
            var statusApproved = _customerService.GetStatus("st-approved");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusApproved && DateTime.Now > p.AdsExpirationDate);//&& p.Published == true
        }

        // BĐS Định giá
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesEstimate()
        {
            var statusEstimate = _propertyService.GetStatus("st-estimate");
            return GetOwnProperties().Where(p => p.Status == statusEstimate);
        }

        // Chờ duyệt
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesPending()
        {
            var statusPending = _propertyService.GetStatus("st-pending");
            return GetOwnProperties().Where(p => p.Status == statusPending && p.Published == true);
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementPending()
        {
            var statusPending = _customerService.GetStatus("st-pending");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusPending);
        }

        // Chưa hợp lệ
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesInvalid()
        {
            var statusInvalid = _propertyService.GetStatus("st-invalid");
            return GetOwnProperties().Where(p => p.Status == statusInvalid);
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementInvalid()
        {
            var statusInvalid = _customerService.GetStatus("st-invalid");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusInvalid);
        }

        // Ngừng đăng
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesNotPublished()
        {
            var statusApproved = _propertyService.GetStatus("st-approved");
            return GetOwnProperties().Where(p => p.Status == statusApproved && p.Published == false);
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementNotPublished()
        {
            var statusApproved = _customerService.GetStatus("st-approved");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusApproved);//&& p.Published == false
        }

        // Đã xóa
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesDeleted()
        {
            var statusDeleted = _propertyService.GetStatus("st-trashed");
            return GetOwnProperties().Where(p => p.Status == statusDeleted );
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementDeleted()
        {
            var statusDeleted = _customerService.GetStatus("st-deleted");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusDeleted);
        }

        // Đang soạn
        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnPropertiesDraft()
        {
            var statusDraft = _propertyService.GetStatus("st-draft");
            return GetOwnProperties().Where(p => p.Status == statusDraft);
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetCustomerRequirementDraft()
        {
            var statusDraft = _customerService.GetStatus("st-draft");
            return GetOwnCustomerRequirement().Where(p => p.Status == statusDraft);
        }


        #endregion

        public IContentQuery<PropertyPart, PropertyPartRecord> GetOwnProperties(string ViewStatus)
        {

            switch (ViewStatus)
            {
                case "all":
                    return GetOwnPropertiesAll();
                case "view":
                    return GetOwnPropertiesDisplay();
                case "notdisplay":
                    return GetOwnPropertiesNotDisplay();
                case "pending":
                    return GetOwnPropertiesPending();
                case "stop":
                    return GetOwnPropertiesNotPublished();
                case "del":
                    return GetOwnPropertiesDeleted();
                case "estimate":
                    return GetOwnPropertiesEstimate();
                case "invalid":
                    return GetOwnPropertiesInvalid();
                case "draft":
                    return GetOwnPropertiesDraft();
                case "userproperty":
                    return _propertyService.GetListPropertyUserFrontEnd();
                case "exchange":
                    var lstPropertyIds = _propertyService.ListOwnPropertyIdsExchange();
                    return GetOwnProperties().Where(r => lstPropertyIds.Contains(r.Id));
                default:
                    return GetOwnPropertiesAll();
            }
        }

        public IContentQuery<CustomerPart, CustomerPartRecord> GetOwnCustomerRequirement(string ViewStatus)
        {
            switch (ViewStatus)
            {
                case "all":
                    return GetCustomerRequirementAll();
                case "view":
                    return GetCustomerRequirementDisplay();
                case "notdisplay":
                    return GetCustomerRequirementNotDisplay();
                case "pending":
                    return GetCustomerRequirementPending();
                case "del":
                    return GetCustomerRequirementDeleted();
                case "draft":
                    return GetCustomerRequirementDraft();
                case "stop":
                    return GetCustomerRequirementNotPublished();
                case "invalid":
                    return GetCustomerRequirementInvalid();
                default:
                    return GetCustomerRequirementAll();
            }
        }

        public int Count(UserPropertyIndexOptions options, string AdsTypeCssClass)
        {
            int AdsTypeId = _propertyService.GetAdsType(AdsTypeCssClass).Id;
            var pList = GetOwnProperties(options.ReturnStatus).Where(p => p.AdsType.Id == AdsTypeId);
            if (options.TypeGroupId > 0)
            {
                var lisTypeIds = Services.ContentManager.Query<PropertyTypePart, PropertyTypePartRecord>().Where(p => p.Group.Id == options.TypeGroupId).List().Select(a => a.Id).ToList();
                pList = pList.Where(p => lisTypeIds.Contains(p.Type.Id));
            }

            if (options.ProvinceId.HasValue) pList = pList.Where(p => p.Province.Id == options.ProvinceId);

            if (options.DistrictIds != null) pList = pList.Where(p => options.DistrictIds.Contains(p.District.Id));

            if (options.StreetId > 0)
            {
                var listStreetIds = Services.ContentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Where(a => a.RelatedStreet.Id == options.StreetId).List().Select(a => a.Id).ToList();
                listStreetIds.Add((int)options.StreetId);
                pList = pList.Where(p => listStreetIds.Contains(p.Street.Id));
            }
            return pList.Count();
        }

        public int CountCustomerRequirement(string ViewStatus, string AdsTypeCssClass)
        {
            int AdsTypeId = _propertyService.GetAdsType(AdsTypeCssClass).Id;
            return GetOwnCustomerRequirement(ViewStatus).Where(p => p.Requirements.Any(a => a.AdsTypePartRecord.Id == AdsTypeId)).Count();
        }

        public int CountUserProperties()
        {
            return _propertyService.GetListPropertyUserFrontEnd().Count();
        }
    }

}