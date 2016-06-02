using Orchard;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.UI.Notify;
using RealEstate.NewLetter.Models;
using RealEstate.NewLetter.Services;
using RealEstate.NewLetter.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Themes;

namespace RealEstate.NewLetter.Controllers
{
    [Themed]
    public class VerifyCustomerExceptionController : Controller, IUpdateModel
    {
        #region Init

        private readonly ISiteService _siteService;
        private readonly INewCustomerService _newcustomerservice;
        private readonly INewletterMessageService _newletterservice;

        public VerifyCustomerExceptionController(INewletterMessageService newletterservice, INewCustomerService newcustomerservice, IOrchardServices services, IShapeFactory shapeFactory, ISiteService siteService)
        {
            Services = services;
            _siteService = siteService;
            _newcustomerservice = newcustomerservice;
            _newletterservice = newletterservice;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion


        public ActionResult VerifyExeptionEmail(string code)
        {
            var customer = Services.ContentManager.Query<CustomerEmailExceptionPart, CustomerEmailExceptionPartRecord>().Where(c => c.CodeRandom == code).List().FirstOrDefault();
            if (customer != null)
            {
                if (!_newcustomerservice.CheckCustomerEmailExceptionTrue(customer.EmailException))
                {
                    var model = new CustomerExceptionVerify
                    {
                        Code = code,
                        EmailException = customer.EmailException
                    };
                    return View(model);
                }   
                else
                {
                    Services.Notifier.Information(T("Địa chỉ email đã được loại trừ khỏi danh sách nhận mail của chúng tôi!"));
                    return Redirect("/bat-dong-san/rao-ban");
                }
            }
            else
            {
                return Redirect("/");
            }
        }

        [HttpPost, ActionName("VerifyExeptionEmail")]
        public ActionResult VerifyExeptionEmailPOST(CustomerExceptionVerify customer)
        {

            if (!string.IsNullOrEmpty(customer.Code))
            {
                var _customer = Services.ContentManager.Query<CustomerEmailExceptionPart, CustomerEmailExceptionPartRecord>().Where(c => c.CodeRandom == customer.Code).List().FirstOrDefault();
                var p = Services.ContentManager.Get<CustomerEmailExceptionPart>(_customer.Id);

                p.StatusActive = true;
            }
            Services.Notifier.Information(T("Địa chỉ email đã được loại trừ khỏi danh sách nhận mail của chúng tôi!"));

            return Redirect("/bat-dong-san/rao-ban");
        }

        #region UpdateModel
        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        #endregion
    }
}