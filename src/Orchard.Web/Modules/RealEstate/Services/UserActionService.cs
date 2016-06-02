using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Logging;
using Orchard.Services;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IUserActionService : IDependency
    {
        bool VerifyUserActionUnicity(string actionName);
        bool VerifyUserActionUnicity(int id, string actionName);

        UserActionPart GetDirection(int? userActionId);
        IEnumerable<UserActionPart> GetUserActions();
    }

    public class UserActionService : IUserActionService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly ISignals _signals;
        private const int CacheTimeSpan = 60*24; // Cache for 24 hours

        public UserActionService(IContentManager contentManager, ICacheManager cacheManager, IClock clock,
            ISignals signals)
        {
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _clock = clock;
            _signals = signals;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyUserActionUnicity(string actionName)
        {
            return
                !_contentManager.Query<UserActionPart, UserActionPartRecord>()
                    .Where(r => r.Name == actionName)
                    .List()
                    .Any();
        }

        public bool VerifyUserActionUnicity(int id, string actionName)
        {
            return
                !_contentManager.Query<UserActionPart, UserActionPartRecord>()
                    .Where(r => r.Name == actionName)
                    .List()
                    .Any(r => r.Id != id);
        }

        public UserActionPart GetDirection(int? userActionId)
        {
            if (userActionId > 0)
            {
                var part = _contentManager.Get<UserActionPart>((int) userActionId);
                if (part != null) return part;
            }
            return null;
        }

        public IEnumerable<UserActionPart> GetUserActions()
        {
            return _cacheManager.Get("UserActions", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("UserActions_Changed"));

                return _contentManager.Query<UserActionPart, UserActionPartRecord>().OrderBy(a => a.SeqOrder).List();
            });
        }
    }
}