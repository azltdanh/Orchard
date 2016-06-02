using System.Linq;
using System.Web.Mvc;
using System;

using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using RealEstate.MiniForum.FrontEnd.Models;
using RealEstateForum.Service.Services;
using Orchard.ContentManagement;
using RealEstate.Services;
using RealEstateForum.Service.ViewModels;
using RealEstate.MiniForum.FrontEnd.Controllers;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumByThreadWidgetPartDriver : ContentPartDriver<ForumByThreadWidgetPart>
    {
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IThreadAdminService _threadService;
        private readonly IHostNameService _hostNameService;
        private readonly IFileCacheService _fileCacheService;
        private readonly IContentManager _contentManager;
        //private readonly ThreadForumFrontEndController _threadController;
        public ForumByThreadWidgetPartDriver(
            IOrchardServices services, 
            IPostForumFrontEndService postForumService, 
            IThreadAdminService threadService, 
            IHostNameService hostNameService,
            IFileCacheService fileCacheService,
            IContentManager contentManager)
        {
            _postForumService = postForumService;
            _threadService = threadService;
            _hostNameService = hostNameService;
            _fileCacheService = fileCacheService;
            _contentManager = contentManager;
            
            Services = services;
            T = NullLocalizer.Instance;

        }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        protected override DriverResult Display(ForumByThreadWidgetPart part, string displayType, dynamic shapeHelper)
        {
            //  lấy chuyên mục theo 
            var hostname = _hostNameService.GetHostNameSite();
            int threadId = Convert.ToInt32(part.ThreadName);
            var model = new ThreadForumFrontEndViewModel();

            model.ContentFromFile = _fileCacheService.ThreadCacheFromFile(threadId);
            model.ThreadInfo = _threadService.GetThreadInfoById(hostname,threadId);
            if (model.ContentFromFile == null)
            {
                model = _postForumService.BuildListPostByThread(threadId, hostname);

                //model.ContentFromFile = "";//RenderView();---Continue

                //_fileCacheService.ThreadCacheToFile(threadId, model.ContentFromFile);
            }

            return ContentShape("Parts_ForumByThreadWidget",
                               () => shapeHelper.DisplayTemplate(
                                   TemplateName: "Parts/ForumByThreadWidget",
                                   Model: model,
                                   Prefix: Prefix));
        }

        //GET
        protected override DriverResult Editor(ForumByThreadWidgetPart part, dynamic shapeHelper)
        {
            //var hostname = _hostNameService.GetHostNameSite();
            part.AvailableThreads = _threadService.GetListThread().OrderBy(r => r.SeqOrder)
                                        .Select(a => new SelectListItem
                                        {
                                            Text = a.Name,
                                            Value = Convert.ToString(a.Id),
                                        });

            if (!string.IsNullOrWhiteSpace(part.ThreadName))
            {
                part.SelectedCategory = part.ThreadName;
            }
            else
            {
                var first = part.AvailableThreads.FirstOrDefault();
                part.SelectedCategory = first == null
                                           ? string.Empty
                                           : first.Value;
            }

            return ContentShape("Parts_ForumByThreadWidget_Edit",
                                () => shapeHelper.EditorTemplate(
                                    TemplateName: "Parts/ForumByThreadWidget.Edit",
                                    Model: part,
                                    Prefix: Prefix));
        }

        //POST
        protected override DriverResult Editor(ForumByThreadWidgetPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            part.ThreadName = part.SelectedCategory;
            return Editor(part, shapeHelper);
        }
    }
}