using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Contrib.OnlineUsers.Models;

namespace Contrib.OnlineUsers.Handlers
{
    public class MembershipHandler:ContentHandler
    {
        public MembershipHandler(IRepository<MembershipPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}