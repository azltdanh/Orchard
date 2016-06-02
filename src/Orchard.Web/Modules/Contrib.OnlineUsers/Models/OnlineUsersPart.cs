using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Contrib.OnlineUsers.Models
{
    public class OnlineUsersPart : ContentPart
    {
        public int? UserCount { get; set; }
        public int? VisitorCount { get; set; }
    }
}