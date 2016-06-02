using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Alias;
using Orchard.Alias.Implementation.Holder;
using Orchard.Alias.Records;
using Orchard.Alias.ViewModels;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using RealEstate.FrontEnd.Models;
using RealEstate.FrontEnd.Services;
using RealEstate.FrontEnd.ViewModels;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Services;

namespace RealEstate.FrontEnd.Controllers
{
    [ValidateInput(false), Admin]
    public class AliasesMetaController : Controller, IUpdateModel
    {
        private readonly IAddressService _addressService;
        private readonly IAliasHolder _aliasHolder;
        private readonly IRepository<AliasRecord> _aliasRepository;
        private readonly IAliasService _aliasService;
        private readonly IRepository<AliasesMetaPartRecord> _aliasesMetaRepository;
        private readonly IContentManager _contentManager;
        private readonly IFastFilterService _fastfilterservice;
        private readonly ISiteService _siteService;
        private readonly IUserGroupService _groupService;

        public AliasesMetaController(
            IOrchardServices services,
            IShapeFactory shapeFactory,
            IFastFilterService fastfilterservice,
            IContentManager contentManager,
            IRepository<AliasRecord> aliasRepository,
            IAliasService aliasService,
            IAddressService addressService,
            IRepository<AliasesMetaPartRecord> aliasesMetaRepository,
            IAliasHolder aliasHolder,
            IUserGroupService groupService,
            ISiteService siteService)
        {
            Services = services;
            _siteService = siteService;
            _fastfilterservice = fastfilterservice;
            _contentManager = contentManager;
            _aliasRepository = aliasRepository;
            _addressService = addressService;
            _aliasService = aliasService;
            _aliasHolder = aliasHolder;
            _aliasesMetaRepository = aliasesMetaRepository;
            _groupService = groupService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
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

        public ActionResult Index(AliasesMetaIndexOptions options, PagerParameters pagerParameters)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to list AliasesMetas")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new AliasesMetaIndexOptions();

            IContentQuery<AliasesMetaPart, AliasesMetaPartRecord> aliasesMetas = Services.ContentManager
                .Query<AliasesMetaPart, AliasesMetaPartRecord>();
            switch (options.Filter)
            {
                case AliasesMetaFilter.All:
                    //propertySettings = propertySettings.Where(u => u.RegistrationStatus == UserStatus.Approved);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                aliasesMetas = aliasesMetas.Where(u => u.Title.Contains(options.Search));
            }

            dynamic pagerShape = Shape.Pager(pager).TotalItemCount(aliasesMetas.Count());

            List<AliasesMetaPart> results = aliasesMetas
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();


            var model = new AliasesMetaIndexViewModel
            {
                AliasesMetas = results
                    .Select(x => new AliasesMetaEntry {AliasesMeta = x.Record})
                    .ToList(),
                Options = options,
                Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Filter", options.Filter);
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage AliasesMetas")))
                return new HttpUnauthorizedResult();

            var viewModel = new AliasesMetaIndexViewModel
            {
                AliasesMetas = new List<AliasesMetaEntry>(),
                Options = new AliasesMetaIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<AliasesMetaEntry> checkedEntries = viewModel.AliasesMetas.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction)
            {
                case AliasesMetaBulkAction.None:
                    break;
                case AliasesMetaBulkAction.Enable:
                    //foreach (AliasesMetaEntry entry in checkedEntries)
                    //{
                    //    //Enable(entry.PropertySetting.Id);
                    //}
                    break;
                case AliasesMetaBulkAction.Disable:
                    //foreach (AliasesMetaEntry entry in checkedEntries)
                    //{
                    //    //Disable(entry.PropertySetting.Id);
                    //}
                    break;
                case AliasesMetaBulkAction.Delete:
                    foreach (AliasesMetaEntry entry in checkedEntries)
                    {
                        Delete(entry.AliasesMeta.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        //public ActionResult Create()
        //{
        //    if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
        //        return new HttpUnauthorizedResult();

        //    var aliasesMeta = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");
        //    dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AliasesMeta.Create",
        //        Model: new AliasesMetaCreateViewModel(), Prefix: null);
        //    editor.Metadata.Position = "2";
        //    dynamic model = Services.ContentManager.BuildEditor(aliasesMeta);
        //    model.Content.Add(editor);

        //    return View((object) model);
        //}

        //[HttpPost, ActionName("Create")]
        //public ActionResult CreatePost(AliasesMetaCreateViewModel createModel)
        //{
        //    if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
        //        return new HttpUnauthorizedResult();

        //    var aliasesMeta = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");

        //    if (ModelState.IsValid)
        //    {
        //        aliasesMeta.Title = createModel.Title;
        //        aliasesMeta.Keywords = createModel.Keywords;
        //        aliasesMeta.Description = createModel.Description;
        //        aliasesMeta.SeqOrder = createModel.SeqOrder;
        //        Services.ContentManager.Create(aliasesMeta);

        //        //Services.Notifier.Information(T("Alias <a href=\"{0}\">{1}</a> đã tạo thành công.",Url.Action("Edit", new { path = }));
        //    }

        //    dynamic model = Services.ContentManager.UpdateEditor(aliasesMeta, this);

        //    if (!ModelState.IsValid)
        //    {
        //        Services.TransactionManager.Cancel();

        //        dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AliasesMeta.Create", Model: createModel,
        //            Prefix: null);
        //        editor.Metadata.Position = "2";
        //        model.Content.Add(editor);

        //        return View((object) model);
        //    }

        //    return RedirectToAction("Index");
        //}

        public ActionResult Edit(string path)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();
            if (path == "/")
            {
                path = String.Empty;
            }

            //Get Current DomainGroupId
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            var aliasRecord = _aliasRepository.Table.Single(a => a.Path == path);
            var aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                .Where(r => r.Alias_Id == aliasRecord.Id && r.DomainGroupId == currentDomainGroup.Id).Slice(1).FirstOrDefault();

            if (aliasesMeta != null)
            {
                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AliasesMeta.Edit",
                    Model: new AliasesMetaEditViewModel
                    {
                        AliasesMeta = aliasesMeta,
                        Title = aliasesMeta.Title,
                        Keywords = aliasesMeta.Keywords,
                        Description = aliasesMeta.Description,
                        SeqOrder = aliasesMeta.SeqOrder,
                        DomainGroupCurrent = currentDomainGroup.Name
                    }, Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(aliasesMeta);
                model.Content.Add(editor);
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object) model);
            }
            else
            {
                var aliasesMetaCreate = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");
                dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AliasesMeta.Create",
                    Model: new AliasesMetaCreateViewModel{DomainGroupCurrent = currentDomainGroup.Name}, Prefix: null);
                editor.Metadata.Position = "2";
                dynamic model = Services.ContentManager.BuildEditor(aliasesMetaCreate);
                model.Content.Add(editor);
                return View((object) model);
            }
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPost(string path, AliasesMetaCreateViewModel createModel)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            string tempPath = path;

            if (path == "/")
            {
                path = String.Empty;
            }

            //Get Current DomainGroupId
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            var aliasRecord = _aliasRepository.Table.Single(a => a.Path == path);
            var aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                .Where(r => r.Alias_Id == aliasRecord.Id && r.DomainGroupId == currentDomainGroup.Id).Slice(1).FirstOrDefault();

            if (aliasesMeta != null)
            {
                dynamic model = Services.ContentManager.UpdateEditor(aliasesMeta, this);

                var editModel = new AliasesMetaEditViewModel {AliasesMeta = aliasesMeta};
                if (TryUpdateModel(editModel))
                {
                    aliasesMeta.Title = editModel.Title;
                    aliasesMeta.Keywords = editModel.Keywords;
                    aliasesMeta.Description = editModel.Description;
                    aliasesMeta.SeqOrder = editModel.SeqOrder;

                    _fastfilterservice.ClearCacheMeta(tempPath); //Clear cache

                    Services.Notifier.Information(T("Alias Meta {0} updated.", path));
                }

                if (!ModelState.IsValid)
                {
                    Services.TransactionManager.Cancel();

                    dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AliasesMeta.Edit", Model: editModel,
                        Prefix: null);
                    editor.Metadata.Position = "2";
                    model.Content.Add(editor);

                    return View((object) model);
                }
            }
            else
            {
                var aliasesMetaCreate = Services.ContentManager.New<AliasesMetaPart>("AliasesMeta");

                if (ModelState.IsValid)
                {
                    aliasesMetaCreate.Title = createModel.Title;
                    aliasesMetaCreate.Keywords = createModel.Keywords;
                    aliasesMetaCreate.Description = createModel.Description;
                    aliasesMetaCreate.SeqOrder = createModel.SeqOrder;
                    aliasesMetaCreate.Alias_Id = aliasRecord.Id;
                    aliasesMetaCreate.DomainGroupId = currentDomainGroup.Id;
                    Services.ContentManager.Create(aliasesMetaCreate);

                    _fastfilterservice.ClearCacheMeta(tempPath); //Clear cache

                    Services.Notifier.Information(T("Alias Meta {0} created.", path));
                }

                dynamic model = Services.ContentManager.UpdateEditor(aliasesMetaCreate, this);

                if (!ModelState.IsValid)
                {
                    Services.TransactionManager.Cancel();

                    dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/AliasesMeta.Create", Model: createModel,
                        Prefix: null);
                    editor.Metadata.Position = "2";
                    model.Content.Add(editor);

                    return View((object) model);
                }
            }

