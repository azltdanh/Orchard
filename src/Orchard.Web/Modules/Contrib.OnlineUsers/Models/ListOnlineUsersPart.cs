using Contrib.OnlineUsers.ViewModels;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contrib.OnlineUsers.Models
{
    public class ListOnlineUsersPart : ContentPart
    {
        public virtual OnlineUsersViewModel ViewModel { get; set; }
    }
}
