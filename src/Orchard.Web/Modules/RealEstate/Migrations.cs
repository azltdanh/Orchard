using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Extensions;
using Orchard.Data.Migration;
using Orchard.Localization;
using Orchard.Security;
using RealEstate.Models;
using RealEstate.Services;

namespace RealEstate
{
    public class Migrations : DataMigrationImpl
    {
        private readonly IMembershipService _membershipService;
        private readonly IPropertyService _propertyService;

        public Migrations(IOrchardServices services, IMembershipService membershipService,
            IPropertyService propertyServices)
        {
            Services = services;
            _membershipService = membershipService;
            _propertyService = propertyServices;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        #region Create

        public int Create()
        {
            #region Property Attributes

            // Creating table PropertyTypeGroupPartRecord
            SchemaBuilder.CreateTable("PropertyTypeGroupPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyTypeGroup",
                cfg => cfg.WithPart("PropertyTypeGroupPart"));

            // Creating table PropertyTypePartRecord
            SchemaBuilder.CreateTable("PropertyTypePartRecord", table => table
                .ContentPartRecord()
                .Column("Group_id", DbType.Int32)
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("UnitPrice", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyType", cfg => cfg.WithPart("PropertyTypePart"));

            // Creating table PropertyStatusPartRecord
            SchemaBuilder.CreateTable("PropertyStatusPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyStatus", cfg => cfg.WithPart("PropertyStatusPart"));

            // Creating table PropertyFlagPartRecord
            SchemaBuilder.CreateTable("PropertyFlagPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("Value", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyFlag", cfg => cfg.WithPart("PropertyFlagPart"));

            // Creating table PropertyLegalStatusPartRecord
            SchemaBuilder.CreateTable("PropertyLegalStatusPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyLegalStatus",
                cfg => cfg.WithPart("PropertyLegalStatusPart"));

            // Creating table DirectionPartRecord
            SchemaBuilder.CreateTable("DirectionPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("Direction", cfg => cfg.WithPart("DirectionPart"));

            // Creating table PropertyLocationPartRecord
            SchemaBuilder.CreateTable("PropertyLocationPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyLocation", cfg => cfg.WithPart("PropertyLocationPart"));

            // Creating table PropertyInteriorPartRecord
            SchemaBuilder.CreateTable("PropertyInteriorPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("UnitPrice", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyInterior", cfg => cfg.WithPart("PropertyInteriorPart"));

            // Creating table AdsTypePartRecord
            SchemaBuilder.CreateTable("AdsTypePartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("AdsType", cfg => cfg.WithPart("AdsTypePart"));

            #endregion

            #region Province, District, Ward, Street

            // Creating table LocationProvincePartRecord
            SchemaBuilder.CreateTable("LocationProvincePartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationProvince", cfg => cfg.WithPart("LocationProvincePart"));

            // Creating table LocationDistrictPartRecord
            SchemaBuilder.CreateTable("LocationDistrictPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationDistrict", cfg => cfg.WithPart("LocationDistrictPart"));

            // Creating table LocationWardPartRecord
            SchemaBuilder.CreateTable("LocationWardPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationWard", cfg => cfg.WithPart("LocationWardPart"));

            // Creating table LocationStreetPartRecord
            SchemaBuilder.CreateTable("LocationStreetPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("Ward_id", DbType.Int32)
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("StreetWidth", DbType.Double)
                .Column("IsOneWayStreet", DbType.Boolean, c => c.WithDefault(false))
                .Column("CoefficientAlley1Max", DbType.Double)
                .Column("CoefficientAlley1Min", DbType.Double)
                .Column("CoefficientAlleyMax", DbType.Double)
                .Column("CoefficientAlleyMin", DbType.Double)
                .Column("CoefficientAlleyEqual", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationStreet", cfg => cfg.WithPart("LocationStreetPart"));

            // Creating table LocationStreetSegmentPartRecord
            SchemaBuilder.CreateTable("LocationStreetSegmentPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("Ward_id", DbType.Int32)
                .Column("Street_id", DbType.Int32)
                .Column("FromNumber", DbType.String)
                .Column("ToNumber", DbType.String)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("StreetWidth", DbType.Double)
                .Column("CoefficientAlley1Max", DbType.Double)
                .Column("CoefficientAlley1Min", DbType.Double)
                .Column("CoefficientAlleyMax", DbType.Double)
                .Column("CoefficientAlleyMin", DbType.Double)
                .Column("CoefficientAlleyEqual", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationStreetSegment",
                cfg => cfg.WithPart("LocationStreetSegmentPart"));

            // Creating table StreetRelationPartRecord
            SchemaBuilder.CreateTable("StreetRelationPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("Ward_id", DbType.Int32)
                .Column("Street_id", DbType.Int32)
                .Column("StreetWidth", DbType.Double)
                .Column("RelatedValue", DbType.Double)
                .Column("RelatedProvince_id", DbType.Int32)
                .Column("RelatedDistrict_id", DbType.Int32)
                .Column("RelatedWard_id", DbType.Int32)
                .Column("RelatedStreet_id", DbType.Int32)
                .Column("RelatedStreetWidth", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("StreetRelation", cfg => cfg.WithPart("StreetRelationPart"));

            #endregion

            #region Payment

            // Creating table PaymentMethodPartRecord
            SchemaBuilder.CreateTable("PaymentMethodPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("PaymentMethod", cfg => cfg.WithPart("PaymentMethodPart"));

            // Creating table PaymentUnitPartRecord
            SchemaBuilder.CreateTable("PaymentUnitPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("PaymentUnit", cfg => cfg.WithPart("PaymentUnitPart"));

            // Creating table PaymentExchangePartRecord
            SchemaBuilder.CreateTable("PaymentExchangePartRecord", table => table
                .ContentPartRecord()
                .Column("USD", DbType.Double)
                .Column("SJC", DbType.Double)
                .Column("Date", DbType.DateTime)
                );
            ContentDefinitionManager.AlterTypeDefinition("PaymentExchange", cfg => cfg.WithPart("PaymentExchangePart"));

            #endregion

            #region Coefficient & Setting

            // Creating table CoefficientLengthPartRecord
            SchemaBuilder.CreateTable("CoefficientLengthPartRecord", table => table
                .ContentPartRecord()
                .Column("WidthRange", DbType.Double)
                .Column("MinLength", DbType.Double)
                .Column("MaxLength", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("CoefficientLength",
                cfg => cfg.WithPart("CoefficientLengthPart"));

            // Creating table CoefficientAlleyPartRecord
            SchemaBuilder.CreateTable("CoefficientAlleyPartRecord", table => table
                .ContentPartRecord()
                .Column("StreetUnitPrice", DbType.Double)
                .Column("CoefficientAlley1Max", DbType.Double)
                .Column("CoefficientAlley1Min", DbType.Double)
                .Column("CoefficientAlleyMax", DbType.Double)
                .Column("CoefficientAlleyMin", DbType.Double)
                .Column("CoefficientAlleyEqual", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("CoefficientAlley", cfg => cfg.WithPart("CoefficientAlleyPart"));

            // Creating table CoefficientAlleyDistancePartRecord
            SchemaBuilder.CreateTable("CoefficientAlleyDistancePartRecord", table => table
                .ContentPartRecord()
                .Column("LastAlleyWidth", DbType.Double)
                .Column("MaxAlleyDistance", DbType.Double)
                .Column("CoefficientDistance", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("CoefficientAlleyDistance",
                cfg => cfg.WithPart("CoefficientAlleyDistancePart"));

            // Creating table PropertySettingPartRecord
            SchemaBuilder.CreateTable("PropertySettingPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String, column => column.WithLength(500))
                .Column("Value", DbType.String, column => column.WithLength(500))
                .Column("Description", DbType.String, column => column.WithLength(500))
                .Column("SeqOrder", DbType.Int32)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertySetting", cfg => cfg.WithPart("PropertySettingPart"));

            #endregion

            #region PROPERTY

            SchemaBuilder.CreateTable("PropertyPartRecord", table => table
                .ContentPartRecord()

                // Type
                .Column("Type_id", DbType.Int32)

                // Address
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("Ward_id", DbType.Int32)
                .Column("Street_id", DbType.Int32)
                .Column("AddressNumber", DbType.String)
                .Column("AlleyNumber", DbType.Int32)
                .Column("OtherProvinceName", DbType.String)
                .Column("OtherDistrictName", DbType.String)
                .Column("OtherWardName", DbType.String)
                .Column("OtherStreetName", DbType.String)

                // LegalStatus
                .Column("LegalStatus_id", DbType.Int32)
                // Direction
                .Column("Direction_id", DbType.Int32)
                // Location
                .Column("Location_id", DbType.Int32)

                // Alley
                .Column("AlleyTurns", DbType.Int32)
                .Column("DistanceToStreet", DbType.Double)
                .Column("AlleyWidth", DbType.Double)
                .Column("AlleyWidth1", DbType.Double)
                .Column("AlleyWidth2", DbType.Double)
                .Column("AlleyWidth3", DbType.Double)
                .Column("AlleyWidth4", DbType.Double)
                .Column("AlleyWidth5", DbType.Double)
                .Column("AlleyWidth6", DbType.Double)
                .Column("AlleyWidth7", DbType.Double)
                .Column("AlleyWidth8", DbType.Double)
                .Column("AlleyWidth9", DbType.Double)
                .Column("StreetWidth", DbType.Double)

                // AreaTotal
                .Column("AreaTotal", DbType.Double)
                .Column("AreaTotalWidth", DbType.Double)
                .Column("AreaTotalLength", DbType.Double)
                .Column("AreaTotalBackWidth", DbType.Double)

                // AreaLegal
                .Column("AreaLegal", DbType.Double)
                .Column("AreaLegalWidth", DbType.Double)
                .Column("AreaLegalLength", DbType.Double)
                .Column("AreaLegalBackWidth", DbType.Double)
                .Column("AreaIlegalRecognized", DbType.Double)
                .Column("AreaIlegalNotRecognized", DbType.Double)

                // Construction
                .Column("AreaConstruction", DbType.Double)
                .Column("AreaUsable", DbType.Double)
                .Column("Floors", DbType.Double)
                .Column("Bedrooms", DbType.Int32)
                .Column("Livingrooms", DbType.Int32)
                .Column("Bathrooms", DbType.Int32)
                .Column("Balconies", DbType.Int32)
                .Column("Interior_id", DbType.Int32)
                .Column("RemainingValue", DbType.Double)

                // Construction
                .Column("HaveBasement", DbType.Boolean)
                .Column("HaveMezzanine", DbType.Boolean)
                .Column("HaveElevator", DbType.Boolean)
                .Column("HaveSwimmingPool", DbType.Boolean)
                .Column("HaveGarage", DbType.Boolean)
                .Column("HaveGarden", DbType.Boolean)
                .Column("HaveTerrace", DbType.Boolean)
                .Column("HaveSkylight", DbType.Boolean)

                // Advantage
                .Column("AdvCornerStreet", DbType.Boolean)
                .Column("AdvCornerStreetAlley", DbType.Boolean)
                .Column("AdvCornerAlley", DbType.Boolean)
                .Column("AdvCornerAlleySmall", DbType.Boolean)
                .Column("AdvDoubleFront", DbType.Boolean)
                .Column("AdvNearSuperMarket", DbType.Boolean)
                .Column("AdvNearTradeCenter", DbType.Boolean)
                .Column("AdvNearPark", DbType.Boolean)
                .Column("AdvSecurityArea", DbType.Boolean)
                .Column("AdvLuxuryResidential", DbType.Boolean)

                // DisAdvantage
                .Column("DAdvFacingAlley", DbType.Boolean)
                .Column("DAdvFacingTemple", DbType.Boolean)
                .Column("DAdvFacingChurch", DbType.Boolean)
                .Column("DadvFacingFuneral", DbType.Boolean)
                .Column("DadvUnderBridge", DbType.Boolean)
                .Column("DAdvFacingDrain", DbType.Boolean)
                .Column("DAdvFacingBigTree", DbType.Boolean)
                .Column("DAdvFacingElectricityCylindrical", DbType.Boolean)
                .Column("DAdvUnderHighVoltageLines", DbType.Boolean)
                .Column("DadvShareWall", DbType.Boolean)
                .Column("DAdvBuildingHeightRestriction", DbType.Boolean)
                .Column("DAdvPlanningSuspended", DbType.Boolean)
                .Column("DAdvComplexSecurityArea", DbType.Boolean)

                // Contact
                .Column("ContactName", DbType.String)
                .Column("ContactPhone", DbType.String)
                .Column("ContactAddress", DbType.String)
                .Column("ContactEmail", DbType.String)

                // Price
                .Column("PriceProposed", DbType.Double)
                .Column("PriceProposedInVND", DbType.Double)
                .Column("PriceEstimatedInVND", DbType.Double)
                .Column("PaymentMethod_id", DbType.Int32)
                .Column("PaymentUnit_id", DbType.Int32)

                // Flag & Status
                .Column("Published", DbType.Boolean)
                .Column("Status_id", DbType.Int32)
                .Column("Flag_id", DbType.Int32)
                .Column("IsExcludeFromPriceEstimation", DbType.Boolean)

                // Advertise
                .Column("AdsOnline", DbType.Boolean)
                .Column("AdsOnlineDate", DbType.DateTime)
                .Column("AdsNewspaper", DbType.Boolean)
                .Column("AdsNewspaperDate", DbType.DateTime)

                // User
                .Column("CreatedDate", DbType.DateTime)
                .Column("CreatedUser_id", DbType.Int32)
                .Column("LastUpdatedDate", DbType.DateTime)
                .Column("LastUpdatedUser_id", DbType.Int32)
                .Column("FirstInfoFromUser_id", DbType.Int32)
                .Column("LastInfoFromUser_id", DbType.Int32)

                // Ads Type
                .Column("AdsType_id", DbType.Int32)
                // Ads Content
                .Column("Title", DbType.String)
                .Column("Description", DbType.String, column => column.WithLength(500))
                .Column("Note", DbType.String)
                .Column("PropertyId", DbType.Int32)
                );
            ContentDefinitionManager.AlterTypeDefinition("Property", cfg => cfg.WithPart("PropertyPart"));

            // Creating table PropertyFilePartRecord
            SchemaBuilder.CreateTable("PropertyFilePartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("Type", DbType.String)
                .Column("Path", DbType.String)
                .Column("Size", DbType.Int32)
                .Column("CreatedDate", DbType.DateTime)
                .Column("CreatedUser_id", DbType.Int32)
                .Column("PropertyPartRecord_Id", DbType.Int32)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyFile", cfg => cfg.WithPart("PropertyFilePart"));

            #endregion

            #region CUSTOMER

            // Creating table CustomerStatusPartRecord
            SchemaBuilder.CreateTable("CustomerStatusPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("CustomerStatus", cfg => cfg.WithPart("CustomerStatusPart"));

            // Creating table CustomerFeedbackPartRecord
            SchemaBuilder.CreateTable("CustomerFeedbackPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("CustomerFeedback", cfg => cfg.WithPart("CustomerFeedbackPart"));

            // Creating table CustomerPartRecord
            SchemaBuilder.CreateTable("CustomerPartRecord", table => table
                .ContentPartRecord()

                // Contact
                .Column("ContactName", DbType.String)
                .Column("ContactPhone", DbType.String)
                .Column("ContactAddress", DbType.String)
                .Column("ContactEmail", DbType.String)

                // Status
                .Column("Status_id", DbType.Int32)
                .Column("Note", DbType.String)

                // User
                .Column("CreatedDate", DbType.DateTime)
                .Column("CreatedUser_id", DbType.Int32)
                .Column("LastUpdatedDate", DbType.DateTime)
                .Column("LastUpdatedUser_id", DbType.Int32)
                .Column("LastCallDate", DbType.DateTime)
                .Column("LastCallUser_id", DbType.Int32)
                .Column("CustomerId", DbType.Int32)
                );
            ContentDefinitionManager.AlterTypeDefinition("Customer", cfg => cfg.WithPart("CustomerPart"));

            // Creating table CustomerPurposePartRecord
            SchemaBuilder.CreateTable("CustomerPurposePartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("CustomerPurpose", cfg => cfg.WithPart("CustomerPurposePart"));

            // Creating table CustomerPurposePartRecordContent
            SchemaBuilder.CreateTable("CustomerPurposePartRecordContent", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("CustomerPartRecord_Id", DbType.Int32)
                .Column("CustomerPurposePartRecord_Id", DbType.Int32)
                );

            // Creating table CustomerRequirementRecord
            SchemaBuilder.CreateTable("CustomerRequirementRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("GroupId", DbType.Int32)
                .Column("CustomerPartRecord_Id", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("LocationWardPartRecord_Id", DbType.Int32)
                .Column("LocationStreetPartRecord_Id", DbType.Int32)
                .Column("MinArea", DbType.Double)
                .Column("MaxArea", DbType.Double)
                .Column("MinWidth", DbType.Double)
                .Column("MaxWidth", DbType.Double)
                .Column("MinLength", DbType.Double)
                .Column("MaxLength", DbType.Double)
                .Column("DirectionPartRecord_Id", DbType.Int32)
                .Column("PropertyLocationPartRecord_Id", DbType.Int32)
                .Column("MinAlleyWidth", DbType.Double)
                .Column("MaxAlleyWidth", DbType.Double)
                .Column("MinAlleyTurns", DbType.Int32)
                .Column("MaxAlleyTurns", DbType.Int32)
                .Column("MinDistanceToStreet", DbType.Double)
                .Column("MaxDistanceToStreet", DbType.Double)
                .Column("MinFloors", DbType.Double)
                .Column("MaxFloors", DbType.Double)
                .Column("MinBedrooms", DbType.Int32)
                .Column("MaxBedrooms", DbType.Int32)
                .Column("MinBathrooms", DbType.Int32)
                .Column("MaxBathrooms", DbType.Int32)
                .Column("MinPrice", DbType.Double)
                .Column("MaxPrice", DbType.Double)
                .Column("PaymentMethodPartRecord_Id", DbType.Int32)
                );

            // Creating table CustomerPropertyRecord
            SchemaBuilder.CreateTable("CustomerPropertyRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("PropertyPartRecord_Id", DbType.Int32)
                .Column("CustomerPartRecord_Id", DbType.Int32)
                .Column("CustomerFeedbackPartRecord_Id", DbType.Int32)
                );

            // Creating table CustomerPropertyUserRecord
            SchemaBuilder.CreateTable("CustomerPropertyUserRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("CustomerPropertyRecord_Id", DbType.Int32)
                .Column("UserPartRecord_Id", DbType.Int32)
                .Column("VisitedDate", DbType.DateTime)
                .Column("IsWorkOverTime", DbType.Boolean)
                );

            #endregion

            #region USERS

            // Creating table UserActionPartRecord
            SchemaBuilder.CreateTable("UserActionPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("Point", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("UserAction", cfg => cfg.WithPart("UserActionPart"));

            // Creating table UserActivityPartRecord
            SchemaBuilder.CreateTable("UserActivityPartRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("CreatedDate", DbType.DateTime)
                .Column("UserPartRecord_Id", DbType.Int32)
                .Column("UserActionPartRecord_Id", DbType.Int32)
                .Column("PropertyPartRecord_Id", DbType.Int32)
                .Column("CustomerPartRecord_Id", DbType.Int32)
                );

            #endregion

            #region REVISION

            // Creating table RevisionPartRecord
            SchemaBuilder.CreateTable("RevisionPartRecord", table => table
                .ContentPartRecord()
                .Column("CreatedDate", DbType.DateTime)
                .Column("CreatedUser_id", DbType.Int32)
                .Column("ContentType", DbType.String)
                .Column("ContentTypeRecordId", DbType.Int32)
                .Column("FieldName", DbType.String)
                .Column("ValueBefore", DbType.String)
                .Column("ValueAfter", DbType.String)
                );
            ContentDefinitionManager.AlterTypeDefinition("Revision", cfg => cfg.WithPart("RevisionPart"));

            #endregion

            #region NOUSE

            // Creating table AddressPartRecord
            //SchemaBuilder.CreateTable("AddressPartRecord", table => table
            //    .ContentPartRecord()
            //    .Column("AddressNumber", DbType.String)
            //    .Column("AlleyNumber", DbType.Int32)
            //    .Column("OtherProvinceName", DbType.String)
            //    .Column("OtherDistrictName", DbType.String)
            //    .Column("OtherWardName", DbType.String)
            //    .Column("OtherStreetName", DbType.String)
            //    .Column("Province_id", DbType.Int32)
            //    .Column("District_id", DbType.Int32)
            //    .Column("Ward_id", DbType.Int32)
            //    .Column("Street_id", DbType.Int32)
            //);
            //ContentDefinitionManager.AlterTypeDefinition("Address", cfg => cfg.WithPart("AddressPart"));

            // Creating table ContactPartRecord
            //SchemaBuilder.CreateTable("ContactPartRecord", table => table
            //    .ContentPartRecord()
            //    .Column("Name", DbType.String)
            //    .Column("Phone", DbType.String)
            //    .Column("Address", DbType.String)
            //    .Column("Email", DbType.String)
            //);
            //ContentDefinitionManager.AlterTypeDefinition("Contact", cfg => cfg.WithPart("ContactPart"));

            #endregion

            return 1;
        }

        #endregion

        #region 1-10

        public int UpdateFrom1()
        {
            #region PROPERTY Attributes

            // Direction
            IEnumerable<DirectionPartRecord> directions =
                new List<DirectionPartRecord>
                {
                    new DirectionPartRecord
                    {
                        Name = "Ðông",
                        ShortName = "Đ",
                        CssClass = null,
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Tây",
                        ShortName = "T",
                        CssClass = null,
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Nam",
                        ShortName = "N",
                        CssClass = null,
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Bắc",
                        ShortName = "B",
                        CssClass = null,
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Đông Bắc",
                        ShortName = "ĐB",
                        CssClass = null,
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Đông Nam",
                        ShortName = "ĐN",
                        CssClass = null,
                        SeqOrder = 6,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Tây Bắc",
                        ShortName = "TB",
                        CssClass = null,
                        SeqOrder = 7,
                        IsEnabled = true
                    },
                    new DirectionPartRecord
                    {
                        Name = "Tây Nam",
                        ShortName = "TN",
                        CssClass = null,
                        SeqOrder = 8,
                        IsEnabled = true
                    },
                };
            foreach (DirectionPartRecord item in directions)
            {
                var model = Services.ContentManager.New<DirectionPart>("Direction");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // Status
            IEnumerable<PropertyStatusPartRecord> status =
                new List<PropertyStatusPartRecord>
                {
                    new PropertyStatusPartRecord
                    {
                        Name = "Mới nhập",
                        ShortName = "",
                        CssClass = "st-new",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Đang rao bán",
                        ShortName = "",
                        CssClass = "st-selling",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Đang thương lượng",
                        ShortName = "",
                        CssClass = "st-negotiate",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Chờ giao dịch",
                        ShortName = "GD",
                        CssClass = "st-trading",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Đã bán",
                        ShortName = "ĐB",
                        CssClass = "st-sold",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Tạm ngưng bán",
                        ShortName = "NB",
                        CssClass = "st-onhold",
                        SeqOrder = 6,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Chờ xóa",
                        ShortName = "CX",
                        CssClass = "st-trash",
                        SeqOrder = 7,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Đã xóa",
                        ShortName = "ĐX",
                        CssClass = "st-deleted",
                        SeqOrder = 8,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Đang chờ duyệt",
                        ShortName = "CD",
                        CssClass = "st-pending",
                        SeqOrder = 9,
                        IsEnabled = true
                    },
                    new PropertyStatusPartRecord
                    {
                        Name = "Không liên lạc được",
                        ShortName = "KLL",
                        CssClass = "st-no-contact",
                        SeqOrder = 10,
                        IsEnabled = true
                    },
                };
            foreach (PropertyStatusPartRecord item in status)
            {
                var model = Services.ContentManager.New<PropertyStatusPart>("PropertyStatus");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // Flag
            IEnumerable<PropertyFlagPartRecord> flags =
                new List<PropertyFlagPartRecord>
                {
                    new PropertyFlagPartRecord
                    {
                        Name = "Bình thường",
                        ShortName = "",
                        CssClass = "deal-normal",
                        SeqOrder = 1,
                        IsEnabled = true,
                        Value = 0
                    },
                    new PropertyFlagPartRecord
                    {
                        Name = "Nhà rẻ",
                        ShortName = "",
                        CssClass = "deal-good",
                        SeqOrder = 2,
                        IsEnabled = true,
                        Value = 15
                    },
                    new PropertyFlagPartRecord
                    {
                        Name = "Nhà rất rẻ",
                        ShortName = "",
                        CssClass = "deal-very-good",
                        SeqOrder = 3,
                        IsEnabled = true,
                        Value = 30
                    },
                    new PropertyFlagPartRecord
                    {
                        Name = "Nhà giá cao",
                        ShortName = "",
                        CssClass = "deal-bad",
                        SeqOrder = 4,
                        IsEnabled = true,
                        Value = -30
                    },
                    new PropertyFlagPartRecord
                    {
                        Name = "Chưa định giá được",
                        ShortName = "",
                        CssClass = "deal-unknow",
                        SeqOrder = 5,
                        IsEnabled = true,
                        Value = 0
                    },
                };
            foreach (PropertyFlagPartRecord item in flags)
            {
                var model = Services.ContentManager.New<PropertyFlagPart>("PropertyFlag");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                model.Value = item.Value;
                Services.ContentManager.Create(model);
            }

            // Interior
            IEnumerable<PropertyInteriorPartRecord> interiors =
                new List<PropertyInteriorPartRecord>
                {
                    new PropertyInteriorPartRecord
                    {
                        Name = "Xây thô",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 1,
                        IsEnabled = true,
                        UnitPrice = 1000000
                    },
                    new PropertyInteriorPartRecord
                    {
                        Name = "Xây dựng cơ bản",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 2,
                        IsEnabled = true,
                        UnitPrice = 2000000
                    },
                    new PropertyInteriorPartRecord
                    {
                        Name = "Xây vừa đủ tiện nghi",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 3,
                        IsEnabled = true,
                        UnitPrice = 3000000
                    },
                    new PropertyInteriorPartRecord
                    {
                        Name = "Xây sang trọng cao cấp",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 4,
                        IsEnabled = true,
                        UnitPrice = 4000000
                    },
                    new PropertyInteriorPartRecord
                    {
                        Name = "Xây cực kỳ cao cấp",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 5,
                        IsEnabled = true,
                        UnitPrice = 5000000
                    },
                };
            foreach (PropertyInteriorPartRecord item in interiors)
            {
                var model = Services.ContentManager.New<PropertyInteriorPart>("PropertyInterior");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                model.UnitPrice = item.UnitPrice;
                Services.ContentManager.Create(model);
            }

            // LegalStatus
            IEnumerable<PropertyLegalStatusPartRecord> legalStatus =
                new List<PropertyLegalStatusPartRecord>
                {
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Sổ hồng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Sổ đỏ",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Mẫu chủ quyền cũ",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Giấy tờ hợp lệ khác",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Hợp đồng góp vốn",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Đang hợp thức hóa",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 6,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Hợp đồng thuê dài hạn",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 7,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Tờ khai nhà đất",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 8,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Không có chủ quyền",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 9,
                        IsEnabled = true
                    },
                    new PropertyLegalStatusPartRecord
                    {
                        Name = "Khác",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 10,
                        IsEnabled = true
                    },
                };
            foreach (PropertyLegalStatusPartRecord item in legalStatus)
            {
                var model = Services.ContentManager.New<PropertyLegalStatusPart>("PropertyLegalStatus");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // TypeGroup
            IEnumerable<PropertyTypeGroupPartRecord> typeGroups =
                new List<PropertyTypeGroupPartRecord>
                {
                    new PropertyTypeGroupPartRecord
                    {
                        Name = "Nhà, đất đô thị",
                        ShortName = "",
                        CssClass = "gp-house",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new PropertyTypeGroupPartRecord
                    {
                        Name = "Căn hộ, chung cư",
                        ShortName = "",
                        CssClass = "gp-apartment",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new PropertyTypeGroupPartRecord
                    {
                        Name = "Các loại nhà đất khác",
                        ShortName = "",
                        CssClass = "gp-land",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new PropertyTypeGroupPartRecord
                    {
                        Name = "Đất dự án phân lô",
                        ShortName = "",
                        CssClass = "gp-project",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new PropertyTypeGroupPartRecord
                    {
                        Name = "Trang trại, resort",
                        ShortName = "",
                        CssClass = "gp-resort",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                };
            foreach (PropertyTypeGroupPartRecord item in typeGroups)
            {
                var model = Services.ContentManager.New<PropertyTypeGroupPart>("PropertyTypeGroup");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // PaymentMethod
            IEnumerable<PaymentMethodPartRecord> paymentMethods =
                new List<PaymentMethodPartRecord>
                {
                    new PaymentMethodPartRecord
                    {
                        Name = "Tỷ đồng",
                        ShortName = "Tỷ",
                        CssClass = "pm-vnd-b",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new PaymentMethodPartRecord
                    {
                        Name = "Triệu đồng",
                        ShortName = "Triệu",
                        CssClass = "pm-vnd-m",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new PaymentMethodPartRecord
                    {
                        Name = "Nghìn đồng",
                        ShortName = "Nghìn",
                        CssClass = "pm-vnd-k",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new PaymentMethodPartRecord
                    {
                        Name = "Lượng vàng",
                        ShortName = "Lượng",
                        CssClass = "pm-sjc",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new PaymentMethodPartRecord
                    {
                        Name = "USD",
                        ShortName = "USD",
                        CssClass = "pm-usd",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                };
            foreach (PaymentMethodPartRecord item in paymentMethods)
            {
                var model = Services.ContentManager.New<PaymentMethodPart>("PaymentMethod");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            #endregion

            #region CUSTOMER Attributes

            // Customer Status
            IEnumerable<CustomerStatusPartRecord> cstatus =
                new List<CustomerStatusPartRecord>
                {
                    new CustomerStatusPartRecord
                    {
                        Name = "Bình thường",
                        ShortName = "",
                        CssClass = "st-new",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Cần mua gấp",
                        ShortName = "",
                        CssClass = "st-high",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Đang thương lượng",
                        ShortName = "",
                        CssClass = "st-negotiate",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Chờ giao dịch",
                        ShortName = "",
                        CssClass = "st-trading",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Đã mua",
                        ShortName = "",
                        CssClass = "st-bought",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Tạm ngưng",
                        ShortName = "",
                        CssClass = "st-onhold",
                        SeqOrder = 6,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Hết nhu cầu",
                        ShortName = "",
                        CssClass = "st-suspended",
                        SeqOrder = 7,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Chờ xóa",
                        ShortName = "",
                        CssClass = "st-trash",
                        SeqOrder = 8,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Đã xóa",
                        ShortName = "",
                        CssClass = "st-deleted",
                        SeqOrder = 9,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Nghi cò",
                        ShortName = "",
                        CssClass = "st-doubt",
                        SeqOrder = 10,
                        IsEnabled = true
                    },
                    new CustomerStatusPartRecord
                    {
                        Name = "Cò",
                        ShortName = "",
                        CssClass = "st-broker",
                        SeqOrder = 11,
                        IsEnabled = true
                    },
                };
            foreach (CustomerStatusPartRecord item in cstatus)
            {
                var model = Services.ContentManager.New<CustomerStatusPart>("CustomerStatus");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // Customer Feedback
            IEnumerable<CustomerFeedbackPartRecord> feedbacks =
                new List<CustomerFeedbackPartRecord>
                {
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Chờ xem",
                        ShortName = "",
                        CssClass = "fb-wait-visit",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Đã xem",
                        ShortName = "",
                        CssClass = "fb-visited",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Không thích",
                        ShortName = "",
                        CssClass = "fb-dislike",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Đang xem xét",
                        ShortName = "",
                        CssClass = "fb-considering",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Đang thương lượng",
                        ShortName = "",
                        CssClass = "fb-dealing",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Chờ đặt cọc",
                        ShortName = "",
                        CssClass = "fb-wait-deposit",
                        SeqOrder = 6,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Đã đặt cọc",
                        ShortName = "",
                        CssClass = "fb-deposited",
                        SeqOrder = 7,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Mua thành công",
                        ShortName = "",
                        CssClass = "fb-bought-successful",
                        SeqOrder = 8,
                        IsEnabled = true
                    },
                    new CustomerFeedbackPartRecord
                    {
                        Name = "Mua thất  bại",
                        ShortName = "",
                        CssClass = "fb-bought-failed",
                        SeqOrder = 9,
                        IsEnabled = true
                    },
                };
            foreach (CustomerFeedbackPartRecord item in feedbacks)
            {
                var model = Services.ContentManager.New<CustomerFeedbackPart>("CustomerFeedback");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // Customer Purpose
            IEnumerable<CustomerPurposePartRecord> purposes =
                new List<CustomerPurposePartRecord>
                {
                    new CustomerPurposePartRecord
                    {
                        Name = "Để ở",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Làm văn phòng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Kinh doanh buôn bán",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 3,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Làm showroom",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 4,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Làm khách sạn",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 5,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Nhà hàng, cafe",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 6,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Cao ốc văn phòng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 7,
                        IsEnabled = true
                    },
                    new CustomerPurposePartRecord
                    {
                        Name = "Mua đầu tư",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 8,
                        IsEnabled = true
                    },
                };
            foreach (CustomerPurposePartRecord item in purposes)
            {
                var model = Services.ContentManager.New<CustomerPurposePart>("CustomerPurpose");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            #endregion

            #region PROPERTY Attributes

            // PaymentUnit
            IEnumerable<PaymentUnitPartRecord> paymentUnits =
                new List<PaymentUnitPartRecord>
                {
                    new PaymentUnitPartRecord
                    {
                        Name = "Tổng diện tích",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new PaymentUnitPartRecord
                    {
                        Name = "m2",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                };
            foreach (PaymentUnitPartRecord item in paymentUnits)
            {
                var model = Services.ContentManager.New<PaymentUnitPart>("PaymentUnit");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // PropertyLocation
            IEnumerable<PropertyLocationPartRecord> locations =
                new List<PropertyLocationPartRecord>
                {
                    new PropertyLocationPartRecord
                    {
                        Name = "Mặt tiền",
                        ShortName = "MT",
                        CssClass = "h-front",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new PropertyLocationPartRecord
                    {
                        Name = "Trong hẻm",
                        ShortName = "H",
                        CssClass = "h-alley",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                };
            foreach (PropertyLocationPartRecord item in locations)
            {
                var model = Services.ContentManager.New<PropertyLocationPart>("PropertyLocation");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            // AdsType
            IEnumerable<AdsTypePartRecord> adsTypes =
                new List<AdsTypePartRecord>
                {
                    new AdsTypePartRecord
                    {
                        Name = "Mua bán",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 1,
                        IsEnabled = true
                    },
                    new AdsTypePartRecord
                    {
                        Name = "Cho thuê",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 2,
                        IsEnabled = true
                    },
                };
            foreach (AdsTypePartRecord item in adsTypes)
            {
                var model = Services.ContentManager.New<AdsTypePart>("AdsType");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            #endregion

            #region USERS Attributes

            // User Action
            IEnumerable<UserActionPartRecord> actions =
                new List<UserActionPartRecord>
                {
                    new UserActionPartRecord
                    {
                        Name = "Thêm mới BĐS",
                        ShortName = "",
                        CssClass = "act-addnew",
                        SeqOrder = 1,
                        IsEnabled = true,
                        Point = 1
                    },
                    new UserActionPartRecord
                    {
                        Name = "Sửa thông tin BĐS",
                        ShortName = "",
                        CssClass = "act-update",
                        SeqOrder = 2,
                        IsEnabled = true,
                        Point = 0.2
                    },
                    new UserActionPartRecord
                    {
                        Name = "Dẫn khách",
                        ShortName = "",
                        CssClass = "act-customer",
                        SeqOrder = 3,
                        IsEnabled = true,
                        Point = 4
                    },
                    new UserActionPartRecord
                    {
                        Name = "BĐS nguồn chờ xóa",
                        ShortName = "",
                        CssClass = "act-deleted",
                        SeqOrder = 4,
                        IsEnabled = true,
                        Point = -1
                    },
                };
            foreach (UserActionPartRecord item in actions)
            {
                var model = Services.ContentManager.New<UserActionPart>("UserAction");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                model.Point = item.Point;
                Services.ContentManager.Create(model);
            }

            // PaymentExchange
            IEnumerable<PaymentExchangePartRecord> paymentExchanges =
                new List<PaymentExchangePartRecord>
                {
                    new PaymentExchangePartRecord {USD = 20960, Date = DateTime.Parse("2012-06-11")},
                    new PaymentExchangePartRecord {SJC = 41810000, Date = DateTime.Parse("2012-06-11")},
                };
            foreach (PaymentExchangePartRecord item in paymentExchanges)
            {
                var model = Services.ContentManager.New<PaymentExchangePart>("PaymentExchange");
                model.Record = item;
                Services.ContentManager.Create(model);
            }

            #endregion

            #region Coefficient

            // CoefficientLength
            IEnumerable<CoefficientLengthPartRecord> coeffLength =
                new List<CoefficientLengthPartRecord>
                {
                    new CoefficientLengthPartRecord {WidthRange = 1, MinLength = 2, MaxLength = 4},
                    new CoefficientLengthPartRecord {WidthRange = 2, MinLength = 3, MaxLength = 5},
                    new CoefficientLengthPartRecord {WidthRange = 2.5, MinLength = 4, MaxLength = 6},
                    new CoefficientLengthPartRecord {WidthRange = 3, MinLength = 6, MaxLength = 8},
                    new CoefficientLengthPartRecord {WidthRange = 3.5, MinLength = 8, MaxLength = 10},
                    new CoefficientLengthPartRecord {WidthRange = 4, MinLength = 10, MaxLength = 15},
                    new CoefficientLengthPartRecord {WidthRange = 5, MinLength = 11, MaxLength = 17},
                    new CoefficientLengthPartRecord {WidthRange = 6, MinLength = 11, MaxLength = 18},
                    new CoefficientLengthPartRecord {WidthRange = 7, MinLength = 12, MaxLength = 20},
                    new CoefficientLengthPartRecord {WidthRange = 8, MinLength = 13, MaxLength = 22},
                    new CoefficientLengthPartRecord {WidthRange = 9, MinLength = 15, MaxLength = 24},
                    new CoefficientLengthPartRecord {WidthRange = 10, MinLength = 16, MaxLength = 25},
                    new CoefficientLengthPartRecord {WidthRange = 20, MinLength = 17, MaxLength = 30},
                    new CoefficientLengthPartRecord {WidthRange = 30, MinLength = 18, MaxLength = 32},
                    new CoefficientLengthPartRecord {WidthRange = 40, MinLength = 19, MaxLength = 34},
                    new CoefficientLengthPartRecord {WidthRange = 50, MinLength = 20, MaxLength = 36},
                    new CoefficientLengthPartRecord {WidthRange = 60, MinLength = 21, MaxLength = 38},
                    new CoefficientLengthPartRecord {WidthRange = 70, MinLength = 22, MaxLength = 40},
                    new CoefficientLengthPartRecord {WidthRange = 80, MinLength = 23, MaxLength = 42},
                    new CoefficientLengthPartRecord {WidthRange = 90, MinLength = 24, MaxLength = 44},
                    new CoefficientLengthPartRecord {WidthRange = 100, MinLength = 25, MaxLength = 45},
                };
            foreach (CoefficientLengthPartRecord item in coeffLength)
            {
                var model = Services.ContentManager.New<CoefficientLengthPart>("CoefficientLength");
                model.WidthRange = item.WidthRange;
                model.MinLength = item.MinLength;
                model.MaxLength = item.MaxLength;
                Services.ContentManager.Create(model);
            }

            // CoefficientAlley
            IEnumerable<CoefficientAlleyPartRecord> coeffAlley =
                new List<CoefficientAlleyPartRecord>
                {
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 50,
                        CoefficientAlley1Max = 0.95,
                        CoefficientAlley1Min = 0.3,
                        CoefficientAlleyMax = 0.98,
                        CoefficientAlleyMin = 0.3,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 100,
                        CoefficientAlley1Max = 0.92,
                        CoefficientAlley1Min = 0.2,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.2,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 150,
                        CoefficientAlley1Max = 0.85,
                        CoefficientAlley1Min = 0.15,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.15,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 200,
                        CoefficientAlley1Max = 0.8,
                        CoefficientAlley1Min = 0.12,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 250,
                        CoefficientAlley1Max = 0.78,
                        CoefficientAlley1Min = 0.12,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 300,
                        CoefficientAlley1Max = 0.75,
                        CoefficientAlley1Min = 0.12,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 350,
                        CoefficientAlley1Max = 0.68,
                        CoefficientAlley1Min = 0.12,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 400,
                        CoefficientAlley1Max = 0.63,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 450,
                        CoefficientAlley1Max = 0.62,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 500,
                        CoefficientAlley1Max = 0.6,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 550,
                        CoefficientAlley1Max = 0.6,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 600,
                        CoefficientAlley1Max = 0.6,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 650,
                        CoefficientAlley1Max = 0.6,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 700,
                        CoefficientAlley1Max = 0.6,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                    new CoefficientAlleyPartRecord
                    {
                        StreetUnitPrice = 750,
                        CoefficientAlley1Max = 0.6,
                        CoefficientAlley1Min = 0.1,
                        CoefficientAlleyMax = 0.95,
                        CoefficientAlleyMin = 0.12,
                        CoefficientAlleyEqual = 0.9
                    },
                };
            foreach (CoefficientAlleyPartRecord item in coeffAlley)
            {
                var model = Services.ContentManager.New<CoefficientAlleyPart>("CoefficientAlley");
                model.StreetUnitPrice = item.StreetUnitPrice;
                model.CoefficientAlley1Max = item.CoefficientAlley1Max;
                model.CoefficientAlley1Min = item.CoefficientAlley1Min;
                model.CoefficientAlleyMax = item.CoefficientAlleyMax;
                model.CoefficientAlleyMin = item.CoefficientAlleyMin;
                model.CoefficientAlleyEqual = item.CoefficientAlleyEqual;
                Services.ContentManager.Create(model);
            }

            // CoefficientAlleyDistance
            IEnumerable<CoefficientAlleyDistancePartRecord> coeffAlleyDistance =
                new List<CoefficientAlleyDistancePartRecord>
                {
                    new CoefficientAlleyDistancePartRecord
                    {
                        LastAlleyWidth = 4,
                        MaxAlleyDistance = 500,
                        CoefficientDistance = 0.15
                    },
                    new CoefficientAlleyDistancePartRecord
                    {
                        LastAlleyWidth = 6,
                        MaxAlleyDistance = 700,
                        CoefficientDistance = 0.15
                    },
                    new CoefficientAlleyDistancePartRecord
                    {
                        LastAlleyWidth = 8,
                        MaxAlleyDistance = 1000,
                        CoefficientDistance = 0.15
                    },
                    new CoefficientAlleyDistancePartRecord
                    {
                        LastAlleyWidth = 8.01,
                        MaxAlleyDistance = 1200,
                        CoefficientDistance = 0.15
                    },
                };
            foreach (CoefficientAlleyDistancePartRecord item in coeffAlleyDistance)
            {
                var model = Services.ContentManager.New<CoefficientAlleyDistancePart>("CoefficientAlleyDistance");
                model.LastAlleyWidth = item.LastAlleyWidth;
                model.MaxAlleyDistance = item.MaxAlleyDistance;
                model.CoefficientDistance = item.CoefficientDistance;
                Services.ContentManager.Create(model);
            }

            #endregion

            #region Setting

            // PropertySetting
            IEnumerable<PropertySettingPartRecord> propertySetting =
                new List<PropertySettingPartRecord>
                {
                    new PropertySettingPartRecord
                    {
                        Name = "AllowedAdminSingleIPs",
                        Value = "222.254.179.74,222.254.179.208,222.254.182.19"
                    },
                    new PropertySettingPartRecord
                    {
                        Name = "AllowedAdminSingleIPsDetails",
                        Value = "222.254.179.74,222.254.179.208,222.254.182.19"
                    },
                    new PropertySettingPartRecord {Name = "AllowedAdminMaskedIPs", Value = "10.2.0.0;255.255.0.0"},
                    new PropertySettingPartRecord {Name = "DeniedAdminSingleIPs", Value = ""},
                    new PropertySettingPartRecord {Name = "DeniedAdminMaskedIPs", Value = ""},
                    new PropertySettingPartRecord {Name = "UploadPictures", Value = "/UserFiles/"},
                    new PropertySettingPartRecord {Name = "DaysToUpdatePrice", Value = "90"},
                    new PropertySettingPartRecord {Name = "DaysToUpdateNegotiateStatus", Value = "7"},
                    new PropertySettingPartRecord {Name = "DGMT_Cung_Doan_Duong", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGMT_Cung_Phuong", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGMT_GT_Tuong_Duong", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGH_Cung_Hem", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGH_Cung_Doan_Duong", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGH_Cung_Doan_Duong_MT", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGH_Cung_Phuong", Value = "6"},
                    new PropertySettingPartRecord {Name = "DGH_GT_Tuong_Duong", Value = "6"},
                    new PropertySettingPartRecord {Name = "Chieu_Ngang_Tieu_Chuan", Value = "4"},
                    new PropertySettingPartRecord {Name = "Chieu_Dai_Tieu_Chuan", Value = "20"},
                    new PropertySettingPartRecord {Name = "Do_Rong_Hem_Toi_Da", Value = "12"},
                    new PropertySettingPartRecord {Name = "Bien_Do_Don_Gia_Mat_Tien", Value = "20"},
                    new PropertySettingPartRecord {Name = "Vi_Pham_Lo_Gioi_Duoc_Cong_Nhan", Value = "50"},
                    new PropertySettingPartRecord {Name = "Vi_Pham_Lo_Gioi_Khong_Cong_Nhan", Value = "20"},
                    new PropertySettingPartRecord {Name = "Co_Tang_Ham", Value = "2"},
                    new PropertySettingPartRecord {Name = "Co_Gac_Lung", Value = "0.5"},
                    new PropertySettingPartRecord {Name = "Co_San_Thuong", Value = "1"},
                    new PropertySettingPartRecord {Name = "Co_Thang_May", Value = "300"},
                    new PropertySettingPartRecord {Name = "Co_Ho_Boi", Value = "150"},
                    new PropertySettingPartRecord {Name = "No_Hau", Value = "2"},
                    new PropertySettingPartRecord {Name = "Dien_Tich_Dat_Lon", Value = "-5"},
                    new PropertySettingPartRecord {Name = "Dien_Tich_Dat_Hep", Value = "-5"},
                    new PropertySettingPartRecord {Name = "Dien_Tich_Dat_Nho", Value = "0"},
                    new PropertySettingPartRecord {Name = "Top_Hau", Value = "-10"},
                };
            foreach (PropertySettingPartRecord item in propertySetting)
            {
                var model = Services.ContentManager.New<PropertySettingPart>("PropertySetting");
                model.Name = item.Name;
                model.Value = item.Value;
                Services.ContentManager.Create(model);
            }

            #endregion

            #region USERS

            // Users
            var userList = new List<string>
            {
                "3truong",
                "Binh",
                "chi",
                "chuong",
                "congty",
                "cuong",
                "du",
                "duchung",
                "dunggd",
                "Duong",
                "duy",
                "Ha",
                "han",
                "hoan",
                "hung",
                "Khanh",
                "KhanhLV",
                "lap",
                "mai",
                "nghia",
                "Ngoc",
                "ngothang",
                "phuoc",
                "Phuong",
                "Quyet",
                "tai",
                "TANH",
                "thang",
                "thang1",
                "Thi",
                "thien",
                "thuy",
                "thuytha",
                "Truongco",
                "Vinh"
            };
            foreach (string userName in userList)
            {
                // Attempt to register the user
                // No need to report this to IUserEventHandler because _membershipService does that for us
                _membershipService.CreateUser(new CreateUserParams(userName, "q1w2e3r4t5y6",
                    userName.ToLower() + "@dinhgianhadat.vn", null, null, false));
            }

            #endregion

            return 2;
        }

        // IMPORT Property Types
        public int UpdateFrom2()
        {
            PropertyTypeGroupPartRecord gpHouse = _propertyService.GetTypeGroup("gp-house");
            PropertyTypeGroupPartRecord gpApartment = _propertyService.GetTypeGroup("gp-apartment");
            PropertyTypeGroupPartRecord gpLand = _propertyService.GetTypeGroup("gp-land");
            PropertyTypeGroupPartRecord gpProject = _propertyService.GetTypeGroup("gp-project");

            // PropertyType
            IEnumerable<PropertyTypePartRecord> types =
                new List<PropertyTypePartRecord>
                {
                    new PropertyTypePartRecord
                    {
                        Name = "Đất ở",
                        ShortName = "",
                        CssClass = "tp-residential-land",
                        SeqOrder = 1,
                        IsEnabled = true,
                        UnitPrice = 0,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Nhà nát",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 2,
                        IsEnabled = true,
                        UnitPrice = 1000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Nhà cấp 4",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 3,
                        IsEnabled = true,
                        UnitPrice = 2000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Nhà gác gỗ",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 4,
                        IsEnabled = true,
                        UnitPrice = 2500000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Nhà phố đúc giả",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 5,
                        IsEnabled = true,
                        UnitPrice = 3000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Nhà phố đúc kiên cố",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 6,
                        IsEnabled = true,
                        UnitPrice = 4000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Biệt thự",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 7,
                        IsEnabled = true,
                        UnitPrice = 4000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Cao ốc văn phòng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 8,
                        IsEnabled = true,
                        UnitPrice = 5000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Khách sạn",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 9,
                        IsEnabled = true,
                        UnitPrice = 6000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Nhà hàng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 10,
                        IsEnabled = true,
                        UnitPrice = 5000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Quán cafe",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 10,
                        IsEnabled = true,
                        UnitPrice = 5000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Kho xưởng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 11,
                        IsEnabled = true,
                        UnitPrice = 2000000,
                        Group = gpHouse
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Mặt bằng",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 12,
                        IsEnabled = true,
                        UnitPrice = 0,
                        Group = gpApartment
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Một phần nhà, đất",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 13,
                        IsEnabled = true,
                        UnitPrice = 0,
                        Group = gpApartment
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Căn hộ, chung cư",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 14,
                        IsEnabled = true,
                        UnitPrice = 5000000,
                        Group = gpApartment
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Đất dự án phân lô",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 15,
                        IsEnabled = true,
                        UnitPrice = 0,
                        Group = gpProject
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Trang trại, resort",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 16,
                        IsEnabled = true,
                        UnitPrice = 0,
                        Group = gpLand
                    },
                    new PropertyTypePartRecord
                    {
                        Name = "Các loại đất khác",
                        ShortName = "",
                        CssClass = "",
                        SeqOrder = 17,
                        IsEnabled = true,
                        UnitPrice = 0,
                        Group = gpLand
                    },
                };
            foreach (PropertyTypePartRecord item in types)
            {
                var model = Services.ContentManager.New<PropertyTypePart>("PropertyType");
                model.Name = item.Name;
                model.ShortName = item.ShortName;
                model.CssClass = item.CssClass;
                model.SeqOrder = item.SeqOrder;
                model.IsEnabled = item.IsEnabled;
                model.UnitPrice = item.UnitPrice;
                model.Group = item.Group;
                Services.ContentManager.Create(model);
            }

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable("LocationDistrictPartRecord",
                table => table.AddColumn("ContactPhone", DbType.String));

            return 4;
        }

        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("LastExportedDate", DbType.DateTime));

            return 5;
        }

        public int UpdateFrom5()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("LastExportedUser_id", DbType.Int32));

            return 6;
        }

        public int UpdateFrom6()
        {
            SchemaBuilder.AlterTable("LocationStreetPartRecord",
                table => table.AddColumn("RelatedStreet_id", DbType.Int32));
            SchemaBuilder.AlterTable("LocationStreetPartRecord", table => table.AddColumn("FromNumber", DbType.Int32));
            SchemaBuilder.AlterTable("LocationStreetPartRecord", table => table.AddColumn("ToNumber", DbType.Int32));

            return 7;
        }

        public int UpdateFrom7()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OtherProjectName", DbType.String));

            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentFloors", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentElevators", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentBasements", DbType.Int32));

            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentFloorTh", DbType.Int32));

            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("ApartmentHaveChildcare", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentHavePark", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("ApartmentHaveSwimmingPool", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("ApartmentHaveSuperMarket", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("ApartmentHaveSportCenter", DbType.Boolean));

            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("InteriorHaveWoodFloor", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("InteriorHaveToiletEquipment", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("InteriorHaveKitchenEquipment", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("InteriorHaveBedCabinets", DbType.Boolean));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("InteriorHaveAirConditioner", DbType.Boolean));

            return 8;
        }

        public int UpdateFrom8()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentNumber", DbType.String));

            return 9;
        }

        public int UpdateFrom9()
        {
            // Creating table CustomerPurposePartRecord
            SchemaBuilder.CreateTable("PropertyAdvantagePartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("AddedValue", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyAdvantage",
                cfg => cfg.WithPart("PropertyAdvantagePart"));

            // Creating table PropertyAdvantagePartRecordContent
            SchemaBuilder.CreateTable("PropertyAdvantagePartRecordContent", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("PropertyPartRecord_Id", DbType.Int32)
                .Column("PropertyAdvantagePartRecord_Id", DbType.Int32)
                );

            return 10;
        }

        public int UpdateFrom10()
        {
            // PropertyAdvantage
            IEnumerable<PropertyAdvantagePartRecord> propertyAdvantage =
                new List<PropertyAdvantagePartRecord>
                {
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Nhà có 2 mặt đường chính",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 1,
                        IsEnabled = true,
                        AddedValue = 10
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Nhà có 1 mặt đường chính và 1 mặt hẻm",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 2,
                        IsEnabled = true,
                        AddedValue = 5
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Nhà có 2 mặt hẻm",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 3,
                        IsEnabled = true,
                        AddedValue = 5
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Nhà có hẻm sau lưng",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 4,
                        IsEnabled = true,
                        AddedValue = 3
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Gần chợ, siêu thị (<500m)",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 5,
                        IsEnabled = true,
                        AddedValue = 2
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Gần công viên, trung tâm giải trí (<500m)",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 6,
                        IsEnabled = true,
                        AddedValue = 2
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Khu dân cư cao cấp có cổng bảo vệ",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 7,
                        IsEnabled = true,
                        AddedValue = 5
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Khu trung tâm thương mại",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 8,
                        IsEnabled = true,
                        AddedValue = 2
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Tiện làm quán cafe, nhà hàng, khách sạn",
                        ShortName = "adv",
                        CssClass = "adv-",
                        SeqOrder = 9,
                        IsEnabled = true,
                        AddedValue = 2
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Đường, hẻm đâm thẳng vào nhà",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 1,
                        IsEnabled = true,
                        AddedValue = -20
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Đối diện hoặc gần sát chùa, nhà thờ",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 2,
                        IsEnabled = true,
                        AddedValue = -10
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Đối diện hoặc gần sát nhà tang lễ, nhà xác",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 3,
                        IsEnabled = true,
                        AddedValue = -15
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Dưới hoặc gần chân cầu, đường điện cao thế",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 4,
                        IsEnabled = true,
                        AddedValue = -30
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Có cống trước nhà",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 5,
                        IsEnabled = true,
                        AddedValue = -5
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Có trụ điện trước nhà",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 6,
                        IsEnabled = true,
                        AddedValue = -5
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Có cây lớn trước nhà",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 7,
                        IsEnabled = true,
                        AddedValue = -5
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Tường chung không thể xây mới",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 8,
                        IsEnabled = true,
                        AddedValue = -10
                    },
                    new PropertyAdvantagePartRecord
                    {
                        Name = "Khu quy hoạch treo",
                        ShortName = "disadv",
                        CssClass = "disadv-",
                        SeqOrder = 9,
                        IsEnabled = true,
                        AddedValue = -30
                    },
                };
            foreach (PropertyAdvantagePartRecord item in propertyAdvantage)
            {
                var model = Services.ContentManager.New<PropertyAdvantagePart>("PropertyAdvantage");
                model.Record = item;
                Services.ContentManager.Create(model);
            }

            return 11;
        }

        #endregion

        #region 11-20

        public int UpdateFrom11()
        {
            SchemaBuilder.AlterTable("StreetRelationPartRecord",
                table => table.AddColumn("CoefficientAlley1Max", DbType.Double));
            SchemaBuilder.AlterTable("StreetRelationPartRecord",
                table => table.AddColumn("CoefficientAlley1Min", DbType.Double));
            SchemaBuilder.AlterTable("StreetRelationPartRecord",
                table => table.AddColumn("CoefficientAlleyEqual", DbType.Double));
            SchemaBuilder.AlterTable("StreetRelationPartRecord",
                table => table.AddColumn("CoefficientAlleyMax", DbType.Double));
            SchemaBuilder.AlterTable("StreetRelationPartRecord",
                table => table.AddColumn("CoefficientAlleyMin", DbType.Double));

            return 12;
        }

        public int UpdateFrom12()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("AdsVIP", DbType.Boolean));

            return 13;
        }

        public int UpdateFrom13()
        {
            // Creating table UserGroupPartRecord
            SchemaBuilder.CreateTable("UserGroupPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("Point", DbType.Double)
                .Column("GroupAdminUser_Id", DbType.Int32)
                .Column("AllowedAdminSingleIPs", DbType.String)
                .Column("AllowedAdminMaskedIPs", DbType.String)
                .Column("DeniedAdminSingleIPs", DbType.String)
                .Column("DeniedAdminMaskedIPs", DbType.String)
                );
            ContentDefinitionManager.AlterTypeDefinition("UserGroup", cfg => cfg.WithPart("UserGroupPart"));

            // Creating table UserInGroupRecord
            SchemaBuilder.CreateTable("UserInGroupRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("UserPartRecord_Id", DbType.Int32)
                .Column("UserGroupPartRecord_Id", DbType.Int32)
                );

            return 14;
        }

        public int UpdateFrom14()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("StatusChangedDate", DbType.DateTime));

            return 15;
        }

        public int UpdateFrom15()
        {
            SchemaBuilder.AlterTable("CustomerPartRecord",
                table => table.AddColumn("StatusChangedDate", DbType.DateTime));

            return 16;
        }

        public int UpdateFrom16()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("PublishAddress", DbType.Boolean));

            return 17;
        }

        public int UpdateFrom17()
        {
            SchemaBuilder.AlterTable("UserGroupPartRecord", table => table.AddColumn("ContactPhone", DbType.String));

            return 18;
        }

        public int UpdateFrom18()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("AdsGoodDeal", DbType.Boolean));

            // Remove some old colunn
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvCornerStreet"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvCornerStreetAlley"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvCornerAlley"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvCornerAlleySmall"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvDoubleFront"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvNearSuperMarket"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvNearTradeCenter"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvNearPark"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvSecurityArea"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdvLuxuryResidential"));

            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvFacingAlley"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvFacingTemple"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvFacingChurch"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DadvFacingFuneral"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DadvUnderBridge"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvFacingDrain"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvFacingBigTree"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvFacingElectricityCylindrical"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvUnderHighVoltageLines"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DadvShareWall"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvBuildingHeightRestriction"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvPlanningSuspended"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("DAdvComplexSecurityArea"));

            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdsOnline"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdsOnlineDate"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdsNewspaper"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("AdsNewspaperDate"));

            return 19;
        }

        public int UpdateFrom19()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("Copied", DbType.Boolean));

            return 20;
        }

        public int UpdateFrom20()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("PriceEstimatedRatingPoint", DbType.Double));

            return 21;
        }

        #endregion

        #region 21-30

        public int UpdateFrom21()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("PriceEstimatedComment", DbType.String, column => column.WithLength(500)));

            return 22;
        }

        public int UpdateFrom22()
        {
            SchemaBuilder.AlterTable("PropertyFilePartRecord",
                table => table.AddColumn("Published", DbType.Boolean, column => column.WithDefault(false)));

            return 23;
        }

        public int UpdateFrom23()
        {
            SchemaBuilder.AlterTable("PropertyFilePartRecord",
                table => table.AddColumn("IsAvatar", DbType.Boolean, column => column.WithDefault(false)));

            return 24;
        }

        public int UpdateFrom24()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("PublishContact", DbType.Boolean));

            return 25;
        }

        public int UpdateFrom25()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsExpirationDate", DbType.DateTime));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsGoodDealExpirationDate", DbType.DateTime));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsVIPExpirationDate", DbType.DateTime));

            return 26;
        }

        public int UpdateFrom26()
        {
            // Nhà chính chủ
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("IsOwner", DbType.Boolean, column => column.WithDefault(false)));
            // Miễn trung gian
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("NoBroker", DbType.Boolean, column => column.WithDefault(false)));

            return 27;
        }

        public int UpdateFrom27()
        {
            // Diện tích đất thổ cư
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("AreaResidential", DbType.Double));

            return 28;
        }

        public int UpdateFrom28()
        {
            // Tổng diện tích sàn
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AreaConstructionFloor", DbType.Double));

            return 29;
        }

        public int UpdateFrom29()
        {
            // Nhà ngân hàng phát mãi
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("IsAuction", DbType.Boolean, column => column.WithDefault(false)));
            // Nhóm BĐS
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("TypeGroup_id", DbType.Int32));

            return 30;
        }

        public int UpdateFrom30()
        {
            // IdStr dùng để search theo Id của BĐS
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("IdStr", DbType.String));

            return 31;
        }

        #endregion

        #region 31-40

        public int UpdateFrom31()
        {
            // AdsTypePartRecord loại tin khách hàng "Cần mua", "Cần thuê"
            SchemaBuilder.AlterTable("CustomerRequirementRecord",
                table => table.AddColumn("AdsTypePartRecord_Id", DbType.Int32));

            return 32;
        }

        public int UpdateFrom32()
        {
            // PropertyTypeGroupPartRecord nhóm BĐS
            SchemaBuilder.AlterTable("CustomerRequirementRecord",
                table => table.AddColumn("PropertyTypeGroupPartRecord_Id", DbType.Int32));

            return 33;
        }

        public int UpdateFrom33()
        {
            // Tên dự án
            SchemaBuilder.AlterTable("CustomerRequirementRecord",
                table => table.AddColumn("OtherProjectName", DbType.String));
            // Vị trí tầng
            SchemaBuilder.AlterTable("CustomerRequirementRecord",
                table => table.AddColumn("MinApartmentFloorTh", DbType.Int32));
            SchemaBuilder.AlterTable("CustomerRequirementRecord",
                table => table.AddColumn("MaxApartmentFloorTh", DbType.Int32));

            return 34;
        }

        public int UpdateFrom34()
        {
            // UserGroup - Group của CreatedUser, không thể thay đổi
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("UserGroup_id", DbType.Int32));

            return 35;
        }

        public int UpdateFrom35()
        {
            SchemaBuilder.AlterTable("UserGroupPartRecord", table => table.AddColumn("DefaultProvince_id", DbType.Int32));
            SchemaBuilder.AlterTable("UserGroupPartRecord",
                table => table.AddColumn("DefaultPropertyStatus_id", DbType.Int32));

            return 36;
        }

        public int UpdateFrom36()
        {
            // Creating table UserGroupLocationRecord
            // Phân quyền cho các Group theo tỉnh, quận, phường.
            SchemaBuilder.CreateTable("UserGroupLocationRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("UserGroupPartRecord_Id", DbType.Int32)
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("LocationWardPartRecord_Id", DbType.Int32)
                );

            return 37;
        }

        public int UpdateFrom37()
        {
            // IdStr dùng để search theo Id của KH
            SchemaBuilder.AlterTable("CustomerPartRecord", table => table.AddColumn("IdStr", DbType.String));

            // UserGroup - Group của CreatedUser, không thể thay đổi
            SchemaBuilder.AlterTable("CustomerPartRecord", table => table.AddColumn("UserGroup_id", DbType.Int32));

            return 38;
        }

        public int UpdateFrom38()
        {
            SchemaBuilder.AlterTable("CustomerPartRecord",
                table => table.AddColumn("Published", DbType.Boolean, column => column.WithDefault(false)));
            SchemaBuilder.AlterTable("CustomerPartRecord",
                table => table.AddColumn("AdsExpirationDate", DbType.DateTime));
            SchemaBuilder.AlterTable("CustomerPartRecord",
                table => table.AddColumn("AdsVIP", DbType.Boolean, column => column.WithDefault(false)));
            SchemaBuilder.AlterTable("CustomerPartRecord",
                table => table.AddColumn("AdsVIPExpirationDate", DbType.DateTime));

            return 39;
        }

        public int UpdateFrom39()
        {
            // ContactPhoneToDisplay - ContactPhone to display on frontend
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("ContactPhoneToDisplay", DbType.String));

            return 40;
        }

        public int UpdateFrom40()
        {
            // Loại tin rao mặc định cho từng Group
            SchemaBuilder.AlterTable("UserGroupPartRecord", table => table.AddColumn("DefaultAdsType_id", DbType.Int32));

            return 41;
        }

        #endregion

        #region 41-50

        public int UpdateFrom41()
        {
            SchemaBuilder.AlterTable("PropertyTypePartRecord", table => table.AddColumn("DefaultImgUrl", DbType.String));

            // Giá thương lượng
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("PriceNegotiable", DbType.Boolean, column => column.WithDefault(false)));

            return 42;
        }

        public int UpdateFrom42()
        {
            // Creating table UserLocationRecord
            // Phân quyền cho các User theo tỉnh, quận, phường.
            SchemaBuilder.CreateTable("UserLocationRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("UserPartRecord_Id", DbType.Int32)
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("LocationWardPartRecord_Id", DbType.Int32)
                );

            return 43;
        }

        public int UpdateFrom43()
        {
            SchemaBuilder.AlterTable("UserGroupPartRecord",
                table => table.AddColumn("NumberOfAdsGoodDeal", DbType.Int32, column => column.WithDefault(0)));
            SchemaBuilder.AlterTable("UserGroupPartRecord",
                table => table.AddColumn("NumberOfAdsVIP", DbType.Int32, column => column.WithDefault(0)));

            return 44;
        }

        public int UpdateFrom44()
        {
            // BĐS nổi bật
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("IsHighlights", DbType.Boolean, column => column.WithDefault(false)));

            return 45;
        }

        public int UpdateFrom45()
        {
            // Chia sẻ BĐS giữa các Group theo tỉnh, quận, phường.
            SchemaBuilder.CreateTable("UserGroupSharedLocationRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("SeederUserGroupPartRecord_Id", DbType.Int32)
                .Column("LeecherUserGroupPartRecord_Id", DbType.Int32)
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("LocationWardPartRecord_Id", DbType.Int32)
                );

            return 46;
        }

        public int UpdateFrom46()
        {
            // BĐS nổi bật
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsHighlight", DbType.Boolean, column => column.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsHighlightExpirationDate", DbType.DateTime));
            SchemaBuilder.AlterTable("UserGroupPartRecord",
                table => table.AddColumn("NumberOfAdsHighlight", DbType.Int32, column => column.WithDefault(0)));

            return 47;
        }

        public int UpdateFrom47()
        {
            // Giá trị hẻm tương đương trong StreetRelation
            SchemaBuilder.AlterTable("StreetRelationPartRecord",
                table => table.AddColumn("RelatedAlleyValue", DbType.Double));

            return 48;
        }

        public int UpdateFrom48()
        {
            // IsAuthenticatedInfo BĐS đã xác thực thông tin
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("IsAuthenticatedInfo", DbType.Boolean, column => column.WithDefault(false)));

            return 49;
        }

        public int UpdateFrom49()
        {
            // PriceEstimatedByStaff Giá định giá bởi nhân viên, chỉ áp dụng cho các BĐS không định giá đc
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("PriceEstimatedByStaff", DbType.Double));

            return 50;
        }

        public int UpdateFrom50()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("Content", DbType.String, column => column.WithLength(500)));

            return 51;
        }

        #endregion

        #region 51-60

        public int UpdateFrom51()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("Description"));

            return 52;
        }

        public int UpdateFrom52()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsGoodDealRequest", DbType.Boolean, column => column.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsVIPRequest", DbType.Boolean, column => column.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("AdsHighlightRequest", DbType.Boolean, column => column.WithDefault(false)));

            return 53;
        }

        public int UpdateFrom53()
        {
            // Creating table UnEstimatedLocationRecord
            // Lưu lại "tỉnh, quận, phường, đường, số nhà" chưa đủ dữ liệu định giá
            SchemaBuilder.CreateTable("UnEstimatedLocationRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("LocationWardPartRecord_Id", DbType.Int32)
                .Column("LocationStreetPartRecord_Id", DbType.Int32)
                .Column("AddressNumber", DbType.String)
                .Column("CreatedDate", DbType.DateTime)
                );

            return 54;
        }

        public int UpdateFrom54()
        {
            // Creating table PropertyTypeConstructionPartRecord
            SchemaBuilder.CreateTable("PropertyTypeConstructionPartRecord", table => table
                .ContentPartRecord()
                .Column("PropertyGroup_id", DbType.Int32)
                .Column("PropertyType_id", DbType.Int32)
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                .Column("UnitPrice", DbType.Double)
                .Column("DefaultImgUrl", DbType.String)
                );
            ContentDefinitionManager.AlterTypeDefinition("PropertyTypeConstruction",
                cfg => cfg.WithPart("PropertyTypeConstructionPart"));

            return 55;
        }

        public int UpdateFrom55()
        {
            SchemaBuilder.AlterTable("PropertyTypeConstructionPartRecord",
                table => table.AddColumn("MinFloor", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyTypeConstructionPartRecord",
                table => table.AddColumn("MaxFloor", DbType.Int32));

            return 56;
        }

        public int UpdateFrom56()
        {
            // Loại công trình BĐS
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("TypeConstruction_id", DbType.Int32));

            return 57;
        }

        public int UpdateFrom57()
        {
            SchemaBuilder.AlterTable("PropertyTypeConstructionPartRecord",
                table => table.AddColumn("IsDefaultInFloorsRange", DbType.Boolean, c => c.WithDefault(false)));

            return 58;
        }

        public int UpdateFrom58()
        {
            // PlanningMapRecord
            // Bản đồ quy hoạch
            SchemaBuilder.CreateTable("PlanningMapRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("LocationWardPartRecord_Id", DbType.Int32)
                .Column("ImagesPath", DbType.String)
                .Column("Width", DbType.Int32)
                .Column("Height", DbType.Int32)
                .Column("MinZoom", DbType.Int32)
                .Column("MaxZoom", DbType.Int32)
                .Column("Ratio", DbType.Double)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(false))
                );

            return 59;
        }

