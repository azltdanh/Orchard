using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using Orchard;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Services;
using Orchard.Messaging.Services;
using Orchard.Environment.Configuration;
using RealEstate.Models;
using RealEstate.ViewModels;

namespace RealEstate.Services
{
    public interface ILocationStreetSegmentService : IDependency
    {
        bool VerifyStreetSegmentUnicity(int streetId, int fromNumber, int toNumber);
        bool VerifyStreetSegmentUnicity(int id, int streetId, int fromNumber, int toNumber);
    }

    public class LocationStreetSegmentService : ILocationStreetSegmentService 
    {
        
        private readonly IContentManager _contentManager;

        public LocationStreetSegmentService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyStreetSegmentUnicity(int streetId, int fromNumber, int toNumber)
        {
            return !_contentManager.Query<LocationStreetSegmentPart, LocationStreetSegmentPartRecord>()
                .Where(s => s.Street.Id == streetId && ((s.FromNumber < fromNumber && s.ToNumber > fromNumber) || (s.FromNumber < toNumber && s.ToNumber > toNumber))).List()
                .Where(s => (s.FromNumber + fromNumber) % 2 == 0).Any();
        }

        public bool VerifyStreetSegmentUnicity(int id, int streetId, int fromNumber, int toNumber)
        {
            return !_contentManager.Query<LocationStreetSegmentPart, LocationStreetSegmentPartRecord>()
                .Where(s => s.Street.Id == streetId && ((s.FromNumber < fromNumber && s.ToNumber > fromNumber) || (s.FromNumber < toNumber && s.ToNumber > toNumber))).List()
                .Where(s => (s.FromNumber + fromNumber) % 2 == 0).Any(street => street.Id != id);
        }

    }
}
