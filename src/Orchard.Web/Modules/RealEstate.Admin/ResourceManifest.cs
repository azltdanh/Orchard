using Orchard.UI.Resources;

namespace RealEstate
{
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("Unobtrusive").SetUrl("jquery.unobtrusive-ajax.min.js").SetDependencies("jQuery")
                .SetCdn("//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.unobtrusive-ajax.min.js", "//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.unobtrusive-ajax.js", true);

            manifest.DefineScript("ValidateUnobtrusive").SetUrl("jquery.validate.unobtrusive.min.js").SetDependencies("jQuery")
                .SetCdn("//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.validate.unobtrusive.min.js", "//ajax.aspnetcdn.com/ajax/mvc/3.0/jquery.validate.unobtrusive.js", true);

            manifest.DefineScript("MicrosoftAjax").SetUrl("MicrosoftAjax.js").SetDependencies("jQuery")
                .SetCdn("//ajax.aspnetcdn.com/ajax/4.0/1/MicrosoftAjax.js", "//ajax.aspnetcdn.com/ajax/4.0/1/MicrosoftAjax.debug.js", true);

            manifest.DefineScript("MicrosoftMvcAjax").SetUrl("MicrosoftMvcAjax.js").SetDependencies("jQuery")
                .SetCdn("//ajax.aspnetcdn.com/ajax/mvc/3.0/MicrosoftMvcAjax.js", "//ajax.aspnetcdn.com/ajax/mvc/3.0/MicrosoftMvcAjax.debug.js", true);

            manifest.DefineScript("Markdown.Converter").SetUrl("Markdown.Converter.min.js");
            manifest.DefineScript("Markdown.Editor").SetUrl("Markdown.Editor.js").SetDependencies("Markdown.Converter");
            manifest.DefineScript("Markdown.Sanitizer").SetUrl("Markdown.Sanitizer.min.js");

            manifest.DefineScript("Cookie").SetUrl("jquery.cookie.js").SetDependencies("jQuery");
            manifest.DefineScript("TimeAgo").SetUrl("jquery.timeago.1.3.js").SetDependencies("jQuery");
            manifest.DefineScript("Common").SetUrl("realestate.admin.common.min.js", "realestate.admin.common.js").SetDependencies("jQuery");

            manifest.DefineScript("Floatheader").SetUrl("jquery.float.headers.js").SetDependencies("jQueryUI", "jQueryMigrate");

            manifest.DefineScript("Rating").SetUrl("jquery-star-rating/jquery.rating.pack.js").SetDependencies("jQuery", "jQueryMigrate");
            manifest.DefineStyle("Rating").SetUrl("~/Modules/RealEstate.Admin/Scripts/jquery-star-rating/jquery.rating.css");

            manifest.DefineScript("ZoomAssets").SetUrl("zoom_assets/zoom.min.js").SetDependencies("jQuery", "jQueryMigrate");
            manifest.DefineStyle("ZoomAssets").SetUrl("~/Modules/RealEstate.Admin/Scripts/zoom_assets/css/zoom.css");

            manifest.DefineScript("PrettyPhoto").SetUrl("prettyPhoto/js/jquery.prettyPhoto.js").SetDependencies("jQuery", "jQueryMigrate");
            manifest.DefineStyle("PrettyPhoto").SetUrl("~/Modules/RealEstate.Admin/Scripts/prettyPhoto/css/prettyPhoto.css");

            manifest.DefineScript("jqPrint").SetUrl("jquery.jqprint-0.3.js").SetDependencies("jQuery");
            manifest.DefineScript("selectboxes").SetUrl("jquery.selectboxes.min.js");

            manifest.DefineScript("Uploadify").SetUrl("uploadify-v3.2/jquery.uploadify.min.js").SetDependencies("jQuery");
            manifest.DefineStyle("Uploadify").SetUrl("~/Modules/RealEstate.Admin/Scripts/uploadify-v3.2/uploadify.css");

            manifest.DefineScript("UploadiFive").SetUrl("uploadifive-1.1.2/jquery.uploadifive.min.js").SetDependencies("jQuery");
            manifest.DefineStyle("UploadiFive").SetUrl("~/Modules/RealEstate.Admin/Scripts/uploadifive-1.1.2/uploadifive.css");

            manifest.DefineStyle("Bootstrap").SetUrl("~/Modules/RealEstate.Admin/Scripts/bootstrap/css/bootstrap.min.css");
            manifest.DefineScript("Bootstrap").SetUrl("~/Modules/RealEstate.Admin/Scripts/bootstrap/js/bootstrap.min.js").SetDependencies("jQuery");

            manifest.DefineStyle("BootstrapCombobox").SetUrl("~/Modules/RealEstate.Admin/Scripts/bootstrap/bootstrap-combobox.min.css", "~/Modules/RealEstate.Admin/Scripts/bootstrap/bootstrap-combobox.css");
            manifest.DefineScript("BootstrapCombobox").SetUrl("bootstrap/bootstrap-combobox.min.js", "bootstrap/bootstrap-combobox.js").SetDependencies("Bootstrap");
            manifest.DefineScript("BootstrapComboboxFilterOnMobile").SetUrl("bootstrap/BootstrapComboboxFilterOnMobile.min.js", "bootstrap/BootstrapComboboxFilterOnMobile.js").SetDependencies("Bootstrap");

            manifest.DefineStyle("BootstrapMultiselect").SetUrl("~/Modules/RealEstate.Admin/Scripts/bootstrap/bootstrap-multiselect.min.css", "~/Modules/RealEstate.Admin/Scripts/bootstrap/bootstrap-multiselect.css");
            manifest.DefineScript("BootstrapMultiselect").SetUrl("bootstrap/bootstrap-multiselect.min.js", "bootstrap/bootstrap-multiselect.js").SetDependencies("Bootstrap");

            manifest.DefineStyle("BootstrapSelectPicker").SetUrl("~/Modules/RealEstate.Admin/Scripts/bootstrap/bootstrap-select.min.css");

            manifest.DefineScript("BootstrapTypeAhead").SetUrl("~/Modules/RealEstate.Admin/Scripts/bootstrap/typeahead.bundle.min.js").SetDependencies("jQuery");

            manifest.DefineScript("json2").SetUrl("~/Modules/RealEstate.Admin/Scripts/select2-3.5.2/json2.js");
            manifest.DefineScript("jquery.mousewheel").SetUrl("~/Modules/RealEstate.Admin/Scripts/select2-3.5.2/jquery.mousewheel.js").SetDependencies("jQuery", "jQueryUI");

            manifest.DefineStyle("prettify").SetUrl("~/Modules/RealEstate.Admin/Scripts/select2-3.5.2/prettify.css");
            manifest.DefineScript("prettify").SetUrl("~/Modules/RealEstate.Admin/Scripts/select2-3.5.2/prettify.min.js");

            manifest.DefineStyle("Select2").SetUrl("~/Modules/RealEstate.Admin/Scripts/select2-3.5.2/select2.css");
            manifest.DefineScript("Select2").SetUrl("~/Modules/RealEstate.Admin/Scripts/select2-3.5.2/select2.min.js").SetDependencies("Bootstrap");

            manifest.DefineStyle("RealEstate_Admin").SetUrl("menu.RealEstate-admin.min.css","menu.RealEstate-admin.css");

            //manifest.DefineScript("AngularJs").SetUrl("angular.min.js");
        }
    }
}
