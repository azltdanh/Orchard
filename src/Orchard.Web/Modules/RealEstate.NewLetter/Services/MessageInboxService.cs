using System;
using System.Collections.Generic;
using System.Linq;

using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using RealEstate.NewLetter.Models;
using Orchard.Users.Models;
using Orchard.UI.Notify;
using Orchard.Messaging.Services;
using RealEstate.NewLetter.ViewModels;
using Orchard.Data;
using RealEstate.Services;

namespace RealEstate.NewLetter.Services
{
    public interface IMessageInboxService : IDependency
    {
        /// <summary>
        /// Send Message, Admin <=> User or User <=> User
        /// </summary>
        /// <param name="title">Title of Message</param>
        /// <param name="content">Content of Message</param>
        /// <param name="UserReceive">UserId Receive</param>
        /// <param name="IsAdmin">True: Admin Inbox(Admin <=> User). False: User Inbox (User <=> User)</param>
        /// <param name="ReadByStaff">True: User => Admin. False: Admin => User (or not Admin Inbox)</param>
        void SendMessage(string title, string content, int UserReceive, bool IsAdmin, bool ReadByStaff);
        void ReplyMessage(int Id, string title, string content, bool IsAdmin, bool ReadByStaff);

        void SendMessageEmail(string usernameOrEmail, string url, string title);
        void SendMessageToEmail(string usernameOrEmail, string content, string title);
        bool DeleteMessageInbox(int Id);
        int CountMessageInboxFromAdmin();
        int CountMessageInboxUser();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1. FromAdmin - 2. FromFriend</param>
        /// <returns></returns>
        IContentQuery<MessageInboxPart, MessageInboxPartRecord> GetListMessageInboxByType(int type, UserPart user);
    }
    public class MessageInboxService : IMessageInboxService
    {
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        private readonly IRepository<MessageInboxPartRecord> _msgInboxRepository;
        private readonly IHostNameService _hostNameService;

