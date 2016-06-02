using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers 
{
    public class LocationApartmentBlockPartHandler : ContentHandler
    {
        public LocationApartmentBlockPartHandler(IRepository<LocationApartmentBlockPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
