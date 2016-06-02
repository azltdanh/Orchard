using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Drivers
{
    public class FilterRulesPartDriver : ContentPartDriver<FilterRulesPart>
    {
        private readonly INotifier _notifier;
        private const string TemplateName = "Parts/FilterRulesPart";

        public Localizer T { get; set; }

        public FilterRulesPartDriver(INotifier notifier)
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(FilterRulesPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_FilterRulesPart",
                () => shapeHelper.Parts_FilterRulesPart(ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(FilterRulesPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_FilterRulesPart",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(FilterRulesPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}