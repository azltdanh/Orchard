using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class UserActionPartHandler : ContentHandler
    {
        public UserActionPartHandler(IRepository<UserActionPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
