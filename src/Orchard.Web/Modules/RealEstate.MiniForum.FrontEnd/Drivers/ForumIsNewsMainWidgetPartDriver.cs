using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;
using RealEstate.Services;
using RealEstateForum.Service.Services;

namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class ForumIsNewsMainWidgetPartDriver : ContentPartDriver<ForumIsNewsMainWidgetPart>
    {
        private readonly IPostForumFrontEndService _postForumService;
        private readonly IHostNameService _hostNameService;

        public ForumIsNewsMainWidgetPartDriver(IPostForumFrontEndService postForumService, IHostNameService hostNameService)
        {
            _postForumService = postForumService;
            _hostNameService = hostNameService;
        }


        protected override DriverResult Display(ForumIsNewsMainWidgetPart part, string displayType, dynamic shapeHelper)
        {
            return ContentShape("Parts_ForumIsNewsMainWidget", () =>
            {
                var shape = shapeHelper.Parts_ForumIsNewsMainWidget();
                return shape;
            });
        }
    }
}