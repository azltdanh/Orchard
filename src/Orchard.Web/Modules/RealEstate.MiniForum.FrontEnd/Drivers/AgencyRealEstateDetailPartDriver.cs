using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Services;
using RealEstate.MiniForum.FrontEnd.Models;
using RealEstate.ViewModels;
using RealEstateForum.Service.Services;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class AgencyRealEstateDetailPartDriver : ContentPartDriver<AgencyRealEstateDetailWidgetPart>
    {
        public AgencyRealEstateDetailPartDriver(IFastFilterService fastfilterservice)
        {
             _fastfilterservice = fastfilterservice;
        }
        private readonly IFastFilterService _fastfilterservice;

        protected override DriverResult Display(AgencyRealEstateDetailWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new PropertyDisplayEntry();
            return ContentShape("Parts_AgencyRealEstateDetailWidget", () =>
            {
                var shape = shapeHelper.Parts_AgencyRealEstateDetailWidget();
                //shape.ContentPart = part;
                shape.ViewModel = _fastfilterservice.InitUserLocationWidget(model);
                return shape;
            });
        }
    }
}
