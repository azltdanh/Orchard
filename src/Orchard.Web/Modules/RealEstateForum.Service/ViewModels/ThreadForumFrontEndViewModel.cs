using System.Collections.Generic;

namespace RealEstateForum.Service.ViewModels
{
    public class ThreadForumFrontEndViewModel
    {
        public List<ThreadForumFrontEndEntry> ListTopics { get; set; }
        public ThreadInfo ThreadInfo { get; set; }
        public Dictionary<string, string> Metas { get; set; }
        public int PostCount { get; set; }
        public string ContentFromFile { get; set; }
    }
    public class ThreadForumFrontEndEntry
    {
        public ThreadInfo TopicInfo { get; set; }
        public PostItemNewest PostNewest { get; set; }
        public List<PostItem> ListPostItem { get; set; }
        public int PostCount { get; set; }
    }
}