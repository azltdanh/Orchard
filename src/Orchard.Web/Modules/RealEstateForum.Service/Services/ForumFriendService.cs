using System;
using System.Collections.Generic;
using System.Linq;

using Orchard;
using RealEstateForum.Service.Models;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Localization;
using Orchard.Users.Models;
using RealEstateForum.Service.ViewModels;
using RealEstate.Services;

namespace RealEstateForum.Service.Services
{
    public interface IForumFriendService : IDependency
    {
        IContentQuery<ForumFriendPart, ForumFriendPartRecord> ListFriendByUser(UserPartRecord user);
        IContentQuery<ForumFriendPart, ForumFriendPartRecord> ListFriendRequestByUser(UserPartRecord user);
        IContentQuery<ForumFriendPart, ForumFriendPartRecord> ListFriendWaitingByUser(UserPartRecord user);
        bool CheckIsFriend(UserPartRecord userRequest, UserPartRecord userReceived);
        List<UserPart> ListFriendRecordByUser(UserPartRecord user);
        List<UserInfo> BuildListFriend(List<UserPart> results, UserPart userCurent);
        int CheckFriend(UserPart userCurent, UserPart userSelect);
        List<UserPart> ListRequestFriendRecordByUser(UserPartRecord user);
        bool AddFriend(UserPartRecord userCurent, UserPartRecord userSelect);
        bool AcceptFriend(UserPartRecord userCurent, UserPartRecord userSelect);
    }

    public class ForumFriendService : IForumFriendService
    {
        private readonly IRepository<ForumFriendPartRecord> _forumFriendRepository;
        private readonly IContentManager _contentManager;
        private readonly IUserGroupService _groupUserService;
        private readonly IUserPersonalService _userRealEstateService;
        public ForumFriendService(
            IRepository<ForumFriendPartRecord> forumFriendRepository,
            IContentManager contentManager,
            IUserPersonalService userRealEstateService,
            IUserGroupService groupUserService
            )
        {
            _contentManager = contentManager;
            _forumFriendRepository = forumFriendRepository;
            _groupUserService = groupUserService;
            _userRealEstateService = userRealEstateService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public IContentQuery<ForumFriendPart, ForumFriendPartRecord> ListFriendByUser(UserPartRecord user)
        {
            return _contentManager.Query<ForumFriendPart, ForumFriendPartRecord>().Where(r => (r.UserRequest == user || r.UserReceived == user) && r.Status).OrderByDescending(r=>r.DateRequest);
        }

        public IContentQuery<ForumFriendPart, ForumFriendPartRecord> ListFriendRequestByUser(UserPartRecord user)
        {
            return _contentManager.Query<ForumFriendPart, ForumFriendPartRecord>().Where(r => r.UserReceived == user && !r.Status).OrderByDescending(r => r.DateRequest);
        }

        public IContentQuery<ForumFriendPart, ForumFriendPartRecord> ListFriendWaitingByUser(UserPartRecord user)
        {
            return _contentManager.Query<ForumFriendPart, ForumFriendPartRecord>().Where(r => r.UserRequest == user && !r.Status).OrderByDescending(r => r.DateRequest);
        }

        public bool CheckIsFriend(UserPartRecord userRequest, UserPartRecord userReceived)
        {
            return _contentManager.Query<ForumFriendPart, ForumFriendPartRecord>().Where(r => r.UserRequest == userRequest && r.UserReceived == userReceived && r.Status).Count() > 0;
        }

        public List<UserPart> ListFriendRecordByUser(UserPartRecord user)
        {
            var query1 = ListFriendByUser(user);
            var query2 = ListFriendByUser(user);

            var listTemp = new List<Int32>();

            listTemp.AddRange(query1.Where(r=>r.UserRequest == user).List().Select(r=> r.UserReceived.Id));

            listTemp.AddRange(query2.Where(r => r.UserReceived == user).List().Select(r => r.UserRequest.Id));

            return _contentManager.Query<UserPart,UserPartRecord>().Where(r=> listTemp.Contains(r.Id)).List().Select(r=>r).ToList();
        }
        public List<UserPart> ListRequestFriendRecordByUser(UserPartRecord user)
        {
            var query1 = ListFriendRequestByUser(user);
            var query2 = ListFriendRequestByUser(user);

            var listTemp = new List<Int32>();

            listTemp.AddRange(query1.Where(r => r.UserRequest == user).List().Select(r => r.UserReceived.Id));

            listTemp.AddRange(query2.Where(r => r.UserReceived == user).List().Select(r => r.UserRequest.Id));

            return _contentManager.Query<UserPart, UserPartRecord>().Where(r => listTemp.Contains(r.Id)).List().Select(r => r).ToList();
        }
        public List<UserInfo> BuildListFriend(List<UserPart> results, UserPart userCurent)
        {
            return results.Select(r => new UserInfo
                {
                    Id = r.Id,
                    UserName = r.UserName,
                    Avatar = _userRealEstateService.GetUserAvatar(r.Id),
                    DisplayName = _userRealEstateService.GetUserNameOrDisplayName(r.Id),
                    CheckFriend = CheckFriend(userCurent,r)
                }).ToList();
        }
        /// <summary>
        /// Check Friend
        /// </summary>
        /// <param name="userSelect"></param>
        /// <returns>1: Đã kết bạn. 2: Đã gửi yêu cầu kết bạn. 3: Nhận yêu cầu kết bạn. 4: Kết bạn</returns>
        public int CheckFriend(UserPart userCurent, UserPart userSelect)
        {
            if (userSelect == null && userCurent == null) return 0;

            if (ListFriendByUser(userCurent.Record).Where(r => r.UserRequest == userSelect.Record || r.UserReceived == userSelect.Record).Count() > 0)
            {
                return 1;//Da ket ban
            }

            if (ListFriendWaitingByUser(userCurent.Record).Where(r => r.UserReceived == userSelect.Record).Count() > 0)
            {
                return 2;//Da gui yeu cau ket ban
            }

            if (ListFriendRequestByUser(userCurent.Record).Where(r => r.UserRequest == userSelect.Record).Count() > 0)
            {
                return 3;//Nhan yeu cau ket ban
            }

            return 4;//Ket ban
        }
        public bool AddFriend(UserPartRecord userCurent, UserPartRecord userSelect)
        {
            var query = _contentManager.Query<ForumFriendPart, ForumFriendPartRecord>().Where(r =>
                                                                               (r.UserRequest == userCurent && r.UserReceived == userSelect)
                                                                            || (r.UserRequest == userSelect && r.UserReceived == userCurent));

            if (query.Count() > 0) return false;

            try
            {
                var record = _contentManager.New<ForumFriendPart>("ForumFriend");
                record.UserRequest = userCurent;
                record.UserReceived = userSelect;
                record.DateRequest = DateTime.Now;
                record.Status = false;

                _contentManager.Create(record);

                return true;
            }
            catch { return false; }
        }
        public bool AcceptFriend(UserPartRecord userCurent, UserPartRecord userSelect)
        {
            var query = _contentManager.Query<ForumFriendPart, ForumFriendPartRecord>().Where(r => r.UserRequest == userSelect && r.UserReceived == userCurent && !r.Status);

            if (query.Count() == 0) return false;

            try
            {
                query.List().FirstOrDefault().Status = true;

                return true;
            }
            catch { return false; }
        }
    }
}