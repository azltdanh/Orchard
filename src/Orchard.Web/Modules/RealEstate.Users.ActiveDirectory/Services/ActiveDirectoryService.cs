using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Orchard;
using Orchard.Data;
using RealEstate.Users.ActiveDirectory.Models;
using System.DirectoryServices.AccountManagement;

namespace RealEstate.Users.ActiveDirectory.Services
{

    public interface IActiveDirectoryService : IDependency
    {
        PrincipalContext GetDefaultPrincipalContext();
        PrincipalContext GetDomainPrincipalContext(string domainName);

        UserPrincipal CreateDomainUserPrincipal(string userName, string email, string password);
        UserPrincipal CreateDomainUserPrincipal(PrincipalContext context, string userName, string email, string password);

        UserPrincipal GetDomainUserPrincipal(string userNameOrEmail);
        bool DeleteDomainUserPrincipal(string userName);
    }

    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly IRepository<SettingRecord> _settingsRepository;
        private readonly IRepository<DomainRecord> _domainsRepository;

        public ActiveDirectoryService(
            IRepository<SettingRecord> settingsRepository,
            IRepository<DomainRecord> domainsRepository
			)
		{
            _settingsRepository = settingsRepository;
            _domainsRepository = domainsRepository;
		}

        #region Activy Directory Func

        #region Helper Methods

        public class DomainUser
        {
            public string Domain { get; set; }
            public string SamAccountName { get; set; }
            public string UserPrincipalName { get; set; }
        }

        public DomainUser ParseDomainUser(string userNameOrEmail)
        {
            if (_domainsRepository.Table.Count() == 0) return null;

            string domainName;
            string domainSamAccountName;
            string domainUserPrincipalName;

            string _defaultDomain = _settingsRepository.Table.FirstOrDefault().DefaultDomain;

            if (userNameOrEmail.Contains('\\'))
            {
                var parts = userNameOrEmail.Split('\\');
                if (parts.Count() != 2)
                    return null; // throw new ArgumentException("Invalid user name");

                domainName = parts[0];
                domainSamAccountName = parts[1];
                domainUserPrincipalName = "";

                if (_domainsRepository.Fetch(d => d.Name == domainName).Count() == 0)
                    return null; // throw new ArgumentException("Unknown domain");
            }
            else if (userNameOrEmail.Contains('@'))
            {
                var parts = userNameOrEmail.Split('@');
                if (parts.Count() != 2)
                    return null; // throw new ArgumentException("Invalid user name");

                domainName = parts[1];

                if (_domainsRepository.Fetch(d => d.Name == domainName).Count() == 0)
                {
                    domainName = _defaultDomain;
                    domainSamAccountName = "";
                    domainUserPrincipalName = userNameOrEmail; // Login by email
                }
                else
                {
                    domainSamAccountName = parts[0];
                    domainUserPrincipalName = "";
                }
            }
            else if (!string.IsNullOrWhiteSpace(_defaultDomain))
            {
                domainName = _defaultDomain;
                domainSamAccountName = userNameOrEmail;
                domainUserPrincipalName = "";
            }
            else return null;

            return new DomainUser { Domain = domainName, SamAccountName = domainSamAccountName, UserPrincipalName = domainUserPrincipalName };
        }

        /// <summary>
        /// Gets the default principal context
        /// </summary>
        /// <returns>Returns the PrincipalContext object</returns>
        public PrincipalContext GetDefaultPrincipalContext()
        {
            string _defaultDomain = _settingsRepository.Table.FirstOrDefault().DefaultDomain;
            return GetDomainPrincipalContext(_defaultDomain);
        }

        /// <summary>
        /// Gets the domain principal context
        /// </summary>
        /// <returns>Returns the PrincipalContext object</returns>
        public PrincipalContext GetDomainPrincipalContext(string domainName)
        {
            if (_domainsRepository.Fetch(d => d.Name == domainName).Any())
            {
                var domain = _domainsRepository.Fetch(d => d.Name == domainName).SingleOrDefault();
                var context = new PrincipalContext(ContextType.Domain, domain.Name, domain.UserName, domain.Password);
                return context;
            }
            return null;
        }

