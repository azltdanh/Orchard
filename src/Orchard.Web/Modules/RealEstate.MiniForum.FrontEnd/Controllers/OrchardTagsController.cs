using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.Tags.Services;
using Orchard.Themes;
using RealEstate.Services;
using Orchard.UI.Navigation;
using RealEstateForum.Service.ViewModels;
using RealEstate.ViewModels;
using RealEstateForum.Service.Services;
using Orchard.UI.Notify;


namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    [Themed]
    public class OrchardTagsController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly ITagService _tagService;
        private readonly IContentManager _contentManager;
        private readonly IHostNameService _hostNameService;
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IPropertyService _propertyService;
        private readonly IPostAdminService _postAdminService;

        public OrchardTagsController(
            IOrchardServices services,
            ISiteService siteService,
            ITagService tagService,
            IContentManager contentManager,
            IPostForumFrontEndService postForumService,
            IPropertyService propertyService,
            IHostNameService hostNameService,
            IShapeFactory shapeFactory,
            IPostAdminService postAdminService)
        {
            Services = services;
            _siteService = siteService;
            _tagService = tagService;
            _contentManager = contentManager;
            _postForumService = postForumService;
            _propertyService = propertyService;
            _postAdminService = postAdminService;
            _hostNameService = hostNameService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }
        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public ActionResult Search(int tagId, PagerParameters pagerParameters)
        {
            var tag = _tagService.GetTag(tagId);

            return RedirectToAction("SearchByTagName", new { tagName = tag.TagName });
        }

        public ActionResult SearchByTagName(string tagName, PagerParameters pagerParameters)
        {
            var pagerForumPost = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var pagerProperty = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var tag = _tagService.GetTagByName(tagName);

            if (tag == null)
            {
                return Redirect("/");
            }

            var taggedItems = _tagService.GetTaggedContentItems(tag.Id).ToList();

            if(taggedItems == null )
                return Redirect("/");

            var listForumPostId = new List<int>();
            var listPropertyId = new List<int>();

            foreach (var content in taggedItems)
            {
                switch (content.ContentItem.ContentType)
                {
                    case "ForumPost":
                        listForumPostId.Add(content.ContentItem.Id);
                        break;
                    case "Property":
                        listPropertyId.Add(content.ContentItem.Id);
                        break;
                }
            }
            #region PageForumPart

            string hostname = _hostNameService.GetHostNameSite();
            var pageForumList = _postAdminService.GetListPostQueryByHostName(hostname).Where(c => listForumPostId.Contains(c.Id));
            var pageTotalCount = pageForumList.Count();
            var pagerForumShape = Shape.Pager(pagerForumPost).TotalItemCount(pageTotalCount);

            var results = pageForumList
                .Slice(pagerForumPost.GetStartIndex(), pagerForumPost.PageSize)
                .OrderBy(p => p.SeqOrder)
                .ThenByDescending(p => p.DateCreated)
                .ToList();

            #endregion

            #region PropertyPart

            var propertyList = _propertyService.GetProperties().Where(c => listPropertyId.Contains(c.Id));
            var propertyCount = propertyList.Count();
            var pagerPropertyShape = Shape.Pager(pagerProperty).TotalItemCount(propertyCount);
            //if (pagerProperty.PageSize < 5) pagerProperty.PageSize = 5;
            //if (pagerProperty.PageSize > 100) pagerProperty.PageSize = 100;

            var resultsProperty = propertyList
                .Slice(pagerProperty.GetStartIndex(), pagerProperty.PageSize)
                .OrderByDescending(p => p.Id)
                .ToList();

            #endregion

            //var totalItemCount = _tagService.GetTaggedContentItemCount(tag.Id); .StripHtml()
            var viewModel = new OrchardTagsViewModel
            {
                TopicForum = new TopicForumFrontEndViewModel
                {
                    TopicInfo = new TopicInfo
                    {
                        ListPostItem = _postForumService.BuildPostByTopicEntry(results)
                    },
                    Pager = pagerForumShape,
                    TotalCount = pageTotalCount,
                },
                TagName = tag.TagName,
                ListPropertyPart = new PropertyDisplayIndexViewModel
                {
                    Properties = _propertyService.BuildPropertiesEntries(resultsProperty),
                    Pager = pagerPropertyShape,
                    TotalCount = propertyCount,
                }
            };

            return View("Search", viewModel);
        }

    }
}