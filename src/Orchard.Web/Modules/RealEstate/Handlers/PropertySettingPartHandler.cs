using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class PropertySettingPartHandler : ContentHandler
    {
        public PropertySettingPartHandler(IRepository<PropertySettingPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
