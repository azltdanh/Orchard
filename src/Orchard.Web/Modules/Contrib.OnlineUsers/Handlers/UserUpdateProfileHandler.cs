using Contrib.OnlineUsers.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.OnlineUsers.Handlers
{
    public class UserUpdateProfileHandler: ContentHandler
    {
        public UserUpdateProfileHandler(IRepository<UserUpdateProfileRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}