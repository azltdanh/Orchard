

using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class VideoManagePartHandler : ContentHandler
    {
        public VideoManagePartHandler(IRepository<VideoManagePartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
