using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.BannerSlide
{
    public class BannerSlideSlide : ImageGalleryPlugin
    {
        private readonly BannerSlideSetting _settings;

        public BannerSlideSlide(BannerSlideSetting bannerSlideSettings)
      {
          _settings = bannerSlideSettings;
      }

      public override string ToString(string cssSelector) {
          return "";//string.Format("$('{0}').cycle({1})", cssSelector, _settings);
      }

      public override string ImageGalleryTemplateName {
          get { return "Parts/Plugins/BannerSlide"; }
      }
    }
}