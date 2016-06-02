using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Google.GData.Client;
using Google.GData.Extensions.MediaRss;
using Google.GData.YouTube;
using Google.YouTube;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.Entities.Places.Request;
using GoogleMapsApi.Entities.Places.Response;

using Orchard;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;
using RealEstate.Helpers;
using RealEstate.Models;
using Location = GoogleMapsApi.Entities.Common.Location;
using Result = GoogleMapsApi.Entities.Places.Response.Result;

namespace RealEstate.Services
{
    public interface IGoogleApiService : IDependency
    {
        Location GeoCode(string address);
        string GetAddressByLocation(double lng, double lat);
        double DistanceTwoPoint(double x1, double x2, double y1, double y2); // tính khoảng cách giữa 2 điểm

        /// <summary>
        ///     Tìm kiếm các địa điểm xung quanh theo điều kiện
        /// </summary>
        /// <param name="location">Vị trí cần tìm(tọa độ điểm)</param>
        /// <param name="keyword">Từ khóa cần tìm (VD: chợ hoặc trường học hoặc ....</param>
        /// <param name="radius">Bán kính xung quanh điểm cần tìm ( Đơn vị: m )</param>
        /// <param name="apiKey">API của ứng dụng đăng ký với google</param>
        /// <returns></returns>
        IEnumerable<Result> ResultSearch(Location location, string keyword, double radius, string apiKey);

        /// <summary>
        ///     Tìm kiếm các địa điểm xung quanh theo điều kiện
        /// </summary>
        /// <param name="lng">Longitute của địa điểm trên bản đồ</param>
        /// <param name="lat">Latitute của địa điểm trên bản đồ</param>
        /// <param name="keyword">Từ khóa cần tìm (VD: chợ hoặc trường học hoặc ....</param>
        /// <param name="radius">Bán kính xung quanh điểm cần tìm ( Đơn vị: m )</param>
        /// <param name="apiKey">API của ứng dụng đăng ký với google</param>
        /// <returns></returns>
        IEnumerable<Result> ResultSearch(double lng, double lat, string keyword, double radius, string apiKey);

        /// <summary>
        ///     Tìm kiếm các địa điểm xung quanh theo điều kiện
        /// </summary>
        /// <param name="address">Địa chỉ của điểm cần tìm (VD: 286/3 Tô Hiến Thành, P15, Quận 10, TP HCM )</param>
        /// <param name="keyword">Từ khóa cần tìm (VD: chợ hoặc trường học hoặc ....</param>
        /// <param name="radius">Bán kính xung quanh điểm cần tìm ( Đơn vị: m )</param>
        /// <param name="apiKey">API của ứng dụng đăng ký với google</param>
        /// <returns></returns>
        IEnumerable<Result> ResultSearch(string address, string keyword, double radius, string apiKey);

        /// <summary>
        ///     Upload to Youtube.com
        /// </summary>
        /// <param name="fileBase"></param>
        /// <param name="title"></param>
        /// <param name="keyword"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Video UploadYoutube(HttpPostedFileBase fileBase, string title, string keyword, string description);

        string VideoIdUpload(HttpPostedFileBase fileBase, string title, string keyword, string description);
        string VideoIdUpload(HttpPostedFileBase fileBase, PropertyPart p);
    }

