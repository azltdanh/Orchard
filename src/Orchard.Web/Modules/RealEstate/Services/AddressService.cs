using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImageResizer;
using Orchard;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Services;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using RealEstate.Models;
using RealEstate.Helpers;

namespace RealEstate.Services
{
    public interface IAddressService : IDependency
    {
        #region Provinces

        LocationProvincePartRecord GetProvince(int? provinceId);
        LocationProvincePartRecord GetProvince(string provinceName);
        IEnumerable<LocationProvincePart> GetProvinces();
        List<SelectListItem> GetSelectListProvinces();
        IEnumerable<LocationProvincePart> GetProvincesForEstimate();

        #endregion

        #region Districts

        LocationDistrictPartRecord GetDistrict(int? districtId);
        LocationDistrictPartRecord GetDistrict(string districtName);
        IEnumerable<LocationDistrictPart> GetDistricts();
        IEnumerable<LocationDistrictPart> GetDistricts(int? provinceId);
        List<SelectListItem> GetSelectListDistricts(int? provinceId);
        IEnumerable<LocationDistrictPart> GetDistrictsForEstimate();

        #endregion

        #region Wards

        LocationWardPartRecord GetWard(int? wardId);

        IContentQuery<LocationWardPart, LocationWardPartRecord> GetWards();
        IEnumerable<LocationWardPart> GetWards(int? districtId);
        List<SelectListItem> GetSelectListWards(int? districtId);
        IEnumerable<LocationWardPart> GetWards(int[] districtIds);
        List<SelectListItem> GetSelectListWards(int[] districtIds);
        IEnumerable<LocationWardPart> GetWardsByProvince(int? provinceId);
        List<SelectListItem> GetSelectListWardsByProvince(int? provinceId);

        #endregion

        #region Streets

        LocationStreetPartRecord GetStreet(int? streetId);
        LocationStreetPartRecord GetStreet(LocationStreetPartRecord relatedStreet, int number);

        IContentQuery<LocationStreetPart, LocationStreetPartRecord> GetStreets();
        IEnumerable<LocationStreetPart> GetStreets(int? districtId);
        List<SelectListItem> GetSelectListStreets(int? districtId);
        IEnumerable<LocationStreetPart> GetStreets(int[] districtIds);
        List<SelectListItem> GetSelectListStreets(int[] districtIds);
        IEnumerable<LocationStreetPart> GetStreetsByProvince(int? provinceId);
        List<SelectListItem> GetSelectListStreetsByProvince(int? provinceId);

        IContentQuery<LocationStreetPart, LocationStreetPartRecord> GetAllStreets();
        IEnumerable<LocationStreetPart> GetAllStreets(int? districtId);
        List<SelectListItem> GetSelectListAllStreets(int? districtId);
        IEnumerable<LocationStreetPart> GetAllStreetsByProvince(int? provinceId);
        List<SelectListItem> GetSelectListAllStreetsByProvince(int? provinceId);

        #endregion

        #region Apartments

        LocationApartmentPartRecord GetApartment(int? apartmentId);

        IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> GetApartments();
        IEnumerable<LocationApartmentPart> GetApartments(int? districtId);
        List<SelectListItem> GetSelectListApartments(int? districtId);
        IEnumerable<LocationApartmentPart> GetApartments(int[] districtIds);
        List<SelectListItem> GetSelectListApartments(int[] districtIds);
        IEnumerable<LocationApartmentPart> GetApartmentsByProvince(int? provinceId);
        List<SelectListItem> GetSelectListApartmentsByProvince(int? provinceId);

        IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetApartmentFiles(LocationApartmentPart apartment);
        IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetApartmentFiles(LocationApartmentPart apartment, PropertyPart p);

        void UploadApartmentImages(IEnumerable<HttpPostedFileBase> files, LocationApartmentPart apartment,
            bool isPublished);

        PropertyFilePart UploadApartmentImage(HttpPostedFileBase file, LocationApartmentPart apartment,
            UserPartRecord createdUser, bool isPublished);

        PropertyFilePart GetApartmentAvatarImage(LocationApartmentPart apartment);

        #endregion

        //void UpdateAddressForContentItem(ContentItem item, EditAddressViewModel model);
    }

