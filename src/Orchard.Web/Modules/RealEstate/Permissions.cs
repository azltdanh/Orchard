using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace RealEstate
{
    public class Permissions : IPermissionProvider {

        public static readonly Permission NoRestrictedIP = new Permission { Description = "---No Restricted IP", Name = "NoRestrictedIP" };
        public static readonly Permission NoRestrictedIPCustomer = new Permission { Description = "---No Restricted IP Customer", Name = "NoRestrictedIPCustomer" };

        #region Properties

        public static readonly Permission ManageProperties = new Permission { Description = "Manage Properties", Name = "ManageProperties" };

        public static readonly Permission PublishProperty = new Permission { Description = "Publish or unpublish any Property", Name = "PublishProperty", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission PublishOwnProperty = new Permission { Description = "Publish or unpublish own Property", Name = "PublishOwnProperty", ImpliedBy = new[] { PublishProperty } };

        public static readonly Permission ApproveProperty = new Permission { Description = "Approve or unpublish any Property", Name = "ApproveProperty", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission ApproveOwnProperty = new Permission { Description = "Approve or unpublish own Property", Name = "ApproveOwnProperty", ImpliedBy = new[] { ApproveProperty } };

        public static readonly Permission CopyProperty = new Permission { Description = "Copy any Property", Name = "CopyProperty", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission CopyOwnProperty = new Permission { Description = "Copy own Property", Name = "CopyOwnProperty", ImpliedBy = new[] { CopyProperty } };

        public static readonly Permission EditProperty = new Permission { Description = "Edit any Properties", Name = "EditProperty", ImpliedBy = new[] { PublishProperty } };
        public static readonly Permission EditOwnProperty = new Permission { Description = "Edit own Properties", Name = "EditOwnProperty", ImpliedBy = new[] { EditProperty, PublishOwnProperty } };

        public static readonly Permission DeleteProperty = new Permission { Description = "Delete any Property", Name = "DeleteProperty", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission DeleteOwnProperty = new Permission { Description = "Delete own Property", Name = "DeleteOwnProperty", ImpliedBy = new[] { DeleteProperty } };

        public static readonly Permission MetaListDealGoodProperties = new Permission { Description = "List all deal good Properties", Name = "MetaListDealGoodProperties", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission MetaListDealVeryGoodProperties = new Permission { Description = "List all deal very good Properties", Name = "MetaListDealVeryGoodProperties", ImpliedBy = new[] { ManageProperties } };

        public static readonly Permission MetaListProperties = new Permission { Description = "List all Properties", Name = "MetaListProperties", ImpliedBy = new[] { EditProperty, PublishProperty, DeleteProperty } };
        public static readonly Permission MetaListOwnProperties = new Permission { Description = "List own Properties", Name = "MetaListOwnProperties", ImpliedBy = new[] { MetaListProperties, EditOwnProperty, PublishOwnProperty, DeleteOwnProperty } };

        public static readonly Permission RequireAddressNumber = new Permission { Description = "---Require Address Number", Name = "RequireAddressNumber" };
        public static readonly Permission EditAddressNumber = new Permission { Description = "---Edit Address Number", Name = "EditAddressNumber", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission EditContactPhoneToDisplay = new Permission { Description = "---Edit Contact Phone To Display", Name = "EditContactPhoneToDisplay", ImpliedBy = new[] { ManageProperties } };

        public static readonly Permission SetAdsGoodDeal = new Permission { Description = "---Set AdsGoodDeal", Name = "SetAdsGoodDeal", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission SetAdsVIP = new Permission { Description = "---Set AdsVIP", Name = "SetAdsVIP", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission SetAdsHighlight = new Permission { Description = "---Set AdsHighlight", Name = "SetAdsHighlight", ImpliedBy = new[] { ManageProperties } };

        public static readonly Permission ExportProperties = new Permission { Description = "---Export Properties", Name = "ExportProperties", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission BulkActionProperties = new Permission { Description = "---Bulk Action Properties", Name = "BulkActionProperties", ImpliedBy = new[] { ManageProperties } };

        public static readonly Permission ShareProperty = new Permission { Description = "Share to Group any Property", Name = "ShareProperty", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission ShareOwnProperty = new Permission { Description = "Share to Group own Property", Name = "ShareOwnProperty", ImpliedBy = new[] { ShareProperty } };
        #endregion
        
        // Quyền xem DebugLog định giá
        public static readonly Permission ViewDebugLogEstimateProperties = new Permission { Description = "---View DebugLog Estimate Properties", Name = "ViewDebugLogEstimateProperties", ImpliedBy = new[] { ManageProperties } };

        #region Quyền xem nhà dùng tham chiếu định giá

        public static readonly Permission ViewReferencedProperties = new Permission { Description = "---View Referenced Properties", Name = "ViewReferencedProperties", ImpliedBy = new[] { ManageProperties } };

        public static readonly Permission AccessNegotiateProperties = new Permission { Description = "---Access Negotiate Properties", Name = "AccessNegotiateProperties", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission AccessTradingProperties = new Permission { Description = "---Access Trading Properties", Name = "AccessTradingProperties", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission AccessSoldProperties = new Permission { Description = "---Access Sold Properties", Name = "AccessSoldProperties", ImpliedBy = new[] { ManageProperties } };
        public static readonly Permission AccessOnHoldProperties = new Permission { Description = "---Access OnHold Properties", Name = "AccessOnHoldProperties", ImpliedBy = new[] { ManageProperties } };

        public static readonly Permission EditPropertyImages = new Permission { Description = "Edit Property Images", Name = "EditPropertyImages", ImpliedBy = new[] { ManageProperties } };

        #endregion

        #region Customers

        public static readonly Permission ManageCustomers = new Permission { Description = "Manage Customers", Name = "ManageCustomers" };

        public static readonly Permission PublishCustomer = new Permission { Description = "Publish or unpublish any Customer", Name = "PublishCustomer", ImpliedBy = new[] { ManageCustomers } };
        public static readonly Permission PublishOwnCustomer = new Permission { Description = "Publish or unpublish own Customer", Name = "PublishOwnCustomer", ImpliedBy = new[] { PublishCustomer } };
        public static readonly Permission EditCustomer = new Permission { Description = "Edit any Customers", Name = "EditCustomer", ImpliedBy = new[] { PublishCustomer } };
        public static readonly Permission EditOwnCustomer = new Permission { Description = "Edit own Customers", Name = "EditOwnCustomer", ImpliedBy = new[] { EditCustomer, PublishOwnCustomer } };
        public static readonly Permission DeleteCustomer = new Permission { Description = "Delete any Customer", Name = "DeleteCustomer", ImpliedBy = new[] { ManageCustomers } };
        public static readonly Permission DeleteCustomerProperty = new Permission { Description = "Delete any Customer Property", Name = "DeleteCustomerProperty", ImpliedBy = new[] { DeleteCustomer } };
        public static readonly Permission DeleteOwnCustomer = new Permission { Description = "Delete own Customer", Name = "DeleteOwnCustomer", ImpliedBy = new[] { DeleteCustomer } };

        public static readonly Permission MetaListCustomers = new Permission { Description = "List all Customers", Name = "MetaListCustomers", ImpliedBy = new[] { EditCustomer, PublishCustomer, DeleteCustomer } };
        public static readonly Permission MetaListOwnCustomers = new Permission { Description = "List own Customers", Name = "MetaListOwnCustomers", ImpliedBy = new[] { MetaListCustomers, EditOwnCustomer, PublishOwnCustomer, DeleteOwnCustomer } };

        public static readonly Permission BulkActionCustomers = new Permission { Description = "---Bulk Action Customers", Name = "BulkActionCustomers", ImpliedBy = new[] { ManageCustomers } };
        public static readonly Permission AccessNegotiateCustomers = new Permission { Description = "---Access Negotiate Customers", Name = "AccessNegotiateCustomers", ImpliedBy = new[] { ManageCustomers } };
        public static readonly Permission AccessTradingCustomers = new Permission { Description = "---Access Trading Customers", Name = "AccessTradingCustomers", ImpliedBy = new[] { ManageCustomers } };

        #endregion

        #region Province, District, Ward, Street, Address

        public static readonly Permission ManageAddressLocations = new Permission { Description = "Manage Address Locations", Name = "ManageAddressLocations" };

        public static readonly Permission ManageLocationProvinces = new Permission { Description = "Manage provinces", Name = "ManageLocationProvinces", ImpliedBy = new[] { ManageAddressLocations } };
        public static readonly Permission ManageLocationDistricts = new Permission { Description = "Manage districts", Name = "ManageLocationDistricts", ImpliedBy = new[] { ManageAddressLocations } };
        public static readonly Permission ManageLocationWards = new Permission { Description = "Manage wards", Name = "ManageLocationWards", ImpliedBy = new[] { ManageAddressLocations } };
        public static readonly Permission ManageLocationStreets = new Permission { Description = "Manage streets", Name = "ManageLocationStreets", ImpliedBy = new[] { ManageAddressLocations } };
        public static readonly Permission ManageStreetRelations = new Permission { Description = "Manage street relations", Name = "ManageStreetRelations", ImpliedBy = new[] { ManageAddressLocations } };

        public static readonly Permission ManageLocationApartments = new Permission { Description = "Manage Apartments", Name = "ManageLocationApartments", ImpliedBy = new[] { ManageAddressLocations } };
        public static readonly Permission ManageLocationApartmentRelations = new Permission { Description = "Manage Apartment relations", Name = "ManageLocationApartmentRelations", ImpliedBy = new[] { ManageAddressLocations } };

        public static readonly Permission ManageDirections = new Permission { Description = "Manage directions", Name = "ManageDirections", ImpliedBy = new[] { ManageAddressLocations } };
        public static readonly Permission ManagePropertyLocations = new Permission { Description = "Manage property locations", Name = "ManagePropertyLocations", ImpliedBy = new[] { ManageAddressLocations } };

        public static readonly Permission MetaListAddressLocations = new Permission { Description = "List Address Locations", Name = "MetaListAddressLocations", ImpliedBy = new[] { ManageLocationProvinces, ManageLocationDistricts, ManageLocationWards, ManageLocationStreets, ManageStreetRelations, ManageLocationApartments, ManageLocationApartmentRelations, ManageDirections, ManagePropertyLocations } };

        #endregion

        #region Property attributes

        public static readonly Permission ManagePropertyAttributes = new Permission { Description = "Manage Property Attributes", Name = "ManagePropertyAttributes" };

        public static readonly Permission ManagePropertyStatus = new Permission { Description = "Manage property status", Name = "ManagePropertyStatus", ImpliedBy = new[] { ManagePropertyAttributes } };
        public static readonly Permission ManagePropertyFlags = new Permission { Description = "Manage property flags", Name = "ManagePropertyFlags", ImpliedBy = new[] { ManagePropertyAttributes } };

        public static readonly Permission ManagePropertyAdvantages = new Permission { Description = "Manage property interiors", Name = "ManagePropertyAdvantages", ImpliedBy = new[] { ManagePropertyAttributes } };
        public static readonly Permission ManagePropertyInteriors = new Permission { Description = "Manage property interiors", Name = "ManagePropertyInteriors", ImpliedBy = new[] { ManagePropertyAttributes } };
        public static readonly Permission ManagePropertyLegalStatus = new Permission { Description = "Manage property legal status", Name = "ManagePropertyLegalStatus", ImpliedBy = new[] { ManagePropertyAttributes } };

        public static readonly Permission ManagePropertyTypes = new Permission { Description = "Manage property types", Name = "ManagePropertyTypes", ImpliedBy = new[] { ManagePropertyAttributes } };
        public static readonly Permission ManagePropertyTypeGroups = new Permission { Description = "Manage property type groups", Name = "ManagePropertyTypeGroups", ImpliedBy = new[] { ManagePropertyAttributes } };

        public static readonly Permission ManageAdsTypes = new Permission { Description = "Manage ads types", Name = "ManageAdsTypes", ImpliedBy = new[] { ManagePropertyAttributes } };

        public static readonly Permission MetaListPropertyAttributes = new Permission { Description = "List Property Attributes", Name = "MetaListPropertyAttributes", ImpliedBy = new[] { ManagePropertyStatus, ManagePropertyFlags, ManagePropertyAdvantages, ManagePropertyInteriors, ManagePropertyLegalStatus, ManagePropertyTypes, ManagePropertyTypeGroups, ManageAdsTypes } };

        #endregion

        #region Customer attributes

        public static readonly Permission ManageCustomerAttributes = new Permission { Description = "Manage Customer Attributes", Name = "ManageCustomerAttributes" };

        public static readonly Permission ManageCustomerStatus = new Permission { Description = "Manage customer status", Name = "ManageCustomerStatus", ImpliedBy = new[] { ManageCustomerAttributes } };
        public static readonly Permission ManageCustomerPurposes = new Permission { Description = "Manage customer Purposes", Name = "ManageCustomerPurposes", ImpliedBy = new[] { ManageCustomerAttributes } };
        public static readonly Permission ManageCustomerFeedbacks = new Permission { Description = "Manage customer Feedbacks", Name = "ManageCustomerFeedbacks", ImpliedBy = new[] { ManageCustomerAttributes } };

        public static readonly Permission MetaListCustomerAttributes = new Permission { Description = "List Customer Attributes", Name = "MetaListCustomerAttributes", ImpliedBy = new[] { ManageCustomerStatus, ManageCustomerPurposes, ManageCustomerFeedbacks } };

        #endregion

        #region Payment

        public static readonly Permission ManagePaymentConfigs = new Permission { Description = "Manage payment Configs", Name = "ManagePaymentConfigs" };

        public static readonly Permission ManagePaymentMethods = new Permission { Description = "Manage payment methods", Name = "ManagePaymentMethods", ImpliedBy = new[] { ManagePaymentConfigs } };
        public static readonly Permission ManagePaymentUnits = new Permission { Description = "Manage payment units", Name = "ManagePaymentUnits", ImpliedBy = new[] { ManagePaymentConfigs } };
        public static readonly Permission ManagePaymentExchanges = new Permission { Description = "Manage payment exchanges", Name = "ManagePaymentExchanges", ImpliedBy = new[] { ManagePaymentConfigs } };

        public static readonly Permission MetaListPaymentConfigs = new Permission { Description = "List payment Configs", Name = "MetaListPaymentConfigs", ImpliedBy = new[] { ManagePaymentMethods, ManagePaymentUnits, ManagePaymentExchanges } };

        #endregion

        // Settings

        public static readonly Permission ManagePropertySettings = new Permission { Description = "Manage Settings", Name = "ManagePropertySettings" };

        // Configs

        public static readonly Permission ManageConfigs = new Permission { Description = "Manage Configs", Name = "ManageConfigs" };

        public static readonly Permission ManageCoefficientWidths = new Permission { Description = "Manage Width Coefficients", Name = "ManageCoefficientWidths", ImpliedBy = new[] { ManageConfigs } };
        public static readonly Permission ManageCoefficientLengths = new Permission { Description = "Manage Length Coefficients", Name = "ManageCoefficientLengths", ImpliedBy = new[] { ManageConfigs } };
        public static readonly Permission ManageCoefficientAlleys = new Permission { Description = "Manage Alley Coefficients", Name = "ManageCoefficientAlleys", ImpliedBy = new[] { ManageConfigs } };
        public static readonly Permission ManageCoefficientAlleyDistances = new Permission { Description = "Manage Alley Distance Coefficients", Name = "ManageCoefficientAlleyDistances", ImpliedBy = new[] { ManageConfigs } };

        public static readonly Permission MetaListConfigs = new Permission { Description = "List Configs", Name = "MetaListConfigs", ImpliedBy = new[] { ManageCoefficientWidths, ManageCoefficientLengths, ManageCoefficientAlleys, ManageCoefficientAlleyDistances, ManagePropertySettings } };

        // Users

        public static readonly Permission ManageUsers = new Permission { Description = "Manage users", Name = "ManageUsers" };
        public static readonly Permission ViewUserPoints = new Permission { Description = "View user points", Name = "ViewUserPoints", ImpliedBy = new[] { ManageUsers } };
        public static readonly Permission ManageListUserOnline = new Permission { Description = "Quản lý danh sách user online", Name = "ManageListUserOnline" };

        // JointGroup
        public static readonly Permission ManageJointGroup = new Permission { Description = "Manage joint group", Name = "ManageJointGroup" };
        public static readonly Permission EditJointGroupProfiles = new Permission { Description = "Edit joint group profiles", Name = "EditJointGroupProfiles", ImpliedBy = new[] { ManageJointGroup } };
        public static readonly Permission EditJointGroupSettings = new Permission { Description = "Edit joint group settings", Name = "EditJointGroupSettings", ImpliedBy = new[] { ManageJointGroup } };
        public static readonly Permission EditJointGroupContacts = new Permission { Description = "Edit joint group contacts", Name = "EditJointGroupContacts", ImpliedBy = new[] { ManageJointGroup } };
        public static readonly Permission EditJointGroupLocations = new Permission { Description = "Edit joint group locations", Name = "EditJointGroupLocations", ImpliedBy = new[] { ManageJointGroup } };
        public static readonly Permission EditJointGroupSharedLocations = new Permission { Description = "Edit joint group shared locations", Name = "EditJointGroupSharedLocations", ImpliedBy = new[] { ManageJointGroup } };
        public static readonly Permission EditJointGroupUserAgencies = new Permission { Description = "Edit joint group user agencies", Name = "EditJointGroupUserAgencies", ImpliedBy = new[] { ManageJointGroup } };

        public static readonly Permission ViewJointGroupUserPoints = new Permission { Description = "View group user points", Name = "ViewJointGroupUserPoints", ImpliedBy = new[] { ManageJointGroup, EditJointGroupProfiles, EditJointGroupSettings, EditJointGroupContacts, EditJointGroupLocations, EditJointGroupSharedLocations, EditJointGroupUserAgencies } };


        // PlanningMaps
        public static readonly Permission ManagePlanningMaps = new Permission { Description = "Manage Planning Maps", Name = "ManagePlanningMaps" };

        // Management AdsPayment
        public static readonly Permission ManageAdsPaymentConfig = new Permission { Description = "Manage AdsPayment Config", Name = "ManageAdsPaymentConfig" };
        public static readonly Permission ManageAdsPaymentHistory = new Permission { Description = "Manage AdsPayment History", Name = "ManageAdsPaymentHistory", ImpliedBy = new[] { ManageAdsPaymentConfig } };
        public static readonly Permission ManageAddAdsPayment = new Permission { Description = "Manage Add AdsPayment", Name = "ManageAddAdsPayment", ImpliedBy = new[] { ManageAdsPaymentConfig, ManageAdsPaymentHistory } };

        //Management YoutubeVideo
        public static readonly Permission ManageYoutubeVideo = new Permission { Description = "Manage Youtube Video", Name = "ManageYoutubeVideo" };
        public static readonly Permission ManageApartmentCart = new Permission { Description = "Manage Apartment Cart", Name = "ManageApartmentCart" };

        //Management UnitInvest
        public static readonly Permission ManageUnitInvest = new Permission { Description = "Manage Unit Invest", Name = "ManageUnitInvest" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                
                NoRestrictedIP,
                NoRestrictedIPCustomer,

                #region Properties

                    ManageProperties,

                    PublishProperty,
                    PublishOwnProperty,
                    ApproveProperty,
                    ApproveOwnProperty,
                    CopyProperty,
                    CopyOwnProperty,
                    EditProperty,
                    EditOwnProperty,
                    DeleteProperty,
                    DeleteOwnProperty,

                    MetaListDealGoodProperties,
                    MetaListDealVeryGoodProperties,
                    MetaListProperties,
                    MetaListOwnProperties,

                    RequireAddressNumber,
                    EditAddressNumber,
                    EditContactPhoneToDisplay,

                    SetAdsGoodDeal,
                    SetAdsVIP,
                    SetAdsHighlight,

                    ExportProperties,
                    BulkActionProperties,

                    ViewDebugLogEstimateProperties,
                    ViewReferencedProperties,

                    AccessNegotiateProperties,
                    AccessTradingProperties,
                    AccessSoldProperties,
                    AccessOnHoldProperties,
                
                    EditPropertyImages,

                    ShareProperty,
                    ShareOwnProperty,
                #endregion

                #region Customers

                ManageCustomers,

                PublishCustomer,
                PublishOwnCustomer,
                EditCustomer,
                EditOwnCustomer,
                DeleteCustomer,
                DeleteCustomerProperty,
                DeleteOwnCustomer,

                MetaListCustomers,
                MetaListOwnCustomers,

                BulkActionCustomers,
                AccessNegotiateCustomers,
                AccessTradingCustomers,
                #endregion

                #region Province, District, Ward, Street, Address

                ManageAddressLocations,

                ManageLocationProvinces,
                ManageLocationDistricts,
                ManageLocationWards,
                ManageLocationStreets,
                ManageStreetRelations,

                ManageLocationApartments,
                ManageLocationApartmentRelations,

                ManageDirections,
                ManagePropertyLocations,

                MetaListAddressLocations,
                #endregion

                #region Property attributes

                ManagePropertyAttributes,

                ManagePropertyStatus,
                ManagePropertyFlags,

                ManagePropertyAdvantages,
                ManagePropertyInteriors,
                ManagePropertyLegalStatus,

                ManagePropertyTypes,
                ManagePropertyTypeGroups,

                ManageAdsTypes,

                MetaListPropertyAttributes,

                #endregion

                #region Customer attributes

                ManageCustomerAttributes,

                ManageCustomerStatus,
                ManageCustomerPurposes,
                ManageCustomerFeedbacks,

                MetaListCustomerAttributes,

                #endregion Payment

                ManagePaymentConfigs,

                ManagePaymentMethods,
                ManagePaymentUnits,
                ManagePaymentExchanges,

                MetaListPaymentConfigs,

                // Settings

                ManagePropertySettings,

                // Configs

                ManageConfigs,

                ManageCoefficientWidths,
                ManageCoefficientLengths,
                ManageCoefficientAlleys,
                ManageCoefficientAlleyDistances,

                MetaListConfigs,

                // Users

                ManageUsers,
                ViewUserPoints,
                ManageListUserOnline,

                // JointGroup

                ManageJointGroup,
                ViewJointGroupUserPoints,
                EditJointGroupProfiles,
                EditJointGroupSettings,
                EditJointGroupContacts,
                EditJointGroupLocations,
                EditJointGroupSharedLocations,
                EditJointGroupUserAgencies,

                // PlanningMaps

                ManagePlanningMaps,

                // AdsPayment
                ManageAdsPaymentConfig,
                ManageAdsPaymentHistory,
                ManageAddAdsPayment,
                ManageYoutubeVideo,
                ManageUnitInvest,
                ManageApartmentCart
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {

                new PermissionStereotype { Name = "REV_NoRestrictedIP", Permissions = new[] { NoRestrictedIP } },
                new PermissionStereotype { Name = "REV_NoRestrictedIPCustomer", Permissions = new[] { NoRestrictedIPCustomer } },
                new PermissionStereotype { Name = "REV_RequireAddressNumber", Permissions = new[] { RequireAddressNumber } },
                new PermissionStereotype { Name = "REV_EditAddressNumber", Permissions = new[] { EditAddressNumber } },
                new PermissionStereotype { Name = "REV_ExportProperties", Permissions = new[] { ExportProperties } },
                new PermissionStereotype { Name = "REV_BulkActionProperties", Permissions = new[] { BulkActionProperties } },
                new PermissionStereotype { Name = "REV_AccessNegotiateProperties", Permissions = new[] { AccessNegotiateProperties } },
                new PermissionStereotype { Name = "REV_AccessTradingProperties", Permissions = new[] { AccessTradingProperties } },
                new PermissionStereotype { Name = "REV_EditPropertyImages", Permissions = new[] { EditPropertyImages } },
                new PermissionStereotype { Name = "REV_BulkActionCustomers", Permissions = new[] { BulkActionCustomers } },
                new PermissionStereotype { Name = "REV_AccessNegotiateCustomers", Permissions = new[] { AccessNegotiateCustomers } },
                new PermissionStereotype { Name = "REV_AccessTradingCustomers", Permissions = new[] { AccessTradingCustomers } },
                new PermissionStereotype { Name = "REV_ViewUserPoints", Permissions = new[] { ViewUserPoints } },

                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {
                        NoRestrictedIP,
                        NoRestrictedIPCustomer,
                        RequireAddressNumber,
                        ManageProperties,
                        ManageCustomers,
                        ManageAddressLocations,
                        ManagePropertyAttributes,
                        ManageCustomerAttributes,
                        ManagePaymentConfigs,
                        ManagePropertySettings,
                        ManageConfigs,
                        ManageUsers,
                        ManageJointGroup,
                        ManagePlanningMaps,
                        ManageAdsPaymentConfig,
                        ManageAddAdsPayment,
                        ManageListUserOnline,
                        ManageYoutubeVideo,
                        ManageUnitInvest,
                        ManageApartmentCart
                    }
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {
                        MetaListProperties,
                        MetaListCustomers,
                        ManageAddressLocations,
                        ManagePropertyAttributes,
                        ManageCustomerAttributes,
                        ManagePaymentConfigs,
                        ManagePropertySettings,
                        ManageUsers,
                        ManagePlanningMaps,
                    }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {RequireAddressNumber,PublishProperty,EditProperty,PublishCustomer,EditCustomer}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {PublishOwnProperty,EditOwnProperty,DeleteOwnProperty}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {EditOwnProperty,DeleteOwnProperty}
                },
            };
        }

    }
}