        #endregion

        #region User Methods

        /// <summary>
        /// Creates new UserPrincipal
        /// </summary>
        /// <returns>Returns the UserPrincipal object</returns>
        public UserPrincipal CreateDomainUserPrincipal(string userName, string email, string password)
        {
            var context = GetDefaultPrincipalContext();
            if (context != null)
                return CreateDomainUserPrincipal(context, userName, email, password);
            return null;
        }

        /// <summary>
        /// Creates new UserPrincipal
        /// </summary>
        /// <returns>Returns the UserPrincipal object</returns>
        public UserPrincipal CreateDomainUserPrincipal(PrincipalContext context, string userName, string email, string password)
        {
            // Check if AD User exists
            var userPrincipal = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName);

            if (userPrincipal == null)
            {
                // Create AD User
                userPrincipal = new UserPrincipal(context, userName, password, true);
                userPrincipal.Name = userName;
                userPrincipal.DisplayName = userName;
                //userPrincipal.SamAccountName = userName;
                userPrincipal.UserPrincipalName = email;
                userPrincipal.EmailAddress = email;
                userPrincipal.PasswordNeverExpires = true;
                //userPrincipal.Enabled = true;
                userPrincipal.Save();

                // Set AD User Password
                //userPrincipal.ChangePassword("", password);
                //userPrincipal.Save();
            }

