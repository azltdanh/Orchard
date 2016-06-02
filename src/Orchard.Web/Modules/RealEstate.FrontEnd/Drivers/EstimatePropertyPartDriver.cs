using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.FrontEnd.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class EstimatePropertyPartDriver : ContentPartDriver<EstimatePropertyWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public EstimatePropertyPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(EstimatePropertyWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new EstimateWidgetViewModel();
            return ContentShape("Parts_EstimatePropertyWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_EstimatePropertyWidget();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitEstimateWidget(model);
                    return shape;
                });
        }
    }
}