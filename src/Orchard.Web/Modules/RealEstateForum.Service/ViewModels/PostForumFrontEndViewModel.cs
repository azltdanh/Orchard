using System;
using System.Collections.Generic;
using System.Linq;
using RealEstate.Helpers;
using RealEstateForum.Service.Models;

namespace RealEstateForum.Service.ViewModels
{
    public class TopicForumFrontEndViewModel
    {
        public TopicInfo TopicInfo { get; set; }
        public Dictionary<string, string> Metas { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }
    public class TopicInfo
    {
        public int ThreadId { get; set; }
        public string ThreadShortName { get; set; }
        public string TopicShortName { get; set; }
        public string TopicName { get; set; }
        public string GetHostName { get; set; }
        public List<PostByTopicEntry> ListPostItem { get; set; }
    }
    public class PostByTopicEntry : PostItemNewest
    {
        public string ThreadShortName { get; set; }
        public int ThreadId { get; set; }
        public string TopicShortName { get; set; }
        public DateTime? DateUpdated { get; set; }
        public UserInfo UserInfo { get; set; }
    }

    public class PostItemNewest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public string DefaultImage { get; set; }
        public string DisplayForNameForUrl
        {
            get
            {
                string url = Title.ToSlug();
                return url.Count() >= 100 ? url.Substring(0, 100) : url;
            }
        }
        public string HostName { get; set; }
    }
    public class PostItem
    {
        public int Id { get; set; }
        public ForumPostStatusPartRecord PostStatus { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public string DefaultImage { get; set; }
        public string ThreadShortName { get; set; }
        public string TopicShortName { get; set; }
        public bool IsPinned { get; set; }
        public UserInfo UserInfo { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string DisplayForNameForUrl 
        {
            get
            {
                string url = Title.ToSlug();
                return url.Count() >= 100 ? url.Substring(0, 100) : url;
            }
        }
        public string GetHostName { get; set; }
    }
    public class PostForumFrontEnDetailViewModel
    {
        public string ThreadShortName { get; set; }
        public PostItem PostDetail { get; set; }
        public UserInfo UserCurrent { get; set; }
        public bool IsShowSignature { get; set; }
        public string ContentFromFile { get; set; }
    }

    public class WidgetPostItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string DisplayForNameForUrl
        {
            get
            {
                string url = Title.ToSlug();
                return url.Count() >= 100 ? url.Substring(0, 100) : url;
            }
        }
        public string ThreadShortName { get; set; }
        public string TopicShortName { get; set; }
        public int ThreadId { get; set; }
        public string DefaultImage { get; set; }
    }
    public class WidgetIsMarketViewModel
    {
        public List<PostByTopicEntry> ListPostWithImage { get; set; }
        public List<WidgetPostItem> ListPostNotImage { get; set; }
    }

    public class ForumSearchResults
    {
        public List<ForumPostEntryFrontEnd> ListPostItem { get; set; }
        public Dictionary<string, string> Metas { get; set; }
        public PostFilterOptions Options { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }
    public class ForumPostEntryFrontEnd
    {
        public ForumPostPart ForumPostItem { get; set; }
        public string ThreadShortName { get; set; }
        public string TopicShortName { get; set; }
        public UserInfo UserInfo { get; set; }
        public string DisplayForNameForUrl
        {
            get
            {
                string url = this.Title.ToSlug();
                return url.Count() >= 100 ? url.Substring(0, 100) : url;
            }
        }
        public string SubContent { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DefaultImage { get; set; }
        public DateTime? DateUpdated { get; set; }
    }

    public class PostOfUserWidgetViewModel
    {
        public List<PostItemOfUserWidgetViewModel> ListPostTitle { get; set; }
        public bool IsOwner { get; set; }
        public dynamic Pager { get; set; }
        public int TotalCount { get; set; }
    }
    public class PostItemOfUserWidgetViewModel
    {
        public int Id { get; set; }
        //public 
        public string Title { get; set; }
        public string DisplayNameUrl
        {
            get
            {
                string url = this.Title.ToSlug();
                return url.Count() >= 100 ? url.Substring(0, 100) : url;
            }
        }
        public string ThreadShortName { get; set; }
        public string TopicShortName { get; set; }
        public string PostStatus { get; set; }

    }
}