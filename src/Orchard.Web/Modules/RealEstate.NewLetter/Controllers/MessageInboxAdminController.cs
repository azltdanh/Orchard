using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Orchard.ContentManagement;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Localization;
using RealEstate.NewLetter.Models;
using RealEstate.NewLetter.ViewModels;
using Orchard.UI.Navigation;
using System.Web.Routing;
using Orchard.Caching;
using Orchard.Services;
using Orchard.Users.Models;
using RealEstate.Services;
using RealEstate.NewLetter.Services;

namespace RealEstate.NewLetter.Controllers
{
    [Admin]
    public class MessageInboxAdminController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly IPropertyService _propertyService;
        private readonly IUserGroupService _groupService;
        private readonly IUserPersonalService _userPersonalService;
        private readonly IMessageInboxService _messageInboxMessage;
        private readonly IUserGroupService _userGroupService;
        private readonly IHostNameService _hostNameService;

        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;
        private int cacheTimeSpan = 60 * 24; // Cache for 24 hours

        public MessageInboxAdminController(IOrchardServices services, IShapeFactory shapeFactory,
            IPropertyService propertyService,
            IUserGroupService groupService,
            ICacheManager cacheManager,
            IUserPersonalService userPersonalService,
            IMessageInboxService messageInboxMessage,
            ISiteService siteService,
            IUserGroupService userGroupService,
            IHostNameService hostNameService,
            IClock clock,
            ISignals signals)
        {
            _userPersonalService = userPersonalService;
            _siteService = siteService;
            _propertyService = propertyService;
            _groupService = groupService;
            _userGroupService = userGroupService;
            _messageInboxMessage = messageInboxMessage;
            _hostNameService = hostNameService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;

            _cacheManager = cacheManager;
            _clock = clock;
            _signals = signals;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        private bool _debugIndex = false;

        // Hộp thư
        public ActionResult Inbox(MessageInboxIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Not Permision ContactInobxNewLetter")))
                return new HttpUnauthorizedResult();

            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            IContentQuery<MessageInboxPart, MessageInboxPartRecord> _list;

            if (hostDefault != null && hostName == hostDefault.Name)
            {
                _list = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(p => p.IsAdmin && p.ReadByStaff && (p.HostName == null || p.HostName == hostName));
            }else
            {
                _list = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(p => p.IsAdmin && p.ReadByStaff && (p.HostName == hostName));
            }

            if (!string.IsNullOrEmpty(options.Search)) _list = _list.Where(r => r.Content.Contains(options.Search) || r.Title.Contains(options.Search)).OrderByDescending(r => r.DateSend);

            switch (options.Filter)
            {
                case MessagesFilter.All:
                    // _list = _list.Where(p => p.);
                    break;
                case MessagesFilter.UnRead:
                    _list = _list.Where(p => !p.IsRead);
                    break;
                case MessagesFilter.Read:
                    _list = _list.Where(p => p.IsRead);
                    break;
                default:
                    break;
            }

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            _list = _list.Where(r => r.ParentId == null || (r.ParentId != null && r.IsRead == false));

            int totalCount = _list.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = _list.OrderByDescending(r=>r.DateSend).Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region Build Model

            var model = new MessageInboxIndexViewModel
            {
                ListMessage = results.Select(r => new MessageInboxEntry
                {
                    MessagePart = r,
                    ReplyCount = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(w=>w.ParentId != null && w.ParentId == r.Id).Count()
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        [HttpPost]
        public ActionResult Inbox(FormCollection form)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Not Permision ContactInobxNewLetter")))
                return new HttpUnauthorizedResult();

            var viewModel = new MessageInboxIndexViewModel { ListMessage = new List<MessageInboxEntry>(), Options = new MessageInboxIndexOptions() };
            UpdateModel(viewModel);

            return RedirectToAction("Inbox", ControllerContext.RouteData.Values);
        }

        //THư đã gửi
        public ActionResult SendInbox(MessageInboxIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Not Permision ContactInobxNewLetter")))
                return new HttpUnauthorizedResult();

            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            IContentQuery<MessageInboxPart, MessageInboxPartRecord> _list;

            if (hostDefault != null && hostName == hostDefault.Name){
                _list = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(p => p.IsAdmin && !p.ReadByStaff && p.ParentId == null
                    && (p.HostName == null || p.HostName == hostName)).OrderByDescending(r => r.DateSend);
            }
            else{
                _list = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                    .Where(p => p.IsAdmin && !p.ReadByStaff && p.ParentId == null
                    && p.HostName == hostName).OrderByDescending(r => r.DateSend);
            }

            if (!string.IsNullOrEmpty(options.Search)) _list = _list.Where(r => r.Content.Contains(options.Search) || r.Title.Contains(options.Search));

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = _list.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = _list.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region Build Model

            var model = new MessageInboxIndexViewModel
            {
                ListMessage = results.Select(r => new MessageInboxEntry
                {
                    MessagePart = r,
                    ReplyCount = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(w => w.ParentId != null && w.ParentId == r.Id).Count()
                }).ToList(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }
        [HttpPost]
        public ActionResult SendInbox(FormCollection form)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Not Permision ContactInobxNewLetter")))
                return new HttpUnauthorizedResult();

            var viewModel = new MessageInboxIndexViewModel { ListMessage = new List<MessageInboxEntry>(), Options = new MessageInboxIndexOptions() };
            UpdateModel(viewModel);

            return RedirectToAction("Inbox", ControllerContext.RouteData.Values);
        }

        //Xem thư
        public ActionResult ViewMessage(int Id)
        {
            var _record = Services.ContentManager.Get<MessageInboxPart>(Id);
            if (_record == null)
                return RedirectToAction("Inbox");

            if(_record.ReadByStaff)
                _record.IsRead = true;

            var listReply = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => r.ParentId != null && r.ParentId == Id).List();
            foreach (var item in listReply)
            {
                if (item.ReadByStaff)
                    item.IsRead = true;
            }

            var model = new ViewMessageInboxViewModel
            {
                ParentMessage = _record,
                ListMessageInboxReply = listReply.OrderBy(r=>r.DateSend).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult ViewMessage(int Id, string Title, string Content)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxReplyNewLetter, T("Not Permision ContactInobxReplyNewLetter")))
                return new HttpUnauthorizedResult();

            var _record = Services.ContentManager.Get<MessageInboxPart>(Id);


            if (!string.IsNullOrEmpty(Content))
            {
                _messageInboxMessage.ReplyMessage(Id, Title, Content, true, false);
            }
            else
                Services.Notifier.Error(T("Vui lòng nhập nội dung tin nhắn!"));

            return RedirectToAction("ViewMessage", new { Id = _record.ParentId != null ? _record.ParentId : Id });
        }

        //Soạn thư mới
        public ActionResult CreateMessage(string returnUrl, string strUser)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Not Permision ContactInobxNewLetter")))
                return new HttpUnauthorizedResult();

            var model = new MessageInboxCreateViewModel
            {
                Email = strUser,
                ReturnUrl = returnUrl
            };
            return View(model);
        }
        [HttpPost]
        public ActionResult CreateMessage(MessageInboxCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ContactInobxNewLetter, T("Not Permision ContactInobxNewLetter")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            if(createModel.IsChecked && string.IsNullOrEmpty(createModel.Email))
            {
                Services.Notifier.Error(T("Vui lòng nhập email nhận tin nhắn!"));

                return View(createModel);
            }

            if (!createModel.IsChecked && !createModel.UserReceive.HasValue)
            {
                Services.Notifier.Error(T("Vui lòng chọn user nhận tin nhắn!"));

                return View(createModel);
            }

            if (ModelState.IsValid)
            {
                if (createModel.IsChecked)//Nhập địa chỉ Email
                {
                    if (_userPersonalService.VerifyExistEmail(createModel.Email))//Nếu có trong danh sách tài khoản
                    {
                        //Gửi vào Email và User
                        var userReceive = _userGroupService.GetUsers().Where(r=>r.Email.ToLower() == createModel.Email.ToLower()).FirstOrDefault();
                        Services.Notifier.Information(T("Email: {0} - ID: {1}", userReceive.Email, userReceive.Id));
                        _messageInboxMessage.SendMessage(createModel.Title, createModel.Content, userReceive.Id, true, false);
                    }
                    else
                    {
                        //Gửi Email
                        //var userReceived = Services.ContentManager.Get<UserPart>(createModel.UserReceive.Value).Record;
                        _messageInboxMessage.SendMessageToEmail(createModel.Email, createModel.Content, !string.IsNullOrEmpty(createModel.Title) ? createModel.Title : "Thư mới từ dinhgianhadat.vn");

                        Services.Notifier.Information(T("Đã gửi tin nhắn đến Email: {0}", createModel.Email));
                        return RedirectToAction("CreateMessage");
                    }
                }
                else
                {
                    _messageInboxMessage.SendMessage(createModel.Title, createModel.Content, createModel.UserReceive.Value, true, false);
                    
                }

                if (!String.IsNullOrEmpty(createModel.ReturnUrl))
                {
                    return this.RedirectLocal(createModel.ReturnUrl);
                }
            }

            if (!ModelState.IsValid)
                return View(createModel);

            return RedirectToAction("CreateMessage");
        }
        public ActionResult DeleteMessageInbox(int Id)
        {
            bool IsDelete = _messageInboxMessage.DeleteMessageInbox(Id);

            return RedirectToAction("Inbox");
        }
        public JsonResult ListUserForCombobox()
        {
            return _cacheManager.Get("ListUserForCombobox", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                ctx.Monitor(_signals.When("ListUserForCombobox_Changed"));

                var user = Services.ContentManager.Query<UserPart, UserPartRecord>();
                var s = user.List().Select(c => new SelectListItem
                {
                    Text = c.UserName,
                    Value = c.Id.ToString()
                }).ToList();

                return Json(new { list = s });
            });

        }

        //Count message Admin

        public ActionResult CountMessageInbox()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            //var group = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            //var belongGroup = _groupService.GetBelongGroup(user.Id);

            int countMessage = 0;
            int countContactInbox = 0;

            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            if (hostDefault != null && hostName == hostDefault.Name)
            {
                countMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                                .Where(r => r.IsAdmin && r.ReadByStaff && r.IsRead == false && (r.HostName == null || r.HostName == hostName)).Count();
                countContactInbox = Services.ContentManager.Query<ContactInboxPart, ContactInboxPartRecord>().Where(r => r.IsRead == false && (r.HostName == null || r.HostName == hostName)).Count();
            }else
            {
                countMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                                .Where(r => r.IsAdmin && r.ReadByStaff && r.IsRead == false && r.HostName == hostName).Count();
                countContactInbox = Services.ContentManager.Query<ContactInboxPart, ContactInboxPartRecord>().Where(r => r.IsRead == false && r.HostName == hostName).Count();
            }


            var data = new
            {
                userId = user != null ? user.Id : 0,

                countAll = (countMessage + countContactInbox),

                countMessage,
                countContactInbox
            };

            return Content(_propertyService.BuildMessageString(data), "text/event-stream");
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