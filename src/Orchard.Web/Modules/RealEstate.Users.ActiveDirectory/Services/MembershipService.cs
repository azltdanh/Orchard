using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using Orchard;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Events;
using Orchard.Roles.Models;
using Orchard.Roles.Services;

namespace RealEstate.Users.ActiveDirectory.Services
{
    [OrchardSuppressDependency("Orchard.Users.Services.MembershipService")]
	public class MembershipService : IMembershipService
	{
		private readonly IOrchardServices _orchardServices;
		private readonly IMessageManager _messageManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly IRoleService _roleService;
		private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
		private readonly IEncryptionService _encryptionService;

		private readonly Orchard.Users.Services.MembershipService _baseMembershipService;

        private readonly IActiveDirectoryService _activeDirectoryService;
		
		public MembershipService(
			
			IOrchardServices orchardServices,
			IMessageManager messageManager,
            IAuthenticationService authenticationService,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRoleService roleService,
			IEnumerable<IUserEventHandler> userEventHandlers,
            IEncryptionService encryptionService,
            IActiveDirectoryService activeDirectoryService,
			IClock clock)
		{
			_orchardServices = orchardServices;
			_messageManager = messageManager;
            _authenticationService = authenticationService;
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
			_userEventHandlers = userEventHandlers;
			_encryptionService = encryptionService;

            //_baseMembershipService = new Orchard.Users.Services.MembershipService(orchardServices, messageManager, userEventHandlers, clock, encryptionService);

            _activeDirectoryService = activeDirectoryService;

			Logger = NullLogger.Instance;
			T = NullLocalizer.Instance;
		}

		public ILogger Logger { get; set; }
		public Localizer T { get; set; }

		#region IMembershipService Members

		public MembershipSettings GetSettings()
		{
            var settings = _baseMembershipService.GetSettings();
            //settings.MinRequiredPasswordLength = 6;
            return settings;
		}

		public IUser CreateUser(CreateUserParams createUserParams)
		{
            try
            {
            // Create UserPrincipal
            var userPrincipal = _activeDirectoryService.CreateDomainUserPrincipal(createUserParams.Username, createUserParams.Email, createUserParams.Password);
            
            // Return local User
            return GetLocalUser(userPrincipal);
            }
            catch
            {
                return _baseMembershipService.CreateUser(createUserParams);
            }
		}

		public IUser GetUser(string username)
		{
            try
            {
                var userPrincipal = _activeDirectoryService.GetDomainUserPrincipal(username);

                if (userPrincipal == null)
                    return _baseMembershipService.GetUser(username);
                else
                    return GetLocalUser(userPrincipal);
            }
            catch
            {
                return _baseMembershipService.GetUser(username);
            }
		}

		public IUser ValidateUser(string userNameOrEmail, string password)
		{
            try
            {
                var userPrincipal = _activeDirectoryService.GetDomainUserPrincipal(userNameOrEmail);
                if (userPrincipal != null)
                {
                    // This user already exists on AD
                    if (userPrincipal.Context.ValidateCredentials(userPrincipal.SamAccountName, password))
                        return GetLocalUser(userPrincipal);
                }
                else
                {
                    // This user does not exists on AD
                    var localUser = _baseMembershipService.ValidateUser(userNameOrEmail, password);
                    if (localUser != null)
                    {
                        // Create new account on AD
                        userPrincipal = _activeDirectoryService.CreateDomainUserPrincipal(localUser.UserName, localUser.Email, password);
                    }
                    return localUser;
                }

                return null;
            }
            catch
            {
                return _baseMembershipService.ValidateUser(userNameOrEmail, password);
            }
		}

		public void SetPassword(IUser user, string password)
		{
            try
            {
                //HACK: probably need a better way to figure out domain membership
                //if (user.UserName.Contains("\\")) throw new InvalidOperationException("You cannot change the password for a domain user");

                // Solution: Change IIS ApllicationPoolIdentify to use an Domain User which have permission to SetPassword (join group Administrators)
                var userPrincipal = _activeDirectoryService.GetDomainUserPrincipal(user.UserName);

                if (userPrincipal != null)
                {
                    userPrincipal.SetPassword(password);
                    userPrincipal.SetPassword(password); // This to prevent old password still work for 15 minutes after SetPassword
                    userPrincipal.UnlockAccount();
                    userPrincipal.Save();

                    _baseMembershipService.SetPassword(user, password);
                }
            }
            catch
            {
                _baseMembershipService.SetPassword(user, password);
            }
		}

		#endregion

        private void UpdateUserRoles(IUser user, IEnumerable<string> roles)
        {
            var currentUserRoleRecords = _userRolesRepository.Fetch(x => x.UserId == user.Id);
            var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
            var targetRoleRecords = _roleService.GetRoles().Where(x => roles.Contains(x.Name));

            foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x)))
            {
                _userRolesRepository.Create(new UserRolesPartRecord { UserId = user.Id, Role = addingRole });
            }

            foreach (var removingRole in currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role)))
            {
                _userRolesRepository.Delete(removingRole);
            }
        }

        /// <summary>
        /// Gets the local user, or create new if not exists
        /// </summary>
        /// <returns>Returns the IUser object</returns>
        public IUser GetLocalUser(UserPrincipal userPrincipal)
        {
            if (userPrincipal == null) return null;

            string normalizedName = userPrincipal.SamAccountName;
            bool isEnable = userPrincipal.Enabled == true;

            var localUser = _baseMembershipService.GetUser(normalizedName);

            if (localUser == null)
            {
                localUser = _baseMembershipService.CreateUser(new CreateUserParams(
                                        normalizedName,
                                        Guid.NewGuid().ToString(),
                                        userPrincipal.EmailAddress,
                                        Guid.NewGuid().ToString(),
                                        Guid.NewGuid().ToString(),
                                        false));
            }

            // Update User roles
            //var roles = userPrincipal.GetGroups().Where(g => g.Name.StartsWith("CMS ")).Select(g => SubstringAfter(g.Name, "CMS ")).ToList();
            //UpdateUserRoles(localUser, roles);

            return localUser;
        }

    }
}
