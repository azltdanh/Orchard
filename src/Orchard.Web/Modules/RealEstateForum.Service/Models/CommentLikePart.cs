using System;
using System.Collections.Generic;

using Orchard.Users.Models;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;

namespace RealEstateForum.Service.Models
{
    public class CommentLikePartRecord : ContentPartRecord
    {
        public virtual CommentForumPartRecord Comment { get; set; }
        public virtual UserPartRecord User { get; set; }
    }
    public class CommentLikePart : ContentPart<CommentLikePartRecord>
    {
        public CommentForumPartRecord Comment
        {
            get { return Record.Comment; }
            set {Record.Comment = value;}
        }
        public UserPartRecord User
        {
            get { return Record.User; }
            set { Record.User = value;}
        }
    }
}