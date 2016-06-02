using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;
using Orchard.Users.Models;
using Orchard.Roles.Models;
using Contrib.OnlineUsers.Models;
using System.Web.Mvc;

namespace RealEstate.ViewModels
{
    #region Group

    class UserGroupViewModel
    {
    }

    public class UserGroupCreateViewModel
    {

        public string ReturnUrl { get; set; }

        #region General

        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int SeqOrder { get; set; }
        public bool IsEnabled { get; set; }
        public double Point { get; set; }

        public int GroupAdminUserId { get; set; }
        public IEnumerable<UserPart> Users { get; set; }

        public string ContactPhone { get; set; }

        public int? DefaultProvinceId { get; set; }
        public List<SelectListItem> Provinces { get; set; }

        public int? DefaultDistrictId { get; set; }
        public List<SelectListItem> Districts { get; set; }

        public int? DefaultPropertyStatusId { get; set; }
        public IEnumerable<PropertyStatusPartRecord> PropertyStatus { get; set; }

        public int? DefaultAdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? DefaultTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        public int NumberOfAdsGoodDeal { get; set; }
        //public int NumberOfAdsVIP { get; set; }
        public int NumberOfAdsHighlight { get; set; }

        #endregion

        #region IPs

        public string AllowedAdminSingleIPs { get; set; }
        public string AllowedAdminMaskedIPs { get; set; }
        public string DeniedAdminSingleIPs { get; set; }
        public string DeniedAdminMaskedIPs { get; set; }
        public bool ApproveAllGroup { get; set; }

        #endregion

        public IContent UserGroupRecord { get; set; }
    }

    public class UserGroupEditViewModel
    {

        public string ReturnUrl { get; set; }

        #region General

        public int Id
        {
            get { return Group.As<UserGroupPart>().Id; }
        }

        [Required]
        public string Name
        {
            get { return Group.As<UserGroupPart>().Name; }
            set { Group.As<UserGroupPart>().Name = value; }
        }
        public string ShortName
        {
            get { return Group.As<UserGroupPart>().ShortName; }
            set { Group.As<UserGroupPart>().ShortName = value; }
        }
        public int SeqOrder
        {
            get { return Group.As<UserGroupPart>().SeqOrder; }
            set { Group.As<UserGroupPart>().SeqOrder = value; }
        }
        public bool IsEnabled
        {
            get { return Group.As<UserGroupPart>().IsEnabled; }
            set { Group.As<UserGroupPart>().IsEnabled = value; }
        }
        public double Point
        {
            get { return Group.As<UserGroupPart>().Point; }
            set { Group.As<UserGroupPart>().Point = value; }
        }

        public int GroupAdminUserId { get; set; }
        public string GroupAdminUserName 
        {
            get { return Group.As<UserGroupPart>().GroupAdminUser.UserName; }
        }
        public IEnumerable<UserPart> Users { get; set; }

        public IEnumerable<UserPartRecord> UsersAvailable { get; set; }
        public int? ToAddUserId { get; set; }
        public int[] ToAddUserIds { get; set; }

        public string ContactPhone
        {
            get { return Group.As<UserGroupPart>().ContactPhone; }
            set { Group.As<UserGroupPart>().ContactPhone = value; }
        }

        public int? DefaultProvinceId { get; set; }
        public List<SelectListItem> Provinces { get; set; }
        
        public int? DefaultDistrictId { get; set; }
        public List<SelectListItem> Districts { get; set; }

        public int? DefaultPropertyStatusId { get; set; }
        public IEnumerable<PropertyStatusPartRecord> PropertyStatus { get; set; }

        public int? DefaultAdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? DefaultTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        public int NumberOfAdsGoodDeal
        {
            get { return Group.As<UserGroupPart>().NumberOfAdsGoodDeal; }
            set { Group.As<UserGroupPart>().NumberOfAdsGoodDeal = value; }
        }

        //public int NumberOfAdsVIP
        //{
        //    get { return Group.As<UserGroupPart>().NumberOfAdsVIP; }
        //    set { Group.As<UserGroupPart>().NumberOfAdsVIP = value; }
        //}

