using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CoefficientAlleyDistancePartHandler : ContentHandler
    {
        public CoefficientAlleyDistancePartHandler(IRepository<CoefficientAlleyDistancePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
