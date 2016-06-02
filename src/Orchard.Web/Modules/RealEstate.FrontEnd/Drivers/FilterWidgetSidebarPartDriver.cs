using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealEstate.FrontEnd.Drivers
{
    public class FilterWidgetSidebarPartDriver : ContentPartDriver<FilterWidgetSidebarPart>
    {
        public FilterWidgetSidebarPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }
        private IFastFilterService _fastfilterservice;

        protected override DriverResult Display(FilterWidgetSidebarPart part, string displayType, dynamic shapeHelper)
        {
            var model = new PropertyDisplayIndexOptions();
            return ContentShape("Parts_FilterWidgetSidebarPart",
                () =>
                {
                    var shape = shapeHelper.Parts_FilterWidgetSidebarPart();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitFilterWidget(model);
                    return shape;
                });
        }
    }
}
