using Orchard.UI.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstate.MiniForum
{
    public class ResourceManifest : IResourceManifestProvider
    {
        public void BuildManifests(ResourceManifestBuilder builder)
        {
            var manifest = builder.Add();
            manifest.DefineScript("realEstate.MiniForum.Admin").SetUrl("realestate.miniforum.admin.js").SetDependencies("jQuery");
            manifest.DefineScript("realEstate.MiniForum.Tinymce").SetUrl("realestate.miniforum.tinymce.js").SetDependencies("TinyMce");
        }
    }
}