using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using JetBrains.Annotations;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Services;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Estimation.Models;
using Orchard.Data;
using System.Web.Mvc;

namespace RealEstate.Services
{
    public interface IEstimationService : IDependency
    {
        int IntParseAddressNumber(string addressNumber);
        int GetTurnsFromAddressNumber(string addressNumber, char alleySymbol, char stopSymbol);
        double GetDistanceFromAddressNumber(string addressNumber);

        double UnitPriceAvgOnStreet(StreetRelationPartRecord streetRelation);

        bool IsEstimateable(int districtId, int wardId, int streetId, string addressNumber, string addressCorner);

        double EstimatePrice(PropertyPart p);
        double EstimatePrice(PropertyPart p, bool debug);

        IEnumerable<PropertyPart> GetPropertyList(string typeGroupCssClass, int? districtId, int? wardId, int? streetId,
            int? apartmentId, bool getFront, bool getAlley, bool getInternalOnly);

        IContentQuery<PropertyPart, PropertyPartRecord> AllProperties(string typeGroupCssClass);

        double GetUnitPriceFromApplicationCache(string key);
        List<int> GetPropertyListFromApplicationCache(string key);
        string GetUnitPriceMsgFromApplicationCache(string key);
        void AddToApplicationCache(string key, object value);
        double GetFromApplicationCache(string key);
        void ClearApplicationCache(int propertyId);

        void BackgroundEstimate();
    }

    [UsedImplicitly]
    public class EstimationService : IEstimationService
    {
        private const int Batch = 10;
        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours

        // Init
        private bool _debugAlleyCoefficients = true;
        private bool _debugEstimate = true;
        private bool _debugEstimateLandPrice = true;
        //private bool _debugEstimateHousePrice = false;
        private bool _debugUnitPrice = true;
        private bool _debugUnitPriceFromMatrixRelations = true;
        private bool _debugUnitPriceOnStreet;
        private bool _debugUnitPriceOnStreetAlley = true;
        private bool _degubGetProperty;

        #region Init

        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IPropertyService _propertyService;
        private readonly ISignals _signals;

        private readonly IRepository<EstimateRecord> _estimateRepository;

