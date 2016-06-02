using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using Facebook;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Users.Models;
using Piedone.Facebook.Suite.Models;
using Piedone.Facebook.Suite.Services;
using RealEstate.Helpers;

namespace RealEstate.Services
{
    public interface IFacebookApiService : IDependency
    {
        bool HaveFacebookUserId();
        long GetFaceBookUserId(int userCurentId);
        string FacebookGetAccesstoken();

        /// <summary>
        ///     Post to facebook
        /// </summary>
        /// <param name="domain">http://dinhgianhadat.vn</param>
        /// <param name="linkDetail"></param>
        /// <param name="postTitle"></param>
        /// <param name="postContent"></param>
        /// <param name="fCaption">Diễn đàn bất động sản - Các thông tin thị trường BĐS mới nhất.</param>
        /// <param name="fMessage">Nơi giao lưu của những người quan tâm BĐS.</param>
        /// <param name="fDefaultImage">http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg</param>
        void PostToFaceBook(string domain, string linkDetail, string postTitle, string postContent, string fCaption,
            string fMessage, string fDefaultImage);

        /// <summary>
        ///     su dung cho duyet tin se dua thong tin bds nay len facebook cua nguoi dang
        /// </summary>
        /// <param name="linkdetail"> link trang chi tiết bđs</param>
        /// <param name="content">Nội dung: giống trang chi tiết bđs</param>
        /// <param name="linkimage">Link đại diện của bđs</param>
        /// <param name="userId">UserId của tin đó</param>
        /// <param name="message"></param>
        /// <param name="displaytitle">Tiêu đề của tin đó</param>
        void PostToYourFacebook(string linkdetail, string content, string message, string displaytitle, string linkimage,
            int userId);
    }

    public class ForumApiService : IFacebookApiService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IFacebookSuiteService _facebookSuiteService;
        private readonly ISignals _signals;
        private const int CacheTimeSpan = 60*24; // Cache for 24 hours

        public ForumApiService(
            IOrchardServices services,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IFacebookSuiteService facebookSuiteService,
            IContentManager contentManager)
        {
            Services = services;
            _contentManager = contentManager;
            _facebookSuiteService = facebookSuiteService;
            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region API Facebook

        public long GetFaceBookUserId(int userCurentId)
        {
            //return _cacheManager.Get("GetFaceBookUserIdCache_" + userCurentId, ctx =>
            //{
            //ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
            //ctx.Monitor(_signals.When("GetFaceBookUserIdCache_" + userCurentId + "_Changed"));

            var facebookUser = _contentManager.Query<FacebookUserPart, FacebookUserPartRecord>()
                .Where(c => c.Id == userCurentId).Slice(1).FirstOrDefault();
            if (facebookUser != null)
            {
                if (facebookUser.FacebookUserId != 0)
                {
                    return facebookUser.FacebookUserId;
                }
                return (long)(-1);
            }
            return (long)(-1);
            //});
        }

        public bool HaveFacebookUserId()
        {
            var user = Services.WorkContext.CurrentUser.As<UserPart>();

            return GetFaceBookUserId(user.Id) != -1;
        }

        public string FacebookGetAccesstoken()
        {
            string appId = _facebookSuiteService.SettingsPart.AppId;
            string appSecret = _facebookSuiteService.SettingsPart.AppSecret;
            const string scope = "publish_stream,manage_pages,offline_access";

            string url2 =
                string.Format(
                    "https://graph.facebook.com/oauth/access_token?grant_type=client_credentials&client_id={0}&client_secret={1}&scope={2}",
                    appId, appSecret, scope);


            var tokens = new Dictionary<string, string>();
            var request = WebRequest.Create(url2) as HttpWebRequest;

            if (request != null)
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        var reader = new StreamReader(response.GetResponseStream());

                        string vals = reader.ReadToEnd();

                        foreach (string token in vals.Split('&'))
                        {
                            tokens.Add(token.Substring(0, token.IndexOf("=")),
                                token.Substring(token.IndexOf("=") + 1, token.Length - token.IndexOf("=") - 1));
                        }
                    }
                }

