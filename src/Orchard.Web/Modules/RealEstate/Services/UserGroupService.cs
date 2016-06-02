using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Contrib.OnlineUsers.Models;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace RealEstate.Services
{
    public interface IUserGroupService : IDependency
    {
        #region Verify

        bool VerifyUserGroupUnicity(string groupName);
        bool VerifyUserGroupUnicity(int id, string groupName);
        bool IsSupervisor(UserGroupPartRecord group, UserPart supervisorUser);
        bool IsSupervisor(UserGroupPartRecord group, UserGroupPartRecord supervisorGroup);

        #endregion

        #region Users

        UserPart GetUser(int? userId);
        UserPart GetUser(string userName);
        UserPart GetUserDefault();
        IEnumerable<UserPart> GetUsers();
        IEnumerable<UserPart> GetUsers(List<int> listIds);
        IEnumerable<UserPart> SearchUsers(string term);
        IEnumerable<UserPart> SearchAvailableGroupMemberUsers(UserGroupPartRecord group, string term);
        IEnumerable<UserPart> SearchAvailableGroupAdminUsers(UserGroupPartRecord group, string term);

        IContentQuery<UserPart, UserPartRecord> GetAvailableGroupMemberUsers(UserGroupPartRecord group);
        IContentQuery<UserPart, UserPartRecord> GetAvailableGroupAdminUsers(UserGroupPartRecord group);

        #endregion

        #region Get Users

        List<int> GetUserInGroupIds();
        List<int> GetUserInGroupIds(int groupId);

        List<int> GetRelevantUserIds(UserPart user);
        List<int> GetRelevantUserIds(UserGroupPartRecord group);

        List<int> GetRelevantGroupIds(UserPart user);
        List<int> GetRelevantGroupIds(UserGroupPartRecord group);

        // Lấy các user cấp dưới
        //List<int> GetSubordinateUserIds(UserPart user);

        #endregion

        #region Groups

        UserGroupPartRecord GetCurrentDomainGroup();
        UserGroupPartRecord GetDefaultDomainGroup();

        UserGroupPartRecord GetGroup(int? groupId);
        IEnumerable<UserGroupPart> SearchGroups(string term);
        IEnumerable<UserGroupPartRecord> GetGroups();

        UserGroupPartRecord GetBelongGroup(int userId);
        UserGroupPartRecord GetJointGroup(int userId);
        UserGroupPartRecord GetOwnGroup(int userId);

        string GetGroupShortName(int userId);
        IEnumerable<UserGroupPartRecord> GetGroupsWithDefault(int? value, string name);

        IEnumerable<UserGroupPartRecord> GetAvailableGroups(UserPart user);
        IEnumerable<UserGroupPartRecord> GetSubordinateGroups(UserGroupPartRecord group);

        #endregion

        #region GroupUser(s)

        IEnumerable<UserPartRecord> GetGroupUsers(UserPart user);

        IEnumerable<UserInGroupEntry> GetGroupUsersEntries(int groupId, string dateFrom, string dateTo);

        #endregion

        #region Action on User

        UserInGroupRecord GetUserInGroup(UserPart user);

        void AddUsersToGroup(int[] userIds, UserGroupPartRecord group);
        UserInGroupRecord AddUserToGroup(UserPart user, UserGroupPartRecord group);

        bool RemoveUserFromGroup(int id);
        bool RemoveUserFromGroup(UserPart user);

        void DeleteUser(UserPart user);
        void TransferUserProperties(UserPart user);

        #endregion

        #region User DefaultSettings

        // DefaultProvince
        int GetUserDefaultProvinceId(UserPart user);
        LocationProvincePartRecord GetUserDefaultProvince(UserPart user);

        // DefaultDistrict
        int GetUserDefaultDistrictId(UserPart user);
        LocationDistrictPartRecord GetUserDefaultDistrict(UserPart user);

        // DefaultAdsType
        AdsTypePartRecord GetUserDefaultAdsType(UserPart user);

        // DefaultTypeGroup
        PropertyTypeGroupPartRecord GetUserDefaultTypeGroup(UserPart user);

        #endregion

        #region Group Locations

        void DeleteGroupLocation(int id);
        List<int> GetGroupLocationProvinceIds(UserGroupPartRecord group);
        List<int> GetGroupLocationDistrictIds(UserGroupPartRecord group);
        List<int> GetGroupLocationWardIds(UserGroupPartRecord group);
        bool IsInGroupLocations(PropertyPart p, UserPart user);
        bool IsInGroupLocations(PropertyPart p, UserGroupPartRecord group);

        #endregion

        #region Group Contacts

        void DeleteGroupContact(int id);

        #endregion

        #region Group Shared Locations

        List<UserGroupSharedLocationRecord> GetGroupSharedLocations(UserGroupPartRecord leecherGroup);
        void DeleteGroupSharedLocation(int id);
        List<int> GetGroupSharedLocationProvinceIds(UserGroupPartRecord seederGroup, UserGroupPartRecord leecherGroup);
        List<int> GetGroupSharedLocationDistrictIds(UserGroupPartRecord seederGroup, UserGroupPartRecord leecherGroup);
        List<int> GetGroupSharedLocationWardIds(UserGroupPartRecord seederGroup, UserGroupPartRecord leecherGroup);
        bool IsInGroupSharedLocations(PropertyPart p, UserPart user);
        bool IsInGroupSharedLocations(PropertyPart p, UserGroupPartRecord leecherGroup);

        #endregion

        #region User Locations

        void DeleteUserLocation(int id);
        void ClearUserLocationCache();
        void ClearUserLocationCache(int? provinceId);

        bool IsInRetrictedAccessGroupLocations(PropertyPart p, UserPart user);
        // RetrictedAccessGroupLocations
        List<UserLocationRecord> GetUserRetrictedAccessGroupLocations(UserPart user);
        // RetrictedAccessGroupProvinces
        List<int> GetUserRetrictedAccessGroupLocationProvinceIds(UserPart user);
        // RetrictedAccessGroupDistricts
        List<int> GetUserRetrictedAccessGroupLocationDistrictIds(UserPart user);
        // RetrictedAccessGroupWards
        List<int> GetUserRetrictedAccessGroupLocationWardIds(UserPart user);

        // Provinces
        List<int> GetUserLocationProvinceIds(UserPart user);

        // Districts
        List<int> GetUserLocationDistrictIds(UserPart user);

        // Wards
        List<int> GetUserLocationWardIds(UserPart user);

        bool IsInUserLocations(PropertyPart p, UserPart user);

        // AgencyUserLocations by ProvinceId
        IEnumerable<UserLocationRecord> GetAgencyUserLocations();
        IEnumerable<UserLocationRecord> GetAgencyUserLocationProvinces(int? provinceId);
        IEnumerable<UserLocationRecord> GetAgencyUserLocationProvinces(int? provinceId, int[] districtIds);
        IEnumerable<UserLocationRecord> GetAgencyUserLocationProvinces(int? provinceId, int? districtIds);

        //UserUpdateProfile
        IEnumerable<UserUpdateProfileRecord> GetUserUpdateProfiles(int[] userPartIds);
        IEnumerable<UserUpdateProfileRecord> GetUserUpdateProfiles(int? userPartId);

        #endregion

        #region User Enable Edit Locations

        // Provinces
        IEnumerable<LocationProvincePart> GetUserEnableEditLocationProvinces(UserPart user);

        // Districts
        IEnumerable<LocationDistrictPart> GetUserEnableEditLocationDistricts(UserPart user, int? provinceId);

        // Wards
        //IEnumerable<LocationWardPartRecord> GetUserEnableEditLocationWards(UserPart user, int? districtId);

        #endregion

        #region Group Roles

        IEnumerable<RoleRecord> GetUserRoles(UserPart user);
        IEnumerable<RoleRecord> GetGroupRoles(UserGroupPartRecord group);
        IEnumerable<RoleRecord> GetUserAvailableRoles(UserPart user);

        #endregion

        #region User Activities & Points

        double GetUserPoints(UserPartRecord user, string dateFrom, string dateTo);
        IEnumerable<UserActivityPartRecord> GetUserActivities(UserPartRecord user);
        IEnumerable<UserActivityPartRecord> GetUserActivities(UserPartRecord user, string dateFrom, string dateTo);

        #endregion

        #region BuildViewModel

        UserGroupEditViewModel BuildEditViewModel(UserGroupPart g, string dateFrom, string dateTo);

        #endregion

        string GetHostNameByUser(UserPart user);
    }

    public class UserGroupService : IUserGroupService
    {
        #region Init

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours
        private readonly IAddressService _addressService;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        //private readonly IPropertyAttributesService _propertyAttributesService;

        private readonly IRepository<CustomerPropertyUserRecord> _customerpropertyUserRepository;
        private readonly IRepository<UserGroupContactRecord> _groupContactRepository;
        private readonly IRepository<UserGroupLocationRecord> _groupLocationRepository;
        private readonly IRepository<UserGroupSharedLocationRecord> _groupSharedLocationRepository;
        private readonly IRepository<RoleRecord> _roleRepository;
        private readonly ISignals _signals;
        private readonly IRepository<UserActivityPartRecord> _userActivityRepository;
        private readonly IRepository<UserInGroupRecord> _userInGroupRepository;
        private readonly IRepository<UserLocationRecord> _userLocationRepository;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IRepository<UserUpdateProfileRecord> _userUpdateRepository;
        private readonly IHostNameService _hostNameService;

        public UserGroupService(
            IAddressService addressService,
            //IPropertyAttributesService propertyAttributesService,

            IRepository<RoleRecord> roleRepository,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRepository<UserActivityPartRecord> userActivityRepository,
            IRepository<UserInGroupRecord> userInGroupRepository,
            IRepository<CustomerPropertyUserRecord> customerpropertyUserRepository,
            IRepository<UserGroupLocationRecord> groupLocationRepository,
            IRepository<UserGroupContactRecord> groupContactRepository,
            IRepository<UserGroupSharedLocationRecord> groupSharedLocationRepository,
            IRepository<UserLocationRecord> userLocationRepository,
            IRepository<UserUpdateProfileRecord> userUpdateRepository,
            IHostNameService hostNameService,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IOrchardServices services
            )
        {
            _addressService = addressService;
            //_propertyAttributesService = propertyAttributesService;

            _roleRepository = roleRepository;
            _userRolesRepository = userRolesRepository;
            _userActivityRepository = userActivityRepository;
            _userInGroupRepository = userInGroupRepository;
            _customerpropertyUserRepository = customerpropertyUserRepository;
            _groupLocationRepository = groupLocationRepository;
            _groupContactRepository = groupContactRepository;
            _groupSharedLocationRepository = groupSharedLocationRepository;
            _userLocationRepository = userLocationRepository;
            _userUpdateRepository = userUpdateRepository;
            _hostNameService = hostNameService;

            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
            Services = services;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }

        #endregion

        #region Verify

        public bool VerifyUserGroupUnicity(string groupName)
        {
            if (_contentManager.Query<UserGroupPart, UserGroupPartRecord>().Where(r => r.Name == groupName).List().Any())
            {
                return false;
            }

            return true;
        }

        public bool VerifyUserGroupUnicity(int id, string groupName)
        {
            if (
                _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .Where(r => r.Name == groupName)
                    .List()
                    .Any(r => r.Id != id))
            {
                return false;
            }

            return true;
        }

        public bool IsSupervisor(UserGroupPartRecord group, UserPart supervisorUser)
        {
            if (group != null && supervisorUser != null)
            {
                return _cacheManager.Get("IsSupervisor_" + group.Id + "_" + supervisorUser.Id, ctx =>
                {
                    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                    ctx.Monitor(_signals.When("IsSupervisor_Changed"));

                    UserGroupPartRecord currentGroup = group;
                    while (currentGroup != null)
                    {
                        if (currentGroup.GroupAdminUser.Id == supervisorUser.Id)
                            return true;
                        currentGroup = GetJointGroup(currentGroup.GroupAdminUser.Id);
                    }
                    return false;
                });
            }

            return false;
        }

        public bool IsSupervisor(UserGroupPartRecord group, UserGroupPartRecord supervisorGroup)
        {
            if (group != null && supervisorGroup != null)
            {
                UserGroupPartRecord currentGroup = group;
                while (currentGroup != null)
                {
                    if (currentGroup == supervisorGroup)
                        return true;
                    currentGroup = GetJointGroup(currentGroup.GroupAdminUser.Id);
                }
            }
            return false;
        }

        #endregion

        #region Users

        public UserPart GetUser(int? userId)
        {
            if (!userId.HasValue) return null;
            var user = _contentManager.Get<UserPart>((int)userId);
            if (user != null)
            {
                return user;
            }
            return GetUserDefault();
        }

        public UserPart GetUser(string userName)
        {
            if (_contentManager.Query<UserPart, UserPartRecord>().Where(a => a.UserName == userName).Count() > 0)
                return
                    _contentManager.Query<UserPart, UserPartRecord>()
                        .Where(a => a.UserName == userName)
                        .List()
                        .FirstOrDefault();
            return GetUserDefault();
        }

        public UserPart GetUserDefault()
        {
            return
                _contentManager.Query<UserPart, UserPartRecord>()
                    .Where(a => a.NormalizedUserName == "congty")
                    .List()
                    .First();
        }

        public IEnumerable<UserPart> GetUsers()
        {
            return _cacheManager.Get("Users", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Users_Changed"));

                return _contentManager.Query<UserPart, UserPartRecord>().OrderBy(a => a.UserName).List();
            });
        }

        public IEnumerable<UserPart> GetUsers(List<int> listIds)
        {
            return GetUsers().Where(a => listIds.Contains(a.Id));
        }

        public IEnumerable<UserPart> SearchUsers(string term)
        {
            return _cacheManager.Get("Users_" + term, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Users_Changed"));

                return _contentManager.Query<UserPart, UserPartRecord>().Where(a => a.UserName.Contains(term)).OrderBy(a => a.UserName).List();
            });
        }

        public IEnumerable<UserPart> SearchAvailableGroupMemberUsers(UserGroupPartRecord group, string term)
        {
            return _cacheManager.Get("AvailableGroupMemberUsers_" + term, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Users_Changed"));

                return GetAvailableGroupMemberUsers(group).Where(a => a.UserName.Contains(term)).OrderBy(a => a.UserName).List();
            });
        }

        public IEnumerable<UserPart> SearchAvailableGroupAdminUsers(UserGroupPartRecord group, string term)
        {
            return _cacheManager.Get("AvailableGroupAdminUsers_" + term, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Users_Changed"));

                return GetAvailableGroupAdminUsers(group).Where(a => a.UserName.Contains(term)).OrderBy(a => a.UserName).List();
            });
        }

        #endregion

        #region Get Users

        public List<int> GetUserInGroupIds()
        {
            List<int> userInGroupIds = _userInGroupRepository.Table.Select(a => a.UserPartRecord.Id).ToList();
            List<int> userAdminGroupIds =
                _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .List()
                    .Select(a => a.GroupAdminUser.Id)
                    .ToList();
            userInGroupIds.AddRange(userAdminGroupIds);

            return userInGroupIds;
        }

        public List<int> GetUserInGroupIds(int groupId)
        {
            if (groupId <= 0)
                return GetUserInGroupIds();

            var group = _contentManager.Get<UserGroupPart>(groupId);
            List<int> userInGroupIds = group.GroupUsers.Select(a => a.Id).ToList();
            userInGroupIds.Add(group.GroupAdminUser.Id);

            return userInGroupIds;
        }

        public IContentQuery<UserPart, UserPartRecord> GetAvailableGroupMemberUsers(UserGroupPartRecord group)
        {
            // Lấy tất cả các user chưa join 1 group nào
            List<int> userInGroup = _userInGroupRepository.Table.Select(a => a.UserPartRecord.Id).ToList();
            var uList = _contentManager.Query<UserPart, UserPartRecord>()
            //.Where(u => _userInGroupRepository.Count(a => a.UserPartRecord.Id == u.Id) == 0);
            .Where(u => !userInGroup.Contains(u.Id));

            // Loại adminUser của group hiện tại
            UserPartRecord userAdmin = group.GroupAdminUser;
            uList = uList.Where(u => u.Id != userAdmin.Id);

            // Lấy group mà adminUser đã join
            UserGroupPartRecord adminGroup = GetJointGroup(userAdmin.Id);

            // Check nếu adminUser có join vào group nào khác
            while (adminGroup != null)
            {
                // Loại các adminUser của parentGroup của group đang xét
                userAdmin = adminGroup.GroupAdminUser;
                uList = uList.Where(u => u.Id != userAdmin.Id);
                adminGroup = GetJointGroup(userAdmin.Id);
            }

            return uList;
        }

        public IContentQuery<UserPart, UserPartRecord> GetAvailableGroupAdminUsers(UserGroupPartRecord group)
        {
            List<int> excludedUserIds = GetRelevantUserIds(group);
            return _contentManager.Query<UserPart, UserPartRecord>().Where(a => !excludedUserIds.Contains(a.Id));

            //List<int> excludedGroupIds = GetRelevantGroupIds(group);
            //return _contentManager.Query<UserPart, UserPartRecord>().Where(a => !_userInGroupRepository.Fetch(b => b.UserPartRecord == a && excludedGroupIds.Contains(b.UserGroupPartRecord.Id)).Any());
        }

        public List<int> GetRelevantUserIds(UserPart user)
        {
            var list = new List<int>();

            UserGroupPartRecord jointGroup = GetJointGroup(user.Id);
            UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);

            if (jointGroup != null)
            {
                list.AddRange(GetRelevantUserIds(jointGroup));
                list.Add(jointGroup.GroupAdminUser.Id);
            }
            else if (ownGroup != null)
            {
                list.AddRange(GetRelevantUserIds(ownGroup));
                list.Add(ownGroup.GroupAdminUser.Id);
            }
            else
            {
                list.Add(user.Id);
            }

            return list;
        }

        public List<int> GetRelevantUserIds(UserGroupPartRecord group)
        {
            return _cacheManager.Get("RelevantUserIds_" + group.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Users_Changed"));

                var list = new List<int>();
                List<int> groupUserIds =
                    _userInGroupRepository.Fetch(a => a.UserGroupPartRecord == group)
                        .Select(a => a.UserPartRecord.Id)
                        .ToList();
                list.AddRange(groupUserIds);

                foreach (int userId in groupUserIds)
                {
                    // Check if user is Admin of sub Group
                    var user = _contentManager.Get<UserPart>(userId);
                    UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
                    if (ownGroup != null)
                    {
                        list.AddRange(GetRelevantUserIds(ownGroup));
                    }
                }
                return list;
            });
        }

        public List<int> GetRelevantGroupIds(UserPart user)
        {
            var list = new List<int>();

            UserGroupPartRecord jointGroup = GetJointGroup(user.Id);
            UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);

            if (jointGroup != null)
            {
                list.AddRange(GetRelevantGroupIds(jointGroup));
            }
            else if (ownGroup != null)
            {
                list.AddRange(GetRelevantGroupIds(ownGroup));
            }

            return list;
        }

        public List<int> GetRelevantGroupIds(UserGroupPartRecord group)
        {
            return _cacheManager.Get("RelevantGroupIds_" + group.Id, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Groups_Changed"));

                var list = new List<int>();
                list.Add(group.Id);
                var users = _userInGroupRepository.Fetch(a => a.UserGroupPartRecord == group).Select(a => a.UserPartRecord);
                foreach (var user in users)
                {
                    UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
                    if (ownGroup != null)
                    {
                        list.AddRange(GetRelevantGroupIds(ownGroup));
                    }
                }

                return list;
            });
        }

        // Lấy các user cấp dưới
        //public List<int> GetSubordinateUserIds(UserPart user)
        //{
        //    var subordinateUserIds = new List<int> { user.Id };
        //    UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
        //    if (ownGroup != null) subordinateUserIds.AddRange(GetRelevantUserIds(ownGroup));
        //    return subordinateUserIds;
        //}

        public IEnumerable<UserPart> GetAvailableUsers()
        {
            List<int> userInGroupIds = _userInGroupRepository.Table.Select(a => a.UserPartRecord.Id).ToList();
            return _contentManager.Query<UserPart, UserPartRecord>().Where(a => !userInGroupIds.Contains(a.Id)).List();
        }

        #endregion

        #region Get Group

        public UserGroupPartRecord GetCurrentDomainGroup()
        {
            UserGroupPartRecord currentDomainGroup = null;

            string hostName = Services.WorkContext.HttpContext.Request.Url != null ?
                                Services.WorkContext.HttpContext.Request.Url.Host :
                                "dinhgianhadat.vn";

            if (_contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.Name == hostName).Count() > 0)
            {
                HostNamePart hostPart =
                    _contentManager.Query<HostNamePart, HostNamePartRecord>()
                        .Where(a => a.Name == hostName)
                        .List()
                        .First();

                if (hostPart != null)
                {
                    currentDomainGroup = hostPart.As<UserGroupPart>().Record;
                }
            }

            return currentDomainGroup;
        }

        public UserGroupPartRecord GetDefaultDomainGroup()
        {
            UserGroupPartRecord defaultDomainGroup = null;

            if (_contentManager.Query<HostNamePart, HostNamePartRecord>()
                    .Where(a => a.CssClass == "host-name-main").Count() > 0)
            {
                HostNamePart hostPart =
                    _contentManager.Query<HostNamePart, HostNamePartRecord>()
                        .Where(a => a.CssClass == "host-name-main")
                        .List()
                        .First();

                if (hostPart != null)
                {
                    defaultDomainGroup = hostPart.As<UserGroupPart>().Record;
                }
            }

            return defaultDomainGroup;
        }

        public UserGroupPartRecord GetGroup(int? groupId)
        {
            if (groupId > 0)
                return _contentManager.Get<UserGroupPart>((int)groupId).Record;
            return null;
        }

        public IEnumerable<UserGroupPart> SearchGroups(string term)
        {
            return _cacheManager.Get("Groups_" + term, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Groups_Changed"));

                return _contentManager.Query<UserGroupPart, UserGroupPartRecord>().Where(a => a.Name.Contains(term)).OrderBy(a => a.Name).List();
            });
        }

        public IEnumerable<UserGroupPartRecord> GetGroups()
        {
            return _cacheManager.Get("Groups", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Groups_Changed"));

                return
                    _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                        .List()
                        .Select(a => a.Record)
                        .OrderBy(a => a.SeqOrder);
            });
        }

        public UserGroupPartRecord GetBelongGroup(int userId)
        {
            UserGroupPartRecord group = GetJointGroup(userId) ?? GetOwnGroup(userId);
            return group;
        }

        public UserGroupPartRecord GetJointGroup(int userId)
        {
            // User joint in a group
            IEnumerable<UserInGroupRecord> group =
                _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == userId).ToList();
            return group.Any() ? group.First().UserGroupPartRecord : null;
        }

        public UserGroupPartRecord GetOwnGroup(int userId)
        {
            // User is admin of a group
            if (
                _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .Where(a => a.GroupAdminUser.Id == userId)
                    .Count() > 0)
            {
                return
                    _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                        .Where(a => a.GroupAdminUser.Id == userId)
                        .Slice(1)
                        .First()
                        .Record;
            }
            return null;
        }

        public string GetGroupShortName(int userId)
        {
            UserGroupPartRecord group = GetJointGroup(userId);
            return group != null ? group.ShortName : "";
        }

        public IEnumerable<UserGroupPartRecord> GetGroupsWithDefault(int? value, string name)
        {
            List<UserGroupPartRecord> list =
                _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .OrderBy(a => a.SeqOrder)
                    .List()
                    .Select(a => a.Record)
                    .ToList();
            list.Insert(0,
                new UserGroupPartRecord { Id = value ?? 0, Name = String.IsNullOrEmpty(name) ? "-- Khách hàng --" : name });
            return list;
        }

        /// <summary>
        ///     Lấy danh sách các Group mà user có thể join
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<UserGroupPartRecord> GetAvailableGroups(UserPart user)
        {
            var exceptList = new List<int>();

            UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
            if (ownGroup != null) exceptList = GetSubordinateGroups(ownGroup).Select(a => a.Id).ToList();

            return
                _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .Where(a => !exceptList.Contains(a.Id))
                    .List()
                    .Select(a => a.Record);
        }

        /// <summary>
        ///     Lấy danh sách các Group con
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IEnumerable<UserGroupPartRecord> GetSubordinateGroups(UserGroupPartRecord group)
        {
            var list = new List<UserGroupPartRecord>();
            if (group != null)
            {
                list.Add(group);
                List<int> groupUserIds =
                    _userInGroupRepository.Fetch(a => a.UserGroupPartRecord == group)
                        .Select(a => a.UserPartRecord.Id)
                        .ToList();
                foreach (int userId in groupUserIds)
                {
                    var user = _contentManager.Get<UserPart>(userId);
                    // Check if user is Admin of sub Group
                    UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
                    if (ownGroup != null)
                    {
                        list.AddRange(GetSubordinateGroups(ownGroup));
                    }
                }
            }
            return list;
        }

        #endregion

        #region GroupUser(s)

        public IEnumerable<UserPartRecord> GetGroupUsers(UserPart user)
        {
            UserGroupPartRecord group = GetBelongGroup(user.Id);
            if (group != null)
            {
                List<UserPartRecord> list =
                    _userInGroupRepository.Fetch(r => r.UserGroupPartRecord == group)
                        .Select(a => a.UserPartRecord)
                        .ToList();
                list.Add(group.GroupAdminUser);
                list = list.OrderBy(a => a.UserName).ToList();
                return list;
            }
            return new List<UserPartRecord>();
        }

        public IEnumerable<UserInGroupEntry> GetGroupUsersEntries(int groupId, string dateFrom, string dateTo)
        {
            return _userInGroupRepository.Fetch(r => r.UserGroupPartRecord.Id == groupId)
                .Select(x => new UserInGroupEntry
                {
                    UserInGroupRecord = x,
                    User = x.UserPartRecord,
                    Points = GetUserPoints(x.UserPartRecord, dateFrom, dateTo),
                    Roles =
                        _userRolesRepository.Fetch(a => a.UserId == x.UserPartRecord.Id)
                            .Select(a => a.Role.Name)
                            .ToList()
                }).OrderBy(a => a.User.UserName);
        }

        #endregion

        #region User Activities & Points

        public double GetUserPoints(UserPartRecord user, string dateFrom, string dateTo)
        {
            return GetUserActivities(user, dateFrom, dateTo).Sum(a => a.UserActionPartRecord.Point);
        }

        public IEnumerable<UserActivityPartRecord> GetUserActivities(UserPartRecord user)
        {
            return _userActivityRepository.Fetch(a => a.UserPartRecord == user).ToList();
        }

        public IEnumerable<UserActivityPartRecord> GetUserActivities(UserPartRecord user, string dateFrom, string dateTo)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            const string format = "dd/MM/yyyy";

            DateTime dateFromConverted = string.IsNullOrEmpty(dateFrom)
                ? DateExtension.GetStartOfCurrentMonth()
                : DateExtension.GetStartOfDate(DateTime.ParseExact(dateFrom, format, provider));

            DateTime dateToConverted = string.IsNullOrEmpty(dateTo)
                ? DateExtension.GetEndOfCurrentMonth()
                : DateExtension.GetEndOfDate(DateTime.ParseExact(dateTo, format, provider));

            // Services.Notifier.Information(T("{0} - {1}", dateFromConverted, dateToConverted));

            return
                _userActivityRepository.Fetch(
                    a =>
                        a.UserPartRecord == user && a.CreatedDate >= dateFromConverted &&
                        a.CreatedDate <= dateToConverted);
        }

        #endregion

        #region Action on User

        public UserInGroupRecord GetUserInGroup(UserPart user)
        {
            if (_userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id).Any())
            {
                return _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id).First();
            }
            return null;
        }

        public void AddUsersToGroup(int[] userIds, UserGroupPartRecord group)
        {
            foreach (int userId in userIds)
            {
                UserPart user = GetUser(userId);
                AddUserToGroup(user, group);
            }
        }

        public UserInGroupRecord AddUserToGroup(UserPart user, UserGroupPartRecord group)
        {
            if (group.GroupAdminUser.Id == user.Id) return null;

            UserInGroupRecord r;

            if (
                _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.UserGroupPartRecord == group)
                    .Any())
            {
                r =
                    _userInGroupRepository.Fetch(a => a.UserGroupPartRecord == group && a.UserPartRecord.Id == user.Id)
                        .First();
                return r;
            }
            if (
                _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.UserGroupPartRecord != group)
                    .Any())
            {
                r =
                    _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.UserGroupPartRecord != group)
                        .First();
                _userInGroupRepository.Delete(r);
            }

            r = new UserInGroupRecord { UserPartRecord = user.Record, UserGroupPartRecord = @group };
            _userInGroupRepository.Create(r);

            return r;
        }

        public bool RemoveUserFromGroup(int id)
        {
            UserInGroupRecord r = _userInGroupRepository.Get(id);
            if (r != null)
            {
                var user = Services.WorkContext.CurrentUser.As<UserPart>();
                if (IsSupervisor(r.UserGroupPartRecord, user) || Services.Authorizer.Authorize(Permissions.ManageUsers))
                {
                    _userInGroupRepository.Delete(r);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveUserFromGroup(UserPart user)
        {
            if (_userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id).Any())
            {
                UserInGroupRecord r = _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id).First();
                if (IsSupervisor(r.UserGroupPartRecord, user) || Services.Authorizer.Authorize(Permissions.ManageUsers))
                {
                    _userInGroupRepository.Delete(r);
                    return true;
                }
            }
            return false;
        }

        public void DeleteUser(UserPart user)
        {
            UserPart userDefault = GetUserDefault();

            // Tranfer user's properties
            TransferUserProperties(user);

            // RealEstate_RevisionPartRecord --> Transfer to DeletedUser
            IEnumerable<RevisionPart> revisions = _contentManager.Query<RevisionPart, RevisionPartRecord>()
                .Where(a => a.CreatedUser.Id == user.Id).List();
            foreach (RevisionPart r in revisions)
            {
                r.CreatedUser = userDefault.Record;
            }

            // RealEstate_CustomerPropertyUserRecord --> Transfer to DeletedUser
            List<CustomerPropertyUserRecord> customerUsers =
                _customerpropertyUserRepository.Fetch(a => a.UserPartRecord.Id == user.Id).ToList();
            foreach (CustomerPropertyUserRecord c in customerUsers)
            {
                c.UserPartRecord = userDefault.Record;
            }

            // RealEstate_UserActivityPartRecord --> Delete
            List<UserActivityPartRecord> activities =
                _userActivityRepository.Fetch(a => a.UserPartRecord.Id == user.Id).ToList();
            foreach (UserActivityPartRecord a in activities)
            {
                _userActivityRepository.Delete(a);
            }

            // RealEstate_UserInGroupRecord --> Delete
            List<UserInGroupRecord> userInGroup =
                _userInGroupRepository.Fetch(a => a.UserPartRecord.Id == user.Id).ToList();
            foreach (UserInGroupRecord u in userInGroup)
            {
                _userInGroupRepository.Delete(u);
            }

            // RealEstate_UserLocationRecord(Nhân viên môi giới) --> Delete
            List<UserLocationRecord> record =
               _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id).ToList();
            foreach (UserLocationRecord a in record)
            {
                _userLocationRepository.Delete(a);
                var hostname = _hostNameService.GetHostNameSite();
                _signals.Trigger("Agencies_" + hostname + "_Changed");
            }

            // Delete User
            _contentManager.Remove(_contentManager.Get(user.Id));
        }

        public void TransferUserProperties(UserPart user)
        {
            if (user != null)
            {
                var userDefault = GetUserDefault();
                var groupDefault = GetBelongGroup(userDefault.Id);

                if (userDefault != null)
                {
                    // chuyển về group AdminUser
                    var userDefaultRecord = userDefault.Record;
                    UserGroupPartRecord jointGroup = GetJointGroup(user.Id);
                    if (jointGroup != null)
                    {
                        if (jointGroup.GroupAdminUser != null) userDefaultRecord = jointGroup.GroupAdminUser;
                        groupDefault = jointGroup;
                    }


                    // RealEstate_PropertyPartRecord --> Transfer to groupAdmin, or DeletedUser
                    IEnumerable<PropertyPart> properties = _contentManager.Query<PropertyPart, PropertyPartRecord>()
                        .Where(a =>
                            (a.CreatedUser != null && a.CreatedUser.Id == user.Id) ||
                            (a.LastUpdatedUser != null && a.LastUpdatedUser.Id == user.Id) ||
                            (a.FirstInfoFromUser != null && a.FirstInfoFromUser.Id == user.Id) ||
                            (a.LastInfoFromUser != null && a.LastInfoFromUser.Id == user.Id) ||
                            (a.LastExportedUser != null && a.LastExportedUser.Id == user.Id)
                        ).List();
                    if (properties != null)
                    {
                        foreach (PropertyPart p in properties)
                        {
                            if (p.CreatedUser != null && p.CreatedUser.Id == user.Id)
                            {
                                p.CreatedUser = userDefaultRecord;
                                p.UserGroup = groupDefault;
                            }
                            if (p.LastUpdatedUser != null && p.LastUpdatedUser.Id == user.Id)
                                p.LastUpdatedUser = userDefaultRecord;
                            if (p.FirstInfoFromUser != null && p.FirstInfoFromUser.Id == user.Id)
                                p.FirstInfoFromUser = userDefaultRecord;
                            if (p.LastInfoFromUser != null && p.LastInfoFromUser.Id == user.Id)
                                p.LastInfoFromUser = userDefaultRecord;
                            if (p.LastExportedUser != null && p.LastExportedUser.Id == user.Id)
                                p.LastExportedUser = userDefaultRecord;
                        }
                    }

                    // RealEstate_PropertyFilePartRecord --> Transfer to groupAdmin, or DeletedUser
                    IEnumerable<PropertyFilePart> files =
                        _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                            .Where(a => a.CreatedUser != null && a.CreatedUser.Id == user.Id).List();
                    if (files != null)
                    {
                        foreach (PropertyFilePart f in files)
                        {
                            f.CreatedUser = userDefaultRecord;
                        }
                    }
                }
            }
        }

        #endregion

        #region User DefaultSettings

        // DefaultProvince
        public int GetUserDefaultProvinceId(UserPart user)
        {
            LocationProvincePartRecord defaultProvince = GetUserDefaultProvince(user);
            if (defaultProvince != null)
                return defaultProvince.Id;
            return 0;
        }

        public LocationProvincePartRecord GetUserDefaultProvince(UserPart user)
        {
            // Get Default from Group Setting
            UserGroupPartRecord group = GetBelongGroup(user.Id);
            if (group != null && group.DefaultProvince != null)
                return group.DefaultProvince;
            return null;
            // TP. Hồ Chí Minh
            //return _addressService.GetProvince("TP. Hồ Chí Minh");
        }

        // DefaultDistrict
        public int GetUserDefaultDistrictId(UserPart user)
        {
            LocationDistrictPartRecord defaultDictrict = GetUserDefaultDistrict(user);
            if (defaultDictrict != null)
                return defaultDictrict.Id;
            return 0;
        }

        public LocationDistrictPartRecord GetUserDefaultDistrict(UserPart user)
        {
            // Get Default from Group Setting
            UserGroupPartRecord group = GetBelongGroup(user.Id);
            if (group != null && group.DefaultDistrict != null)
                return group.DefaultDistrict;
            return null;
        }

        // DefaultAdsType
        public AdsTypePartRecord GetUserDefaultAdsType(UserPart user)
        {
            // User Setting
            UserInGroupRecord userInGroup = GetUserInGroup(user);
            if (userInGroup != null && userInGroup.DefaultAdsType != null)
                return userInGroup.DefaultAdsType;

            // Group Setting
            UserGroupPartRecord group = GetBelongGroup(user.Id);
            if (group != null && group.DefaultAdsType != null)
                return group.DefaultAdsType;

            // Default: ad-selling
            return
                _contentManager.Query<AdsTypePart, AdsTypePartRecord>()
                    .Where(a => a.CssClass == "ad-selling")
                    .List()
                    .First()
                    .Record;
        }

        // DefaultTypeGroup
        public PropertyTypeGroupPartRecord GetUserDefaultTypeGroup(UserPart user)
        {
            // User Setting
            UserInGroupRecord userInGroup = GetUserInGroup(user);
            if (userInGroup != null && userInGroup.DefaultTypeGroup != null)
                return userInGroup.DefaultTypeGroup;

            // Group Setting
            UserGroupPartRecord group = GetBelongGroup(user.Id);
            if (group != null && group.DefaultTypeGroup != null)
                return group.DefaultTypeGroup;

            // Default: gp-house
            return
                _contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>()
                    .Where(a => a.CssClass == "gp-house")
                    .List()
                    .First()
                    .Record;
        }

        // DefaultPropertyStatus
        public PropertyStatusPartRecord GetUserDefaultPropertyStatus(UserPart user)
        {
            UserGroupPartRecord group = GetBelongGroup(user.Id);
            if (group != null && group.DefaultPropertyStatus != null)
                return group.DefaultPropertyStatus;
            // st-new
            return
                _contentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                    .Where(a => a.CssClass == "st-new")
                    .List()
                    .First()
                    .Record;
        }

        #endregion

        #region Group Locations

        public void DeleteGroupLocation(int id)
        {
            UserGroupLocationRecord record = _groupLocationRepository.Get(id);
            _groupLocationRepository.Delete(record);
        }

        public List<int> GetGroupLocationProvinceIds(UserGroupPartRecord group)
        {
            var result = new List<int>();
            IEnumerable<UserGroupLocationRecord> provinces =
                _groupLocationRepository.Fetch(
                    a =>
                        a.UserGroupPartRecord == group && a.LocationProvincePartRecord != null &&
                        a.LocationDistrictPartRecord == null && a.LocationWardPartRecord == null).ToList();
            if (provinces.Any())
                result = provinces.Select(a => a.LocationProvincePartRecord.Id).ToList();
            return result;
        }

        public List<int> GetGroupLocationDistrictIds(UserGroupPartRecord group)
        {
            var result = new List<int>();
            IEnumerable<UserGroupLocationRecord> districts =
                _groupLocationRepository.Fetch(
                    a =>
                        a.UserGroupPartRecord == group && a.LocationProvincePartRecord != null &&
                        a.LocationDistrictPartRecord != null && a.LocationWardPartRecord == null).ToList();
            if (districts.Any())
                result = districts.Select(a => a.LocationDistrictPartRecord.Id).ToList();
            return result;
        }

        public List<int> GetGroupLocationWardIds(UserGroupPartRecord group)
        {
            var result = new List<int>();
            IEnumerable<UserGroupLocationRecord> wards =
                _groupLocationRepository.Fetch(
                    a =>
                        a.UserGroupPartRecord == group && a.LocationProvincePartRecord != null &&
                        a.LocationDistrictPartRecord != null && a.LocationWardPartRecord != null).ToList();
            if (wards.Any()) result = wards.Select(a => a.LocationWardPartRecord.Id).ToList();
            return result;
        }

        public bool IsInGroupLocations(PropertyPart p, UserPart user)
        {
            // Check if user is GroupAdmin
            UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
            if (ownGroup != null)
                return IsInGroupLocations(p, ownGroup);

            return false;
        }

        public bool IsInGroupLocations(PropertyPart p, UserGroupPartRecord group)
        {
            if (p == null || group == null) return false;

            List<int> groupLocationProvinceIds = GetGroupLocationProvinceIds(group);
            List<int> groupLocationDistrictIds = GetGroupLocationDistrictIds(group);
            List<int> groupLocationWardIds = GetGroupLocationWardIds(group);

            int provinceId = p.Province != null ? p.Province.Id : -1;
            int districtId = p.District != null ? p.District.Id : -1;
            int wardId = p.Ward != null ? p.Ward.Id : -1;

            if (groupLocationProvinceIds.Contains(provinceId) ||
                groupLocationDistrictIds.Contains(districtId) ||
                groupLocationWardIds.Contains(wardId))
                return true;

            return false;
        }

        public IEnumerable<UserGroupLocationRecord> GetGroupLocations()
        {
            return _groupLocationRepository.Table.ToList();
        }

        public IEnumerable<UserGroupLocationRecord> GetGroupLocations(UserGroupPartRecord group)
        {
            return _groupLocationRepository.Fetch(r => r.UserGroupPartRecord == group).ToList();
        }

        #endregion

        #region Group Contacts

        public void DeleteGroupContact(int id)
        {
            UserGroupContactRecord record = _groupContactRepository.Get(id);
            _groupContactRepository.Delete(record);
        }

        public IEnumerable<UserGroupContactRecord> GetGroupContacts()
        {
            return _groupContactRepository.Table.ToList();
        }

        public IEnumerable<UserGroupContactRecord> GetGroupContacts(UserGroupPartRecord group)
        {
            return _groupContactRepository.Fetch(r => r.UserGroupPartRecord == group).ToList();
        }

        #endregion

        #region Group Shared Locations

        public List<UserGroupSharedLocationRecord> GetGroupSharedLocations(UserGroupPartRecord leecherGroup)
        {
            return _groupSharedLocationRepository.Fetch(r => r.LeecherUserGroupPartRecord == leecherGroup && r.SeederUserGroupPartRecord != null && r.LocationProvincePartRecord != null).ToList();
        }

        public void DeleteGroupSharedLocation(int id)
        {
            UserGroupSharedLocationRecord record = _groupSharedLocationRepository.Get(id);
            _groupSharedLocationRepository.Delete(record);
        }

        public List<int> GetGroupSharedLocationProvinceIds(UserGroupPartRecord seederGroup,
            UserGroupPartRecord leecherGroup)
        {
            var result = new List<int>();
            IEnumerable<UserGroupSharedLocationRecord> provinces =
                _groupSharedLocationRepository.Fetch(
                    a =>
                        (a.SeederUserGroupPartRecord == seederGroup || a.LeecherUserGroupPartRecord == leecherGroup) &&
                        a.LocationProvincePartRecord != null && a.LocationDistrictPartRecord == null &&
                        a.LocationWardPartRecord == null).ToList();
            if (provinces.Any())
                result = provinces.Select(a => a.LocationProvincePartRecord.Id).ToList();
            return result;
        }

        public List<int> GetGroupSharedLocationDistrictIds(UserGroupPartRecord seederGroup,
            UserGroupPartRecord leecherGroup)
        {
            var result = new List<int>();
            IEnumerable<UserGroupSharedLocationRecord> districts =
                _groupSharedLocationRepository.Fetch(
                    a =>
                        (a.SeederUserGroupPartRecord == seederGroup || a.LeecherUserGroupPartRecord == leecherGroup) &&
                        a.LocationProvincePartRecord != null && a.LocationDistrictPartRecord != null &&
                        a.LocationWardPartRecord == null).ToList();
            if (districts.Any())
                result = districts.Select(a => a.LocationDistrictPartRecord.Id).ToList();
            return result;
        }

        public List<int> GetGroupSharedLocationWardIds(UserGroupPartRecord seederGroup, UserGroupPartRecord leecherGroup)
        {
            var result = new List<int>();
            IEnumerable<UserGroupSharedLocationRecord> wards =
                _groupSharedLocationRepository.Fetch(
                    a =>
                        (a.SeederUserGroupPartRecord == seederGroup || a.LeecherUserGroupPartRecord == leecherGroup) &&
                        a.LocationProvincePartRecord != null && a.LocationDistrictPartRecord != null &&
                        a.LocationWardPartRecord != null).ToList();
            if (wards.Any()) result = wards.Select(a => a.LocationWardPartRecord.Id).ToList();
            return result;
        }

        public bool IsInGroupSharedLocations(PropertyPart p, UserPart user)
        {
            UserGroupPartRecord jointGroup = GetJointGroup(user.Id);
            if (jointGroup != null)
                if (IsInGroupSharedLocations(p, jointGroup))
                    return true;
            // Check if user is GroupAdmin
            UserGroupPartRecord ownGroup = GetOwnGroup(user.Id);
            if (ownGroup != null)
                if (IsInGroupSharedLocations(p, ownGroup))
                    return true;

            return false;
        }

        public bool IsInGroupSharedLocations(PropertyPart p, UserGroupPartRecord leecherGroup)
        {
            if (p == null || p.UserGroup == null || leecherGroup == null) return false;

            UserGroupPartRecord seederGroup = p.UserGroup;

            List<int> groupSharedLocationProvinceIds = GetGroupSharedLocationProvinceIds(seederGroup, leecherGroup);
            List<int> groupSharedLocationDistrictIds = GetGroupSharedLocationDistrictIds(seederGroup, leecherGroup);
            List<int> groupSharedLocationWardIds = GetGroupSharedLocationWardIds(seederGroup, leecherGroup);

            int provinceId = p.Province != null ? p.Province.Id : -1;
            int districtId = p.District != null ? p.District.Id : -1;
            int wardId = p.Ward != null ? p.Ward.Id : -1;

            if (groupSharedLocationProvinceIds.Contains(provinceId) ||
                groupSharedLocationDistrictIds.Contains(districtId) ||
                groupSharedLocationWardIds.Contains(wardId))
                return true;

            return false;
        }

        public IEnumerable<UserGroupSharedLocationRecord> GetGroupSharedLocations()
        {
            return _groupSharedLocationRepository.Table.ToList();
        }

        #endregion

        #region User Locations

        public void DeleteUserLocation(int id)
        {
            UserLocationRecord record = _userLocationRepository.Get(id);
            _userLocationRepository.Delete(record);
        }

        public bool IsInRetrictedAccessGroupLocations(PropertyPart p, UserPart user)
        {
            if (p == null || p.UserGroup == null || user == null) return false;

            // không bị giới hạn khu vực
            if (!_userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.RetrictedAccessGroupProperties == true && a.LocationProvincePartRecord != null).Any())
            {
                return true;
            }

            List<int> retrictedAccessLocationProvinceIds = GetUserRetrictedAccessGroupLocationProvinceIds(user);
            List<int> retrictedAccessLocationDistrictIds = GetUserRetrictedAccessGroupLocationDistrictIds(user);
            List<int> retrictedAccessLocationWardIds = GetUserRetrictedAccessGroupLocationWardIds(user);

            int provinceId = p.Province != null ? p.Province.Id : -1;
            int districtId = p.District != null ? p.District.Id : -1;
            int wardId = p.Ward != null ? p.Ward.Id : -1;

            if ((retrictedAccessLocationProvinceIds.Count > 0 && retrictedAccessLocationProvinceIds.Contains(provinceId)) ||
                (retrictedAccessLocationDistrictIds.Count > 0 && retrictedAccessLocationDistrictIds.Contains(districtId)) ||
                (retrictedAccessLocationWardIds.Count > 0 && retrictedAccessLocationWardIds.Contains(wardId)))
                return true;

            return false;
        }

        // RetrictedAccessGroupLocations
        public List<UserLocationRecord> GetUserRetrictedAccessGroupLocations(UserPart user)
        {
            return _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.RetrictedAccessGroupProperties == true && a.LocationProvincePartRecord != null).ToList();
        }

        // RetrictedAccessGroupProvinces
        public List<int> GetUserRetrictedAccessGroupLocationProvinceIds(UserPart user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> provinces =
                _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.RetrictedAccessGroupProperties == true
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord == null &&
                                                   a.LocationWardPartRecord == null).ToList();
            if (provinces.Any())
                result = provinces.Select(a => a.LocationProvincePartRecord.Id).ToList();
            return result;
        }

        // RetrictedAccessGroupDistricts
        public List<int> GetUserRetrictedAccessGroupLocationDistrictIds(UserPart user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> districts =
                _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.RetrictedAccessGroupProperties == true
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord != null &&
                                                   a.LocationWardPartRecord == null).ToList();
            if (districts.Any())
                result = districts.Select(a => a.LocationDistrictPartRecord.Id).ToList();
            return result;
        }

        // RetrictedAccessGroupWards
        public List<int> GetUserRetrictedAccessGroupLocationWardIds(UserPart user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> wards =
                _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.RetrictedAccessGroupProperties == true
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord != null &&
                                                   a.LocationWardPartRecord != null).ToList();
            if (wards.Any()) result = wards.Select(a => a.LocationWardPartRecord.Id).ToList();
            return result;
        }

        // Provinces
        public List<int> GetUserLocationProvinceIds(UserPart user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> provinces =
                _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.EnableAccessProperties == true
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord == null &&
                                                   a.LocationWardPartRecord == null).ToList();
            if (provinces.Any())
                result = provinces.Select(a => a.LocationProvincePartRecord.Id).ToList();
            return result;
        }

        // Districts
        public List<int> GetUserLocationDistrictIds(UserPart user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> districts =
                _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.EnableAccessProperties == true
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord != null &&
                                                   a.LocationWardPartRecord == null).ToList();
            if (districts.Any())
                result = districts.Select(a => a.LocationDistrictPartRecord.Id).ToList();
            return result;
        }

        // Wards
        public List<int> GetUserLocationWardIds(UserPart user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> wards =
                _userLocationRepository.Fetch(a => a.UserPartRecord.Id == user.Id && a.EnableAccessProperties == true
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord != null &&
                                                   a.LocationWardPartRecord != null).ToList();
            if (wards.Any()) result = wards.Select(a => a.LocationWardPartRecord.Id).ToList();
            return result;
        }

        public bool IsInUserLocations(PropertyPart p, UserPart user)
        {
            if (p == null || user == null) return false;

            List<int> userLocationProvinceIds = GetUserLocationProvinceIds(user);
            List<int> userLocationDistrictIds = GetUserLocationDistrictIds(user);
            List<int> userLocationWardIds = GetUserLocationWardIds(user);

            int provinceId = p.Province != null ? p.Province.Id : -1;
            int districtId = p.District != null ? p.District.Id : -1;
            int wardId = p.Ward != null ? p.Ward.Id : -1;

            if (userLocationProvinceIds.Contains(provinceId) ||
                userLocationDistrictIds.Contains(districtId) ||
                userLocationWardIds.Contains(wardId))
                return true;

            return false;
        }

        public IEnumerable<UserLocationRecord> GetAgencyUserLocations()
        {
            var hostname = _hostNameService.GetHostNameSite();
            var hostPart = _hostNameService.GetHostNamePart(hostname);

            return _cacheManager.Get("Agencies_" + hostname, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Agencies_" + hostname + "_Changed"));

                List<UserLocationRecord> userLocation = new List<UserLocationRecord>();
                if (hostPart != null)
                {
                    if (hostPart.CssClass == "host-name-main")
                        userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                            && (a.UserGroupRecord == null || a.UserGroupRecord.Id == hostPart.Id))
                            .ToList();
                    else
                        userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                            && a.UserGroupRecord.Id == hostPart.Id)
                            .ToList();
                }
                return userLocation;
            });
        }

        public IEnumerable<UserLocationRecord> GetAgencyUserLocationProvinces(int? provinceId)
        {
            var hostname = _hostNameService.GetHostNameSite();
            var hostPart = _hostNameService.GetHostNamePart(hostname);

            return _cacheManager.Get("Agencies_" + hostname + "_" + provinceId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Agencies_" + hostname + "_Changed"));
                ctx.Monitor(_signals.When("Agencies_" + hostname + "_" + provinceId + "_Changed"));

                List<UserLocationRecord> userLocation = new List<UserLocationRecord>();
                if (hostPart != null)
                {
                    if (hostPart.CssClass == "host-name-main")
                        userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                            && (a.UserGroupRecord == null || a.UserGroupRecord.Id == hostPart.Id)
                            && a.LocationProvincePartRecord != null && a.LocationProvincePartRecord.Id == provinceId
                            )
                            .ToList();
                    else
                        userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                            && a.UserGroupRecord.Id == hostPart.Id
                            && a.LocationProvincePartRecord != null && a.LocationProvincePartRecord.Id == provinceId
                            )
                            .ToList();
                }
                return userLocation;
            });
        }

        public IEnumerable<UserLocationRecord> GetAgencyUserLocationProvinces(int? provinceId, int[] districtIds)
        {
            if (!provinceId.HasValue) provinceId = 0;

            var hostname = _hostNameService.GetHostNameSite();
            var hostPart = _hostNameService.GetHostNamePart(hostname);

            if (districtIds != null)
                return _cacheManager.Get("Agencies_" + hostname + "_" + provinceId + "_" + districtIds, ctx =>
                {
                    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                    ctx.Monitor(_signals.When("Agencies_" + hostname + "_Changed"));
                    ctx.Monitor(_signals.When("Agencies_" + hostname + "_" + provinceId + "_" + districtIds + "_Changed"));

                    List<UserLocationRecord> userLocation = new List<UserLocationRecord>();
                    if (hostPart != null)
                    {
                        if (hostPart.CssClass == "host-name-main")
                            userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                                && (a.UserGroupRecord == null || a.UserGroupRecord.Id == hostPart.Id) &&
                                    (
                                        //(a.LocationDistrictPartRecord == null && a.LocationProvincePartRecord != null && a.LocationProvincePartRecord.Id == provinceId)
                                        //||
                                        (a.LocationDistrictPartRecord != null && districtIds.Contains(a.LocationDistrictPartRecord.Id))
                                    )
                                )
                                .ToList();
                        else
                            userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                                && a.UserGroupRecord.Id == hostPart.Id &&
                                    (
                                        //(a.LocationDistrictPartRecord == null && a.LocationProvincePartRecord != null && a.LocationProvincePartRecord.Id == provinceId)
                                        //||
                                        (a.LocationDistrictPartRecord != null && districtIds.Contains(a.LocationDistrictPartRecord.Id))
                                    )
                                )
                                .ToList();
                    }
                    return userLocation;
                });
            return new List<UserLocationRecord>();
        }

        public IEnumerable<UserLocationRecord> GetAgencyUserLocationProvinces(int? provinceId, int? districtId)
        {
            var hostname = _hostNameService.GetHostNameSite();
            var hostPart = _hostNameService.GetHostNamePart(hostname);

            if (!provinceId.HasValue) provinceId = 0;

            if (districtId != null)
                return _cacheManager.Get("Agency_" + hostname + "_" + provinceId + "_" + districtId, ctx =>
                {
                    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                    ctx.Monitor(_signals.When("Agencies_" + hostname + "_Changed"));
                    ctx.Monitor(_signals.When("Agency_" + hostname + "_" + provinceId + "_" + districtId + "_Changed"));

                    List<UserLocationRecord> userLocation = new List<UserLocationRecord>();
                    if (hostPart != null)
                    {
                        if (hostPart.CssClass == "host-name-main")
                            userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                                && (a.UserGroupRecord == null || a.UserGroupRecord.Id == hostPart.Id)
                                && a.LocationProvincePartRecord.Id == provinceId &&
                                a.LocationDistrictPartRecord != null && a.LocationDistrictPartRecord.Id == districtId)
                                .ToList();
                        else
                            userLocation = userLocation = _userLocationRepository.Fetch(a => a.EnableIsAgencies && (a.EndDateAgencing >= DateTime.Now || a.EndDateAgencing == null)
                                && a.UserGroupRecord.Id == hostPart.Id
                                && a.LocationProvincePartRecord.Id == provinceId
                                && a.LocationDistrictPartRecord != null && a.LocationDistrictPartRecord.Id == districtId)
                                .ToList();
                    }
                    return userLocation;
                });
            return new List<UserLocationRecord>();
        }

        public void ClearUserLocationCache()
        {
            var hostname = _hostNameService.GetHostNameSite();
            _signals.Trigger("Agencies_" + hostname + "_Changed");
        }

        public void ClearUserLocationCache(int? provinceId)
        {
            var hostname = _hostNameService.GetHostNameSite();
            _signals.Trigger("Agencies_" + hostname + "_" + provinceId + "_Changed");
        }

        public IEnumerable<UserUpdateProfileRecord> GetUserUpdateProfiles(int[] userPartIds)
        {
            if (userPartIds != null)
                return
                    _userUpdateRepository.Fetch(a => userPartIds.Contains(a.Id))
                        .ToList();
            return new List<UserUpdateProfileRecord>();
        }

        public IEnumerable<UserUpdateProfileRecord> GetUserUpdateProfiles(int? userPartId)
        {
            if (!userPartId.HasValue) userPartId = 0;
            return _userUpdateRepository.Fetch(a => a.Id == userPartId)
                        .ToList();
        }

        public IEnumerable<UserLocationRecord> GetUserLocations()
        {
            return _userLocationRepository.Table.ToList();
        }

        public IEnumerable<UserLocationRecord> GetUserLocations(UserPartRecord user)
        {
            return _userLocationRepository.Fetch(r => r.UserPartRecord == user).ToList();
        }

        #endregion

        #region User Enable Edit Locations

        // Provinces

        public IEnumerable<LocationProvincePart> GetUserEnableEditLocationProvinces(UserPart user)
        {
            IEnumerable<LocationProvincePart> result = new List<LocationProvincePart>();
            IEnumerable<LocationProvincePartRecord> provinces =
                _userLocationRepository.Fetch(
                    a =>
                        a.UserPartRecord == user.Record && a.EnableEditLocations && a.LocationProvincePartRecord != null)
                        .Select(a => a.LocationProvincePartRecord)
                    .ToList();
            if (provinces.Any())
            {
                List<int> ids = provinces.Select(a => a.Id).ToList();
                result =
                    _contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                        .Where(a => ids.Contains(a.Id))
                        .List();
            }
            return result;
        }

        public IEnumerable<LocationDistrictPart> GetUserEnableEditLocationDistricts(UserPart user, int? provinceId)
        {
            IEnumerable<LocationDistrictPart> result = new List<LocationDistrictPart>();

            // Get Provinces that user have permission
            IEnumerable<UserLocationRecord> provinces =
                _userLocationRepository.Fetch(a => a.UserPartRecord == user.Record && a.EnableEditLocations
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationProvincePartRecord.Id == provinceId &&
                                                   a.LocationDistrictPartRecord == null &&
                                                   a.LocationWardPartRecord == null).ToList();

            if (provinces.Any())
            {
                // Enable all districts in province
                result = _addressService.GetDistricts(provinceId);
            }
            else
            {
                // Select districts form User's locations
                IEnumerable<UserLocationRecord> districts =
                    _userLocationRepository.Fetch(a => a.UserPartRecord == user.Record && a.EnableEditLocations
                                                       && a.LocationProvincePartRecord != null &&
                                                       a.LocationProvincePartRecord.Id == provinceId &&
                                                       a.LocationDistrictPartRecord != null).ToList();
                if (districts.Any())
                {
                    List<int> districtIds = districts.Select(a => a.LocationDistrictPartRecord.Id).Distinct().ToList();
                    result = _addressService.GetDistricts().Where(r => districtIds.Contains(r.Id));
                }
            }

            return result;
        }

        public List<int> GetUserEnableEditLocationProvinceIds(UserPartRecord user)
        {
            var result = new List<int>();
            IEnumerable<UserLocationRecord> provinces =
                _userLocationRepository.Fetch(a => a.UserPartRecord == user && a.EnableEditLocations
                                                   && a.LocationProvincePartRecord != null &&
                                                   a.LocationDistrictPartRecord == null &&
                                                   a.LocationWardPartRecord == null).ToList();
            if (provinces.Any())
                result = provinces.Select(a => a.LocationProvincePartRecord.Id).ToList();
            return result;
        }

        //public IEnumerable<LocationWardPartRecord> GetUserEnableEditLocationWards(UserPartRecord user, int? districtId)
        //{
        //    var result = new List<LocationWardPartRecord>();
        //    var wards = _userLocationRepository.Fetch(a => a.UserPartRecord == user && a.EnableEditLocations == true 
        //        && a.LocationDistrictPartRecord != null && a.LocationDistrictPartRecord.Id == districtId && a.LocationWardPartRecord != null);
        //    if (wards != null && wards.Any()) result = wards.Select(a => a.LocationWardPartRecord).Distinct().ToList();
        //    return result;
        //}

        #endregion

        #region Group Roles

        public IEnumerable<RoleRecord> GetUserRoles(UserPart user)
        {
            // Return all Roles
            return _userRolesRepository.Fetch(a => a.UserId == user.Id).Select(a => a.Role).OrderBy(a => a.Name);
        }

        public IEnumerable<RoleRecord> GetGroupRoles(UserGroupPartRecord group)
        {
            var groupAdmin = _contentManager.Get<UserPart>(group.GroupAdminUser.Id);
            List<int> groupAdminRoles = GetUserRoles(groupAdmin).Select(a => a.Id).ToList();
            return
                _roleRepository.Fetch(a => groupAdminRoles.Contains(a.Id) && a.Name.StartsWith("RE"))
                    .OrderBy(a => a.Name);
        }

        public IEnumerable<RoleRecord> GetUserAvailableRoles(UserPart user)
        {
            UserGroupPartRecord jointGroup = GetJointGroup(user.Id);
            if (jointGroup != null && jointGroup.ShortName != "DPH")
                return GetGroupRoles(jointGroup);
            if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner) ||
                Services.Authorizer.Authorize(Permissions.ManageUsers))
                return _roleRepository.Fetch(a => a.Name.StartsWith("RE")).OrderBy(a => a.Name);

            return null;
        }

        #endregion

        #region BuildViewModel

        public UserGroupEditViewModel BuildEditViewModel(UserGroupPart group, string dateFrom, string dateTo)
        {
            int defaultProvinceId = group.DefaultProvince != null ? group.DefaultProvince.Id : 0;
            int defaultDistrictId = group.DefaultDistrict != null ? group.DefaultDistrict.Id : 0;

            return new UserGroupEditViewModel
            {
                Group = group,
                GroupAdminUserId = group.GroupAdminUser.Id,
                //Users = GetAvailableGroupAdminUsers(group.Record),
                //UsersAvailable = GetAvailableGroupMemberUsers(group.Record),

                // Provinces
                Provinces = _addressService.GetSelectListProvinces(),
                DefaultProvinceId = defaultProvinceId,

                // Districts
                Districts = _addressService.GetSelectListDistricts(defaultProvinceId),
                DefaultDistrictId = defaultDistrictId,

                // Users
                GroupUsers = GetGroupUsersEntries(group.Id, dateFrom, dateTo),
                Roles = _roleRepository.Table.ToList(),
                DateFrom =
                    String.IsNullOrEmpty(dateFrom)
                        ? DateExtension.GetStartOfCurrentMonth().ToString("dd/MM/yyyy")
                        : dateFrom,
                DateTo =
                    String.IsNullOrEmpty(dateTo) ? DateExtension.GetEndOfCurrentMonth().ToString("dd/MM/yyyy") : dateTo,
            };
        }

        #endregion

        public string GetHostNameByUser(UserPart user)
        {
            var belongGroup = GetBelongGroup(user.Id);
            string hostname = _hostNameService.GetHostNameById(belongGroup.Id).DomainName ?? _hostNameService.GetHostNameById(belongGroup.Id).ShortName;// _hostNameService.get

            return !string.IsNullOrEmpty(hostname) ? hostname : "dinhgianhadat.vn";
        }
    }
}