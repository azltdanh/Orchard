using Contrib.OnlineUsers.Models;
using Contrib.OnlineUsers.ViewModels;
using Contrib.Voting.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Users.Models;
//using RealEstate.Forum.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contrib.OnlineUsers.Drivers
{
    public class ListOnlineUsersPartDriver : ContentPartDriver<ListOnlineUsersPart>
    {
        private readonly IRepository<VoteRecord> _voteRepository;
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;
        //private readonly IUserFriendService _userService;

        public Localizer T { get; set; }

        public ListOnlineUsersPartDriver(IRepository<VoteRecord> voteRepository, INotifier notifier/*,IUserFriendService userService*/, IContentManager contentManager)
        {
            _voteRepository = voteRepository;
            _notifier = notifier;
            //_userService = userService;
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ListOnlineUsersPart part, string displayType, dynamic shapeHelper)
        {
            //var lastActiveTime = DateTime.UtcNow.AddMinutes(-15);
            //var _lstOnline = _contentManager.Query<MembershipPart, MembershipPartRecord>().Where(record => record.LastActive >= lastActiveTime).List();
            //var ViewModel = new OnlineUsersViewModel
            //{
            //    OnlineUsers = _lstOnline
            //    .Select(o => new OnlineUsersEntry
            //    {
            //        UserName = _contentManager.Get<UserPart>(o.Record.Id).UserName,
            //        id = o.Record.Id,
            //        Email = _contentManager.Get<UserPart>(o.Record.Id).Email,
            //        UserDisplayName = _userService.GetUserPersonal(o.Record.Id).DisplayName
            //    }).ToList()
            //};


            return ContentShape("Parts_ListOnlineUsers",
                () => shapeHelper.DisplayTemplate(
                    TemplateName: "Parts/ListOnlineUsers",
                    Model: part,
                    Prefix: Prefix));
        }
    }
}
