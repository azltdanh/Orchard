using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationApartmentPartHandler : ContentHandler
    {
        public LocationApartmentPartHandler(IRepository<LocationApartmentPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
