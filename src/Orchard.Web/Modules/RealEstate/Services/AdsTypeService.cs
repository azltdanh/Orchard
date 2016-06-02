using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IAdsTypeService : IDependency
    {
        bool VerifyAdsTypeUnicity(string adsTypeName);
        bool VerifyAdsTypeUnicity(int id, string adsTypeName);
    }

    public class AdsTypeService : IAdsTypeService
    {
        private readonly IContentManager _contentManager;

        public AdsTypeService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyAdsTypeUnicity(string adsTypeName)
        {
            return
                !_contentManager.Query<AdsTypePart, AdsTypePartRecord>()
                    .Where(adsType => adsType.Name == adsTypeName)
                    .List()
                    .Any();
        }

        public bool VerifyAdsTypeUnicity(int id, string adsTypeName)
        {
            return
                !_contentManager.Query<AdsTypePart, AdsTypePartRecord>()
                    .Where(adsType => adsType.Name == adsTypeName)
                    .List()
                    .Any(adsType => adsType.Id != id);
        }
    }
}