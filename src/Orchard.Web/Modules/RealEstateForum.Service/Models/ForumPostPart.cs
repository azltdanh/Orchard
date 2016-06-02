using System;
using System.ComponentModel.DataAnnotations;

using Orchard.ContentManagement;
using Orchard.Users.Models;
using System.Linq;

using Orchard.ContentManagement.Records;
using RealEstate.Helpers;

namespace RealEstateForum.Service.Models
{
    public class ForumPostPartRecord : ContentPartRecord
    {
        public virtual ForumThreadPartRecord Thread { get; set; }
        public virtual UserPartRecord Blog { get; set; }
        public virtual int? BlogPostId { get; set; }
        public virtual string Title { get; set; }
        [StringLength(500000)]
        public virtual string Description { get; set; }
        [StringLength(500000)]
        public virtual string Content { get; set; }
        public virtual UserPartRecord UserPost { get; set; }
        public virtual string CssImage { get; set; }
        public virtual bool IsPinned { get; set; }
        public virtual DateTime? TimeExpiredPinned { get; set; }
        public virtual bool IsProject { get; set; }
        public virtual bool IsMarket { get; set; }
        public virtual bool IsHeighLight { get; set; }
        public virtual PublishStatusPartRecord PublishStatus { get; set; }
        public virtual DateTime? DateCreated { get; set; }
        public virtual DateTime? DateUpdated { get; set; }
        public virtual long Views { get; set; }
        public virtual ForumPostStatusPartRecord StatusPost { get; set; }
        public virtual bool IsShareBlog { get; set; }
        public virtual DateTime? BlogDateCreated { get; set; }
        public virtual int SeqOrder { get; set; }
        public virtual string HostName { get; set; }
    }
    public class ForumPostPart : ContentPart<ForumPostPartRecord>
    {
        public ForumThreadPartRecord Thread
        {
            get { return Record.Thread; }
            set { Record.Thread = value; }
        }
        public UserPartRecord Blog
        {
            get { return Record.Blog; }
            set { Record.Blog = value; }
        }

        public int? BlogPostId
        {
            get { return Retrieve(r => r.BlogPostId);}
            set { Store(r => r.BlogPostId, value); }
        }
        public string Title
        {
            get { return Retrieve(p=>p.Title); }
            set { Store(p => p.Title, value); }
        }
        [StringLength(1000000)]
        public string Description
        {
            get { return Retrieve(p => p.Description); }
            set { Store(p => p.Description, value); }
        }
        [StringLength(1000000)]
        public string Content
        {
            get { return Retrieve(p => p.Content); }//HttpUtility.HtmlDecode(
            set { Store(p => p.Content, value); }
        }
        public UserPartRecord UserPost
        {
            get { return Record.UserPost; }
            set { Record.UserPost = value; }
        }
        public string CssImage
        {
            get { return Retrieve(p => p.CssImage); }
            set { Store(p => p.CssImage, value); }
        }
        public bool IsPinned
        {
            get { return Retrieve(p => p.IsPinned); }
            set { Store(p => p.IsPinned, value); }
        }
        public DateTime? TimeExpiredPinned
        {
            get { return Retrieve(p => p.TimeExpiredPinned); }
            set { Store(p => p.TimeExpiredPinned, value); }
        }
        public bool IsProject
        {
            get { return Retrieve(p => p.IsProject); }
            set { Store(p => p.IsProject, value); }
        }
        public bool IsMarket
        {
            get { return Retrieve(p => p.IsMarket); }
            set { Store(p => p.IsMarket, value); }
        }
        public bool IsHeighLight
        {
            get { return Retrieve(p => p.IsHeighLight); }
            set { Store(p => p.IsHeighLight, value); }
        }
        public PublishStatusPartRecord PublishStatus
        {
            get { return Record.PublishStatus; }
            set { Record.PublishStatus = value; }
        }
        public DateTime? DateCreated
        {
            get { return Retrieve(p => p.DateCreated); }
            set { Store(p => p.DateCreated, value); }
        }
        public DateTime? DateUpdated
        {
            get { return Retrieve(p => p.DateUpdated); }
            set { Store(p => p.DateUpdated, value); }
        }
        public long Views
        {
            get { return Retrieve(p => p.Views); }
            set { Store(p => p.Views, value); }
        }

        public ForumPostStatusPartRecord StatusPost
        {
            get { return Record.StatusPost; }
            set { Record.StatusPost = value; }
        }
        public bool IsShareBlog
        {
            get { return Retrieve(p => p.IsShareBlog); }
            set { Store(p => p.IsShareBlog, value); }
        }
        public DateTime? BlogDateCreated
        {
            get { return Retrieve(p => p.BlogDateCreated); }
            set { Store(p => p.BlogDateCreated, value); }
        }
        public int SeqOrder
        {
            get { return Retrieve(p => p.SeqOrder); }
            set { Store(p => p.SeqOrder, value); }
        }
        public string HostName
        {
            get { return Retrieve(p => p.HostName); }
            set { Store(p => p.HostName, value); }
        }
        public string DisplayForUrl//Short Name & Title
        {
            get
            {
                string url = "";

                url += Record.Thread.ShortName;

                string urlTitle = Record.Title.ToSlug();//{TopicShortName}/{Title}

                url += urlTitle.Count() >= 100 ? "/" + urlTitle.Substring(0, 100) : "/" + urlTitle;

                return url;
            }
        }

        public string DisplayNameForUrl
        {
            get
            {
                string url = Title.ToSlug();
                return url.Count() >= 100 ? url.Substring(0, 100) : url;
            }
        }
    }
}