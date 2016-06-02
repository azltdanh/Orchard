using System.Linq;
using System.Web.Routing;
using Orchard.ContentManagement.Drivers;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.Services;

namespace RealEstate.FrontEnd.Drivers
{
    public class PropertyHighlightsPartDriver : ContentPartDriver<PropertyHighlightsWidgetPart>
    {
        private readonly IFastFilterService _fastfilterservice;
        private readonly IPropertyService _propertyService;
        private readonly RequestContext _requestContext;

        public PropertyHighlightsPartDriver(RequestContext requestContext, IFastFilterService fastfilterservice,
            IPropertyService propertyService)
        {
            _requestContext = requestContext;
            _fastfilterservice = fastfilterservice;
            _propertyService = propertyService;
        }

        protected override DriverResult Display(PropertyHighlightsWidgetPart part, string displayType,
            dynamic shapeHelper)
        {
            string adsTypeCssClass = "ad-selling";
            if (_requestContext.RouteData.Values["AdsTypeCssClass"] != null)
            {
                adsTypeCssClass = _requestContext.RouteData.Values["AdsTypeCssClass"].ToString();
            }

            return ContentShape("Parts_PropertyHighlightsWidget",
                () =>
                {
                    dynamic shape = shapeHelper.Parts_PropertyHighlightsWidget();
                    shape.ContentPart = part;
                    // preload 2 properties
                    shape.HighlightProperties =
                        _fastfilterservice.GetHighlightProperties(adsTypeCssClass)
                            .Slice(0, 2)
                            .Select(p => _propertyService.BuildPropertyEntryFrontEnd(p))
                            .ToList();
                    return shape;
                });
        }
    }
}