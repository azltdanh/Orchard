using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.API.ViewModels
{
    public class NavItem
    {
        public string ItemName { get; set; }
        public string ItemUrl { get; set; }
        public List<NavItem> Items { get; set; }
    }
}