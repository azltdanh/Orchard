using Contrib.OnlineUsers.Models;

namespace RealEstate.UserControlPanel.ViewModels
{
    public class UserProfileViewModel
    {
        public UserUpdateProfilePart UserProfile { get; set; }
    }
    public class PostVIPViewModel
    {
        public long Amount { get; set; }
        public string AmountVND { get; set; }
        public int AdsTypeVIP { get; set; }
        public string DateVipFrom { get; set; }
        public string DateVipTo { get; set; }
        public long[] UnitArray { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
        public int DistrictId { get; set; }
        public string AdsTypeCssClass { get; set; }
    }
}