    public class AddressService : IAddressService
    {
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly ISignals _signals;

        private const int CacheTimeSpan = 60 * 24; // Cache for 24 hours

        //private static readonly object SyncLock = new object();

        public AddressService(
            IClock clock,
            ISignals signals,
            ICacheManager cacheManager,
            IContentManager contentManager,
            IOrchardServices services)
        {
            _clock = clock;
            _signals = signals;
            _cacheManager = cacheManager;
            _contentManager = contentManager;

            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        #region Provinces

        public LocationProvincePartRecord GetProvince(int? provinceId)
        {
            if (!provinceId.HasValue) return null;
            return _contentManager.Get((int)provinceId).As<LocationProvincePart>().Record;
        }

        public LocationProvincePartRecord GetProvince(string provinceName)
        {
            if (string.IsNullOrEmpty(provinceName)) return null;
            if (_contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                .Where(a => a.Name == provinceName).Count() > 0)
            {
                return
                _contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                    .Where(a => a.Name == provinceName)
                    .List()
                    .First()
                    .Record;
            }
            return null;
        }

        public IEnumerable<LocationProvincePart> GetProvinces()
        {
            return _cacheManager.Get("Provinces", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Provinces_Changed"));

                return
                    _contentManager.Query<LocationProvincePart, LocationProvincePartRecord>()
                        .OrderBy(a => a.SeqOrder)
                        .List();
            });
        }

        public List<SelectListItem> GetSelectListProvinces()
        {
            return _cacheManager.Get("GetSelectListProvinces", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Provinces_Changed"));

                var selectListProvinces =
                    GetProvinces().Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();

                //selectListProvinces.Insert(0, new SelectListItem {Text = "Tất cả Tỉnh / TP", Value = "0"});

                return selectListProvinces;
            });
        }

        public IEnumerable<LocationProvincePart> GetProvincesForEstimate()
        {
            var provincesForEstimate = new List<string> { "TP. Hồ Chí Minh" }; //, "Hà Nội"
            return GetProvinces().Where(a => provincesForEstimate.Contains(a.Name));
        }

        #endregion

        #region Districts

        public LocationDistrictPartRecord GetDistrict(int? districtId)
        {
            if (!districtId.HasValue) return null;
            return _contentManager.Get((int)districtId).As<LocationDistrictPart>().Record;
        }

        public LocationDistrictPartRecord GetDistrict(string districtName)
        {
            if (string.IsNullOrEmpty(districtName)) return null;
            if (_contentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>()
                .Where(a => a.Name == districtName).Count() > 0)
            {
                return
                _contentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>()
                    .Where(a => a.Name == districtName)
                    .List()
                    .First()
                    .Record;
            }
            return null;
        }

