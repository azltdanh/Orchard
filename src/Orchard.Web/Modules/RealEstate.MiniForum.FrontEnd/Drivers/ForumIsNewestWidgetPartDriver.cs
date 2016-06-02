using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;
using RealEstate.Services;
using RealEstateForum.Service.Services;
namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsNewestWidgetPartDriver : ContentPartDriver<ForumIsNewestWidgetPart>
    {
        public ForumIsNewestWidgetPartDriver(IPostForumFrontEndService postForumService, IHostNameService hostNameService)
        {
            _postForumService = postForumService;
            _hostNameService = hostNameService;
        }
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IHostNameService _hostNameService;

        protected override DriverResult Display(ForumIsNewestWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsNewestWidget", () =>
            {
                string hostname = _hostNameService.GetHostNameSite();
                var shape = shapeHelper.Parts_ForumIsNewestWidget();
                shape.ContentPart = part;
                shape.ViewModel = _postForumService.BuildListPostNewestWdiget(hostname);
                return shape;
            });
        }
    }
}