        public int NumberOfAdsHighlight
        {
            get { return Group.As<UserGroupPart>().NumberOfAdsHighlight; }
            set { Group.As<UserGroupPart>().NumberOfAdsHighlight = value; }
        }

        #endregion

        #region IPs

        public string AllowedAdminSingleIPs
        {
            get { return Group.As<UserGroupPart>().AllowedAdminSingleIPs; }
            set { Group.As<UserGroupPart>().AllowedAdminSingleIPs = value; }
        }
        public string AllowedAdminMaskedIPs
        {
            get { return Group.As<UserGroupPart>().AllowedAdminMaskedIPs; }
            set { Group.As<UserGroupPart>().AllowedAdminMaskedIPs = value; }
        }
        public string DeniedAdminSingleIPs
        {
            get { return Group.As<UserGroupPart>().DeniedAdminSingleIPs; }
            set { Group.As<UserGroupPart>().DeniedAdminSingleIPs = value; }
        }
        public string DeniedAdminMaskedIPs
        {
            get { return Group.As<UserGroupPart>().DeniedAdminMaskedIPs; }
            set { Group.As<UserGroupPart>().DeniedAdminMaskedIPs = value; }
        }

        #endregion

        #region ApproveAllGroup

        public bool ApproveAllGroup
        {
            get { return Group.As<UserGroupPart>().ApproveAllGroup; }
            set { Group.As<UserGroupPart>().ApproveAllGroup = value; }
        }

        #endregion

        #region GroupUsers

        public IEnumerable<UserInGroupEntry> GroupUsers { get; set; }
        public IEnumerable<RoleRecord> Roles { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public dynamic Pager { get; set; }

        #endregion

        #region GroupLocations

        public IEnumerable<UserGroupLocationRecord> GroupLocations { get; set; }

        #endregion

        #region GroupContacts

        public IEnumerable<UserGroupContactRecord> GroupContacts { get; set; }

        #endregion
        
        public IContent Group { get; set; }
    }

