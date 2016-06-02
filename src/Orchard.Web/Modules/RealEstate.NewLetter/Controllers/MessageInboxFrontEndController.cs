using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Orchard.Themes;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Logging;
using Orchard.Localization;
using Orchard.UI.Navigation;
using RealEstate.NewLetter.Models;
using Orchard.Settings;
using RealEstate.NewLetter.ViewModels;
using System.Web.Routing;
using Orchard.Users.Models;
using Orchard.UI.Notify;
using RealEstate.Helpers;
using Orchard.Data;
using RealEstate.NewLetter.Services;
using RealEstate.Services;

namespace RealEstate.NewLetter.Controllers
{
    [Themed]
    public class MessageInboxFrontEndController : Controller, IUpdateModel
    {
        private readonly ISiteService _siteService;
        private readonly IRepository<MessageInboxPartRecord> _msgInboxRepository;
        private readonly IMessageInboxService _messageInbox;
        private readonly IHostNameService _hostNameService;

        public MessageInboxFrontEndController(IOrchardServices services, 
            IShapeFactory shapeFactory, 
            ISiteService siteService, 
            IMessageInboxService messageInbox,
            IRepository<MessageInboxPartRecord> msgInboxRepository,
            IHostNameService hostNameService)
        {
            _siteService = siteService;
            _msgInboxRepository = msgInboxRepository;
            _messageInbox = messageInbox;
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

        // Hộp thư đến
        [Authorize]
        public ActionResult Inbox(PagerParameters pagerParameters)
        {
            if (Services.WorkContext.CurrentUser == null)
                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });

            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            IContentQuery<MessageInboxPart, MessageInboxPartRecord> listMessage;

            if (hostDefault != null && hostName == hostDefault.Name)
            {
                listMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                .Where(r => r.UserReceived == Services.WorkContext.CurrentUser.As<UserPart>().Record 
                && r.IsAdmin && r.ReadByStaff == false && r.IsUserDelete == false 
                && (r.HostName == null || r.HostName == hostName)).OrderByDescending(r => r.DateSend);
            }
            else
            {
                listMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                .Where(r => r.UserReceived == Services.WorkContext.CurrentUser.As<UserPart>().Record
                && r.IsAdmin && r.ReadByStaff == false && r.IsUserDelete == false
                && r.HostName == hostName).OrderByDescending(r => r.DateSend);
            }

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = listMessage.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = listMessage.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            //if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region Build Model

            var model = new MessageInboxIndexViewModel
            {
                ListMessage = results.Select(r => new MessageInboxEntry
                {
                    MessagePart = r,
                    ReplyCount = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(w => w.ParentId != null && w.ParentId == r.Id && w.IsUserDelete == false).Count()
                }).ToList(),
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

        //Xem tin nhắn
        [Authorize]
        public ActionResult ViewMessage(int Id)
        {
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            var _record = Services.ContentManager.Get<MessageInboxPart>(Id);
            if (_record == null || _record.IsUserDelete || (_record.UserSend != userCurent && _record.UserReceived != userCurent))
                return RedirectToAction("Inbox");

            if (_record.ParentId != null)
                return RedirectToAction("ViewMessage", new { Id = _record.ParentId});

            if (_record.UserReceived != null && _record.UserReceived == userCurent)
                _record.IsRead = true;

            var listReply = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => r.ParentId != null && r.ParentId == Id && r.IsUserDelete == false).List();
            foreach (var item in listReply)
            {
                if (!item.ReadByStaff)//thư đến
                    item.IsRead = true;
            }

            var model = new ViewMessageInboxViewModel
            {
                ParentMessage = _record,
                ListMessageInboxReply = listReply.OrderBy(r => r.DateSend).ToList()
            };

            return View(model);
        }

