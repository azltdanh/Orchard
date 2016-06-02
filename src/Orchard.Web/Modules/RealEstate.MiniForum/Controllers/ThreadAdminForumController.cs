using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Notify;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

using RealEstateForum.Service.Services;
using RealEstateForum.Service.ViewModels;
using RealEstateForum.Service.Models;
using RealEstateForum.Service;
using RealEstate.Services;
using Orchard.Users.Models;


namespace RealEstate.MiniForum.Controllers
{
    [Admin]
    public class ThreadAdminForumController : Controller, IUpdateModel
    {
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;
        private readonly IThreadAdminService _threadService;
        private readonly IPostAdminService _postAdminService;
        private readonly IHostNameService _hostNameService;
        private readonly IUserGroupService _groupService;

        public ThreadAdminForumController(ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services,
            IThreadAdminService threadService,
            IUserGroupService groupService,
            IHostNameService hostNameService,
            IPostAdminService postAdminService)
        {
            _signals = signals;
            _siteService = siteService;
            _threadService = threadService;
            _postAdminService = postAdminService;
            _hostNameService = hostNameService;
            _groupService = groupService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }
        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region //Thread Managerment
        public ActionResult Index(ThreadOptions options, PagerParameters pagerParameters)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var listThread = _threadService.GetListThreadFromCache(hostname);

            #region ORDER
            switch (options.Order)
            {
                case ThreadOrder.Id:
                    listThread = listThread.OrderBy(u => u.Id);
                    break;
                case ThreadOrder.Name:
                    listThread = listThread.OrderBy(u => u.Name);
                    break;
                case ThreadOrder.ShortName:
                    listThread = listThread.OrderBy(u => u.ShortName);
                    break;

            }
            #endregion

            var pagerShape = Shape.Pager(pager).TotalItemCount(listThread.Count());
            var results = listThread
                .Skip(pager.GetStartIndex()).Take(pager.PageSize)
                .ToList();

            #region BUILD MODEL
            var model = new ThreadAdminIndexViewModel
            {
                ListThreadEntry = results.Select(c => new ThreadEntry
                {
                    ThreadPart = c,
                    CountChild = _threadService.CountTopicByThreadIdToCache(c.Id, hostname)
                }).ToList(),
                Options = options,
                Pager = pagerShape
            };
            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();
            routeData.Values.Add("Options.ThreadOrder", options.Order);

            pagerShape.RouteData(routeData);