    public class UserGroupIndexViewModel
    {
        public IList<UserGroupEntry> UserGroups { get; set; }
        public UserGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class GroupActivitiesIndexViewModel
    {
        public string ReturnUrl { get; set; }
        public UserGroupPart Group { get; set; }

        // Activities
        public bool EnableViewUserPoints { get; set; }
        public IEnumerable<UserInGroupEntry> GroupUsers { get; set; }
        public IEnumerable<RoleRecord> Roles { get; set; }
        
        // Profile
        public bool EnableEditProfile { get; set; }
        #region Profile

        [Required]
        public string Name
        {
            get { return Group.As<UserGroupPart>().Name; }
            set { Group.As<UserGroupPart>().Name = value; }
        }
        public string ShortName
        {
            get { return Group.As<UserGroupPart>().ShortName; }
            set { Group.As<UserGroupPart>().ShortName = value; }
        }
        public int SeqOrder
        {
            get { return Group.As<UserGroupPart>().SeqOrder; }
            set { Group.As<UserGroupPart>().SeqOrder = value; }
        }
        public bool IsEnabled
        {
            get { return Group.As<UserGroupPart>().IsEnabled; }
            set { Group.As<UserGroupPart>().IsEnabled = value; }
        }
        public double Point
        {
            get { return Group.As<UserGroupPart>().Point; }
            set { Group.As<UserGroupPart>().Point = value; }
        }

        public int GroupAdminUserId
        {
            get { return Group.As<UserGroupPart>().GroupAdminUser.Id; }
            set { }
        }
        public string GroupAdminUserName
        {
            get { return Group.As<UserGroupPart>().GroupAdminUser.UserName; }
        }
        public IEnumerable<UserPart> AvailableGroupAdminUsers { get; set; }

        public string ContactPhone
        {
            get { return Group.As<UserGroupPart>().ContactPhone; }
            set { Group.As<UserGroupPart>().ContactPhone = value; }
        }

        public int? DefaultProvinceId 
        {
            get { return Group.As<UserGroupPart>().DefaultProvince != null ? Group.As<UserGroupPart>().DefaultProvince.Id : 0; }
            set { } 
        }

        public int? DefaultDistrictId
        {
            get { return Group.As<UserGroupPart>().DefaultDistrict != null ? Group.As<UserGroupPart>().DefaultDistrict.Id : 0; }
            set { }
        }

        public int? DefaultPropertyStatusId 
        {
            get { return Group.As<UserGroupPart>().DefaultPropertyStatus != null ? Group.As<UserGroupPart>().DefaultPropertyStatus.Id : 0; }
            set { }
        }
        public IEnumerable<PropertyStatusPartRecord> PropertyStatus { get; set; }

        public int? DefaultAdsTypeId 
        {
            get { return Group.As<UserGroupPart>().DefaultAdsType != null ? Group.As<UserGroupPart>().DefaultAdsType.Id : 0; }
            set { }
        }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? DefaultTypeGroupId
        {
            get { return Group.As<UserGroupPart>().DefaultTypeGroup != null ? Group.As<UserGroupPart>().DefaultTypeGroup.Id : 0; }
            set { }
        }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        public int NumberOfAdsGoodDeal
        {
            get { return Group.As<UserGroupPart>().NumberOfAdsGoodDeal; }
            set { Group.As<UserGroupPart>().NumberOfAdsGoodDeal = value; }
        }

        //public int NumberOfAdsVIP
        //{
        //    get { return Group.As<UserGroupPart>().NumberOfAdsVIP; }
        //    set { Group.As<UserGroupPart>().NumberOfAdsVIP = value; }
        //}

        public int NumberOfAdsHighlight
        {
            get { return Group.As<UserGroupPart>().NumberOfAdsHighlight; }
            set { Group.As<UserGroupPart>().NumberOfAdsHighlight = value; }
        }

        public IEnumerable<UserPartRecord> AvailableGroupMemberUsers { get; set; }
        public int? ToAddUserId { get; set; }
        public int[] ToAddUserIds { get; set; }

        #endregion

        // IPs
        public bool EnableEditSettings { get; set; }
        public string CurrentUserIpAddress { get; set; }
        public bool CheckAllowedIPs { get; set; }

        #region IPs

        public string AllowedAdminSingleIPs
        {
            get { return Group.As<UserGroupPart>().AllowedAdminSingleIPs; }
            set { Group.As<UserGroupPart>().AllowedAdminSingleIPs = value; }
        }
        public string AllowedAdminMaskedIPs
        {
            get { return Group.As<UserGroupPart>().AllowedAdminMaskedIPs; }
            set { Group.As<UserGroupPart>().AllowedAdminMaskedIPs = value; }
        }
        public string DeniedAdminSingleIPs
        {
            get { return Group.As<UserGroupPart>().DeniedAdminSingleIPs; }
            set { Group.As<UserGroupPart>().DeniedAdminSingleIPs = value; }
        }
        public string DeniedAdminMaskedIPs
        {
            get { return Group.As<UserGroupPart>().DeniedAdminMaskedIPs; }
            set { Group.As<UserGroupPart>().DeniedAdminMaskedIPs = value; }
        }

        #endregion

        #region ApproveAllGroup

        public bool ApproveAllGroup
        {
            get { return Group.As<UserGroupPart>().ApproveAllGroup; }
            set { Group.As<UserGroupPart>().ApproveAllGroup = value; }
        }

        #endregion

        // Locations
        public bool EnableEditLocations { get; set; }
        public IEnumerable<UserGroupLocationRecord> GroupLocations { get; set; }

        // GroupAddUserAgencies
        public bool EnableGroupAddUserAgencies { get; set; }
        public IEnumerable<UserPart> Users { get; set; }
        public IEnumerable<UserLocationRecord> UserLocations { get; set; }

        // Contacts
        public bool EnableEditContacts { get; set; }
        public IEnumerable<UserGroupContactRecord> GroupContacts { get; set; }

        // SharedLocations
        public bool EnableEditSharedLocations { get; set; }
        public IEnumerable<UserGroupSharedLocationRecord> GroupSharedLocations { get; set; }

        public UserGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public dynamic PagerSecond { get; set; }
        public double TotalExecutionTime { get; set; }
    }

