using System.Linq;
using System.Web.Mvc;

using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Themes;
using RealEstate.Services;


namespace RealEstate.UserControlPanel.Controllers
{
    [Themed]
    public class ContactController : Controller, IUpdateModel
    {
        private readonly IAdsPaymentHistoryService _adsPaymentService;
        public ContactController(IOrchardServices services, IShapeFactory shapeFactory, IAdsPaymentHistoryService adsPaymentService)
        {
            _adsPaymentService = adsPaymentService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }
        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult AjaxLoadAdsPrice()
        {
            var price = _adsPaymentService.GetPaymentConfigs().Where(r => r.CssClass != "ins-money").OrderBy(r => r.Value).ToDictionary(r=> r.CssClass,x => x.Value);

            ViewBag.Price = price;
            return PartialView();
        }

        public ActionResult AjaxLoadContactOnline()
        {
            return PartialView();
        }

        public ActionResult AjaxLoadValuationCertificatePrice()
        {
            return PartialView();
        }
        //public ActionResult AjaxLoadBannerPrice()
        //{
        //    return PartialView();
        //}

        #region UpdateModel

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        #endregion
    }
}
