using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.NoneSlide
{
    public class NoneSlideFactory : PluginFactory
    {
        public override ImageGalleryPlugin Plugin
        {
            get { return new NoneSlide(new NoneSlideSetting()); }
        }

        public override PluginResourceDescriptor PluginResourceDescriptor
        {
            get { return new NoneSlideResourceDescriptor(); }
        }
    }
}