using System.Web.Mvc;

using Orchard;
using Orchard.Themes;
using Orchard.Settings;
using Orchard.DisplayManagement;
using Orchard.ContentManagement;
using Orchard.Localization;


namespace RealEstate.MiniForum.FrontEnd.Controllers
{
      [Themed]
    public class ImportUpdateUserController : Controller , IUpdateModel
    {
        private readonly ISiteService _siteService;
        public ImportUpdateUserController(
            IOrchardServices services,
            ISiteService siteService,
            IShapeFactory shapeFactory)
        {
            Services = services;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }
        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        #region Import
        //public ActionResult ImportUpdateUser()
        //{
        //    if (!Services.Authorizer.Authorize(Permissions.ManagePropertySettings, T("Not authorized to list frontendSettings")))
        //        return new HttpUnauthorizedResult();

        //    var results = Services.ContentManager.Query<UserPart, UserPartRecord>().List();

        //    foreach (var item in results)
        //    {
        //        if (results != null)
        //        {
        //            var re = Services.ContentManager.Get(item.Id);

        //            var userPart = re.As<UserPart>();
        //            var userinfoPart = re.As<UserPersonalInformationPart>();
        //            var avatar = re.As<AvatarProfilePart>();
        //            var userProfile = re.As<UserProfileForumInfomationPart>();
        //            var userInfomation = re.As<UserProfileInfomationPart>();
        //            var userupdate = re.As<UserUpdateProfilePart>();

        //            var detailModel = new UseProFileEntry
        //            {
        //                UserInformation = userPart,
        //                PersonalInformation = userinfoPart,
        //                AvatarInfomation = avatar,
        //                ProfileForumInformation = userProfile,
        //                UserInfomation = userInfomation
        //            };

        //            if (TryUpdateModel(detailModel))
        //            {
        //                if (!string.IsNullOrEmpty(detailModel.AvatarInfomation.ImageUrl))
        //                    userupdate.Avatar = detailModel.AvatarInfomation.ImageUrl;

        //                if (!string.IsNullOrEmpty(detailModel.PersonalInformation.FirstName))
        //                    userupdate.FirstName = detailModel.PersonalInformation.FirstName;

        //                if (!string.IsNullOrEmpty(detailModel.PersonalInformation.LastName))
        //                    userupdate.LastName = detailModel.PersonalInformation.LastName;

        //                if (!string.IsNullOrEmpty(detailModel.PersonalInformation.DisplayName))
        //                    userupdate.DisplayName = detailModel.PersonalInformation.DisplayName;

        //                if (!string.IsNullOrEmpty(detailModel.UserInfomation.DateOfBirth.ToString()))
        //                    userupdate.DateOfBirth = detailModel.UserInfomation.DateOfBirth;
        //                //userupdate.Gender = detailModel.PersonalInformation.Gender;

        //                if (!string.IsNullOrEmpty(detailModel.UserInfomation.Address))
        //                    userupdate.Address = detailModel.UserInfomation.Address;

        //                if (!string.IsNullOrEmpty(detailModel.UserInfomation.Phone))
        //                    userupdate.Phone = detailModel.UserInfomation.Phone;

        //                if (!string.IsNullOrEmpty(detailModel.ProfileForumInformation.Email))
        //                    userupdate.Email = detailModel.ProfileForumInformation.Email;

        //                if (!string.IsNullOrEmpty(detailModel.ProfileForumInformation.Job))
        //                    userupdate.Job = detailModel.ProfileForumInformation.Job;

        //                if (!string.IsNullOrEmpty(detailModel.ProfileForumInformation.Level))
        //                    userupdate.Level = detailModel.ProfileForumInformation.Level;

        //                if (!string.IsNullOrEmpty(detailModel.UserInfomation.Website))
        //                    userupdate.Website = detailModel.UserInfomation.Website;

        //                if (!string.IsNullOrEmpty(detailModel.ProfileForumInformation.Favourite))
        //                    userupdate.Note = detailModel.ProfileForumInformation.Favourite;

        //                if (!string.IsNullOrEmpty(detailModel.ProfileForumInformation.SignatureContent))
        //                    userupdate.Signature = detailModel.ProfileForumInformation.SignatureContent;

        //                userupdate.IsSignature = detailModel.ProfileForumInformation.CheckShowSignature;

        //                Services.ContentManager.Publish(userupdate.ContentItem);
        //            }

        //            Services.Notifier.Information(T("User {0} cập nhật thành công!", item.Id));
        //        }

        //    }
        //    Services.Notifier.Information(T("{0} cập nhật thành công!", results.Count()));

        //    return View();
        //}
        #endregion
      
        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}
