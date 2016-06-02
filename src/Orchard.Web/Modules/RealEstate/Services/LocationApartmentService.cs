using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.ViewModels;
using System.Web.Mvc;

namespace RealEstate.Services
{
    public interface ILocationApartmentService : IDependency
    {
        bool VerifyApartmentUnicity(string apartmentName, string apartmentBlock, int districtId);
        bool VerifyApartmentUnicity(int id, string apartmentName, string apartmentBlock, int districtId);

        bool VerifyApartmentBlockUnicity(string blockName, int apartmentId);
        bool VerifyApartmentBlockUnicity(int id, string blockName, int apartmentId);

        bool VerifyApartmentBlockShortNameUnicity(string shortName, int apartmentId);
        bool VerifyApartmentBlockShortNameUnicity(int id, string shortName, int apartmentId);


        LocationApartmentPart LocationApartmentPart(int id);
        IEnumerable<LocationApartmentBlockPart> LocationApartmentBlockParts(int? apartmentId);
        void ClearCacheApartmentBlocks();
        
        LocationApartmentBlockPart LocationApartmentBlockPart(int id);
        List<int> CheckIsDeleteApartmentBlock(int id);

        void UpdateApartmentBlock(int blockId, int floorTotal, int roomTotal);
        List<SelectListItem> GetListApartmentBlockInfo(int blockId);

    }

    public class LocationApartmentService : ILocationApartmentService
    {
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly ISignals _signals;

        public LocationApartmentService(IContentManager contentManager, IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IOrchardServices services)
        {
            _contentManager = contentManager;
            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;

            Logger = NullLogger.Instance;

            Services = services;
            T = NullLocalizer.Instance;
        }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours
        public ILogger Logger { get; set; }

        public bool VerifyApartmentUnicity(string apartmentName, string apartmentBlock, int districtId)
        {
            return
                !_contentManager.Query<LocationApartmentPart, LocationApartmentPartRecord>()
                    .Where(
                        apartment =>
                            apartment.Name == apartmentName && apartment.Block == apartmentBlock &&
                            apartment.District.Id == districtId)
                    .List()
                    .Any();
        }

        public bool VerifyApartmentUnicity(int id, string apartmentName, string apartmentBlock, int districtId)
        {
            return
                !_contentManager.Query<LocationApartmentPart, LocationApartmentPartRecord>()
                    .Where(
                        apartment =>
                            apartment.Name == apartmentName && apartment.Block == apartmentBlock &&
                            apartment.District.Id == districtId)
                    .List()
                    .Any(apartment => apartment.Id != id);
        }

        public bool VerifyApartmentBlockUnicity(string blockName, int apartmentId)
        {
            return _contentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>()
                .Where(r => r.LocationApartment.Id == apartmentId && r.IsActive && r.BlockName == blockName)
                .Count() > 0;
        }

        public bool VerifyApartmentBlockUnicity(int id, string blockName, int apartmentId)
        {
            return _contentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>()
                .Where(r => r.Id != id && r.LocationApartment.Id == apartmentId && r.IsActive && r.BlockName == blockName)
                .Count() > 0;
        }

        public bool VerifyApartmentBlockShortNameUnicity(string shortName, int apartmentId)
        {
            return _contentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>()
                .Where(r => r.LocationApartment.Id == apartmentId && r.IsActive &&  r.ShortName == shortName)
                .Count() > 0;
        }

        public bool VerifyApartmentBlockShortNameUnicity(int id, string shortName, int apartmentId)
        {
            return _contentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>()
                .Where(r => r.Id != id && r.LocationApartment.Id == apartmentId && r.IsActive && r.ShortName == shortName)
                .Count() > 0;
        }


        public LocationApartmentPart LocationApartmentPart(int id)
        {
            return _contentManager.Get<LocationApartmentPart>(id);
        }

        public IEnumerable<LocationApartmentBlockPart> LocationApartmentBlockParts(int? apartmentId)
        {
            return LocationApartmentBlockPartsCache().Where(r => r.LocationApartment.Id == apartmentId);
        }

        public IEnumerable<LocationApartmentBlockPart> LocationApartmentBlockPartsCache()
        {
            return _cacheManager.Get("ApartmentBlocks", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("ApartmentBlocks_Changed"));

                return _contentManager.Query<LocationApartmentBlockPart, LocationApartmentBlockPartRecord>().Where(r=>r.IsActive)
                        .List();
            });
        }
        public void ClearCacheApartmentBlocks()
        {
            _signals.Trigger("ApartmentBlocks_Changed");
        }

        public LocationApartmentBlockPart LocationApartmentBlockPart(int id)
        {
            return _contentManager.Get<LocationApartmentBlockPart>(id);
        }

        public List<int> CheckIsDeleteApartmentBlock(int id)
        {
            var lstproperty =
                _contentManager.Query<PropertyPart, PropertyPartRecord>()
                    .Where(r => r.ApartmentBlock != null && r.ApartmentBlock.Id == id);
            if (lstproperty == null) return null;

            return lstproperty.List().Select(r => r.Id).ToList();
        }

        public void UpdateApartmentBlock(int blockId, int floorTotal, int roomTotal)
        {
            var blockPart = _contentManager.Get<LocationApartmentBlockPart>(blockId);
            if (blockPart == null) return;

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (blockPart.FloorTotal != floorTotal)
                blockPart.FloorTotal = floorTotal;

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (blockPart.ApartmentEachFloor != roomTotal)
                blockPart.ApartmentEachFloor = roomTotal;
        }

        public List<SelectListItem> GetListApartmentBlockInfo(int blockId)
        {
            var lst = _contentManager.Query<ApartmentBlockInfoPart, ApartmentBlockInfoPartRecord>()
                .Where(r => r.ApartmentBlock.Id == blockId && r.IsActive)
                .List().Select(r => new SelectListItem
                {
                    Text = r.ApartmentName,
                    Value = r.Id.ToString()
                }).ToList();
            return lst;
        }

    }
}