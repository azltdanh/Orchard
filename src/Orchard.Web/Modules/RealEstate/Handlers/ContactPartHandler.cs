using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class ContactPartHandler : ContentHandler
    {
        public ContactPartHandler(IRepository<ContactPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
