using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ILocationStreetService : IDependency
    {
        bool VerifyStreetUnicity(string streetName, int districtId);
        bool VerifyStreetUnicity(int id, string streetName, int districtId);
        bool VerifyStreetSegmentUnicity(int streetId, int fromNumber, int toNumber, int districtId);
        bool VerifyStreetSegmentUnicity(int id, int streetId, int fromNumber, int toNumber, int districtId);
    }

    public class LocationStreetService : ILocationStreetService
    {
        private readonly IContentManager _contentManager;

        public LocationStreetService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyStreetUnicity(string streetName, int districtId)
        {
            return
                !_contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                    .Where(street => street.Name == streetName && street.District.Id == districtId)
                    .List()
                    .Any();
        }

        public bool VerifyStreetUnicity(int id, string streetName, int districtId)
        {
            return
                !_contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                    .Where(street => street.Name == streetName && street.District.Id == districtId)
                    .List()
                    .Any(street => street.Id != id);
        }

        public bool VerifyStreetSegmentUnicity(int streetId, int fromNumber, int toNumber, int districtId)
        {
            return !_contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                .Where(s => s.District.Id == districtId && s.RelatedStreet != null && s.RelatedStreet.Id == streetId
                            &&
                            ((s.FromNumber <= fromNumber && s.ToNumber >= fromNumber) ||
                             (s.FromNumber <= toNumber && s.ToNumber >= toNumber))).List()
                .Where(s => (s.FromNumber + fromNumber)%2 == 0).Any();
        }

        public bool VerifyStreetSegmentUnicity(int id, int streetId, int fromNumber, int toNumber, int districtId)
        {
            return !_contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                .Where(s => s.District.Id == districtId && s.RelatedStreet != null && s.RelatedStreet.Id == streetId
                            &&
                            ((s.FromNumber <= fromNumber && s.ToNumber >= fromNumber) ||
                             (s.FromNumber <= toNumber && s.ToNumber >= toNumber))).List()
                .Where(s => (s.FromNumber + fromNumber)%2 == 0).Any(s => s.Id != id);
        }
    }
}