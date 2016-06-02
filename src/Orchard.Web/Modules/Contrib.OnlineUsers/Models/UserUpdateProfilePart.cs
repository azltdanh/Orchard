using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Contrib.OnlineUsers.Models
{
    public class UserUpdateProfilePart : ContentPart<UserUpdateProfileRecord>
    {
        public string Avatar
        {
            get { return Retrieve(r=>r.Avatar); }
            set { Store(r=>r.Avatar,value); }
        }
        public string FirstName
        {
            get { return Retrieve(r=>r.FirstName); }
            set { Store(r=>r.FirstName,value); }
        }
        public string LastName
        {
            get { return Retrieve(r => r.LastName); }
            set { Store(r=>r.LastName,value); }
        }
        public string DisplayName
        {
            get { return Retrieve(r=>r.DisplayName); }
            set { Store(r=>r.DisplayName,value); }
        }
        public Gender Gender
        {
            get { return Retrieve(r=>r.Gender); }
            set { Store(r=>r.Gender,value); }
        }
        public DateTime? DateOfBirth
        {
            get { return Retrieve(r=>r.DateOfBirth); }
            set { Store(r=>r.DateOfBirth,value); }
        }
        public string Address
        {
            get { return Retrieve(r=>r.Address); }
            set { Store(r=>r.Address,value); }
        }
        public string Phone
        {
            get { return Retrieve(r=>r.Phone); }
            set { Store(r=>r.Phone,value); }
        }
        public string Email
        {
            get { return Retrieve(r=>r.Email); }
            set { Store(r=>r.Email,value); }
        }
        public string Job
        {
            get { return Retrieve(r=>r.Job); }
            set { Store(r=>r.Job,value); }
        }
        public string Level
        {
            get { return Retrieve(r=>r.Level); }
            set { Store(r => r.Level, value); }
        }
        public string Website
        {
            get { return Retrieve(r=>r.Website); }
            set { Store(r=>r.Website,value); }
        }
        public string Note
        {
            get { return Retrieve(r=>r.Note); }
            set { Store(r=>r.Note,value); }
        }
        public string Signature
        {
            get { return Retrieve(r=>r.Signature); }
            set { Store(r=>r.Signature,value); }
        }
        public bool IsSignature
        {
            get { return Retrieve(r => r.IsSignature); }
            set { Store(r=>r.IsSignature,value); }
        }
        public string AreaAgencies 
        {
            get { return Retrieve(r=>r.AreaAgencies); }
            set { Store(r=>r.AreaAgencies,value); } 
        }
        public DateTime? EndDateAgencing 
        {
            get { return Retrieve(r=>r.EndDateAgencing); }
            set { Store(r=>r.EndDateAgencing,value); }
        }
        public string NickName
        {
            get { return Retrieve(r => r.NickName); }
            set { Store(r => r.NickName, value); }
        }
        public bool IsSelling
        {
            get { return Retrieve(r => r.IsSelling); }
            set { Store(r => r.IsSelling, value); }
        }
        public bool IsLeasing
        {
            get { return Retrieve(r => r.IsLeasing); }
            set { Store(r => r.IsLeasing, value); }
        }
        public Published PublishPhone
        {
            get { return Retrieve(r=>r.PublishPhone); }
            set { Store(r=>r.PublishPhone,value); }
        }
        public Published PublishAddress
        {
            get { return Retrieve(r=>r.PublishAddress); }
            set { Store(r=>r.PublishAddress,value); }
        }
        public Published PublishJob
        {
            get { return Retrieve(r=>r.PublishJob); }
            set { Store(r=>r.PublishJob,value); }
        }
        public Published PublishLevel
        {
            get { return Retrieve(r=>r.PublishLevel); }
            set { Store(r=>r.PublishLevel,value); }
        }      
    }
    public enum Gender
    {
        Male,
        Female
    }
    public enum Published
    {
        Publish,
        OnlyFriends,
        UnPublish
    }
    public class UserUpdateProfileRecord : ContentPartRecord
    {
        public virtual string Avatar { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual Gender Gender { get; set; }
        public virtual DateTime? DateOfBirth { get; set; }
        public virtual string Address { get; set; }
        public virtual string Phone { get; set; }
        public virtual string Email { get; set; }
        public virtual string Job { get; set; }
        public virtual string Level { get; set; }
        public virtual string Website { get; set; }
        public virtual string Note { get; set; }
        public virtual string Signature { get; set; }
        public virtual bool IsSignature { get; set; }
        public virtual string AreaAgencies { get; set; }
        public virtual DateTime? EndDateAgencing { get; set; }
        public virtual string NickName { get; set; }
        public virtual bool IsSelling { get; set; }
        public virtual bool IsLeasing { get; set; }

        public virtual Published PublishPhone { get; set; }
        public virtual Published PublishAddress { get; set; }
        public virtual Published PublishJob { get; set; }
        public virtual Published PublishLevel { get; set; }
    }
}