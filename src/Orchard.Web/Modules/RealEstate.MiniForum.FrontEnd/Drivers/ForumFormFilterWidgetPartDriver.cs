using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;
using RealEstate.Services;
using RealEstateForum.Service.Services;
using RealEstateForum.Service.ViewModels;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumFormFilterWidgetPartDriver : ContentPartDriver<ForumFormFilterWidgetPart>
    {
        public ForumFormFilterWidgetPartDriver(IPostForumFrontEndService postForumService, IHostNameService hostNameService)
        {
            _postForumService = postForumService;
            _hostNameService = hostNameService;
        }
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IHostNameService _hostNameService;


        protected override DriverResult Display(ForumFormFilterWidgetPart part, string displayType, dynamic shapeHelper)
        {
            string hostname = _hostNameService.GetHostNameSite();
            return ContentShape("Parts_ForumFormFilterWidget",
                               () => shapeHelper.DisplayTemplate(
                                   TemplateName: "Parts/ForumFormFilterWidget",
                                   Model: _postForumService.BuildPostFilterOption(new PostFilterOptions(), hostname),
                                   Prefix: null));
        }
    }
}