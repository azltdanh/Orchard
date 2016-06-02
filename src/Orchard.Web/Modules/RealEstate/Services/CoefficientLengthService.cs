using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface ICoefficientLengthService : IDependency
    {
        bool VerifyCoefficientLengthUnicity(double widthRange);
        bool VerifyCoefficientLengthUnicity(int id, double widthRange);
    }

    public class CoefficientLengthService : ICoefficientLengthService
    {
        private readonly IContentManager _contentManager;

        public CoefficientLengthService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyCoefficientLengthUnicity(double widthRange)
        {
            return
                !_contentManager.Query<CoefficientLengthPart, CoefficientLengthPartRecord>()
                    .Where(a => a.WidthRange == widthRange)
                    .List()
                    .Any();
        }

        public bool VerifyCoefficientLengthUnicity(int id, double widthRange)
        {
            return
                !_contentManager.Query<CoefficientLengthPart, CoefficientLengthPartRecord>()
                    .Where(a => a.WidthRange == widthRange)
                    .List()
                    .Any(a => a.Id != id);
        }
    }
}