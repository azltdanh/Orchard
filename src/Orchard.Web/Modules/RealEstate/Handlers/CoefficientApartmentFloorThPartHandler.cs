using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CoefficientApartmentFloorThPartHandler : ContentHandler
    {
        public CoefficientApartmentFloorThPartHandler(IRepository<CoefficientApartmentFloorThPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
