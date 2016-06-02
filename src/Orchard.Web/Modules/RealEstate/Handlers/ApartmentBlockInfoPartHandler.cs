using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;
namespace RealEstate.Handlers
{
    public class ApartmentBlockInfoPartHandler : ContentHandler
    {
        public ApartmentBlockInfoPartHandler(IRepository<ApartmentBlockInfoPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
