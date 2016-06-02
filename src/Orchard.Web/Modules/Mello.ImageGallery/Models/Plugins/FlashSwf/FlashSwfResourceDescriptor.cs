using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.FlashSwf
{
    public class FlashSwfResourceDescriptor : PluginResourceDescriptor
    {
        public FlashSwfResourceDescriptor()
        {
            //AddScript("Scripts/jquery.cycle.all.js");
        }

        public override string PluginName {
            get { return "FlashSwf"; }
        }
    }
}