        [HttpPost, Authorize]
        public ActionResult ViewMessage(int? Id, string Title, string Content)
        {
            if (string.IsNullOrEmpty(Content))
            {
                Services.Notifier.Error(T("Vui lòng nhập nội dung tin nhắn."));
                return RedirectToAction("ViewMessage", new { Id = Id });
            }

            if (!Id.HasValue)
            {
                Services.Notifier.Error(T("Giá trị ID tin nhắn không đúng. Vui lòng kiểm tra lại."));
                return RedirectToAction("ViewMessage", new { Id = Id });
            }

            var _record = Services.ContentManager.Get<MessageInboxPart>(Id.Value);
            if (_record == null)
                return RedirectToAction("Inbox");

            _record.IsRead = false;
            var _newRecord = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            _newRecord.Title = Title.StripHtml();
            _newRecord.Content = Content.StripHtml();
            _newRecord.DateSend = DateTime.Now;
            _newRecord.IsAdmin = true;
            _newRecord.ReadByStaff = true;
            _newRecord.PreviousId = Id;//Hiện tại chưa dùng đến previousid
            _newRecord.IsUserDelete = false;
            _newRecord.ParentId = Id;
            _newRecord.UserSend = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            _newRecord.UserReceived = null;
            _newRecord.IsRead = false;

            Services.ContentManager.Create(_newRecord);

            Services.Notifier.Information(T("Đã gửi trả lời thành công!"));
            return RedirectToAction("ViewMessage", new { Id = Id });
        }

        //Soạn tin nhắn mới
        [Authorize]
        public ActionResult SendMessage()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult SendMessage(string Title, string Content)
        {
            if(string.IsNullOrEmpty(Title))
            {
                Services.Notifier.Error(T("Tiêu đề tin nhắn không được để trống."));
                return View();
            }
            if (string.IsNullOrEmpty(Content))
            {
                Services.Notifier.Error(T("Nội dung tin nhắn không được để trống."));
                return View();
            }

            var _record = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            var hostName = _hostNameService.GetHostNameSite();

            _record.Title = Title.StripHtml();
            _record.Content = Content.StripHtml();
            _record.IsAdmin = true;
            _record.ReadByStaff = true;
            _record.DateSend = DateTime.Now;
            _record.ParentId = null;
            _record.PreviousId = null;
            _record.UserReceived = null;
            _record.UserSend = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            _record.HostName = hostName;

            Services.ContentManager.Create(_record);
            Services.Notifier.Information(T("Tin nhắn của bạn đã được gửi thành công! Chúng tôi sẽ xem xét và trả lời bạn trong thời gian sớm nhất!"));

            return RedirectToAction("SendInbox");
        }

        // Thư đã gửi
        [Authorize]
        public ActionResult SendInbox(PagerParameters pagerParameters)
        {
            if (Services.WorkContext.CurrentUser == null)
                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel" });

            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            IContentQuery<MessageInboxPart, MessageInboxPartRecord> listMessage;
            if (hostDefault != null && hostName == hostDefault.Name)
            {
                listMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                   .Where(r => r.UserSend == Services.WorkContext.CurrentUser.As<UserPart>().Record
                               && r.ParentId == null
                               && r.IsAdmin && r.ReadByStaff == true
                               && r.IsUserDelete == false && (r.HostName == hostName || r.HostName == null)).OrderByDescending(r => r.DateSend);
            }
            else
            {
                listMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                   .Where(r => r.UserSend == Services.WorkContext.CurrentUser.As<UserPart>().Record
                               && r.ParentId == null
                               && r.IsAdmin && r.ReadByStaff == true
                               && r.IsUserDelete == false && r.HostName == hostName).OrderByDescending(r => r.DateSend);
            }

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = listMessage.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = listMessage.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            //if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region Build Model

            var model = new MessageInboxIndexViewModel
            {
                ListMessage = results.Select(r => new MessageInboxEntry
                {
                    MessagePart = r,
                    ReplyCount = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(w => w.ParentId != null && w.ParentId == r.Id && w.IsUserDelete == false).Count()
                }).ToList(),
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

        [Authorize]
        public ActionResult AjaxDeleteMessage(int Id)
        {
            var _record = Services.ContentManager.Get<MessageInboxPart>(Id);
            if (_record == null)
                return Json(new { status = false, message = "Tin nhắn này không tồn tại"});
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();

            if ((_record.UserSend != null && _record.UserSend != userCurent.Record)
             && _record.UserReceived != null && _record.UserReceived != userCurent.Record)
                return Json(new { status = false, message = "Không thể xóa tin nhắn này." });

            var listMsgChild = _msgInboxRepository.Fetch(a => a.ParentId != null && a.ParentId == Id);

            if (listMsgChild.Any())
            {
                foreach (var item in listMsgChild)
                {
                    item.IsUserDelete = true;
                }
            }
            _record.IsUserDelete = true;

            return Json(new { status = true});
        }

        [Authorize]
        public ActionResult AjaxDeleteMessageUser(int Id)
        {
            var _record = Services.ContentManager.Get<MessageInboxPart>(Id);
            if (_record == null)
                return Json(new { status = false, message = "Tin nhắn này không tồn tại" });
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();

            if ((_record.UserSend != null && _record.UserSend != userCurent.Record)
             && _record.UserReceived != null && _record.UserReceived != userCurent.Record)
                return Json(new { status = false, message = "Không thể xóa tin nhắn này." });

            var listMsgChild = _msgInboxRepository.Fetch(a => a.ParentId != null && a.ParentId == Id);

            if (listMsgChild.Any())
            {
                foreach (var item in listMsgChild)
                {
                    item.IsUserDelete = true;
                }
            }
            _record.IsUserDelete = true;

            return Json(new { status = true });
        }
        
        //Notifi count
        public ActionResult AjaxCountMessageNotifi()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            if(user == null)
                return Json(new {message = 0});

            int countMessage = 0;
            var hostName = _hostNameService.GetHostNameSite();
            var hostDefault = _hostNameService.GetHostNamePartByClass("host-name-main");

            if (hostDefault != null && hostName == hostDefault.Name)
            {
                countMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => 
                                r.ReadByStaff == false
                                && r.IsUserDelete == false
                                && r.UserReceived == user.Record
                                && r.IsRead == false && (r.HostName == null || r.HostName == hostName)).Count();
            }
            else
            {
                countMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => 
                                r.ReadByStaff == false
                                && r.IsUserDelete == false
                                && r.UserReceived == user.Record
                                && r.IsRead == false && r.HostName == hostName).Count();
            }
            return Json(new { message = countMessage});
        }

