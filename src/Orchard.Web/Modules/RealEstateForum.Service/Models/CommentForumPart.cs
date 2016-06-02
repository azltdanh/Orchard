using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Users.Models;

namespace RealEstateForum.Service.Models
{
    public class CommentForumPartRecord : ContentPartRecord
    {
        public virtual ForumPostPartRecord ForumPost { get; set; }
        public virtual UserPartRecord UserComment { get; set; }
        public virtual int ParentCommentId { get; set; }

        [StringLength(100000)]
        public virtual string Content { get; set; }
        public virtual DateTime? DateCreated { get; set; }
        public virtual int SortOrder { get; set; }
    }
    public class CommentForumPart : ContentPart<CommentForumPartRecord>
    {
        public ForumPostPartRecord ForumPost
        {
            get { return Record.ForumPost; }
            set { Record.ForumPost = value; }
        }
        public UserPartRecord UserComment 
        {
            get { return Record.UserComment; }
            set { Record.UserComment = value; }
        }
        public int ParentCommentId
        {
            get { return Record.ParentCommentId; }
            set { Record.ParentCommentId = value; }
        }
        [StringLength(100000)]
        public string Content
        {
            get { return Retrieve(p=>p.Content); }
            set { Store(p=>p.Content,value); }
        }
        public DateTime? DateCreated
        {
            get { return Retrieve(r=>r.DateCreated); }
            set { Store(r=>r.DateCreated,value); }
        }
        public int SortOrder
        {
            get { return Retrieve(r=>r.SortOrder); }
            set { Store(r=>r.SortOrder,value); }
        }
    }
}