        public EstimationService(
            IPropertyService propertyService,
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IRepository<EstimateRecord> estimateRepository,
            IOrchardServices services)
        {
            _propertyService = propertyService;

            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;
            _estimateRepository = estimateRepository;

            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;

            if (!Services.Authorizer.Authorize(Permissions.ViewDebugLogEstimateProperties))
            {
                _debugEstimate = false;
                _debugEstimateLandPrice = false;
                //_debugEstimateHousePrice = false;
                _debugUnitPrice = false;
                _debugUnitPriceOnStreet = false;
                _debugUnitPriceOnStreetAlley = false;
                _debugUnitPriceFromMatrixRelations = false;
                _debugAlleyCoefficients = false;
                _degubGetProperty = false;
            }
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        #endregion

        #region STRING FUNCTION

        public int CountChar(string source, char countChar)
        {
            int count = 0;
            try
            {
                count += source.Count(t => t == countChar);
            }
            catch (Exception)
            {
                count = 0;
            }
            return count;
        }

        public int CountChar(string source, char countChar, char stopChar)
        {
            int count = 0;
            try
            {
                count += source.TakeWhile(t => t != stopChar).Count(t => t == countChar);
            }
            catch (Exception)
            {
                count = 0;
            }
            return count;
        }

        #endregion

        #region AddressNumber FUNCTION

        public int IntParseAddressNumber(string addressNumber)
        {
            if (String.IsNullOrEmpty(addressNumber)) return -1;
            try
            {
                string[] list = addressNumber.Split('/');
                Match firstmatchedValue = Regex.Match(list[0], @"\d+", RegexOptions.IgnorePatternWhitespace);
                return int.Parse(firstmatchedValue.Value);
            }
            catch
            {
                return -1;
            }
        }

        public int GetTurnsFromAddressNumber(string addressNumber, char alleySymbol, char stopSymbol)
        {
            if (String.IsNullOrEmpty(addressNumber)) return 0;

            return string.IsNullOrEmpty(stopSymbol.ToString(CultureInfo.InvariantCulture))
                ? CountChar(addressNumber, alleySymbol)
                : CountChar(addressNumber, alleySymbol, stopSymbol);
        }

        public double GetDistanceFromAddressNumber(string addressNumber)
        {
            if (String.IsNullOrEmpty(addressNumber)) return 0;

            double distance = 0;
            double chieuNgangTieuChuan = double.Parse(GetSetting("Chieu_Ngang_Tieu_Chuan") ?? "4");
            // double.Parse(ConfigurationManager.AppSettings["Chieu_Ngang_Tieu_Chuan"] ?? "4");
            double chieuDaiTieuChuan = double.Parse(GetSetting("Chieu_Dai_Tieu_Chuan") ?? "20");
            // double.Parse(ConfigurationManager.AppSettings["Chieu_Dai_Tieu_Chuan"] ?? "20");

            string[] list = addressNumber.Split('/');
            for (int i = 1; i < list.Length; i++)
            {
                Match firstmatchedValue = Regex.Match(list[i], @"\d+", RegexOptions.IgnorePatternWhitespace);
                if (!string.IsNullOrEmpty(firstmatchedValue.Value))
                {
                    distance += int.Parse(firstmatchedValue.Value) / 2 * chieuNgangTieuChuan;
                }
            }
            return distance + GetTurnsFromAddressNumber(addressNumber, '/', '+')*chieuDaiTieuChuan;
        }

        #endregion

        #region ESTIMATE

        public double EstimatePrice(PropertyPart p, bool debug)
        {
            if (!Services.Authorizer.Authorize(Permissions.ViewDebugLogEstimateProperties))
            {
                debug = false;
            }

            _debugEstimate = debug;
            _debugEstimateLandPrice = debug;
            //_debugEstimateHousePrice = debug;
            _debugUnitPrice = debug;
            _debugUnitPriceOnStreet = debug;
            _debugUnitPriceOnStreetAlley = debug;
            _debugUnitPriceFromMatrixRelations = debug;
            _debugAlleyCoefficients = debug;
            _degubGetProperty = debug;

            return EstimatePrice(p);
        }

        public double EstimatePrice(PropertyPart p)
        {
            DateTime startEstimatePrice = DateTime.Now;

            string key = p.Id.ToString();
            double priceEstimatedInVnd = 0;

            var gpHouse = GetTypeGroup("gp-house");
            var gpApartment = GetTypeGroup("gp-apartment");

            if (p.TypeGroup != null && p.TypeGroup.Id == gpHouse.Id && p.District != null && p.Ward != null &&
                p.Street != null)
            {
                // gp-house Định giá nhà phố

                DateTime startEstimatePriceHouse = DateTime.Now;
                double priceHouseEstimated = EstimateHousePrice(p);
                if (_debugEstimate)
                    Services.Notifier.Information(T("ĐG nhà {0}", (DateTime.Now - startEstimatePriceHouse).TotalSeconds));

                DateTime startEstimatePriceLand = DateTime.Now;
                double priceLandEstimated = EstimateLandPrice(p);
                if (_debugEstimate)
                    Services.Notifier.Information(T("ĐG đất {0}", (DateTime.Now - startEstimatePriceLand).TotalSeconds));

                double estimatedPrice = priceLandEstimated > 0 ? (priceLandEstimated + priceHouseEstimated) : 0;

                double percent = 100;
                var percentMsg = new List<string>();

                #region ĐẶC ĐIỂM TỐT

                double frontWidth = p.AreaTotalWidth ?? 0;
                double backWidth = p.AreaTotalBackWidth ?? frontWidth;
                double length = p.AreaTotalLength ?? 0;
                double areaTotal = p.AreaTotal ?? ((frontWidth + backWidth)/2*length);
                double areaLegal = areaTotal;

                // Dùng thông tin diện tích hợp qui hoạch (nếu có)
                if ((p.AreaLegalWidth.HasValue && p.AreaLegalLength.HasValue))
                {
                    frontWidth = (double) p.AreaLegalWidth;
                    backWidth = p.AreaLegalBackWidth.HasValue ? (double) p.AreaLegalBackWidth : frontWidth;
                    length = (double) p.AreaLegalLength;
                }
                if ((p.AreaLegalWidth.HasValue && p.AreaLegalLength.HasValue) || p.AreaLegal.HasValue)
                {
                    areaLegal = p.AreaLegal ?? (frontWidth + backWidth)/2*length;
                }

                // Nếu nở hậu MT < MH < MT+1 cộng (2%)
                if (frontWidth < backWidth && backWidth < frontWidth + 1)
                {
                    double toAdd = double.Parse(GetSetting("No_Hau") ?? "2");
                    percent += toAdd; // double.Parse(ConfigurationManager.AppSettings["No_Hau"] ?? "2");
                    percentMsg.Add("Nở hậu +" + toAdd + "%");
                }

                //foreach (var adv in p.Advantages)
                //{
                //    _percent += adv.AddedValue;
                //}

                #endregion

                #region ĐẶC ĐIỂM XẤU

                // DTĐất > 200m2
                if (areaLegal > 200)
                {
                    double toAdd = double.Parse(GetSetting("Dien_Tich_Dat_Lon") ?? "-5");
                    percent += toAdd;
                    percentMsg.Add("Dien_Tich_Dat_Lon " + toAdd + "%");
                }

                // DTĐất > 50m2 và NgangMT < 4m (-5%)
                if (areaLegal > 50 && frontWidth < 4)
                {
                    double toAdd = double.Parse(GetSetting("Dien_Tich_Dat_Hep") ?? "-5");
                    percent += toAdd;
                    percentMsg.Add("Dien_Tich_Dat_Hep " + toAdd + "%");
                }

                // DTĐất < 36m2 hoặc NgangMT < 3m (-10%)
                if (areaLegal < 36 || frontWidth < 3)
                {
                    double toAdd = double.Parse(GetSetting("Dien_Tich_Dat_Nho") ?? "-10");
                    percent += toAdd; // double.Parse(ConfigurationManager.AppSettings["Dien_Tich_Dat_Nho"] ?? "-10");
                    percentMsg.Add("Dien_Tich_Dat_Nho " + toAdd + "%");
                }

                // Tóp hậu MT - MH > 0.5m (-8%)
                if (frontWidth - backWidth > 0.2)
                {
                    double toAdd = double.Parse(GetSetting("Top_Hau") ?? "-8");
                    percent += toAdd; // double.Parse(ConfigurationManager.AppSettings["Top_Hau"] ?? "-8");
                    percentMsg.Add("Top_Hau " + toAdd + "%");
                }

                //foreach (var disadv in p.DisAdvantages)
                //{
                //    _percent += disadv.AddedValue;
                //}

                #endregion

                // Advantages
                percent += _propertyService.GetPropertyAdvantages(p).Sum(adv => adv.AddedValue);

                // OtherAdvantages
                if (p.OtherAdvantages > 0)
                {
                    percent += (double) p.OtherAdvantages;
                }

                // DisAdvantages
                percent += _propertyService.GetPropertyDisAdvantages(p).Sum(adv => adv.AddedValue);

                // OtherDisAdvantages
                if (p.OtherDisAdvantages.HasValue && Math.Abs((double) p.OtherDisAdvantages) > 0)
                {
                    percent -= Math.Abs((double) p.OtherDisAdvantages);
                }

                #region MỤC ĐÍCH ĐỊNH GIÁ

                //if (string.IsNullOrEmpty(formValues["radPurpose"]))
                //{
                //    ModelState.AddModelError("radPurpose", "Vui lòng chọn mục đích định giá");
                //    return View();
                //}
                //else
                //{
                //    // Để bán (+5%) 
                //    if (formValues["radPurpose"].Contains("sale"))
                //        _percent += double.Parse(ConfigurationManager.AppSettings["De_Ban"] ?? "5");

                //    // Để mua (-5%) 
                //    if (formValues["radPurpose"].Contains("buy"))
                //        _percent += double.Parse(ConfigurationManager.AppSettings["De_Mua"] ?? "-5");

                //    // Thế chấp (-10%) 
                //    if (formValues["radPurpose"].Contains("mortgage"))
                //        _percent += double.Parse(ConfigurationManager.AppSettings["The_Chap"] ?? "-10");

                //    // Mục đích khác (+0%) 
                //    if (formValues["radPurpose"].Contains("other"))
                //        _percent += double.Parse(ConfigurationManager.AppSettings["Muc_Dich_Khac"] ?? "0");

                //    ViewBag.Purpose = formValues["radPurpose"];
                //}

                #endregion

                AddToApplicationCache(key + "_debug_priceHouseEstimated", priceHouseEstimated);
                AddToApplicationCache(key + "_debug_priceLandEstimated", priceLandEstimated);
                AddToApplicationCache(key + "_debug_percent", percent - 100);
                AddToApplicationCache(key + "_debug_percent_msg", String.Join(", ", percentMsg));

                if (_debugEstimate)
                    Services.Notifier.Information(T("ĐG BĐS {0}", (DateTime.Now - startEstimatePrice).TotalSeconds));

                // Định giá đặc điểm TỐT không được phép vượt quá 150% giá trị BĐS và đặc điểm XẤU không được dưới 50% giá trị BĐS
                if (percent > 150)
                {
                    percent = 150;
                    AddToApplicationCache(key + "_debug_percent_msg",
                        String.Join(", ", "Đặc điểm TỐT không được phép vượt quá 150% giá trị BĐS"));
                }
                if (percent < 50)
                {
                    percent = 50;
                    AddToApplicationCache(key + "_debug_percent_msg",
                        String.Join(", ", "Đặc điểm XẤU không được phép dưới 50% giá trị BĐS"));
                }

                priceEstimatedInVnd = estimatedPrice*percent/100;
            }
            else if (p.TypeGroup != null && p.TypeGroup.Id == gpApartment.Id)
            {
                // gp-apartment Định giá chung cư
            }
            else
            {
                // Clear UnitPrice in Cache
                ClearApplicationCache(p.Id);
            }

            #region Set Flag

            PropertyFlagPart dealNormal = GetFlag("deal-normal");
            PropertyFlagPart dealUnknow = GetFlag("deal-unknow");
            PropertyFlagPart dealBad = GetFlag("deal-bad");
            PropertyFlagPart dealGood = GetFlag("deal-good");
            PropertyFlagPart dealVeryGood = GetFlag("deal-very-good");

            // BĐS rao bán
            if (p.AdsType.CssClass == "ad-selling")
            {
                // Chỉ TỰ ĐỘNG đánh giá các BĐS rao bán
                if (priceEstimatedInVnd > 0)
                {
                    if (p.PriceProposedInVND > 0)
                    {
                        double percent = (priceEstimatedInVnd - (double) p.PriceProposedInVND)/
                                         (double) p.PriceProposedInVND*100;
                        if (percent >= dealVeryGood.Value)
                        {
                            p.Flag = dealVeryGood.Record;
                        }
                        else if (percent >= dealGood.Value)
                        {
                            p.Flag = dealGood.Record;
                        }
                        else if (percent <= dealBad.Value)
                        {
                            p.Flag = dealBad.Record;
                            p.AdsGoodDeal = false;
                            p.AdsGoodDealExpirationDate = null;
                        }
                        else
                        {
                            p.Flag = dealNormal.Record;
                            p.AdsGoodDeal = false;
                            p.AdsGoodDealExpirationDate = null;
                        }
                    }
                    else
                    {
                        // BĐS định giá
                        p.Flag = dealNormal.Record;
                        p.AdsGoodDeal = false;
                        p.AdsGoodDealExpirationDate = null;
                    }
                }
                else
                {
                    p.Flag = dealUnknow.Record;

                    // Tự động correct BĐS Giá rẻ hiện trên trang chủ
                    if (p.PriceEstimatedByStaff > 0 && p.PriceProposedInVND > 0 &&
                        p.PriceEstimatedByStaff > p.PriceProposed)
                    {
                        // BĐS có thể quảng cáo vào BĐS giá rẻ
                    }
                    else
                    {
                        p.AdsGoodDeal = false;
                        p.AdsGoodDealExpirationDate = null;
                        var Url = new UrlHelper(HttpContext.Current.Request.RequestContext);
                        Services.Notifier.Warning(T("BĐS <a href='{0}'>{1} - Địa chỉ: {2}</a> không thể đưa vào BĐS giá rẻ do Giá rao bán cao hơn Giá định giá của nhân viên.", Url.Action("Edit", new { p.Id }), p.Id, p.DisplayForAddressForOwner));
                    }
                }
            }

            #endregion

            return priceEstimatedInVnd;
        }

        public double EstimateHousePrice(PropertyPart p)
        {
            double unitConstruction = 0; // Triệu VND

            // ĐƠN GIÁ LOẠI CÔNG TRÌNH XÂY DỰNG (TR / m2)
            if (p.TypeConstruction != null){
                try
                {
                    unitConstruction += p.TypeConstruction.UnitPrice;
                }
                catch (Exception)
                {
                    var typeConstruction = _contentManager.Get<PropertyTypeConstructionPart>(p.TypeConstruction.Id);
                    if (typeConstruction != null && typeConstruction.UnitPrice > 0)
                        unitConstruction += typeConstruction.UnitPrice;
                }
            }
            else
            {
                if (p.Type != null)
                {
                    // lấy Loại công trình Default theo số tầng
                    IContentQuery<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord> filter = _contentManager
                        .Query<PropertyTypeConstructionPart, PropertyTypeConstructionPartRecord>()
                        .Where(a => a.IsEnabled)
                        .Where(a => a.PropertyType.Id == p.Type.Id)
                        .Where(a => a.IsDefaultInFloorsRange)
                        .OrderBy(a => a.SeqOrder);

                    if (p.Floors > 0)
                    {
                        filter =
                            filter.Where(
                                a =>
                                    (a.MinFloor <= p.Floors || a.MinFloor == null) &&
                                    (a.MaxFloor >= p.Floors || a.MaxFloor == null));
                    }
                    else if (p.Floors <= 0 || p.Floors == null)
                    {
                        // Nhà phố liền kề trệt
                        filter = filter.Where(a => a.MaxFloor == 0);
                    }


                    if (filter != null && filter.Count() > 0)
                    {
                        PropertyTypeConstructionPart typeConstructionDefault = filter.Slice(1).First();
                        unitConstruction += typeConstructionDefault.UnitPrice;
                    }
                    else
                    {
                        try
                        {
                            unitConstruction += p.Type.UnitPrice;
                        }
                        catch (Exception)
                        {
                            var type = _contentManager.Get<PropertyTypePart>(p.Type.Id);
                            if (type != null && type.UnitPrice > 0)
                                unitConstruction += type.UnitPrice;
                        }
                    }
                }
            }
            // ĐƠN GIÁ HOÀN THIỆN NỘI THẤT (1TR -> 5TR)
            if (p.Interior != null)
            {
                try
                {
                    if(p.Interior.UnitPrice > 0)
                        unitConstruction += p.Interior.UnitPrice;
                }
                catch (Exception)
                {
                    var interior = _contentManager.Get<PropertyInteriorPart>(p.Interior.Id);
                    if (interior != null && interior.UnitPrice > 0)
                        unitConstruction += interior.UnitPrice;
                }
            }

            // TÍNH TỔNG DIỆN TÍCH SÀN XÂY DỰNG
            double areaConstructionFloor = _propertyService.CalcAreaConstructionFloor(p);

            // TÍNH GIÁ NHÀ THEO TỔNG DIỆN TÍCH SÀN XÂY DỰNG
            double priceHouse = areaConstructionFloor*(unitConstruction/1000000000)*(p.RemainingValue ?? 100)/100;

            #region GIÁ TRỊ CỘNG THÊM

            if (p.HaveElevator) priceHouse += double.Parse(GetSetting("Co_Thang_May") ?? "200")/1000;
            if (p.HaveSwimmingPool) priceHouse += double.Parse(GetSetting("Co_Ho_Boi") ?? "100")/1000;

            #endregion

            return priceHouse;
        }

        public double EstimateLandPrice(PropertyPart p)
        {
            DateTime startEstimateLandPrice = DateTime.Now;

            string key = p.Id.ToString(CultureInfo.InvariantCulture);

            double unitPriceEstimate;
            double aCoeff = 1;

            var locationFront = GetLocation("h-front");

            if (p.Location.Id == locationFront.Id)
            {
                DateTime startEstimateUnitPriceOnStreet = DateTime.Now;
                double unitPriceOnStreet = UnitPriceOnStreet(p.Id, "gp-house", p.District.Id, p.Ward.Id, p.Street.Id,
                    p.AlleyNumber, true);
                if (_debugEstimateLandPrice)
                    Services.Notifier.Information(T("ĐG UnitPriceOnStreet {0}",
                        (DateTime.Now - startEstimateUnitPriceOnStreet).TotalSeconds));
                unitPriceEstimate = unitPriceOnStreet;
            }
            else
            {
                //DateTime startEstimateAlleyCoefficients = DateTime.Now;
                aCoeff = AlleyCoefficients(p);
                //Services.Notifier.Information(T("ĐG AlleyCoefficients {0}", (DateTime.Now - startEstimateAlleyCoefficients).TotalSeconds));

                DateTime startUnitPriceOnStreetAlley = DateTime.Now;
                unitPriceEstimate = UnitPriceOnStreetAlley(p.Id, "gp-house", p.District.Id, p.Ward.Id, p.Street.Id,
                    p.AlleyNumber, true);
                if (_debugEstimateLandPrice)
                    Services.Notifier.Information(T("ĐG UnitPriceOnStreetAlley {0}",
                        (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));
            }

            double frontWidth = p.AreaTotalWidth ?? 0;
            double backWidth = p.AreaTotalBackWidth ?? frontWidth;
            double length = p.AreaTotalLength ?? 0;
            double areaTotal = p.AreaTotal ?? (frontWidth + backWidth)/2*length;
            double areaLegal = areaTotal;
            double areaIlegalRecognized = 0;
            double areaIlegalNotRecognized = 0;

            // Dùng thông tin diện tích hợp qui hoạch (nếu có)
            if (p.AreaLegalWidth.HasValue && p.AreaLegalLength.HasValue)
            {
                frontWidth = (double) p.AreaLegalWidth;
                backWidth = p.AreaLegalBackWidth ?? frontWidth;
                length = (double) p.AreaLegalLength;

                areaLegal = p.AreaLegal ?? (frontWidth + backWidth)/2*length;

                areaIlegalRecognized = p.AreaIlegalRecognized ?? 0;
                areaIlegalNotRecognized = p.AreaIlegalNotRecognized ?? (areaTotal - areaLegal - areaIlegalRecognized);
            }

            double areaStandard = frontWidth*length;
            double areaExcess = areaLegal - areaStandard;

            //DateTime startEstimateLengthCoefficients = DateTime.Now;
            double lCoeff = LengthCoefficients(frontWidth, length);
            //Services.Notifier.Information(T("ĐG LengthCoefficients {0}", (DateTime.Now - startEstimateLengthCoefficients).TotalSeconds));

            //DateTime startEstimateWidthCoefficients = DateTime.Now;
            double wCoeff = WidthCoefficients(frontWidth, backWidth, length, areaLegal);
            //Services.Notifier.Information(T("ĐG WidthCoefficients {0}", (DateTime.Now - startEstimateWidthCoefficients).TotalSeconds));

            // Tính DT Ngang _areaWidth
            double areaWidth = AreaWidth(frontWidth, backWidth, length, areaLegal, wCoeff);

            // Giá đất = Đơn giá * HS hẻm * (DT Ngang * HS dài + DTvpcn + DTvpkcn)
            double landPrice = unitPriceEstimate*aCoeff*
                               (
                                   areaWidth*lCoeff
                                   +
                                   (areaIlegalRecognized*
                                    double.Parse(GetSetting("Vi_Pham_Lo_Gioi_Duoc_Cong_Nhan") ?? "60")/100)
                                   +
                                   (areaIlegalNotRecognized*
                                    double.Parse(GetSetting("Vi_Pham_Lo_Gioi_Khong_Cong_Nhan") ?? "20")/100)
                                   );

            //DateTime startApplicationCache = DateTime.Now;

            //AddToApplicationCache(key + "_debug_unitPrice", _unitPrice);
            AddToApplicationCache(key + "_debug_unitPriceEstimate", unitPriceEstimate);

            AddToApplicationCache(key + "_debug_areaLegal", areaLegal);
            AddToApplicationCache(key + "_debug_frontWidth", frontWidth);
            AddToApplicationCache(key + "_debug_backWidth", backWidth);
            AddToApplicationCache(key + "_debug_length", length);

            AddToApplicationCache(key + "_debug_areaIlegalRecognized", areaIlegalRecognized);
            AddToApplicationCache(key + "_debug_areaIlegalNotRecognized", areaIlegalNotRecognized);

            AddToApplicationCache(key + "_debug_areaStandard", areaStandard);
            AddToApplicationCache(key + "_debug_areaExcess", areaExcess);

            AddToApplicationCache(key + "_debug_aCoeff", aCoeff);
            AddToApplicationCache(key + "_debug_lCoeff", lCoeff);
            AddToApplicationCache(key + "_debug_wCoeff", wCoeff);

            AddToApplicationCache(key + "_debug_areaWidth", areaWidth);

            //Services.Notifier.Information(T("Save ApplicationCache {0}", (DateTime.Now - startApplicationCache).TotalSeconds));

            if (_debugEstimateLandPrice)
                Services.Notifier.Information(T("ĐG EstimateLandPrice {0}",
                    (DateTime.Now - startEstimateLandPrice).TotalSeconds));

            return landPrice;
        }

        public double EstimateApartmentPrice(PropertyPart p)
        {
            DateTime startEstimateApartmentPrice = DateTime.Now;

            //string key = p.Id.ToString();

            //double unitPriceEstimate;

            double apartmentPrice = 0;

            //bool getInternalOnly = true;


            if (_debugEstimateLandPrice)
                Services.Notifier.Information(T("ĐG EstimateApartmentPrice {0}",
                    (DateTime.Now - startEstimateApartmentPrice).TotalSeconds));

            return apartmentPrice;
        }

        #endregion

        #region COEFFICIENTS

        public double AlleyCoefficients(PropertyPart p)
        {
            string cacheKey = p.Id + "_aCoeff";

            // Kiểm tra trong dữ liệu tạm
            return _cacheManager.Get(cacheKey, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When(cacheKey + "_Expired"));

                double aCoeff = 0;

                #region CACHE

                //double aCoeff = GetUnitPriceFromApplicationCache(cacheKey);
                //if (aCoeff > 0) return aCoeff;

                #endregion

                DateTime startAlleyCoefficients = DateTime.Now;

                var provinceHaNoi =
                    _contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                        .Where(a => a.Name == "Hà Nội")
                        .List()
                        .First();

                string addressNumber = p.AddressNumber;
                if (p.Province.Id == provinceHaNoi.Id && !String.IsNullOrEmpty(p.AddressCorner))
                    addressNumber = p.AddressCorner + "/" + p.AddressNumber;

                //DateTime startAlleyTurns = DateTime.Now;
                int turns = p.AlleyTurns ?? GetTurnsFromAddressNumber(addressNumber, '/', '+');
                if (turns <= 0) return 0;
                //if (_debugAlleyCoefficients) Services.Notifier.Information(T("AlleyTurns BĐS {0} take {1}", p.DisplayForAddressForOwner, (DateTime.Now - startAlleyTurns).TotalSeconds));

                //DateTime startAlleyDistance = DateTime.Now;
                double distance = p.DistanceToStreet ?? GetDistanceFromAddressNumber(addressNumber);
                //if (distance <= 0) return 0;
                if (distance <= 0) distance = 50; // Set mặc định 50m cho các trường hợp không tính được khoảng cách từ số nhà
                //if (_debugAlleyCoefficients) Services.Notifier.Information(T("AlleyDistance BĐS {0} take {1}", p.DisplayForAddressForOwner, (DateTime.Now - startAlleyDistance).TotalSeconds));

                //DateTime startAlleyWidths = DateTime.Now;
                double? w1 = p.AlleyWidth1,
                    w2 = p.AlleyWidth2,
                    w3 = p.AlleyWidth3,
                    w4 = p.AlleyWidth4,
                    w5 = p.AlleyWidth5,
                    w6 = p.AlleyWidth6,
                    w7 = p.AlleyWidth7,
                    w8 = p.AlleyWidth8,
                    w9 = p.AlleyWidth9;
                if (!w1.HasValue && !w2.HasValue && !w3.HasValue && !w4.HasValue && !w5.HasValue && !w6.HasValue &&
                    !w7.HasValue && !w8.HasValue && !w9.HasValue) return 0;

                // Backward
                if (w9.HasValue && !w8.HasValue) w8 = w9;
                if (w8.HasValue && !w7.HasValue) w7 = w8;
                if (w7.HasValue && !w6.HasValue) w6 = w7;
                if (w6.HasValue && !w5.HasValue) w5 = w6;
                if (w5.HasValue && !w4.HasValue) w4 = w5;
                if (w4.HasValue && !w3.HasValue) w3 = w4;
                if (w3.HasValue && !w2.HasValue) w2 = w3;
                if (w2.HasValue && !w1.HasValue) w1 = w2;

                // Fordward
                if (w1.HasValue && !w2.HasValue) w2 = w1;
                if (w2.HasValue && !w3.HasValue) w3 = w2;
                if (w3.HasValue && !w4.HasValue) w4 = w3;
                if (w4.HasValue && !w5.HasValue) w5 = w4;
                if (w5.HasValue && !w6.HasValue) w6 = w5;
                if (w6.HasValue && !w7.HasValue) w7 = w6;
                if (w7.HasValue && !w8.HasValue) w8 = w7;
                if (w8.HasValue && !w9.HasValue) w9 = w8;

                var widths = new List<double>
                {
                    (double) w1,
                    (double) w2,
                    (double) w3,
                    (double) w4,
                    (double) w5,
                    (double) w6,
                    (double) w7,
                    (double) w8,
                    (double) w9
                };
                //if (_debugAlleyCoefficients) Services.Notifier.Information(T("AlleyWidths BĐS {0} take {1}", p.DisplayForAddressForOwner, (DateTime.Now - startAlleyWidths).TotalSeconds));

                if (_debugAlleyCoefficients)
                    Services.Notifier.Information(T("AlleyCoefficients BĐS {0} take {1}", p.DisplayForAddressForOwner,
                        (DateTime.Now - startAlleyCoefficients).TotalSeconds));

                DateTime startCoeff = DateTime.Now;
                aCoeff = AlleyCoefficients(turns, widths, distance, p.Street.Id, p.Ward.Id, p.District.Id, p.AlleyNumber,
                    p.Id);
                if (_debugAlleyCoefficients)
                    Services.Notifier.Information(T("startCoeff BĐS {0} take {1}", p.DisplayForAddressForOwner,
                        (DateTime.Now - startCoeff).TotalSeconds));

                //AddToApplicationCache(cacheKey, aCoeff);

                return aCoeff;
            });
        }

