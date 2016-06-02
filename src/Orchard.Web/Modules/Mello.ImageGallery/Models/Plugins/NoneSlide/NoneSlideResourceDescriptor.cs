using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.NoneSlide
{
    public class NoneSlideResourceDescriptor : PluginResourceDescriptor
    {
        public NoneSlideResourceDescriptor()
        {
            //AddScript("Scripts/jquery.cycle.all.js");
        }

        public override string PluginName {
            get { return "NoneSlide"; }
        }
    }
}