using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Orchard.ContentManagement.Records;
using Orchard.Users.Models;
using Orchard.ContentManagement;

namespace RealEstateForum.Service.Models
{
    public class ForumFriendPartRecord : ContentPartRecord
    {
        public virtual UserPartRecord UserRequest { get; set; }
        public virtual UserPartRecord UserReceived { get; set; }
        public virtual bool Status { get; set; }
        public virtual DateTime? DateRequest { get; set; }
    }
    public class ForumFriendPart : ContentPart<ForumFriendPartRecord>
    {
        public UserPartRecord UserRequest
        {
            get { return Record.UserRequest; }
            set { Record.UserRequest = value; } 
        }
        public UserPartRecord UserReceived
        {
            get { return Record.UserReceived; }
            set { Record.UserReceived = value; }
        }
        public bool Status
        {
            get { return Retrieve(r=> r.Status); }
            set { Store(r=>r.Status,value); } 
        }
        public DateTime? DateRequest
        {
            get { return Retrieve(r=>r.DateRequest); }
            set { Store(r=>r.DateRequest,value); }
        }
    }
}