using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using System.Collections.Generic;

using RealEstate.Models;
using System.Web.Mvc;
using System.Collections;
using Orchard.Roles.Models;

namespace RealEstate.ViewModels
{
    public class UserInGroupCreateViewModel
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập User Name.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập E-mail.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Password.")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 7)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Confirm Password.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public int? GroupId { get; set; }
        public IEnumerable<SelectListItem> Groups { get; set; }

        public int? DefaultAdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? DefaultTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        // Roles
        public bool EnableEditRoles { get; set; }
        public IList<Orchard.Roles.ViewModels.UserRoleEntry> Roles { get; set; }

    }

    public class UserInGroupEditViewModel  
    {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập User Name.")]
        public string UserName {
            get { return User.As<UserPart>().UserName; }
            set { User.As<UserPart>().UserName = value; }
        }

        [Required(ErrorMessage = "Vui lòng nhập E-mail.")]
        public string Email {
            get { return User.As<UserPart>().Email; }
            set { User.As<UserPart>().Email = value; }
        }

        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }

        public int? GroupId { get; set; }
        public IEnumerable<SelectListItem> Groups { get; set; }

        public int? DefaultAdsTypeId { get; set; }
        public IEnumerable<AdsTypePartRecord> AdsTypes { get; set; }

        public int? DefaultTypeGroupId { get; set; }
        public IEnumerable<PropertyTypeGroupPartRecord> TypeGroups { get; set; }

        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public IEnumerable<UserActivityPartRecord> Activities { get; set; }
        public UserActivitiesIndexViewModel UserActivities { get; set; }

        public IContent User { get; set; }
    }

    public class UsersInGroupIndexViewModel
    {
        public IList<UserInGroupEntry> Users { get; set; }
        public UserInGroupIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class UserInGroupIndexOptions
    {
        public string Search { get; set; }
        public UsersOrder Order { get; set; }
        public UsersFilter Filter { get; set; }
        public UsersBulkAction BulkAction { get; set; }
        public int? GroupId { get; set; }
        public IEnumerable<UserGroupPartRecord> Groups { get; set; }
        public int? RoleId { get; set; }
        public IEnumerable<RoleRecord> Roles { get; set; }
    }

    public enum UsersOrder
    {
        Name,
        Email
    }

    public enum UsersFilter
    {
        All,
        Approved,
        Pending,
        EmailPending
    }

    public enum UsersBulkAction
    {
        None,
        Delete,
        Disable,
        Approve,
        ChallengeEmail,
        SkipChallengeEmail
    }
}