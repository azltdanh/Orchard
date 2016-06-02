using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICoefficientApartmentFloorsService : IDependency
    {
        bool VerifyCoefficientApartmentFloorsUnicity(double floors);
        bool VerifyCoefficientApartmentFloorsUnicity(int id, double floors);
    }

    public class CoefficientApartmentFloorsService : ICoefficientApartmentFloorsService
    {
        private readonly IContentManager _contentManager;

        public CoefficientApartmentFloorsService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCoefficientApartmentFloorsUnicity(double floors)
        {
            return
                !_contentManager.Query<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord>()
                    .Where(a => a.Floors == floors)
                    .List()
                    .Any();
        }

        public bool VerifyCoefficientApartmentFloorsUnicity(int id, double floors)
        {
            return
                !_contentManager.Query<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord>()
                    .Where(a => a.Floors == floors)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}