        #region /Blog-ca-nhan/tin-nhan

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1.FromAdmin - 2. FromFriend</param>
        /// <returns></returns>
        public ActionResult AjaxLoadMessageInboxBlog(int type, PagerParameters pagerParameters)
        {
            if (type != 1 && type != 2)
                type = 2;

            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>();
            int totalCount = 0;
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var pList = _messageInbox.GetListMessageInboxByType(type, userCurent);

            totalCount = pList.Count();

            var results = pList//.OrderByDescending(r=>r.DateSend)
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            #region BUILD MODEL

            var pagerShape = Shape.Pager(pager);
            pagerShape.TotalItemCount(totalCount);

            var model = new MessageInboxIndexViewModel
            {
                ListMessage = results.Select(r => new MessageInboxEntry
                {
                    MessagePart = r,
                    ReplyCount = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(w => w.ParentId != null && w.ParentId == r.Id && w.IsUserDelete == false).Count()
                }).ToList(),
                Pager = pagerShape,
                TotalCount = totalCount,
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView(model);
        }

        public ActionResult AjaxLoadMessageSendBlog(PagerParameters pagerParameters)
        {
            var listMessage = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>()
                .Where(r => r.UserSend == Services.WorkContext.CurrentUser.As<UserPart>().Record
                            && (r.ParentId == null || (r.ParentId != null && !r.IsRead))
                            && !r.IsAdmin
                            && r.IsUserDelete == false).OrderByDescending(r => r.DateSend);

            #region SLICE

            DateTime startSlice = DateTime.Now;

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = listMessage.Count();
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            var results = listMessage.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            //if (_debugIndex) Services.Notifier.Information(T("SLICE {0}", (DateTime.Now - startSlice).TotalSeconds));

            #endregion

            #region Build Model

            var model = new MessageInboxIndexViewModel
            {
                ListMessage = results.Select(r => new MessageInboxEntry
                {
                    MessagePart = r,
                    ReplyCount = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(w => w.ParentId != null && w.ParentId == r.Id && w.IsUserDelete == false).Count()
                }).ToList(),
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            // maintain previous route data when generating page links
            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return PartialView(model);
        }

        [Authorize]
        public ActionResult ViewMessageUser(int Id)
        {
            var userCurent = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            var _record = Services.ContentManager.Get<MessageInboxPart>(Id);
            if (_record == null || _record.IsUserDelete || (_record.UserSend != userCurent && _record.UserReceived != userCurent))
                return RedirectToAction("MyMessage", "PersonalPage", new { area = "RealEstate.MiniForum.FrontEnd"});

            if (_record.ParentId != null)
                return RedirectToAction("ViewMessageUser", new { Id = _record.ParentId });

            if (_record.UserReceived != null && _record.UserReceived == userCurent)
                _record.IsRead = true;

            var listReply = Services.ContentManager.Query<MessageInboxPart, MessageInboxPartRecord>().Where(r => r.ParentId != null && r.ParentId == Id && r.IsUserDelete == false).List();
            foreach (var item in listReply)
            {
                if(item.UserReceived == userCurent)
                    item.IsRead = true;
            }

            var model = new ViewMessageInboxViewModel
            {
                ParentMessage = _record,
                ListMessageInboxReply = listReply.OrderBy(r => r.DateSend).ToList()
            };

            return View(model);
        }

        [HttpPost, Authorize]
        public ActionResult ViewMessageUser(int? Id, int? UserReceivedId , string Title, string Content)
        {
            if (string.IsNullOrEmpty(Content))
            {
                Services.Notifier.Error(T("Vui lòng nhập nội dung tin nhắn."));
                return RedirectToAction("ViewMessageUser", new { Id = Id });
            }

            if (!Id.HasValue)
            {
                Services.Notifier.Error(T("Giá trị ID tin nhắn không đúng. Vui lòng kiểm tra lại."));
                return RedirectToAction("ViewMessageUser", new { Id = Id });
            }

            if (!UserReceivedId.HasValue)
            {
                Services.Notifier.Error(T("Ko thể gửi dc tin nhắn."));
                return RedirectToAction("ViewMessageUser", new { Id = Id });
            }

            var _record = Services.ContentManager.Get<MessageInboxPart>(Id.Value);
            if (_record == null)
                return RedirectToAction("MyMessage", "PersonalPage", new { area = "RealEstate.MiniForum.FrontEnd" });

            _record.IsRead = false;
            var _newRecord = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            _newRecord.Title = Title.StripHtml();
            _newRecord.Content = Content.StripHtml();
            _newRecord.DateSend = DateTime.Now;
            _newRecord.IsAdmin = false;
            _newRecord.ReadByStaff = false;
            _newRecord.PreviousId = Id;//Hiện tại chưa dùng đến previousid
            _newRecord.IsUserDelete = false;
            _newRecord.ParentId = Id;
            _newRecord.UserSend = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            _newRecord.UserReceived = Services.ContentManager.Get<UserPart>(UserReceivedId.Value).Record ;
            _newRecord.IsRead = false;

            Services.ContentManager.Create(_newRecord);

            Services.Notifier.Information(T("Đã gửi trả lời thành công!"));
            return RedirectToAction("ViewMessageUser", new { Id = Id });
        }

        //TIn nhắn cá nhân
        [HttpPost, Authorize]
        public ActionResult SendMessageUser(string Title, string Content)
        {
            //if (string.IsNullOrEmpty(Title))
            //{
            //    Services.Notifier.Error(T("Tiêu đề tin nhắn không được để trống."));
            //    return View();
            //}
            //if (string.IsNullOrEmpty(Content))
            //{
            //    Services.Notifier.Error(T("Nội dung tin nhắn không được để trống."));
            //    return View();
            //}

            //var _record = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            //_record.Title = Title.StripHtml();
            //_record.Content = Content.StripHtml();
            //_record.IsAdmin = false;
            //_record.ReadByStaff = false;
            //_record.DateSend = DateTime.Now;
            //_record.ParentId = null;
            //_record.PreviousId = null;
            //_record.UserReceived = null;
            //_record.UserSend = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            //Services.ContentManager.Create(_record);
            //Services.Notifier.Information(T("Tin nhắn của bạn đã được gửi thành công!"));

            return RedirectToAction("MyMessage", "PersonalPage", new { area = "RealEstate.MiniForum.FrontEnd" });
        }

        [HttpPost,Authorize]
        public ActionResult AjaxSendMessageUser(FormCollection frm)
        {
            string title = frm["Title"];
            string content = frm["Content"];

            if (string.IsNullOrEmpty(content))
                return Json(new { status = false, message = "Vui lòng nhập nội dung tin nhắn" });

            int UserId = 0;
            if (!int.TryParse(frm["UserIdMessage"], out UserId))
                return Json(new { status = false, message = "User nhận không đúng, vui lòng kiểm tra lại!"});

            var _record = Services.ContentManager.New<MessageInboxPart>("MessageInbox");
            _record.Title = !string.IsNullOrEmpty(title) ? title.StripHtml() : title;
            _record.Content = content.StripHtml();
            _record.IsAdmin = false;
            _record.ReadByStaff = false;
            _record.DateSend = DateTime.Now;
            _record.ParentId = null;
            _record.PreviousId = null;
            _record.UserReceived = null;
            _record.UserSend = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            _record.UserReceived = Services.ContentManager.Get<UserPart>(UserId).Record;

            Services.ContentManager.Create(_record);


            return Json(new { status = true });
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