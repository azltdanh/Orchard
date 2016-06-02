using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using RealEstate.Models;

namespace RealEstate.Handlers
{
    public class CustomerFeedbackPartHandler : ContentHandler
    {
        public CustomerFeedbackPartHandler(IRepository<CustomerFeedbackPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
