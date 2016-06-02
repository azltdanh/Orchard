using Mello.ImageGallery.Models.Plugins.BannerSlide;
using Mello.ImageGallery.Models.Plugins.FlashSwf;
using Mello.ImageGallery.Models.Plugins.LightBox;
using Mello.ImageGallery.Models.Plugins.NoneSlide;
using Mello.ImageGallery.Models.Plugins.PrettyPhoto;
using Mello.ImageGallery.Models.Plugins.SiteLink;
using Mello.ImageGallery.Models.Plugins.SliderCycle;
using Mello.ImageGallery.Models.Plugins.SlideViewerPro;

namespace Mello.ImageGallery.Models.Plugins {
    public abstract class PluginFactory {
        public static PluginFactory GetFactory(Plugin plugin) {
            if (plugin == Plugins.Plugin.PrettyPhoto) {
                return new PrettyPhotoFactory();
            }
            if (plugin == Plugins.Plugin.SlideViewerPro) {
            return new SlideViewerProFactory();
            }
            if (plugin == Plugins.Plugin.SliderCycle)
            {
                return new SliderCycleFactory();
            }
            if (plugin == Plugins.Plugin.NoneSlide)
            {
                return new NoneSlideFactory();
            }
            if (plugin == Plugins.Plugin.FlashSwf)
            {
                return new FlashSwfFactory();
            }
            if (plugin == Plugins.Plugin.SiteLink)
            {
                return new SiteLinkFactory();
            }
            if (plugin == Plugins.Plugin.BannerSlide)
            {
                return new BannerSlideFactory();
            }
            return new LightBoxFactory();
        }

        public abstract ImageGalleryPlugin Plugin { get; }

        public abstract PluginResourceDescriptor PluginResourceDescriptor { get; }
    }
}