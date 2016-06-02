

using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class VideoTypePartHandler : ContentHandler
    {
        public VideoTypePartHandler(IRepository<VideoTypePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