        public double AlleyCoefficients(int turns, List<double> widths, double distance, int streetId, int wardId,
            int districtId, int alleyNumber, int key)
        {
            DateTime startGetRelations = DateTime.Now;
            var street = Services.ContentManager.Get(streetId).As<LocationStreetPart>();
            StreetRelationPart streetRelation =
                Services.ContentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                    .Where(a => a.District.Id == districtId && a.Ward.Id == wardId && a.Street.Id == streetId
                                && a.CoefficientAlley1Max > 0
                                && a.CoefficientAlley1Min > 0
                                && a.CoefficientAlleyEqual > 0
                                && a.CoefficientAlleyMax > 0
                                && a.CoefficientAlleyMin > 0).List().FirstOrDefault();

            if (_debugAlleyCoefficients)
                Services.Notifier.Information(T("LẤY BẢNG Relations take {0}",
                    (DateTime.Now - startGetRelations).TotalSeconds));

            if (streetRelation != null)
            {
                #region LẤY HỆ SỐ CẤP HẺM

                DateTime startAlleyLevel = DateTime.Now;

                // Lấy hệ số theo Tên Đường
                double heSoHemCap1Max = streetRelation.CoefficientAlley1Max ?? 0;
                double heSoHemCap1Min = streetRelation.CoefficientAlley1Min ?? 0;

                double heSoHemCapNEqual = streetRelation.CoefficientAlleyEqual ?? 0;
                double heSoHemCapNMax = streetRelation.CoefficientAlleyMax ?? 0;
                double heSoHemCapNMin = streetRelation.CoefficientAlleyMin ?? 0;

                if (_debugAlleyCoefficients)
                    Services.Notifier.Information(T("LẤY HỆ SỐ CẤP HẺM take {0}",
                        (DateTime.Now - startAlleyLevel).TotalSeconds));

                #endregion

                #region LẤY ĐỘ RỘNG HẺM TỐI ĐA

                DateTime startMaxAlleyWidth = DateTime.Now;

                double doRongHemToiDa = double.Parse(GetSetting("Do_Rong_Hem_Toi_Da") ?? "12");

                // Compare with street width in LocationStreets
                if (street.StreetWidth.HasValue && street.StreetWidth > 0)
                    doRongHemToiDa = Math.Min(doRongHemToiDa, (double) street.StreetWidth);

                if (_debugAlleyCoefficients)
                    Services.Notifier.Information(T("LẤY ĐỘ RỘNG HẺM TỐI ĐA take {0}",
                        (DateTime.Now - startMaxAlleyWidth).TotalSeconds));

                #endregion

                #region TÍNH HỆ SỐ HẺM THEO CẤP HẺM CUỐI

                DateTime startAlleyLast = DateTime.Now;
                string alleyUnitMsg = "";
                double alleyUnit = 1;

                double hmin = 0;

                for (int i = 1; i <= turns; i++)
                {
                    if (i == 1)
                    {
                        // Tính hẻm cấp 1
                        double h1 = widths[0];
                        hmin = h1;
                        if (h1 >= doRongHemToiDa)
                        {
                            alleyUnit = alleyUnit*heSoHemCap1Max;
                        }
                        else
                        {
                            alleyUnit = ((alleyUnit*heSoHemCap1Max - heSoHemCap1Min)/doRongHemToiDa)*h1 + heSoHemCap1Min;
                        }
                        alleyUnitMsg = String.Format("{0:#,0.##}", alleyUnit);
                    }
                    else
                    {
                        // Tính các cấp hẻm lớn hơn cấp 2
                        double hn = widths[i - 1];
                        double preWidth = widths[i - 2];

                        if (hn >= doRongHemToiDa) // H(n) >= 12m
                        {
                            alleyUnit = heSoHemCapNMax*alleyUnit;
                        }
                        else if (Equals(hn, preWidth)) // H(n) = H(n-1)
                        {
                            alleyUnit = heSoHemCapNEqual*alleyUnit;
                        }
                        else if (hn > hmin)
                        {
                            // hs(n)=(max-equa)*hs*(Hn-Hmin)/(12-Hmin)+0.9*hs
                            alleyUnit = (heSoHemCapNMax - heSoHemCapNEqual)*alleyUnit*(hn - hmin)/
                                        (doRongHemToiDa - hmin) + (heSoHemCapNEqual*alleyUnit);
                        }
                        else
                        {
                            if (hmin >= doRongHemToiDa)
                            {
                                alleyUnit = ((alleyUnit*heSoHemCapNEqual - heSoHemCapNMin)*hn/doRongHemToiDa) +
                                            heSoHemCapNMin;
                            }
                            else
                            {
                                alleyUnit = ((alleyUnit*heSoHemCapNEqual - heSoHemCapNMin)*hn/hmin) + heSoHemCapNMin;
                            }
                            hmin = hn;
                        }
                        alleyUnitMsg += " / " + String.Format("{0:#,0.##}", alleyUnit);
                    }
                }

                AddToApplicationCache(key + "_debug_aUnitPrice", alleyUnitMsg);
                if (_debugAlleyCoefficients)
                    Services.Notifier.Information(T("TÍNH HỆ SỐ HẺM THEO CẤP HẺM CUỐI take {0}",
                        (DateTime.Now - startAlleyLast).TotalSeconds));

                #endregion

                #region LẤY KHOẢNG CÁCH MT TỐI ĐA THEO ĐỘ RỘNG HẺM CUỐI

                DateTime startMaxAlleyDistance = DateTime.Now;

                double khoangCachMtToiDa;
                double heSoKhoangCachMtToiThieu;

                double doRongHemCuoi = widths[turns - 1];

                if (
                    _contentManager.Query<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord>()
                        .Where(a => a.LastAlleyWidth >= doRongHemCuoi)
                        .List()
                        .Any())
                {
                    CoefficientAlleyDistancePart coeffDistance =
                        _contentManager.Query<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord>()
                            .Where(a => a.LastAlleyWidth >= doRongHemCuoi)
                            .OrderBy(a => a.LastAlleyWidth)
                            .List()
                            .FirstOrDefault();

                    khoangCachMtToiDa = coeffDistance.MaxAlleyDistance;
                    heSoKhoangCachMtToiThieu = coeffDistance.CoefficientDistance;
                }
                else
                {
                    CoefficientAlleyDistancePart lastCoeffDistance =
                        _contentManager.Query<CoefficientAlleyDistancePart, CoefficientAlleyDistancePartRecord>()
                            .OrderByDescending(a => a.LastAlleyWidth)
                            .List()
                            .FirstOrDefault();

                    khoangCachMtToiDa = lastCoeffDistance.MaxAlleyDistance;
                    heSoKhoangCachMtToiThieu = lastCoeffDistance.CoefficientDistance;
                }

                if (_debugAlleyCoefficients)
                    Services.Notifier.Information(T("LẤY KHOẢNG CÁCH MT TỐI ĐA THEO ĐỘ RỘNG HẺM CUỐI take {0}",
                        (DateTime.Now - startMaxAlleyDistance).TotalSeconds));

                #endregion

                double alleyCoeff;

                if (khoangCachMtToiDa > distance)
                {
                    alleyCoeff = heSoKhoangCachMtToiThieu +
                                 ((alleyUnit - heSoKhoangCachMtToiThieu)*(khoangCachMtToiDa - distance)/
                                  khoangCachMtToiDa);
                }
                else
                {
                    alleyCoeff = heSoKhoangCachMtToiThieu;
                }

                return alleyCoeff;
            }
            var ward = _contentManager.Get(wardId).As<LocationWardPart>();
            if (_debugAlleyCoefficients)
                Services.Notifier.Error(
                    T("NoRelation: Chưa có dữ liệu hệ số hẻm của đường {0}, {1}, {2} trong bảng quan hệ.",
                        street.DisplayForStreetName, ward.Name, street.District.Name));
            return 0;
        }

        public double LengthCoefficients(double width, double length)
        {
            List<CoefficientLengthPart> configLength =
                _contentManager.Query<CoefficientLengthPart, CoefficientLengthPartRecord>()
                    .OrderBy(a => a.WidthRange)
                    .List()
                    .ToList();

            double minL = configLength[0].MinLength; // 2
            double maxL = configLength[0].MaxLength; // 5

            if (length <= minL) return 0.5;

            if (width <= 2)
            {
                if (length > maxL)
                    return (0.6*(5*maxL - length))/(4*maxL) + 0.4;

                if (length > minL && length <= maxL)
                    return 1;
            }

            //double widthRange = 0;
            double minLength = 0;
            double maxLength = 0;

            if (configLength.Any(a => a.WidthRange >= width))
            {
                CoefficientLengthPart coeffLength =
                    configLength.Where(a => a.WidthRange >= width).OrderBy(a => a.WidthRange).FirstOrDefault();

                if (coeffLength != null)
                {
                    //widthRange = coeffLength.WidthRange;
                    minLength = coeffLength.MinLength;
                    maxLength = coeffLength.MaxLength;
                }
            }
            else
            {
                CoefficientLengthPart lastCoeffLength =
                    configLength.OrderByDescending(a => a.WidthRange).FirstOrDefault();

                if (lastCoeffLength != null)
                {
                    //widthRange = lastCoeffLength.WidthRange;
                    minLength = lastCoeffLength.MinLength;
                    maxLength = lastCoeffLength.MaxLength;
                }
            }

            if (length < minLength)
            {
                if (length <= 3) return 0.7;
                return (0.2*(length - 3))/(minLength - 3) + 0.8;
            }
            if (length > maxLength)
            {
                if (length > 5*maxLength) return 0.4;
                return (0.6*(5*maxLength - length))/(4*maxLength) + 0.4;
            }

            return 1;
        }

        public double WidthCoefficients(double frontWidth, double backWidth, double length, double totalArea)
        {
            double wCoeff = 1;

            double standardArea = frontWidth*length;
            double excessArea = totalArea - standardArea;

            if (Equals(frontWidth, backWidth))
            {
                if (Equals(totalArea, standardArea))
                {
                    wCoeff = 1;
                }
                else if (totalArea > standardArea)
                {
                    if (totalArea > 2*standardArea)
                    {
                        wCoeff = 0.5;
                    }
                    else
                    {
                        wCoeff = 0.5 + 0.5*(1 - excessArea/standardArea);
                    }
                    //_wCoeff = ((0.5 / standardArea) * (excessArea / 2)) + 0.5;
                }
                else if (totalArea < standardArea)
                {
                    wCoeff = totalArea/standardArea;
                }
            }
            else if (frontWidth < backWidth)
            {
                if (Equals(totalArea, standardArea))
                {
                    if (backWidth/frontWidth <= 3)
                    {
                        wCoeff = (((0.5/(2*frontWidth))*((3*frontWidth) - backWidth))) + 0.5;
                    }
                    else
                    {
                        wCoeff = 0.5;
                    }
                }
                else if (totalArea > standardArea)
                {
                    if (backWidth/frontWidth <= 4)
                    {
                        wCoeff = ((0.6/(3*frontWidth))*((7*frontWidth - backWidth)/2) + 0.4);
                    }
                    else
                    {
                        wCoeff = 0.4;
                    }
                }
                else if (totalArea < standardArea)
                {
                    if (backWidth/frontWidth <= 3)
                    {
                        //_wCoeff = ((0.5 / frontWidth) * ((3 * frontWidth) - backWidth)) + 0.5;
                        //_wCoeff = (0.5 - (0.5 * backWidth) / (3 * frontWidth)) + 0.5;
                        wCoeff = totalArea/standardArea;
                    }
                    else
                    {
                        wCoeff = 0.5;
                    }
                }
            }
            else if (frontWidth > backWidth)
            {
                if (totalArea <= standardArea)
                {
                    wCoeff = ((0.5/frontWidth)*backWidth) + 0.5;
                }
                else if (totalArea > standardArea)
                {
                    if (frontWidth/backWidth < 5)
                    {
                        wCoeff = ((0.7/(4*backWidth))*((9*backWidth - frontWidth)/2) + 0.3);
                    }
                    else
                    {
                        wCoeff = 0.3;
                    }
                }
            }

            return wCoeff;
        }

        public double AreaWidth(double frontWidth, double backWidth, double length, double areaLegal, double wCoeff)
        {
            // DT chuẩn
            double areaStandard = frontWidth*length;

            // DT vượt chuẩn
            double areaExcess = areaLegal - areaStandard;

            if (areaLegal > areaStandard)
            {
                if (frontWidth <= backWidth)
                {
                    // DT Ngang = (DTchuẩn + DTvượtchuẩn * HSngang)
                    return (areaStandard + areaExcess*wCoeff);
                }
                if (frontWidth > backWidth)
                {
                    // DT Ngang = (DT chuẩn * ((0.5*MH/MT) + 0.5) + HSngang*DTVượtchuẩn) 
                    return (areaStandard*((0.5*backWidth/frontWidth) + 0.5) + wCoeff*areaExcess);
                }
            }
            // areaLegal <= areaStandard
            // DT Ngang = DTQH * HSngang
            return (areaLegal*wCoeff);
        }

        /// <summary>
        ///     HsCC: Hệ số chung cư
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double ApartmentCoefficients(PropertyPart p)
        {
            // HsCc: Hệ số chung cư
            double hsCc = -1;

            if (!p.ApartmentFloors.HasValue || p.ApartmentFloors <= 0)
            {
                if (p.Apartment != null && p.Apartment.Floors > 0)
                {
                    p.ApartmentFloors = p.Apartment.Floors;
                }
            }

            if (p.ApartmentFloors > 0)
            {
                // HsTCc: Hệ số tầng chung cư
                double hsTCc;

                // lấy giá trị theo apartmentFloors
                IContentQuery<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord> list = _contentManager
                    .Query<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord>()
                    .Where(a => a.Floors >= p.ApartmentFloors)
                    .OrderBy(a => a.Floors);

                if (list.Count() > 0)
                {
                    hsTCc = list.Slice(1).First().CoefficientApartmentFloors;
                }
                else
                {
                    // apartmentFloors không có trong config, lấy giá trị Floors lớn nhất
                    hsTCc =
                        _contentManager.Query<CoefficientApartmentFloorsPart, CoefficientApartmentFloorsPartRecord>()
                            .OrderByDescending(a => a.Floors).Slice(1).First().CoefficientApartmentFloors;
                }

                // HsT: Hệ số các tiện ích cộng thêm

                IEnumerable<PropertyAdvantagePartRecord> apartmentAdvantages =
                    _propertyService.GetPropertyApartmentAdvantages(p);
                double hsTi = apartmentAdvantages.Sum(adv => adv.AddedValue/100);

                // HsCc: Hệ số chung cư = Hệ số tầng chung cư + Hệ số các tiện ích
                hsCc = hsTCc + hsTi;
            }

            return hsCc;
        }

        /// <summary>
        ///     HsT: Hệ số tầng căn hộ
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public double ApartmentFloorThCoefficients(PropertyPart p)
        {
            // HsT: Hệ số tầng
            double hsT = -1;

            if (!p.ApartmentFloors.HasValue || p.ApartmentFloors <= 0)
            {
                if (p.Apartment != null && p.Apartment.Floors > 0)
                {
                    p.ApartmentFloors = p.Apartment.Floors;
                }
            }

            if (p.ApartmentFloors > 0 && p.ApartmentFloorTh > 0)
            {
                // lấy hệ số theo MaxFloors
                IContentQuery<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord> list = _contentManager
                    .Query<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>()
                    .Where(
                        a =>
                            a.MaxFloors > 0 && a.MaxFloors >= p.ApartmentFloors &&
                            a.ApartmentFloorTh == p.ApartmentFloorTh)
                    .OrderBy(a => a.MaxFloors);

                if (list.Count() > 0)
                {
                    hsT = list.Slice(1).First().CoefficientApartmentFloorTh;
                }
                else
                {
                    // lấy hệ số theo ApartmentFloorTh khi MaxFloors không xác định
                    list = _contentManager.Query<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>
                        ()
                        .Where(a => a.MaxFloors == null && a.ApartmentFloorTh == p.ApartmentFloorTh)
                        .OrderBy(a => a.ApartmentFloorTh);

                    if (list.Count() > 0)
                    {
                        hsT = list.Slice(1).First().CoefficientApartmentFloorTh;
                    }
                    else
                    {
                        // lấy hệ số theo ApartmentFloorTh lớn nhất
                        hsT =
                            _contentManager
                                .Query<CoefficientApartmentFloorThPart, CoefficientApartmentFloorThPartRecord>()
                                .Where(a => a.MaxFloors == null)
                                .OrderByDescending(a => a.ApartmentFloorTh)
                                .Slice(1)
                                .First()
                                .CoefficientApartmentFloorTh;
                    }
                }
            }

            return hsT;
        }

        #endregion

        #region UNITPRICE

