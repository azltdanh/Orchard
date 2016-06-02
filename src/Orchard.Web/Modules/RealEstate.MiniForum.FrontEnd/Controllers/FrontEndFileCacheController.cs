using System.IO;
using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using System.Web.Mvc;

namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    //public interface IFrontEndFileCacheController : IDependency
    //{
    //    string RenderView(object model);
    //}
    public class FrontEndFileCacheController : Controller
    {
        public FrontEndFileCacheController(IOrchardServices services)
        {
            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;

        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public string RenderView(object model)
        {
            Services.Notifier.Information(T("Controller: {0}", ControllerContext));
            string temp = RenderView(ControllerContext, "ThreadContentFileCache", new ViewDataDictionary(model));

            Services.Notifier.Information(T("TEMP: {0}", temp));

            return temp;
        }

        private static string RenderView(ControllerContext controller, string viewName, ViewDataDictionary viewData)
        {
            var controllerContext = controller;

            var viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);

            StringWriter stringWriter;

            using (stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controllerContext,
                    viewResult.View,
                    viewData,
                    controllerContext.Controller.TempData,
                    stringWriter);

                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
            }

            return stringWriter.ToString();
        }
	}
}