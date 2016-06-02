using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using Orchard.ContentManagement;

namespace RealEstate.NewLetter.Models
{
    public class MessageInboxPartRecord : ContentPartRecord
    {
        public virtual int? ParentId { get; set; }
        public virtual int? PreviousId { get; set; }
        public virtual UserPartRecord UserSend { get; set; }
        public virtual UserPartRecord UserReceived { get; set; }
        public virtual string Title { get; set; }
        public virtual string Content { get; set; }
        public virtual DateTime? DateSend { get; set; }
        public virtual bool IsRead { get; set; }
        public virtual bool IsAdmin { get; set; }//True: Admin <=> User, False: User <=> User
        public virtual bool ReadByStaff { get; set; }//True: Inbox , False: Send 
        public virtual bool IsUserDelete { get; set; }// True : User đã xóa, False: Bình thường. [Trường hợp tin nhắn admin]
        public virtual string HostName { get; set; }
    }
    public class MessageInboxPart : ContentPart<MessageInboxPartRecord>
    {
        public int? ParentId
        {
            get { return Retrieve(r=>r.ParentId); }
            set { Store(r=>r.ParentId,value); }
        }
        public int? PreviousId
        {
            get { return Retrieve(r=>r.PreviousId); }
            set { Store(r=>r.PreviousId,value); }
        }
        public UserPartRecord UserSend
        {
            get { return Record.UserSend; }
            set { Record.UserSend = value; }
        }
        public UserPartRecord UserReceived
        {
            get { return Record.UserReceived; }
            set { Record.UserReceived = value; }
        }
        public string Title
        {
            get { return Retrieve(r=>r.Title); }
            set { Store(r=>r.Title, value); }
        }
        public string Content
        {
            get { return Retrieve(r=>r.Content); }
            set { Store(r=>r.Content,value); }
        }
        public DateTime? DateSend 
        {
            get { return Retrieve(r=>r.DateSend); }
            set { Store(r=>r.DateSend,value); }
        }
        public bool IsRead
        {
            get { return Retrieve(r=>r.IsRead); }
            set { Store(r=>r.IsRead,value); }
        }
        public bool IsAdmin//True: Admin <=> User, False: User <=> User
        {
            get { return Retrieve(r=>r.IsAdmin); }
            set { Store(r=>r.IsAdmin,value); }
        }
        public bool ReadByStaff//True: Inbox , False: Send
        {
            get { return Retrieve(r=>r.ReadByStaff); }
            set { Store(r=>r.ReadByStaff,value); }
        }
        public bool IsUserDelete // True : User đã xóa, False: Bình thường. [Trường hợp tin nhắn admin]
        {
            get { return Retrieve(r=>r.IsUserDelete); }
            set { Store(r=>r.IsUserDelete,value); }
        }
        public string HostName
        {
            get { return Retrieve(r => r.HostName); }
            set { Store(r => r.HostName, value); }
        }
    }
}