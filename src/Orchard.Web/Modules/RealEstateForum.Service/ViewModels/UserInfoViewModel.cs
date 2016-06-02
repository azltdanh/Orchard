using Contrib.OnlineUsers.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateForum.Service.ViewModels
{
    public class UserInfo
    {
        public int Id { get; set; }
        public int CheckFriend { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string Signature { get; set; }
    }
    public class UserUpdateProfileEditViewModel
    {
        public string ReturnUrl { get; set; }
        public int UserProfileId { get; set; }
        public IEnumerable<UserUpdateProfilePart> UserUpdates { get; set; }

        public string Avatar
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Avatar; }
            set { UserUpdate.As<UserUpdateProfilePart>().Avatar = value; }
        }

        public string FirstName
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().FirstName; }
            set { UserUpdate.As<UserUpdateProfilePart>().FirstName = value; }
        }
        public string LastName
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().LastName; }
            set { UserUpdate.As<UserUpdateProfilePart>().LastName = value; }
        }
        public string DisplayName
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().DisplayName; }
            set { UserUpdate.As<UserUpdateProfilePart>().DisplayName = value; }
        }
        public Gender Gender
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Gender; }
            set { UserUpdate.As<UserUpdateProfilePart>().Gender = value; }
        }
        public DateTime? DateOfBirth
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().DateOfBirth; }
            set { UserUpdate.As<UserUpdateProfilePart>().DateOfBirth = value; }
        }
        public string Address
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Address; }
            set { UserUpdate.As<UserUpdateProfilePart>().Address = value; }
        }
        public string Phone
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Phone; }
            set { UserUpdate.As<UserUpdateProfilePart>().Phone = value; }
        }
        public string Email
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Email; }
            set { UserUpdate.As<UserUpdateProfilePart>().Email = value; }
        }
        public string Job
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Job; }
            set { UserUpdate.As<UserUpdateProfilePart>().Job = value; }
        }
        public string Level
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Level; }
            set { UserUpdate.As<UserUpdateProfilePart>().Level = value; }
        }
        public string Website
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Website; }
            set { UserUpdate.As<UserUpdateProfilePart>().Website = value; }
        }
        public string Note
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Note; }
            set { UserUpdate.As<UserUpdateProfilePart>().Note = value; }
        }
        public string Signature
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().Signature; }
            set { UserUpdate.As<UserUpdateProfilePart>().Signature = value; }
        }
        public bool IsSignature
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().IsSignature; }
            set { UserUpdate.As<UserUpdateProfilePart>().IsSignature = value; }
        }
        public Published PublishPhone
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().PublishPhone; }
            set { UserUpdate.As<UserUpdateProfilePart>().PublishPhone = value; }
        }
        public Published PublishAddress
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().PublishAddress; }
            set { UserUpdate.As<UserUpdateProfilePart>().PublishAddress = value; }
        }
        public Published PublishJob
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().PublishJob; }
            set { UserUpdate.As<UserUpdateProfilePart>().PublishJob = value; }
        }
        public Published PublishLevel
        {
            get { return UserUpdate.As<UserUpdateProfilePart>().PublishLevel; }
            set { UserUpdate.As<UserUpdateProfilePart>().PublishLevel = value; }
        }
        public IContent UserUpdate { get; set; }
    }
    public class UserUpdateProfileOptions
    {
        public int? UserId { get; set; }
        public UserPart UserPart { get; set; }
        public UserUpdateProfilePart UserUpdateProfilePart { get; set; }
        public IEnumerable<UserPart> UserParts { get; set; }
        public IEnumerable<UserUpdateProfilePart> UserUpdates { get; set; }
        public IEnumerable<UserUpdateEntry> UserUpdateEntries { get; set; }
    }
    public class UserUpdateEntry
    {
        public UserPart UserPart { get; set; }
        public UserUpdateProfilePart UserUpdateProfilePart { get; set; }
    }
}