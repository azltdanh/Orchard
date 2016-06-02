using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    class UserActionViewModel
    {
    }

    public class UserActionCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public double Point { get; set; }

        public IContent UserAction { get; set; }
    }

    public class UserActionEditViewModel
    {
        [Required]
        public string Name
        {
            get { return UserAction.As<UserActionPart>().Name; }
            set { UserAction.As<UserActionPart>().Name = value; }
        }

        public string ShortName
        {
            get { return UserAction.As<UserActionPart>().ShortName; }
            set { UserAction.As<UserActionPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return UserAction.As<UserActionPart>().CssClass; }
            set { UserAction.As<UserActionPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return UserAction.As<UserActionPart>().SeqOrder; }
            set { UserAction.As<UserActionPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return UserAction.As<UserActionPart>().IsEnabled; }
            set { UserAction.As<UserActionPart>().IsEnabled = value; }
        }

        public double Point
        {
            get { return UserAction.As<UserActionPart>().Point; }
            set { UserAction.As<UserActionPart>().Point = value; }
        }

        public IContent UserAction { get; set; }
    }

    public class UserActionIndexViewModel
    {
        public IList<UserActionEntry> UserActions { get; set; }
        public UserActionIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class UserActionEntry
    {
        public UserActionPartRecord UserAction { get; set; }
        public bool IsChecked { get; set; }
    }

    public class UserActionIndexOptions
    {
        public string Search { get; set; }
        public UserActionOrder Order { get; set; }
        public UserActionFilter Filter { get; set; }
        public UserActionBulkAction BulkAction { get; set; }
    }

    public enum UserActionOrder
    {
        SeqOrder,
        Name
    }

    public enum UserActionFilter
    {
        All
    }

    public enum UserActionBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}
