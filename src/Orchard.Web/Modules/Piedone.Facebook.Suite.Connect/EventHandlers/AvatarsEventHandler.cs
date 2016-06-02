using System.IO;
using System.Net;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Logging;
using Orchard.UI.Notify;
using Piedone.Facebook.Suite.Models;
using Orchard;
using Orchard.Localization;
using Contrib.OnlineUsers.Models;
using Orchard.FileSystems.Media;
using System;

namespace Piedone.Facebook.Suite.EventHandlers
{
    [OrchardFeature("Piedone.Facebook.Suite.Connect")]
    public class AvatarsEventHandler : IFacebookConnectEventHandler
    {
        private readonly IStorageProvider _storageProvider;

        public ILogger Logger { get; set; }

        public AvatarsEventHandler(IOrchardServices services, IStorageProvider storageProvider)
        {
            Services = services;
            _storageProvider = storageProvider; 
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public void UserUpdated(IFacebookUser facebookUser)
        {
            var part = facebookUser.As<FacebookUserPart>();

                using (var wc = new WebClient())
                {
                    try
                    {
                       // Services.Notifier.Information(T("Value: {0}", GetAvatarUrl(part.Id)));
                        if (!string.IsNullOrEmpty(GetAvatarUrl(part.Id))) return;

                        var stream = new MemoryStream(wc.DownloadData(part.GetPictureLink()));

                        //Services.Notifier.Warning(part.GetPictureLink().ToString());
                        SaveAvatarFile(part.Id, stream, "jpg"); // We could look at the bytes to detect the file type, but rather not
                    }
                    catch (WebException ex)
                    {
                        Logger.Error(ex, "Downloading of Facebok profile picture failed: " + ex.Message);
                    }
                    #region note by piedone
                    // Async versions throw exception regarding the transaction

                    //wc.DownloadDataCompleted += _taskFactory.BuildAsyncEventHandler<object, DownloadDataCompletedEventArgs>(
                    //    (sender, e) =>
                    //    {
                    //        if (e.Error == null)
                    //        {
                    //            var stream = new MemoryStream(e.Result);
                    //            _avatarsService.SaveAvatarFile(facebookUserPart.Id, stream, "jpg"); // We could look at the bytes to detect the file type, but rather not
                    //        }

                    //        else
                    //        {
                    //            string message = "Downloading of Facebok profile picture failed: " + e.Error.Message;
                    //            Logger.Error(e.Error, message);
                    //        }
                    //    }, false).Invoke;

                    //wc.DownloadDataCompleted +=
                    //    (sender, e) =>
                    //    {
                    //        if (e.Error == null)
                    //        {
                    //            var stream = new MemoryStream(e.Result);
                    //            _avatarsService.SaveAvatarFile(facebookUserPart.Id, stream, "jpg"); // We could look at the bytes to detect the file type, but rather not
                    //        }

                    //        else
                    //        {
                    //            string message = "Downloading of Facebok profile picture failed: " + e.Error.Message;
                    //            Logger.Error(e.Error, message);
                    //        }
                    //    };


                    //wc.DownloadDataAsync(new Uri(part.PictureLink));
                    #endregion
                }
        }

        public void DeleteAvatarFile(int id)
        {
            Services.ContentManager.Get<UserUpdateProfilePart>(id).Avatar = "";
            // Maybe to be used in the Handler in OnRemoved(). But since removing is not hard deleting, this isn't required yet.
            _storageProvider.DeleteFile(GetFilePath(id, Services.ContentManager.Get<UserUpdateProfilePart>(id).Avatar));
        }

        public string GetAvatarUrl(int id)
        {
            var extension = Services.ContentManager.Get<UserUpdateProfilePart>(id).Avatar;
            if (String.IsNullOrEmpty(extension)) return ""; // There is no avatar yet.

            return _storageProvider.GetPublicUrl(GetFilePath(id, extension));
        }

        private string GetFilePath(int id, string extension)
        {
            return "Media/Default/Avatars" + "/" + id + "." + extension;
        }

        private string StripExtension(string extension)
        {
            return extension.Trim().TrimStart('.').ToLowerInvariant();
        }

        public bool SaveAvatarFile(int id, Stream stream, string extension)
        {
            extension = StripExtension(extension);

            var filePath = GetFilePath(id, extension);

            try
            {
                _storageProvider.DeleteFile(filePath);
            }
            catch (Exception)
            {
            }

            _storageProvider.SaveStream(filePath, stream);

            var avatar = Services.ContentManager.Get<UserUpdateProfilePart>(id);
            avatar.Avatar = id + "." + extension;
            //_contentManager.Flush();

            return true;
        }
    }
}