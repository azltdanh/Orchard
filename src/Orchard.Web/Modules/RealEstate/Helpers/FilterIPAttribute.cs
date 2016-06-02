using System;
using System.Web;
using System.Web.Mvc;
using RealEstate.Services;

namespace RealEstate.Helpers
{
    /// <summary>
    ///     Filter by IP address
    /// </summary>
    public class FilterIPAttribute : AuthorizeAttribute
    {
        private readonly IPropertySettingService _settingService;

        public FilterIPAttribute(IPropertySettingService settingService)
        {
            _settingService = settingService;
        }

        #region Allowed

        /// <summary>
        ///     List of allowed IPs
        /// </summary>
        private readonly IPList allowedIPListToCheck = new IPList();

        /// <summary>
        ///     Comma seperated string of allowable IPs. Example "10.2.5.41,192.168.0.22"
        /// </summary>
        /// <value></value>
        public string AllowedSingleIPs { get; set; }

        /// <summary>
        ///     Comma seperated string of allowable IPs with masks. Example "10.2.0.0;255.255.0.0,10.3.0.0;255.255.0.0"
        /// </summary>
        /// <value>The masked I ps.</value>
        public string AllowedMaskedIPs { get; set; }

        /// <summary>
        ///     Gets or sets the configuration key for allowed single IPs
        /// </summary>
        /// <value>The configuration key single I ps.</value>
        public string ConfigurationKeyAllowedSingleIPs { get; set; }

        /// <summary>
        ///     Gets or sets the configuration key allowed mmasked IPs
        /// </summary>
        /// <value>The configuration key masked I ps.</value>
        public string ConfigurationKeyAllowedMaskedIPs { get; set; }

        #endregion

        #region Denied

        /// <summary>
        ///     List of denied IPs
        /// </summary>
        private readonly IPList deniedIPListToCheck = new IPList();

        /// <summary>
        ///     Comma seperated string of denied IPs. Example "10.2.5.41,192.168.0.22"
        /// </summary>
        /// <value></value>
        public string DeniedSingleIPs { get; set; }

        /// <summary>
        ///     Comma seperated string of denied IPs with masks. Example "10.2.0.0;255.255.0.0,10.3.0.0;255.255.0.0"
        /// </summary>
        /// <value>The masked I ps.</value>
        public string DeniedMaskedIPs { get; set; }


        /// <summary>
        ///     Gets or sets the configuration key for denied single IPs
        /// </summary>
        /// <value>The configuration key single I ps.</value>
        public string ConfigurationKeyDeniedSingleIPs { get; set; }

        /// <summary>
        ///     Gets or sets the configuration key for denied masked IPs
        /// </summary>
        /// <value>The configuration key masked I ps.</value>
        public string ConfigurationKeyDeniedMaskedIPs { get; set; }

        #endregion

        /// <summary>
        ///     Determines whether access to the core framework is authorized.
        /// </summary>
        /// <param name="httpContext">
        ///     The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP
        ///     request.
        /// </param>
        /// <returns>
        ///     true if access is authorized; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext" /> parameter is null.</exception>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.IsInRole("Administrator"))
            {
                if (httpContext == null)
                    throw new ArgumentNullException("httpContext");

                string userIpAddress = httpContext.Request.UserHostAddress;

                try
                {
                    // Check that the IP is allowed to access
                    bool ipAllowed = CheckAllowedIPs(userIpAddress);

                    // Check that the IP is not denied to access
                    bool ipDenied = CheckDeniedIPs(userIpAddress);

                    // Only allowed if allowed and not denied
                    bool finallyAllowed = ipAllowed && !ipDenied;

                    return finallyAllowed;
                }
                catch (Exception e)
                {
                    // Log the exception, probably something wrong with the configuration
                    throw (e);
                }
            }
            return true; // if there was an exception, then we return true
        }

