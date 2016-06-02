using Contrib.OnlineUsers.Models;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using RealEstateForum.Service.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstateForum.Service.ViewModels
{
    public class UnitInvestIndexViewModel
    {
        public List<UnitInvestEntry> UnitInvests { get; set; }
        public UnitInvestPart UnitInvestPart { get; set; }
        public UnitInvestOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class UnitInvestOptions
    {
        public string Search { get; set; }
        public int? UnitInvetstId { get; set; }
        public IEnumerable<UnitInvestPart> UnitInvests { get; set; }
        public UnitInvestBulkAction BulkAction { get; set; }
    }

    public class UnitInvestEntry
    {
        public UnitInvestPart UnitInvestPartPart { get; set; }
        public bool IsChecked { get; set; }
    }

    public class UnitInvestCreateViewModel
    {
        public string ReturnUrl { get; set; }
        public IEnumerable<UnitInvestPart> UnitInvests{ get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung tên nhà tài trợ.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn ảnh đại diện")]
        public string Avatar { get; set; }

        public string Website { get; set; }
        public string Content { get; set; }
        public int SeqOrder { get; set; }
        public int GroupId { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class UnitInvestEditViewModel
    {
        public string ReturnUrl { get; set; }

        public IEnumerable<UnitInvestPart> UnitInvests { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhà tài trợ")]
        public string Name
        {
            get { return UnitInvest.As<UnitInvestPart>().Name; }
            set { UnitInvest.As<UnitInvestPart>().Name = value; }
        }

        [Required(ErrorMessage = "Vui lòng chọn ảnh đại diện")]
        public string Avatar
        {
            get { return UnitInvest.As<UnitInvestPart>().Avatar; }
            set { UnitInvest.As<UnitInvestPart>().Avatar = value; }
        }

        public string Website
        {
            get { return UnitInvest.As<UnitInvestPart>().Website; }
            set { UnitInvest.As<UnitInvestPart>().Website = value; }
        }

        public string Content
        {
            get { return UnitInvest.As<UnitInvestPart>().Content; }
            set { UnitInvest.As<UnitInvestPart>().Content = value; }
        }

        public int SeqOrder
        {
            get { return UnitInvest.As<UnitInvestPart>().SeqOrder; }
            set { UnitInvest.As<UnitInvestPart>().SeqOrder = value; }
        }

        public int GroupId
        {
            get { return UnitInvest.As<UnitInvestPart>().GroupId; }
            set { UnitInvest.As<UnitInvestPart>().GroupId = value; }
        }

        public bool IsEnabled
        {
            get { return UnitInvest.As<UnitInvestPart>().IsEnabled; }
            set { UnitInvest.As<UnitInvestPart>().IsEnabled = value; }
        }
        

        public IContent UnitInvest { get; set; }
    }

    public enum UnitInvestBulkAction
    {
        Delete,
        Enable,
        Disable,
    }
}