        public double UnitPrice(PropertyPart p)
        {
            //string cacheKey = p.Id.ToString() + "_debug_unitPrice";
            //return _cacheManager.Get(cacheKey, ctx =>
            //{
            //    ctx.Monitor(_signals.When(p.Id.ToString() + "_Changed"));

            #region UnitPrice

            DateTime startUnitPrice = DateTime.Now;

            //Services.Notifier.Information(T("Id {0} PriceProposed {1}", p.Id, p.PriceProposed));
            if (!p.PriceProposed.HasValue || p.PriceProposed <= 0) return 0;

            // Kiểm tra trong dữ liệu tạm
            string key = p.Id.ToString(CultureInfo.InvariantCulture);
            double unitPrice = GetFromApplicationCache(key + "_debug_unitPrice");
            if (unitPrice > 0)
            {
                return unitPrice;
            }

            var gpHouse = GetTypeGroup("gp-house");
            var gpApartment = GetTypeGroup("gp-apartment");

            if (p.TypeGroup.Id == gpHouse.Id)
            {
                #region Nhà Phố

                double frontWidth = p.AreaTotalWidth ?? 0;
                double backWidth = p.AreaTotalBackWidth ?? frontWidth;
                double length = p.AreaTotalLength ?? 0;
                double areaTotal = p.AreaTotal ?? ((frontWidth + backWidth)/2*length);
                double areaLegal = areaTotal;
                double areaIlegalRecognized = 0;
                double areaIlegalNotRecognized = 0;

                // Dùng thông tin diện tích hợp qui hoạch (nếu có)
                if (p.AreaLegalWidth.HasValue && p.AreaLegalLength.HasValue)
                {
                    frontWidth = (double) p.AreaLegalWidth;
                    backWidth = p.AreaLegalBackWidth ?? frontWidth;
                    length = (double) p.AreaLegalLength;

                    areaLegal = p.AreaLegal ?? ((frontWidth + backWidth)/2*length);
                    areaIlegalRecognized = (p.AreaIlegalRecognized ?? 0)*
                                           double.Parse(GetSetting("Vi_Pham_Lo_Gioi_Duoc_Cong_Nhan") ?? "60")/100;
                    // double.Parse(ConfigurationManager.AppSettings["Vi_Pham_Lo_Gioi_Duoc_Cong_Nhan"] ?? "60") / 100;
                    areaIlegalNotRecognized = (p.AreaIlegalNotRecognized ??
                                               (areaTotal - areaLegal - (p.AreaIlegalRecognized ?? 0)))*
                                              double.Parse(GetSetting("Vi_Pham_Lo_Gioi_Khong_Cong_Nhan") ?? "20")/100;
                    // double.Parse(ConfigurationManager.AppSettings["Vi_Pham_Lo_Gioi_Khong_Cong_Nhan"] ?? "20") / 100;
                }

                // HS hẻm
                double aCoeff = 1; // AlleyCoefficients(model);

                var locationFront = GetLocation("h-front");

                if (p.Location.Id != locationFront.Id)
                {
                    aCoeff = AlleyCoefficients(p);
                    if (aCoeff <= 0) return 0;
                }

                // HS dài
                double lengthCoefficients = LengthCoefficients(frontWidth, length);

                // HS ngang
                double widthCoefficients = WidthCoefficients(frontWidth, backWidth, length, areaLegal);

                double estimateHousePrice = EstimateHousePrice(p);

                double priceProposedInVnd = _propertyService.CaclPriceProposedInVnd(p) ?? 0;

                double landPrice = priceProposedInVnd - estimateHousePrice;

                // Tính DT Ngang _areaWidth
                double areaWidth = AreaWidth(frontWidth, backWidth, length, areaLegal, widthCoefficients);

                // Giá đất = Đơn giá * (DT Ngang * HS dài * HS hẻm + DTvpcn + DTvpkcn)
                unitPrice = landPrice/
                            (areaWidth*lengthCoefficients*aCoeff + areaIlegalRecognized + areaIlegalNotRecognized);

                #endregion
            }
            else if (p.TypeGroup.Id == gpApartment.Id)
            {
                #region Chung cư

                // Đơn giá thô căn hộ = Giá rao bán - Giá trị gia tăng riêng của căn hộ - Giá nội thất

                // "ĐgT=(Gb-Gt-Nt)/m2"

                double gT = 0, gNt = 0, m2 = p.AreaUsable ?? 0;

                // Gb: Giá rao bán
                double gB = _propertyService.CaclPriceProposedInVnd(p) ?? 0;

                // Gt: Giá trị gia tăng riêng của căn hộ
                if (p.Balconies >= 2)
                {
                    // có 2 view +5%
                    gT = gB*5/100;
                }

                // Nt: Giá nội thất theo bảng đầu tư nội thất
                if (p.Interior != null)
                {
                    var interior = _contentManager.Get<PropertyInteriorPart>(p.Interior.Id);
                    if(interior.UnitPrice > 0)
                        gNt = interior.UnitPrice * m2;
                }

                // ĐgT: Đơn giá thô căn hộ 
                double đgT = (gB - gT - gNt)/m2;

                // HsT: Hệ số tầng
                double hsT = ApartmentFloorThCoefficients(p);

                // HsCc: Hệ số chung cư
                double hsCc = ApartmentCoefficients(p);

                if (hsT > 0 && hsCc > 0)
                {
                    // ĐgT1: Đơn giá thô tầng 1 (tương tự đơn giá MT của nhà phố)

                    // "ĐgT1=ĐgT/(HsT*HsCc)"

                    // ĐgT1: Đơn giá thô tầng 1
                    double đgT1 = đgT/(hsT*hsCc);

                    unitPrice = đgT1;
                }

                #endregion
            }

            AddToApplicationCache(key + "_debug_unitPrice", unitPrice);

            //Services.Notifier.Information(T("Id {0} UnitPrice {1}", p.Id, _unitPrice));
            if (_debugUnitPrice)
                Services.Notifier.Warning(T("BĐS {0} - UnitPrice {1} Tỷ / m2 (take {2})", p.DisplayForAddressForOwner,
                    String.Format("{0:#,0.###}", unitPrice), (DateTime.Now - startUnitPrice).TotalSeconds));

            #endregion

            return unitPrice;
            //});
        }

        public double UnitPriceAvg(List<PropertyPart> listEstate, ref List<int> listId)
        {
            listId = new List<int>();
            var listPrice = new List<double>();

            foreach (PropertyPart item in listEstate)
            {
                double unitPrice = UnitPrice(item);
                if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                {
                    listPrice.Add(unitPrice);
                    listId.Add(item.Id);
                }
            }

            return UnitPriceAvg(listPrice);
        }

        public double UnitPriceAvg(List<double> listPrice)
        {
            double unitPriceAvg = 0;

            if (listPrice.Count > 0)
            {
                double medianValue = UnitPriceMedian(listPrice);

                listPrice.Sort();

                // Nếu có >= 6BĐS có đơn giá MT <=15% giá trung vị thì lấy trung bình đơn giá của những BĐS đó. Không cần lấy những BĐS có đơn giá >15% giá trung vị.
                List<double> listPriceBelowMedian =
                    listPrice.Where(a => Math.Abs(Math.Round((a - medianValue)/medianValue*100, 0)) < 15).ToList();

                if (listPriceBelowMedian.Count >= 6)
                {
                    unitPriceAvg = listPriceBelowMedian.Average();
                    if (_debugUnitPrice)
                        Services.Notifier.Error(
                            T("Đơn giá MT dùng ĐG {0} Tỷ / m2 từ {1} BĐS có đơn giá MT <= 15% giá trung vị",
                                unitPriceAvg, listPriceBelowMedian.Count));
                }
                else
                {
                    double acceptRateMarginBottom =
                        double.Parse(GetSetting("Bien_Do_Don_Gia_Mat_Tien_thap_hon_TV") ?? "30");
                    double acceptRateMarginTop = double.Parse(GetSetting("Bien_Do_Don_Gia_Mat_Tien_cao_hon_TV") ?? "10");
                    // int.Parse(ConfigurationManager.AppSettings["Bien_Do_Don_Gia_Mat_Tien"] ?? "20"); // Config

                    // Nếu giá trị đang xét chênh lệch quá 20% so với giá trị trung vị => kéo giá trị về gần Giá trị trung vị
                    for (int i = 0; i < listPrice.Count; i++)
                    {
                        double cur = listPrice[i];
                        double rate = Math.Abs(Math.Round((cur - medianValue)/medianValue*100, 0));

                        if (cur <= medianValue)
                        {
                            // Bien_Do_Don_Gia_Mat_Tien_thap_hon_TV
                            if (rate > acceptRateMarginBottom)
                                listPrice[i] = medianValue*(100 - acceptRateMarginBottom)/100;
                        }
                        else
                        {
                            // Bien_Do_Don_Gia_Mat_Tien_cao_hon_TV
                            if (rate > acceptRateMarginTop)
                                listPrice[i] = medianValue*(100 + acceptRateMarginTop)/100;
                        }
                    }

                    //return medianValue;
                    unitPriceAvg = listPrice.Average();
                    if (_debugUnitPrice)
                        Services.Notifier.Error(T("Đơn giá MT dùng ĐG {0} Tỷ / m2 từ {1} BĐS", unitPriceAvg,
                            listPrice.Count()));
                }
            }

            return unitPriceAvg;
        }

        public double UnitPriceMedian(List<double> listPrice)
        {
            // Tìm TRUNG VỊ của dãy số
            // Trong trường hợp n là số lẻ
            // Median = {n ÷ 2}th value
            // Trong trường hợp n là số chẵn
            // Median = (value below median + value above median) ÷ 2

            double medianValue = 0;

            listPrice.Sort();
            int count = listPrice.Count;

            if (count > 2)
                if (count%2 == 0)
                {
                    // count is even, need to get the middle two elements, add them together, then divide by 2
                    medianValue = (listPrice[(count/2) - 1] + listPrice[(count/2)])/2;
                }
                else
                {
                    // count is odd, simply get the middle element.
                    medianValue = listPrice[(count/2)];
                }

            return medianValue;
        }

        #endregion

        #region UNITPRICE ON STREET

        public double UnitPriceAvgOnStreet(StreetRelationPartRecord streetRelation)
        {
            double unitPriceOnStreet = 0;
            var listId = new List<int>();

            if (streetRelation.CoefficientAlley1Max > 0 && streetRelation.CoefficientAlley1Min > 0 &&
                streetRelation.CoefficientAlleyEqual > 0 && streetRelation.CoefficientAlleyMax > 0 &&
                streetRelation.CoefficientAlleyMin > 0)
                return unitPriceOnStreet;

            List<PropertyPart> pList =
                GetPropertyList("gp-house", streetRelation.District.Id, streetRelation.Ward.Id, streetRelation.Street.Id,
                    null, true, false, true).ToList();

            // Lấy ít nhất 4 căn MT cùng Đường, Phường, Quận
            int minPropertiesRequired = int.Parse(GetSetting("DGMT_Cung_Phuong") ?? "4");
            if (pList.Count >= minPropertiesRequired)
            {
                unitPriceOnStreet = UnitPriceAvg(pList, ref listId);
            }

            if (listId.Count < minPropertiesRequired)
            {
                // Lấy ít nhất 5 căn MT nằm trên các đường có giá trị tương đương
                minPropertiesRequired = int.Parse(GetSetting("DGMT_GT_Tuong_Duong") ?? "5");
                unitPriceOnStreet = UnitPriceFromMatrixRelations("gp-house", streetRelation.District.Id,
                    streetRelation.Ward.Id, streetRelation.Street.Id, minPropertiesRequired, true, false, true,
                    ref listId);
            }

            if (listId.Count >= minPropertiesRequired && unitPriceOnStreet > 0)
            {
                double unitPrice = unitPriceOnStreet*1000; // Đổi từ Tỷ VND => Triệu VND

                if (
                    _contentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                        .Where(a => a.StreetUnitPrice >= unitPrice)
                        .List()
                        .Any())
                {
                    // Lấy hệ số theo Đơn Giá MT gần nhất
                    CoefficientAlleyPart coeffAlley =
                        _contentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                            .Where(a => a.StreetUnitPrice >= unitPrice)
                            .OrderBy(a => a.StreetUnitPrice)
                            .List()
                            .FirstOrDefault();

                    if (coeffAlley != null)
                    {
                        streetRelation.CoefficientAlley1Max = coeffAlley.CoefficientAlley1Max;
                        streetRelation.CoefficientAlley1Min = coeffAlley.CoefficientAlley1Min;

                        streetRelation.CoefficientAlleyEqual = coeffAlley.CoefficientAlleyEqual;
                        streetRelation.CoefficientAlleyMax = coeffAlley.CoefficientAlleyMax;
                        streetRelation.CoefficientAlleyMin = coeffAlley.CoefficientAlleyMin;
                    }
                }
                else
                {
                    // Lấy hệ số theo Đơn Giá MT lớn nhất
                    CoefficientAlleyPart lastCoeffAlley =
                        _contentManager.Query<CoefficientAlleyPart, CoefficientAlleyPartRecord>()
                            .OrderByDescending(a => a.StreetUnitPrice)
                            .List()
                            .FirstOrDefault();

                    if (lastCoeffAlley != null)
                    {
                        streetRelation.CoefficientAlley1Max = lastCoeffAlley.CoefficientAlley1Max;
                        streetRelation.CoefficientAlley1Min = lastCoeffAlley.CoefficientAlley1Min;

                        streetRelation.CoefficientAlleyEqual = lastCoeffAlley.CoefficientAlleyEqual;
                        streetRelation.CoefficientAlleyMax = lastCoeffAlley.CoefficientAlleyMax;
                        streetRelation.CoefficientAlleyMin = lastCoeffAlley.CoefficientAlleyMin;
                    }
                }
            }

            return unitPriceOnStreet;
        }

        public bool IsEstimateable(int districtId, int wardId, int streetId, string addressNumber, string addressCorner)
        {
            _debugEstimate = false;
            _debugEstimateLandPrice = false;
            //_debugEstimateHousePrice = false;
            _debugUnitPrice = false;
            _debugUnitPriceOnStreet = false;
            _debugUnitPriceOnStreetAlley = false;
            _debugUnitPriceFromMatrixRelations = false;
            _debugAlleyCoefficients = false;
            _degubGetProperty = false;

            int number = IntParseAddressNumber(addressNumber);
            if (!String.IsNullOrEmpty(addressCorner))
                number = IntParseAddressNumber(addressCorner);

            double unitPriceOnStreet = UnitPriceOnStreet(0, "gp-house", districtId, wardId, streetId, number, true);
            if (unitPriceOnStreet > 0) return true;

            double unitPriceOnStreetAlley = UnitPriceOnStreetAlley(0, "gp-house", districtId, wardId, streetId, number,
                true);
            if (unitPriceOnStreetAlley > 0) return true;

            return false;
        }

