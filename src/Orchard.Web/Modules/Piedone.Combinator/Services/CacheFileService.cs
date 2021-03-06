﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Mvc;
using Orchard.Services;
using Piedone.Combinator.EventHandlers;
using Piedone.Combinator.Extensions;
using Piedone.Combinator.Models;

namespace Piedone.Combinator.Services
{
    [OrchardFeature("Piedone.Combinator")]
    public class CacheFileService : ICacheFileService
    {
        private readonly IStorageProvider _storageProvider;
        private readonly IRepository<CombinedFileRecord> _fileRepository;
        private readonly ICombinatorResourceManager _combinatorResourceManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClock _clock;
        private readonly ICombinatorEventHandler _combinatorEventHandler;

        #region In-memory caching fields
        private readonly ICacheManager _cacheManager;
        private readonly ICombinatorEventMonitor _combinatorEventMonitor;
        private const string CachePrefix = "Piedone.Combinator.";
        #endregion

        #region Static file caching fields
        private const string _rootPath = "Modules/Piedone/Combinator/";
        private const string _stylesPath = _rootPath + "Styles/";
        private const string _scriptsPath = _rootPath + "Scripts/";
        #endregion


        public CacheFileService(
            IStorageProvider storageProvider,
            IRepository<CombinedFileRecord> fileRepository,
            ICombinatorResourceManager combinatorResourceManager,
            IHttpContextAccessor httpContextAccessor,
            IClock clock,
            ICombinatorEventHandler combinatorEventHandler,
            ICacheManager cacheManager,
            ICombinatorEventMonitor combinatorEventMonitor)
        {
            _storageProvider = storageProvider;
            _fileRepository = fileRepository;
            _combinatorResourceManager = combinatorResourceManager;
            _httpContextAccessor = httpContextAccessor;
            _clock = clock;
            _combinatorEventHandler = combinatorEventHandler;

            _cacheManager = cacheManager;
            _combinatorEventMonitor = combinatorEventMonitor;
        }


        public void Save(int hashCode, CombinatorResource resource)
        {
            var sliceCount = _fileRepository.Count(file => file.HashCode == hashCode);

            var fileRecord = new CombinedFileRecord()
            {
                HashCode = hashCode,
                Slice = ++sliceCount,
                Type = resource.Type,
                LastUpdatedUtc = _clock.UtcNow,
                Settings = _combinatorResourceManager.SerializeResourceSettings(resource)
            };

            if (!String.IsNullOrEmpty(resource.Content))
            {
                var path = MakePath(fileRecord);

                using (var stream = _storageProvider.CreateFile(path).OpenWrite())
                {
                    var bytes = Encoding.UTF8.GetBytes(resource.Content);
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            _fileRepository.Create(fileRecord);
        }

        public IList<CombinatorResource> GetCombinedResources(int hashCode)
        {
            return _cacheManager.Get(MakeCacheKey("GetCombinedResources." + hashCode.ToString()), ctx =>
            {
                _combinatorEventMonitor.MonitorCacheEmptied(ctx);

                var files = GetRecords(hashCode);
                var fileCount = files.Count;

                var resources = new List<CombinatorResource>(fileCount);

                foreach (var file in files)
                {
                    var resource = _combinatorResourceManager.ResourceFactory(file.Type);

                    resource.FillRequiredContext("CombinedResource" + file.Id.ToString(), _storageProvider.GetPublicUrl(MakePath(file)));
                    _combinatorResourceManager.DeserializeSettings(file.Settings, resource);

                    // This means the storage public url is not a local url (like it's with Azure blog storage)
                    if (!resource.IsOriginal && resource.IsCdnResource)
                        ResourceProcessingService.ConvertRelativeUrlsToAbsolute(resource, _httpContextAccessor.Current().Request.Url);

                    resource.LastUpdatedUtc = file.LastUpdatedUtc ?? _clock.UtcNow;
                    resources.Add(resource);
                }

                return resources;
            });
        }

        public bool Exists(int hashCode)
        {
            return _cacheManager.Get(MakeCacheKey("Exists." + hashCode.ToString()), ctx =>
            {
                _combinatorEventMonitor.MonitorCacheEmptied(ctx);
                // Maybe also check if the file exists?
                return _fileRepository.Count(file => file.HashCode == hashCode) != 0;
            });
        }

        public int GetCount()
        {
            return _fileRepository.Table.Count();
        }

        //public void Delete(int hashCode)
        //{
        //    DeleteFiles(GetRecords(hashCode));

        //    TriggerCacheChangedSignal(hashCode);
        //}

        public void Empty()
        {
            var files = _fileRepository.Table.ToList();
            DeleteFiles(files);

            // We don't check if there were any files in a DB here, we try to delete even if there weren't. This adds robustness: with emptying the cache
            // everything can be reset, even if the user or a deploy process manipulated the DB or the file system.
            // Also removing files and subfolders separately is necessary as just removing the root folder would yield a directory not empty exception.
            if (_storageProvider.FolderExists(_scriptsPath))
            {
                _storageProvider.DeleteFolder(_scriptsPath);
                Thread.Sleep(300); // This is to ensure we don't get an "access denied" when deleting the root folder 
            }

            if (_storageProvider.FolderExists(_stylesPath))
            {
                _storageProvider.DeleteFolder(_stylesPath);
                Thread.Sleep(300);
            }


            if (_storageProvider.FolderExists(_rootPath))
            {
                _storageProvider.DeleteFolder(_rootPath);
            }

            _combinatorEventHandler.CacheEmptied();
        }

        public void WriteSpriteStream(string fileName, SpriteStreamWriter streamWriter)
        {
            var path = _stylesPath + "Sprites/" + fileName;
            var spriteFile = _storageProvider.CreateFile(path);
            var publicUrl = _storageProvider.GetPublicUrl(path);
            using (var stream = spriteFile.OpenWrite())
            {
                streamWriter(stream, publicUrl);
            }
        }


        private List<CombinedFileRecord> GetRecords(int hashCode)
        {
            return _fileRepository.Fetch(file => file.HashCode == hashCode).ToList();
        }

        private void DeleteFiles(List<CombinedFileRecord> files)
        {
            foreach (var file in files)
            {
                _fileRepository.Delete(file);
                if (_storageProvider.FileExists(MakePath(file)))
                {
                    _storageProvider.DeleteFile(MakePath(file));
                }
            }
        }


        private static string MakePath(CombinedFileRecord file)
        {
            // Maybe others will come, therefore the architecture
            string extension = "";
            string folderPath = "";
            if (file.Type == ResourceType.JavaScript)
            {
                folderPath = _scriptsPath;
                extension = "js";
            }
            else if (file.Type == ResourceType.Style)
            {
                folderPath = _stylesPath;
                extension = "css";
            }

            return folderPath + file.GetFileName() + "." + extension;
        }

        private static string MakeCacheKey(string name)
        {
            return CachePrefix + name;
        }
    }
}