using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Users.Events;
using Orchard.Users.Models;
using Orchard.Messaging.Services;
using System.Collections.Generic;
using Orchard.Services;
using Orchard;

namespace Piedone.Facebook.Suite.Connect.Services
{
    public interface IFacebookMembershipSerivce : IDependency
    {
        //IUser FaceBookCreateUser(string userName, string password, string email);
        //IUser FacebookGetUserByEmail(string email);
        //
        //IUser FaceCreateUser(CreateUserParams createUserParams);
        //MembershipSettings FaceGetSettings();
        //IUser FaceGetUser(string username);
        //IUser FaceGetUserByEmail(string email);//
        //void FaceSetPassword(IUser user, string password);
        //IUser FaceValidateUser(string userNameOrEmail, string password);
    }
    public class FacebookMembershipSerivce : IFacebookMembershipSerivce
    {
        //private readonly IMembershipFrontEndService _membershipfrontendService;

        private readonly IOrchardServices _orchardServices;
        private readonly IMessageManager _messageManager;
        private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
        private readonly IEncryptionService _encryptionService;
        //private readonly IPageForumService _pageforumService;

        public FacebookMembershipSerivce(IOrchardServices orchardServices,
            IMessageManager messageManager,
            IEnumerable<IUserEventHandler> userEventHandlers,
            IClock clock,
            //IPageForumService pageforumService,
            IEncryptionService encryptionService)
        {
            _orchardServices = orchardServices;
            _messageManager = messageManager;
            _userEventHandlers = userEventHandlers;
            _encryptionService = encryptionService;
            //_pageforumService = pageforumService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region
        public MembershipSettings FaceGetSettings()
        {
            var settings = new MembershipSettings();
            // accepting defaults
            return settings;
        }


        //public IUser FaceCreateUser(CreateUserParams createUserParams)
        //{
        //    Logger.Information("CreateUser {0} {1}", createUserParams.Username, createUserParams.Email);

        //    var registrationSettings = _orchardServices.WorkContext.CurrentSite.As<RegistrationSettingsPart>();

        //    var user = _orchardServices.ContentManager.New<UserPart>("User");

        //    user.Record.UserName = createUserParams.Username;
        //    user.Record.Email = createUserParams.Email;
        //    user.Record.NormalizedUserName = createUserParams.Username.ToLowerInvariant();
        //    user.Record.HashAlgorithm = "SHA1";
        //    SetPassword(user.Record, createUserParams.Password);

            
        //    if (registrationSettings != null)
        //    {
        //        user.Record.RegistrationStatus = registrationSettings.UsersAreModerated ? UserStatus.Pending : UserStatus.Approved;
        //        user.Record.EmailStatus = registrationSettings.UsersMustValidateEmail ? UserStatus.Pending : UserStatus.Approved;
        //    }

        //    if (createUserParams.IsApproved)
        //    {
        //        user.Record.RegistrationStatus = UserStatus.Approved;
        //        user.Record.EmailStatus = UserStatus.Approved;
        //    }

        //    var userContext = new UserContext { User = user, Cancel = false };
        //    foreach (var userEventHandler in _userEventHandlers)
        //    {
        //        userEventHandler.Creating(userContext);
        //    }

        //    if (userContext.Cancel)
        //    {
        //        return null;
        //    }

        //    _orchardServices.ContentManager.Create(user);
        //    //_orchardServices.ContentManager.Create(homepage);

        //    //homepage.Record.UserForum = _pageforumService.GetUser(user.Id);
        //    //homepage.Record.ParentPageId = homepage.Id;

        //    foreach (var userEventHandler in _userEventHandlers)
        //    {
        //        userEventHandler.Created(userContext);
        //        if (user.RegistrationStatus == UserStatus.Approved)
        //        {
        //            userEventHandler.Approved(user);
        //        }
        //    }

        //    if (registrationSettings != null && registrationSettings.UsersAreModerated && registrationSettings.NotifyModeration && !createUserParams.IsApproved)
        //    {
        //        var usernames = String.IsNullOrWhiteSpace(registrationSettings.NotificationsRecipients)
        //                            ? new string[0]
        //                            : registrationSettings.NotificationsRecipients.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //        foreach (var userName in usernames)
        //        {
        //            if (String.IsNullOrWhiteSpace(userName))
        //            {
        //                continue;
        //            }
        //            var recipient = FaceGetUser(userName);
        //            if (recipient != null)
        //                _messageManager.Send(recipient.ContentItem.Record, MessageTypes.Moderation, "email", new Dictionary<string, string> { { "UserName", createUserParams.Username }, { "Email", createUserParams.Email } });
        //        }
        //    }

        //    return user;
        //}

        public IUser FaceGetUser(string username)
        {
            var lowerName = username == null ? "" : username.ToLowerInvariant();

            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).Slice(1).FirstOrDefault();
        }
        // add by kenji nguyen
        public IUser FaceGetUserByEmail(string email)
        {
            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == email).Slice(1).FirstOrDefault();
        }
        public IUser FaceValidateUser(string userNameOrEmail, string password)
        {
            var lowerName = userNameOrEmail == null ? "" : userNameOrEmail.ToLowerInvariant();

            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).Slice(1).FirstOrDefault();

