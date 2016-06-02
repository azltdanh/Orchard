using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Contrib.OnlineUsers.Models;
using Orchard.Localization;
using Orchard.Caching;
using Orchard.Services;
using Orchard.Logging;
using Orchard;

namespace RealEstate.FrontEnd.Services
{
    public interface IOnlineUsersService : IDependency
    {
        //OnlineUsers
        int GetOnlineUsers();
        //Check user online
        bool CheckIsUserOnline(int userId);
        IEnumerable<MembershipPart> GetListOnlineUsers();
    }

    public class OnlineUsersService : IOnlineUsersService
    {
        // Init

        #region Init

        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;

        public OnlineUsersService(
            IContentManager contentManager,
            ICacheManager cacheManager,
            IClock clock)
        {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _clock = clock;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #endregion

        public int GetOnlineUsers()
        {
            var thresholdTime = DateTime.Now.AddMinutes(-15);
            var onlineUsers = _contentManager.Query<MembershipPart, MembershipPartRecord>().Where(record => record.LastActive >= thresholdTime).List();
            return onlineUsers.Count();
        }
        public bool CheckIsUserOnline(int userId)
        {
            var lastActiveTime = DateTime.UtcNow.AddMinutes(-15);
            return _contentManager.Query<MembershipPart, MembershipPartRecord>()
                .Where(record => record.Id == userId && record.LastActive >= lastActiveTime).List().Any() ? true : false;

        }
        public IEnumerable<MembershipPart> GetListOnlineUsers()
        {
            var lastActiveTime = DateTime.UtcNow.AddMinutes(-15);
            return _contentManager.Query<MembershipPart, MembershipPartRecord>().Where(record => record.LastActive >= lastActiveTime).List();
        }

    }
}
