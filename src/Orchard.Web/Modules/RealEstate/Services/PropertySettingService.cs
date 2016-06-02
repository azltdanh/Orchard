using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Users.Models;
using RealEstate.Helpers;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertySettingService : IDependency
    {
        bool VerifyPropertySettingUnicity(string settingName);
        bool VerifyPropertySettingUnicity(int id, string settingName);
        string GetSetting(string settingName);
        bool CheckAllowedIPs();
        bool CheckAllowedIPsCustomer();
        bool CheckAllowedIPs(string userIpAddress);
    }

    public class PropertySettingService : IPropertySettingService
    {
        private readonly IContentManager _contentManager;
        private readonly IUserGroupService _groupService;

        public PropertySettingService(
            IUserGroupService groupService,
            IContentManager contentManager,
            IOrchardServices services)
        {
            _groupService = groupService;
            _contentManager = contentManager;
            Services = services;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public IOrchardServices Services { get; set; }

        public bool VerifyPropertySettingUnicity(string settingName)
        {
            return
                !_contentManager.Query<PropertySettingPart, PropertySettingPartRecord>()
                    .Where(a => a.Name == settingName)
                    .List()
                    .Any();
        }

        public bool VerifyPropertySettingUnicity(int id, string settingName)
        {
            return
                !_contentManager.Query<PropertySettingPart, PropertySettingPartRecord>()
                    .Where(a => a.Name == settingName)
                    .List()
                    .Any(a => a.Id != id);
        }

        public string GetSetting(string settingName)
        {
            if (
                _contentManager.Query<PropertySettingPart, PropertySettingPartRecord>()
                    .Where(a => a.Name == settingName)
                    .List()
                    .Any())
                return
                    _contentManager.Query<PropertySettingPart, PropertySettingPartRecord>()
                        .Where(a => a.Name == settingName)
                        .List()
                        .First()
                        .Value;
            return null;
        }

        /// <summary>
        ///     Checks the allowed IPs.
        /// </summary>
        /// <returns></returns>
        public bool CheckAllowedIPs()
        {
            if (Services.Authorizer.Authorize(Permissions.NoRestrictedIP))
                return true;

            return CheckAllowedIPs(Services.WorkContext.HttpContext.Request.UserHostAddress);
        }

        /// <summary>
        ///     Checks the allowed IPs Customer.
        /// </summary>
        /// <returns></returns>
        public bool CheckAllowedIPsCustomer()
        {
            if (Services.Authorizer.Authorize(Permissions.NoRestrictedIPCustomer))
                return true;

            return CheckAllowedIPs(Services.WorkContext.HttpContext.Request.UserHostAddress);
        }

        /// <summary>
        ///     Checks the allowed IPs.
        /// </summary>
        /// <param name="userIpAddress">The user ip address.</param>
        /// <returns></returns>
        public bool CheckAllowedIPs(string userIpAddress)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            UserGroupPartRecord group = _groupService.GetJointGroup(user.Id);

            var allowedIpListToCheck = new IPList();

            if (group != null)
            {
                // Populate the IPList with the Single IPs
                if (!string.IsNullOrEmpty(group.AllowedAdminSingleIPs))
                {
                    SplitAndAddSingleIPs(group.AllowedAdminSingleIPs, allowedIpListToCheck);
                }

                // Populate the IPList with the Masked IPs
                if (!string.IsNullOrEmpty(group.AllowedAdminMaskedIPs))
                {
                    SplitAndAddMaskedIPs(group.AllowedAdminMaskedIPs, allowedIpListToCheck);
                }
            }

            // Check if there are more settings from the configuration (Web.config)
            string configurationAllowedAdminSingleIPs = GetSetting("AllowedAdminSingleIPs");
            if (!string.IsNullOrEmpty(configurationAllowedAdminSingleIPs))
            {
                if (!string.IsNullOrEmpty(configurationAllowedAdminSingleIPs))
                {
                    SplitAndAddSingleIPs(configurationAllowedAdminSingleIPs, allowedIpListToCheck);
                }
            }

            string configurationAllowedAdminMaskedIPs = GetSetting("AllowedAdminMaskedIPs");
            if (!string.IsNullOrEmpty(configurationAllowedAdminMaskedIPs))
            {
                if (!string.IsNullOrEmpty(configurationAllowedAdminMaskedIPs))
                {
                    SplitAndAddMaskedIPs(configurationAllowedAdminMaskedIPs, allowedIpListToCheck);
                }
            }

            return allowedIpListToCheck.CheckNumber(userIpAddress);
        }

        /// <summary>
        ///     Splits the incoming ip string of the format "IP,IP" example "10.2.0.0,10.3.0.0" and adds the result to the IPList
        /// </summary>
        /// <param name="ips">The ips.</param>
        /// <param name="list">The list.</param>
        public void SplitAndAddSingleIPs(string ips, IPList list)
        {
            string[] splitSingleIPs = ips.Split(',');
            foreach (string ip in splitSingleIPs)
                list.Add(ip);
        }

        /// <summary>
        ///     Splits the incoming ip string of the format "IP;MASK,IP;MASK" example "10.2.0.0;255.255.0.0,10.3.0.0;255.255.0.0"
        ///     and adds the result to the IPList
        /// </summary>
        /// <param name="ips">The ips.</param>
        /// <param name="list">The list.</param>
        public void SplitAndAddMaskedIPs(string ips, IPList list)
        {
            string[] splitMaskedIPs = ips.Split(',');
            foreach (string maskedIp in splitMaskedIPs)
            {
                string[] ipAndMask = maskedIp.Split(';');
                if (ipAndMask.Length == 2)
                    list.Add(ipAndMask[0], ipAndMask[1]); // IP;MASK
            }
        }
    }
}