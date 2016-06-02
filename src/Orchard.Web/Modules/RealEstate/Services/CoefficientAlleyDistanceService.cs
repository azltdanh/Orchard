using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICoefficientAlleyDistanceService : IDependency
    {
        bool VerifyCoefficientAlleyDistanceUnicity(double lastAlleyWidth);
        bool VerifyCoefficientAlleyDistanceUnicity(int id, double lastAlleyWidth);
    }

    public class CoefficientAlleyDistanceService : ICoefficientAlleyDistanceService
    {
        private readonly IContentManager _contentManager;

        public CoefficientAlleyDistanceService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCoefficientAlleyDistanceUnicity(double lastAlleyWidth)
        {
            return
                !_contentManager.Query<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord>()
                    .Where(a => a.LastAlleyWidth == lastAlleyWidth)
                    .List()
                    .Any();
        }

        public bool VerifyCoefficientAlleyDistanceUnicity(int id, double lastAlleyWidth)
        {
            return
                !_contentManager.Query<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord>()
                    .Where(a => a.LastAlleyWidth == lastAlleyWidth)
                    .List()
                    .Any(direction => direction.Id != id);
        }
    }
}