        public IEnumerable<LocationDistrictPart> GetDistricts()
        {
            return _cacheManager.Get("Districts", ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Districts_Changed"));

                return
                    _contentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>()
                        .List()
                        .OrderBy(a => a.Province.SeqOrder)
                        .ThenBy(a => a.SeqOrder);
            });
        }

        public IEnumerable<LocationDistrictPart> GetDistricts(int? provinceId)
        {
            if (!provinceId.HasValue) provinceId = 0;
            return GetDistricts().Where(a => a.Province.Id == provinceId);
        }

        public List<SelectListItem> GetSelectListDistricts(int? provinceId)
        {
            return _cacheManager.Get("GetSelectListDistricts_" + provinceId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Districts_Changed"));

                var selectListDistricts =
                    _contentManager.Query<LocationDistrictPart, LocationDistrictPartRecord>()
                        .Where(a => a.Province.Id == provinceId)
                        .List()
                        .OrderBy(a => a.Province.SeqOrder)
                        .ThenBy(a => a.SeqOrder)
                        .Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() })
                        .ToList();

                //selectListDistricts.Insert(0, new SelectListItem {Text = "Tất cả Quận / Huyện", Value = "0"});

                return selectListDistricts;
            });
        }

        public IEnumerable<LocationDistrictPart> GetDistrictsForEstimate()
        {
            var districtsForEstimate = new List<string>
            {
                "Quận 1",
                "Quận 3",
                "Quận 5",
                "Quận 10",
                "Quận 11",
                "Quận Bình Thạnh",
                "Quận Gò Vấp",
                "Quận Phú Nhuận",
                "Quận Tân Bình",
                "Quận Tân Phú"
            };
            return GetDistricts().Where(a => districtsForEstimate.Contains(a.Name));
        }

        #endregion

        #region Wards

        public LocationWardPartRecord GetWard(int? wardId)
        {
            if (!wardId.HasValue) return null;
            return _contentManager.Get((int)wardId).As<LocationWardPart>().Record;
        }

        public IContentQuery<LocationWardPart, LocationWardPartRecord> GetWards()
        {
            //return _cacheManager.Get("Wards", ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("Wards_Changed"));

            return _contentManager.Query<LocationWardPart, LocationWardPartRecord>();

            //});
        }

        public IEnumerable<LocationWardPart> GetWards(int? districtId)
        {
            if (!districtId.HasValue) districtId = 0;
            //return _cacheManager.Get("DistrictWards" + districtId, ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("Wards_Changed"));

            return
                GetWards()
                    .Where(a => a.District.Id == districtId)
                    .List()
                    .OrderBy(a => a.Province.SeqOrder)
                    .ThenBy(a => a.District.SeqOrder)
                    .ThenBy(a => a.SeqOrder);
            //});
        }

        public List<SelectListItem> GetSelectListWards(int? districtId)
        {
            return _cacheManager.Get("GetSelectListWards_" + districtId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Wards_Changed"));

                var selectListWards =
                    GetWards(districtId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListWards.Insert(0, new SelectListItem {Text = "Tất cả Phường / Xã", Value = "0"});

                return selectListWards;
            });
        }

        public IEnumerable<LocationWardPart> GetWards(int[] districtIds)
        {
            if (districtIds != null)
                //return _cacheManager.Get("DistrictsWards" + String.Join(",", districtIds), ctx =>
                //{
                //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
                //    ctx.Monitor(_signals.When("Wards_Changed"));

                return
                    GetWards()
                        .Where(a => districtIds.Contains(a.District.Id))
                        .List()
                        .OrderBy(a => a.Province.SeqOrder)
                        .ThenBy(a => a.District.SeqOrder)
                        .ThenBy(a => a.SeqOrder);
            //});
            return new List<LocationWardPart>();
        }

        public List<SelectListItem> GetSelectListWards(int[] districtIds)
        {
            return _cacheManager.Get("GetSelectListWards_" + String.Join(",", districtIds), ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Wards_Changed"));

                var selectListWards =
                    GetWards(districtIds)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListWards.Insert(0, new SelectListItem {Text = "Tất cả Phường / Xã", Value = "0"});

                return selectListWards;
            });
        }

        public IEnumerable<LocationWardPart> GetWardsByProvince(int? provinceId)
        {
            if (!provinceId.HasValue) provinceId = 0;
            //return _cacheManager.Get("ProvinceWards" + provinceId, ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("Wards_Changed"));

            return _contentManager.Query<LocationWardPart, LocationWardPartRecord>()
                .Where(a => a.Province.Id == provinceId).List().OrderBy(a => a.District.SeqOrder).ThenBy(a => a.Name);
            //});
        }

        public List<SelectListItem> GetSelectListWardsByProvince(int? provinceId)
        {
            return _cacheManager.Get("GetSelectListWardsByProvince_" + provinceId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Wards_Changed"));

                var selectListWardsByProvince =
                    GetWardsByProvince(provinceId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListWardsByProvince.Insert(0, new SelectListItem {Text = "Tất cả Phường / Xã", Value = "0"});

                return selectListWardsByProvince;
            });
        }

        #endregion

        #region Streets

        public LocationStreetPartRecord GetStreet(int? streetId)
        {
            if (!streetId.HasValue) return null;
            return _contentManager.Get((int)streetId).As<LocationStreetPart>().Record;
        }

        public LocationStreetPartRecord GetStreet(LocationStreetPartRecord relatedStreet, int number)
        {
            if (
                _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                    .Where(a => a.RelatedStreet.Id == relatedStreet.Id && number >= a.FromNumber && number <= a.ToNumber)
                    .Count() > 0)
            {
                List<LocationStreetPart> listStreet =
                    _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                        .Where(
                            a =>
                                a.RelatedStreet.Id == relatedStreet.Id && number >= a.FromNumber && number <= a.ToNumber)
                        .List()
                        .Where(a => (a.FromNumber + number) % 2 == 0).ToList();
                if (listStreet.Any())
                    return listStreet.First().Record;
            }
            return null;
        }

        public IContentQuery<LocationStreetPart, LocationStreetPartRecord> GetStreets()
        {
            return
                _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>()
                    .Where(a => a.RelatedStreet == null);
        }

        public IEnumerable<LocationStreetPart> GetStreets(int? districtId)
        {
            if (!districtId.HasValue) districtId = 0;
            return GetStreets().Where(a => a.District.Id == districtId).List().OrderBy(a => a.Name);
        }

        public List<SelectListItem> GetSelectListStreets(int? districtId)
        {
            return _cacheManager.Get("GetSelectListStreets_" + districtId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Streets_Changed"));

                var selectListStreets =
                    GetStreets(districtId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListStreets.Insert(0, new SelectListItem {Text = "Tất cả các Đường", Value = "0"});

                return selectListStreets;
            });
        }

        public IEnumerable<LocationStreetPart> GetStreets(int[] districtIds)
        {
            if (districtIds != null)
                return
                    GetStreets()
                        .Where(a => districtIds.Contains(a.District.Id))
                        .List()
                        .OrderBy(a => a.District.SeqOrder)
                        .ThenBy(a => a.Name);
            return new List<LocationStreetPart>();
        }

        public List<SelectListItem> GetSelectListStreets(int[] districtIds)
        {
            return _cacheManager.Get("GetSelectListStreets_" + String.Join(",", districtIds), ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Streets_Changed"));

                var selectListStreets =
                    GetStreets(districtIds)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListStreets.Insert(0, new SelectListItem {Text = "Tất cả các Đường", Value = "0"});

                return selectListStreets;
            });
        }

        public IEnumerable<LocationStreetPart> GetStreetsByProvince(int? provinceId)
        {
            if (!provinceId.HasValue) provinceId = 0;
            return
                GetStreets()
                    .Where(a => a.Province.Id == provinceId)
                    .List()
                    .OrderBy(a => a.District.SeqOrder)
                    .ThenBy(a => a.Name);
        }

        public List<SelectListItem> GetSelectListStreetsByProvince(int? provinceId)
        {
            return _cacheManager.Get("GetSelectListStreetsByProvince_" + provinceId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Streets_Changed"));

                var selectListStreetsByProvince =
                    GetStreetsByProvince(provinceId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListStreetsByProvince.Insert(0, new SelectListItem {Text = "Tất cả các Đường", Value = "0"});

                return selectListStreetsByProvince;
            });
        }

        public IContentQuery<LocationStreetPart, LocationStreetPartRecord> GetAllStreets()
        {
            //return _cacheManager.Get("AllStreets", ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("Streets_Changed"));
            //    return _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>().List().Select(a => a.Record).OrderBy(a => a.Province.SeqOrder).ThenBy(a => a.District.SeqOrder).ThenBy(a => a.Name);
            //});
            return _contentManager.Query<LocationStreetPart, LocationStreetPartRecord>();
        }

        public IEnumerable<LocationStreetPart> GetAllStreets(int? districtId)
        {
            if (!districtId.HasValue) districtId = 0;
            //return _cacheManager.Get("AllDistrictStreets" + districtId, ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheTimeSpan)));
            //    ctx.Monitor(_signals.When("Streets_Changed"));

            return GetAllStreets().Where(a => a.District.Id == districtId).List().OrderBy(a => a.Name);
            //});
        }

        public List<SelectListItem> GetSelectListAllStreets(int? districtId)
        {
            return _cacheManager.Get("GetSelectListAllStreets_" + districtId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Streets_Changed"));

                var selectListAllStreets =
                    GetAllStreets(districtId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + (s.RelatedStreet != null ? " (" + s.FromNumber + " - " + s.ToNumber + ")" : "") + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListAllStreets.Insert(0, new SelectListItem {Text = "Tất cả các Đường", Value = "0"});

                return selectListAllStreets;
            });
        }

        public IEnumerable<LocationStreetPart> GetAllStreetsByProvince(int? provinceId)
        {
            if (!provinceId.HasValue) provinceId = 0;
            //return _cacheManager.Get("AllProvinceStreets_" + provinceId, ctx =>
            //{
            //    ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
            //    ctx.Monitor(_signals.When("Streets_Changed"));

            return
                GetAllStreets()
                    .Where(a => a.Province.Id == provinceId)
                    .List()
                    .OrderBy(a => a.District.SeqOrder)
                    .ThenBy(a => a.Name);
            //});
        }

        public List<SelectListItem> GetSelectListAllStreetsByProvince(int? provinceId)
        {
            return _cacheManager.Get("GetSelectListAllStreetsByProvince_" + provinceId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Streets_Changed"));

                var selectListAllStreetsByProvince =
                    GetAllStreetsByProvince(provinceId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + (s.RelatedStreet != null ? " (" + s.FromNumber + " - " + s.ToNumber + ")" : "") + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListAllStreetsByProvince.Insert(0, new SelectListItem {Text = "Tất cả các Đường", Value = "0"});

                return selectListAllStreetsByProvince;
            });
        }

        #endregion

        #region Apartments

        public LocationApartmentPartRecord GetApartment(int? apartmentId)
        {
            if (!apartmentId.HasValue) return null;
            return _contentManager.Get((int)apartmentId).As<LocationApartmentPart>().Record;
        }

        public IContentQuery<LocationApartmentPart, LocationApartmentPartRecord> GetApartments()
        {
            return _contentManager.Query<LocationApartmentPart, LocationApartmentPartRecord>();
        }

        public IEnumerable<LocationApartmentPart> GetApartments(int? districtId)
        {
            if (!districtId.HasValue) districtId = 0;
            return GetApartments()
                .Where(a => a.District.Id == districtId)
                .List()
                .OrderBy(a => a.Province.SeqOrder)
                .ThenBy(a => a.District.SeqOrder)
                .ThenBy(a => a.Name);
        }

        public List<SelectListItem> GetSelectListApartments(int? districtId)
        {
            return _cacheManager.Get("GetSelectListApartments_" + districtId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Apartments_Changed"));

                var selectListApartments =
                    GetApartments(districtId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListApartments.Insert(0, new SelectListItem {Text = "Tất cả dự án / chung cư", Value = "0"});

                return selectListApartments;
            });
        }

        public IEnumerable<LocationApartmentPart> GetApartments(int[] districtIds)
        {
            if (districtIds != null)
                return GetApartments()
                    .Where(a => districtIds.Contains(a.District.Id))
                    .List()
                    .OrderBy(a => a.Province.SeqOrder)
                    .ThenBy(a => a.District.SeqOrder)
                    .ThenBy(a => a.Name);
            return new List<LocationApartmentPart>();
        }

        public List<SelectListItem> GetSelectListApartments(int[] districtIds)
        {
            return _cacheManager.Get("GetSelectListApartments_" + String.Join(",", districtIds), ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Apartments_Changed"));

                var selectListApartments =
                    GetApartments(districtIds)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListApartments.Insert(0, new SelectListItem {Text = "Tất cả dự án / chung cư", Value = "0"});

                return selectListApartments;
            });
        }

        public IEnumerable<LocationApartmentPart> GetApartmentsByProvince(int? provinceId)
        {
            if (!provinceId.HasValue) provinceId = 0;
            return GetApartments()
                .Where(a => a.Province.Id == provinceId)
                .List()
                .OrderBy(a => a.District.SeqOrder)
                .ThenBy(a => a.Name);
        }

        public List<SelectListItem> GetSelectListApartmentsByProvince(int? provinceId)
        {
            return _cacheManager.Get("GetSelectListApartmentsByProvince_" + provinceId, ctx =>
            {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(CacheTimeSpan)));
                ctx.Monitor(_signals.When("Apartments_Changed"));

                var selectListApartmentsByProvince =
                    GetApartmentsByProvince(provinceId)
                        .Select(
                            s =>
                                new SelectListItem
                                {
                                    Text = s.Name + " - " + s.District.ShortName,
                                    Value = s.Id.ToString()
                                })
                        .ToList();

                //selectListApartmentsByProvince.Insert(0, new SelectListItem {Text = "Tất cả dự án / chung cư", Value = "0"});

                return selectListApartmentsByProvince;
            });
        }

        public IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetApartmentFiles(LocationApartmentPart apartment)
        {
            return
                _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(
                        a => !a.IsDeleted && a.LocationApartmentPartRecord != null && a.LocationApartmentPartRecord == apartment.Record);
        }
        public IContentQuery<PropertyFilePart, PropertyFilePartRecord> GetApartmentFiles(LocationApartmentPart apartment, PropertyPart p)
        {
            var propertyFiles =
                _contentManager.Query<PropertyFilePart, PropertyFilePartRecord>()
                    .Where(
                        a => !a.IsDeleted && a.LocationApartmentPartRecord != null && a.LocationApartmentPartRecord == apartment.Record);

            if (p != null && p.ApartmentBlockInfoPartRecord != null)
                return propertyFiles.Where(r => r.ApartmentBlockInfoPartRecord == null ||
                                                (r.ApartmentBlockInfoPartRecord != null && r.ApartmentBlockInfoPartRecord == p.ApartmentBlockInfoPartRecord));

            return propertyFiles;
        }

        public void UploadApartmentImages(IEnumerable<HttpPostedFileBase> files, LocationApartmentPart apartment,
            bool isPublished)
        {
            UserPartRecord createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            // Verify that the user selected a file
            var httpPostedFileBases = files as IList<HttpPostedFileBase> ?? files.ToList();
            if (files != null && httpPostedFileBases.Any())
            {
                foreach (HttpPostedFileBase file in httpPostedFileBases)
                {
                    UploadApartmentImage(file, apartment, createdUser, isPublished);
                }
            }
        }

        public PropertyFilePart UploadApartmentImage(HttpPostedFileBase fileBase, LocationApartmentPart apartment,
            UserPartRecord createdUser, bool isPublished)
        {
            var pFile = Services.ContentManager.New<PropertyFilePart>("PropertyFile");

            if (fileBase != null && fileBase.ContentLength > 0)
            {
                DateTime createdDate = DateTime.Now;

                var fileLocation = fileBase.SaveAsUserFiles(apartment.Id);

                // Image Record
                pFile.CreatedDate = createdDate;
                pFile.CreatedUser = createdUser;
                pFile.Apartment = apartment.Record;
                pFile.Name = fileBase.FileName;
                pFile.Type = "image";
                pFile.Size = fileBase.ContentLength;
                pFile.Path = fileLocation;
                pFile.Published = isPublished;

                Services.ContentManager.Create(pFile);

                Services.Notifier.Information(T("File {0} uploaded successfully.", fileBase.FileName));
            }

            return pFile;
        }

        public PropertyFilePart GetApartmentAvatarImage(LocationApartmentPart apartment)
        {
            IEnumerable<PropertyFilePart> propertyFiles = GetApartmentFiles(apartment).Where(a => a.Published).List().ToList();
            if (propertyFiles.Any())
            {
                if (propertyFiles.Any(a => a.IsAvatar))
                {
                    return propertyFiles.First(a => a.IsAvatar); // get Avatar image
                }
                return propertyFiles.First(); // get First image in the list
            }

            return null;
        }

        #endregion

        //public void UpdateAddressForContentItem(ContentItem item, EditAddressViewModel model) {
        //    var addressPart = item.As<AddressPart>();

        //    addressPart.Province = _provinceRepository.Get(s => s.Id == model.ProvinceId);
        //    addressPart.District = _districtRepository.Get(s => s.Id == model.DistrictId);
        //    addressPart.Ward = _wardRepository.Get(s => s.Id == model.WardId);
        //    addressPart.Street = _streetRepository.Get(s => s.Id == model.StreetId);

        //    addressPart.AddressNumber = model.AddressNumber;
        //    addressPart.AlleyNumber = model.AlleyNumber;

        //    addressPart.OtherProvinceName = model.OtherProvinceName;
        //    addressPart.OtherDistrictName = model.OtherDistrictName;
        //    addressPart.OtherWardName = model.OtherWardName;
        //    addressPart.OtherStreetName = model.OtherStreetName;            
        //}
    }
}