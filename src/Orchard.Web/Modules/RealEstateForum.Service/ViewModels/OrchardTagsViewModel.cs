using RealEstate.ViewModels;

namespace RealEstateForum.Service.ViewModels
{
    public class OrchardTagsViewModel
    {
        public TopicForumFrontEndViewModel TopicForum { get; set; }
        //public int ForumPostTotalCount { get; set; }
        //public IEnumerable<PageForumPart> ListPageForumPart { get; set; }
        public PropertyDisplayIndexViewModel ListPropertyPart { get; set; }
        public string TagName { get; set; }
        public dynamic Pager { get; set; }
    }
}