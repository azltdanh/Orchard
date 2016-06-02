using System.Text.RegularExpressions;
using Orchard;
using Orchard.Environment.Extensions;

namespace Vandelay.Industries.Services {
    [OrchardFeature("Vandelay.ThemePicker")]
    public class UserDomainThemeSelectionRule : IThemeSelectionRule {
        private readonly IWorkContextAccessor _workContextAccessor;

        public UserDomainThemeSelectionRule(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public bool Matches(string name, string criterion) {
            var host = _workContextAccessor.GetContext().HttpContext.Request.Url.Host;
            if (host == null) return false;
            return host == criterion;
        }
    }
}