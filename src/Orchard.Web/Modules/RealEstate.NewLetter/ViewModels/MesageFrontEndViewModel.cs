using RealEstateForum.Service.ViewModels;

namespace RealEstate.NewLetter.ViewModels
{
    public class MesageFrontEndViewModel
    {
        public int CountMessageInboxFrontEnd { get; set; }
        public int CountMessageInboxAdmin { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}