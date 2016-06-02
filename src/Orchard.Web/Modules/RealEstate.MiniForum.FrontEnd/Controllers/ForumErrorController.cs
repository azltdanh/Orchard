using Orchard.Themes;
using System.Web.Mvc;

namespace RealEstate.MiniForum.FrontEnd.Controllers
{
    [Themed]
    public class ForumErrorController : Controller
    {

        public ActionResult ForumNotFound()
        {
            return View();
        }

        #region Redirect from RealEstate.ForumFrontEnd

        public ActionResult RedirectMessage()
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectMyPhotoAlbum()
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectViewMyPhotoAlbum(string albumname, int Id)
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectYourPhotoAlbum(string username, int Id)
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectViewYourPhotoAlbum(string photoname, string userId, int albumId)
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectMyListFriend()
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectYourListFriend(string username, int Id)
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectRequestListFriend()
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectForumHomePage()
        {
            return Redirect("/dien-dan/dien-dan-bat-dong-san");
        }
        public ActionResult RedirectFromExternalLink(string url)
        {
            if (string.IsNullOrEmpty(url))
                url = "/";
            ViewBag.Url = url;

            return View();
        }
        #endregion
    }
}