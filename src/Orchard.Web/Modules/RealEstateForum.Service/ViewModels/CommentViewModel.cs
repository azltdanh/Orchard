using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealEstateForum.Service.ViewModels
{
    public class CommentIndexViewModel
    {
        public int TotalCount { get; set; }
        public List<CommentPostViewModel> ListComment { get; set; }
        public bool IsMangagementOrAdmin { get; set; }
        public dynamic Pager { get; set; }
    }
    public class CommentPostViewModel
    {
        public int CommentId { get; set; }
        public int ParentCommentId { get; set; }
        public List<SubCommentEntry> ListSubComment { get; set; }
        public string Content { get; set; }
        public string TimeAgo { get; set; }
        public int SeqOrder { get; set; }
        public bool IsOwner { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    public class SubCommentEntry
    {
        public int SubCommentId { get; set; }
        public string SubCommentContent { get; set; }
        public string TimeAgo { get; set; }
        public bool IsOwner { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    public class ResultByPostComment
    {
        public int Id { get; set; }
        public bool IsSuccess { get; set; }
        public string TimeAgo { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}