            if (user == null)
                user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerName).Slice(1).FirstOrDefault();

            if (user == null || ValidatePassword(user.As<UserPart>().Record, password) == false)
                return null;

            if (user.EmailStatus != UserStatus.Approved)
                return null;

            if (user.RegistrationStatus != UserStatus.Approved)
                return null;

            return user;
        }
        public void FaceSetPassword(IUser user, string password)
        {
            if (!user.Is<UserPart>())
                throw new InvalidCastException();

            var userRecord = user.As<UserPart>().Record;
            SetPassword(userRecord, password);
        }


        void SetPassword(UserPartRecord partRecord, string password)
        {
            switch (FaceGetSettings().PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    SetPasswordClear(partRecord, password);
                    break;
                case MembershipPasswordFormat.Hashed:
                    SetPasswordHashed(partRecord, password);
                    break;
                case MembershipPasswordFormat.Encrypted:
                    SetPasswordEncrypted(partRecord, password);
                    break;
                default:
                    throw new ApplicationException(T("Unexpected password format value").ToString());
            }
        }

        private bool ValidatePassword(UserPartRecord partRecord, string password)
        {
            // Note - the password format stored with the record is used
            // otherwise changing the password format on the site would invalidate
            // all logins
            switch (partRecord.PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    return ValidatePasswordClear(partRecord, password);
                case MembershipPasswordFormat.Hashed:
                    return ValidatePasswordHashed(partRecord, password);
                case MembershipPasswordFormat.Encrypted:
                    return ValidatePasswordEncrypted(partRecord, password);
                default:
                    throw new ApplicationException("Unexpected password format value");
            }
        }

        private static void SetPasswordClear(UserPartRecord partRecord, string password)
        {
            partRecord.PasswordFormat = MembershipPasswordFormat.Clear;
            partRecord.Password = password;
            partRecord.PasswordSalt = null;
        }

        private static bool ValidatePasswordClear(UserPartRecord partRecord, string password)
        {
            return partRecord.Password == password;
        }

        private static void SetPasswordHashed(UserPartRecord partRecord, string password)
        {

            var saltBytes = new byte[0x10];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetBytes(saltBytes);
            }

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            byte[] hashBytes;
            using (var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm))
            {
                hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            }

            partRecord.PasswordFormat = MembershipPasswordFormat.Hashed;
            partRecord.Password = Convert.ToBase64String(hashBytes);
            partRecord.PasswordSalt = Convert.ToBase64String(saltBytes);
        }

        private static bool ValidatePasswordHashed(UserPartRecord partRecord, string password)
        {

            var saltBytes = Convert.FromBase64String(partRecord.PasswordSalt);

            var passwordBytes = Encoding.Unicode.GetBytes(password);

            var combinedBytes = saltBytes.Concat(passwordBytes).ToArray();

            byte[] hashBytes;
            using (var hashAlgorithm = HashAlgorithm.Create(partRecord.HashAlgorithm))
            {
                hashBytes = hashAlgorithm.ComputeHash(combinedBytes);
            }

            return partRecord.Password == Convert.ToBase64String(hashBytes);
        }

        private void SetPasswordEncrypted(UserPartRecord partRecord, string password)
        {
            partRecord.Password = Convert.ToBase64String(_encryptionService.Encode(Encoding.UTF8.GetBytes(password)));
            partRecord.PasswordSalt = null;
            partRecord.PasswordFormat = MembershipPasswordFormat.Encrypted;
        }

        private bool ValidatePasswordEncrypted(UserPartRecord partRecord, string password)
        {
            return String.Equals(password, Encoding.UTF8.GetString(_encryptionService.Decode(Convert.FromBase64String(partRecord.Password))), StringComparison.Ordinal);
        }
        #endregion
    }
}