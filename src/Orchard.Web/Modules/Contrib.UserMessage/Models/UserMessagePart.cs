using System;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Contrib.UserMessage.Models {
    public class UserMessagePart : ContentPart<UserMessagePartRecord> {
        
        public string SentFrom
        {
            get { return Record.SentFrom; }
            set { Record.SentFrom = value; }
        }

        [Required, StringLength(128)]
        public string SentTo {
            get { return Record.SentTo; }
            set { Record.SentTo = value; }
        }

        [Required, StringLength(256)]
        public string Title {
            get { return Record.Title; }
            set { Record.Title = value; }
        }

        [Required, StringLength(500)]
        public string Message
        {
            get { return Record.Message; }
            set { Record.Message = value; }
        }

        public DateTime SentDateTime {
            get { return Record.SentDateTime; }
            set { Record.SentDateTime = value; }
        }

        public bool SenderRemoved {
            get { return Record.SenderRemoved; }
            set { Record.SenderRemoved = value; }
        }

        public bool ReceiverRemoved {
            get { return Record.ReceiverRemoved; }
            set { Record.ReceiverRemoved = value; }
        }

        public bool ReceiverRead
        {
            get { return Record.ReceiverRead; }
            set { Record.ReceiverRead = value; }
        }

        //private readonly LazyField<IContent> _previousMsg = new LazyField<IContent>();

        //public LazyField<IContent> PreviousMessageField { get { return _previousMsg; } }

        public UserMessagePartRecord PreviousMessage
        {
            get { return Record.PreviousMessage; }
            set { Record.PreviousMessage = value; }
        }
    }
}