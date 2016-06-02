using System.Collections.Generic;
using System.Linq;
using Contrib.OnlineUsers.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Models;

namespace RealEstate.Services
{
    public interface IUserPersonalService : IDependency
    {
        UserUpdateProfilePart GetUserPersonal(int? userId);
        IEnumerable<UserUpdateProfileRecord> GetUserUpdates(string nameAgency);
        IEnumerable<UserPartRecord> GetUserParts(string username);
        UserUpdateProfilePart GetUserPersonalPart();
        string GetUserNameOrDisplayName(int userId);
        string GetUserAvatar(int userId);

        bool VerifyExistEmail(string email);
    }

    public class UserPersonalService : IUserPersonalService
    {
        private readonly IContentManager _contentManager;

        public UserPersonalService(IContentManager contentManager,
            IOrchardServices services)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public UserUpdateProfilePart GetUserPersonal(int? userId)
        {
            if (!userId.HasValue)
                return null;
            var userUpdate = _contentManager.Get((int) userId).As<UserUpdateProfilePart>();
            return userUpdate;
        }

        public IEnumerable<UserUpdateProfileRecord> GetUserUpdates(string nameAgency)
        {
            return
                _contentManager.Query<UserUpdateProfilePart, UserUpdateProfileRecord>()
                    .Where(a => a.DisplayName.Contains(nameAgency))
                    .List()
                    .Select(a => a.Record);
        }

        public IEnumerable<UserPartRecord> GetUserParts(string username)
        {
            return
                _contentManager.Query<UserPart, UserPartRecord>()
                    .Where(a => a.UserName.Contains(username))
                    .List()
                    .Select(a => a.Record);
        }

        public UserUpdateProfilePart GetUserPersonalPart()
        {
            IUser currentUser = Services.WorkContext.CurrentUser;
            return currentUser != null ? _contentManager.Get(currentUser.Id).As<UserUpdateProfilePart>() : null;
        }

        public string GetUserNameOrDisplayName(int userId)
        {
            var userProfile = _contentManager.Get(userId).As<UserUpdateProfilePart>();
            if (userProfile == null) return null;

            return !string.IsNullOrEmpty(userProfile.DisplayName)
                ? userProfile.DisplayName
                : userProfile.As<UserPart>().UserName.Split('@')[0];
        }

        public string GetUserAvatar(int userId)
        {
            string getAvatar = GetUserPersonal(userId).Avatar;

            return !string.IsNullOrEmpty(getAvatar) ? getAvatar : "/Themes/Bootstrap/Styles/images/avatar-default.jpg";
        }

        public bool VerifyExistEmail(string email)
        {
            string emailLower = email.ToLower();

            return _contentManager.Query<UserPart, UserPartRecord>().Where(r => r.Email == emailLower).Count() > 0;
        }
    }
}