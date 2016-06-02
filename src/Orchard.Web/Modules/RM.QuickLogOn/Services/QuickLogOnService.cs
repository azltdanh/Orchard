using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Users.Models;
using RM.QuickLogOn.Providers;

namespace RM.QuickLogOn.Services
{
    public interface IQuickLogOnService : IDependency
    {
        IEnumerable<IQuickLogOnProvider> GetProviders();
        QuickLogOnResponse LogOn(QuickLogOnRequest request);

    }

    public class QuickLogOnService : IQuickLogOnService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEnumerable<IQuickLogOnProvider> _providers;

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public QuickLogOnService(IEnumerable<IQuickLogOnProvider> providers,
                                 IMembershipService membershipService,
                                 IAuthenticationService authenticationService,
                                 IOrchardServices orchardServices)
        {
            _providers = providers;
            _membershipService = membershipService;
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IEnumerable<IQuickLogOnProvider> GetProviders()
        {
            return _providers;
        }

        public QuickLogOnResponse LogOn(QuickLogOnRequest request)
        {
            var currentUser = _authenticationService.GetAuthenticatedUser();
            if (currentUser != null) _authenticationService.SignOut();

            var lowerEmail = request.Email == null ? "" : request.Email.ToLowerInvariant();

            //var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName).List().FirstOrDefault();

            //if (user == null)
            var user = _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Email == lowerEmail).Slice(1).FirstOrDefault();

            if (user == null)
            {
                user = _membershipService.CreateUser(new CreateUserParams(lowerEmail, Guid.NewGuid().ToString(), lowerEmail, null, null, true)) as UserPart;
                //user = _userPersonalService.CreateUser(new CreateUserParams(lowerEmail, Guid.NewGuid().ToString(), lowerEmail, null, null, true)) as UserPart;
                user.EmailStatus = UserStatus.Approved;
            }

            ////if (user.RegistrationStatus != UserStatus.Approved)
            ////{
            ////    return new QuickLogOnResponse { User = null, Error = T("User was disabled by site administrator"), ReturnUrl = request.ReturnUrl };
            ////}

            _authenticationService.SignIn(user, request.RememberMe);

            return new QuickLogOnResponse { User = user, ReturnUrl = request.ReturnUrl };
        }
    }
}
