using Orchard.UI.Resources;

namespace RealEstate.MiniForum.FrontEnd
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            //manifest.DefineScript("JsOOPFrontEnd").SetUrl("JsOOP.js").SetDependencies("jQuery");
            //manifest.DefineScript("common.friendFrontEnd").SetUrl("common.friend.js").SetDependencies("JsOOPFrontEnd");

            manifest.DefineScript("realEstate.miniforum.media").SetUrl("common.forum.media.js").SetDependencies("TinyMce", "Uploadify");
            manifest.DefineScript("realEstate.MiniForum.FrontEnd").SetUrl("common.miniforum.frontend.js").SetDependencies("jQuery");
            //manifest.DefineScript("Mini.CommonForumFrontEnd").SetUrl("common.personalpage.js").SetDependencies("JsOOPFrontEnd");
            //manifest.DefineScript("Mini.js.functionFrontEnd").SetUrl("js.function.js").SetDependencies("JsOOPFrontEnd");
           //manifest.DefineScript("JsPageUser").SetUrl("PageUserProfile.js").SetDependencies("jQuery", "JsOOPFrontEnd");
        }
    }
}