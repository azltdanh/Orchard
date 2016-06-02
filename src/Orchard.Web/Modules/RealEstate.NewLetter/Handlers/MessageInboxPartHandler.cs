using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.NewLetter.Models;

namespace RealEstate.NewLetter.Handlers
{
    public class MessageInboxPartHandler : ContentHandler
    {
        public MessageInboxPartHandler(IRepository<MessageInboxPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}