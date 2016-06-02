using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;

namespace Contrib.OnlineUsers.Models
{
    public class MembershipPartRecord : ContentPartRecord
    {
        public virtual DateTime? LastActive { get; set; }
    }
}