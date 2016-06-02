using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Themes;
using Orchard.UI.Notify;
using RealEstate.Models;
using RealEstate.Services;
using RealEstate.ViewModels;
using Orchard.Users.Models;
using System;
using System.Globalization;
using System.Collections.Generic;
using RealEstate.FrontEnd.Services;
using RealEstate.FrontEnd.ViewModels;

namespace RealEstate.FrontEnd.Controllers
{
    [Themed, Authorize]
    public class PropertyExchangeController : Controller, IUpdateModel
    {
        #region Init

        private readonly IPropertyExchangeService _propertyExchangeService;
        private readonly IPropertyService _propertyService;
        private readonly IUserGroupService _groupService;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IFastFilterService _fastfilterService;

        public PropertyExchangeController(
            IShapeFactory shapeFactory, 
            IOrchardServices services, 
            IPropertyExchangeService propertyExchangeService, 
            IPropertyService propertyService,
            IUserGroupService groupService,
            IAddressService addressService,
            ICustomerService customerService,
            IFastFilterService fastfilterService)
        {
            _propertyExchangeService = propertyExchangeService;
            _propertyService = propertyService;
            _groupService = groupService;
            _addressService = addressService;
            _customerService = customerService;
            _fastfilterService = fastfilterService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
            Services = services;
        }

        private dynamic Shape { get; set; }
        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #endregion

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


        public ActionResult RequirementExchangeCreate(int pId)
        {
            #region Validate

            //Kiểm tra property nếu đã có trong table PropertyExchange
            var propertyExchange = _propertyService.GetExchangePartRecordByPropertyId(pId);

            // Nếu chưa có => trở về trang tạo CreateProperty
            if (propertyExchange == null) return RedirectToAction("CreateProperty");

            // Nếu đã có thì kiểm tra xem có khách hàng hay chưa?
            if (propertyExchange != null && propertyExchange.Customer != null) return RedirectToAction("RequirementExchangeEdit", new { id = propertyExchange.Id, groupId = propertyExchange.Customer.Requirements.First().GroupId });

            #endregion

            var c = Services.ContentManager.New<CustomerPart>("Customer");
            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/RequirementExchange.Create",
                Model: _propertyExchangeService.BuildCustomerRequirementExchangeCreate(pId), Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(c);
            model.Content.Add(editor);

            return View((object)model);
        }

