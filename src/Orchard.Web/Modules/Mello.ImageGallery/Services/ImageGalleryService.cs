using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using ICSharpCode.SharpZipLib.Zip;

using Mello.ImageGallery.Models;

using Orchard;
using Orchard.Data;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;
using Orchard.Validation;
using Orchard.UI.Notify;
using RealEstate.Services;


namespace Mello.ImageGallery.Services
{
	public class ImageGalleryService : IImageGalleryService
	{
		private const string ImageGalleriesMediaFolder = "ImageGalleries";
		private const int ThumbnailDefaultSize = 100;
		private const bool DefaultKeepAspectRatio = true;
		private const bool DefaultExpandToFill = false;

		private readonly IMediaLibraryService _mediaService;
		private readonly IThumbnailService _thumbnailService;
		private readonly IRepository<ImageGallerySettingsRecord> _repository;
		private readonly IRepository<ImageGalleryImageSettingsRecord> _imageRepository;
		private readonly IRepository<ImageGalleryRecord> _imageGalleryPartRepository;
		private readonly IOrchardServices _services;
		private readonly IStorageProvider _storageProvider;
        private readonly IHostNameService _hostNameService;

        private readonly IList<string> _imageFileFormats = new[] { "BMP", "GIF", "EXIF", "JPG", "PNG", "TIFF", "SWF" };
        private readonly IList<string> _fileFormats = new[] { "BMP", "GIF", "EXIF", "JPG", "PNG", "TIFF", "ZIP", "SWF" }; 

		//TODO: Remove Image repository as soon as it can cascade the saving
		public ImageGalleryService(IMediaLibraryService mediaService, IRepository<ImageGallerySettingsRecord> repository,
								   IRepository<ImageGalleryImageSettingsRecord> imageRepository, IThumbnailService thumbnailService,
								   IRepository<ImageGalleryRecord> imageGalleryPartRepository, IOrchardServices services,
                                   IStorageProvider storageProvider, IHostNameService hostNameService)
		{
			_storageProvider = storageProvider;
			_services = services;
			_imageGalleryPartRepository = imageGalleryPartRepository;
			_repository = repository;
			_mediaService = mediaService;
			_imageRepository = imageRepository;
			_thumbnailService = thumbnailService;
            _hostNameService = hostNameService;

			if (_mediaService.GetMediaFolders(string.Empty).All(o => o.Name != ImageGalleriesMediaFolder))
			{
				_mediaService.CreateFolder(string.Empty, ImageGalleriesMediaFolder);
			}
		}

		public IEnumerable<string> AllowedFileFormats
		{
			get { return _fileFormats; }
		}

		public IEnumerable<Models.ImageGallery> GetImageGalleries()
		{
            string hostname = _hostNameService.GetHostNameSite();
            if (_hostNameService.GetHostNamePartByClass("host-name-main") != null && _hostNameService.GetHostNamePartByClass("host-name-main").Name == hostname)
			    return _mediaService.GetMediaFolders(ImageGalleriesMediaFolder).Select(CreateImageGalleryFromMediaFolder);
            else
            {
                string folder = hostname + "/" + ImageGalleriesMediaFolder;
                return _mediaService.GetMediaFolders(folder).Select(CreateImageGalleryFromMediaFolder);
            }
		}

		public void CreateImageGallery(string name)
		{
            string hostname = _hostNameService.GetHostNameSite();
            if(_hostNameService.GetHostNamePartByClass("host-name-main") != null && _hostNameService.GetHostNamePartByClass("host-name-main").Name == hostname)
			    _mediaService.CreateFolder(ImageGalleriesMediaFolder, name);
            else
            {
                string folder = hostname + "/" + ImageGalleriesMediaFolder;
                _mediaService.CreateFolder(folder, name);
            }
		}

