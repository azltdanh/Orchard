using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.FrontEnd.Drivers
{
    public class ApartmentHighlightsPartDriver : ContentPartDriver<ApartmentHighlightsWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;
        private readonly IPropertyService _propertyService;
        private readonly RequestContext _requestContext;

        public ApartmentHighlightsPartDriver(RequestContext requestContext, IFastFilterService fastfilterservice,
            IPropertyService propertyService)
        {
            _requestContext = requestContext;
            _fastfilterservice = fastfilterservice;
            _propertyService = propertyService;
        }

        protected override DriverResult Display(ApartmentHighlightsWidgetPart part, string displayType,
            dynamic shapeHelper)
        {
            var model = new LocationApartmentDisplayOptions();
            return ContentShape("Parts_ApartmentHighlightsWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_ApartmentHighlightsWidget();
                    shape.ContentPart = part;
                    shape.ViewModel = _fastfilterservice.InitApartmentHighlightsWidget(model);
                    return shape;
                });
        }
    }
}