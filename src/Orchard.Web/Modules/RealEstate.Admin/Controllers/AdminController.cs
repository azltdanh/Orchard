using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;

namespace RealEstate.Admin.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        public AdminController(
            IOrchardServices services,
            IShapeFactory shapeFactory)
        {
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}