    public class GoogleApiService : IGoogleApiService
    {
        public GoogleApiService(IOrchardServices services)
        {
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        #region Search Maps Api

        public Location GeoCode(string address)
        {
            var geoRequest = new GeocodingRequest {Address = address};


            GeocodingResponse geoResponse = GoogleMaps.Geocode.Query(geoRequest);

            GoogleMapsApi.Entities.Geocoding.Response.Result geoResult = geoResponse.Results.First();

            return geoResult.Geometry.Location;
        }

        public string GetAddressByLocation(double lng, double lat)
        {
            var location = new Location(lat, lng);

            var geoRequest = new GeocodingRequest {Location = location};

            GeocodingResponse geoResponse = GoogleMaps.Geocode.Query(geoRequest);

            GoogleMapsApi.Entities.Geocoding.Response.Result geoResult = geoResponse.Results.First();

            return geoResult.FormattedAddress;
        }

        public IEnumerable<Result> ResultSearch(Location location, string keyword, double radius, string apiKey)
            //Location location
        {
            var placerequest = new PlacesRequest
            {
                ApiKey = apiKey,
                Keyword = keyword,
                Radius = radius,
                Location = location,
                Sensor = false
            };
            PlacesResponse response = GoogleMaps.Places.Query(placerequest);

            return response.Results;
        }

        public IEnumerable<Result> ResultSearch(double lng, double lat, string keyword, double radius, string apiKey)
            //Location location
        {
            //search
            var locate = new Location(lat, lng);

            var placerequest = new PlacesRequest
            {
                ApiKey = apiKey,
                Keyword = keyword,
                Radius = radius,
                Location = locate,
                Sensor = false
            };
            PlacesResponse response = GoogleMaps.Places.Query(placerequest);

            return response.Results;
        }

        public IEnumerable<Result> ResultSearch(string address, string keyword, double radius, string apiKey)
        {
            var placerequest = new PlacesRequest
            {
                ApiKey = apiKey,
                Keyword = keyword,
                Radius = radius,
                Location = GeoCode(address),
                Sensor = false
            };

            PlacesResponse response = GoogleMaps.Places.Query(placerequest);

            return response.Results;
        }

        public double DistanceTwoPoint(double x1, double x2, double y1, double y2) // tính khoảng cách điểm trên Map
        {
            return Math.Round(Math.Sqrt(Math.Abs(((x1 - x2) * (x1 - x2)) + ((y1 - y2) * (y1 - y2)))) * 60 * 1852, 0);
        }

        #endregion

        #region Youtube upload

        public Video UploadYoutube(HttpPostedFileBase fileBase, string title, string keyword, string description)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                // Register for a Youtube Develper key: http://code.google.com/apis/youtube/dashboard/ or https://console.developers.google.com
                var settings = new YouTubeRequestSettings("dinhgianhadat-youtube",
                    "AIzaSyBvgttRoxiIIwylL6CFE3FC7Rl_dsXNRXU",
                    "uploadvideo.dph@gmail.com", "01696494327");

               // Services.Notifier.Information(T("Start Time: {0}", (DateTime.Now - startTime).TotalSeconds));

                DateTime startExtension = DateTime.Now;
                string extension;
                switch (Path.GetExtension(fileBase.FileName))
                {
                    case "wmv":
                        extension = "video/x-ms-wmv";
                        break;
                    case "mp4":
                        extension = "video/mp4";
                        break;
                    case "mov":
                        extension = "video/quicktime";
                        break;
                    case "flv":
                        extension = "video/x-flv";
                        break;
                    case "mpg":
                    case "mpeg":
                        extension = "video/mpeg";
                        break;
                    case "3gp":
                        extension = "video/3gpp";
                        break;
                    case "mkv":
                        extension = "video/x-matroska";
                        break;
                    default:
                        extension = "video/x-msvideo";
                        break;
                }
               // Services.Notifier.Information(T("startExtension: {0}", (DateTime.Now - startExtension).TotalSeconds));

                DateTime startRequest = DateTime.Now;

                var request = new YouTubeRequest(settings);
                var newVideo = new Video { Title = title };
                newVideo.Tags.Add(new MediaCategory("News", YouTubeNameTable.CategorySchema));
                newVideo.Keywords = keyword;
                newVideo.Description = description;
                //newVideo.Tags.Add(new MediaCategory("tag 1, tag 2", YouTubeNameTable.DeveloperTagSchema));
                //newVideo.YouTubeEntry.Location = new GeoRssWhere(10.77836, 106.664468);
                newVideo.YouTubeEntry.Private = false;

               // Services.Notifier.Information(T("startRequest: {0}", (DateTime.Now - startRequest).TotalSeconds));

                DateTime startRequestFactory = DateTime.Now;

                ((GDataRequestFactory)request.Service.RequestFactory).Timeout = 9999999;
                var service = new YouTubeService("dinhgianhadat-youtube", "AIzaSyBvgttRoxiIIwylL6CFE3FC7Rl_dsXNRXU");
                ((GDataRequestFactory)service.RequestFactory).KeepAlive = false;

               // Services.Notifier.Information(T("startRequestFactory: {0}", (DateTime.Now - startRequestFactory).TotalSeconds));

                DateTime startMediaSource = DateTime.Now;

                newVideo.YouTubeEntry.MediaSource = new MediaFileSource(fileBase.InputStream, fileBase.FileName,
                    extension);

               // Services.Notifier.Information(T("startMediaSource: {0}", (DateTime.Now - startMediaSource).TotalSeconds));

                return request.Upload(newVideo);
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(T("YoutubeUpload Error: {0}", ex.Message));
                return null;
            }
        }

        public string VideoIdUpload(HttpPostedFileBase fileBase, string title, string keyword, string description)
        {
            DateTime startUploadYoutube = DateTime.Now;

            Video video = UploadYoutube(fileBase, title, keyword, description);

            Services.Notifier.Information(T("startUploadYoutube: {0}", (DateTime.Now - startUploadYoutube).TotalSeconds));

            if(video != null)
                return video.VideoId;
            return "";
        }

        public string VideoIdUpload(HttpPostedFileBase fileBase, PropertyPart p)
        {
            string description = !string.IsNullOrEmpty(p.Content)
                ? p.Content.StripHtml()
                : p.DisplayForAreaConstructionLocationInfo.StripHtml();

            Video video = UploadYoutube(fileBase, "dinhgianhadat.vn - " + p.DisplayForTitleSEO, p.DisplayForTitleSEO,description);

            return video.VideoId;
        }

        #endregion
    }
}