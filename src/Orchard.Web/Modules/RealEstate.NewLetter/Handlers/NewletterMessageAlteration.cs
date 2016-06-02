using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Messaging.Events;
using Orchard.Messaging.Models;
using Orchard.Settings;
using RealEstate.NewLetter.Models;

namespace RealEstate.NewLetter.Handlers
{
    public class NewletterMessageAlteration : IMessageEventHandler
    { 
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public NewletterMessageAlteration(IContentManager contentManager, ISiteService siteService)
        {
            _contentManager = contentManager;
            _siteService = siteService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Sending(MessageContext context)
        {
            if (context.MessagePrepared)
                return;

            switch (context.Type)
            {
                case MessageNewletterType.NewLetterCustomer:
                    //var recipient3 = GetRecipient(context);
                    context.MailMessage.Subject = T("{0}", context.Properties["Title"]).Text;
                    context.MailMessage.Body =
                        T("Bạn vui lòng click vào link dưới đây, để xem kết quả tìm kiếm bất động sản phù hợp với yêu cầu của bạn: <br/>{0} <br > {1} <br> Từ chối nhận email từ dinhgianhadat.vn, click vào link sau: {2}", context.Properties["ContentDefault"], context.Properties["ContentLink"], context.Properties["Denined"]).Text;
                    FormatEmailBody(context);
                    context.MessagePrepared = true;
                    break;
            }
        }

        private static void FormatEmailBody(MessageContext context)
        {
            context.MailMessage.Body = "<p style=\"font-family:Arial, Helvetica; font-size:10pt;\">" + context.MailMessage.Body;
            context.MailMessage.Body += "</p>";
        }

        public void Sent(MessageContext context)
        {
        }
    }
}