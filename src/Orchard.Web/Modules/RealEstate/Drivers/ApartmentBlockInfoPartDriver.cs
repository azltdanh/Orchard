
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;
namespace RealEstate.Drivers
{
    public class ApartmentBlockInfoPartDriver : ContentPartDriver<ApartmentBlockInfoPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ApartmentBlockInfoPart";

        public Localizer T { get; set; }

        public ApartmentBlockInfoPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ApartmentBlockInfoPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ApartmentBlockInfoPart",
                () => shapeHelper.Parts_ApartmentBlockInfoPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ApartmentBlockInfoPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ApartmentBlockInfoPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ApartmentBlockInfoPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("ApartmentBlockInfoPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during ApartmentBlockInfoPart update!"));
            }
            return Editor(part, shapeHelper);
        }
    }
}
