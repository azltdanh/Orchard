using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.BannerSlide
{
    public class BannerSlideResourceDescriptor : PluginResourceDescriptor
    {
        public BannerSlideResourceDescriptor()
        {
            //AddScript("Scripts/jquery.cycle.all.js");
        }

        public override string PluginName {
            get { return "BannerSlide"; }
        }
    }
}