        [HttpPost]
        public ActionResult RequirementExchangeCreate(int pId,CustomerRequirementExchangeCreateViewModel viewModel)
        {
            #region Validate

            //Kiểm tra property nếu đã có trong table PropertyExchange
            var propertyExchange = _propertyService.GetExchangePartRecordByPropertyId(pId);

            // Nếu chưa có => trở về trang tạo CreateProperty
            if (propertyExchange == null) return RedirectToAction("CreateProperty");

            // Nếu đã có thì kiểm tra xem có khách hàng hay chưa?
            if (propertyExchange != null && propertyExchange.Customer != null) return RedirectToAction("RequirementExchangeEdit", new { id = propertyExchange.Id, groupId = propertyExchange.Customer.Requirements.First().GroupId });

            #endregion

            var p = Services.ContentManager.Get<PropertyPart>(pId);

            #region VALIDATION

            if (string.IsNullOrEmpty(viewModel.TypeGroupCssClass))
            {
                AddModelError("TypeGroupCssClass", T("Bạn chưa chọn loại BĐS."));
            }

            if (!viewModel.ProvinceId.HasValue)
            {
                AddModelError("ProvinceId", T("Bạn chưa chọn Tỉnh / Thành Phố."));
            }

            if (string.IsNullOrEmpty(viewModel.Phone))
            {
                AddModelError("Phone", T("Vui lòng nhập Số điện thoại."));
            }

            if (!string.IsNullOrEmpty(viewModel.Email))
            {
                if (!Regex.IsMatch(viewModel.Email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
                {
                    // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                    ModelState.AddModelError("email", T("Bạn vui lòng cung cấp một địa chỉ e-mail hợp lệ."));
                }
            }

            if (string.IsNullOrEmpty(viewModel.Name))
            {
                AddModelError("Name", T("Vui lòng nhập tên."));
            }

            #endregion

            #region Create New Customer

            DateTime createdDate = DateTime.Now;
            UserPartRecord createdUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;
            var belongGroup = _groupService.GetBelongGroup(createdUser.Id);

            var c = Services.ContentManager.New<CustomerPart>("Customer");

            // Contact
            c.ContactName = viewModel.Name;
            c.ContactPhone = viewModel.Phone;
            c.ContactEmail = viewModel.Email;

            // Status
            c.Status = _customerService.GetStatus("st-pending");//-----
            c.StatusChangedDate = createdDate;

            // User
            c.CreatedDate = createdDate;
            c.CreatedUser = createdUser;
            c.LastUpdatedDate = createdDate;
            c.LastUpdatedUser = createdUser;
            c.Note = viewModel.Note;
            c.Published = true;

            // UserGroup
            c.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();

            // AdsExpirationDate
            //if (viewModel.AdsExpirationDateValue.HasValue)
            //{
            //    switch (viewModel.AdsExpirationDateValue)
            //    {
            //        case 1:
            //            c.AdsExpirationDate = DateTime.Now.AddDays(10);
            //            p.AdsExpirationDate = DateTime.Now.AddDays(10);
            //            break;
            //        case 2:
            //            c.AdsExpirationDate = DateTime.Now.AddDays(20);
            //            p.AdsExpirationDate = DateTime.Now.AddDays(20);
            //            break;
            //        case 3:
            //            c.AdsExpirationDate = DateTime.Now.AddDays(30);
            //            p.AdsExpirationDate = DateTime.Now.AddDays(30);
            //            break;
            //        case 4:
            //            c.AdsExpirationDate = DateTime.Now.AddDays(60);
            //            p.AdsExpirationDate = DateTime.Now.AddDays(60);
            //            break;
            //        case 5:
            //            c.AdsExpirationDate = DateTime.Now.AddDays(90);
            //            p.AdsExpirationDate = DateTime.Now.AddDays(90);
            //            break;
            //    }
            //}

            Services.ContentManager.Create(c);

            // IdStr
            c.IdStr = c.Id.ToString(CultureInfo.InvariantCulture);

            // Purposes
            _customerService.UpdatePurposesForContentItem(c, viewModel.Purposes);

            #endregion

            var createModel = new CustomerEditRequirementViewModel();

            if (ModelState.IsValid)
            {
                #region Update Customer Requirements

                createModel.AdsTypeId = _propertyService.GetAdsType(viewModel.AdsTypeCssClass).Id;
                createModel.PropertyTypeGroupId = _propertyService.GetTypeGroup(viewModel.TypeGroupCssClass).Id;

                createModel.ProvinceId = viewModel.ProvinceId;
                createModel.DistrictIds = viewModel.DistrictIds;
                createModel.WardIds = viewModel.WardIds;
                createModel.StreetIds = viewModel.StreetIds;

                createModel.ApartmentIds = viewModel.ApartmentIds;
                createModel.OtherProjectName = viewModel.OtherProjectName;

                createModel.DirectionIds = viewModel.DirectionIds;

                createModel.LocationId = viewModel.LocationId;
                createModel.MinAlleyWidth = viewModel.MinAlleyWidth;

                createModel.MinArea = viewModel.MinArea;
                createModel.MinWidth = viewModel.MinWidth;
                createModel.MinLength = viewModel.MinLength;

                createModel.MinFloors = viewModel.MinFloors;
                createModel.MinBedrooms = viewModel.MinBedrooms;

                #region ApartmentFloorThRange

                switch (viewModel.ApartmentFloorThRange)
                {
                    case PropertyDisplayApartmentFloorTh.All:
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                        createModel.MinApartmentFloorTh = 1;
                        createModel.MaxApartmentFloorTh = 3;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                        createModel.MinApartmentFloorTh = 4;
                        createModel.MaxApartmentFloorTh = 7;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                        createModel.MinApartmentFloorTh = 8;
                        createModel.MaxApartmentFloorTh = 12;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                        createModel.MinApartmentFloorTh = 12;
                        break;
                }

                #endregion

                #region AlleyTurnsRange

                int locationFront = _propertyService.GetLocation("h-front").Id;
                int locationAlley = _propertyService.GetLocation("h-alley").Id;

                switch (viewModel.AlleyTurnsRange)
                {
                    case PropertyDisplayLocation.All: // Tat ca cac vi tri
                        break;
                    case PropertyDisplayLocation.AllWalk: // Mat Tien
                        createModel.LocationId = locationFront;
                        break;
                    case PropertyDisplayLocation.Alley6: // hem 6m tro len
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 6;
                        break;
                    case PropertyDisplayLocation.Alley5:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 5;
                        break;
                    case PropertyDisplayLocation.Alley4:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 4;
                        break;
                    case PropertyDisplayLocation.Alley3:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 3;
                        break;
                    case PropertyDisplayLocation.Alley2:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 2;
                        break;
                    case PropertyDisplayLocation.Alley:
                        createModel.LocationId = locationAlley;
                        createModel.MinAlleyWidth = 1;
                        break;
                }

                #endregion

                //Add Requirement
                int id = _fastfilterService.UpdateRequirements(c.Record, createModel);
                
                #endregion

                var pExchange = _propertyService.GetExchangePartRecordByPropertyId(pId);

                #region Update PropertyExchange

                _propertyExchangeService.UpdatePropertyExchange(pExchange.Id, c, viewModel.ExchangeTypeClass, viewModel.Values, viewModel.PaymentMethodId);

                #endregion

                Services.Notifier.Information(T("Thông tin BĐS muốn trao đổi <a href='{0}'>{1}</a> đã lưu thành công. Chúng tôi sẽ duyệt tin và đưa lên trang web trong thời gian 24h",
                    Url.Action("RequirementExchangeEdit", new { groupId = id, pExchange.Id }), p.DisplayForAddress));

                return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel", returnstatus = "exchange" }); ;
            }
            #region ERROR HANDLE

            dynamic model = Services.ContentManager.UpdateEditor(c, this);
            Services.TransactionManager.Cancel();

            dynamic editor = Shape.EditorTemplate(
                TemplateName: "Parts/RequirementExchange.Create",
                Model: _propertyExchangeService.BuildCustomerRequirementExchangeCreate(pId), Prefix: null);
            editor.Metadata.Position = "2";
            model.Content.Add(editor);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);

            #endregion
        }

        public ActionResult RequirementExchangeEdit(int id, int groupId)//id: PropertyExchangeId, groupId: Requirement Group Id
        {
            var pExchange = _propertyService.GetExchangePartRecord(id);

            if (pExchange == null)
                return RedirectToAction("CreateProperty");

            var viewmodel = _propertyExchangeService.BuildCustomerRequirementExchange(groupId, pExchange.Property.Id);
            viewmodel.ExchangeTypeClass = pExchange.ExchangeType != null ? pExchange.ExchangeType.CssClass : "";
            viewmodel.Values = pExchange.ExchangeValues;

            dynamic editor = Shape.EditorTemplate(TemplateName: "Parts/RequirementExchange.Edit", Model: viewmodel, Prefix: null);
            editor.Metadata.Position = "2";
            dynamic model = Services.ContentManager.BuildEditor(viewmodel.Customer);
            model.Content.Add(editor);
            return View((object)model);
        }

        [HttpPost]
        public ActionResult RequirementExchangeEdit(int id, int groupId,FormCollection frm)
            //id: PropertyExchangeId, groupId: Requirement Group Id
        {
            IEnumerable<CustomerRequirementRecord> requirements = _customerService.GetRequirements(groupId);
            CustomerRequirementRecord req = requirements.First();
            var customer = Services.ContentManager.Get<CustomerPart>(req.CustomerPartRecord.Id);

            var pExchange = _propertyService.GetExchangePartRecord(id);
            var p = pExchange.Property;

            dynamic model = Services.ContentManager.UpdateEditor(customer, this);

            DateTime lastUpdatedDate = DateTime.Now;
            UserPartRecord lastUpdatedUser = Services.WorkContext.CurrentUser.As<UserPart>().Record;

            var editModel = new CustomerRequirementExchangeEditViewModel { Customer = customer };

            if (TryUpdateModel(editModel))
            {
                #region VALIDATION

                if (!editModel.ProvinceId.HasValue)
                {
                    AddModelError("ProvinceId", T("Bạn chưa chọn Tỉnh / Thành Phố."));
                }

                if (string.IsNullOrEmpty(editModel.TypeGroupCssClass))
                {
                    AddModelError("TypeGroupCssClass", T("Bạn chưa chọn loại BĐS."));
                }
                if (string.IsNullOrEmpty(editModel.Phone))
                {
                    AddModelError("Phone", T("Vui lòng nhập Số điện thoại."));
                }

                if (!string.IsNullOrEmpty(editModel.Email))
                {
                    if (!Regex.IsMatch(editModel.Email, UserPart.EmailPattern, RegexOptions.IgnoreCase))
                    {
                        // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                        ModelState.AddModelError("Email", T("Bạn vui lòng cung cấp một địa chỉ e-mail hợp lệ."));
                    }
                }

                if (string.IsNullOrEmpty(editModel.Name))
                {
                    AddModelError("Name", T("Vui lòng nhập tên."));
                }

                #endregion

                #region UPDATE MODEL

                // UserGroup
                if (customer.UserGroup == null)
                {
                    customer.UserGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
                }

                // User
                customer.LastUpdatedDate = lastUpdatedDate;
                customer.LastUpdatedUser = lastUpdatedUser;

                customer.Status = _customerService.GetStatus("st-pending");
                customer.Note = editModel.Note;

                customer.ContactName = editModel.Name;
                customer.ContactPhone = editModel.Phone;
                customer.ContactEmail = editModel.Email;

                // AdsExpirationDate
                //if (editModel.AdsExpirationDateValue.HasValue)
                //{
                //    switch (editModel.AdsExpirationDateValue)
                //    {
                //        case 1:
                //            customer.AdsExpirationDate = DateTime.Now.AddDays(10);
                //            p.AdsExpirationDate = DateTime.Now.AddDays(10);
                //            break;
                //        case 2:
                //            customer.AdsExpirationDate = DateTime.Now.AddDays(20);
                //            p.AdsExpirationDate = DateTime.Now.AddDays(20);
                //            break;
                //        case 3:
                //            customer.AdsExpirationDate = DateTime.Now.AddDays(30);
                //            p.AdsExpirationDate = DateTime.Now.AddDays(30);
                //            break;
                //        case 4:
                //            customer.AdsExpirationDate = DateTime.Now.AddDays(60);
                //            p.AdsExpirationDate = DateTime.Now.AddDays(60);
                //            break;
                //        case 5:
                //            customer.AdsExpirationDate = DateTime.Now.AddDays(90);
                //            p.AdsExpirationDate = DateTime.Now.AddDays(90);
                //            break;
                //    }
                //}


                // Purposes
                _customerService.UpdatePurposesForContentItem(customer, editModel.Purposes);

                var replaceModel = new CustomerEditRequirementViewModel
                {
                    GroupId = groupId,
                    AdsTypeId = _propertyService.GetAdsType(editModel.AdsTypeCssClass).Id,
                    PropertyTypeGroupId = _propertyService.GetTypeGroup(editModel.TypeGroupCssClass).Id,
                    ProvinceId = editModel.ProvinceId ?? _addressService.GetProvince("TP. Hồ Chí Minh").Id,
                    DistrictIds = editModel.DistrictIds,
                    WardIds = editModel.WardIds,
                    StreetIds = editModel.StreetIds,
                    DirectionIds = editModel.DirectionIds,
                    ApartmentIds = editModel.ApartmentIds,
                    OtherProjectName = editModel.OtherProjectName,
                    LocationId = editModel.LocationId,
                    MinFloors = editModel.MinFloors,
                    MinBedrooms = editModel.MinBedrooms,
                    MinArea = editModel.MinArea,
                    MinWidth = editModel.MinWidth,
                    MinLength = editModel.MinLength,
                    MinAlleyWidth = editModel.MinAlleyWidth
                };

                #region Update Customer Requirements

                replaceModel.MinFloors = editModel.MinFloors;
                replaceModel.MinPrice = editModel.MinPrice;
                replaceModel.MaxPrice = editModel.MaxPrice;

                #region ApartmentFloorThRange

                switch (editModel.ApartmentFloorThRange)
                {
                    case PropertyDisplayApartmentFloorTh.All:
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh1To3:
                        replaceModel.MinApartmentFloorTh = 1;
                        replaceModel.MaxApartmentFloorTh = 3;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh4To7:
                        replaceModel.MinApartmentFloorTh = 4;
                        replaceModel.MaxApartmentFloorTh = 7;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh8To12:
                        replaceModel.MinApartmentFloorTh = 8;
                        replaceModel.MaxApartmentFloorTh = 12;
                        break;
                    case PropertyDisplayApartmentFloorTh.ApartmentFloorTh12:
                        replaceModel.MinApartmentFloorTh = 12;
                        break;
                }

                #endregion

                #region AlleyTurnsRange

                int locationFront = _propertyService.GetLocation("h-front").Id;
                int locationAlley = _propertyService.GetLocation("h-alley").Id;

                switch (editModel.AlleyTurnsRange)
                {
                    case PropertyDisplayLocation.All: // Tat ca cac vi tri
                        break;
                    case PropertyDisplayLocation.AllWalk: // Mat Tien
                        replaceModel.LocationId = locationFront;
                        break;
                    case PropertyDisplayLocation.Alley6: // hem 6m tro len
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 6;
                        break;
                    case PropertyDisplayLocation.Alley5:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 5;
                        break;
                    case PropertyDisplayLocation.Alley4:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 4;
                        break;
                    case PropertyDisplayLocation.Alley3:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 3;
                        break;
                    case PropertyDisplayLocation.Alley2:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 2;
                        break;
                    case PropertyDisplayLocation.Alley:
                        replaceModel.LocationId = locationAlley;
                        replaceModel.MinAlleyWidth = 1;
                        break;
                }

                #endregion

                //Add Requirement
                _fastfilterService.UpdateRequirements(customer.Record, replaceModel);

                Services.Notifier.Information(T("Thông tin BĐS muốn trao đổi <a href='{0}'>{1}</a> đã lưu thành công. Chúng tôi sẽ duyệt tin và đưa lên trang web trong thời gian 24h",
                    Url.Action("RequirementExchangeEdit", new { groupId = groupId, id = id }), p.Id));

                #endregion

                #endregion

                #region Update PropertyExchangePart

                _propertyExchangeService.UpdatePropertyExchange(id, editModel.ExchangeTypeClass,editModel.Values,editModel.PaymentMethodId);

                #endregion

            }

            #region ERROR HANDLE

            if (!ModelState.IsValid)
            {
                Services.TransactionManager.Cancel();

                editModel = _propertyExchangeService.BuildCustomerRequirementExchange(groupId, pExchange.Property.Id);
                editModel.ExchangeTypeClass = pExchange.ExchangeType != null ? pExchange.ExchangeType.CssClass : "";
                editModel.Values = pExchange.ExchangeValues;

                dynamic editor = Shape.EditorTemplate(
                    TemplateName: "Parts/RequirementExchange.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            #endregion

            if (!String.IsNullOrEmpty(editModel.ReturnUrl))
            {
                return this.RedirectLocal(editModel.ReturnUrl);
            }
            return RedirectToAction("Index", "User", new { area = "RealEstate.UserControlPanel", returnstatus = "exchange" }); ;
        }


        public ActionResult AjaxLoadRequirementDetail(int id)
        {
            var p = Services.ContentManager.Get<PropertyPart>(id);

            CustomerDetailViewModel viewModel = _fastfilterService.BuildPropertyExchangeDetailViewModel(p);
            return PartialView(viewModel);
        }

    }
}
