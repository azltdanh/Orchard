using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Settings;
using Piedone.Facebook.Suite.Models;
using Orchard.Environment;

namespace Piedone.Facebook.Suite.Handlers
{
    [OrchardFeature("Piedone.Facebook.Suite.Connect")]
    public class FacebookConnectPopupWidgetPartHandler : ContentHandler
    {
        public FacebookConnectPopupWidgetPartHandler(Work<ISiteService> siteServiceWork)
        {
            OnActivated<FacebookConnectPopupWidgetPart>((context, part) =>
                {
                    part.PermissionsField.Loader(() => siteServiceWork.Value.GetSiteSettings().As<FacebookConnectSettingsPart>().Permissions);
                });
        }
    }
}