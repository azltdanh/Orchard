using System;
using Orchard.ContentManagement.Records;

namespace Contrib.UserMessage.Models {
    public class UserMessagePartRecord : ContentPartRecord {
        public virtual string SentFrom { get; set; }
        public virtual string SentTo { get; set; }
        public virtual string Title { get; set; }
        public virtual string Message { get; set; }
        public virtual DateTime SentDateTime { get; set; }
        public virtual bool SenderRemoved { get; set; }
        public virtual bool ReceiverRemoved { get; set; }
        public virtual bool ReceiverRead { get; set; }
        public virtual UserMessagePartRecord PreviousMessage { get; set; }
    }
}