using Orchard.ContentManagement;
using RealEstate.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RealEstate.NewLetter.ViewModels
{
    public class ListCustomerViewModel
    {
    }
    public class ListCustomerLetterIndexOptions
    {
        public string Search { get; set; }
        public ListCustomerLetterFilter Filter { get; set; }
        public ListCustomerLetterBulkAction BulkAction { get; set; }
    }

    public enum ListCustomerLetterFilter
    {
        All
    }
    public class ListCustomerLetterIndexViewModel
    {
        public IList<ListCustomerLetterEntry> ListCustomerLetters { get; set; }
        public ListCustomerLetterIndexOptions Options { get; set; }
        //public List<int> listcustomerId { get; set; }
        //public List<string> listcustomerEmail { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ListCustomerLetterEntry
    {
        public CustomerPartRecord ListCustomerLetter { get; set; }
        public int _count { get; set; }
        public bool StatusException { get; set; }
        public bool IsChecked { get; set; }
    }
    public class ListCustomerLetterSendMailViewModel
    {
        public List<int> listcustomerId { get; set; }
        public string _lstcustomerId { get; set; }
        public List<string> listcustomerEmail { get; set; }
        [Required]
        public string MailTitle { get; set; }
        [Required]
        public string MailContent { get; set; }
        public string link { get; set; }

    }

    public class CustomerExceptionVerify
    {
        public string Code { get; set; }
        public string EmailException { get; set; }

        public IContent CustomerEmailException { get; set; }
    }
    public enum ListCustomerLetterBulkAction
    {
        None,
        SendMail
    }
}