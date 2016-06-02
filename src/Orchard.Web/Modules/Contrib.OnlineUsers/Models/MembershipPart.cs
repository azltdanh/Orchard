using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Contrib.OnlineUsers.Models
{
    public class MembershipPart : ContentPart<MembershipPartRecord>
    {
        public DateTime? LastActive
        {
            get { return Retrieve(r => r.LastActive); }
            set { Store(r => r.LastActive, value); }
        }
    }
}