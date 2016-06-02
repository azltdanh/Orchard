using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstateForum.Service.Models;


namespace RealEstateForum.Service.Handlers
{
    public class FilterRulesPartHandler : ContentHandler
    {
        public FilterRulesPartHandler(IRepository<FilterRulesPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}