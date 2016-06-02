using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ILocationDistrictService : IDependency
    {
        bool VerifyDistrictUnicity(string districtName);
        bool VerifyDistrictUnicity(int id, string districtName);
    }

    public class LocationDistrictService : ILocationDistrictService
    {
        private readonly IContentManager _contentManager;

        public LocationDistrictService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyDistrictUnicity(string districtName)
        {
            return
                !_contentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>()
                    .Where(a => a.Name == districtName)
                    .List()
                    .Any();
        }

        public bool VerifyDistrictUnicity(int id, string districtName)
        {
            return
                !_contentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>()
                    .Where(a => a.Name == districtName)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}