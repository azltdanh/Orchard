using System.Collections.Generic;
using RealEstate.Users.ActiveDirectory.Models;

namespace RealEstate.Users.ActiveDirectory.ViewModels
{
	public class SettingsViewModel
	{
		public string DefaultDomain { get; set; }
		public IEnumerable<DomainRecord> Domains { get; set; }
	}
}
