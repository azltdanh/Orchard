using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using RealEstate.Models;

namespace RealEstate.Drivers
{
	
    public class HostNamePartDriver : ContentPartDriver<HostNamePart>
    {
        private const string TemplateName = "Parts/HostNamePart";

        public Localizer T { get; set; }

        public HostNamePartDriver()
        {
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(HostNamePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_HostNamePart", () => shapeHelper.Parts_HostNamePart(
                ContentPart: part,
                Name: part.Name,
                ShortName: part.ShortName,
                CssClass: part.CssClass,
                IsEnabled: part.IsEnabled
                ));
        }

        protected override DriverResult Editor(HostNamePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_HostNamePart_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(HostNamePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}