        public MessageInboxService(
            IOrchardServices services,
            IMessageManager messageManager,
            IContentManager contentManager,
            IRepository<MessageInboxPartRecord> msgInboxRepository,
            IHostNameService hostNameService)
        {
            Services = services;
            _contentManager = contentManager;
            _messageManager = messageManager;
            _msgInboxRepository = msgInboxRepository;
            _hostNameService = hostNameService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public void SendMessage(string title, string content, int UserReceive, bool IsAdmin, bool ReadByStaff)
        {
            var _record = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            var _userReceive = Services.ContentManager.Get<UserPart>(UserReceive).Record;
            var hostName = _hostNameService.GetHostNameSite();

            _record.Title = title;
            _record.Content = content;
            _record.DateSend = DateTime.Now;
            _record.IsRead = false;
            _record.IsAdmin = IsAdmin;
            _record.ReadByStaff = ReadByStaff;
            _record.ParentId = null;
            _record.PreviousId = null;
            _record.UserSend = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            _record.UserReceived = _userReceive;
            _record.HostName = hostName;

            Services.ContentManager.Create(_record);

            //Gửi Email
            SendMessageEmail(_userReceive.Email, "http://dinhgianhadat.vn/control-panel/xem-thu/" + _record.Id, !string.IsNullOrEmpty(title) ? title : "Thư mới từ dinhgianhadat.vn");

            Services.Notifier.Information(T("Đã gửi tin nhắn đến nhân viên! {0}. Email: {1} ", _userReceive.UserName, _userReceive.Email));
        }

        public void ReplyMessage(int Id, string title, string content, bool IsAdmin, bool ReadByStaff)
        {
            var _oldRecord = Services.ContentManager.Get<MessageInboxPart>(Id).Record;
            var _record = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            var _userReceive = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            var hostName = _hostNameService.GetHostNameSite();

            _record.Title = !string.IsNullOrEmpty(title) ? title : _oldRecord.Title;
            _record.Content = content;
            _record.DateSend = DateTime.Now;
            _record.IsRead = false;
            _record.IsAdmin = IsAdmin;
            _record.ReadByStaff = ReadByStaff;
            _record.ParentId = _oldRecord.ParentId.HasValue ? _oldRecord.ParentId : Id;
            _record.PreviousId = Id;
            _record.UserSend = _userReceive;
            _record.UserReceived = _oldRecord.UserSend;
            _record.HostName = hostName;

            Services.ContentManager.Create(_record);

            //Gửi Email
            SendMessageEmail(_userReceive.Email, "http://dinhgianhadat.vn/control-panel/xem-thu/" + _record.Id, !string.IsNullOrEmpty(title) ? title : "Thư mới từ dinhgianhadat.vn");

            Services.Notifier.Information(T("Đã gửi tin nhắn đến nhân viên! {0}. Email: {1} ", _userReceive.UserName, _userReceive.Email));
        }

        public void SendMessageEmail(string usernameOrEmail, string url, string title)
        {
            var lowerName = usernameOrEmail.ToLowerInvariant();
            var user = _contentManager.Query<UserPart, UserPartRecord>().Where(u => u.NormalizedUserName == lowerName || u.Email == lowerName).Slice(1).FirstOrDefault();

            if (usernameOrEmail != null)
            {
                _messageManager.Send(user.ContentItem.Record, MessageType.SendMessage, "email", new Dictionary<string, string> { { "ReplyUrl", url }, { "Title", title } });
            }
        }
        //messsage suggestion
        public void SendMessageToEmail(string usernameOrEmail, string content, string title)
        {
            var lowerName = usernameOrEmail.ToLowerInvariant();
            IEnumerable<string> email = new string[] { lowerName };
            if (usernameOrEmail != null)
            {
                _messageManager.Send(email, MessageType.SendMessage, "email", new Dictionary<string, string> { { "Content", content }, { "Title", title } });
            }
        }

        public bool DeleteMessageInbox(int Id)
        {
            var m = _contentManager.Get<MessageInboxPart>(Id).Record;

            if (m == null)
                return false;

            var listMsgChild = _msgInboxRepository.Fetch(a => a.ParentId != null && a.ParentId == Id);

            if (listMsgChild.Any())
            {
                foreach (var item in listMsgChild)
                {
                    _msgInboxRepository.Delete(item);
                }
            }

            _msgInboxRepository.Delete(m);

            return true;
        }

        public int CountMessageInboxFromAdmin()
        {
            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");
            int countInbox = 0;
            if (hostDefault != null && hostName == hostDefault.Name)
            {
                countInbox = _contentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => r.IsAdmin && r.ReadByStaff == false
                        && r.UserReceived == Services.WorkContext.CurrentUser.As<UserPart>().Record && r.IsRead == false && r.IsUserDelete == false && (r.HostName == null || r.HostName == hostName)).Count(); 
            }
            else
            {
                countInbox = _contentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => r.IsAdmin && r.ReadByStaff == false
                        && r.UserReceived == Services.WorkContext.CurrentUser.As<UserPart>().Record && r.IsRead == false && r.IsUserDelete == false && r.HostName == hostName).Count();
            }
            return countInbox;
        }

        public int CountMessageInboxUser()
        {
            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");
            int countInbox = 0;
            if (hostDefault != null && hostName == hostDefault.Name)
            {
                countInbox = _contentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(r => !r.IsAdmin && r.UserReceived == Services.WorkContext.CurrentUser.As<UserPart>().Record 
                        && r.IsRead == false && r.IsUserDelete == false && (r.HostName == null || r.HostName == hostName)).Count();
            }
            else
            {
                 countInbox = _contentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(r => !r.IsAdmin && r.UserReceived == Services.WorkContext.CurrentUser.As<UserPart>().Record 
                        && r.IsRead == false && r.IsUserDelete == false && r.HostName == hostName).Count();
            }
            return countInbox;
        }

        public IContentQuery<MessageInboxPart, MessageInboxPartRecord> GetListMessageInboxByType(int type, UserPart user)
        {
            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            if (hostDefault != null && hostName == hostDefault.Name)
            {
                if (type == 1)
                    return Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                        .Where(r => r.UserReceived == user.Record && r.IsAdmin && r.ReadByStaff == false && r.IsUserDelete == false && (r.HostName == null || r.HostName == hostName)).OrderByDescending(r => r.DateSend);
                else
                    return Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                        .Where(r => r.UserReceived == user.Record && !r.IsAdmin && r.IsUserDelete == false && (r.HostName == null || r.HostName == hostName)).OrderByDescending(r => r.DateSend);
            }
            else
            {
                if (type == 1)
                    return Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                        .Where(r => r.UserReceived == user.Record && r.IsAdmin && r.ReadByStaff == false && r.IsUserDelete == false && r.HostName == hostName).OrderByDescending(r => r.DateSend);
                else
                    return Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                        .Where(r => r.UserReceived == user.Record && !r.IsAdmin && r.IsUserDelete == false && r.HostName == hostName).OrderByDescending(r => r.DateSend);
            }
           
        }
    }
}