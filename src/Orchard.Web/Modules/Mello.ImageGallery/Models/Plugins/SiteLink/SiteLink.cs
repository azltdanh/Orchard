using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.SiteLink
{
    public class SiteLink : ImageGalleryPlugin
    {
        private readonly SiteLinkSetting _settings;

        public SiteLink(SiteLinkSetting sitelinkSettings)
      {
          _settings = sitelinkSettings;
      }

      public override string ToString(string cssSelector) {
          return "";//string.Format("$('{0}').cycle({1})", cssSelector, _settings);
      }

      public override string ImageGalleryTemplateName {
          get { return "Parts/Plugins/SiteLink"; }
      }
    }
}