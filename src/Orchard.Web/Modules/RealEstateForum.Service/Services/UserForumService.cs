using System;
using System.Collections.Generic;
using System.Linq;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Users.Models;
﻿using Contrib.OnlineUsers.Models;
using RealEstate.Services;
using RealEstateForum.Service.ViewModels;

using Contrib.OnlineUsers.ViewModels;

namespace RealEstateForum.Service.Services
{
    public interface IUserForumService : IDependency
    {
        IContentQuery<MembershipPart, MembershipPartRecord> GetListUserOnline();
        IList<OnlineUsersEntry> BuildListUserOnlineEntry(IEnumerable<MembershipPart> results);
        UserUpdateProfileOptions GetProfileUserPart(UserUpdateProfileOptions options);
        UserUpdateProfilePart GetUserUpdatePart(int userId);
    }
    public class UserForumService : IUserForumService
    {
        private readonly IContentManager _contentManager;
        private readonly IUserPersonalService _userRealEstateService;

        public UserForumService(
            IOrchardServices services,
            IContentManager contentManager,
            IUserPersonalService userRealEstateService)
        {
            Services = services;
            _contentManager = contentManager;
            _userRealEstateService = userRealEstateService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public IContentQuery<MembershipPart, MembershipPartRecord> GetListUserOnline()
        {
            var lastActiveTime = DateTime.UtcNow.AddMinutes(-15);
            return Services.ContentManager.Query<MembershipPart, MembershipPartRecord>().Where(record => record.LastActive >= lastActiveTime);
        }
		public UserUpdateProfileOptions GetProfileUserPart(UserUpdateProfileOptions options)
        {
            var result = Services.ContentManager.Get(Services.WorkContext.CurrentUser.Id);
            var userPart = result.As<UserPart>();
            var userUpdatePart = result.As<UserUpdateProfilePart>();
            options.UserPart = userPart;
            options.UserUpdateProfilePart = userUpdatePart;
            return options;
        }
        public UserUpdateProfilePart GetUserUpdatePart(int userId)
        {          
            var result = Services.ContentManager.Get<UserUpdateProfilePart>(userId);
            return result;
        }
        public IList<OnlineUsersEntry> BuildListUserOnlineEntry(IEnumerable<MembershipPart> results)
        {
            return results
                .Select(o => new OnlineUsersEntry
                {
                    UserName = Services.ContentManager.Get<UserPart>(o.Id).UserName,
                    id = o.Id,
                    Email = Services.ContentManager.Get<UserPart>(o.Id).Email,
                    UserDisplayName = _userRealEstateService.GetUserNameOrDisplayName(o.Id)
                }).ToList();
        }
    }
}