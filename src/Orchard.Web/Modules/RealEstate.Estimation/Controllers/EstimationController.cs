using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Transactions;
using System.Web.Mvc;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using RealEstate.Estimation.Models;
using RealEstate.Estimation.ViewModels;
using RealEstate.Models;
using RealEstate.Services;

namespace RealEstate.Estimation.Controllers
{
    [Themed]
    public class EstimationController : Controller
    {
        private const int Batch = 1;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRepository<EstimateRecord> _estimateRepository;
        private readonly IEstimationService _estimationService;
        private readonly IMembershipService _membershipService;

        private readonly IPropertySettingService _settingService;

        private readonly ISignals _signals;

        private readonly ISiteService _siteService;

        public EstimationController(
            IAuthenticationService authenticationService,
            IMembershipService membershipService,
            IEstimationService estimationService,
            IPropertySettingService settingService,
            IRepository<EstimateRecord> estimateRepository,
            ISignals signals,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IOrchardServices services)
        {
            _authenticationService = authenticationService;
            _membershipService = membershipService;

            _settingService = settingService;
            _estimationService = estimationService;

            _estimateRepository = estimateRepository;

            _signals = signals;

            _siteService = siteService;

            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        [ValidateInput(false), Admin]
        public ActionResult Index(EstimateIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(StandardPermissions.SiteOwner,
                    T("Not authorized to list Estimation History")))
                return new HttpUnauthorizedResult();

            //RealEstate.Estimation.Services.ScheduledTask.

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new EstimateIndexOptions();

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(_estimateRepository.Count(a => a.Id > 0));

            List<EstimateRecord> list =
                _estimateRepository.Fetch(a => a.Id > 0, o => o.Desc(f => f.Id), (pager.Page - 1)*pager.PageSize,
                    pager.PageSize).ToList();

            //List<EstimateRecord> results = list.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            //int pListCount = _estimationService.GetPropertyList("gp-house", null, null, null, null, true, true, false)
            //    .OrderBy(a => a.District.SeqOrder)
            //    .ThenBy(a => a.Ward.SeqOrder)
            //    .ThenBy(a => a.Street.Name)
            //    .ThenBy(a => a.AlleyNumber)
            //    .Count();

            int pListCount = 0;

            var model = new EstimateIndexViewModel
            {
                Estimates = list,
                Options = options,
                Pager = pagerShape,
                PropertiesCount = pListCount
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult RunEstimate(int startIndex)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
                throw new AuthenticationException("");

            List<PropertyPart> pList =
                _estimationService.GetPropertyList("gp-house", null, null, null, null, true, true, false)
                    .OrderBy(a => a.District.SeqOrder)
                    .ThenBy(a => a.Ward.SeqOrder)
                    .ThenBy(a => a.Street.Name)
                    .ThenBy(a => a.AlleyNumber)
                    .Skip(startIndex).Take(Batch)
                    .ToList();

            string msg = "";


            foreach (PropertyPart item in pList)
            {
                try
                {
                    DateTime startEstimateTime = DateTime.Now;

                    item.PriceEstimatedInVND = _estimationService.EstimatePrice(item, false);

                    msg = String.Format("Định giá BĐS {0} - {1} --> {2:#,0.### Tỷ} Take {3}", item.Id,
                        item.DisplayForAddressForOwner, item.PriceEstimatedInVND,
                        (DateTime.Now - startEstimateTime).TotalSeconds);
                }
                catch (Exception e)
                {
                    msg = String.Format("Định giá BĐS {0} - Error: {1}", item.Id, e.Message);
                }
            }


            return new JsonResult {Data = new {data = startIndex + Batch, msg}};
        }


        public ActionResult EstimateAllProperties()
        {
            // 112.78.4.125
            if (Request.UserHostAddress == "112.78.1.50" || Request.UserHostAddress == "112.78.1.47" ||
                Request.UserHostAddress == "127.0.0.1")
            {
                IUser user = _membershipService.GetUser("admin");
                _authenticationService.SignIn(user, false);

                #region Update Status

                DateTime current = DateTime.Now;

                // Update AdsContent Expired
                try
                {
                    IEnumerable<PropertyPart> adsExpiredProperties =
                        Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                            .Where(a =>
                                (a.AdsHighlight && a.AdsHighlightExpirationDate < current) ||
                                (a.AdsGoodDeal && a.AdsGoodDealExpirationDate < current) ||
                                (a.AdsVIP && a.AdsVIPExpirationDate < current) ||
                                (a.AdsVIP == false && a.SeqOrder > 0)
                            )
                            .List();

                    foreach (PropertyPart item in adsExpiredProperties)
                    {
                        if (item.AdsHighlightExpirationDate < current) item.AdsHighlight = false;
                        if (item.AdsGoodDealExpirationDate < current) item.AdsGoodDeal = false;
                        if (item.AdsVIPExpirationDate < current)
                        {
                            item.AdsVIP = false;
                            item.SeqOrder = 0;
                        }
                        if (item.AdsVIP == false && item.SeqOrder > 0)
                        {
                            item.SeqOrder = 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Services.Notifier.Error(T("{0}", e.Message));
                }

                DateTime dateToUpdateNegotiateStatus =
                    DateTime.Now.AddDays(-(int.Parse(_settingService.GetSetting("DaysToUpdateNegotiateStatus") ?? "7")));
                int pnegotiateStatusId =
                    Services.ContentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                        .Where(a => a.CssClass == "st-negotiate")
                        .List()
                        .First()
                        .Id;

                // Update negotiate Properties
                try
                {
                    IEnumerable<PropertyPart> properties =
                        Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                            .Where(
                                a =>
                                    a.Status.Id == pnegotiateStatusId &&
                                    ((a.StatusChangedDate != null && a.StatusChangedDate < dateToUpdateNegotiateStatus) ||
                                     a.LastUpdatedDate < dateToUpdateNegotiateStatus)).List();

                    PropertyStatusPartRecord psellingStatus =
                        Services.ContentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                            .Where(a => a.CssClass == "st-selling")
                            .List()
                            .First()
                            .Record;
                    foreach (PropertyPart item in properties)
                    {
                        item.Status = psellingStatus;
                        item.StatusChangedDate = DateTime.Now;
                    }
                }
                catch (Exception e)
                {
                    Services.Notifier.Error(T("{0}", e.Message));
                }

                // Update negotiate Customers
                try
                {
                    int cnegotiateStatusId =
                        Services.ContentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>()
                            .Where(a => a.CssClass == "st-negotiate")
                            .List()
                            .First()
                            .Id;

                    IEnumerable<CustomerPart> customers =
                        Services.ContentManager.Query<CustomerPart, CustomerPartRecord>()
                            .Where(
                                a =>
                                    a.Status.Id == cnegotiateStatusId &&
                                    ((a.StatusChangedDate != null && a.StatusChangedDate < dateToUpdateNegotiateStatus) ||
                                     a.LastUpdatedDate < dateToUpdateNegotiateStatus)).List();

                    CustomerStatusPartRecord cStatusNew =
                        Services.ContentManager.Query<CustomerStatusPart, CustomerStatusPartRecord>()
                            .Where(a => a.CssClass == "st-new")
                            .List()
                            .First()
                            .Record;
                    foreach (CustomerPart item in customers)
                    {
                        bool haveNegotiateProperties =
                            item.Properties.Any(
                                a =>
                                    a.Status.Id == pnegotiateStatusId &&
                                    ((a.StatusChangedDate != null && a.StatusChangedDate < dateToUpdateNegotiateStatus) ||
                                     a.LastUpdatedDate < dateToUpdateNegotiateStatus));
                        if (!haveNegotiateProperties) item.Status = cStatusNew;
                    }
                }
                catch (Exception e)
                {
                    Services.Notifier.Error(T("{0}", e.Message));
                }

                #endregion

                // Start Estimation
                //return RedirectToAction("EstimateAllStart");
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        #region ESTIMATE ALL PROPERTIES

        public ActionResult EstimateAllStart()
        {
            if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
            {
                // Clear Cache
                //HttpContext.Application.Clear();
                IDictionaryEnumerator iDenum = HttpContext.Cache.GetEnumerator();
                while (iDenum.MoveNext())
                {
                    HttpContext.Cache.Remove(iDenum.Key.ToString());
                }

                List<PropertyPart> pList =
                    _estimationService.GetPropertyList("gp-house", null, null, null, null, true, true, false)
                        .OrderBy(a => a.District.SeqOrder)
                        .ThenBy(a => a.Ward.SeqOrder)
                        .ThenBy(a => a.Street.Name)
                        .ThenBy(a => a.AlleyNumber)
                        .ToList();

                //var model = new EstimateViewModel() { Properties = pList, TotalItems = pList.Count, StartIndex = 0, StartTime = DateTime.Now, Messages = new List<string>() };

                var record = new EstimateRecord
                {
                    StartTime = DateTime.Now,
                    TotalItems = pList.Count,
                    SucessItems = 0,
                    Msg = "Start"
                };
                _estimateRepository.Create(record);

                return RedirectToAction("EstimateAll",
                    new
                    {
                        totalItems = record.TotalItems,
                        sucessItems = record.SucessItems,
                        startIndex = 0,
                        startTime = record.StartTime
                    });
            }

            return RedirectToAction("Index");
        }

        public ActionResult EstimateAll(int totalItems, int sucessItems, int startIndex, DateTime startTime)
        {
            if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
            {
                DateTime startEstimateAllTime = DateTime.Now;
                using (new TransactionScope(TransactionScopeOption.Suppress, new TimeSpan(5, 0, 0)))
                {
                    int sucessCount = 0;
                    var errorIds = new List<int>();

                    List<PropertyPart> pList =
                        _estimationService.GetPropertyList("gp-house", null, null, null, null, true, true, false)
                            .OrderBy(a => a.District.SeqOrder)
                            .ThenBy(a => a.Ward.SeqOrder)
                            .ThenBy(a => a.Street.Name)
                            .ThenBy(a => a.AlleyNumber)
                            .Skip(startIndex).Take(100)
                            .ToList();

                    if (pList.Count > 0)
                    {
                        foreach (PropertyPart item in pList)
                        {
                            try
                            {
                                DateTime startEstimateTime = DateTime.Now;
                                item.PriceEstimatedInVND = _estimationService.EstimatePrice(item, false);
                                if (item.PriceEstimatedInVND != null && item.PriceEstimatedInVND > 0)
                                {
                                    sucessCount++;
                                }
                                Services.Notifier.Information(T("Định giá BĐS {0} - {1} Take {2}", item.Id,
                                    item.DisplayForAddressForOwner, (DateTime.Now - startEstimateTime).TotalSeconds));
                            }
                            catch (Exception e)
                            {
                                errorIds.Add(item.Id);
                                Services.Notifier.Error(T("Định giá BĐS {0} - {1} Error: {2}", item.Id,
                                    item.DisplayForAddressForOwner, e.Message));
                            }
                        }

                        DateTime endTime = DateTime.Now;
                        TimeSpan duration = endTime - startEstimateAllTime;

                        LocalizedString msg =
                            T("Định giá {0}/{1} StartIndex {2} TotalItems {3} BĐS StartTime {4} EndTime {5} Take {6}",
                                sucessCount, pList.Count, startIndex, totalItems, startEstimateAllTime, endTime,
                                string.Format("{0:00.## phút}", duration.TotalMinutes));

                        Services.Notifier.Information(msg);

                        startIndex += 100;
                        sucessItems += sucessCount;

                        string returnUrl = Url.Action("EstimateAll",
                            new {totalItems, sucessItems, startIndex, startTime});
                        var model = new EstimateViewModel
                        {
                            TotalItems = totalItems,
                            StartIndex = startIndex,
                            StartTime = startTime,
                            ReturnUrl = returnUrl
                        };

                        EstimateRecord record = _estimateRepository.Fetch(r => r.StartTime == startTime).First();
                        if (record != null)
                        {
                            record.EndTime = endTime;
                            record.SucessItems = sucessItems;
                            record.Msg = msg.ToString();
                            if (errorIds.Count > 0)
                                record.ErrorMsg += (String.IsNullOrEmpty(record.ErrorMsg) ? "" : ",") +
                                                   String.Join(",", errorIds);

                            _estimateRepository.Update(record);
                        }

                        return View("EstimateAll", model);
                    }
                    return RedirectToAction("EstimateAllEnd", new {totalItems, sucessItems, startIndex, startTime});
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult EstimateAllEnd(int totalItems, int sucessItems, int startIndex, DateTime startTime)
        {
            if (Services.Authorizer.Authorize(StandardPermissions.SiteOwner))
            {
                // Clear Cache
                string cacheKey = "EstimateProperties_" + String.Join("_", null, null, null, true, true);
                _signals.Trigger(cacheKey + "_Expired");

                // SignOut
                _authenticationService.SignOut();

                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;
                LocalizedString msg = T("Định giá {0}/{1} BĐS Start {2} End {3} Take {4}", sucessItems, totalItems,
                    startTime, endTime, string.Format("{0:00.## phút}", duration.TotalMinutes));

                EstimateRecord record = _estimateRepository.Fetch(r => r.StartTime == startTime).First();
                if (record != null)
                {
                    record.EndTime = endTime;
                    record.SucessItems = sucessItems;
                    record.Msg = msg.ToString();

                    _estimateRepository.Update(record);
                }

                Services.Notifier.Information(msg);

                return View();
            }

            return RedirectToAction("Index");
        }

        #endregion
    }
}