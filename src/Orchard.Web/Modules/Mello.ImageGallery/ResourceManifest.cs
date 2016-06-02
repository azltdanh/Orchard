using Orchard.UI.Resources;

namespace Mello.ImageGallery {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("ImageGalleryAdmin").SetUrl("image-gallery-admin.css");
            builder.Add().DefineStyle("ImageGallery").SetUrl("image-gallery.css");

            builder.Add().DefineScript("jQueryMultiFile").SetDependencies("jquery").SetUrl("jquery.MultiFile.pack.js");
            builder.Add().DefineScript("jQueryUISortable").SetDependencies("jquery").SetUrl("sortable-interaction-jquery-ui-1.8.10.custom.min.js");

            //builder.Add().DefineScript("jquery-noneslide").SetDependencies("jquery").SetUrl("jquery-noneslide.js");
            // add to themed
            builder.Add().DefineScript("jquery.cycle.all").SetUrl("jquery.cycle.all.min.js").SetDependencies("jQuery");
            builder.Add().DefineScript("jquery.slideViewerPro").SetUrl("jquery.slideViewerPro.1.5.js").SetDependencies("jQuery");
            builder.Add().DefineScript("jquery.timers").SetUrl("jquery.timers-1.2.js").SetDependencies("jQuery");
            builder.Add().DefineScript("jquery.sitelink").SetUrl("sitelink.js").SetDependencies("jQuery");

            builder.Add().DefineStyle("svwp_style").SetUrl("svwp_style.css");
        }
    }
}