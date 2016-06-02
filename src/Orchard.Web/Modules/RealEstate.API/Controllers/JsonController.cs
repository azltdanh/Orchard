using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using System.Web;
using Orchard.Security;
using RealEstate.Models;
using RealEstate.Services;

namespace RealEstate.API.Controllers
{
    [AllowCrossSiteJsonAttribute]
    public class JsonController : CrossSiteController
    {
        private readonly IMembershipService _membershipService;
        private readonly IPropertyService _propertyService;
        private readonly IAddressService _addressService;


        public JsonController(IOrchardServices services, IPropertyService propertyService, IAddressService addressService, IMembershipService membershipService)
        {
            Services = services;
            _propertyService = propertyService;
            _addressService = addressService;
            _membershipService = membershipService;
        }
        public IOrchardServices Services { get; set; }

        // GET: Json
        //[HttpPost]
        public JsonResult Getjson(int id)
        {
            try
            {
                //var postForum = Services.ContentManager.New<ForumPostPart>("ForumPost");

                //if (ModelState.IsValid)
                //{
                //    postForum.Thread = null;
                //    postForum.Title = "Title";

                //    postForum.Content = "Content";

                //    postForum.UserPost = null;
                //    postForum.DateCreated = DateTime.Now;
                //    postForum.DateUpdated = DateTime.Now;
                //    postForum.PublishStatus = null;
                //    postForum.StatusPost = null;
                //    postForum.Views = 0;

                //    //Share blog
                //    postForum.IsShareBlog = false;
                //    postForum.Blog = null;
                //    postForum.BlogDateCreated = DateTime.Now;
                //    postForum.HostName = "";

                //    Services.ContentManager.Create(postForum);
                //}

                HttpCookie myCookie = new HttpCookie("MyTestCookie");
                DateTime now = DateTime.Now;

                // Set the cookie value.
                myCookie.Value = now.ToString();
                // Set the cookie expiration date.
                myCookie.Expires = now.AddMinutes(1);

                // Add the cookie.
                Response.Cookies.Add(myCookie);

                return Json(new { status = true, id = 3, cookie = Request.Cookies.Get("MyTestCookie").Value }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex){
                return Json(new { status = true, id = 4, str = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Login(string userName, string password)
        {
            try
            {
                var valid = ValidateLogOn(userName, password);

                return Json(new { userName = userName, temp = valid != null ? valid.Email : "null roi" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }

        private IUser ValidateLogOn(string userNameOrEmail, string password)
        {
            bool validate = true;

            if (String.IsNullOrEmpty(userNameOrEmail))
            {
                //ModelState.AddModelError("userNameOrEmail", T("You must specify a username or e-mail."));
                //validate = false;
            }
            if (String.IsNullOrEmpty(password))
            {
                //ModelState.AddModelError("password", T("You must specify a password."));
                //validate = false;
            }

            if (!validate)
                return null;

            var user = _membershipService.ValidateUser(userNameOrEmail, password);
            if (user == null)
            {
                //ModelState.AddModelError("_FORM", T("The username or e-mail or password provided is incorrect."));
            }

            return user;
        }

        public JsonResult GetApartmentInfoForJson(int apartmentId, int propertyId = 0)
        {

            var apartment = Services.ContentManager.Get<LocationApartmentPart>(apartmentId);
            var property = Services.ContentManager.Get<PropertyPart>(propertyId);

            if (apartment == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            List<string> advantageCssClass =
                _propertyService.GetAdvantagesForApartment(apartment)
                    .Where(a => a.ShortName == "apartment-adv")
                    .Select(a => a.CssClass).ToList();

            var avatarImage = _addressService.GetApartmentFiles(apartment, property).List()
                .Select(r => new { r.Path, r.Id, r.IsAvatar }).ToList();

            string note = "";
            if (!string.IsNullOrEmpty(apartment.Investors)) note += "\n- Chủ đầu tư: " + apartment.Investors;
            if (!string.IsNullOrEmpty(apartment.CurrentBuildingStatus))
                note += "\n- Hiện trạng: " + apartment.CurrentBuildingStatus;
            if (!string.IsNullOrEmpty(apartment.ManagementFees))
                note += "\n- Phí quản lý: " + apartment.ManagementFees;
            if (apartment.AreaTotal.HasValue) note += "\n- Diện tích khuôn viên: " + apartment.AreaTotal;
            if (apartment.AreaConstruction.HasValue)
                note += "\n- Diện tích sàn xây dựng: " + apartment.AreaConstruction;
            if (apartment.AreaGreen.HasValue) note += "\n- Diện tích công viên cây xanh: " + apartment.AreaGreen;
            if (apartment.AreaTradeFloors.HasValue)
                note += "\n- Diện tích sàn thương mại: " + apartment.AreaTradeFloors;
            if (apartment.AreaBasements.HasValue) note += "\n- Diện tích tầng hầm: " + apartment.AreaBasements;

            return Json(new
            {
                success = true,
                apartmentWardId = apartment.Ward != null ? apartment.Ward.Id : 0,
                apartmentStreetId = apartment.Street != null ? apartment.Street.Id : 0,
                apartmentAddressNumber = apartment.AddressNumber,
                apartmentList = avatarImage,
                apartmentFloors = apartment.Floors,
                apartmentTradeFloors = apartment.TradeFloors,
                apartmentElevators = apartment.Elevators,
                apartmentBasements = apartment.Basements,
                apartmentDescription = apartment.Description + note,
                apartmentHaveChildcare = advantageCssClass.Contains("apartment-adv-Childcare"),
                apartmentHavePark = advantageCssClass.Contains("apartment-adv-Park"),
                apartmentHaveSwimmingPool = advantageCssClass.Contains("apartment-adv-SwimmingPool"),
                apartmentHaveSuperMarket = advantageCssClass.Contains("apartment-adv-SuperMarket"),
                apartmentHaveSportCenter = advantageCssClass.Contains("apartment-adv-SportCenter"),
                apartmentHaveHospital = advantageCssClass.Contains("apartment-adv-Hospital"),
                apartmentHavePublicArea = advantageCssClass.Contains("apartment-adv-PublicArea"),
            }, JsonRequestBehavior.AllowGet);
        }

        //public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
        //{
        //    public override void OnActionExecuting(ActionExecutingContext filterContext)
        //    {
        //        filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "http://localhost:65290");
        //        filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
        //        filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
        //        filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Max-Age", "1728000");
        //        //filterContext.RequestContext.HttpContext.Response.End();
        //        base.OnActionExecuting(filterContext);
        //    }
        //}
    }
}