		public void DeleteImageGallery(string name)
		{
			var gallerySettings = GetImageGallerySettings(GetMediaPath(name));

			foreach (ImageGalleryImage image in GetImageGallery(name).Images)
			{
				DeleteImage(name, image.Name, GetImageSettings(gallerySettings, image.Name));
			}

			if (gallerySettings != null)
				_repository.Delete(gallerySettings);
			_mediaService.DeleteFolder(GetMediaPath(name));
		}

		public void RenameImageGallery(string imageGalleryName, string newName)
		{
			string mediaPath = GetMediaPath(imageGalleryName);
			_mediaService.RenameFolder(mediaPath, newName);

			ImageGallerySettingsRecord settings = GetImageGallerySettings(imageGalleryName);
			if (settings != null)
			{
				settings.ImageGalleryName = newName;
				_repository.Update(settings);
			}

			IEnumerable<ImageGalleryRecord> records = _imageGalleryPartRepository.Fetch(partRecord => partRecord.ImageGalleryName == imageGalleryName);

			foreach (ImageGalleryRecord imageGalleryRecord in records)
			{
				imageGalleryRecord.ImageGalleryName = newName;
				_imageGalleryPartRepository.Update(imageGalleryRecord);
			}
		}

		public void UpdateImageGalleryProperties(string imageGalleryName, int thumbnailHeight, int thumbnailWidth, bool keepAspectRatio, bool expandToFill)
		{
			var imageGallery = GetImageGallery(imageGalleryName);

			if (imageGallery != null) {
				var imageGallerySettings = GetImageGallerySettings(imageGallery.MediaPath);

				if (imageGallerySettings == null) {
					CreateImageGallerySettings(imageGallery.MediaPath, thumbnailHeight, thumbnailWidth, keepAspectRatio, expandToFill);
				}
				else {
					imageGallerySettings.ThumbnailHeight = thumbnailHeight;
					imageGallerySettings.ThumbnailWidth = thumbnailWidth;
					imageGallerySettings.KeepAspectRatio = keepAspectRatio;
					imageGallerySettings.ExpandToFill = expandToFill;

					_repository.Update(imageGallerySettings);
				}
			}
		}

		public ImageGalleryImage GetImage(string imageGalleryName, string imageName)
		{
			string imageGalleryMediaPath = GetMediaPath(imageGalleryName);
			ImageGallerySettingsRecord imageGallerySettings = GetImageGallerySettings(imageGalleryMediaPath);

			MediaFile file = _mediaService.GetMediaFiles(imageGalleryMediaPath)
				.SingleOrDefault(mediaFile => mediaFile.Name == imageName);

			if (file == null)
			{
				return null;
			}

			return CreateImageFromMediaFile(file, imageGallerySettings);
		}

		public Models.ImageGallery GetImageGallery(string imageGalleryName)
		{
			if (imageGalleryName.Contains("\\") || imageGalleryName.Contains("/"))
				imageGalleryName = GetName(imageGalleryName);

            MediaFolder mediaFolder;

            string hostname = _hostNameService.GetHostNameSite();
            if (_hostNameService.GetHostNamePartByClass("host-name-main") != null && _hostNameService.GetHostNamePartByClass("host-name-main").Name == hostname)
                mediaFolder = _mediaService.GetMediaFolders(ImageGalleriesMediaFolder).SingleOrDefault(m => m.Name == imageGalleryName);
            else
            {
                string folder = hostname + "/" + ImageGalleriesMediaFolder;
                mediaFolder = _mediaService.GetMediaFolders(folder).SingleOrDefault(m => m.Name == imageGalleryName);
            }

			if (mediaFolder != null)
			{
				return CreateImageGalleryFromMediaFolder(mediaFolder);
			}
			return null;
		}

		public void AddImage(string imageGalleryName, HttpPostedFileBase imageFile)
		{
			AddImage(imageGalleryName, imageFile.FileName, imageFile.InputStream);
		}

