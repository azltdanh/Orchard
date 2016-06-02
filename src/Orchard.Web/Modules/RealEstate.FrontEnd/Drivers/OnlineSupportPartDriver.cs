using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstateForum.Service.Models;
using RealEstateForum.Service.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class OnlineSupportPartDriver : ContentPartDriver<OnlineSupportWidgetPart>
    {
        public OnlineSupportPartDriver(IOrchardServices services)
        {
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(OnlineSupportWidgetPart part, string displayType, dynamic shapeHelper)
        {
            SupportOnlineConfigPart supportonline = Services.ContentManager
                .Query<SupportOnlineConfigPart, SupportOnlineConfigPartRecord>()
                .List()
                .OrderByDescending(c => c.Id)
                .FirstOrDefault();

            var viewModel = new SupportOnlineWidgetViewModel {SupportOnlineModel = supportonline};

            return ContentShape("Parts_OnlineSupportWidget",
                () => shapeHelper.DisplayTemplate(
                    TemplateName: "Parts/OnlineSupportWidget",
                    Model: viewModel,
                    Prefix: Prefix));
        }
    }
}