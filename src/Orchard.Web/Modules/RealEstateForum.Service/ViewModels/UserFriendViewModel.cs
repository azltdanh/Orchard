using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealEstateForum.Service.ViewModels;

namespace RealEstateForum.Service.ViewModels
{
    public class UserFriendIndexViewModel
    {
        public List<UserInfo> ListUser { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }
}