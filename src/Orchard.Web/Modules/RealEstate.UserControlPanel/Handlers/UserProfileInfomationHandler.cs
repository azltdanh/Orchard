using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.UserControlPanel.Models;

namespace RealEstate.UserControlPanel.Handlers
{
    public class UserProfileInfomationHandler : ContentHandler
    {
        public UserProfileInfomationHandler(IRepository<UserProfileInfomationPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}