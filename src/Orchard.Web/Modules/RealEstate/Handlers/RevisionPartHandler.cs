using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class RevisionPartHandler : ContentHandler
    {
        public RevisionPartHandler(IRepository<RevisionPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
