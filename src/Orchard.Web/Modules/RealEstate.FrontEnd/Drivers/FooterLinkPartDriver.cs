using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class FooterLinkPartDriver : ContentPartDriver<FooterLinkWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;

        public FooterLinkPartDriver(IFastFilterService fastfilterservice)
        {
            _fastfilterservice = fastfilterservice;
        }

        protected override DriverResult Display(FooterLinkWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new PropertyDisplayIndexOptions();
            return ContentShape("Parts_FooterLinkWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_FooterLinkWidget();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitFilterWidget(model);
                    return shape;
                });
        }
    }
}