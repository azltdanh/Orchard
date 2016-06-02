using System;
using System.Collections.Generic;
using RealEstateForum.Service.Models;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace RealEstateForum.Service.ViewModels
{
    public class HostNameViewModel
    {
    }
    public class HostNameCreateViewModel
    {
        //HostName
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string CssClass { get; set; }
        public int SeqOrder { get; set; }
        public bool IsEnabled { get; set; }

        public string ReturnUrl { get; set; }
    }
    public class HostNameEditViewModel
    {
        //HostNames
        public string Name
        {
            get { return HostNames.As<HostNamePart>().Name; }
            set { HostNames.As<HostNamePart>().Name = value; }
        }
        public string ShortName
        {
            get { return HostNames.As<HostNamePart>().ShortName; }
            set { HostNames.As<HostNamePart>().ShortName = value; }
        }
        public string CssClass
        {
            get { return HostNames.As<HostNamePart>().CssClass; }
            set { HostNames.As<HostNamePart>().CssClass = value; }
        }
        public int SeqOrder
        {
            get { return HostNames.As<HostNamePart>().SeqOrder; }
            set { HostNames.As<HostNamePart>().SeqOrder = value; }
        }
        public bool IsEnabled
        {
            get { return HostNames.As<HostNamePart>().IsEnabled; }
            set { HostNames.As<HostNamePart>().IsEnabled = value; }
        }

        public string ReturnUrl { get; set; }

        public IContent HostNames { get; set; }
    }
    public class HostNameIndexViewModel
    {
        public HostNameIndexOptions Options { get; set; }
        public IList<HostNameEntry> HostNames { get; set; }

        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }
    public class HostNameEntry
    {
        public HostNamePart HostName { get; set; }
    }
    public class HostNameIndexOptions
    {
        public string ReturnUrl { get; set; }

        public int HostNameId { get; set; }
        public IEnumerable<HostNamePart> HostNames { get; set; }

        public HostNameOrder Order { get; set; }
    }
    public enum HostNameOrder
    {
        Id,
        Name,
        ShortName
    }
}