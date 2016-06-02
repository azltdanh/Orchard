using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Logging;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IDirectionService : IDependency
    {
        bool VerifyDirectionUnicity(string directionName);
        bool VerifyDirectionUnicity(int id, string directionName);
    }

    public class DirectionService : IDirectionService
    {
        private readonly IContentManager _contentManager;

        public DirectionService(IContentManager contentManager)
        {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public bool VerifyDirectionUnicity(string directionName)
        {
            return
                !_contentManager.Query<DirectionPart, DirectionPartRecord>()
                    .Where(r => r.Name == directionName)
                    .List()
                    .Any();
        }

        public bool VerifyDirectionUnicity(int id, string directionName)
        {
            return
                !_contentManager.Query<DirectionPart, DirectionPartRecord>()
                    .Where(r => r.Name == directionName)
                    .List()
                    .Any(r => r.Id != id);
        }
    }
}