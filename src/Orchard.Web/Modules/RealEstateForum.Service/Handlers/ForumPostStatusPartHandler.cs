using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.Handlers
{
    public class ForumPostStatusPartHandler : ContentHandler
    {
        public ForumPostStatusPartHandler(IRepository<ForumPostStatusPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}