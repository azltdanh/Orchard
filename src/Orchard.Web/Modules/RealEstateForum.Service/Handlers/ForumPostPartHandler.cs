using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Handlers
{
    public class ForumPostPartHandler : ContentHandler
    {
        public ForumPostPartHandler(IRepository<ForumPostPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}