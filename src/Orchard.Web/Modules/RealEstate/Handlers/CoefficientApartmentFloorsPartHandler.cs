using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CoefficientApartmentFloorsPartHandler : ContentHandler
    {
        public CoefficientApartmentFloorsPartHandler(IRepository<CoefficientApartmentFloorsPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
