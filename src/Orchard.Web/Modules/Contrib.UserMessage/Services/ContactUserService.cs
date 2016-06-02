using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.UserMessage.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Security;

namespace Contrib.UserMessage.Services {
    public class UserMessageService : IUserMessageService {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;

        public UserMessageService(IContentManager contentManager,
            IOrchardServices orchardServices,
            IMembershipService membershipService) {
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _membershipService = membershipService;

            T = NullLocalizer.Instance;
        }

        protected Localizer T { get; set; }

        public IEnumerable<UserMessagePart> GetReceivedMessages(IUser user) {
            return _contentManager
                .Query<UserMessagePart, UserMessagePartRecord>()
                .Where(message => message.SentTo == user.UserName &&
                    !message.ReceiverRemoved)
                .List();
        }

        public IEnumerable<UserMessagePart> GetSentMessages(IUser user) {
            return _contentManager
                .Query<UserMessagePart, UserMessagePartRecord>()
                .Where(message => message.SentFrom == user.UserName &&
                    !message.SenderRemoved)
                .List();
        }

        public UserMessagePart GetMessage(int messageId) {
            return _contentManager
                .Query<UserMessagePart, UserMessagePartRecord>()
                .Where(message => message.Id == messageId)
                .List()
                .FirstOrDefault();
        }

        public void DeleteMessageSent(int messageId) {
            UserMessagePart userMessagePart = GetMessage(messageId);
            if (!userMessagePart.SenderRemoved) {
                // Remove just by the sender
                userMessagePart.SenderRemoved = true;
            }
            else if (userMessagePart.ReceiverRemoved) {
                // If it was already removed by the sender and also removed by
                // the receiver, remove the message for good
                _contentManager.Remove(GetMessage(messageId).ContentItem);
            }
        }

        public void DeleteMessageReceived(int messageId) {
            UserMessagePart userMessagePart = GetMessage(messageId);
            if (!userMessagePart.ReceiverRemoved) {
                // Remove just by the receiver
                userMessagePart.ReceiverRemoved = true;
            }
            else if(userMessagePart.SenderRemoved) {
                // If it was already removed by the receiver and also removed by
                // the sender, remove the message for good
                _contentManager.Remove(GetMessage(messageId).ContentItem);
            }
        }

        public void SendMessage(UserMessagePart userMessagePart) {
            if (_membershipService.GetUser(userMessagePart.SentTo) == null) {
                throw new OrchardException(T("Invalid user"));
            }

            userMessagePart.SentFrom = _orchardServices.WorkContext.CurrentUser.UserName;
            userMessagePart.SentDateTime = DateTime.UtcNow;
            _contentManager.Create(userMessagePart);
        }
    }
}