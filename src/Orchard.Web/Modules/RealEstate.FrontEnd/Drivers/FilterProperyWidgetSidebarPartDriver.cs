using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class FilterProperyWidgetSidebarPartDriver : ContentPartDriver<FilterProperyWidgetSidebarPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public FilterProperyWidgetSidebarPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(FilterProperyWidgetSidebarPart part, string displayType,
            dynamic shapeHelper)
        {
            var model = new PropertyDisplayIndexOptions();
            return ContentShape("Parts_FilterPropertyWidgetSidebar",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_FilterPropertyWidgetSidebar();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitFilterWidget(model);
                    return shape;
                });
        }
    }
}