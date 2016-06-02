using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.ViewModels;

namespace RealEstate.Services
{
    public interface IAdsPaymentHistoryService : IDependency
    {
        IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> Search(AdsPaymentOptions options);
        AdsPaymentConfigPartRecord GetPaymentConfigByCssClass(string cssClass);
        int GetPaymentConfigValue(string cssClass);
        AdsPaymentConfigPartRecord GetPaymentConfigByVipValue(int vipValue);
        IEnumerable<AdsPaymentConfigPartRecord> GetPaymentConfigs();
        IEnumerable<AdsPaymentConfigPartRecord> GetPaymentConfigsAsVip();

        string TotalAmountVND();

        IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> PaymentHistoryFilter(
            AdsPaymentFrontEndOptions options);

        AdsPaymentHistoryPart GetPaymentHistoryLasted(UserPart user);
        AdsPaymentHistoryPart GetPaymentHistoryByProperty(PropertyPart p);

        bool CheckIsHaveMoney(int adsTypeVip, PropertyPart p, int oldSeqOrder, DateTime? oldAdsVipExpirationDate,
            int newDays);

        string ConvertoVND(long number);

        bool UpdatePaymentHistory(string oldStatusCssClass, int oldSeqOrder, bool isRefresh,
            DateTime? oldAdsVipExpirationDate, PropertyPart p, int adsTypeVip, UserPart user, double intDays);

        bool CancelAdsVIPRequest(int oldVip, UserPart user, PropertyPart p);
        void ApprovedPaymentHistory(PropertyPart p);
        void NotApprovedPaymentHistory(PropertyPart p);
        void AddPaymentHistory(PayType paytype, UserPart user, long amount, string note);

        #region New update 01/01/2015

        void UpdatePaymentHistoryV2(string oldStatusCssClass, int oldSeqOrder, //bool isRefresh,
        DateTime? oldAdsVipExpirationDate, PropertyPart p, UserPart user, int newVip, int newDays);
        bool CheckHaveMoney(bool oldVipRequest, int oldVip, DateTime? expireVip, int newVip, int newDays);
        bool CheckIsValidVip(int adsVipType, int districtId, string adsTypeCssClass);
        long CalcPrice(int adsVipType, int days);

        #endregion
    }

    public class AdsPaymentHistoryService : IAdsPaymentHistoryService
    {
        private readonly IContentManager _contentManager;

        private readonly List<string> _paymentCssClass = new List<string>
        {
            "ins-money",
            "ins-admin-money",
            "ins-promotion-money",
            "ex-deduction-money",
        };