            return tokens["access_token"];
        }

        /// <summary>
        ///     Post to facebook
        /// </summary>
        /// <param name="domain">http://dinhgianhadat.vn</param>
        /// <param name="linkDetail"></param>
        /// <param name="postTitle"></param>
        /// <param name="postContent"></param>
        /// <param name="fCaption">Diễn đàn bất động sản - Các thông tin thị trường BĐS mới nhất.</param>
        /// <param name="fMessage">Nơi giao lưu của những người quan tâm BĐS.</param>
        /// <param name="fDefaultImage">http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg</param>
        public void PostToFaceBook(string domain, string linkDetail, string postTitle, string postContent,
            string fCaption, string fMessage, string fDefaultImage)
        {
            int userCurentId = Services.WorkContext.CurrentUser.Id;
            if (GetFaceBookUserId(userCurentId) != -1)
            {
                string accessToken = FacebookGetAccesstoken();

                #region get pagecontent

                string content = postContent.StripHtml();
                string imageSrc = postContent.GetImagesSrc();
                string linkimage = !string.IsNullOrEmpty(imageSrc)
                    ? imageSrc.Contains("http://")
                        ? imageSrc
                        : domain + imageSrc
                    : fDefaultImage;

                #endregion

                #region postwall to facebook v2

                dynamic messagePost = new ExpandoObject();
                messagePost.picture = linkimage;
                messagePost.link = linkDetail.Contains("http://") ? linkDetail : domain + "/" + linkDetail;
                messagePost.name = postTitle;

                messagePost.caption = fCaption;
                messagePost.description = content;
                messagePost.message = fMessage;

                string acccessToken = accessToken;
                var appp = new FacebookClient(acccessToken);
                try
                {
                    JsonObject postId = appp.Post("/" + GetFaceBookUserId(userCurentId) + "/feed", messagePost);
                    //Services.Notifier.Information(T("FA ID {0}", postId["id"]));
                }
                catch (Exception)
                {
                    //Services.Notifier.Information(T("FA ERROR {0} - {1}", e.Message, e.Source));
                }

                #endregion
            }
        }


        /// <summary>
        ///     su dung cho duyet tin se dua thong tin bds nay len facebook cua nguoi dang
        /// </summary>
        /// <param name="linkdetail"> link trang chi tiết bđs</param>
        /// <param name="content">Nội dung: giống trang chi tiết bđs</param>
        /// <param name="linkimage">Link đại diện của bđs</param>
        /// <param name="userId">UserId của tin đó</param>
        /// <param name="message"></param>
        /// <param name="displaytitle">Tiêu đề của tin đó</param>
        public void PostToYourFacebook(string linkdetail, string content, string message, string displaytitle,
            string linkimage, int userId)
        {
            long facebookId = GetFaceBookUserId(userId);
            if (facebookId != -1)
            {
                #region get pagecontent

                string pageContent = content.StripHtml();

                #endregion

                #region postwall to facebook v1

                dynamic messagePost = new ExpandoObject();
                messagePost.picture = !string.IsNullOrEmpty(linkimage)
                    ? "http://dinhgianhadat.vn" + linkimage
                    : "http://dinhgianhadat.vn/Themes/TheRealEstate/Styles/images/dinhgianhadat-vinarev.jpg";
                // cần thay thế lại
                messagePost.link = "http://dinhgianhadat.vn" + linkdetail;
                messagePost.name = displaytitle.StripHtml();

                messagePost.caption = "Công ty Cổ Phần Định Giá BĐS trực tuyến Việt Nam";
                messagePost.description = pageContent;
                messagePost.message = message.StripHtml();

                string acccessToken = FacebookGetAccesstoken();

                var appp = new FacebookClient(acccessToken);
                try
                {
                    JsonObject postId = appp.Post("/" + facebookId + "/feed", messagePost);
                    //Services.Notifier.Information(T("FA ID {0} - FacebookId: {1}", postId["id"]));
                }
                catch (Exception)
                {
                    //Services.Notifier.Information(T("FA ERROR {0} - {1} - {2}", e.Message, e.Source, FacebookId));
                }

                #endregion
            }
        }

        #endregion
    }
}