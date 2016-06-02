using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using RealEstate.Users.ActiveDirectory.Models;
using RealEstate.Users.ActiveDirectory.ViewModels;

namespace RealEstate.Users.ActiveDirectory.Controllers
{
	[Themed, Admin]
	public class ActiveDirectoryController : Controller
	{
		private readonly ShellSettings _shellSettings;
		private readonly IRepository<SettingRecord> _settingsRepository;
		private readonly IRepository<DomainRecord> _domainsRepository;

		public Localizer T { get; set; }
		public IOrchardServices Services { get; set; }
		public ILogger Logger { get; set; }

		public ActiveDirectoryController(
			IRepository<SettingRecord> settingsRepository,
			IRepository<DomainRecord> domainsRepository,
			ShellSettings shellSettings,
			IExtensionManager extensionManager,
			IOrchardServices services)
		{
			_settingsRepository = settingsRepository;
			_domainsRepository = domainsRepository;
			_shellSettings = shellSettings;

			T = NullLocalizer.Instance;
			Services = services;
			Logger = NullLogger.Instance;
		}

        public ActionResult Settings()
        {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to view Active Directory settings")))
                return new HttpUnauthorizedResult();

            var settings = _settingsRepository.Table.FirstOrDefault();

            return View(new SettingsViewModel()
            {
                DefaultDomain = settings.DefaultDomain,
                Domains = _domainsRepository.Fetch(d => true)
            });
        }

        [HttpPost]
        public ActionResult Save(string defaultDomain)
        {
            if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to save Active Directory settings")))
                return new HttpUnauthorizedResult();

            var settings = _settingsRepository.Table.FirstOrDefault();
            settings.DefaultDomain = defaultDomain;
            _settingsRepository.Update(settings);

            return RedirectToAction("Settings");
        }

		public ActionResult RemoveDomain(int id)
		{
			if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to remove domains")))
				return new HttpUnauthorizedResult();

			_domainsRepository.Delete(_domainsRepository.Get(id));
			Services.Notifier.Information(T("The domain has been removed successfully."));
			return RedirectToAction("Settings");
		}

		public ActionResult AddDomain()
		{
			if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add domains")))
				return new HttpUnauthorizedResult();

			return View(new DomainRecord());
		}

		[HttpPost]
		public ActionResult AddDomain(string name, string userName, string password)
		{
			if (_shellSettings.Name != ShellSettings.DefaultName || !Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add domains")))
				return new HttpUnauthorizedResult();

			var domain = new DomainRecord
				{
					Name = name,
					UserName = userName,
					Password = password
				};

			try
			{
				if (String.IsNullOrWhiteSpace(name))
				{
					ModelState.AddModelError("Name", T("Name is required").Text);
				}

				if (!ModelState.IsValid)
					return View(domain);

				_domainsRepository.Create(domain);

				return RedirectToAction("Settings");
			}
			catch (Exception exception)
			{
				//this.Error(exception, T("Adding domain failed: {0}", exception.Message), Logger, Services.Notifier);
                Services.Notifier.Error(T("Adding domain failed: {0}", exception.Message));

				return View(domain);
			}
		}
	}
}
