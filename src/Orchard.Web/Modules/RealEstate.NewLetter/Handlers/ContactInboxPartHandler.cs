using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.NewLetter.Models;

namespace RealEstate.NewLetter.Handlers
{
    public class ContactInboxPartHandler : ContentHandler
    {
        public ContactInboxPartHandler(IRepository<ContactInboxPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}