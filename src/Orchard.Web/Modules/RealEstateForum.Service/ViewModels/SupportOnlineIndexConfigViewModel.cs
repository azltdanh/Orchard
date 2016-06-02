using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

using Orchard.ContentManagement;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.ViewModels
{
    public class SupportOnlineIndexConfigViewModel
    {
        public IList<SupportOnlineEntry> Supportss { get; set; }
        public SupportOnlineOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }
    public class SupportOnlineOptions
    {
        public string Search { get; set; }
    }
    public class SupportOnlineEntry
    {
        public SupportOnlineConfigPartRecord Supports { get; set; }
        public bool IsChecked { get; set; }
    }

    public class SupportOnlineWidgetViewModel
    {
        public SupportOnlineConfigPart SupportOnlineModel { get; set; }
    }

    public class SupportOnlineCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string NumberPhone { get; set; }

        public string YahooNick { get; set; }

        public string SkypeNick { get; set; }

        public IContent SupportOnline { get; set; }
    }

    public class SupportOnlineEditViewModel
    {
        public string ReturnUrl { get; set; }
        [Required]
        public string NumberPhone
        {
            get { return SupportOnline.As<SupportOnlineConfigPart>().NumberPhone; }
            set { SupportOnline.As<SupportOnlineConfigPart>().NumberPhone = value; }
        }

        public string YahooNick
        {
            get { return SupportOnline.As<SupportOnlineConfigPart>().YahooNick; }
            set { SupportOnline.As<SupportOnlineConfigPart>().YahooNick = value; }
        }

        public string SkypeNick
        {
            get { return SupportOnline.As<SupportOnlineConfigPart>().SkypeNick; }
            set { SupportOnline.As<SupportOnlineConfigPart>().SkypeNick = value; }
        }

        public IContent SupportOnline { get; set; }
    }
}