            #endregion
            return View(model);
        }
        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection frmCollection, string returnUrl)
        {
            var viewModel = new ThreadAdminIndexViewModel { ListThreadEntry = new List<ThreadEntry>(), Options = new ThreadOptions() };
            UpdateModel(viewModel);

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var checkedEntries = viewModel.ListThreadEntry.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case ThreadBulkAction.None:
                    break;
                case ThreadBulkAction.Enable:
                    foreach (var entry in checkedEntries)
                    {
                        _threadService.CloseOrOpenThread(entry.ThreadPart.Id);
                        _threadService.ClearCacheUpdateThread(hostname);
                    }
                    break;
                case ThreadBulkAction.Disable:
                    foreach (var entry in checkedEntries)
                    {
                        _threadService.CloseOrOpenThread(entry.ThreadPart.Id);
                        _threadService.ClearCacheUpdateThread(hostname);
                    }
                    break;
                case ThreadBulkAction.Delete:
                    foreach (var entry in from entry in checkedEntries let isDelete = _threadService.DeleteThreadById(entry.ThreadPart.Id, hostname) where !isDelete select entry)
                    {
                        Services.Notifier.Information(T("Chuyên mục {0} có tồn tại chuyên đề, không thể xóa!", entry.ThreadPart.Name));
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult ThreadCreate(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var thread = Services.ContentManager.New<ForumThreadPart>("ForumThread");
            dynamic model = Services.ContentManager.BuildEditor(thread);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Create", Model: new ThreadAdminCreateViewModel { ReturnUrl = returnUrl }, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost]
        public ActionResult ThreadCreate(ThreadAdminCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var thread = Services.ContentManager.New<ForumThreadPart>("ForumThread");

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            if (!string.IsNullOrEmpty(createModel.Name) && _threadService.CheckIsExistThreadName(hostname,createModel.Name))
                AddModelError("ThreadNameExist", T("Tên chuyên mục đã tồn tại. Vui lòng nhập tên khác."));
            if (!string.IsNullOrEmpty(createModel.ShortName) && _threadService.CheckIsExistThreadShortName(hostname, createModel.ShortName))
                AddModelError("ThreadShortNameExist", T("ShortName chuyên mục đã tồn tại. Vui lòng nhập tên khác."));

            if (ModelState.IsValid)
            {
                thread.Name = createModel.Name;
                thread.Description = createModel.Description;
                thread.SeqOrder = createModel.SeqOrder;
                thread.ShortName = createModel.ShortName;
                thread.DefaultImage = createModel.DefaultImage;
                thread.IsOpen = createModel.IsOpen;
                thread.ParentThreadId = null;
                thread.DateCreated = DateTime.Now;
                thread.HostName = hostname;

                Services.ContentManager.Create(thread);

                // Upload Icon
                var imageUpload = Request.Files["DefaultImage"];
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    thread.DefaultImage = _threadService.UploadFileThreadIcon(imageUpload, thread.Id);
                }

                _threadService.ClearCacheUpdateThread(hostname);//Clear Cache

                Services.Notifier.Information(T("Chuyên mục {0} đã được tạo thành công.", thread.Name));
            }


            dynamic model = Services.ContentManager.UpdateEditor(thread, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.Redirect(createModel.ReturnUrl);
            }
            return RedirectToAction("index");
        } 

        public ActionResult ThreadEdit(int id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var threadForum = Services.ContentManager.Get<ForumThreadPart>(id);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Edit", Model: new ThreadAdminEditViewModel { ForumThread = threadForum, ReturnUrl = returnUrl }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(threadForum);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost]
        public ActionResult ThreadEdit(int id)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var threadForum = Services.ContentManager.Get<ForumThreadPart>(id);
            dynamic model = Services.ContentManager.UpdateEditor(threadForum, this);

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var editModel = new ThreadAdminEditViewModel { ForumThread = threadForum };

            if (TryUpdateModel(editModel))
            {
                if (_threadService.CheckIsExistThreadName(hostname,id, editModel.Name))
                {
                    AddModelError("ThreadNameExist", T("Tên chuyên mục đã tồn tại. Vui lòng nhập tên khác."));
                }
                if (_threadService.CheckIsExistThreadName(hostname,id, editModel.ShortName))
                {
                    AddModelError("ThreadShortNameExist", T("ShortName chuyên mục đã tồn tại. Vui lòng nhập tên khác."));
                }

                if (ModelState.IsValid)
                {
                    threadForum.Name = editModel.Name;
                    threadForum.ShortName = editModel.ShortName;
                    threadForum.Description = editModel.Description;
                    threadForum.SeqOrder = editModel.SeqOrder;
                    threadForum.IsOpen = editModel.IsOpen;

                    // Upload Icon
                    var imageUpload = Request.Files["DefaultImage"];
                    if (imageUpload != null && imageUpload.ContentLength > 0)
                    {
                        threadForum.DefaultImage = _threadService.UploadFileThreadIcon(imageUpload, threadForum.Id);
                    }

                    _threadService.ClearCacheUpdateThread(hostname);//Clear Cache
                    Services.Notifier.Information(T("Chuyên mục {0} đã cập nhật thành công.", threadForum.Name));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.Redirect(editModel.ReturnUrl);

            }
            return RedirectToAction("index");
        }

        public ActionResult ThreadDelete(int id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            bool isDelete = _threadService.DeleteThreadById(id, hostname);
            if (!isDelete)
            {
                Services.Notifier.Information(T("Chuyên mục {0} đang tồn tại chuyên đề, bạn không thể xóa được.",id));
                return RedirectToAction("index");
            }
            else
            {
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("index");
            }
        }
        #endregion

        #region //Thread Managerment by Hostname
        public ActionResult HThreadIndex(ThreadOptions options, PagerParameters pagerParameters)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var listThread = _threadService.GetListThreadFromCache(hostname);

            #region ORDER
            switch (options.Order)
            {
                case ThreadOrder.Id:
                    listThread = listThread.OrderBy(u => u.Id);
                    break;
                case ThreadOrder.Name:
                    listThread = listThread.OrderBy(u => u.Name);
                    break;
                case ThreadOrder.ShortName:
                    listThread = listThread.OrderBy(u => u.ShortName);
                    break;

            }
            #endregion

            var pagerShape = Shape.Pager(pager).TotalItemCount(listThread.Count());
            var results = listThread
                .Skip(pager.GetStartIndex()).Take(pager.PageSize)
                .ToList();

            #region BUILD MODEL
            var model = new ThreadAdminIndexViewModel
            {
                ListThreadEntry = results.Select(c => new ThreadEntry
                {
                    ThreadPart = c,
                    CountChild = _threadService.CountTopicByThreadIdToCache(c.Id, hostname)
                }).ToList(),
                Options = options,
                Pager = pagerShape
            };
            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();
            routeData.Values.Add("Options.ThreadOrder", options.Order);

            pagerShape.RouteData(routeData);

            #endregion
            return View(model);
        }
        [HttpPost]
        [Orchard.Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult HThreadIndex(FormCollection frmCollection, string returnUrl)
        {
            var viewModel = new ThreadAdminIndexViewModel { ListThreadEntry = new List<ThreadEntry>(), Options = new ThreadOptions() };
            UpdateModel(viewModel);

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var checkedEntries = viewModel.ListThreadEntry.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case ThreadBulkAction.None:
                    break;
                case ThreadBulkAction.Enable:
                    foreach (var entry in checkedEntries)
                    {
                        _threadService.CloseOrOpenThread(entry.ThreadPart.Id);
                        _threadService.ClearCacheUpdateThread(hostname);
                    }
                    break;
                case ThreadBulkAction.Disable:
                    foreach (var entry in checkedEntries)
                    {
                        _threadService.CloseOrOpenThread(entry.ThreadPart.Id);
                        _threadService.ClearCacheUpdateThread(hostname);
                    }
                    break;
                case ThreadBulkAction.Delete:
                    foreach (var entry in from entry in checkedEntries let isDelete = _threadService.DeleteThreadById(entry.ThreadPart.Id, hostname) where !isDelete select entry)
                    {
                        Services.Notifier.Information(T("Chuyên mục {0} có tồn tại chuyên đề, không thể xóa!", entry.ThreadPart.Name));
                    }
                    break;
            }

            return RedirectToAction("HThreadIndex", ControllerContext.RouteData.Values);
        }

        public ActionResult HThreadCreate(string returnUrl)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var thread = Services.ContentManager.New<ForumThreadPart>("ForumThread");
            dynamic model = Services.ContentManager.BuildEditor(thread);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Create", Model: new ThreadAdminCreateViewModel { ReturnUrl = returnUrl }, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost]
        public ActionResult HThreadCreate(ThreadAdminCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var thread = Services.ContentManager.New<ForumThreadPart>("ForumThread");

            if (!string.IsNullOrEmpty(createModel.Name) && _threadService.CheckIsExistThreadName(hostname, createModel.Name))
                AddModelError("ThreadNameExist", T("Tên chuyên mục đã tồn tại. Vui lòng nhập tên khác."));
            if (!string.IsNullOrEmpty(createModel.ShortName) && _threadService.CheckIsExistThreadShortName(hostname, createModel.ShortName))
                AddModelError("ThreadShortNameExist", T("ShortName chuyên mục đã tồn tại. Vui lòng nhập tên khác."));

            if (ModelState.IsValid)
            {
                thread.Name = createModel.Name;
                thread.Description = createModel.Description;
                thread.SeqOrder = createModel.SeqOrder;
                thread.ShortName = createModel.ShortName;
                thread.DefaultImage = createModel.DefaultImage;
                thread.IsOpen = createModel.IsOpen;
                thread.ParentThreadId = null;
                thread.DateCreated = DateTime.Now;
                thread.HostName = hostname;

                Services.ContentManager.Create(thread);

                // Upload Icon
                var imageUpload = Request.Files["DefaultImage"];
                if (imageUpload != null && imageUpload.ContentLength > 0)
                {
                    thread.DefaultImage = _threadService.UploadFileThreadIcon(imageUpload, thread.Id);
                }

                _threadService.ClearCacheUpdateThread(hostname);//Clear Cache

                Services.Notifier.Information(T("Chuyên mục {0} đã được tạo thành công.", thread.Name));
            }


            dynamic model = Services.ContentManager.UpdateEditor(thread, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.Redirect(createModel.ReturnUrl);
            }
            return RedirectToAction("HThreadindex");
        }

        public ActionResult HThreadEdit(int id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var threadForum = Services.ContentManager.Get<ForumThreadPart>(id);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Edit", Model: new ThreadAdminEditViewModel { ForumThread = threadForum, ReturnUrl = returnUrl }, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(threadForum);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost]
        public ActionResult HThreadEdit(int Id)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var threadForum = Services.ContentManager.Get<ForumThreadPart>(Id);
            dynamic model = Services.ContentManager.UpdateEditor(threadForum, this);

            var editModel = new ThreadAdminEditViewModel { ForumThread = threadForum };

            if (TryUpdateModel(editModel))
            {
                if (_threadService.CheckIsExistThreadName(hostname,Id, editModel.Name))
                {
                    AddModelError("ThreadNameExist", T("Tên chuyên mục đã tồn tại. Vui lòng nhập tên khác."));
                }
                if (_threadService.CheckIsExistThreadName(hostname, Id, editModel.ShortName))
                {
                    AddModelError("ThreadShortNameExist", T("ShortName chuyên mục đã tồn tại. Vui lòng nhập tên khác."));
                }

                if (ModelState.IsValid)
                {
                    threadForum.Name = editModel.Name;
                    threadForum.ShortName = editModel.ShortName;
                    threadForum.Description = editModel.Description;
                    threadForum.SeqOrder = editModel.SeqOrder;
                    threadForum.IsOpen = editModel.IsOpen;

                    // Upload Icon
                    var imageUpload = Request.Files["DefaultImage"];
                    if (imageUpload != null && imageUpload.ContentLength > 0)
                    {
                        threadForum.DefaultImage = _threadService.UploadFileThreadIcon(imageUpload, threadForum.Id);
                    }

                    _threadService.ClearCacheUpdateThread(hostname);//Clear Cache
                    Services.Notifier.Information(T("Chuyên mục {0} đã cập nhật thành công.", threadForum.Name));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Thread.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.Redirect(editModel.ReturnUrl);

            }
            return RedirectToAction("HThreadIndex");
        }

        public ActionResult HThreadDelete(int Id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            bool isDelete = _threadService.DeleteThreadById(Id, hostname);
            _threadService.ClearCacheUpdateThread(hostname);//Clear Cache
            if (!isDelete)
            {
                Services.Notifier.Information(T("Chuyên mục {0} đang tồn tại chuyên đề, bạn không thể xóa được.", Id));
                return RedirectToAction("index");
            }
            else
            {
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("index");
            }
        }

        #endregion

        #region //Topic
        public ActionResult TopicIndex(int Id,ThreadOptions options, PagerParameters pagerParameters)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var listTopic = _threadService.GetListTopicFromCache(Id, hostname);

            #region ORDER
            switch (options.Order)
            {
                case ThreadOrder.Id:
                    listTopic = listTopic.OrderBy(u => u.Id);
                    break;
                case ThreadOrder.Name:
                    listTopic = listTopic.OrderBy(u => u.Name);
                    break;
                case ThreadOrder.ShortName:
                    listTopic = listTopic.OrderBy(u => u.ShortName);
                    break;

            }
            #endregion

            var pagerShape = Shape.Pager(pager).TotalItemCount(listTopic.Count());
            var results = listTopic
                .Skip(pager.GetStartIndex()).Take(pager.PageSize)
                .ToList();

            #region BUILD MODEL
            var model = new ThreadAdminIndexViewModel
            {
                ListThreadEntry = results.Select(c => new ThreadEntry
                {
                    ThreadPart = c,
                    CountChild = _postAdminService.CountPostByTopicToCache(c.Id, hostname)
                }).ToList(),
                ThreadInfo = _threadService.GetThreadInfoById(hostname, Id),
                Options = options,
                Pager = pagerShape
            };
            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();
            routeData.Values.Add("Options.ThreadOrder", options.Order);

            pagerShape.RouteData(routeData);

            #endregion
            return View(model);
        }
        [HttpPost]
        public ActionResult TopicIndex(FormCollection frmCollection, string returnUrl)
        {
            try
            {
                var viewModel = new ThreadAdminIndexViewModel { ListThreadEntry = new List<ThreadEntry>(), Options = new ThreadOptions() };
                UpdateModel(viewModel);

                var user = Services.WorkContext.CurrentUser.As<UserPart>();
                string hostname = _groupService.GetHostNameByUser(user);

                var checkedEntries = viewModel.ListThreadEntry.Where(c => c.IsChecked);
                switch (viewModel.Options.BulkAction)
                {
                    case ThreadBulkAction.None:
                        break;
                    case ThreadBulkAction.Delete:
                        foreach (var entry in checkedEntries)
                        {

                            bool isDelete = _threadService.DeleteTopicById(entry.ThreadPart.Id, hostname);
                            if (!isDelete)
                                Services.Notifier.Information(T("Chuyên đề {0} có tồn tại bài viết, không thể xóa!", entry.ThreadPart.Name));

                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("Error: {0}", ex.Message));
            }
            return RedirectToAction("TopicIndex", ControllerContext.RouteData.Values);
        }

        public ActionResult TopicCreate(int Id,string returnUrl)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var thread = Services.ContentManager.New<ForumThreadPart>("ForumThread");
            dynamic model = Services.ContentManager.BuildEditor(thread);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Topic.Create", Model: new ThreadAdminCreateViewModel { ReturnUrl = returnUrl, ThreadId = Id }, Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost]
        public ActionResult TopicCreate(ThreadAdminCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var thread = Services.ContentManager.New<ForumThreadPart>("ForumThread");
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            if (!string.IsNullOrEmpty(createModel.Name) && _threadService.CheckIsExistTopicName(hostname, createModel.ThreadId, createModel.Name))
                AddModelError("TopicNameExist", T("Tên chuyên đề đã tồn tại. Vui lòng nhập tên khác."));
            if (!string.IsNullOrEmpty(createModel.ShortName) && _threadService.CheckIsExistTopicShortName(hostname, createModel.ThreadId, createModel.ShortName))
                AddModelError("TopicShortNameExist", T("ShortName chuyên đề đã tồn tại. Vui lòng nhập tên khác."));

            if (ModelState.IsValid)
            {
                thread.Name = createModel.Name;
                thread.Description = createModel.Description;
                thread.SeqOrder = createModel.SeqOrder;
                thread.ShortName = createModel.ShortName;
                thread.ParentThreadId = createModel.ThreadId;
                thread.DateCreated = DateTime.Now;
                thread.HostName = hostname;

                Services.ContentManager.Create(thread);

                _threadService.ClearCacheUpdateTopic(thread, hostname);//Clear Cache

                Services.Notifier.Information(T("Chuyên đề {0} đã được tạo thành công.", thread.Name));
            }


            dynamic model = Services.ContentManager.UpdateEditor(thread, this);
            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Topic.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            if (!String.IsNullOrEmpty(createModel.ReturnUrl))
            {
                return this.Redirect(createModel.ReturnUrl);
            }
            return RedirectToAction("index");
        }

        public ActionResult TopicEdit(int id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var topicForum = Services.ContentManager.Get<ForumThreadPart>(id);

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Topic.Edit", Model: new ThreadAdminEditViewModel
            {
                ForumThread = topicForum,
                ReturnUrl = returnUrl,
                ListThread = _threadService.GetListThreadFromCache(hostname).Select(r => new ThreadInfo() { Id = r.Id, Name = r.Name }).ToList()
            }
            , Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(topicForum);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }
        [HttpPost]
        public ActionResult TopicEdit(int id)
        {
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var topicForum = Services.ContentManager.Get<ForumThreadPart>(id);
            dynamic model = Services.ContentManager.UpdateEditor(topicForum, this);

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var editModel = new ThreadAdminEditViewModel { ForumThread = topicForum };
            int newThreadId = editModel.ThreadId.Value;

            if (TryUpdateModel(editModel))
            {
                if (_threadService.CheckIsExistTopicName(hostname, topicForum.ParentThreadId.Value, id, editModel.Name))
                    AddModelError("ThreadNameExist", T("Tên chuyên đề đã tồn tại. Vui lòng nhập tên khác."));
                if (_threadService.CheckIsExistTopicShortName(hostname, topicForum.ParentThreadId.Value, id, editModel.ShortName))
                    AddModelError("ThreadShortNameExist", T("ShortName chuyên đề đã tồn tại. Vui lòng nhập tên khác."));

                if (ModelState.IsValid)
                {
                    topicForum.Name = editModel.Name;
                    topicForum.ShortName = editModel.ShortName;
                    topicForum.SeqOrder = editModel.SeqOrder;
                    topicForum.Description = editModel.Description;

                    _threadService.ClearCacheUpdateTopic(topicForum, hostname);//Clear Cache
                    if (newThreadId != topicForum.ParentThreadId.Value)
                    {
                        topicForum.ParentThreadId = editModel.ThreadId;
                    }
                    Services.Notifier.Information(T("Chuyên đề {0} đã cập nhật thành công.", topicForum.Name));
                }
            }

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Topic.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.Redirect(editModel.ReturnUrl);

            }
            return RedirectToAction("TopicIndex");
        }

        public ActionResult TopicDelete(int Id, string returnUrl)
        {
            // Permision here
            if (!Services.Authorizer.Authorize(MiniForumPermissions.ManagementMiniForum, T("Không có quyền quản lý Forum")))
                return new HttpUnauthorizedResult();

            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            bool isDelete = _threadService.DeleteTopicById(Id, hostname);
            if (!isDelete)
            {
                Services.Notifier.Information(T("Chuyên đề {0} đang tồn tại bài viết, bạn không thể xóa được.", Id));
                return RedirectToAction("TopicIndex");
            }
            else
            {
                Services.Notifier.Information(T("Đã xóa thành công chuyên đề {0}.", Id));
                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect(returnUrl);
                else
                    return RedirectToAction("TopicIndex");
            }
        }
        #endregion

        #region //Ajax

        [HttpPost]
        public JsonResult AjaxLoadTopic(int threadId)
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            string hostname = _groupService.GetHostNameByUser(user);

            var listTopic = _threadService.GetListTopicFromCache(threadId, hostname).Select(r => new HashViewModel { Id = r.Id, Value = r.Name }).ToList();

            return Json(new { list = listTopic});
        }
        #endregion

        #region //UpdateModel
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