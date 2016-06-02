using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Records;
using Orchard.ContentManagement;

namespace RealEstate.UserControlPanel.Models
{
    public class UserProfileInfomationPartRecord : ContentPartRecord
    {
        public virtual string Address { get; set; }
        public virtual string Phone { get; set; }
        public virtual string CompanyName { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual string Website { get; set; }
    }

    public class UserProfileInfomationPart : ContentPart<UserProfileInfomationPartRecord>
    {
        public string Address
        {
            get { return Record.Address; }
            set { Record.Address = value; }
        }
        public string Phone
        {
            get { return Record.Phone; }
            set { Record.Phone = value; }
        }
        public string CompanyName
        {
            get { return Record.CompanyName; }
            set { Record.CompanyName = value; }
        }
        public DateTime? DateOfBirth
        {
            get { return Record.DateOfBirth; }
            set { Record.DateOfBirth = value; }
        }
        public string Website
        {
            get { return Record.Website; }
            set { Record.Website = value; }
        }
    }
}