using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.FlashSwf
{
    public class FlashSwfFactory : PluginFactory
    {
        public override ImageGalleryPlugin Plugin
        {
            get { return new FlashSwfSlide(new FlashSwfSlideSetting()); }
        }

        public override PluginResourceDescriptor PluginResourceDescriptor
        {
            get { return new FlashSwfResourceDescriptor(); }
        }
    }
}