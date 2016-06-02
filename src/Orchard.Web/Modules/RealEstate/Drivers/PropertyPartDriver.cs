using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    [OrchardFeature("RealEstate")]
    public class PropertyPartDriver : ContentPartDriver<PropertyPart>
    {
        public PropertyPartDriver(IOrchardServices services)
        {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        protected override DriverResult Display(PropertyPart part, string displayType, dynamic shapeHelper)
        {
            //PropertyViewModel viewModel = _propertyService.BuildViewModel(part);
            return ContentShape("Parts_Property",
                () => shapeHelper.Parts_Property(ContentItem: part.ContentItem, ViewModel: part));
        }

        // GET
        protected override DriverResult Editor(PropertyPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Property_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: "Parts/Property", Model: part, Prefix: Prefix));
        }

        // POST
        protected override DriverResult Editor(PropertyPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                //Services.Notifier.Information(T("PropertyPart edited successfully"));
            }
            return Editor(part, shapeHelper);
        }
    }
}