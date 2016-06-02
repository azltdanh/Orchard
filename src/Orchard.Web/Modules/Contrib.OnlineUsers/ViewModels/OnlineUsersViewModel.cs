using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contrib.OnlineUsers.ViewModels
{
    public class OnlineUsersViewModel
    {
        public IList<OnlineUsersEntry> OnlineUsers { get; set; }
        public int TotalCount { get; set; }
        public dynamic Pager { get; set; }
    }
    public class OnlineUsersEntry
    {
        public UserPartRecord OnlineUser { get; set; }
        public string UserName { get; set; }
        public int id { get; set; }
        public string Email { get; set; }
        public string UserDisplayName { get; set; }
        public bool IsChecked { get; set; }
    }
}
