using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Handlers
{
    public class ForumFriendPartHandler : ContentHandler
    {
        public ForumFriendPartHandler(IRepository<ForumFriendPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}