		public void AddImage(string imageGalleryName, string fileName, Stream imageFile)
		{
			if (!IsFileAllowed(fileName, true))
			{
				throw new InvalidOperationException(string.Format("{0} is not a valid file.", fileName));
			}

			// Zip file processing is different from Media module since we want the folders structure to be flattened
			if (IsZipFile(Path.GetExtension(fileName)))
			{
				UnzipMediaFileArchive(imageGalleryName, imageFile);
			}
			else
			{
				_mediaService.UploadMediaFile(GetMediaPath(imageGalleryName), fileName, imageFile);
			}
            /*add*/
            UpdateImageProperties(imageGalleryName, fileName,"dinh gia nha dat","","",null,null,false,false,true);
		}

        public void UpdateImageProperties(string imageGalleryName, string imageName, string imageTitle, string imageCaption/*added*/, string href, string DateBegin, string DateEnd, bool Enable, bool Blank, bool IsPublish)
		{
            UpdateImageProperties(imageGalleryName, imageName, imageTitle, imageCaption, null/*added*/, href, DateBegin, DateEnd, Enable, Blank,IsPublish);
		}

        private void UpdateImageProperties(string imageGalleryName, string imageName, string imageTitle, string imageCaption, int? position/*added*/, string href, string DateBegin, string DateEnd, bool Enable, bool Blank, bool IsPublish)
        {
            #region add

            DateTime? dateBegin = DateTime.Now;
            DateTime? dateEnd = DateTime.Now;
            if (Enable)
            {
                IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);
                dateBegin = DateTime.Parse(DateBegin, culture, System.Globalization.DateTimeStyles.AssumeLocal);

                IFormatProvider culture1 = new System.Globalization.CultureInfo("fr-FR", true);
                dateEnd = DateTime.Parse(DateEnd, culture1, System.Globalization.DateTimeStyles.AssumeLocal);
            }

            #endregion

            var image = GetImage(imageGalleryName, imageName);
			var imageGallery = GetImageGallery(imageGalleryName);

			var imageGallerySettings = GetImageGallerySettings(imageGallery.MediaPath);

			if (imageGallerySettings.ImageSettings.Any(o => o.Name == image.Name))
			{
				var imageSetting = imageGallerySettings.ImageSettings.Single(o => o.Name == image.Name);
				imageSetting.Caption = imageCaption;
				imageSetting.Title = imageTitle;
                imageSetting.href = href;/*added*/
                imageSetting.DateBegin = dateBegin;
                imageSetting.DateEnd = dateEnd;
                imageSetting.Enable = Enable;
			    imageSetting.IsPublish = IsPublish;
                imageSetting.Blank = Blank;/*added*/

				if (position.HasValue)
					imageSetting.Position = position.Value;
				_imageRepository.Update(imageSetting); // TODO: Remove when cascade is fixed
			}
			else
			{
                var imageSetting = new ImageGalleryImageSettingsRecord { Caption = imageCaption, Name = image.Name, Title = imageTitle, /*added*/href = href, DateBegin = dateBegin, DateEnd = dateEnd, Enable = Enable, Blank = Blank, IsPublish = IsPublish};
				if (position.HasValue)
					imageSetting.Position = position.Value;
				imageGallerySettings.ImageSettings.Add(imageSetting);
				_imageRepository.Create(imageSetting); // TODO: Remove when cascade is fixed
			}

			// TODO: See how to cascade changes          
			_repository.Update(imageGallerySettings);
		}