        public AdsPaymentHistoryService
            (
            IOrchardServices services,
            IContentManager contentManager
            )
        {
            _contentManager = contentManager;


            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region New update 01/01/2015

        public long CalcPrice(int adsVipType, int days)
        {
            //If là tin thường
            //if (adsVipType == 0) return 1000 * days;// adsVipType = 4;//Tin thường có giá trị Vip Value = 4

            var record = GetPaymentConfigByVipValue(adsVipType);
            return record.Value * days;
        }

        public bool CheckIsValidVip(int adsVipType, int districtId, string adsTypeCssClass)
        {
            //Tin VIP
            if (adsVipType > 0) return true;

            //Tin Thường trong các quận trung tâm:
            //var districtAllowed = new List<int> { 313,315,317,322,323,328,329,326,330 };//1,3,5,10,11,PN,TB,BThanh,TPhu

            //Và là tin rao bán thì
            //if (adsVipType == 0 && adsTypeCssClass == "ad-selling" && districtAllowed.Contains(districtId))
                //return true;

            return false;
        }

        //False - 0 - 2015-04-02 23:59:59.000 - 3 - 10
        public bool CheckHaveMoney(bool oldVipRequest, int oldVip, DateTime? expireVip, int newVip, int newDays)
        {
            bool isDebug = false;

            var newPrice = CalcPrice(newVip, newDays);
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            var accountBalance = GetPaymentHistoryLasted(user);

            //Calprice Old
            double oldPrice = 0;
            int dayolds = 0;
            if (expireVip != null && expireVip >= DateTime.Now)
            {
                dayolds = (int)(expireVip.Value - DateTime.Now).TotalDays;

                oldPrice = CalcPrice(oldVip, dayolds);
            }

            if (isDebug)
            {
                Services.Notifier.Information(T("Dayolds: {0} ", dayolds));
                Services.Notifier.Information(T("oldPrice: {0} ", oldPrice));
            }

            if(oldPrice == 0)//Tin mới
            {
                if (accountBalance == null)
                    return false;

                return accountBalance.EndBalance >= newPrice;
            }
            else//Tin đang chờ duyệt(DK: Tin cũ trước chưa phải là tin vip)
            {
                //Nếu số tiền tin trước > tin sau
                if (oldPrice > newPrice)
                    return true;
                else
                {
                    //Nếu số tiền tin trước < tin sau
                    double priceChange = newPrice - oldPrice;
                    return accountBalance.EndBalance >= priceChange;
                }
            }
        }

        public void UpdatePaymentHistoryV2(string oldStatusCssClass, int oldSeqOrder,
            DateTime? oldAdsVipExpirationDate, PropertyPart p, UserPart user, int newVip, int newDays)
        {
            try
            {
                var totalNewPrice = CalcPrice(newVip, newDays);
                var accountBalance = GetPaymentHistoryLasted(user);

                var oldPaymentConfig = GetPaymentConfigByVipValue(oldSeqOrder);//4. Vip 0 (tin thường)
                var newPaymentConfig = GetPaymentConfigByVipValue(newVip);//4. Vip 0 (tin thường)

                //Calprice Old
                long oldPrice = 0;
                if (oldAdsVipExpirationDate != null && oldAdsVipExpirationDate >= DateTime.Now)
                {
                    int dayolds = (int)(oldAdsVipExpirationDate.Value - DateTime.Now).TotalDays;
                    oldPrice = CalcPrice(oldSeqOrder, dayolds);
                }
                //Nếu là tin mới: DK: Tin cũ trước chưa phải là tin vip(Đã kiểm tra ngoài action: !oldIsAdsVip)
                if (oldPrice == 0)//Tin mới
                {
                    //- Tính tiền và trừ ra trong tài khoản & Update PaymentHistory
                    var newAds = Services.ContentManager.New<AdsPaymentHistoryPart>("AdsPaymentHistory");

                    newAds.User = user.Record;
                    newAds.UserPerform = user.Record;
                    newAds.PaymentConfig = newPaymentConfig;
                    newAds.Property = p.Record;
                    newAds.Note = newPaymentConfig.Description;
                    newAds.DateTrading = DateTime.Now;
                    newAds.StartBalance = accountBalance.EndBalance;
                    newAds.EndBalance = accountBalance.EndBalance > 0 ? accountBalance.EndBalance - totalNewPrice : 0;
                    newAds.TransactionValue = totalNewPrice;
                    newAds.PayStatus = false;
                    newAds.IsInternal = false;
                    newAds.PostingDates = (int)newDays;

                    Services.ContentManager.Create(newAds);

                    Services.Notifier.Information(T("BĐS yêu cầu đăng tin {0}", newPaymentConfig.Name));
                }
                else
                {
                    //Nếu là tin đang chờ duyệt: Là tin chưa được duyệt(Đã kiểm tra ngoài action: !oldIsAdsVip)
                    //- Kiểm tra số tiền tin trước và tin sau có chênh lệch ko?
                    var paymentHistory = GetPaymentHistoryByProperty(p);


                    //Nếu số tiền tin trước > tin sau
                    if (oldPrice > totalNewPrice)
                    {
                        paymentHistory.EndBalance = accountBalance.EndBalance + (oldPrice - totalNewPrice);
                    }
                    else
                    {
                        //Nếu số tiền tin trước < tin sau
                        paymentHistory.EndBalance = accountBalance.EndBalance - (totalNewPrice - oldPrice);
                    }

                    string info = T(" - Đã update lịch sử tin {0}, Số ngày: {1} => {2}, Số ngày: {3}",
                                                    oldPaymentConfig.Name, (int)(oldAdsVipExpirationDate.Value - DateTime.Now).TotalDays, newPaymentConfig.Name, newDays).ToString();

                    paymentHistory.Note = newPaymentConfig.Description + info;
                    paymentHistory.PaymentConfig = newPaymentConfig;
                    paymentHistory.DateTrading = DateTime.Now;
                    paymentHistory.PostingDates = (int)newDays;
                    paymentHistory.TransactionValue = totalNewPrice;
                    paymentHistory.IsInternal = false;

                    Services.Notifier.Information(T(info));

                }
            }catch(Exception ex)
            {
                Services.Notifier.Information(T("Error: {0}",ex.Message));
            }
        }

        #endregion

        public IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> Search(AdsPaymentOptions options)
        {
            IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> list =
                _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>();

            list = options.IsInternal ? list.Where(r => r.IsInternal) : list.Where(r => r.IsInternal != true);

            if (!string.IsNullOrEmpty(options.TypeVipCssClass))
                list = list.Where(r => r.PaymentConfig == GetPaymentConfigByCssClass(options.TypeVipCssClass));

            if (options.UserId.HasValue) list = list.Where(r => r.User.Id == options.UserId);

            if (!string.IsNullOrEmpty(options.UserName))
            {
                IContentQuery<UserPart, UserPartRecord> users =
                    _contentManager.Query<UserPart, UserPartRecord>()
                        .Where(
                            a =>
                                a.UserName.Contains(options.UserName.Trim()) ||
                                a.Email.Contains(options.UserName.Trim()));
                if (users.Count() > 0)
                {
                    List<int> userIds = users.List().Select(a => a.Id).ToList();
                    list = list.Where(p => userIds.Contains(p.User.Id));
                }
            }

            const string format = "dd/MM/yyyy";
            CultureInfo provider = CultureInfo.InvariantCulture;
            const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

            DateTime start, end;

            DateTime.TryParseExact(options.StartDateTrading, format, provider, style, out start);
            DateTime.TryParseExact(options.EndDateTrading, format, provider, style, out end);

            if (start.Year > 1) list = list.Where(p => p.DateTrading >= start);
            if (end.Year > 1) list = list.Where(p => p.DateTrading <= end.AddHours(24));

            return list;
        }

        public AdsPaymentConfigPartRecord GetPaymentConfigByCssClass(string cssClass)
        {
            return
                _contentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                    .Where(r => r.CssClass == cssClass)
                    .Slice(1)
                    .First()
                    .Record;
        }

        public int GetPaymentConfigValue(string cssClass)//Đơn giá tin vip/ngày
        {
            int tempValue = 100;
            int.TryParse(GetPaymentConfigByCssClass(cssClass).Value.ToString(CultureInfo.InvariantCulture),
                out tempValue);

            return tempValue;
        }

        public AdsPaymentConfigPartRecord GetPaymentConfigByVipValue(int vipValue)
        {
            //trên form nếu gửi về giá trị 0 là tin thường thì sẽ lấy giá trị là 4 đã lưu trong table AdsPaymentConfigPartRecord
            if (vipValue == 0) vipValue = 4;

            return
                _contentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                    .Where(r => r.VipValue == vipValue)
                    .Slice(1)
                    .First()
                    .Record;
        }

        public IEnumerable<AdsPaymentConfigPartRecord> GetPaymentConfigs()
        {
            return _contentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                .List()
                .Select(r => r.Record);
        }

        public IEnumerable<AdsPaymentConfigPartRecord> GetPaymentConfigsAsVip()
        {
            return
                _contentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                    .List()
                    .Where(r => !_paymentCssClass.Contains(r.CssClass))
                    .Select(r => r.Record);
        }

        public string TotalAmountVND() //Tính tổng tiền user đã tự nạp;
        {
            long amount =
                _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                    .Where(r => r.PaymentConfig == GetPaymentConfigByCssClass("ins-money"))
                    .List()
                    .Sum(r => r.TransactionValue);

            return ConvertoVND(amount);
        }

        //Front End
        public IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> PaymentHistoryFilter(
            AdsPaymentFrontEndOptions options)
        {
            UserPartRecord user = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> list =
                _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>().Where(r => r.User == user);


            if (!string.IsNullOrEmpty(options.TypeVipCssClass))
                list = list.Where(r => r.PaymentConfig == GetPaymentConfigByCssClass(options.TypeVipCssClass));


            const string format = "dd/MM/yyyy";
            CultureInfo provider = CultureInfo.InvariantCulture;
            const DateTimeStyles style = DateTimeStyles.AdjustToUniversal;

            DateTime start, end;

            DateTime.TryParseExact(options.StartDateTrading, format, provider, style, out start);
            DateTime.TryParseExact(options.EndDateTrading, format, provider, style, out end);

            if (start.Year > 1) list = list.Where(p => p.DateTrading >= start);
            if (end.Year > 1) list = list.Where(p => p.DateTrading <= end.AddHours(24));

            return list;
        }

        public AdsPaymentHistoryPart GetPaymentHistoryLasted(UserPart user)//Số tiền còn trong tài khoản
        {
            IEnumerable<AdsPaymentHistoryPart> paymentLasted = _contentManager
                .Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                .Where(r => r.User.Id == user.Id).OrderByDescending(c => c.DateTrading).Slice(1).ToList();

            return paymentLasted.Any() ? paymentLasted.First() : null;
        }

        public AdsPaymentHistoryPart GetPaymentHistoryByProperty(PropertyPart p)//Giao dịch gần nhất của Property này
        {
            return _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                .Where(r => r.Property == p.Record).OrderByDescending(r => r.DateTrading).Slice(1).FirstOrDefault();
        }

        /// <summary>
        /// </summary>
        /// <param name="adsTypeVip"></param>
        /// <param name="p"></param>
        /// <param name="oldSeqOrder"></param>
        /// <param name="oldAdsVipExpirationDate"></param>
        /// <param name="newDays"></param>
        /// <returns></returns>
        public bool CheckIsHaveMoney(int adsTypeVip, PropertyPart p, int oldSeqOrder, DateTime? oldAdsVipExpirationDate,
            int newDays)
        {
            List<int> paymentConfigIds =
                GetPaymentConfigs().Where(a => _paymentCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

            AdsPaymentConfigPartRecord newPaymentConfig = GetPaymentConfigByVipValue(adsTypeVip); // giá tin vip
            var user = Services.WorkContext.CurrentUser.As<UserPart>();
            AdsPaymentHistoryPart lastedHistory = GetPaymentHistoryLasted(user);

            //Chưa có tiền trong tài khoản
            if (lastedHistory == null)
                return false;

            if (p == null) //Trường hợp tin này mới đăng
                return newPaymentConfig.Value*newDays <= lastedHistory.EndBalance;

            IEnumerable<AdsPaymentHistoryPart> ads =
                _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                    .Where(
                        r =>
                            r.Property != null && r.Property == p.Record &&
                            !paymentConfigIds.Contains(r.PaymentConfig.Id))
                    .OrderByDescending(r => r.DateTrading).List();
            if (ads.Any()) // Nếu p.Record này đã giao dịch tin vip
            {
                //int oldDays = ads.FirstOrDefault().PostingDates;
                AdsPaymentConfigPartRecord oldPaymentConfig = GetPaymentConfigByVipValue(oldSeqOrder);

                if (oldAdsVipExpirationDate.HasValue && oldAdsVipExpirationDate > DateTime.Now) //Còn hạn đăng tin vip
                {
                    //Số ngày còn hạn đăng tin vip VD: đăng 5 ngày, nhưng mới đăng 3 ngày giờ muốn tăng thêm ngày đăng VIP nữa
                    double daysHas = (oldAdsVipExpirationDate.Value - DateTime.Now).TotalDays;
                    if (daysHas >= newDays)
                        return true;

                    return (newPaymentConfig.Value*newDays - daysHas*oldPaymentConfig.Value) <= lastedHistory.EndBalance;
                }
                return newPaymentConfig.Value*newDays <= lastedHistory.EndBalance;
            }
            return newPaymentConfig.Value*newDays <= lastedHistory.EndBalance;
        }

        public string ConvertoVND(long number)
        {
            string price = number >= 0 ? number.ToString(CultureInfo.InvariantCulture) : "0";
            int count = price.Count();
            string priceStr;
            switch (count)
            {
                case 1:
                case 2:
                case 3:
                    priceStr = number + " đồng";
                    break;
                case 4:
                case 5:
                case 6:
                    priceStr = number/1000 + " nghìn ";
                    if (number%1000 > 0)
                        priceStr += number%1000 + " đồng";
                    else
                        priceStr += " đồng";
                    break;
                case 7:
                case 8:
                case 9:
                    priceStr = number/1000000 + " triệu ";
                    if (number%1000000 > 0)
                    {
                        long thousand = number%1000000;
                        priceStr += thousand/1000 + " nghìn";
                        if (thousand%1000 > 0)
                            priceStr += thousand%1000 + " đồng";
                        else
                            priceStr += " đồng";
                    }
                    else
                        priceStr += " đồng";
                    break;
                case 10:
                case 11:
                case 12:
                    priceStr = number/1000000000 + " tỷ";
                    if (number%1000000000 > 0)
                    {
                        long hundredThousand = number%1000000000;
                        priceStr += hundredThousand/1000000 + " triệu";

                        if (hundredThousand%1000000 > 0)
                        {
                            long thousand = number%1000000;
                            priceStr += thousand/1000 + " nghìn";
                            if (thousand%1000 > 0)
                                priceStr += thousand%1000 + " đồng";
                            else
                                priceStr += " đồng";
                        }
                        else
                            priceStr += " đồng";
                    }
                    else
                        priceStr += " đồng";

                    break;

                default:
                    priceStr = number + " đồng";
                    break;
            }
            return priceStr;
        }

        public bool UpdatePaymentHistory(string oldStatusCssClass, int oldSeqOrder, bool isRefresh,
            DateTime? oldAdsVipExpirationDate, PropertyPart p, int adsTypeVip, UserPart user, double intDays)
        {
            try
            {
                List<int> paymentConfigIds =
                    GetPaymentConfigs().Where(a => _paymentCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

                AdsPaymentConfigPartRecord newPaymentConfig = GetPaymentConfigByVipValue(adsTypeVip);
                AdsPaymentHistoryPart lastedHistory = GetPaymentHistoryLasted(user);
                long endBalance = lastedHistory != null ? lastedHistory.EndBalance : 0;

                IEnumerable<AdsPaymentHistoryPart> ads =
                    _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                        .Where(
                            r =>
                                r.Property != null && r.Property == p.Record &&
                                !paymentConfigIds.Contains(r.PaymentConfig.Id))
                        .OrderByDescending(r => r.DateTrading)
                        .List().ToList();

                if (ads.Any())
                {
                    var paymentRecord = _contentManager.Get<AdsPaymentHistoryPart>(ads.First().Id);
                    AdsPaymentConfigPartRecord oldPaymentConfig = GetPaymentConfigByVipValue(oldSeqOrder);
                    //Số ngày còn hạn đăng tin vip VD: đăng 5 ngày, nhưng mới đăng 3 ngày giờ muốn tăng thêm ngày đăng VIP nữa
                    var oldPostingDates =
                        (int)
                            (oldAdsVipExpirationDate != null && oldAdsVipExpirationDate.Value > DateTime.Now
                                ? (oldAdsVipExpirationDate.Value - DateTime.Now).TotalDays
                                : 0); // paymentRecord.PostingDates;

                    long oldAmount = oldPaymentConfig.Value*oldPostingDates; //Số tiền tin vip cũ
                    var newAmount = (long) (newPaymentConfig.Value*intDays); //Số tiền tin vip mới

                    if (oldStatusCssClass == "st-approved" && isRefresh) // Tin đã duyệt và làm mới lại
                    {
                        if (oldPostingDates < intDays)
                            //Nếu đã làm mới lại => số ngày đăng vip ko đc nhỏ hơn số ngày đã đăng còn lại
                        {
                            paymentRecord.PaymentConfig = newPaymentConfig;
                            paymentRecord.Property = p.Record;
                            paymentRecord.Note = newPaymentConfig.Description;
                            paymentRecord.DateTrading = DateTime.Now;
                            paymentRecord.StartBalance = endBalance;
                            paymentRecord.EndBalance = endBalance > 0 ? endBalance - (newAmount - oldAmount) : 0; //----
                            paymentRecord.TransactionValue = newAmount - oldAmount;
                            paymentRecord.PayStatus = false;
                            paymentRecord.IsInternal = false;
                            paymentRecord.PostingDates = (int) intDays;

                            Services.Notifier.Information(T("BĐS yêu cầu đăng tin {0}", newPaymentConfig.Name));
                        }
                    }
                    else if (oldStatusCssClass == "st-pending")
                        // Tin đã đăng VIP nhưng chưa được duyệt nên có thể sửa và cập nhật lại giá tin VIP
                    {
                        if (newAmount >= oldAmount)
                        {
                            paymentRecord.EndBalance = endBalance - (newAmount - oldAmount);
                        }
                        else
                        {
                            paymentRecord.EndBalance = endBalance + (oldAmount - newAmount);
                        }

                        paymentRecord.Note = newPaymentConfig.Description +
                                             T(" - Đã update lịch sử tin {0}, Số ngày: {1} => {2}, Số ngày: {3}",
                                                 oldPaymentConfig.VipValue == 0
                                                     ? "Tin thường"
                                                     : oldPaymentConfig.Name, oldPostingDates, newPaymentConfig.Name,
                                                 intDays);
                        paymentRecord.PaymentConfig = newPaymentConfig;
                        paymentRecord.DateTrading = DateTime.Now;
                        paymentRecord.PostingDates = (int) intDays;
                        paymentRecord.TransactionValue = newAmount;
                        paymentRecord.IsInternal = false;

                        Services.Notifier.Information(
                            T(" - Đã update lịch sử tin {0}, Số ngày: {1} => {2}, Số ngày: {3}",
                                oldPaymentConfig.VipValue == 0 ? "Tin thường" : oldPaymentConfig.Name,
                                oldPostingDates, newPaymentConfig.Name, intDays));
                    }
                }
                else
                {
                    var newAds = Services.ContentManager.New<AdsPaymentHistoryPart>("AdsPaymentHistory");

                    newAds.User = user.Record;
                    newAds.UserPerform = user.Record;
                    newAds.PaymentConfig = newPaymentConfig;
                    newAds.Property = p.Record;
                    newAds.Note = newPaymentConfig.Description;
                    newAds.DateTrading = DateTime.Now;
                    newAds.StartBalance = endBalance;
                    newAds.EndBalance = endBalance > 0 ? endBalance - newPaymentConfig.Value*(int) intDays : 0;
                    newAds.TransactionValue = newPaymentConfig.Value*(int) intDays;
                    newAds.PayStatus = false;
                    newAds.IsInternal = false;
                    newAds.PostingDates = (int) intDays;

                    Services.ContentManager.Create(newAds);

                    Services.Notifier.Information(T("BĐS yêu cầu đăng tin {0}", newPaymentConfig.Name));
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        

        public bool CancelAdsVIPRequest(int oldVip, UserPart user, PropertyPart p)
        {
            try
            {
                List<int> paymentConfigIds =
                    GetPaymentConfigs().Where(a => _paymentCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

                AdsPaymentConfigPartRecord paymentConfig = GetPaymentConfigByVipValue(oldVip);

                IEnumerable<AdsPaymentHistoryPart> ads =
                    _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                        .Where(r => r.Property == p.Record && !paymentConfigIds.Contains(r.PaymentConfig.Id))
                        .OrderByDescending(r => r.DateTrading)
                        .List().ToList();

                if (!ads.Any()) return false;
                if (p.AdsVIPExpirationDate == null || p.AdsVIPExpirationDate.Value <= DateTime.Now) return false;

                var paymentRecord = _contentManager.Get<AdsPaymentHistoryPart>(ads.First().Id);
                var days = (int) (p.AdsVIPExpirationDate.Value - DateTime.Now).TotalDays;

                paymentRecord.PaymentConfig = paymentConfig;
                paymentRecord.DateTrading = DateTime.Now;
                paymentRecord.Note = paymentConfig.Description +
                                     T(" - Đã update lịch sử tin {0}", paymentConfig.Name);
                paymentRecord.StartBalance = paymentRecord.EndBalance;
                paymentRecord.EndBalance = paymentRecord.EndBalance + paymentConfig.Value*days;
                paymentRecord.TransactionValue = paymentConfig.Value*days;
                paymentRecord.PostingDates = 0;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ApprovedPaymentHistory(PropertyPart p)
        {
            List<int> paymentConfigIds =
                GetPaymentConfigs().Where(a => _paymentCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            IContentQuery<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord> paymentHistory =
                _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                    .Where(
                        r =>
                            r.Property == p.Record && r.PaymentConfig != null &&
                            !paymentConfigIds.Contains(r.PaymentConfig.Id));
            p.IsRefresh = false;
            p.AdsVIPRequest = false;

            if (paymentHistory != null && paymentHistory.Count() > 0)
                paymentHistory.Slice(1).First().PayStatus = true;
        }

        public void NotApprovedPaymentHistory(PropertyPart p)
        {
            List<int> paymentConfigIds =
                GetPaymentConfigs().Where(a => _paymentCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();

            var paymentHistory =
                _contentManager.Query<AdsPaymentHistoryPart, AdsPaymentHistoryPartRecord>()
                    .Where(
                        r =>
                            r.Property == p.Record && r.PaymentConfig != null &&
                            !paymentConfigIds.Contains(r.PaymentConfig.Id));
            p.IsRefresh = false;
            p.AdsVIPRequest = false;
            p.AdsVIP = false;
            p.AdsVIPExpirationDate = null;
            p.SeqOrder = 0;

            if (paymentHistory != null && paymentHistory.List().Any(r => !r.PayStatus))
            {
                paymentHistory.List().First().PayStatus = true;

                var ads = Services.ContentManager.New<AdsPaymentHistoryPart>("AdsPaymentHistory");
                AdsPaymentHistoryPart lastedHistory =
                    GetPaymentHistoryLasted(_contentManager.Get<UserPart>(p.CreatedUser.Id));
                AdsPaymentConfigPartRecord payByProperty = GetPaymentConfigByVipValue(p.SeqOrder);

                ads.User = p.CreatedUser;
                ads.UserPerform = Services.WorkContext.CurrentUser.As<UserPart>().Record;
                ads.StartBalance = lastedHistory != null ? lastedHistory.EndBalance : 0;
                ads.EndBalance = lastedHistory != null
                    ? lastedHistory.EndBalance + payByProperty.Value
                    : payByProperty.Value;
                ads.TransactionValue = payByProperty.Value;
                ads.Property = p.Record;
                ads.PaymentConfig = GetPaymentConfigByCssClass("ins-admin-money");
                ads.DateTrading = DateTime.Now;
                ads.PayStatus = true;
                ads.Note = "Hoàn tiền cho tin đăng " + p.Id + " đã không được duyệt.";

                Services.ContentManager.Create(ads);
            }
        }

        //Nạp tiền từ thẻ cào và tin nhắn - RealEstate.UserControlPanel/User
        public void AddPaymentHistory(PayType paytype, UserPart user, long amount, string note)
        {
            try
            {
                long amountPromotion;
                if (paytype == PayType.PayByCard)
                    amountPromotion = amount + (amount*GetPaymentConfigValue("ins-promotion-money"))/100; //Khuyến mãi
                else amountPromotion = amount;

                AdsPaymentHistoryPart paymentLasted = GetPaymentHistoryLasted(user);
                var ads = Services.ContentManager.New<AdsPaymentHistoryPart>("AdsPaymentHistory");
                ads.User = user.Record;
                ads.UserPerform = user.Record;
                ads.StartBalance = paymentLasted != null ? paymentLasted.EndBalance : 0;
                ads.EndBalance = paymentLasted != null ? paymentLasted.EndBalance + amountPromotion : amountPromotion;
                ads.TransactionValue = amount;
                ads.Property = null;
                ads.PaymentConfig = GetPaymentConfigByCssClass("ins-money");
                ads.DateTrading = DateTime.Now;
                ads.PayStatus = true;
                ads.Note = !string.IsNullOrEmpty(note) ? note : "Gửi " + ConvertoVND(amount) + " vào tài khoản.";

                Services.ContentManager.Create(ads);
            }
            catch (Exception e)
            {
                Services.Notifier.Error(T("{0}", e.Message));
            }
        }
    }
}