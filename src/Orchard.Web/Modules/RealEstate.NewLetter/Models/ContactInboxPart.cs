using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.Models
{
    public class ContactInboxPartRecord : ContentPartRecord
    {
        public virtual string FullName { get; set; }
        public virtual string Email { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Title { get; set; }
        [StringLength(100000)]
        public virtual string Content { get; set; }
        public virtual string Link { get; set; }
        public virtual bool IsRead { get; set; }
        public virtual DateTime? DateCreated { get; set; }
        public virtual string HostName { get; set; }
    }
    public class ContactInboxPart : ContentPart<ContactInboxPartRecord>
    {
        public string FullName
        {
            get { return Retrieve(r=>r.FullName); }
            set { Store(r=>r.FullName,value); }
        }
        public string Email
        {
            get { return Retrieve(r=>r.Email); }
            set { Store(r=>r.Email,value); }
        }
        public string Phone
        {
            get { return Retrieve(r=>r.Phone); }
            set { Store(r=>r.Phone,value); }
        }
        public string Title
        {
            get { return Retrieve(r=>r.Title); }
            set { Store(r=>r.Title,value); }
        }
        [StringLength(100000)]
        public string Content
        {
            get { return Retrieve(r=>r.Content); }
            set { Store(r=>r.Content,value); }
        }
        public string Link
        {
            get { return Retrieve(r=>r.Link); }
            set { Store(r=>r.Link,value); }
        }
        public bool IsRead {
            get { return Retrieve(r=>r.IsRead); }
            set { Store(r=>r.IsRead,value); } 
        }
        public DateTime? DateCreated
        {
            get { return Retrieve(r=>r.DateCreated); }
            set { Store(r=>r.DateCreated,value); }
        }
        public string HostName
        {
            get { return Retrieve(r => r.HostName); }
            set { Store(r => r.HostName, value); }
        }
    }
}