        public int UpdateFrom59()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("PlacesAround", DbType.String, column => column.WithLength(10000)));

            return 60;
        }

        public int UpdateFrom60()
        {
            // VideoId on Youtube.com
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("YoutubeId", DbType.String, column => column.WithLength(10000)));

            return 61;
        }

        #endregion

        #region 61-70

        public int UpdateFrom61()
        {
            // Creating table LocationApartmentPartRecord
            SchemaBuilder.CreateTable("LocationApartmentPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("Floors", DbType.Int32)
                .Column("Basements", DbType.Int32)
                .Column("Elevators", DbType.Int32)
                .Column("SeqOrder", DbType.Int32)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationApartment",
                cfg => cfg.WithPart("LocationApartmentPart"));


            // Creating table LocationApartmentAdvantagePartRecordContent
            SchemaBuilder.CreateTable("LocationApartmentAdvantagePartRecordContent", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("LocationApartmentPartRecord_Id", DbType.Int32)
                .Column("PropertyAdvantagePartRecord_Id", DbType.Int32)
                );

            // ApartmentAdvantages
            IEnumerable<PropertyAdvantagePartRecord> locationApartmentAdvantage =
                new List<PropertyAdvantagePartRecord>
                {
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-adv",
                        CssClass = "apartment-adv-Childcare",
                        SeqOrder = 1,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Nhà trẻ, mẫu giáo"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-adv",
                        CssClass = "apartment-adv-Park",
                        SeqOrder = 2,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Công viên"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-adv",
                        CssClass = "apartment-adv-SwimmingPool",
                        SeqOrder = 3,
                        IsEnabled = true,
                        AddedValue = 10,
                        Name = "Hồ bơi"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-adv",
                        CssClass = "apartment-adv-SuperMarket",
                        SeqOrder = 4,
                        IsEnabled = true,
                        AddedValue = 10,
                        Name = "Siêu thị"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-adv",
                        CssClass = "apartment-adv-SportCenter",
                        SeqOrder = 5,
                        IsEnabled = true,
                        AddedValue = 5,
                        Name = "Khu thể thao"
                    },
                };
            foreach (PropertyAdvantagePartRecord item in locationApartmentAdvantage)
            {
                var model = Services.ContentManager.New<PropertyAdvantagePart>("PropertyAdvantage");
                model.Record = item;
                Services.ContentManager.Create(model);
            }


            // Creating table LocationApartmentRelationPartRecord
            SchemaBuilder.CreateTable("LocationApartmentRelationPartRecord", table => table
                .ContentPartRecord()
                .Column("Province_id", DbType.Int32)
                .Column("District_id", DbType.Int32)
                .Column("LocationApartment_id", DbType.Int32)
                .Column("RelatedValue", DbType.Double)
                .Column("RelatedProvince_id", DbType.Int32)
                .Column("RelatedDistrict_id", DbType.Int32)
                .Column("RelatedLocationApartment_id", DbType.Int32)
                );
            ContentDefinitionManager.AlterTypeDefinition("LocationApartmentRelation",
                cfg => cfg.WithPart("LocationApartmentRelationPart"));

            return 62;
        }

        public int UpdateFrom62()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("Apartment_id", DbType.Int32));
            SchemaBuilder.AlterTable("CustomerRequirementRecord",
                table => table.AddColumn("LocationApartmentPartRecord_Id", DbType.Int32));

            return 63;
        }

        public int UpdateFrom63()
        {
            // alter table LocationApartmentPartRecord
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("Block", DbType.String));

            // Creating table CoefficientApartmentFloorsPartRecord
            SchemaBuilder.CreateTable("CoefficientApartmentFloorsPartRecord", table => table
                .ContentPartRecord()
                .Column("Floors", DbType.Double)
                .Column("CoefficientApartmentFloors", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("CoefficientApartmentFloors",
                cfg => cfg.WithPart("CoefficientApartmentFloorsPart"));

            // Creating table CoefficientApartmentFloorThPartRecord
            SchemaBuilder.CreateTable("CoefficientApartmentFloorThPartRecord", table => table
                .ContentPartRecord()
                .Column("MaxFloors", DbType.Double)
                .Column("ApartmentFloorTh", DbType.Double)
                .Column("CoefficientApartmentFloorTh", DbType.Double)
                );
            ContentDefinitionManager.AlterTypeDefinition("CoefficientApartmentFloorTh",
                cfg => cfg.WithPart("CoefficientApartmentFloorThPart"));

            return 64;
        }

        public int UpdateFrom64()
        {
            // alter table PropertyFilePartRecord
            SchemaBuilder.AlterTable("PropertyFilePartRecord",
                table => table.AddColumn("LocationApartmentPartRecord_Id", DbType.Int32));

            // alter table LocationApartmentPartRecord
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("Description", DbType.String, column => column.WithLength(1000)));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("TradeFloors", DbType.Int32));

            return 65;
        }

        public int UpdateFrom65()
        {
            // ApartmentInteriorAdvantages
            IEnumerable<PropertyAdvantagePartRecord> apartmentInteriorAdvantages =
                new List<PropertyAdvantagePartRecord>
                {
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-interior-adv",
                        CssClass = "apartment-interior-adv-WoodFloor",
                        SeqOrder = 1,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Sàn gỗ"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-interior-adv",
                        CssClass = "apartment-interior-adv-ToiletEquipment",
                        SeqOrder = 2,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Thiết bị vệ sinh"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-interior-adv",
                        CssClass = "apartment-interior-adv-KitchenEquipment",
                        SeqOrder = 3,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Thiết bị nhà bếp"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-interior-adv",
                        CssClass = "apartment-interior-adv-BedCabinets",
                        SeqOrder = 4,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Giường tủ, bàn ghế"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "apartment-interior-adv",
                        CssClass = "apartment-interior-adv-AirConditioner",
                        SeqOrder = 5,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Máy lạnh"
                    },
                };
            foreach (PropertyAdvantagePartRecord item in apartmentInteriorAdvantages)
            {
                var model = Services.ContentManager.New<PropertyAdvantagePart>("PropertyAdvantage");
                model.Record = item;
                Services.ContentManager.Create(model);
            }

            // ConstructionAdvantages
            IEnumerable<PropertyAdvantagePartRecord> constructionAdvantages =
                new List<PropertyAdvantagePartRecord>
                {
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Basement",
                        SeqOrder = 1,
                        IsEnabled = true,
                        AddedValue = 2,
                        Name = "Có tầng hầm"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Mezzanine",
                        SeqOrder = 2,
                        IsEnabled = true,
                        AddedValue = 0.5,
                        Name = "Có lửng"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Terrace",
                        SeqOrder = 3,
                        IsEnabled = true,
                        AddedValue = 1,
                        Name = "Có sân thượng"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Garage",
                        SeqOrder = 4,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Có gara ô tô"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Elevator",
                        SeqOrder = 5,
                        IsEnabled = true,
                        AddedValue = 200,
                        Name = "Có thang máy"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-SwimmingPool",
                        SeqOrder = 6,
                        IsEnabled = true,
                        AddedValue = 100,
                        Name = "Có hồ bơi"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Garden",
                        SeqOrder = 7,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Có sân vườn"
                    },
                    new PropertyAdvantagePartRecord
                    {
                        ShortName = "construction-adv",
                        CssClass = "construction-adv-Skylight",
                        SeqOrder = 8,
                        IsEnabled = true,
                        AddedValue = 0,
                        Name = "Có giếng trời"
                    },
                };
            foreach (PropertyAdvantagePartRecord item in constructionAdvantages)
            {
                var model = Services.ContentManager.New<PropertyAdvantagePart>("PropertyAdvantage");
                model.Record = item;
                Services.ContentManager.Create(model);
            }

            return 66;
        }

        public int UpdateFrom66()
        {
            // alter table PropertyFilePartRecord
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("ApartmentTradeFloors", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AlterColumn("Content", column => column.WithLength(10000).WithType(DbType.String)));
            return 67;
        }

        public int UpdateFrom67()
        {
            // chọn Quận mặc định cho group
            SchemaBuilder.AlterTable("UserGroupPartRecord", table => table.AddColumn("DefaultDistrict_id", DbType.Int32));

            return 68;
        }

        public int UpdateFrom68()
        {
            // Creating table UserGroupContactRecord
            // Contact cho các Group theo tỉnh, quận và Loại tin rao.
            SchemaBuilder.CreateTable("UserGroupContactRecord", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("UserGroupPartRecord_Id", DbType.Int32)
                .Column("LocationProvincePartRecord_Id", DbType.Int32)
                .Column("LocationDistrictPartRecord_Id", DbType.Int32)
                .Column("AdsTypePartRecord_Id", DbType.Int32)
                .Column("PropertyTypeGroupPartRecord_Id", DbType.Int32)
                .Column("ContactPhone", DbType.String)
                );

            return 69;
        }

        public int UpdateFrom69()
        {
            // Giới hạn quyền của User trong việc xem các BĐS theo User's Location
            SchemaBuilder.AlterTable("UserLocationRecord",
                table => table.AddColumn("EnableAccessProperties", DbType.Boolean));
            // Giới hạn quyền của User trong việc Edit Location Wards, Streets, Street Relations, Apartment, Apartment Relations theo User's Location
            SchemaBuilder.AlterTable("UserLocationRecord",
                table => table.AddColumn("EnableEditLocations", DbType.Boolean));

            return 70;
        }

        public int UpdateFrom70()
        {
            // Thêm Ngõ / Ngách dành cho BĐS Hà Nội
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("AddressCorner", DbType.String));
            SchemaBuilder.AlterTable("UnEstimatedLocationRecord",
                table => table.AddColumn("AddressCorner", DbType.String));

            return 71;
        }

        #endregion

        #region 71-80

        public int UpdateFrom71()
        {
            // Nhóm BĐS mặc định cho Group
            SchemaBuilder.AlterTable("UserGroupPartRecord",
                table => table.AddColumn("DefaultTypeGroup_id", DbType.Int32));

            return 72;
        }

        public int UpdateFrom72()
        {
            // Loại tin rao & Nhóm BĐS mặc định cho từng User trong Group
            SchemaBuilder.AlterTable("UserInGroupRecord", table => table.AddColumn("DefaultAdsType_id", DbType.Int32));
            SchemaBuilder.AlterTable("UserInGroupRecord", table => table.AddColumn("DefaultTypeGroup_id", DbType.Int32));

            return 73;
        }

        public int UpdateFrom73()
        {
            // Drop ApartmentAdvantages
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("ApartmentHaveChildcare"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("ApartmentHavePark"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("ApartmentHaveSwimmingPool"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("ApartmentHaveSuperMarket"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("ApartmentHaveSportCenter"));

            // Drop ApartmentInteriorAdvantages
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("InteriorHaveWoodFloor"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("InteriorHaveToiletEquipment"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("InteriorHaveKitchenEquipment"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("InteriorHaveBedCabinets"));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("InteriorHaveAirConditioner"));

            return 74;
        }

        public int UpdateFrom74()
        {
            // Sắp xếp thứ tự cho tin VIP quảng cáo: 0 => 3 {0: Tin thường, 1 => 3 : VIP}
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("SeqOrder", DbType.Int32, c => c.WithDefault(0)));

            return 75;
        }

        public int UpdateFrom75()
        {
            // Creating table AdsPaymentConfigPartRecord
            SchemaBuilder.CreateTable("AdsPaymentConfigPartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("Description", DbType.String)
                .Column("Value", DbType.Int64)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );
            ContentDefinitionManager.AlterTypeDefinition("AdsPaymentConfig", cfg => cfg.WithPart("AdsPaymentConfigPart"));

            // Creating table AdsPaymentHistoryPartRecord
            SchemaBuilder.CreateTable("AdsPaymentHistoryPartRecord", table => table
                .ContentPartRecord()
                .Column("User_Id", DbType.Int32)
                .Column("UserPerform_Id", DbType.Int32)
                .Column("EndBalance", DbType.Int64)
                .Column("StartBalance", DbType.Int64)
                .Column("Property_Id", DbType.Int32)
                .Column("PaymentConfig_Id", DbType.Int32)
                .Column("DateTrading", DbType.DateTime)
                .Column("Note", DbType.String)
                .Column("PayStatus", DbType.Boolean, c => c.WithDefault(false))
                );
            ContentDefinitionManager.AlterTypeDefinition("AdsPaymentHistory",
                cfg => cfg.WithPart("AdsPaymentHistoryPart"));

            IEnumerable<AdsPaymentConfigPartRecord> paymentconfig =
                new List<AdsPaymentConfigPartRecord>
                {
                    new AdsPaymentConfigPartRecord
                    {
                        Name = "Nạp tiền",
                        CssClass = "ins-money",
                        Value = 0,
                        IsEnabled = true,
                        Description = "Nộp tiền vào tài khoản"
                    },
                    new AdsPaymentConfigPartRecord
                    {
                        Name = "VIP 1",
                        CssClass = "ex-vip-1",
                        Value = 15000,
                        IsEnabled = true,
                        Description = "Đăng tin VIP 1"
                    },
                    new AdsPaymentConfigPartRecord
                    {
                        Name = "VIP 2",
                        CssClass = "ex-vip-2",
                        Value = 10000,
                        IsEnabled = true,
                        Description = "Đăng tin VIP 2"
                    },
                    new AdsPaymentConfigPartRecord
                    {
                        Name = "VIP 3",
                        CssClass = "ex-vip-3",
                        Value = 5000,
                        IsEnabled = true,
                        Description = "Đăng tin VIP 3"
                    },
                };
            foreach (AdsPaymentConfigPartRecord item in paymentconfig)
            {
                var model = Services.ContentManager.New<AdsPaymentConfigPart>("AdsPaymentConfig");
                model.Name = item.Name;
                model.CssClass = item.CssClass;
                model.Description = item.Description;
                model.Value = item.Value;
                model.IsEnabled = item.IsEnabled;
                Services.ContentManager.Create(model);
            }

            return 76;
        }

        public int UpdateFrom76()
        {
            // Giá trị tin VIP lưu trong lịch sử giao dịch
            SchemaBuilder.AlterTable("AdsPaymentConfigPartRecord",
                table => table.AddColumn("VipValue", DbType.Int32, c => c.WithDefault(0)));

            // UPdate
            AdsPaymentConfigPart vip1 =
                Services.ContentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                    .Where(r => r.CssClass == "ex-vip-1")
                    .List()
                    .First();
            vip1.VipValue = 3;
            AdsPaymentConfigPart vip2 =
                Services.ContentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                    .Where(r => r.CssClass == "ex-vip-2")
                    .List()
                    .First();
            vip2.VipValue = 2;
            AdsPaymentConfigPart vip3 =
                Services.ContentManager.Query<AdsPaymentConfigPart, AdsPaymentConfigPartRecord>()
                    .Where(r => r.CssClass == "ex-vip-3")
                    .List()
                    .First();
            vip3.VipValue = 1;

            return 77;
        }

        public int UpdateFrom77()
        {
            // Xác định là tin rao làm mới
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("IsRefresh", DbType.Boolean, c => c.WithDefault(false)));
            return 78;
        }

        public int UpdateFrom78()
        {
            // số ngày đăng tin
            SchemaBuilder.AlterTable("AdsPaymentHistoryPartRecord",
                table => table.AddColumn("PostingDates", DbType.Int32, c => c.WithDefault(0)));
            return 79;
        }

        public int UpdateFrom79()
        {
            // alter table LocationApartmentPartRecord
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("StreetAddress", DbType.String));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("DistanceToCentral", DbType.String));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("OtherAdvantages", DbType.String));

            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("Investors", DbType.String));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("CurrentBuildingStatus", DbType.String));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("ManagementFees", DbType.String));

            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("AreaTotal", DbType.Double));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("AreaConstruction", DbType.Double));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("AreaGreen", DbType.Double));

            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("AreaTradeFloors", DbType.Double));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("AreaBasements", DbType.Double));

            // Đặc điểm tốt khác làm tăng giá trị của BĐS %
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OtherAdvantages", DbType.Double));
            // Đặc điểm xấu khác làm giảm giá trị BĐS %
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OtherDisAdvantages", DbType.Double));

            return 80;
        }

        public int UpdateFrom80()
        {
            // alter table LocationApartmentPartRecord
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("Ward_id", DbType.Int32));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("Street_id", DbType.Int32));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord",
                table => table.AddColumn("AddressNumber", DbType.String));

            return 81;
        }

        #endregion

        #region 81-90

        public int UpdateFrom81()
        {
            // Đặc điểm tốt khác làm tăng giá trị của BĐS %
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("OtherAdvantagesDesc", DbType.String));
            // Đặc điểm xấu khác làm giảm giá trị BĐS %
            SchemaBuilder.AlterTable("PropertyPartRecord",
                table => table.AddColumn("OtherDisAdvantagesDesc", DbType.String));

            return 82;
        }

        public int UpdateFrom82()
        {
            // Xác định đăng tin VIP ở nội bộ hay khách hàng
            SchemaBuilder.AlterTable("AdsPaymentHistoryPartRecord",
                table => table.AddColumn("IsInternal", DbType.Boolean, c => c.WithDefault(false)));

            return 83;
        }

        public int UpdateFrom83()
        {
            SchemaBuilder.AlterTable("PropertyFilePartRecord",
                table => table.AddColumn("IsDeleted", DbType.Boolean, column => column.WithDefault(false)));

            return 84;
        }

        public int UpdateFrom84()
        {
            // Cho phép user làm nhân viên môi giới.
            SchemaBuilder.AlterTable("UserLocationRecord",
                table => table.AddColumn("EnableIsAgencies", DbType.Boolean, c => c.WithDefault(false)));

            return 85;
        }

        public int UpdateFrom85()
        {
            // Cho phép user làm nhân viên môi giới.
            SchemaBuilder.AlterTable("UserLocationRecord", table => table.AddColumn("AreaAgencies", DbType.String));

            return 86;
        }

        public int UpdateFrom86()
        {
            // Cho phép user làm nhân viên môi giới.
            SchemaBuilder.AlterTable("UserLocationRecord", table => table.AddColumn("EndDateAgencing", DbType.DateTime));

            return 87;
        }

        public int UpdateFrom87()
        {
            // Giá trị giao dịch
            SchemaBuilder.AlterTable("AdsPaymentHistoryPartRecord",
                table => table.AddColumn("TransactionValue", DbType.Int64));

            return 88;
        }

        public int UpdateFrom88()
        {
            // Area for filter only
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("Area", DbType.Double));

            return 89;
        }

        public int UpdateFrom89()
        {
            SchemaBuilder.CreateTable("HostNamePartRecord", table => table
                .ContentPartRecord()
                .Column("Name", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("CssClass", DbType.String)
                .Column("IsEnabled", DbType.Boolean, c => c.WithDefault(true))
                );

            ContentDefinitionManager.AlterTypeDefinition("HostName", cfg => cfg.WithPart("HostNamePart"));

            ContentDefinitionManager.AlterTypeDefinition("UserGroup", cfg => cfg.WithPart("HostNamePart"));

            ContentDefinitionManager.AlterPartDefinition(typeof (HostNamePart).Name, cfg => cfg
                .Attachable());

            return 90;
        }

        public int UpdateFrom90()
        {
            ContentDefinitionManager.AlterTypeDefinition("PropertyGroup",
                cfg => cfg.WithPart(typeof (PropertyGroupPart).Name));

            return 91;
        }

        #endregion

        #region 91 - 100

        public int UpdateFrom91()
        {
            SchemaBuilder.CreateTable("PropertyGroupPartRecord", table => table
                .ContentPartRecord()
                .Column("PropertyId", DbType.Int32)
                .Column("UserGroupId", DbType.Int32)
                );
            return 92;
        }

        public int UpdateFrom92()
        {
            // Trạng thái duyệt tin của Group.
            SchemaBuilder.AlterTable("PropertyGroupPartRecord", table => table.AddColumn("IsApproved", DbType.Boolean));

            return 93;
        }

        public int UpdateFrom93()
        {
            // Trạng thái duyệt tin của Group.
            SchemaBuilder.AlterTable("UserGroupPartRecord", table => table.AddColumn("ApproveAllGroup", DbType.Boolean));

            return 94;
        }

        public int UpdateFrom94()
        {
            // ApartmentBlock_id
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentBlock_id", DbType.Int32));

            // Creating table LocationApartmentBlockPartRecord
            SchemaBuilder.CreateTable("LocationApartmentBlockPartRecord", table => table
                .ContentPartRecord()
                .Column("LocationApartment_id", DbType.Int32)
                .Column("BlockName", DbType.String)
                .Column("ShortName", DbType.String)
                .Column("FloorTotal", DbType.Int32)
                .Column("ApartmentEachFloor", DbType.Int32)
                .Column("SeqOrder", DbType.Int32)
            );

            ContentDefinitionManager.AlterTypeDefinition("LocationApartmentBlock", cfg => cfg.WithPart("LocationApartmentBlockPart"));

            return 95;
        }
        public int UpdateFrom95()
        {
            // Trạng thái Block
            SchemaBuilder.AlterTable("LocationApartmentBlockPartRecord", table => table.AddColumn("IsActive", DbType.Boolean,cf=> cf.WithDefault(true)));

            return 96;
        }

        public int UpdateFrom96()
        {
            // HostNamePartRecord
            SchemaBuilder.AlterTable("HostNamePartRecord", table => table.AddColumn("GoogleAnalyticsKey", DbType.String));
            SchemaBuilder.AlterTable("HostNamePartRecord", table => table.AddColumn("DomainName", DbType.String));
            SchemaBuilder.AlterTable("HostNamePartRecord", table => table.AddColumn("UseAsyncTracking", DbType.Boolean, cf => cf.WithDefault(true)));
            SchemaBuilder.AlterTable("HostNamePartRecord", table => table.AddColumn("TrackOnAdmin", DbType.Boolean, cf => cf.WithDefault(true)));

            return 97;
        }

        public int UpdateFrom97()
        {
            SchemaBuilder.AlterTable("UserLocationRecord", table => table.AddColumn("HostName", DbType.String));

            return 98;
        }

        public int UpdateFrom98()
        {
            // Order By Domain
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OrderByGroupPhuoc", DbType.Boolean, cf => cf.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OrderByGroupDatPho", DbType.Boolean, cf => cf.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OrderByGroupNghia", DbType.Boolean, cf => cf.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OrderByGroupCLBHN", DbType.Boolean, cf => cf.WithDefault(false)));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OrderByGroupDuLieuNhaDat", DbType.Boolean, cf => cf.WithDefault(false)));

            //Update
            var properties =
                Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
                    .Where(r => r.UserGroup != null && r.UserGroup.Id != 103993).List();

            foreach (var property in properties)
            {
                switch (property.UserGroup.Id)
                {
                    case 742406:
                        property.OrderByGroupPhuoc = true;
                        property.OrderByGroupDatPho = false;
                        property.OrderByGroupNghia = false;
                        property.OrderByGroupCLBHN = false;
                        property.OrderByGroupDuLieuNhaDat = false;
                        break;

                    case 611671:
                        property.OrderByGroupPhuoc = false;
                        property.OrderByGroupDatPho = true;
                        property.OrderByGroupNghia = false;
                        property.OrderByGroupCLBHN = false;
                        property.OrderByGroupDuLieuNhaDat = false;
                        break;

                    case 749499:
                        property.OrderByGroupPhuoc = false;
                        property.OrderByGroupDatPho = false;
                        property.OrderByGroupNghia = true;
                        property.OrderByGroupCLBHN = false;
                        property.OrderByGroupDuLieuNhaDat = false;
                        break;

                    case 592882:
                        property.OrderByGroupPhuoc = false;
                        property.OrderByGroupDatPho = false;
                        property.OrderByGroupNghia = false;
                        property.OrderByGroupCLBHN = true;
                        property.OrderByGroupDuLieuNhaDat = false;
                        break;

                    case 768363:
                        property.OrderByGroupPhuoc = false;
                        property.OrderByGroupDatPho = false;
                        property.OrderByGroupNghia = false;
                        property.OrderByGroupCLBHN = false;
                        property.OrderByGroupDuLieuNhaDat = true;
                        break;
                }
            }
            return 99;
        }

        public int UpdateFrom99()
        {
            SchemaBuilder.AlterTable("UserLocationRecord", table => table.DropColumn("HostName"));

            SchemaBuilder.AlterTable("UserLocationRecord", table => table.AddColumn("UserGroupRecord_id", DbType.Int32));
            return 100;
        }

        public int UpdateFrom100()
        {
            SchemaBuilder.AlterTable("UserLocationRecord", table => table.AddColumn("IsSelling", DbType.Boolean));
            SchemaBuilder.AlterTable("UserLocationRecord", table => table.AddColumn("IsLeasing", DbType.Boolean));
            return 101;
        }

        #endregion

        #region 101 - 110

        public int UpdateFrom101()
        {
            // BĐS đã bán bởi người trong Group
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("IsSoldByGroup", DbType.Boolean));

            return 102;
        }

        public int UpdateFrom102()
        {
            // Creating table PropertyUserPartRecordContent
            SchemaBuilder.CreateTable("PropertyUserPartRecordContent", table => table
                .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                .Column("PropertyPartRecord_Id", DbType.Int32)
                .Column("UserPartRecord_Id", DbType.Int32)
                );

            return 103;
        }

        public int UpdateFrom103()
        {
            // Creating table NewsVideoPartRecord
            SchemaBuilder.CreateTable("NewsVideoPartRecord", table => table
                    .ContentPartRecord()
                    .Column("Title", DbType.String)
                    .Column("Description", DbType.String)
                    .Column("YoutubeId", DbType.String)
                    .Column("Enable", DbType.Boolean,c=>c.WithDefault(true))
                    .Column("SeqOrder", DbType.Int32)
                );

            ContentDefinitionManager.AlterTypeDefinition("NewsVideo",
                cfg => cfg.WithPart("NewsVideoPart"));

            return 104;
        }
        public int UpdateFrom104()
        {
            //Drop NewsVideoPartRecord table
            SchemaBuilder.DropTable("NewsVideoPartRecord");

            // Creating table VideoTypePartRecord
            SchemaBuilder.CreateTable("VideoTypePartRecord", table => table
                    .ContentPartRecord()
                    .Column("Name", DbType.String)
                );

            ContentDefinitionManager.AlterTypeDefinition("VideoType",
                cfg => cfg.WithPart("VideoTypePart"));

            // Creating table VideoManagePartRecord
            SchemaBuilder.CreateTable("VideoManagePartRecord", table => table
                    .ContentPartRecord()
                    .Column("VideoType_id", DbType.Int32)
                    .Column("Title", DbType.String)
                    .Column("Keyword", DbType.String)
                    .Column("Description", DbType.String)
                    .Column("YoutubeId", DbType.String)
                    .Column("Enable", DbType.Boolean, c => c.WithDefault(true))
                    .Column("Publish", DbType.Boolean, c => c.WithDefault(true))
                    .Column("SeqOrder", DbType.Int32)
                    .Column("DomainGroupId", DbType.Int32)
                );

            ContentDefinitionManager.AlterTypeDefinition("VideoManage",
                cfg => cfg.WithPart("VideoManagePart"));

            // VideoTypePartRecord
            IEnumerable<VideoTypePartRecord> videotypes =
                new List<VideoTypePartRecord>
                {
                    new VideoTypePartRecord { Name = "Video nhà đất"},
                    new VideoTypePartRecord { Name = "Video tin tức"}
                };

            foreach (VideoTypePartRecord item in videotypes)
            {
                var model = Services.ContentManager.New<VideoTypePart>("VideoType");
                model.Name = item.Name;
                Services.ContentManager.Create(model);
            }


            return 105;
        }

        public int UpdateFrom105()
        {
            // Publish clip Youtube
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("YoutubePublish", DbType.Boolean));

            return 106;
        }

        public int UpdateFrom106()
        {
            // Creating table ExchangeTypePartRecord
            SchemaBuilder.CreateTable("ExchangeTypePartRecord", table => table
                    .ContentPartRecord()
                    .Column("Name", DbType.String)
                    .Column("CssClass", DbType.String)
                );

            ContentDefinitionManager.AlterTypeDefinition("ExchangeType",
                cfg => cfg.WithPart("ExchangeTypePart"));

            // Creating table PropertyExchangePartRecord
            SchemaBuilder.CreateTable("PropertyExchangePartRecord", table => table
                    .Column("Id", DbType.Int32, column => column.PrimaryKey().Identity())
                    .Column("User_id", DbType.Int32)
                    .Column("Property_id", DbType.Int32)
                    .Column("Customer_id", DbType.Int32)
                    .Column("ExchangeValues", DbType.Double)
                    .Column("ExchangeType_id", DbType.Int32)
                    .Column("PaymentMethod_id", DbType.Int32)
                );

            //ContentDefinitionManager.AlterTypeDefinition("PropertyExchange",
                //cfg => cfg.WithPart("PropertyExchangePart"));


            //ExchangeTypePartRecord
            IEnumerable<ExchangeTypePartRecord> exchangeType =
                new List<ExchangeTypePartRecord>
                {
                    new ExchangeTypePartRecord { Name = "Cao hơn (Bù thêm tiền)", CssClass = "exchange-more"},
                    new ExchangeTypePartRecord { Name = "Ngang giá", CssClass = "exchange-parity"},
                    new ExchangeTypePartRecord { Name = "Thấp hơn (Muốn nhận thêm tiền)", CssClass = "exchange-lower"}
                };

            foreach (var item in exchangeType)
            {
                var model = Services.ContentManager.New<ExchangeTypePart>("ExchangeType");
                model.Name = item.Name;
                model.CssClass = item.CssClass;
                Services.ContentManager.Create(model);
            }

            return 107;
        }

        public int UpdateFrom107()
        {
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("OrderByGroupDinhGiaNhaDat", DbType.Boolean, cf => cf.WithDefault(false)));

            //Update
            //var properties =
            //    Services.ContentManager.Query<PropertyPart, PropertyPartRecord>()
            //        .Where(r => r.UserGroup != null && r.UserGroup.Id == 103993).List();

            //foreach (var property in properties)
            //{
            //    property.OrderByGroupDinhGiaNhaDat = true;
            //}

            return 108;
        }
        public int UpdateFrom108()
        {
            SchemaBuilder.AlterTable("VideoManagePartRecord", table => table.AddColumn("Image", DbType.String));

            return 109;
        }
		public int UpdateFrom109()
        {
            // Creating table GroupInApartmentBlockPartRecord
            SchemaBuilder.CreateTable("GroupInApartmentBlockPartRecord", table => table
                    .ContentPartRecord()
                    .Column("ApartmentBlock_id", DbType.Int32)
                    .Column("FloorFrom", DbType.Int32)
                    .Column("FloorTo", DbType.Int32)
                    .Column("ApartmentPerFloor", DbType.Int32)
                    .Column("ApartmentGroupPosition", DbType.Int32)
                );

            ContentDefinitionManager.AlterTypeDefinition("GroupInApartmentBlock",
                cfg => cfg.WithPart("GroupInApartmentBlockPart"));


            SchemaBuilder.AlterTable("LocationApartmentBlockPartRecord", table => table.AddColumn("GroupFloorInBlockTotal", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentPositionTh", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("GroupInApartmentBlock_id", DbType.Int32));

            return 110;
        }
        #endregion


        #region 111 - 120

        public int UpdateFrom110()
        {
            // Creating table ApartmentBlockInfoPartRecord
            SchemaBuilder.CreateTable("ApartmentBlockInfoPartRecord", table => table
                    .ContentPartRecord()
                    .Column("ApartmentBlock_id", DbType.Int32)
                    .Column("ApartmentName", DbType.String)
                    .Column("ApartmentArea", DbType.Double)
                    .Column("ApartmentBedrooms", DbType.Int32)
                    .Column("ApartmentBathrooms", DbType.Int32)
                    .Column("PriceAverage", DbType.Double)
                    .Column("IsActive", DbType.Boolean)
                    .Column("OrtherContent", DbType.String, column => column.WithLength(500))
                );

            ContentDefinitionManager.AlterTypeDefinition("ApartmentBlockInfo",
                cfg => cfg.WithPart("ApartmentBlockInfoPart"));

            SchemaBuilder.AlterTable("PropertyFilePartRecord", table => table.AddColumn("ApartmentBlockInfoPartRecord_id", DbType.Int32));
            SchemaBuilder.AlterTable("PropertyPartRecord", table => table.AddColumn("ApartmentBlockInfoPartRecord_id", DbType.Int32));

            return 111;
        }

        public int UpdateFrom111()
        {
            SchemaBuilder.AlterTable("ApartmentBlockInfoPartRecord", table => table.DropColumn("PriceAverage"));
            SchemaBuilder.AlterTable("LocationApartmentBlockPartRecord", table => table.AddColumn("PriceAverage", DbType.Double));
            SchemaBuilder.AlterTable("GroupInApartmentBlockPartRecord", table => table.AddColumn("IsActive", DbType.Boolean,cf=>cf.WithDefault(true)));

            return 112;
        }
        public int UpdateFrom112()
        {
            SchemaBuilder.AlterTable("ApartmentBlockInfoPartRecord", table => table.AddColumn("Avatar",DbType.String));
            SchemaBuilder.AlterTable("LocationApartmentBlockPartRecord", table => table.DropColumn("SeqOrder"));

            return 113;
        }

        public int UpdateFrom113()
        {
            var item = new AdsPaymentConfigPartRecord
                    {
                        Name = "Vip 0 (Tin thường)",
                        CssClass = "ex-vip-0",
                        Value = 1000,
                        IsEnabled = true,
                        Description = "Đăng tin thường có phí",
                        VipValue = 4
                    };

            var model = Services.ContentManager.New<AdsPaymentConfigPart>("AdsPaymentConfig");
            model.Name = item.Name;
            model.CssClass = item.CssClass;
            model.Description = item.Description;
            model.Value = item.Value;
            model.IsEnabled = item.IsEnabled;
            model.VipValue = item.VipValue;
            Services.ContentManager.Create(model);

            return 114;
        }

        public int UpdateFrom114()
        {
            //Dien tich su dung thuc te ( dien tich thong thuy)
            SchemaBuilder.AlterTable("ApartmentBlockInfoPartRecord", table => table.AddColumn("RealAreaUse", DbType.Double));

            return 115;
        }

        public int UpdateFrom115()
        {
            SchemaBuilder.AlterTable("GroupInApartmentBlockPartRecord", table => table.AddColumn("SeqOrder", DbType.Int32));

            return 116;
        }

        public int UpdateFrom116()
        {
            // Giới hạn quyền của User chỉ được xem các BĐS trong từng khu vực của Group
            SchemaBuilder.AlterTable("UserLocationRecord",
                table => table.AddColumn("RetrictedAccessGroupProperties", DbType.Boolean));

            return 117;
        }

        public int UpdateFrom117()
        {
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("PriceAverage", DbType.Double));

            return 118;
        }

        public int UpdateFrom118()
        {
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("IsHighlight", DbType.Boolean));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("HighlightExpiredTime", DbType.DateTime));

            return 119;
        }

        public int UpdateFrom119()
        {
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.DropColumn("IsHighlight"));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("IsHighlight", DbType.Boolean, c => c.WithDefault(false)));

            return 120;
        }

        public int UpdateFrom120()
        {
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("CreatedDate", DbType.DateTime));
            SchemaBuilder.AlterTable("LocationApartmentPartRecord", table => table.AddColumn("UpdatedDate", DbType.DateTime));

            return 121;
        }

        #endregion

        //public int UpdateFrom83()
        //{
        //    ContentDefinitionManager.AlterTypeDefinition("Property", cfg => cfg
        //        .WithPart("CommonPart")
        //        .WithPart("TitlePart")
        //        .WithPart("AutoroutePart", builder => builder
        //            .WithSetting("AutorouteSettings.AllowCustomPattern", "true")
        //            .WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
        //            .WithSetting("AutorouteSettings.PatternDefinitions", "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-page'}]")
        //            .WithSetting("AutorouteSettings.DefaultPatternIndex", "0"))
        //        .WithPart("BodyPart")
        //        .WithPart("CommentsPart")
        //        .WithPart("TagsPart"));

        //    return 84;
        //}

        //// Drop ConstructionAdvantages
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveBasement"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveMezzanine"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveTerrace"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveGarage"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveElevator"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveSwimmingPool"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveGarden"));
        //SchemaBuilder.AlterTable("PropertyPartRecord", table => table.DropColumn("HaveSkylight"));
    }
}