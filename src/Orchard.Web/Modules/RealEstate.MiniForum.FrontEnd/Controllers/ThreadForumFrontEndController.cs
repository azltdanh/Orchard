using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

using Orchard;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.Settings;

using RealEstateForum.Service.Services;
using RealEstateForum.Service.ViewModels;
using RealEstateForum.Service.Models;
using RealEstate.Services;
using RealEstate.FrontEnd.Services;
using RealEstate.Helpers;



namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    [Themed]
    public class ThreadForumFrontEndController : Controller
    {
        private readonly IThreadAdminService _threadService;
        private readonly IPostAdminService _postService;
        private readonly ISiteService _siteService;
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IHostNameService _hostNameService;
        private readonly IFastFilterService _fastFilterService;
        private readonly IFileCacheService _fileCacheService;

        public ThreadForumFrontEndController(
            IOrchardServices services, 
            IThreadAdminService threadService, 
            IShapeFactory shapeFactory, 
            IPostAdminService postService, 
            IPostForumFrontEndService postForumService,
            IHostNameService hostNameService,
            IFastFilterService fastFilterService,
            IFileCacheService fileCacheService,
            ISiteService siteService
            )
        {
            _threadService = threadService;
            _postService = postService;
            _postForumService = postForumService;
            _fastFilterService = fastFilterService;
            _hostNameService = hostNameService;
            _fileCacheService = fileCacheService;
            _siteService = siteService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }
        dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult HomeForum()
        {
            string path = Url.Action("HomeForum", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd" });
            ViewBag.Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0,1));
            return View();
        }
        #region Danh sách bài viết theo chuyên mục

        public ActionResult ListPostByThread(string shortName)
        {
            string hostname = _hostNameService.GetHostNameSite();
            var thread = _threadService.GetThreadInfoByShortName(hostname,shortName);
            if (thread == null)
                return RedirectToAction("ForumNotFound", "ForumError", new {area = "RealEstate.MiniForum.FrontEnd"});

            string path = Url.Action("ListPostByThread", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", ShortName = shortName });//, ThreadId = thread.Id

            var model = new ThreadForumFrontEndViewModel();//_postForumService.BuildListPostByThread(thread.Id, hostname);
            model.Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0,1));

            //Cache file
            model.ContentFromFile = _fileCacheService.ThreadCacheFromFile(thread.Id);
            model.ThreadInfo = thread;
            if (model.ContentFromFile == null)
            {
                var contentModel = _postForumService.BuildListPostByThread(thread.Id, hostname);

                model.ContentFromFile = this.RenderView("ThreadContentFileCache", contentModel);//RenderView(contentModel);
                _fileCacheService.ThreadCacheToFile(thread.Id, model.ContentFromFile);
            }

            return View(model);
        }

        #endregion 

        #region Danh sách bài viết theo chuyên đề

        public ActionResult ViewTopic(int topicId, PagerParameters pagerParameters)
        {
            string path = Url.Action("ViewTopic", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd", TopicId = topicId });
            string hostname = _hostNameService.GetHostNameSite();

            var topicPart = _threadService.GetTopicPartRecordById(topicId, hostname);
            if (topicPart != null)
            {
                var thread = _threadService.GetThreadInfoByTopic(hostname, topicPart.Record);
                string topicShortName = _threadService.GetTopicPartRecordById(topicId,hostname).ShortName;

                var listPost = _postService.GetListPostQueryByHostName(hostname).Where(r => r.Thread.Id == topicId).OrderByDescending(r => r.DateUpdated);
                var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
                int totalCount = listPost.Count();

                var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);
                IEnumerable<ForumPostPart> results;//.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList(); ;

                results = listPost.Where(r => string.IsNullOrEmpty(r.HostName)).Slice(pager.GetStartIndex(),pager.PageSize).ToList();

                //Services.Notifier.Information(T("results: {0}", results.Count()));
                var model = new TopicForumFrontEndViewModel()
                {
                    TopicInfo = new TopicInfo
                    {
                        ListPostItem = _postForumService.BuildPostByTopicEntry(results,thread),
                        TopicName = topicPart.Name,
                        ThreadShortName = thread.ShortName,
                        TopicShortName = topicShortName,
                        ThreadId = thread.Id,
                    },
                    Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0,1)),
                    Pager = pagerShape,
                    TotalCount = totalCount
                };

                #region ROUTE DATA

                var routeData = new RouteData();

                pagerShape.RouteData(routeData);

                #endregion

                return View(model);
            }
            else
            {
                return RedirectToAction("ForumNotFound", "ForumError", new { area = "RealEstate.MiniForum.FrontEnd" });
            }
        }

        // Tin tức nổi bật
        public ActionResult ListPostIsHighlight(PagerParameters pagerParameters)
        {
            string path = Url.Action("ListPostIsHighlight", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd" });

            //DateTime gethostname = DateTime.Now;
            string hostname = _hostNameService.GetHostNameSite();
            //Services.Notifier.Information(T("gethostname: {0}", (DateTime.Now - gethostname).TotalSeconds));

            //DateTime startlistPost = DateTime.Now;
            var listPost = _postService.GetListPostQueryByHostName(hostname).Where(r => r.IsHeighLight).OrderByDescending(r => r.DateUpdated);
            //Services.Notifier.Information(T("startlistPost: {0}", (DateTime.Now - startlistPost).TotalSeconds));

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            //DateTime starttotalCount = DateTime.Now;
            int totalCount = listPost.Count();
            //Services.Notifier.Information(T("starttotalCount: {0}", (DateTime.Now - starttotalCount).TotalSeconds));

            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            //DateTime startresults = DateTime.Now;
            var results = listPost.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            //Services.Notifier.Information(T("startresults: {0}", (DateTime.Now - startresults).TotalSeconds));

            //DateTime startmodel = DateTime.Now;
            var model = new TopicForumFrontEndViewModel()
            {
                TopicInfo = new TopicInfo
                {
                    ListPostItem = _postForumService.BuildPostByTopicEntry(results),
                    TopicName = "Tin tức nổi bật",
                },
                Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0, 1)),
                Pager = pagerShape,
                TotalCount = totalCount
            };
            //Services.Notifier.Information(T("startmodel: {0}", (DateTime.Now - startmodel).TotalSeconds));

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        // Tin thị trường
        public ActionResult ListPostIsMarket(PagerParameters pagerParameters)
        {
            string path = Url.Action("ListPostIsMarket", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd" });

            //DateTime gethostname = DateTime.Now;
            string hostname = _hostNameService.GetHostNameSite();
            //Services.Notifier.Information(T("gethostname: {0}", (DateTime.Now - gethostname).TotalSeconds));

            //DateTime startlistPost = DateTime.Now;
            var listPost = _postService.GetListPostQueryByHostName(hostname).Where(r => r.IsMarket).OrderByDescending(r => r.DateUpdated);
            //Services.Notifier.Information(T("startlistPost: {0}", (DateTime.Now - startlistPost).TotalSeconds));

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            //DateTime starttotalCount = DateTime.Now;
            int totalCount = listPost.Count();
            //Services.Notifier.Information(T("starttotalCount: {0}", (DateTime.Now - starttotalCount).TotalSeconds));

            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            //DateTime startresults = DateTime.Now;
            var results = listPost.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            //Services.Notifier.Information(T("startresults: {0}", (DateTime.Now - startresults).TotalSeconds));

            //DateTime startmodel = DateTime.Now;
            var model = new TopicForumFrontEndViewModel()
            {
                TopicInfo = new TopicInfo
                {
                    ListPostItem = _postForumService.BuildPostByTopicEntry(results),
                    TopicName = "Tin tức thị trường",
                },
                Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0,1)),
                Pager = pagerShape,
                TotalCount = totalCount
            };
            //Services.Notifier.Information(T("startmodel: {0}", (DateTime.Now - startmodel).TotalSeconds));

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }
        // Tin dự án
        public ActionResult ListPostIsProject(PagerParameters pagerParameters)
        {
            string path = Url.Action("ListPostIsProject", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd" });
            string hostname = _hostNameService.GetHostNameSite();

            //DateTime start1 = DateTime.Now;
            var listPost = _postService.GetListPostQueryByHostName(hostname).Where(r=>r.IsProject).OrderByDescending(r => r.DateUpdated);
            //Services.Notifier.Information(T("start1: {0}", (DateTime.Now - start1)));
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            int totalCount = listPost.Count();

            //DateTime start2 = DateTime.Now;
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);//
            var results = listPost.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            //Services.Notifier.Information(T("start2: {0}", (DateTime.Now - start2)));


            //DateTime start3 = DateTime.Now;
            var model = new TopicForumFrontEndViewModel()
            {
                TopicInfo = new TopicInfo
                {
                    ListPostItem = _postForumService.BuildPostByTopicEntry(results),
                    TopicName = "Tin tức dự án bất động sản",
                },
                Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0,1)),
                Pager = pagerShape,
                TotalCount = totalCount
            };
            //Services.Notifier.Information(T("start3: {0}", (DateTime.Now - start3)));


            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View("ListPostIsMarket", model);
        }
        // Tin mới nhất
        public ActionResult ListPostNews(PagerParameters pagerParameters)
        {
            string path = Url.Action("ListPostNews", "ThreadForumFrontEnd", new { area = "RealEstate.MiniForum.FrontEnd" });
            string hostname = _hostNameService.GetHostNameSite();

            //hnrea khong ko hiển thị chuyen muc dien-dan.
            int threadId = 880570;
            var _notShowThreadId = _threadService.GetListTopicQueryByThreadId(hostname, threadId).List().Select(r => r.Id).ToList();
            //DateTime start1 = DateTime.Now;
            var listPost = _postService.GetListPostQueryByHostName(hostname).Where(a => !_notShowThreadId.Contains(a.Thread.Id)).OrderByDescending(r => r.DateUpdated);
            //Services.Notifier.Information(T("start1: {0}", (DateTime.Now - start1)));
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            int totalCount = listPost.Count();

            //DateTime start2 = DateTime.Now;
            var pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);//
            var results = listPost.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            //Services.Notifier.Information(T("start2: {0}", (DateTime.Now - start2)));

            //DateTime start3 = DateTime.Now;
            var model = new TopicForumFrontEndViewModel()
            {
                TopicInfo = new TopicInfo
                {
                    ListPostItem = _postForumService.BuildPostByTopicEntry(results),
                    TopicName = "Bài viết mới",
                },
                Metas = _fastFilterService.PropertyGetSeoMeta(path.Remove(0, 1)),
                Pager = pagerShape,
                TotalCount = totalCount
            };
            //Services.Notifier.Information(T("start3: {0}", (DateTime.Now - start3)));

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View("ListPostNews", model);
        }

        #endregion
    }
}