            return RedirectToAction("Index", "Admin", new RouteValueDictionary { { "area", "Orchard.Alias" } });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (
                !Services.Authorizer.Authorize(Permissions.ManagePropertySettings,
                    T("Not authorized to manage AliasesMetas")))
                return new HttpUnauthorizedResult();

            var aliasesMeta = Services.ContentManager.Get<AliasesMetaPart>(id);

            if (aliasesMeta != null)
            {
                Services.ContentManager.Remove(aliasesMeta.ContentItem);
                Services.Notifier.Information(T("AliasesMeta {0} deleted", aliasesMeta.Title));
            }

            return RedirectToAction("Index");
        }

        #region Alias Auto create

        public ActionResult AliasCollectionCreate()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            AliasesMetaCreatedOptions model = _fastfilterservice.InitAliasCreate(new AliasesMetaCreatedOptions());
            return View(model);
        }

        [HttpPost, ActionName("AliasCollectionCreate")]
        public ActionResult AliasCollectionCreatePost(AliasesMetaCreatedOptions options, FormCollection frmCollection)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            #region Init

            string route = "AdsTypeCssClass="; //RealEstate.FrontEnd/PropertySearch/ResultFilter?
            string alias = "nha-dat";
            //string _pStatus = "";
            string rStatus = "";
            string adsTypeClass = "";

            int adsType = 0; //AdsTypeCssClass flag_1
            int typeGroup = 1; //TypeGroupCssClass flag_2
            string anytype = ""; // Trạng thái bđs

            #endregion

            #region process params

            foreach (string key in frmCollection.AllKeys)
            {
                if (key.Contains("RequestVerificationToken")) continue;

                //AdsTypeCssClass - TypeGroupCssClass - ProvinceId - DistrictIds
                foreach (string value in frmCollection[key].Split(','))
                {
                    #region AdsTypeCssClass

                    if (key == "AdsTypeCssClass")
                    {
                        route += value;
                        switch (value)
                        {
                            case "ad-selling": //Nhà đất bán
                                alias += "-ban";
                                adsTypeClass = "ban";
                                adsType = 1;
                                break;
                            case "ad-leasing": //Nhà đất cho thuê
                                alias += "-cho-thue";
                                adsTypeClass = "cho-thue";
                                adsType = 2;
                                break;
                            case "ad-buying": //Nhà đất cần mua
                                alias += "-can-mua";
                                adsTypeClass = "can-mua";
                                adsType = 3;
                                break;
                            case "ad-renting": //Nhà đất cần thuê
                                alias += "-can-thue";
                                adsTypeClass = "can-thue";
                                adsType = 4;
                                break;
                        }
                    }

                    #endregion

                    #region TypeGroupCssClass

                    if (key == "TypeGroupCssClass")
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            route += "&TypeGroupCssClass=" + value;
                        }
                        switch (value)
                        {
                            case "gp-house": //Đất ở hoặc Nhà
                                alias += "-nha-pho";
                                typeGroup = 1;
                                break;
                            case "gp-apartment": //Căn hộ, Chung cư
                                alias += "-can-ho";
                                typeGroup = 2;
                                break;
                            case "gp-land": //Các loại đất khác
                                alias += "-dat";
                                typeGroup = 3;
                                break;
                            case "gp-house-land": //Nhà phố & đất
                                alias = adsTypeClass + "-nha-dat";
                                typeGroup = 4;
                                break;
                            default:
                                typeGroup = 5; //4
                                break;
                        }
                    }

                    #endregion
                }
            }

            #region trạng thái bđs

            //if (options.AllAnyType)// Tìm tất cả
            //{
            //    _rStatus += "&AllAnyType=1";
            //    //_pStatus = "";
            //    _anytype = "";
            //}

            if (options.AdsGoodDeal) // BĐS giá rẻ
            {
                rStatus += "&AdsGoodDeal=true";
                alias += "-gia-re";
                anytype = "Giá rẻ";
            }
            if (options.AdsVIP) // BĐS Giao dịch gấp
            {
                rStatus += "&AdsVIP=true";
                alias += "-giao-dich-gap";
                anytype = "Giao dịch gấp";
            }
            if (options.IsOwner) // BĐS chính chủ
            {
                rStatus += "&IsOwner=true";
                alias += "-chinh-chu";
                anytype = "Chính chủ";
            }
            if (options.IsAuction) // BĐS phát mãi
            {
                rStatus += "&IsAuction=true";
                alias += "-phat-mai";
                anytype = "Phát mãi";
            }

            #endregion

            #endregion

            string rootroute = "RealEstate.FrontEnd/PropertySearch/ResultFilter?";
            if (adsType == 3 || adsType == 4) // Cần mua hoặc cần thuê
            {
                rootroute = "RealEstate.FrontEnd/PropertySearch/ResultFilterRequirement?";
            }

            if (!options.IsCheckProvince)
            {
                Services.Notifier.Information(T("Chọn tất cả tỉnh"));

                List<LocationProvincePart> provinces = _addressService.GetProvinces().ToList();
                    // Danh sách tỉnh thành.

                #region tạo alias theo tỉnh

                for (int i = 0; i < provinces.Count(); i++)
                {
                    string aliaspath = alias + "-" + provinces[i].Name.ToSlug(); // param
                    string routepath = rootroute + route + "&ProvinceId=" + provinces[i].Id + rStatus; //param

                    CreateAliasRecord(options, aliaspath, adsType, typeGroup, routepath, "", provinces[i].Name,
                        anytype);

                    Services.Notifier.Information(T("Alias: {0} - <br> Route: {1}", aliaspath, routepath));
                }

                #endregion
            }
            else
            {
                #region Tạo alias theo Quận/Huyện

                // ProvinceIds
                if (options.ProvinceIds != null)
                {
                    if (options.ProvinceIds.Count() > 0)
                    {
                        for (int i = 0; i < options.ProvinceIds.Count(); i++)
                        {
                            if (options.ProvinceIds[i] != 0)
                            {
                                string provinceName = _addressService.GetProvince(options.ProvinceIds[i]).Name;

                                #region Tạo alias theo tỉnh

                                string aliaspathProvince = alias + "-" + provinceName.ToSlug(); // param
                                string routepathProvince = rootroute + route + "&ProvinceId=" + options.ProvinceIds[i] +
                                                           rStatus; //param

                                CreateAliasRecord(options, aliaspathProvince, adsType, typeGroup, routepathProvince, "",
                                    provinceName, anytype);

                                Services.Notifier.Information(T("Alias: {0} - <br> Route: {1}", aliaspathProvince,
                                    routepathProvince));

                                #endregion

                                Services.Notifier.Information(T("Tạo alias theo quận/huyện - {0} ", provinceName));

                                List<LocationDistrictPart> districts =
                                    _addressService.GetDistricts(options.ProvinceIds[i]).ToList();
                                    // Danh sách Quận/Huyện

                                foreach (LocationDistrictPart dist in districts)
                                {
                                    string aliaspath = alias + "-" + dist.Name.ToSlug() + "-" + provinceName.ToSlug();
                                        // param
                                    string routepath = rootroute + route + rStatus + "&DistrictIds=" + dist.Id +
                                                       "&ProvinceId=" + options.ProvinceIds[i]; //param

                                    CreateAliasRecord(options, aliaspath, adsType, typeGroup, routepath, dist.Name,
                                        provinceName, anytype);

                                    Services.Notifier.Information(T("Alias: {0} - <br> Route: {1}", aliaspath, routepath));
                                }
                            }
                        }
                    }
                    else
                    {
                        Services.Notifier.Information(T("Chưa chọn tỉnh thành nào! "));
                    }
                }

                #endregion
            }
            AliasesMetaCreatedOptions model = _fastfilterservice.InitAliasCreate(new AliasesMetaCreatedOptions());

            return View("AliasCollectionCreate", model);
        }

        public void CreateAliasRecord(AliasesMetaCreatedOptions options, string aliaspath, int adsType, int typeGroup,
            string routepath, string districtName, string provinceName, string anytype)
        {
            #region Chưa có alias

            RouteValueDictionary aliasrecord = _aliasService.Get(aliaspath);

            if (aliasrecord == null) // nếu alias đó chưa có
            {
                //1. Tạo alias
                _aliasService.Set(aliaspath, routepath, "custom");

                //2. Tạo meta keyword description
                if (aliaspath == "/")
                {
                    aliaspath = String.Empty;
                }
                AliasRecord aliasRecord = _aliasRepository.Table.Single(a => a.Path == aliaspath);

                if (!options.IsCheckUpdateMeta) // Nếu tự điền Meta: Title, Keywords, Description
                {
                    _fastfilterservice.UpdateAliasMeta(aliasRecord, options, districtName, provinceName, anytype);
                }
                else
                {
                    _fastfilterservice.UpdateAliasMeta(aliasRecord,
                        _fastfilterservice.Metas(districtName + provinceName, anytype, adsType, typeGroup));
                }

                Services.Notifier.Information(T("{0} chưa có alias. <br/>{1} - {2} :  đã được tạo", provinceName,
                    aliaspath, routepath));
            }
                #endregion

                #region Đã có alias

            else // đã có alias rồi
            {
                AliasRecord aliasRecord = _aliasRepository.Table.Single(a => a.Path == aliaspath);
                if (!options.IsCheckUpdateMeta) // Nếu tự điền Meta: Title, Keywords, Description
                {
                    _fastfilterservice.UpdateAliasMeta(aliasRecord, options, districtName, provinceName, anytype);
                }
                else
                {
                    _fastfilterservice.UpdateAliasMeta(aliasRecord,
                        _fastfilterservice.Metas(districtName + provinceName, anytype, adsType, typeGroup));
                }
                Services.Notifier.Information(T("{0} Có alias rồi", provinceName));
            }

            #endregion
        }

        //Apartment
        public ActionResult AliasCollectionApartmentCreate()
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            return View(new AliasesMetaCreatedOptions());
        }

        [HttpPost, ActionName("AliasCollectionApartmentCreate")]
        public ActionResult AliasCollectionApartmentCreatePost(AliasesMetaCreatedOptions options)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            const string alias = "du-an";
            const string rootroute = "RealEstate.FrontEnd/PropertySearch/ResultFilterApartment?";

            List<LocationProvincePart> provinces = _addressService.GetProvinces().ToList(); // Danh sách tỉnh thành.

            foreach (LocationProvincePart province in provinces)
            {
                IEnumerable<LocationDistrictPart> districts = _addressService.GetDistricts(province.Id);
                string aliasPath = alias + "-" + province.Name.ToSlug(); // param
                string routePath = rootroute + "ApartmentProvinceId=" + province.Id; //param

                ApartmentAliasCreate(aliasPath, routePath, options, "", province.Name);

                foreach (LocationDistrictPart district in districts)
                {
                    string aliaspath = alias + "-" + district.Name.ToSlug() + "-" + province.Name.ToSlug(); // param
                    string routepath = rootroute + "ApartmentProvinceId=" + province.Id + "&ApartmentDistrictIds=" +
                                       district.Id; //param

                    ApartmentAliasCreate(aliaspath, routepath, options, district.Name, province.Name);
                }
            }

            return View("AliasCollectionApartmentCreate", new AliasesMetaCreatedOptions());
        }

        public void ApartmentAliasCreate(string aliaspath, string routepath, AliasesMetaCreatedOptions options,
            string district, string province)
        {
            //Tạo alias
            RouteValueDictionary aliasrecord = _aliasService.Get(aliaspath);
            if (aliasrecord == null)
            {
                //1. Tạo alias
                _aliasService.Set(aliaspath, routepath, "custom");

                //2. Tạo meta keyword description
                if (aliaspath == "/")
                {
                    aliaspath = String.Empty;
                }
                AliasRecord aliasRecord = _aliasRepository.Table.Single(a => a.Path == aliaspath);

                _fastfilterservice.UpdateAliasMeta(aliasRecord, options, district, province);

                Services.Notifier.Information(T("{0} chưa có alias. <br/>{1} - {2} :  đã được tạo", province, aliaspath,
                    routepath));
            }
            else
            {
                AliasRecord aliasRecord = _aliasRepository.Table.Single(a => a.Path == aliaspath);

                _fastfilterservice.UpdateAliasMeta(aliasRecord, options, district, province);

                Services.Notifier.Information(T("{0} Có alias rồi", province));
            }
        }

        #region  ReplacePathByRoute

        /*
        public JsonResult ReplacePathByRoute()
        {
            var AliasRecordList = _aliasRepository.Fetch(r => r.Path.Contains("nha-dat-rao-ban"));

            List<string> AliasArray = new List<string>();

            foreach (var _alias in AliasRecordList)
            {
                _alias.Path = _alias.Path.Replace("nha-dat-rao-ban", "nha-dat");
                AliasArray.Add(_alias.Path);
            }
            return Json(new { status = AliasArray }, JsonRequestBehavior.AllowGet);
        }
        */

        [HttpPost]
        public ActionResult ReplaceRouteByCharacter(FormCollection frm)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            //var newDict = new Dictionary<string, string>();
            //newDict.Add("_IsAdsGoodDeal=1", "AdsGoodDeal=true");
            //newDict.Add("_IsAdsVIP=1", "AdsVIP=true");
            //newDict.Add("_IsOwner=1", "IsOwner=true");
            //newDict.Add("_IsAuction=1", "IsAuction=true");

            var aliasArray = new List<string>();
            IEnumerable<AliasInfo> aliases = _aliasHolder.GetMaps().SelectMany(x => x.GetAliases());

            foreach (AliasInfo alias in aliases)
            {
                VirtualPathData virtualPathData =
                    _aliasService.LookupVirtualPaths(alias.RouteValues.ToRouteValueDictionary(), HttpContext)
                        .FirstOrDefault();

                if (virtualPathData != null)
                {
                    string routePath = virtualPathData.VirtualPath;
                    if (routePath.Contains(frm["WordReplace"]))
                    {
                        string wordByReplace = "";
                        if (!string.IsNullOrEmpty(frm["WordByReplace"])) wordByReplace = frm["WordByReplace"];
                        string routePathReplace = routePath.Replace(frm["WordReplace"], wordByReplace);
                        Services.Notifier.Information(T("{0}", routePathReplace));
                        aliasArray.Add(routePathReplace);
                        _aliasService.Set(alias.Path, routePathReplace, "Custom");
                    }
                }
            }

            AliasesMetaCreatedOptions model = _fastfilterservice.InitAliasCreate(new AliasesMetaCreatedOptions());
            Services.Notifier.Information(T("{0} - {1} - {2}", aliasArray.Count(), frm["WordReplace"],
                frm["WordByReplace"]));

            return View("AliasCollectionCreate", model);
        }

        [HttpPost]
        public ActionResult RemoveAliasByAliasPath(string pathRemove)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            var aliasArray = new List<string>();
            string aliasString = "";
            IEnumerable<AliasInfo> aliases = _aliasHolder.GetMaps().SelectMany(x => x.GetAliases());

            if (!String.IsNullOrWhiteSpace(pathRemove))
            {
                string invariantSearch = pathRemove.ToLowerInvariant();
                aliases = aliases.Where(u => u.Path.StartsWith(invariantSearch));
            }
            aliases = aliases.ToList();

            foreach (AliasInfo item in aliases)
            {
                //Xóa Meta
                AliasRecord aliasRecord = _aliasRepository.Table.Single(a => a.Path == item.Path);
                AliasesMetaPartRecord aliasesMetaRecord =
                    _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                        .Where(a => a.Alias_Id == aliasRecord.Id).Slice(1).Select(r => r.Record).FirstOrDefault();

                _aliasService.Delete(item.Path); //Xóa Alias
                if (aliasesMetaRecord != null)
                {
                    _aliasesMetaRepository.Delete(aliasesMetaRecord); // XÓa Meta
                }


                aliasString += "http://dinhgianhadat.vn/" + item.Path + "\n";
                aliasArray.Add(item.Path);
                Services.Notifier.Information(T("{0}", item.Path));
            }
            string folder = Server.MapPath("~/Media/alias_path_remove.txt");

            System.IO.File.WriteAllText(folder, aliasString);

            AliasesMetaCreatedOptions model = _fastfilterservice.InitAliasCreate(new AliasesMetaCreatedOptions());
            Services.Notifier.Information(T("{0}", aliasArray.Count()));

            return View("AliasCollectionCreate", model);
        }

        #endregion

        #endregion

        #region Search Orchard.Alias

        public ActionResult AliasSearch(AdminIndexOptions options, PagerParameters pagerParameters, string search)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            // default options
            if (options == null)
                options = new AdminIndexOptions();

            var pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);
            if (string.IsNullOrEmpty(options.Search))
            {
                options.Search = search;
            }

            switch (options.Filter)
            {
                case AliasFilter.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            IEnumerable<AliasInfo> aliases = _aliasHolder.GetMaps().SelectMany(x => x.GetAliases()).ToList();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                string invariantSearch = options.Search.Trim().ToLowerInvariant();
                aliases = aliases.Where(u => u.Path.Contains(invariantSearch)).ToList();
            }

            dynamic pagerShape = Services.New.Pager(pager).TotalItemCount(aliases.Count());

            switch (options.Order)
            {
                case AliasOrder.Path:
                    aliases = aliases.OrderBy(x => x.Path);
                    break;
            }

            if (pager.PageSize != 0)
            {
                aliases = aliases.Skip(pager.GetStartIndex()).Take(pager.PageSize);
            }

            var model = new AdminIndexViewModel
            {
                Options = options,
                Pager = pagerShape,
                AliasEntries = aliases.Select(x => new AliasEntry {Alias = x, IsChecked = false}).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult AliasSearch(FormCollection input)
        {
            //if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
            //    return new HttpUnauthorizedResult();

            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized Administrator")))
                return new HttpUnauthorizedResult();

            var viewModel = new AdminIndexViewModel
            {
                AliasEntries = new List<AliasEntry>(),
                Options = new AdminIndexOptions()
            };
            UpdateModel(viewModel);

            IEnumerable<AliasEntry> checkedItems = viewModel.AliasEntries.Where(c => c.IsChecked);

            switch (viewModel.Options.BulkAction)
            {
                case AliasBulkAction.None:
                    break;
                case AliasBulkAction.Delete:
                    foreach (AliasEntry checkedItem in checkedItems)
                    {
                        _aliasService.Delete(checkedItem.Alias.Path);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return RedirectToAction("AliasSearch");
        }

        //OverWrite Delete Alias module Orchard.Alias
        [HttpPost]
        public ActionResult DeleteAlias(string path, string returnUrl)
        {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage aliases")))
                return new HttpUnauthorizedResult();

            string tempPath = path;
            if (path == "/")
            {
                path = String.Empty;
            }


            #region Delete in table AliasesMetaPartRecord

            var aliasRecord = _aliasRepository.Table.Single(a => a.Path == path);
            var aliasesMeta = _contentManager.Query<AliasesMetaPart, AliasesMetaPartRecord>()
                .Where(r => r.Alias_Id == aliasRecord.Id).List();

            foreach (var meta in aliasesMeta)
            {
                //Services.ContentManager.Remove(meta.ContentItem);
                _aliasesMetaRepository.Delete(meta.Record);
            }
            _aliasService.Delete(path);

            _fastfilterservice.ClearCacheMeta(tempPath); //Clear cache

            #endregion

            Services.Notifier.Information(T("Alias {0} deleted", path));

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Admin", new RouteValueDictionary { { "area", "Orchard.Alias" } });
        }

        #endregion
    }
}