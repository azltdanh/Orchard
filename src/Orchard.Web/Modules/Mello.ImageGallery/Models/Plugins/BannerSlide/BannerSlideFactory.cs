using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.BannerSlide
{
    public class BannerSlideFactory : PluginFactory
    {
        public override ImageGalleryPlugin Plugin
        {
            get { return new BannerSlideSlide(new BannerSlideSetting()); }
        }

        public override PluginResourceDescriptor PluginResourceDescriptor
        {
            get { return new BannerSlideResourceDescriptor(); }
        }
    }
}