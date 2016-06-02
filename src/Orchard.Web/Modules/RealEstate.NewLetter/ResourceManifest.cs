using Orchard.UI.Resources;

namespace RealEstate.Forum {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("newletter.common").SetUrl("newletter.common.js").SetDependencies("jQuery");

            //manifest.DefineStyle("adsstyle").SetUrl("adsstyle.css");
       }
    }
}
