using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace RealEstate.Admin.Controllers
{
    [Themed]
    public class AutoLoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;

        public AutoLoginController(
            IOrchardServices services,
            IAuthenticationService authenticationService,
            IRepository<UserRolesPartRecord> userRolesRepository,
            IRoleService roleService,
            IMembershipService membershipService)
        {
            Services = services;
            _authenticationService = authenticationService;
            _userRolesRepository = userRolesRepository;
            _roleService = roleService;
            _membershipService = membershipService;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index()
        {
            IUser currentUser = _authenticationService.GetAuthenticatedUser();

            if (currentUser == null)
            {
                if (!LoginUsingWindowsCredentials())
                {
                    // login failed, redirect to normal logon page
                    return new RedirectResult("~/Users/Account/Logon");
                }
            }
            return View("Index");
        }

        private bool LoginUsingWindowsCredentials()
        {
            bool userIsValid = false;

            // check if we can login using Windows credentials
            string userName = Request.ServerVariables["LOGON_USER"];
            Services.Notifier.Information(T("LOGON_USER {0}", userName));

            if (!String.IsNullOrEmpty(userName))
            {
                string email;
                IEnumerable<string> roles;
                if (GetUserDetails(userName, out email, out roles))
                {
                    IUser user = _membershipService.GetUser(userName) ?? _membershipService.CreateUser(
                        new CreateUserParams(
                            userName,
                            "NotUsed!",
                            email,
                            null,
                            null,
                            true
                            )
                        );

                    UpdateUserRoles(user, roles);

                    // sign in
                    _authenticationService.SignIn(user, false);
                    userIsValid = true;
                }
            }
            return userIsValid;
        }

        private static bool GetUserDetails(string fullUserName, out string email, out IEnumerable<string> roles)
        {
            string domain = SubStringBefore(fullUserName, "\\");
            string userName = SubstringAfter(fullUserName, "\\");

            // default values
            email = null;
            roles = null;

            using (var ctx = new PrincipalContext(ContextType.Domain, domain))
            {
                using (UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(ctx, userName))
                {
                    if (userPrincipal == null)
                    {
                        return false;
                    }

                    email = userPrincipal.EmailAddress;
                    // NOTE: assuming "CMS " prefix on role names here
                    roles =
                        userPrincipal.GetGroups()
                            .Where(g => g.Name.StartsWith("CMS "))
                            .Select(g => SubstringAfter(g.Name, "CMS "))
                            .ToList();

                    return true;
                }
            }
        }

        private static string SubStringBefore(string source, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            int index = compareInfo.IndexOf(source, value, CompareOptions.Ordinal);
            if (index < 0)
            {
                //No such substring
                return string.Empty;
            }
            return source.Substring(0, index);
        }

        public static string SubstringAfter(string source, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return source;
            }
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            int index = compareInfo.IndexOf(source, value, CompareOptions.Ordinal);
            if (index < 0)
            {
                //No such substring
                return string.Empty;
            }
            return source.Substring(index + value.Length);
        }

        private void UpdateUserRoles(IUser user, IEnumerable<string> roles)
        {
            IEnumerable<UserRolesPartRecord> currentUserRoleRecords =
                _userRolesRepository.Fetch(x => x.UserId == user.Id).ToList();
            IEnumerable<RoleRecord> currentRoleRecords = currentUserRoleRecords.Select(x => x.Role);
            IEnumerable<RoleRecord> targetRoleRecords =
                _roleService.GetRoles().Where(x => roles.Contains(x.Name)).ToList();

            foreach (RoleRecord addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x)))
            {
                _userRolesRepository.Create(new UserRolesPartRecord {UserId = user.Id, Role = addingRole});
            }

            foreach (
                UserRolesPartRecord removingRole in
                    currentUserRoleRecords.Where(x => !targetRoleRecords.Contains(x.Role)))
            {
                _userRolesRepository.Delete(removingRole);
            }
        }
    }
}