using System.Web.Mvc;
using Orchard;
using Orchard.UI.Notify;
using Orchard.DisplayManagement;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Filters;
using Orchard.UI.Resources;
using Piedone.Facebook.Suite.Services;
using Orchard.Localization;

namespace Piedone.Facebook.Suite.Filters
{
    [OrchardFeature("Piedone.Facebook.Suite")]
    public class FacebookSuiteFilter : FilterProvider, IResultFilter
    {
        private readonly IResourceManager _resourceManager;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;
        private readonly IFacebookSuiteService _facebookSuiteService;

        public FacebookSuiteFilter(
            IResourceManager resourceManager, 
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory,
            IFacebookSuiteService facebookSuiteService,
            IOrchardServices services)
        {
            _resourceManager = resourceManager;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
            _facebookSuiteService = facebookSuiteService;
            T = NullLocalizer.Instance;
            Services = services;
        }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #region IResultFilter Members

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Should only run on a full view rendering result
            //if (!(filterContext.Result is ViewResult))
            if (filterContext.RouteData.Values["controller"].ToString() != "Account")//(filterContext.RouteData.Values["controller"].ToString() != "PropertySearch" && filterContext.RouteData.Values["action"].ToString() != "RealEstateDetail")
                return;

            _resourceManager.Require("script", "FacebookSuite"); // As Script.AtHead() is not working in FacebookInit shape

            _workContextAccessor.GetContext(filterContext).Layout.Body.Items.Insert( // Include the shape at the beginning of body
                0,
                _shapeFactory.FacebookInit(
                    AppId: _facebookSuiteService.SettingsPart.AppId,
                    Culture: _workContextAccessor.GetContext(filterContext).CurrentCulture
                    )
                );

        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        #endregion
    }
}