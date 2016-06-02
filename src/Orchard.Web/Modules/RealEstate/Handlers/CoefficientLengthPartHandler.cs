using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CoefficientLengthPartHandler : ContentHandler
    {
        public CoefficientLengthPartHandler(IRepository<CoefficientLengthPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
