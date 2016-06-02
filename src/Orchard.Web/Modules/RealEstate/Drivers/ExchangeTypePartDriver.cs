using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstate.Models;

namespace RealEstate.Drivers
{
    public class ExchangeTypePartDriver : ContentPartDriver<ExchangeTypePart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/ExchangeTypePart";

        public Localizer T { get; set; }

        public ExchangeTypePartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ExchangeTypePart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ExchangeTypePart",
                () => shapeHelper.Parts_ExchangeTypePart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ExchangeTypePart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_ExchangeTypePart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ExchangeTypePart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("ExchangeTypePart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during ExchangeTypePart update!"));
            }
            return Editor(part, shapeHelper);
        }
    }
}
