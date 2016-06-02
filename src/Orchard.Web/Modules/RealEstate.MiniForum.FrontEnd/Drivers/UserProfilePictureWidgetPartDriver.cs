using Orchard.ContentManagement.Drivers;
using RealEstate.MiniForum.FrontEnd.Models;
using RealEstateForum.Service.Services;
using RealEstateForum.Service.ViewModels;
namespace RealEstate.MiniForum.FrontEnd.Drivers
{
    public class UserProfilePictureWidgetPartDriver : ContentPartDriver<UserProfilePictureWidgetPart>
    {
        public UserProfilePictureWidgetPartDriver(IUserForumService userforumservices)
        {
            _userforumservices = userforumservices;
        }
        private readonly IUserForumService _userforumservices;
        protected override DriverResult Display(UserProfilePictureWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var model = new UserUpdateProfileOptions();
            return ContentShape("Parts_UserProfilePictureWidget", () =>
            {
                var shape = shapeHelper.Parts_UserProfilePictureWidget();
                shape.ContentPart = part;
                shape.ViewModel = _userforumservices.GetProfileUserPart(model);
                return shape;
            });
        }
    }
}
