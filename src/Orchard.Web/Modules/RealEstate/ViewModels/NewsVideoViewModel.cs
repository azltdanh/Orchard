using System.Collections.Generic;
using Orchard.ContentManagement;
using RealEstate.Models;

namespace RealEstate.ViewModels
{
    public class NewsVideoIndexViewModel
    {
        public List<NewsVideoEntry> NewsVideos { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class NewsVideoEntry
    {
        public NewsVideoPart NewsVideoPart { get; set; }
        public bool IsChecked { get; set; }
    }

    public class NewsVideoCreateViewModel
    {
        public string ReturnUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Enable { get; set; }
        public int SeqOrder { get; set; }
    }

    public class NewsVideoEditViewModel
    {
        public string ReturnUrl { get; set; }

        public int Id
        {
            get { return NewsVideo.As<NewsVideoPart>().Id; }
        }

        public string Title
        {
            get { return NewsVideo.As<NewsVideoPart>().Title; }
            set { NewsVideo.As<NewsVideoPart>().Title = value; }
        }
        public string Description
        {
            get { return NewsVideo.As<NewsVideoPart>().Description; }
            set { NewsVideo.As<NewsVideoPart>().Description = value; }
        }
        public bool Enable
        {
            get { return NewsVideo.As<NewsVideoPart>().Enable; }
            set { NewsVideo.As<NewsVideoPart>().Enable = value; }
        }
        public int SeqOrder
        {
            get { return NewsVideo.As<NewsVideoPart>().SeqOrder; }
            set { NewsVideo.As<NewsVideoPart>().SeqOrder = value; }
        }

        public IContent NewsVideo { get; set; }
    }
}
