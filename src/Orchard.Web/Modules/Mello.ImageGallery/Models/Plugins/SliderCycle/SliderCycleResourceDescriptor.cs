using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.SliderCycle
{
    public class SliderCycleResourceDescriptor : PluginResourceDescriptor
    {
        public SliderCycleResourceDescriptor()
        {
            //AddScript("Scripts/jquery.cycle.all.js");
        }

        public override string PluginName {
          get { return "SliderCycle"; }
        }
    }
}