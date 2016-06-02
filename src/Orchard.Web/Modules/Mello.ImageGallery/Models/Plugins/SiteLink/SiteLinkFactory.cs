using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.SiteLink
{
    public class SiteLinkFactory : PluginFactory
    {
        public override ImageGalleryPlugin Plugin
        {
            get { return new SiteLink(new SiteLinkSetting()); }
        }

        public override PluginResourceDescriptor PluginResourceDescriptor
        {
            get { return new SiteLinkResourceDescriptor(); }
        }
    }
}