using Orchard.UI.Resources;
using System;

namespace RealEstate
{
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();

            manifest.DefineScript("jqueryAutoNumeric").SetUrl("autoNumeric.min.js").SetDependencies("jQuery");

            manifest.DefineScript("jquery.flexslider").SetUrl("jquery.flexslider-min.js").SetDependencies("jQuery");
            manifest.DefineScript("jqueryValidate").SetUrl("jquery.validate.min.js").SetDependencies("jQuery");
            manifest.DefineScript("jqueryValidate-additional").SetUrl("additional-methods.min.js").SetDependencies("jqueryValidate");

            manifest.DefineStyle("PrintCss").SetUrl("print.css").AddAttribute("media", "print");
            
            /*RealEstate.FrontEnd Property*/
            manifest.DefineScript("FrontEnd.Common").SetUrl("realestate.frontend.common.min.js", "realestate.frontend.common.js").SetDependencies("jQuery");
            manifest.DefineScript("FrontEnd.Property.Edit").SetUrl("realestate.frontend.property.edit.min.js", "realestate.frontend.property.edit.js").SetDependencies("jQuery", "BootstrapCombobox", "jqueryAutoNumeric", "jqueryValidate");
            manifest.DefineScript("FrontEnd.Property.Filter").SetUrl("realestate.frontend.property.filter.min.js", "realestate.frontend.property.filter.js").SetDependencies("jQuery", "BootstrapCombobox", "BootstrapMultiselect", "jqueryAutoNumeric");

            manifest.DefineStyle("FrontEnd.ViewMap").SetUrl("realestate.frontend.viewmap.css");
            manifest.DefineStyle("FrontEnd.ViewMap.min").SetUrl("realestate.frontend.viewmap.min.css");
            manifest.DefineStyle("FrontEnd.ViewMap.IE").SetUrl("realestate.frontend.viewmap.ie.css");
            manifest.DefineScript("FrontEnd.ViewMap").SetUrl("realestate.frontend.viewmap-src.js").SetDependencies("jQuery");
            manifest.DefineScript("FrontEnd.ViewMap.min").SetUrl("realestate.frontend.viewmap.min.js").SetDependencies("jQuery");

        }
    }
}
