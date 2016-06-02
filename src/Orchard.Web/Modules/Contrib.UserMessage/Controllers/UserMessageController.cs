using System;
using System.Web.Mvc;
using Contrib.UserMessage.Models;
using Contrib.UserMessage.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI;
using Orchard.UI.Notify;
using Orchard.Themes;

namespace Contrib.UserMessage.Controllers {
    [Themed]
    [ValidateInput(false)]
    public class UserMessageController : Controller, IUpdateModel {
        protected readonly IUserMessageService _contactUserService;

        public UserMessageController(
            IOrchardServices services,
            IUserMessageService contactUserService) {

            Services = services;
            _contactUserService = contactUserService;

            T = NullLocalizer.Instance;
        }

        public virtual ActionResult Index() {
            if (!Services.Authorizer.Authorize(Permissions.ManageContactMessages, T("Not allowed to manage messages")))
                return new HttpUnauthorizedResult();

            return View(_contactUserService.GetReceivedMessages(Services.WorkContext.CurrentUser));
        }

        public virtual ActionResult SentMessages() {
            if (!Services.Authorizer.Authorize(Permissions.ManageContactMessages, T("Not allowed to manage messages")))
                return new HttpUnauthorizedResult();

            return View(_contactUserService.GetSentMessages(Services.WorkContext.CurrentUser));
        }

        public virtual ActionResult Details(int id) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageContactMessages, T("Not allowed to manage messages")))
                    return new HttpUnauthorizedResult();

                UserMessagePart userMessagePart = _contactUserService.GetMessage(id);

                // Make sure the user is accessing messages he can
                if (userMessagePart.SentFrom != Services.WorkContext.CurrentUser.UserName &&
                    userMessagePart.SentTo != Services.WorkContext.CurrentUser.UserName) {
                    throw new OrchardException(T("Invalid message"));
                }

                dynamic model = Services.ContentManager.BuildDisplay(userMessagePart);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            } catch (Exception exception) {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Getting details for message {0} failed", exception.Message));
                return RedirectToAction("Index");
            }
        }

        public virtual ActionResult DeleteReceived(int id) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageContactMessages, T("Not allowed to manage messages")))
                    return new HttpUnauthorizedResult();

                _contactUserService.DeleteMessageReceived(id);

                Services.Notifier.Information(T("Message was successfully deleted"));
            } catch (Exception exception) {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Deleting message failed: {0}", exception.Message));
            }

            return RedirectToAction("Index");
        }

        public virtual ActionResult DeleteSent(int id) {
            try {
                if (!Services.Authorizer.Authorize(Permissions.ManageContactMessages, T("Not allowed to manage messages")))
                    return new HttpUnauthorizedResult();

                _contactUserService.DeleteMessageSent(id);

                Services.Notifier.Information(T("Message was successfully deleted"));
            } catch (Exception exception) {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Deleting message failed: {0}", exception.Message));
            }

            return RedirectToAction("SentMessages");
        }

        public virtual ActionResult Send() {
            try {
                if (!Services.Authorizer.Authorize(Permissions.SendContactMessages, T("Not allowed to manage feeds")))
                    return new HttpUnauthorizedResult();

                UserMessagePart userMessagePart = Services.ContentManager.New<UserMessagePart>("UserMessage");
                if (userMessagePart == null)
                    return HttpNotFound();

                dynamic model = Services.ContentManager.BuildEditor(userMessagePart);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            } catch (Exception exception) {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Creating message failed: {0}", exception.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Send")]
        public virtual ActionResult SendPOST() {
            try {
                if (!Services.Authorizer.Authorize(Permissions.SendContactMessages, T("Not allowed to manage messages")))
                    return new HttpUnauthorizedResult();

                // Create in-memory part
                UserMessagePart userMessagePart = Services.ContentManager.New<UserMessagePart>("UserMessage");
                if (userMessagePart == null)
                    return HttpNotFound();

                // Sync in-memory part with data from form
                dynamic model = Services.ContentManager.UpdateEditor(userMessagePart, this);
                if (!ModelState.IsValid) {
                    Services.TransactionManager.Cancel();

                    // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                    return View((object)model);
                }

                // If data from form was valid, send the message
                _contactUserService.SendMessage(userMessagePart);
                Services.Notifier.Information(T("Message was successfully sent"));
                return RedirectToAction("SentMessages");
            } catch (Exception exception) {
                Services.TransactionManager.Cancel();
                Services.Notifier.Error(T("Sending message failed: {0}", exception.Message));
                return RedirectToAction("Send");
            }
        }

        protected IOrchardServices Services { get; set; }
        protected Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}