        /// <summary>
        ///     Checks the allowed IPs.
        /// </summary>
        /// <param name="userIpAddress">The user ip address.</param>
        /// <returns></returns>
        public bool CheckAllowedIPs(string userIpAddress)
        {
            // Populate the IPList with the Single IPs
            if (!string.IsNullOrEmpty(AllowedSingleIPs))
            {
                SplitAndAddSingleIPs(AllowedSingleIPs, allowedIPListToCheck);
            }

            // Populate the IPList with the Masked IPs
            if (!string.IsNullOrEmpty(AllowedMaskedIPs))
            {
                SplitAndAddMaskedIPs(AllowedMaskedIPs, allowedIPListToCheck);
            }

            // Check if there are more settings from the configuration (Web.config)
            if (!string.IsNullOrEmpty(ConfigurationKeyAllowedSingleIPs))
            {
                string configurationAllowedAdminSingleIPs =
                    _settingService.GetSetting("ConfigurationKeyAllowedSingleIPs");
                    //ConfigurationManager.AppSettings[ConfigurationKeyAllowedSingleIPs];
                if (!string.IsNullOrEmpty(configurationAllowedAdminSingleIPs))
                {
                    SplitAndAddSingleIPs(configurationAllowedAdminSingleIPs, allowedIPListToCheck);
                }
            }

            if (!string.IsNullOrEmpty(ConfigurationKeyAllowedMaskedIPs))
            {
                string configurationAllowedAdminMaskedIPs =
                    _settingService.GetSetting("ConfigurationKeyAllowedMaskedIPs");
                    //ConfigurationManager.AppSettings[ConfigurationKeyAllowedMaskedIPs];
                if (!string.IsNullOrEmpty(configurationAllowedAdminMaskedIPs))
                {
                    SplitAndAddMaskedIPs(configurationAllowedAdminMaskedIPs, allowedIPListToCheck);
                }
            }

            return allowedIPListToCheck.CheckNumber(userIpAddress);
        }

        /// <summary>
        ///     Checks the denied IPs.
        /// </summary>
        /// <param name="userIpAddress">The user ip address.</param>
        /// <returns></returns>
        public bool CheckDeniedIPs(string userIpAddress)
        {
            // Populate the IPList with the Single IPs
            if (!string.IsNullOrEmpty(DeniedSingleIPs))
            {
                SplitAndAddSingleIPs(DeniedSingleIPs, deniedIPListToCheck);
            }

            // Populate the IPList with the Masked IPs
            if (!string.IsNullOrEmpty(DeniedMaskedIPs))
            {
                SplitAndAddMaskedIPs(DeniedMaskedIPs, deniedIPListToCheck);
            }

            // Check if there are more settings from the configuration (Web.config)
            if (!string.IsNullOrEmpty(ConfigurationKeyDeniedSingleIPs))
            {
                string configurationDeniedAdminSingleIPs = _settingService.GetSetting("ConfigurationKeyDeniedSingleIPs");
                    //ConfigurationManager.AppSettings[ConfigurationKeyDeniedSingleIPs];
                if (!string.IsNullOrEmpty(configurationDeniedAdminSingleIPs))
                {
                    SplitAndAddSingleIPs(configurationDeniedAdminSingleIPs, deniedIPListToCheck);
                }
            }

            if (!string.IsNullOrEmpty(ConfigurationKeyDeniedMaskedIPs))
            {
                string configurationDeniedAdminMaskedIPs = _settingService.GetSetting("ConfigurationKeyDeniedMaskedIPs");
                    //ConfigurationManager.AppSettings[ConfigurationKeyDeniedMaskedIPs];
                if (!string.IsNullOrEmpty(configurationDeniedAdminMaskedIPs))
                {
                    SplitAndAddMaskedIPs(configurationDeniedAdminMaskedIPs, deniedIPListToCheck);
                }
            }

            return deniedIPListToCheck.CheckNumber(userIpAddress);
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
                list.Add(ipAndMask[0], ipAndMask[1]); // IP;MASK
            }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }
    }
}

/*
1. Directly specifying the IPs in the code
[FilterIP(
         AllowedSingleIPs="10.2.5.55,192.168.2.2",
         AllowedMaskedIPs="10.2.0.0;255.255.0.0,192.168.2.0;255.255.255.0"
    )]
    public class HomeController {
      // Some code here
    }
 * 
 2. Or, Loading the configuration from the Web.config
 [FilterIP(
         ConfigurationKeyAllowedSingleIPs="AllowedAdminSingleIPs",
         ConfigurationKeyAllowedMaskedIPs="AllowedAdminMaskedIPs",
         ConfigurationKeyDeniedSingleIPs="DeniedAdminSingleIPs",
         ConfigurationKeyDeniedMaskedIPs="DeniedAdminMaskedIPs"
    )]
    public class HomeController {
      // Some code here
    }
 * 
<configuration>
<appSettings>
    <add key="AllowedAdminSingleIPs" value="localhost,127.0.0.1"/> <!-- Example "10.2.80.21,192.168.2.2" -->
    <add key="AllowedAdminMaskedIPs" value="10.2.0.0;255.255.0.0"/> <!-- Example "10.2.0.0;255.255.0.0,192.168.2.0;255.255.255.0" -->
    <add key="DeniedAdminSingleIPs" value=""/>    <!-- Example "10.2.80.21,192.168.2.2" -->
    <add key="DeniedAdminMaskedIPs" value=""/>    <!-- Example "10.2.0.0;255.255.0.0,192.168.2.0;255.255.255.0" -->
</appSettings>
</configuration>

 * */