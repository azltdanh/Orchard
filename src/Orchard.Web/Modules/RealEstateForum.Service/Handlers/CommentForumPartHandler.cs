using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Handlers
{
    public class CommentForumPartHandler : ContentHandler
    {
        public CommentForumPartHandler(IRepository<CommentForumPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}