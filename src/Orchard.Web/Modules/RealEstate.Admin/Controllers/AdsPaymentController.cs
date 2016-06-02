using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [Admin]
    public class AdsPaymentController : Controller, IUpdateModel
    {
        private readonly IAdsPaymentHistoryService _adsPaymentService;

        private readonly List<string> _cssClassFromAdmin = new List<string>
        {
            "ins-admin-money",
            "ins-promotion-money",
            "ex-deduction-money",
        };

        private readonly IUserGroupService _groupService;
        private readonly ISiteService _siteService;

        public AdsPaymentController(IShapeFactory shapeFactory, ISiteService siteService, IOrchardServices services,
            IAdsPaymentHistoryService adsPaymentService, IUserGroupService groupService)
        {
            _adsPaymentService = adsPaymentService;
            _groupService = groupService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            _siteService = siteService;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties,
            string[] excludeProperties)
        {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public ActionResult Index(AdsPaymentOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageAdsPaymentHistory,
                    T("Không có quyền Quản lý lịch sử giao dịch")))
                return new HttpUnauthorizedResult();

            #region Init Options

            options.ListAdsPaymentConfig = _adsPaymentService.GetPaymentConfigs();
            //options.ListUsers = _groupService.GetUsers();

            #endregion

            #region Filter

            IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> list = _adsPaymentService.Search(options);

            long totalTemp = 0;
            if (options.TypeVipCssClass == "ins-money"){
                totalTemp = list.List().Sum(r => r.TransactionValue);
            }

            #endregion

            #region Slice

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<AdsPaymentHistoryPart> results =
                list.OrderByDescending(r => r.DateTrading).Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            #endregion

            #region build Model

            var model = new AdsPaymentIndexViewModel
            {
                AdsPaymentEntry = results.Select(r => new AdsPaymentEntry
                {
                    AdsPaymentHistory = r.Record
                }).ToList(),
                AmountTotalVND = totalTemp != 0 ? _adsPaymentService.ConvertoVND(totalTemp) : _adsPaymentService.TotalAmountVND(),
                Options = options,
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        public ActionResult AddPayment(string paymentCssClass)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsPaymentHistory, T("Không có quyền Quản lý")))
                return new HttpUnauthorizedResult();

            var payment = Services.ContentManager.New<AdsPaymentHistoryPart>("AdsPaymentHistory");

            dynamic model = Services.ContentManager.BuildEditor(payment);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentHistory.Create",
                Model: new AdsPaymentCreate
                {
                    //Users = _groupService.GetUsers(),
                    PaymentCssClass = paymentCssClass
                }, Prefix: null);

            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("AddPayment")]
        public ActionResult AddPaymentPost(AdsPaymentCreate createModel)
        {
            if (!Services.Authorizer.Authorize(Permissions.ManageAdsPaymentHistory, T("Không có quyền Quản lý")))
                return new HttpUnauthorizedResult();

            var ads = Services.ContentManager.New<AdsPaymentHistoryPart>("AdsPaymentHistory");
            UserPart userSelect = _groupService.GetUser(createModel.ToUserId);

            if (string.IsNullOrEmpty(createModel.PaymentCssClass) ||
                !_cssClassFromAdmin.Contains(createModel.PaymentCssClass))
                createModel.PaymentCssClass = "ins-admin-money";

            AdsPaymentHistoryPart adsNewest =
                Services.ContentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                    .Where(r => r.User.Id == userSelect.Id)
                    .OrderByDescending(r => r.DateTrading)
                    .Slice(1)
                    .FirstOrDefault();
            if (ModelState.IsValid)
            {
                long results;
                if (!long.TryParse(createModel.Amount.ToString(CultureInfo.InvariantCulture), out results))
                    AddModelError("AmountNotNumber", T("Số tiền phải là 1 số nguyên lớn 0. VD: 500000"));
                else if (createModel.PaymentCssClass == "ex-deduction-money" && results > adsNewest.EndBalance)
                    AddModelError("AmountNotDeduction",
                        T("Số tiền quá lớn ko thể trừ. Số tiền trong tài khoản là: {0}", adsNewest.EndBalance));
            }

            //Services.Notifier.Information(T("UserName: {0} - EndBalance: {1}", userSelect.UserName, adsNewest != null ?  adsNewest.EndBalance : 00));

            #region CreateRecord

            if (ModelState.IsValid)
            {
                long createAmount = 0;
                AdsPaymentConfigPartRecord paymentConfig =
                    _adsPaymentService.GetPaymentConfigByCssClass(createModel.PaymentCssClass);
                var currentUser = Services.WorkContext.CurrentUser.As<UserPart>();

                if (createModel.PaymentCssClass == "ins-admin-money") //Admin nạp tiền
                    createAmount = createModel.Amount;
                else if (createModel.PaymentCssClass == "ex-deduction-money")
                    createAmount = (-1)*createModel.Amount;
                else if (createModel.PaymentCssClass == "ins-promotion-money") //Khuyến mãi
                    createAmount = (createModel.Amount*_adsPaymentService.GetPaymentConfigValue("ins-promotion-money"))/
                                   100;

                ads.User = userSelect.Record;
                ads.UserPerform = currentUser.Record;
                ads.StartBalance = adsNewest != null ? adsNewest.EndBalance : 0;
                ads.EndBalance = adsNewest != null ? adsNewest.EndBalance + createAmount : createAmount;
                ads.TransactionValue = createAmount;
                ads.Property = null;
                ads.PaymentConfig = paymentConfig;
                ads.DateTrading = DateTime.Now;
                ads.PayStatus = true;
                ads.Note = createModel.Note;

                Services.ContentManager.Create(ads);
            }

            dynamic model = Services.ContentManager.UpdateEditor(ads, this);

            #endregion

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();
                //createModel.Users = _groupService.GetUsers();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentHistory.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            #endregion

            Services.Notifier.Information(T("Đã thêm giao dịch thành công!"));

            return RedirectToAction("Index");
        }

        #region Management Config Payment

        public ActionResult PaymentConfigIndex(PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageAdsPaymentConfig,
                    T("Không có quyền Config giá đăng tin VIP")))
                return new HttpUnauthorizedResult();

            List<AdsPaymentConfigPartRecord> list = _adsPaymentService.GetPaymentConfigs().ToList();

            #region Slice

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            int totalCount = list.Count();
            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(totalCount);

            List<AdsPaymentConfigPartRecord> results = list.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            #endregion

            #region build Model

            var model = new AdsPaymentConfigIndexViewModel
            {
                AdsPaymentConfigs = results.Select(r => new PaymentConfigEntry
                {
                    AdsPaymentConfig = r
                }).ToList(),
                Pager = pagerShape,
                TotalCount = totalCount
            };

            #endregion

            #region ROUTE DATA

            var routeData = new RouteData();

            pagerShape.RouteData(routeData);

            #endregion

            return View(model);
        }

        public ActionResult PaymentConfigCreate()
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageAdsPaymentConfig,
                    T("Không có quyền Config giá đăng tin VIP")))
                return new HttpUnauthorizedResult();

            var adsPayment = Services.ContentManager.New<AdsPaymentConfigPart>("AdsPaymentConfig");
            dynamic model = Services.ContentManager.BuildEditor(adsPayment);

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentConfigPart.Create",
                Model: new PaymentConfigCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost]
        public ActionResult PaymentConfigCreate(PaymentConfigCreateViewModel createModel)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageAdsPaymentConfig,
                    T("Không có quyền Config giá đăng tin VIP")))
                return new HttpUnauthorizedResult();

            var adsPayment = Services.ContentManager.New<AdsPaymentConfigPart>("AdsPaymentConfig");

            if (ModelState.IsValid)
            {
                adsPayment.Name = createModel.Name.Trim();
                adsPayment.CssClass = createModel.CssClass.Trim();
                adsPayment.Description = createModel.Description;
                adsPayment.Value = createModel.Value;
                adsPayment.VipValue = 0;
                adsPayment.IsEnabled = createModel.IsEnabled;

                Services.ContentManager.Create(adsPayment);

                Services.Notifier.Information(T("Giá trị: <a href=\"{0}\">{1}</a> đã được thêm.",
// ReSharper disable once Mvc.AreaNotResolved
                    Url.Action("PaymentConfigEdit", "AdsPayment", new {area = "RealEstate.Admin", adsPayment.Id}),
                    adsPayment.Name));
            }

            if (!ModelState.IsValid)
            {
                dynamic model = Services.ContentManager.BuildEditor(adsPayment);

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentConfigPart.Create", Model: createModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                return View((object) model);
            }

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return RedirectToAction("PaymentConfigIndex");
        }

        public ActionResult PaymentConfigEdit(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageAdsPaymentConfig,
                    T("Không có quyền Config giá đăng tin VIP")))
                return new HttpUnauthorizedResult();

            var payment = Services.ContentManager.Get<AdsPaymentConfigPart>(id);

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/PaymentConfigPart.Edit",
                Model: new PaymentConfigEditViewModel
                {
                    PaymentConfig = payment,
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(payment);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("PaymentConfigEdit")]
        public ActionResult PaymentConfigEditPost(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManageAdsPaymentConfig,
                    T("Không có quyền Config giá đăng tin VIP")))
                return new HttpUnauthorizedResult();

            var payment = Services.ContentManager.Get<AdsPaymentConfigPart>(id);

            dynamic model = Services.ContentManager.UpdateEditor(payment, this);

            var editModel = new PaymentConfigEditViewModel {PaymentConfig = payment};

            TryUpdateModel(editModel); //Update

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/PaymentConfigPart.Edit", Model: editModel,
                    Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            Services.Notifier.Information(T("Nội dung <a href='{0}'>{1}</a> đã được cập nhật.",
                Url.Action("PaymentConfigEdit", new {payment.Id}), payment.Name));

            return RedirectToAction("PaymentConfigIndex");
        }

        #endregion
    }
}