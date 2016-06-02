using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using RealEstate.NewLetter.ViewModels;
using Orchard.Settings;
using RealEstate.NewLetter.Models;
using System.Web.Routing;
using Orchard.Security;
using Orchard.UI.Notify;
using RealEstate.Services;


namespace RealEstate.NewLetter.Controllers
{
    [Admin]
    public class ContactInboxAdminController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly IHostNameService _hostNameService;
        public ContactInboxAdminController(IOrchardServices services, IShapeFactory shapeFactory, IHostNameService hostNameService, ISiteService siteService)
        {
            _siteService = siteService;
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

        public ActionResult Index(ContactInboxOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Không có quyền quản lý hộp thư liên hệ")))
                return new HttpUnauthorizedResult();
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new ContactInboxOptions();

            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            var contactInbox = Services.ContentManager
                .Query<ContactInboxPart, ContactInboxPartRecord>();

            if (hostDefault != null && hostName == hostDefault.Name){
                contactInbox = contactInbox.Where(p =>p.HostName == null || p.HostName == hostName);
            }
            else{
                contactInbox = contactInbox.Where(p => p.HostName == hostName);
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                contactInbox = contactInbox.Where(u => u.Content.Contains(options.Search) || u.Title.Contains(options.Search) || u.Email.Contains(options.Search));
            }
            var pagerShape = Shape.Pager(pager).TotalItemCount(contactInbox.Count());

            var results = contactInbox
                .Slice(pager.GetStartIndex(), pager.PageSize).OrderByDescending(c => c.DateCreated)
                .ToList();

            var model = new ContactInboxIndexViewModel
            {
                ContactInboxs = results
                    .Select(x => new ContactInboxEntry { ContactInbox = x })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Không có quyền quản lý hộp thư liên hệ")))
                return new HttpUnauthorizedResult();

            var viewModel = new ContactInboxIndexViewModel { ContactInboxs = new List<ContactInboxEntry>(), Options = new ContactInboxOptions() };
            UpdateModel(viewModel);

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult ViewInbox(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Không có quyền quản lý hộp thư liên hệ")))
                return new HttpUnauthorizedResult();

            var contact = Services.ContentManager.Get<ContactInboxPart>(id);

            if (contact != null)
            {
                var model = new ContactInboxEntry
                {
                    ContactInbox = contact
                };

                return View(model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Không có quyền quản lý hộp thư liên hệ")))
                return new HttpUnauthorizedResult();

            var contact = Services.ContentManager.Get<ContactInboxPart>(id);
            if (contact != null)
            {
                Services.ContentManager.Remove(contact.ContentItem);
                Services.Notifier.Information(T("Tin nhắn: \" {0} \" đã được xóa", contact.Title));
            }

            return RedirectToAction("Index");
        }

        public ActionResult CheckIsRead(int id)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Không có quyền quản lý hộp thư liên hệ")))
                return Json(new { status = false });

            var contact = Services.ContentManager.Get<ContactInboxPart>(id);

            if (contact == null)
            {
                Services.Notifier.Error(T("Có lỗi xảy ra! Xử lý ko thành công"));
                return RedirectToAction("Index");
            }

            contact.IsRead = !contact.IsRead;
            Services.Notifier.Information(T("Tin nhắn đã được xử lý."));

            return RedirectToAction("Index");
        }

        public ActionResult AjaxCheckIsRead(int id)//never use
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Không có quyền quản lý hộp thư liên hệ")))
                return Json(new { status = false});

            var contact = Services.ContentManager.Get<ContactInboxPart>(id);

            if (contact == null)
                return Json(new { status = false});

            contact.IsRead = !contact.IsRead;

            return Json(new { status = true});

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