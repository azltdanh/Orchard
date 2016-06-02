using RealEstate.NewLetter.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.ViewModels
{
    public class ContactInboxViewModel
    {
    }
    public class ContactInboxCreateViewModel
    {
        [StringLength(255,ErrorMessage="Tên quá dài")]
        [Required(ErrorMessage = "Vui lòng nhập tên đầy đủ")]
        public string FullName { get; set; }

        [StringLength(255, ErrorMessage = "Email quá dài")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        public string Email { get; set; }

        [StringLength(255, ErrorMessage = "Điện thoại quá dài")]
        public string Phone { get; set; }

        [StringLength(255, ErrorMessage = "Tiêu đề quá dài")]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; }

        [StringLength(10000, ErrorMessage="Tối đa 10.000 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string Content { get; set; }

        public string Link { get; set; }
    }

    public class ContactInboxOptions
    {
        public string Search { get; set; }
    }
    public class ContactInboxIndexViewModel
    {
        public IList<ContactInboxEntry> ContactInboxs { get; set; }
        public ContactInboxOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ContactInboxEntry
    {
        public ContactInboxPart ContactInbox { get; set; }
    }
}