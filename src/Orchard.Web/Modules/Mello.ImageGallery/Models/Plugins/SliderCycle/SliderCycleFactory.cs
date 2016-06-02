using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.SliderCycle
{
    public class SliderCycleFactory : PluginFactory
    {
        public override ImageGalleryPlugin Plugin
        {
            get { return new SliderCycle(new SliderCycleSetting()); }
        }

        public override PluginResourceDescriptor PluginResourceDescriptor
        {
            get { return new SliderCycleResourceDescriptor(); }
        }
    }
}