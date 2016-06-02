using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICoefficientAlleyService : IDependency
    {
        bool VerifyCoefficientAlleyUnicity(double streetUnitPrice);
        bool VerifyCoefficientAlleyUnicity(int id, double streetUnitPrice);
    }

    public class CoefficientAlleyService : ICoefficientAlleyService
    {
        private readonly IContentManager _contentManager;

        public CoefficientAlleyService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCoefficientAlleyUnicity(double streetUnitPrice)
        {
            return
                !_contentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                    .Where(a => a.StreetUnitPrice == streetUnitPrice)
                    .List()
                    .Any();
        }

        public bool VerifyCoefficientAlleyUnicity(int id, double streetUnitPrice)
        {
            return
                !_contentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                    .Where(a => a.StreetUnitPrice == streetUnitPrice)
                    .List()
                    .Any(direction => direction.Id != id);
        }
    }
}