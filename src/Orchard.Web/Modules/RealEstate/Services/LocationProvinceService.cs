using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ILocationProvinceService : IDependency
    {
        bool VerifyProvinceUnicity(string provinceName);
        bool VerifyProvinceUnicity(int id, string provinceName);
    }

    public class LocationProvinceService : ILocationProvinceService
    {
        private readonly IContentManager _contentManager;

        public LocationProvinceService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyProvinceUnicity(string provinceName)
        {
            return
                !_contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                    .Where(province => province.Name == provinceName)
                    .List()
                    .Any();
        }

        public bool VerifyProvinceUnicity(int id, string provinceName)
        {
            return
                !_contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                    .Where(province => province.Name == provinceName)
                    .List()
                    .Any(province => province.Id != id);
        }
    }
}