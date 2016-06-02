using System.Collections.Generic;
using Contrib.UserMessage.Models;
using Orchard;
using Orchard.Security;

namespace Contrib.UserMessage.Services {
    public interface IUserMessageService : IDependency {
        IEnumerable<UserMessagePart> GetReceivedMessages(IUser user);
        IEnumerable<UserMessagePart> GetSentMessages(IUser user);

        void SendMessage(UserMessagePart userMessagePart);
        UserMessagePart GetMessage(int messageId);
        void DeleteMessageSent(int messageId);
        void DeleteMessageReceived(int messageId);
    }
}