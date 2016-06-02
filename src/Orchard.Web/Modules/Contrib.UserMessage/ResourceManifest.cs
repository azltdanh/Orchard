using Orchard.UI.Resources;

namespace Contrib.UserMessage {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("UserMessage").SetUrl("orchard-contactuser-usermessages.css");
        }
    }
}
