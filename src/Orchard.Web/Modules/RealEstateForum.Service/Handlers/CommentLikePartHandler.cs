using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Handlers
{
    public class CommentLikePartHandler : ContentHandler
    {
        public CommentLikePartHandler(IRepository<CommentLikePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}