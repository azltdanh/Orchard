using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICoefficientApartmentFloorThService : IDependency
    {
        bool VerifyCoefficientApartmentFloorThUnicity(double? maxFloors, double apartmentFloorTh);
        bool VerifyCoefficientApartmentFloorThUnicity(int id, double? maxFloors, double apartmentFloorTh);
    }

    public class CoefficientApartmentFloorThService : ICoefficientApartmentFloorThService
    {
        private readonly IContentManager _contentManager;

        public CoefficientApartmentFloorThService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCoefficientApartmentFloorThUnicity(double? maxFloors, double apartmentFloorTh)
        {
            return
                !_contentManager.Query<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>()
                    .Where(a => a.MaxFloors == maxFloors && a.ApartmentFloorTh == apartmentFloorTh)
                    .List()
                    .Any();
        }

        public bool VerifyCoefficientApartmentFloorThUnicity(int id, double? maxFloors, double apartmentFloorTh)
        {
            return
                !_contentManager.Query<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>()
                    .Where(a => a.MaxFloors == maxFloors && a.ApartmentFloorTh == apartmentFloorTh)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}