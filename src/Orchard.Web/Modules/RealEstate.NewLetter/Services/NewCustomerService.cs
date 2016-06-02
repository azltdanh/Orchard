using Orchard;
using Orchard.ContentManagement;
using Orchard.Security;
using RealEstate.NewLetter.ViewModels;
using RealEstate.Models;
using RealEstate.NewLetter.Models;
using RealEstate.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealEstate.NewLetter.Services
{
    public interface INewCustomerService : IDependency
    {
        IContentQuery<PropertyPart, PropertyPartRecord> GetListFastFilterServices(InfomationAddressViewModel options);

        List<PropertyPart> PropertiesPart(int id);
        int PropertiesPartCount(int id);
        List<InfomationAddressViewModel> PropertiesPartInfo(int id);
        CustomerPart ICustomerPartById(int id);
        bool CheckCustomerEmailException(string email);
        bool CheckCustomerEmailExceptionTrue(string email);
        void SetStatusEmailException(string email);
        //string linksearch(int id);
    }

    public class NewCustomerService : INewCustomerService
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthenticationService _authenticationService;
        private readonly IPropertyService _propertyService;
        #region Init
        public NewCustomerService(IContentManager contentManager, IOrchardServices services, IAuthenticationService authenticationService, IPropertyService propertyService)
        {
            Services = services;
            _contentManager = contentManager;
            _propertyService = propertyService;
            _authenticationService = authenticationService;
        }
        public IOrchardServices Services { get; set; }
        #endregion

        public IContentQuery<PropertyPart, PropertyPartRecord> GetListFastFilterServices(InfomationAddressViewModel options)
        {
            int StatusApprovedId = _propertyService.GetStatus("st-approved").Id;
            int statusSelling = _propertyService.GetStatus("st-selling ").Id;
            List<string> statusCssClass = new List<string> { "st-selling", "st-approved", "st-new" }; // -- Đang rao bán -- (RAO BÁN, KH RAO BÁN)
            List<int> statusIds = _propertyService.GetStatus().Where(a => statusCssClass.Contains(a.CssClass)).Select(a => a.Id).ToList();
            var pList = Services.ContentManager
                .Query<PropertyPart, PropertyPartRecord>()
                .Where(p => statusIds.Contains(p.Status.Id) && p.Published == true)
                .OrderByDescending(p => p.AdsExpirationDate);
            if (options.PropertyId.HasValue)
            {
                pList = pList.Where(p => p.Id == options.PropertyId);
            }
            else
            {
                #region FILTER
                // AdsType
                if (!string.IsNullOrEmpty(options.AdsTypeCssClass))
                {
                    options.AdsTypeId = _propertyService.GetAdsType(options.AdsTypeCssClass).Id;
                }
                else
                {
                    if (options.AdsTypeId.HasValue)
                    {
                        pList = pList.Where(p => p.AdsType.Id == options.AdsTypeId);
                    }
                    else
                    {
                        options.AdsTypeId = _propertyService.GetAdsType("ad-selling").Id;
                    }
                }


                // Types
                if (options.TypeGroupId > 0)
                {
                    var TypeIds = _propertyService.GetTypes().Where(a => a.Group.Id == options.TypeGroupId).Select(a => a.Id).ToList();

                    if (options.flagAsideFirst == true) // Get by aside first link
                    {
                        if (options.TypeGroupId == _propertyService.GetTypeGroup("gp-land").Id)
                        {
                            var id = _propertyService.GetType("tp-residential-land").Id; // lay id dat o
                            TypeIds.Add(id);
                        }
                    }
                    pList = pList.Where(a => TypeIds.Contains(a.Type.Id));
                }
                if (options.TypeId.HasValue)
                {
                    pList = pList.Where(a => a.Type.Id == options.TypeId);
                }
                // Province
                if (options.ProvinceId > 0) pList = pList.Where(p => p.Province.Id == options.ProvinceId);

                //AdsExpirationDate

                // Districts
                if (options.DistrictIds != null)
                    if (options.DistrictIds.Count() > 0) pList = pList.Where(p => options.DistrictIds.Contains(p.District.Id));
                if (options.DistrictId.HasValue)
                    pList = pList.Where(p => p.District.Id == options.DistrictId);
                // Wards
                if (options.WardIds != null)
                    if (options.WardIds.Count() > 0) pList = pList.Where(p => options.WardIds.Contains(p.Ward.Id));
                if (options.WardId.HasValue)
                    pList = pList.Where(p => p.Ward.Id == options.WardId);
                // Streets
                if (options.StreetIds != null)
                    if (options.StreetIds.Count() > 0)
                    {
                        // Lấy tất cả BĐS trên Đường và Đoạn Đường
                        var listRelatedStreetIds = _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>().Where(a => options.StreetIds.Contains(a.RelatedStreet.Id)).List().Select(a => a.Id).ToList();
                        listRelatedStreetIds.AddRange(options.StreetIds);
                        pList = pList.Where(p => listRelatedStreetIds.Contains(p.Street.Id));
                    }
                if (options.StreetId.HasValue)
                    pList = pList.Where(p => p.Street.Id == options.StreetId);

                // Directions
                if (options.DirectionIds != null)
                    if (options.DirectionIds.Count() > 0) pList = pList.Where(p => options.DirectionIds.Contains(p.Direction.Id));

                // Area & Floor
                if (options.MinAreaTotal.HasValue) pList = pList.Where(p => p.AreaTotal >= options.MinAreaTotal);
                if (options.MinAreaTotalWidth.HasValue) pList = pList.Where(p => p.AreaTotalWidth >= options.MinAreaTotalWidth);
                if (options.MinAreaTotalLength.HasValue) pList = pList.Where(p => p.AreaTotalLength >= options.MinAreaTotalLength);
                if (options.MinFloors.HasValue) pList = pList.Where(p => p.Floors >= options.MinFloors);
                if (options.MinAreaUsable.HasValue) pList = pList.Where(p => p.AreaUsable >= options.MinAreaUsable);
                // Price
                #region TIM KIEM THEO GIA

                // Convert Price to VND
                if (options.PaymentMethodId != 0)
                {
                    string paymentMethodCssClass = _propertyService.GetPaymentMethod(options.PaymentMethodId).CssClass;

                    double MinPriceVND = options.MinPriceProposed.HasValue ? _propertyService.ConvertToVndB((double)options.MinPriceProposed, paymentMethodCssClass) : 0;
                    double MaxPriceVND = options.MaxPriceProposed.HasValue ? _propertyService.ConvertToVndB((double)options.MaxPriceProposed, paymentMethodCssClass) : 0;

                    if (MinPriceVND > 0) pList = pList.Where(p => p.PriceProposedInVND >= MinPriceVND);
                    if (MaxPriceVND > 0) pList = pList.Where(p => p.PriceProposedInVND <= MaxPriceVND);
                }

                //PaymentMethodPartRecord PaymentMethod = _propertyService.GetPaymentMethod("pm-vnd-b");
                //if (options.PaymentMethodId != 0)
                //{
                //    PaymentMethod = _propertyService.GetPaymentMethod(options.PaymentMethodId);
                //}
                //if (options.MinPriceProposed.HasValue)
                //    options.MinPriceProposedInVND = _propertyService.ConvertToVndB((double)options.MinPriceProposed, PaymentMethod.CssClass);
                //if (options.MaxPriceProposed.HasValue)
                //    options.MaxPriceProposedInVND = _propertyService.ConvertToVndB((double)options.MaxPriceProposed, PaymentMethod.CssClass);

                //if (options.MinPriceProposedInVND.HasValue) pList = pList.Where(p => p.PriceProposedInVND >= options.MinPriceProposedInVND);
                //if (options.MaxPriceProposedInVND.HasValue) pList = pList.Where(p => p.PriceProposedInVND <= options.MaxPriceProposedInVND);

                #endregion

                if (!String.IsNullOrEmpty(options.OtherProjectName))
                    pList = pList.Where(p => p.OtherProjectName.Contains(options.OtherProjectName));

                if (options.AdsGoodDeal == true)
                {
                    pList = pList.Where(p => p.AdsGoodDeal == true && p.AdsGoodDealExpirationDate >= DateTime.Now);
                }
                if (options.AdsVIP == true)
                {
                    pList = pList.Where(p => p.AdsVIP == true && p.AdsVIPExpirationDate >= DateTime.Now);
                }
                if (options.IsOwner == true)
                {
                    pList = pList.Where(p => p.IsOwner == true);
                }
                if (options.IsAuction == true)
                {
                    pList = pList.Where(p => p.IsAuction == true);
                }
                if (options.LocationId.HasValue)
                {
                    pList = pList.Where(p => p.Location.Id == options.LocationId);
                }
                #region ORDER WALK

                int LocationFront = _propertyService.GetLocation("h-front").Id;
                int LocationAlley = _propertyService.GetLocation("h-alley").Id;
                switch (options.OrderWalk)
                {
                    case InfomationAddressOrderWalk.All: // Tat ca cac vi tri
                        pList = pList.Where(p => p.Location.Id == LocationFront || p.Location.Id == LocationAlley);
                        break;
                    case InfomationAddressOrderWalk.AllWalk: // Mat Tien
                        pList = pList.Where(p => p.Location.Id == LocationFront);
                        break;
                    case InfomationAddressOrderWalk.Alley6: // hem 6m tro len
                        pList = pList.Where(p => p.Location.Id == LocationAlley && p.AlleyWidth >= 6);
                        break;
                    case InfomationAddressOrderWalk.Alley5:
                        pList = pList.Where(p => p.Location.Id == LocationAlley && p.AlleyWidth >= 5);
                        break;
                    case InfomationAddressOrderWalk.Alley4:
                        pList = pList.Where(p => p.Location.Id == LocationAlley && p.AlleyWidth >= 4);
                        break;
                    case InfomationAddressOrderWalk.Alley3:
                        pList = pList.Where(p => p.Location.Id == LocationAlley && p.AlleyWidth >= 3);
                        break;
                    case InfomationAddressOrderWalk.Alley2:
                        pList = pList.Where(p => p.Location.Id == LocationAlley && p.AlleyWidth >= 2);
                        break;
                    case InfomationAddressOrderWalk.Alley:
                        pList = pList.Where(p => p.Location.Id == LocationAlley);
                        break;
                    default: break;
                }

                #endregion

                #region ORDER APARTMENTFLOORTH
                switch (options.OrderApartmentFloorTh)
                {
                    case InfomationAddressOrderApartmentFloorTh.All:
                        break;
                    case InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh1To3:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 1 && p.ApartmentFloorTh <= 3);
                        break;
                    case InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh4To7:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 4 && p.ApartmentFloorTh <= 7);
                        break;
                    case InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh8To12:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 8 && p.ApartmentFloorTh <= 12);
                        break;
                    case InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh12:
                        pList = pList.Where(p => p.ApartmentFloorTh >= 12);
                        break;
                }

                #endregion

                #region BĐS GIÁ RẺ & GIAO DỊCH GẤP

                if (options.AnyType != null && options.AnyType.Count() > 0)
                {
                    if (options.AnyType.Contains(0))
                    {
                        // Tìm tất cả các Flags
                    }
                    else
                    {
                        if (options.AnyType.Contains(1) && options.AnyType.Contains(2) && options.AnyType.Contains(3) && options.AnyType.Contains(4))
                        {
                            // Tìm BĐS Giá rẻ hoặc Giao dịch gấp
                            pList = pList.Where(p => p.AdsGoodDeal == true || p.AdsVIP == true || p.IsOwner == true || !string.IsNullOrEmpty(p.AddressNumber) || p.IsAuction == true);
                        }
                        else
                        {
                            if (options.AnyType.Contains(1))
                            {
                                // Tìm BĐS Giá rẻ
                                pList = pList.Where(p => p.AdsGoodDeal == true);
                            }
                            if (options.AnyType.Contains(2))
                            {
                                // Tìm BĐS Giao dịch gấp
                                pList = pList.Where(p => p.AdsVIP == true);
                            }
                            if (options.AnyType.Contains(3))
                            {
                                //Tìm BĐS Chính chủ
                                pList = pList.Where(p => p.AddressNumber.Equals("") == false || p.IsOwner == true);
                            }
                            if (options.AnyType.Contains(4))
                            {
                                //Tìm BĐS phát mãi
                                pList = pList.Where(p => p.IsAuction == true);
                            }
                        }
                    }
                }

                #endregion

                #region Set TitleArticle
                if (options.AdsGoodDeal == true)
                {
                    options.TitleArticle = "Nhà đất giá rẻ";
                }
                if (options.AdsVIP == true)
                {
                    options.TitleArticle = "Nhà đất giao dịch gấp";
                }
                if (options.AdsNormal == true)
                {
                    options.TitleArticle = "Tin mới đăng";
                }
                if (string.IsNullOrEmpty(options.TitleArticle))
                {
                    options.TitleArticle = "Kết quả tìm kiếm";
                }
                #endregion

                if (!string.IsNullOrEmpty(options.SearchPhone))
                {
                    pList = pList.Where(p => p.ContactPhone.Contains(options.SearchPhone)
                        || p.ContactPhoneToDisplay.Contains(options.SearchPhone));
                }

                #endregion
            }


            return pList;
        }

        public List<PropertyPart> PropertiesPart(int id)
        {

            #region Get all Properties from customer's requirements

            var c = Services.ContentManager.Get<CustomerPart>(id);
            var listProperties = new List<PropertyPart>();
            var groupIds = c.Requirements.Where(r => r.IsEnabled).Select(r => r.GroupId).ToList();

            foreach (var groupId in groupIds)
            {
                var reqs = c.Requirements.Where(r => r.GroupId == groupId).ToList();
                var req = reqs.First();

                InfomationAddressViewModel options = new InfomationAddressViewModel();

                options.ProvinceId = req.LocationProvincePartRecord.Id;
                options.DistrictIds = reqs.Where(r => r.LocationDistrictPartRecord != null).Select(r => r.LocationDistrictPartRecord.Id).ToArray();
                options.WardIds = reqs.Where(r => r.LocationWardPartRecord != null).Select(r => r.LocationWardPartRecord.Id).ToArray();
                options.StreetIds = reqs.Where(r => r.LocationStreetPartRecord != null).Select(r => r.LocationStreetPartRecord.Id).ToArray();
                options.DirectionIds = reqs.Where(r => r.DirectionPartRecord != null).Select(r => r.DirectionPartRecord.Id).ToArray();

                PropertyLocationPartRecord LocationFront = _propertyService.GetLocation("h-front");
                PropertyLocationPartRecord LocationAlley = _propertyService.GetLocation("h-alley");

                if (req.PropertyLocationPartRecord == null) options.OrderWalk = InfomationAddressOrderWalk.All;

                if (req.PropertyLocationPartRecord == LocationFront) options.OrderWalk = InfomationAddressOrderWalk.AllWalk;
                if (req.PropertyLocationPartRecord == LocationAlley) options.OrderWalk = InfomationAddressOrderWalk.Alley;


                if (req.MinApartmentFloorTh == null && req.MaxApartmentFloorTh == null) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.All;
                if (req.MinApartmentFloorTh == 1 && req.MaxApartmentFloorTh == 3) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh1To3;
                if (req.MinApartmentFloorTh == 4 && req.MaxApartmentFloorTh == 7) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh4To7;
                if (req.MinApartmentFloorTh == 8 && req.MaxApartmentFloorTh == 12) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh8To12;
                if (req.MinApartmentFloorTh == 12 && req.MaxApartmentFloorTh == null) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh12;

                //options.MinAlleyTurns = req.MinAlleyTurns;
                //options.MaxAlleyTurns = req.MaxAlleyTurns;
                options.MinAlleyWidth = req.MinAlleyWidth;
                //options.MaxAlleyWidth = req.MaxAlleyWidth;
                //options.MinDistanceToStreet = req.MinDistanceToStreet;
                //options.MaxDistanceToStreet = req.MaxDistanceToStreet;

                options.MinAreaTotal = req.MinArea;
                //options.MaxAreaTotal = req.MaxArea;
                options.MinAreaTotalWidth = req.MinWidth;
                //options.MaxAreaTotalWidth = req.MaxWidth;
                options.MinAreaTotalLength = req.MinLength;
                //options.MaxAreaTotalLength = req.MaxLength;

                options.MinFloors = req.MinFloors;
                //options.MaxFloors = req.MaxFloors;
                options.MinBedrooms = req.MinBedrooms;
                //options.MaxBedrooms = req.MaxBedrooms;
                //options.MinBathrooms = req.MinBathrooms;
                //options.MaxBathrooms = req.MaxBathrooms;

                options.MinPriceProposed = req.MinPrice.HasValue ? _propertyService.ConvertToVndB((double)req.MinPrice, req.PaymentMethodPartRecord.CssClass) : 0;
                options.MaxPriceProposed = req.MaxPrice.HasValue ? _propertyService.ConvertToVndB((double)req.MaxPrice, req.PaymentMethodPartRecord.CssClass) : 0;

                IContentQuery<PropertyPart, PropertyPartRecord> temp = GetListFastFilterServices(options);

                if (temp.Count() > 0)
                    listProperties.AddRange(temp.List());
            }
            listProperties = listProperties.Distinct().ToList();
            #endregion

            return listProperties;
        }
        public int PropertiesPartCount(int id)
        {
            #region Get all Properties from customer's requirements
            int count = 0;
            var c = Services.ContentManager.Get<CustomerPart>(id);
            var groupIds = c.Requirements.Where(r => r.IsEnabled).Select(r => r.GroupId).ToList();

            foreach (var groupId in groupIds)
            {
                var reqs = c.Requirements.Where(r => r.GroupId == groupId).ToList();
                var req = reqs.First();

                InfomationAddressViewModel options = new InfomationAddressViewModel();

                options.ProvinceId = req.LocationProvincePartRecord.Id;
                options.DistrictIds = reqs.Where(r => r.LocationDistrictPartRecord != null).Select(r => r.LocationDistrictPartRecord.Id).ToArray();
                options.WardIds = reqs.Where(r => r.LocationWardPartRecord != null).Select(r => r.LocationWardPartRecord.Id).ToArray();
                options.StreetIds = reqs.Where(r => r.LocationStreetPartRecord != null).Select(r => r.LocationStreetPartRecord.Id).ToArray();
                options.DirectionIds = reqs.Where(r => r.DirectionPartRecord != null).Select(r => r.DirectionPartRecord.Id).ToArray();

                PropertyLocationPartRecord LocationFront = _propertyService.GetLocation("h-front");
                PropertyLocationPartRecord LocationAlley = _propertyService.GetLocation("h-alley");

                if (req.PropertyLocationPartRecord == null) options.OrderWalk = InfomationAddressOrderWalk.All;

                if (req.PropertyLocationPartRecord == LocationFront) options.OrderWalk = InfomationAddressOrderWalk.AllWalk;
                if (req.PropertyLocationPartRecord == LocationAlley) options.OrderWalk = InfomationAddressOrderWalk.Alley;


                if (req.MinApartmentFloorTh == null && req.MaxApartmentFloorTh == null) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.All;
                if (req.MinApartmentFloorTh == 1 && req.MaxApartmentFloorTh == 3) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh1To3;
                if (req.MinApartmentFloorTh == 4 && req.MaxApartmentFloorTh == 7) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh4To7;
                if (req.MinApartmentFloorTh == 8 && req.MaxApartmentFloorTh == 12) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh8To12;
                if (req.MinApartmentFloorTh == 12 && req.MaxApartmentFloorTh == null) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh12;

                //options.MinAlleyTurns = req.MinAlleyTurns;
                //options.MaxAlleyTurns = req.MaxAlleyTurns;
                options.MinAlleyWidth = req.MinAlleyWidth;
                //options.MaxAlleyWidth = req.MaxAlleyWidth;
                //options.MinDistanceToStreet = req.MinDistanceToStreet;
                //options.MaxDistanceToStreet = req.MaxDistanceToStreet;

                options.MinAreaTotal = req.MinArea;
                //options.MaxAreaTotal = req.MaxArea;
                options.MinAreaTotalWidth = req.MinWidth;
                //options.MaxAreaTotalWidth = req.MaxWidth;
                options.MinAreaTotalLength = req.MinLength;
                //options.MaxAreaTotalLength = req.MaxLength;

                options.MinFloors = req.MinFloors;
                //options.MaxFloors = req.MaxFloors;
                options.MinBedrooms = req.MinBedrooms;
                //options.MaxBedrooms = req.MaxBedrooms;
                //options.MinBathrooms = req.MinBathrooms;
                //options.MaxBathrooms = req.MaxBathrooms;

                options.MinPriceProposed = req.MinPrice.HasValue ? _propertyService.ConvertToVndB((double)req.MinPrice, req.PaymentMethodPartRecord.CssClass) : 0;
                options.MaxPriceProposed = req.MaxPrice.HasValue ? _propertyService.ConvertToVndB((double)req.MaxPrice, req.PaymentMethodPartRecord.CssClass) : 0;

                IContentQuery<PropertyPart, PropertyPartRecord> temp = GetListFastFilterServices(options);

                if (temp.Count() > 0)
                    count += temp.Count();
            }
            #endregion

            return count;
        }
        public List<InfomationAddressViewModel> PropertiesPartInfo(int id)
        {
            #region Get all Properties from customer's requirements

            var c = Services.ContentManager.Get<CustomerPart>(id);
            var listProperties = new List<InfomationAddressViewModel>();
            var groupIds = c.Requirements.Where(r => r.IsEnabled).Select(r => r.GroupId).ToList();

            foreach (var groupId in groupIds)
            {
                var reqs = c.Requirements.Where(r => r.GroupId == groupId).ToList();
                var req = reqs.First();

                InfomationAddressViewModel options = new InfomationAddressViewModel();

                options.ProvinceId = req.LocationProvincePartRecord.Id;
                options.DistrictIds = reqs.Where(r => r.LocationDistrictPartRecord != null).Select(r => r.LocationDistrictPartRecord.Id).ToArray();
                options.WardIds = reqs.Where(r => r.LocationWardPartRecord != null).Select(r => r.LocationWardPartRecord.Id).ToArray();
                options.StreetIds = reqs.Where(r => r.LocationStreetPartRecord != null).Select(r => r.LocationStreetPartRecord.Id).ToArray();
                options.DirectionIds = reqs.Where(r => r.DirectionPartRecord != null).Select(r => r.DirectionPartRecord.Id).ToArray();


                PropertyLocationPartRecord LocationFront = _propertyService.GetLocation("h-front");
                PropertyLocationPartRecord LocationAlley = _propertyService.GetLocation("h-alley");

                if (req.PropertyLocationPartRecord == null) options.OrderWalk = InfomationAddressOrderWalk.All;

                if (req.PropertyLocationPartRecord == LocationFront) options.OrderWalk = InfomationAddressOrderWalk.AllWalk;
                if (req.PropertyLocationPartRecord == LocationAlley) options.OrderWalk = InfomationAddressOrderWalk.Alley;


                if (req.MinApartmentFloorTh == null && req.MaxApartmentFloorTh == null) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.All;
                if (req.MinApartmentFloorTh == 1 && req.MaxApartmentFloorTh == 3) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh1To3;
                if (req.MinApartmentFloorTh == 4 && req.MaxApartmentFloorTh == 7) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh4To7;
                if (req.MinApartmentFloorTh == 8 && req.MaxApartmentFloorTh == 12) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh8To12;
                if (req.MinApartmentFloorTh == 12 && req.MaxApartmentFloorTh == null) options.OrderApartmentFloorTh = InfomationAddressOrderApartmentFloorTh.ApartmentFloorTh12;

                //options.MinAlleyTurns = req.MinAlleyTurns;
                //options.MaxAlleyTurns = req.MaxAlleyTurns;
                options.MinAlleyWidth = req.MinAlleyWidth;
                //options.MaxAlleyWidth = req.MaxAlleyWidth;
                //options.MinDistanceToStreet = req.MinDistanceToStreet;
                //options.MaxDistanceToStreet = req.MaxDistanceToStreet;

                options.MinAreaTotal = req.MinArea;
                //options.MaxAreaTotal = req.MaxArea;
                options.MinAreaTotalWidth = req.MinWidth;
                //options.MaxAreaTotalWidth = req.MaxWidth;
                options.MinAreaTotalLength = req.MinLength;
                //options.MaxAreaTotalLength = req.MaxLength;

                options.MinFloors = req.MinFloors;
                //options.MaxFloors = req.MaxFloors;
                options.MinBedrooms = req.MinBedrooms;
                //options.MaxBedrooms = req.MaxBedrooms;
                //options.MinBathrooms = req.MinBathrooms;
                //options.MaxBathrooms = req.MaxBathrooms;

                options.MinPriceProposed = req.MinPrice.HasValue ? _propertyService.ConvertToVndB((double)req.MinPrice, req.PaymentMethodPartRecord.CssClass) : 0;
                options.MaxPriceProposed = req.MaxPrice.HasValue ? _propertyService.ConvertToVndB((double)req.MaxPrice, req.PaymentMethodPartRecord.CssClass) : 0;

                //IContentQuery<PropertyPart, PropertyPartRecord> temp = _fastfilterService.GetListFastFilterService(options);

               
                //if (temp.Count() > 0)
                    listProperties.Add(options);
            }

            listProperties = listProperties.Distinct().ToList();
            #endregion

            return listProperties;

        }
        public CustomerPart ICustomerPartById(int id)
        {
            return Services.ContentManager.Get<CustomerPart>(id);
        }

        public bool CheckCustomerEmailException(string email)
        {
            string _email = email.ToLowerInvariant();
            return Services.ContentManager.Query<CustomerEmailExceptionPart, CustomerEmailExceptionPartRecord>().Where(c => c.EmailException == _email).List().Any() ? true : false;
        }

        public bool CheckCustomerEmailExceptionTrue(string email)
        {
            string _email = email.ToLowerInvariant();

            return Services.ContentManager.Query<CustomerEmailExceptionPart, CustomerEmailExceptionPartRecord>().Where(c => c.EmailException == _email && c.StatusActive == true).List().Any() ? true : false;
        }

        public void SetStatusEmailException(string email)
        {
            string _email = email.ToLowerInvariant();
            if (CheckCustomerEmailExceptionTrue(email))
            {
                var stEmail = Services.ContentManager.Query<CustomerEmailExceptionPart, CustomerEmailExceptionPartRecord>().Where(c => c.EmailException == _email).List().FirstOrDefault();
                stEmail.StatusActive = false;
            }
        }

        
    }
}