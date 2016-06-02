using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Orchard;
using Orchard.UI.Notify;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.Logging;
using Orchard.DisplayManagement;
using RealEstate.NewLetter.Models;
using RealEstate.NewLetter.ViewModels;
using RealEstate.Services;
using Orchard.Users.Models;
using Contrib.OnlineUsers.Models;


namespace RealEstate.NewLetter.Controllers
{
    [Themed]
    public class ContactInboxFrontEndController : Controller, IUpdateModel
    {
        private readonly IHostNameService _hostNameService;
        public ContactInboxFrontEndController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IHostNameService hostNameService)
        {
            _hostNameService = hostNameService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public ActionResult SendMessageContact()
        {
            if (Services.WorkContext.CurrentUser != null)
            {
                var suggestion = Services.ContentManager.New<ContactInboxPart>("ContactInbox");
                var userId = Services.WorkContext.CurrentUser.Id;
                var userprofile = Services.ContentManager.Get<UserUpdateProfilePart>((int)userId).Record;

                var editor = Shape.EditorTemplate(TemplateName: "Parts/ContactInbox.SendMessage",
                            Model: new ContactInboxCreateViewModel { FullName = userprofile.FirstName + " " + userprofile.LastName, Email = userprofile.Email, Phone = userprofile.Phone }
                        , Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(suggestion);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            else
            {
                var suggestion = Services.ContentManager.New<ContactInboxPart>("ContactInbox");
                var editor = Shape.EditorTemplate(TemplateName: "Parts/ContactInbox.SendMessage",
                            Model: new ContactInboxCreateViewModel(), Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(suggestion);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
        }
        [HttpPost, ActionName("SendMessageContact")]
        public ActionResult SendMessageContactPOST(ContactInboxCreateViewModel createmodel)
        {
            var contact = Services.ContentManager.New<ContactInboxPart>("ContactInbox");
            var hostName = _hostNameService.GetHostNameSite();

            if (ModelState.IsValid)
            {
                contact.FullName = createmodel.FullName;
                contact.Email = createmodel.Email;
                contact.Phone = createmodel.Phone;
                contact.Title = createmodel.Title;
                contact.Content = createmodel.Content;
                contact.Link = createmodel.Link;
                contact.IsRead = false;
                contact.DateCreated = DateTime.Now;
                contact.HostName = hostName;

                Services.ContentManager.Create(contact);
            }

            dynamic model = Services.ContentManager.BuildEditor(contact);

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                var editor = Shape.EditorTemplate(TemplateName: "Parts/ContactInbox.SendMessage", Model: createmodel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            Services.Notifier.Information(T("Nội dung thư của bạn đã được gửi đi! Chúng tôi sẽ trả lời bạn trong thời gian sớm nhất vào địa chỉ email mà bạn đã cung cấp cho chúng tôi."));

            return RedirectToAction("SendMessageContact");
        }

        public ActionResult MessageContactRedirect()
        {
            return RedirectToAction("SendMessageContact");
        }
        #region Ajax
        public ActionResult AjaxSendMessageContact(FormCollection frm)
        {
            //Validate
            if (string.IsNullOrEmpty(frm["Content"]))
            {
                return Json(new { status = false, message = "Vui lòng nhập nội dung" });
            }

            try
            {
                var contact = Services.ContentManager.New<ContactInboxPart>("ContactInbox");
                contact.Title = !string.IsNullOrEmpty(frm["Title"]) ? frm["Title"] : "Khách hàng báo lỗi";
                contact.Content = frm["Content"];
                contact.Email = "";
                contact.Phone = "";
                contact.FullName = frm["FullName"];
                contact.Link = frm["Link"];
                contact.IsRead = false;
                contact.DateCreated = DateTime.Now;

                Services.ContentManager.Create(contact);

                return Json(new { status = true, message = "Đã gửi thông tin thành công. Cảm ơn bạn đã góp ý báo lỗi, chúng tôi sẽ trả lời và hỗ trợ bạn trong thời gian sớm nhất!" });
            }
            catch
            {
                return Json(new { status = false, message = "Lỗi không gửi được!" });
            }
        }
        #endregion

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