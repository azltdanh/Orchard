using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mello.ImageGallery.Models.Plugins.NoneSlide
{
    public class NoneSlide : ImageGalleryPlugin
    {
        private readonly NoneSlideSetting _settings;

        public NoneSlide(NoneSlideSetting sliderCycleSettings)
      {
            _settings = sliderCycleSettings;
      }

      public override string ToString(string cssSelector) {
          return "";//string.Format("$('{0}').cycle({1})", cssSelector, _settings);
      }

      public override string ImageGalleryTemplateName {
          get { return "Parts/Plugins/NoneSlider"; }
      }
    }
}