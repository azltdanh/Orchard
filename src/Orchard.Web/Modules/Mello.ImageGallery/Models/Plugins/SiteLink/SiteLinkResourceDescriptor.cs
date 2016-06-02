using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.SiteLink
{
    public class SiteLinkResourceDescriptor : PluginResourceDescriptor
    {
        public SiteLinkResourceDescriptor()
        {
            //AddScript("Scripts/jquery.cycle.all.js");
        }

        public override string PluginName {
            get { return "SiteLink"; }
        }
    }
}