using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Mvc;
using Orchard.ContentManagement;
//using Orchard.Core.Contents.Controllers;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.NewLetter.Models;
using RealEstate.NewLetter.Services;
using RealEstate.NewLetter.ViewModels;

namespace RealEstate.NewLetter.Controllers
{
    [ValidateInput(false), Admin]
    public class ListCustomerController : Controller, IUpdateModel
    {
        #region Init

        private readonly INewCustomerService _newcustomerservice;
        private readonly INewletterMessageService _newletterservice;
        private readonly ISiteService _siteService;

        public ListCustomerController(INewletterMessageService newletterservice, INewCustomerService newcustomerservice,
            IOrchardServices services, IShapeFactory shapeFactory, ISiteService siteService)
        {
            Services = services;
            _siteService = siteService;
            _newcustomerservice = newcustomerservice;
            _newletterservice = newletterservice;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion

        public ActionResult Index(ListCustomerLetterIndexOptions options, PagerParameters pagerParameters)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new ListCustomerLetterIndexOptions();

            IContentQuery<CustomerPart, CustomerPartRecord> customer = Services.ContentManager
                .Query<CustomerPart, CustomerPartRecord>()
                .Where(c => c.ContactEmail != null)
                .OrderByDescending(c => c.Id);

            switch (options.Filter)
            {
                case ListCustomerLetterFilter.All:
                    //propertyLocations = propertyLocations.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                customer =
                    customer.Where(
                        u =>
                            u.ContactEmail.Contains(options.Search) || u.ContactPhone.Contains(options.Search) ||
                            u.ContactName.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(customer.Count());


            List<CustomerPart> results = customer
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new ListCustomerLetterIndexViewModel
            {
                ListCustomerLetters = results
                    .Select(x => new ListCustomerLetterEntry
                    {
                        ListCustomerLetter = x.Record,
                        _count = _newcustomerservice.PropertiesPartCount(x.Id),
                        StatusException = _newcustomerservice.CheckCustomerEmailExceptionTrue(x.ContactEmail)
                    })
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
                return new HttpUnauthorizedResult();

            var viewModel = new ListCustomerLetterIndexViewModel
            {
                ListCustomerLetters = new List<ListCustomerLetterEntry>(),
                Options = new ListCustomerLetterIndexOptions()
            };
            UpdateModel(viewModel);

            List<ListCustomerLetterEntry> checkedEntries =
                viewModel.ListCustomerLetters.Where(c => c.IsChecked).ToList();
            switch (viewModel.Options.BulkAction)
            {
                case ListCustomerLetterBulkAction.None:
                    break;
                case ListCustomerLetterBulkAction.SendMail:
                    //1. Lay ra danh sach cac id customer, email customer
                    var listcustomerId = new List<int>();
                    var listcustomerEmail1 = new List<string>();

                    //2. 
                    foreach (ListCustomerLetterEntry entry in checkedEntries)
                    {
                        listcustomerId.Add(entry.ListCustomerLetter.Id);
                        listcustomerEmail1.Add(
                            _newcustomerservice.ICustomerPartById(entry.ListCustomerLetter.Id).ContactEmail);
                    }
                    var model = new ListCustomerLetterSendMailViewModel
                    {
                        listcustomerEmail = listcustomerEmail1,
                        listcustomerId = listcustomerId,
                        link =
                            "http://dinhgianhadat.vn" +
                            Url.Action("ResultFilter", "PropertySearch", new {area = "RealEstate.FrontEnd"})
                    };
                    return View("SendMail", model);
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        [FormValueRequired("submit.BulkSend")]
        public ActionResult Index(ListCustomerLetterSendMailViewModel viewmodel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Bạn không có quyền")))
                return new HttpUnauthorizedResult();

            string[] lst = viewmodel._lstcustomerId.Split(',');
            var lstemailext = new List<string>();

            for (int i = 0; i < lst.Length - 1; i++)
            {
                // Get Email
                string _EmailException = "";
                //Tao code de xac nhan từ chối nhan mail
                var random = new Random();
                string codeDenied = random.Next().ToString();

                _EmailException = _newcustomerservice.ICustomerPartById(int.Parse(lst[i])).ContactEmail;

                if (!_newcustomerservice.CheckCustomerEmailExceptionTrue(_EmailException))
                {
                    #region check & send email

                    // Kiem tra xem email da co trong table loai tru hay chua
                    if (!_newcustomerservice.CheckCustomerEmailException(_EmailException))
                    {
                        //
                        if (i > 0)
                        {
                            if (!lstemailext.Any(c => c.Contains(_EmailException)))
                            {
                                //them code vao table loai tru
                                var CException =
                                    Services.ContentManager.New<CustomerEmailExceptionPart>("CustomerEmailException");
                                CException.CodeRandom = codeDenied;
                                CException.EmailException = _EmailException;
                                CException.StatusActive = false; //trang thai fale la chua xac nhan da loai tru
                                Services.ContentManager.Create(CException);
                            }
                        }
                        else
                        {
                            //them code vao table loai tru
                            var CException =
                                Services.ContentManager.New<CustomerEmailExceptionPart>("CustomerEmailException");
                            CException.CodeRandom = codeDenied;
                            CException.EmailException = _EmailException;
                            CException.StatusActive = false; //trang thai fale la chua xac nhan da loai tru
                            Services.ContentManager.Create(CException);
                        }
                    }
                    else
                    {
                        codeDenied =
                            Services.ContentManager.Query<CustomerEmailExceptionPart, CustomerEmailExceptionPartRecord>()
                                .Where(c => c.EmailException == _EmailException)
                                .List()
                                .FirstOrDefault()
                                .CodeRandom;
                    }

                    lstemailext.Add(_EmailException); // de xet xem da email da co hay chua

                    // Tim kiem ket qua
                    List<PropertyPart> _lstproperty = _newcustomerservice.PropertiesPart(int.Parse(lst[i]));
                    List<InfomationAddressViewModel> _listLink =
                        _newcustomerservice.PropertiesPartInfo(int.Parse(lst[i]));


                    //var model = new PropertyDisplayIndexViewModelFront
                    //{
                    //    PropertiesNew = _lstproperty.Select
                    //    (
                    //        x => new PropertyDisplayEntry { Property = x }
                    //    ).ToList()
                    //};


                    if (_lstproperty != null)
                    {
                        var _link = new List<string>();
                        foreach (InfomationAddressViewModel n in _listLink)
                        {
                            _link.Add("http://dinhgianhadat.vn" + Url.Action("ResultFilter", "PropertySearch", new
                            {
                                area = "RealEstate.FrontEnd",
                                btSubmit = "SearchAction",
                                ProvinceId = n.PropertyId,
                                DistrictIds = n.DistrictId,
                                WardIds = n.WardId,
                                StreetIds = n.StreetId,
                                AdsTypeId = n.AdsType,
                                n.TypeGroupId,
                                DirectionIds = n.DistrictId,
                                n.OrderWalk,
                                n.OrderApartmentFloorTh,
                                n.PaymentMethodId,
                                n.MinAreaTotal,
                                n.MinAreaUsable,
                                n.MinAreaTotalWidth,
                                n.MinAreaTotalLength,
                                n.MinFloors,
                                n.MinPriceProposed,
                                n.MaxPriceProposed,
                                btSearch = "Tìm + Kiếm"
                            }));
                        }

                        string codeUrl = "http://dinhgianhadat.vn/" +
                                         Url.Action("VerifyExeptionEmail", "VerifyCustomerException",
                                             new {code = codeDenied});

                        _newletterservice.NewLetterList(
                            _newcustomerservice.ICustomerPartById(int.Parse(lst[i])).ContactEmail, _link,
                            viewmodel.MailContent, viewmodel.MailTitle, codeUrl);
                    }

                    #endregion
                }
            }


            Services.Notifier.Information(T("Đã gửi email thành công! {0}", (lst.Length - 1)));
            //return RedirectToAction("Index", ControllerContext.RouteData.Values);
            return RedirectToAction("Index");
        }

        #region UpdateModel

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        #endregion
    }
}