        public void UpdateIsPublishFile(string imageGalleryName)/*Add*/
	    {
            var imageGallery = GetImageGallery(imageGalleryName);

            var imageGallerySettings = GetImageGallerySettings(imageGallery.MediaPath);

            foreach (var item in imageGallerySettings.ImageSettings)
            {
                item.IsPublish = false;
            }
	    }
        // Update Flash
        public void UpdateFlashProperties(string imageGalleryName, string imageName, string imageTitle, string imageCaption, int? position/*added*/, string href, string DateBegin, string DateEnd, bool Enable, bool Blank, bool IsPublish)
        {
            IFormatProvider culture = new System.Globalization.CultureInfo("fr-FR", true);
            DateTime _DateBegin = DateTime.Parse(DateBegin, culture, System.Globalization.DateTimeStyles.AssumeLocal);

            IFormatProvider culture1 = new System.Globalization.CultureInfo("fr-FR", true);
            DateTime _DateEnd = DateTime.Parse(DateEnd, culture1, System.Globalization.DateTimeStyles.AssumeLocal);

            //var imageGallery = GetImageGallery(imageGalleryName);

            //var imageGallerySettings = GetImageGallerySettings(imageGallery.MediaPath);

            var imageSetting = new ImageGalleryImageSettingsRecord { Caption = imageCaption, Name = imageName, Title = imageTitle, /*added*/href = href, DateBegin = _DateBegin, DateEnd = _DateEnd, Enable = Enable, Blank = Blank , IsPublish = IsPublish};
            if (position.HasValue)
                imageSetting.Position = position.Value;
            //imageGallerySettings.ImageSettings.Add(imageSetting);
            _imageRepository.Create(imageSetting); // TODO: Remove when cascade is fixed

            // TODO: See how to cascade changes          
            //_repository.Update(imageGallerySettings);
        }

		private ImageGallerySettingsRecord GetImageGallerySettings(string imageGalleryName)
		{
			if (imageGalleryName.Contains("\\") || imageGalleryName.Contains("/"))
				imageGalleryName = GetName(imageGalleryName);
			return _repository.Get(o => o.ImageGalleryName == imageGalleryName);
		}

		private ImageGalleryImageSettingsRecord GetImageSettings(ImageGallerySettingsRecord imageGallerySettings, string imageName)
		{
			if (imageGallerySettings == null || imageGallerySettings.ImageSettings == null)
				return null;
			return imageGallerySettings.ImageSettings.SingleOrDefault(o => o.Name == imageName);
		}

		private ImageGalleryImageSettingsRecord GetImageSettings(string imageGalleryName, string imageName)
		{
			var imageGallerySettings = GetImageGallerySettings(GetMediaPath(imageGalleryName));

			return imageGallerySettings.ImageSettings.SingleOrDefault(o => o.Name == imageName);
		}

		private Models.ImageGallery CreateImageGalleryFromMediaFolder(MediaFolder mediaFolder)
		{
			var images = _mediaService.GetMediaFiles(mediaFolder.MediaPath);
			ImageGallerySettingsRecord imageGallerySettings = GetImageGallerySettings(GetName(mediaFolder.MediaPath)) ??
															  CreateImageGallerySettings(mediaFolder.MediaPath, ThumbnailDefaultSize,
																						 ThumbnailDefaultSize, DefaultKeepAspectRatio, DefaultExpandToFill);

			return new Models.ImageGallery
			{
				Id = imageGallerySettings.Id,
				LastUpdated = mediaFolder.LastUpdated,
				MediaPath = mediaFolder.MediaPath,
				Name = mediaFolder.Name,
				Size = mediaFolder.Size,
				User = mediaFolder.User,
				ThumbnailHeight = imageGallerySettings.ThumbnailHeight,
				ThumbnailWidth = imageGallerySettings.ThumbnailWidth,
				Images = images.Select(image => CreateImageFromMediaFile(image, imageGallerySettings)).OrderBy(image => image.Position),
				KeepAspectRatio = imageGallerySettings.KeepAspectRatio,
				ExpandToFill = imageGallerySettings.ExpandToFill
			};
		}

		private ImageGallerySettingsRecord CreateImageGallerySettings(
			string imageGalleryMediaPath, 
			int thumbnailHeight, 
			int thumbnailWidth,
			bool keepAspectRatio, 
			bool expandToFill)
		{
			ImageGallerySettingsRecord imageGallerySettings = new ImageGallerySettingsRecord
			{
				ImageGalleryName = GetName(imageGalleryMediaPath),
				ThumbnailHeight = thumbnailHeight,
				ThumbnailWidth = thumbnailWidth,
				KeepAspectRatio = keepAspectRatio,
				ExpandToFill = expandToFill
			};
			_repository.Create(imageGallerySettings);

			return imageGallerySettings;
		}

