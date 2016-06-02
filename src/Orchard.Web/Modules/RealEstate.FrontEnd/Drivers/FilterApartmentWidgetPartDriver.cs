using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class FilterApartmentWidgetPartDriver : ContentPartDriver<FilterApartmentWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public FilterApartmentWidgetPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(FilterApartmentWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new LocationApartmentDisplayOptions();
            return ContentShape("Parts_FilterApartmentWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_FilterApartmentWidget();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitFilterApartmentWidget(model);
                    return shape;
                });
        }
    }
}