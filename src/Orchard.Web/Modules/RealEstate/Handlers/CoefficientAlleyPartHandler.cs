using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CoefficientAlleyPartHandler : ContentHandler
    {
        public CoefficientAlleyPartHandler(IRepository<CoefficientAlleyPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