		private ImageGalleryImage CreateImageFromMediaFile(MediaFile mediaFile, ImageGallerySettingsRecord imageGallerySettings)
		{
			if (imageGallerySettings == null)
			{
				throw new ArgumentNullException("imageGallerySettings");
			}

			var imageSettings = GetImageSettings(imageGallerySettings, mediaFile.Name);
            Thumbnail thumbnail = null;
            if (mediaFile.Type != ".swf")//thanhtuank9
            {
                bool isValidThumbnailSize = imageGallerySettings.ThumbnailWidth > 0 &&
                  imageGallerySettings.ThumbnailHeight > 0;


                if (isValidThumbnailSize)
                {
                    thumbnail = _thumbnailService.GetThumbnail(_storageProvider.Combine(mediaFile.FolderName, mediaFile.Name),
                                                                  imageGallerySettings.ThumbnailWidth,
                                                                  imageGallerySettings.ThumbnailHeight,
                                                                  imageGallerySettings.KeepAspectRatio,
                                                                  imageGallerySettings.ExpandToFill);
                }
            }
            else
            {
                thumbnail = new Thumbnail();
                thumbnail.Width = imageGallerySettings.ThumbnailWidth;
                thumbnail.Height = imageGallerySettings.ThumbnailHeight;
            }
			
			return new ImageGalleryImage
			{
				PublicUrl = _mediaService.GetMediaPublicUrl(mediaFile.FolderName, mediaFile.Name),
				Name = mediaFile.Name,
				Size = mediaFile.Size,
				User = mediaFile.User,
				LastUpdated = mediaFile.LastUpdated,
				Caption = imageSettings == null ? string.Empty : imageSettings.Caption,
				Thumbnail = thumbnail,
                href = imageSettings == null ? null : imageSettings.href,/*added get data*/
                DateBegin = imageSettings == null ? null : Convert.ToDateTime(imageSettings.DateBegin).ToString("dd/MM/yyyy"),/*added*/
                DateEnd = imageSettings == null ? null : Convert.ToDateTime(imageSettings.DateEnd.ToString()).ToString("dd/MM/yyyy"),/*added*/
                Enable = imageSettings == null || imageSettings.Enable,/*added*/
                Blank = imageSettings == null || imageSettings.Blank,/*added*/
				Title = imageSettings == null ? null : imageSettings.Title,
                IsPublish = imageSettings == null || imageSettings.IsPublish,
				Position = imageSettings == null ? 0 : imageSettings.Position
			};
		}

		private string GetMediaPath(string imageGalleryName)
		{
            string hostname = _hostNameService.GetHostNameSite();
            if (_hostNameService.GetHostNamePartByClass("host-name-main") != null && _hostNameService.GetHostNamePartByClass("host-name-main").Name == hostname)
			    return _storageProvider.Combine(ImageGalleriesMediaFolder, imageGalleryName);
            else
            {
                string folder = hostname + "/" + ImageGalleriesMediaFolder;
                return _storageProvider.Combine(folder, imageGalleryName);
            }
		}

		private string GetName(string mediaPath)
		{
			return mediaPath.Split(new[] { '\\', '/' }).Last();
		}

		public void DeleteImage(string imageGalleryName, string imageName)
		{
			var imageSettings = GetImageSettings(imageGalleryName, imageName);
			DeleteImage(imageGalleryName, imageName, imageSettings);
		}

		public string GetPublicUrl(string path)
		{
			return _mediaService.GetMediaPublicUrl(Path.GetDirectoryName(path), Path.GetFileName(path));
		}

		public bool IsFileAllowed(string fileName, bool allowZip) {
			return (IsImageFile(fileName) || (allowZip &&
			                                  IsZipFile(Path.GetExtension(fileName))));
		}

		public bool IsFileAllowed(HttpPostedFileBase postedFile)
		{
			if (postedFile == null)
			{
				return false;
			}

			return IsFileAllowed(postedFile.FileName, true);
		}

