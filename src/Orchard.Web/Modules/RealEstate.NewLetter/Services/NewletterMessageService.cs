using Orchard;
using Orchard.ContentManagement;
using Orchard.Messaging.Services;
using RealEstate.NewLetter.Models;
using System.Collections.Generic;

namespace RealEstate.NewLetter.Services
{
    public interface INewletterMessageService : IDependency
    {
        void NewLetterList(string usernameOrEmail, List<string> contentLink, string contentDefault, string title, string radomcode);
    }

    public class NewletterMessageService : INewletterMessageService
    {
        private readonly IContentManager _contentManager;
        private readonly IMessageManager _messageManager;
        public NewletterMessageService(
            IOrchardServices services,
            IMessageManager messageManager,
            IContentManager contentManager)
        {
            Services = services;
            _messageManager = messageManager;
            _contentManager = contentManager;
        }
        public IOrchardServices Services { get; set; }

        public void NewLetterList(string usernameOrEmail, List<string> contentLink, string contentDefault, string title, string radomcode)
        {
            string _contentLink = "";
            if (contentLink.Count > 0) 
            {
                foreach (var n in contentLink)
                {
                    _contentLink += n + " <br/><br/>";
                }
            }
            IEnumerable<string> email = new string[] { usernameOrEmail };
            _messageManager.Send(email, MessageNewletterType.NewLetterCustomer, "email", new Dictionary<string, string> { { "ContentLink", _contentLink }, { "ContentDefault", contentDefault }, { "Title", title }, {"Denined", radomcode} });
        }
    }
}