            // Add user to group CMS_Users
            var groupPrincipal = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, "CMS_Users");
            AddUserToGroup(userPrincipal, groupPrincipal);

            return userPrincipal;
        }

        /// <summary>
        /// Gets the UserPrincipal
        /// </summary>
        /// <returns>Returns the UserPrincipal object</returns>
        public UserPrincipal GetDomainUserPrincipal(string userNameOrEmail)
        {
            var domainUser = ParseDomainUser(userNameOrEmail);
            if (domainUser != null)
            {
                var context = GetDomainPrincipalContext(domainUser.Domain);
                if (context != null)
                {
                    if (!string.IsNullOrEmpty(domainUser.SamAccountName))
                    {
                        return UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, domainUser.SamAccountName);
                    }
                    else if (!string.IsNullOrEmpty(domainUser.UserPrincipalName))
                    {
                        return UserPrincipal.FindByIdentity(context, IdentityType.UserPrincipalName, domainUser.UserPrincipalName);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the UserPrincipal
        /// </summary>
        /// <returns>Returns the UserPrincipal object</returns>
        public bool DeleteDomainUserPrincipal(string userName)
        {
            try
            {
                var userPrincipal = GetDomainUserPrincipal(userName);
                if (userPrincipal != null)
                    userPrincipal.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Group Methods

        /// <summary>
        /// Adds the user for a given group
        /// </summary>
        /// <param name="userPrincipal">The user you want to add to a group</param>
        /// <param name="groupPrincipal">The group you want the user to be added in</param>
        /// <returns>Returns true if successful</returns>
        public bool AddUserToGroup(UserPrincipal userPrincipal, GroupPrincipal groupPrincipal)
        {
            try
            {
                if (userPrincipal != null && groupPrincipal != null)
                {
                    if (!IsUserGroupMember(userPrincipal, groupPrincipal))
                    {
                        groupPrincipal.Members.Add(userPrincipal);
                        groupPrincipal.Save();
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes user from a given group
        /// </summary>
        /// <param name="userPrincipal">The user you want to remove from a group</param>
        /// <param name="groupPrincipal">The group you want the user to be removed from</param>
        /// <returns>Returns true if successful</returns>
        public bool RemoveUserFromGroup(UserPrincipal userPrincipal, GroupPrincipal groupPrincipal)
        {
            try
            {
                if (userPrincipal != null && groupPrincipal != null)
                {
                    if (IsUserGroupMember(userPrincipal, groupPrincipal))
                    {
                        groupPrincipal.Members.Remove(userPrincipal);
                        groupPrincipal.Save();
                    }
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if user is a member of a given group
        /// </summary>
        /// <param name="userPrincipal">The user you want to validate</param>
        /// <param name="groupPrincipal">The group you want to check the 
        /// membership of the user</param>
        /// <returns>Returns true if user is a group member</returns>
        public bool IsUserGroupMember(UserPrincipal userPrincipal, GroupPrincipal groupPrincipal)
        {
            if (userPrincipal != null && groupPrincipal != null)
            {
                return groupPrincipal.Members.Contains(userPrincipal);
            }
            return false;
        }

        #endregion

        #endregion

        #region ADFunc

        #region Validate Methods

        /// <summary>
        /// Validates the username and password of a given user
        /// </summary>
        /// <param name="sUserName">The username to validate</param>
        /// <param name="sPassword">The password of the username to validate</param>
        /// <returns>Returns True of user is valid</returns>
        public bool ValidateCredentials(string sUserName, string sPassword)
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext();
            return oPrincipalContext.ValidateCredentials(sUserName, sPassword);
        }

        /// <summary>
        /// Checks if the User Account is Expired
        /// </summary>
        /// <param name="sUserName">The username to check</param>
        /// <returns>Returns true if Expired</returns>
        public bool IsUserExpired(string sUserName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            if (oUserPrincipal.AccountExpirationDate != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if user exists on AD
        /// </summary>
        /// <param name="sUserName">The username to check</param>
        /// <returns>Returns true if username Exists</returns>
        public bool IsUserExisiting(string sUserName)
        {
            if (GetUserPrincipal(sUserName) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if user account is locked
        /// </summary>
        /// <param name="sUserName">The username to check</param>
        /// <returns>Returns true of Account is locked</returns>
        public bool IsAccountLocked(string sUserName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            return oUserPrincipal.IsAccountLockedOut();
        }
        #endregion

        #region Search Methods

        /// <summary>
        /// Gets a certain user on Active Directory
        /// </summary>
        /// <param name="sUserName">The username to get</param>
        /// <returns>Returns the UserPrincipal Object</returns>
        public UserPrincipal GetUserPrincipal(string sUserName)
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext();

            UserPrincipal oUserPrincipal =
               UserPrincipal.FindByIdentity(oPrincipalContext, sUserName);
            return oUserPrincipal;
        }

        /// <summary>
        /// Gets a certain group on Active Directory
        /// </summary>
        /// <param name="sGroupName">The group to get</param>
        /// <returns>Returns the GroupPrincipal Object</returns>
        public GroupPrincipal GetGroup(string sGroupName)
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext();

            GroupPrincipal oGroupPrincipal =
               GroupPrincipal.FindByIdentity(oPrincipalContext, sGroupName);
            return oGroupPrincipal;
        }

        #endregion

        #region User Account Methods

        /// <summary>
        /// Sets the user password
        /// </summary>
        /// <param name="sUserName">The username to set</param>
        /// <param name="sNewPassword">The new password to use</param>
        /// <param name="sMessage">Any output messages</param>
        public void SetUserPassword(string sUserName, string sNewPassword, out string sMessage)
        {
            try
            {
                UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
                oUserPrincipal.SetPassword(sNewPassword);
                sMessage = "";
            }
            catch (Exception ex)
            {
                sMessage = ex.Message;
            }
        }

        /// <summary>
        /// Enables a disabled user account
        /// </summary>
        /// <param name="sUserName">The username to enable</param>
        public void EnableUserAccount(string sUserName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            oUserPrincipal.Enabled = true;
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Force disabling of a user account
        /// </summary>
        /// <param name="sUserName">The username to disable</param>
        public void DisableUserAccount(string sUserName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            oUserPrincipal.Enabled = false;
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Force expire password of a user
        /// </summary>
        /// <param name="sUserName">The username to expire the password</param>
        public void ExpireUserPassword(string sUserName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            oUserPrincipal.ExpirePasswordNow();
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Unlocks a locked user account
        /// </summary>
        /// <param name="sUserName">The username to unlock</param>
        public void UnlockUserAccount(string sUserName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            oUserPrincipal.UnlockAccount();
            oUserPrincipal.Save();
        }

        /// <summary>
        /// Creates a new user on Active Directory
        /// </summary>
        /// <param name="sOU">The OU location you want to save your user</param>
        /// <param name="sUserName">The username of the new user</param>
        /// <param name="sPassword">The password of the new user</param>
        /// <param name="sGivenName">The given name of the new user</param>
        /// <param name="sSurname">The surname of the new user</param>
        /// <returns>returns the UserPrincipal object</returns>
        public UserPrincipal CreateNewUserPrincipal(string sOU, string sUserName, string sPassword, string sGivenName, string sSurname)
        {
            if (!IsUserExisiting(sUserName))
            {
                PrincipalContext oPrincipalContext = GetPrincipalContext(sOU);

                UserPrincipal oUserPrincipal = new UserPrincipal
                   (oPrincipalContext, sUserName, sPassword, true);

                //User Log on Name
                oUserPrincipal.UserPrincipalName = sUserName;
                oUserPrincipal.GivenName = sGivenName;
                oUserPrincipal.Surname = sSurname;
                oUserPrincipal.Save();

                return oUserPrincipal;
            }
            else
            {
                return GetUserPrincipal(sUserName);
            }
        }

        /// <summary>
        /// Deletes a user in Active Directory
        /// </summary>
        /// <param name="sUserName">The username you want to delete</param>
        /// <returns>Returns true if successfully deleted</returns>
        public bool DeleteUser(string sUserName)
        {
            try
            {
                UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);

                oUserPrincipal.Delete();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Group Methods

        /// <summary>
        /// Creates a new group in Active Directory
        /// </summary>
        /// <param name="sOU">The OU location you want to save your new Group</param>
        /// <param name="sGroupName">The name of the new group</param>
        /// <param name="sDescription">The description of the new group</param>
        /// <param name="oGroupScope">The scope of the new group</param>
        /// <param name="bSecurityGroup">True is you want this group 
        /// to be a security group, false if you want this as a distribution group</param>
        /// <returns>Returns the GroupPrincipal object</returns>
        public GroupPrincipal CreateNewGroup(string sOU, string sGroupName,
           string sDescription, GroupScope oGroupScope, bool bSecurityGroup)
        {
            PrincipalContext oPrincipalContext = GetPrincipalContext(sOU);

            GroupPrincipal oGroupPrincipal = new GroupPrincipal(oPrincipalContext, sGroupName);
            oGroupPrincipal.Description = sDescription;
            oGroupPrincipal.GroupScope = oGroupScope;
            oGroupPrincipal.IsSecurityGroup = bSecurityGroup;
            oGroupPrincipal.Save();

            return oGroupPrincipal;
        }

        /// <summary>
        /// Adds the user for a given group
        /// </summary>
        /// <param name="sUserName">The user you want to add to a group</param>
        /// <param name="sGroupName">The group you want the user to be added in</param>
        /// <returns>Returns true if successful</returns>
        public bool AddUserToGroup(string sUserName, string sGroupName)
        {
            try
            {
                UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
                GroupPrincipal oGroupPrincipal = GetGroup(sGroupName);
                if (oUserPrincipal == null || oGroupPrincipal == null)
                {
                    if (!IsUserGroupMember(sUserName, sGroupName))
                    {
                        oGroupPrincipal.Members.Add(oUserPrincipal);
                        oGroupPrincipal.Save();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Removes user from a given group
        /// </summary>
        /// <param name="sUserName">The user you want to remove from a group</param>
        /// <param name="sGroupName">The group you want the user to be removed from</param>
        /// <returns>Returns true if successful</returns>
        public bool RemoveUserFromGroup(string sUserName, string sGroupName)
        {
            try
            {
                UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
                GroupPrincipal oGroupPrincipal = GetGroup(sGroupName);
                if (oUserPrincipal == null || oGroupPrincipal == null)
                {
                    if (IsUserGroupMember(sUserName, sGroupName))
                    {
                        oGroupPrincipal.Members.Remove(oUserPrincipal);
                        oGroupPrincipal.Save();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if user is a member of a given group
        /// </summary>
        /// <param name="sUserName">The user you want to validate</param>
        /// <param name="sGroupName">The group you want to check the 
        /// membership of the user</param>
        /// <returns>Returns true if user is a group member</returns>
        public bool IsUserGroupMember(string sUserName, string sGroupName)
        {
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);
            GroupPrincipal oGroupPrincipal = GetGroup(sGroupName);

            if (oUserPrincipal == null || oGroupPrincipal == null)
            {
                return oGroupPrincipal.Members.Contains(oUserPrincipal);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a list of the users group memberships
        /// </summary>
        /// <param name="sUserName">The user you want to get the group memberships</param>
        /// <returns>Returns an arraylist of group memberships</returns>
        public ArrayList GetUserGroups(string sUserName)
        {
            ArrayList myItems = new ArrayList();
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);

            PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetGroups();

            foreach (Principal oResult in oPrincipalSearchResult)
            {
                myItems.Add(oResult.Name);
            }
            return myItems;
        }

        /// <summary>
        /// Gets a list of the users authorization groups
        /// </summary>
        /// <param name="sUserName">The user you want to get authorization groups</param>
        /// <returns>Returns an arraylist of group authorization memberships</returns>
        public ArrayList GetUserAuthorizationGroups(string sUserName)
        {
            ArrayList myItems = new ArrayList();
            UserPrincipal oUserPrincipal = GetUserPrincipal(sUserName);

            PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetAuthorizationGroups();

            foreach (Principal oResult in oPrincipalSearchResult)
            {
                myItems.Add(oResult.Name);
            }
            return myItems;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the base principal context
        /// </summary>
        /// <returns>Returns the PrincipalContext object</returns>
        public PrincipalContext GetPrincipalContext()
        {
            string _defaultDomain = _settingsRepository.Table.FirstOrDefault().DefaultDomain;
            var domain = _domainsRepository.Fetch(d => d.Name == _defaultDomain).SingleOrDefault();
            var context = new PrincipalContext(ContextType.Domain, domain.Name, domain.UserName, domain.Password);
            //PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain, domain.Name, "CN=Users,DC=dinhgianhadat,DC=vn", ContextOptions.SimpleBind, domain.UserName, domain.Password);
            return context;
        }

        /// <summary>
        /// Gets the principal context on specified OU
        /// </summary>
        /// <param name="sOU">The OU you want your Principal Context to run on</param>
        /// <returns>Returns the PrincipalContext object</returns>
        public PrincipalContext GetPrincipalContext(string sOU)
        {
            string _defaultDomain = _settingsRepository.Table.FirstOrDefault().DefaultDomain;
            var domain = _domainsRepository.Fetch(d => d.Name == _defaultDomain).SingleOrDefault();
            PrincipalContext oPrincipalContext =
               new PrincipalContext(ContextType.Domain, domain.Name, sOU,
               ContextOptions.SimpleBind, domain.UserName, domain.Password);
            return oPrincipalContext;
        }

        #endregion

        #endregion

        #region Unused

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

        //private Tuple<string, string> ParseDomainUser(string userName)
        //{
        //    if (_domainsRepository.Table.Count() == 0) return null;

        //    string domainName;
        //    string domainUserName;
        //    string _defaultDomain = _settingsRepository.Table.FirstOrDefault().DefaultDomain;

        //    if (userName.Contains('\\'))
        //    {
        //        var parts = userName.Split('\\');
        //        if (parts.Count() != 2) throw new ArgumentException("Invalid user name");
        //        domainName = parts[0];
        //        domainUserName = parts[1];

        //        if (_domainsRepository.Fetch(d => d.Name == domainName).Count() == 0)
        //            throw new ArgumentException("Unknown domain");
        //    }
        //    else if (userName.Contains('@'))
        //    {
        //        var parts = userName.Split('@');
        //        if (parts.Count() != 2) return null;
        //        domainName = parts[1];
        //        domainUserName = parts[0];

        //        if (_domainsRepository.Fetch(d => d.Name == domainName).Count() == 0) return null;
        //    }
        //    else if (!string.IsNullOrWhiteSpace(_defaultDomain))
        //    {
        //        return new Tuple<string, string>(_defaultDomain, userName);
        //    }
        //    else return null;

        //    return new Tuple<string, string>(domainName, domainUserName);
        //}

        #endregion

    }
}
