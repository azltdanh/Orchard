
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;
namespace RealEstate.Handlers
{
    public class GroupInApartmentBlockPartHandler : ContentHandler
    {
        public GroupInApartmentBlockPartHandler(IRepository<GroupInApartmentBlockPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
