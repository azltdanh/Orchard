using Mello.ImageGallery.Models.Plugins.NoneSlide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.FlashSwf
{
    public class FlashSwfSlide : ImageGalleryPlugin
    {
        private readonly FlashSwfSlideSetting _settings;

        public FlashSwfSlide(FlashSwfSlideSetting flashSwfSettings)
      {
          _settings = flashSwfSettings;
      }

      public override string ToString(string cssSelector) {
          return "";//string.Format("$('{0}').cycle({1})", cssSelector, _settings);
      }

      public override string ImageGalleryTemplateName {
          get { return "Parts/Plugins/FlashSwf"; }
      }
    }
}