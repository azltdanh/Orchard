using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class FilterHeaderPartDriver : ContentPartDriver<FilterHeaderPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public FilterHeaderPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(FilterHeaderPart part, string displayType, dynamic shapeHelper)
        {
            var model = new PropertyDisplayIndexOptions();
            return ContentShape("Parts_FilterHeader",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_FilterHeader();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitFilterWidget(model);
                    return shape;
                });
        }
    }
}