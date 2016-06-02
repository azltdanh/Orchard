using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class CustomerFeedbackViewModel
    {
    }

    public class CustomerFeedbackCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public string ShortName { get; set; }

        public string CssClass { get; set; }

        public int SeqOrder { get; set; }

        public bool IsEnabled { get; set; }

        public IContent CustomerFeedback { get; set; }
    }

    public class CustomerFeedbackEditViewModel
    {
        [Required]
        public string Name
        {
            get { return CustomerFeedback.As<CustomerFeedbackPart>().Name; }
            set { CustomerFeedback.As<CustomerFeedbackPart>().Name = value; }
        }

        public string ShortName
        {
            get { return CustomerFeedback.As<CustomerFeedbackPart>().ShortName; }
            set { CustomerFeedback.As<CustomerFeedbackPart>().ShortName = value; }
        }

        public string CssClass
        {
            get { return CustomerFeedback.As<CustomerFeedbackPart>().CssClass; }
            set { CustomerFeedback.As<CustomerFeedbackPart>().CssClass = value; }
        }

        public int SeqOrder
        {
            get { return CustomerFeedback.As<CustomerFeedbackPart>().SeqOrder; }
            set { CustomerFeedback.As<CustomerFeedbackPart>().SeqOrder = value; }
        }

        public bool IsEnabled
        {
            get { return CustomerFeedback.As<CustomerFeedbackPart>().IsEnabled; }
            set { CustomerFeedback.As<CustomerFeedbackPart>().IsEnabled = value; }
        }

        public IContent CustomerFeedback { get; set; }
    }

    public class CustomerFeedbackIndexViewModel
    {
        public IList<CustomerFeedbackEntry> Feedbacks { get; set; }
        public CustomerFeedbackIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CustomerFeedbackEntry
    {
        public CustomerFeedbackPartRecord Feedback { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CustomerFeedbackIndexOptions
    {
        public string Search { get; set; }
        public CustomerFeedbackOrder Order { get; set; }
        public CustomerFeedbackFilter Filter { get; set; }
        public CustomerFeedbackBulkAction BulkAction { get; set; }
    }

    public enum CustomerFeedbackOrder
    {
        SeqOrder,
        Name
    }

    public enum CustomerFeedbackFilter
    {
        All
    }

    public enum CustomerFeedbackBulkAction
    {
        None,
        Enable,
        Disable,
        Delete,
    }
}