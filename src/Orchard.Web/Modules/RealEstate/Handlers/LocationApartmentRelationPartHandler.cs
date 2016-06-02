using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class LocationApartmentRelationPartHandler : ContentHandler
    {
        public LocationApartmentRelationPartHandler(IRepository<LocationApartmentRelationPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
