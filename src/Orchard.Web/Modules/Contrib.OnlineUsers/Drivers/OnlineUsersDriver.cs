using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Contrib.OnlineUsers.Models;
using Orchard.UI.Notify;
using Orchard.Localization;
using Contrib.Voting.Models;
using Orchard.Data;

namespace Contrib.OnlineUsers.Drivers
{
    public class OnlineUsersDriver:ContentPartDriver<OnlineUsersPart>
    {

        private readonly IRepository<VoteRecord> _voteRepository;
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;

        public Localizer T { get; set; }

        public OnlineUsersDriver(IRepository<VoteRecord> voteRepository, INotifier notifier, IContentManager contentManager)
        {
            _voteRepository = voteRepository;
            _notifier = notifier;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(OnlineUsersPart part, string displayType, dynamic shapeHelper)
        {
            var lastActiveTime = DateTime.UtcNow.AddMinutes(-15);
            var _userCount = _contentManager.Query<MembershipPart, MembershipPartRecord>().Where(record => record.LastActive >= lastActiveTime).Count();
            var _visitorCount = _voteRepository.Fetch(a => a.CreatedUtc >= lastActiveTime).GroupBy(a => a.Username).Count();

            return ContentShape("Parts_OnlineUsers", () =>
            {
                var shape = shapeHelper.Parts_OnlineUsers();
                shape.ContentPart = part;
                shape.UserCount = _userCount;
                shape.VisitorCount = _visitorCount;
                return shape;
            });
        }
    }
}