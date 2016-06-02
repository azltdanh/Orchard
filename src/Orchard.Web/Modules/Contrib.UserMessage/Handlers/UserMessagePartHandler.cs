using Contrib.UserMessage.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.ContentManagement;

namespace Contrib.UserMessage.Handlers {
    public class UserMessagePartHandler : ContentHandler {
        
        private readonly IContentManager _contentManager;

        public UserMessagePartHandler(
            IRepository<UserMessagePartRecord> repository,
            IContentManager contentManager)
        {
            Filters.Add(StorageFilter.For(repository));
            _contentManager = contentManager;

            //OnInitializing<UserMessagePart>(PropertySetHandlers);
            //OnLoaded<UserMessagePart>(LazyLoadHandlers);
        }

        //void LazyLoadHandlers(LoadContentContext context, UserMessagePart part)
        //{
        //    // add handlers that will load content just-in-time
        //    part.PreviousMessageField.Loader(() =>
        //        part.Record.PreviousMessage == null ?
        //        null : _contentManager.Get(part.Record.PreviousMessage.Id));
        //}

        //static void PropertySetHandlers(InitializingContentContext context, UserMessagePart part)
        //{
        //    // add handlers that will update records when part properties are set
        //    part.PreviousMessageField.Setter(sponsor =>
        //    {
        //        part.Record.PreviousMessage = sponsor == null
        //            ? null
        //            : sponsor.ContentItem.Record;
        //        return sponsor;
        //    });

        //    // Force call to setter if we had already set a value
        //    if (part.PreviousMessageField.Value != null)
        //        part.PreviousMessageField.Value = part.PreviousMessageField.Value;
        //}
    }
}