    public class UserGroupIndexOptions
    {
        public string Search { get; set; }
        public UserGroupOrder Order { get; set; }
        public UserGroupFilter Filter { get; set; }
        public UserGroupBulkAction BulkAction { get; set; }

        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public double TotalPoints { get; set; }

        // Activities
        public int? UserActionId { get; set; }
        public IEnumerable<UserActionPart> UserActions { get; set; }

        // Location
        public int? ProvinceId { get; set; }
        public List<SelectListItem> Provinces { get; set; }
        public int? DistrictId { get; set; }
        public List<SelectListItem> Districts { get; set; }
        public int? WardId { get; set; }
        public IEnumerable<LocationWardPart> Wards { get; set; }
        public bool RetrictedAccessGroupProperties { get; set; }
        public bool EnableAccessProperties { get; set; }
        public bool EnableEditLocations { get; set; }
        public bool EnableIsAgencies { get; set; }
        public int? UserLocationEditId { get; set; }
        public string AreaAgencies { get; set; }
        public string NameAgencies { get; set; }
        public bool IsSelling { get; set; }
        public bool IsLeasing { get; set; }
        public DateTime? EndDateAgencing { get; set; }
        public int[] UserPartIds { get; set; }

        // Contact
        public int? AdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }
        public int? TypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }
        public String ContactPhone { get; set; }
        public int? GroupContactEditId { get; set; }

        // Shared Location
        public int? SharedProvinceId { get; set; }
        public int? SharedDistrictId { get; set; }
        public int? SharedWardId { get; set; }
        public int? GroupId { get; set; }
        public IEnumerable<UserGroupPartRecord> Groups { get; set; }

        public int? UserId { get; set; }
        public IEnumerable<UserPart> Users { get; set; }
    }

    public class GroupLocationsIndexViewModel
    {
        public UserGroupPart Group { get; set; }
        public IEnumerable<UserGroupLocationRecord> GroupLocations { get; set; }
        public UserGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class GroupContactsIndexViewModel
    {
        public UserGroupPart Group { get; set; }
        public IEnumerable<UserGroupContactRecord> GroupContacts { get; set; }
        public UserGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class UserGroupEntry
    {
        public UserGroupPartRecord UserGroup { get; set; }
        public HostNamePart HostName { get; set; }
        public bool IsChecked { get; set; }
    }

    #endregion

    #region User

    public class UserActivitiesIndexViewModel
    {
        public UserPart User { get; set; }

        // Activities
        public bool EnableViewUserPoints { get; set; }
        public IEnumerable<UserActivityPartRecord> UserActivities { get; set; }
        
        // Account
        public bool EnableEditProfile { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        //Profile
        public UserUpdateProfilePart UserUpdateProfile { get; set; }

        // Setting
        public int? GroupId { get; set; }
        public IEnumerable<UserGroupPartRecord> Groups { get; set; }

        public int? DefaultAdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? DefaultTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }


        // Roles
        public bool EnableEditRoles { get; set; }
        public IList<Orchard.Roles.ViewModels.UserRoleEntry> Roles { get; set; }

        // Locations
        public bool EnableEditLocations { get; set; }
        public IEnumerable<UserLocationRecord> UserLocations { get; set; }
        public int[] UserPartIds { get; set; }
        public IEnumerable<UserUpdateOptions> UserUpdateOptions { get; set; }

        public UserGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }

        public UserGroupPartRecord OwnGroup { get; set; }
        public UserGroupPartRecord JointGroup { get; set; }
    }

    public class UserInGroupEntry
    {
        public UserInGroupRecord UserInGroupRecord { get; set; }
        public UserPartRecord User { get; set; }
        public UserGroupPartRecord Group { get; set; }
        public double Points { get; set; }
        public List<string> Roles { get; set; }
        public bool IsChecked { get; set; }
    }

    #endregion

    public enum UserGroupOrder
    {
        SeqOrder,
        Name
    }

    public enum UserGroupFilter
    {
        All
    }

    public enum UserGroupBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
