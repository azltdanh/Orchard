using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class DirectionPartHandler : ContentHandler
    {
        public DirectionPartHandler(IRepository<DirectionPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
