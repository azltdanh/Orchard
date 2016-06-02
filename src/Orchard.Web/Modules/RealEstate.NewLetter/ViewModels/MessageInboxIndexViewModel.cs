using Orchard.ContentManagement;
using RealEstate.NewLetter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.ViewModels
{
    public class ViewMessageInboxViewModel
    {
        public MessageInboxPart ParentMessage { get; set; }
        public IList<MessageInboxPart> ListMessageInboxReply { get; set; }

    }
    public class MessageInboxCreateViewModel
    {
        public int? UserReceive { get; set; }
        public string Title { get; set; }
        [Required(ErrorMessage="Vui lòng nhập nội dung thư")]
        public string Content { get; set; }
        public string ReturnUrl { get; set; }

        public bool IsChecked { get; set; }
        public string Email { get; set; }

        public IContent Messages { get; set; }
    }
    public class MessageInboxIndexViewModel
    {
        public MessageInboxIndexOptions Options { get; set; }
        public IList<MessageInboxEntry> ListMessage { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string ReturnUrl { get; set; }
    }
    public class MessageInboxEntry
    {
        public MessageInboxPart MessagePart { get; set; }
        public int ReplyCount { get; set; }
    }
    public class MessageInboxIndexOptions
    {
        public string Search { get; set; }
        public MessagesFilter Filter { get; set; }
    }
    public enum MessagesFilter
    {
        All,
        UnRead,
        Read
    }
}