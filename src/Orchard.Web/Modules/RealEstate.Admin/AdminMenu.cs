using Orchard;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Users.Models;
using RealEstate.Services;
using Orchard.ContentManagement;
using RealEstate.Models;


namespace RealEstate
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IUserGroupService _groupService;

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        public string MenuName { get { return "admin"; } }

        public AdminMenu(
            IUserGroupService groupService,
            IOrchardServices services
            )
        {
            _groupService = groupService;
            Services = services;
        }

        public void GetNavigation(NavigationBuilder builder)
        {
            var currentDomainGroup = _groupService.GetCurrentDomainGroup() ?? _groupService.GetDefaultDomainGroup();
            var user = Services.WorkContext.CurrentUser;

            if (currentDomainGroup != null && currentDomainGroup.GroupAdminUser.Id == user.Id)
            {
                builder.AddImageSet("Groups")
               .Add(T("Groups"), "0.50",
                   menu =>
                       menu.Action("Activities", "UserGroupAdmin", new { area = "RealEstate.Admin", Id = currentDomainGroup.Id }));
            }

            builder.AddImageSet("RealEstate")
                .Add(T("BĐS nội bộ"), "0.10",
                    menu =>
                        menu.Action("Index", "PropertyAdmin", new {area = "RealEstate.Admin"})
                            .Permission(Permissions.MetaListOwnProperties)
                            .Add(T("Bất Động Sản"), "1.0",
                                subMenu =>
                                    subMenu.Action("Index", "PropertyAdmin", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.MetaListOwnProperties)
                                        .LocalNav())
                            .Add(T("BĐS Chờ duyệt"), "2.0",
                                subMenu =>
                                    subMenu.Action("Index", "PropertyAdmin",
                                        new {area = "RealEstate.Admin", ProvinceId = "0", StatusId = "29"})
                                        .Permission(Permissions.ApproveProperty)
                                        .LocalNav())
                            .Add(T("BĐS lưu theo dõi"), "3.0",
                                subMenu =>
                                    subMenu.Action("Index", "PropertyAdmin",
                                        new { area = "RealEstate.Admin", IsPropertiesWatchList = "true" })
                                        .Permission(Permissions.MetaListOwnProperties)
                                        .LocalNav())
                            .Add(T("Trao đổi BĐS"), "4.0",
                                subMenu =>
                                    subMenu.Action("Index", "PropertyAdmin",
                                        new { area = "RealEstate.Admin", IsPropertiesExchange = "true" })
                                        .Permission(Permissions.MetaListOwnProperties)
                                        .LocalNav())
                            .Add(T("Giỏ hàng dự án"), "5.0",
                                subMenu =>
                                    subMenu.Action("ApartmentWithCartIndex", "LocationApartmentAdmin",
                                        new { area = "RealEstate.Admin" })
                                        .Permission(Permissions.EditOwnProperty)
                                        .LocalNav())

                //.Add(T("BĐS đăng VIP"), "3.0", subMenu => subMenu.Action("Index", "PropertyAdmin", new { area = "RealEstate.Admin", ProvinceId = "0", StatusId = "0", AdsRequest = true }).Permission(Permissions.ApproveProperty).LocalNav())
                )
                .Add(T("BĐS Chờ duyệt"), "0.11",
                    menu =>
                        menu.Action("Index", "PropertyAdmin",
                            new {area = "RealEstate.Admin", ProvinceId = "0", StatusId = "29"})
                            .Permission(Permissions.ApproveProperty)
                            .LocalNav())
                .Add(T("BĐS Chờ duyệt"), "0.11",
                    menu =>
                        menu.Action("Index", "PropertyAdmin",
                            new { area = "RealEstate.Admin", ProvinceId = "0", StatusId = "29" })
                            .Permission(Permissions.ApproveProperty)
                            .LocalNav())
                .Add(T("BĐS chưa ĐG được"), "0.12",
                    menu =>
                        menu.Action("Index", "UnEstimatedLocationAdmin", new {area = "RealEstate.Admin"})
                            .Permission(Permissions.ManageProperties)
                            .LocalNav())
                .Add(T("BĐS group khác"), "0.13",
                    menu =>
                        menu.Action("Group", "PropertyAdmin", new {area = "RealEstate.Admin"})
                            .Permission(Permissions.MetaListOwnProperties)
                            .Add(T("BĐS group khác"), "1.0",
                                item =>
                                    item.Action("Group", "PropertyAdmin", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.MetaListOwnProperties)
                                        .LocalNav())
                            .Add(T("BĐS đã duyệt của group khác"), "10.0",
                                item =>
                                    item.Action("Group", "PropertyAdmin",
                                        new {area = "RealEstate.Admin", GroupApproved = "Approved"})
                                        .Permission(Permissions.ApproveProperty)
                                        .LocalNav())
                )
                .Add(T("Khách hàng"), "0.20",
                    menu =>
                        menu.Action("Index", "CustomerAdmin", new {area = "RealEstate.Admin"})
                            .Permission(Permissions.MetaListOwnCustomers)
                            .Add(T("Khách hàng"), "1.0",
                                subMenu =>
                                    subMenu.Action("Index", "CustomerAdmin", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.MetaListOwnCustomers)
                                        .LocalNav())
                            .Add(T("KH Chờ duyệt"), "3.0",
                                subMenu =>
                                    subMenu.Action("Index", "CustomerAdmin",
                                        new {area = "RealEstate.Admin", ProvinceId = "0", StatusId = "132823"})
                                        .Permission(Permissions.ApproveProperty)
                                        .LocalNav())
                )
                .Add(T("KH Chờ duyệt"), "0.21",
                    menu =>
                        menu.Action("Index", "CustomerAdmin", new {area = "RealEstate.Admin", StatusId = "132823"})
                            .Permission(Permissions.ApproveProperty)
                            .LocalNav())
                .Add(T("Groups & Users"), "0.30",
                    menu =>
                        menu.Action("Index", "UserGroupAdmin", new {area = "RealEstate.Admin"})
                            .Permission(Permissions.ManageUsers)
                            .Add(T("Groups"), "30.0",
                                item =>
                                    item.Action("Index", "UserGroupAdmin", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.ManageUsers)
                                        .LocalNav())
                            .Add(T("Users"), "31.0",
                                item =>
                                    item.Action("Index", "Users", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.ManageUsers)
                                        .LocalNav())
                            .Add(T("Actions"), "32.0",
                                item =>
                                    item.Action("Index", "UserActionAdmin", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.ManageUsers)
                                        .LocalNav())
                )
                .Add(T("Group của tôi"), "0.31",
                    menu =>
                        menu.Action("MyGroup", "UserGroupAdmin", new { area = "RealEstate.Admin" })
                            .Permission(Permissions.ViewJointGroupUserPoints)
                )
                .Add(T("Quảng cáo tin VIP"), "0.35",
                    menu =>
                        menu.Action("Index", "AdsPayment", new {area = "RealEstate.Admin"})
                            .Permission(Permissions.ManageAdsPaymentHistory)
                            .Add(T("Quản lý giao dịch"), "30.0",
                                item =>
                                    item.Action("Index", "AdsPayment", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.ManageAdsPaymentHistory)
                                        .LocalNav())
                            .Add(T("Config AdsPayment"), "32.0",
                                item =>
                                    item.Action("PaymentConfigIndex", "AdsPayment", new {area = "RealEstate.Admin"})
                                        .Permission(Permissions.ManageAdsPaymentConfig)
                                        .LocalNav())
                )
                .Add(T("RealEstate Settings"), "0.40",
                    menu =>
                    {
                        menu.LinkToFirstChild(false);

                        #region Address Locations

                        menu.Add(T("Address Locations"), "10.0",
                            subMenu =>
                                subMenu.Action("Index", "LocationStreetAdmin", new {area = "RealEstate.Admin"})
                                    .Permission(Permissions.MetaListAddressLocations)
                                    .Add(T("Provinces"), "10.0",
                                        item =>
                                            item.Action("Index", "LocationProvinceAdmin",
                                                new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageLocationProvinces)
                                                .LocalNav())
                                    .Add(T("Districts"), "11.0",
                                        item =>
                                            item.Action("Index", "LocationDistrictAdmin",
                                                new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageLocationDistricts)
                                                .LocalNav())
                                    .Add(T("Wards"), "12.0",
                                        item =>
                                            item.Action("Index", "LocationWardAdmin", new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageLocationWards)
                                                .LocalNav())
                                    .Add(T("Streets"), "13.0",
                                        item =>
                                            item.Action("Index", "LocationStreetAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageLocationStreets)
                                                .LocalNav())
                                    .Add(T("Streets Import"), "14.0",
                                        item =>
                                            item.Action("Index", "LocationImport", new { area = "RealEstate.Admin" })
                                                .Permission(StandardPermissions.SiteOwner)
                                                .LocalNav())
                                    .Add(T("Street Relations"), "15.0",
                                        item =>
                                            item.Action("Index", "StreetRelationAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageStreetRelations)
                                                .LocalNav())
                                    .Add(T("Directions"), "16.0",
                                        item =>
                                            item.Action("Index", "DirectionAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageDirections)
                                                .LocalNav())
                                    .Add(T("Locations"), "17.0",
                                        item =>
                                            item.Action("Index", "PropertyLocationAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyLocations)
                                                .LocalNav())
                                    .Add(T("Apartments"), "18.0",
                                        item =>
                                            item.Action("Index", "LocationApartmentAdmin",
                                                new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageLocationApartments)
                                                .LocalNav())
                                    .Add(T("Apartment Relations"), "19.0",
                                        item =>
                                            item.Action("Index", "LocationApartmentRelationAdmin",
                                                new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageLocationApartmentRelations)
                                                .LocalNav())
                                    .Add(T("Apartment Block"), "20.0",
                                        item =>
                                            item.Action("ApartmentBlockIndex", "LocationApartmentAdmin",
                                                new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageLocationApartments)
                                                .LocalNav())
                            );

                        #endregion

                        #region Property Attributes

                        menu.Add(T("Property Attributes"), "20.0",
                            subMenu =>
                                subMenu.Action("Index", "PropertyStatusAdmin", new {area = "RealEstate.Admin"})
                                    .Permission(Permissions.MetaListPropertyAttributes)
                                    .Add(T("Ads"), "10.0",
                                        item =>
                                            item.Action("Index", "AdsTypeAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageAdsTypes)
                                                .LocalNav())
                                    .Add(T("Status"), "20.0",
                                        item =>
                                            item.Action("Index", "PropertyStatusAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyStatus)
                                                .LocalNav())
                                    .Add(T("Flags"), "30.0",
                                        item =>
                                            item.Action("Index", "PropertyFlagAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyFlags)
                                                .LocalNav())
                                    .Add(T("Adv"), "40.0",
                                        item =>
                                            item.Action("Index", "PropertyAdvantageAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyAdvantages)
                                                .LocalNav())
                                    .Add(T("DisAdv"), "50.0",
                                        item =>
                                            item.Action("DisAdvantages", "PropertyAdvantageAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyAdvantages)
                                                .LocalNav())
                                    .Add(T("Groups"), "60.0",
                                        item =>
                                            item.Action("Index", "PropertyTypeGroupAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyTypeGroups)
                                                .LocalNav())
                                    .Add(T("Types"), "70.0",
                                        item =>
                                            item.Action("Index", "PropertyTypeAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyTypes)
                                                .LocalNav())
                                    .Add(T("Constructions"), "80.0",
                                        item =>
                                            item.Action("Index", "PropertyTypeConstructionAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyTypes)
                                                .LocalNav())
                                    .Add(T("Interiors"), "90.0",
                                        item =>
                                            item.Action("Index", "PropertyInteriorAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyInteriors)
                                                .LocalNav())
                                    .Add(T("Legal"), "100.0",
                                        item =>
                                            item.Action("Index", "PropertyLegalStatusAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertyLegalStatus)
                                                .LocalNav())
                            );

                        #endregion

                        #region Customer Attributes

                        menu.Add(T("Customer Attributes"), "25.0",
                            subMenu =>
                                subMenu.Action("Index", "CustomerStatusAdmin", new {area = "RealEstate.Admin"})
                                    .Permission(Permissions.MetaListCustomerAttributes)
                                    .Add(T("Status"), "25.0",
                                        item =>
                                            item.Action("Index", "CustomerStatusAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageCustomerStatus)
                                                .LocalNav())
                                    .Add(T("Purposes"), "25.0",
                                        item =>
                                            item.Action("Index", "CustomerPurposeAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageCustomerPurposes)
                                                .LocalNav())
                                    .Add(T("Feedbacks"), "25.0",
                                        item =>
                                            item.Action("Index", "CustomerFeedbackAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageCustomerFeedbacks)
                                                .LocalNav())
                            );

                        #endregion

                        #region Payment Configs

                        menu.Add(T("Payment Configs"), "30.0",
                            subMenu =>
                                subMenu.Action("Index", "PaymentMethodAdmin", new {area = "RealEstate.Admin"})
                                    .Permission(Permissions.MetaListPaymentConfigs)
                                    .Add(T("Payment Methods"), "30.0",
                                        item =>
                                            item.Action("Index", "PaymentMethodAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePaymentMethods)
                                                .LocalNav())
                                    .Add(T("Payment Units"), "30.0",
                                        item =>
                                            item.Action("Index", "PaymentUnitAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePaymentUnits)
                                                .LocalNav())
                                    .Add(T("Payment Exchanges"), "30.0",
                                        item =>
                                            item.Action("Index", "PaymentExchangeAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePaymentExchanges)
                                                .LocalNav())
                            );

                        #endregion

                        #region Configs

                        menu.Add(T("Configs"), "35.0",
                            subMenu =>
                                subMenu.Action("Index", "PropertySettingAdmin", new {area = "RealEstate.Admin"})
                                    .Permission(Permissions.MetaListConfigs)
                                    .Add(T("Settings"), "10.0",
                                        item =>
                                            item.Action("Index", "PropertySettingAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePropertySettings)
                                                .LocalNav())
                                    .Add(T("Length Coefficients"), "20.0",
                                        item =>
                                            item.Action("Index", "CoefficientLengthAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageCoefficientLengths)
                                                .LocalNav())
                                    .Add(T("Alley Coefficients"), "30.0",
                                        item =>
                                            item.Action("Index", "CoefficientAlleyAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageCoefficientAlleys)
                                                .LocalNav())
                                    .Add(T("Alley Distance Coefficients"), "40.0",
                                        item =>
                                            item.Action("Index", "CoefficientAlleyDistanceAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageCoefficientAlleyDistances)
                                                .LocalNav())
                                    .Add(T("ApartmentFloors Coefficients"), "50.0",
                                        item =>
                                            item.Action("Index", "CoefficientApartmentFloorsAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageConfigs)
                                                .LocalNav())
                                    .Add(T("ApartmentFloorTh Coefficients"), "60.0",
                                        item =>
                                            item.Action("Index", "CoefficientApartmentFloorThAdmin",
                                                new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManageConfigs)
                                                .LocalNav())
                            );

                        #endregion

                        #region Planning Maps

                        menu.Add(T("Planning Maps"), "60.0",
                            subMenu =>
                                subMenu.Action("Index", "PlanningMapAdmin", new {area = "RealEstate.Admin"})
                                    .Permission(Permissions.ManagePlanningMaps)
                                    .Add(T("Planning Maps"), "0.0",
                                        item =>
                                            item.Action("Index", "PlanningMapAdmin", new {area = "RealEstate.Admin"})
                                                .Permission(Permissions.ManagePlanningMaps)
                                                .LocalNav())
                            );

                        #endregion

                        #region Management NewsVideo
                            menu.Add(T("Quản lý Video"), "60.0",
                            subMenu =>
                                subMenu.Action("Index", "VideoManage", new { area = "RealEstate.Admin" })
                                    .Permission(Permissions.ManageYoutubeVideo)
                                    .Add(T("Quản lý video"), "0.0",
                                        item =>
                                            item.Action("Index", "VideoManage", new { area = "RealEstate.Admin" })
                                                .Permission(Permissions.ManageYoutubeVideo)
                                                .LocalNav())
                            );
                        #endregion

                        #region Management UnitInvest
                            menu.Add(T("QL đơn vị tài trợ"), "60.0",
                            subMenu =>
                                subMenu.Action("Index", "UnitInvest", new { area = "RealEstate.MiniForum" })
                                    .Permission(Permissions.ManageUnitInvest)
                                    .Add(T("QL đơn vị tài trợ"), "0.0",
                                        item =>
                                            item.Action("Index", "UnitInvest", new { area = "RealEstate.MiniForum" })
                                                .Permission(Permissions.ManageUnitInvest)
                                                .LocalNav())
                            );
                        #endregion

                    })
                .Add(T("Upgrade to 1.7"), "0", item =>
                {
                    item.LinkToFirstChild(true);
                    item.Permission(StandardPermissions.SiteOwner);
                })
                .Add(T("Content"), "1.4", item =>
                {
                    item.LinkToFirstChild(true);
                    item.Permission(StandardPermissions.SiteOwner);
                })                
                .Add(T("Jobs Queue"), "15.0", item =>
                {
                    item.Action("List", "Admin", new {area = "Orchard.JobsQueue"});
                    item.LinkToFirstChild(false);
                    item.Permission(StandardPermissions.SiteOwner);
                });

            ;
        }
    }
}