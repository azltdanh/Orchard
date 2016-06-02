using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class MapPlanningPropertyPartDriver : ContentPartDriver<MapPlanningWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public MapPlanningPropertyPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(MapPlanningWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new PlanningMapIndexOptions();
            return ContentShape("Parts_MapPlanningWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_MapPlanningWidget();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitMapPlanningWidget(model);
                    return shape;
                });
        }
    }
}