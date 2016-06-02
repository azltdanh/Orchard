using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class FilterPartDriver : ContentPartDriver<FilterWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public FilterPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(FilterWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new PropertyDisplayIndexOptions();
            return ContentShape("Parts_FilterWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_FilterWidget();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitFilterWidget(model);
                    return shape;
                });
        }
    }
}