		private bool IsImageFile(string fileName)
		{
			string extension = Path.GetExtension(fileName);
			if (extension == null)
				return false;
			extension = extension.TrimStart('.');

			return _imageFileFormats.Any(o => extension.Equals(o, StringComparison.OrdinalIgnoreCase));
		}

		private void DeleteImage(string imageGalleryName, string imageName, ImageGalleryImageSettingsRecord imageSettings)
		{
			if (imageSettings != null)
			{
				_imageRepository.Delete(imageSettings);
			}
			_mediaService.DeleteFile(GetMediaPath(imageGalleryName), imageName);
            _thumbnailService.DeleteThumbnail(_storageProvider.Combine(GetMediaPath(imageGalleryName), imageName));
		}


		public void ReorderImages(string imageGalleryName, IEnumerable<string> images)
		{
			Models.ImageGallery imageGallery = GetImageGallery(imageGalleryName);
			int position = 0;
			var trimedImages = images.Select(x => x.Trim());

			foreach (string image in trimedImages)
			{
				ImageGalleryImage imageGalleryImage = imageGallery.Images.Single(o => o.Name == image);
				imageGalleryImage.Position = position++;
                UpdateImageProperties(imageGalleryName, imageGalleryImage.Name, imageGalleryImage.Title, imageGalleryImage.Caption,
                                      imageGalleryImage.Position, imageGalleryImage.href, imageGalleryImage.DateBegin, imageGalleryImage.DateEnd, imageGalleryImage.Enable, imageGalleryImage.Blank, imageGalleryImage.IsPublish);
			}

			foreach (ImageGalleryImage imageGalleryImage in imageGallery.Images.Where(o => !images.Contains(o.Name)))
			{
				imageGalleryImage.Position = position++;
                UpdateImageProperties(imageGalleryName, imageGalleryImage.Name, imageGalleryImage.Title, imageGalleryImage.Caption,
                                      imageGalleryImage.Position, imageGalleryImage.href, imageGalleryImage.DateBegin, imageGalleryImage.DateEnd, imageGalleryImage.Enable, imageGalleryImage.Blank, imageGalleryImage.IsPublish);
			}
		}

		// TODO: Submit a path to Media module to make this method public?
		/// <summary>
		/// Determines if a file is a Zip Archive based on its extension.
		/// </summary>
		/// <param name="extension">The extension of the file to analyze.</param>
		/// <returns>True if the file is a Zip archive; false otherwise.</returns>
		private static bool IsZipFile(string extension)
		{
			return string.Equals(extension.TrimStart('.'), "zip", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Unzips a media archive file flattening the folder structure.
		/// </summary>
		/// <param name="imageGallery">Image gallery name.</param>
		/// <param name="zipStream">The archive file stream.</param>
		protected void UnzipMediaFileArchive(string imageGallery, Stream zipStream)
		{
			Argument.ThrowIfNullOrEmpty(imageGallery, "imageGallery");
			Argument.ThrowIfNull(zipStream, "zipStream");

			using (ZipInputStream fileInflater = new ZipInputStream(zipStream))
			{
				ZipEntry entry;

				while ((entry = fileInflater.GetNextEntry()) != null)
				{
					if (!entry.IsDirectory && !string.IsNullOrEmpty(entry.Name))
					{
						// Handle Mac OS X meta files
						if (entry.Name.StartsWith("__MACOSX", StringComparison.OrdinalIgnoreCase)) continue;
						// Skip disallowed files
						if (IsFileAllowed(entry.Name, false))
						{
							string fileName = Path.GetFileName(entry.Name);

							try
							{
								AddImage(imageGallery, fileName, fileInflater);
							}
							catch (ArgumentException argumentException)
							{
								if (argumentException.Message.Contains(fileName))
								{
									_services.Notifier.Warning(new LocalizedString(string.Format("File \"{0}\" skipped since it already exists.", fileName)));
								}
								else
								{
									throw;
								}
							}
						}
					}
				}
			}
		}
	}
}