        public double UnitPriceOnStreet(int id, string typeGroupCssClass, int districtId, int wardId, int streetId,
            int number, bool getInternalOnly)
        {
            string key = id.ToString(CultureInfo.InvariantCulture);
            if (id == 0) key = String.Join("_", districtId, wardId, streetId, number);

            // Kiểm tra trong dữ liệu tạm

            #region CACHE

            double unitPriceOnStreet = GetUnitPriceFromApplicationCache(key);
            if (unitPriceOnStreet > 0)
            {
                string msg = GetUnitPriceMsgFromApplicationCache(key + "_msg");
                if (!msg.Contains("CACHE:")) AddToApplicationCache(key + "_msg", "CACHE: " + msg);
                return unitPriceOnStreet;
            }

            #endregion

            var listId = new List<int>();
            int minPropertiesRequired = int.Parse(GetSetting("DGMT_Cung_Doan_Duong") ?? "3");

            //DateTime startUnitPriceOnStreet = DateTime.Now;

            List<PropertyPart> pList =
                GetPropertyList(typeGroupCssClass, null, null, streetId, null, true, false, getInternalOnly)
                    .ToList();

            var street = _contentManager.Get<LocationStreetPart>(streetId);
            if (street.RelatedStreet != null) // ĐOẠN ĐƯỜNG
            {
                #region ĐOẠN ĐƯỜNG

                if (pList.Count >= minPropertiesRequired)
                {
                    unitPriceOnStreet = UnitPriceAvg(pList, ref listId);
                    if (listId.Count >= minPropertiesRequired)
                    {
                        AddToApplicationCache(key, unitPriceOnStreet);
                        AddToApplicationCache(key + "_msg",
                            "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                            listId.Count + "</a></strong> căn MT cùng đoạn đường.");
                        AddToApplicationCache(key + "_list", listId);
                        return unitPriceOnStreet;
                    }
                }

                #endregion
            }
            else // ĐƯỜNG CHÍNH
            {
                #region 100 SỐ NHÀ

                List<PropertyPart> listProperties;
                // Lấy ít nhất 3 căn MT cùng đường trong khoảng +-100 số nhà
                if (number > 0)
                {
                    listProperties =
                        pList.Where(e => e.AlleyNumber >= (number - 100) && e.AlleyNumber <= (number + 100)).ToList();
                    if (listProperties.Count >= minPropertiesRequired)
                    {
                        unitPriceOnStreet = UnitPriceAvg(listProperties, ref listId);
                        if (listId.Count >= minPropertiesRequired)
                        {
                            AddToApplicationCache(key, unitPriceOnStreet);
                            AddToApplicationCache(key + "_msg",
                                "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                                listId.Count + "</a></strong> căn MT cùng đường trong khoảng +-100 số nhà.");
                            AddToApplicationCache(key + "_list", listId);
                            return unitPriceOnStreet;
                        }
                    }
                }

                #endregion

                #region CÙNG PHƯỜNG

                // Lấy ít nhất 4 căn MT cùng Đường, Phường, Quận
                listProperties = pList.Where(e => e.Ward.Id == wardId).ToList();
                minPropertiesRequired = int.Parse(GetSetting("DGMT_Cung_Phuong") ?? "4");
                if (listProperties.Count >= minPropertiesRequired)
                {
                    unitPriceOnStreet = UnitPriceAvg(listProperties, ref listId);
                    if (listId.Count >= minPropertiesRequired)
                    {
                        AddToApplicationCache(key, unitPriceOnStreet);
                        AddToApplicationCache(key + "_msg",
                            "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                            listId.Count + "</a></strong> căn MT cùng Đường, Phường, Quận.");
                        AddToApplicationCache(key + "_list", listId);
                        return unitPriceOnStreet;
                    }
                }

                #endregion
            }

            #region MT TƯƠNG ĐƯƠNG

            // Lấy ít nhất 5 căn MT nằm trên các đường có giá trị tương đương
            minPropertiesRequired = int.Parse(GetSetting("DGMT_GT_Tuong_Duong") ?? "5");
            unitPriceOnStreet = UnitPriceFromMatrixRelations(typeGroupCssClass, districtId, wardId, streetId,
                minPropertiesRequired, true, false, getInternalOnly, ref listId);
            //_unitPriceOnStreet = UnitPriceFromRelations(districtId, wardId, streetId, minPropertiesRequired, true, ref listId);
            if (listId.Count >= minPropertiesRequired)
            {
                AddToApplicationCache(key, unitPriceOnStreet);
                AddToApplicationCache(key + "_msg",
                    "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" + listId.Count +
                    "</a></strong> căn MT nằm trên các đường có giá trị tương đương.");
                AddToApplicationCache(key + "_list", listId);
                return unitPriceOnStreet;
            }

            #endregion

            if (getInternalOnly)
            {
                // BĐS nội bộ không đủ, phải lấy từ các group khác
                if (_debugUnitPriceOnStreet)
                    Services.Notifier.Warning(T("BĐS nội bộ không đủ, phải lấy từ các group khác"));
                return UnitPriceOnStreet(id, typeGroupCssClass, districtId, wardId, streetId, number, false);
            }

            return 0;
        }

        public double UnitPriceOnStreetAlley(int id, string typeGroupCssClass, int districtId, int wardId, int streetId,
            int alleyNumber, bool getInternalOnly)
        {
            string key = id.ToString(CultureInfo.InvariantCulture);
            if (id == 0) key = String.Join("_", districtId, wardId, streetId, alleyNumber);

            // Kiểm tra trong dữ liệu tạm

            #region CACHE

            double unitPriceOnStreetAlley = GetUnitPriceFromApplicationCache(key);
            if (unitPriceOnStreetAlley > 0)
            {
                string msg = GetUnitPriceMsgFromApplicationCache(key + "_msg");
                if (!msg.Contains("CACHE:")) AddToApplicationCache(key + "_msg", "CACHE: " + msg);
                return unitPriceOnStreetAlley;
            }

            #endregion

            var listId = new List<int>();
            int minPropertiesRequired = int.Parse(GetSetting("DGH_Cung_Hem") ?? "3");

            DateTime startUnitPriceOnStreetAlley = DateTime.Now;

            List<PropertyPart> pList =
                GetPropertyList(typeGroupCssClass, districtId, null, streetId, null, false, true, getInternalOnly)
                    .ToList();
            if (_debugUnitPriceOnStreetAlley)
                Services.Notifier.Information(T("Lấy các BĐS Hẻm cùng đường, quận take {0}",
                    (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

            List<PropertyPart> listAlleyProperties;

            #region CÙNG HẺM

            if (alleyNumber > 0)
            {
                // Lấy ít nhất 3 căn cùng hẻm (không phân biệt cấp hẻm)
                listAlleyProperties = pList.Where(e => e.AlleyNumber == alleyNumber).ToList();
                if (listAlleyProperties.Count >= minPropertiesRequired)
                {
                    unitPriceOnStreetAlley = UnitPriceAvg(listAlleyProperties, ref listId);
                    if (listId.Count >= minPropertiesRequired)
                    {
                        AddToApplicationCache(key, unitPriceOnStreetAlley);
                        AddToApplicationCache(key + "_msg",
                            "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                            listId.Count + "</a></strong> căn cùng hẻm (không phân biệt cấp hẻm).");
                        AddToApplicationCache(key + "_list", listId);

                        if (_debugUnitPriceOnStreetAlley)
                            Services.Notifier.Information(
                                T(
                                    "Lấy ít nhất " + minPropertiesRequired +
                                    " căn cùng hẻm (không phân biệt cấp hẻm) take {0}",
                                    (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

                        return unitPriceOnStreetAlley;
                    }
                }
            }

            #endregion

            var street = _contentManager.Get<LocationStreetPart>(streetId);
            if (street.RelatedStreet != null) // ĐOẠN ĐƯỜNG
            {
                #region ĐOẠN ĐƯỜNG

                minPropertiesRequired = int.Parse(GetSetting("DGH_Cung_Doan_Duong") ?? "4");
                if (pList.Count >= minPropertiesRequired)
                {
                    unitPriceOnStreetAlley = UnitPriceAvg(pList, ref listId);
                    if (listId.Count >= minPropertiesRequired)
                    {
                        AddToApplicationCache(key, unitPriceOnStreetAlley);
                        AddToApplicationCache(key + "_msg",
                            "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                            listId.Count + "</a></strong> căn Hẻm cùng đoạn đường.");
                        AddToApplicationCache(key + "_list", listId);

                        if (_debugUnitPriceOnStreetAlley)
                            Services.Notifier.Information(
                                T("Lấy ít nhất " + minPropertiesRequired + " căn Hẻm cùng đoạn đường take {0}",
                                    (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

                        return unitPriceOnStreetAlley;
                    }
                }

                #endregion
            }
            else // ĐƯỜNG CHÍNH
            {
                #region 100 SỐ NHÀ

                if (alleyNumber > 0)
                {
                    // Lấy ít nhất 4 căn hẻm trên đoạn đường giới hạn +-100 số nhà MT
                    listAlleyProperties =
                        pList.Where(e => e.AlleyNumber >= (alleyNumber - 100) && e.AlleyNumber <= (alleyNumber + 100))
                            .ToList();
                    minPropertiesRequired = int.Parse(GetSetting("DGH_Cung_Doan_Duong") ?? "4");
                    if (listAlleyProperties.Count >= minPropertiesRequired)
                    {
                        unitPriceOnStreetAlley = UnitPriceAvg(listAlleyProperties, ref listId);
                        if (listId.Count >= minPropertiesRequired)
                        {
                            AddToApplicationCache(key, unitPriceOnStreetAlley);
                            AddToApplicationCache(key + "_msg",
                                "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                                listId.Count + "</a></strong> căn Hẻm trên đoạn đường giới hạn +-100 số nhà MT.");
                            AddToApplicationCache(key + "_list", listId);

                            if (_debugUnitPriceOnStreetAlley)
                                Services.Notifier.Information(
                                    T(
                                        "Lấy ít nhất  " + minPropertiesRequired +
                                        "  căn Hẻm trên đoạn đường giới hạn +-100 số nhà MT take {0}",
                                        (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

                            return unitPriceOnStreetAlley;
                        }
                    }
                }

                #endregion

                #region CÙNG PHƯỜNG

                // Lấy ít nhất 5 căn cả hẻm cùng phường
                listAlleyProperties = pList.Where(e => e.Ward.Id == wardId).ToList();
                minPropertiesRequired = int.Parse(GetSetting("DGH_Cung_Phuong") ?? "5");
                if (listAlleyProperties.Count >= minPropertiesRequired)
                {
                    unitPriceOnStreetAlley = UnitPriceAvg(listAlleyProperties, ref listId);
                    if (listId.Count >= minPropertiesRequired)
                    {
                        AddToApplicationCache(key, unitPriceOnStreetAlley);
                        AddToApplicationCache(key + "_msg",
                            "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" +
                            listId.Count + "</a></strong> căn Hẻm cùng phường.");
                        AddToApplicationCache(key + "_list", listId);

                        if (_debugUnitPriceOnStreetAlley)
                            Services.Notifier.Information(
                                T("Lấy ít nhất  " + minPropertiesRequired + "  căn Hẻm cùng phường take {0}",
                                    (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

                        return unitPriceOnStreetAlley;
                    }
                }

                #endregion
            }

            #region Hẻm TƯƠNG ĐƯƠNG

            // Lấy ít nhất 5 căn Hẻm nằm trên các đường có giá trị tương đương
            minPropertiesRequired = int.Parse(GetSetting("DGH_GT_Tuong_Duong") ?? "5");
            unitPriceOnStreetAlley = UnitPriceFromMatrixRelations(typeGroupCssClass, districtId, wardId, streetId,
                minPropertiesRequired, false, true, getInternalOnly, ref listId);
            //_unitPriceOnStreetAlley = UnitPriceFromRelations(districtId, wardId, streetId, minPropertiesRequired, false, ref listId);
            if (listId.Count >= minPropertiesRequired)
            {
                AddToApplicationCache(key, unitPriceOnStreetAlley);
                AddToApplicationCache(key + "_msg",
                    "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" + listId.Count +
                    "</a></strong> căn Hẻm nằm trên các đường có giá trị tương đương.");
                AddToApplicationCache(key + "_list", listId);

                if (_debugUnitPriceOnStreetAlley)
                    Services.Notifier.Information(
                        T(
                            "Lấy ít nhất  " + minPropertiesRequired +
                            "  căn Hẻm nằm trên các đường có giá trị tương đương take {0}",
                            (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

                return unitPriceOnStreetAlley;
            }

            #endregion

            #region Hẻm + MT TƯƠNG ĐƯƠNG

            // Lấy ít nhất 5 căn cả hẻm và MT nằm trên các đường có giá trị tương đương
            minPropertiesRequired = int.Parse(GetSetting("DGH_GT_Tuong_Duong") ?? "5");

            unitPriceOnStreetAlley = UnitPriceFromMatrixRelations(typeGroupCssClass, districtId, wardId, streetId,
                minPropertiesRequired, true, true, getInternalOnly, ref listId);
            //_unitPriceOnStreetAlley = UnitPriceFromRelations(districtId, wardId, streetId, minPropertiesRequired, false, ref listId);
            if (listId.Count >= minPropertiesRequired)
            {
                AddToApplicationCache(key, unitPriceOnStreetAlley);
                AddToApplicationCache(key + "_msg",
                    "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" + listId.Count +
                    "</a></strong> căn cả Hẻm và MT nằm trên các đường có giá trị tương đương.");
                AddToApplicationCache(key + "_list", listId);

                if (_debugUnitPriceOnStreetAlley)
                    Services.Notifier.Information(
                        T(
                            "Lấy ít nhất  " + minPropertiesRequired +
                            "  căn cả Hẻm và MT nằm trên các đường có giá trị tương đương take {0}",
                            (DateTime.Now - startUnitPriceOnStreetAlley).TotalSeconds));

                return unitPriceOnStreetAlley;
            }

            #endregion

            if (getInternalOnly)
            {
                // BĐS nội bộ không đủ, phải lấy từ các group khác
                if (_debugUnitPriceOnStreetAlley)
                    Services.Notifier.Warning(T("BĐS nội bộ không đủ, phải lấy từ các group khác"));
                return UnitPriceOnStreetAlley(id, typeGroupCssClass, districtId, wardId, streetId, alleyNumber, false);
            }

            return 0;
        }

        public double UnitPriceFromMatrixRelations(string typeGroupCssClass, int districtId, int wardId, int streetId,
            int minPropertiesRequired, bool getFront, bool getAlley, bool getInternalOnly, ref List<int> listId)
        {
            var listPrice = new List<double>();
            listId = new List<int>();

            var listUsed = new List<StreetRelationPart>();
            var listCurrent = new List<StreetRelationEntry>();
            var listWaiting = new List<StreetRelationEntry>();

            int currentDistrictId = districtId;
            int currentWardId = wardId;
            int currentStreetId = streetId;

            if (
                _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                    .Where(
                        a =>
                            a.District.Id == currentDistrictId && a.Ward.Id == currentWardId &&
                            a.Street.Id == currentStreetId)
                    .Count() > 0)
            {
                int generation = 0;
                int maxDepthRelation = int.Parse(GetSetting("DG_Gioi_Han_Duong_Tuong_Duong") ?? "3");

                // Node is a Child

                #region Node is a Child

                if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Information(T("C: Node is a Child"));

                StreetRelationPart currentRelation =
                    _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                        .Where(
                            a =>
                                a.District.Id == currentDistrictId && a.Ward.Id == currentWardId &&
                                a.Street.Id == currentStreetId)
                        .List()
                        .FirstOrDefault();

                listCurrent.Add(new StreetRelationEntry
                {
                    StreetRelation = currentRelation,
                    AssociatedFrontValue = 100,
                    AssociatedFrontMsg = "100",
                    AssociatedAlleyValue = 100,
                    AssociatedAlleyMsg = "100"
                });

                while (listPrice.Count < minPropertiesRequired && listCurrent.Count > 0 &&
                       generation <= maxDepthRelation)
                {
                    foreach (StreetRelationEntry entry in listCurrent)
                    {
                        StreetRelationPart relation = entry.StreetRelation;
                        if (!listUsed.Contains(relation))
                        {
                            // Cho vào danh sách đã xử lý
                            listUsed.Add(relation);
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Information(
                                    T("C{0}: Node đang xử lý đường {1} = (MT {2}% / H {3}%) đường {4}", generation,
                                        relation.DisplayForStreetName, relation.RelatedValue, relation.RelatedAlleyValue,
                                        relation.DisplayForRelatedStreetName));

                            #region Xử lý entry

                            // Lấy tất cả các căn cùng Đường, Phường, Quận hiện tại
                            List<PropertyPart> pList =
                                GetPropertyList(typeGroupCssClass, relation.District.Id, relation.Ward.Id,
                                    relation.Street.Id, null, getFront, getAlley, getInternalOnly).ToList();
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Information(T("C{0}: Xét {1} BĐS thuộc đường {2}", generation,
                                    pList.Count(), relation.DisplayForStreetName));
                            double percentFront = entry.AssociatedFrontValue;
                            double percentAlley = entry.AssociatedAlleyValue;

                            // Tính đơn giá theo % chuyển đổi

                            #region Tính đơn giá theo % chuyển đổi

                            foreach (PropertyPart item in pList)
                            {
                                double unitPrice = UnitPrice(item);
                                if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                                {

                                    var locationFront = GetLocation("h-front");

                                    if (item.Location.Id == locationFront.Id)
                                    {
                                        // BĐS tham chiếu là MT
                                        double unitPriceConverted = unitPrice*(percentFront/100);
                                        listPrice.Add(unitPriceConverted);
                                        listId.Add(item.Id);
                                        if (_debugUnitPriceFromMatrixRelations)
                                            Services.Notifier.Warning(
                                                T(
                                                    "C{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (MT {5}% - Msg: {6})",
                                                    generation, item.DisplayForAddressForOwner, item.DisplayForAreaTotal,
                                                    String.Format("{0:#,0.###}", unitPrice),
                                                    String.Format("{0:#,0.###}", unitPriceConverted),
                                                    String.Format("{0:#,0}", percentFront), entry.AssociatedFrontMsg));
                                    }
                                    else
                                    {
                                        // BĐS tham chiếu là Hẻm
                                        double unitPriceConverted = unitPrice*(percentAlley/100);
                                        listPrice.Add(unitPriceConverted);
                                        listId.Add(item.Id);
                                        if (_debugUnitPriceFromMatrixRelations)
                                            Services.Notifier.Warning(
                                                T(
                                                    "C{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (H {5}% - Msg: {6})",
                                                    generation, item.DisplayForAddressForOwner, item.DisplayForAreaTotal,
                                                    String.Format("{0:#,0.###}", unitPrice),
                                                    String.Format("{0:#,0.###}", unitPriceConverted),
                                                    String.Format("{0:#,0}", percentAlley), entry.AssociatedAlleyMsg));
                                    }
                                }
                            }

                            #endregion

                            #region Hàng đợi

                            // Lấy Node cha vào hàng đợi

                            #region Lấy Node cha vào hàng đợi

                            List<StreetRelationPart> ancestorRelation =
                                _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                                    .Where(
                                        a =>
                                            a.District == relation.RelatedDistrict && a.Ward == relation.RelatedWard &&
                                            a.Street == relation.RelatedStreet)
                                    .List()
                                    .ToList();
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Information(T("C{0}: Có {1} Node cha của đường {2}", generation,
                                    ancestorRelation.Count(), relation.DisplayForStreetName));
                            if (ancestorRelation != null && ancestorRelation.Count > 0)
                            {
                                listWaiting.AddRange(from record in ancestorRelation
                                    where !listUsed.Contains(record)
                                    select new StreetRelationEntry
                                    {
                                        StreetRelation = record,
                                        AssociatedFrontValue = ((entry.AssociatedFrontValue*relation.RelatedValue)/100),
                                        AssociatedFrontMsg =
                                            entry.AssociatedFrontMsg + "x" +
                                            relation.RelatedValue.ToString(CultureInfo.InvariantCulture),
                                        AssociatedAlleyValue =
                                            ((entry.AssociatedAlleyValue*
                                              (relation.RelatedAlleyValue ?? relation.RelatedValue))/100),
                                        AssociatedAlleyMsg =
                                            entry.AssociatedAlleyMsg + "x" +
                                            (relation.RelatedAlleyValue ?? relation.RelatedValue).ToString(
                                                CultureInfo.InvariantCulture),
                                    });
                            }
                            else
                            {
                                // Không có node cha
                                if (relation.RelatedDistrict != null && relation.RelatedWard != null &&
                                    relation.RelatedStreet != null)
                                {
                                    if (_debugUnitPriceFromMatrixRelations)
                                        Services.Notifier.Information(T("C{0}: Tạo Node cha tạm thời của đường {1}",
                                            generation, relation.DisplayForRelatedStreetName));
                                    // Tạo node cha giả
                                    var fakeRelationPartRecord =
                                        Services.ContentManager.New<StreetRelationPart>("StreetRelation");
                                    fakeRelationPartRecord.District = relation.RelatedDistrict;
                                    fakeRelationPartRecord.Ward = relation.RelatedWard;
                                    fakeRelationPartRecord.Street = relation.RelatedStreet;
                                    fakeRelationPartRecord.RelatedValue = 100;
                                    fakeRelationPartRecord.RelatedAlleyValue = 100;
                                    fakeRelationPartRecord.RelatedDistrict = null;
                                    fakeRelationPartRecord.RelatedWard = null;
                                    fakeRelationPartRecord.RelatedStreet = null;

                                    if (!listUsed.Contains(fakeRelationPartRecord))
                                    {
                                        if (_debugUnitPriceFromMatrixRelations)
                                            Services.Notifier.Information(T(
                                                "C{0}: Tạo Node cha tạm thời của đường {1}", generation,
                                                relation.DisplayForRelatedStreetName));
                                        listWaiting.Add(new StreetRelationEntry
                                        {
                                            StreetRelation = fakeRelationPartRecord,
                                            AssociatedFrontValue =
                                                ((entry.AssociatedFrontValue*relation.RelatedValue)/100),
                                            AssociatedFrontMsg =
                                                entry.AssociatedFrontMsg + "x" +
                                                relation.RelatedValue.ToString(CultureInfo.InvariantCulture),
                                            AssociatedAlleyValue =
                                                ((entry.AssociatedAlleyValue*
                                                  (relation.RelatedAlleyValue ?? relation.RelatedValue))/100),
                                            AssociatedAlleyMsg =
                                                entry.AssociatedAlleyMsg + "x" +
                                                (relation.RelatedAlleyValue ?? relation.RelatedValue).ToString(
                                                    CultureInfo.InvariantCulture),
                                        });
                                    }
                                }
                                // Lấy các BĐS của relatedStreet đưa vào tính toán
                                // Lấy tất cả các căn cùng Đường, Phường, Quận related của node hiện tại

                                #region

                                //pList = GetPropertyList(relation.RelatedDistrict.Id, relation.RelatedWard.Id, relation.RelatedStreet.Id, getFront, getAlley, getInternalOnly).ToList();
                                //if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Information(T("C{0}: Xét {1} BĐS thuộc đường {2}", _generation, pList.Count(), relation.DisplayForRelatedStreetName));
                                //_percentFront = entry.AssociatedFrontValue * relation.RelatedValue / 100;
                                //_percentAlley = entry.AssociatedAlleyValue * (relation.RelatedAlleyValue ?? relation.RelatedValue) / 100;

                                //foreach (var item in pList)
                                //{
                                //    double unitPrice = UnitPrice(item);
                                //    if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                                //    {
                                //        if (item.Location.CssClass == "h-front")
                                //        {
                                //            // BĐS tham chiếu là MT
                                //            double unitPriceConverted = unitPrice * (_percentFront / 100);
                                //            _listPrice.Add(unitPriceConverted);
                                //            listId.Add(item.Id);
                                //            if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Warning(T("C{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (MT {5}% - Msg: {6})", _generation, item.DisplayForAddressForOwner, item.DisplayForAreaTotal, String.Format("{0:#,0.###}", unitPrice), String.Format("{0:#,0.###}", unitPriceConverted), String.Format("{0:#,0}", _percentFront), entry.AssociatedFrontMsg + "x" + relation.RelatedValue.ToString()));
                                //        }
                                //        else
                                //        {
                                //            // BĐS tham chiếu là Hẻm
                                //            double unitPriceConverted = unitPrice * (_percentAlley / 100);
                                //            _listPrice.Add(unitPriceConverted);
                                //            listId.Add(item.Id);
                                //            if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Warning(T("C{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (H {5}% - Msg: {6})", _generation, item.DisplayForAddressForOwner, item.DisplayForAreaTotal, String.Format("{0:#,0.###}", unitPrice), String.Format("{0:#,0.###}", unitPriceConverted), String.Format("{0:#,0}", _percentAlley), entry.AssociatedAlleyMsg + "x" + (relation.RelatedAlleyValue ?? relation.RelatedValue).ToString()));
                                //        }
                                //    }
                                //}

                                #endregion

                                // Lấy các node cùng cha với node hiện tại
                                //var siblingRelation = _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>().Where(a => a.RelatedDistrict == relation.RelatedDistrict && a.RelatedWard == relation.RelatedWard && a.RelatedStreet == relation.RelatedStreet).List().Select(a => a.Record);
                                //if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Information(T("C{0}: Có {1} Node cùng cha của đường {2}", _generation, siblingRelation.Count(), relation.DisplayForStreetName));
                                //if (siblingRelation != null && siblingRelation.Count() > 0)
                                //{
                                //    foreach (var record in siblingRelation)
                                //    {
                                //        if (!UsedList.Contains(record))
                                //        {
                                //            WaitingList.Add(new StreetRelationEntry
                                //            {
                                //                StreetRelation = record,
                                //                AssociatedFrontValue = (relation.RelatedValue / record.RelatedValue) * 100,
                                //                AssociatedFrontMsg = relation.RelatedValue.ToString() + "/" + record.RelatedValue.ToString(),
                                //                AssociatedAlleyValue = ((relation.RelatedAlleyValue ?? relation.RelatedValue) / (record.RelatedAlleyValue ?? record.RelatedValue)) * 100,
                                //                AssociatedAlleyMsg = (relation.RelatedAlleyValue ?? relation.RelatedValue).ToString() + "/" + (record.RelatedAlleyValue ?? record.RelatedValue).ToString(),
                                //            });
                                //        }
                                //    }
                                //}
                            }

                            #endregion

                            // Lấy Node con vào hàng đợi

                            #region Lấy Node con vào hàng đợi

                            IEnumerable<StreetRelationPart> descendantRelation =
                                _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                                    .Where(
                                        a =>
                                            a.RelatedDistrict == relation.District &&
                                            a.RelatedWard == relation.Ward &&
                                            a.RelatedStreet == relation.Street)
                                    .List().ToList();
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Information(T("C{0}: Có {1} Node con của đường {2}", generation,
                                    descendantRelation.Count(), relation.DisplayForStreetName));
                            if (descendantRelation != null && descendantRelation.Any())
                            {
                                listWaiting.AddRange(from record in descendantRelation
                                    where !listUsed.Contains(record)
                                    select new StreetRelationEntry
                                    {
                                        StreetRelation = record,
                                        AssociatedFrontValue = ((entry.AssociatedFrontValue/record.RelatedValue)*100),
                                        AssociatedFrontMsg = entry.AssociatedFrontMsg + "/" + record.RelatedValue,
                                        AssociatedAlleyValue =
                                            ((entry.AssociatedAlleyValue/
                                              (record.RelatedAlleyValue ?? record.RelatedValue))*100),
                                        AssociatedAlleyMsg =
                                            entry.AssociatedAlleyMsg + "/" +
                                            (record.RelatedAlleyValue ?? record.RelatedValue),
                                    });
                            }

                            #endregion

                            #endregion

                            /* Kết thúc xử lý 1 entry */

                            #endregion
                        }
                        /* Không có trong bảng quan hệ */
                    }

                    if (_debugUnitPriceFromMatrixRelations)
                        Services.Notifier.Information(T("C{0}: Hàng đợi {1}", generation, listWaiting.Count));

                    // Đưa hàng đợi vào hàng xử lý tiếp theo
                    listCurrent = listWaiting;
                    listWaiting = new List<StreetRelationEntry>();
                    generation++;
                }

                #endregion
            }
            else if (
                _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                    .Where(
                        a =>
                            a.RelatedDistrict.Id == currentDistrictId && a.RelatedWard.Id == currentWardId &&
                            a.RelatedStreet.Id == currentStreetId)
                    .Count() > 0)
            {
                int generation = 0;
                int maxDepthRelation = int.Parse(GetSetting("DG_Gioi_Han_Duong_Tuong_Duong") ?? "3");

                // Node is a Father and dont have any ancestor

                #region Node is a Father

                if (_debugUnitPriceFromMatrixRelations)
                    Services.Notifier.Information(T("F: Node is a Father and dont have any ancestor"));

                // Lấy tất cả các căn cùng Đường, Phường, Quận hiện tại
                List<PropertyPart> curList =
                    GetPropertyList(typeGroupCssClass, currentDistrictId, currentWardId, currentStreetId, null,
                        getFront, getAlley, getInternalOnly).ToList();
                double curpercentFront = 100;
                double curpercentAlley = 100;

                // Calc value from Property list
                foreach (PropertyPart item in curList)
                {
                    double unitPrice = UnitPrice(item);
                    if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                    {

                        var locationFront = GetLocation("h-front");

                        if (item.Location.Id == locationFront.Id)
                        {
                            // BĐS tham chiếu là MT
                            double unitPriceConverted = unitPrice*(curpercentFront/100);
                            listPrice.Add(unitPriceConverted);
                            listId.Add(item.Id);
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Warning(
                                    T("F{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (MT {5}% - Msg: {6})",
                                        generation, item.DisplayForAddressForOwner, item.DisplayForAreaTotal,
                                        String.Format("{0:#,0.###}", unitPrice),
                                        String.Format("{0:#,0.###}", unitPriceConverted),
                                        String.Format("{0:#,0}", curpercentFront), curpercentFront));
                        }
                        else
                        {
                            // BĐS tham chiếu là Hẻm
                            double unitPriceConverted = unitPrice*(curpercentAlley/100);
                            listPrice.Add(unitPriceConverted);
                            listId.Add(item.Id);
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Warning(
                                    T("F{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (H {5}% - Msg: {6})",
                                        generation, item.DisplayForAddressForOwner, item.DisplayForAreaTotal,
                                        String.Format("{0:#,0.###}", unitPrice),
                                        String.Format("{0:#,0.###}", unitPriceConverted),
                                        String.Format("{0:#,0}", curpercentAlley), curpercentAlley));
                        }
                    }
                }

                // Lấy Node con vào hàng đợi
                IEnumerable<StreetRelationPart> descendantRelation =
                    _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                        .Where(
                            a =>
                                a.RelatedDistrict.Id == currentDistrictId &&
                                a.RelatedWard.Id == currentWardId &&
                                a.RelatedStreet.Id == currentStreetId)
                        .List().ToList();
                //if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Information(T("Có {0} Node con của đường {1}", descendantRelation.Count(), relation.DisplayForStreetName));
                if (descendantRelation != null && descendantRelation.Any())
                {
                    listCurrent.AddRange(from record in descendantRelation
                        where !listUsed.Contains(record)
                        select new StreetRelationEntry
                        {
                            StreetRelation = record,
                            AssociatedFrontValue = curpercentFront/record.RelatedValue*100,
                            AssociatedFrontMsg = string.Format("{0}/{1}", curpercentFront, record.RelatedValue),
                            AssociatedAlleyValue =
                                curpercentAlley/(record.RelatedAlleyValue ?? record.RelatedValue)*100,
                            AssociatedAlleyMsg =
                                string.Format("{0}/{1}", curpercentAlley,
                                    (record.RelatedAlleyValue ?? record.RelatedValue)),
                        });
                }
                generation++;

                while (listPrice.Count < minPropertiesRequired && listCurrent.Count > 0 &&
                       generation <= maxDepthRelation)
                {
                    foreach (StreetRelationEntry entry in listCurrent)
                    {
                        StreetRelationPart relation = entry.StreetRelation;
                        if (!listUsed.Contains(relation))
                        {
                            // Cho vào danh sách đã xử lý
                            listUsed.Add(relation);

                            // Lấy tất cả các căn cùng Đường, Phường, Quận hiện tại
                            List<PropertyPart> pList =
                                GetPropertyList(typeGroupCssClass, relation.District.Id, relation.Ward.Id,
                                    relation.Street.Id, null, getFront, getAlley, getInternalOnly).ToList();
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Information(T("F{0}: Xét {1} BĐS thuộc đường {2}", generation,
                                    pList.Count(), relation.DisplayForStreetName));
                            double percentFront = entry.AssociatedFrontValue;
                            double percentAlley = entry.AssociatedAlleyValue;

                            // Calc value from Property list
                            foreach (PropertyPart item in pList)
                            {
                                double unitPrice = UnitPrice(item);
                                if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                                {

                                    var locationFront = GetLocation("h-front");

                                    if (item.Location.Id == locationFront.Id)
                                    {
                                        // BĐS tham chiếu là MT
                                        double unitPriceConverted = unitPrice*(percentFront/100);
                                        listPrice.Add(unitPriceConverted);
                                        listId.Add(item.Id);
                                        if (_debugUnitPriceFromMatrixRelations)
                                            Services.Notifier.Warning(
                                                T(
                                                    "F{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (MT {5}% - Msg: {6})",
                                                    generation, item.DisplayForAddressForOwner,
                                                    item.DisplayForAreaTotal,
                                                    String.Format("{0:#,0.###}", unitPrice),
                                                    String.Format("{0:#,0.###}", unitPriceConverted),
                                                    String.Format("{0:#,0}", percentFront), entry.AssociatedFrontMsg));
                                    }
                                    else
                                    {
                                        // BĐS tham chiếu là Hẻm
                                        double unitPriceConverted = unitPrice*(percentAlley/100);
                                        listPrice.Add(unitPriceConverted);
                                        listId.Add(item.Id);
                                        if (_debugUnitPriceFromMatrixRelations)
                                            Services.Notifier.Warning(
                                                T(
                                                    "F{0}: BĐS {1} {2} unitPrice {3} --> ĐG chuyển đổi {4} (H {5}% - Msg: {6})",
                                                    generation, item.DisplayForAddressForOwner,
                                                    item.DisplayForAreaTotal,
                                                    String.Format("{0:#,0.###}", unitPrice),
                                                    String.Format("{0:#,0.###}", unitPriceConverted),
                                                    String.Format("{0:#,0}", percentAlley), entry.AssociatedAlleyMsg));
                                    }
                                }
                            }

                            // Lấy Node con vào hàng đợi
                            descendantRelation =
                                _contentManager.Query<StreetRelationPart, StreetRelationPartRecord>()
                                    .Where(
                                        a =>
                                            a.RelatedDistrict == relation.District &&
                                            a.RelatedWard == relation.Ward &&
                                            a.RelatedStreet == relation.Street)
                                    .List().ToList();
                            if (_debugUnitPriceFromMatrixRelations)
                                Services.Notifier.Information(T("F{0}: Có {1} Node con của đường {2}", generation,
                                    descendantRelation.Count(), relation.DisplayForStreetName));
                            if (descendantRelation.Any())
                            {
                                listWaiting.AddRange(from record in descendantRelation
                                    where !listUsed.Contains(record)
                                    select new StreetRelationEntry
                                    {
                                        StreetRelation = record,
                                        AssociatedFrontValue =
                                            ((entry.AssociatedFrontValue/record.RelatedValue)*100),
                                        AssociatedFrontMsg =
                                            entry.AssociatedFrontMsg + "/" +
                                            record.RelatedValue.ToString(CultureInfo.InvariantCulture),
                                        AssociatedAlleyValue =
                                            ((entry.AssociatedAlleyValue/
                                              (record.RelatedAlleyValue ?? record.RelatedValue))*100),
                                        AssociatedAlleyMsg =
                                            entry.AssociatedAlleyMsg + "/" +
                                            (record.RelatedAlleyValue ?? record.RelatedValue).ToString(
                                                CultureInfo.InvariantCulture),
                                    });
                            }
                        }
                        /* Kết thúc xử lý 1 entry */
                    }

                    if (_debugUnitPriceFromMatrixRelations)
                        Services.Notifier.Information(T("F{0}: Hàng đợi {1}", generation, listWaiting.Count));

                    // Đưa hàng đợi vào hàng xử lý tiếp theo
                    listCurrent = listWaiting;
                    listWaiting = new List<StreetRelationEntry>();
                    generation++;
                }

                #endregion
            }

            if (listPrice.Count >= minPropertiesRequired)
            {
                // Đã đủ dữ liệu để định giá
                if (_debugUnitPriceFromMatrixRelations)
                    Services.Notifier.Information(T("ĐỊNH GIÁ TỪ {0} BĐS", listPrice.Count));
                return UnitPriceAvg(listPrice);
            }

            // Không đủ dữ liệu để định giá
            if (_debugUnitPriceFromMatrixRelations) Services.Notifier.Information(T("KHÔNG ĐỦ DỮ LIỆU ĐỂ ĐỊNH GIÁ"));
            return 0;
        }

        #region UnitPriceFromRelations

        /*
        public double UnitPriceFromRelations(int districtId, int wardId, int streetId, int count, bool GetFrontOnly, ref List<int> listId)
        {
            List<double> _listPrice = new List<double>();
            listId = new List<int>();

            bool endOfAncestors = false;
            double ancestorPercent = 100;
            int ancestorDistrictId = districtId;
            int ancestorWardId = wardId;
            int ancestorStreetId = streetId;

            bool endOfDescendants = false;
            int descendantLevel = 0;

            var curDescendantStreets = _streetRelationRepository.Table.Where(a => a.District.Id == districtId && a.Ward.Id == wardId && a.Street.Id == streetId).ToList();

            while (_listPrice.Count < count && (!endOfAncestors || !endOfDescendants))
            {
                #region Ancestors

                if (!endOfAncestors)
                {
                    // Kiểm tra xem còn cấp cha trong bảng quan hệ không?
                    if (_streetRelationRepository.Table.Any(a => a.District.Id == ancestorDistrictId && a.Ward.Id == ancestorWardId && a.Street.Id == ancestorStreetId))
                    {
                        var ancestorStreet = _streetRelationRepository.Table.SingleOrDefault(a => a.District.Id == ancestorDistrictId && a.Ward.Id == ancestorWardId && a.Street.Id == ancestorStreetId);

                        // Lấy thông tin quan hệ cấp cha
                        ancestorDistrictId = ancestorStreet.RelatedDistrict.Id;
                        ancestorWardId = ancestorStreet.RelatedWard.Id;
                        ancestorStreetId = ancestorStreet.RelatedStreet.Id;
                        ancestorPercent = (ancestorPercent / 100) * (ancestorStreet.RelatedValue / 100) * 100;

                        // Tính tất cả các BĐS cùng cha
                        List<StreetRelationPartRecord> descendantStreets = _streetRelationRepository.Table.Where(a => a.RelatedDistrict.Id == ancestorStreet.RelatedDistrict.Id && a.RelatedWard.Id == ancestorStreet.RelatedWard.Id && a.RelatedStreet.Id == ancestorStreet.RelatedStreet.Id).ToList();
                        foreach (var des in descendantStreets)
                        {
                            List<PropertyPart> pListD = GetPropertyList(des.District.Id, des.Ward.Id, des.Street.Id, GetFrontOnly);
                            double _percent = des.RelatedValue;

                            // Calc value from Property list
                            foreach (var item in pListD)
                            {
                                var unitPrice = UnitPrice(item);
                                if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                                {
                                    unitPrice = unitPrice * ((ancestorPercent / 100) / (_percent / 100));
                                    _listPrice.Add(unitPrice);
                                    listId.Add(item.Id);
                                }
                            }
                        }
                    }
                    else
                    {
                        endOfAncestors = true;

                        // Calc current Property
                        List<PropertyPart> pList = GetPropertyList(ancestorDistrictId, ancestorWardId, ancestorStreetId, GetFrontOnly);
                        foreach (var item in pList)
                        {
                            var unitPrice = UnitPrice(item);
                            if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                            {
                                unitPrice = unitPrice * ancestorPercent / 100;
                                _listPrice.Add(unitPrice);
                                listId.Add(item.Id);
                            }
                        }
                    }
                }

                #endregion

                #region Descendants

                if (!endOfDescendants)
                {
                    List<StreetRelationPartRecord> newDescendantStreets = new List<StreetRelationPartRecord>();
                    // Calc value of descendants
                    foreach (var descendant in curDescendantStreets)
                    {
                        if (_streetRelationRepository.Table.Any(a => a.RelatedDistrict.Id == descendant.District.Id && a.RelatedWard.Id == descendant.Ward.Id && a.RelatedStreet.Id == descendant.Street.Id))
                        {
                            List<StreetRelationPartRecord> descendantStreets = _streetRelationRepository.Table.Where(a => a.RelatedDistrict.Id == descendant.District.Id && a.RelatedWard.Id == descendant.Ward.Id && a.RelatedStreet.Id == descendant.Street.Id).ToList();
                            newDescendantStreets.AddRange(descendantStreets); // Đưa các Node con vào hàng chờ để xét lần lặp sau
                            foreach (var des in descendantStreets)
                            {
                                List<PropertyPart> pListD = GetPropertyList(des.District.Id, des.Ward.Id, des.Street.Id, GetFrontOnly);

                                // Calc related percent
                                double _percent = des.RelatedValue;
                                var cur = des;
                                for (int i = descendantLevel; i > 0; i--)
                                {
                                    var parent = _streetRelationRepository.Table.SingleOrDefault(a => a.District.Id == cur.RelatedDistrict.Id && a.Ward.Id == cur.RelatedWard.Id && a.Street.Id == cur.RelatedStreet.Id);
                                    _percent = (_percent / 100) * (parent.RelatedValue / 100) * 100;
                                    cur = parent;
                                }

                                // Calc value from Property list
                                foreach (var item in pListD)
                                {
                                    var unitPrice = UnitPrice(item);
                                    if (unitPrice > 0 && !double.IsInfinity(unitPrice))
                                    {
                                        unitPrice = unitPrice / (_percent / 100);
                                        _listPrice.Add(unitPrice);
                                        listId.Add(item.Id);
                                    }
                                }
                            }
                        }
                    }

                    descendantLevel++;
                    // Check if end of descendants
                    if (newDescendantStreets.Count > 0)
                        curDescendantStreets = newDescendantStreets;
                    else
                        endOfDescendants = true;
                }

                #endregion
            }

            if (_listPrice.Count >= count)
            {
                return UnitPriceAvg(_listPrice);
            }
            return 0;
        }
        */

        #endregion

        #endregion

        // String Extensions

        // AddressNumber Extensions

        // ESTIMATE

        // COEFFICIENTS

        public double UnitPriceOnApartment(PropertyPart p, bool getInternalOnly)
        {
            string key = p.Id.ToString(CultureInfo.InvariantCulture);
            //if (id == 0) key = String.Join("_", district.Id, ward.Id, street.Id, number);

            // Kiểm tra trong dữ liệu tạm

            #region CACHE

            double unitPriceOnStreet = GetUnitPriceFromApplicationCache(key);
            if (unitPriceOnStreet > 0)
            {
                string msg = GetUnitPriceMsgFromApplicationCache(key + "_msg");
                if (!msg.Contains("CACHE:")) AddToApplicationCache(key + "_msg", "CACHE: " + msg);
                return unitPriceOnStreet;
            }

            #endregion

            var listId = new List<int>();
            int minPropertiesRequired = int.Parse(GetSetting("DGCC_Cung_Chung_Cu") ?? "3");

            // Cùng tên Chung cư
            List<PropertyPart> pList =
                GetPropertyList("gp-apartment", p.District.Id, null, null, p.Apartment.Id, true, true, getInternalOnly)
                    .ToList();

            #region CÙNG CHUNG CƯ

            if (pList.Count >= minPropertiesRequired)
            {
                unitPriceOnStreet = UnitPriceAvg(pList, ref listId);
                if (listId.Count >= minPropertiesRequired)
                {
                    AddToApplicationCache(key, unitPriceOnStreet);
                    AddToApplicationCache(key + "_msg",
                        "Định giá từ <strong><a target='_blank' href='" + GetUrl(key + "_list") + "'>" + listId.Count +
                        "</a></strong> căn cùng chung cư.");
                    AddToApplicationCache(key + "_list", listId);
                    return unitPriceOnStreet;
                }
            }

            #endregion

            // Cùng tên Chung cư hoặc cùng Địa chỉ

            // Lấy theo bảng quan hệ lập sẵn

            // Cùng Đường

            // Cùng Phường

            // Cùng Quận

            return 0;
        }

        #region GET PROPERTY LIST

        public IEnumerable<PropertyPart> GetPropertyList(string typeGroupCssClass, int? districtId, int? wardId,
            int? streetId, int? apartmentId, bool getFront, bool getAlley, bool getInternalOnly)
        {
            // Refresh every 23 hours or when signal was triggered
            string cacheKey = "EstimateProperties_" + String.Join("_",
                typeGroupCssClass ?? "null",
                districtId > 0 ? districtId.ToString() : "null",
                wardId > 0 ? wardId.ToString() : "null",
                streetId > 0 ? streetId.ToString() : "null",
                apartmentId > 0 ? apartmentId.ToString() : "null",
                getFront.ToString(),
                getAlley.ToString(),
                getInternalOnly.ToString());

            return _cacheManager.Get(cacheKey, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When(cacheKey + "_Expired"));

                #region Query Properties

                DateTime startGetAllProperties = DateTime.Now;

                IContentQuery<PropertyPart, PropertyPartRecord> pList = AllProperties(typeGroupCssClass);

                if (_degubGetProperty)
                    Services.Notifier.Information(T("Lấy tất cả BĐS {0}",
                        (DateTime.Now - startGetAllProperties).TotalSeconds));

                DateTime startGetPropertyFilter = DateTime.Now;

                if (getInternalOnly)
                {
                    // UserGroup DPH
                    UserGroupPart userGroup =
                        Services.ContentManager.Query<UserGroupPart, UserGroupPartRecord>()
                            .Where(a => a.ShortName == "DPH")
                            .List()
                            .First();
                    if (userGroup != null)
                        pList = pList.Where(a => a.UserGroup != null && a.UserGroup.Id == userGroup.Id);
                }

                if (districtId > 0)
                {
                    pList = pList.Where(a => a.District != null && a.District.Id == districtId);
                }
                if (wardId > 0)
                {
                    pList = pList.Where(a => a.Ward != null && a.Ward.Id == wardId);
                }
                if (streetId > 0)
                {
                    pList = pList.Where(a => a.Street != null && a.Street.Id == streetId);
                }
                if (apartmentId > 0)
                {
                    pList = pList.Where(a => a.Apartment != null && a.Apartment.Id == apartmentId);
                }

                if (typeGroupCssClass == "gp-house")
                {

                    var locationFront = GetLocation("h-front");

                    if (getFront && !getAlley)
                    {
                        pList = pList.Where(a => a.Location.Id == locationFront.Id);
                    }
                    if (!getFront && getAlley)
                    {
                        pList = pList.Where(a => a.Location.Id != locationFront.Id);
                    }
                }

                if (_degubGetProperty)
                    Services.Notifier.Information(T("Lọc BĐS theo điều kiện {0} take {1}", cacheKey,
                        (DateTime.Now - startGetPropertyFilter).TotalSeconds));

                #endregion

                return pList.List();
            });
        }

        public IContentQuery<PropertyPart, PropertyPartRecord> AllProperties(string typeGroupCssClass)
            //public IEnumerable<PropertyPart> AllProperties()
        {
            // Refresh every 23 hours or when signal was triggered
            //string cacheKey = "EstimateProperties_All";
            //return _cacheManager.Get(cacheKey, ctx =>
            //{
            //ctx.Monitor(_clock.When(TimeSpan.FromMinutes(60 * 23)));
            //ctx.Monitor(_signals.When(cacheKey + "_Expired"));

            DateTime startGetConditionList = DateTime.Now;

            // LastUpdatedDate
            DateTime needToUpdateDate = DateTime.Now.AddDays(-double.Parse(GetSetting("DaysToUpdatePrice") ?? "90"));

            // AdsType
            int adsTypeId =
                Services.ContentManager.Query<AdsTypePart, AdsTypePartRecord>()
                    .Where(a => a.CssClass == "ad-selling")
                    .List()
                    .First()
                    .Id;

            // Status
            var includeList = new List<string> {"st-selling", "st-negotiate", "st-trading", "st-sold"};
            // RAO BÁN, THƯƠNG LƯỢNG, CHỜ GIAO DỊCH, ĐÃ BÁN
            List<int> statusIds = GetStatus().Where(a => includeList.Contains(a.CssClass)).Select(a => a.Id).ToList();

            if (_degubGetProperty)
                Services.Notifier.Information(T("Lấy các điều kiện lọc {0}",
                    (DateTime.Now - startGetConditionList).TotalSeconds));

            DateTime startGetPropertyList = DateTime.Now;

            IContentQuery<PropertyPart, PropertyPartRecord> pList = _contentManager
                .Query<PropertyPart, PropertyPartRecord>() //_pTypeHouseRepository.Table // chỉ lấy Nhà ở, đất ở
                .Where(a => a.LastUpdatedDate > needToUpdateDate)
                // Chỉ lấy các BĐS mới cập nhật trong thời gian 90 (Config) ngày trở lại
                .Where(a => a.AdsType != null && a.AdsType.Id == adsTypeId) // Chỉ lấy các BĐS rao bán
                .Where(e => statusIds.Contains(e.Status.Id))
                // Chỉ lấy các BĐS ĐANG RAO BÁN, ĐANG THƯƠNG LƯỢNG, CHỜ GIAO DỊCH, ĐÃ BÁN 
                .Where(a => a.PriceProposed != null && a.PriceProposed > 0 && a.IsExcludeFromPriceEstimation == false)
                ;

            // Types
            if (typeGroupCssClass == "gp-house")
            {
                // Các loại BĐS khi rao bán: Đất thổ cư; Nhà phố; Biệt thự; Khách sạn; Cao ốc Văn Phòng; Kho xưởng
                var propertyTypeList = new List<string>
                {
                    "tp-residential-land",
                    "tp-house",
                    "tp-concrete-house",
                    "tp-villa",
                    "tp-office-building",
                    "tp-hotel",
                    "tp-warehouse-workshop"
                };
                List<int> propertyTypeIds =
                    GetTypes().Where(a => propertyTypeList.Contains(a.CssClass)).Select(a => a.Id).ToList();

                pList =
                    pList.Where(a => propertyTypeIds.Contains(a.Type.Id))
                        .Where(
                            a => a.District != null && a.Ward != null && a.Street != null && a.AddressNumber != null);
            }
            else if (typeGroupCssClass == "gp-apartment")
            {
                // Các loại CC khi rao bán: Chung cư; Căn hộ cao cấp
                var propertyTypeList = new List<string> {"tp-apartment", "tp-luxury-apartment"};
                List<int> propertyTypeIds =
                    GetTypes().Where(a => propertyTypeList.Contains(a.CssClass)).Select(a => a.Id).ToList();

                pList =
                    pList.Where(a => propertyTypeIds.Contains(a.Type.Id))
                        .Where(a => a.Apartment != null && a.ApartmentFloorTh > 0 && a.AreaUsable > 0);
            }

            if (_degubGetProperty)
                Services.Notifier.Information(T("Lấy BĐS có address, có giá rao, loại gp-house {0}",
                    (DateTime.Now - startGetPropertyList).TotalSeconds));

            return pList; //.List();

            //});
        }

        #endregion

        #region CACHE CONTROL

        public double GetUnitPriceFromApplicationCache(string key)
        {
            //return double.Parse(HttpContext.Current.Application[key] != null ? HttpContext.Current.Application[key].ToString() : "0");
            if (HttpContext.Current != null)
                return double.Parse(HttpContext.Current.Cache[key] != null ? HttpContext.Current.Cache[key].ToString() : "0");
            return 0;
        }

        public List<int> GetPropertyListFromApplicationCache(string key)
        {
            //return (HttpContext.Current.Application[key] != null ? HttpContext.Current.Application[key] as List<int> : new List<int>());

            if (HttpContext.Current != null)
                return (HttpContext.Current.Cache[key] != null
                ? HttpContext.Current.Cache[key] as List<int>
                : new List<int>());
            return new List<int>();
        }

        public string GetUnitPriceMsgFromApplicationCache(string key)
        {
            //return (HttpContext.Current.Application[key] != null ? HttpContext.Current.Application[key].ToString() : "N/A");

            if (HttpContext.Current != null)
                return (HttpContext.Current.Cache[key] != null ? HttpContext.Current.Cache[key].ToString() : "N/A");
            return "N/A";
        }

        public void AddToApplicationCache(string key, object value)
        {
            //HttpContext.Current.Application.Lock();
            //HttpContext.Current.Application[key] = value;
            //HttpContext.Current.Application.UnLock();

            if (HttpContext.Current != null)
                HttpContext.Current.Cache[key] = value;
        }

        public double GetFromApplicationCache(string key)
        {
            if (HttpContext.Current != null)
                return double.Parse(HttpContext.Current.Cache[key] != null ? HttpContext.Current.Cache[key].ToString() : "0");
            return 0;
        }

        public void ClearApplicationCache(int propertyId)
        {
            //HttpContext.Current.Application.Lock();

            //var listKey = HttpContext.Current.Application.AllKeys.Where(a => a.Contains(key)).ToList();

            //for (int i = listKey.Count - 1; i >= 0; i--)
            //{
            //    HttpContext.Current.Application.Remove(listKey[i]);
            //}

            //HttpContext.Current.Application.UnLock();

            if (HttpContext.Current != null)
            {
                var itemsToRemove = new List<string>();

                IDictionaryEnumerator enumerator = HttpContext.Current.Cache.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string enumeratorKey = enumerator.Key.ToString();
                    if (enumeratorKey == propertyId.ToString() || enumeratorKey.StartsWith(propertyId.ToString() + "_"))
                    {
                        itemsToRemove.Add(enumeratorKey);
                    }
                }

                foreach (string itemToRemove in itemsToRemove)
                {
                    HttpContext.Current.Cache.Remove(itemToRemove);
                }
            }

            var p = _contentManager.Get<PropertyPart>(propertyId);

            _signals.Trigger(p.Id + "_Changed");
            _signals.Trigger(p.Id + "_aCoeff" + "_Expired");

            // Clear ListProperties use to Estimate Cache
            if (p.TypeGroup == null || p.District == null || p.Ward == null || p.Street == null) return;
            if (p.Location == null) return;
            string typeGroupCssClass = p.TypeGroup != null ? p.TypeGroup.CssClass : "null";
            string districtId = p.District != null ? p.District.Id.ToString() : "null";
            string wardId = p.Ward != null ? p.Ward.Id.ToString() : "null";
            string streetId = p.Street != null ? p.Street.Id.ToString() : "null";
            string apartmentId = p.Apartment != null ? p.Apartment.Id.ToString() : "null";

            bool isInternal = !_propertyService.IsExternalProperty(p);

            #region gp-house

            #region CÙNG ĐƯỜNG, PHƯỜNG, QUẬN

            // Front only - Internal
            string cacheKeyUnitPriceAvgOnStreetFrontInternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, wardId, streetId,
                                   "null", true.ToString(), false.ToString(), true.ToString());
            _signals.Trigger(cacheKeyUnitPriceAvgOnStreetFrontInternal + "_Expired");

            // Alley only - Internal
            string cacheKeyUnitPriceAvgOnStreetAlleyInternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, wardId, streetId,
                                   "null", false.ToString(), true.ToString(), true.ToString());
            _signals.Trigger(cacheKeyUnitPriceAvgOnStreetAlleyInternal + "_Expired");

            // Front and Alley - Internal
            string cacheKeyUnitPriceAvgOnStreetFrontAlleyInternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, wardId, streetId,
                                   "null", true.ToString(), true.ToString(), true.ToString());
            _signals.Trigger(cacheKeyUnitPriceAvgOnStreetFrontAlleyInternal + "_Expired");

            // Front only - Internal + External
            string cacheKeyUnitPriceAvgOnStreetFrontInternalExternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, wardId, streetId,
                                   "null", true.ToString(), false.ToString(), false.ToString());
            _signals.Trigger(cacheKeyUnitPriceAvgOnStreetFrontInternalExternal + "_Expired");

            // Alley only - Internal + External
            string cacheKeyUnitPriceAvgOnStreetAlleyInternalExternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, wardId, streetId,
                                   "null", false.ToString(), true.ToString(), false.ToString());
            _signals.Trigger(cacheKeyUnitPriceAvgOnStreetAlleyInternalExternal + "_Expired");

            // Front and Alley - Internal + External
            string cacheKeyUnitPriceAvgOnStreetFrontAlleyInternalExternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, wardId, streetId,
                                   "null", true.ToString(), true.ToString(), false.ToString());
            _signals.Trigger(cacheKeyUnitPriceAvgOnStreetFrontAlleyInternalExternal + "_Expired");

            #endregion

            #region ĐOẠN ĐƯỜNG

            // ĐOẠN ĐƯỜNG - Internal
            string cacheKeyUnitPriceOnStreetInternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, "null", "null", streetId,
                                   "null", true.ToString(), false.ToString(), true.ToString());
            _signals.Trigger(cacheKeyUnitPriceOnStreetInternal + "_Expired");

            // ĐOẠN ĐƯỜNG - Internal + External
            string cacheKeyUnitPriceOnStreetExternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, "null", "null", streetId,
                                   "null", true.ToString(), false.ToString(), false.ToString());
            _signals.Trigger(cacheKeyUnitPriceOnStreetExternal + "_Expired");

            #endregion

            #region CÙNG ĐƯỜNG, QUẬN

            // CÙNG HẺM CÙNG ĐƯỜNG - Internal
            string cacheKeyUnitPriceOnStreetAlley = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, "null", streetId,
                                   "null", false.ToString(), true.ToString(), true.ToString());
            _signals.Trigger(cacheKeyUnitPriceOnStreetAlley + "_Expired");

            // CÙNG HẺM CÙNG ĐƯỜNG - Internal + External
            string cacheKeyUnitPriceOnStreetAlleyExternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, "null", streetId,
                                   "null", false.ToString(), true.ToString(), false.ToString());
            _signals.Trigger(cacheKeyUnitPriceOnStreetAlleyExternal + "_Expired");

            #endregion

            #endregion

            #region gp-apartment

            #region CÙNG CHUNG CƯ

            // CÙNG CHUNG CƯ - Internal
            string cacheKeyUnitPriceOnApartment = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, "null", "null",
                                   apartmentId, true.ToString(), true.ToString(), true.ToString());
            _signals.Trigger(cacheKeyUnitPriceOnApartment + "_Expired");

            // CÙNG CHUNG CƯ - Internal + External
            string cacheKeyUnitPriceOnApartmentExternal = "EstimateProperties_" +
                                                  String.Join("_", typeGroupCssClass, districtId, "null", "null",
                                   apartmentId, true.ToString(), true.ToString(), false.ToString());
            _signals.Trigger(cacheKeyUnitPriceOnApartmentExternal + "_Expired");

            #endregion

            #endregion
            
        }

        #endregion

        #region Helper

        public string GetUrl(string key)
        {
            return "/Admin/RealEstate?Options.List=" + key;
        }

        public PropertyFlagPart GetFlag(string flagCssClass)
        {
            if (string.IsNullOrEmpty(flagCssClass)) return null;
            return
                _contentManager.Query<PropertyFlagPart, PropertyFlagPartRecord>()
                    .Where(a => a.CssClass == flagCssClass)
                    .List()
                    .FirstOrDefault();
        }

        public IEnumerable<PropertyStatusPartRecord> GetStatus()
        {
            return
                _contentManager.Query<PropertyStatusPart, PropertyStatusPartRecord>()
                    .OrderBy(a => a.SeqOrder)
                    .List()
                    .Select(a => a.Record);
        }

        public PropertyTypeGroupPart GetTypeGroup(string typeGroupCssClass)
        {
            if (string.IsNullOrEmpty(typeGroupCssClass)) return null;
            return
                _contentManager.Query<PropertyTypeGroupPart, PropertyTypeGroupPartRecord>()
                    .Where(a => a.CssClass == typeGroupCssClass)
                    .List()
                    .FirstOrDefault();
        }

        public IEnumerable<PropertyTypePartRecord> GetTypes()
        {
            return
                _contentManager.Query<PropertyTypePart, PropertyTypePartRecord>()
                    .OrderBy(a => a.SeqOrder)
                    .List()
                    .Select(a => a.Record);
        }

        public PropertyLocationPart GetLocation(string locationCssClass)
        {
            if (string.IsNullOrEmpty(locationCssClass)) return null;
            return
                _contentManager.Query<PropertyLocationPart, PropertyLocationPartRecord>()
                    .Where(a => a.CssClass == locationCssClass)
                    .List()
                    .FirstOrDefault();
        }

        public string GetSetting(string settingName)
        {
            PropertySettingPart setting =
                _contentManager.Query<PropertySettingPart, PropertySettingPartRecord>()
                    .Where(a => a.Name == settingName)
                    .List()
                    .FirstOrDefault();
            if (setting != null) return setting.Value;
            return null;
        }

        public class StreetRelationEntry
        {
            public StreetRelationPart StreetRelation { get; set; }
            public double AssociatedFrontValue { get; set; }
            public string AssociatedFrontMsg { get; set; }
            public double AssociatedAlleyValue { get; set; }
            public string AssociatedAlleyMsg { get; set; }
            public bool IsAncestor { get; set; }
            public bool IsChecked { get; set; }
        }

        #endregion

        #region BackgroundTask

        public void BackgroundEstimate()
        {

            var startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
            int startIndex = 0;
            int endIndex = 0;

            EstimateRecord record;

            DateTime time = startTime;
            if (_estimateRepository.Fetch(r => r.StartTime >= time).Any())
            {
                record = _estimateRepository.Fetch(r => r.StartTime >= time).First();

                startTime = record.StartTime;
                startIndex = record.StartIndex;
                endIndex = record.EndIndex;
            }
            else
            {
                var lastRecord = _estimateRepository.Fetch(a => a.Id > 0, o => o.Desc(f => f.Id), 0, 1).ToList().First();

                record = new EstimateRecord
                {
                    StartTime = DateTime.Now,
                    StartIndex = lastRecord.EndIndex,
                    EndIndex = lastRecord.EndIndex,
                };
                _estimateRepository.Create(record);
            }

            #region Query Properties

            var pList = AllProperties("gp-house")
                    //.Where(a => a.Street.Id == 4918) // Đường Điện Biên Phủ
                    //.Where(a => a.Id == 12715)
                    .OrderBy(a => a.District.Id)
                    .OrderBy(a => a.Ward.Id)
                    .OrderBy(a => a.Street.Id)
                    .OrderBy(a => a.AlleyNumber)
                    ;

            UserGroupPart userGroup =
                _contentManager.Query<UserGroupPart, UserGroupPartRecord>()
                    .Where(a => a.ShortName == "DPH")
                    .List()
                    .First();
            if (userGroup != null)
                pList = pList.Where(a => a.UserGroup != null && a.UserGroup.Id == userGroup.Id);

            var result = pList
                //.List()
                .Slice(endIndex, Batch)
                .ToList();

            #endregion


            if (result.Count > 0)
            {

                int sucessCount = 0;
                var errorIds = new List<int>();
                string msg = "";

                foreach (PropertyPart item in result)
                {
                    try
                    {
                        DateTime startEstimateTime = DateTime.Now;

                        item.PriceEstimatedInVND = EstimatePrice(item);

                        if (item.PriceEstimatedInVND > 0)
                        {
                            sucessCount++;
                        }

                        msg = String.Format("Định giá BĐS {0} - {1} --> {2:#,0.### Tỷ} Take {3}", item.Id,
                            item.DisplayForAddressForOwner, item.PriceEstimatedInVND,
                            (DateTime.Now - startEstimateTime).TotalSeconds);
                    }
                    catch (Exception e)
                    {
                        errorIds.Add(item.Id);
                        msg = String.Format("Định giá BĐS {0} - Error: {1}", item.Id, e.Message);
                    }
                }

                // Update record

                record.StartTime = startTime;
                record.EndTime = DateTime.Now;

                record.StartIndex = startIndex;
                record.EndIndex = endIndex + Batch;

                record.TotalItems += Batch;
                record.SucessItems += sucessCount;

                record.Msg = msg;
                //if (errorIds.Count > 0)
                //    record.ErrorMsg += (String.IsNullOrEmpty(record.ErrorMsg) ? "" : ",") +
                //                       String.Join(",", errorIds);
            }
            else
            {
                record.EndIndex = 0;
                Logger.Error("BackgroundTaskFired.Count = 0 ");
            }

            // Save record
            _estimateRepository.Update(record);

            //Logger.Error("BackgroundTaskFired.Processing... " + result.Count);
        }

        #endregion
    }
}