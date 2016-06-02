using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ILocationWardService : IDependency
    {
        bool VerifyWardUnicity(string wardName, int districtId);
        bool VerifyWardUnicity(int id, string wardName, int districtId);
    }

    public class LocationWardService : ILocationWardService
    {
        private readonly IContentManager _contentManager;

        public LocationWardService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyWardUnicity(string wardName, int districtId)
        {
            return
                !_contentManager.Query<LocationWardPart, LocationWardPartRecord>()
                    .Where(ward => ward.Name == wardName && ward.District.Id == districtId)
                    .List()
                    .Any();
        }

        public bool VerifyWardUnicity(int id, string wardName, int districtId)
        {
            return
                !_contentManager.Query<LocationWardPart, LocationWardPartRecord>()
                    .Where(ward => ward.Name == wardName && ward.District.Id == districtId)
                    .List()
                    .Any(ward => ward.Id != id);
        }
    }
}