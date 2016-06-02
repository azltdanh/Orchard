using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;

namespace RealEstate.Admin.Controllers
{
    [ValidateInput(false), Admin]
    public class LocationImportController : Controller
    {
        private readonly IAddressService _addressService;
        private readonly ILocationStreetService _streetService;

        public LocationImportController(
            IOrchardServices services,
            IAddressService addressService,
            ILocationStreetService streetService,
            IShapeFactory shapeFactory)
        {
            Services = services;
            _addressService = addressService;
            _streetService = streetService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }


        public ActionResult Index()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage users")))
                return new HttpUnauthorizedResult();

            var street = Services.ContentManager.New<LocationStreetPart>("LocationStreet");

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/LocationStreet.Import",
                Model: new LocationStreetCreateViewModel
                {
                    Provinces = _addressService.GetProvinces(),
                    Districts = _addressService.GetDistricts(0),
                    //Wards = _addressService.GetWards(-1),
                    //Streets = _addressService.GetStreets(-1),
                    IsEnabled = true,
                },
                Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(street);
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object) model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost(HttpPostedFileBase locationStreetFile, LocationStreetCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage Import")))
                return new HttpUnauthorizedResult();

            if (locationStreetFile == null)
            {
                Services.Notifier.Add(NotifyType.Error, T("Vui lòng chọn file để import."));
                var street = Services.ContentManager.New<LocationStreetPart>("LocationStreet");

                dynamic editor = Shape.EditorTemplate(
                    TemplateName: "Parts/LocationStreet.Import",
                    Model: new LocationStreetCreateViewModel
                    {
                        Provinces = _addressService.GetProvinces(),
                        Districts = _addressService.GetDistricts(0),
                        IsEnabled = true,
                    },
                    Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(street);
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }

            string[] lines;

            using (var reader = new StreamReader(locationStreetFile.InputStream))
                lines = Regex.Split(reader.ReadToEnd(), "\\r?\\n");

            if (!lines.Any())
            {
                Services.Notifier.Add(NotifyType.Error, T("file không có tên đường nào."));
                return View(createModel);
            }

            lines.First().CsvSplit();

            var allResults = new List<LocationStreetResults>();

            LocationDistrictPartRecord district = _addressService.GetDistrict(createModel.DistrictId);

            lines
                //.Skip(1)//Skip the header
                .ToList()
                .ForEach(l =>
                {
                    if (string.IsNullOrEmpty(l))
                        return;
                    string[] s = l.CsvSplit();

                    var input = new LocationStreetInput
                    {
                        Name = s[0]
                    };

                    var result = new LocationStreetResults(input);

                    if (!string.IsNullOrEmpty(input.Name))
                        if (!_streetService.VerifyStreetUnicity(input.Name, createModel.DistrictId))
                            result.AddError(T("Tên đường đã tồn tại. {0}", input.Name));

                    var street = Services.ContentManager.New<LocationStreetPart>("LocationStreet");
                    if (result.Valid)
                    {
                        street.SeqOrder = createModel.SeqOrder;
                        street.IsEnabled = createModel.IsEnabled;
                        street.Province = district.Province;
                        street.District = district;
                        street.Ward = null;
                            // _addressService.GetWards().SingleOrDefault(a => a.Id == createModel.WardId);

                        street.Name = input.Name;
                        street.RelatedStreet = null;
                        street.FromNumber = null;
                        street.ToNumber = null;

                        street.StreetWidth = null;
                        street.CoefficientAlley1Max = createModel.CoefficientAlley1Max;
                        street.CoefficientAlley1Min = createModel.CoefficientAlley1Min;
                        street.CoefficientAlleyEqual = createModel.CoefficientAlleyEqual;
                        street.CoefficientAlleyMax = createModel.CoefficientAlleyMax;
                        street.CoefficientAlleyMin = createModel.CoefficientAlleyMin;

                        Services.ContentManager.Create(street);

                        result.AddMessage(T("Tên đường {0} đã được tạo.", input.Name));
                    }

                    allResults.Add(result);
                });
            return View("ImportComplete", allResults);
        }
    }
}